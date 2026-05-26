using WildNatureExplorer.Domain.Base;

namespace WildNatureExplorer.Domain.Entities;

public class Country : Entity
{
    public string Name { get; set; } = null!;
    public string NormalizedName { get; set; } = null!;

    public ICollection<SpeciesCountry> Species { get; set; } = new List<SpeciesCountry>();
}
