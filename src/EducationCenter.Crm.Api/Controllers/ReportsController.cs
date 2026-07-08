using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EducationCenter.Crm.Api.Contracts;
using EducationCenter.Crm.Application.Reports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EducationCenter.Crm.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/reports")]
public sealed class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("tuition")]
    [ProducesResponseType(typeof(ApiResponse<TuitionReportResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetTuitionReport(CancellationToken cancellationToken)
    {
        var response = await _reportService.GetTuitionReportAsync(cancellationToken);
        return Ok(ApiResponse<TuitionReportResponse>.Ok(response));
    }

    [HttpGet("classes")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<ClassReportItem>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetClassReport(CancellationToken cancellationToken)
    {
        var response = await _reportService.GetClassReportAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyCollection<ClassReportItem>>.Ok(response));
    }

    [HttpGet("teachers")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<TeacherReportItem>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetTeacherReport(CancellationToken cancellationToken)
    {
        var response = await _reportService.GetTeacherReportAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyCollection<TeacherReportItem>>.Ok(response));
    }
}
