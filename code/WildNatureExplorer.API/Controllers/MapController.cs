using Microsoft.AspNetCore.Mvc;
using WildNatureExplorer.Application.DTOs.Geo;
using WildNatureExplorer.Application.Interfaces.Repositories;
using WildNatureExplorer.Application.Interfaces.Services;

namespace WildNatureExplorer.API.Controllers;

[ApiController]
[Route("api/map")]
public class MapController : ControllerBase
{
    private readonly ISpeciesRepository _speciesRepository;
    private readonly ICountryRepository _countryRepository;
    private readonly IPathSimulationService _pathSimulationService;

    public MapController(
        ISpeciesRepository speciesRepository,
        ICountryRepository countryRepository,
        IPathSimulationService pathSimulationService)
    {
        _speciesRepository = speciesRepository;
        _countryRepository = countryRepository;
        _pathSimulationService = pathSimulationService;
    }

    /// <summary>
    /// Get all species points for a country with full details
    /// </summary>
    [HttpGet("country/{countryId:guid}")]
    public async Task<IActionResult> GetByCountry(Guid countryId, [FromQuery] string? filter = null)
    {
        var species = await _speciesRepository.GetByCountryAsync(countryId);

        // Apply filter if specified
        if (!string.IsNullOrEmpty(filter))
        {
            species = filter.ToLower() switch
            {
                "dangerous" => species.Where(s => s.IsDangerous).ToList(),
                "rare" => species.Where(s => s.IsRare).ToList(),
                _ => species
            };
        }

        var points = species.SelectMany(s => s.Locations.Select(l => new
        {
            SpeciesId = s.Id,
            s.CommonName,
            s.ScientificName,
            s.Description,
            s.IsDangerous,
            s.IsRare,
            l.Latitude,
            l.Longitude,
            LocationDescription = l.Description
        }));

        return Ok(points);
    }

    /// <summary>
    /// Get only dangerous species with their locations for danger mode
    /// </summary>
    [HttpGet("country/{countryId:guid}/dangerous")]
    public async Task<IActionResult> GetDangerousByCountry(Guid countryId)
    {
        var species = await _speciesRepository.GetByCountryAsync(countryId);
        var dangerousSpecies = species.Where(s => s.IsDangerous);

        var points = dangerousSpecies.SelectMany(s => s.Locations.Select(l => new
        {
            SpeciesId = s.Id,
            s.CommonName,
            s.ScientificName,
            s.Description,
            s.IsDangerous,
            s.IsRare,
            l.Latitude,
            l.Longitude,
            LocationDescription = l.Description,
            DangerRadiusKm = 10.0 // Default 10km danger radius
        }));

        return Ok(points);
    }

    /// <summary>
    /// Search for a specific animal in a country
    /// </summary>
    [HttpGet("country/{countryId:guid}/search")]
    public async Task<IActionResult> SearchInCountry(Guid countryId, [FromQuery] string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return BadRequest("Search query is required");

        var species = await _speciesRepository.GetByCountryAsync(countryId);
        var matchingSpecies = species.Where(s =>
            s.CommonName.Contains(query, StringComparison.OrdinalIgnoreCase) ||
            s.ScientificName.Contains(query, StringComparison.OrdinalIgnoreCase)
        );

        var results = matchingSpecies.SelectMany(s => s.Locations.Select(l => new
        {
            SpeciesId = s.Id,
            s.CommonName,
            s.ScientificName,
            s.Description,
            s.IsDangerous,
            s.IsRare,
            l.Latitude,
            l.Longitude,
            LocationDescription = l.Description
        }));

        return Ok(new
        {
            Query = query,
            Found = results.Any(),
            Count = results.Count(),
            Results = results
        });
    }

