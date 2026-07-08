using EducationCenter.Crm.Domain.People;

namespace EducationCenter.Crm.Domain.Classes;

public sealed class Class
{
    public Guid Id { get; set; }

    public required string Name { get; set; }

    public string? Subject { get; set; }

    public decimal MonthlyFee { get; set; }

    public Guid? TeacherId { get; set; }

    public Teacher? Teacher { get; set; }

    public string Status { get; set; } = ClassStatuses.Upcoming;

    public int MinStudents { get; set; }

    public int MaxStudents { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? UpdatedAtUtc { get; set; }
}
