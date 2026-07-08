using System;
using System.Collections.Generic;

namespace EducationCenter.Crm.Application.Tuition;

// --- Request DTOs ---

public sealed record GenerateTuitionRequest(
    Guid ClassId,
    string Month // "YYYY-MM"
);

public sealed record TuitionPreviewResponse(
    Guid StudentId,
    string StudentName,
    decimal BaseAmount,
    int TotalSessions,
    int CancelledSessions,
    decimal DeductAmount,
    decimal EstimatedTotal
);

public sealed record AdjustTuitionRequest(
    decimal AdjustAmount,
    string Reason
);

public sealed record ApplyDiscountRequest(
    string DiscountCode
);

/// <summary>Override bank/account from system settings (optional).</summary>
public sealed record GenerateVietQrRequest(
    string? OverrideContent = null // null = use auto-generated content
);

/// <summary>Admin manually sets payment transfer content for an invoice.</summary>
public sealed record UpdatePaymentContentRequest(
    string PaymentContent
);

public sealed record SubmitPaymentProofRequest(
    string PaymentProofUrl
);

public sealed record ConfirmPaymentRequest(
    decimal PaidAmount,
    string? Note
);

// --- Response DTOs ---

public sealed record TuitionInvoiceResponse(
    Guid Id,
    Guid StudentId,
    string StudentName,
    Guid ClassId,
    string ClassName,
    string Month,
    decimal BaseAmount,
    decimal DiscountAmount,
    decimal DeductAmount,
    decimal AdjustAmount,
    decimal TotalAmount,
    string? AdjustReason,
    string Status,
    string? PaymentProofUrl,
    string? VietQrUrl,
    bool VietQrOutdated,
    DateTime? VietQrGeneratedAt,
    decimal? PaidAmount,
    string? PaymentNote,
    string? OperationHistory,
    DateTime CreatedAtUtc,
    string? PaymentContent
);

