using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WildNatureExplorer.Domain.Entities;

namespace WildNatureExplorer.Infrastructure.Data.Configurations;

public class UserSightingConfiguration : IEntityTypeConfiguration<UserSighting>
{
    public void Configure(EntityTypeBuilder<UserSighting> builder)
    {
        builder.ToTable("UserSightings");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Latitude)
               .IsRequired();
        builder.Property(x => x.Longitude)
               .IsRequired();

        // No length cap — we accept either an http(s) URL or a base64
        // data: URL of the photo the user already uploaded for recognition,
        // which is well above the 2048-char varchar limit.
        builder.Property(x => x.ImageUrl)
               .HasColumnType("text");

        builder.Property(x => x.Notes)
               .HasMaxLength(500);

        // Recognized animal name — required, even when the species isn't in
        // our catalogue. Used as the fallback display name everywhere.
        builder.Property(x => x.CommonName)
               .IsRequired()
               .HasMaxLength(200);

        builder.Property(x => x.ScientificName)
               .HasMaxLength(200);

        builder.Property(x => x.SightedAt)
               .IsRequired();

        builder.Property(x => x.CreatedAt)
               .IsRequired();
        builder.Property(x => x.UpdatedAt)
               .IsRequired();

        builder.HasIndex(x => x.UserId)
               .HasDatabaseName("IX_UserSightings_UserId");

        builder.HasIndex(x => new { x.UserId, x.SightedAt })
               .HasDatabaseName("IX_UserSightings_UserId_SightedAt");

        builder.HasIndex(x => x.SpeciesId)
               .HasDatabaseName("IX_UserSightings_SpeciesId");

        // Note: PostgreSQL treats NULL SpeciesId as distinct, so this still
        // dedupes catalogued species but allows multiple free-form entries.
        builder.HasIndex(x => new { x.UserId, x.SpeciesId, x.SightedAt })
               .IsUnique()
               .HasDatabaseName("UX_UserSightings_User_Species_SightedAt");

        builder.HasOne(x => x.User)
               .WithMany()
               .HasForeignKey(x => x.UserId)
               .OnDelete(DeleteBehavior.Cascade);

        // Optional link to Species. When the curated species row is removed,
        // the user's history is preserved by setting SpeciesId to NULL — the
        // CommonName / ScientificName columns keep displaying.
        builder.HasOne(x => x.Species)
               .WithMany()
               .HasForeignKey(x => x.SpeciesId)
               .IsRequired(false)
               .OnDelete(DeleteBehavior.SetNull);
    }
}
