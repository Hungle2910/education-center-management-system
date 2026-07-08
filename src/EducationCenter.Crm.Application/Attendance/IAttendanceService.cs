using System;
using System.Threading;
using System.Threading.Tasks;

namespace EducationCenter.Crm.Application.Attendance;

public interface IAttendanceService
{
    Task<OccurrenceAttendanceDto?> GetAttendanceByOccurrenceAsync(Guid occurrenceId, CancellationToken cancellationToken);
    
    Task SubmitAttendanceAsync(SubmitAttendanceRequest request, string auditor, CancellationToken cancellationToken);
}
