using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using EducationCenter.Crm.Application.Common.Interfaces;
using EducationCenter.Crm.Domain.People;
using EducationCenter.Crm.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace EducationCenter.Crm.Infrastructure.Logging;

public sealed class AuditLogService : IAuditLogService
{
    private readonly ApplicationDbContext _db;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditLogService(ApplicationDbContext db, IHttpContextAccessor httpContextAccessor)
    {
        _db = db;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task LogAsync(
        Guid? userId,
        string? userEmail,
        string? userFullName,
        string action,
        string? entityType,
        string? entityId,
        string? details,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        
        // Auto resolve user from HTTP context if not explicitly provided
        if (userId is null && httpContext?.User?.Identity?.IsAuthenticated == true)
        {
            var idClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(idClaim, out var parsedId))
            {
                userId = parsedId;
            }
            userEmail ??= httpContext.User.FindFirst(ClaimTypes.Email)?.Value;
            userFullName ??= httpContext.User.FindFirst(ClaimTypes.Name)?.Value;
        }

        // Auto resolve IP if not provided
        if (string.IsNullOrEmpty(ipAddress) && httpContext?.Connection != null)
        {
            ipAddress = httpContext.Connection.RemoteIpAddress?.ToString();
        }

        var log = new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            UserEmail = userEmail,
            UserFullName = userFullName,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            Details = details,
            IpAddress = ipAddress,
            TimestampUtc = DateTime.UtcNow
        };

        _db.AuditLogs.Add(log);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<PagedResult<AuditLogDto>> GetAuditLogsAsync(
        string? searchTerm,
        string? action,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _db.AuditLogs.AsNoTracking();

        if (!string.IsNullOrEmpty(action))
        {
            query = query.Where(x => x.Action == action);
        }

        if (!string.IsNullOrEmpty(searchTerm))
        {
            searchTerm = searchTerm.ToLower();
            query = query.Where(x =>
                (x.UserEmail != null && x.UserEmail.ToLower().Contains(searchTerm)) ||
                (x.UserFullName != null && x.UserFullName.ToLower().Contains(searchTerm)) ||
                x.Action.ToLower().Contains(searchTerm) ||
                (x.EntityType != null && x.EntityType.ToLower().Contains(searchTerm)) ||
                (x.Details != null && x.Details.ToLower().Contains(searchTerm))
            );
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.TimestampUtc)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new AuditLogDto(
                x.Id,
                x.UserId,
                x.UserEmail,
                x.UserFullName,
                x.Action,
                x.EntityType,
                x.EntityId,
                x.Details,
                x.IpAddress,
                x.TimestampUtc
            ))
            .ToListAsync(cancellationToken);

        return new PagedResult<AuditLogDto>(items, totalCount, pageNumber, pageSize);
    }
}
