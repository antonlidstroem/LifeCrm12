using MudBlazor;

namespace LifeCrm.Web.Components.Layout;

/// <summary>
/// LifeCrm custom MudBlazor theme.
/// Colours: deep teal brand, neutral greys.
/// Typography: Inter font throughout.
/// Shape: rounded corners (radius 10px default).
/// Elevation: flatter than MudBlazor defaults — enterprise SaaS aesthetic.
/// </summary>
public static class AppTheme
{
    public static readonly MudTheme Theme = new()
    {
        // ── Palette ─────────────────────────────────────────────────────────
        PaletteLight = new PaletteLight
        {
            Primary         = "#0F5B6B",
            PrimaryDarken   = "#093F4A",
            PrimaryLighten  = "#28A0B8",
            PrimaryContrastText = "#FFFFFF",

            Secondary       = "#3D5A80",
            SecondaryContrastText = "#FFFFFF",

            Tertiary        = "#2D6A4F",
            TertiaryContrastText = "#FFFFFF",

            Success         = "#2E7D52",
            Warning         = "#B45309",
            Error           = "#B91C1C",
            Info            = "#1D5FAD",

            Background      = "#F7F8FA",
            Surface         = "#FFFFFF",
            DrawerBackground = "#0D2B32",
            DrawerText      = "rgba(255,255,255,0.72)",
            DrawerIcon      = "rgba(255,255,255,0.55)",

            AppbarBackground = "#FFFFFF",
            AppbarText      = "#161B22",

            TextPrimary     = "#161B22",
            TextSecondary   = "#57606A",
            TextDisabled    = "#8B949E",

            ActionDefault   = "#57606A",
            ActionDisabled  = "#C6CDD4",
            ActionDisabledBackground = "#F0F2F5",

            LinesDefault    = "#E1E4E8",
            LinesInputs     = "#C6CDD4",
            TableLines      = "#E1E4E8",
            TableStriped    = "#F7F8FA",
            TableHover      = "#F0FAFB",

            Divider         = "#E1E4E8",
            DividerLight    = "#F0F2F5",

            GrayDefault     = "#8B949E",
            GrayLight       = "#C6CDD4",
            GrayLighter     = "#E1E4E8",

            OverlayDark     = "rgba(0,0,0,0.5)",
            OverlayLight    = "rgba(255,255,255,0.8)",

            HoverOpacity    = 0.04,
            FocusedOpacity  = 0.08,
            SelectedOpacity = 0.08,
            ActivatedOpacity = 0.12,
            PressedOpacity  = 0.12,
        },

        // ── Typography ───────────────────────────────────────────────────────
        Typography = new Typography
        {
            Default = new DefaultTypography
            {
                FontFamily    = new[] { "Inter", "-apple-system", "BlinkMacSystemFont", "Segoe UI", "sans-serif" },
                FontSize      = "14px",
                FontWeight    = "400",
                LineHeight    = "1.5",
                LetterSpacing = "0.01em"
            },
            H1 = new H1Typography
            {
                FontFamily    = new[] { "Inter", "sans-serif" },
                FontSize      = "2rem",
                FontWeight    = "700",
                LineHeight    = "1.2",
                LetterSpacing = "-0.03em"
            },
            H2 = new H2Typography
            {
                FontSize    = "1.625rem",
                FontWeight  = "700",
                LetterSpacing = "-0.025em"
            },
            H3 = new H3Typography
            {
                FontSize    = "1.375rem",
                FontWeight  = "600",
                LetterSpacing = "-0.02em"
            },
            H4 = new H4Typography
            {
                FontSize    = "1.125rem",
                FontWeight  = "600",
                LetterSpacing = "-0.015em"
            },
            H5 = new H5Typography
            {
                FontSize    = "1rem",
                FontWeight  = "600",
                LetterSpacing = "-0.01em"
            },
            H6 = new H6Typography
            {
                FontSize    = "0.875rem",
                FontWeight  = "600",
                LetterSpacing = "-0.005em"
            },
            Subtitle1 = new Subtitle1Typography
            {
                FontSize    = "0.9375rem",
                FontWeight  = "500",
                LetterSpacing = "0"
            },
            Subtitle2 = new Subtitle2Typography
            {
                FontSize    = "0.8125rem",
                FontWeight  = "500",
                LetterSpacing = "0.01em"
            },
            Body1 = new Body1Typography
            {
                FontSize    = "0.875rem",
                FontWeight  = "400",
                LetterSpacing = "0.01em"
            },
            Body2 = new Body2Typography
            {
                FontSize    = "0.8125rem",
                FontWeight  = "400",
                LetterSpacing = "0.01em"
            },
            Button = new ButtonTypography
            {
                FontSize      = "0.8125rem",
                FontWeight    = "500",
                LetterSpacing = "0.01em",
                TextTransform = "none"
            },
            Caption = new CaptionTypography
            {
                FontSize    = "0.75rem",
                FontWeight  = "400",
                LetterSpacing = "0.02em"
            },
            Overline = new OverlineTypography
            {
                FontSize      = "0.6875rem",
                FontWeight    = "600",
                LetterSpacing = "0.08em",
                TextTransform = "uppercase"
            }
        },

        // ── Shape ────────────────────────────────────────────────────────────
        LayoutProperties = new LayoutProperties
        {
            DefaultBorderRadius = "10px",
            DrawerWidthLeft     = "248px",
            DrawerMiniWidthLeft = "56px",
            AppbarHeight        = "60px"
        },

        // ── Shadows (flatter, softer) ─────────────────────────────────────────
        Shadows = new Shadow
        {
            Elevation = new string[]
            {
                "none",                                                                              // 0
                "0 1px 2px rgba(0,0,0,0.06), 0 1px 3px rgba(0,0,0,0.04)",                          // 1
                "0 2px 4px rgba(0,0,0,0.07), 0 1px 3px rgba(0,0,0,0.05)",                          // 2
                "0 3px 8px rgba(0,0,0,0.08), 0 2px 4px rgba(0,0,0,0.05)",                          // 3
                "0 4px 12px rgba(0,0,0,0.09), 0 2px 6px rgba(0,0,0,0.06)",                         // 4
                "0 6px 16px rgba(0,0,0,0.10), 0 3px 8px rgba(0,0,0,0.06)",                         // 5
                "0 8px 20px rgba(0,0,0,0.10), 0 4px 10px rgba(0,0,0,0.07)",                        // 6
                "0 10px 24px rgba(0,0,0,0.11), 0 4px 12px rgba(0,0,0,0.07)",                       // 7
                "0 12px 28px rgba(0,0,0,0.12), 0 6px 14px rgba(0,0,0,0.08)",                       // 8
                "0 14px 32px rgba(0,0,0,0.12), 0 6px 16px rgba(0,0,0,0.08)",                       // 9
                "0 16px 36px rgba(0,0,0,0.13), 0 8px 18px rgba(0,0,0,0.09)",                       // 10
                "0 18px 40px rgba(0,0,0,0.14), 0 8px 20px rgba(0,0,0,0.09)",                       // 11
                "0 20px 44px rgba(0,0,0,0.14), 0 10px 22px rgba(0,0,0,0.10)",                      // 12
                "0 22px 48px rgba(0,0,0,0.15), 0 10px 24px rgba(0,0,0,0.10)",                      // 13
                "0 24px 52px rgba(0,0,0,0.15), 0 12px 26px rgba(0,0,0,0.10)",                      // 14
                "0 26px 56px rgba(0,0,0,0.16), 0 12px 28px rgba(0,0,0,0.11)",                      // 15
                "0 28px 60px rgba(0,0,0,0.16), 0 14px 30px rgba(0,0,0,0.11)",                      // 16
                "0 30px 64px rgba(0,0,0,0.17), 0 14px 32px rgba(0,0,0,0.11)",                      // 17
                "0 32px 68px rgba(0,0,0,0.17), 0 16px 34px rgba(0,0,0,0.12)",                      // 18
                "0 34px 72px rgba(0,0,0,0.18), 0 16px 36px rgba(0,0,0,0.12)",                      // 19
                "0 36px 76px rgba(0,0,0,0.18), 0 18px 38px rgba(0,0,0,0.12)",                      // 20
                "0 38px 80px rgba(0,0,0,0.19), 0 18px 40px rgba(0,0,0,0.13)",                      // 21
                "0 40px 84px rgba(0,0,0,0.19), 0 20px 42px rgba(0,0,0,0.13)",                      // 22
                "0 42px 88px rgba(0,0,0,0.20), 0 20px 44px rgba(0,0,0,0.13)",                      // 23
                "0 44px 92px rgba(0,0,0,0.20), 0 22px 46px rgba(0,0,0,0.14)",                      // 24
                "0 2px 16px rgba(0,0,0,0.12), 0 1px 4px rgba(0,0,0,0.08)"                          // 25 (popover)
            }
        },

        // ── Z-index ───────────────────────────────────────────────────────────
        ZIndex = new ZIndex
        {
            Drawer  = 1200,
            Popover = 1300,
            Modal   = 1400,
            Snackbar = 1500,
            Tooltip  = 1600
        }
    };
}
