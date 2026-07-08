using System;
using System.Collections.Generic;

namespace EducationCenter.Crm.Application.Admissions;

public sealed record LeadDto(
    Guid Id,
    string StudentName,
    string? ParentName,
    string ParentPhone,
    string? Email,
    string? Source,
    string Status,
    string? Notes,
    DateTime CreatedAtUtc);

public sealed record CreateLeadRequest(
    string StudentName,
    string? ParentName,
    string ParentPhone,
    string? Email,
    string? Source,
    string? Notes);

public sealed record UpdateLeadRequest(
    string StudentName,
    string? ParentName,
    string ParentPhone,
    string? Email,
    string? Source,
    string Status,
    string? Notes);

public sealed record TrialSessionDto(
    Guid Id,
    Guid LeadId,
    string StudentName,
    Guid ClassId,
    string ClassName,
    DateOnly TrialDate,
    Guid? TeacherId,
    string? TeacherName,
    string? Feedback,
    string? Result,
    string? Notes,
    DateTime CreatedAtUtc);

public sealed record ScheduleTrialRequest(
    Guid LeadId,
    Guid ClassId,
    DateOnly TrialDate,
    Guid? TeacherId,
    string? Notes);

public sealed record EvaluateTrialRequest(
    string? Feedback,
    string Result, // Đăng ký, Không đăng ký
    string? Notes);

public sealed record ParentCareLogDto(
    Guid Id,
    Guid? ParentId,
    string? ParentName,
    Guid? LeadId,
    string? LeadStudentName,
    Guid StaffId,
    string ContactType,
    string Notes,
    DateTime LoggedAtUtc);

public sealed record CreateCareLogRequest(
    Guid? ParentId,
    Guid? LeadId,
    Guid StaffId,
    string ContactType,
    string Notes);
