namespace LifeCrm.Core.Entities;

public class PeopleGroupReached : TenantEntity
{
    public Guid ReportId             { get; set; }
    public string JpCode             { get; set; } = string.Empty;
    public string PeopleGroupName    { get; set; } = string.Empty;
    public string Country            { get; set; } = string.Empty;
    public string? Language          { get; set; }
    public int? EstimatedReached     { get; set; }
    public string? Notes             { get; set; }
    public MissionReport? Report     { get; set; }
}
