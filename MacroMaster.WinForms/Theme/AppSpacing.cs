namespace MacroMaster.WinForms.Theme;

/// <summary>
/// All values are authored at 96 DPI (1x baseline).
/// AppScale.Scale() multiplies them by the detected Windows DPI factor at runtime.
/// </summary>
internal static class AppSpacing
{
    public static int TitleBarHeight => AppScale.Scale(54);
    public static int TitleBarButtonWidth => AppScale.Scale(48);
    public static int TitleBarButtonHeight => AppScale.Scale(40);
    public static int TitleBarIconSize => AppScale.Scale(32);
    public static int TitleBarStatusDotSize => AppScale.Scale(8);
    public static int WindowResizeBorder => AppScale.Scale(8);
    public static int WindowMaximizedPadding => AppScale.Scale(8);
    public static int ToolbarHeight => AppScale.Scale(82);
    public static int LibraryPanelWidth => AppScale.Scale(380);
    public static int CardPadding => AppScale.Scale(18);
    public static int GapSmall => AppScale.Scale(8);
    public static int GapMedium => AppScale.Scale(12);
    public static int Radius => AppScale.Scale(12);
}
