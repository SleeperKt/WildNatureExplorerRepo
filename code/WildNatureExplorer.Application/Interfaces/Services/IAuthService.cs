using WildNatureExplorer.Application.DTOs.Users;
using WildNatureExplorer.Application.DTOs.Auth;

namespace WildNatureExplorer.Application.Interfaces.Services;

public interface IAuthService
{
    Task<UserDto> RegisterAsync(RegisterUserDto registerDto);
    Task<string> LoginAsync(LoginUserDto loginDto);
}
