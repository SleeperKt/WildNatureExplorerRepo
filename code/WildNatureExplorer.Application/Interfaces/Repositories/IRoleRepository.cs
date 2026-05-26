using WildNatureExplorer.Domain.Entities;

namespace WildNatureExplorer.Application.Interfaces.Repositories;

public interface IRoleRepository
{
    Task<Role?> GetByIdAsync(Guid id);
    Task<Role?> GetByNameAsync(string roleName);
    Task AddAsync(Role role);
    Task<IEnumerable<Role>> GetRolesByUserIdAsync(Guid userId);
}
