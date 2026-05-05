using WildNatureExplorer.Application.DTOs.Users;
using WildNatureExplorer.Application.Interfaces.Services;
using WildNatureExplorer.Application.Interfaces.Repositories;
using WildNatureExplorer.Domain.Entities;
using WildNatureExplorer.Application.Common;

namespace WildNatureExplorer.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserDto?> GetUserAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) return null;

        return new UserDto(user.Id, user.Email, user.FirstName, user.LastName, user.IsActive);
    }

    public async Task UpdateProfileAsync(Guid userId, UpdateUserDto updateDto)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) throw new ResourceNotFoundException("User", userId.ToString());

        user.UpdateProfile(updateDto.FirstName, updateDto.LastName, updateDto.Email);
        var existingUser = await _userRepository.GetByEmailAsync(updateDto.Email);
        if (existingUser != null && existingUser.Id != userId)
            throw new ValidationException("Email already in use", "EMAIL_ALREADY_IN_USE");
        await _userRepository.UpdateAsync(user);
    }

    public async Task DeleteAccountAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) throw new ResourceNotFoundException("User", userId.ToString());

        user.Deactivate();
        await _userRepository.UpdateAsync(user);
    }
}
