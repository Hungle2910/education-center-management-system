namespace EducationCenter.Crm.Application.CoreData;

public sealed record TeacherRequest(
    string FullName,
    string? Email,
    string? PhoneNumber,
    string? Subject,
    bool IsActive);

public sealed record TeacherResponse(
    Guid Id,
    string FullName,
    string? Email,
    string? PhoneNumber,
    string? Subject,
    bool IsActive);
