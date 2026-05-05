using MacroMaster.WinForms.Theme;

namespace MacroMaster.WinForms.Forms;

internal sealed class AppConfirmDialog : Form
{
    private AppConfirmDialog(
        string title,
        string message,
        string confirmText,
        string cancelText,
        bool isDanger)
    {
        Text = title;
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.None;
        ShowInTaskbar = false;
        AutoScaleMode = AutoScaleMode.None;
        BackColor = DesignTokens.Surface;
        ForeColor = DesignTokens.TextPrimary;
        Font = DesignTokens.FontUiNormal;
        Padding = new Padding(DesignTokens.Scale(18));
        ClientSize = new Size(DesignTokens.Scale(390), DesignTokens.Scale(190));
        KeyPreview = true;

        Button confirmButton = CreateButton(confirmText, isDanger);
        Button cancelButton = CreateButton(cancelText, isPrimary: false);
        confirmButton.DialogResult = DialogResult.OK;
        cancelButton.DialogResult = DialogResult.Cancel;
        AcceptButton = confirmButton;
        CancelButton = cancelButton;

        var rootLayoutPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            BackColor = DesignTokens.Surface,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        rootLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        rootLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, DesignTokens.Scale(38)));
        rootLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
        rootLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, DesignTokens.Scale(52)));

        rootLayoutPanel.Controls.Add(
            new Label
            {
                Dock = DockStyle.Fill,
                Text = title,
                Font = DesignTokens.FontUiBold,
                ForeColor = DesignTokens.TextPrimary,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleLeft,
                AutoEllipsis = true
            },
            0,
            0);

        rootLayoutPanel.Controls.Add(
            new Label
            {
                Dock = DockStyle.Fill,
                Text = message,
                Font = DesignTokens.FontUiNormal,
                ForeColor = DesignTokens.TextSecondary,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleLeft,
                AutoEllipsis = true
            },
            0,
            1);

        var buttonPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false,
            BackColor = DesignTokens.Surface,
            Padding = new Padding(0, DesignTokens.Scale(10), 0, 0)
        };
        buttonPanel.Controls.Add(cancelButton);
        buttonPanel.Controls.Add(confirmButton);
        rootLayoutPanel.Controls.Add(buttonPanel, 0, 2);

        Controls.Add(rootLayoutPanel);
        KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
                Close();
            }
        };
    }

    public static bool ShowDanger(
        IWin32Window owner,
        string title,
        string message,
        string confirmText = "Evet",
        string cancelText = "Hayir")
    {
        using var dialog = new AppConfirmDialog(
            title,
            message,
            confirmText,
            cancelText,
            isDanger: true);

        return dialog.ShowDialog(owner) == DialogResult.OK;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        using var borderPen = new Pen(DesignTokens.BorderBright);
        e.Graphics.DrawRectangle(borderPen, 0, 0, Width - 1, Height - 1);
    }

    private static Button CreateButton(string text, bool isPrimary)
    {
        var button = new Button
        {
            Width = DesignTokens.Scale(112),
            Height = DesignTokens.Scale(34),
            Margin = new Padding(DesignTokens.Scale(8), 0, 0, 0),
            Text = text,
            FlatStyle = FlatStyle.Flat,
            UseVisualStyleBackColor = false,
            Font = DesignTokens.FontUiBold,
            BackColor = isPrimary ? Color.FromArgb(92, 25, 47) : DesignTokens.Surface2,
            ForeColor = DesignTokens.TextPrimary
        };

        button.FlatAppearance.BorderColor = isPrimary
            ? DesignTokens.AccentRed
            : DesignTokens.BorderBright;
        button.FlatAppearance.BorderSize = 1;

        return button;
    }
}
