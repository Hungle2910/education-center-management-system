using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using EducationCenter.Crm.Api.Contracts;
using EducationCenter.Crm.Application.Tuition;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EducationCenter.Crm.Api.Controllers;

[ApiController]
[Route("api/payment-settings")]
public sealed class PaymentSettingsController : ControllerBase
{
    private readonly IPaymentSettingService _paymentSettingService;
    private readonly IVietQrService _vietQrService;

    public PaymentSettingsController(
        IPaymentSettingService paymentSettingService,
        IVietQrService vietQrService)
    {
        _paymentSettingService = paymentSettingService;
        _vietQrService = vietQrService;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Staff")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<PaymentSettingResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _paymentSettingService.GetAllAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyCollection<PaymentSettingResponse>>.Ok(result));
    }

    [HttpGet("default")]
    [Authorize(Roles = "Admin,Staff,Parent")]
    [ProducesResponseType(typeof(ApiResponse<PaymentSettingResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDefault(CancellationToken cancellationToken)
    {
        var result = await _paymentSettingService.GetDefaultAsync(cancellationToken);
        return result == null
            ? NotFound(ApiResponse<object>.Fail("Chưa cấu hình tài khoản nhận tiền."))
            : Ok(ApiResponse<PaymentSettingResponse>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Admin,Staff")]
    [ProducesResponseType(typeof(ApiResponse<PaymentSettingResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _paymentSettingService.GetByIdAsync(id, cancellationToken);
        return result == null
            ? NotFound(ApiResponse<object>.Fail("Không tìm thấy cấu hình thanh toán."))
            : Ok(ApiResponse<PaymentSettingResponse>.Ok(result));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<PaymentSettingResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] PaymentSettingRequest request, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        try
        {
            var result = await _paymentSettingService.CreateAsync(request, userId, cancellationToken);
            return Ok(ApiResponse<PaymentSettingResponse>.Ok(result, "Lưu cấu hình thanh toán thành công."));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<PaymentSettingResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] PaymentSettingRequest request, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        try
        {
            var result = await _paymentSettingService.UpdateAsync(id, request, userId, cancellationToken);
            return result == null
                ? NotFound(ApiResponse<object>.Fail("Không tìm thấy cấu hình thanh toán."))
                : Ok(ApiResponse<PaymentSettingResponse>.Ok(result, "Lưu cấu hình thanh toán thành công."));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
    }

    [HttpPost("{id:guid}/set-default")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetDefault(Guid id, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        try
        {
            var success = await _paymentSettingService.SetDefaultAsync(id, userId, cancellationToken);
            return success
                ? Ok(ApiResponse<object>.Ok(null, "Đặt mặc định thành công."))
                : NotFound(ApiResponse<object>.Fail("Không tìm thấy cấu hình thanh toán."));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
    }

    [HttpPost("{id:guid}/test-vietqr")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> TestVietQr(Guid id, CancellationToken cancellationToken)
    {
        var setting = await _paymentSettingService.GetByIdAsync(id, cancellationToken);
        if (setting == null)
            return NotFound(ApiResponse<object>.Fail("Không tìm thấy cấu hình thanh toán."));

        var testUrl = _vietQrService.GenerateQuickLink(
            setting.BankId,
            setting.AccountNo,
            setting.AccountName,
            1000,
            "TEST VIETQR",
            setting.VietQrTemplate);

        return Ok(ApiResponse<string>.Ok(testUrl, "Tạo VietQR thành công."));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var success = await _paymentSettingService.DeleteAsync(id, userId, cancellationToken);
        return success
            ? Ok(ApiResponse<object>.Ok(null, "Xóa cấu hình thanh toán thành công."))
            : NotFound(ApiResponse<object>.Fail("Không tìm thấy cấu hình thanh toán."));
    }

    private Guid? GetCurrentUserId()
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(idClaim, out var parsedId) ? parsedId : null;
    }
}
