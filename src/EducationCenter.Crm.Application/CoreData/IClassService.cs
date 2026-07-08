namespace EducationCenter.Crm.Application.CoreData;

public interface IClassService
{
    Task<IReadOnlyCollection<ClassResponse>> GetAllAsync(CancellationToken cancellationToken);

    Task<ClassResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<ClassResponse> CreateAsync(ClassRequest request, CancellationToken cancellationToken);

    Task<ClassResponse?> UpdateAsync(Guid id, ClassRequest request, CancellationToken cancellationToken);

    Task<(bool Success, string? Error)> DeleteAsync(Guid id, CancellationToken cancellationToken);
}

