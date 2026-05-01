namespace MacroMaster.WinForms.Theme;

internal static class DesignTokens
{
    public static readonly float DensityScale = ResolveDensityScale();

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

    public static readonly int TitleBarHeight = Scale(42);
    public static readonly int ToolbarHeight = Scale(72);
    public static readonly int LibraryPanelWidth = Scale(380);
    public static readonly int BottomPanelHeight = Scale(240);
    public static readonly int CardPadding = Scale(18);
    public static readonly int GapSmall = Scale(8);
    public static readonly int GapMedium = Scale(12);
    public static readonly int Radius = Scale(12);

    public static readonly Font FontMono = new("Consolas", ScaleFont(9.25f), FontStyle.Regular, GraphicsUnit.Point);
    public static readonly Font FontUiNormal = new("Segoe UI", ScaleFont(9f), FontStyle.Regular, GraphicsUnit.Point);
    public static readonly Font FontUiBold = new("Segoe UI", ScaleFont(9f), FontStyle.Bold, GraphicsUnit.Point);
    public static readonly Font FontUiSmall = new("Segoe UI", ScaleFont(8.25f), FontStyle.Regular, GraphicsUnit.Point);
    public static readonly Font FontUiLarge = new("Segoe UI", ScaleFont(13f), FontStyle.Bold, GraphicsUnit.Point);

    public static int Scale(int value)
    {
        return (int)Math.Round(value * DensityScale);
    }

    public static float ScaleFont(float value)
    {
        return MathF.Round(value * DensityScale, 2);
    }

    private static float ResolveDensityScale()
    {
        Size workingAreaSize = Screen.PrimaryScreen?.WorkingArea.Size ?? new Size(1280, 760);
        float widthScale = workingAreaSize.Width / 1920f;
        float heightScale = workingAreaSize.Height / 1080f;
        float screenScale = MathF.Min(widthScale, heightScale);

        return Math.Clamp(screenScale, 1f, 1.35f);
    }
}
