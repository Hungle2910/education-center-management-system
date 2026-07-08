using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using EducationCenter.Crm.Application.Common.Interfaces;
using EducationCenter.Crm.Infrastructure.Logging;
using EducationCenter.Crm.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EducationCenter.Crm.Tests;

public sealed class AuditLogServiceTests
{
    private readonly DbContextOptions<ApplicationDbContext> _dbOptions;

    public AuditLogServiceTests()
    {
        _dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public async Task LogAsync_SavesRecordToDatabaseCorrectly()
    {
        // Arrange
        await using var dbContext = new ApplicationDbContext(_dbOptions);
        var httpContextAccessor = new FakeHttpContextAccessor();
        var service = new AuditLogService(dbContext, httpContextAccessor);

        // Act
        await service.LogAsync(
            Guid.NewGuid(),
            "staff@test.local",
            "Nhân viên A",
            "Tạo lớp học",
            "Class",
            "123",
            "Lớp học Toán 10 mới được tạo",
            "192.168.1.1",
            CancellationToken.None);

        // Assert
        var logs = await dbContext.AuditLogs.ToListAsync();
        Assert.Single(logs);
        Assert.Equal("Tạo lớp học", logs[0].Action);
        Assert.Equal("staff@test.local", logs[0].UserEmail);
        Assert.Equal("Nhân viên A", logs[0].UserFullName);
        Assert.Equal("Class", logs[0].EntityType);
        Assert.Equal("123", logs[0].EntityId);
        Assert.Equal("192.168.1.1", logs[0].IpAddress);
    }

    [Fact]
    public async Task LogAsync_ResolvesUserFromHttpContext_WhenArgumentsAreNull()
    {
        // Arrange
        await using var dbContext = new ApplicationDbContext(_dbOptions);
        
        var userId = Guid.NewGuid();
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Email, "admin@test.local"),
            new Claim(ClaimTypes.Name, "Quản trị viên")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        
        var httpContext = new DefaultHttpContext { User = principal };
        httpContext.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("127.0.0.1");
        
        var httpContextAccessor = new FakeHttpContextAccessor { HttpContext = httpContext };
        var service = new AuditLogService(dbContext, httpContextAccessor);

        // Act
        await service.LogAsync(
            null, null, null,
            "Điểm danh lớp học",
            null, null,
            "Hoàn tất điểm danh",
            null,
            CancellationToken.None);

        // Assert
        var log = await dbContext.AuditLogs.SingleAsync();
        Assert.Equal("Điểm danh lớp học", log.Action);
        Assert.Equal(userId, log.UserId);
        Assert.Equal("admin@test.local", log.UserEmail);
        Assert.Equal("Quản trị viên", log.UserFullName);
        Assert.Equal("127.0.0.1", log.IpAddress);
    }

    private sealed class FakeHttpContextAccessor : IHttpContextAccessor
    {
        public HttpContext? HttpContext { get; set; }
    }
}
