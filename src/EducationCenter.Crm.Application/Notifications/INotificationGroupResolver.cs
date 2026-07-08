using System.Security.Claims;

namespace EducationCenter.Crm.Application.Notifications;

public interface INotificationGroupResolver
{
    IReadOnlyCollection<string> ResolveGroups(ClaimsPrincipal user);
}
