namespace WildNatureExplorer.Application.DTOs.Library;

/// <summary>
/// Single saved encounter in the user's library, enriched with the species data
/// the frontend needs to render a map marker and a card.
/// </summary>
public class SightingResponse
{
    public Guid Id { get; set; }

    /// <summary>
    /// Link to the curated <c>Species</c> row, or <c>null</c> when the entry
    /// is free-form (the recognised animal isn't in our catalogue). The
    /// frontend uses the <c>null</c> case to render a "?" badge on the card.
    /// </summary>
    public Guid? SpeciesId { get; set; }

    public string CommonName { get; set; } = string.Empty;
    public string ScientificName { get; set; } = string.Empty;
    public bool IsDangerous { get; set; }
    public bool IsRare { get; set; }

    public double Latitude { get; set; }
    public double Longitude { get; set; }

    public string? ImageUrl { get; set; }
    public string? Notes { get; set; }

    public DateTime SightedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
