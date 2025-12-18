using WildNatureExplorer.Application.DTOs.Users;
using WildNatureExplorer.Application.DTOs.Auth;

namespace WildNatureExplorer.Application.Interfaces.Services;

public interface IAiService
{
    Task<Guid> AnalyzeImageAsync(Guid userId, byte[] imageBytes);
    Task<string> AskAsync(Guid sessionId, string question);
    Task SubmitFeedbackAsync(Guid sessionId, int rating, string? comment);
}
