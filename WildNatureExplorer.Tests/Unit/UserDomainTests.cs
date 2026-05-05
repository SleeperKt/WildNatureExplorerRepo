using WildNatureExplorer.Domain.Entities;
using Xunit;

namespace WildNatureExplorer.Tests.Unit;

public class UserDomainTests
{
    [Fact]
    [Trait("Category", "Unit")]
    public void User_AddRole_IdempotentPerRole()
    {
        var user = new User(Guid.NewGuid(), "u@x.com", "h", "a", "b");
        var role = new Role(Guid.NewGuid(), "User", "");

        user.AddRole(role);
        user.AddRole(role);

        Assert.Single(user.UserRoles);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void User_Deactivate_MarksInactive()
    {
        var user = new User(Guid.NewGuid(), "u@x.com", "h", "a", "b");
        user.Deactivate();
        Assert.False(user.IsActive);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void User_UpdateProfile_ChangesFields()
    {
        var user = new User(Guid.NewGuid(), "old@x.com", "h", "a", "b");
        user.UpdateProfile("n", "m", "new@x.com");
        Assert.Equal("new@x.com", user.Email);
        Assert.Equal("n", user.FirstName);
        Assert.Equal("m", user.LastName);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void User_AcceptTerms_SetsFlags()
    {
        var user = new User(Guid.NewGuid(), "u@x.com", "h", "a", "b");
        user.AcceptTerms("9.9");
        Assert.True(user.AcceptedTerms);
        Assert.Equal("9.9", user.TermsVersion);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Role_ctor_TrimsNames()
    {
        var r = new Role(Guid.NewGuid(), "Admin", "desc");
        Assert.Equal("Admin", r.RoleName);
    }
}
