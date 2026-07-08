using EducationCenter.Crm.Application.Tuition;
using Xunit;

namespace EducationCenter.Crm.Tests;

public sealed class PaymentContentGeneratorTests
{
    // ─── Format Tests ──────────────────────────────────────────────────────────

    [Fact]
    public void Generate_VietnameseNameWithAccents_OutputsPascalCaseNoAccent()
    {
        var result = PaymentContentGenerator.Generate("Nguyễn Văn An", "Toán 10", "2026-07");

        // Expected: NguyenVanAn-Toan10 07/26
        Assert.StartsWith("NguyenVanAn", result);
    }

    [Fact]
    public void Generate_MonthFormat_IsMMSlashYY()
    {
        var result = PaymentContentGenerator.Generate("Le Van B", "Anh Van", "2026-07");

        // Month part must be MM/YY format = 07/26
        Assert.Contains("07/26", result);
    }

    [Fact]
    public void Generate_ClassName_RemovesSpacesAndAccents()
    {
        var result = PaymentContentGenerator.Generate("Le Van B", "Tiếng Anh Lớp 10", "2026-07");

        // ClassName: TiengAnhLop10 (no spaces, no accents)
        Assert.Contains("TiengAnhLop10", result);
    }

    [Fact]
    public void Generate_Format_MatchesSpecPattern()
    {
        // Spec: {StudentNameNoAccentPascalCase}-{ClassNameNoAccentNoSpaces} {MM/YY}
        var result = PaymentContentGenerator.Generate("Trần Thị Bình", "Toán 12A", "2026-08");

        // StudentName: TranThiBinh, ClassName: Toan12A, Month: 08/26
        Assert.Equal("TranThiBinh-Toan12A 08/26", result);
    }

    [Fact]
    public void Generate_SpecialCharacterD_ReplacedWithD()
    {
        // đ/Đ phải được replace thành d/D (không qua NFD normalization)
        var result = PaymentContentGenerator.Generate("Đinh Đức Dũng", "Địa lý", "2026-07");

        Assert.DoesNotContain("đ", result);
        Assert.DoesNotContain("Đ", result);
        Assert.Contains("Dinh", result);
    }

    [Fact]
    public void Generate_EmptyStudentName_ReturnsHyphenClassName()
    {
        var result = PaymentContentGenerator.Generate("", "Toán 10", "2026-07");

        Assert.StartsWith("-Toan10", result);
    }

    [Theory]
    [InlineData("2026-01", "01/26")]
    [InlineData("2026-12", "12/26")]
    [InlineData("2027-06", "06/27")]
    public void FormatMonth_VariousInputs_MatchesMMSlashYY(string month, string expected)
    {
        var result = PaymentContentGenerator.Generate("Nguyen A", "Toan", month);

        Assert.EndsWith(expected, result);
    }

    [Fact]
    public void Generate_LongStudentName_ProducesPascalCase()
    {
        var result = PaymentContentGenerator.Generate("Lê Nguyễn Hoàng Minh Tuấn", "Lý 11", "2026-07");

        // Should not contain accented characters
        Assert.DoesNotContain("ễ", result);
        Assert.DoesNotContain("à", result);
        Assert.DoesNotContain("ấ", result);
    }
}
