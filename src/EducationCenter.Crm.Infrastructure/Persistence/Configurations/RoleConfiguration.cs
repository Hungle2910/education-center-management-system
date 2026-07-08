using EducationCenter.Crm.Domain.Identity;
using EducationCenter.Crm.Infrastructure.Persistence.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EducationCenter.Crm.Infrastructure.Persistence.Configurations;

public sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles");

        builder.HasKey(role => role.Id);

        builder.Property(role => role.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(role => role.Name)
            .IsUnique();

        builder.Property(role => role.DisplayName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(role => role.Description)
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(role => role.IsSystemRole)
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(role => role.CreatedAtUtc)
            .IsRequired();

        builder.HasData(DefaultRoles.All);
    }
}
