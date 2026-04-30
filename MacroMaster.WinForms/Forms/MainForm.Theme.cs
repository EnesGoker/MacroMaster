using MacroMaster.WinForms.Theme;

namespace MacroMaster.WinForms.Forms;

public partial class MainForm
{
    private void ApplyTheme()
    {
        SuspendLayout();

        BackColor = AppColors.PageBackground;
        ForeColor = AppColors.TextPrimary;
        Font = AppFonts.Body;
        Padding = AppSpacing.PagePadding;

        ResumeLayout(performLayout: true);
    }
}
