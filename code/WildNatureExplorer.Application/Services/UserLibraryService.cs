using WildNatureExplorer.Application.Common;
using WildNatureExplorer.Application.DTOs.Library;
using WildNatureExplorer.Application.Interfaces.Repositories;
using WildNatureExplorer.Application.Interfaces.Services;
using WildNatureExplorer.Domain.Entities;

namespace WildNatureExplorer.Application.Services;

public class UserLibraryService : IUserLibraryService
{
    private readonly IUserSightingRepository _sightings;
    private readonly ISpeciesRepository _species;

    public UserLibraryService(
        IUserSightingRepository sightings,
        ISpeciesRepository species)
    {
        _sightings = sightings;
        _species = species;
    }

    public async Task<SightingResponse> CreateAsync(Guid userId, CreateSightingRequest request)
    {
        // Resolve the species link if the frontend provided an ID. When the
        // species isn't in our catalogue, the request still goes through —
        // we just save the sighting as a free-form / custom entry.
        Species? species = null;
        if (request.SpeciesId is { } speciesId && speciesId != Guid.Empty)
        {
            species = await _species.GetByIdAsync(speciesId);
            if (species is null)
                throw new ResourceNotFoundException("Species", speciesId.ToString());
        }

        var sighting = new UserSighting(
            id: Guid.NewGuid(),
            userId: userId,
            speciesId: species?.Id,
            commonName: species?.CommonName ?? request.CommonName,
            scientificName: species?.ScientificName ?? request.ScientificName,
            latitude: request.Latitude,
            longitude: request.Longitude,
            imageUrl: request.ImageUrl,
            notes: request.Notes,
            sightedAt: request.SightedAt ?? DateTime.UtcNow);

        await _sightings.AddAsync(sighting);

        return ToDto(sighting, species);
    }

    public async Task<List<SightingResponse>> GetMyLibraryAsync(Guid userId)
    {
        var rows = await _sightings.GetByUserAsync(userId);
        return rows.Select(s => ToDto(s, s.Species)).ToList();
    }

    public async Task<SightingResponse?> GetByIdAsync(Guid userId, Guid sightingId)
    {
        var s = await _sightings.GetByIdAsync(sightingId);
        if (s is null || s.UserId != userId) return null;
        return ToDto(s, s.Species);
    }

    public async Task DeleteAsync(Guid userId, Guid sightingId)
    {
        var s = await _sightings.GetByIdAsync(sightingId)
            ?? throw new ResourceNotFoundException("UserSighting", sightingId.ToString());

        if (s.UserId != userId)
            throw new UnauthorizedAccessException("You can only delete your own sightings.");

        await _sightings.DeleteAsync(s);
    }

    public async Task<List<NearbySightingResponse>> GetNearbyAsync(
        Guid userId,
        NearbySightingsRequest request)
    {
        return await _sightings.GetNearbyAsync(
            userId,
            request.Latitude,
            request.Longitude,
            request.RadiusKm);
    }

    private static SightingResponse ToDto(UserSighting s, Species? species) => new()
    {
        Id = s.Id,
        SpeciesId = s.SpeciesId,
        // Prefer the catalogued species' canonical names when linked,
        // otherwise fall back to the recognized text on the sighting itself.
        CommonName = species?.CommonName ?? s.CommonName,
        ScientificName = species?.ScientificName ?? s.ScientificName ?? string.Empty,
        IsDangerous = species?.IsDangerous ?? false,
        IsRare = species?.IsRare ?? false,
        Latitude = s.Latitude,
        Longitude = s.Longitude,
        ImageUrl = s.ImageUrl,
        Notes = s.Notes,
        SightedAt = s.SightedAt,
        CreatedAt = s.CreatedAt,
    };
}
