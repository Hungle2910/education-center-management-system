using EducationCenter.Crm.Domain.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EducationCenter.Crm.Infrastructure.Persistence.Configurations;

public sealed class SystemSettingConfiguration : IEntityTypeConfiguration<SystemSetting>
{
    public void Configure(EntityTypeBuilder<SystemSetting> builder)
    {
        builder.ToTable("SystemSettings");

        builder.HasKey(setting => setting.Key);

        builder.Property(setting => setting.Key)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(setting => setting.Value)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(setting => setting.Description)
            .HasMaxLength(300);

        builder.Property(setting => setting.IsSensitive)
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(setting => setting.UpdatedAtUtc)
            .IsRequired();
    }
}
