namespace EducationCenter.Crm.Domain.People;

public sealed class StudentParent
{
    public Guid StudentId { get; set; }

    public Student Student { get; set; } = null!;

    public Guid ParentId { get; set; }

    public Parent Parent { get; set; } = null!;

    public string? Relationship { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}
