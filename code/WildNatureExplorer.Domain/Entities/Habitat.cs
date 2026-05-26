using WildNatureExplorer.Domain.Base;

namespace WildNatureExplorer.Domain.Entities;

public class Habitat : Entity
{
    public string Name { get; set; } = null!;
    public string NormalizedName { get; set; } = null!;

    public ICollection<SpeciesHabitat> Species { get; set; } = new List<SpeciesHabitat>();
}
