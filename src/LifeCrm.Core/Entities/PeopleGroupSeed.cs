namespace LifeCrm.Core.Entities;

public class PeopleGroupSeed : BaseEntity
{
    public string JpCode      { get; set; } = string.Empty;
    public string Name        { get; set; } = string.Empty;
    public string Country     { get; set; } = string.Empty;
    public string? Language   { get; set; }
    public int? Population    { get; set; }
    public bool IsUnreached   { get; set; } = true;
    public string? Region     { get; set; }
}
