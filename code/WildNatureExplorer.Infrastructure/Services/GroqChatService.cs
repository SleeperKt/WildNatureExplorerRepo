using System.Net.Http.Headers;
using System.Text.Json;
using WildNatureExplorer.Application.AI.PromptPolicies;
using WildNatureExplorer.Application.DTOs.AI;
using System.Net.Http.Json;

namespace WildNatureExplorer.Infrastructure.Services
{
    public class GroqChatService
    {
        private readonly HttpClient _http;

        public GroqChatService(HttpClient http)
        {
            _http = http;
            _http.BaseAddress = new Uri("https://api.groq.com/openai/v1/");
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer",
                    Environment.GetEnvironmentVariable("GROQ_API_KEY"));
        }

        public async Task<AnimalAnalysisResponseDto> AskStructuredAsync(string userPrompt)
        {
            AnimalPromptPolicy.Validate(userPrompt);

            var body = new
            {
                model = "llama-3.1-8b-instant",
                messages = new[]
                {
                    new { role = "system", content = AnimalPromptPolicy.BuildSystemPrompt() },
                    new { role = "user", content = userPrompt }
                }
            };

            var response = await _http.PostAsJsonAsync("chat/completions", body);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            var content = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? string.Empty;

            var usageElement = doc.RootElement.GetProperty("usage");
            var usage = new UsageDto
            {
                QueueTime = usageElement.GetProperty("queue_time").GetDouble(),
                PromptTokens = usageElement.GetProperty("prompt_tokens").GetInt32(),
                CompletionTokens = usageElement.GetProperty("completion_tokens").GetInt32(),
                TotalTokens = usageElement.GetProperty("total_tokens").GetInt32(),
                TotalTime = usageElement.GetProperty("total_time").GetDouble()
            };

            return new AnimalAnalysisResponseDto
            {
                Animal = new AnimalInfoDto { Description = content },
                Technical = new TechnicalInfoDto { Usage = usage }
            };
        }

        public async Task<ChatResponseDto> AskChatAsync(string userPrompt)
        {
            AnimalPromptPolicy.Validate(userPrompt);

            var body = new
            {
                model = "llama-3.1-8b-instant",
                messages = new[]
                {
                    new { role = "system", content = AnimalPromptPolicy.BuildSystemPrompt() },
                    new { role = "user", content = userPrompt }
                }
            };

            var response = await _http.PostAsJsonAsync("chat/completions", body);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            var content = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? string.Empty;

            var usageElement = doc.RootElement.GetProperty("usage");
            var usage = new UsageDto
            {
                QueueTime = usageElement.GetProperty("queue_time").GetDouble(),
                PromptTokens = usageElement.GetProperty("prompt_tokens").GetInt32(),
                CompletionTokens = usageElement.GetProperty("completion_tokens").GetInt32(),
                TotalTokens = usageElement.GetProperty("total_tokens").GetInt32(),
                TotalTime = usageElement.GetProperty("total_time").GetDouble()
            };

            return new ChatResponseDto
            {
                Answer = content,
                Technical = new TechnicalInfoDto { Usage = usage }
            };
        }
    }
}

