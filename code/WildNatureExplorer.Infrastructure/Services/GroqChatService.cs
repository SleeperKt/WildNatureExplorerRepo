using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WildNatureExplorer.Application.AI.PromptPolicies;
using WildNatureExplorer.Application.DTOs.AI;
using WildNatureExplorer.Application.Options;

namespace WildNatureExplorer.Infrastructure.Services
{
    public class GroqChatService
    {
        private readonly HttpClient _http;
        private readonly ChatLlmOptions _opt;
        private readonly ILogger<GroqChatService> _logger;

        public GroqChatService(HttpClient http, IOptions<ChatLlmOptions> options,
            ILogger<GroqChatService> logger)
        {
            _http = http;
            _opt = options.Value;
            _logger = logger;

            var key = string.IsNullOrWhiteSpace(_opt.ApiKey)
                ? Environment.GetEnvironmentVariable("GROQ_API_KEY")
                : _opt.ApiKey.Trim();
            if (string.IsNullOrEmpty(key))
                throw new InvalidOperationException(
                    "Groq API key missing: set GROQ_API_KEY or ChatLlm:ApiKey.");

            var baseUri = _opt.BaseUrl.TrimEnd('/') + "/";
            _http.BaseAddress = new Uri(baseUri);
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", key);
            _http.Timeout = TimeSpan.FromSeconds(Math.Clamp(_opt.RequestTimeoutSeconds, 15, 120));
        }

        /// <summary>Typed upstream failure carrying retry classification for dual-model failover.</summary>
        private sealed class GroqUpstreamException : Exception
        {
            public HttpStatusCode StatusCode { get; }
            public bool IsTransient { get; }

            public GroqUpstreamException(string message, HttpStatusCode statusCode, bool isTransient)
                : base(message)
            {
                StatusCode = statusCode;
                IsTransient = isTransient;
            }
        }

        public async Task<AnimalAnalysisResponseDto> AskStructuredAsync(string recognizedSpeciesLabel)
        {
            AnimalPromptPolicy.Validate(recognizedSpeciesLabel);

            var userTurn = AnimalPromptPolicy.BuildStructuredSpeciesUserPrompt(recognizedSpeciesLabel);
            var turns = new[]
            {
                new ChatTurn("system", AnimalPromptPolicy.BuildStructuredSpeciesSystemPrompt()),
                new ChatTurn("user", userTurn),
            };

            var llm = await ExecuteWithFallbackAsync(turns, _opt.StructuredTemperature, _opt.StructuredMaxTokens);

            return new AnimalAnalysisResponseDto
            {
                Animal = new AnimalInfoDto { Description = llm.Content },
                Technical = new TechnicalInfoDto
                {
                    Usage = llm.Usage,
                    Inference = new InferenceMetadataDto
                    {
                        PrimaryModel = _opt.PrimaryModel,
                        ConfiguredFallbackModel = _opt.FallbackModel,
                        EffectiveModel = llm.ModelUsed,
                        UsedFallbackModel = llm.UsedFallbackModel,
                        LlmSkipped = false
                    }
                }
            };
        }

        public Task<LlmChatResult> AskChatAsync(IReadOnlyList<ChatTurn> conversation,
            string? extraKnowledgeContext = null)
            => AskChatInternalAsync(conversation, extraKnowledgeContext);

        public Task<LlmChatResult> AskChatAsync(string userPrompt)
        {
            AnimalPromptPolicy.Validate(userPrompt);
            return AskChatInternalAsync(new[] { new ChatTurn("user", userPrompt) }, null);
        }

        private async Task<LlmChatResult> AskChatInternalAsync(IReadOnlyList<ChatTurn> conversation,
            string? extraKnowledgeContext)
        {
            if (conversation == null || conversation.Count == 0)
                throw new ArgumentException("Conversation must contain at least one turn.", nameof(conversation));

            var systemPrompt = AnimalPromptPolicy.BuildChatSystemPrompt();
            if (!string.IsNullOrWhiteSpace(extraKnowledgeContext))
            {
                systemPrompt +=
                    "\n\nREFERENCE SNIPPETS (may be imperfect; reconcile carefully):\n"
                    + extraKnowledgeContext.Trim();
            }

            var allTurns = new List<ChatTurn>(conversation.Count + 1)
            {
                new ChatTurn("system", systemPrompt)
            };
            allTurns.AddRange(conversation);

            return await ExecuteWithFallbackAsync(allTurns, _opt.ChatTemperature, _opt.ChatMaxTokens);
        }

