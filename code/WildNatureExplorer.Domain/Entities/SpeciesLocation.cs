using WildNatureExplorer.Domain.Base;

namespace WildNatureExplorer.Domain.Entities;

public class SpeciesLocation : Entity
{
    public Guid SpeciesId { get; set; }
    public Species Species { get; set; } = null!;

    public double Latitude { get; set; }
    public double Longitude { get; set; }

    public string? Description { get; set; }
}
