using MacroMaster.Application.Abstractions;
using MacroMaster.WinForms.Theme;

namespace MacroMaster.WinForms.Forms;

public sealed class HotkeySettingsDialog : Form
{
    private const int ComboBoxWidth = 190;
    private static readonly Size MinimumClientSize = new(DesignTokens.Scale(720), DesignTokens.Scale(392));

    private static readonly IReadOnlyList<HotkeyModifierOption> ModifierOptions = CreateModifierOptions();
    private static readonly IReadOnlyList<HotkeyKeyOption> KeyOptions = CreateKeyOptions();

    private readonly ComboBox _recordModifierComboBox = CreateComboBox();
    private readonly ComboBox _recordKeyComboBox = CreateComboBox();
    private readonly ComboBox _playbackModifierComboBox = CreateComboBox();
    private readonly ComboBox _playbackKeyComboBox = CreateComboBox();
    private readonly ComboBox _stopModifierComboBox = CreateComboBox();
    private readonly ComboBox _stopKeyComboBox = CreateComboBox();
    private readonly ComboBox _hotkeySettingsModifierComboBox = CreateComboBox();
    private readonly ComboBox _hotkeySettingsKeyComboBox = CreateComboBox();

    public HotkeySettingsDialog(HotkeySettings currentSettings)
    {
        ArgumentNullException.ThrowIfNull(currentSettings);

        SelectedHotkeySettings = currentSettings.Clone();

        SuspendLayout();

        Text = "Kisayol Ayarlari";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        AutoScaleMode = AutoScaleMode.None;
        AutoSize = false;
        BackColor = DesignTokens.Surface;
        ForeColor = DesignTokens.TextPrimary;
        Font = DesignTokens.FontUiNormal;
        Padding = new Padding(DesignTokens.Scale(18));

        Button applyButton = CreateButton("Kaydet", isPrimary: true);
        Button cancelButton = CreateButton("Iptal", isPrimary: false);
        Button resetDefaultsButton = CreateButton("Varsayilan", isPrimary: false);
        cancelButton.DialogResult = DialogResult.Cancel;

        applyButton.Click += applyButton_Click;
        resetDefaultsButton.Click += resetDefaultsButton_Click;

        AcceptButton = applyButton;
        CancelButton = cancelButton;

        PopulateComboBox(_recordModifierComboBox, ModifierOptions);
        PopulateComboBox(_playbackModifierComboBox, ModifierOptions);
        PopulateComboBox(_stopModifierComboBox, ModifierOptions);
        PopulateComboBox(_hotkeySettingsModifierComboBox, ModifierOptions);
        PopulateComboBox(_recordKeyComboBox, KeyOptions);
        PopulateComboBox(_playbackKeyComboBox, KeyOptions);
        PopulateComboBox(_stopKeyComboBox, KeyOptions);
        PopulateComboBox(_hotkeySettingsKeyComboBox, KeyOptions);

        TableLayoutPanel settingsTableLayoutPanel = CreateSettingsTableLayoutPanel();
        AddHeaderRow(settingsTableLayoutPanel);
        AddHotkeyRow(
            settingsTableLayoutPanel,
            1,
            "Kayit Degistir",
            _recordModifierComboBox,
            _recordKeyComboBox);
        AddHotkeyRow(
            settingsTableLayoutPanel,
            2,
            "Oynatma Degistir",
            _playbackModifierComboBox,
            _playbackKeyComboBox);
        AddHotkeyRow(
            settingsTableLayoutPanel,
            3,
            "Durdur",
            _stopModifierComboBox,
            _stopKeyComboBox);
        AddHotkeyRow(
            settingsTableLayoutPanel,
            4,
            "Kisayol Ayarlari",
            _hotkeySettingsModifierComboBox,
            _hotkeySettingsKeyComboBox);

        FlowLayoutPanel buttonFlowLayoutPanel = CreateButtonFlowLayoutPanel(
            cancelButton,
            applyButton,
            resetDefaultsButton);

        TableLayoutPanel rootLayoutPanel = CreateRootLayoutPanel(
            settingsTableLayoutPanel,
            buttonFlowLayoutPanel);

        Controls.Add(rootLayoutPanel);

        ApplyHotkeySettings(currentSettings);

        ResumeLayout(performLayout: true);

        ClientSize = MinimumClientSize;
        MinimumSize = SizeFromClientSize(ClientSize);
        PerformLayout();
    }

    public HotkeySettings SelectedHotkeySettings { get; private set; }

