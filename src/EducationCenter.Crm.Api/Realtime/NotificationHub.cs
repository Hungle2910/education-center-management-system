using EducationCenter.Crm.Application.Notifications;
using Microsoft.AspNetCore.Authorization;

namespace EducationCenter.Crm.Api.Realtime;

[Authorize]
public sealed class NotificationHub : GroupedHub
{
    public NotificationHub(INotificationGroupResolver groupResolver)
        : base(groupResolver)
    {
    }
}
