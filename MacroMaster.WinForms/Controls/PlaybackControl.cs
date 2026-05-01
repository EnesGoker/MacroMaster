using MacroMaster.WinForms.Theme;
using System.Globalization;

namespace MacroMaster.WinForms.Controls;

public readonly record struct PlaybackControlState(
    string StatusText,
    int PlayedEventCount,
    int TotalEventCount,
    int TotalDurationMs,
    double SpeedMultiplier,
    int RepeatCount,
    bool LoopIndefinitely,
    int InitialDelayMs,
    bool CanPlayback,
    bool CanStop,
    PlaybackButtonState PlaybackButtonState);

internal sealed class PlaybackControl : UserControl
{
    private readonly TableLayoutPanel _metricsLayoutPanel;
    private readonly Label _statusCaptionLabel;
    private readonly Label _statusValueLabel;
    private readonly Label _currentCaptionLabel;
    private readonly Label _currentValueLabel;
    private readonly Label _durationCaptionLabel;
    private readonly Label _durationValueLabel;
    private readonly Label _settingsCaptionLabel;
    private readonly Label _settingsValueLabel;
    private readonly Panel _progressTrackPanel;
    private readonly Panel _progressFillPanel;
    private readonly Label _progressLabel;
    private readonly Button _playbackButton;
    private readonly Button _stopButton;

    private double _progressRatio;

    public event EventHandler? PlaybackClicked;
    public event EventHandler? StopClicked;

    public PlaybackControl()
    {
        SetStyle(
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.ResizeRedraw,
            true);

        DoubleBuffered = true;
        BackColor = DesignTokens.Surface;
        ForeColor = DesignTokens.TextPrimary;
        Font = DesignTokens.FontUiNormal;

        _metricsLayoutPanel = new TableLayoutPanel();
        _statusCaptionLabel = CreateCaptionLabel("Durum");
        _statusValueLabel = CreateValueLabel("Hazir");
        _currentCaptionLabel = CreateCaptionLabel("Mevcut Olay");
        _currentValueLabel = CreateValueLabel("0 / 0");
        _durationCaptionLabel = CreateCaptionLabel("Toplam Sure");
        _durationValueLabel = CreateValueLabel("0 ms");
        _settingsCaptionLabel = CreateCaptionLabel("Ayarlar");
        _settingsValueLabel = CreateValueLabel("1x | 1 tekrar");
        _progressTrackPanel = new Panel();
        _progressFillPanel = new Panel();
        _progressLabel = CreateCaptionLabel("0%");
        _playbackButton = new Button();
        _stopButton = new Button();

        BuildLayout();
        WireEvents();
        UpdateState(new PlaybackControlState("Hazir", 0, 0, 0, 1, 1, false, 0, false, false, PlaybackButtonState.Play));
    }

    public void UpdateState(PlaybackControlState state)
    {
        int safeTotal = Math.Max(0, state.TotalEventCount);
        int safePlayed = Math.Clamp(state.PlayedEventCount, 0, safeTotal);

        _statusValueLabel.Text = state.StatusText;
        _currentValueLabel.Text = FormattableString.Invariant($"{safePlayed} / {safeTotal}");
        _durationValueLabel.Text = FormattableString.Invariant($"{Math.Max(0, state.TotalDurationMs)} ms");
        _settingsValueLabel.Text = FormatSettingsSummary(state);
        _progressRatio = safeTotal == 0
            ? 0
            : Math.Clamp((double)safePlayed / safeTotal, 0, 1);
        _progressLabel.Text = FormattableString.Invariant($"{_progressRatio:P0}");

        _playbackButton.Text = state.PlaybackButtonState switch
        {
            PlaybackButtonState.Pause => "Duraklat",
            PlaybackButtonState.Resume => "Devam Et",
            _ => "Oynat"
        };
        _playbackButton.Enabled = state.CanPlayback;
        _stopButton.Enabled = state.CanStop;

        ApplyButtonAccent(
            _playbackButton,
            state.PlaybackButtonState == PlaybackButtonState.Pause
                ? DesignTokens.AccentOrange
                : DesignTokens.Accent);
        ApplyButtonAccent(_stopButton, DesignTokens.AccentRed);
        UpdateProgressFill();
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        UpdateProgressFill();
    }

