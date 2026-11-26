using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;
using MediaColor = System.Windows.Media.Color;

namespace SpartonFaceAdmin.App.Themes;

internal static class SpartronicsAppearance
{
    // Exact tokens shared with Rework / FQC Desktop (ShippingBoardScan) and MTC Web UI.
    public static readonly MediaColor Accent = MediaColor.FromRgb(0x1E, 0x4E, 0x8C);
    public static readonly MediaColor AccentPrimary = MediaColor.FromRgb(0x1E, 0x4E, 0x8C);
    public static readonly MediaColor AccentSecondary = MediaColor.FromRgb(0x16, 0x3B, 0x6B);
    public static readonly MediaColor AccentTertiary = MediaColor.FromRgb(0x11, 0x22, 0x36);

    public static void Apply()
    {
        ApplicationThemeManager.Apply(ApplicationTheme.Light, WindowBackdropType.None, updateAccent: false);
        ApplicationAccentColorManager.Apply(Accent, AccentPrimary, AccentSecondary, AccentTertiary);
    }
}
