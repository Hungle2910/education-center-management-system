using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EducationCenter.Crm.Application.Reports;
using EducationCenter.Crm.Domain.Classes;
using EducationCenter.Crm.Domain.People;
using EducationCenter.Crm.Infrastructure.Persistence;
using EducationCenter.Crm.Infrastructure.Reports;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EducationCenter.Crm.Tests;

public sealed class ReportServiceTests : IDisposable
{
    private readonly ApplicationDbContext _db;
    private readonly ReportService _service;

    public ReportServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _db = new ApplicationDbContext(options);
        _db.Database.EnsureCreated();
        _service = new ReportService(_db);
    }

    public void Dispose()
    {
        _db.Database.EnsureDeleted();
        _db.Dispose();
    }

    [Fact]
    public async Task GetTuitionReportAsync_ShouldCalculateCorrectSums()
    {
        // Arrange
        var cls = new Class { Id = Guid.NewGuid(), Name = "Toán nâng cao 9" };
        var invoice1 = new TuitionInvoice
        {
            Id = Guid.NewGuid(),
            StudentId = Guid.NewGuid(),
            ClassId = cls.Id,
            Month = "2026-07",
            BaseAmount = 1500000,
            TotalAmount = 1500000,
            PaidAmount = 1500000,
            Status = "Đã thanh toán"
        };
        var invoice2 = new TuitionInvoice
        {
            Id = Guid.NewGuid(),
            StudentId = Guid.NewGuid(),
            ClassId = cls.Id,
            Month = "2026-07",
            BaseAmount = 1500000,
            TotalAmount = 1500000,
            Status = "Chưua thanh toán" // typo intended to simulate unpaid
        };
        _db.Classes.Add(cls);
        _db.TuitionInvoices.AddRange(invoice1, invoice2);
        await _db.SaveChangesAsync();

        // Act
        var result = await _service.GetTuitionReportAsync(CancellationToken.None);

        // Assert
        Assert.Equal(1500000, result.TotalCollected);
        Assert.Single(result.RevenueByClass);
        Assert.Equal("Toán nâng cao 9", result.RevenueByClass.First().ClassName);
    }

    [Fact]
    public async Task GetClassReportAsync_ShouldIdentifyAtRiskClasses()
    {
        // Arrange
        var cls = new Class
        {
            Id = Guid.NewGuid(),
            Name = "Toán 10",
            MinStudents = 5,
            MaxStudents = 15,
            Status = "Đang học"
        };
        _db.Classes.Add(cls);
        await _db.SaveChangesAsync();

        // Act
        var result = await _service.GetClassReportAsync(CancellationToken.None);

        // Assert
        Assert.Single(result);
        var item = result.First();
        Assert.Equal("Toán 10", item.ClassName);
        Assert.Equal(0, item.ActiveStudentCount);
        Assert.True(item.IsAtRiskOfLoss); // 0 active students < MinStudents (5)
    }
}
