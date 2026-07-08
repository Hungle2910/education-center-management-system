using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EducationCenter.Crm.Application.Schedules;
using EducationCenter.Crm.Domain.Classes;
using EducationCenter.Crm.Domain.People;
using EducationCenter.Crm.Infrastructure.Persistence;
using EducationCenter.Crm.Infrastructure.Schedules;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EducationCenter.Crm.Tests;

public sealed class MakeupServiceTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ScheduleService _service;

    public MakeupServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ApplicationDbContext(options);
        _dbContext.Database.EnsureCreated();
        _service = new ScheduleService(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    [Fact]
    public async Task CancelOccurrenceAsync_ShouldSetStatusToCancelled()
    {
        // Arrange
        var occurrence = new ScheduleOccurrence
        {
            Id = Guid.NewGuid(),
            ClassId = Guid.NewGuid(),
            Date = DateOnly.FromDateTime(DateTime.Today),
            StartTime = new TimeOnly(10, 0),
            EndTime = new TimeOnly(11, 30),
            Status = "Đã lên lịch",
            CreatedAtUtc = DateTime.UtcNow
        };
        _dbContext.ScheduleOccurrences.Add(occurrence);
        await _dbContext.SaveChangesAsync();

        var request = new CancelSessionRequest("Tạo học bù");

        // Act
        await _service.CancelOccurrenceAsync(occurrence.Id, request, CancellationToken.None);

        // Assert
        var saved = await _dbContext.ScheduleOccurrences.FindAsync(occurrence.Id);
        Assert.Equal("Đã hủy", saved!.Status);
    }

    [Fact]
    public async Task RegisterIndividualMakeupAsync_ShouldCreateMakeupAndChangeAttendanceStatus()
    {
        // Arrange
        var absentOccurrence = new ScheduleOccurrence
        {
            Id = Guid.NewGuid(),
            ClassId = Guid.NewGuid(),
            Date = DateOnly.FromDateTime(DateTime.Today.AddDays(-2)),
            StartTime = new TimeOnly(10, 0),
            EndTime = new TimeOnly(11, 30),
            Status = "Đã học",
            CreatedAtUtc = DateTime.UtcNow
        };
        var makeupOccurrence = new ScheduleOccurrence
        {
            Id = Guid.NewGuid(),
            ClassId = Guid.NewGuid(),
            Date = DateOnly.FromDateTime(DateTime.Today.AddDays(2)),
            StartTime = new TimeOnly(10, 0),
            EndTime = new TimeOnly(11, 30),
            Status = "Đã lên lịch",
            CreatedAtUtc = DateTime.UtcNow
        };
        _dbContext.ScheduleOccurrences.Add(absentOccurrence);
        _dbContext.ScheduleOccurrences.Add(makeupOccurrence);

        var student = new Student
        {
            Id = Guid.NewGuid(),
            FullName = "Học sinh A",
            Status = StudentStatuses.Active,
            CreatedAtUtc = DateTime.UtcNow
        };
        _dbContext.Students.Add(student);

        var attendance = new Attendance
        {
            Id = Guid.NewGuid(),
            OccurrenceId = absentOccurrence.Id,
            StudentId = student.Id,
            Status = "Vắng có phép",
            CreatedAtUtc = DateTime.UtcNow
        };
        _dbContext.Attendances.Add(attendance);

        await _dbContext.SaveChangesAsync();

        var request = new ScheduleIndividualMakeupRequest(student.Id, absentOccurrence.Id, makeupOccurrence.Id);

        // Act
        await _service.RegisterIndividualMakeupAsync(request, CancellationToken.None);

        // Assert
        var makeupEntry = await _dbContext.IndividualMakeups.FirstOrDefaultAsync(im => im.StudentId == student.Id);
        Assert.NotNull(makeupEntry);
        Assert.Equal(absentOccurrence.Id, makeupEntry.AbsentOccurrenceId);
        Assert.Equal(makeupOccurrence.Id, makeupEntry.MakeupOccurrenceId);

        var updatedAttendance = await _dbContext.Attendances.FindAsync(attendance.Id);
        Assert.Equal("Đã học bù", updatedAttendance!.Status);
    }
}
