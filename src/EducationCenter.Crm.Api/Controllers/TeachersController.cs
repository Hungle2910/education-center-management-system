using EducationCenter.Crm.Api.Contracts;
using EducationCenter.Crm.Application.CoreData;
using EducationCenter.Crm.Domain.Identity;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EducationCenter.Crm.Api.Controllers;

[ApiController]
[Authorize(Policy = AppRoles.Staff)]
[Route("api/teachers")]
public sealed class TeachersController : ControllerBase
{
    private readonly ITeacherService _teacherService;
    private readonly IValidator<TeacherRequest> _validator;

    public TeachersController(ITeacherService teacherService, IValidator<TeacherRequest> validator)
    {
        _teacherService = teacherService;
        _validator = validator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<TeacherResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var teachers = await _teacherService.GetAllAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyCollection<TeacherResponse>>.Ok(teachers));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<TeacherResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var teacher = await _teacherService.GetByIdAsync(id, cancellationToken);
        return teacher is null
            ? NotFound(ApiResponse<object>.Fail("Không tìm thấy giáo viên."))
            : Ok(ApiResponse<TeacherResponse>.Ok(teacher));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<TeacherResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] TeacherRequest request,
        CancellationToken cancellationToken)
    {
        var validation = await _validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return BadRequest(ApiResponse<object>.Fail(
                "Vui lòng kiểm tra lại thông tin.",
                validation.Errors.Select(error => error.ErrorMessage).ToArray()));
        }

        var response = await _teacherService.CreateAsync(request, cancellationToken);
        return Ok(ApiResponse<TeacherResponse>.Ok(response, "Thêm giáo viên thành công."));
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<TeacherResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] TeacherRequest request,
        CancellationToken cancellationToken)
    {
        var validation = await _validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return BadRequest(ApiResponse<object>.Fail(
                "Vui lòng kiểm tra lại thông tin.",
                validation.Errors.Select(error => error.ErrorMessage).ToArray()));
        }

        var response = await _teacherService.UpdateAsync(id, request, cancellationToken);
        return response is null
            ? NotFound(ApiResponse<object>.Fail("Không tìm thấy giáo viên."))
            : Ok(ApiResponse<TeacherResponse>.Ok(response, "Cập nhật giáo viên thành công."));
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var (success, error) = await _teacherService.DeleteAsync(id, cancellationToken);
        if (!success)
        {
            // Phân biệt Not Found vs Business Rule violation
            var isNotFound = error?.Contains("Không tìm thấy") == true;
            return isNotFound
                ? NotFound(ApiResponse<object>.Fail(error!))
                : BadRequest(ApiResponse<object>.Fail(error!));
        }

        return Ok(ApiResponse<object>.Ok(null, "Xóa giáo viên thành công."));
    }
}

