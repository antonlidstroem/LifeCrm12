using System.ComponentModel.DataAnnotations;
using LifeCrm.Core.Enums;

namespace LifeCrm.Application.Campaigns.DTOs;

public record CampaignListDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public CampaignStatus Status { get; init; }
    public decimal? BudgetGoal { get; init; }
    public decimal TotalRaised { get; init; }
    public decimal? ProgressPercent { get; init; }
    public DateOnly? StartDate { get; init; }
    public DateOnly? EndDate { get; init; }
    public int DonationCount { get; init; }
}

public record CampaignDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public CampaignStatus Status { get; init; }
    public decimal? BudgetGoal { get; init; }
    public decimal TotalRaised { get; init; }
    public decimal? ProgressPercent { get; init; }
    public DateOnly? StartDate { get; init; }
    public DateOnly? EndDate { get; init; }
    public string? Notes { get; init; }
    public int DonationCount { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? LastModifiedAt { get; init; }
}

public record CreateCampaignRequest
{
    [Required][MaxLength(200)] public string Name { get; init; } = string.Empty;
    [MaxLength(2000)] public string? Description { get; init; }
    [Range(0, 100_000_000)] public decimal? BudgetGoal { get; init; }
    public DateOnly? StartDate { get; init; }
    public DateOnly? EndDate { get; init; }
    public CampaignStatus Status { get; init; } = CampaignStatus.Active;
    [MaxLength(2000)] public string? Notes { get; init; }
}

public record UpdateCampaignRequest : CreateCampaignRequest
{
    [Required] public Guid Id { get; init; }
}

// ── Campaign-specific newsletter send request ───────────────────────────────
// NOTE: Preview/Result DTOs come from LifeCrm.Application.Newsletters.DTOs
// to avoid duplication. Only the campaign-scoped send request is defined here.
public record CampaignSendNewsletterRequest
{
    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.MaxLength(500)]
    public string Subject { get; init; } = string.Empty;

    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.MaxLength(100_000)]
    public string HtmlBody { get; init; } = string.Empty;

    [System.ComponentModel.DataAnnotations.MaxLength(500)]
    public string? TagFilter { get; init; }
}
