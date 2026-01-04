using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WildNatureExplorer.Application.Interfaces.Services;
using WildNatureExplorer.Application.DTOs.Users;

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

    [HttpGet("me")]
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


    [HttpPut("me")]
    public async Task<IActionResult> UpdateProfile(UpdateUserDto request)
    {
        await _userService.UpdateProfileAsync(GetUserId(), request);
        return NoContent();
    }

    [HttpDelete("me")]
    public async Task<IActionResult> DeleteAccount()
    {
        await _userService.DeleteAccountAsync(GetUserId());
        return NoContent();
    }
}
