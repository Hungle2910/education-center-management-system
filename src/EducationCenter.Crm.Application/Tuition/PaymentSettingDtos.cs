using System;

namespace EducationCenter.Crm.Application.Tuition;

public sealed record PaymentSettingRequest(
    string BankId,
    string BankName,
    string AccountNo,
    string AccountName,
    string VietQrTemplate,
    bool IsDefault,
    bool IsActive
);

public sealed record PaymentSettingResponse(
    Guid Id,
    string BankId,
    string BankName,
    string AccountNo,
    string AccountName,
    string VietQrTemplate,
    bool IsDefault,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
