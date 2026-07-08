namespace EducationCenter.Crm.Application.CoreData;

public interface IParentService
{
    Task<IReadOnlyCollection<ParentResponse>> GetAllAsync(CancellationToken cancellationToken);

    Task<ParentResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<ParentResponse> CreateAsync(ParentRequest request, CancellationToken cancellationToken);

    Task<ParentResponse?> UpdateAsync(Guid id, ParentRequest request, CancellationToken cancellationToken);

    Task<(bool Success, string? Error)> DeleteAsync(Guid id, CancellationToken cancellationToken);
}

