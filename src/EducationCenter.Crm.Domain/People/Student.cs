using EducationCenter.Crm.Domain.Classes;

namespace EducationCenter.Crm.Domain.People;

public sealed class Student
{
    public Guid Id { get; set; }

    public string? StudentCode { get; set; }

    public required string FullName { get; set; }

    public string? Email { get; set; }

    public string? PhoneNumber { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public string Status { get; set; } = StudentStatuses.Active;

    public Guid? BranchId { get; set; }
    public Branch? Branch { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? UpdatedAtUtc { get; set; }

    public ICollection<StudentParent> StudentParents { get; set; } = new List<StudentParent>();
}
