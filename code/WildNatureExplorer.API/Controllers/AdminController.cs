using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WildNatureExplorer.Application.DTOs.Users;
using WildNatureExplorer.Application.Interfaces.Repositories;
using WildNatureExplorer.Application.Interfaces.Services;

namespace WildNatureExplorer.API.Controllers;

/// <summary>
/// Administrative operations restricted to users in the Admin role.
/// </summary>
[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;

    public AdminController(
        IAdminService adminService,
        IUserRepository userRepository,
        IRoleRepository roleRepository)
    {
        _adminService = adminService;
        _userRepository = userRepository;
        _roleRepository = roleRepository;
    }

    /// <summary>
    /// Lists every registered user (dashboard / moderation workflows).
    /// </summary>
    [HttpGet("users")]
    [SwaggerOperation(Summary = "List all users (Admin only).")]
    [ProducesResponseType(typeof(IEnumerable<UserDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _adminService.GetAllUsersAsync();
        return Ok(users);
    }

    /// <summary>
    /// Grants the Moderator role to an existing user idempotent-style (role appended when missing).
    /// </summary>
    [HttpPost("users/{userId}/moderator")]
    [SwaggerOperation(Summary = "Promote user to Moderator role.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SetModerator(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) return NotFound("User not found");

        var moderatorRole = await _roleRepository.GetByNameAsync("Moderator");
        if (moderatorRole == null) return BadRequest("Moderator role not found");

        user.AddRole(moderatorRole);
        await _userRepository.UpdateAsync(user);

        return Ok(new { Message = $"User {user.Email} is now a Moderator" });
    }
}
