using WildNatureExplorer.Domain.Entities;

namespace WildNatureExplorer.Application.Interfaces.Repositories;

public interface ICountryRepository
{
    Task<Country?> GetByNormalizedNameAsync(string normalizedName);
    Task<Country?> GetByIdAsync(Guid id);
    Task<List<Country>> GetAllAsync();
    Task AddAsync(Country country);
}
