using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EducationCenter.Crm.Application.Reports;

public interface IReportService
{
    Task<TuitionReportResponse> GetTuitionReportAsync(CancellationToken cancellationToken);
    
    Task<IReadOnlyCollection<ClassReportItem>> GetClassReportAsync(CancellationToken cancellationToken);
    
    Task<IReadOnlyCollection<TeacherReportItem>> GetTeacherReportAsync(CancellationToken cancellationToken);
}
