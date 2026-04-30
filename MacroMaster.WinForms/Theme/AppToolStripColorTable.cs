namespace MacroMaster.WinForms.Theme;

internal sealed class AppToolStripColorTable : ProfessionalColorTable
{
    public override Color MenuStripGradientBegin => Color.FromArgb(10, 15, 28);
    public override Color MenuStripGradientEnd => Color.FromArgb(10, 15, 28);
    public override Color ToolStripGradientBegin => Color.FromArgb(10, 15, 28);
    public override Color ToolStripGradientMiddle => Color.FromArgb(10, 15, 28);
    public override Color ToolStripGradientEnd => Color.FromArgb(10, 15, 28);
    public override Color StatusStripGradientBegin => Color.FromArgb(9, 14, 24);
    public override Color StatusStripGradientEnd => Color.FromArgb(9, 14, 24);
    public override Color ImageMarginGradientBegin => Color.FromArgb(14, 20, 38);
    public override Color ImageMarginGradientMiddle => Color.FromArgb(14, 20, 38);
    public override Color ImageMarginGradientEnd => Color.FromArgb(14, 20, 38);
    public override Color ToolStripBorder => Color.FromArgb(28, 38, 61);
    public override Color MenuBorder => Color.FromArgb(28, 38, 61);
    public override Color SeparatorDark => Color.FromArgb(35, 47, 74);
    public override Color SeparatorLight => Color.FromArgb(14, 20, 38);
    public override Color MenuItemSelected => Color.FromArgb(31, 45, 75);
    public override Color MenuItemBorder => AppColors.Primary;
    public override Color MenuItemSelectedGradientBegin => Color.FromArgb(31, 45, 75);
    public override Color MenuItemSelectedGradientEnd => Color.FromArgb(31, 45, 75);
    public override Color MenuItemPressedGradientBegin => Color.FromArgb(20, 31, 53);
    public override Color MenuItemPressedGradientMiddle => Color.FromArgb(20, 31, 53);
    public override Color MenuItemPressedGradientEnd => Color.FromArgb(20, 31, 53);
    public override Color ButtonSelectedBorder => AppColors.Primary;
    public override Color ButtonPressedBorder => AppColors.Primary;
    public override Color ButtonSelectedGradientBegin => Color.FromArgb(31, 45, 75);
    public override Color ButtonSelectedGradientMiddle => Color.FromArgb(31, 45, 75);
    public override Color ButtonSelectedGradientEnd => Color.FromArgb(31, 45, 75);
    public override Color ButtonPressedGradientBegin => Color.FromArgb(20, 31, 53);
    public override Color ButtonPressedGradientMiddle => Color.FromArgb(20, 31, 53);
    public override Color ButtonPressedGradientEnd => Color.FromArgb(20, 31, 53);
}
