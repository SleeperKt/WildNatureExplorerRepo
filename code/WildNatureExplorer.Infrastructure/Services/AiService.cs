using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WildNatureExplorer.Application.AI;
using WildNatureExplorer.Application.AI.PromptPolicies;
using WildNatureExplorer.Application.Common;
using WildNatureExplorer.Application.DTOs.AI;
using WildNatureExplorer.Application.Interfaces.Services;
using WildNatureExplorer.Application.Options;
using WildNatureExplorer.Domain.Entities;
using WildNatureExplorer.Infrastructure.Data;
using System.Text.RegularExpressions;

namespace WildNatureExplorer.Infrastructure.Services
{
    public class AiService : IAiService
    {
        private readonly AppDbContext _db;
        private readonly HuggingFaceVisionService _vision;
        private readonly AnimalDetectVisionService _animalDetect;
        private readonly GroqChatService _chat;
        private readonly WildlifeKnowledgeRetriever _kb;
        private readonly ILogger<AiService> _logger;
        private readonly AiInferenceOptions _inferenceOpts;
        private readonly ChatLlmOptions _llmOpts;

        public AiService(
            AppDbContext db,
            HuggingFaceVisionService vision,
            AnimalDetectVisionService animalDetect,
            GroqChatService chat,
            WildlifeKnowledgeRetriever kb,
            ILogger<AiService> logger,
            IOptions<AiInferenceOptions> inferenceOpts,
            IOptions<ChatLlmOptions> llmOpts)
        {
            _db = db;
            _vision = vision;
            _animalDetect = animalDetect;
            _chat = chat;
            _kb = kb;
            _logger = logger;
            _inferenceOpts = inferenceOpts.Value;
            _llmOpts = llmOpts.Value;
        }

        public async Task<Guid> AnalyzeImageAsync(Guid userId, byte[] imageBytes, string? recognizer = null)
        {
            var session = await AnalyzeImageStructuredAsync(userId, imageBytes, recognizer: recognizer);
            return session.SessionId;
        }

        public async Task<AnimalAnalysisResponseDto> AnalyzeImageStructuredAsync(Guid userId, byte[] imageBytes,
            Guid? sessionId = null, string? recognizer = null)
        {
            var animalName = await RecognizeAnimalAsync(imageBytes, recognizer);
            if (string.IsNullOrWhiteSpace(animalName))
                animalName = "unknown animal";

            AiSession session;

            if (sessionId != null && sessionId != Guid.Empty)
            {
                var existing = await _db.AiSessions.FindAsync(sessionId.Value);
                if (existing == null || existing.IsEnded || existing.UserId != userId)
                {
                    throw new InvalidAiSessionException(
                        "Image analysis was requested against a session that is missing, ended, or not owned by you. Call POST /api/ai/start and use the returned session id.");
                }

                session = existing;
                session.AnimalName = animalName;
                session.ImageUrl = "uploaded-image";
                _db.AiSessions.Update(session);
                await _db.SaveChangesAsync();
            }
            else
            {
                session = new AiSession
                {
                    UserId = userId,
                    AnimalName = animalName,
                    ImageUrl = "uploaded-image"
                };
                _db.AiSessions.Add(session);
                await _db.SaveChangesAsync();
            }

            var response = await _chat.AskStructuredAsync(animalName);

            response.Animal.Name = animalName;
            ParseAnimalFields(response.Animal);

            _db.AiMessages.Add(new AiMessage
            {
                SessionId = session.Id,
                Role = "AI",
                Content = response.Animal.Description
            });
            await _db.SaveChangesAsync();

            response.SessionId = session.Id;
            response.Technical.Inference ??= new InferenceMetadataDto();
            response.Technical.Inference.PrimaryModel ??= _llmOpts.PrimaryModel;
            response.Technical.Inference.EffectiveModel ??= response.Technical.Inference.PrimaryModel;

            return response;
        }

        private async Task<string> RecognizeAnimalAsync(byte[] imageBytes, string? recognizer)
        {
            var selected = (recognizer ?? "huggingface").Trim().ToLowerInvariant();

            return selected switch
            {
                "animaldetect" => await _animalDetect.RecognizeAnimalAsync(imageBytes),
                "huggingface" => await _vision.RecognizeAnimalAsync(imageBytes),
                _ => throw new ArgumentException("Unsupported recognizer. Use 'huggingface' or 'animaldetect'.",
                    nameof(recognizer))
            };
        }

