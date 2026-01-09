using WildNatureExplorer.Domain.Entities;

namespace WildNatureExplorer.Application.Interfaces.Services;

public interface ISpeciesService
{
    Task<Species?> GetAsync(Guid id);
    Task<List<Species>> SearchAsync(
        bool? isDangerous,
        bool? isRare,
        List<Guid>? countryIds,
        List<Guid>? habitatIds,
        List<Guid>? colorIds,
        List<Guid>? sizeIds);

    Task<Species?> GetByCommonNameAsync(string commonName);

    Task<List<string>> GetNameSuggestionsAsync(string prefix);
}