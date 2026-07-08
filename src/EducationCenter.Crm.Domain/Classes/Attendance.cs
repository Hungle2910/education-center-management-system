using System;
using EducationCenter.Crm.Domain.People;

namespace EducationCenter.Crm.Domain.Classes;

public sealed class Attendance
{
    public Guid Id { get; set; }
    
    public Guid OccurrenceId { get; set; }
    public ScheduleOccurrence? Occurrence { get; set; }

    public Guid StudentId { get; set; }
    public Student? Student { get; set; }

    public string Status { get; set; } = "Có mặt"; // Có mặt, Vắng có phép, Vắng không phép, Đi trễ, Đã học bù
    public string? Notes { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? LastModifiedAtUtc { get; set; }
    public string? AuditedBy { get; set; }
}
