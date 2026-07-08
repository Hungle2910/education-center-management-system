using EducationCenter.Crm.Domain.People;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EducationCenter.Crm.Infrastructure.Persistence.Configurations;

public sealed class ParentCareLogConfiguration : IEntityTypeConfiguration<ParentCareLog>
{
    public void Configure(EntityTypeBuilder<ParentCareLog> builder)
    {
        builder.ToTable("ParentCareLogs");
        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.Parent)
            .WithMany()
            .HasForeignKey(x => x.ParentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Lead)
            .WithMany()
            .HasForeignKey(x => x.LeadId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.ContactType).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Notes).HasMaxLength(1000).IsRequired();
    }
}
