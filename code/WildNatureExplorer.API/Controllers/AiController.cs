using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WildNatureExplorer.Application.Interfaces.Services;
using WildNatureExplorer.Application.DTOs.AI;

namespace WildNatureExplorer.API.Controllers;

[ApiController]
[Route("api/ai")]
[Authorize]
public class AiController : ControllerBase
{
    private readonly IAiService _ai;

    public AiController(IAiService ai)
    {
        _ai = ai;
    }

    [HttpPost("analyze/{sessionId}")]
    public async Task<IActionResult> Analyze(Guid sessionId, IFormFile image)
    {
        using var ms = new MemoryStream();
        await image.CopyToAsync(ms);

        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _ai.AnalyzeImageStructuredAsync(userId, ms.ToArray(), sessionId);

        return Ok(result);
    }

    [HttpPost("start")]
    public async Task<IActionResult> Start()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var sessionId = await _ai.StartSessionAsync(userId);
        return Ok(new { SessionId = sessionId });
    }

    [HttpPost("ask/{sessionId}")]
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
        // Try common names and casing variations
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
        // If it's a raw string
        if (body.ValueKind == System.Text.Json.JsonValueKind.String) return body.GetString();
        return null;
    }

    [HttpPost("feedback/{sessionId}")]
    public async Task<IActionResult> Feedback(Guid sessionId, [FromBody] AiFeedbackDto dto)
    {
        await _ai.SubmitFeedbackAsync(sessionId, dto.Rating, dto.Comment);
        return Ok();
    }

    [HttpPost("end/{sessionId}")]
    public async Task<IActionResult> EndSession(Guid sessionId)
    {
        await _ai.EndSessionAsync(sessionId);
        return Ok();
    }
}