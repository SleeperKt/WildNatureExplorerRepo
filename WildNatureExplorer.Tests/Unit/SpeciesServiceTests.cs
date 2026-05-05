using Moq;
using WildNatureExplorer.Application.Interfaces.Repositories;
using WildNatureExplorer.Application.Services;
using WildNatureExplorer.Domain.Entities;
using Xunit;

namespace WildNatureExplorer.Tests.Unit;

public class SpeciesServiceTests
{
    [Fact]
    [Trait("Category", "Unit")]
    public async Task GetAsync_DelegatesToRepository()
    {
        var repo = new Mock<ISpeciesRepository>();
        var id = Guid.NewGuid();
        var species = new Species { CommonName = "Lion", ScientificName = "P. leo" };
        repo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(species);

        var sut = new SpeciesService(repo.Object);

        var result = await sut.GetAsync(id);

        Assert.Same(species, result);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task SearchAsync_PassesFiltersThrough()
    {
        var repo = new Mock<ISpeciesRepository>();
        var ids = new List<Species>();
        repo.Setup(r => r.SearchAsync(true, false, It.IsAny<List<Guid>?>(), null, null, null)).ReturnsAsync(ids);

        var sut = new SpeciesService(repo.Object);

        var result = await sut.SearchAsync(true, false, new List<Guid> { Guid.NewGuid() }, null, null, null);

        Assert.Same(ids, result);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task GetByCommonNameAsync_Delegates()
    {
        var repo = new Mock<ISpeciesRepository>();
        repo.Setup(r => r.GetByCommonNameAsync("Tiger")).ReturnsAsync((Species?)null);

        var sut = new SpeciesService(repo.Object);

        Assert.Null(await sut.GetByCommonNameAsync("Tiger"));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task GetNameSuggestionsAsync_Delegates()
    {
        var repo = new Mock<ISpeciesRepository>();
        repo.Setup(r => r.SearchNamesAsync("Li")).ReturnsAsync(new List<string> { "Lion" });

        var sut = new SpeciesService(repo.Object);

        var list = await sut.GetNameSuggestionsAsync("Li");
        Assert.Single(list);
        Assert.Equal("Lion", list[0]);
    }
}
