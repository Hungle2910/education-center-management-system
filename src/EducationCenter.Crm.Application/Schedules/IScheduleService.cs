using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EducationCenter.Crm.Application.Schedules;

public interface IScheduleService
{
    Task<ScheduleResponse> CreateScheduleAsync(CreateScheduleRequest request, CancellationToken cancellationToken);
    
    Task<IReadOnlyCollection<ScheduleOccurrenceResponse>> GetCalendarAsync(DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken);
    
    Task<ConflictCheckResponse> CheckConflictsAsync(ConflictCheckRequest request, CancellationToken cancellationToken);

    Task CancelOccurrenceAsync(Guid occurrenceId, CancelSessionRequest request, CancellationToken cancellationToken);

    Task RegisterIndividualMakeupAsync(ScheduleIndividualMakeupRequest request, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<EligibleAbsentStudentDto>> GetEligibleAbsentStudentsAsync(Guid occurrenceId, CancellationToken cancellationToken);
}
