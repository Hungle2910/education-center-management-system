using System;

namespace EducationCenter.Crm.Domain.People;

public static class LeadStatuses
{
    public const string New = "Mới quan tâm";
    public const string Consulted = "Đã tư vấn";
    public const string TrialScheduled = "Đã hẹn học thử";
    public const string TrialCompleted = "Đã học thử";
    public const string Registered = "Đã đăng ký";
    public const string NotRegistered = "Không đăng ký";
    public const string RecareNeeded = "Cần chăm sóc lại";
}

public class Lead
{
    public Guid Id { get; set; }
    public string StudentName { get; set; } = null!;
    public string? ParentName { get; set; }
    public string ParentPhone { get; set; } = null!;
    public string? Email { get; set; }
    public string? Source { get; set; }
    public string Status { get; set; } = LeadStatuses.New;
    public string? Notes { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}

public class TrialSession
{
    public Guid Id { get; set; }
    public Guid LeadId { get; set; }
    public Lead? Lead { get; set; }
    public Guid ClassId { get; set; }
    public EducationCenter.Crm.Domain.Classes.Class? Class { get; set; }
    public DateOnly TrialDate { get; set; }
    public Guid? TeacherId { get; set; }
    public Teacher? Teacher { get; set; }
    public string? Feedback { get; set; }
    public string? Result { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}

public class ParentCareLog
{
    public Guid Id { get; set; }
    public Guid? ParentId { get; set; }
    public Parent? Parent { get; set; }
    public Guid? LeadId { get; set; }
    public Lead? Lead { get; set; }
    public Guid StaffId { get; set; }
    public string ContactType { get; set; } = null!; // Đã gọi, Nhắn Zalo, Gửi học phí, Hẹn đóng tiền, Phản hồi lịch học, Cần chăm sóc lại
    public string Notes { get; set; } = null!;
    public DateTime LoggedAtUtc { get; set; } = DateTime.UtcNow;
}
