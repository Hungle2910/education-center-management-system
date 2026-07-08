namespace EducationCenter.Crm.Application.CoreData;

public sealed record ParentRequest(
    string FullName,
    string? Email,
    string PhoneNumber,
    IReadOnlyCollection<StudentLinkRequest>? Students);

public sealed record ParentResponse(
    Guid Id,
    string FullName,
    string? Email,
    string PhoneNumber,
    string ZaloLink,
    IReadOnlyCollection<StudentSummaryResponse> Students);
