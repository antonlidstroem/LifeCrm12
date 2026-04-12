using LifeCrm.Core.Enums;

namespace LifeCrm.Core.Entities;

public class PrayerPoint : TenantEntity
{
    public Guid? ReportId            { get; set; }
    public string Title              { get; set; } = string.Empty;
    public string Detail             { get; set; } = string.Empty;
    public PrayerStatus Status       { get; set; } = PrayerStatus.Active;
    public DateTimeOffset? AnsweredAt  { get; set; }
    public string? AnsweredNote      { get; set; }
    public string CreatedBy          { get; set; } = "system";
    public MissionReport? Report     { get; set; }
}
