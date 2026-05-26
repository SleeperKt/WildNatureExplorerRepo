using WildNatureExplorer.Domain.Base;

namespace WildNatureExplorer.Domain.Entities;

public class SpeciesHabitat
{
    public Guid SpeciesId { get; set; }
    public Species Species { get; set; } = null!;

    public Guid HabitatId { get; set; }
    public Habitat Habitat { get; set; } = null!;
}