    private void applyButton_Click(object? sender, EventArgs e)
    {
        _ = sender;
        _ = e;

        try
        {
            HotkeySettings selectedHotkeySettings = BuildHotkeySettings();
            HotkeySettingsValidator.Validate(selectedHotkeySettings, "Kisayol ayarlari penceresi uygulama");
            SelectedHotkeySettings = selectedHotkeySettings;
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                this,
                ex.Message,
                "MacroMaster",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void resetDefaultsButton_Click(object? sender, EventArgs e)
    {
        _ = sender;
        _ = e;
        ApplyHotkeySettings(HotkeySettings.CreateDefault());
    }

    private void ApplyHotkeySettings(HotkeySettings hotkeySettings)
    {
        SelectModifier(_recordModifierComboBox, hotkeySettings.RecordToggleHotkey.Modifiers);
        SelectModifier(_playbackModifierComboBox, hotkeySettings.PlaybackToggleHotkey.Modifiers);
        SelectModifier(_stopModifierComboBox, hotkeySettings.StopHotkey.Modifiers);
        SelectModifier(_hotkeySettingsModifierComboBox, hotkeySettings.HotkeySettingsHotkey.Modifiers);
        SelectKey(_recordKeyComboBox, hotkeySettings.RecordToggleHotkey.VirtualKeyCode);
        SelectKey(_playbackKeyComboBox, hotkeySettings.PlaybackToggleHotkey.VirtualKeyCode);
        SelectKey(_stopKeyComboBox, hotkeySettings.StopHotkey.VirtualKeyCode);
        SelectKey(_hotkeySettingsKeyComboBox, hotkeySettings.HotkeySettingsHotkey.VirtualKeyCode);
    }

    private HotkeySettings BuildHotkeySettings()
    {
        return new HotkeySettings
        {
            RecordToggleHotkey = new HotkeyBinding(
                GetSelectedKey(_recordKeyComboBox),
                GetSelectedModifiers(_recordModifierComboBox)),
            PlaybackToggleHotkey = new HotkeyBinding(
                GetSelectedKey(_playbackKeyComboBox),
                GetSelectedModifiers(_playbackModifierComboBox)),
            StopHotkey = new HotkeyBinding(
                GetSelectedKey(_stopKeyComboBox),
                GetSelectedModifiers(_stopModifierComboBox)),
            HotkeySettingsHotkey = new HotkeyBinding(
                GetSelectedKey(_hotkeySettingsKeyComboBox),
                GetSelectedModifiers(_hotkeySettingsModifierComboBox))
        };
    }

    private static TableLayoutPanel CreateRootLayoutPanel(
        TableLayoutPanel settingsTableLayoutPanel,
        FlowLayoutPanel buttonFlowLayoutPanel)
    {
        TableLayoutPanel rootLayoutPanel = new()
        {
            AutoSize = false,
            ColumnCount = 1,
            RowCount = 3,
            Dock = DockStyle.Fill,
            Margin = Padding.Empty,
            Padding = Padding.Empty,
            BackColor = DesignTokens.Surface,
            GrowStyle = TableLayoutPanelGrowStyle.FixedSize
        };

        rootLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        rootLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, DesignTokens.Scale(64)));
        rootLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
        rootLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, DesignTokens.Scale(56)));
        rootLayoutPanel.Controls.Add(CreateHeaderPanel(), 0, 0);
        rootLayoutPanel.Controls.Add(settingsTableLayoutPanel, 0, 1);
        rootLayoutPanel.Controls.Add(buttonFlowLayoutPanel, 0, 2);

        return rootLayoutPanel;
    }

    private static TableLayoutPanel CreateHeaderPanel()
    {
        var headerLayoutPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = DesignTokens.Surface,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        headerLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        headerLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 55f));
        headerLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 45f));

        headerLayoutPanel.Controls.Add(
            new Label
            {
                Dock = DockStyle.Fill,
                Text = "Kisayol Ayarlari",
                Font = DesignTokens.FontUiBold,
                ForeColor = DesignTokens.TextPrimary,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleLeft
            },
            0,
            0);
        headerLayoutPanel.Controls.Add(
            new Label
            {
                Dock = DockStyle.Fill,
                Text = "Global kisayollari secin ve kaydedin.",
                Font = DesignTokens.FontUiSmall,
                ForeColor = DesignTokens.TextSecondary,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleLeft
            },
            0,
            1);

        return headerLayoutPanel;
    }

    private static TableLayoutPanel CreateSettingsTableLayoutPanel()
    {
        TableLayoutPanel settingsTableLayoutPanel = new()
        {
            AutoSize = false,
            ColumnCount = 3,
            RowCount = 5,
            Dock = DockStyle.Fill,
            Margin = Padding.Empty,
            Padding = new Padding(DesignTokens.Scale(14), DesignTokens.Scale(12), DesignTokens.Scale(14), DesignTokens.Scale(12)),
            BackColor = DesignTokens.SurfaceInset,
            GrowStyle = TableLayoutPanelGrowStyle.FixedSize
        };

        settingsTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34f));
        settingsTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33f));
        settingsTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33f));

        return settingsTableLayoutPanel;
    }

    private static FlowLayoutPanel CreateButtonFlowLayoutPanel(params Button[] buttons)
    {
        FlowLayoutPanel buttonFlowLayoutPanel = new()
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            BackColor = DesignTokens.Surface,
            Margin = Padding.Empty,
            Padding = new Padding(0, DesignTokens.Scale(12), 0, 0),
            WrapContents = false,
            Anchor = AnchorStyles.Right
        };

        foreach (Button button in buttons)
        {
            buttonFlowLayoutPanel.Controls.Add(button);
        }

        return buttonFlowLayoutPanel;
    }

    private static Button CreateButton(string text, bool isPrimary)
    {
        var button = new Button
        {
            Width = DesignTokens.Scale(118),
            Height = DesignTokens.Scale(34),
            Margin = new Padding(DesignTokens.Scale(8), 0, 0, 0),
            Text = text,
            FlatStyle = FlatStyle.Flat,
            UseVisualStyleBackColor = false,
            Font = DesignTokens.FontUiBold,
            BackColor = isPrimary ? DesignTokens.AccentDeep : DesignTokens.Surface2,
            ForeColor = DesignTokens.TextPrimary
        };

        button.FlatAppearance.BorderColor = isPrimary
            ? DesignTokens.Accent
            : DesignTokens.BorderBright;
        button.FlatAppearance.BorderSize = 1;
        return button;
    }

    private static ComboBox CreateComboBox()
    {
        return new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            DropDownWidth = DesignTokens.Scale(ComboBoxWidth),
            Dock = DockStyle.Fill,
            Width = DesignTokens.Scale(ComboBoxWidth),
            Margin = new Padding(0, DesignTokens.Scale(4), DesignTokens.Scale(10), DesignTokens.Scale(4)),
            FlatStyle = FlatStyle.Flat,
            BackColor = DesignTokens.Surface,
            ForeColor = DesignTokens.TextPrimary,
            Font = DesignTokens.FontUiNormal
        };
    }

    private static void PopulateComboBox<TOption>(
        ComboBox comboBox,
        IReadOnlyList<TOption> options)
    {
        comboBox.DisplayMember = nameof(HotkeyModifierOption.DisplayText);
        comboBox.DataSource = options.ToList();
    }

    private static void AddHeaderRow(TableLayoutPanel tableLayoutPanel)
    {
        tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, DesignTokens.Scale(30)));

        tableLayoutPanel.Controls.Add(CreateHeaderLabel("Islem"), 0, 0);
        tableLayoutPanel.Controls.Add(CreateHeaderLabel("Degistirici"), 1, 0);
        tableLayoutPanel.Controls.Add(CreateHeaderLabel("Tus"), 2, 0);
    }

    private static void AddHotkeyRow(
        TableLayoutPanel tableLayoutPanel,
        int rowIndex,
        string actionLabel,
        ComboBox modifierComboBox,
        ComboBox keyComboBox)
    {
        tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, DesignTokens.Scale(48)));

        modifierComboBox.Dock = DockStyle.Fill;
        keyComboBox.Dock = DockStyle.Fill;
        keyComboBox.Margin = new Padding(0, DesignTokens.Scale(4), 0, DesignTokens.Scale(4));

        tableLayoutPanel.Controls.Add(
            new Label
            {
                Dock = DockStyle.Fill,
                Text = actionLabel,
                Font = DesignTokens.FontUiBold,
                ForeColor = DesignTokens.TextPrimary,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = new Padding(0, 0, DesignTokens.Scale(12), 0)
            },
            0,
            rowIndex);
        tableLayoutPanel.Controls.Add(modifierComboBox, 1, rowIndex);
        tableLayoutPanel.Controls.Add(keyComboBox, 2, rowIndex);
    }

    private static Label CreateHeaderLabel(string text)
    {
        return new Label
        {
            Dock = DockStyle.Fill,
            Font = DesignTokens.FontUiSmall,
            ForeColor = DesignTokens.TextSecondary,
            BackColor = Color.Transparent,
            Margin = new Padding(0, 0, DesignTokens.Scale(12), DesignTokens.Scale(4)),
            Text = text,
            TextAlign = ContentAlignment.MiddleLeft
        };
    }

    private static void SelectModifier(
        ComboBox comboBox,
        HotkeyModifiers modifiers)
    {
        comboBox.SelectedItem = ModifierOptions.First(
            option => option.Modifiers == modifiers);
    }

    private static void SelectKey(
        ComboBox comboBox,
        int virtualKeyCode)
    {
        HotkeyKeyOption? keyOption = KeyOptions.FirstOrDefault(
            option => option.VirtualKeyCode == virtualKeyCode);

        if (keyOption is null)
        {
            throw new InvalidOperationException(
                $"Kisayol duzenleyici {virtualKeyCode} sanal tus kodunu desteklemiyor.");
        }

        comboBox.SelectedItem = keyOption;
    }

    private static HotkeyModifiers GetSelectedModifiers(ComboBox comboBox)
    {
        return comboBox.SelectedItem is HotkeyModifierOption modifierOption
            ? modifierOption.Modifiers
            : throw new InvalidOperationException("Bir kisayol degistiricisi secilmelidir.");
    }

    private static int GetSelectedKey(ComboBox comboBox)
    {
        return comboBox.SelectedItem is HotkeyKeyOption keyOption
            ? keyOption.VirtualKeyCode
            : throw new InvalidOperationException("Bir kisayol tusu secilmelidir.");
    }

    private static List<HotkeyModifierOption> CreateModifierOptions()
    {
        return
        [
            new HotkeyModifierOption("Yok", HotkeyModifiers.None),
            new HotkeyModifierOption("Ctrl", HotkeyModifiers.Control),
            new HotkeyModifierOption("Shift", HotkeyModifiers.Shift),
            new HotkeyModifierOption("Alt", HotkeyModifiers.Alt),
            new HotkeyModifierOption("Win", HotkeyModifiers.Windows),
            new HotkeyModifierOption("Ctrl+Shift", HotkeyModifiers.Control | HotkeyModifiers.Shift),
            new HotkeyModifierOption("Ctrl+Alt", HotkeyModifiers.Control | HotkeyModifiers.Alt),
            new HotkeyModifierOption("Ctrl+Win", HotkeyModifiers.Control | HotkeyModifiers.Windows),
            new HotkeyModifierOption("Shift+Alt", HotkeyModifiers.Shift | HotkeyModifiers.Alt),
            new HotkeyModifierOption("Shift+Win", HotkeyModifiers.Shift | HotkeyModifiers.Windows),
            new HotkeyModifierOption("Alt+Win", HotkeyModifiers.Alt | HotkeyModifiers.Windows),
            new HotkeyModifierOption(
                "Ctrl+Shift+Alt",
                HotkeyModifiers.Control | HotkeyModifiers.Shift | HotkeyModifiers.Alt),
            new HotkeyModifierOption(
                "Ctrl+Shift+Win",
                HotkeyModifiers.Control | HotkeyModifiers.Shift | HotkeyModifiers.Windows),
            new HotkeyModifierOption(
                "Ctrl+Alt+Win",
                HotkeyModifiers.Control | HotkeyModifiers.Alt | HotkeyModifiers.Windows),
            new HotkeyModifierOption(
                "Shift+Alt+Win",
                HotkeyModifiers.Shift | HotkeyModifiers.Alt | HotkeyModifiers.Windows),
            new HotkeyModifierOption(
                "Ctrl+Shift+Alt+Win",
                HotkeyModifiers.Control | HotkeyModifiers.Shift | HotkeyModifiers.Alt | HotkeyModifiers.Windows)
        ];
    }

    private static List<HotkeyKeyOption> CreateKeyOptions()
    {
        List<HotkeyKeyOption> options = [];

        for (int keyCode = 1; keyCode <= 0xFE; keyCode++)
        {
            if (HotkeySettingsValidator.UnsupportedPrimaryVirtualKeys.Contains(keyCode))
            {
                continue;
            }

            string displayText = GetKeyDisplayText(keyCode);
            options.Add(new HotkeyKeyOption(displayText, keyCode));
        }

        return options
            .OrderBy(option => option.DisplayText, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static string GetKeyDisplayText(int virtualKeyCode)
    {
        return VirtualKeyDisplayNameFormatter.Format(virtualKeyCode);
    }

    private static string FormatEnumKeyName(int virtualKeyCode)
    {
        string keyName = ((Keys)virtualKeyCode).ToString();
        return int.TryParse(keyName, out _)
            ? $"VK {virtualKeyCode}"
            : keyName;
    }

    private sealed record HotkeyModifierOption(
        string DisplayText,
        HotkeyModifiers Modifiers);

    private sealed record HotkeyKeyOption(
        string DisplayText,
        int VirtualKeyCode);
}
