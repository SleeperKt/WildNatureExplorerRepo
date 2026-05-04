using Microsoft.EntityFrameworkCore;
using WildNatureExplorer.Application.DTOs.Library;
using WildNatureExplorer.Application.Interfaces.Repositories;
using WildNatureExplorer.Domain.Entities;
using WildNatureExplorer.Infrastructure.Data;

namespace WildNatureExplorer.Infrastructure.Repositories;

public class UserSightingRepository : IUserSightingRepository
{
    private readonly AppDbContext _context;

    public UserSightingRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<UserSighting?> GetByIdAsync(Guid id) =>
        _context.UserSightings
            .Include(x => x.Species)
            .FirstOrDefaultAsync(x => x.Id == id);

    public async Task<List<UserSighting>> GetByUserAsync(Guid userId) =>
        await _context.UserSightings
            .Include(x => x.Species)
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.SightedAt)
            .ToListAsync();

    public async Task AddAsync(UserSighting sighting)
    {
        _context.UserSightings.Add(sighting);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(UserSighting sighting)
    {
        _context.UserSightings.Remove(sighting);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Calls the PostgreSQL function <c>fn_user_nearby_sightings</c>.
    /// All filtering / distance math runs inside Postgres on the GIST index.
    /// </summary>
    public async Task<List<NearbySightingResponse>> GetNearbyAsync(
        Guid userId,
        double latitude,
        double longitude,
        double radiusKm)
    {
        const string sql = @"
            SELECT
                sighting_id,
                species_id,
                common_name,
                scientific_name,
                is_dangerous,
                is_rare,
                latitude,
                longitude,
                image_url,
                notes,
                sighted_at,
                created_at,
                distance_km
            FROM fn_user_nearby_sightings(@p_user_id, @p_lat, @p_lng, @p_radius_km);";

        var connection = _context.Database.GetDbConnection();
        await _context.Database.OpenConnectionAsync();
        try
        {
            using var command = connection.CreateCommand();
            command.CommandText = sql;

            AddParam(command, "@p_user_id", userId);
            AddParam(command, "@p_lat", latitude);
            AddParam(command, "@p_lng", longitude);
            AddParam(command, "@p_radius_km", (decimal)radiusKm);

            var result = new List<NearbySightingResponse>();
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                // The function returns Guid.Empty as a sentinel when the
                // sighting isn't linked to a catalogued species; surface it
                // as a true `null` so the API contract is consistent.
                var rawSpeciesId = reader.GetGuid(1);

                result.Add(new NearbySightingResponse
                {
                    Id = reader.GetGuid(0),
                    SpeciesId = rawSpeciesId == Guid.Empty ? null : rawSpeciesId,
                    CommonName = reader.GetString(2),
                    ScientificName = reader.GetString(3),
                    IsDangerous = reader.GetBoolean(4),
                    IsRare = reader.GetBoolean(5),
                    Latitude = reader.GetDouble(6),
                    Longitude = reader.GetDouble(7),
                    ImageUrl = reader.IsDBNull(8) ? null : reader.GetString(8),
                    Notes = reader.IsDBNull(9) ? null : reader.GetString(9),
                    SightedAt = reader.GetDateTime(10),
                    CreatedAt = reader.GetDateTime(11),
                    DistanceKm = Convert.ToDouble(reader.GetValue(12))
                });
            }
            return result;
        }
        finally
        {
            await _context.Database.CloseConnectionAsync();
        }
    }

    private static void AddParam(System.Data.Common.DbCommand cmd, string name, object value)
    {
        var p = cmd.CreateParameter();
        p.ParameterName = name;
        p.Value = value;
        cmd.Parameters.Add(p);
    }
}
