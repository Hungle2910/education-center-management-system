using EducationCenter.Crm.Api.Contracts;
using EducationCenter.Crm.Application.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EducationCenter.Crm.Api.Controllers;

[ApiController]
[Authorize(Roles = DashboardRoles)]
[Route("api/dashboard")]
public sealed class DashboardController : ControllerBase
{
    private const string DashboardRoles = "Admin,Staff";

    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("admin/overview")]
    [ProducesResponseType(typeof(ApiResponse<AdminOverviewResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AdminOverview(
        [FromQuery] int? month,
        [FromQuery] int? year,
        [FromQuery] Guid? classId,
        CancellationToken cancellationToken)
    {
        var response = await _dashboardService.GetAdminOverviewAsync(
            new DashboardFilter(month, year, classId),
            cancellationToken);

        return Ok(ApiResponse<AdminOverviewResponse>.Ok(response));
    }

    [HttpGet("admin/operations")]
    [ProducesResponseType(typeof(ApiResponse<OperationsDashboardResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AdminOperations(
        [FromQuery] int? month,
        [FromQuery] int? year,
        [FromQuery] Guid? classId,
        CancellationToken cancellationToken)
    {
        var response = await _dashboardService.GetOperationsAsync(
            new DashboardFilter(month, year, classId),
            cancellationToken);

        return Ok(ApiResponse<OperationsDashboardResponse>.Ok(response));
    }

    [HttpGet("staff")]
    [ProducesResponseType(typeof(ApiResponse<OperationsDashboardResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public Task<IActionResult> Staff(
        [FromQuery] int? month,
        [FromQuery] int? year,
        [FromQuery] Guid? classId,
        CancellationToken cancellationToken)
    {
        return AdminOperations(month, year, classId, cancellationToken);
    }
}
