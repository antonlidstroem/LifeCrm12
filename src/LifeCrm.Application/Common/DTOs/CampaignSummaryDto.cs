namespace LifeCrm.Application.Common.DTOs;
public record CampaignSummaryDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public decimal? BudgetGoal { get; init; }
    public decimal TotalRaised { get; init; }
    public decimal? ProgressPercent { get; init; }
}