        private async Task<LlmChatResult> ExecuteWithFallbackAsync(IReadOnlyList<ChatTurn> turns,
            double temperature,
            int maxTokens)
        {
            try
            {
                return await PostCompletionOnceAsync(_opt.PrimaryModel, turns, temperature, maxTokens,
                    usedFallback: false);
            }
            catch (Exception ex) when (ShouldTryFallback(ex) &&
                !string.Equals(_opt.FallbackModel, _opt.PrimaryModel, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning(ex, "Groq primary model '{Primary}' failed — trying fallback '{Fb}'.",
                    _opt.PrimaryModel, _opt.FallbackModel);

                try
                {
                    return await PostCompletionOnceAsync(_opt.FallbackModel, turns, temperature, maxTokens,
                        usedFallback: true);
                }
                catch (Exception inner)
                {
                    _logger.LogError(inner, "Groq fallback '{Fb}' failed.", _opt.FallbackModel);
                    throw new HttpRequestException("LLM inference failed after primary + fallback attempt.", inner);
                }
            }
        }

        private static bool ShouldTryFallback(Exception ex) =>
            ex switch
            {
                GroqUpstreamException ue => ue.IsTransient,
                TaskCanceledException => true,
                _ => false
            };

        private async Task<LlmChatResult> PostCompletionOnceAsync(string model,
            IReadOnlyList<ChatTurn> turns,
            double temperature,
            int maxTokens,
            bool usedFallback)
        {
            var body = BuildBody(model, turns, temperature, maxTokens);
            using var response = await _http.PostAsJsonAsync("chat/completions", body);
            var payload = await response.Content.ReadAsStringAsync();

            JsonDocument doc;
            try
            {
                doc = JsonDocument.Parse(payload);
            }
            catch (JsonException jx)
            {
                throw new GroqUpstreamException($"Invalid JSON from Groq: {jx.Message}",
                    HttpStatusCode.BadGateway,
                    isTransient: true);
            }

            using (doc)
            {
                var root = doc.RootElement;

                if (!response.IsSuccessStatusCode)
                {
                    var errMsg = ExtractError(payload, root);
                    throw new GroqUpstreamException(
                        $"Groq HTTP {(int)response.StatusCode}: {errMsg ?? response.ReasonPhrase}",
                        response.StatusCode,
                        IsTransientStatus(response.StatusCode));
                }

                if (!root.TryGetProperty("choices", out var choices) ||
                    choices.ValueKind != JsonValueKind.Array ||
                    choices.GetArrayLength() == 0)
                {
                    throw new InvalidOperationException("Groq completion missing usable `choices` array.");
                }

                JsonElement msg;
                try
                {
                    msg = choices[0].GetProperty("message").GetProperty("content");
                }
                catch (Exception)
                {
                    throw new InvalidOperationException("Malformed Groq completion message envelope.");
                }

                var content = msg.ValueKind switch
                {
                    JsonValueKind.String => msg.GetString() ?? string.Empty,
                    JsonValueKind.Null => string.Empty,
                    JsonValueKind.Array => string.Concat(msg.EnumerateArray()
                        .Select(e => e.ValueKind == JsonValueKind.Object &&
                                     e.TryGetProperty("text", out var txt)
                                     && txt.ValueKind == JsonValueKind.String
                                ? txt.GetString()
                                : e.GetRawText())),
                    _ => msg.GetRawText()
                };

                var usage = UsageParser.Parse(root);
                var modelFromBody =
                    root.TryGetProperty("model", out var elt) &&
                    elt.ValueKind == JsonValueKind.String &&
                    elt.GetString() is { } ms
                        ? ms
                        : model;

                return new LlmChatResult
                {
                    Content = content.Trim(),
                    Usage = usage,
                    ModelUsed = modelFromBody,
                    UsedFallbackModel = usedFallback
                };
            }

            static bool IsTransientStatus(HttpStatusCode code)
            {
                return code is HttpStatusCode.TooManyRequests
                       or HttpStatusCode.RequestTimeout
                       or HttpStatusCode.BadGateway
                       or HttpStatusCode.ServiceUnavailable
                       or HttpStatusCode.GatewayTimeout;
            }

            static string? ExtractError(string rawPayload, JsonElement root)
            {
                if (root.TryGetProperty("error", out var er))
                {
                    if (er.ValueKind == JsonValueKind.Object &&
                        er.TryGetProperty("message", out var m) &&
                        m.ValueKind == JsonValueKind.String)
                        return m.GetString();
                    return er.ToString();
                }

                return rawPayload.Length > 400 ? rawPayload[..400] + "..." : rawPayload;
            }
        }

        private static object BuildBody(string model, IReadOnlyList<ChatTurn> turns, double temperature,
            int maxTokens) =>
            new
            {
                model,
                messages = turns.Select(t => new { role = t.Role, content = t.Content }).ToArray(),
                temperature,
                max_tokens = maxTokens,
            };
    }

    /// <summary>OpenAI-compatible usage block with Groq extras optional.</summary>
    internal static class UsageParser
    {
        public static UsageDto Parse(JsonElement root)
        {
            if (!root.TryGetProperty("usage", out var u) || u.ValueKind != JsonValueKind.Object)
                return new UsageDto();

            return new UsageDto
            {
                QueueTime = GetDouble(u, "queue_time"),
                PromptTokens = GetInt(u, "prompt_tokens"),
                CompletionTokens = GetInt(u, "completion_tokens"),
                TotalTokens = GetInt(u, "total_tokens"),
                TotalTime = GetDouble(u, "total_time")
            };

            static double GetDouble(JsonElement e, string name)
            {
                return e.TryGetProperty(name, out var p) && p.ValueKind == JsonValueKind.Number
                    ? p.GetDouble()
                    : 0;
            }

            static int GetInt(JsonElement e, string name)
            {
                if (!e.TryGetProperty(name, out var p) || p.ValueKind != JsonValueKind.Number)
                    return 0;
                return p.TryGetInt32(out var v) ? v : (int)Math.Round(p.GetDouble());
            }
        }
    }

    /// <summary>One Groq/OpenAI chat turn.</summary>
    public readonly record struct ChatTurn(string Role, string Content);
}
