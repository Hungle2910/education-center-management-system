namespace EducationCenter.Crm.Application.Notifications;

public interface INotificationService
{
    Task SendToUserAsync(string userId, NotificationDto notification, CancellationToken cancellationToken = default);

    Task SendToRoleAsync(string role, NotificationDto notification, CancellationToken cancellationToken = default);

    Task SendToGroupAsync(string groupName, NotificationDto notification, CancellationToken cancellationToken = default);
}
