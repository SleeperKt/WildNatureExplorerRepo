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

        builder.Property(x => x.CommonName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.ScientificName)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.Description)
            .IsRequired();

        builder.HasOne(x => x.Size)
            .WithMany(x => x.Species)
            .HasForeignKey(x => x.SizeId);
    }
}
