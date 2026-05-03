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
        _remainingTimeLabel = CreateTimeLabel(ContentAlignment.MiddleLeft);
        _statusLabel = new Label
        {
            Dock = DockStyle.Fill,
            Font = DesignTokens.FontUiBold,
            ForeColor = Color.FromArgb(52, 199, 89), // green — Hazır
            BackColor = DesignTokens.Surface,
            TextAlign = ContentAlignment.MiddleLeft,
            AutoEllipsis = true
        };
        _eventCounterLabel = CreateTimeLabel(ContentAlignment.MiddleLeft);
        _settingsLabel = CreateMetaLabel(ContentAlignment.MiddleRight);
        _playbackButton = new IconButton(IconButtonKind.Play) { ButtonSizeOverride = DesignTokens.Scale(52) };
        _stopButton = new IconButton(IconButtonKind.Stop);
        _toolTip = new ToolTip();

        BuildLayout();
        WireEvents();
        UpdateState(new PlaybackControlState("Hazır", 0, 0, 0, 0, 1, 1, false, 0, false, false, PlaybackButtonState.Play));
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
        _remainingTimeLabel.Text = FormatDuration(Math.Max(0, safeTotalDurationMs - safePlayedDurationMs));
        _statusLabel.Text = state.StatusText;
        _statusLabel.ForeColor = state.StatusText is "Hazır" or "Hazir" or "Bos" or "Boş"
            ? Color.FromArgb(52, 199, 89)
            : DesignTokens.TextPrimary;
        _eventCounterLabel.Text = FormattableString.Invariant($"{safePlayedEvents} / {safeTotalEvents}");
        _settingsLabel.Text = FormatSettingsSummary(state);

        _playbackButton.Kind = state.PlaybackButtonState switch
        {
            PlaybackButtonState.Pause => IconButtonKind.Pause,
            _ => IconButtonKind.Play
        };

        _playbackButton.Enabled = state.CanPlayback;
        _stopButton.Enabled = state.CanStop;

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
            RowCount = 4,
            BackColor = DesignTokens.Surface,
            Margin = Padding.Empty,
            Padding = new Padding(
                DesignTokens.Scale(16),
                DesignTokens.Scale(10),
                DesignTokens.Scale(16),
                DesignTokens.Scale(8))
        };
        rootLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        rootLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, DesignTokens.Scale(36))); // status row
        rootLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, DesignTokens.Scale(28))); // progress bar
        rootLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, DesignTokens.Scale(20))); // progress % label
        rootLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));                    // buttons

        // Row 0: Status info — Durum | Mevcut Olay | Geçen Süre | Kalan Süre
        var statusPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            RowCount = 2,
            BackColor = DesignTokens.Surface,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        statusPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
        statusPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
        statusPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
        statusPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
        statusPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, DesignTokens.Scale(16)));
        statusPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

        var captionDurum = CreateCaptionLabel("Durum");
        var captionOlay = CreateCaptionLabel("Mevcut Olay");
        var captionGecen = CreateCaptionLabel("Geçen Süre");
        var captionKalan = CreateCaptionLabel("Kalan Süre");

        statusPanel.Controls.Add(captionDurum, 0, 0);
        statusPanel.Controls.Add(captionOlay, 1, 0);
        statusPanel.Controls.Add(captionGecen, 2, 0);
        statusPanel.Controls.Add(captionKalan, 3, 0);
        statusPanel.Controls.Add(_statusLabel, 0, 1);
        statusPanel.Controls.Add(_eventCounterLabel, 1, 1);
        statusPanel.Controls.Add(_elapsedTimeLabel, 2, 1);
        statusPanel.Controls.Add(_remainingTimeLabel, 3, 1);

        // Row 1: Progress bar
        // Row 2: Progress % (right-aligned)
        var progressPercentLabel = new Label
        {
            Dock = DockStyle.Fill,
            Font = DesignTokens.FontUiSmall,
            ForeColor = DesignTokens.TextSecondary,
            BackColor = DesignTokens.Surface,
            TextAlign = ContentAlignment.MiddleRight,
            AutoEllipsis = true
        };
        _progressBar.PercentLabel = progressPercentLabel;

        // Row 3: Buttons — centered, 5 buttons like reference
        var controlsLayoutPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 7,
            RowCount = 1,
            BackColor = DesignTokens.Surface,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        controlsLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
        controlsLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, DesignTokens.Scale(52)));
        controlsLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, DesignTokens.Scale(52)));
        controlsLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, DesignTokens.Scale(72)));
        controlsLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, DesignTokens.Scale(52)));
        controlsLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, DesignTokens.Scale(52)));
        controlsLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));

        var skipBackButton = new IconButton(IconButtonKind.SkipBack);
        var stepBackButton = new IconButton(IconButtonKind.StepBack);
        var stepForwardButton = new IconButton(IconButtonKind.StepForward);
        skipBackButton.Enabled = false;
        stepBackButton.Enabled = false;
        stepForwardButton.Enabled = false;
        _playbackButton.ButtonSizeOverride = DesignTokens.Scale(52);

        controlsLayoutPanel.Controls.Add(skipBackButton, 1, 0);
        controlsLayoutPanel.Controls.Add(stepBackButton, 2, 0);
        controlsLayoutPanel.Controls.Add(_playbackButton, 3, 0);
        controlsLayoutPanel.Controls.Add(stepForwardButton, 4, 0);
        controlsLayoutPanel.Controls.Add(_stopButton, 5, 0);

        _toolTip.SetToolTip(skipBackButton, "Onceki kayit adimi henuz aktif degil");
        _toolTip.SetToolTip(stepBackButton, "Geri adim henuz aktif degil");
        _toolTip.SetToolTip(stepForwardButton, "Ileri adim henuz aktif degil");

        rootLayoutPanel.Controls.Add(statusPanel, 0, 0);
        rootLayoutPanel.Controls.Add(_progressBar, 0, 1);
        rootLayoutPanel.Controls.Add(progressPercentLabel, 0, 2);
        rootLayoutPanel.Controls.Add(controlsLayoutPanel, 0, 3);
        Controls.Add(rootLayoutPanel);
    }

    private static Label CreateCaptionLabel(string text)
    {
        return new Label
        {
            Dock = DockStyle.Fill,
            Text = text,
            Font = DesignTokens.FontUiSmall,
            ForeColor = DesignTokens.TextSecondary,
            BackColor = DesignTokens.Surface,
            TextAlign = ContentAlignment.MiddleLeft
        };
    }

    private void WireEvents()
    {
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

        public Label? PercentLabel { get; set; }

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
                if (Math.Abs(_progress - normalized) < 0.001) return;
                _progress = normalized;
                if (PercentLabel != null)
                    PercentLabel.Text = FormattableString.Invariant($"{_progress * 100:0}%");
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
            ButtonSizeOverride = DesignTokens.Scale(46);
        }

        public int ButtonSizeOverride { get; set; }

        // Keep old name working
        public int ButtonSize
        {
            get => ButtonSizeOverride;
            init => ButtonSizeOverride = value;
        }

        public IconButtonKind Kind
        {
            get => _kind;
            set
            {
                if (_kind == value) return;
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
            int size = Math.Min(ButtonSizeOverride, Math.Min(Width - inset, Height - inset));
            var bounds = new Rectangle(
                (Width - size) / 2,
                (Height - size) / 2,
                size,
                size);
            bool isPrimary = Kind is IconButtonKind.Play or IconButtonKind.Pause;
            Color iconColor = Enabled ? DesignTokens.TextPrimary : DesignTokens.TextMuted;
            Color background = isPrimary ? DesignTokens.AccentDeep : DesignTokens.Surface2;
            Color border = Enabled
                ? isPrimary ? DesignTokens.Accent : DesignTokens.BorderBright
                : DesignTokens.Border;

            if (ClientRectangle.Contains(PointToClient(MousePosition)) && Enabled)
                background = isPrimary ? Color.FromArgb(35, 116, 239) : DesignTokens.Surface3;

            using GraphicsPath backgroundPath = CreateRoundPath(bounds, Math.Min(20, size / 2));
            using var backgroundBrush = new SolidBrush(background);
            using var borderPen = new Pen(border, 1f);
            e.Graphics.FillPath(backgroundBrush, backgroundPath);
            e.Graphics.DrawPath(borderPen, backgroundPath);

            using var iconBrush = new SolidBrush(iconColor);
            DrawIcon(e.Graphics, bounds, iconBrush);
        }

        private void DrawIcon(Graphics g, Rectangle b, Brush brush)
        {
            switch (Kind)
            {
                case IconButtonKind.SkipBack:    DrawSkipBackIcon(g, b, brush); break;
                case IconButtonKind.StepBack:    DrawStepBackIcon(g, b, brush); break;
                case IconButtonKind.StepForward: DrawStepForwardIcon(g, b, brush); break;
                case IconButtonKind.Pause:       DrawPauseIcon(g, b, brush); break;
                case IconButtonKind.Stop:        DrawStopIcon(g, b, brush); break;
                default:                         DrawPlayIcon(g, b, brush); break;
            }
        }

        private static void DrawPlayIcon(Graphics g, Rectangle b, Brush brush)
        {
            int s = b.Width / 3;
            int x = b.Left + b.Width / 2 - s / 3;
            int y = b.Top + b.Height / 2;
            g.FillPolygon(brush, new Point[] {
                new(x - s / 2, y - s), new(x - s / 2, y + s), new(x + s, y)
            });
        }

        private static void DrawPauseIcon(Graphics g, Rectangle b, Brush brush)
        {
            int bw = Math.Max(4, b.Width / 9);
            int bh = b.Height / 3;
            int gap = bw;
            int x = b.Left + b.Width / 2 - bw - gap / 2;
            int y = b.Top + b.Height / 2 - bh / 2;
            g.FillRectangle(brush, x, y, bw, bh);
            g.FillRectangle(brush, x + bw + gap, y, bw, bh);
        }

        private static void DrawStopIcon(Graphics g, Rectangle b, Brush brush)
        {
            int s = b.Width / 4;
            g.FillRectangle(brush,
                b.Left + b.Width / 2 - s / 2,
                b.Top + b.Height / 2 - s / 2, s, s);
        }

        // |◀  skip to beginning
        private static void DrawSkipBackIcon(Graphics g, Rectangle b, Brush brush)
        {
            int ih = b.Height / 3;
            int iw = b.Width / 3;
            int cx = b.Left + b.Width / 2;
            int cy = b.Top + b.Height / 2;
            int barW = Math.Max(3, b.Width / 12);
            g.FillRectangle(brush, cx - iw, cy - ih / 2, barW, ih);
            g.FillPolygon(brush, new Point[] {
                new(cx + iw / 2, cy - ih / 2),
                new(cx + iw / 2, cy + ih / 2),
                new(cx - iw / 2, cy)
            });
        }

        // ◀  step back one event
        private static void DrawStepBackIcon(Graphics g, Rectangle b, Brush brush)
        {
            int ih = b.Height / 3;
            int iw = b.Width / 4;
            int cx = b.Left + b.Width / 2;
            int cy = b.Top + b.Height / 2;
            g.FillPolygon(brush, new Point[] {
                new(cx + iw, cy - ih / 2),
                new(cx + iw, cy + ih / 2),
                new(cx - iw, cy)
            });
        }

        // ▶  step forward one event
        private static void DrawStepForwardIcon(Graphics g, Rectangle b, Brush brush)
        {
            int ih = b.Height / 3;
            int iw = b.Width / 4;
            int cx = b.Left + b.Width / 2;
            int cy = b.Top + b.Height / 2;
            g.FillPolygon(brush, new Point[] {
                new(cx - iw, cy - ih / 2),
                new(cx - iw, cy + ih / 2),
                new(cx + iw, cy)
            });
        }

    } // end IconButton

    private enum IconButtonKind
    {
        SkipBack,
        StepBack,
        Play,
        Pause,
        StepForward,
        Stop,
        Previous // legacy alias
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
