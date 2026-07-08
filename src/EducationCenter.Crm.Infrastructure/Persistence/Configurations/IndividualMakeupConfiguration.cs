using EducationCenter.Crm.Domain.Classes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EducationCenter.Crm.Infrastructure.Persistence.Configurations;

public sealed class IndividualMakeupConfiguration : IEntityTypeConfiguration<IndividualMakeup>
{
    public void Configure(EntityTypeBuilder<IndividualMakeup> builder)
    {
        builder.ToTable("IndividualMakeups");

        builder.HasKey(im => im.Id);

        builder.HasOne(im => im.Student)
            .WithMany()
            .HasForeignKey(im => im.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(im => im.AbsentOccurrence)
            .WithMany()
            .HasForeignKey(im => im.AbsentOccurrenceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(im => im.MakeupOccurrence)
            .WithMany()
            .HasForeignKey(im => im.MakeupOccurrenceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(im => new { im.StudentId, im.AbsentOccurrenceId }).IsUnique();
    }
}
