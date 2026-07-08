using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using EducationCenter.Crm.Application.Tuition;
using EducationCenter.Crm.Domain.Classes;
using EducationCenter.Crm.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

using EducationCenter.Crm.Application.Common.Interfaces;

namespace EducationCenter.Crm.Infrastructure.Tuition;

public sealed class TuitionService : ITuitionService
{
    private readonly ApplicationDbContext _db;
    private readonly IAuditLogService _auditLogService;
    private readonly IVietQrService _vietQrService;
    private readonly IPaymentSettingService _paymentSettingService;

    public TuitionService(
        ApplicationDbContext db,
        IAuditLogService auditLogService,
        IVietQrService vietQrService,
        IPaymentSettingService paymentSettingService)
    {
        _db = db;
        _auditLogService = auditLogService;
        _vietQrService = vietQrService;
        _paymentSettingService = paymentSettingService;
    }

    // ---------------------------------------------------------------
    // Preview: calculate fees per student BEFORE generating invoices
    // ---------------------------------------------------------------
    public async Task<List<TuitionPreviewResponse>> PreviewTuitionAsync(GenerateTuitionRequest request)
    {
        var classEntity = await _db.Classes.FindAsync(request.ClassId)
            ?? throw new KeyNotFoundException("Không tìm thấy lớp học.");

        var (yearStr, monthStr) = ParseMonth(request.Month);
        var year = int.Parse(yearStr);
        var month = int.Parse(monthStr);

        var occurrences = await _db.ScheduleOccurrences
            .Where(o => o.ClassId == request.ClassId
                     && o.Date.Year == year
                     && o.Date.Month == month)
            .ToListAsync();

        int total = occurrences.Count;
        int cancelled = occurrences.Count(o => o.Status == "Đã hủy" && o.Reason == "Trừ học phí tháng sau");

        var studentIds = await _db.Attendances
            .Where(a => _db.ScheduleOccurrences
                .Where(o => o.ClassId == request.ClassId)
                .Select(o => o.Id)
                .Contains(a.OccurrenceId))
            .Select(a => a.StudentId)
            .Distinct()
            .ToListAsync();

        var students = await _db.Students
            .Where(s => studentIds.Contains(s.Id))
            .ToListAsync();

        decimal deductPerSession = total > 0 ? classEntity.MonthlyFee / total : 0;
        decimal deduct = Math.Round(deductPerSession * cancelled, 2);

        return students.Select(s => new TuitionPreviewResponse(
            s.Id,
            s.FullName,
            classEntity.MonthlyFee,
            total,
            cancelled,
            deduct,
            classEntity.MonthlyFee - deduct
        )).ToList();
    }

    // ---------------------------------------------------------------
    // Generate invoices for the entire class for the given month
    // ---------------------------------------------------------------
    public async Task<List<TuitionInvoiceResponse>> GenerateTuitionAsync(GenerateTuitionRequest request)
    {
        var classEntity = await _db.Classes.FindAsync(request.ClassId)
            ?? throw new KeyNotFoundException("Không tìm thấy lớp học.");

        var (yearStr, monthStr) = ParseMonth(request.Month);
        var year = int.Parse(yearStr);
        var month = int.Parse(monthStr);

        var occurrences = await _db.ScheduleOccurrences
            .Where(o => o.ClassId == request.ClassId
                     && o.Date.Year == year
                     && o.Date.Month == month)
            .ToListAsync();

        int total = occurrences.Count;
        int cancelled = occurrences.Count(o => o.Status == "Đã hủy" && o.Reason == "Trừ học phí tháng sau");
        decimal deductPerSession = total > 0 ? classEntity.MonthlyFee / total : 0;
        decimal deduct = Math.Round(deductPerSession * cancelled, 2);

        var studentIds = await _db.Attendances
            .Where(a => _db.ScheduleOccurrences
                .Where(o => o.ClassId == request.ClassId)
                .Select(o => o.Id)
                .Contains(a.OccurrenceId))
            .Select(a => a.StudentId)
            .Distinct()
            .ToListAsync();

        var result = new List<TuitionInvoice>();

        foreach (var studentId in studentIds)
        {
            // Avoid duplicate invoices
            var existing = await _db.TuitionInvoices
                .FirstOrDefaultAsync(ti => ti.StudentId == studentId
                                        && ti.ClassId == request.ClassId
                                        && ti.Month == request.Month);
            if (existing is not null) continue;

            var invoice = new TuitionInvoice
            {
                Id = Guid.NewGuid(),
                StudentId = studentId,
                ClassId = request.ClassId,
                Month = request.Month,
                BaseAmount = classEntity.MonthlyFee,
                DiscountAmount = 0,
                DeductAmount = deduct,
                AdjustAmount = 0,
                TotalAmount = classEntity.MonthlyFee - deduct,
                Status = "Chưa thanh toán",
                VietQrOutdated = true,
                CreatedAtUtc = DateTime.UtcNow
            };

            _db.TuitionInvoices.Add(invoice);
            result.Add(invoice);
        }

        await _db.SaveChangesAsync();
        return await MapInvoicesAsync(result);
    }

