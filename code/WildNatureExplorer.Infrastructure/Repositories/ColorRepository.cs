using Microsoft.EntityFrameworkCore;
using WildNatureExplorer.Application.Interfaces.Repositories;
using WildNatureExplorer.Domain.Entities;
using WildNatureExplorer.Infrastructure.Data;

namespace WildNatureExplorer.Infrastructure.Repositories;

public class ColorRepository : IColorRepository
{
    private readonly AppDbContext _context;

    public ColorRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Color?> GetByNormalizedNameAsync(string normalizedName)
        => await _context.Colors
            .FirstOrDefaultAsync(x => x.NormalizedName == normalizedName);

    public async Task<Color?> GetByIdAsync(Guid id)
        => await _context.Colors.FindAsync(id);

    public async Task<List<Color>> GetAllAsync()
        => await _context.Colors.OrderBy(x => x.Name).ToListAsync();

    public async Task AddAsync(Color color)
    {
        _context.Colors.Add(color);
        await _context.SaveChangesAsync();
    }
}
