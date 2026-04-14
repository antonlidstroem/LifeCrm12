using System.ComponentModel.DataAnnotations;
using LifeCrm.Core.Enums;

namespace LifeCrm.Application.Campaigns.DTOs;

public record CampaignListDto
{
    public Guid   Id              { get; init; }
    public string Name            { get; init; } = string.Empty;
    public CampaignStatus Status  { get; init; }
    public decimal? BudgetGoal   { get; init; }
    public decimal TotalRaised   { get; init; }
    public decimal? ProgressPercent { get; init; }
    public DateOnly? StartDate   { get; init; }
    public DateOnly? EndDate     { get; init; }
    public int DonationCount     { get; init; }
    public Guid? ProjectId       { get; init; }
    public string ProjectName    { get; init; } = string.Empty;
}

public record CampaignDto
{
    public Guid   Id              { get; init; }
    public string Name            { get; init; } = string.Empty;
    public string? Description   { get; init; }
    public CampaignStatus Status  { get; init; }
    public decimal? BudgetGoal   { get; init; }
    public decimal TotalRaised   { get; init; }
    public decimal? ProgressPercent { get; init; }
    public DateOnly? StartDate   { get; init; }
    public DateOnly? EndDate     { get; init; }
    public string? Notes         { get; init; }
    public int DonationCount     { get; init; }
    public Guid? ProjectId       { get; init; }
    public string ProjectName    { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt     { get; init; }
    public DateTimeOffset? LastModifiedAt { get; init; }
}

public record CreateCampaignRequest
{
    [Required][MaxLength(200)] public string Name { get; init; } = string.Empty;
    [MaxLength(2000)]          public string? Description { get; init; }
    [Range(0, 100_000_000)]    public decimal? BudgetGoal { get; init; }
    public DateOnly? StartDate { get; init; }
    public DateOnly? EndDate   { get; init; }
    public CampaignStatus Status { get; init; } = CampaignStatus.Active;
    [MaxLength(2000)] public string? Notes { get; init; }

    /// <summary>
    /// The project this campaign belongs to.
    /// Required by the application layer — every campaign must be under a project.
    /// Nullable in the DTO to gracefully handle missing values with validation error.
    /// </summary>
    public Guid? ProjectId { get; init; }
}

public record UpdateCampaignRequest : CreateCampaignRequest
{
    [Required] public Guid Id { get; init; }
}

// ── Campaign newsletter send request ────────────────────────────────────────
public record CampaignSendNewsletterRequest
{
    [Required][MaxLength(500)]    public string Subject   { get; init; } = string.Empty;
    [Required][MaxLength(100_000)] public string HtmlBody { get; init; } = string.Empty;
    [MaxLength(500)]               public string? TagFilter { get; init; }
}
