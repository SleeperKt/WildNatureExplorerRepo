using Microsoft.EntityFrameworkCore;
using WildNatureExplorer.Application.Interfaces.Repositories;
using WildNatureExplorer.Domain.Entities;
using WildNatureExplorer.Infrastructure.Data;

namespace WildNatureExplorer.Infrastructure.Repositories;

public class HabitatRepository : IHabitatRepository
{
    private readonly AppDbContext _context;

    public HabitatRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Habitat?> GetByNormalizedNameAsync(string normalizedName)
        => await _context.Habitats
            .FirstOrDefaultAsync(x => x.NormalizedName == normalizedName);

    public async Task<Habitat?> GetByIdAsync(Guid id)
        => await _context.Habitats.FindAsync(id);

    public async Task<List<Habitat>> GetAllAsync()
        => await _context.Habitats.OrderBy(x => x.Name).ToListAsync();

    public async Task AddAsync(Habitat habitat)
    {
        _context.Habitats.Add(habitat);
        await _context.SaveChangesAsync();
    }
}
