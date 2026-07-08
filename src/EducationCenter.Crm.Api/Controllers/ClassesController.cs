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
[Route("api/classes")]
public sealed class ClassesController : ControllerBase
{
    private readonly IClassService _classService;
    private readonly IValidator<ClassRequest> _validator;

    public ClassesController(IClassService classService, IValidator<ClassRequest> validator)
    {
        _classService = classService;
        _validator = validator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<ClassResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var classes = await _classService.GetAllAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyCollection<ClassResponse>>.Ok(classes));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ClassResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var classRoom = await _classService.GetByIdAsync(id, cancellationToken);
        return classRoom is null
            ? NotFound(ApiResponse<object>.Fail("Không tìm thấy lớp học."))
            : Ok(ApiResponse<ClassResponse>.Ok(classRoom));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ClassResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] ClassRequest request,
        CancellationToken cancellationToken)
    {
        var validation = await _validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return BadRequest(ApiResponse<object>.Fail(
                "Vui lòng kiểm tra lại thông tin.",
                validation.Errors.Select(error => error.ErrorMessage).ToArray()));
        }

        var response = await _classService.CreateAsync(request, cancellationToken);
        return Ok(ApiResponse<ClassResponse>.Ok(response, "Tạo lớp thành công."));
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ClassResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] ClassRequest request,
        CancellationToken cancellationToken)
    {
        var validation = await _validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return BadRequest(ApiResponse<object>.Fail(
                "Vui lòng kiểm tra lại thông tin.",
                validation.Errors.Select(error => error.ErrorMessage).ToArray()));
        }

        var response = await _classService.UpdateAsync(id, request, cancellationToken);
        return response is null
            ? NotFound(ApiResponse<object>.Fail("Không tìm thấy lớp học."))
            : Ok(ApiResponse<ClassResponse>.Ok(response, "Cập nhật lớp học thành công."));
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var (success, error) = await _classService.DeleteAsync(id, cancellationToken);
        if (!success)
        {
            var isNotFound = error?.Contains("Không tìm thấy") == true;
            return isNotFound
                ? NotFound(ApiResponse<object>.Fail(error!))
                : BadRequest(ApiResponse<object>.Fail(error!));
        }

        return Ok(ApiResponse<object>.Ok(null, "Xóa lớp học thành công."));
    }
}

