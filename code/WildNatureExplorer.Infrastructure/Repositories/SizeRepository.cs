using Microsoft.EntityFrameworkCore;
using WildNatureExplorer.Application.Interfaces.Repositories;
using WildNatureExplorer.Domain.Entities;
using WildNatureExplorer.Infrastructure.Data;

namespace WildNatureExplorer.Infrastructure.Repositories;

public class SizeRepository : ISizeRepository
{
    private readonly AppDbContext _context;

    public SizeRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Size?> GetByNormalizedNameAsync(string normalizedName)
        => await _context.Sizes
            .FirstOrDefaultAsync(x => x.NormalizedName == normalizedName);

    public async Task<Size?> GetByIdAsync(Guid id)
        => await _context.Sizes.FindAsync(id);

    public async Task<List<Size>> GetAllAsync()
        => await _context.Sizes.OrderBy(x => x.Name).ToListAsync();

    public async Task AddAsync(Size size)
    {
        _context.Sizes.Add(size);
        await _context.SaveChangesAsync();
    }
}
