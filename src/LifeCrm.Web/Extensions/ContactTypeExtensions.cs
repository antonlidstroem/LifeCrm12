using LifeCrm.Core.Enums;
using MudBlazor;

namespace LifeCrm.Web.Extensions;

public static class ContactTypeExtensions
{
    public static Color ToMudColor(this ContactType type) => type switch
    {
        ContactType.Individual   => Color.Primary,
        ContactType.Organization => Color.Secondary,
        ContactType.Church       => Color.Tertiary,
        ContactType.Foundation   => Color.Info,
        _                        => Color.Default
    };
}
