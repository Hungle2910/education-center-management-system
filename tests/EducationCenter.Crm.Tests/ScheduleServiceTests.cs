using System;
using System.Collections.Generic;
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

public sealed class ScheduleServiceTests
{
    private async Task<ApplicationDbContext> GetDbContextAsync()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var dbContext = new ApplicationDbContext(options);
        
        // Seed dummy data
        var teacher = new Teacher
        {
            Id = Guid.NewGuid(),
            FullName = "Giáo viên A",
            Email = "teacher@test.local",
            PhoneNumber = "84909123456",
            Subject = "Toán",
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };
        dbContext.Teachers.Add(teacher);

        var room = new Room
        {
            Id = Guid.NewGuid(),
            Name = "Phòng 101",
            Capacity = 15,
            CreatedAtUtc = DateTime.UtcNow
        };
        dbContext.Rooms.Add(room);

        var cls = new Class
        {
            Id = Guid.NewGuid(),
            Name = "Lớp 9A",
            Subject = "Toán",
            TeacherId = teacher.Id,
            MinStudents = 5,
            MaxStudents = 15,
            CreatedAtUtc = DateTime.UtcNow
        };
        dbContext.Classes.Add(cls);

        await dbContext.SaveChangesAsync();
        return dbContext;
    }

    [Fact]
    public async Task CreateScheduleAsync_WithInvalidTime_ShouldThrowArgumentException()
    {
        using var dbContext = await GetDbContextAsync();
        var service = new ScheduleService(dbContext);

        var request = new CreateScheduleRequest(
            Guid.NewGuid(),
            DayOfWeek.Monday,
            new TimeOnly(10, 0),
            new TimeOnly(9, 0), // Start after End
            Guid.NewGuid(),
            null);

        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            service.CreateScheduleAsync(request, CancellationToken.None));

        Assert.Equal("Thời gian bắt đầu phải trước thời gian kết thúc.", exception.Message);
    }

    [Fact]
    public async Task CreateScheduleAsync_WithRoomConflict_ShouldThrowInvalidOperationException()
    {
        using var dbContext = await GetDbContextAsync();
        var service = new ScheduleService(dbContext);

        var cls = await dbContext.Classes.FirstAsync();
        var room = await dbContext.Rooms.FirstAsync();

        // 1. Create first schedule
        await service.CreateScheduleAsync(
            new CreateScheduleRequest(cls.Id, DayOfWeek.Monday, new TimeOnly(8, 0), new TimeOnly(9, 30), room.Id, cls.TeacherId),
            CancellationToken.None);

        // 2. Try create second overlapping schedule in the same room
        var request = new CreateScheduleRequest(
            cls.Id,
            DayOfWeek.Monday,
            new TimeOnly(9, 0), // Overlaps 8:00 - 9:30
            new TimeOnly(10, 30),
            room.Id,
            cls.TeacherId);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateScheduleAsync(request, CancellationToken.None));

        Assert.Equal("Không thể lưu lịch. Lớp này đang trùng phòng hoặc trùng giáo viên. Vui lòng chọn thời gian khác.", exception.Message);
    }
}
