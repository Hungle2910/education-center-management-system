using EducationCenter.Crm.Application.Notifications;
using Microsoft.AspNetCore.Authorization;

namespace EducationCenter.Crm.Api.Realtime;

[Authorize]
public sealed class PaymentHub : GroupedHub
{
    public PaymentHub(INotificationGroupResolver groupResolver)
        : base(groupResolver)
    {
    }
}
