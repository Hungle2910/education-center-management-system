using System;
using EducationCenter.Crm.Domain.People;

namespace EducationCenter.Crm.Domain.Classes;

public sealed class IndividualMakeup
{
    public Guid Id { get; set; }

    public Guid StudentId { get; set; }
    public Student? Student { get; set; }

    public Guid AbsentOccurrenceId { get; set; }
    public ScheduleOccurrence? AbsentOccurrence { get; set; }

    public Guid MakeupOccurrenceId { get; set; }
    public ScheduleOccurrence? MakeupOccurrence { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
