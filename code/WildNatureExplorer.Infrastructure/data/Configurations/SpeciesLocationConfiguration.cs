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

        builder.HasIndex(x => x.SpeciesId)
            .HasDatabaseName("IX_SpeciesLocations_SpeciesId");

        builder.HasIndex(x => x.CreatedAt)
            .HasDatabaseName("IX_SpeciesLocations_CreatedAt");

        builder.Property(x => x.Latitude)
            .IsRequired();

        builder.Property(x => x.Longitude)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        builder.HasOne(x => x.Species)
            .WithMany(x => x.Locations)
            .HasForeignKey(x => x.SpeciesId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
