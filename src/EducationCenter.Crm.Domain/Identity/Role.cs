namespace EducationCenter.Crm.Domain.Identity;

public sealed class Role
{
    public Guid Id { get; set; }

    public required string Name { get; set; }

    public required string DisplayName { get; set; }

    public required string Description { get; set; }

    public bool IsSystemRole { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; }

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
