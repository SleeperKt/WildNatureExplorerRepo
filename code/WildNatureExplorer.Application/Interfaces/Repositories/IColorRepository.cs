using WildNatureExplorer.Domain.Entities;

namespace WildNatureExplorer.Application.Interfaces.Repositories;

public interface IColorRepository
{
    Task<Color?> GetByNormalizedNameAsync(string normalizedName);
    Task<List<Color>> GetAllAsync();
    Task AddAsync(Color color);
}
