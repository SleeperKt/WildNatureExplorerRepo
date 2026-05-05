using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using WildNatureExplorer.Domain.Entities;
using WildNatureExplorer.Infrastructure.Services;
using Xunit;

namespace WildNatureExplorer.Tests.Unit;

public class JwtTokenServiceTests
{
    private static IConfiguration BuildConfig(string? jwtKey = null)
    {
        var dict = new Dictionary<string, string?>
        {
            ["JWT_KEY"] = jwtKey ?? "K4t5h9wL8fJ2qP1vZ3yX8rQ0sM7bN6pD11!!",
            ["JWT_ISSUER"] = "TestIssuer",
            ["JWT_AUDIENCE"] = "TestAudience"
        };
        return new ConfigurationBuilder().AddInMemoryCollection(dict!).Build();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void GenerateToken_ReturnsParsableJwt_WithRoles()
    {
        var service = new JwtTokenService(BuildConfig());
        var roles = new List<Role>
        {
            new(Guid.NewGuid(), "Admin", "a"),
            new(Guid.NewGuid(), "User", "u")
        };

        var token = service.GenerateToken(Guid.NewGuid(), "test@example.com", roles);

        Assert.False(string.IsNullOrEmpty(token));

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);
        Assert.Contains(jwt.Claims, c => c.Type == ClaimTypes.Role && c.Value == "Admin");
        Assert.Contains(jwt.Claims, c => c.Type == ClaimTypes.Role && c.Value == "User");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void GenerateToken_MissingJwtKey_Throws()
    {
        var cfg = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["JWT_ISSUER"] = "x",
            ["JWT_AUDIENCE"] = "y"
        }).Build();
        var service = new JwtTokenService(cfg);

        Assert.Throws<InvalidOperationException>(() =>
            service.GenerateToken(Guid.NewGuid(), "e@e.com", Array.Empty<Role>()));
    }
}
