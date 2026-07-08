namespace EducationCenter.Crm.Application.Auth;

public sealed record AuthUserResponse(
    Guid Id,
    string Email,
    string FullName,
    IReadOnlyCollection<string> Roles,
    IReadOnlyCollection<string> Permissions);
