using System.Drawing;
using System.Windows.Forms;

namespace MacroMaster.WinForms.Theme;

internal enum AppShellLayoutMode
{
    Expanded,
    Compact,
    Constrained
}

internal readonly record struct AppShellLayoutProfile(
    AppShellLayoutMode Mode,
    Size PreferredClientSize,
    Size MinimumClientSize,
    Padding RootPadding,
    AppShellChromeMetrics Chrome,
    float FitRatio);

internal readonly record struct AppShellChromeMetrics(
    int TitleBarRowHeight,
    int TitleBarIconColumnWidth,
    int TitleBarStatusWidth,
    int TitleBarButtonWidth,
    int TitleBarButtonHeight,
    Padding TitleBarPadding,
    Padding TitleBarStatusMargin,
    Padding TitleBarButtonMargin,
    int ToolbarRowHeight,
    int ToolbarControlMinimumHeight,
    Padding ToolbarHostMargin,
    Padding ToolbarContentPadding,
    Padding ToolbarControlPadding,
    Padding ToolbarButtonMargin);

internal static class AppShellLayoutProfileResolver
{
    private const int BaselineClientWidth = 1280;
    private const int BaselineClientHeight = 760;
    private const int MinimumClientWidth = 960;
    private const int MinimumClientHeight = 620;
    private const float CompactFitThreshold = 0.94f;
    private const float ConstrainedFitThreshold = 0.78f;

    public static AppShellLayoutProfile Resolve(
        Rectangle workingArea,
        float densityScale)
    {
        float safeDensityScale = Math.Clamp(densityScale, 1f, 3f);
        Size desiredClientSize = ScaleSize(
            BaselineClientWidth,
            BaselineClientHeight,
            safeDensityScale);
        Size minimumClientSize = ScaleSize(
            MinimumClientWidth,
            MinimumClientHeight,
            ResolveMinimumScale(safeDensityScale));
        Size availableClientSize = ResolveAvailableClientSize(
            workingArea,
            desiredClientSize,
            minimumClientSize,
            safeDensityScale);
        Size preferredClientSize = new(
            Math.Clamp(desiredClientSize.Width, minimumClientSize.Width, availableClientSize.Width),
            Math.Clamp(desiredClientSize.Height, minimumClientSize.Height, availableClientSize.Height));

        float fitRatio = Math.Min(
            (float)preferredClientSize.Width / Math.Max(1, desiredClientSize.Width),
            (float)preferredClientSize.Height / Math.Max(1, desiredClientSize.Height));
        AppShellLayoutMode mode = ResolveMode(fitRatio);

        return new AppShellLayoutProfile(
            mode,
            preferredClientSize,
            minimumClientSize,
            ResolveRootPadding(mode, safeDensityScale),
            ResolveChromeMetrics(mode, safeDensityScale),
            fitRatio);
    }

    private static float ResolveMinimumScale(float densityScale)
    {
        return Math.Clamp(densityScale, 1f, 1.25f);
    }

    private static Size ResolveAvailableClientSize(
        Rectangle workingArea,
        Size desiredClientSize,
        Size minimumClientSize,
        float densityScale)
    {
        if (workingArea.Width <= 0 || workingArea.Height <= 0)
        {
            return desiredClientSize;
        }

        int margin = Scale(24, Math.Clamp(densityScale, 1f, 1.5f));
        int availableWidth = Math.Max(minimumClientSize.Width, workingArea.Width - (margin * 2));
        int availableHeight = Math.Max(minimumClientSize.Height, workingArea.Height - (margin * 2));

        return new Size(availableWidth, availableHeight);
    }

    private static AppShellLayoutMode ResolveMode(float fitRatio)
    {
        if (fitRatio >= CompactFitThreshold)
        {
            return AppShellLayoutMode.Expanded;
        }

        return fitRatio >= ConstrainedFitThreshold
            ? AppShellLayoutMode.Compact
            : AppShellLayoutMode.Constrained;
    }

    private static Padding ResolveRootPadding(
        AppShellLayoutMode mode,
        float densityScale)
    {
        float paddingScale = mode switch
        {
            AppShellLayoutMode.Expanded => densityScale,
            AppShellLayoutMode.Compact => Math.Min(densityScale, 1.5f),
            _ => Math.Min(densityScale, 1.25f)
        };

        return new Padding(
            Scale(18, paddingScale),
            0,
            Scale(18, paddingScale),
            Scale(16, paddingScale));
    }

