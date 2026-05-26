using WildNatureExplorer.Domain.Entities;

namespace WildNatureExplorer.Application.Interfaces.Repositories;

public interface ISizeRepository
{
    Task<Size?> GetByNormalizedNameAsync(string normalizedName);
    Task<List<Size>> GetAllAsync();
    Task AddAsync(Size size);
}
