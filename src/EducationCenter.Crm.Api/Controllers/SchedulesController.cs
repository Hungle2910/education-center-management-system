using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using EducationCenter.Crm.Api.Contracts;
using EducationCenter.Crm.Application.Common;
using EducationCenter.Crm.Application.Schedules;
using EducationCenter.Crm.Domain.Identity;
using EducationCenter.Crm.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EducationCenter.Crm.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin,Staff,Teacher")]
[Route("api/schedules")]
public sealed class SchedulesController : ControllerBase
{
    private readonly IScheduleService _scheduleService;
    private readonly ApplicationDbContext _dbContext;

    public SchedulesController(IScheduleService scheduleService, ApplicationDbContext dbContext)
    {
        _scheduleService = scheduleService;
        _dbContext = dbContext;
    }

    [HttpPost]
    [Authorize(Policy = AppRoles.Staff)]
    [ProducesResponseType(typeof(ApiResponse<ScheduleResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateScheduleRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await _scheduleService.CreateScheduleAsync(request, cancellationToken);
            return Ok(ApiResponse<ScheduleResponse>.Ok(response, "Cập nhật lịch học thành công."));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(ApiResponse<object>.Fail(exception.Message));
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(ApiResponse<object>.Fail(exception.Message));
        }
    }

    [HttpGet("calendar")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<ScheduleOccurrenceResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCalendar(
        [FromQuery] DateOnly startDate,
        [FromQuery] DateOnly endDate,
        CancellationToken cancellationToken)
    {
        var occurrences = await _scheduleService.GetCalendarAsync(startDate, endDate, cancellationToken);
        return Ok(ApiResponse<IReadOnlyCollection<ScheduleOccurrenceResponse>>.Ok(occurrences));
    }

    [HttpGet("conflicts/check")]
    [ProducesResponseType(typeof(ApiResponse<ConflictCheckResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CheckConflicts(
        [FromQuery] DateOnly date,
        [FromQuery] TimeOnly startTime,
        [FromQuery] TimeOnly endTime,
        [FromQuery] Guid roomId,
        [FromQuery] Guid? teacherId,
        [FromQuery] Guid? excludeOccurrenceId,
        CancellationToken cancellationToken)
    {
        var request = new ConflictCheckRequest(excludeOccurrenceId, date, startTime, endTime, roomId, teacherId);
        var response = await _scheduleService.CheckConflictsAsync(request, cancellationToken);
        return Ok(ApiResponse<ConflictCheckResponse>.Ok(response));
    }

    [HttpPost("occurrence/{occurrenceId:guid}/cancel")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CancelOccurrence(
        Guid occurrenceId,
        [FromBody] CancelSessionRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            await _scheduleService.CancelOccurrenceAsync(occurrenceId, request, cancellationToken);
            return Ok(ApiResponse<object>.Ok(null, "Hủy buổi học thành công."));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
    }

    [HttpPost("individual-makeup")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RegisterIndividualMakeup(
        [FromBody] ScheduleIndividualMakeupRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            await _scheduleService.RegisterIndividualMakeupAsync(request, cancellationToken);
            return Ok(ApiResponse<object>.Ok(null, "Đăng ký học bù cá nhân thành công."));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
    }

    [HttpGet("occurrence/{occurrenceId:guid}/eligible-absent-students")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<EligibleAbsentStudentDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEligibleAbsentStudents(
        Guid occurrenceId,
        CancellationToken cancellationToken)
    {
        var students = await _scheduleService.GetEligibleAbsentStudentsAsync(occurrenceId, cancellationToken);
        return Ok(ApiResponse<IReadOnlyCollection<EligibleAbsentStudentDto>>.Ok(students));
    }

    [HttpGet("ical/{userId:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetIcalCalendar(Guid userId, CancellationToken cancellationToken)
    {
        var studentClassIds = await _dbContext.TuitionInvoices
            .Where(i => i.StudentId == userId)
            .Select(i => i.ClassId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var occurrences = await _dbContext.ScheduleOccurrences
            .Include(so => so.Class)
            .Include(so => so.Room)
            .Include(so => so.Teacher)
            .Where(so => so.TeacherId == userId || studentClassIds.Contains(so.ClassId))
            .OrderBy(so => so.Date)
            .ThenBy(so => so.StartTime)
            .ToListAsync(cancellationToken);

        var icalContent = IcalHelper.GenerateIcalString(occurrences);
        var bytes = Encoding.UTF8.GetBytes(icalContent);
        return File(bytes, "text/calendar", $"Lich_Hoc_{userId}.ics");
    }
}
