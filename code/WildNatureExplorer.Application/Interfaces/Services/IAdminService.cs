using WildNatureExplorer.Application.DTOs.Users;
using WildNatureExplorer.Domain.Entities;

namespace WildNatureExplorer.Application.Interfaces.Services;

public interface IAdminService
{
    Task<IEnumerable<UserDto>> GetAllUsersAsync();

    Task AssignModeratorRoleAsync(Guid adminId, Guid userId);
}
