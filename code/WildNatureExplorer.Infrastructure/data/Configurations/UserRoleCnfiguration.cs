using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WildNatureExplorer.Domain.Entities;

namespace WildNatureExplorer.Infrastructure.Data.Configurations;

public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable("UserRoles");

        builder.HasKey(x => new { x.UserId, x.RoleId });

        builder.HasIndex(x => x.UserId)
               .HasDatabaseName("IX_UserRoles_UserId");

        builder.HasIndex(x => x.RoleId)
               .HasDatabaseName("IX_UserRoles_RoleId");

        builder.HasIndex(x => x.AssignedAt)
               .HasDatabaseName("IX_UserRoles_AssignedAt");

        builder.Property(x => x.AssignedAt)
               .IsRequired();

        builder.HasOne(x => x.User)
               .WithMany(x => x.UserRoles)
               .HasForeignKey(x => x.UserId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Role)
               .WithMany(x => x.Users)
               .HasForeignKey(x => x.RoleId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
