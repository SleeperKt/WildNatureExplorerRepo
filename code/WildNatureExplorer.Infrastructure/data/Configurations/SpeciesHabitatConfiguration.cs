using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WildNatureExplorer.Domain.Entities;

public class SpeciesHabitatConfiguration : IEntityTypeConfiguration<SpeciesHabitat>
{
    public void Configure(EntityTypeBuilder<SpeciesHabitat> builder)
    {
        builder.ToTable("SpeciesHabitats");

        builder.HasKey(x => new { x.SpeciesId, x.HabitatId });

        builder.HasIndex(x => x.SpeciesId)
            .HasDatabaseName("IX_SpeciesHabitats_SpeciesId");

        builder.HasIndex(x => x.HabitatId)
            .HasDatabaseName("IX_SpeciesHabitats_HabitatId");

        builder.HasOne(x => x.Species)
            .WithMany(x => x.Habitats)
            .HasForeignKey(x => x.SpeciesId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Habitat)
            .WithMany(x => x.Species)
            .HasForeignKey(x => x.HabitatId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}