using WildNatureExplorer.Application.Interfaces.Services;
using WildNatureExplorer.Domain.Entities;
using WildNatureExplorer.Infrastructure.Data;
using WildNatureExplorer.Application.DTOs.AI;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;

namespace WildNatureExplorer.Infrastructure.Services
{
    public class AiService : IAiService
    {
        private readonly AppDbContext _db;
        private readonly HuggingFaceVisionService _vision;
        private readonly AnimalDetectVisionService _animalDetect;
        private readonly GroqChatService _chat;

        public AiService(
            AppDbContext db,
            HuggingFaceVisionService vision,
            AnimalDetectVisionService animalDetect,
            GroqChatService chat)
        {
            _db = db;
            _vision = vision;
            _animalDetect = animalDetect;
            _chat = chat;
        }

        public async Task<Guid> AnalyzeImageAsync(Guid userId, byte[] imageBytes, string? recognizer = null)
        {
            var session = await AnalyzeImageStructuredAsync(userId, imageBytes, recognizer: recognizer);
            return session.SessionId;
        }

        public async Task<AnimalAnalysisResponseDto> AnalyzeImageStructuredAsync(Guid userId, byte[] imageBytes, Guid? sessionId = null, string? recognizer = null)
        {
            var animalName = await RecognizeAnimalAsync(imageBytes, recognizer);

            AiSession session;

            if (sessionId != null && sessionId != Guid.Empty)
            {
                var existing = await _db.AiSessions.FindAsync(sessionId.Value);
                if (existing != null && !existing.IsEnded && existing.UserId == userId)
                {
                    session = existing;
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

            var response = await _chat.AskStructuredAsync($"Describe {animalName}: danger level, habitat, rarity.");

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
            return response;
        }

        private async Task<string> RecognizeAnimalAsync(byte[] imageBytes, string? recognizer)
        {
            var selected = (recognizer ?? "huggingface").Trim().ToLowerInvariant();

            return selected switch
            {
                "animaldetect" => await _animalDetect.RecognizeAnimalAsync(imageBytes),
                "huggingface" => await _vision.RecognizeAnimalAsync(imageBytes),
                _ => throw new ArgumentException("Unsupported recognizer. Use 'huggingface' or 'animaldetect'.", nameof(recognizer))
            };
        }

        public async Task<ChatResponseDto> AskAsync(Guid userId, Guid? sessionId, string question)
        {
            Guid actualSessionId;

            // If no session provided, create a new one
            if (sessionId == null || sessionId == Guid.Empty)
            {
                var newSession = new AiSession
                {
                    UserId = userId,
                    AnimalName = "General Question",
                    ImageUrl = "text-based"
                };
                _db.AiSessions.Add(newSession);
                await _db.SaveChangesAsync();
                actualSessionId = newSession.Id;
            }
            else
            {
                // Ensure session exists and belongs to user and is not ended
                var existing = await _db.AiSessions.FindAsync(sessionId.Value);
                if (existing == null || existing.IsEnded || existing.UserId != userId)
                {
                    // create a fresh session instead
                    var newSession = new AiSession
                    {
                        UserId = userId,
                        AnimalName = "General Question",
                        ImageUrl = "text-based"
                    };
                    _db.AiSessions.Add(newSession);
                    await _db.SaveChangesAsync();
                    actualSessionId = newSession.Id;
                }
                else
                {
                    actualSessionId = sessionId.Value;
                }
            }

            // Get conversation history for context
            var messages = await _db.AiMessages
                .Where(m => m.SessionId == actualSessionId)
                .OrderBy(m => m.Id)
                .ToListAsync();

            // Build conversation context
            var conversationHistory = string.Join("\n", 
                messages.Select(m => $"{m.Role}: {m.Content}"));

            var fullContext = string.IsNullOrEmpty(conversationHistory) 
                ? question 
                : $"{conversationHistory}\nUser: {question}";

            var response = await _chat.AskChatAsync(fullContext);

            _db.AiMessages.AddRange(
                new AiMessage { SessionId = actualSessionId, Role = "User", Content = question },
                new AiMessage { SessionId = actualSessionId, Role = "AI", Content = response.Answer }
            );
            await _db.SaveChangesAsync();

            // Add session ID to response
            response.SessionId = actualSessionId;
            return response;
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
            var text = animal.Description;

            var habitatMatch = Regex.Match(text, @"- \**Habitat\**:\s*(.*?)(?:\n|$)", RegexOptions.IgnoreCase);
            var dangerMatch = Regex.Match(text, @"- \**Danger Level\**:\s*(.*?)(?:\n|$)", RegexOptions.IgnoreCase);
            var rarityMatch = Regex.Match(text, @"- \**Rarity\**:\s*(.*?)(?:\n|$)", RegexOptions.IgnoreCase);

            if (habitatMatch.Success)
                animal.Habitat = habitatMatch.Groups[1].Value.Trim();
            if (dangerMatch.Success)
                animal.DangerLevel = dangerMatch.Groups[1].Value.Trim();
            if (rarityMatch.Success)
                animal.RarityLevel = rarityMatch.Groups[1].Value.Trim();

            text = text.Replace("**", "");
            text = Regex.Replace(text, @"\n{2,}", "\n");

            text = Regex.Replace(text, @"- Habitat:.*(\n|$)", "", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, @"- Danger Level:.*(\n|$)", "", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, @"- Rarity:.*(\n|$)", "", RegexOptions.IgnoreCase);

            animal.Description = text.Trim();
        }
    }
}