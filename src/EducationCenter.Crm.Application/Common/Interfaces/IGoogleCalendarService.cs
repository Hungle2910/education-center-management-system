using System;
using System.Threading;
using System.Threading.Tasks;

namespace EducationCenter.Crm.Application.Common.Interfaces;

public interface IGoogleCalendarService
{
    Task<string?> CreateEventAsync(
        string summary,
        string location,
        string description,
        DateTime startDateTime,
        DateTime endDateTime,
        CancellationToken cancellationToken);

    Task UpdateEventAsync(
        string eventId,
        string summary,
        string location,
        string description,
        DateTime startDateTime,
        DateTime endDateTime,
        CancellationToken cancellationToken);

    Task DeleteEventAsync(
        string eventId,
        CancellationToken cancellationToken);
}
