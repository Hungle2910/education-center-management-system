namespace EducationCenter.Crm.Application.CoreData;

public sealed record ClassRequest(
    string Name,
    string? Subject,
    Guid? TeacherId,
    string? Status,
    int MinStudents,
    int MaxStudents);

public sealed record ClassResponse(
    Guid Id,
    string Name,
    string? Subject,
    Guid? TeacherId,
    string? TeacherName,
    string Status,
    int MinStudents,
    int MaxStudents);
