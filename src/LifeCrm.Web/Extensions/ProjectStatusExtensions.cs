using LifeCrm.Core.Enums;
using MudBlazor;

namespace LifeCrm.Web.Extensions;

public static class ProjectStatusExtensions
{
    public static Color ToMudColor(this ProjectStatus status) => status switch
    {
        ProjectStatus.Planning  => Color.Default,
        ProjectStatus.Active    => Color.Success,
        ProjectStatus.Completed => Color.Info,
        ProjectStatus.OnHold    => Color.Warning,
        ProjectStatus.Archived  => Color.Default,
        _                       => Color.Default
    };
}
