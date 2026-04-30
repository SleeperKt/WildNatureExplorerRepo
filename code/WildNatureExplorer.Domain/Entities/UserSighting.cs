using WildNatureExplorer.Domain.Base;

namespace WildNatureExplorer.Domain.Entities;

/// <summary>
/// A single animal saved by a user into their personal library
/// (image already produced by the AI recognition flow + GPS coordinates).
///
/// <para>
/// <b>Species linkage</b> — when the recognized name matches an entry in the
/// curated <see cref="Species"/> table, <see cref="SpeciesId"/> is filled and
/// the sighting inherits the species' rich metadata (rarity, danger, scientific
/// name…). When the AI returns a species we don't have catalogued yet (e.g.
/// "Hartebeest"), we still let the user save it: <see cref="SpeciesId"/> is
/// <c>null</c>, and we keep the recognized name on the row itself via
/// <see cref="CommonName"/> / <see cref="ScientificName"/>.
/// </para>
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
