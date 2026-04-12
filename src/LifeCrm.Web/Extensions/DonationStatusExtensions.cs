using LifeCrm.Core.Enums;
using MudBlazor;

namespace LifeCrm.Web.Extensions;

public static class DonationStatusExtensions
{
    public static Color ToMudColor(this DonationStatus status) => status switch
    {
        DonationStatus.Confirmed => Color.Success,
        DonationStatus.Pending   => Color.Warning,
        DonationStatus.Refunded  => Color.Info,
        DonationStatus.Voided    => Color.Error,
        _                        => Color.Default
    };
}
