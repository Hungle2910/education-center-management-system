namespace EducationCenter.Crm.Application.CoreData;

public interface ITeacherService
{
    Task<IReadOnlyCollection<TeacherResponse>> GetAllAsync(CancellationToken cancellationToken);

    Task<TeacherResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<TeacherResponse> CreateAsync(TeacherRequest request, CancellationToken cancellationToken);

    Task<TeacherResponse?> UpdateAsync(Guid id, TeacherRequest request, CancellationToken cancellationToken);

    /// <summary>Trả về false nếu giáo viên đang được gán lớp học.</summary>
    Task<(bool Success, string? Error)> DeleteAsync(Guid id, CancellationToken cancellationToken);
}

