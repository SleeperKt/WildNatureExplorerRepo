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

        builder.HasIndex(x => x.CreatedAt)
               .HasDatabaseName("IX_Roles_CreatedAt");

        builder.Property(x => x.RoleName)
               .IsRequired()
               .HasMaxLength(50);

        builder.Property(x => x.Description)
               .HasMaxLength(256);

        builder.Property(x => x.CreatedAt)
               .IsRequired();

        builder.Property(x => x.UpdatedAt)
               .IsRequired();

        builder.HasMany(x => x.Users)
               .WithOne(x => x.Role)
               .HasForeignKey(x => x.RoleId);
    }
}
