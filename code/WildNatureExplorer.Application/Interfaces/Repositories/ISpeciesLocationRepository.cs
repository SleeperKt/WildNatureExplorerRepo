using WildNatureExplorer.Domain.Entities;

namespace WildNatureExplorer.Application.Interfaces.Repositories
{
    public interface ISpeciesLocationRepository
    {
        Task AddAsync(SpeciesLocation location);
        Task<IEnumerable<SpeciesLocation>> GetBySpeciesIdAsync(Guid speciesId);
    }
}
