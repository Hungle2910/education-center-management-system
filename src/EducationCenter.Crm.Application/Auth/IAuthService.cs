using System.Security.Claims;

namespace EducationCenter.Crm.Application.Auth;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken);

    Task<AuthUserResponse?> GetCurrentUserAsync(ClaimsPrincipal principal, CancellationToken cancellationToken);

    Task LogoutAsync(ClaimsPrincipal principal, CancellationToken cancellationToken);
}
