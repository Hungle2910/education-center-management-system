using EducationCenter.Crm.Domain.Classes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EducationCenter.Crm.Infrastructure.Persistence.Configurations;

public sealed class TuitionInvoiceConfiguration : IEntityTypeConfiguration<TuitionInvoice>
{
    public void Configure(EntityTypeBuilder<TuitionInvoice> builder)
    {
        builder.ToTable("TuitionInvoices");

        builder.HasKey(ti => ti.Id);

        builder.Property(ti => ti.Month)
            .IsRequired()
            .HasMaxLength(7); // "YYYY-MM"

        builder.Property(ti => ti.BaseAmount).HasPrecision(18, 2);
        builder.Property(ti => ti.DiscountAmount).HasPrecision(18, 2);
        builder.Property(ti => ti.DeductAmount).HasPrecision(18, 2);
        builder.Property(ti => ti.AdjustAmount).HasPrecision(18, 2);
        builder.Property(ti => ti.TotalAmount).HasPrecision(18, 2);

        builder.Property(ti => ti.AdjustReason).HasMaxLength(300);
        builder.Property(ti => ti.Status).IsRequired().HasMaxLength(50);
        builder.Property(ti => ti.PaymentProofUrl).HasMaxLength(500);
        builder.Property(ti => ti.VietQrUrl).HasMaxLength(500);
        builder.Property(ti => ti.OperationHistory).HasMaxLength(4000);

        builder.HasOne(ti => ti.Student)
            .WithMany()
            .HasForeignKey(ti => ti.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ti => ti.Class)
            .WithMany()
            .HasForeignKey(ti => ti.ClassId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(ti => new { ti.StudentId, ti.ClassId, ti.Month }).IsUnique();
    }
}
