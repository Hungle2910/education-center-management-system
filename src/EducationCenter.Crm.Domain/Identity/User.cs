namespace EducationCenter.Crm.Domain.Identity;

public sealed class User
{
    public Guid Id { get; set; }

    public required string Email { get; set; }

    public required string FullName { get; set; }

    public string? PhoneNumber { get; set; }

    public string? PasswordHash { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? UpdatedAtUtc { get; set; }

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
