namespace WildNatureExplorer.Application.DTOs.Library;

/// <summary>
/// Query parameters for the "find animals around me" endpoint.
/// </summary>
public class NearbySightingsRequest
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }

    /// <summary>Search radius in kilometres. Allowed range: (0, 1000].</summary>
    public double RadiusKm { get; set; } = 25;
}
