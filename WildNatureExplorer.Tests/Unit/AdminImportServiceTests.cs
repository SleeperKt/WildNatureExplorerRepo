using System.Text;
using Moq;
using WildNatureExplorer.Application.Common;
using WildNatureExplorer.Application.DTOs.Admin;
using WildNatureExplorer.Application.Interfaces.Repositories;
using WildNatureExplorer.Application.Services;
using WildNatureExplorer.Domain.Entities;
using WildNatureExplorer.Tests.TestSupport;
using Xunit;

namespace WildNatureExplorer.Tests.Unit;

public class AdminImportServiceTests
{
    private static AdminImportService CreateSut(
        out Mock<ISpeciesRepository> species,
        out Mock<ICountryRepository> countries,
        out Mock<IColorRepository> colors,
        out Mock<IHabitatRepository> habitats,
        out Mock<ISizeRepository> sizes,
        out Mock<ISpeciesLocationRepository> locations)
    {
        species = new Mock<ISpeciesRepository>();
        countries = new Mock<ICountryRepository>();
        colors = new Mock<IColorRepository>();
        habitats = new Mock<IHabitatRepository>();
        sizes = new Mock<ISizeRepository>();
        locations = new Mock<ISpeciesLocationRepository>();

        WireAutoIds(sizes, countries, colors, habitats);

        return new AdminImportService(
            species.Object,
            countries.Object,
            colors.Object,
            habitats.Object,
            sizes.Object,
            locations.Object);
    }

    private static void WireAutoIds(
        Mock<ISizeRepository> sizes,
        Mock<ICountryRepository> countries,
        Mock<IColorRepository> colors,
        Mock<IHabitatRepository> habitats)
    {
        sizes.Setup(s => s.AddAsync(It.IsAny<Size>()))
            .Callback<Size>(x => x.WithTestId(Guid.NewGuid()))
            .Returns(Task.CompletedTask);

        countries.Setup(c => c.AddAsync(It.IsAny<Country>()))
            .Callback<Country>(x => x.WithTestId(Guid.NewGuid()))
            .Returns(Task.CompletedTask);

        colors.Setup(c => c.AddAsync(It.IsAny<Color>()))
            .Callback<Color>(x => x.WithTestId(Guid.NewGuid()))
            .Returns(Task.CompletedTask);

        habitats.Setup(h => h.AddAsync(It.IsAny<Habitat>()))
            .Callback<Habitat>(x => x.WithTestId(Guid.NewGuid()))
            .Returns(Task.CompletedTask);

        sizes.Setup(s => s.GetByNormalizedNameAsync(It.IsAny<string>())).ReturnsAsync((Size?)null);
        countries.Setup(c => c.GetByNormalizedNameAsync(It.IsAny<string>())).ReturnsAsync((Country?)null);
        colors.Setup(c => c.GetByNormalizedNameAsync(It.IsAny<string>())).ReturnsAsync((Color?)null);
        habitats.Setup(h => h.GetByNormalizedNameAsync(It.IsAny<string>())).ReturnsAsync((Habitat?)null);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task ImportSingleSpeciesAsync_AlreadyExists_Throws()
    {
        var sut = CreateSut(out var species, out _, out _, out _, out _, out _);
        species.Setup(s => s.SearchNamesAsync("Tiger")).ReturnsAsync(new List<string> { "Tiger" });

        await Assert.ThrowsAsync<ValidationException>(() =>
            sut.ImportSingleSpeciesAsync(new AdminSpeciesImportDto(
                "Tiger",
                "Tigris",
                null,
                true,
                false,
                "Kenya",
                "Orange",
                "Savanna",
                "Large")));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task ImportSingleSpeciesAsync_NewSpecies_AddsOnce()
    {
        var sut = CreateSut(out var species, out _, out _, out _, out _, out _);
        species.Setup(s => s.SearchNamesAsync("Serval")).ReturnsAsync(new List<string>());
        species.Setup(s => s.AddAsync(It.IsAny<Species>())).Returns(Task.CompletedTask);

        await sut.ImportSingleSpeciesAsync(new AdminSpeciesImportDto(
            "Serval",
            "Leptailurus serval",
            null,
            false,
            false,
            "Kenya",
            "Yellow",
            "Grassland",
            "Medium"));

        species.Verify(s => s.AddAsync(It.IsAny<Species>()), Times.Once);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task ImportSpeciesCsvAsync_SkipsDuplicatesAndImportsFreshRows()
    {
        var sut = CreateSut(out var species, out _, out _, out _, out _, out _);

        species.SetupSequence(s => s.SearchNamesAsync("Alpha"))
            .ReturnsAsync(new List<string>())
            .ReturnsAsync(new List<string> { "Alpha" });

        species.Setup(s => s.SearchNamesAsync("Beta")).ReturnsAsync(new List<string>());
        species.Setup(s => s.AddAsync(It.IsAny<Species>())).Returns(Task.CompletedTask);

        var csv = """
                  CommonName,ScientificName,Description,IsDangerous,IsRare,Countries,Colors,Habitats,Size
                  Alpha,A.al,,false,false,Kenya,Yellow,Savanna,Small
                  Alpha,A.al,,false,false,Kenya,Yellow,Savanna,Small
                  Beta,B.be,,false,false,Kenya,Yellow,Savanna,Small
                  """;

        await sut.ImportSpeciesCsvAsync(new AdminSpeciesCsvDto
        {
            FileStream = new MemoryStream(Encoding.UTF8.GetBytes(csv)),
            FileName = "rows.csv"
        });

        species.Verify(s => s.AddAsync(It.IsAny<Species>()), Times.Exactly(2));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task ImportSpeciesLocationsCsvAsync_InvalidLat_Throws()
    {
        var sut = CreateSut(out _, out _, out _, out _, out _, out var locations);

        var csv = """
                  Latitude,Longitude,Description
                  100,10,x
                  """;
        await Assert.ThrowsAsync<ValidationException>(() =>
            sut.ImportSpeciesLocationsCsvAsync(Guid.NewGuid(), new MemoryStream(Encoding.UTF8.GetBytes(csv))));
        locations.Verify(l => l.AddAsync(It.IsAny<SpeciesLocation>()), Times.Never);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task ImportSpeciesLocationsCsvAsync_Valid_AddsLocations()
    {
        var sut = CreateSut(out _, out _, out _, out _, out _, out var locations);
        locations.Setup(l => l.AddAsync(It.IsAny<SpeciesLocation>())).Returns(Task.CompletedTask);

        var csv = """
                  Latitude,Longitude,Description
                  45.5,9.2,Near lake
                  """;

        await sut.ImportSpeciesLocationsCsvAsync(Guid.NewGuid(), new MemoryStream(Encoding.UTF8.GetBytes(csv)));

        locations.Verify(l => l.AddAsync(It.IsAny<SpeciesLocation>()), Times.Once);
    }
}
