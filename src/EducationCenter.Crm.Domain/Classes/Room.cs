namespace EducationCenter.Crm.Domain.Classes;

public sealed class Room
{
    public Guid Id { get; set; }

    public required string Name { get; set; }

    public int Capacity { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? UpdatedAtUtc { get; set; }
}
