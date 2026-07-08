using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EducationCenter.Crm.Application.Tuition;

public interface IPaymentSettingService
{
    Task<IReadOnlyCollection<PaymentSettingResponse>> GetAllAsync(CancellationToken cancellationToken);

    Task<PaymentSettingResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<PaymentSettingResponse?> GetDefaultAsync(CancellationToken cancellationToken);

    Task<PaymentSettingResponse> CreateAsync(PaymentSettingRequest request, Guid? userId, CancellationToken cancellationToken);

    Task<PaymentSettingResponse?> UpdateAsync(Guid id, PaymentSettingRequest request, Guid? userId, CancellationToken cancellationToken);

    Task<bool> SetDefaultAsync(Guid id, Guid? userId, CancellationToken cancellationToken);

    Task<bool> DeleteAsync(Guid id, Guid? userId, CancellationToken cancellationToken);
}
