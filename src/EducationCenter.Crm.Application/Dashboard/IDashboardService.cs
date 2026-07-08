namespace EducationCenter.Crm.Application.Dashboard;

public interface IDashboardService
{
    Task<AdminOverviewResponse> GetAdminOverviewAsync(
        DashboardFilter filter,
        CancellationToken cancellationToken);

    Task<OperationsDashboardResponse> GetOperationsAsync(
        DashboardFilter filter,
        CancellationToken cancellationToken);
}
