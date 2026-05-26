using WildNatureExplorer.Application.DTOs.Users;

namespace WildNatureExplorer.Application.Interfaces.Services;

public interface IUserService
{
    Task<UserDto?> GetUserAsync(Guid userId);
    Task UpdateProfileAsync(Guid userId, UpdateUserDto updateDto);
    Task DeleteAccountAsync(Guid userId);
}
