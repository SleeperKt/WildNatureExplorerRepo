using Microsoft.EntityFrameworkCore;
using WildNatureExplorer.Application.Interfaces.Repositories;
using WildNatureExplorer.Domain.Entities;
using WildNatureExplorer.Infrastructure.Data;

namespace WildNatureExplorer.Infrastructure.Repositories;

public class SpeciesRepository : ISpeciesRepository
{
    private readonly AppDbContext _context;

    public SpeciesRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Species?> GetByIdAsync(Guid id)
        => await _context.Species
            .Include(x => x.Size)
            .Include(x => x.Colors).ThenInclude(x => x.Color)
            .Include(x => x.Habitats).ThenInclude(x => x.Habitat)
            .Include(x => x.Countries).ThenInclude(x => x.Country)
            .Include(x => x.Locations)
            .FirstOrDefaultAsync(x => x.Id == id);

    public async Task<List<Species>> GetByCountryAsync(Guid countryId)
        => await _context.Species
            .Where(s => s.Countries.Any(c => c.CountryId == countryId))
            .Include(s => s.Locations)
            .ToListAsync();

    public async Task<List<string>> SearchNamesAsync(string prefix)
        => await _context.Species
            .Where(s => s.CommonName.StartsWith(prefix))
            .Select(s => s.CommonName)
            .OrderBy(x => x)
            .Take(10)
            .ToListAsync();

    public async Task<List<Species>> SearchAsync(
        bool? isDangerous,
        bool? isRare,
        List<Guid>? countryIds,
        List<Guid>? habitatIds,
        List<Guid>? colorIds,
        List<Guid>? sizeIds)
    {
        var query = _context.Species.AsQueryable();

        if (isDangerous.HasValue)
            query = query.Where(x => x.IsDangerous == isDangerous);

        if (isRare.HasValue)
            query = query.Where(x => x.IsRare == isRare);

        if (sizeIds?.Any() == true)
            query = query.Where(x => sizeIds.Contains(x.SizeId));

        if (countryIds?.Any() == true)
            query = query.Where(x => x.Countries.Any(c => countryIds.Contains(c.CountryId)));

        if (habitatIds?.Any() == true)
            query = query.Where(x => x.Habitats.Any(h => habitatIds.Contains(h.HabitatId)));

        if (colorIds?.Any() == true)
            query = query.Where(x => x.Colors.Any(c => colorIds.Contains(c.ColorId)));

        return await query
            .Include(x => x.Size)
            .Include(x => x.Colors).ThenInclude(x => x.Color)
            .Include(x => x.Habitats).ThenInclude(x => x.Habitat)
            .Include(x => x.Countries).ThenInclude(x => x.Country)
            .ToListAsync();
    }

    public async Task<Species?> GetByCommonNameAsync(string commonName)
    {
        return await _context.Species
            .Include(s => s.Size)
            .Include(s => s.Colors).ThenInclude(c => c.Color)
            .Include(s => s.Habitats).ThenInclude(h => h.Habitat)
            .Include(s => s.Countries).ThenInclude(c => c.Country)
            .FirstOrDefaultAsync(s =>
                s.CommonName.ToLower() == commonName.ToLower());
    }
    public async Task AddAsync(Species species)
    {
        _context.Species.Add(species);
        await _context.SaveChangesAsync();
    }
}
