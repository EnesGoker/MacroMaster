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
    float FitRatio);

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
