using EducationCenter.Crm.Domain.Classes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EducationCenter.Crm.Infrastructure.Persistence.Configurations;

public sealed class AttendanceConfiguration : IEntityTypeConfiguration<EducationCenter.Crm.Domain.Classes.Attendance>
{
    public void Configure(EntityTypeBuilder<EducationCenter.Crm.Domain.Classes.Attendance> builder)
    {
        builder.ToTable("Attendances");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Status)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(a => a.Notes)
            .HasMaxLength(500);

        builder.Property(a => a.AuditedBy)
            .HasMaxLength(100);

        builder.HasOne(a => a.Occurrence)
            .WithMany()
            .HasForeignKey(a => a.OccurrenceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.Student)
            .WithMany()
            .HasForeignKey(a => a.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes for performance
        builder.HasIndex(a => new { a.OccurrenceId, a.StudentId }).IsUnique();
    }
}
