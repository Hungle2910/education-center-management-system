using EducationCenter.Crm.Domain.Identity;

namespace EducationCenter.Crm.Application.Auth;

public interface IJwtTokenService
{
    JwtTokenResult CreateToken(User user, IReadOnlyCollection<string> roles);
}

public sealed record JwtTokenResult(string AccessToken, DateTime ExpiresAtUtc);
