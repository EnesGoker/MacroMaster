using MacroMaster.WinForms.Theme;
using System.Drawing.Drawing2D;
using System.Globalization;

namespace MacroMaster.WinForms.Controls;

public readonly record struct PlaybackControlState(
    string StatusText,
    int PlayedEventCount,
    int TotalEventCount,
    int PlayedDurationMs,
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
    private readonly PlaybackProgressBar _progressBar;
    private readonly Label _elapsedTimeLabel;
    private readonly Label _remainingTimeLabel;
    private readonly Label _statusLabel;
    private readonly Label _eventCounterLabel;
    private readonly Label _settingsLabel;
    private readonly IconButton _backButton;
    private readonly IconButton _playbackButton;
    private readonly IconButton _stopButton;
    private readonly ToolTip _toolTip;

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

        _progressBar = new PlaybackProgressBar();
        _elapsedTimeLabel = CreateTimeLabel(ContentAlignment.MiddleLeft);
        _remainingTimeLabel = CreateTimeLabel(ContentAlignment.MiddleRight);
        _statusLabel = CreateMetaLabel(ContentAlignment.MiddleLeft);
        _eventCounterLabel = CreateMetaLabel(ContentAlignment.MiddleCenter);
        _settingsLabel = CreateMetaLabel(ContentAlignment.MiddleRight);
        _backButton = new IconButton(IconButtonKind.Previous);
        _playbackButton = new IconButton(IconButtonKind.Play) { ButtonSize = DesignTokens.Scale(56) };
        _stopButton = new IconButton(IconButtonKind.Stop);
        _toolTip = new ToolTip();

        BuildLayout();
        WireEvents();
        UpdateState(new PlaybackControlState("Hazir", 0, 0, 0, 0, 1, 1, false, 0, false, false, PlaybackButtonState.Play));
    }

    public void UpdateState(PlaybackControlState state)
    {
        int safeTotalEvents = Math.Max(0, state.TotalEventCount);
        int safePlayedEvents = Math.Clamp(state.PlayedEventCount, 0, safeTotalEvents);
        int safeTotalDurationMs = Math.Max(0, state.TotalDurationMs);
        int safePlayedDurationMs = Math.Clamp(state.PlayedDurationMs, 0, safeTotalDurationMs);
        double progress = ResolveProgressRatio(
            safePlayedEvents,
            safeTotalEvents,
            safePlayedDurationMs,
            safeTotalDurationMs);

        _progressBar.Progress = progress;
        _elapsedTimeLabel.Text = FormatDuration(safePlayedDurationMs);
        _remainingTimeLabel.Text = "-" + FormatDuration(Math.Max(0, safeTotalDurationMs - safePlayedDurationMs));
        _statusLabel.Text = state.StatusText;
        _eventCounterLabel.Text = FormattableString.Invariant($"{safePlayedEvents} / {safeTotalEvents} olay");
        _settingsLabel.Text = FormatSettingsSummary(state);

        _playbackButton.Kind = state.PlaybackButtonState switch
        {
            PlaybackButtonState.Pause => IconButtonKind.Pause,
            _ => IconButtonKind.Play
        };

        _playbackButton.Enabled = state.CanPlayback;
        _stopButton.Enabled = state.CanStop;
        _backButton.Enabled = state.CanStop;

        _toolTip.SetToolTip(_backButton, "Oynatmayi durdur");
        _toolTip.SetToolTip(_playbackButton, state.PlaybackButtonState switch
        {
            PlaybackButtonState.Pause => "Duraklat",
            PlaybackButtonState.Resume => "Devam et",
            _ => "Oynat"
        });
        _toolTip.SetToolTip(_stopButton, "Durdur");
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _toolTip.Dispose();
        }

        base.Dispose(disposing);
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
            Padding = new Padding(0, DesignTokens.Scale(2), 0, 0)
        };
        rootLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        rootLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, DesignTokens.Scale(62)));
        rootLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, DesignTokens.Scale(34)));
        rootLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

        var progressLayoutPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 2,
            BackColor = DesignTokens.Surface,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        progressLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
        progressLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
        progressLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, DesignTokens.Scale(30)));
        progressLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
        progressLayoutPanel.Controls.Add(_progressBar, 0, 0);
        progressLayoutPanel.SetColumnSpan(_progressBar, 2);
        progressLayoutPanel.Controls.Add(_elapsedTimeLabel, 0, 1);
        progressLayoutPanel.Controls.Add(_remainingTimeLabel, 1, 1);

        var metaLayoutPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 1,
            BackColor = DesignTokens.Surface,
            Margin = new Padding(0, 0, 0, DesignTokens.Scale(2)),
            Padding = Padding.Empty
        };
        metaLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34f));
        metaLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 32f));
        metaLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34f));
        metaLayoutPanel.Controls.Add(_statusLabel, 0, 0);
        metaLayoutPanel.Controls.Add(_eventCounterLabel, 1, 0);
        metaLayoutPanel.Controls.Add(_settingsLabel, 2, 0);

        var controlsLayoutPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 5,
            RowCount = 1,
            BackColor = DesignTokens.Surface,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        controlsLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
        controlsLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, DesignTokens.Scale(72)));
        controlsLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, DesignTokens.Scale(92)));
        controlsLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, DesignTokens.Scale(72)));
        controlsLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
        controlsLayoutPanel.Controls.Add(_backButton, 1, 0);
        controlsLayoutPanel.Controls.Add(_playbackButton, 2, 0);
        controlsLayoutPanel.Controls.Add(_stopButton, 3, 0);

        rootLayoutPanel.Controls.Add(progressLayoutPanel, 0, 0);
        rootLayoutPanel.Controls.Add(metaLayoutPanel, 0, 1);
        rootLayoutPanel.Controls.Add(controlsLayoutPanel, 0, 2);
        Controls.Add(rootLayoutPanel);
    }

    private void WireEvents()
    {
        _backButton.Click += (_, _) => StopClicked?.Invoke(this, EventArgs.Empty);
        _playbackButton.Click += (_, _) => PlaybackClicked?.Invoke(this, EventArgs.Empty);
        _stopButton.Click += (_, _) => StopClicked?.Invoke(this, EventArgs.Empty);
    }

    private static double ResolveProgressRatio(
        int playedEventCount,
        int totalEventCount,
        int playedDurationMs,
        int totalDurationMs)
    {
        if (totalDurationMs > 0)
        {
            return Math.Clamp((double)playedDurationMs / totalDurationMs, 0, 1);
        }

        return totalEventCount == 0
            ? 0
            : Math.Clamp((double)playedEventCount / totalEventCount, 0, 1);
    }

    private static Label CreateTimeLabel(ContentAlignment alignment)
    {
        return new Label
        {
            Dock = DockStyle.Fill,
            Font = DesignTokens.FontUiBold,
            ForeColor = DesignTokens.TextPrimary,
            BackColor = DesignTokens.Surface,
            TextAlign = alignment
        };
    }

    private static Label CreateMetaLabel(ContentAlignment alignment)
    {
        return new Label
        {
            Dock = DockStyle.Fill,
            Font = DesignTokens.FontUiNormal,
            ForeColor = DesignTokens.TextSecondary,
            BackColor = DesignTokens.Surface,
            TextAlign = alignment,
            AutoEllipsis = true
        };
    }

    private static string FormatDuration(int milliseconds)
    {
        TimeSpan timeSpan = TimeSpan.FromMilliseconds(Math.Max(0, milliseconds));
        return timeSpan.TotalHours >= 1
            ? FormattableString.Invariant($"{(int)timeSpan.TotalHours:00}:{timeSpan.Minutes:00}:{timeSpan.Seconds:00}")
            : FormattableString.Invariant($"{timeSpan.Minutes:00}:{timeSpan.Seconds:00}");
    }

    private static string FormatSettingsSummary(PlaybackControlState state)
    {
        string repeatText = state.LoopIndefinitely
            ? "sonsuz"
            : state.RepeatCount <= 1
                ? "1 tekrar"
                : string.Create(CultureInfo.InvariantCulture, $"{state.RepeatCount} tekrar");
        string delayText = state.InitialDelayMs > 0
            ? string.Create(CultureInfo.InvariantCulture, $" | {state.InitialDelayMs} ms")
            : string.Empty;

        return string.Create(
            CultureInfo.InvariantCulture,
            $"{state.SpeedMultiplier:0.##}x | {repeatText}{delayText}");
    }

    private sealed class PlaybackProgressBar : Control
    {
        private double _progress;

        public PlaybackProgressBar()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint,
                true);

            Dock = DockStyle.Fill;
            Margin = Padding.Empty;
            BackColor = DesignTokens.Surface;
        }

        public double Progress
        {
            get => _progress;
            set
            {
                double normalized = Math.Clamp(value, 0, 1);
                if (Math.Abs(_progress - normalized) < 0.001)
                {
                    return;
                }

                _progress = normalized;
                Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            int trackHeight = DesignTokens.Scale(7);
            int thumbRadius = DesignTokens.Scale(7);
            int left = thumbRadius;
            int right = Width - thumbRadius;
            int y = Height / 2 - trackHeight / 2;
            int width = Math.Max(1, right - left);
            var trackBounds = new Rectangle(left, y, width, trackHeight);
            int fillWidth = (int)Math.Round(width * _progress);
            var fillBounds = new Rectangle(left, y, Math.Max(trackHeight, fillWidth), trackHeight);
            int thumbX = left + fillWidth;

            using var trackBrush = new SolidBrush(Color.FromArgb(70, 80, 96));
            using var fillBrush = new SolidBrush(DesignTokens.AccentDeep);
            using var thumbBrush = new SolidBrush(DesignTokens.Accent);
            using var thumbBorderPen = new Pen(DesignTokens.Surface, 2f);

            using GraphicsPath trackPath = CreateRoundPath(trackBounds, trackHeight);
            e.Graphics.FillPath(trackBrush, trackPath);

            if (fillWidth > 0)
            {
                using GraphicsPath fillPath = CreateRoundPath(fillBounds, trackHeight);
                e.Graphics.FillPath(fillBrush, fillPath);
            }

            var thumbBounds = new Rectangle(
                thumbX - thumbRadius,
                Height / 2 - thumbRadius,
                thumbRadius * 2,
                thumbRadius * 2);
            e.Graphics.FillEllipse(thumbBrush, thumbBounds);
            e.Graphics.DrawEllipse(thumbBorderPen, thumbBounds);
        }
    }

    private sealed class IconButton : Control
    {
        private IconButtonKind _kind;

        public IconButton(IconButtonKind kind)
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint,
                true);

            _kind = kind;
            Dock = DockStyle.Fill;
            Margin = new Padding(DesignTokens.Scale(4));
            BackColor = DesignTokens.Surface;
            Cursor = Cursors.Hand;
            ButtonSize = DesignTokens.Scale(46);
        }

        public int ButtonSize { get; init; }

        public IconButtonKind Kind
        {
            get => _kind;
            set
            {
                if (_kind == value)
                {
                    return;
                }

                _kind = value;
                Invalidate();
            }
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            Cursor = Enabled ? Cursors.Hand : Cursors.Default;
            Invalidate();
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            int inset = DesignTokens.Scale(4);
            int size = Math.Min(ButtonSize, Math.Min(Width - inset, Height - inset));
            var bounds = new Rectangle(
                (Width - size) / 2,
                (Height - size) / 2,
                size,
                size);
            bool isPrimary = Kind is IconButtonKind.Play or IconButtonKind.Pause;
            Color iconColor = Enabled
                ? DesignTokens.TextPrimary
                : DesignTokens.TextMuted;
            Color background = isPrimary
                ? DesignTokens.AccentDeep
                : DesignTokens.Surface2;
            Color border = Enabled
                ? isPrimary ? DesignTokens.Accent : DesignTokens.BorderBright
                : DesignTokens.Border;

            if (ClientRectangle.Contains(PointToClient(MousePosition)) && Enabled)
            {
                background = isPrimary
                    ? Color.FromArgb(35, 116, 239)
                    : DesignTokens.Surface3;
            }

            using GraphicsPath backgroundPath = CreateRoundPath(bounds, Math.Min(20, size / 2));
            using var backgroundBrush = new SolidBrush(background);
            using var borderPen = new Pen(border, 1f);
            e.Graphics.FillPath(backgroundBrush, backgroundPath);
            e.Graphics.DrawPath(borderPen, backgroundPath);

            using var iconBrush = new SolidBrush(iconColor);
            DrawIcon(e.Graphics, bounds, iconBrush);
        }

        private void DrawIcon(Graphics graphics, Rectangle bounds, Brush brush)
        {
            switch (Kind)
            {
                case IconButtonKind.Previous:
                    DrawPreviousIcon(graphics, bounds, brush);
                    break;
                case IconButtonKind.Pause:
                    DrawPauseIcon(graphics, bounds, brush);
                    break;
                case IconButtonKind.Stop:
                    DrawStopIcon(graphics, bounds, brush);
                    break;
                default:
                    DrawPlayIcon(graphics, bounds, brush);
                    break;
            }
        }

        private static void DrawPlayIcon(Graphics graphics, Rectangle bounds, Brush brush)
        {
            int iconSize = bounds.Width / 3;
            int x = bounds.Left + bounds.Width / 2 - iconSize / 3;
            int y = bounds.Top + bounds.Height / 2;
            Point[] points =
            [
                new(x - iconSize / 2, y - iconSize),
                new(x - iconSize / 2, y + iconSize),
                new(x + iconSize, y)
            ];
            graphics.FillPolygon(brush, points);
        }

        private static void DrawPauseIcon(Graphics graphics, Rectangle bounds, Brush brush)
        {
            int barWidth = Math.Max(4, bounds.Width / 9);
            int barHeight = bounds.Height / 3;
            int gap = barWidth;
            int x = bounds.Left + bounds.Width / 2 - barWidth - gap / 2;
            int y = bounds.Top + bounds.Height / 2 - barHeight / 2;
            graphics.FillRectangle(brush, x, y, barWidth, barHeight);
            graphics.FillRectangle(brush, x + barWidth + gap, y, barWidth, barHeight);
        }

        private static void DrawPreviousIcon(Graphics graphics, Rectangle bounds, Brush brush)
        {
            int iconHeight = bounds.Height / 3;
            int iconWidth = bounds.Width / 3;
            int centerX = bounds.Left + bounds.Width / 2;
            int centerY = bounds.Top + bounds.Height / 2;
            int barWidth = Math.Max(3, bounds.Width / 14);
            graphics.FillRectangle(
                brush,
                centerX - iconWidth,
                centerY - iconHeight / 2,
                barWidth,
                iconHeight);
            Point[] points =
            [
                new(centerX + iconWidth / 2, centerY - iconHeight / 2),
                new(centerX + iconWidth / 2, centerY + iconHeight / 2),
                new(centerX - iconWidth / 2, centerY)
            ];
            graphics.FillPolygon(brush, points);
        }

        private static void DrawStopIcon(Graphics graphics, Rectangle bounds, Brush brush)
        {
            int iconSize = bounds.Width / 4;
            var iconBounds = new Rectangle(
                bounds.Left + bounds.Width / 2 - iconSize / 2,
                bounds.Top + bounds.Height / 2 - iconSize / 2,
                iconSize,
                iconSize);
            graphics.FillRectangle(brush, iconBounds);
        }
    }

    private enum IconButtonKind
    {
        Previous,
        Play,
        Pause,
        Stop
    }

    private static GraphicsPath CreateRoundPath(Rectangle bounds, int radius)
    {
        var path = new GraphicsPath();
        int diameter = Math.Min(radius * 2, Math.Min(bounds.Width, bounds.Height));

        if (diameter <= 1)
        {
            path.AddRectangle(bounds);
            return path;
        }

        var arc = new Rectangle(bounds.Left, bounds.Top, diameter, diameter);
        path.AddArc(arc, 180, 90);
        arc.X = bounds.Right - diameter;
        path.AddArc(arc, 270, 90);
        arc.Y = bounds.Bottom - diameter;
        path.AddArc(arc, 0, 90);
        arc.X = bounds.Left;
        path.AddArc(arc, 90, 90);
        path.CloseFigure();
        return path;
    }
}
