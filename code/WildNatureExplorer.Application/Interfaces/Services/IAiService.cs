using WildNatureExplorer.Application.DTOs.Users;
using WildNatureExplorer.Application.DTOs.Auth;
using WildNatureExplorer.Application.DTOs.AI;

namespace WildNatureExplorer.Application.Interfaces.Services;

public interface IAiService
{
    Task<Guid> AnalyzeImageAsync(Guid userId, byte[] imageBytes);
    Task<AnimalAnalysisResponseDto> AnalyzeImageStructuredAsync(Guid userId, byte[] imageBytes);
    Task<ChatResponseDto> AskAsync(Guid sessionId, string question);
    Task SubmitFeedbackAsync(Guid sessionId, int rating, string? comment);
}