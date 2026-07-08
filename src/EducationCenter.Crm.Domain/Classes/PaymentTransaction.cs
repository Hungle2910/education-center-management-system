using System;

namespace EducationCenter.Crm.Domain.Classes;

public sealed class PaymentTransaction
{
    public Guid Id { get; set; }

    public Guid TuitionInvoiceId { get; set; }
    public TuitionInvoice? TuitionInvoice { get; set; }

    public decimal Amount { get; set; }
    public string? TransactionCode { get; set; } // e.g. bank transfer reference code
    public string? PaymentMethod { get; set; } // VietQR, Cash, BankTransfer
    public string? PayeeName { get; set; } // Staff who received/approved
    public string? Note { get; set; }

    public DateTime TransactionTimeUtc { get; set; } = DateTime.UtcNow;
}
