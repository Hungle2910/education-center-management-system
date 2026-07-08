using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EducationCenter.Crm.Application.Attendance;
using EducationCenter.Crm.Application.Common.Interfaces;
using EducationCenter.Crm.Domain.Classes;
using EducationCenter.Crm.Domain.People;
using EducationCenter.Crm.Infrastructure.Attendance;
using EducationCenter.Crm.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EducationCenter.Crm.Tests;

public sealed class AttendanceServiceTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly AttendanceService _service;

    public AttendanceServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ApplicationDbContext(options);
        _dbContext.Database.EnsureCreated();
        _service = new AttendanceService(_dbContext, new FakeAuditLogService());
    }

    private sealed class FakeAuditLogService : IAuditLogService
    {
        public Task LogAsync(Guid? userId, string? userEmail, string? userFullName, string action, string? entityType, string? entityId, string? details, string? ipAddress, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task<PagedResult<AuditLogDto>> GetAuditLogsAsync(string? searchTerm, string? action, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new PagedResult<AuditLogDto>(Array.Empty<AuditLogDto>(), 0, pageNumber, pageSize));
        }
    }

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    [Fact]
    public async Task GetAttendanceByOccurrenceAsync_ShouldReturnDefaultPresent_WhenNoAttendanceExists()
    {
        // Arrange
        var @class = new Class
        {
            Id = Guid.NewGuid(),
            Name = "Toán nâng cao 9",
            MinStudents = 5,
            MaxStudents = 15,
            CreatedAtUtc = DateTime.UtcNow
        };
        _dbContext.Classes.Add(@class);

        var occurrence = new ScheduleOccurrence
        {
            Id = Guid.NewGuid(),
            ClassId = @class.Id,
            Class = @class,
            Date = DateOnly.FromDateTime(DateTime.Today),
            StartTime = new TimeOnly(8, 0, 0),
            EndTime = new TimeOnly(9, 30, 0),
            Status = "Đã lên lịch",
            CreatedAtUtc = DateTime.UtcNow
        };
        _dbContext.ScheduleOccurrences.Add(occurrence);

        var student = new Student
        {
            Id = Guid.NewGuid(),
            FullName = "Nguyễn Văn A",
            Status = StudentStatuses.Active,
            CreatedAtUtc = DateTime.UtcNow
        };
        _dbContext.Students.Add(student);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.GetAttendanceByOccurrenceAsync(occurrence.Id, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Toán nâng cao 9", result.ClassName);
        Assert.Single(result.Students);
        Assert.Equal("Nguyễn Văn A", result.Students[0].StudentName);
        Assert.Equal("Có mặt", result.Students[0].Status);
    }

    [Fact]
    public async Task SubmitAttendanceAsync_ShouldSaveAttendanceAndSetOccurrenceStatusToDone()
    {
        // Arrange
        var @class = new Class
        {
            Id = Guid.NewGuid(),
            Name = "Toán nâng cao 9",
            MinStudents = 5,
            MaxStudents = 15,
            CreatedAtUtc = DateTime.UtcNow
        };
        _dbContext.Classes.Add(@class);

        var occurrence = new ScheduleOccurrence
        {
            Id = Guid.NewGuid(),
            ClassId = @class.Id,
            Class = @class,
            Date = DateOnly.FromDateTime(DateTime.Today),
            StartTime = new TimeOnly(8, 0, 0),
            EndTime = new TimeOnly(9, 30, 0),
            Status = "Đã lên lịch",
            CreatedAtUtc = DateTime.UtcNow
        };
        _dbContext.ScheduleOccurrences.Add(occurrence);

        var student = new Student
        {
            Id = Guid.NewGuid(),
            FullName = "Nguyễn Văn A",
            Status = StudentStatuses.Active,
            CreatedAtUtc = DateTime.UtcNow
        };
        _dbContext.Students.Add(student);
        await _dbContext.SaveChangesAsync();

        var request = new SubmitAttendanceRequest(
            occurrence.Id,
            new List<StudentAttendanceDto>
            {
                new StudentAttendanceDto(student.Id, student.FullName, "Vắng có phép", "Xin nghỉ ốm")
            }
        );

        // Act
        await _service.SubmitAttendanceAsync(request, "Teacher Hoang", CancellationToken.None);

        // Assert
        var savedOccurrence = await _dbContext.ScheduleOccurrences.FindAsync(occurrence.Id);
        Assert.Equal("Đã học", savedOccurrence!.Status);

        var savedAttendance = await _dbContext.Attendances.FirstOrDefaultAsync(a => a.OccurrenceId == occurrence.Id);
        Assert.NotNull(savedAttendance);
        Assert.Equal("Vắng có phép", savedAttendance.Status);
        Assert.Equal("Xin nghỉ ốm", savedAttendance.Notes);
        Assert.Equal("Teacher Hoang", savedAttendance.AuditedBy);
    }
}
