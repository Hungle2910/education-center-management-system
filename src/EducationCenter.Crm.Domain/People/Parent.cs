namespace EducationCenter.Crm.Domain.People;

public sealed class Parent
{
    public Guid Id { get; set; }

    public required string FullName { get; set; }

    public string? Email { get; set; }

    public required string PhoneNumber { get; set; }

    public required string ZaloLink { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? UpdatedAtUtc { get; set; }

    public ICollection<StudentParent> StudentParents { get; set; } = new List<StudentParent>();
}
