using System;
using EducationCenter.Crm.Domain.Classes;

namespace EducationCenter.Crm.Application.Schedules;

public sealed record CreateScheduleRequest(
    Guid ClassId,
    DayOfWeek DayOfWeek,
    TimeOnly StartTime,
    TimeOnly EndTime,
    Guid RoomId,
    Guid? TeacherId);

public sealed record ScheduleResponse(
    Guid Id,
    Guid ClassId,
    string ClassName,
    DayOfWeek DayOfWeek,
    TimeOnly StartTime,
    TimeOnly EndTime,
    Guid RoomId,
    string RoomName,
    Guid? TeacherId,
    string? TeacherName);

public sealed record ScheduleOccurrenceResponse(
    Guid Id,
    Guid ClassId,
    string ClassName,
    DateOnly Date,
    TimeOnly StartTime,
    TimeOnly EndTime,
    Guid RoomId,
    string RoomName,
    Guid? TeacherId,
    string? TeacherName,
    string Status,
    string? Reason);

public sealed record ConflictCheckRequest(
    Guid? ExcludeOccurrenceId,
    DateOnly Date,
    TimeOnly StartTime,
    TimeOnly EndTime,
    Guid RoomId,
    Guid? TeacherId);

public sealed record ConflictCheckResponse(
    bool HasConflict,
    string? Message);
