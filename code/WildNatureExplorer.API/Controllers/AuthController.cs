using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WildNatureExplorer.Application.DTOs.Auth;
using WildNatureExplorer.Application.DTOs.Users;
using WildNatureExplorer.Application.Interfaces.Services;

namespace WildNatureExplorer.API.Controllers;

/// <summary>
/// Registration, login, and acceptance of legal terms. Successful login returns a JWT used as Bearer token on protected routes.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Creates a new account with default User role (Admin when email matches configured admin bootstrap address).
    /// </summary>
    [HttpPost("register")]
    [SwaggerOperation(Summary = "Register a new user account.")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Register(RegisterUserDto request)
    {
        var user = await _authService.RegisterAsync(request);
        return Ok(user);
    }

    /// <summary>
    /// Validates credentials and returns a JWT plus terms acceptance flags.
    /// </summary>
    [HttpPost("login")]
    [SwaggerOperation(Summary = "Authenticate and obtain JWT access token.")]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Login(LoginUserDto request)
    {
        var result = await _authService.LoginAsync(request);
        return Ok(result);
    }

    /// <summary>
    /// Records that the authenticated user accepted the current terms version (JWT subject claim required).
    /// </summary>
    [HttpPost("accept-terms")]
    [SwaggerOperation(Summary = "Accept legal terms for the current user (Bearer JWT).")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> AcceptTerms()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (!Guid.TryParse(userId, out var guid))
            return Unauthorized();

        await _authService.AcceptTermsAsync(guid);

        return Ok();
    }
}
