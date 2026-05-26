using Moq;
using WildNatureExplorer.Application.Common;
using WildNatureExplorer.Application.DTOs.Users;
using WildNatureExplorer.Application.Interfaces.Repositories;
using WildNatureExplorer.Application.Services;
using WildNatureExplorer.Domain.Entities;
using Xunit;

namespace WildNatureExplorer.Tests.Unit;

public class UserServiceTests
{
    [Fact]
    [Trait("Category", "Unit")]
    public async Task GetUserAsync_Missing_ReturnsNull()
    {
        var repo = new Mock<IUserRepository>();
        repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((User?)null);

        var sut = new UserService(repo.Object);
        Assert.Null(await sut.GetUserAsync(Guid.NewGuid()));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task GetUserAsync_Found_ReturnsDto()
    {
        var repo = new Mock<IUserRepository>();
        var u = new User(Guid.NewGuid(), "p@x.com", "h", "P", "Q");
        repo.Setup(r => r.GetByIdAsync(u.Id)).ReturnsAsync(u);

        var sut = new UserService(repo.Object);
        var dto = await sut.GetUserAsync(u.Id);

        Assert.NotNull(dto);
        Assert.Equal("p@x.com", dto!.Email);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task UpdateProfileAsync_NewEmailWhenAvailable_Updates()
    {
        var repo = new Mock<IUserRepository>();
        var userId = Guid.NewGuid();
        var user = new User(userId, "old@x.com", "h", "O", "L");
        repo.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
        repo.Setup(r => r.GetByEmailAsync("new@x.com")).ReturnsAsync((User?)null);

        var sut = new UserService(repo.Object);
        await sut.UpdateProfileAsync(userId, new UpdateUserDto("O", "L", "new@x.com"));

        Assert.Equal("new@x.com", user.Email);
        repo.Verify(r => r.UpdateAsync(user), Times.Once);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task UpdateProfileAsync_SameEmailAllowed_Updates()
    {
        var repo = new Mock<IUserRepository>();
        var userId = Guid.NewGuid();
        var user = new User(userId, "old@x.com", "h", "O", "L");
        repo.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
        repo.Setup(r => r.GetByEmailAsync("old@x.com")).ReturnsAsync(user);

        var sut = new UserService(repo.Object);
        await sut.UpdateProfileAsync(userId, new UpdateUserDto("N", "M", "old@x.com"));

        repo.Verify(r => r.UpdateAsync(user), Times.Once);
        Assert.Equal("N", user.FirstName);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task UpdateProfileAsync_EmailTakenByOther_Throws()
    {
        var repo = new Mock<IUserRepository>();
        var userId = Guid.NewGuid();
        var user = new User(userId, "a@x.com", "h", "A", "A");
        var other = new User(Guid.NewGuid(), "b@x.com", "h", "B", "B");
        repo.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
        repo.Setup(r => r.GetByEmailAsync("b@x.com")).ReturnsAsync(other);

        var sut = new UserService(repo.Object);
        await Assert.ThrowsAsync<ValidationException>(() =>
            sut.UpdateProfileAsync(userId, new UpdateUserDto("A", "A", "b@x.com")));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task UpdateProfileAsync_UserMissing_ThrowsResourceNotFound()
    {
        var repo = new Mock<IUserRepository>();
        repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((User?)null);

        var sut = new UserService(repo.Object);
        await Assert.ThrowsAsync<ResourceNotFoundException>(() =>
            sut.UpdateProfileAsync(Guid.NewGuid(), new UpdateUserDto("a", "b", "e@e.com")));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task DeleteAccountAsync_Deactivates()
    {
        var repo = new Mock<IUserRepository>();
        var u = new User(Guid.NewGuid(), "d@x.com", "h", "D", "D");
        repo.Setup(r => r.GetByIdAsync(u.Id)).ReturnsAsync(u);

        var sut = new UserService(repo.Object);
        await sut.DeleteAccountAsync(u.Id);

        Assert.False(u.IsActive);
        repo.Verify(r => r.UpdateAsync(u), Times.Once);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task DeleteAccountAsync_UserMissing_Throws()
    {
        var repo = new Mock<IUserRepository>();
        repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((User?)null);

        var sut = new UserService(repo.Object);
        await Assert.ThrowsAsync<ResourceNotFoundException>(() => sut.DeleteAccountAsync(Guid.NewGuid()));
    }
}
