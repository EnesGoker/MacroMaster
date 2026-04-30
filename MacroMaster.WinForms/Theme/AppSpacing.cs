namespace MacroMaster.WinForms.Theme;

internal static class AppSpacing
{
    public const int Xs = 4;
    public const int Sm = 8;
    public const int Md = 12;
    public const int Lg = 16;
    public const int Xl = 24;
    public const int Xxl = 32;

    public static Padding PagePadding => new(Xl);
    public static Padding CardPadding => new(Lg);
    public static Padding SectionPadding => new(Md);
}
