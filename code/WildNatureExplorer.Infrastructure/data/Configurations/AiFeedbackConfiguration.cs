using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WildNatureExplorer.Domain.Entities;

namespace WildNatureExplorer.Infrastructure.Data.Configurations;

public class AiFeedbackConfiguration : IEntityTypeConfiguration<AiFeedback>
{
    public void Configure(EntityTypeBuilder<AiFeedback> builder)
    {
        builder.ToTable("AiFeedbacks");

        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.SessionId)
            .HasDatabaseName("IX_AiFeedbacks_SessionId");

        builder.HasIndex(x => x.CreatedAt)
            .HasDatabaseName("IX_AiFeedbacks_CreatedAt");

        builder.Property(x => x.Rating)
               .IsRequired();

        builder.Property(x => x.Comment)
               .HasMaxLength(1000);

        builder.Property(x => x.CreatedAt)
               .IsRequired();

        builder.Property(x => x.UpdatedAt)
               .IsRequired();

        builder.HasOne(x => x.Session)
               .WithMany()
               .HasForeignKey(x => x.SessionId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
