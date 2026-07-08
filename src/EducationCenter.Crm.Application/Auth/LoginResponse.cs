namespace EducationCenter.Crm.Application.Auth;

public sealed record LoginResponse(
    string AccessToken,
    DateTime ExpiresAtUtc,
    AuthUserResponse User);
