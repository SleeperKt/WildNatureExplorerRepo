using WildNatureExplorer.Domain.Base;

namespace WildNatureExplorer.Domain.Entities;

public class SpeciesCountry
{
    public Guid SpeciesId { get; set; }
    public Species Species { get; set; } = null!;

    public Guid CountryId { get; set; }
    public Country Country { get; set; } = null!;
}
