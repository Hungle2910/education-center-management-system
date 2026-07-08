using System;
using System.Collections.Generic;

namespace EducationCenter.Crm.Application.Reports;

public sealed record TuitionReportResponse(
    decimal TotalCollected,
    decimal TotalUnpaid,
    decimal TotalOverdue,
    IReadOnlyCollection<ClassRevenueItem> RevenueByClass,
    IReadOnlyCollection<MonthlyRevenueItem> RevenueByMonth);

public sealed record ClassRevenueItem(
    Guid ClassId,
    string ClassName,
    decimal Revenue);

public sealed record MonthlyRevenueItem(
    string Month,
    decimal Revenue);

public sealed record ClassReportItem(
    Guid ClassId,
    string ClassName,
    int ActiveStudentCount,
    int TargetStudentCount,
    string Status,
    bool IsAtRiskOfLoss);

public sealed record TeacherReportItem(
    Guid TeacherId,
    string TeacherName,
    int CompletedLessonsCount,
    int CancelledLessonsCount,
    int MakeupLessonsCount,
    decimal ProjectedSalary,
    decimal PaidSalary);
