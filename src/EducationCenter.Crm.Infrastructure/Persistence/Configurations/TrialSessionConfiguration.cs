using EducationCenter.Crm.Domain.People;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EducationCenter.Crm.Infrastructure.Persistence.Configurations;

public sealed class TrialSessionConfiguration : IEntityTypeConfiguration<TrialSession>
{
    public void Configure(EntityTypeBuilder<TrialSession> builder)
    {
        builder.ToTable("TrialSessions");
        builder.HasKey(x => x.Id);
        
        builder.HasOne(x => x.Lead)
            .WithMany()
            .HasForeignKey(x => x.LeadId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Class)
            .WithMany()
            .HasForeignKey(x => x.ClassId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Teacher)
            .WithMany()
            .HasForeignKey(x => x.TeacherId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Property(x => x.Feedback).HasMaxLength(500);
        builder.Property(x => x.Result).HasMaxLength(50);
        builder.Property(x => x.Notes).HasMaxLength(500);
    }
}
