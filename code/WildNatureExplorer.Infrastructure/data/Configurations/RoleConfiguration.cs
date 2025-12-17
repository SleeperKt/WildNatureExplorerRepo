using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WildNatureExplorer.Domain.Entities;

namespace WildNatureExplorer.Infrastructure.Data.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles");

        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.RoleName)
               .IsUnique();

        builder.Property(x => x.RoleName)
               .IsRequired()
               .HasMaxLength(50);

        builder.Property(x => x.Description)
               .HasMaxLength(256);

        builder.HasMany(x => x.Users)
               .WithOne(x => x.Role)
               .HasForeignKey(x => x.RoleId);
    }
}
