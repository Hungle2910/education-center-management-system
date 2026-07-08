using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EducationCenter.Crm.Application.Tuition;

public interface ITuitionService
{
    Task<List<TuitionPreviewResponse>> PreviewTuitionAsync(GenerateTuitionRequest request);
    Task<List<TuitionInvoiceResponse>> GenerateTuitionAsync(GenerateTuitionRequest request);
    Task<List<TuitionInvoiceResponse>> GetInvoicesAsync(Guid? classId, string? month);
    Task<TuitionInvoiceResponse> GetInvoiceByIdAsync(Guid id);
    Task<TuitionInvoiceResponse> AdjustInvoiceAsync(Guid id, AdjustTuitionRequest request);
    Task<TuitionInvoiceResponse> ApplyDiscountAsync(Guid id, ApplyDiscountRequest request);
    Task<TuitionInvoiceResponse> GenerateVietQrAsync(Guid id, GenerateVietQrRequest request, CancellationToken cancellationToken);
    Task<TuitionInvoiceResponse> UpdatePaymentContentAsync(Guid id, string paymentContent, CancellationToken cancellationToken);
    Task<TuitionInvoiceResponse> SubmitPaymentProofAsync(Guid id, SubmitPaymentProofRequest request);
    Task<TuitionInvoiceResponse> ConfirmPaymentAsync(Guid id, ConfirmPaymentRequest request);
}


