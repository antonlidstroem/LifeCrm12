using LifeCrm.Core.Enums;

namespace LifeCrm.Core.Entities;

public class MissionReport : TenantEntity
{
    public string Title               { get; set; } = string.Empty;
    public DateOnly ReportDate        { get; set; }
    public string? Location           { get; set; }
    public ReportStatus Status        { get; set; } = ReportStatus.Draft;
    public ReportLanguage Language    { get; set; } = ReportLanguage.English;
    public Guid? CampaignId           { get; set; }
    public Guid? ProjectId            { get; set; }
    public Guid AuthorUserId          { get; set; }
    public string AuthorName          { get; set; } = string.Empty;
    public string HtmlBody            { get; set; } = string.Empty;
    public int? EventsHeld            { get; set; }
    public int? TotalAttendees        { get; set; }
    public int? NewContacts           { get; set; }
    public int? MaterialsDistributed  { get; set; }
    public DateTimeOffset? SubmittedAt { get; set; }
    public DateTimeOffset? ApprovedAt  { get; set; }
    public string? ApprovedBy         { get; set; }
    public string? ReviewComment      { get; set; }
    public Campaign? Campaign         { get; set; }
    public Project? Project           { get; set; }
    public ICollection<DecisionCount>     Decisions    { get; set; } = new List<DecisionCount>();
    public ICollection<PeopleGroupReached> PeopleGroups { get; set; } = new List<PeopleGroupReached>();
    public ICollection<PrayerPoint>       PrayerPoints { get; set; } = new List<PrayerPoint>();
}
