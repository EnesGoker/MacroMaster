namespace MacroMaster.WinForms.Theme;

internal static class DesignTokens
{
    public static readonly Color Background = Color.FromArgb(7, 10, 16);
    public static readonly Color Surface = Color.FromArgb(18, 21, 31);
    public static readonly Color Surface2 = Color.FromArgb(25, 30, 45);
    public static readonly Color Surface3 = Color.FromArgb(34, 41, 62);
    public static readonly Color SurfaceInset = Color.FromArgb(9, 13, 21);
    public static readonly Color SurfaceHover = Color.FromArgb(31, 38, 58);

    public static readonly Color Border = Color.FromArgb(38, 48, 77);
    public static readonly Color BorderSoft = Color.FromArgb(27, 35, 56);
    public static readonly Color BorderBright = Color.FromArgb(62, 75, 112);

    public static readonly Color Accent = Color.FromArgb(79, 158, 255);
    public static readonly Color AccentDeep = Color.FromArgb(24, 100, 218);
    public static readonly Color AccentSoft = Color.FromArgb(14, 55, 108);
    public static readonly Color AccentPurple = Color.FromArgb(123, 95, 255);
    public static readonly Color AccentRed = Color.FromArgb(255, 79, 106);
    public static readonly Color AccentRedSoft = Color.FromArgb(76, 27, 44);
    public static readonly Color AccentGreen = Color.FromArgb(61, 255, 160);
    public static readonly Color AccentOrange = Color.FromArgb(255, 170, 61);

    public static readonly Color TextPrimary = Color.FromArgb(240, 244, 255);
    public static readonly Color TextSecondary = Color.FromArgb(156, 168, 202);
    public static readonly Color TextMuted = Color.FromArgb(96, 108, 143);

    public const int TitleBarHeight = 42;
    public const int ToolbarHeight = 66;
    public const int LibraryPanelWidth = 380;
    public const int BottomPanelHeight = 220;
    public const int CardPadding = 18;
    public const int GapSmall = 8;
    public const int GapMedium = 12;
    public const int Radius = 12;

    public static readonly Font FontMono = new("Consolas", 9.25f, FontStyle.Regular, GraphicsUnit.Point);
    public static readonly Font FontUiNormal = new("Segoe UI", 9f, FontStyle.Regular, GraphicsUnit.Point);
    public static readonly Font FontUiBold = new("Segoe UI", 9f, FontStyle.Bold, GraphicsUnit.Point);
    public static readonly Font FontUiSmall = new("Segoe UI", 8.25f, FontStyle.Regular, GraphicsUnit.Point);
    public static readonly Font FontUiLarge = new("Segoe UI", 13f, FontStyle.Bold, GraphicsUnit.Point);
}
