using WildNatureExplorer.Application.DTOs.Library;
using WildNatureExplorer.Domain.Entities;

namespace WildNatureExplorer.Application.Interfaces.Repositories;

public interface IUserSightingRepository
{
    Task<UserSighting?> GetByIdAsync(Guid id);
    Task<List<UserSighting>> GetByUserAsync(Guid userId);
    Task AddAsync(UserSighting sighting);
    Task DeleteAsync(UserSighting sighting);

    /// <summary>
    /// Calls the PostgreSQL function <c>fn_user_nearby_sightings</c> and returns
    /// the user's saved sightings within <paramref name="radiusKm"/> kilometres
    /// of the supplied origin, ordered by distance ascending.
    /// </summary>
    Task<List<NearbySightingResponse>> GetNearbyAsync(
        Guid userId,
        double latitude,
        double longitude,
        double radiusKm);
}
