using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EducationCenter.Crm.Application.Common.Interfaces;

public record AuditLogDto(
    Guid Id,
    Guid? UserId,
    string? UserEmail,
    string? UserFullName,
    string Action,
    string? EntityType,
    string? EntityId,
    string? Details,
    string? IpAddress,
    DateTime TimestampUtc
);

public record PagedResult<T>(
    IReadOnlyCollection<T> Items,
    int TotalCount,
    int PageNumber,
    int PageSize
);

public interface IAuditLogService
{
    Task LogAsync(
        Guid? userId,
        string? userEmail,
        string? userFullName,
        string action,
        string? entityType,
        string? entityId,
        string? details,
        string? ipAddress,
        CancellationToken cancellationToken = default);

    Task<PagedResult<AuditLogDto>> GetAuditLogsAsync(
        string? searchTerm,
        string? action,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);
}
