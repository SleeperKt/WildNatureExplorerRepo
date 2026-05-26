using WildNatureExplorer.Application.Interfaces.Repositories;
using WildNatureExplorer.Application.Interfaces.Services;
using WildNatureExplorer.Domain.Entities;

namespace WildNatureExplorer.Application.Services;

public class SpeciesService : ISpeciesService
{
    private readonly ISpeciesRepository _speciesRepository;

    public SpeciesService(ISpeciesRepository speciesRepository)
    {
        _speciesRepository = speciesRepository;
    }

    public async Task<Species?> GetAsync(Guid id)
        => await _speciesRepository.GetByIdAsync(id);

    public async Task<List<Species>> SearchAsync(
        bool? isDangerous,
        bool? isRare,
        List<Guid>? countryIds,
        List<Guid>? habitatIds,
        List<Guid>? colorIds,
        List<Guid>? sizeIds)
        => await _speciesRepository.SearchAsync(
            isDangerous,
            isRare,
            countryIds,
            habitatIds,
            colorIds,
            sizeIds);

    public async Task<Species?> GetByCommonNameAsync(string commonName)
        => await _speciesRepository.GetByCommonNameAsync(commonName);

    public async Task<List<string>> GetNameSuggestionsAsync(string prefix)
        => await _speciesRepository.SearchNamesAsync(prefix);
}