using Microsoft.EntityFrameworkCore;
using WildNatureExplorer.Application.Interfaces.Repositories;
using WildNatureExplorer.Domain.Entities;
using WildNatureExplorer.Infrastructure.Data;

namespace WildNatureExplorer.Infrastructure.Repositories;

public class CountryRepository : ICountryRepository
{
    private readonly AppDbContext _context;

    public CountryRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Country?> GetByNormalizedNameAsync(string normalizedName)
        => await _context.Countries
            .FirstOrDefaultAsync(x => x.NormalizedName == normalizedName);

    public async Task<Country?> GetByIdAsync(Guid id)
        => await _context.Countries.FindAsync(id);

    public async Task<List<Country>> GetAllAsync()
        => await _context.Countries.OrderBy(x => x.Name).ToListAsync();

    public async Task AddAsync(Country country)
    {
        _context.Countries.Add(country);
        await _context.SaveChangesAsync();
    }
}
