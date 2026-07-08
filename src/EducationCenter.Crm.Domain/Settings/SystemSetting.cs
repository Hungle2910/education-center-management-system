namespace EducationCenter.Crm.Domain.Settings;

public sealed class SystemSetting
{
    public required string Key { get; set; }

    public required string Value { get; set; }

    public string? Description { get; set; }

    public bool IsSensitive { get; set; }

    public DateTime UpdatedAtUtc { get; set; }
}
