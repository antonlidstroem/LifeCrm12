namespace LifeCrm.Application.Common.DTOs;
public record DashboardDto
{
    public decimal DonationsThisMonth { get; init; }
    public decimal DonationsLastMonth { get; init; }
    public decimal DonationsMoMChangePercent { get; init; }
    public int TotalContacts { get; init; }
    public int NewContactsThisMonth { get; init; }
    public int ActiveCampaigns { get; init; }
    public IReadOnlyList<CampaignSummaryDto> TopCampaigns { get; init; } = Array.Empty<CampaignSummaryDto>();
    public IReadOnlyList<ActivityFeedItemDto> RecentActivity { get; init; } = Array.Empty<ActivityFeedItemDto>();
}
