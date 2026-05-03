namespace MacroMaster.WinForms.Theme;

/// <summary>
/// All values are authored at 96 DPI (1x baseline).
/// AppScale.Scale() multiplies them by the detected Windows DPI factor at runtime.
/// </summary>
internal static class AppSpacing
{
    public static readonly int TitleBarHeight = AppScale.Scale(42);
    public static readonly int ToolbarHeight = AppScale.Scale(82);
    public static readonly int LibraryPanelWidth = AppScale.Scale(380);
    public static readonly int CardPadding = AppScale.Scale(18);
    public static readonly int GapSmall = AppScale.Scale(8);
    public static readonly int GapMedium = AppScale.Scale(12);
    public static readonly int Radius = AppScale.Scale(12);
}