        public async Task<ChatResponseDto> AskAsync(Guid userId, Guid? sessionId, string question)
        {
            var normalized = AiRequestAnalysis.NormalizeWhitespace(question);

            if (normalized.Length > _inferenceOpts.MaxUserMessageChars)
            {
                throw new ValidationException(
                    $"Question is too long ({normalized.Length} characters). Limit is {_inferenceOpts.MaxUserMessageChars}.",
                    "AI_MESSAGE_TOO_LONG");
            }

            try
            {
                AnimalPromptPolicy.Validate(normalized);
            }
            catch (InvalidOperationException ex)
            {
                throw new SafetyPolicyException(ex.Message);
            }

            Guid actualSessionId = await ResolveOrCreateAskSessionStrictAsync(userId, sessionId);

            if (NatureToolRouter.TryResolve(normalized, out var toolAnswer))
            {
                await PersistTurnAsync(actualSessionId, normalized, toolAnswer.Answer);
                toolAnswer.SessionId = actualSessionId;

                toolAnswer.Technical ??= new TechnicalInfoDto();
                toolAnswer.Technical.Usage ??= new UsageDto();
                toolAnswer.Technical.Inference!.PrimaryModel = _llmOpts.PrimaryModel;
                toolAnswer.Technical.Inference.EffectiveModel ??= "(no LLM — deterministic tool)";
                return toolAnswer;
            }

            var messages = await _db.AiMessages
                .Where(m => m.SessionId == actualSessionId)
                .OrderBy(m => m.Id)
                .ToListAsync();

            var conversation = new List<ChatTurn>(messages.Count + 1);
            foreach (var m in messages)
            {
                var role = string.Equals(m.Role, "AI", StringComparison.OrdinalIgnoreCase)
                    ? "assistant"
                    : "user";
                conversation.Add(new ChatTurn(role, m.Content));
            }

            conversation.Add(new ChatTurn("user", normalized));

            var (snippet, chunkIds) = _kb.Retrieve(normalized);

            try
            {
                var intent = AiRequestAnalysis.ClassifyIntent(normalized);
                var script = AiRequestAnalysis.DetectInputScript(normalized);

                var llm = await _chat.AskChatAsync(conversation,
                    string.IsNullOrWhiteSpace(snippet) ? null : snippet);

                await PersistTurnAsync(actualSessionId, normalized, llm.Content);

                var response = new ChatResponseDto
                {
                    Answer = llm.Content,
                    AnswerMarkdown = AiMarkdownFormatter.NormalizeForMarkdown(llm.Content),
                    SessionId = actualSessionId,
                    Technical = new TechnicalInfoDto
                    {
                        Usage = llm.Usage,
                        Inference = new InferenceMetadataDto
                        {
                            PrimaryModel = _llmOpts.PrimaryModel,
                            ConfiguredFallbackModel = _llmOpts.FallbackModel,
                            EffectiveModel = llm.ModelUsed,
                            UsedFallbackModel = llm.UsedFallbackModel,
                            Intent = intent,
                            DetectedInputScript = script,
                            RetrievalChunkIds = chunkIds,
                            ToolsUsed = Array.Empty<string>(),
                            LlmSkipped = false
                        }
                    }
                };

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "LLM inference failed after tool routing skipped; session={Session}",
                    actualSessionId);
                throw;
            }
        }

        private async Task<Guid> ResolveOrCreateAskSessionStrictAsync(Guid userId, Guid? sessionId)
        {
            if (sessionId == null || sessionId == Guid.Empty)
            {
                var ns = new AiSession
                {
                    UserId = userId,
                    AnimalName = "General Question",
                    ImageUrl = "text-based"
                };
                _db.AiSessions.Add(ns);
                await _db.SaveChangesAsync();
                return ns.Id;
            }

            var existing = await _db.AiSessions.FindAsync(sessionId.Value);
            if (existing == null || existing.IsEnded || existing.UserId != userId)
            {
                throw new InvalidAiSessionException(
                    "Chat session id is unknown, belongs to another user, or ended. Start a new conversation with POST /api/ai/start.");
            }

            return sessionId.Value;
        }

