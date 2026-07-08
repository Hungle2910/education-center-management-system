using EducationCenter.Crm.Domain.Classes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EducationCenter.Crm.Infrastructure.Persistence.Configurations;

public sealed class PaymentTransactionConfiguration : IEntityTypeConfiguration<PaymentTransaction>
{
    public void Configure(EntityTypeBuilder<PaymentTransaction> builder)
    {
        builder.ToTable("PaymentTransactions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Amount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.TransactionCode)
            .HasMaxLength(150);

        builder.Property(x => x.PaymentMethod)
            .HasMaxLength(50);

        builder.Property(x => x.PayeeName)
            .HasMaxLength(150);

        builder.Property(x => x.Note)
            .HasMaxLength(500);

        builder.HasOne(x => x.TuitionInvoice)
            .WithMany(i => i.Transactions)
            .HasForeignKey(x => x.TuitionInvoiceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
