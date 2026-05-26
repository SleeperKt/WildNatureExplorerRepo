namespace WildNatureExplorer.Application.DTOs.Library;

/// <summary>
/// Same shape as <see cref="SightingResponse"/> plus the great-circle distance
/// (in km) from the query origin to the saved sighting.
/// Returned by the <c>fn_user_nearby_sightings</c> PostgreSQL function.
/// </summary>
public class NearbySightingResponse : SightingResponse
{
    public double DistanceKm { get; set; }
}
