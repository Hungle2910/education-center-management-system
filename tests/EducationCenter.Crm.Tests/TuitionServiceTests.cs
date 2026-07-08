using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EducationCenter.Crm.Application.Common.Interfaces;
using EducationCenter.Crm.Application.Tuition;
using EducationCenter.Crm.Domain.Classes;
using EducationCenter.Crm.Domain.People;
using EducationCenter.Crm.Domain.Settings;
using EducationCenter.Crm.Infrastructure.Persistence;
using EducationCenter.Crm.Infrastructure.Tuition;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EducationCenter.Crm.Tests;

public sealed class TuitionServiceTests
{
    private static ApplicationDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    // ─── Seed Helpers ─────────────────────────────────────────────────────────

    private static async Task<(Guid classId, Guid studentId, Guid occurrenceId)> SeedClassWithAttendanceAsync(
        ApplicationDbContext db,
        decimal monthlyFee,
        string month,
        string occurrenceStatus = "Đã học",
        string? occurrenceReason = null)
    {
        var teacher = new Teacher { Id = Guid.NewGuid(), FullName = "Giáo viên A", Email = "a@test.com" };
        var student = new Student { Id = Guid.NewGuid(), FullName = "Học sinh X", Email = "x@test.com" };
        var cls = new Class
        {
            Id = Guid.NewGuid(),
            Name = "Toán 10",
            MonthlyFee = monthlyFee,
            CreatedAtUtc = DateTime.UtcNow
        };

        var parts = month.Split('-');
        var occurrence = new ScheduleOccurrence
        {
            Id = Guid.NewGuid(),
            ClassId = cls.Id,
            Date = new DateOnly(int.Parse(parts[0]), int.Parse(parts[1]), 15),
            StartTime = new TimeOnly(8, 0),
            EndTime = new TimeOnly(10, 0),
            RoomId = Guid.NewGuid(),
            Status = occurrenceStatus,
            Reason = occurrenceReason,
            CreatedAtUtc = DateTime.UtcNow
        };

        var attendance = new EducationCenter.Crm.Domain.Classes.Attendance
        {
            Id = Guid.NewGuid(),
            OccurrenceId = occurrence.Id,
            StudentId = student.Id,
            Status = "Có mặt"
        };

        db.Teachers.Add(teacher);
        db.Students.Add(student);
        db.Classes.Add(cls);
        db.ScheduleOccurrences.Add(occurrence);
        db.Attendances.Add(attendance);
        await db.SaveChangesAsync();
        return (cls.Id, student.Id, occurrence.Id);
    }

    // ─── Tests ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task PreviewTuitionAsync_NoOccurrences_ReturnsZeroDeduction()
    {
        using var db = CreateDb();
        var (classId, studentId, _) = await SeedClassWithAttendanceAsync(db, 1_500_000, "2026-07");
        var service = CreateService(db);

        var result = await service.PreviewTuitionAsync(new GenerateTuitionRequest(classId, "2026-07"));

        Assert.Single(result);
        Assert.Equal(0, result[0].CancelledSessions);
        Assert.Equal(1_500_000, result[0].EstimatedTotal);
    }

    [Fact]
    public async Task PreviewTuitionAsync_WithCancelledSession_DeductsCorrectly()
    {
        using var db = CreateDb();
        // Add one normal occurrence and one cancelled (Trừ học phí tháng sau)
        var (classId, studentId, _) = await SeedClassWithAttendanceAsync(db, 2_000_000, "2026-07");
        var cancelOcc = new ScheduleOccurrence
        {
            Id = Guid.NewGuid(),
            ClassId = classId,
            Date = new DateOnly(2026, 7, 22),
            StartTime = new TimeOnly(8, 0),
            EndTime = new TimeOnly(10, 0),
            RoomId = Guid.NewGuid(),
            Status = "Đã hủy",
            Reason = "Trừ học phí tháng sau",
            CreatedAtUtc = DateTime.UtcNow
        };
        db.ScheduleOccurrences.Add(cancelOcc);
        await db.SaveChangesAsync();

        var service = CreateService(db);
        var result = await service.PreviewTuitionAsync(new GenerateTuitionRequest(classId, "2026-07"));

        Assert.Single(result);
        Assert.Equal(1, result[0].CancelledSessions);
        Assert.Equal(2, result[0].TotalSessions);
        // 2_000_000 / 2 sessions = 1_000_000 deduct per cancelled session
        Assert.Equal(1_000_000, result[0].DeductAmount);
        Assert.Equal(1_000_000, result[0].EstimatedTotal);
    }

    [Fact]
    public async Task GenerateTuitionAsync_CreatesSingleInvoice()
    {
        using var db = CreateDb();
        var (classId, studentId, _) = await SeedClassWithAttendanceAsync(db, 1_200_000, "2026-07");
        var service = CreateService(db);

        var result = await service.GenerateTuitionAsync(new GenerateTuitionRequest(classId, "2026-07"));

        Assert.Single(result);
        Assert.Equal(1_200_000, result[0].BaseAmount);
        Assert.Equal("Chưa thanh toán", result[0].Status);
    }

