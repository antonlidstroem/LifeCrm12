using LifeCrm.Core.Enums;
using MudBlazor;

namespace LifeCrm.Web.Extensions;

public static class CampaignStatusExtensions
{
    public static Color ToMudColor(this CampaignStatus status) => status switch
    {
        CampaignStatus.Active    => Color.Success,
        CampaignStatus.Completed => Color.Info,
        CampaignStatus.Draft     => Color.Default,
        CampaignStatus.Paused    => Color.Warning,
        CampaignStatus.Cancelled => Color.Error,
        _                        => Color.Default
    };
}
