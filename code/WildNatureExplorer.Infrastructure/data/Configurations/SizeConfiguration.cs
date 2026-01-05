using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WildNatureExplorer.Domain.Entities;

namespace WildNatureExplorer.Infrastructure.Data.Configurations;

public class SizeConfiguration : IEntityTypeConfiguration<Size>
{
    public void Configure(EntityTypeBuilder<Size> builder)
    {
        builder.ToTable("Sizes");

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