    [Fact]
    public async Task GenerateTuitionAsync_NoDuplicateInvoicesOnRerun()
    {
        using var db = CreateDb();
        var (classId, studentId, _) = await SeedClassWithAttendanceAsync(db, 1_000_000, "2026-07");
        var service = CreateService(db);

        await service.GenerateTuitionAsync(new GenerateTuitionRequest(classId, "2026-07"));
        var second = await service.GenerateTuitionAsync(new GenerateTuitionRequest(classId, "2026-07"));

        Assert.Empty(second); // no duplicates
        Assert.Single(await service.GetInvoicesAsync(classId, "2026-07"));
    }

    [Fact]
    public async Task AdjustInvoiceAsync_UpdatesAmountAndLogsAudit()
    {
        using var db = CreateDb();
        var (classId, studentId, _) = await SeedClassWithAttendanceAsync(db, 1_000_000, "2026-07");
        var service = CreateService(db);

        var invoices = await service.GenerateTuitionAsync(new GenerateTuitionRequest(classId, "2026-07"));
        var adjusted = await service.AdjustInvoiceAsync(invoices[0].Id, new AdjustTuitionRequest(100_000, "Thêm phí tài liệu"));

        Assert.Equal(100_000, adjusted.AdjustAmount);
        Assert.Equal(1_100_000, adjusted.TotalAmount);
        Assert.False(string.IsNullOrEmpty(adjusted.OperationHistory), "OperationHistory should have audit entry");
    }

    [Fact]
    public async Task ApplyDiscountAsync_PercentageCode_ReducesTotal()
    {
        using var db = CreateDb();
        var (classId, studentId, _) = await SeedClassWithAttendanceAsync(db, 2_000_000, "2026-07");

        var code = new DiscountCode
        {
            Id = Guid.NewGuid(),
            Code = "GIAM10",
            DiscountType = "Percentage",
            Value = 10,
            StartDate = new DateOnly(2026, 1, 1),
            EndDate = new DateOnly(2026, 12, 31),
            MaxUses = 100,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };
        db.DiscountCodes.Add(code);
        await db.SaveChangesAsync();

        var service = CreateService(db);
        var invoices = await service.GenerateTuitionAsync(new GenerateTuitionRequest(classId, "2026-07"));
        var result = await service.ApplyDiscountAsync(invoices[0].Id, new ApplyDiscountRequest("GIAM10"));

        Assert.Equal(200_000, result.DiscountAmount);   // 10% of 2M
        Assert.Equal(1_800_000, result.TotalAmount);
    }

