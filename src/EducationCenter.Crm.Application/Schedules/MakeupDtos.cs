using System;

namespace EducationCenter.Crm.Application.Schedules;

public sealed record CancelSessionRequest(
    string Action // "Tạo học bù" hoặc "Trừ học phí"
);

public sealed record ScheduleIndividualMakeupRequest(
    Guid StudentId,
    Guid AbsentOccurrenceId,
    Guid MakeupOccurrenceId
);

public sealed record EligibleAbsentStudentDto(
    Guid StudentId,
    string StudentName,
    Guid AbsentOccurrenceId,
    string? Notes
);
