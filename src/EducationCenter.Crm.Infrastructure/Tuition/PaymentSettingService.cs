using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using EducationCenter.Crm.Application.Common.Interfaces;
using EducationCenter.Crm.Application.Tuition;
using EducationCenter.Crm.Domain.Settings;
using EducationCenter.Crm.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EducationCenter.Crm.Infrastructure.Tuition;

public sealed class PaymentSettingService : IPaymentSettingService
{
    private readonly ApplicationDbContext _db;
    private readonly IAuditLogService _auditLogService;

    public PaymentSettingService(ApplicationDbContext db, IAuditLogService auditLogService)
    {
        _db = db;
        _auditLogService = auditLogService;
    }

    public async Task<IReadOnlyCollection<PaymentSettingResponse>> GetAllAsync(CancellationToken cancellationToken)
    {
        var settings = await _db.PaymentSettings
            .AsNoTracking()
            .OrderByDescending(x => x.IsDefault)
            .ThenByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        return settings.Select(ToResponse).ToArray();
    }

    public async Task<PaymentSettingResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var setting = await _db.PaymentSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return setting == null ? null : ToResponse(setting);
    }

    public async Task<PaymentSettingResponse?> GetDefaultAsync(CancellationToken cancellationToken)
    {
        var setting = await _db.PaymentSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.IsDefault && x.IsActive, cancellationToken);

        // Fallback sang cấu hình bất kỳ đầu tiên nếu không có mặc định
        setting ??= await _db.PaymentSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.IsActive, cancellationToken);

        return setting == null ? null : ToResponse(setting);
    }

    public async Task<PaymentSettingResponse> CreateAsync(PaymentSettingRequest request, Guid? userId, CancellationToken cancellationToken)
    {
        ValidateRequest(request);

        var setting = new PaymentSetting
        {
            Id = Guid.NewGuid(),
            BankId = request.BankId.Trim(),
            BankName = request.BankName.Trim(),
            AccountNo = request.AccountNo.Trim(),
            AccountName = request.AccountName.Trim().ToUpperInvariant(),
            VietQrTemplate = string.IsNullOrWhiteSpace(request.VietQrTemplate) ? "compact2" : request.VietQrTemplate.Trim(),
            IsDefault = request.IsDefault,
            IsActive = request.IsActive,
            CreatedAtUtc = DateTime.UtcNow,
            CreatedByUserId = userId
        };

        if (setting.IsDefault)
        {
            await ClearOtherDefaultsAsync(Guid.Empty, cancellationToken);
        }

        _db.PaymentSettings.Add(setting);
        await _db.SaveChangesAsync(cancellationToken);

        await _auditLogService.LogAsync(
            userId, null, null,
            "Thêm cấu hình thanh toán",
            "PaymentSetting",
            setting.Id.ToString(),
            $"Thêm tài khoản {setting.BankName} - {setting.AccountNo}. Mặc định: {setting.IsDefault}.",
            null, cancellationToken);

        return ToResponse(setting);
    }

    public async Task<PaymentSettingResponse?> UpdateAsync(Guid id, PaymentSettingRequest request, Guid? userId, CancellationToken cancellationToken)
    {
        ValidateRequest(request);

        var setting = await _db.PaymentSettings.FindAsync(new object[] { id }, cancellationToken);
        if (setting == null) return null;

        var oldBank = setting.BankName;
        var oldNo = setting.AccountNo;

        setting.BankId = request.BankId.Trim();
        setting.BankName = request.BankName.Trim();
        setting.AccountNo = request.AccountNo.Trim();
        setting.AccountName = request.AccountName.Trim().ToUpperInvariant();
        setting.VietQrTemplate = string.IsNullOrWhiteSpace(request.VietQrTemplate) ? "compact2" : request.VietQrTemplate.Trim();
        setting.IsDefault = request.IsDefault;
        setting.IsActive = request.IsActive;
        setting.UpdatedAtUtc = DateTime.UtcNow;
        setting.UpdatedByUserId = userId;

        if (setting.IsDefault)
        {
            await ClearOtherDefaultsAsync(id, cancellationToken);
        }

        await _db.SaveChangesAsync(cancellationToken);

        await _auditLogService.LogAsync(
            userId, null, null,
            "Sửa cấu hình thanh toán",
            "PaymentSetting",
            setting.Id.ToString(),
            $"Cập nhật tài khoản từ {oldBank} {oldNo} thành {setting.BankName} {setting.AccountNo}.",
            null, cancellationToken);

        return ToResponse(setting);
    }

    public async Task<bool> SetDefaultAsync(Guid id, Guid? userId, CancellationToken cancellationToken)
    {
        var setting = await _db.PaymentSettings.FindAsync(new object[] { id }, cancellationToken);
        if (setting == null) return false;

        if (!setting.IsActive)
            throw new InvalidOperationException("Không thể đặt cấu hình đang khóa làm mặc định.");

        setting.IsDefault = true;
        setting.UpdatedAtUtc = DateTime.UtcNow;
        setting.UpdatedByUserId = userId;

        await ClearOtherDefaultsAsync(id, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);

        await _auditLogService.LogAsync(
            userId, null, null,
            "Đặt cấu hình thanh toán mặc định",
            "PaymentSetting",
            setting.Id.ToString(),
            $"Đặt tài khoản {setting.BankName} - {setting.AccountNo} làm mặc định.",
            null, cancellationToken);

        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid? userId, CancellationToken cancellationToken)
    {
        var setting = await _db.PaymentSettings.FindAsync(new object[] { id }, cancellationToken);
        if (setting == null) return false;

        var bankName = setting.BankName;
        var accountNo = setting.AccountNo;

        _db.PaymentSettings.Remove(setting);
        await _db.SaveChangesAsync(cancellationToken);

        await _auditLogService.LogAsync(
            userId, null, null,
            "Xóa cấu hình thanh toán",
            "PaymentSetting",
            id.ToString(),
            $"Xóa tài khoản {bankName} - {accountNo}.",
            null, cancellationToken);

        return true;
    }

    private void ValidateRequest(PaymentSettingRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.BankId))
            throw new ArgumentException("Mã ngân hàng bắt buộc.");

        if (string.IsNullOrWhiteSpace(request.BankName))
            throw new ArgumentException("Tên ngân hàng bắt buộc.");

        if (string.IsNullOrWhiteSpace(request.AccountNo))
            throw new ArgumentException("Vui lòng nhập số tài khoản.");

        if (string.IsNullOrWhiteSpace(request.AccountName))
            throw new ArgumentException("Vui lòng nhập tên tài khoản.");

        if (!Regex.IsMatch(request.AccountNo.Trim(), @"^\d+$"))
            throw new ArgumentException("Số tài khoản chỉ được chứa chữ số.");
    }

    private async Task ClearOtherDefaultsAsync(Guid excludeId, CancellationToken cancellationToken)
    {
        var defaults = await _db.PaymentSettings
            .Where(x => x.IsDefault && x.Id != excludeId)
            .ToListAsync(cancellationToken);

        foreach (var item in defaults)
        {
            item.IsDefault = false;
            item.UpdatedAtUtc = DateTime.UtcNow;
        }
    }

    private static PaymentSettingResponse ToResponse(PaymentSetting x) => new(
        x.Id,
        x.BankId,
        x.BankName,
        x.AccountNo,
        x.AccountName,
        x.VietQrTemplate,
        x.IsDefault,
        x.IsActive,
        x.CreatedAtUtc,
        x.UpdatedAtUtc
    );
}
