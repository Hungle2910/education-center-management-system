using System;
using System.Threading;
using System.Threading.Tasks;
using EducationCenter.Crm.Application.Tuition;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EducationCenter.Crm.Api.Controllers;

[ApiController]
[Route("api/tuition")]
[Authorize(Roles = "Admin,Staff")]
public sealed class TuitionController : ControllerBase
{
    private readonly ITuitionService _tuition;

    public TuitionController(ITuitionService tuition)
    {
        _tuition = tuition;
    }

    /// <summary>Preview tuition fees for a class before generating invoices.</summary>
    [HttpPost("preview")]
    public async Task<IActionResult> Preview([FromBody] GenerateTuitionRequest request)
    {
        var result = await _tuition.PreviewTuitionAsync(request);
        return Ok(new { data = result });
    }

    /// <summary>Generate monthly invoices for all enrolled students of a class.</summary>
    [HttpPost("generate")]
    public async Task<IActionResult> Generate([FromBody] GenerateTuitionRequest request)
    {
        var result = await _tuition.GenerateTuitionAsync(request);
        return Ok(new { data = result, message = $"Đã tạo {result.Count} hoá đơn học phí." });
    }

    /// <summary>List all invoices with optional filters.</summary>
    [HttpGet("invoices")]
    public async Task<IActionResult> GetInvoices([FromQuery] Guid? classId, [FromQuery] string? month)
    {
        var result = await _tuition.GetInvoicesAsync(classId, month);
        return Ok(new { data = result });
    }

    /// <summary>Get a single invoice by ID.</summary>
    [HttpGet("invoice/{id:guid}")]
    public async Task<IActionResult> GetInvoice(Guid id)
    {
        try
        {
            var result = await _tuition.GetInvoiceByIdAsync(id);
            return Ok(new { data = result });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>Manually adjust an invoice amount with a reason.</summary>
    [HttpPost("invoice/{id:guid}/adjust")]
    public async Task<IActionResult> Adjust(Guid id, [FromBody] AdjustTuitionRequest request)
    {
        try
        {
            var result = await _tuition.AdjustInvoiceAsync(id, request);
            return Ok(new { data = result, message = "Đã điều chỉnh hoá đơn." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>Apply a discount code to an invoice.</summary>
    [HttpPost("invoice/{id:guid}/apply-discount")]
    public async Task<IActionResult> ApplyDiscount(Guid id, [FromBody] ApplyDiscountRequest request)
    {
        try
        {
            var result = await _tuition.ApplyDiscountAsync(id, request);
            return Ok(new { data = result, message = "Đã áp dụng mã giảm giá." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Tạo mã VietQR thanh toán học phí.</summary>
    [HttpPost("invoice/{id:guid}/generate-vietqr")]
    public async Task<IActionResult> GenerateVietQr(Guid id, [FromBody] GenerateVietQrRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _tuition.GenerateVietQrAsync(id, request, cancellationToken);
            return Ok(new { data = result, message = "Tạo VietQR thành công." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Cập nhật nội dung chuyển khoản thủ công cho hoá đơn.</summary>
    [HttpPut("invoice/{id:guid}/payment-content")]
    public async Task<IActionResult> UpdatePaymentContent(Guid id, [FromBody] UpdatePaymentContentRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _tuition.UpdatePaymentContentAsync(id, request.PaymentContent, cancellationToken);
            return Ok(new { data = result, message = "Cập nhật nội dung chuyển khoản thành công. QR cần tạo lại." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Parent or staff reports payment with proof URL.</summary>
    [HttpPost("invoice/{id:guid}/submit-payment-proof")]
    [Authorize(Roles = "Admin,Staff,Parent")]
    public async Task<IActionResult> SubmitPaymentProof(Guid id, [FromBody] SubmitPaymentProofRequest request)
    {
        try
        {
            var result = await _tuition.SubmitPaymentProofAsync(id, request);
            return Ok(new { data = result, message = "Đã gửi biên lai thanh toán." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Admin confirms payment, supporting under/over payment.</summary>
    [HttpPost("invoice/{id:guid}/confirm-payment")]
    public async Task<IActionResult> ConfirmPayment(Guid id, [FromBody] ConfirmPaymentRequest request)
    {
        try
        {
            var result = await _tuition.ConfirmPaymentAsync(id, request);
            return Ok(new { data = result, message = "Đã xác nhận thanh toán." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}

