using WildNatureExplorer.Domain.Entities;
using WildNatureExplorer.Infrastructure.Repositories;
using WildNatureExplorer.Tests.TestSupport;
using Xunit;

namespace WildNatureExplorer.Tests.Integration;

public class UserSightingRepositoryTests
{
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Add_GetById_IncludesSpeciesWhenLinked()
    {
        await using var ctx = InfrastructureIntegrationFixture.CreateContext();

        var userId = Guid.NewGuid();
        var user = new User(userId, "wild@example.com", "hash", "Ada", "Lovelace");
        ctx.Users.Add(user);

        var size = new Size { Name = "S", NormalizedName = "S" }.WithTestId(Guid.NewGuid());
        ctx.Sizes.Add(size);
        await ctx.SaveChangesAsync();

        var species = new Species
        {
            CommonName = "Brown Bear",
            ScientificName = "Ursus arctos",
            Description = "",
            IsDangerous = true,
            IsRare = false,
            SizeId = size.Id
        }.WithTestId(Guid.NewGuid());
        ctx.Species.Add(species);
        await ctx.SaveChangesAsync();

        var sightingId = Guid.NewGuid();
        var sighting = new UserSighting(
            sightingId,
            userId,
            species.Id,
            species.CommonName,
            species.ScientificName,
            62.5,
            25.8,
            null,
            null,
            DateTime.UtcNow);

        var repo = new UserSightingRepository(ctx);
        await repo.AddAsync(sighting);

        var loaded = await repo.GetByIdAsync(sightingId);

        Assert.NotNull(loaded);
        Assert.NotNull(loaded!.Species);
        Assert.Equal("Brown Bear", loaded.Species!.CommonName);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetByUserAsync_ReturnsOnlyThatUser_OrderedBySightedAtDesc()
    {
        await using var ctx = InfrastructureIntegrationFixture.CreateContext();

        var userA = Guid.NewGuid();
        var userB = Guid.NewGuid();
        ctx.Users.Add(new User(userA, "a@x.com", "h", "A", "A"));
        ctx.Users.Add(new User(userB, "b@x.com", "h", "B", "B"));

        var size = new Size { Name = "S", NormalizedName = "S" }.WithTestId(Guid.NewGuid());
        ctx.Sizes.Add(size);
        await ctx.SaveChangesAsync();

        var older = new UserSighting(
            Guid.NewGuid(),
            userA,
            null,
            "Older",
            null,
            0,
            0,
            null,
            null,
            DateTime.UtcNow.AddDays(-2));

        var newer = new UserSighting(
            Guid.NewGuid(),
            userA,
            null,
            "Newer",
            null,
            1,
            1,
            null,
            null,
            DateTime.UtcNow);

        ctx.UserSightings.AddRange(older, newer);
        ctx.UserSightings.Add(new UserSighting(
            Guid.NewGuid(),
            userB,
            null,
            "OtherUser",
            null,
            2,
            2,
            null,
            null,
            DateTime.UtcNow));

        await ctx.SaveChangesAsync();

        var repo = new UserSightingRepository(ctx);
        var rows = await repo.GetByUserAsync(userA);

        Assert.Equal(2, rows.Count);
        Assert.Equal("Newer", rows[0].CommonName);
        Assert.Equal("Older", rows[1].CommonName);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task DeleteAsync_RemovesRow()
    {
        await using var ctx = InfrastructureIntegrationFixture.CreateContext();

        var userId = Guid.NewGuid();
        ctx.Users.Add(new User(userId, "u@x.com", "h", "U", "U"));

        var size = new Size { Name = "S", NormalizedName = "S" }.WithTestId(Guid.NewGuid());
        ctx.Sizes.Add(size);
        await ctx.SaveChangesAsync();

        var id = Guid.NewGuid();
        var row = new UserSighting(id, userId, null, "Fox", null, 10, 10, null, null, DateTime.UtcNow);
        ctx.UserSightings.Add(row);
        await ctx.SaveChangesAsync();

        var repo = new UserSightingRepository(ctx);
        await repo.DeleteAsync(row);

        Assert.Null(await repo.GetByIdAsync(id));
    }
}
