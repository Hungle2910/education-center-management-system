using EducationCenter.Crm.Domain.Classes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EducationCenter.Crm.Infrastructure.Persistence.Configurations;

public sealed class ClassConfiguration : IEntityTypeConfiguration<Class>
{
    public void Configure(EntityTypeBuilder<Class> builder)
    {
        builder.ToTable("Classes");

        builder.HasKey(classRoom => classRoom.Id);

        builder.Property(classRoom => classRoom.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(classRoom => classRoom.Subject)
            .HasMaxLength(150);

        builder.Property(classRoom => classRoom.MonthlyFee)
            .HasPrecision(18, 2);

        builder.Property(classRoom => classRoom.Status)
            .HasMaxLength(50)
            .HasDefaultValue(ClassStatuses.Upcoming)
            .IsRequired();

        builder.Property(classRoom => classRoom.CreatedAtUtc)
            .IsRequired();

        builder.HasOne(classRoom => classRoom.Teacher)
            .WithMany()
            .HasForeignKey(classRoom => classRoom.TeacherId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
