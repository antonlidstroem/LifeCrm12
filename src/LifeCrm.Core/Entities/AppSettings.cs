namespace LifeCrm.Core.Entities;

/// <summary>System-wide key-value settings. Not tenant-scoped.</summary>
public class AppSettings : BaseEntity
{
    public string Key   { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}
