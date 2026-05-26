using Moq;
using WildNatureExplorer.Application.Common;
using WildNatureExplorer.Application.DTOs.Library;
using WildNatureExplorer.Application.Interfaces.Repositories;
using WildNatureExplorer.Application.Services;
using WildNatureExplorer.Domain.Entities;
using WildNatureExplorer.Tests.TestSupport;
using Xunit;

namespace WildNatureExplorer.Tests.Unit;

public class UserLibraryServiceTests
{
    private readonly Guid _uid = Guid.NewGuid();

    [Fact]
    [Trait("Category", "Unit")]
    public async Task CreateAsync_UnknownSpeciesId_ThrowsResourceNotFound()
    {
        var sightings = new Mock<IUserSightingRepository>();
        var species = new Mock<ISpeciesRepository>();
        var sid = Guid.NewGuid();
        species.Setup(s => s.GetByIdAsync(sid)).ReturnsAsync((Species?)null);

        var sut = new UserLibraryService(sightings.Object, species.Object);

        await Assert.ThrowsAsync<ResourceNotFoundException>(() =>
            sut.CreateAsync(_uid, new CreateSightingRequest { SpeciesId = sid, CommonName = "X", Latitude = 1, Longitude = 2 }));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task CreateAsync_WithSpecies_PrefersCatalogueNames()
    {
        var sightings = new Mock<IUserSightingRepository>();
        var speciesRepo = new Mock<ISpeciesRepository>();
        var sid = Guid.NewGuid();
        var sp = new Species
        {
            CommonName = "Catalogue",
            ScientificName = "Sci",
            IsDangerous = true,
            IsRare = false
        }.WithTestId(sid);
        speciesRepo.Setup(s => s.GetByIdAsync(sid)).ReturnsAsync(sp);

        UserSighting? captured = null;
        sightings.Setup(s => s.AddAsync(It.IsAny<UserSighting>()))
            .Callback<UserSighting>(x => captured = x)
            .Returns(Task.CompletedTask);

        var sut = new UserLibraryService(sightings.Object, speciesRepo.Object);

        await sut.CreateAsync(_uid, new CreateSightingRequest
        {
            SpeciesId = sid,
            CommonName = "Ignored",
            Latitude = 10,
            Longitude = 20
        });

        Assert.NotNull(captured);
        Assert.Equal("Catalogue", captured!.CommonName);
        Assert.Equal(sid, captured.SpeciesId);
        sightings.Verify(s => s.AddAsync(It.IsAny<UserSighting>()), Times.Once);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task GetByIdAsync_WrongOwner_ReturnsNull()
    {
        var sightings = new Mock<IUserSightingRepository>();
        var speciesRepo = new Mock<ISpeciesRepository>();
        var sightId = Guid.NewGuid();
        var otherUser = Guid.NewGuid();

        sightings.Setup(s => s.GetByIdAsync(sightId)).ReturnsAsync(new UserSighting(
            sightId, otherUser, null, "Bear", null, 0, 0, null, null, DateTime.UtcNow));

        var sut = new UserLibraryService(sightings.Object, speciesRepo.Object);

        Assert.Null(await sut.GetByIdAsync(_uid, sightId));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task DeleteAsync_NotOwner_ThrowsUnauthorized()
    {
        var sightings = new Mock<IUserSightingRepository>();
        var speciesRepo = new Mock<ISpeciesRepository>();
        var sightId = Guid.NewGuid();
        sightings.Setup(s => s.GetByIdAsync(sightId)).ReturnsAsync(new UserSighting(
            sightId, Guid.NewGuid(), null, "Bear", null, 0, 0, null, null, DateTime.UtcNow));

        var sut = new UserLibraryService(sightings.Object, speciesRepo.Object);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => sut.DeleteAsync(_uid, sightId));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task GetNearbyAsync_Delegates()
    {
        var sightings = new Mock<IUserSightingRepository>();
        var speciesRepo = new Mock<ISpeciesRepository>();
        sightings.Setup(s => s.GetNearbyAsync(_uid, 1, 2, 5)).ReturnsAsync(new List<NearbySightingResponse>());

        var sut = new UserLibraryService(sightings.Object, speciesRepo.Object);

        var req = new NearbySightingsRequest { Latitude = 1, Longitude = 2, RadiusKm = 5 };
        var result = await sut.GetNearbyAsync(_uid, req);

        Assert.Empty(result);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task GetMyLibraryAsync_MapsRows()
    {
        var sightings = new Mock<IUserSightingRepository>();
        var speciesRepo = new Mock<ISpeciesRepository>();
        var row = new UserSighting(Guid.NewGuid(), _uid, null, "Fox", null, 1, 2, null, null, DateTime.UtcNow);
        sightings.Setup(s => s.GetByUserAsync(_uid)).ReturnsAsync(new List<UserSighting> { row });

        var sut = new UserLibraryService(sightings.Object, speciesRepo.Object);

        var list = await sut.GetMyLibraryAsync(_uid);

        Assert.Single(list);
        Assert.Equal("Fox", list[0].CommonName);
    }
}
