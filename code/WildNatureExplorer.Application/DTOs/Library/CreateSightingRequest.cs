namespace WildNatureExplorer.Application.DTOs.Library;

/// <summary>
/// Create library sighting: optional <see cref="SpeciesId"/> when catalogued; otherwise use <see cref="CommonName"/> (and optionally <see cref="ScientificName"/>) for free-form rows.
/// </summary>
public class CreateSightingRequest
{
    /// <summary>Optional; must reference an existing species when set.</summary>
    public Guid? SpeciesId { get; set; }

    /// <summary>Required display name; primary when <see cref="SpeciesId"/> is null.</summary>
    public string CommonName { get; set; } = string.Empty;

    /// <summary>Optional scientific name when not linked to catalogue.</summary>
    public string? ScientificName { get; set; }

    /// <summary>Latitude [-90, 90].</summary>
    public double Latitude { get; set; }

    /// <summary>Longitude [-180, 180].</summary>
    public double Longitude { get; set; }

    /// <summary>Optional image (e.g. data URL).</summary>
    public string? ImageUrl { get; set; }

    /// <summary>Optional note (≤ 500 chars).</summary>
    public string? Notes { get; set; }

    /// <summary>Observation time; defaults to server clock.</summary>
    public DateTime? SightedAt { get; set; }
}
