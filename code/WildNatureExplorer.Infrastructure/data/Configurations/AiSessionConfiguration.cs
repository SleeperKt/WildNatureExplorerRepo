using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WildNatureExplorer.Domain.Entities;

namespace WildNatureExplorer.Infrastructure.Data.Configurations;

public class AiSessionConfiguration : IEntityTypeConfiguration<AiSession>
{
    public void Configure(EntityTypeBuilder<AiSession> builder)
    {
        builder.ToTable("AiSessions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.AnimalName)
               .IsRequired()
               .HasMaxLength(200);

        builder.Property(x => x.ImageUrl)
               .IsRequired();

        builder.Property(x => x.CreatedAt)
               .IsRequired();

        builder.HasMany(x => x.Messages)
               .WithOne(x => x.Session)
               .HasForeignKey(x => x.SessionId);
    }
}