    // ---------------------------------------------------------------
    // List invoices with optional filters
    // ---------------------------------------------------------------
    public async Task<List<TuitionInvoiceResponse>> GetInvoicesAsync(Guid? classId, string? month)
    {
        var query = _db.TuitionInvoices
            .Include(ti => ti.Student)
            .Include(ti => ti.Class)
            .AsQueryable();

        if (classId.HasValue) query = query.Where(ti => ti.ClassId == classId.Value);
        if (!string.IsNullOrEmpty(month)) query = query.Where(ti => ti.Month == month);

        var invoices = await query.OrderByDescending(ti => ti.CreatedAtUtc).ToListAsync();
        return invoices.Select(MapInvoice).ToList();
    }

    // ---------------------------------------------------------------
    // Get single invoice
    // ---------------------------------------------------------------
    public async Task<TuitionInvoiceResponse> GetInvoiceByIdAsync(Guid id)
    {
        var invoice = await _db.TuitionInvoices
            .Include(ti => ti.Student)
            .Include(ti => ti.Class)
            .FirstOrDefaultAsync(ti => ti.Id == id)
            ?? throw new KeyNotFoundException("Không tìm thấy hoá đơn.");
        return MapInvoice(invoice);
    }

    // ---------------------------------------------------------------
    // Manual adjustment with audit log
    // ---------------------------------------------------------------
    public async Task<TuitionInvoiceResponse> AdjustInvoiceAsync(Guid id, AdjustTuitionRequest request)
    {
        var invoice = await _db.TuitionInvoices
            .Include(ti => ti.Student)
            .Include(ti => ti.Class)
            .FirstOrDefaultAsync(ti => ti.Id == id)
            ?? throw new KeyNotFoundException("Không tìm thấy hoá đơn.");

        AppendAuditEntry(invoice, "Điều chỉnh thủ công", new
        {
            OldAdjust = invoice.AdjustAmount,
            NewAdjust = request.AdjustAmount,
            Reason = request.Reason
        });

        invoice.AdjustAmount = request.AdjustAmount;
        invoice.AdjustReason = request.Reason;
        invoice.TotalAmount = invoice.BaseAmount - invoice.DiscountAmount - invoice.DeductAmount + invoice.AdjustAmount;
        // Mark QR as outdated when total amount changes
        if (invoice.VietQrUrl is not null) invoice.VietQrOutdated = true;
        invoice.UpdatedAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        await _auditLogService.LogAsync(
            null, null, null,
            "Điều chỉnh học phí thủ công",
            "TuitionInvoice",
            invoice.Id.ToString(),
            $"Điều chỉnh số tiền: {request.AdjustAmount}. Lý do: {request.Reason}",
            null);

        return MapInvoice(invoice);
    }

    // ---------------------------------------------------------------
    // Apply a discount code
    // ---------------------------------------------------------------
    public async Task<TuitionInvoiceResponse> ApplyDiscountAsync(Guid id, ApplyDiscountRequest request)
    {
        var invoice = await _db.TuitionInvoices
            .Include(ti => ti.Student)
            .Include(ti => ti.Class)
            .FirstOrDefaultAsync(ti => ti.Id == id)
            ?? throw new KeyNotFoundException("Không tìm thấy hoá đơn.");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var code = await _db.DiscountCodes
            .FirstOrDefaultAsync(dc =>
                dc.Code == request.DiscountCode &&
                dc.IsActive &&
                dc.StartDate <= today &&
                dc.EndDate >= today &&
                (dc.MaxUses == 0 || dc.UsesCount < dc.MaxUses))
            ?? throw new InvalidOperationException("Mã giảm giá không hợp lệ hoặc đã hết lượt sử dụng.");

        decimal discount = code.DiscountType == "Percentage"
            ? Math.Round(invoice.BaseAmount * code.Value / 100, 2)
            : code.Value;

        AppendAuditEntry(invoice, "Áp mã giảm giá", new
        {
            DiscountCode = request.DiscountCode,
            OldDiscount = invoice.DiscountAmount,
            NewDiscount = discount
        });

        invoice.DiscountAmount = discount;
        invoice.TotalAmount = invoice.BaseAmount - invoice.DiscountAmount - invoice.DeductAmount + invoice.AdjustAmount;
        if (invoice.VietQrUrl is not null) invoice.VietQrOutdated = true;
        invoice.UpdatedAtUtc = DateTime.UtcNow;

        code.UsesCount += 1;
        await _db.SaveChangesAsync();

        await _auditLogService.LogAsync(
            null, null, null,
            "Áp mã giảm giá",
            "TuitionInvoice",
            invoice.Id.ToString(),
            $"Áp dụng mã giảm giá {request.DiscountCode}. Số tiền giảm: {discount}.",
            null);

        return MapInvoice(invoice);
    }

