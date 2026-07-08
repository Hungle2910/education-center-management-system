using EducationCenter.Crm.Application.Notifications;
using Microsoft.AspNetCore.Authorization;

namespace EducationCenter.Crm.Api.Realtime;

[Authorize]
public sealed class CalendarHub : GroupedHub
{
    public CalendarHub(INotificationGroupResolver groupResolver)
        : base(groupResolver)
    {
    }
}
