using MacroMaster.WinForms.Theme;
using System.Drawing.Drawing2D;
using System.Globalization;

namespace MacroMaster.WinForms.Controls;

public readonly record struct SessionSummaryState(
    string StatusText,
    string SessionName,
    int EventCount,
    int TotalDurationMs,
    string FileName,
    bool CanOptimize);

internal sealed class SessionSummaryControl : UserControl
{
    private readonly Label _statusValueLabel;
    private readonly Label _eventCountValueLabel;
    private readonly Label _durationValueLabel;
    private readonly Label _sessionNameValueLabel;
    private readonly Label _fileNameValueLabel;
    private readonly SummaryActionButton _optimizeButton;

    public event EventHandler? OptimizeRequested;

    public SessionSummaryControl()
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

        _statusValueLabel = CreateValueLabel();
        _eventCountValueLabel = CreateValueLabel();
        _durationValueLabel = CreateValueLabel();
        _sessionNameValueLabel = CreateValueLabel();
        _fileNameValueLabel = CreateValueLabel();
        _optimizeButton = new SummaryActionButton();

        BuildLayout();
        UpdateState(new SessionSummaryState("Bos", "Oturum yok", 0, 0, "Kaydedilmedi", false));
    }

    public void UpdateState(SessionSummaryState state)
    {
        _statusValueLabel.Text = state.StatusText;
        _eventCountValueLabel.Text = state.EventCount.ToString(CultureInfo.InvariantCulture);
        _durationValueLabel.Text = FormattableString.Invariant($"{Math.Max(0, state.TotalDurationMs)} ms");
        _sessionNameValueLabel.Text = state.SessionName;
        _fileNameValueLabel.Text = state.FileName;
        _optimizeButton.Enabled = state.CanOptimize;
        _statusValueLabel.ForeColor = state.StatusText.Equals("Bos", StringComparison.OrdinalIgnoreCase)
            ? DesignTokens.TextPrimary
            : DesignTokens.AccentGreen;
    }

    private void BuildLayout()
    {
        var rootLayoutPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 6,
            BackColor = DesignTokens.Surface,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        rootLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        rootLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 20f));
        rootLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 20f));
        rootLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 20f));
        rootLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 20f));
        rootLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 20f));
        rootLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, DesignTokens.Scale(42)));

        rootLayoutPanel.Controls.Add(CreateSummaryCard("Durum", _statusValueLabel, new Padding(0, 0, 0, DesignTokens.Scale(6))), 0, 0);
        rootLayoutPanel.Controls.Add(CreateSummaryCard("Olay", _eventCountValueLabel, new Padding(0, 0, 0, DesignTokens.Scale(6))), 0, 1);
        rootLayoutPanel.Controls.Add(CreateSummaryCard("Sure", _durationValueLabel, new Padding(0, 0, 0, DesignTokens.Scale(6))), 0, 2);
        rootLayoutPanel.Controls.Add(CreateSummaryCard(
            "Oturum",
            _sessionNameValueLabel,
            new Padding(0, 0, 0, DesignTokens.Scale(6))),
            0,
            3);
        rootLayoutPanel.Controls.Add(CreateSummaryCard(
            "Dosya",
            _fileNameValueLabel,
            new Padding(0, 0, 0, DesignTokens.Scale(6))),
            0,
            4);
        rootLayoutPanel.Controls.Add(CreateOptimizeButtonHost(), 0, 5);

        Controls.Add(rootLayoutPanel);
    }

    private Panel CreateOptimizeButtonHost()
    {
        var hostPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = DesignTokens.Surface,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };

        _optimizeButton.Dock = DockStyle.Fill;
        _optimizeButton.Text = "Optimize Et";
        _optimizeButton.Click += (_, _) => OptimizeRequested?.Invoke(this, EventArgs.Empty);
        hostPanel.Controls.Add(_optimizeButton);
        return hostPanel;
    }

    private static SoftPanel CreateSummaryCard(string caption, Label valueLabel, Padding margin)
    {
        var panel = new SoftPanel
        {
            Dock = DockStyle.Fill,
            BackColor = DesignTokens.SurfaceInset,
            BorderColor = DesignTokens.BorderSoft,
            Margin = margin,
            Padding = new Padding(
                DesignTokens.Scale(12),
                DesignTokens.Scale(10),
                DesignTokens.Scale(12),
                DesignTokens.Scale(6))
        };

        valueLabel.ForeColor = DesignTokens.TextPrimary;

        var layoutPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = Color.Transparent,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        layoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, DesignTokens.Scale(20)));
        layoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
        layoutPanel.Controls.Add(CreateCaptionLabel(caption), 0, 0);
        layoutPanel.Controls.Add(valueLabel, 0, 1);
        panel.Controls.Add(layoutPanel);
        return panel;
    }

    private static Label CreateCaptionLabel(string text)
    {
        return new Label
        {
            Dock = DockStyle.Fill,
            Text = text,
            Font = DesignTokens.FontUiSmall,
            ForeColor = DesignTokens.TextSecondary,
            BackColor = Color.Transparent,
            TextAlign = ContentAlignment.MiddleLeft,
            AutoEllipsis = true
        };
    }

    private static Label CreateValueLabel()
    {
        return new Label
        {
            Dock = DockStyle.Fill,
            Font = DesignTokens.FontUiBold,
            ForeColor = DesignTokens.TextPrimary,
            BackColor = Color.Transparent,
            TextAlign = ContentAlignment.MiddleLeft,
            AutoEllipsis = true
        };
    }

    private sealed class SoftPanel : Panel
    {
        public Color BorderColor { get; set; } = DesignTokens.BorderSoft;

        public SoftPanel()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint,
                true);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            e.Graphics.Clear(Parent?.BackColor ?? DesignTokens.Surface);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle bounds = Rectangle.Inflate(ClientRectangle, -1, -1);
            if (bounds.Width <= 0 || bounds.Height <= 0)
            {
                return;
            }

            using GraphicsPath path = CreateRoundPath(bounds, DesignTokens.Scale(10));
            using var fillBrush = new SolidBrush(BackColor);
            using var borderPen = new Pen(BorderColor);
            e.Graphics.FillPath(fillBrush, path);
            e.Graphics.DrawPath(borderPen, path);
        }
    }

    private sealed class SummaryActionButton : Button
    {
        private bool _isHovered;
        private bool _isPressed;

        public SummaryActionButton()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint,
                true);

            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            Font = DesignTokens.FontUiBold;
            ForeColor = DesignTokens.TextPrimary;
            BackColor = DesignTokens.Surface;
            Cursor = Cursors.Hand;
            UseVisualStyleBackColor = false;
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            _isHovered = true;
            Invalidate();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            _isHovered = false;
            _isPressed = false;
            Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnMouseDown(MouseEventArgs mevent)
        {
            if (Enabled && mevent.Button == MouseButtons.Left)
            {
                _isPressed = true;
                Invalidate();
            }

            base.OnMouseDown(mevent);
        }

        protected override void OnMouseUp(MouseEventArgs mevent)
        {
            _isPressed = false;
            Invalidate();
            base.OnMouseUp(mevent);
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            Cursor = Enabled ? Cursors.Hand : Cursors.Default;
            Invalidate();
            base.OnEnabledChanged(e);
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            pevent.Graphics.Clear(Parent?.BackColor ?? DesignTokens.Surface);
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            Graphics graphics = pevent.Graphics;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle bounds = Rectangle.Inflate(ClientRectangle, -1, -1);
            if (bounds.Width <= 0 || bounds.Height <= 0)
            {
                return;
            }

            Color fillColor = ResolveFillColor();
            Color borderColor = Enabled
                ? DesignTokens.Accent
                : DesignTokens.BorderSoft;
            Color textColor = Enabled
                ? DesignTokens.TextPrimary
                : DesignTokens.TextMuted;

            using GraphicsPath path = CreateRoundPath(bounds, DesignTokens.Scale(8));
            using var fillBrush = new SolidBrush(fillColor);
            using var borderPen = new Pen(borderColor, Math.Max(1f, DesignTokens.DensityScale));
            graphics.FillPath(fillBrush, path);
            graphics.DrawPath(borderPen, path);

            TextRenderer.DrawText(
                graphics,
                Text,
                Font,
                bounds,
                textColor,
                TextFormatFlags.HorizontalCenter |
                TextFormatFlags.VerticalCenter |
                TextFormatFlags.EndEllipsis |
                TextFormatFlags.NoPrefix);
        }

        private Color ResolveFillColor()
        {
            if (!Enabled)
            {
                return Color.FromArgb(16, 20, 31);
            }

            if (_isPressed)
            {
                return DesignTokens.AccentDeep;
            }

            if (_isHovered || Focused)
            {
                return Color.FromArgb(20, DesignTokens.Accent);
            }

            return DesignTokens.Surface2;
        }
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
