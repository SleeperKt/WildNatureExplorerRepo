using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WildNatureExplorer.Application.DTOs.Users;
using WildNatureExplorer.Application.Interfaces.Services;

namespace WildNatureExplorer.API.Controllers;

/// <summary>
/// Profile operations for the authenticated user (<c>/me</c>).
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new Exception("User not found"));

    /// <summary>
    /// Returns first name, last name, and email for the JWT subject.
    /// </summary>
    [HttpGet("me")]
    [SwaggerOperation(Summary = "Get current user profile.")]
    [ProducesResponseType(typeof(UpdateUserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMe()
    {
        var user = await _userService.GetUserAsync(GetUserId());
        if (user == null) return NotFound(new { message = "User not found" });

        var response = new UpdateUserDto(
            FirstName: user.FirstName,
            LastName: user.LastName,
            Email: user.Email
        );

        return Ok(response);
    }

    /// <summary>
    /// Updates profile fields for the JWT subject.
    /// </summary>
    [HttpPut("me")]
    [SwaggerOperation(Summary = "Update current user profile.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateProfile(UpdateUserDto request)
    {
        await _userService.UpdateProfileAsync(GetUserId(), request);
        return NoContent();
    }

    /// <summary>
    /// Permanently deletes the JWT subject account.
    /// </summary>
    [HttpDelete("me")]
    [SwaggerOperation(Summary = "Delete current user account.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteAccount()
    {
        await _userService.DeleteAccountAsync(GetUserId());
        return NoContent();
    }
}
