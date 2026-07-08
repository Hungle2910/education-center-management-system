namespace EducationCenter.Crm.Application.Notifications;

public sealed record NotificationDto(
    string Title,
    string Message,
    string Level,
    DateTime CreatedAt);
