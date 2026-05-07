using MacroMaster.WinForms.Platform;
using MacroMaster.WinForms.Theme;

namespace MacroMaster.WinForms.Forms;

internal sealed class MacroNameEditDialog : Form
{
    private readonly TextBox _nameTextBox = new();
    private readonly ThemedDialogButton _cancelButton;

    public MacroNameEditDialog(string currentName)
    {
        MacroName = string.IsNullOrWhiteSpace(currentName)
            ? "MakroOturumu"
            : currentName.Trim();

        Text = "Makro Ismi Duzenle";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        AutoScaleMode = AutoScaleMode.None;
        BackColor = DesignTokens.Surface;
        ForeColor = DesignTokens.TextPrimary;
        Font = DesignTokens.FontUiNormal;
        ClientSize = new Size(DesignTokens.Scale(420), DesignTokens.Scale(206));
        MinimumSize = Size;

        _cancelButton = CreateDialogButton("Iptal", ThemedDialogButtonStyle.Secondary);
        BuildLayout();
    }

    public string MacroName { get; private set; }

    private void BuildLayout()
    {
        var rootLayoutPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            BackColor = DesignTokens.Surface,
            Padding = new Padding(DesignTokens.Scale(18)),
            Margin = Padding.Empty
        };
        rootLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, DesignTokens.Scale(36)));
        rootLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, DesignTokens.Scale(28)));
        rootLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, DesignTokens.Scale(44)));
        rootLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

        var titleLabel = new Label
        {
            Dock = DockStyle.Fill,
            Text = "Makro adini duzenle",
            Font = DesignTokens.FontUiBold,
            ForeColor = DesignTokens.TextPrimary,
            BackColor = Color.Transparent,
            TextAlign = ContentAlignment.MiddleLeft
        };

        var fieldLabel = new Label
        {
            Dock = DockStyle.Fill,
            Text = "Makro adi",
            Font = DesignTokens.FontUiSmall,
            ForeColor = DesignTokens.TextSecondary,
            BackColor = Color.Transparent,
            TextAlign = ContentAlignment.BottomLeft
        };

        _nameTextBox.Dock = DockStyle.Fill;
        _nameTextBox.Text = MacroName;
        _nameTextBox.BorderStyle = BorderStyle.FixedSingle;
        _nameTextBox.BackColor = DesignTokens.SurfaceInset;
        _nameTextBox.ForeColor = DesignTokens.TextPrimary;
        _nameTextBox.Font = DesignTokens.FontUiNormal;
        _nameTextBox.Margin = new Padding(0, DesignTokens.Scale(6), 0, DesignTokens.Scale(6));

        var buttonLayoutPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            BackColor = Color.Transparent,
            Margin = Padding.Empty,
            Padding = new Padding(0, DesignTokens.Scale(10), 0, 0)
        };

        var saveButton = CreateDialogButton("Kaydet", ThemedDialogButtonStyle.Primary);

        saveButton.Click += (_, _) => SaveAndClose();
        _cancelButton.Click += (_, _) =>
        {
            DialogResult = DialogResult.Cancel;
            Close();
        };

        AcceptButton = saveButton;
        CancelButton = _cancelButton;
        buttonLayoutPanel.Controls.Add(saveButton);
        buttonLayoutPanel.Controls.Add(_cancelButton);

        rootLayoutPanel.Controls.Add(titleLabel, 0, 0);
        rootLayoutPanel.Controls.Add(fieldLabel, 0, 1);
        rootLayoutPanel.Controls.Add(_nameTextBox, 0, 2);
        rootLayoutPanel.Controls.Add(buttonLayoutPanel, 0, 3);

        Controls.Add(rootLayoutPanel);
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);

        WindowChromeNative.TryApplyDwmBoolAttribute(
            Handle,
            DwmWindowAttribute.UseImmersiveDarkMode,
            enabled: true);
        WindowChromeNative.TryApplyDwmCornerPreference(
            Handle,
            DwmWindowCornerPreference.Round);
        WindowChromeNative.TryApplyDwmColorAttribute(
            Handle,
            DwmWindowAttribute.BorderColor,
            DesignTokens.Border);
        WindowChromeNative.TryApplyDwmColorAttribute(
            Handle,
            DwmWindowAttribute.CaptionColor,
            DesignTokens.Surface);
        WindowChromeNative.TryApplyDwmColorAttribute(
            Handle,
            DwmWindowAttribute.TextColor,
            DesignTokens.TextPrimary);
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        _nameTextBox.Focus();
        _nameTextBox.SelectAll();
    }

    private void SaveAndClose()
    {
        string trimmedName = _nameTextBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(trimmedName))
        {
            MessageBox.Show(
                this,
                "Makro adi bos olamaz.",
                "MacroMaster",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return;
        }

        MacroName = trimmedName;
        DialogResult = DialogResult.OK;
        Close();
    }

    private static ThemedDialogButton CreateDialogButton(string text, ThemedDialogButtonStyle style)
    {
        return new ThemedDialogButton(style)
        {
            Text = text,
            Width = DesignTokens.Scale(112),
            Height = DesignTokens.Scale(34),
            Margin = new Padding(DesignTokens.Scale(8), 0, 0, 0)
        };
    }
}
