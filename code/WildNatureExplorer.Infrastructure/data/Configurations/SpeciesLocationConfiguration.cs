using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WildNatureExplorer.Domain.Entities;

namespace WildNatureExplorer.Infrastructure.Data.Configurations;

public class SpeciesLocationConfiguration : IEntityTypeConfiguration<SpeciesLocation>
{
    public void Configure(EntityTypeBuilder<SpeciesLocation> builder)
    {
        builder.ToTable("SpeciesLocations");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Latitude)
            .IsRequired();

        builder.Property(x => x.Longitude)
            .IsRequired();
    }
}
