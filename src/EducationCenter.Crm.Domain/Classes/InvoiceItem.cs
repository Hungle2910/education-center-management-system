using System;

namespace EducationCenter.Crm.Domain.Classes;

public sealed class InvoiceItem
{
    public Guid Id { get; set; }
    
    public Guid TuitionInvoiceId { get; set; }
    public TuitionInvoice? TuitionInvoice { get; set; }

    public required string Name { get; set; } // e.g. "Học phí", "Giáo trình", "Đồng phục"
    public decimal Amount { get; set; }
    public int Quantity { get; set; } = 1;
    public decimal SubTotal { get; set; }

    // Audit fields
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }
}
