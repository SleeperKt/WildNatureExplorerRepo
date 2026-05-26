using WildNatureExplorer.Domain.Entities;

namespace WildNatureExplorer.Application.Interfaces.Repositories;

public interface ISpeciesRepository
{
    Task<Species?> GetByIdAsync(Guid id);

    Task<List<Species>> SearchAsync(
        bool? isDangerous,
        bool? isRare,
        List<Guid>? countryIds,
        List<Guid>? habitatIds,
        List<Guid>? colorIds,
        List<Guid>? sizeIds);

    Task<List<Species>> GetByCountryAsync(Guid countryId);
    Task<Species?> GetByCommonNameAsync(string commonName);

    Task<List<string>> SearchNamesAsync(string prefix);

    Task AddAsync(Species species);
}
