using WildNatureExplorer.Domain.Entities;

namespace WildNatureExplorer.Application.Interfaces.Repositories;

public interface IHabitatRepository
{
    Task<Habitat?> GetByNormalizedNameAsync(string normalizedName);
    Task<List<Habitat>> GetAllAsync();
    Task AddAsync(Habitat habitat);
}
