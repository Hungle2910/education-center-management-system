using System.Text.RegularExpressions;

namespace EducationCenter.Crm.Application.Common.PhoneNumbers;

public sealed partial class VietnamPhoneNumberNormalizer : IPhoneNumberNormalizer
{
    public bool TryNormalize(string? phoneNumber, out string normalizedPhoneNumber)
    {
        normalizedPhoneNumber = string.Empty;

        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            return false;
        }

        var cleaned = RemoveSeparatorsRegex()
            .Replace(phoneNumber.Trim(), string.Empty);

        if (cleaned.StartsWith("+84", StringComparison.Ordinal))
        {
            cleaned = $"84{cleaned[3..]}";
        }
        else if (cleaned.StartsWith('0'))
        {
            cleaned = $"84{cleaned[1..]}";
        }

        if (!VietnamPhoneRegex().IsMatch(cleaned))
        {
            return false;
        }

        normalizedPhoneNumber = cleaned;
        return true;
    }

    [GeneratedRegex(@"[\s\.\-\(\)]", RegexOptions.Compiled)]
    private static partial Regex RemoveSeparatorsRegex();

    [GeneratedRegex(@"^84\d{9}$", RegexOptions.Compiled)]
    private static partial Regex VietnamPhoneRegex();
}