    // ---------------------------------------------------------------
    // Generate VietQR URL (using DB payment settings)
    // ---------------------------------------------------------------
    public async Task<TuitionInvoiceResponse> GenerateVietQrAsync(Guid id, GenerateVietQrRequest request, CancellationToken cancellationToken)
    {
        var invoice = await _db.TuitionInvoices
            .Include(ti => ti.Student)
            .Include(ti => ti.Class)
            .FirstOrDefaultAsync(ti => ti.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException("Không tìm thấy hoá đơn.");

        // Lấy cấu hình ngân hàng mặc định từ DB
        var bankSetting = await _paymentSettingService.GetDefaultAsync(cancellationToken)
            ?? throw new InvalidOperationException("Chưa có cấu hình thanh toán mặc định. Vui lòng thiết lập trong Cài đặt.");

        // Sinh nội dung chuyển khoản: dùng override hoặc tự sinh theo quy tắc
        string paymentContent = !string.IsNullOrWhiteSpace(request.OverrideContent)
            ? request.OverrideContent
            : PaymentContentGenerator.Generate(
                invoice.Student?.FullName ?? "",
                invoice.Class?.Name ?? "",
                invoice.Month);

        // Sinh URL QR từ VietQrService
        var amount = Math.Round(invoice.TotalAmount, 0);
        var qrUrl = _vietQrService.GenerateQuickLink(
            bankSetting.BankId,
            bankSetting.AccountNo,
            bankSetting.AccountName,
            amount,
            paymentContent,
            bankSetting.VietQrTemplate);

        AppendAuditEntry(invoice, "Tạo VietQR", new
        {
            Amount = amount,
            Content = paymentContent,
            QrUrl = qrUrl,
            BankName = bankSetting.BankName
        });

        invoice.VietQrUrl = qrUrl;
        invoice.PaymentContent = paymentContent;
        invoice.VietQrOutdated = false;
        invoice.VietQrGeneratedAt = DateTime.UtcNow;
        invoice.UpdatedAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);

        await _auditLogService.LogAsync(
            null, null, null,
            "Tạo VietQR",
            "TuitionInvoice",
            invoice.Id.ToString(),
            $"Tạo mã QR thanh toán {bankSetting.BankName}. Số tiền: {amount:N0}. Nội dung: {paymentContent}.",
            null);

        return MapInvoice(invoice);
    }

    // ---------------------------------------------------------------
    // Update payment content manually (Admin override)
    // ---------------------------------------------------------------
    public async Task<TuitionInvoiceResponse> UpdatePaymentContentAsync(Guid id, string paymentContent, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(paymentContent))
            throw new ArgumentException("Nội dung chuyển khoản không được để trống.");

        var invoice = await _db.TuitionInvoices
            .Include(ti => ti.Student)
            .Include(ti => ti.Class)
            .FirstOrDefaultAsync(ti => ti.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException("Không tìm thấy hoá đơn.");

        var oldContent = invoice.PaymentContent;

        AppendAuditEntry(invoice, "Cập nhật nội dung CK", new
        {
            OldContent = oldContent,
            NewContent = paymentContent
        });

        invoice.PaymentContent = paymentContent;
        // Đánh dấu QR cần tạo lại vì nội dung đã thay đổi
        if (invoice.VietQrUrl is not null)
            invoice.VietQrOutdated = true;
        invoice.UpdatedAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);

        await _auditLogService.LogAsync(
            null, null, null,
            "Cập nhật nội dung chuyển khoản",
            "TuitionInvoice",
            invoice.Id.ToString(),
            $"Nội dung cũ: {oldContent}. Nội dung mới: {paymentContent}. QR cần tạo lại.",
            null);

