using MacroMaster.WinForms.Theme;
using System.Drawing.Drawing2D;

namespace MacroMaster.WinForms.Controls;

internal sealed class TitleBarControl : UserControl
{
    private readonly Label _appNameLabel;
    private readonly Label _subtitleLabel;
    private readonly Label _statusLabel;
    private readonly StatusDotControl _statusDot;
    private readonly CaptionButton _minimizeButton;
    private readonly CaptionButton _maximizeButton;
    private readonly CaptionButton _closeButton;

    public TitleBarControl()
    {
        SetStyle(
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.ResizeRedraw |
            ControlStyles.UserPaint,
            true);

        DoubleBuffered = true;
        BackColor = DesignTokens.Background;
        ForeColor = DesignTokens.TextPrimary;
        Font = DesignTokens.FontUiNormal;
        MinimumSize = new Size(0, DesignTokens.TitleBarHeight);

        var rootLayoutPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 7,
            RowCount = 1,
            BackColor = Color.Transparent,
            Margin = Padding.Empty,
            Padding = new Padding(
                DesignTokens.Scale(12),
                DesignTokens.Scale(4),
                DesignTokens.Scale(8),
                DesignTokens.Scale(4))
        };

        rootLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, DesignTokens.TitleBarIconSize + DesignTokens.Scale(10)));
        rootLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        rootLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, DesignTokens.Scale(112)));
        rootLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, DesignTokens.TitleBarButtonWidth));
        rootLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, DesignTokens.TitleBarButtonWidth));
        rootLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, DesignTokens.TitleBarButtonWidth));
        rootLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, DesignTokens.Scale(2)));

        var logoPanel = new LogoPanel
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 0, DesignTokens.Scale(8), 0)
        };

        var textLayoutPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = Color.Transparent,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        textLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 58f));
        textLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 42f));

        _appNameLabel = new Label
        {
            Dock = DockStyle.Fill,
            Text = "MacroMaster Kontrol Merkezi",
            Font = DesignTokens.FontUiBold,
            ForeColor = DesignTokens.TextPrimary,
            BackColor = Color.Transparent,
            TextAlign = ContentAlignment.BottomLeft,
            AutoEllipsis = true
        };

        _subtitleLabel = new Label
        {
            Dock = DockStyle.Fill,
            Text = "Kayit, oynatim ve JSON/XML destegi",
            Font = DesignTokens.FontUiSmall,
            ForeColor = DesignTokens.TextSecondary,
            BackColor = Color.Transparent,
            TextAlign = ContentAlignment.TopLeft,
            AutoEllipsis = true
        };

        textLayoutPanel.Controls.Add(_appNameLabel, 0, 0);
        textLayoutPanel.Controls.Add(_subtitleLabel, 0, 1);

        var statusLayoutPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = Color.Transparent,
            Margin = new Padding(DesignTokens.Scale(6), 0, DesignTokens.Scale(8), 0),
            Padding = Padding.Empty
        };
        statusLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, DesignTokens.TitleBarStatusDotSize + DesignTokens.Scale(8)));
        statusLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));

        _statusDot = new StatusDotControl
        {
            Dock = DockStyle.Fill,
            DotColor = DesignTokens.AccentGreen,
            Margin = Padding.Empty
        };

        _statusLabel = new Label
        {
            Dock = DockStyle.Fill,
            Text = "Hazir",
            Font = DesignTokens.FontUiNormal,
            ForeColor = DesignTokens.TextSecondary,
            BackColor = Color.Transparent,
            TextAlign = ContentAlignment.MiddleLeft,
            AutoEllipsis = true
        };

        statusLayoutPanel.Controls.Add(_statusDot, 0, 0);
        statusLayoutPanel.Controls.Add(_statusLabel, 1, 0);

        _minimizeButton = new CaptionButton(CaptionButtonKind.Minimize);
        _maximizeButton = new CaptionButton(CaptionButtonKind.Maximize);
        _closeButton = new CaptionButton(CaptionButtonKind.Close);

        _minimizeButton.Click += (_, _) => MinimizeRequested?.Invoke(this, EventArgs.Empty);
        _maximizeButton.Click += (_, _) => MaximizeRestoreRequested?.Invoke(this, EventArgs.Empty);
        _closeButton.Click += (_, _) => CloseRequested?.Invoke(this, EventArgs.Empty);

        rootLayoutPanel.Controls.Add(logoPanel, 0, 0);
        rootLayoutPanel.Controls.Add(textLayoutPanel, 1, 0);
        rootLayoutPanel.Controls.Add(statusLayoutPanel, 2, 0);
        rootLayoutPanel.Controls.Add(_minimizeButton, 3, 0);
        rootLayoutPanel.Controls.Add(_maximizeButton, 4, 0);
        rootLayoutPanel.Controls.Add(_closeButton, 5, 0);

        Controls.Add(rootLayoutPanel);
    }

    public event EventHandler? MinimizeRequested;
    public event EventHandler? MaximizeRestoreRequested;
    public event EventHandler? CloseRequested;

    public void SetTitle(string title, string subtitle)
    {
        _appNameLabel.Text = title;
        _subtitleLabel.Text = subtitle;
    }

    public void SetStatus(string status, Color color)
    {
        _statusLabel.Text = status;
        _statusDot.DotColor = color;
        _statusDot.Invalidate();
    }

    public void SetMaximized(bool isMaximized)
    {
        _maximizeButton.Kind = isMaximized
            ? CaptionButtonKind.Restore
            : CaptionButtonKind.Maximize;
    }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
        using var brush = new LinearGradientBrush(
            ClientRectangle,
            Color.FromArgb(15, 20, 32),
            DesignTokens.Background,
            LinearGradientMode.Horizontal);
        e.Graphics.FillRectangle(brush, ClientRectangle);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        using var bottomPen = new Pen(DesignTokens.BorderSoft);
        e.Graphics.DrawLine(bottomPen, 0, Height - 1, Width, Height - 1);
    }

    private enum CaptionButtonKind
    {
        Minimize,
        Maximize,
        Restore,
        Close
    }

    private sealed class LogoPanel : Control
    {
        public LogoPanel()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint,
                true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            int size = Math.Min(DesignTokens.TitleBarIconSize, Math.Min(Width, Height) - DesignTokens.Scale(4));
            if (size <= 0)
            {
                return;
            }

            var bounds = new Rectangle(
                (Width - size) / 2,
                (Height - size) / 2,
                size,
                size);

            using var path = CreateRoundedRectanglePath(bounds, DesignTokens.Scale(7));
            using var fillBrush = new SolidBrush(Color.FromArgb(28, 67, 138));
            using var borderPen = new Pen(Color.FromArgb(160, DesignTokens.Accent));
            using var textBrush = new SolidBrush(DesignTokens.Accent);
            using var font = new Font("Segoe UI", Math.Max(8f, size * 0.46f), FontStyle.Bold, GraphicsUnit.Pixel);
            using var stringFormat = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

            e.Graphics.FillPath(fillBrush, path);
            e.Graphics.DrawPath(borderPen, path);
            e.Graphics.DrawString("M", font, textBrush, bounds, stringFormat);
        }
    }

    private sealed class StatusDotControl : Control
    {
        public Color DotColor { get; set; } = DesignTokens.AccentGreen;

        public StatusDotControl()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint,
                true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            int size = Math.Min(DesignTokens.TitleBarStatusDotSize, Math.Min(Width, Height));
            var bounds = new Rectangle(
                (Width - size) / 2,
                (Height - size) / 2,
                size,
                size);

            using var glowBrush = new SolidBrush(Color.FromArgb(55, DotColor));
            using var dotBrush = new SolidBrush(DotColor);
            e.Graphics.FillEllipse(glowBrush, Rectangle.Inflate(bounds, DesignTokens.Scale(2), DesignTokens.Scale(2)));
            e.Graphics.FillEllipse(dotBrush, bounds);
        }
    }

    private sealed class CaptionButton : Button
    {
        private bool _isPressed;
        private CaptionButtonKind _kind;

        public CaptionButton(CaptionButtonKind kind)
        {
            _kind = kind;
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint,
                true);

            Dock = DockStyle.Fill;
            FlatStyle = FlatStyle.Flat;
            Margin = new Padding(DesignTokens.Scale(2), 0, 0, 0);
            MinimumSize = new Size(DesignTokens.TitleBarButtonWidth, DesignTokens.TitleBarButtonHeight);
            TabStop = false;
            Cursor = Cursors.Hand;
            FlatAppearance.BorderSize = 0;
            BackColor = Color.Transparent;
        }

        public CaptionButtonKind Kind
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

        protected override void OnMouseDown(MouseEventArgs mevent)
        {
            _isPressed = mevent.Button == MouseButtons.Left;
            Invalidate();
            base.OnMouseDown(mevent);
        }

        protected override void OnMouseUp(MouseEventArgs mevent)
        {
            _isPressed = false;
            Invalidate();
            base.OnMouseUp(mevent);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            _isPressed = false;
            Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            pevent.Graphics.Clear(Parent?.BackColor ?? DesignTokens.Background);
            pevent.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            Color fill = ResolveFillColor();
            Color icon = Enabled
                ? DesignTokens.TextPrimary
                : DesignTokens.TextMuted;

            Rectangle buttonBounds = Rectangle.Inflate(ClientRectangle, -DesignTokens.Scale(2), -DesignTokens.Scale(4));
            using var path = CreateRoundedRectanglePath(buttonBounds, DesignTokens.Scale(7));
            using var fillBrush = new SolidBrush(fill);
            pevent.Graphics.FillPath(fillBrush, path);

            using var iconPen = new Pen(icon, Math.Max(1.6f, DesignTokens.DensityScale * 1.35f))
            {
                StartCap = LineCap.Round,
                EndCap = LineCap.Round
            };

            DrawIcon(pevent.Graphics, iconPen);
        }

        private Color ResolveFillColor()
        {
            if (!Enabled)
            {
                return Color.Transparent;
            }

            if (_kind == CaptionButtonKind.Close && ClientRectangle.Contains(PointToClient(Cursor.Position)))
            {
                return _isPressed
                    ? Color.FromArgb(190, 185, 38, 61)
                    : Color.FromArgb(150, 255, 79, 106);
            }

            if (_isPressed)
            {
                return DesignTokens.Surface3;
            }

            return ClientRectangle.Contains(PointToClient(Cursor.Position))
                ? DesignTokens.SurfaceHover
                : Color.Transparent;
        }

        private void DrawIcon(Graphics graphics, Pen pen)
        {
            int iconSize = DesignTokens.Scale(12);
            int centerX = Width / 2;
            int centerY = Height / 2;
            int left = centerX - iconSize / 2;
            int top = centerY - iconSize / 2;
            int right = left + iconSize;
            int bottom = top + iconSize;

            switch (_kind)
            {
                case CaptionButtonKind.Minimize:
                    graphics.DrawLine(pen, left, centerY + iconSize / 4, right, centerY + iconSize / 4);
                    break;
                case CaptionButtonKind.Maximize:
                    graphics.DrawRectangle(pen, left, top, iconSize, iconSize);
                    break;
                case CaptionButtonKind.Restore:
                    graphics.DrawRectangle(pen, left + DesignTokens.Scale(3), top, iconSize - DesignTokens.Scale(3), iconSize - DesignTokens.Scale(3));
                    graphics.DrawRectangle(pen, left, top + DesignTokens.Scale(3), iconSize - DesignTokens.Scale(3), iconSize - DesignTokens.Scale(3));
                    break;
                case CaptionButtonKind.Close:
                    graphics.DrawLine(pen, left, top, right, bottom);
                    graphics.DrawLine(pen, right, top, left, bottom);
                    break;
            }
        }
    }

    private static GraphicsPath CreateRoundedRectanglePath(Rectangle bounds, int radius)
    {
        var path = new GraphicsPath();

        if (bounds.Width <= 0 || bounds.Height <= 0)
        {
            return path;
        }

        int diameter = Math.Min(radius * 2, Math.Min(bounds.Width, bounds.Height));
        if (diameter <= 1)
        {
            path.AddRectangle(bounds);
            return path;
        }

        var arc = new Rectangle(bounds.Location, new Size(diameter, diameter));
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
