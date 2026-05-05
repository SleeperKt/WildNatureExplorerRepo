using WildNatureExplorer.Domain.Entities;
using Xunit;

namespace WildNatureExplorer.Tests.Unit;

public class UserSightingTests
{
    [Fact]
    [Trait("Category", "Unit")]
    public void Ctor_InvalidLatitude_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new UserSighting(Guid.NewGuid(), Guid.NewGuid(), null, "Bear", null, 91, 0, null, null, DateTime.UtcNow));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Ctor_EmptyCommonName_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            new UserSighting(Guid.NewGuid(), Guid.NewGuid(), null, " ", null, 0, 0, null, null, DateTime.UtcNow));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Ctor_TrimsWhitespaceNames()
    {
        var s = new UserSighting(Guid.NewGuid(), Guid.NewGuid(), null, "  Wolf  ", "  Canis  ", 10, 20, null, null, DateTime.UtcNow);
        Assert.Equal("Wolf", s.CommonName);
        Assert.Equal("Canis", s.ScientificName);
    }
}