        private async Task PersistTurnAsync(Guid sessionId, string userText, string assistantText)
        {
            _db.AiMessages.AddRange(
                new AiMessage { SessionId = sessionId, Role = "User", Content = userText },
                new AiMessage { SessionId = sessionId, Role = "AI", Content = assistantText }
            );

            await _db.SaveChangesAsync();
        }

        public async Task<Guid> StartSessionAsync(Guid userId, string? initialContext = null)
        {
            var session = new AiSession
            {
                UserId = userId,
                AnimalName = string.IsNullOrEmpty(initialContext) ? "General" : initialContext,
                ImageUrl = "text-based"
            };

            _db.AiSessions.Add(session);
            await _db.SaveChangesAsync();
            return session.Id;
        }

        public async Task EndSessionAsync(Guid sessionId)
        {
            var session = await _db.AiSessions.FindAsync(sessionId);
            if (session == null) return;
            session.IsEnded = true;
            session.EndedAt = DateTime.UtcNow;
            _db.AiSessions.Update(session);
            await _db.SaveChangesAsync();
        }

        public async Task SubmitFeedbackAsync(Guid sessionId, int rating, string? comment)
        {
            _db.AiFeedbacks.Add(new AiFeedback
            {
                SessionId = sessionId,
                Rating = rating,
                Comment = comment
            });

            await _db.SaveChangesAsync();
        }

        private void ParseAnimalFields(AnimalInfoDto animal)
        {
            var raw = animal.Description ?? string.Empty;

            static string? MatchBullet(string input, string label)
            {
                var esc = Regex.Escape(label);
                var re =
                    new Regex(@"^\s*-\s*\**" + esc + @"\**:\s*(.+)$",
                        RegexOptions.IgnoreCase | RegexOptions.Multiline);
                var m = re.Match(input);
                return m.Success ? m.Groups[1].Value.Trim() : null;
            }

            animal.Habitat = MatchBullet(raw, "Habitat") ?? string.Empty;
            animal.DangerLevel = MatchBullet(raw, "Risk to humans")
                                 ?? MatchBullet(raw, "Danger Level")
                                 ?? MatchBullet(raw, "Danger to humans")
                                 ?? string.Empty;
            animal.RarityLevel = MatchBullet(raw, "Wild population rarity")
                                 ?? MatchBullet(raw, "Population rarity")
                                 ?? MatchBullet(raw, "Rarity")
                                 ?? MatchBullet(raw, "Conservation status")
                                 ?? string.Empty;

            static string StripMd(string s) => s.Replace("**", string.Empty).Trim();

            var overviewMatch = Regex.Match(raw,
                @"\*\*Overview:\*\*\s*(.+?)(?=\r?\n\s*-\s*\*\*|\r?\n\s*\*\*Interesting fact:\*\*|\z)",
                RegexOptions.Singleline | RegexOptions.IgnoreCase);
            if (!overviewMatch.Success)
            {
                overviewMatch = Regex.Match(raw,
                    @"^Overview:\s*(.+?)(?=\r?\n\s*-|\r?\n\s*Interesting fact|\z)",
                    RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.Multiline);
            }

            var factMatch = Regex.Match(raw,
                @"\*\*Interesting fact:\*\*\s*(.+?)(?=\r?\n|$)",
                RegexOptions.Singleline | RegexOptions.IgnoreCase);
            if (!factMatch.Success)
            {
                factMatch = Regex.Match(raw,
                    @"^Interesting fact:\s*(.+)$",
                    RegexOptions.IgnoreCase | RegexOptions.Multiline);
            }

            var parts = new List<string>();
            if (overviewMatch.Success)
                parts.Add(StripMd(overviewMatch.Groups[1].Value));
            if (factMatch.Success)
                parts.Add(StripMd(factMatch.Groups[1].Value));

            animal.Description =
                string.Join("\n\n", parts.Where(p => !string.IsNullOrWhiteSpace(p))).Trim();
        }
    }
}
