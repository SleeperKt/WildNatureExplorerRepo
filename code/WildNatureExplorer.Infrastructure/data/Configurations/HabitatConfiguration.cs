using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WildNatureExplorer.Domain.Entities;

namespace WildNatureExplorer.Infrastructure.Data.Configurations;

public class HabitatConfiguration : IEntityTypeConfiguration<Habitat>
{
    public void Configure(EntityTypeBuilder<Habitat> builder)
    {
        builder.ToTable("Habitats");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.NormalizedName)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(x => x.NormalizedName)
            .IsUnique();
    }
}
