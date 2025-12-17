using Microsoft.EntityFrameworkCore;
using WildNatureExplorer.Application.Interfaces.Repositories;
using WildNatureExplorer.Domain.Entities;
using WildNatureExplorer.Infrastructure.Data;

namespace WildNatureExplorer.Infrastructure.Repositories;

public class RoleRepository : IRoleRepository
{
    private readonly AppDbContext _context;

    public RoleRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Role?> GetByNameAsync(string roleName)
        => await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == roleName);

    public async Task<Role?> GetByIdAsync(Guid id)
        => await _context.Roles.FindAsync(id);
}
