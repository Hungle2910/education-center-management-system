namespace EducationCenter.Crm.Application.Dashboard;

public sealed record DashboardFilter(
    int? Month,
    int? Year,
    Guid? ClassId);

public sealed record AdminOverviewResponse(
    decimal TotalTuitionRevenue,
    int ActiveStudentCount,
    int ActiveClassCount,
    int RecruitingClassCount,
    int ScheduleConflictCount);

public sealed record OperationsDashboardResponse(
    IReadOnlyCollection<TodayScheduleItem> TodaySchedules,
    int TrialSessionsTodayCount,
    IReadOnlyCollection<PendingPaymentInvoiceItem> PendingPaymentInvoices);

public sealed record TodayScheduleItem(
    Guid OccurrenceId,
    Guid ClassId,
    string ClassName,
    DateOnly Date,
    TimeOnly StartTime,
    TimeOnly EndTime,
    Guid RoomId,
    string RoomName,
    Guid? TeacherId,
    string? TeacherName,
    string Status);

public sealed record PendingPaymentInvoiceItem(
    Guid InvoiceId,
    Guid StudentId,
    string StudentName,
    Guid ClassId,
    string ClassName,
    string Month,
    decimal TotalAmount,
    decimal? PaidAmount,
    string Status,
    string? PaymentProofUrl,
    DateTime CreatedAtUtc);
