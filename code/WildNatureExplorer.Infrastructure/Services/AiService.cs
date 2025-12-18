using WildNatureExplorer.Application.Interfaces.Services;
using WildNatureExplorer.Domain.Entities;
using WildNatureExplorer.Infrastructure.Data;

namespace WildNatureExplorer.Infrastructure.Services;

public class AiService : IAiService
{
    private readonly AppDbContext _db;
    private readonly HuggingFaceVisionService _vision;
    private readonly GroqChatService _chat;

    public AiService(
        AppDbContext db,
        HuggingFaceVisionService vision,
        GroqChatService chat)
    {
        _db = db;
        _vision = vision;
        _chat = chat;
    }

    public async Task<Guid> AnalyzeImageAsync(Guid userId, byte[] imageBytes)
    {
        var animal = await _vision.RecognizeAnimalAsync(imageBytes);

        var session = new AiSession
        {
            UserId = userId,
            AnimalName = animal,
            ImageUrl = "uploaded-image"
        };

        _db.AiSessions.Add(session);
        await _db.SaveChangesAsync();

        var info = await _chat.AskAsync(
            $"Describe {animal}: danger level, habitat, rarity."
        );

        _db.AiMessages.Add(new AiMessage
        {
            SessionId = session.Id,
            Role = "AI",
            Content = info
        });

        await _db.SaveChangesAsync();
        return session.Id;
    }

    public async Task<string> AskAsync(Guid sessionId, string question)
    {
        var answer = await _chat.AskAsync(question);

        _db.AiMessages.AddRange(
            new AiMessage { SessionId = sessionId, Role = "User", Content = question },
            new AiMessage { SessionId = sessionId, Role = "AI", Content = answer }
        );

        await _db.SaveChangesAsync();
        return answer;
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
}
