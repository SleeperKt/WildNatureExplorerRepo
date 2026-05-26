using WildNatureExplorer.Domain.Base;

namespace WildNatureExplorer.Domain.Entities;

public class Color : Entity
{
    public string Name { get; set; } = null!;
    public string NormalizedName { get; set; } = null!;

    public ICollection<SpeciesColor> Species { get; set; } = new List<SpeciesColor>();
}
