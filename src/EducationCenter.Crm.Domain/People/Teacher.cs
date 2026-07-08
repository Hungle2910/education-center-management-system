namespace EducationCenter.Crm.Domain.People;

public sealed class Teacher
{
    public Guid Id { get; set; }

    public required string FullName { get; set; }

    public string? Email { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Subject { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? UpdatedAtUtc { get; set; }
}
