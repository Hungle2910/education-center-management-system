using System;
using EducationCenter.Crm.Domain.People;

namespace EducationCenter.Crm.Domain.Classes;

public sealed class ScheduleOccurrence
{
    public Guid Id { get; set; }

    public Guid? ClassScheduleId { get; set; }

    public ClassSchedule? ClassSchedule { get; set; }

    public Guid ClassId { get; set; }

    public Class? Class { get; set; }

    public DateOnly Date { get; set; }

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public Guid RoomId { get; set; }

    public Room? Room { get; set; }

    public Guid? TeacherId { get; set; }

    public Teacher? Teacher { get; set; }

    public string Status { get; set; } = "Đã lên lịch";

    public string? Reason { get; set; }
    
    public string? GoogleEventId { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? UpdatedAtUtc { get; set; }
}
