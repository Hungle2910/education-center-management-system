namespace EducationCenter.Crm.Domain.Classes;

public static class ClassStatuses
{
    public const string Upcoming = "Sắp khai giảng";
    public const string Active = "Đang học";
    public const string Recruiting = "Cần tuyển thêm";
    public const string Paused = "Tạm dừng";
    public const string Completed = "Đã kết thúc";
    public const string Cancelled = "Đã hủy";

    public static readonly IReadOnlySet<string> All = new HashSet<string>
    {
        Upcoming,
        Active,
        Recruiting,
        Paused,
        Completed,
        Cancelled
    };
}
