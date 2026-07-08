using System;
using System.Globalization;
using System.Text;

namespace EducationCenter.Crm.Application.Tuition;

public static class PaymentContentGenerator
{
    public static string Generate(string studentName, string className, string month)
    {
        // 1. Chuyển StudentName sang không dấu và PascalCase
        var cleanStudentName = ToPascalCaseNoAccent(studentName);

        // 2. Chuyển ClassName sang không dấu và xóa khoảng trắng (giữ số và dấu chấm)
        var cleanClassName = RemoveSpacesAndAccent(className);

        // 3. Chuyển Month "YYYY-MM" sang "MM/YY"
        var formattedMonth = FormatMonth(month);

        return $"{cleanStudentName}-{cleanClassName} {formattedMonth}";
    }

    private static string ToPascalCaseNoAccent(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;

        var noAccent = RemoveVietnameseDiacritics(text);
        
        // Chia tách theo các khoảng trắng để tạo PascalCase
        var words = noAccent.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        var sb = new StringBuilder();
        foreach (var word in words)
        {
            if (word.Length > 0)
            {
                sb.Append(char.ToUpperInvariant(word[0]));
                if (word.Length > 1)
                {
                    sb.Append(word.Substring(1).ToLowerInvariant());
                }
            }
        }
        return sb.ToString();
    }

    private static string RemoveSpacesAndAccent(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;

        var noAccent = RemoveVietnameseDiacritics(text);
        
        // Giữ lại ký tự không phải khoảng trắng
        var sb = new StringBuilder();
        foreach (var c in noAccent)
        {
            if (!char.IsWhiteSpace(c))
            {
                sb.Append(c);
            }
        }
        return sb.ToString();
    }

    private static string FormatMonth(string month)
    {
        if (string.IsNullOrWhiteSpace(month)) return "00/00";

        var parts = month.Split('-');
        if (parts.Length == 2 && parts[0].Length == 4 && parts[1].Length == 2)
        {
            var yearShort = parts[0].Substring(2); // e.g. "26" từ "2026"
            var monthStr = parts[1]; // e.g. "07"
            return $"{monthStr}/{yearShort}";
        }
        return "00/00";
    }

    private static string RemoveVietnameseDiacritics(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;

        var normalized = text.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (var c in normalized)
        {
            var cat = CharUnicodeInfo.GetUnicodeCategory(c);
            if (cat != UnicodeCategory.NonSpacingMark)
            {
                sb.Append(c);
            }
        }

        return sb.ToString()
            .Normalize(NormalizationForm.FormC)
            .Replace('đ', 'd').Replace('Đ', 'D');
    }
}
