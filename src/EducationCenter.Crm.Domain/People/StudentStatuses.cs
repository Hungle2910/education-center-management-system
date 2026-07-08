namespace EducationCenter.Crm.Domain.People;

public static class StudentStatuses
{
    public const string Trial = "Học thử";
    public const string Active = "Đang học";
    public const string Paused = "Tạm nghỉ";
    public const string Reserved = "Bảo lưu";
    public const string Transferred = "Chuyển lớp";
    public const string Left = "Đã nghỉ";
    public const string Completed = "Hoàn thành";

    public static readonly IReadOnlySet<string> All = new HashSet<string>
    {
        Trial,
        Active,
        Paused,
        Reserved,
        Transferred,
        Left,
        Completed
    };
}
