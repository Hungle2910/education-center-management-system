using System;

namespace EducationCenter.Crm.Domain.Settings;

public sealed class DiscountCode
{
    public Guid Id { get; set; }

    public required string Code { get; set; } // e.g. "HE2026", "GIAM20"

    public required string DiscountType { get; set; } // Percentage, FixedAmount

    public decimal Value { get; set; } // % or fixed cash amount

    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }

    public int MaxUses { get; set; }
    public int UsesCount { get; set; }
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
