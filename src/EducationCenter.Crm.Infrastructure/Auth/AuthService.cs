using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using EducationCenter.Crm.Application.Auth;
using EducationCenter.Crm.Domain.Identity;
using EducationCenter.Crm.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using EducationCenter.Crm.Application.Common.Interfaces;

namespace EducationCenter.Crm.Infrastructure.Auth;

public sealed class AuthService : IAuthService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ILogger<AuthService> _logger;
    private readonly IAuditLogService _auditLogService;

    public AuthService(
        ApplicationDbContext dbContext,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService,
        ILogger<AuthService> logger,
        IAuditLogService auditLogService)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
        _logger = logger;
        _auditLogService = auditLogService;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            _logger.LogWarning("Đăng nhập thất bại do thiếu email hoặc mật khẩu");
            throw new InvalidCredentialsException();
        }

        var email = request.Email.Trim();
        var user = await LoadUserByEmailAsync(email, cancellationToken);

        if (user is null ||
            !user.IsActive ||
            user.PasswordHash is null ||
            !_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Đăng nhập thất bại cho email {Email}", email);
            throw new InvalidCredentialsException();
        }

        var roles = GetRoleNames(user);
        var token = _jwtTokenService.CreateToken(user, roles);
        var responseUser = CreateUserResponse(user);

        _logger.LogInformation(
            "Đăng nhập thành công cho user {UserId} với email {Email}",
            user.Id,
            user.Email);

        await _auditLogService.LogAsync(
            user.Id,
            user.Email,
            user.FullName,
            "Đăng nhập",
            "User",
            user.Id.ToString(),
            "Đăng nhập hệ thống thành công",
            null,
            cancellationToken);

        return new LoginResponse(token.AccessToken, token.ExpiresAtUtc, responseUser);
    }

    public async Task<AuthUserResponse?> GetCurrentUserAsync(
        ClaimsPrincipal principal,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId(principal);
        if (userId is null)
        {
            return null;
        }

        var user = await LoadUserByIdAsync(userId.Value, cancellationToken);
        return user is null || !user.IsActive ? null : CreateUserResponse(user);
    }

    public Task LogoutAsync(ClaimsPrincipal principal, CancellationToken cancellationToken)
    {
        var userId = GetUserId(principal);
        _logger.LogInformation("Đăng xuất cho user {UserId}", userId);
        return Task.CompletedTask;
    }

    private async Task<User?> LoadUserByEmailAsync(string email, CancellationToken cancellationToken)
    {
        return await _dbContext.Users
            .Include(user => user.UserRoles)
                .ThenInclude(userRole => userRole.Role)
                    .ThenInclude(role => role.RolePermissions)
                        .ThenInclude(rolePermission => rolePermission.Permission)
            .FirstOrDefaultAsync(user => user.Email == email, cancellationToken);
    }

    private async Task<User?> LoadUserByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _dbContext.Users
            .Include(user => user.UserRoles)
                .ThenInclude(userRole => userRole.Role)
                    .ThenInclude(role => role.RolePermissions)
                        .ThenInclude(rolePermission => rolePermission.Permission)
            .AsNoTracking()
            .FirstOrDefaultAsync(user => user.Id == id, cancellationToken);
    }

    private static AuthUserResponse CreateUserResponse(User user)
    {
        return new AuthUserResponse(
            user.Id,
            user.Email,
            user.FullName,
            GetRoleNames(user),
            GetPermissionCodes(user));
    }

    private static IReadOnlyCollection<string> GetRoleNames(User user)
    {
        return user.UserRoles
            .Select(userRole => userRole.Role.Name)
            .OrderBy(role => role)
            .ToArray();
    }

    private static IReadOnlyCollection<string> GetPermissionCodes(User user)
    {
        return user.UserRoles
            .SelectMany(userRole => userRole.Role.RolePermissions)
            .Select(rolePermission => rolePermission.Permission.Code)
            .Distinct()
            .OrderBy(permission => permission)
            .ToArray();
    }

    private static Guid? GetUserId(ClaimsPrincipal principal)
    {
        var value = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        return Guid.TryParse(value, out var userId) ? userId : null;
    }
}
