using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EducationCenter.Crm.Application.Common;
using EducationCenter.Crm.Infrastructure.Services;
using Xunit;

namespace EducationCenter.Crm.Tests;

public sealed class ExcelServiceTests
{
    private readonly ExcelService _service = new();

    [Fact]
    public void ExportToExcel_ShouldGenerateBytes()
    {
        // Arrange
        var data = new List<TestRow>
        {
            new() { Name = "Hoc Sinh A", Age = 15 },
            new() { Name = "Hoc Sinh B", Age = 16 }
        };

        // Act
        var bytes = _service.ExportToExcel(data, "Test Sheet");

        // Assert
        Assert.NotNull(bytes);
        Assert.True(bytes.Length > 0);
    }

    [Fact]
    public void GetStudentImportTemplate_ShouldGenerateBytes()
    {
        // Act
        var bytes = _service.GetStudentImportTemplate();

        // Assert
        Assert.NotNull(bytes);
        Assert.True(bytes.Length > 0);
    }

    [Fact]
    public void ParseStudentImport_ShouldParseCorrectly()
    {
        // Arrange
        var bytes = _service.GetStudentImportTemplate();
        using var stream = new MemoryStream(bytes);

        // Act
        var rows = _service.ParseStudentImport(stream);

        // Assert
        Assert.NotNull(rows);
        Assert.Single(rows);
        var row = rows.First();
        Assert.Equal("Nguyễn Văn A", row.StudentName);
        Assert.Equal("Nguyễn Văn B", row.ParentName);
        Assert.Equal("0909123456", row.ParentPhone);
        Assert.Equal("15/08/2012", row.DateOfBirthString);
        Assert.Equal("Toán 9A", row.ClassName);
    }

    private class TestRow
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
    }
}
