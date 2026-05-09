using System.Runtime.InteropServices;

namespace MacroMaster.WinForms.Theme;

/// <summary>
/// Enterprise-grade DPI-aware scaling system.
///
/// Strategy:
///   1. Resolve system DPI at startup/fallback via Win32 API (GetDpiForSystem).
///   2. Allow explicit DPI injection via ConfigureForDpi(int dpi) for per-window/per-monitor lifecycle.
///   3. Normalize against the standard 96 DPI baseline → yields a true scale factor
///      (e.g. 100% = 1.0f, 125% = 1.25f, 150% = 1.5f, 200% = 2.0f).
///   4. All layout values are authored at 96 DPI (1x) and multiplied by this factor.
///   5. Fonts are scaled separately with a tighter clamp so they don't blow up on 4K.
///
/// Why NOT use Screen.WorkingArea?
///   WorkingArea returns *logical* pixels already adjusted by DPI, so dividing by a
///   reference resolution double-counts the OS scaling — exactly the bug this replaces.
///
/// Why NOT use AutoScaleMode.Dpi on controls?
///   Our layout is built entirely in code with Scale() calls. Windows AutoScale would
///   apply a second pass on top, causing the double-scaling seen at 125%/150% DPI.
///   All Designer files must have AutoScaleMode = None.
/// </summary>
internal static class AppScale
{
    private const float BaseDpi = 96f;
    private static readonly object Sync = new();
    private static int _currentDpi = (int)BaseDpi;
    private static float _densityScale = 1f;
    private static float _fontScale = 1f;

    static AppScale()
    {
        Refresh();
    }

    // True Windows DPI scale: 1.0 at 96dpi, 1.25 at 120dpi, 1.5 at 144dpi, 2.0 at 192dpi
    public static int CurrentDpi
    {
        get
        {
            lock (Sync)
            {
                return _currentDpi;
            }
        }
    }

    public static float DensityScale
    {
        get
        {
            lock (Sync)
            {
                return _densityScale;
            }
        }
    }

    // Font scale is clamped tighter — large fonts on high-DPI look disproportionate
    public static float FontScale
    {
        get
        {
            lock (Sync)
            {
                return _fontScale;
            }
        }
    }

    public static void Refresh()
    {
        int dpi = ResolveWindowsDpi();
        ConfigureForDpi(dpi);
    }

    public static void ConfigureForDpi(int dpi)
    {
        lock (Sync)
        {
            int normalizedDpi = dpi <= 0 ? (int)BaseDpi : dpi;
            float density = Math.Max(1f, normalizedDpi / BaseDpi);
            _currentDpi = normalizedDpi;
            _densityScale = density;
            _fontScale = Math.Clamp(density, 1f, 1.15f);
        }
    }

    public static int Scale(int value) =>
        (int)Math.Round(value * DensityScale);

    public static float ScaleFont(float value) =>
        MathF.Round(value * FontScale, 2);

    // -------------------------------------------------------------------------
    // Win32 DPI detection
    // -------------------------------------------------------------------------

    [DllImport("user32.dll")]
    private static extern int GetDpiForSystem();

    private static int ResolveWindowsDpi()
    {
        try
        {
            int sysDpi = GetDpiForSystem();
            if (sysDpi > 0)
            {
                return sysDpi;
            }
        }
        catch
        {
            // GetDpiForSystem unavailable (pre-Win10) → fall back to Graphics
        }

        // Fallback: read DPI from a temporary Graphics context (works on Win7/8)
        try
        {
            using var screen = Graphics.FromHwnd(IntPtr.Zero);
            int dpi = (int)Math.Round(screen.DpiX);
            if (dpi > 0)
            {
                return dpi;
            }
        }
        catch
        {
            // Ignore
        }

        return (int)BaseDpi; // Safe default: no scaling
    }
}