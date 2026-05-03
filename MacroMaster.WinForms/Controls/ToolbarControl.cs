using MacroMaster.WinForms.Theme;
using System.Drawing.Drawing2D;

namespace MacroMaster.WinForms.Controls;

public enum PlaybackButtonState
{
    Play,
    Pause,
    Resume
}

public readonly record struct ToolbarButtonState(
    bool CanRecord,
    bool CanStop,
    bool CanPlayback,
    bool CanSave,
    bool CanLoad,
    bool CanEditHotkeys);

public partial class ToolbarControl : UserControl
{
    private string? _recordHotkey = "F8";
    private string? _stopHotkey = "F10";
    private string? _playbackHotkey = "F9";
    private string? _hotkeysHotkey = "F12";
    private string _recordLabel = "Kaydi Baslat";
    private string _playbackLabel = "Oynat";

    public event EventHandler? RecordToggleClicked;
    public event EventHandler? StopClicked;
    public event EventHandler? PlaybackToggleClicked;
    public event EventHandler? SaveClicked;
    public event EventHandler? LoadClicked;
    public event EventHandler? HotkeysClicked;

    public ToolbarControl()
    {
        InitializeComponent();
        ApplyTheme();
        WireEvents();
        SetHotkeyHints("F8", "F10", "F9", "F12");
    }

    public void UpdateRecordButton(bool isRecording)
    {
        _recordLabel = isRecording ? "Kaydi Durdur" : "Kaydi Baslat";
        SetButtonText(recordButton, _recordLabel, _recordHotkey);
        ApplyButtonAccent(recordButton, DesignTokens.AccentRed, DesignTokens.AccentRedSoft);
    }

    public void UpdatePlaybackButton(PlaybackButtonState state)
    {
        _playbackLabel = state switch
        {
            PlaybackButtonState.Pause => "Duraklat",
            PlaybackButtonState.Resume => "Devam Et",
            _ => "Oynat"
        };

        SetButtonText(playbackButton, _playbackLabel, _playbackHotkey);
        ApplyButtonAccent(playbackButton, DesignTokens.Accent, DesignTokens.AccentSoft);
    }

    public void SetButtonsEnabled(ToolbarButtonState state)
    {
        recordButton.Enabled = state.CanRecord;
        stopButton.Enabled = state.CanStop;
        playbackButton.Enabled = state.CanPlayback;
        saveButton.Enabled = state.CanSave;
        loadButton.Enabled = state.CanLoad;
        hotkeysButton.Enabled = state.CanEditHotkeys;
    }

    public void SetHotkeyHints(string record, string stop, string playback, string hotkeys)
    {
        _recordHotkey = record;
        _stopHotkey = stop;
        _playbackHotkey = playback;
        _hotkeysHotkey = hotkeys;

        SetButtonText(recordButton, _recordLabel, _recordHotkey);
        SetButtonText(stopButton, "Durdur", _stopHotkey);
        SetButtonText(playbackButton, _playbackLabel, _playbackHotkey);
        SetButtonText(hotkeysButton, "Kisayollar", _hotkeysHotkey);
    }

    private void WireEvents()
    {
        recordButton.Click += (_, _) => RecordToggleClicked?.Invoke(this, EventArgs.Empty);
        stopButton.Click += (_, _) => StopClicked?.Invoke(this, EventArgs.Empty);
        playbackButton.Click += (_, _) => PlaybackToggleClicked?.Invoke(this, EventArgs.Empty);
        saveButton.Click += (_, _) => SaveClicked?.Invoke(this, EventArgs.Empty);
        loadButton.Click += (_, _) => LoadClicked?.Invoke(this, EventArgs.Empty);
        hotkeysButton.Click += (_, _) => HotkeysClicked?.Invoke(this, EventArgs.Empty);
    }

    private void ApplyTheme()
    {
        BackColor = DesignTokens.Surface;
        MinimumSize = new Size(0, DesignTokens.ToolbarHeight);
        MaximumSize = new Size(int.MaxValue, int.MaxValue);

        foreach (Control control in toolbarLayoutPanel.Controls)
        {
            if (control is ToolbarButton button)
            {
                ApplyToolbarButtonStyle(button);
            }
        }

        ApplyButtonAccent(recordButton, DesignTokens.AccentRed, DesignTokens.AccentRedSoft);
        ApplyButtonAccent(playbackButton, DesignTokens.Accent, DesignTokens.AccentSoft);
        ApplyButtonAccent(stopButton, DesignTokens.BorderBright, DesignTokens.Surface2);
    }

