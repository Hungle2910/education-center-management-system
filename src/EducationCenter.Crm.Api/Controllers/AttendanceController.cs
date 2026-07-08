using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using EducationCenter.Crm.Api.Contracts;
using EducationCenter.Crm.Application.Attendance;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EducationCenter.Crm.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin,Staff,Teacher")]
[Route("api/attendance")]
public sealed class AttendanceController : ControllerBase
{
    private readonly IAttendanceService _attendanceService;

    public AttendanceController(IAttendanceService attendanceService)
    {
        _attendanceService = attendanceService;
    }

    [HttpGet("occurrence/{occurrenceId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<OccurrenceAttendanceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByOccurrence(
        Guid occurrenceId, 
        CancellationToken cancellationToken)
    {
        var attendance = await _attendanceService.GetAttendanceByOccurrenceAsync(occurrenceId, cancellationToken);
        if (attendance is null)
        {
            return NotFound(ApiResponse<object>.Fail("Không tìm thấy thông tin buổi học thực tế."));
        }

        return Ok(ApiResponse<OccurrenceAttendanceDto>.Ok(attendance));
    }

    [HttpPost("submit")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Submit(
        [FromBody] SubmitAttendanceRequest request,
        CancellationToken cancellationToken)
    {
        var auditor = User.FindFirstValue(ClaimTypes.Name) ?? User.FindFirstValue(ClaimTypes.Email) ?? "System";
        
        try
        {
            await _attendanceService.SubmitAttendanceAsync(request, auditor, cancellationToken);
            return Ok(ApiResponse<object>.Ok(null, "Lưu điểm danh thành công."));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
    }
}