    [Fact]
    public async Task ApplyDiscountAsync_InvalidCode_ThrowsInvalidOperation()
    {
        using var db = CreateDb();
        var (classId, studentId, _) = await SeedClassWithAttendanceAsync(db, 1_000_000, "2026-07");
        var service = CreateService(db);

        var invoices = await service.GenerateTuitionAsync(new GenerateTuitionRequest(classId, "2026-07"));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.ApplyDiscountAsync(invoices[0].Id, new ApplyDiscountRequest("INVALID")));
    }

    [Fact]
    public async Task GenerateVietQrAsync_UsesDefaultBankSetting_ReturnsQrUrl()
    {
        using var db = CreateDb();
        var (classId, studentId, _) = await SeedClassWithAttendanceAsync(db, 1_500_000, "2026-07");

        // Seed a default PaymentSetting
        var setting = new PaymentSetting
        {
            Id = Guid.NewGuid(),
            BankName = "Vietcombank",
            BankId = "vietcombank",
            AccountNo = "1021965186",
            AccountName = "LE DOAN GIA HUNG",
            VietQrTemplate = "compact2",
            IsDefault = true,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };
        db.PaymentSettings.Add(setting);
        await db.SaveChangesAsync();

        var service = CreateService(db);
        var invoices = await service.GenerateTuitionAsync(new GenerateTuitionRequest(classId, "2026-07"));
        var result = await service.GenerateVietQrAsync(invoices[0].Id, new GenerateVietQrRequest(), CancellationToken.None);

        Assert.False(result.VietQrOutdated);
        Assert.NotNull(result.VietQrUrl);
        Assert.Contains("vietcombank", result.VietQrUrl);
        Assert.Contains("1021965186", result.VietQrUrl);
        Assert.Contains("amount=1500000", result.VietQrUrl);
        Assert.NotNull(result.VietQrGeneratedAt);
    }

    [Fact]
    public async Task GenerateVietQrAsync_WithOverrideContent_UsesOverrideContent()
    {
        using var db = CreateDb();
        var (classId, studentId, _) = await SeedClassWithAttendanceAsync(db, 1_000_000, "2026-07");

        var setting = new PaymentSetting
        {
            Id = Guid.NewGuid(),
            BankName = "Vietcombank",
            BankId = "vietcombank",
            AccountNo = "1021965186",
            AccountName = "LE DOAN GIA HUNG",
            VietQrTemplate = "compact2",
            IsDefault = true,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };
        db.PaymentSettings.Add(setting);
        await db.SaveChangesAsync();

        var service = CreateService(db);
        var invoices = await service.GenerateTuitionAsync(new GenerateTuitionRequest(classId, "2026-07"));
        var overrideContent = "HOCPHI-TEST-001";
        var result = await service.GenerateVietQrAsync(invoices[0].Id, new GenerateVietQrRequest(overrideContent), CancellationToken.None);

        Assert.Contains(Uri.EscapeDataString(overrideContent), result.VietQrUrl!);
        Assert.Equal(overrideContent, result.PaymentContent);
    }

    [Fact]
    public async Task GenerateVietQrAsync_NoDefaultBank_ThrowsInvalidOperation()
    {
        using var db = CreateDb();
        var (classId, studentId, _) = await SeedClassWithAttendanceAsync(db, 1_000_000, "2026-07");
        var service = CreateService(db);

        var invoices = await service.GenerateTuitionAsync(new GenerateTuitionRequest(classId, "2026-07"));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.GenerateVietQrAsync(invoices[0].Id, new GenerateVietQrRequest(), CancellationToken.None));
    }

    [Fact]
    public async Task AdjustInvoice_AfterQrGenerated_MarksQrOutdated()
    {
        using var db = CreateDb();
        var (classId, studentId, _) = await SeedClassWithAttendanceAsync(db, 2_000_000, "2026-07");

        var setting = new PaymentSetting
        {
            Id = Guid.NewGuid(),
            BankName = "Vietcombank",
            BankId = "vietcombank",
            AccountNo = "1021965186",
            AccountName = "LE DOAN GIA HUNG",
            VietQrTemplate = "compact2",
            IsDefault = true,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };
        db.PaymentSettings.Add(setting);
        await db.SaveChangesAsync();

        var service = CreateService(db);
        var invoices = await service.GenerateTuitionAsync(new GenerateTuitionRequest(classId, "2026-07"));
        // Generate QR first
        var qrResult = await service.GenerateVietQrAsync(invoices[0].Id, new GenerateVietQrRequest(), CancellationToken.None);
        Assert.False(qrResult.VietQrOutdated);

        // Now adjust the invoice
        var adjusted = await service.AdjustInvoiceAsync(invoices[0].Id, new AdjustTuitionRequest(200_000, "Phí phần mềm"));

        Assert.True(adjusted.VietQrOutdated, "QR phải được đánh dấu outdated sau khi chỉnh sửa số tiền");
    }

    [Fact]
    public async Task UpdatePaymentContentAsync_ChangesContent_MarksQrOutdated()
    {
        using var db = CreateDb();
        var (classId, studentId, _) = await SeedClassWithAttendanceAsync(db, 1_000_000, "2026-07");

        var setting = new PaymentSetting
        {
            Id = Guid.NewGuid(),
            BankName = "Vietcombank",
            BankId = "vietcombank",
            AccountNo = "1021965186",
            AccountName = "LE DOAN GIA HUNG",
            VietQrTemplate = "compact2",
            IsDefault = true,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };
        db.PaymentSettings.Add(setting);
        await db.SaveChangesAsync();

        var service = CreateService(db);
        var invoices = await service.GenerateTuitionAsync(new GenerateTuitionRequest(classId, "2026-07"));
        await service.GenerateVietQrAsync(invoices[0].Id, new GenerateVietQrRequest(), CancellationToken.None);

        var updated = await service.UpdatePaymentContentAsync(invoices[0].Id, "NOIDUNG-MOI", CancellationToken.None);

        Assert.Equal("NOIDUNG-MOI", updated.PaymentContent);
        Assert.True(updated.VietQrOutdated, "QR phải được đánh dấu outdated sau khi cập nhật nội dung");
    }

    private static TuitionService CreateService(ApplicationDbContext db)
    {
        var vietQrService = new VietQrService();
        var paymentSettingService = new PaymentSettingService(db, new FakeAuditLogService());
        return new TuitionService(db, new FakeAuditLogService(), vietQrService, paymentSettingService);
    }

    private sealed class FakeAuditLogService : IAuditLogService
    {
        public Task LogAsync(Guid? userId, string? userEmail, string? userFullName, string action, string? entityType, string? entityId, string? details, string? ipAddress, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task<PagedResult<AuditLogDto>> GetAuditLogsAsync(string? searchTerm, string? action, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new PagedResult<AuditLogDto>(Array.Empty<AuditLogDto>(), 0, pageNumber, pageSize));
        }
    }
}
