using System;
using EducationCenter.Crm.Application.Tuition;

namespace EducationCenter.Crm.Infrastructure.Tuition;

public sealed class VietQrService : IVietQrService
{
    public string GenerateQuickLink(
        string bankId, 
        string accountNo, 
        string accountName, 
        decimal amount, 
        string paymentContent, 
        string template = "compact2")
    {
        if (string.IsNullOrWhiteSpace(bankId))
            throw new ArgumentException("Mã ngân hàng không được để trống.", nameof(bankId));

        if (string.IsNullOrWhiteSpace(accountNo))
            throw new ArgumentException("Vui lòng nhập số tài khoản.", nameof(accountNo));

        if (string.IsNullOrWhiteSpace(accountName))
            throw new ArgumentException("Vui lòng nhập tên tài khoản.", nameof(accountName));

        if (amount <= 0)
            throw new ArgumentException("Số tiền không hợp lệ.");

        var cleanTemplate = string.IsNullOrWhiteSpace(template) ? "compact2" : template.Trim();

        var encodedContent = Uri.EscapeDataString(paymentContent ?? string.Empty);
        var encodedName = Uri.EscapeDataString(accountName.Trim());
        var cleanAmount = (long)Math.Round(amount);

        return $"https://img.vietqr.io/image/{bankId.Trim()}-{accountNo.Trim()}-{cleanTemplate}.png?amount={cleanAmount}&addInfo={encodedContent}&accountName={encodedName}";
    }
}
