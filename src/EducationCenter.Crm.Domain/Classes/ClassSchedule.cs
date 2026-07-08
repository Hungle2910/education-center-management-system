using System;

namespace EducationCenter.Crm.Domain.Classes;

public sealed class ClassSchedule
{
    public Guid Id { get; set; }

    public Guid ClassId { get; set; }

    public Class? Class { get; set; }

    public DayOfWeek DayOfWeek { get; set; }

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public Guid RoomId { get; set; }

    public Room? Room { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? UpdatedAtUtc { get; set; }
}
