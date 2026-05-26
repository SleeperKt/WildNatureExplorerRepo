using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WildNatureExplorer.Domain.Entities;

namespace WildNatureExplorer.Infrastructure.Data.Configurations;

public class AiMessageConfiguration : IEntityTypeConfiguration<AiMessage>
{
    public void Configure(EntityTypeBuilder<AiMessage> builder)
    {
        builder.ToTable("AiMessages");

        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.SessionId)
            .HasDatabaseName("IX_AiMessages_SessionId");

        builder.HasIndex(x => x.CreatedAt)
            .HasDatabaseName("IX_AiMessages_CreatedAt");

        builder.Property(x => x.Role)
               .IsRequired()
               .HasMaxLength(20);

        builder.Property(x => x.Content)
               .IsRequired();

        builder.Property(x => x.CreatedAt)
               .IsRequired();

        builder.Property(x => x.UpdatedAt)
               .IsRequired();
    }
}
