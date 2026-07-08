using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using EducationCenter.Crm.Api.Contracts;
using EducationCenter.Crm.Application.Admissions;
using EducationCenter.Crm.Domain.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EducationCenter.Crm.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin,Staff")]
[Route("api/admissions")]
public sealed class AdmissionsController : ControllerBase
{
    private readonly IAdmissionsService _admissionsService;

    public AdmissionsController(IAdmissionsService admissionsService)
    {
        _admissionsService = admissionsService;
    }

    [HttpGet("leads")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<LeadDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLeads(CancellationToken cancellationToken)
    {
        var leads = await _admissionsService.GetLeadsAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyCollection<LeadDto>>.Ok(leads));
    }

    [HttpPost("leads")]
    [ProducesResponseType(typeof(ApiResponse<LeadDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateLead([FromBody] CreateLeadRequest request, CancellationToken cancellationToken)
    {
        var lead = await _admissionsService.CreateLeadAsync(request, cancellationToken);
        return Ok(ApiResponse<LeadDto>.Ok(lead, "Thêm học sinh tiềm năng thành công."));
    }

    [HttpPut("leads/{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<LeadDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateLead(Guid id, [FromBody] UpdateLeadRequest request, CancellationToken cancellationToken)
    {
        var lead = await _admissionsService.UpdateLeadAsync(id, request, cancellationToken);
        return Ok(ApiResponse<LeadDto>.Ok(lead, "Cập nhật thông tin thành công."));
    }

    [HttpPost("leads/{id:guid}/convert")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ConvertLead(Guid id, CancellationToken cancellationToken)
    {
        await _admissionsService.ConvertLeadToStudentAsync(id, cancellationToken);
        return Ok(ApiResponse<object>.Ok(null, "Chuyển đổi thành học viên chính thức thành công."));
    }

    [HttpGet("trials")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<TrialSessionDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTrialSessions(CancellationToken cancellationToken)
    {
        var trials = await _admissionsService.GetTrialSessionsAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyCollection<TrialSessionDto>>.Ok(trials));
    }

    [HttpPost("trials")]
    [ProducesResponseType(typeof(ApiResponse<TrialSessionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ScheduleTrial([FromBody] ScheduleTrialRequest request, CancellationToken cancellationToken)
    {
        var trial = await _admissionsService.ScheduleTrialSessionAsync(request, cancellationToken);
        return Ok(ApiResponse<TrialSessionDto>.Ok(trial, "Đặt lịch học thử thành công."));
    }

    [HttpPost("trials/{id:guid}/evaluate")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> EvaluateTrial(Guid id, [FromBody] EvaluateTrialRequest request, CancellationToken cancellationToken)
    {
        await _admissionsService.EvaluateTrialSessionAsync(id, request, cancellationToken);
        return Ok(ApiResponse<object>.Ok(null, "Đánh giá kết quả học thử thành công."));
    }

    [HttpGet("care-logs")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<ParentCareLogDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCareLogs([FromQuery] Guid? parentId, [FromQuery] Guid? leadId, CancellationToken cancellationToken)
    {
        var logs = await _admissionsService.GetParentCareLogsAsync(parentId, leadId, cancellationToken);
        return Ok(ApiResponse<IReadOnlyCollection<ParentCareLogDto>>.Ok(logs));
    }

    [HttpPost("care-logs")]
    [ProducesResponseType(typeof(ApiResponse<ParentCareLogDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateCareLog([FromBody] CreateCareLogRequest request, CancellationToken cancellationToken)
    {
        var staffIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var staffId = Guid.TryParse(staffIdClaim, out var parsedGuid) ? parsedGuid : Guid.Empty;

        var reqWithStaff = request with { StaffId = staffId };

        var log = await _admissionsService.CreateCareLogAsync(reqWithStaff, cancellationToken);
        return Ok(ApiResponse<ParentCareLogDto>.Ok(log, "Lưu nhật ký chăm sóc thành công."));
    }
}
