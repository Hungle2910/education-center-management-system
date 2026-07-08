using System;
using EducationCenter.Crm.Domain.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EducationCenter.Crm.Infrastructure.Persistence.Configurations;

public sealed class PaymentSettingConfiguration : IEntityTypeConfiguration<PaymentSetting>
{
    public void Configure(EntityTypeBuilder<PaymentSetting> builder)
    {
        builder.ToTable("PaymentSettings");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.BankId)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.BankName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.AccountNo)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.AccountName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.VietQrTemplate)
            .HasMaxLength(50)
            .HasDefaultValue("compact2");

        builder.HasData(new PaymentSetting
        {
            Id = Guid.Parse("99999999-9999-9999-9999-999999999999"),
            BankName = "Vietcombank",
            BankId = "vietcombank",
            AccountNo = "1021965186",
            AccountName = "LE DOAN GIA HUNG",
            VietQrTemplate = "compact2",
            IsDefault = true,
            IsActive = true,
            CreatedAtUtc = new DateTime(2026, 7, 7, 0, 0, 0, DateTimeKind.Utc)
        });
    }
}
