using EducationCenter.Crm.Application.Dashboard;
using EducationCenter.Crm.Domain.Classes;
using EducationCenter.Crm.Domain.People;
using EducationCenter.Crm.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EducationCenter.Crm.Infrastructure.Dashboard;

public sealed class DashboardService : IDashboardService
{
    private const string PaidStatus = "Đã thanh toán";
    private const string PendingConfirmationStatus = "Chờ xác nhận";

    private readonly ApplicationDbContext _dbContext;

    public DashboardService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AdminOverviewResponse> GetAdminOverviewAsync(
        DashboardFilter filter,
        CancellationToken cancellationToken)
    {
        var invoiceQuery = ApplyInvoiceFilter(_dbContext.TuitionInvoices.AsNoTracking(), filter)
            .Where(invoice => invoice.Status == PaidStatus);

        var revenue = await invoiceQuery
            .SumAsync(invoice => invoice.PaidAmount ?? invoice.TotalAmount, cancellationToken);

        var activeStudentCount = await _dbContext.Students
            .AsNoTracking()
            .CountAsync(student => student.Status == StudentStatuses.Active, cancellationToken);

        var classQuery = ApplyClassFilter(_dbContext.Classes.AsNoTracking(), filter);
        var activeClassCount = await classQuery
            .CountAsync(classRoom => classRoom.Status == ClassStatuses.Active, cancellationToken);
        var recruitingClassCount = await classQuery
            .CountAsync(classRoom => classRoom.Status == ClassStatuses.Recruiting, cancellationToken);

        var scheduleConflictCount = await CountScheduleConflictsAsync(filter, cancellationToken);

        return new AdminOverviewResponse(
            revenue,
            activeStudentCount,
            activeClassCount,
            recruitingClassCount,
            scheduleConflictCount);
    }

    public async Task<OperationsDashboardResponse> GetOperationsAsync(
        DashboardFilter filter,
        CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var scheduleQuery = _dbContext.ScheduleOccurrences
            .Include(occurrence => occurrence.Class)
            .Include(occurrence => occurrence.Room)
            .Include(occurrence => occurrence.Teacher)
            .AsNoTracking()
            .Where(occurrence => occurrence.Date == today);

        if (filter.ClassId.HasValue)
        {
            scheduleQuery = scheduleQuery.Where(occurrence => occurrence.ClassId == filter.ClassId.Value);
        }

        var todaySchedules = await scheduleQuery
            .OrderBy(occurrence => occurrence.StartTime)
            .Select(occurrence => new TodayScheduleItem(
                occurrence.Id,
                occurrence.ClassId,
                occurrence.Class != null ? occurrence.Class.Name : "Lớp học",
                occurrence.Date,
                occurrence.StartTime,
                occurrence.EndTime,
                occurrence.RoomId,
                occurrence.Room != null ? occurrence.Room.Name : "Phòng học",
                occurrence.TeacherId,
                occurrence.Teacher != null ? occurrence.Teacher.FullName : null,
                occurrence.Status))
            .ToArrayAsync(cancellationToken);

        var pendingInvoices = await ApplyInvoiceFilter(
                _dbContext.TuitionInvoices
                    .Include(invoice => invoice.Student)
                    .Include(invoice => invoice.Class)
                    .AsNoTracking(),
                filter)
            .Where(invoice => invoice.Status == PendingConfirmationStatus)
            .OrderByDescending(invoice => invoice.CreatedAtUtc)
            .Select(invoice => new PendingPaymentInvoiceItem(
                invoice.Id,
                invoice.StudentId,
                invoice.Student != null ? invoice.Student.FullName : "Học sinh",
                invoice.ClassId,
                invoice.Class != null ? invoice.Class.Name : "Lớp học",
                invoice.Month,
                invoice.TotalAmount,
                invoice.PaidAmount,
                invoice.Status,
                invoice.PaymentProofUrl,
                invoice.CreatedAtUtc))
            .ToArrayAsync(cancellationToken);

        return new OperationsDashboardResponse(
            todaySchedules,
            TrialSessionsTodayCount: 0,
            pendingInvoices);
    }

    private async Task<int> CountScheduleConflictsAsync(
        DashboardFilter filter,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.ScheduleOccurrences
            .AsNoTracking();

        if (filter.ClassId.HasValue)
        {
            query = query.Where(occurrence => occurrence.ClassId == filter.ClassId.Value);
        }

        if (filter.Month.HasValue && filter.Year.HasValue)
        {
            query = query.Where(occurrence =>
                occurrence.Date.Month == filter.Month.Value &&
                occurrence.Date.Year == filter.Year.Value);
        }
        else if (filter.Year.HasValue)
        {
            query = query.Where(occurrence => occurrence.Date.Year == filter.Year.Value);
        }

        var occurrences = await query
            .Select(occurrence => new
            {
                occurrence.Id,
                occurrence.Date,
                occurrence.StartTime,
                occurrence.EndTime,
                occurrence.RoomId,
                occurrence.TeacherId
            })
            .ToArrayAsync(cancellationToken);

        return occurrences.Count(current => occurrences.Any(other =>
            other.Id != current.Id &&
            other.Date == current.Date &&
            current.StartTime < other.EndTime &&
            other.StartTime < current.EndTime &&
            (other.RoomId == current.RoomId ||
             (current.TeacherId.HasValue && other.TeacherId == current.TeacherId))));
    }

    private static IQueryable<TuitionInvoice> ApplyInvoiceFilter(
        IQueryable<TuitionInvoice> query,
        DashboardFilter filter)
    {
        if (filter.ClassId.HasValue)
        {
            query = query.Where(invoice => invoice.ClassId == filter.ClassId.Value);
        }

        if (filter.Month.HasValue && filter.Year.HasValue)
        {
            var monthValue = $"{filter.Year.Value:D4}-{filter.Month.Value:D2}";
            query = query.Where(invoice => invoice.Month == monthValue);
        }
        else if (filter.Year.HasValue)
        {
            var yearPrefix = $"{filter.Year.Value:D4}-";
            query = query.Where(invoice => invoice.Month.StartsWith(yearPrefix));
        }

        return query;
    }

    private static IQueryable<Class> ApplyClassFilter(
        IQueryable<Class> query,
        DashboardFilter filter)
    {
        return filter.ClassId.HasValue
            ? query.Where(classRoom => classRoom.Id == filter.ClassId.Value)
            : query;
    }
}
