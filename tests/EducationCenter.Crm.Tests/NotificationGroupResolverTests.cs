using System.Security.Claims;
using EducationCenter.Crm.Application.Notifications;
using EducationCenter.Crm.Domain.Identity;

namespace EducationCenter.Crm.Tests;

public sealed class NotificationGroupResolverTests
{
    [Fact]
    public void ResolveGroups_WithRoleAndOwnershipClaims_ReturnsExpectedSignalRGroups()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(ClaimTypes.Role, AppRoles.Admin),
            new Claim(ClaimTypes.Role, AppRoles.Teacher),
            new Claim("teacherId", "teacher-123")
        ], "Test"));
        var resolver = new NotificationGroupResolver();

        var groups = resolver.ResolveGroups(user);

        Assert.Contains("admin", groups);
        Assert.Contains("teacher:teacher-123", groups);
    }
}