    private void BuildLayout()
    {
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
        rootLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 62f));
        rootLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 34f));
        rootLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

        ConfigureMetricsLayout();

        var progressLayoutPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = DesignTokens.Surface,
            Margin = new Padding(0, 4, 0, 0),
            Padding = Padding.Empty
        };
        progressLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        progressLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 54f));

        _progressTrackPanel.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        _progressTrackPanel.Height = 8;
        _progressTrackPanel.BackColor = DesignTokens.Background;
        _progressTrackPanel.Margin = new Padding(0, 0, 8, 0);
        _progressTrackPanel.Controls.Add(_progressFillPanel);
        _progressFillPanel.BackColor = DesignTokens.Accent;
        _progressFillPanel.Dock = DockStyle.Left;
        _progressFillPanel.Width = 0;
        _progressTrackPanel.Resize += (_, _) => UpdateProgressFill();

        _progressLabel.Dock = DockStyle.Fill;
        _progressLabel.TextAlign = ContentAlignment.MiddleRight;

        progressLayoutPanel.Controls.Add(_progressTrackPanel, 0, 0);
        progressLayoutPanel.Controls.Add(_progressLabel, 1, 0);

        var buttonLayoutPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            ColumnCount = 3,
            RowCount = 1,
            BackColor = DesignTokens.Surface,
            Height = 48,
            Margin = new Padding(0, 8, 0, 0),
            Padding = Padding.Empty
        };
        buttonLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
        buttonLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170f));
        buttonLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150f));
        buttonLayoutPanel.Controls.Add(_playbackButton, 1, 0);
        buttonLayoutPanel.Controls.Add(_stopButton, 2, 0);

        ConfigureCommandButton(_playbackButton, "Oynat");
        ConfigureCommandButton(_stopButton, "Durdur");

        rootLayoutPanel.Controls.Add(_metricsLayoutPanel, 0, 0);
        rootLayoutPanel.Controls.Add(progressLayoutPanel, 0, 1);
        rootLayoutPanel.Controls.Add(buttonLayoutPanel, 0, 2);
        Controls.Add(rootLayoutPanel);
    }

    private void ConfigureMetricsLayout()
    {
        _metricsLayoutPanel.Dock = DockStyle.Fill;
        _metricsLayoutPanel.ColumnCount = 4;
        _metricsLayoutPanel.RowCount = 2;
        _metricsLayoutPanel.BackColor = DesignTokens.Surface;
        _metricsLayoutPanel.Margin = Padding.Empty;
        _metricsLayoutPanel.Padding = Padding.Empty;
        _metricsLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
        _metricsLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
        _metricsLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
        _metricsLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
        _metricsLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 45f));
        _metricsLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 55f));

        AddMetric(_statusCaptionLabel, _statusValueLabel, 0);
        AddMetric(_currentCaptionLabel, _currentValueLabel, 1);
        AddMetric(_durationCaptionLabel, _durationValueLabel, 2);
        AddMetric(_settingsCaptionLabel, _settingsValueLabel, 3);
    }

    private void AddMetric(Label captionLabel, Label valueLabel, int column)
    {
        _metricsLayoutPanel.Controls.Add(captionLabel, column, 0);
        _metricsLayoutPanel.Controls.Add(valueLabel, column, 1);
    }

    private void WireEvents()
    {
        _playbackButton.Click += (_, _) => PlaybackClicked?.Invoke(this, EventArgs.Empty);
        _stopButton.Click += (_, _) => StopClicked?.Invoke(this, EventArgs.Empty);
    }

    private void UpdateProgressFill()
    {
        if (_progressTrackPanel.Width <= 0)
        {
            return;
        }

        _progressFillPanel.Width = Math.Clamp(
            (int)Math.Round(_progressTrackPanel.Width * _progressRatio),
            0,
            _progressTrackPanel.Width);
    }

    private static Label CreateCaptionLabel(string text)
    {
        return new Label
        {
            Dock = DockStyle.Fill,
            Text = text,
            Font = DesignTokens.FontUiNormal,
            ForeColor = DesignTokens.TextSecondary,
            BackColor = DesignTokens.Surface,
            TextAlign = ContentAlignment.BottomLeft
        };
    }

    private static Label CreateValueLabel(string text)
    {
        return new Label
        {
            Dock = DockStyle.Fill,
            Text = text,
            Font = DesignTokens.FontUiBold,
            ForeColor = DesignTokens.TextPrimary,
            BackColor = DesignTokens.Surface,
            TextAlign = ContentAlignment.TopLeft,
            AutoEllipsis = true
        };
    }

    private static void ConfigureCommandButton(Button button, string text)
    {
        button.Dock = DockStyle.Fill;
        button.Text = text;
        button.BackColor = DesignTokens.Surface2;
        button.ForeColor = DesignTokens.TextPrimary;
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderColor = DesignTokens.BorderBright;
        button.FlatAppearance.BorderSize = 1;
        button.Font = DesignTokens.FontUiBold;
        button.Margin = new Padding(8, 0, 0, 0);
        button.UseVisualStyleBackColor = false;
        button.Cursor = Cursors.Hand;
    }

    private static void ApplyButtonAccent(Button button, Color accent)
    {
        button.FlatAppearance.BorderColor = accent;
    }

    private static string FormatSettingsSummary(PlaybackControlState state)
    {
        string repeatText = state.LoopIndefinitely
            ? "sonsuz dongu"
            : state.RepeatCount <= 1
            ? "1 tekrar"
            : string.Create(CultureInfo.InvariantCulture, $"{state.RepeatCount} tekrar");
        string delayText = state.InitialDelayMs > 0
            ? string.Create(CultureInfo.InvariantCulture, $" | {state.InitialDelayMs} ms gecikme")
            : string.Empty;

        return string.Create(
            CultureInfo.InvariantCulture,
            $"{state.SpeedMultiplier:0.##}x | {repeatText}{delayText}");
    }
}
