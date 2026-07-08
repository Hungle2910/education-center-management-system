using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EducationCenter.Crm.Application.Admissions;

public interface IAdmissionsService
{
    Task<IReadOnlyCollection<LeadDto>> GetLeadsAsync(CancellationToken cancellationToken);
    Task<LeadDto> CreateLeadAsync(CreateLeadRequest request, CancellationToken cancellationToken);
    Task<LeadDto> UpdateLeadAsync(Guid id, UpdateLeadRequest request, CancellationToken cancellationToken);
    Task ConvertLeadToStudentAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<TrialSessionDto>> GetTrialSessionsAsync(CancellationToken cancellationToken);
    Task<TrialSessionDto> ScheduleTrialSessionAsync(ScheduleTrialRequest request, CancellationToken cancellationToken);
    Task EvaluateTrialSessionAsync(Guid id, EvaluateTrialRequest request, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<ParentCareLogDto>> GetParentCareLogsAsync(Guid? parentId, Guid? leadId, CancellationToken cancellationToken);
    Task<ParentCareLogDto> CreateCareLogAsync(CreateCareLogRequest request, CancellationToken cancellationToken);
}
