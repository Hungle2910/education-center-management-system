using System;

namespace EducationCenter.Crm.Application.Tuition;

public interface IVietQrService
{
    string GenerateQuickLink(
        string bankId, 
        string accountNo, 
        string accountName, 
        decimal amount, 
        string paymentContent, 
        string template = "compact2");
}
