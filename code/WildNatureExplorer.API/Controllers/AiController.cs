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

    [HttpPost("analyze")]
    public async Task<IActionResult> Analyze(IFormFile image)
    {
        using var ms = new MemoryStream();
        await image.CopyToAsync(ms);

        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var sessionId = await _ai.AnalyzeImageAsync(userId, ms.ToArray());

        return Ok(new { sessionId });
    }

    [HttpPost("ask/{sessionId}")]
    public async Task<IActionResult> Ask(Guid sessionId, [FromBody] string question)
    {
        var answer = await _ai.AskAsync(sessionId, question);
        return Ok(answer);
    }

    [HttpPost("feedback/{sessionId}")]
    public async Task<IActionResult> Feedback(Guid sessionId, [FromBody] AiFeedbackDto dto)
    {
        await _ai.SubmitFeedbackAsync(sessionId, dto.Rating, dto.Comment);
        return Ok();
    }
}
