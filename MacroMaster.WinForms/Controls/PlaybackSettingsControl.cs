using MacroMaster.Domain.Models;
using MacroMaster.WinForms.Theme;

namespace MacroMaster.WinForms.Controls;

public partial class PlaybackSettingsControl : UserControl
{
    private bool _isApplyingSettings;

    public event EventHandler? SettingsChanged;

    public PlaybackSettingsControl()
    {
        InitializeComponent();
        SetStyle(
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.ResizeRedraw |
            ControlStyles.UserPaint,
            true);

        DoubleBuffered = true;
        ApplyTheme();
        PopulateSpeedOptions();
        speedComboBox.DrawItem += speedComboBox_DrawItem;
        WireEvents();
        ApplySettings(new PlaybackSettings());
    }

    public PlaybackSettings GetCurrentSettings()
    {
        return new PlaybackSettings
        {
            SpeedMultiplier = preserveTimingCheckBox.Checked
                ? 1.0
                : GetSelectedSpeedMultiplier(),
            RepeatCount = Decimal.ToInt32(repeatCountNumericUpDown.Value),
            InitialDelayMs = Decimal.ToInt32(initialDelayNumericUpDown.Value),
            LoopIndefinitely = loopIndefinitelyCheckBox.Checked,
            UseRelativeCoordinates = relativeCoordinatesCheckBox.Checked,
            StopOnError = stopOnErrorCheckBox.Checked,
            PreserveOriginalTiming = preserveTimingCheckBox.Checked
        };
    }

    public void ApplySettings(PlaybackSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        _isApplyingSettings = true;

        try
        {
            SelectSpeedMultiplier(settings.SpeedMultiplier);
            repeatCountNumericUpDown.Value = ClampDecimal(
                settings.RepeatCount,
                repeatCountNumericUpDown.Minimum,
                repeatCountNumericUpDown.Maximum);
            initialDelayNumericUpDown.Value = ClampDecimal(
                settings.InitialDelayMs,
                initialDelayNumericUpDown.Minimum,
                initialDelayNumericUpDown.Maximum);
            preserveTimingCheckBox.Checked = settings.PreserveOriginalTiming;
            stopOnErrorCheckBox.Checked = settings.StopOnError;
            loopIndefinitelyCheckBox.Checked = settings.LoopIndefinitely;
            relativeCoordinatesCheckBox.Checked = settings.UseRelativeCoordinates;
            RefreshDependentControlState();
        }
        finally
        {
            _isApplyingSettings = false;
        }
    }

    public void SetControlsEnabled(bool enabled)
    {
        preserveTimingCheckBox.Enabled = enabled;
        stopOnErrorCheckBox.Enabled = enabled;
        loopIndefinitelyCheckBox.Enabled = enabled;
        relativeCoordinatesCheckBox.Enabled = enabled;
        initialDelayNumericUpDown.Enabled = enabled;
        speedComboBox.Enabled = enabled && !preserveTimingCheckBox.Checked;
        repeatCountNumericUpDown.Enabled = enabled && !loopIndefinitelyCheckBox.Checked;
    }

    private void ApplyTheme()
    {
        BackColor = DesignTokens.Surface;
        ForeColor = DesignTokens.TextPrimary;
        Font = DesignTokens.FontUiNormal;
        MinimumSize = new Size(0, DesignTokens.Scale(160));
        ApplyDensityScale();

        rootLayoutPanel.BackColor = DesignTokens.Surface;
        settingsLayoutPanel.BackColor = DesignTokens.Surface;
        dividerPanel.BackColor = DesignTokens.BorderSoft;
        initialDelayUnitLabel.ForeColor = DesignTokens.TextSecondary;
        initialDelayUnitLabel.BackColor = Color.Transparent;
        initialDelayUnitLabel.Font = DesignTokens.FontUiNormal;

        foreach (Control control in settingsLayoutPanel.Controls)
        {
            ApplyChildTheme(control);
        }
    }

    private static void ApplyChildTheme(Control control)
    {
        control.ForeColor = DesignTokens.TextSecondary;
        control.BackColor = control is TextBox or ComboBox or NumericUpDown
            ? DesignTokens.SurfaceInset
            : Color.Transparent;
        control.Font = DesignTokens.FontUiNormal;

        if (control is NumericUpDown numericUpDown)
        {
            numericUpDown.BackColor = DesignTokens.SurfaceInset;
            numericUpDown.ForeColor = DesignTokens.TextPrimary;
            numericUpDown.BorderStyle = BorderStyle.FixedSingle;
            numericUpDown.TextAlign = HorizontalAlignment.Left;
            numericUpDown.MinimumSize = new Size(0, DesignTokens.Scale(32));
            return;
        }
        else if (control is ComboBox comboBox)
        {
            comboBox.BackColor = DesignTokens.SurfaceInset;
            comboBox.ForeColor = DesignTokens.TextPrimary;
            comboBox.FlatStyle = FlatStyle.Flat;
            comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox.DrawMode = DrawMode.OwnerDrawFixed;
            comboBox.ItemHeight = DesignTokens.Scale(26);
            comboBox.MinimumSize = new Size(0, DesignTokens.Scale(32));
            return;
        }
        else if (control is CheckBox checkBox)
        {
            checkBox.ForeColor = DesignTokens.TextPrimary;
            checkBox.FlatStyle = FlatStyle.Flat;
            checkBox.FlatAppearance.BorderColor = DesignTokens.BorderBright;
            checkBox.FlatAppearance.CheckedBackColor = DesignTokens.AccentDeep;
            checkBox.FlatAppearance.MouseOverBackColor = DesignTokens.SurfaceHover;
        }

        foreach (Control child in control.Controls)
        {
            ApplyChildTheme(child);
        }
    }

