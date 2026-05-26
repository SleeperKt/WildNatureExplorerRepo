using WildNatureExplorer.Domain.Base;

namespace WildNatureExplorer.Domain.Entities;

/// <summary>
/// User-saved sighting: optional link to <see cref="Species"/> when recognised name matches the catalogue; otherwise <see cref="SpeciesId"/> is null and names live on this row.
/// </summary>
public class UserSighting : Entity
{
    private UserSighting() { }

    public UserSighting(
        Guid id,
        Guid userId,
        Guid? speciesId,
        string commonName,
        string? scientificName,
        double latitude,
        double longitude,
        string? imageUrl,
        string? notes,
        DateTime sightedAt)
    {
        if (string.IsNullOrWhiteSpace(commonName))
            throw new ArgumentException("CommonName is required.", nameof(commonName));
        if (latitude is < -90 or > 90)
            throw new ArgumentOutOfRangeException(nameof(latitude), "Latitude must be between -90 and 90.");
        if (longitude is < -180 or > 180)
            throw new ArgumentOutOfRangeException(nameof(longitude), "Longitude must be between -180 and 180.");

        Id = id;
        UserId = userId;
        SpeciesId = speciesId;
        CommonName = commonName.Trim();
        ScientificName = string.IsNullOrWhiteSpace(scientificName) ? null : scientificName.Trim();
        Latitude = latitude;
        Longitude = longitude;
        ImageUrl = imageUrl;
        Notes = notes;
        SightedAt = sightedAt == default ? DateTime.UtcNow : sightedAt;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;

    /// <summary>
    /// Optional link to the curated <see cref="Species"/> catalogue. When
    /// <c>null</c>, the recognized animal is not in our database yet and the
    /// row stands alone via <see cref="CommonName"/>.
    /// </summary>
    public Guid? SpeciesId { get; private set; }
    public Species? Species { get; private set; }

    /// <summary>Common name of the animal as recognized / typed by the user.</summary>
    public string CommonName { get; private set; } = string.Empty;

    /// <summary>Optional scientific name (only available for catalogued species).</summary>
    public string? ScientificName { get; private set; }

    public double Latitude { get; private set; }
    public double Longitude { get; private set; }

    public string? ImageUrl { get; private set; }
    public string? Notes { get; private set; }

    public DateTime SightedAt { get; private set; }

    public void UpdateNotes(string? notes)
    {
        Notes = notes;
        UpdatedAt = DateTime.UtcNow;
    }
}
