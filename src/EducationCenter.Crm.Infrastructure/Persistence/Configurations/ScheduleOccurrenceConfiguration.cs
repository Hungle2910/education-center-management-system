using EducationCenter.Crm.Domain.Classes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EducationCenter.Crm.Infrastructure.Persistence.Configurations;

public sealed class ScheduleOccurrenceConfiguration : IEntityTypeConfiguration<ScheduleOccurrence>
{
    public void Configure(EntityTypeBuilder<ScheduleOccurrence> builder)
    {
        builder.ToTable("ScheduleOccurrences");

        builder.HasKey(so => so.Id);

        builder.HasOne(so => so.ClassSchedule)
            .WithMany()
            .HasForeignKey(so => so.ClassScheduleId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(so => so.Class)
            .WithMany()
            .HasForeignKey(so => so.ClassId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(so => so.Room)
            .WithMany()
            .HasForeignKey(so => so.RoomId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(so => so.Teacher)
            .WithMany()
            .HasForeignKey(so => so.TeacherId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Property(so => so.Status)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(so => so.Reason)
            .HasMaxLength(500);

        builder.Property(so => so.Date)
            .IsRequired();

        builder.Property(so => so.StartTime)
            .IsRequired();

        builder.Property(so => so.EndTime)
            .IsRequired();

        builder.Property(so => so.GoogleEventId)
            .HasMaxLength(200);

        builder.HasIndex(so => so.GoogleEventId);
    }
}
