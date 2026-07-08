using System;
using EducationCenter.Crm.Domain.People;

namespace EducationCenter.Crm.Domain.Classes;

public sealed class TuitionInvoice
{
    public Guid Id { get; set; }

    public Guid StudentId { get; set; }
    public Student? Student { get; set; }

    public Guid ClassId { get; set; }
    public Class? Class { get; set; }

    public required string Month { get; set; } // e.g. "2026-07"

    public decimal BaseAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal DeductAmount { get; set; }
    public decimal AdjustAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string? AdjustReason { get; set; }

    public string Status { get; set; } = "Chưa thanh toán"; // Chưa thanh toán, Chờ xác nhận, Đã thanh toán, Thanh toán thiếu, Thanh toán dư, Quá hạn, Đã hủy, Đã hoàn tiền
    public string? PaymentProofUrl { get; set; }
    public string? VietQrUrl { get; set; }
    public bool VietQrOutdated { get; set; } = true; // true until QR is generated; becomes true again if amount/content changes
    public DateTime? VietQrGeneratedAt { get; set; }

    public string? PaymentContent { get; set; }

    // Spec compatibility properties
    public string? VietQrImageUrl 
    { 
        get => VietQrUrl; 
        set => VietQrUrl = value; 
    }
    public bool IsVietQrOutdated 
    { 
        get => VietQrOutdated; 
        set => VietQrOutdated = value; 
    }
    public decimal FinalAmount 
    { 
        get => TotalAmount; 
        set => TotalAmount = value; 
    }
    public string PaymentStatus 
    { 
        get => Status; 
        set => Status = value; 
    }

    // Actual amount received when admin confirms payment (supports under/over payment)
    public decimal? PaidAmount { get; set; }
    public string? PaymentNote { get; set; }

    public string? OperationHistory { get; set; } // JSON array of audit entries

    // Navigation properties
    public ICollection<InvoiceItem> Items { get; set; } = new List<InvoiceItem>();
    public ICollection<PaymentTransaction> Transactions { get; set; } = new List<PaymentTransaction>();

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }
}