    private void ApplyDensityScale()
    {
        rootLayoutPanel.Padding = new Padding(
            DesignTokens.Scale(18),
            DesignTokens.Scale(10),
            DesignTokens.Scale(18),
            DesignTokens.Scale(10));

        // Col 0: label, Col 1: input, Col 2: unit(32px), Col 3: divider(1px), Col 4: checkboxes
        settingsLayoutPanel.ColumnStyles[2].Width = DesignTokens.Scale(32);
        settingsLayoutPanel.ColumnStyles[3].Width = Math.Max(1, DesignTokens.Scale(1));

        int inputTopMargin = DesignTokens.Scale(4);
        int checkLeftMargin = DesignTokens.Scale(15);
        speedComboBox.Margin = new Padding(DesignTokens.Scale(3), inputTopMargin, DesignTokens.Scale(3), inputTopMargin);
        repeatCountNumericUpDown.Margin = speedComboBox.Margin;
        initialDelayNumericUpDown.Margin = speedComboBox.Margin;
        preserveTimingCheckBox.Margin = new Padding(checkLeftMargin, inputTopMargin, DesignTokens.Scale(3), inputTopMargin);
        loopIndefinitelyCheckBox.Margin = preserveTimingCheckBox.Margin;
        stopOnErrorCheckBox.Margin = preserveTimingCheckBox.Margin;
        relativeCoordinatesCheckBox.Margin = preserveTimingCheckBox.Margin;
    }

    private void PopulateSpeedOptions()
    {
        speedComboBox.Items.Clear();
        speedComboBox.Items.AddRange(["0.25x", "0.50x", "1.00x", "2.00x", "4.00x"]);
        speedComboBox.SelectedIndex = 2;
    }

    private void speedComboBox_DrawItem(object? sender, DrawItemEventArgs e)
    {
        _ = sender;

        if (e.Index < 0)
        {
            return;
        }

        bool isSelected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
        Color backColor = isSelected ? DesignTokens.AccentSoft : DesignTokens.SurfaceInset;
        Color textColor = isSelected ? DesignTokens.TextPrimary : DesignTokens.TextSecondary;

        using var backgroundBrush = new SolidBrush(backColor);
        e.Graphics.FillRectangle(backgroundBrush, e.Bounds);

        string text = speedComboBox.Items[e.Index]?.ToString() ?? string.Empty;
        var textBounds = new Rectangle(e.Bounds.Left + 8, e.Bounds.Top, e.Bounds.Width - 12, e.Bounds.Height);
        TextRenderer.DrawText(
            e.Graphics,
            text,
            DesignTokens.FontUiNormal,
            textBounds,
            textColor,
            TextFormatFlags.VerticalCenter |
            TextFormatFlags.Left |
            TextFormatFlags.EndEllipsis);
    }

    private void WireEvents()
    {
        speedComboBox.SelectedIndexChanged += OnSettingChanged;
        repeatCountNumericUpDown.ValueChanged += OnSettingChanged;
        initialDelayNumericUpDown.ValueChanged += OnSettingChanged;
        preserveTimingCheckBox.CheckedChanged += OnSettingChanged;
        stopOnErrorCheckBox.CheckedChanged += OnSettingChanged;
        loopIndefinitelyCheckBox.CheckedChanged += OnSettingChanged;
        relativeCoordinatesCheckBox.CheckedChanged += OnSettingChanged;
    }

    private void OnSettingChanged(object? sender, EventArgs e)
    {
        _ = sender;
        _ = e;

        RefreshDependentControlState();

        if (!_isApplyingSettings)
        {
            SettingsChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private void RefreshDependentControlState()
    {
        speedComboBox.Enabled = !preserveTimingCheckBox.Checked;
        repeatCountNumericUpDown.Enabled = !loopIndefinitelyCheckBox.Checked;
    }

    private double GetSelectedSpeedMultiplier()
    {
        return speedComboBox.SelectedItem?.ToString() switch
        {
            "0.25x" => 0.25,
            "0.50x" => 0.5,
            "2.00x" => 2.0,
            "4.00x" => 4.0,
            _ => 1.0
        };
    }

    private void SelectSpeedMultiplier(double speedMultiplier)
    {
        var targetText = speedMultiplier switch
        {
            <= 0.25 => "0.25x",
            <= 0.5 => "0.50x",
            >= 4.0 => "4.00x",
            >= 2.0 => "2.00x",
            _ => "1.00x"
        };

        speedComboBox.SelectedItem = targetText;
    }

    private static decimal ClampDecimal(
        decimal value,
        decimal minimum,
        decimal maximum)
    {
        return Math.Min(Math.Max(value, minimum), maximum);
    }
}
