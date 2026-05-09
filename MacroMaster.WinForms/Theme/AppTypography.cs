namespace MacroMaster.WinForms.Theme;

internal static class AppTypography
{
    private static readonly object Sync = new();
    private static Font? _fontMono;
    private static Font? _fontUiNormal;
    private static Font? _fontUiBold;
    private static Font? _fontUiSmall;
    private static Font? _fontUiLarge;

    public static Font FontMono => GetOrCreate(ref _fontMono, "Consolas", 9.25f, FontStyle.Regular);
    public static Font FontUiNormal => GetOrCreate(ref _fontUiNormal, "Segoe UI", 9f, FontStyle.Regular);
    public static Font FontUiBold => GetOrCreate(ref _fontUiBold, "Segoe UI", 9f, FontStyle.Bold);
    public static Font FontUiSmall => GetOrCreate(ref _fontUiSmall, "Segoe UI", 8.25f, FontStyle.Regular);
    public static Font FontUiLarge => GetOrCreate(ref _fontUiLarge, "Segoe UI", 13f, FontStyle.Bold);

    public static void Refresh()
    {
        lock (Sync)
        {
            // Do not dispose old Font instances here. Controls may still keep old Font references,
            // and disposing now could trigger ObjectDisposedException in active UI paths.
            // For now we only invalidate cache slots; controlled disposal can be added in a
            // future lifecycle phase when ownership boundaries are explicit.
            _fontMono = null;
            _fontUiNormal = null;
            _fontUiBold = null;
            _fontUiSmall = null;
            _fontUiLarge = null;
        }
    }

    private static Font GetOrCreate(ref Font? slot, string family, float size, FontStyle style)
    {
        lock (Sync)
        {
            slot ??= CreateFont(family, size, style);
            return slot;
        }
    }

    private static Font CreateFont(string family, float size, FontStyle style) =>
        new(family, AppScale.ScaleFont(size), style, GraphicsUnit.Point);
}
