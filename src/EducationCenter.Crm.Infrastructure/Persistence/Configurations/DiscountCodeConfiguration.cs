using EducationCenter.Crm.Domain.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EducationCenter.Crm.Infrastructure.Persistence.Configurations;

public sealed class DiscountCodeConfiguration : IEntityTypeConfiguration<DiscountCode>
{
    public void Configure(EntityTypeBuilder<DiscountCode> builder)
    {
        builder.ToTable("DiscountCodes");

        builder.HasKey(dc => dc.Id);

        builder.Property(dc => dc.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(dc => dc.Code).IsUnique();

        builder.Property(dc => dc.DiscountType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(dc => dc.Value)
            .HasPrecision(18, 2)
            .IsRequired();
    }
}
