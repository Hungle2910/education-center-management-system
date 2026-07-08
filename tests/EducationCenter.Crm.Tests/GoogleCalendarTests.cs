using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EducationCenter.Crm.Application.Common.Interfaces;
using EducationCenter.Crm.Application.Schedules;
using EducationCenter.Crm.Domain.Classes;
using EducationCenter.Crm.Domain.People;
using EducationCenter.Crm.Infrastructure.Persistence;
using EducationCenter.Crm.Infrastructure.Schedules;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace EducationCenter.Crm.Tests;

public sealed class GoogleCalendarTests : IDisposable
{
    private readonly ApplicationDbContext _db;
    private readonly Mock<IGoogleCalendarService> _gcalMock;
    private readonly ScheduleService _service;

    public GoogleCalendarTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _db = new ApplicationDbContext(options);
        _db.Database.EnsureCreated();

        _gcalMock = new Mock<IGoogleCalendarService>();
        _service = new ScheduleService(_db, _gcalMock.Object);
    }

    public void Dispose()
    {
        _db.Database.EnsureDeleted();
        _db.Dispose();
    }

    [Fact]
    public async Task CreateScheduleAsync_ShouldInvokeCreateEventOnGoogleCalendar()
    {
        // Arrange
        var teacher = new Teacher { Id = Guid.NewGuid(), FullName = "Thay A" };
        var cls = new Class 
        { 
            Id = Guid.NewGuid(), 
            Name = "Lop GCal 1", 
            TeacherId = teacher.Id,
            Status = "Đang học"
        };
        var room = new Room { Id = Guid.NewGuid(), Name = "Phong 101" };

        _db.Teachers.Add(teacher);
        _db.Classes.Add(cls);
        _db.Rooms.Add(room);
        await _db.SaveChangesAsync();

        var request = new CreateScheduleRequest(
            cls.Id,
            DayOfWeek.Monday,
            new TimeOnly(8, 0),
            new TimeOnly(10, 0),
            room.Id,
            teacher.Id);

        _gcalMock.Setup(g => g.CreateEventAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<DateTime>(),
            It.IsAny<DateTime>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync("gcal-event-123");

        // Act
        var result = await _service.CreateScheduleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        
        // Verify we created 4 occurrences and invoked Google Calendar for each
        _gcalMock.Verify(g => g.CreateEventAsync(
            It.Is<string>(s => s.Contains("Lop GCal 1")),
            "Phong 101",
            It.IsAny<string>(),
            It.IsAny<DateTime>(),
            It.IsAny<DateTime>(),
            It.IsAny<CancellationToken>()), Times.Exactly(4));

        var occurrences = await _db.ScheduleOccurrences.ToListAsync();
        Assert.Equal(4, occurrences.Count);
        Assert.All(occurrences, o => Assert.Equal("gcal-event-123", o.GoogleEventId));
    }

    [Fact]
    public async Task CancelOccurrenceAsync_ShouldInvokeDeleteEventOnGoogleCalendar()
    {
        // Arrange
        var occurrence = new ScheduleOccurrence
        {
            Id = Guid.NewGuid(),
            ClassId = Guid.NewGuid(),
            Date = DateOnly.FromDateTime(DateTime.Today),
            StartTime = new TimeOnly(14, 0),
            EndTime = new TimeOnly(16, 0),
            RoomId = Guid.NewGuid(),
            Status = "Đã lên lịch",
            GoogleEventId = "gcal-event-999",
            CreatedAtUtc = DateTime.UtcNow
        };
        _db.ScheduleOccurrences.Add(occurrence);
        await _db.SaveChangesAsync();

        _gcalMock.Setup(g => g.DeleteEventAsync("gcal-event-999", It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.CancelOccurrenceAsync(occurrence.Id, new CancelSessionRequest("Trừ học phí"), CancellationToken.None);

        // Assert
        _gcalMock.Verify(g => g.DeleteEventAsync("gcal-event-999", It.IsAny<CancellationToken>()), Times.Once);
        
        var updated = await _db.ScheduleOccurrences.FindAsync(occurrence.Id);
        Assert.NotNull(updated);
        Assert.Equal("Đã hủy", updated.Status);
        Assert.Null(updated.GoogleEventId);
    }
}
