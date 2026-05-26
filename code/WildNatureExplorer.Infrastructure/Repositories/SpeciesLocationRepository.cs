using Microsoft.EntityFrameworkCore;
using WildNatureExplorer.Application.Interfaces.Repositories;
using WildNatureExplorer.Domain.Entities;
using WildNatureExplorer.Infrastructure.Data;

namespace WildNatureExplorer.Infrastructure.Repositories
{
    public class SpeciesLocationRepository : ISpeciesLocationRepository
    {
        private readonly AppDbContext _context;

        public SpeciesLocationRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(SpeciesLocation location)
        {
            _context.SpeciesLocations.Add(location);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<SpeciesLocation>> GetBySpeciesIdAsync(Guid speciesId)
        {
            return await _context.SpeciesLocations
                .Where(sl => sl.SpeciesId == speciesId)
                .ToListAsync();
        }
    }
}
