using WildNatureExplorer.Application.DTOs.Users;
using WildNatureExplorer.Application.DTOs.Auth;
using WildNatureExplorer.Application.Interfaces.Services;
using WildNatureExplorer.Application.Interfaces.Repositories;
using WildNatureExplorer.Domain.Entities;

namespace WildNatureExplorer.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<UserDto> RegisterAsync(RegisterUserDto registerDto)
    {
        var existingUser = await _userRepository.GetByEmailAsync(registerDto.Email);
        if (existingUser != null) throw new Exception("Email already in use");

        var passwordHash = _passwordHasher.HashPassword(registerDto.Password);
        var user = new User(Guid.NewGuid(), registerDto.Email, passwordHash, registerDto.FirstName, registerDto.LastName);

        await _userRepository.AddAsync(user);

        return new UserDto(user.Id, user.Email, user.FirstName, user.LastName, user.IsActive);
    }

    public async Task<string> LoginAsync(LoginUserDto loginDto)
    {
        var user = await _userRepository.GetByEmailAsync(loginDto.Email);
        if (user == null || !_passwordHasher.VerifyPassword(loginDto.Password, user.PasswordHash))
            throw new Exception("Invalid email or password");

        return _jwtTokenService.GenerateToken(user.Id, user.Email);
    }
}
