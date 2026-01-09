using WildNatureExplorer.Application.DTOs.Users;
using WildNatureExplorer.Application.Interfaces.Repositories;
using WildNatureExplorer.Application.Interfaces.Services;
using WildNatureExplorer.Domain.Entities;

namespace WildNatureExplorer.Application.Services;

public class AdminService : IAdminService
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;

    public AdminService(IUserRepository userRepository, IRoleRepository roleRepository)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
    }

    public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
    {
        var users = await _userRepository.GetAllAsync();
        var result = users.Select(u => new UserDto(
            u.Id,
            u.Email,
            u.FirstName,
            u.LastName,
            u.IsActive
            ));
        return result;
    }

    public async Task AssignModeratorRoleAsync(Guid adminId, Guid userId)
    {
        var admin = await _userRepository.GetByIdAsync(adminId);
        if (admin == null || !admin.UserRoles.Any(ur => ur.Role.RoleName == "Admin"))
            throw new Exception("Only Admin can assign Moderator role.");

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new Exception("User not found.");

        var moderatorRole = await _roleRepository.GetByNameAsync("Moderator");
        if (moderatorRole == null)
            throw new Exception("Moderator role not found in database.");

        user.AddRole(moderatorRole);
        await _userRepository.UpdateAsync(user);
    }
}
