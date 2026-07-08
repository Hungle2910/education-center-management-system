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
[Route("api/students")]
public sealed class StudentsController : ControllerBase
{
    private readonly IStudentService _studentService;
    private readonly IValidator<StudentRequest> _validator;

    public StudentsController(IStudentService studentService, IValidator<StudentRequest> validator)
    {
        _studentService = studentService;
        _validator = validator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<StudentResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var students = await _studentService.GetAllAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyCollection<StudentResponse>>.Ok(students));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<StudentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var student = await _studentService.GetByIdAsync(id, cancellationToken);
        return student is null
            ? NotFound(ApiResponse<object>.Fail("Không tìm thấy học sinh."))
            : Ok(ApiResponse<StudentResponse>.Ok(student));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<StudentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] StudentRequest request,
        CancellationToken cancellationToken)
    {
        var validation = await _validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return BadRequest(ApiResponse<object>.Fail(
                "Vui lòng kiểm tra lại thông tin.",
                validation.Errors.Select(error => error.ErrorMessage).ToArray()));
        }

        var response = await _studentService.CreateAsync(request, cancellationToken);
        return Ok(ApiResponse<StudentResponse>.Ok(response, "Thêm học sinh thành công."));
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<StudentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] StudentRequest request,
        CancellationToken cancellationToken)
    {
        var validation = await _validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return BadRequest(ApiResponse<object>.Fail(
                "Vui lòng kiểm tra lại thông tin.",
                validation.Errors.Select(error => error.ErrorMessage).ToArray()));
        }

        var response = await _studentService.UpdateAsync(id, request, cancellationToken);
        return response is null
            ? NotFound(ApiResponse<object>.Fail("Không tìm thấy học sinh."))
            : Ok(ApiResponse<StudentResponse>.Ok(response, "Cập nhật học sinh thành công."));
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var (success, error) = await _studentService.DeleteAsync(id, cancellationToken);
        if (!success)
        {
            var isNotFound = error?.Contains("Không tìm thấy") == true;
            return isNotFound
                ? NotFound(ApiResponse<object>.Fail(error!))
                : BadRequest(ApiResponse<object>.Fail(error!));
        }

        return Ok(ApiResponse<object>.Ok(null, "Xóa học sinh thành công."));
    }
}

