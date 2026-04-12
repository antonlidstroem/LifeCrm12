using System.ComponentModel.DataAnnotations;
using LifeCrm.Core.Enums;

namespace LifeCrm.Application.Reports.DTOs;

public record MissionReportListDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public DateOnly ReportDate { get; init; }
    public string? Location { get; init; }
    public ReportStatus Status { get; init; }
    public ReportLanguage Language { get; init; }
    public string AuthorName { get; init; } = string.Empty;
    public string? CampaignName { get; init; }
    public string? ProjectName { get; init; }
    public int DecisionTotal { get; init; }
    public int PrayerCount { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? SubmittedAt { get; init; }
    public DateTimeOffset? ApprovedAt { get; init; }
}

public record MissionReportDetailDto : MissionReportListDto
{
    public string HtmlBody { get; init; } = string.Empty;
    public Guid? CampaignId { get; init; }
    public Guid? ProjectId  { get; init; }
    public int? EventsHeld { get; init; }
    public int? TotalAttendees { get; init; }
    public int? NewContacts { get; init; }
    public int? MaterialsDistributed { get; init; }
    public string? ReviewComment { get; init; }
    public string? ApprovedBy { get; init; }
    public IReadOnlyList<DecisionCountDto>      Decisions    { get; init; } = Array.Empty<DecisionCountDto>();
    public IReadOnlyList<PeopleGroupReachedDto> PeopleGroups { get; init; } = Array.Empty<PeopleGroupReachedDto>();
    public IReadOnlyList<PrayerPointDto>        PrayerPoints { get; init; } = Array.Empty<PrayerPointDto>();
}

public record DecisionCountDto
{
    public Guid Id { get; init; }
    public DecisionType DecisionType { get; init; }
    public int Count { get; init; }
}

public record PeopleGroupReachedDto
{
    public Guid Id { get; init; }
    public string JpCode { get; init; } = string.Empty;
    public string PeopleGroupName { get; init; } = string.Empty;
    public string Country { get; init; } = string.Empty;
    public string? Language { get; init; }
    public int? EstimatedReached { get; init; }
    public string? Notes { get; init; }
}

public record PrayerPointDto
{
    public Guid Id { get; init; }
    public Guid? ReportId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Detail { get; init; } = string.Empty;
    public PrayerStatus Status { get; init; }
    public DateTimeOffset? AnsweredAt { get; init; }
    public string? AnsweredNote { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}

public record PeopleGroupSearchDto
{
    public string JpCode { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Country { get; init; } = string.Empty;
    public string? Language { get; init; }
    public int? Population { get; init; }
    public bool IsUnreached { get; init; }
}

public record CreateReportRequest
{
    [Required][MaxLength(300)] public string Title { get; init; } = string.Empty;
    [Required] public DateOnly ReportDate { get; init; } = DateOnly.FromDateTime(DateTime.Today);
    [MaxLength(200)] public string? Location { get; init; }
    public Guid? CampaignId { get; init; }
    public Guid? ProjectId  { get; init; }
    public ReportLanguage Language { get; init; } = ReportLanguage.English;
}

public record UpdateReportRequest
{
    [Required] public Guid Id { get; init; }
    [Required][MaxLength(300)] public string Title { get; init; } = string.Empty;
    [Required] public DateOnly ReportDate { get; init; }
    [MaxLength(200)] public string? Location { get; init; }
    public Guid? CampaignId { get; init; }
    public Guid? ProjectId  { get; init; }
    public ReportLanguage Language { get; init; } = ReportLanguage.English;
    [MaxLength(500_000)] public string HtmlBody { get; init; } = string.Empty;
    public int? EventsHeld { get; init; }
    public int? TotalAttendees { get; init; }
    public int? NewContacts { get; init; }
    public int? MaterialsDistributed { get; init; }
}

public record UpsertDecisionCountRequest
{
    [Required] public DecisionType DecisionType { get; init; }
    [Range(0, 999_999)] public int Count { get; init; }
}

public record AddPeopleGroupRequest
{
    [Required][MaxLength(20)]  public string JpCode { get; init; } = string.Empty;
    [Required][MaxLength(200)] public string PeopleGroupName { get; init; } = string.Empty;
    [Required][MaxLength(100)] public string Country { get; init; } = string.Empty;
    [MaxLength(100)] public string? Language { get; init; }
    [Range(0, 10_000_000)] public int? EstimatedReached { get; init; }
    [MaxLength(2000)] public string? Notes { get; init; }
}

public record CreatePrayerPointRequest
{
    public Guid? ReportId { get; init; }
    [Required][MaxLength(300)] public string Title { get; init; } = string.Empty;
    [Required][MaxLength(5000)] public string Detail { get; init; } = string.Empty;
}

public record MarkAnsweredRequest
{
    [MaxLength(2000)] public string? AnsweredNote { get; init; }
}

public record ReturnForRevisionRequest
{
    [Required][MaxLength(2000)] public string Comment { get; init; } = string.Empty;
}

public record AnsweredPrayerWidgetDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? AnsweredNote { get; init; }
    public DateTimeOffset AnsweredAt { get; init; }
    public string? ReportTitle { get; init; }
    public Guid? ReportId { get; init; }
}
