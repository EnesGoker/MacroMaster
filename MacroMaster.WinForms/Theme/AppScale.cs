namespace MacroMaster.WinForms.Theme;

internal static class AppScale
{
    private static readonly Size ReferenceWorkingAreaSize = new(1920, 1080);
    private static readonly Size FallbackWorkingAreaSize = new(1280, 760);

    public static readonly float DensityScale = ResolveDensityScale();
    public static readonly float FontScale = Math.Clamp(DensityScale, 1f, 1.08f);

    public static int Scale(int value)
    {
        return (int)Math.Round(value * DensityScale);
    }

    public static float ScaleFont(float value)
    {
        return MathF.Round(value * FontScale, 2);
    }

    private static float ResolveDensityScale()
    {
        Size workingAreaSize = Screen.PrimaryScreen?.WorkingArea.Size ?? FallbackWorkingAreaSize;
        float widthScale = workingAreaSize.Width / (float)ReferenceWorkingAreaSize.Width;
        float heightScale = workingAreaSize.Height / (float)ReferenceWorkingAreaSize.Height;
        float screenScale = MathF.Min(widthScale, heightScale);

        return Math.Clamp(screenScale, 1f, 1.25f);
    }
}
