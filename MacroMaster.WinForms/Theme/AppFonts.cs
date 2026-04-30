namespace MacroMaster.WinForms.Theme;

internal static class AppFonts
{
    private const string DefaultFamily = "Segoe UI";
    private const string MonospaceFamily = "Consolas";

    public static readonly Font Title = new(DefaultFamily, 18f, FontStyle.Bold, GraphicsUnit.Point);
    public static readonly Font SectionTitle = new(DefaultFamily, 11f, FontStyle.Bold, GraphicsUnit.Point);
    public static readonly Font Body = new(DefaultFamily, 9f, FontStyle.Regular, GraphicsUnit.Point);
    public static readonly Font BodyStrong = new(DefaultFamily, 9f, FontStyle.Bold, GraphicsUnit.Point);
    public static readonly Font Caption = new(DefaultFamily, 8f, FontStyle.Regular, GraphicsUnit.Point);
    public static readonly Font Button = new(DefaultFamily, 9f, FontStyle.Bold, GraphicsUnit.Point);
    public static readonly Font Monospace = new(MonospaceFamily, 9f, FontStyle.Regular, GraphicsUnit.Point);
}
