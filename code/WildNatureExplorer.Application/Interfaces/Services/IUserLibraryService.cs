using WildNatureExplorer.Application.DTOs.Library;

namespace WildNatureExplorer.Application.Interfaces.Services;

public interface IUserLibraryService
{
    Task<SightingResponse> CreateAsync(Guid userId, CreateSightingRequest request);
    Task<List<SightingResponse>> GetMyLibraryAsync(Guid userId);
    Task<SightingResponse?> GetByIdAsync(Guid userId, Guid sightingId);
    Task DeleteAsync(Guid userId, Guid sightingId);

    Task<List<NearbySightingResponse>> GetNearbyAsync(
        Guid userId,
        NearbySightingsRequest request);
}
