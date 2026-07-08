using EducationCenter.Crm.Domain.People;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EducationCenter.Crm.Infrastructure.Persistence.Configurations;

public sealed class StudentParentConfiguration : IEntityTypeConfiguration<StudentParent>
{
    public void Configure(EntityTypeBuilder<StudentParent> builder)
    {
        builder.ToTable("StudentParents");

        builder.HasKey(studentParent => new
        {
            studentParent.StudentId,
            studentParent.ParentId
        });

        builder.Property(studentParent => studentParent.Relationship)
            .HasMaxLength(50);

        builder.Property(studentParent => studentParent.CreatedAtUtc)
            .IsRequired();

        builder.HasOne(studentParent => studentParent.Student)
            .WithMany(student => student.StudentParents)
            .HasForeignKey(studentParent => studentParent.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(studentParent => studentParent.Parent)
            .WithMany(parent => parent.StudentParents)
            .HasForeignKey(studentParent => studentParent.ParentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
