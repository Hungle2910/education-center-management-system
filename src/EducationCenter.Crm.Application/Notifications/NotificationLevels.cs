namespace EducationCenter.Crm.Application.Notifications;

public static class NotificationLevels
{
    public const string Information = "Thông tin";
    public const string Success = "Thành công";
    public const string Warning = "Cảnh báo";
    public const string Critical = "Khẩn cấp";

    public static readonly IReadOnlySet<string> All = new HashSet<string>
    {
        Information,
        Success,
        Warning,
        Critical
    };
}
