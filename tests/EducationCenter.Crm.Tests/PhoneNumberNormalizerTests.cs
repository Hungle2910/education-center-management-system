using EducationCenter.Crm.Application.Common.PhoneNumbers;

namespace EducationCenter.Crm.Tests;

public sealed class PhoneNumberNormalizerTests
{
    private readonly VietnamPhoneNumberNormalizer _normalizer = new();

    [Theory]
    [InlineData("0909123456")]
    [InlineData("090-912-3456")]
    [InlineData("090.912.3456")]
    [InlineData("+84909123456")]
    public void TryNormalize_ReturnsVietnamCountryCodePhoneNumber(string phoneNumber)
    {
        var isValid = _normalizer.TryNormalize(phoneNumber, out var normalizedPhoneNumber);

        Assert.True(isValid);
        Assert.Equal("84909123456", normalizedPhoneNumber);
    }
}