    private static AppShellChromeMetrics ResolveChromeMetrics(
        AppShellLayoutMode mode,
        float densityScale)
    {
        float chromeScale = ResolveChromeScale(mode, densityScale);

        return new AppShellChromeMetrics(
            TitleBarRowHeight: Scale(46, chromeScale),
            TitleBarIconColumnWidth: Scale(40, chromeScale),
            TitleBarStatusWidth: Scale(ResolveStatusWidth(mode), chromeScale),
            TitleBarButtonWidth: Scale(ResolveTitleButtonWidth(mode), chromeScale),
            TitleBarButtonHeight: Scale(ResolveTitleButtonHeight(mode), chromeScale),
            TitleBarPadding: new Padding(
                0,
                0,
                Scale(ResolveTitleBarRightPadding(mode), chromeScale),
                Scale(2, chromeScale)),
            TitleBarStatusMargin: new Padding(
                Scale(6, chromeScale),
                Scale(2, chromeScale),
                Scale(8, chromeScale),
                Scale(2, chromeScale)),
            TitleBarButtonMargin: new Padding(
                Scale(2, chromeScale),
                Scale(1, chromeScale),
                0,
                Scale(ResolveTitleButtonBottomMargin(mode), chromeScale)),
            ToolbarRowHeight: Scale(ResolveToolbarRowHeight(mode), chromeScale),
            ToolbarControlMinimumHeight: Scale(ResolveToolbarControlMinimumHeight(mode), chromeScale),
            ToolbarHostMargin: new Padding(
                0,
                Scale(ResolveToolbarHostTopMargin(mode), chromeScale),
                0,
                Scale(4, chromeScale)),
            ToolbarContentPadding: new Padding(
                Scale(ResolveToolbarContentHorizontalPadding(mode), chromeScale),
                Scale(ResolveToolbarContentVerticalPadding(mode), chromeScale),
                Scale(ResolveToolbarContentHorizontalPadding(mode), chromeScale),
                Scale(ResolveToolbarContentVerticalPadding(mode), chromeScale)),
            ToolbarControlPadding: new Padding(
                Scale(ResolveToolbarControlHorizontalPadding(mode), chromeScale),
                Scale(ResolveToolbarControlVerticalPadding(mode), chromeScale),
                Scale(ResolveToolbarControlHorizontalPadding(mode), chromeScale),
                Scale(ResolveToolbarControlVerticalPadding(mode), chromeScale)),
            ToolbarButtonMargin: new Padding(
                Scale(ResolveToolbarButtonHorizontalMargin(mode), chromeScale),
                Scale(ResolveToolbarButtonVerticalMargin(mode), chromeScale),
                Scale(ResolveToolbarButtonHorizontalMargin(mode), chromeScale),
                Scale(ResolveToolbarButtonVerticalMargin(mode), chromeScale)));
    }

    private static float ResolveChromeScale(
        AppShellLayoutMode mode,
        float densityScale)
    {
        return mode switch
        {
            AppShellLayoutMode.Expanded => densityScale,
            AppShellLayoutMode.Compact => Math.Min(densityScale, 1.45f),
            _ => Math.Min(densityScale, 1.25f)
        };
    }

    private static int ResolveStatusWidth(AppShellLayoutMode mode)
    {
        return mode switch
        {
            AppShellLayoutMode.Constrained => 108,
            AppShellLayoutMode.Compact => 120,
            _ => 132
        };
    }

    private static int ResolveTitleButtonWidth(AppShellLayoutMode mode)
    {
        return mode == AppShellLayoutMode.Constrained ? 42 : 48;
    }

    private static int ResolveTitleButtonHeight(AppShellLayoutMode mode)
    {
        return mode == AppShellLayoutMode.Constrained ? 36 : 40;
    }

    private static int ResolveTitleBarRightPadding(AppShellLayoutMode mode)
    {
        return mode == AppShellLayoutMode.Constrained ? 6 : 10;
    }

    private static int ResolveTitleButtonBottomMargin(AppShellLayoutMode mode)
    {
        return mode == AppShellLayoutMode.Constrained ? 4 : 7;
    }

    private static int ResolveToolbarRowHeight(AppShellLayoutMode mode)
    {
        return mode switch
        {
            AppShellLayoutMode.Constrained => 88,
            AppShellLayoutMode.Compact => 98,
            _ => 106
        };
    }

    private static int ResolveToolbarControlMinimumHeight(AppShellLayoutMode mode)
    {
        return mode switch
        {
            AppShellLayoutMode.Constrained => 58,
            AppShellLayoutMode.Compact => 66,
            _ => 82
        };
    }

    private static int ResolveToolbarHostTopMargin(AppShellLayoutMode mode)
    {
        return mode switch
        {
            AppShellLayoutMode.Constrained => 8,
            AppShellLayoutMode.Compact => 12,
            _ => 16
        };
    }

    private static int ResolveToolbarContentHorizontalPadding(AppShellLayoutMode mode)
    {
        return mode == AppShellLayoutMode.Constrained ? 12 : 18;
    }

    private static int ResolveToolbarContentVerticalPadding(AppShellLayoutMode mode)
    {
        return mode == AppShellLayoutMode.Expanded ? 7 : 6;
    }

    private static int ResolveToolbarControlHorizontalPadding(AppShellLayoutMode mode)
    {
        return mode switch
        {
            AppShellLayoutMode.Constrained => 7,
            AppShellLayoutMode.Compact => 9,
            _ => 12
        };
    }

    private static int ResolveToolbarControlVerticalPadding(AppShellLayoutMode mode)
    {
        return mode switch
        {
            AppShellLayoutMode.Constrained => 5,
            AppShellLayoutMode.Compact => 7,
            _ => 10
        };
    }

    private static int ResolveToolbarButtonHorizontalMargin(AppShellLayoutMode mode)
    {
        return mode == AppShellLayoutMode.Constrained ? 4 : 5;
    }

    private static int ResolveToolbarButtonVerticalMargin(AppShellLayoutMode mode)
    {
        return mode == AppShellLayoutMode.Expanded ? 6 : 4;
    }

    private static Size ScaleSize(
        int width,
        int height,
        float scale)
    {
        return new Size(Scale(width, scale), Scale(height, scale));
    }

    private static int Scale(int value, float scale)
    {
        return (int)Math.Round(value * scale);
    }
}
