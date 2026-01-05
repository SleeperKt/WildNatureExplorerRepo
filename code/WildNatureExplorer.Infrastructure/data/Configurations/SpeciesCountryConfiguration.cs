using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WildNatureExplorer.Domain.Entities;

public class SpeciesCountryConfiguration : IEntityTypeConfiguration<SpeciesCountry>
{
    public void Configure(EntityTypeBuilder<SpeciesCountry> builder)
    {
        builder.ToTable("SpeciesCountries");

        builder.HasKey(x => new { x.SpeciesId, x.CountryId });

        builder.HasOne(x => x.Species)
            .WithMany(x => x.Countries)
            .HasForeignKey(x => x.SpeciesId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Country)
            .WithMany(x => x.Species)
            .HasForeignKey(x => x.CountryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
