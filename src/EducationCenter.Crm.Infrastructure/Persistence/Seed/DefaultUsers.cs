using EducationCenter.Crm.Domain.Identity;

namespace EducationCenter.Crm.Infrastructure.Persistence.Seed;

/// <summary>
/// Tài liệu thông tin tài khoản mặc định hệ thống (seed trực tiếp qua SQL/script).
/// Email:    admin@test.local
/// Password: Admin@123
/// Hash:     PBKDF2-SHA256.100000.AAAAAAAAAAAAAAAAAAAAAA==.LQEQyPd1b2YtLYcAPUbXDCBCy2zLIeYZRRUE4jXQwOU=
/// </summary>
public static class DefaultUsers
{
    public static readonly Guid AdminUserId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

    public const string AdminEmail = "admin@test.local";

    /// <summary>
    /// PBKDF2-SHA256 hash của "Admin@123" với zero-salt, 100000 iterations.
    /// Dùng để reset password nếu cần.
    /// </summary>
    public const string AdminPasswordHash =
        "PBKDF2-SHA256.100000.AAAAAAAAAAAAAAAAAAAAAA==.LQEQyPd1b2YtLYcAPUbXDCBCy2zLIeYZRRUE4jXQwOU=";
}

