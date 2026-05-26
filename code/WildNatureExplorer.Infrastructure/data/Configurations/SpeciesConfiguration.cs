using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WildNatureExplorer.Domain.Entities;

namespace WildNatureExplorer.Infrastructure.Data.Configurations;

public class SpeciesConfiguration : IEntityTypeConfiguration<Species>
{
    public void Configure(EntityTypeBuilder<Species> builder)
    {
        builder.ToTable("Species");

        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.SizeId)
            .HasDatabaseName("IX_Species_SizeId");

        builder.HasIndex(x => x.CreatedAt)
            .HasDatabaseName("IX_Species_CreatedAt");

        builder.Property(x => x.CommonName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.ScientificName)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.Description)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        builder.HasOne(x => x.Size)
            .WithMany(x => x.Species)
            .HasForeignKey(x => x.SizeId);
    }
}