        return MapInvoice(invoice);
    }

    // ---------------------------------------------------------------
    // Parent reports payment (submit proof)
    // ---------------------------------------------------------------
    public async Task<TuitionInvoiceResponse> SubmitPaymentProofAsync(Guid id, SubmitPaymentProofRequest request)
    {
        var invoice = await _db.TuitionInvoices
            .Include(ti => ti.Student)
            .Include(ti => ti.Class)
            .FirstOrDefaultAsync(ti => ti.Id == id)
            ?? throw new KeyNotFoundException("Không tìm thấy hoá đơn.");

        if (invoice.Status == "Đã thanh toán")
            throw new InvalidOperationException("Hoá đơn này đã được xác nhận thanh toán.");

        AppendAuditEntry(invoice, "Báo đã thanh toán", new
        {
            ProofUrl = request.PaymentProofUrl,
            OldStatus = invoice.Status
        });

        invoice.PaymentProofUrl = request.PaymentProofUrl;
        invoice.Status = "Chờ xác nhận";
        invoice.UpdatedAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        await _auditLogService.LogAsync(
            null, null, null,
            "Báo đã thanh toán",
            "TuitionInvoice",
            invoice.Id.ToString(),
            $"Phụ huynh báo đã thanh toán học phí. Trạng thái cũ: {invoice.Status}.",
            null);

        return MapInvoice(invoice);
    }

    // ---------------------------------------------------------------
    // Admin confirms payment — handles under/over payment
    // ---------------------------------------------------------------
    public async Task<TuitionInvoiceResponse> ConfirmPaymentAsync(Guid id, ConfirmPaymentRequest request)
    {
        var invoice = await _db.TuitionInvoices
            .Include(ti => ti.Student)
            .Include(ti => ti.Class)
            .FirstOrDefaultAsync(ti => ti.Id == id)
            ?? throw new KeyNotFoundException("Không tìm thấy hoá đơn.");

        var newStatus = request.PaidAmount switch
        {
            var paid when paid >= invoice.TotalAmount * 0.9999m && paid <= invoice.TotalAmount * 1.0001m
                => "Đã thanh toán",
            var paid when paid < invoice.TotalAmount
                => "Thanh toán thiếu",
            _   => "Thanh toán dư"
        };

        AppendAuditEntry(invoice, "Xác nhận thanh toán", new
        {
            PaidAmount = request.PaidAmount,
            RequiredAmount = invoice.TotalAmount,
            OldStatus = invoice.Status,
            NewStatus = newStatus,
            Note = request.Note
        });

        invoice.PaidAmount = request.PaidAmount;
        invoice.PaymentNote = request.Note;
        invoice.Status = newStatus;
        invoice.UpdatedAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        await _auditLogService.LogAsync(
            null, null, null,
            "Xác nhận thanh toán",
            "TuitionInvoice",
            invoice.Id.ToString(),
            $"Xác nhận số tiền đã nhận: {request.PaidAmount}. Trạng thái mới: {newStatus}. Ghi chú: {request.Note}",
            null);

        return MapInvoice(invoice);
    }

    // ---------------------------------------------------------------
    // Helpers
    // ---------------------------------------------------------------
    private static (string year, string month) ParseMonth(string month)
    {
        var parts = month.Split('-');
        if (parts.Length != 2) throw new ArgumentException("Month phải có định dạng YYYY-MM.");
        return (parts[0], parts[1]);
    }

    private static void AppendAuditEntry(TuitionInvoice invoice, string action, object data)
    {
        var history = string.IsNullOrEmpty(invoice.OperationHistory)
            ? new List<object>()
            : JsonSerializer.Deserialize<List<object>>(invoice.OperationHistory) ?? new List<object>();

        history.Add(new { At = DateTime.UtcNow, Action = action, Data = data });
        invoice.OperationHistory = JsonSerializer.Serialize(history);
    }

    private static string RemoveVietnameseDiacritics(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;

        // Normalize to decomposed form, then strip combining characters
        var normalized = text.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (var c in normalized)
        {
            var cat = CharUnicodeInfo.GetUnicodeCategory(c);
            if (cat != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }

        return sb.ToString()
            .Normalize(NormalizationForm.FormC)
            .Replace('đ', 'd').Replace('Đ', 'D')
            .ToUpperInvariant();
    }

    private static TuitionInvoiceResponse MapInvoice(TuitionInvoice ti) => new(
        ti.Id,
        ti.StudentId,
        ti.Student?.FullName ?? "",
        ti.ClassId,
        ti.Class?.Name ?? "",
        ti.Month,
        ti.BaseAmount,
        ti.DiscountAmount,
        ti.DeductAmount,
        ti.AdjustAmount,
        ti.TotalAmount,
        ti.AdjustReason,
        ti.Status,
        ti.PaymentProofUrl,
        ti.VietQrUrl,
        ti.VietQrOutdated,
        ti.VietQrGeneratedAt,
        ti.PaidAmount,
        ti.PaymentNote,
        ti.OperationHistory,
        ti.CreatedAtUtc,
        ti.PaymentContent
    );

    private async Task<List<TuitionInvoiceResponse>> MapInvoicesAsync(List<TuitionInvoice> invoices)
    {
        var ids = invoices.Select(i => i.Id).ToList();
        var loaded = await _db.TuitionInvoices
            .Include(ti => ti.Student)
            .Include(ti => ti.Class)
            .Where(ti => ids.Contains(ti.Id))
            .ToListAsync();
        return loaded.Select(MapInvoice).ToList();
    }
}

