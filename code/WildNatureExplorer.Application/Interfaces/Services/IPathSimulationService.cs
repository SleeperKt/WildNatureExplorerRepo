using WildNatureExplorer.Application.DTOs.Geo;

namespace WildNatureExplorer.Application.Interfaces.Services;

public interface IPathSimulationService
{
    /// <summary>
    /// Simulate a path using database function (PostGIS/PostgreSQL)
    /// </summary>
    Task<PathSimulationResponse> SimulatePathDatabaseAsync(PathSimulationRequest request);

    /// <summary>
    /// Simulate a path using client-side calculation (sent to backend for consistency check)
    /// </summary>
    Task<PathSimulationResponse> SimulatePathClientAsync(PathSimulationRequest request);
}
