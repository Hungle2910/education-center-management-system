using EducationCenter.Crm.Domain.People;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EducationCenter.Crm.Infrastructure.Persistence.Configurations;

public sealed class TeacherConfiguration : IEntityTypeConfiguration<Teacher>
{
    public void Configure(EntityTypeBuilder<Teacher> builder)
    {
        builder.ToTable("Teachers");

        builder.HasKey(teacher => teacher.Id);

        builder.Property(teacher => teacher.FullName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(teacher => teacher.Email)
            .HasMaxLength(256);

        builder.HasIndex(teacher => teacher.Email);

        builder.Property(teacher => teacher.PhoneNumber)
            .HasMaxLength(20);

        builder.HasIndex(teacher => teacher.PhoneNumber);

        builder.Property(teacher => teacher.Subject)
            .HasMaxLength(150);

        builder.Property(teacher => teacher.IsActive)
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(teacher => teacher.CreatedAtUtc)
            .IsRequired();
    }
}
