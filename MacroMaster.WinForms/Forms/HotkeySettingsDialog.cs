using MacroMaster.Application.Abstractions;

namespace MacroMaster.WinForms.Forms;

public sealed class HotkeySettingsDialog : Form
{
    private const int ComboBoxWidth = 170;
    private static readonly Size MinimumClientSize = new(560, 220);

    private static readonly IReadOnlyList<HotkeyModifierOption> ModifierOptions = CreateModifierOptions();
    private static readonly IReadOnlyList<HotkeyKeyOption> KeyOptions = CreateKeyOptions();

    private readonly ComboBox _recordModifierComboBox = CreateComboBox();
    private readonly ComboBox _recordKeyComboBox = CreateComboBox();
    private readonly ComboBox _playbackModifierComboBox = CreateComboBox();
    private readonly ComboBox _playbackKeyComboBox = CreateComboBox();
    private readonly ComboBox _stopModifierComboBox = CreateComboBox();
    private readonly ComboBox _stopKeyComboBox = CreateComboBox();

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
        AutoScaleMode = AutoScaleMode.Font;
        AutoSize = false;
        Padding = new Padding(12);

        Button applyButton = CreateButton("Uygula");
        Button cancelButton = CreateButton("Vazgec");
        cancelButton.DialogResult = DialogResult.Cancel;
        Button resetDefaultsButton = CreateButton("Varsayilanlara Don");

        applyButton.Click += applyButton_Click;
        resetDefaultsButton.Click += resetDefaultsButton_Click;

        AcceptButton = applyButton;
        CancelButton = cancelButton;

        PopulateComboBox(_recordModifierComboBox, ModifierOptions);
        PopulateComboBox(_playbackModifierComboBox, ModifierOptions);
        PopulateComboBox(_stopModifierComboBox, ModifierOptions);
        PopulateComboBox(_recordKeyComboBox, KeyOptions);
        PopulateComboBox(_playbackKeyComboBox, KeyOptions);
        PopulateComboBox(_stopKeyComboBox, KeyOptions);

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

        Size preferredContentSize = GetPreferredContentSize(
            settingsTableLayoutPanel,
            buttonFlowLayoutPanel);
        Size computedClientSize = new(
            preferredContentSize.Width + Padding.Horizontal,
            preferredContentSize.Height + Padding.Vertical);
        ClientSize = new Size(
            Math.Max(computedClientSize.Width, MinimumClientSize.Width),
            Math.Max(computedClientSize.Height, MinimumClientSize.Height));
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
        SelectKey(_recordKeyComboBox, hotkeySettings.RecordToggleHotkey.VirtualKeyCode);
        SelectKey(_playbackKeyComboBox, hotkeySettings.PlaybackToggleHotkey.VirtualKeyCode);
        SelectKey(_stopKeyComboBox, hotkeySettings.StopHotkey.VirtualKeyCode);
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
                GetSelectedModifiers(_stopModifierComboBox))
        };
    }

    private static TableLayoutPanel CreateRootLayoutPanel(
        TableLayoutPanel settingsTableLayoutPanel,
        FlowLayoutPanel buttonFlowLayoutPanel)
    {
        TableLayoutPanel rootLayoutPanel = new()
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 1,
            RowCount = 2,
            Dock = DockStyle.Fill,
            Margin = Padding.Empty,
            Padding = Padding.Empty,
            GrowStyle = TableLayoutPanelGrowStyle.FixedSize
        };

        rootLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        rootLayoutPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        rootLayoutPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        rootLayoutPanel.Controls.Add(settingsTableLayoutPanel, 0, 0);
        rootLayoutPanel.Controls.Add(buttonFlowLayoutPanel, 0, 1);

        return rootLayoutPanel;
    }

    private static TableLayoutPanel CreateSettingsTableLayoutPanel()
    {
        TableLayoutPanel settingsTableLayoutPanel = new()
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 3,
            RowCount = 4,
            Margin = Padding.Empty,
            Padding = Padding.Empty,
            GrowStyle = TableLayoutPanelGrowStyle.FixedSize
        };

        settingsTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        settingsTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        settingsTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

        return settingsTableLayoutPanel;
    }

    private static FlowLayoutPanel CreateButtonFlowLayoutPanel(params Button[] buttons)
    {
        FlowLayoutPanel buttonFlowLayoutPanel = new()
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            FlowDirection = FlowDirection.RightToLeft,
            Margin = new Padding(0, 12, 0, 0),
            Padding = Padding.Empty,
            WrapContents = false,
            Anchor = AnchorStyles.Right
        };

        foreach (Button button in buttons)
        {
            buttonFlowLayoutPanel.Controls.Add(button);
        }

        return buttonFlowLayoutPanel;
    }

    private static Size GetPreferredContentSize(
        TableLayoutPanel settingsTableLayoutPanel,
        FlowLayoutPanel buttonFlowLayoutPanel)
    {
        Size settingsSize = settingsTableLayoutPanel.GetPreferredSize(Size.Empty);
        Size buttonSize = buttonFlowLayoutPanel.GetPreferredSize(Size.Empty);

        int width = Math.Max(
            settingsSize.Width,
            buttonSize.Width + buttonFlowLayoutPanel.Margin.Horizontal);
        int height = settingsSize.Height
            + buttonSize.Height
            + buttonFlowLayoutPanel.Margin.Vertical;

        return new Size(width, height);
    }

    private static Button CreateButton(string text)
    {
        return new Button
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Margin = new Padding(6, 0, 0, 0),
            Text = text,
            UseVisualStyleBackColor = true
        };
    }

    private static ComboBox CreateComboBox()
    {
        return new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            DropDownWidth = ComboBoxWidth,
            Width = ComboBoxWidth,
            Margin = new Padding(0, 6, 12, 0)
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
        tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));

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
        tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        modifierComboBox.Anchor = AnchorStyles.Left;
        keyComboBox.Anchor = AnchorStyles.Left;
        keyComboBox.Margin = new Padding(0, 6, 0, 0);

        tableLayoutPanel.Controls.Add(
            new Label
            {
                AutoSize = true,
                Anchor = AnchorStyles.Left,
                Text = actionLabel,
                Margin = new Padding(0, 9, 12, 0)
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
            AutoSize = true,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 162),
            Margin = new Padding(0, 0, 12, 6),
            Text = text
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
        return virtualKeyCode switch
        {
            0x08 => "Geri Sil",
            0x09 => "Tab",
            0x0D => "Enter",
            0x1B => "Esc",
            0x20 => "Bosluk",
            0x21 => "Sayfa Yukari",
            0x22 => "Sayfa Asagi",
            0x23 => "End",
            0x24 => "Home",
            0x25 => "Sol Ok",
            0x26 => "Yukari Ok",
            0x27 => "Sag Ok",
            0x28 => "Asagi Ok",
            0x2D => "Araya Ekle",
            0x2E => "Sil",
            0x2C => "Ekran Goruntusu",
            0x13 => "Duraklat",
            0x14 => "Buyuk Harf Kilidi",
            0x90 => "Sayi Kilidi",
            0x91 => "Kaydirma Kilidi",
            0x6A => "NumPad *",
            0x6B => "NumPad +",
            0x6D => "NumPad -",
            0x6E => "NumPad .",
            0x6F => "NumPad /",
            _ => FormatEnumKeyName(virtualKeyCode)
        };
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
