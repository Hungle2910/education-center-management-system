using EducationCenter.Crm.Domain.Identity;

namespace EducationCenter.Crm.Application.Notifications;

public static class NotificationGroupNames
{
    public const string Admin = "admin";
    public const string Staff = "staff";

    public static string ForRole(string role)
    {
        return role switch
        {
            AppRoles.Admin => Admin,
            AppRoles.Staff => Staff,
            _ => role.Trim().ToLowerInvariant()
        };
    }

    public static string ForTeacher(string teacherId) => $"teacher:{teacherId}";

    public static string ForParent(string parentId) => $"parent:{parentId}";

    public static string ForStudent(string studentId) => $"student:{studentId}";
}
