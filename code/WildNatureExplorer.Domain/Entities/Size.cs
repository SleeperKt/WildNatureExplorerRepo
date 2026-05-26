using WildNatureExplorer.Domain.Base;

namespace WildNatureExplorer.Domain.Entities;

public class Size : Entity
{
    public string Name { get; set; } = null!;
    public string NormalizedName { get; set; } = null!;

    public ICollection<Species> Species { get; set; } = new List<Species>();
}