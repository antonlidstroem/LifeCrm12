// src/LifeCrm.Web/Theme/LifeCrmTheme.cs
using MudBlazor;

namespace LifeCrm.Web.Theme;

public static class LifeCrmTheme
{
    public static MudTheme Create() => new()
    {
        PaletteDark = new PaletteDark
        {
            Background          = "#0C1520",
            BackgroundGray      = "#111C2B",
            Surface             = "#172032",
            DrawerBackground    = "#111C2B",
            AppbarBackground    = "#111C2B",
            AppbarText          = "#C8D8E8",

            Primary             = "#2E8EFF",
            PrimaryContrastText = "#FFFFFF",
            PrimaryDarken       = "#1A6FD4",
            PrimaryLighten      = "#60AAFF",

            Secondary             = "#2D3F55",
            SecondaryContrastText = "#C8D8E8",

            Success             = "#22C55E",
            SuccessContrastText = "#FFFFFF",
            Warning             = "#F59E0B",
            WarningContrastText = "#0C1520",
            Error               = "#EF4444",
            ErrorContrastText   = "#FFFFFF",
            Info                = "#38BDF8",
            InfoContrastText    = "#FFFFFF",

            TextPrimary         = "#E8F0F8",
            TextSecondary       = "#7A95B0",
            TextDisabled        = "#3D5270",

            Divider             = "#1F3048",
            DividerLight        = "#253A52",
            TableLines          = "#1A2D42",
            TableHover          = "#192A3E",
            TableStriped        = "#152438",

            LinesDefault        = "#2D4060",
            LinesInputs         = "#2D4060",

            ActionDefault            = "#7A95B0",
            ActionDisabled           = "#3D5270",
            ActionDisabledBackground = "#172032",

            DrawerText          = "#C8D8E8",
            DrawerIcon          = "#7A95B0",

            OverlayDark         = "rgba(8, 14, 24, 0.80)",
            OverlayLight        = "rgba(8, 14, 24, 0.60)",

            Black               = "#060D18",
            White               = "#E8F0F8",
            Dark                = "#0C1520",
            DarkContrastText    = "#E8F0F8",
        },

        Typography = new Typography
        {
            Default = new DefaultTypography
            {
                FontFamily    = new[] { "DM Sans", "system-ui", "sans-serif" },
                FontSize      = "0.875rem",
                FontWeight    = "400",
                LineHeight    = "1.6",
                LetterSpacing = "0.01em",
            },
            H1 = new H1Typography
            {
                FontFamily    = new[] { "DM Sans", "system-ui", "sans-serif" },
                FontSize      = "2rem",
                FontWeight    = "600",
                LineHeight    = "1.25",
                LetterSpacing = "-0.02em",
            },
            H2 = new H2Typography
            {
                FontFamily    = new[] { "DM Sans", "system-ui", "sans-serif" },
                FontSize      = "1.5rem",
                FontWeight    = "600",
                LineHeight    = "1.3",
                LetterSpacing = "-0.015em",
            },
            H3 = new H3Typography
            {
                FontFamily    = new[] { "DM Sans", "system-ui", "sans-serif" },
                FontSize      = "1.25rem",
                FontWeight    = "600",
                LineHeight    = "1.35",
                LetterSpacing = "-0.01em",
            },
            H4 = new H4Typography
            {
                FontFamily = new[] { "DM Sans", "system-ui", "sans-serif" },
                FontSize   = "1.125rem",
                FontWeight = "600",
                LineHeight = "1.4",
            },
            H5 = new H5Typography { FontSize = "1rem",    FontWeight = "600" },
            H6 = new H6Typography { FontSize = "0.75rem", FontWeight = "600", LetterSpacing = "0.08em" },
            Body1 = new Body1Typography
            {
                FontSize   = "0.875rem",
                FontWeight = "400",
                LineHeight = "1.6",
            },
            Body2 = new Body2Typography
            {
                FontSize   = "0.8125rem",
                FontWeight = "400",
                LineHeight = "1.5",
            },
            Button = new ButtonTypography
            {
                FontFamily    = new[] { "DM Sans", "system-ui", "sans-serif" },
                FontSize      = "0.8125rem",
                FontWeight    = "500",
                LetterSpacing = "0.03em",
                TextTransform = "none",
            },
            Caption = new CaptionTypography
            {
                FontSize      = "0.6875rem",
                FontWeight    = "400",
                LetterSpacing = "0.04em",
            },
            Overline = new OverlineTypography
            {
                FontSize      = "0.6875rem",
                FontWeight    = "600",
                LetterSpacing = "0.1em",
            },
            Subtitle1 = new Subtitle1Typography { FontSize = "0.9375rem", FontWeight = "500" },
            Subtitle2 = new Subtitle2Typography
            {
                FontSize      = "0.75rem",
                FontWeight    = "600",
                LetterSpacing = "0.06em",
            },
        },

        LayoutProperties = new LayoutProperties
        {
            DefaultBorderRadius = "6px",
            DrawerWidthLeft     = "240px",
            DrawerMiniWidthLeft = "68px",
            AppbarHeight        = "52px",
        },

        ZIndex = new ZIndex
        {
            Drawer   = 1200,
            AppBar   = 1100,
            Popover  = 1300,
            Dialog   = 1400,
            Snackbar = 1500,
        },

        Shadows = new Shadow
        {
            Elevation = Enumerable.Range(0, 25).Select(i => i == 0
                ? "none"
                : $"0 {i * 4}px {i * 12}px rgba(0,0,0,{Math.Min(0.4 + i * 0.02, 0.97):F2})")
                .ToArray()
        }
    };
}
