using System.Security.Claims;
using EducationCenter.Crm.Domain.Identity;

namespace EducationCenter.Crm.Application.Notifications;

public sealed class NotificationGroupResolver : INotificationGroupResolver
{
    private static readonly string[] TeacherClaimTypes = ["teacherId", "TeacherId"];
    private static readonly string[] ParentClaimTypes = ["parentId", "ParentId"];
    private static readonly string[] StudentClaimTypes = ["studentId", "StudentId"];

    public IReadOnlyCollection<string> ResolveGroups(ClaimsPrincipal user)
    {
        var groups = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var role in user.FindAll(ClaimTypes.Role).Select(claim => claim.Value))
        {
            if (role is AppRoles.Admin or AppRoles.Staff)
            {
                groups.Add(NotificationGroupNames.ForRole(role));
            }
        }

        AddOwnershipGroups(user, groups);

        return groups
            .OrderBy(group => group, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static void AddOwnershipGroups(ClaimsPrincipal user, ISet<string> groups)
    {
        AddGroupFromClaims(user, TeacherClaimTypes, NotificationGroupNames.ForTeacher, groups);
        AddGroupFromClaims(user, ParentClaimTypes, NotificationGroupNames.ForParent, groups);
        AddGroupFromClaims(user, StudentClaimTypes, NotificationGroupNames.ForStudent, groups);
    }

    private static void AddGroupFromClaims(
        ClaimsPrincipal user,
        IEnumerable<string> claimTypes,
        Func<string, string> groupFactory,
        ISet<string> groups)
    {
        foreach (var claimType in claimTypes)
        {
            foreach (var value in user.FindAll(claimType).Select(claim => claim.Value))
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    groups.Add(groupFactory(value.Trim()));
                }
            }
        }
    }
}
