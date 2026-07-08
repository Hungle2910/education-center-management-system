using EducationCenter.Crm.Domain.People;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EducationCenter.Crm.Infrastructure.Persistence.Configurations;

public sealed class LeadConfiguration : IEntityTypeConfiguration<Lead>
{
    public void Configure(EntityTypeBuilder<Lead> builder)
    {
        builder.ToTable("Leads");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.StudentName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.ParentName).HasMaxLength(100);
        builder.Property(x => x.ParentPhone).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Email).HasMaxLength(100);
        builder.Property(x => x.Source).HasMaxLength(100);
        builder.Property(x => x.Status).HasMaxLength(50).IsRequired();
    }
}