    private static void ApplyToolbarButtonStyle(ToolbarButton button)
    {
        button.FillColor = DesignTokens.Surface2;
        button.HoverFillColor = DesignTokens.SurfaceHover;
        button.PressedFillColor = DesignTokens.Surface3;
        button.BorderColor = DesignTokens.BorderBright;
        button.TextColor = DesignTokens.TextPrimary;
        button.DisabledFillColor = Color.FromArgb(21, 25, 38);
        button.DisabledBorderColor = DesignTokens.BorderSoft;
        button.DisabledTextColor = DesignTokens.TextMuted;
        button.Font = DesignTokens.FontUiBold;
        button.TextAlign = ContentAlignment.MiddleCenter;
        button.Margin = new Padding(
            DesignTokens.Scale(5),
            DesignTokens.Scale(6),
            DesignTokens.Scale(5),
            DesignTokens.Scale(6));
        button.Cursor = Cursors.Hand;
    }

    private static void ApplyButtonAccent(ToolbarButton button, Color accent, Color fill)
    {
        button.BorderColor = accent;
        button.FillColor = fill;
        button.Invalidate();
    }

    private static void SetButtonText(Button button, string label, string? hotkey)
    {
        button.Text = string.IsNullOrWhiteSpace(hotkey)
            ? label
            : $"{label} ({hotkey})";
    }

    private sealed class ToolbarButton : Button
    {
        private bool _isPressed;

        public ToolbarButton()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint,
                true);

            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            UseVisualStyleBackColor = false;
        }

        public Color FillColor { get; set; } = DesignTokens.Surface2;

        public Color HoverFillColor { get; set; } = DesignTokens.SurfaceHover;

        public Color PressedFillColor { get; set; } = DesignTokens.Surface3;

        public Color BorderColor { get; set; } = DesignTokens.BorderBright;

        public Color TextColor { get; set; } = DesignTokens.TextPrimary;

        public Color DisabledFillColor { get; set; } = DesignTokens.Surface;

        public Color DisabledBorderColor { get; set; } = DesignTokens.BorderSoft;

        public Color DisabledTextColor { get; set; } = DesignTokens.TextMuted;

        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            Cursor = Enabled ? Cursors.Hand : Cursors.Default;
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs mevent)
        {
            base.OnMouseDown(mevent);
            _isPressed = true;
            Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs mevent)
        {
            base.OnMouseUp(mevent);
            _isPressed = false;
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
            _isPressed = false;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            pevent.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle bounds = Rectangle.Inflate(ClientRectangle, -1, -1);
            if (bounds.Width <= 0 || bounds.Height <= 0)
            {
                return;
            }

            bool isHovered = ClientRectangle.Contains(PointToClient(MousePosition));
            Color fill = ResolveFillColor(isHovered);
            Color border = Enabled ? BorderColor : DisabledBorderColor;
            Color text = Enabled ? TextColor : DisabledTextColor;

            using GraphicsPath path = CreateRoundedRectanglePath(bounds, DesignTokens.Scale(8));
            using var fillBrush = new SolidBrush(fill);
            using var borderPen = new Pen(border);
            pevent.Graphics.FillPath(fillBrush, path);
            pevent.Graphics.DrawPath(borderPen, path);

            TextRenderer.DrawText(
                pevent.Graphics,
                Text,
                Font,
                bounds,
                text,
                TextFormatFlags.HorizontalCenter |
                TextFormatFlags.VerticalCenter |
                TextFormatFlags.EndEllipsis |
                TextFormatFlags.NoPrefix);
        }

        private Color ResolveFillColor(bool isHovered)
        {
            if (!Enabled)
            {
                return DisabledFillColor;
            }

            if (_isPressed)
            {
                return PressedFillColor;
            }

            return isHovered ? HoverFillColor : FillColor;
        }
    }

    private static GraphicsPath CreateRoundedRectanglePath(Rectangle bounds, int radius)
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