using WildNatureExplorer.Domain.Base;

namespace WildNatureExplorer.Domain.Entities;

public class SpeciesColor
{
    public Guid SpeciesId { get; set; }
    public Species Species { get; set; } = null!;

    public Guid ColorId { get; set; }
    public Color Color { get; set; } = null!;
}
