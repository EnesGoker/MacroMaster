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
        MinimumSize = new Size(0, DesignTokens.BottomPanelHeight);

        rootLayoutPanel.BackColor = DesignTokens.Surface;
        settingsLayoutPanel.BackColor = DesignTokens.Surface;
        titleLabel.ForeColor = DesignTokens.TextPrimary;
        titleLabel.Font = DesignTokens.FontUiBold;
        dividerPanel.BackColor = DesignTokens.BorderSoft;

        foreach (Control control in settingsLayoutPanel.Controls)
        {
            ApplyChildTheme(control);
        }
    }

    private void ApplyChildTheme(Control control)
    {
        control.ForeColor = DesignTokens.TextSecondary;
        control.BackColor = control is TextBox or ComboBox or NumericUpDown
            ? DesignTokens.SurfaceInset
            : Color.Transparent;
        control.Font = control is Label label && label == titleLabel
            ? DesignTokens.FontUiBold
            : DesignTokens.FontUiNormal;

        if (control is NumericUpDown numericUpDown)
        {
            numericUpDown.BackColor = DesignTokens.SurfaceInset;
            numericUpDown.ForeColor = DesignTokens.TextPrimary;
            numericUpDown.BorderStyle = BorderStyle.FixedSingle;
            numericUpDown.TextAlign = HorizontalAlignment.Right;
            return;
        }
        else if (control is ComboBox comboBox)
        {
            comboBox.BackColor = DesignTokens.SurfaceInset;
            comboBox.ForeColor = DesignTokens.TextPrimary;
            comboBox.FlatStyle = FlatStyle.Flat;
            comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox.DrawMode = DrawMode.OwnerDrawFixed;
            comboBox.ItemHeight = 24;
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
