using EducationCenter.Crm.Domain.People;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EducationCenter.Crm.Infrastructure.Persistence.Configurations;

public sealed class ParentConfiguration : IEntityTypeConfiguration<Parent>
{
    public void Configure(EntityTypeBuilder<Parent> builder)
    {
        builder.ToTable("Parents");

        builder.HasKey(parent => parent.Id);

        builder.Property(parent => parent.FullName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(parent => parent.Email)
            .HasMaxLength(256);

        builder.HasIndex(parent => parent.Email);

        builder.Property(parent => parent.PhoneNumber)
            .HasMaxLength(20)
            .IsRequired();

        builder.HasIndex(parent => parent.PhoneNumber);

        builder.Property(parent => parent.ZaloLink)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(parent => parent.CreatedAtUtc)
            .IsRequired();
    }
}
