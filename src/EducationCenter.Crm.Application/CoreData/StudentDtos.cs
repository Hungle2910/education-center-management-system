namespace EducationCenter.Crm.Application.CoreData;

public sealed record StudentRequest(
    string FullName,
    string? StudentCode,
    string? Email,
    string? PhoneNumber,
    DateOnly? DateOfBirth,
    string? Status,
    IReadOnlyCollection<ParentLinkRequest>? Parents);

public sealed record StudentResponse(
    Guid Id,
    string FullName,
    string? StudentCode,
    string? Email,
    string? PhoneNumber,
    DateOnly? DateOfBirth,
    string Status,
    IReadOnlyCollection<ParentSummaryResponse> Parents);

public sealed record ParentLinkRequest(Guid ParentId, string? Relationship);

public sealed record ParentSummaryResponse(
    Guid Id,
    string FullName,
    string? Email,
    string PhoneNumber,
    string ZaloLink,
    string? Relationship);

public sealed record StudentLinkRequest(Guid StudentId, string? Relationship);

public sealed record StudentSummaryResponse(
    Guid Id,
    string FullName,
    string? StudentCode,
    string? PhoneNumber,
    string? Relationship);
