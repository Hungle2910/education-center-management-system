namespace EducationCenter.Crm.Application.CoreData;

public interface IStudentService
{
    Task<IReadOnlyCollection<StudentResponse>> GetAllAsync(CancellationToken cancellationToken);

    Task<StudentResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<StudentResponse> CreateAsync(StudentRequest request, CancellationToken cancellationToken);

    Task<StudentResponse?> UpdateAsync(Guid id, StudentRequest request, CancellationToken cancellationToken);

    Task<(bool Success, string? Error)> DeleteAsync(Guid id, CancellationToken cancellationToken);
}

