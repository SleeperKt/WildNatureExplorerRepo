using Microsoft.EntityFrameworkCore;
using WildNatureExplorer.Domain.Entities;
using WildNatureExplorer.Infrastructure.Repositories;
using Xunit;

namespace WildNatureExplorer.Tests.Integration;

public class UserRepositoryTests
{
    [Fact]
    [Trait("Category", "Integration")]
    public async Task AddUser_Should_AddUserToDatabase()
    {
        await using var context = InfrastructureIntegrationFixture.CreateContext();
        var repo = new UserRepository(context);
        var user = new User(
            Guid.NewGuid(),
            "test@example.com",
            "hash",
            "FirstName",
            "LastName");

        await repo.AddAsync(user);

        var userFromDb = await context.Users.FirstOrDefaultAsync(u => u.Email == "test@example.com");
        Assert.NotNull(userFromDb);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetByEmailAsync_IncludesRoles_WhenAssigned()
    {
        await using var context = InfrastructureIntegrationFixture.CreateContext();

        var role = new Role(Guid.NewGuid(), "User", "");
        context.Roles.Add(role);

        var userId = Guid.NewGuid();
        var user = new User(userId, "linked@example.com", "hash", "L", "U");
        user.AddRole(role);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var repo = new UserRepository(context);
        var loaded = await repo.GetByEmailAsync("linked@example.com");

        Assert.NotNull(loaded);
        Assert.Single(loaded!.UserRoles);
        Assert.Equal("User", loaded.UserRoles.First().Role.RoleName);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task UpdateAsync_PersistsChanges()
    {
        await using var context = InfrastructureIntegrationFixture.CreateContext();
        var user = new User(Guid.NewGuid(), "mut@example.com", "hash", "Old", "Name");
        context.Users.Add(user);
        await context.SaveChangesAsync();

        user.UpdateProfile("New", "Surname", "mut@example.com");

        var repo = new UserRepository(context);
        await repo.UpdateAsync(user);

        var fromDb = await context.Users.AsNoTracking().FirstAsync(u => u.Id == user.Id);
        Assert.Equal("New", fromDb.FirstName);
        Assert.Equal("Surname", fromDb.LastName);
    }
}
