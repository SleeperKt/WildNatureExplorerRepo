using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Swashbuckle.AspNetCore.Annotations;
using WildNatureExplorer.Application.Interfaces.Services;
using WildNatureExplorer.Application.DTOs.AI;

namespace WildNatureExplorer.API.Controllers;

/// <summary>
/// AI-assisted wildlife image analysis and chat (JWT required; fixed-window rate limiting on this controller).
/// </summary>
[ApiController]
[Route("api/ai")]
[Authorize]
[EnableRateLimiting("AiEndpoints")]
public class AiController : ControllerBase
{
    private readonly IAiService _ai;

    public AiController(IAiService ai)
    {
        _ai = ai;
    }

    [HttpPost("analyze/{sessionId}")]
    [SwaggerOperation(Summary = "Analyze an uploaded wildlife image for the authenticated user session.")]
    [ProducesResponseType(typeof(AnimalAnalysisResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Analyze(Guid sessionId, IFormFile image, [FromQuery] string? recognizer = null)
    {
        using var ms = new MemoryStream();
        await image.CopyToAsync(ms);

        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _ai.AnalyzeImageStructuredAsync(userId, ms.ToArray(), sessionId, recognizer);

        return Ok(result);
    }

    [HttpPost("start")]
    [SwaggerOperation(Summary = "Start an AI chat/analysis session.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Start()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var sessionId = await _ai.StartSessionAsync(userId);
        return Ok(new { SessionId = sessionId });
    }

    [HttpPost("ask/{sessionId}")]
    [SwaggerOperation(Summary = "Ask the wildlife assistant in an existing session (JSON \"questionAboutNature\", \"QuestionAboutNature\", or \"question\"/\"Question\").")]
    [ProducesResponseType(typeof(ChatResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Ask(Guid sessionId, [FromBody] System.Text.Json.JsonElement body)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var question = ExtractQuestionFromBody(body);
        if (string.IsNullOrWhiteSpace(question)) return BadRequest(new { message = "Missing question" });
        var answer = await _ai.AskAsync(userId, sessionId, question);
        return Ok(answer);
    }

    private static string? ExtractQuestionFromBody(System.Text.Json.JsonElement body)
    {
        if (body.ValueKind == System.Text.Json.JsonValueKind.Object)
        {
            if (body.TryGetProperty("questionAboutNature", out var p) && p.ValueKind == System.Text.Json.JsonValueKind.String)
                return p.GetString();
            if (body.TryGetProperty("QuestionAboutNature", out p) && p.ValueKind == System.Text.Json.JsonValueKind.String)
                return p.GetString();
            if (body.TryGetProperty("question", out p) && p.ValueKind == System.Text.Json.JsonValueKind.String)
                return p.GetString();
            if (body.TryGetProperty("Question", out p) && p.ValueKind == System.Text.Json.JsonValueKind.String)
                return p.GetString();
        }

        if (body.ValueKind == System.Text.Json.JsonValueKind.String) return body.GetString();
        return null;
    }

    [HttpPost("feedback/{sessionId}")]
    [SwaggerOperation(Summary = "Submit end-of-session feedback.")]
    public async Task<IActionResult> Feedback(Guid sessionId, [FromBody] AiFeedbackDto dto)
    {
        await _ai.SubmitFeedbackAsync(sessionId, dto.Rating, dto.Comment);
        return Ok();
    }

    [HttpPost("end/{sessionId}")]
    [SwaggerOperation(Summary = "Mark the AI session as ended.")]
    public async Task<IActionResult> EndSession(Guid sessionId)
    {
        await _ai.EndSessionAsync(sessionId);
        return Ok();
    }
}