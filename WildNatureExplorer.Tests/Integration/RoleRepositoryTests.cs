using WildNatureExplorer.Domain.Entities;
using WildNatureExplorer.Infrastructure.Repositories;
using Xunit;

namespace WildNatureExplorer.Tests.Integration;

public class RoleRepositoryTests
{
    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetByNameAsync_ReturnsSeededRole()
    {
        await using var ctx = InfrastructureIntegrationFixture.CreateContext();

        var roleId = Guid.NewGuid();
        ctx.Roles.Add(new Role(roleId, "Moderator", "mod"));
        await ctx.SaveChangesAsync();

        var repo = new RoleRepository(ctx);
        var role = await repo.GetByNameAsync("Moderator");

        Assert.NotNull(role);
        Assert.Equal(roleId, role!.Id);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetRolesByUserIdAsync_JoinsUserRoles()
    {
        await using var ctx = InfrastructureIntegrationFixture.CreateContext();

        var roleUser = new Role(Guid.NewGuid(), "User", "u");
        var roleAdmin = new Role(Guid.NewGuid(), "Admin", "a");
        ctx.Roles.AddRange(roleUser, roleAdmin);

        var userId = Guid.NewGuid();
        var user = new User(userId, "multi@example.com", "hash", "M", "R");
        user.AddRole(roleUser);
        user.AddRole(roleAdmin);
        ctx.Users.Add(user);

        await ctx.SaveChangesAsync();

        var repo = new RoleRepository(ctx);
        var roles = (await repo.GetRolesByUserIdAsync(userId)).ToList();

        Assert.Equal(2, roles.Count);
        Assert.Contains(roles, r => r.RoleName == "User");
        Assert.Contains(roles, r => r.RoleName == "Admin");
    }
}
