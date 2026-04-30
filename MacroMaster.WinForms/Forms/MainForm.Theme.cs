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
        Padding = Padding.Empty;

        if (_contentHost is not null)
        {
            _contentHost.BackColor = AppColors.PageBackground;
        }

        ApplyToolStripTheme(_menuStrip);
        ApplyToolStripTheme(_statusStrip);

        ResumeLayout(performLayout: true);
    }

    private void ApplyToolStripTheme(ToolStrip? strip)
    {
        if (strip is null)
        {
            return;
        }

        strip.BackColor = Color.FromArgb(10, 15, 28);
        strip.ForeColor = AppColors.TextPrimary;
        strip.Font = AppFonts.Body;
        strip.RenderMode = ToolStripRenderMode.Professional;
        strip.Renderer = new ToolStripProfessionalRenderer(new AppToolStripColorTable());

        foreach (ToolStripItem item in strip.Items)
        {
            ApplyToolStripItemTheme(item);
        }
    }

    private void ApplyToolStripItemTheme(ToolStripItem item)
    {
        item.BackColor = Color.Transparent;
        item.ForeColor = AppColors.TextPrimary;
        item.Font = AppFonts.Body;

        if (item is ToolStripDropDownItem dropDownItem)
        {
            foreach (ToolStripItem childItem in dropDownItem.DropDownItems)
            {
                ApplyToolStripItemTheme(childItem);
            }
        }
    }
}
