using System;

namespace EducationCenter.Crm.Domain.Settings;

public sealed class PaymentSetting
{
    public Guid Id { get; set; }

    public required string BankId { get; set; }

    public required string BankName { get; set; }

    public required string AccountNo { get; set; }

    public required string AccountName { get; set; }

    public string VietQrTemplate { get; set; } = "compact2";

    public bool IsDefault { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAtUtc { get; set; }

    public Guid? CreatedByUserId { get; set; }

    public Guid? UpdatedByUserId { get; set; }

    // Compatibility properties for spec requests
    public DateTime CreatedAt => CreatedAtUtc;
    public DateTime? UpdatedAt => UpdatedAtUtc;
}
