using WildNatureExplorer.Application.DTOs.Users;
using WildNatureExplorer.Application.DTOs.Auth;
using WildNatureExplorer.Application.DTOs.AI;

namespace WildNatureExplorer.Application.Interfaces.Services;

public interface IAiService
{
    Task<Guid> AnalyzeImageAsync(Guid userId, byte[] imageBytes, string? recognizer = null);
    Task<AnimalAnalysisResponseDto> AnalyzeImageStructuredAsync(Guid userId, byte[] imageBytes, Guid? sessionId = null, string? recognizer = null);
    Task<ChatResponseDto> AskAsync(Guid userId, Guid? sessionId, string question);
    Task SubmitFeedbackAsync(Guid sessionId, int rating, string? comment);
    Task<Guid> StartSessionAsync(Guid userId, string? initialContext = null);
    Task EndSessionAsync(Guid sessionId);
}