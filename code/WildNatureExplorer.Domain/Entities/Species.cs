using WildNatureExplorer.Domain.Base;

namespace WildNatureExplorer.Domain.Entities;

public class Species : Entity
{
    public string CommonName { get; set; } = null!;
    public string ScientificName { get; set; } = null!;
    public string Description { get; set; } = null!;

    public bool IsDangerous { get; set; }
    public bool IsRare { get; set; }

    public Guid SizeId { get; set; }
    public Size Size { get; set; } = null!;

    public ICollection<SpeciesColor> Colors { get; set; } = new List<SpeciesColor>();
    public ICollection<SpeciesHabitat> Habitats { get; set; } = new List<SpeciesHabitat>();
    public ICollection<SpeciesCountry> Countries { get; set; } = new List<SpeciesCountry>();

    public ICollection<SpeciesLocation> Locations { get; set; } = new List<SpeciesLocation>();
}
