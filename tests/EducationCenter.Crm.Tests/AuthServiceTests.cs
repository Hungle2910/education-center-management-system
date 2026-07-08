using EducationCenter.Crm.Application.Auth;
using EducationCenter.Crm.Application.Common.Interfaces;
using EducationCenter.Crm.Domain.Identity;
using EducationCenter.Crm.Infrastructure.Auth;
using EducationCenter.Crm.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace EducationCenter.Crm.Tests;

public sealed class AuthServiceTests
{
    [Fact]
    public async Task LoginAsync_WithWrongPassword_ShouldReturnInvalidCredentialsMessage()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var passwordHasher = new Pbkdf2PasswordHasher();

        await using var dbContext = new ApplicationDbContext(options);
        dbContext.Users.Add(new User
        {
            Id = Guid.NewGuid(),
            Email = "admin@test.local",
            FullName = "Quản trị viên",
            PasswordHash = passwordHasher.Hash("MatKhauDung123!"),
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        });
        await dbContext.SaveChangesAsync();

        var authService = new AuthService(
            dbContext,
            passwordHasher,
            new FakeJwtTokenService(),
            NullLogger<AuthService>.Instance,
            new FakeAuditLogService());

        var exception = await Assert.ThrowsAsync<InvalidCredentialsException>(() =>
            authService.LoginAsync(
                new LoginRequest("admin@test.local", "sai-mat-khau"),
                CancellationToken.None));

        Assert.Equal("Tài khoản hoặc mật khẩu không chính xác.", exception.Message);
    }

    private sealed class FakeJwtTokenService : IJwtTokenService
    {
        public JwtTokenResult CreateToken(User user, IReadOnlyCollection<string> roles)
        {
            return new JwtTokenResult("token", DateTime.UtcNow.AddMinutes(5));
        }
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
}
