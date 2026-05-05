using Microsoft.Extensions.Configuration;
using Moq;
using WildNatureExplorer.Application.Common;
using WildNatureExplorer.Application.DTOs.Auth;
using WildNatureExplorer.Application.Interfaces.Repositories;
using WildNatureExplorer.Application.Interfaces.Services;
using WildNatureExplorer.Application.Services;
using WildNatureExplorer.Domain.Entities;
using WildNatureExplorer.Infrastructure.Services;
using Xunit;

namespace WildNatureExplorer.Tests.Unit;

public class AuthServiceTests
{
    private static AuthService CreateSut(
        out Mock<IUserRepository> users,
        out Mock<IRoleRepository> roles,
        IConfiguration? configuration = null)
    {
        users = new Mock<IUserRepository>();
        roles = new Mock<IRoleRepository>();
        IPasswordHasher ph = new PasswordHasher();
        var jwt = new JwtTokenService(BuildJwtConfiguration());

        configuration ??= new ConfigurationBuilder().Build();

        return new AuthService(users.Object, ph, jwt, roles.Object, configuration);
    }

    private static IConfiguration BuildJwtConfiguration(string? adminEmail = null)
    {
        var d = new Dictionary<string, string?>
        {
            ["JWT_KEY"] = "K4t5h9wL8fJ2qP1vZ3yX8rQ0sM7bN6pD11!!",
            ["JWT_ISSUER"] = "issuer",
            ["JWT_AUDIENCE"] = "audience"
        };
        if (adminEmail != null)
            d["ADMIN_EMAIL"] = adminEmail;

        return new ConfigurationBuilder().AddInMemoryCollection(d!).Build();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task RegisterAsync_DuplicateEmail_ThrowsValidationException()
    {
        var sut = CreateSut(out var users, out var roles);
        users.Setup(u => u.GetByEmailAsync("dup@x.com")).ReturnsAsync(new User(Guid.NewGuid(), "dup@x.com", "h", "a", "b"));

        await Assert.ThrowsAsync<ValidationException>(() =>
            sut.RegisterAsync(new RegisterUserDto("dup@x.com", "Aa11!!aaaa", "A", "B")));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task RegisterAsync_DefaultUser_AssignsUserRole()
    {
        var sut = CreateSut(out var users, out var roles, BuildJwtConfiguration());
        users.Setup(u => u.GetByEmailAsync("new@x.com")).ReturnsAsync((User?)null);

        var role = new Role(Guid.NewGuid(), "User", "");
        roles.Setup(r => r.GetByNameAsync("User")).ReturnsAsync(role);

        var dto = await sut.RegisterAsync(new RegisterUserDto("new@x.com", "Aa11!!aaaa", "A", "B"));

        Assert.Equal("new@x.com", dto.Email);
        users.Verify(u => u.AddAsync(It.IsAny<User>()), Times.Once);
        users.Verify(u => u.UpdateAsync(It.Is<User>(usr => usr.UserRoles.Any())), Times.Once);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task RegisterAsync_AdminEmail_AssignsAdminRole()
    {
        var sut = CreateSut(out var users, out var roles, BuildJwtConfiguration(adminEmail: "boss@x.com"));
        users.Setup(u => u.GetByEmailAsync("boss@x.com")).ReturnsAsync((User?)null);

        var adminRole = new Role(Guid.NewGuid(), "Admin", "");
        roles.Setup(r => r.GetByNameAsync("Admin")).ReturnsAsync(adminRole);

        await sut.RegisterAsync(new RegisterUserDto("boss@x.com", "Aa11!!bbbb", "A", "B"));

        roles.Verify(r => r.GetByNameAsync("Admin"), Times.Once);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task LoginAsync_InvalidCredentials_Throws()
    {
        var sut = CreateSut(out var users, out _);
        users.Setup(u => u.GetByEmailAsync("x@x.com")).ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<ValidationException>(() =>
            sut.LoginAsync(new LoginUserDto("x@x.com", "Aa11!!aaaa")));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task LoginAsync_Valid_ReturnsTokenAndTermsFlags()
    {
        var sut = CreateSut(out var users, out var roles);
        var ph = new PasswordHasher();
        var hash = ph.HashPassword("Aa11!!aaaa");
        var user = new User(Guid.NewGuid(), "ok@x.com", hash, "F", "L");

        users.Setup(u => u.GetByEmailAsync("ok@x.com")).ReturnsAsync(user);
        roles.Setup(r => r.GetRolesByUserIdAsync(user.Id)).ReturnsAsync(Array.Empty<Role>());

        var result = await sut.LoginAsync(new LoginUserDto("ok@x.com", "Aa11!!aaaa"));

        Assert.False(string.IsNullOrEmpty(result.Token));
        Assert.False(result.AcceptedTerms);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task AssignRoleAsync_MissingRole_ThrowsResourceNotFound()
    {
        var sut = CreateSut(out var users, out var roles);
        roles.Setup(r => r.GetByNameAsync("Ghost")).ReturnsAsync((Role?)null);
        var user = new User(Guid.NewGuid(), "u@x.com", "h", "a", "b");

        await Assert.ThrowsAsync<ResourceNotFoundException>(() => sut.AssignRoleAsync(user, "Ghost"));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task AcceptTermsAsync_MissingUser_Throws()
    {
        var sut = CreateSut(out var users, out _);
        var id = Guid.NewGuid();
        users.Setup(u => u.GetByIdAsync(id)).ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<ResourceNotFoundException>(() => sut.AcceptTermsAsync(id));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task AcceptTermsAsync_SetsTermsVersion()
    {
        var sut = CreateSut(out var users, out _);
        var user = new User(Guid.NewGuid(), "u@x.com", "h", "a", "b");
        users.Setup(u => u.GetByIdAsync(user.Id)).ReturnsAsync(user);

        await sut.AcceptTermsAsync(user.Id);

        Assert.True(user.AcceptedTerms);
        Assert.Equal(WildNatureExplorer.Application.Common.Terms.CurrentVersion, user.TermsVersion);
        users.Verify(u => u.UpdateAsync(user), Times.Once);
    }
}
