namespace MacroMaster.WinForms.Theme;

internal static class DesignTokens
{
    public static readonly Color Background = Color.FromArgb(10, 13, 18);
    public static readonly Color Surface = Color.FromArgb(20, 23, 32);
    public static readonly Color Surface2 = Color.FromArgb(28, 32, 48);
    public static readonly Color Surface3 = Color.FromArgb(35, 40, 64);

    public static readonly Color Border = Color.FromArgb(42, 48, 80);
    public static readonly Color BorderBright = Color.FromArgb(53, 61, 96);

    public static readonly Color Accent = Color.FromArgb(79, 158, 255);
    public static readonly Color AccentPurple = Color.FromArgb(123, 95, 255);
    public static readonly Color AccentRed = Color.FromArgb(255, 79, 106);
    public static readonly Color AccentGreen = Color.FromArgb(61, 255, 160);
    public static readonly Color AccentOrange = Color.FromArgb(255, 170, 61);

    public static readonly Color TextPrimary = Color.FromArgb(232, 234, 242);
    public static readonly Color TextSecondary = Color.FromArgb(139, 146, 176);
    public static readonly Color TextMuted = Color.FromArgb(85, 95, 128);

    public const int TitleBarHeight = 42;
    public const int ToolbarHeight = 62;
    public const int LibraryPanelWidth = 380;
    public const int BottomPanelHeight = 220;
    public const int CardPadding = 16;
    public const int GapSmall = 8;
    public const int GapMedium = 12;
    public const int Radius = 8;

    public static readonly Font FontMono = new("Consolas", 9.25f, FontStyle.Regular, GraphicsUnit.Point);
    public static readonly Font FontUiNormal = new("Segoe UI", 9f, FontStyle.Regular, GraphicsUnit.Point);
    public static readonly Font FontUiBold = new("Segoe UI", 9f, FontStyle.Bold, GraphicsUnit.Point);
    public static readonly Font FontUiLarge = new("Segoe UI", 13f, FontStyle.Bold, GraphicsUnit.Point);
}
