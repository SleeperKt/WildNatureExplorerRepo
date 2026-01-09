using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WildNatureExplorer.Application.Interfaces.Services;
using WildNatureExplorer.Application.DTOs.Users;
using WildNatureExplorer.Application.Interfaces.Repositories;

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

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _adminService.GetAllUsersAsync();
        return Ok(users);
    }

    [HttpPost("users/{userId}/moderator")]
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
