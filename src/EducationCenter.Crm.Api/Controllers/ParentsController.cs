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
[Route("api/parents")]
public sealed class ParentsController : ControllerBase
{
    private readonly IParentService _parentService;
    private readonly IValidator<ParentRequest> _validator;

    public ParentsController(IParentService parentService, IValidator<ParentRequest> validator)
    {
        _parentService = parentService;
        _validator = validator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<ParentResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var parents = await _parentService.GetAllAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyCollection<ParentResponse>>.Ok(parents));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ParentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var parent = await _parentService.GetByIdAsync(id, cancellationToken);
        return parent is null
            ? NotFound(ApiResponse<object>.Fail("Không tìm thấy phụ huynh."))
            : Ok(ApiResponse<ParentResponse>.Ok(parent));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ParentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] ParentRequest request,
        CancellationToken cancellationToken)
    {
        var validation = await _validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return BadRequest(ApiResponse<object>.Fail(
                "Vui lòng kiểm tra lại thông tin.",
                validation.Errors.Select(error => error.ErrorMessage).ToArray()));
        }

        var response = await _parentService.CreateAsync(request, cancellationToken);
        return Ok(ApiResponse<ParentResponse>.Ok(response, "Thêm phụ huynh thành công."));
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ParentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] ParentRequest request,
        CancellationToken cancellationToken)
    {
        var validation = await _validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return BadRequest(ApiResponse<object>.Fail(
                "Vui lòng kiểm tra lại thông tin.",
                validation.Errors.Select(error => error.ErrorMessage).ToArray()));
        }

        var response = await _parentService.UpdateAsync(id, request, cancellationToken);
        return response is null
            ? NotFound(ApiResponse<object>.Fail("Không tìm thấy phụ huynh."))
            : Ok(ApiResponse<ParentResponse>.Ok(response, "Cập nhật phụ huynh thành công."));
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var (success, error) = await _parentService.DeleteAsync(id, cancellationToken);
        if (!success)
        {
            var isNotFound = error?.Contains("Không tìm thấy") == true;
            return isNotFound
                ? NotFound(ApiResponse<object>.Fail(error!))
                : BadRequest(ApiResponse<object>.Fail(error!));
        }

        return Ok(ApiResponse<object>.Ok(null, "Xóa phụ huynh thành công."));
    }
}