    /// <summary>
    /// Get country bounds for map focusing
    /// </summary>
    [HttpGet("country/{countryId:guid}/bounds")]
    public async Task<IActionResult> GetCountryBounds(Guid countryId)
    {
        var country = await _countryRepository.GetByIdAsync(countryId);
        if (country == null)
            return NotFound("Country not found");

        var species = await _speciesRepository.GetByCountryAsync(countryId);
        var locations = species.SelectMany(s => s.Locations).ToList();

        if (!locations.Any())
        {
            return Ok(new
            {
                CountryId = countryId,
                CountryName = country.Name,
                HasData = false
            });
        }

        var bounds = new
        {
            North = locations.Max(l => l.Latitude),
            South = locations.Min(l => l.Latitude),
            East = locations.Max(l => l.Longitude),
            West = locations.Min(l => l.Longitude)
        };

        return Ok(new
        {
            CountryId = countryId,
            CountryName = country.Name,
            HasData = true,
            Bounds = bounds,
            Center = new
            {
                Latitude = (bounds.North + bounds.South) / 2,
                Longitude = (bounds.East + bounds.West) / 2
            }
        });
    }

    /// <summary>
    /// Check proximity to dangerous animals
    /// </summary>
    [HttpPost("proximity-check")]
    public async Task<IActionResult> CheckProximity([FromBody] ProximityCheckRequest request)
    {
        if (request.CountryId == Guid.Empty)
            return BadRequest("Country ID is required");

        var species = await _speciesRepository.GetByCountryAsync(request.CountryId);
        var dangerousLocations = species
            .Where(s => s.IsDangerous)
            .SelectMany(s => s.Locations.Select(l => new
            {
                SpeciesId = s.Id,
                s.CommonName,
                l.Latitude,
                l.Longitude,
                DangerRadiusKm = 10.0
            }))
            .ToList();

        var alerts = new List<object>();
        const double radiusKm = 10.0;

        foreach (var loc in dangerousLocations)
        {
            var distance = CalculateDistance(
                request.UserLatitude, request.UserLongitude,
                loc.Latitude, loc.Longitude
            );

            if (distance <= radiusKm)
            {
                alerts.Add(new
                {
                    loc.SpeciesId,
                    loc.CommonName,
                    loc.Latitude,
                    loc.Longitude,
                    DistanceKm = Math.Round(distance, 2),
                    IsInDangerZone = distance <= radiusKm,
                    Warning = distance <= 5 ? "CRITICAL" : distance <= 8 ? "WARNING" : "CAUTION"
                });
            }
        }

        return Ok(new
        {
            UserLocation = new { request.UserLatitude, request.UserLongitude },
            Timestamp = DateTime.UtcNow,
            AlertCount = alerts.Count,
            Alerts = alerts.OrderBy(a => ((dynamic)a).DistanceKm)
        });
    }

    /// <summary>
    /// Simulate a path and check for danger zones
    /// Supports both database-driven (PostgreSQL) and client-side simulation modes
    /// </summary>
    [HttpPost("simulate-path")]
    public async Task<IActionResult> SimulatePath([FromBody] PathSimulationRequest request)
    {
        if (request.CountryId == Guid.Empty)
            return BadRequest("Country ID is required");

        if (request.Waypoints == null || request.Waypoints.Count < 2)
            return BadRequest("At least 2 waypoints are required");

        try
        {
            PathSimulationResponse result = request.SimulationMode switch
            {
                "database" => await _pathSimulationService.SimulatePathDatabaseAsync(request),
                "client" => await _pathSimulationService.SimulatePathClientAsync(request),
                _ => await _pathSimulationService.SimulatePathClientAsync(request) // Default to client
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Calculate distance between two coordinates using Haversine formula
    /// </summary>
    private static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double earthRadiusKm = 6371.0;

        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return earthRadiusKm * c;
    }

    private static double ToRadians(double degrees) => degrees * Math.PI / 180.0;
}

public class ProximityCheckRequest
{
    public Guid CountryId { get; set; }
    public double UserLatitude { get; set; }
    public double UserLongitude { get; set; }
}
