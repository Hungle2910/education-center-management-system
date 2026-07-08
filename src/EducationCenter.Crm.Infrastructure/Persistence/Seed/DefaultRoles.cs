using EducationCenter.Crm.Domain.Identity;

namespace EducationCenter.Crm.Infrastructure.Persistence.Seed;

public static class DefaultRoles
{
    private static readonly DateTime SeededAtUtc = new(2026, 7, 7, 0, 0, 0, DateTimeKind.Utc);

    public static readonly Guid AdminId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid StaffId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static readonly Guid TeacherId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    public static readonly Guid ParentId = Guid.Parse("44444444-4444-4444-4444-444444444444");
    public static readonly Guid StudentId = Guid.Parse("55555555-5555-5555-5555-555555555555");

    public static IReadOnlyCollection<Role> All { get; } =
    [
        new Role
        {
            Id = AdminId,
            Name = "Admin",
            DisplayName = "Quản trị viên",
            Description = "Toàn quyền hệ thống",
            IsSystemRole = true,
            CreatedAtUtc = SeededAtUtc
        },
        new Role
        {
            Id = StaffId,
            Name = "Staff",
            DisplayName = "Nhân viên",
            Description = "Quản lý vận hành",
            IsSystemRole = true,
            CreatedAtUtc = SeededAtUtc
        },
        new Role
        {
            Id = TeacherId,
            Name = "Teacher",
            DisplayName = "Giáo viên",
            Description = "Quản lý lớp đang dạy",
            IsSystemRole = true,
            CreatedAtUtc = SeededAtUtc
        },
        new Role
        {
            Id = ParentId,
            Name = "Parent",
            DisplayName = "Phụ huynh",
            Description = "Theo dõi lịch học và học phí của con",
            IsSystemRole = true,
            CreatedAtUtc = SeededAtUtc
        },
        new Role
        {
            Id = StudentId,
            Name = "Student",
            DisplayName = "Học sinh",
            Description = "Theo dõi lịch học cá nhân",
            IsSystemRole = true,
            CreatedAtUtc = SeededAtUtc
        }
    ];
}
