using Moq;
using WildNatureExplorer.Application.Common;
using WildNatureExplorer.Application.Interfaces.Repositories;
using WildNatureExplorer.Application.Services;
using WildNatureExplorer.Domain.Entities;
using Xunit;

namespace WildNatureExplorer.Tests.Unit;

public class AdminServiceTests
{
    [Fact]
    [Trait("Category", "Unit")]
    public async Task GetAllUsersAsync_MapsRepositoryRows()
    {
        var users = new Mock<IUserRepository>();
        var roles = new Mock<IRoleRepository>();
        var u = new User(Guid.NewGuid(), "a@x.com", "h", "A", "B");
        users.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<User> { u });

        var sut = new AdminService(users.Object, roles.Object);
        var list = (await sut.GetAllUsersAsync()).ToList();

        Assert.Single(list);
        Assert.Equal("a@x.com", list[0].Email);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task AssignModeratorRoleAsync_UnauthorizedWhenCallerNotAdmin()
    {
        var users = new Mock<IUserRepository>();
        var roles = new Mock<IRoleRepository>();

        var adminId = Guid.NewGuid();
        var adminRole = new Role(Guid.NewGuid(), "Admin", "");
        roles.Setup(r => r.GetByNameAsync("Admin")).ReturnsAsync(adminRole);

        var badAdmin = new User(adminId, "a@x.com", "h", "A", "A");
        users.Setup(r => r.GetByIdAsync(adminId)).ReturnsAsync(badAdmin);

        var sut = new AdminService(users.Object, roles.Object);
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            sut.AssignModeratorRoleAsync(adminId, Guid.NewGuid()));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task AssignModeratorRoleAsync_TargetMissing_ThrowsResourceNotFound()
    {
        var users = new Mock<IUserRepository>();
        var roles = new Mock<IRoleRepository>();

        var adminId = Guid.NewGuid();
        var adminRole = new Role(Guid.NewGuid(), "Admin", "");
        roles.Setup(r => r.GetByNameAsync("Admin")).ReturnsAsync(adminRole);

        var admin = new User(adminId, "admin@x.com", "h", "A", "A");
        admin.AddRole(adminRole);
        users.Setup(r => r.GetByIdAsync(adminId)).ReturnsAsync(admin);

        var missing = Guid.NewGuid();
        users.Setup(r => r.GetByIdAsync(missing)).ReturnsAsync((User?)null);

        var sut = new AdminService(users.Object, roles.Object);
        await Assert.ThrowsAsync<ResourceNotFoundException>(() =>
            sut.AssignModeratorRoleAsync(adminId, missing));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task AssignModeratorRoleAsync_ModeratorRoleMissing_Throws()
    {
        var users = new Mock<IUserRepository>();
        var roles = new Mock<IRoleRepository>();

        var adminId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var adminRole = new Role(Guid.NewGuid(), "Admin", "");

        roles.Setup(r => r.GetByNameAsync("Admin")).ReturnsAsync(adminRole);
        roles.Setup(r => r.GetByNameAsync("Moderator")).ReturnsAsync((Role?)null);

        var admin = new User(adminId, "admin@x.com", "h", "A", "A");
        admin.AddRole(adminRole);
        var target = new User(userId, "u@x.com", "h", "U", "U");

        users.Setup(r => r.GetByIdAsync(adminId)).ReturnsAsync(admin);
        users.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(target);

        var sut = new AdminService(users.Object, roles.Object);
        await Assert.ThrowsAsync<ResourceNotFoundException>(() =>
            sut.AssignModeratorRoleAsync(adminId, userId));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task AssignModeratorRoleAsync_Success_AddsRoleAndSaves()
    {
        var users = new Mock<IUserRepository>();
        var roles = new Mock<IRoleRepository>();

        var adminId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var adminRole = new Role(Guid.NewGuid(), "Admin", "");
        var modRole = new Role(Guid.NewGuid(), "Moderator", "m");

        roles.Setup(r => r.GetByNameAsync("Admin")).ReturnsAsync(adminRole);
        roles.Setup(r => r.GetByNameAsync("Moderator")).ReturnsAsync(modRole);

        var admin = new User(adminId, "admin@x.com", "h", "A", "A");
        admin.AddRole(adminRole);

        var target = new User(userId, "u@x.com", "h", "U", "U");

        users.Setup(r => r.GetByIdAsync(adminId)).ReturnsAsync(admin);
        users.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(target);

        var sut = new AdminService(users.Object, roles.Object);
        await sut.AssignModeratorRoleAsync(adminId, userId);

        Assert.Contains(target.UserRoles, ur => ur.RoleId == modRole.Id);
        users.Verify(r => r.UpdateAsync(target), Times.Once);
    }
}
