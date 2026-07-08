namespace EducationCenter.Crm.Application.Common.PhoneNumbers;

public interface IPhoneNumberNormalizer
{
    bool TryNormalize(string? phoneNumber, out string normalizedPhoneNumber);
}
