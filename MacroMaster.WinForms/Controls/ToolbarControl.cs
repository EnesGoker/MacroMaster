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
    bool CanSaveLibrary,
    bool CanSaveJson,
    bool CanSaveXml,
    bool CanLoadJson,
    bool CanLoadXml,
    bool CanEditHotkeys);

public partial class ToolbarControl : UserControl
{
    private bool _canSaveLibrary;
    private bool _canSaveJson;
    private bool _canSaveXml;
    private bool _canLoadJson;
    private bool _canLoadXml;
    private string? _recordHotkey;
    private string? _stopHotkey;
    private string? _playbackHotkey;
    private string? _hotkeysHotkey;
    private string _recordLabel = "Kaydı Başlat";
    private string _playbackLabel = "Oynat";

    public event EventHandler? RecordToggleClicked;
    public event EventHandler? StopClicked;
    public event EventHandler? PlaybackToggleClicked;
    public event EventHandler? SaveLibraryClicked;
    public event EventHandler? SaveJsonClicked;
    public event EventHandler? SaveXmlClicked;
    public event EventHandler? LoadJsonClicked;
    public event EventHandler? LoadXmlClicked;
    public event EventHandler? HotkeysClicked;

    public ToolbarControl()
    {
        InitializeComponent();
        ApplyTheme();
        WireEvents();
        SetHotkeyHints(null, null, null, null);
    }

    public void UpdateRecordButton(bool isRecording)
    {
        _recordLabel = isRecording ? "Kaydı Durdur" : "Kaydı Başlat";
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
        saveButton.Enabled = state.CanSaveLibrary || state.CanSaveJson || state.CanSaveXml;
        loadButton.Enabled = state.CanLoadJson || state.CanLoadXml;
        hotkeysButton.Enabled = state.CanEditHotkeys;

        _canSaveLibrary = state.CanSaveLibrary;
        _canSaveJson = state.CanSaveJson;
        _canSaveXml = state.CanSaveXml;
        _canLoadJson = state.CanLoadJson;
        _canLoadXml = state.CanLoadXml;
    }

    public void SetHotkeyHints(string? record, string? stop, string? playback, string? hotkeys)
    {
        _recordHotkey = record;
        _stopHotkey = stop;
        _playbackHotkey = playback;
        _hotkeysHotkey = hotkeys;

        SetButtonText(recordButton, _recordLabel, _recordHotkey);
        SetButtonText(stopButton, "Durdur", _stopHotkey);
        SetButtonText(playbackButton, _playbackLabel, _playbackHotkey);
        SetButtonText(hotkeysButton, "Kısayollar", _hotkeysHotkey);
    }

    private void WireEvents()
    {
        recordButton.Click += (_, _) => RecordToggleClicked?.Invoke(this, EventArgs.Empty);
        stopButton.Click += (_, _) => StopClicked?.Invoke(this, EventArgs.Empty);
        playbackButton.Click += (_, _) => PlaybackToggleClicked?.Invoke(this, EventArgs.Empty);
        saveButton.Click += (_, _) => ShowSaveMenu();
        loadButton.Click += (_, _) => ShowLoadMenu();
        hotkeysButton.Click += (_, _) => HotkeysClicked?.Invoke(this, EventArgs.Empty);
    }

    private void ShowSaveMenu()
    {
        ThemedDropDownMenu.ShowFor(
            saveButton,
            [
                new ThemedDropDownMenuItem(
                    "Kütüphaneye Kaydet",
                    () => SaveLibraryClicked?.Invoke(this, EventArgs.Empty),
                    _canSaveLibrary),
                ThemedDropDownMenuItem.Separator(),
                new ThemedDropDownMenuItem(
                    "JSON Kaydet",
                    () => SaveJsonClicked?.Invoke(this, EventArgs.Empty),
                    _canSaveJson),
                new ThemedDropDownMenuItem(
                    "XML Kaydet",
                    () => SaveXmlClicked?.Invoke(this, EventArgs.Empty),
                    _canSaveXml)
            ],
            new Point(0, saveButton.Height + DesignTokens.Scale(4)),
            CreateToolbarMenuOptions(saveButton));
    }

    private void ShowLoadMenu()
    {
        ThemedDropDownMenu.ShowFor(
            loadButton,
            [
                new ThemedDropDownMenuItem(
                    "JSON Yükle",
                    () => LoadJsonClicked?.Invoke(this, EventArgs.Empty),
                    _canLoadJson),
                new ThemedDropDownMenuItem(
                    "XML Yükle",
                    () => LoadXmlClicked?.Invoke(this, EventArgs.Empty),
                    _canLoadXml)
            ],
            new Point(0, loadButton.Height + DesignTokens.Scale(4)),
            CreateToolbarMenuOptions(loadButton));
    }

    private static ThemedDropDownMenuOptions CreateToolbarMenuOptions(Control owner)
    {
        return new ThemedDropDownMenuOptions
        {
            MinimumWidth = Math.Max(owner.Width, DesignTokens.Scale(184)),
            MaximumWidth = DesignTokens.Scale(300),
            ItemHeight = DesignTokens.Scale(42),
            SeparatorHeight = DesignTokens.Scale(14),
            VerticalPadding = DesignTokens.Scale(8),
            HorizontalPadding = DesignTokens.Scale(14)
        };
    }

    private void ApplyTheme()
    {
        BackColor = DesignTokens.Surface;
        toolbarLayoutPanel.BackColor = DesignTokens.Surface;
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
        button.BackColor = DesignTokens.Surface;
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
            pevent.Graphics.Clear(Parent?.BackColor ?? DesignTokens.Surface);
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

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            pevent.Graphics.Clear(Parent?.BackColor ?? DesignTokens.Surface);
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
