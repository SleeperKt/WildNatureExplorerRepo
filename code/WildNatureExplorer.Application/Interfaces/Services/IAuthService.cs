using WildNatureExplorer.Application.DTOs.Auth;
using WildNatureExplorer.Application.DTOs.Users;

namespace WildNatureExplorer.Application.Interfaces.Services;

public interface IAuthService
{
    Task<UserDto> RegisterAsync(RegisterUserDto registerDto);
    Task<LoginResponseDto> LoginAsync(LoginUserDto loginDto);
    Task AcceptTermsAsync(Guid userId);
}