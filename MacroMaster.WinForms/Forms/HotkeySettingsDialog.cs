using MacroMaster.WinForms.Controls;
using MacroMaster.WinForms.Theme;

namespace MacroMaster.WinForms.Forms;

internal sealed class HotkeySettingsDialog : Form
{
    private readonly ComboBox _recordComboBox;
    private readonly ComboBox _playbackComboBox;
    private readonly ComboBox _stopComboBox;
    private readonly Label _errorLabel;

    public HotkeySettingsDialog(int recordToggleVirtualKey, int playbackToggleVirtualKey, int stopVirtualKey)
    {
        Text = "Kisayol Ayarlari";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        ClientSize = new Size(460, 312);
        BackColor = AppColors.PageBackground;
        ForeColor = AppColors.TextPrimary;
        Font = AppFonts.Body;

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 1,
            BackColor = AppColors.PageBackground,
            Padding = new Padding(AppSpacing.Xl)
        };
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

        var card = new ModernCard
        {
            Dock = DockStyle.Fill
        };

        var content = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 6,
            BackColor = Color.Transparent
        };
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 32f));
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 38f));
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 38f));
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 38f));
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 32f));
        content.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

        var titleLabel = CreateLabel("Global Kisayollar", AppFonts.SectionTitle, AppColors.Primary);
        content.Controls.Add(titleLabel, 0, 0);

        _recordComboBox = CreateHotkeyComboBox(recordToggleVirtualKey);
        _playbackComboBox = CreateHotkeyComboBox(playbackToggleVirtualKey);
        _stopComboBox = CreateHotkeyComboBox(stopVirtualKey);

        content.Controls.Add(CreateInputRow("Kaydi Baslat / Durdur", _recordComboBox), 0, 1);
        content.Controls.Add(CreateInputRow("Oynat / Duraklat", _playbackComboBox), 0, 2);
        content.Controls.Add(CreateInputRow("Acil Durdurma", _stopComboBox), 0, 3);

        _errorLabel = CreateLabel("Yalnizca F1-F12 tuslari desteklenir.", AppFonts.Caption, AppColors.TextMuted);
        content.Controls.Add(_errorLabel, 0, 4);

        var actionsPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = Color.Transparent
        };
        actionsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
        actionsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));

        var cancelButton = new ModernButton
        {
            Text = "Vazgec",
            Variant = ModernButtonVariant.Ghost,
            Dock = DockStyle.Fill,
            Margin = new Padding(AppSpacing.Xs)
        };
        cancelButton.Click += (_, _) =>
        {
            DialogResult = DialogResult.Cancel;
            Close();
        };

        var saveButton = new ModernButton
        {
            Text = "Uygula",
            Variant = ModernButtonVariant.Primary,
            Dock = DockStyle.Fill,
            Margin = new Padding(AppSpacing.Xs)
        };
        saveButton.Click += (_, _) => SaveSelection();

        actionsPanel.Controls.Add(cancelButton, 0, 0);
        actionsPanel.Controls.Add(saveButton, 1, 0);
        content.Controls.Add(actionsPanel, 0, 5);

        card.Controls.Add(content);
        root.Controls.Add(card, 0, 0);
        Controls.Add(root);

        AcceptButton = saveButton;
        CancelButton = cancelButton;
    }

    public int RecordToggleVirtualKey => GetSelectedVirtualKey(_recordComboBox);

    public int PlaybackToggleVirtualKey => GetSelectedVirtualKey(_playbackComboBox);

    public int StopVirtualKey => GetSelectedVirtualKey(_stopComboBox);

    private void SaveSelection()
    {
        var recordVirtualKey = RecordToggleVirtualKey;
        var playbackVirtualKey = PlaybackToggleVirtualKey;
        var stopVirtualKey = StopVirtualKey;

        if (recordVirtualKey == playbackVirtualKey
            || recordVirtualKey == stopVirtualKey
            || playbackVirtualKey == stopVirtualKey)
        {
            _errorLabel.Text = "Her islem icin farkli bir kisayol secmelisin.";
            _errorLabel.ForeColor = AppColors.Danger;
            return;
        }

        DialogResult = DialogResult.OK;
        Close();
    }

    private static Control CreateInputRow(string labelText, ComboBox comboBox)
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = Color.Transparent,
            Margin = Padding.Empty
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60f));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40f));

        var label = CreateLabel(labelText, AppFonts.Body, AppColors.TextSecondary);
        label.Dock = DockStyle.Fill;
        layout.Controls.Add(label, 0, 0);
        layout.Controls.Add(comboBox, 1, 0);

        return layout;
    }

    private static ComboBox CreateHotkeyComboBox(int selectedVirtualKey)
    {
        var comboBox = new ComboBox
        {
            Dock = DockStyle.Fill,
            DropDownStyle = ComboBoxStyle.DropDownList,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(10, 15, 28),
            ForeColor = AppColors.TextPrimary,
            Font = AppFonts.Body,
            Margin = new Padding(AppSpacing.Sm, 0, 0, 0)
        };

        for (var virtualKey = 0x70; virtualKey <= 0x7B; virtualKey++)
        {
            comboBox.Items.Add(new HotkeyOption(virtualKey));
        }

        var defaultIndex = Math.Clamp(selectedVirtualKey - 0x70, 0, comboBox.Items.Count - 1);
        comboBox.SelectedIndex = defaultIndex;
        return comboBox;
    }

    private static int GetSelectedVirtualKey(ComboBox comboBox)
    {
        return comboBox.SelectedItem is HotkeyOption option
            ? option.VirtualKey
            : 0x77;
    }

    private static Label CreateLabel(string text, Font font, Color color)
    {
        return new Label
        {
            AutoSize = false,
            Dock = DockStyle.Fill,
            Text = text,
            Font = font,
            ForeColor = color,
            BackColor = Color.Transparent,
            TextAlign = ContentAlignment.MiddleLeft
        };
    }

    private sealed class HotkeyOption
    {
        public HotkeyOption(int virtualKey)
        {
            VirtualKey = virtualKey;
            DisplayText = MainForm.GetHotkeyDisplayText(virtualKey);
        }

        public int VirtualKey { get; }

        public string DisplayText { get; }

        public override string ToString()
        {
            return DisplayText;
        }
    }
}
