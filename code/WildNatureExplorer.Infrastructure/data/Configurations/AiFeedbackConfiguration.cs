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

        builder.Property(x => x.Rating)
               .IsRequired();

        builder.Property(x => x.Comment)
               .HasMaxLength(1000);

        builder.Property(x => x.CreatedAt)
               .IsRequired();
    }
}
