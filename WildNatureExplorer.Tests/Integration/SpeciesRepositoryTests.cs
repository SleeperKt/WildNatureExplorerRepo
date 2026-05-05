using Microsoft.EntityFrameworkCore;
using WildNatureExplorer.Domain.Entities;
using WildNatureExplorer.Infrastructure.Data;
using WildNatureExplorer.Infrastructure.Repositories;
using WildNatureExplorer.Tests.TestSupport;
using Xunit;

namespace WildNatureExplorer.Tests.Integration;

public class SpeciesRepositoryTests
{
    [Fact]
    [Trait("Category", "Integration")]
    public async Task AddAndGetById_LoadsIncludes_SizeAndCountries()
    {
        await using var ctx = InfrastructureIntegrationFixture.CreateContext();

        var size = new Size { Name = "Large", NormalizedName = "LARGE" }.WithTestId(Guid.NewGuid());
        var country = new Country { Name = "Kenya", NormalizedName = "KENYA" }.WithTestId(Guid.NewGuid());
        ctx.Sizes.Add(size);
        ctx.Countries.Add(country);
        await ctx.SaveChangesAsync();

        var speciesId = Guid.NewGuid();
        var species = new Species
        {
            CommonName = "Savannah Elephant",
            ScientificName = "Loxodonta africana",
            Description = "",
            IsDangerous = false,
            IsRare = false,
            SizeId = size.Id
        }.WithTestId(speciesId);
        species.Countries.Add(new SpeciesCountry { CountryId = country.Id });

        ctx.Species.Add(species);
        await ctx.SaveChangesAsync();

        var repo = new SpeciesRepository(ctx);
        var loaded = await repo.GetByIdAsync(speciesId);

        Assert.NotNull(loaded);
        Assert.Equal("Savannah Elephant", loaded!.CommonName);
        Assert.NotNull(loaded.Size);
        Assert.Single(loaded.Countries);
        Assert.Equal(country.Id, loaded.Countries.First().CountryId);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task SearchNamesAsync_ReturnsPrefixMatches_Ordered()
    {
        await using var ctx = InfrastructureIntegrationFixture.CreateContext();
        await SeedMinimalSpeciesAsync(ctx, "Tiger");
        await SeedMinimalSpeciesAsync(ctx, "Tiger Beetle");
        await SeedMinimalSpeciesAsync(ctx, "Zebra");

        var repo = new SpeciesRepository(ctx);
        var names = await repo.SearchNamesAsync("Tiger");

        Assert.Equal(new[] { "Tiger", "Tiger Beetle" }, names);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetByCommonNameAsync_IsCaseInsensitive()
    {
        await using var ctx = InfrastructureIntegrationFixture.CreateContext();
        await SeedMinimalSpeciesAsync(ctx, "Red Fox");

        var repo = new SpeciesRepository(ctx);
        var hit = await repo.GetByCommonNameAsync("RED FOX");

        Assert.NotNull(hit);
        Assert.Equal("Red Fox", hit!.CommonName);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task SearchAsync_FiltersByCountryId()
    {
        await using var ctx = InfrastructureIntegrationFixture.CreateContext();

        var size = new Size { Name = "M", NormalizedName = "M" }.WithTestId(Guid.NewGuid());
        var italy = new Country { Name = "Italy", NormalizedName = "ITALY" }.WithTestId(Guid.NewGuid());
        var spain = new Country { Name = "Spain", NormalizedName = "SPAIN" }.WithTestId(Guid.NewGuid());
        ctx.Sizes.Add(size);
        ctx.Countries.AddRange(italy, spain);
        await ctx.SaveChangesAsync();

        var inItaly = new Species
        {
            CommonName = "Italian Sparrow",
            ScientificName = "Passer italiae",
            Description = "",
            IsDangerous = false,
            IsRare = false,
            SizeId = size.Id
        }.WithTestId(Guid.NewGuid());
        inItaly.Countries.Add(new SpeciesCountry { CountryId = italy.Id });

        var inSpain = new Species
        {
            CommonName = "Iberian Lynx",
            ScientificName = "Lynx pardinus",
            Description = "",
            IsDangerous = true,
            IsRare = true,
            SizeId = size.Id
        }.WithTestId(Guid.NewGuid());
        inSpain.Countries.Add(new SpeciesCountry { CountryId = spain.Id });

        ctx.Species.AddRange(inItaly, inSpain);
        await ctx.SaveChangesAsync();

        var repo = new SpeciesRepository(ctx);
        var results = await repo.SearchAsync(null, null, new List<Guid> { italy.Id }, null, null, null);

        Assert.Single(results);
        Assert.Equal("Italian Sparrow", results[0].CommonName);
    }

    private static async Task SeedMinimalSpeciesAsync(AppDbContext ctx, string commonName)
    {
        var size = await ctx.Sizes.FirstOrDefaultAsync();
        if (size == null)
        {
            size = new Size { Name = "Any", NormalizedName = "ANY" }.WithTestId(Guid.NewGuid());
            ctx.Sizes.Add(size);
            await ctx.SaveChangesAsync();
        }

        ctx.Species.Add(new Species
        {
            CommonName = commonName,
            ScientificName = "sp.",
            Description = "",
            IsDangerous = false,
            IsRare = false,
            SizeId = size.Id
        }.WithTestId(Guid.NewGuid()));

        await ctx.SaveChangesAsync();
    }
}
