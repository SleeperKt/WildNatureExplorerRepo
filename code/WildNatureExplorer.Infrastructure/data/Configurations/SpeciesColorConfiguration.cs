using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WildNatureExplorer.Domain.Entities;

public class SpeciesColorConfiguration : IEntityTypeConfiguration<SpeciesColor>
{
    public void Configure(EntityTypeBuilder<SpeciesColor> builder)
    {
        builder.ToTable("SpeciesColors");

        builder.HasKey(x => new { x.SpeciesId, x.ColorId });

        builder.HasIndex(x => x.SpeciesId)
            .HasDatabaseName("IX_SpeciesColors_SpeciesId");

        builder.HasIndex(x => x.ColorId)
            .HasDatabaseName("IX_SpeciesColors_ColorId");

        builder.HasOne(x => x.Species)
            .WithMany(x => x.Colors)
            .HasForeignKey(x => x.SpeciesId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Color)
            .WithMany(x => x.Species)
            .HasForeignKey(x => x.ColorId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}