using EducationCenter.Crm.Application.Notifications;
using Microsoft.AspNetCore.SignalR;

namespace EducationCenter.Crm.Api.Realtime;

public sealed class SignalRNotificationService : INotificationService
{
    private readonly IHubContext<NotificationHub> _notificationHubContext;

    public SignalRNotificationService(IHubContext<NotificationHub> notificationHubContext)
    {
        _notificationHubContext = notificationHubContext;
    }

    public Task SendToUserAsync(
        string userId,
        NotificationDto notification,
        CancellationToken cancellationToken = default)
    {
        return _notificationHubContext.Clients
            .User(userId)
            .SendAsync(NotificationHubEvents.ReceiveNotification, notification, cancellationToken);
    }

    public Task SendToRoleAsync(
        string role,
        NotificationDto notification,
        CancellationToken cancellationToken = default)
    {
        return SendToGroupAsync(NotificationGroupNames.ForRole(role), notification, cancellationToken);
    }

    public Task SendToGroupAsync(
        string groupName,
        NotificationDto notification,
        CancellationToken cancellationToken = default)
    {
        return _notificationHubContext.Clients
            .Group(groupName)
            .SendAsync(NotificationHubEvents.ReceiveNotification, notification, cancellationToken);
    }
}
