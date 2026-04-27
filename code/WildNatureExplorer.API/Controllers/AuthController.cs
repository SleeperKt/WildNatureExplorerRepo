using Microsoft.AspNetCore.Mvc;
using WildNatureExplorer.Application.Interfaces.Services;
using WildNatureExplorer.Application.DTOs.Auth;
using WildNatureExplorer.Application.DTOs.Users;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterUserDto request)
    {
        var user = await _authService.RegisterAsync(request);
        return Ok(user);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginUserDto request)
    {
        var result = await _authService.LoginAsync(request);
        return Ok(result);
    }

    [HttpPost("accept-terms")]
    public async Task<IActionResult> AcceptTerms()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (!Guid.TryParse(userId, out var guid))
            return Unauthorized();

        await _authService.AcceptTermsAsync(guid);

        return Ok();
    }
}