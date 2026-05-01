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

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        using var borderPen = new Pen(DesignTokens.Border);
        var bounds = Rectangle.Inflate(ClientRectangle, -1, -1);
        e.Graphics.DrawRectangle(borderPen, bounds);
    }

    private void ApplyTheme()
    {
        BackColor = DesignTokens.Surface;
        ForeColor = DesignTokens.TextPrimary;
        Font = DesignTokens.FontUiNormal;
        MinimumSize = new Size(0, DesignTokens.BottomPanelHeight);

        titleLabel.ForeColor = DesignTokens.TextPrimary;
        titleLabel.Font = DesignTokens.FontUiBold;
        dividerPanel.BackColor = DesignTokens.Border;

        foreach (Control control in settingsLayoutPanel.Controls)
        {
            ApplyChildTheme(control);
        }
    }

    private void ApplyChildTheme(Control control)
    {
        control.ForeColor = DesignTokens.TextSecondary;
        control.BackColor = control is TextBox or ComboBox or NumericUpDown
            ? DesignTokens.Background
            : Color.Transparent;
        control.Font = control is Label label && label == titleLabel
            ? DesignTokens.FontUiBold
            : DesignTokens.FontUiNormal;

        if (control is NumericUpDown numericUpDown)
        {
            numericUpDown.BackColor = DesignTokens.Background;
            numericUpDown.ForeColor = DesignTokens.TextPrimary;
            numericUpDown.BorderStyle = BorderStyle.FixedSingle;
            numericUpDown.TextAlign = HorizontalAlignment.Right;
            return;
        }
        else if (control is ComboBox comboBox)
        {
            comboBox.BackColor = DesignTokens.Background;
            comboBox.ForeColor = DesignTokens.TextPrimary;
            comboBox.FlatStyle = FlatStyle.Flat;
            comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            return;
        }
        else if (control is CheckBox checkBox)
        {
            checkBox.ForeColor = DesignTokens.TextPrimary;
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
