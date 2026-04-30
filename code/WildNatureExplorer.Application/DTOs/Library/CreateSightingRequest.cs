namespace WildNatureExplorer.Application.DTOs.Library;

/// <summary>
/// Payload for saving a recognised animal into the current user's library.
///
/// <para>
/// <see cref="SpeciesId"/> is now optional: when the AI recognises a species
/// we have catalogued it should be filled (the frontend resolves the name via
/// <c>/api/species/by-name</c>). When the species is not in our database the
/// frontend still sends the recognized <see cref="CommonName"/> so the entry
/// is saved as a custom / free-form sighting.
/// </para>
/// </summary>
public class CreateSightingRequest
{
    /// <summary>Optional link to a Species row. When set, must be an existing species.</summary>
    public Guid? SpeciesId { get; set; }

    /// <summary>
    /// Common name of the animal — required for free-form saves and used as the
    /// canonical display name when no <see cref="SpeciesId"/> is provided.
    /// </summary>
    public string CommonName { get; set; } = string.Empty;

    /// <summary>Optional scientific name for free-form saves.</summary>
    public string? ScientificName { get; set; }

    /// <summary>Latitude in degrees, [-90, 90].</summary>
    public double Latitude { get; set; }

    /// <summary>Longitude in degrees, [-180, 180].</summary>
    public double Longitude { get; set; }

    /// <summary>
    /// Optional image of the encounter. The frontend usually passes the photo
    /// the user already uploaded for recognition as a base64 data URL.
    /// </summary>
    public string? ImageUrl { get; set; }

    /// <summary>Optional free-form note (≤ 500 chars).</summary>
    public string? Notes { get; set; }

    /// <summary>When the encounter happened. If omitted, server time is used.</summary>
    public DateTime? SightedAt { get; set; }
}
