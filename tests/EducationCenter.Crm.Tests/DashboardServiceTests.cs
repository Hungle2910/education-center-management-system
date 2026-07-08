using EducationCenter.Crm.Application.Dashboard;
using EducationCenter.Crm.Domain.Classes;
using EducationCenter.Crm.Domain.People;
using EducationCenter.Crm.Infrastructure.Dashboard;
using EducationCenter.Crm.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EducationCenter.Crm.Tests;

public sealed class DashboardServiceTests
{
    [Fact]
    public async Task GetAdminOverviewAsync_CountsOnlyPaidInvoicesAsRevenue()
    {
        using var dbContext = CreateDbContext();
        var classId = Guid.NewGuid();
        var studentId = Guid.NewGuid();

        dbContext.Classes.Add(new Class
        {
            Id = classId,
            Name = "Toán 10",
            Status = ClassStatuses.Active,
            CreatedAtUtc = DateTime.UtcNow
        });
        dbContext.Students.Add(new Student
        {
            Id = studentId,
            FullName = "Nguyễn Văn A",
            Status = StudentStatuses.Active,
            CreatedAtUtc = DateTime.UtcNow
        });
        dbContext.TuitionInvoices.AddRange(
            CreateInvoice(studentId, classId, "2026-07", "Đã thanh toán", 1_500_000, paidAmount: 1_500_000),
            CreateInvoice(studentId, classId, "2026-07", "Chưa thanh toán", 1_200_000),
            CreateInvoice(studentId, classId, "2026-07", "Đã hủy", 900_000));
        await dbContext.SaveChangesAsync();

        var service = new DashboardService(dbContext);

        var overview = await service.GetAdminOverviewAsync(
            new DashboardFilter(Month: 7, Year: 2026, ClassId: null),
            CancellationToken.None);

        Assert.Equal(1_500_000, overview.TotalTuitionRevenue);
        Assert.Equal(1, overview.ActiveStudentCount);
        Assert.Equal(1, overview.ActiveClassCount);
    }

    [Fact]
    public async Task GetAdminOverviewAsync_CountsScheduleConflictsByRoomOrTeacher()
    {
        using var dbContext = CreateDbContext();
        var classId = Guid.NewGuid();
        var roomId = Guid.NewGuid();
        var teacherId = Guid.NewGuid();

        dbContext.Classes.Add(new Class
        {
            Id = classId,
            Name = "Toán 10",
            Status = ClassStatuses.Active,
            CreatedAtUtc = DateTime.UtcNow
        });
        dbContext.ScheduleOccurrences.AddRange(
            CreateOccurrence(classId, roomId, teacherId, new TimeOnly(8, 0), new TimeOnly(9, 30)),
            CreateOccurrence(classId, roomId, Guid.NewGuid(), new TimeOnly(9, 0), new TimeOnly(10, 0)),
            CreateOccurrence(classId, Guid.NewGuid(), teacherId, new TimeOnly(9, 0), new TimeOnly(10, 0)));
        await dbContext.SaveChangesAsync();

        var service = new DashboardService(dbContext);

        var overview = await service.GetAdminOverviewAsync(
            new DashboardFilter(Month: 7, Year: 2026, ClassId: null),
            CancellationToken.None);

        Assert.Equal(3, overview.ScheduleConflictCount);
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static TuitionInvoice CreateInvoice(
        Guid studentId,
        Guid classId,
        string month,
        string status,
        decimal totalAmount,
        decimal? paidAmount = null)
    {
        return new TuitionInvoice
        {
            Id = Guid.NewGuid(),
            StudentId = studentId,
            ClassId = classId,
            Month = month,
            BaseAmount = totalAmount,
            TotalAmount = totalAmount,
            PaidAmount = paidAmount,
            Status = status,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    private static ScheduleOccurrence CreateOccurrence(
        Guid classId,
        Guid roomId,
        Guid teacherId,
        TimeOnly startTime,
        TimeOnly endTime)
    {
        return new ScheduleOccurrence
        {
            Id = Guid.NewGuid(),
            ClassId = classId,
            RoomId = roomId,
            TeacherId = teacherId,
            Date = new DateOnly(2026, 7, 7),
            StartTime = startTime,
            EndTime = endTime,
            Status = "Đã lên lịch",
            CreatedAtUtc = DateTime.UtcNow
        };
    }
}
