using EducationCenter.Crm.Domain.People;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EducationCenter.Crm.Infrastructure.Persistence.Configurations;

public sealed class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.ToTable("Students");

        builder.HasKey(student => student.Id);

        builder.Property(student => student.StudentCode)
            .HasMaxLength(50);

        builder.HasIndex(student => student.StudentCode);

        builder.Property(student => student.FullName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(student => student.Email)
            .HasMaxLength(256);

        builder.HasIndex(student => student.Email);

        builder.Property(student => student.PhoneNumber)
            .HasMaxLength(20);

        builder.HasIndex(student => student.PhoneNumber);

        builder.Property(student => student.Status)
            .HasMaxLength(50)
            .HasDefaultValue(StudentStatuses.Active)
            .IsRequired();

        builder.Property(student => student.CreatedAtUtc)
            .IsRequired();

        builder.HasOne(student => student.Branch)
            .WithMany()
            .HasForeignKey(student => student.BranchId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
