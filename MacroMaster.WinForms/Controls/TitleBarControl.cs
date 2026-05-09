using MacroMaster.WinForms.Theme;
using System.Drawing.Drawing2D;

namespace MacroMaster.WinForms.Controls;

internal sealed class TitleBarControl : UserControl
{
    private const int IconCol = 0;
    private const int TitleCol = 1;
    private const int StatusCol = 2;
    private const int MinCol = 3;
    private const int MaxCol = 4;
    private const int CloseCol = 5;
    private const int SpacerCol = 6;

    private readonly TableLayoutPanel _rootLayoutPanel;
    private readonly LogoPanel _logoPanel;
    private readonly Label _appNameLabel;
    private readonly StatusPillControl _statusPill;
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

        _rootLayoutPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 7,
            RowCount = 1,
            BackColor = DesignTokens.Background,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };

        _rootLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 0f));
        _rootLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        _rootLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 0f));
        _rootLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 0f));
        _rootLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 0f));
        _rootLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 0f));
        _rootLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 0f));

        _logoPanel = new LogoPanel
        {
            Dock = DockStyle.Fill,
            BackColor = DesignTokens.Background,
            Margin = Padding.Empty
        };

        var textLayoutPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 1,
            BackColor = DesignTokens.Background,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        textLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

        _appNameLabel = new Label
        {
            Dock = DockStyle.Fill,
            Text = "MacroMaster Kontrol Merkezi",
            ForeColor = DesignTokens.TextPrimary,
            BackColor = DesignTokens.Background,
            TextAlign = ContentAlignment.MiddleLeft,
            AutoEllipsis = true
        };

        textLayoutPanel.Controls.Add(_appNameLabel, 0, 0);

        _statusPill = new StatusPillControl
        {
            Dock = DockStyle.Fill,
            BackColor = Color.Transparent,
        };

        _minimizeButton = new CaptionButton(CaptionButtonKind.Minimize);
        _maximizeButton = new CaptionButton(CaptionButtonKind.Maximize);
        _closeButton = new CaptionButton(CaptionButtonKind.Close);

        _minimizeButton.Click += (_, _) => MinimizeRequested?.Invoke(this, EventArgs.Empty);
        _maximizeButton.Click += (_, _) => MaximizeRestoreRequested?.Invoke(this, EventArgs.Empty);
        _closeButton.Click += (_, _) => CloseRequested?.Invoke(this, EventArgs.Empty);

        _rootLayoutPanel.Controls.Add(_logoPanel, IconCol, 0);
        _rootLayoutPanel.Controls.Add(textLayoutPanel, TitleCol, 0);
        _rootLayoutPanel.Controls.Add(_statusPill, StatusCol, 0);
        _rootLayoutPanel.Controls.Add(_minimizeButton, MinCol, 0);
        _rootLayoutPanel.Controls.Add(_maximizeButton, MaxCol, 0);
        _rootLayoutPanel.Controls.Add(_closeButton, CloseCol, 0);

        Controls.Add(_rootLayoutPanel);
        AttachDragForwarding(this);
        ApplyDpiMetrics();
    }

    public event EventHandler? DragRequested;
    public event EventHandler? MinimizeRequested;
    public event EventHandler? MaximizeRestoreRequested;
    public event EventHandler? CloseRequested;

    public void SetTitle(string title)
    {
        _appNameLabel.Text = title;
    }

    public void SetStatus(string status, Color color)
    {
        _statusPill.SetStatus(status, color);
    }

    public void SetMaximized(bool isMaximized)
    {
        _maximizeButton.Kind = isMaximized
            ? CaptionButtonKind.Restore
            : CaptionButtonKind.Maximize;
    }

    public bool IsInteractiveClientPoint(Point clientPoint)
    {
        Control? hitControl = FindDeepestChildAtPoint(this, clientPoint);
        return hitControl is Button;
    }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
        e.Graphics.Clear(DesignTokens.Background);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        using var bottomPen = new Pen(DesignTokens.BorderSoft);
        e.Graphics.DrawLine(bottomPen, 0, Height - 1, Width, Height - 1);
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        ApplyDpiMetrics();
    }

    protected override void OnParentChanged(EventArgs e)
    {
        base.OnParentChanged(e);
        ApplyDpiMetrics();
    }

    private void ApplyDpiMetrics()
    {
        Font = DesignTokens.FontUiNormal;
        _appNameLabel.Font = DesignTokens.FontUiBold;
        MinimumSize = new Size(0, DesignTokens.TitleBarHeight);

        _rootLayoutPanel.Padding = new Padding(
            0,
            0,
            DesignTokens.Scale(10),
            DesignTokens.Scale(2));

        _rootLayoutPanel.ColumnStyles[IconCol].Width = DesignTokens.TitleBarIconSize + DesignTokens.Scale(8);
        _rootLayoutPanel.ColumnStyles[StatusCol].Width = DesignTokens.Scale(132);
        _rootLayoutPanel.ColumnStyles[MinCol].Width = DesignTokens.TitleBarButtonWidth;
        _rootLayoutPanel.ColumnStyles[MaxCol].Width = DesignTokens.TitleBarButtonWidth;
        _rootLayoutPanel.ColumnStyles[CloseCol].Width = DesignTokens.TitleBarButtonWidth;
        _rootLayoutPanel.ColumnStyles[SpacerCol].Width = DesignTokens.Scale(2);

        _logoPanel.Margin = new Padding(0, 0, DesignTokens.Scale(8), 0);
        _logoPanel.Invalidate();

        _statusPill.Margin = new Padding(
            DesignTokens.Scale(6),
            DesignTokens.Scale(2),
            DesignTokens.Scale(8),
            DesignTokens.Scale(2));
        _statusPill.ApplyDpiMetrics();
        _minimizeButton.ApplyDpiMetrics();
        _maximizeButton.ApplyDpiMetrics();
        _closeButton.ApplyDpiMetrics();

        PerformLayout();
        Invalidate();
    }

    private enum CaptionButtonKind
    {
        Minimize,
        Maximize,
        Restore,
        Close
    }

    private static Control? FindDeepestChildAtPoint(Control parent, Point point)
    {
        for (int index = parent.Controls.Count - 1; index >= 0; index--)
        {
            Control child = parent.Controls[index];
            if (!child.Visible || !child.Enabled || !child.Bounds.Contains(point))
            {
                continue;
            }

            var childPoint = new Point(point.X - child.Left, point.Y - child.Top);
            return FindDeepestChildAtPoint(child, childPoint) ?? child;
        }

        return null;
    }

    private void AttachDragForwarding(Control control)
    {
        control.MouseDown += titleBarControl_MouseDown;

        foreach (Control child in control.Controls)
        {
            AttachDragForwarding(child);
        }
    }

    private void titleBarControl_MouseDown(object? sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left)
        {
            return;
        }

        Point screenPoint = ((Control)sender!).PointToScreen(e.Location);
        Point titleBarPoint = PointToClient(screenPoint);
        if (IsInteractiveClientPoint(titleBarPoint))
        {
            return;
        }

        DragRequested?.Invoke(this, EventArgs.Empty);
    }

    private sealed class StatusPillControl : Control
    {
        private string _statusText = "Hazir";
        private Color _dotColor = DesignTokens.AccentGreen;

        public StatusPillControl()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.SupportsTransparentBackColor |
                ControlStyles.UserPaint,
                true);

            Font = DesignTokens.FontUiNormal;
            ForeColor = DesignTokens.TextSecondary;
        }

        public void ApplyDpiMetrics()
        {
            Font = DesignTokens.FontUiNormal;
            ForeColor = DesignTokens.TextSecondary;
            Invalidate();
        }

        public void SetStatus(string status, Color dotColor)
        {
            _statusText = string.IsNullOrWhiteSpace(status)
                ? "Hazir"
                : status;
            _dotColor = dotColor;
            Invalidate();
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            e.Graphics.Clear(Parent?.BackColor == Color.Transparent
                ? DesignTokens.Background
                : Parent?.BackColor ?? DesignTokens.Background);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Rectangle bounds = Rectangle.Inflate(ClientRectangle, -1, -1);
            if (bounds.Width <= 0 || bounds.Height <= 0)
            {
                return;
            }

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            using var path = CreateRoundedRectanglePath(bounds, DesignTokens.Scale(8));
            using var fillBrush = new SolidBrush(DesignTokens.SurfaceInset);
            using var borderPen = new Pen(DesignTokens.BorderSoft);

            e.Graphics.FillPath(fillBrush, path);
            e.Graphics.DrawPath(borderPen, path);

            int dotSize = DesignTokens.TitleBarStatusDotSize;
            int dotX = bounds.Left + DesignTokens.Scale(12);
            int dotY = bounds.Top + (bounds.Height - dotSize) / 2;
            var dotBounds = new Rectangle(dotX, dotY, dotSize, dotSize);

            using var glowBrush = new SolidBrush(Color.FromArgb(55, _dotColor));
            using var dotBrush = new SolidBrush(_dotColor);
            e.Graphics.FillEllipse(glowBrush, Rectangle.Inflate(dotBounds, DesignTokens.Scale(2), DesignTokens.Scale(2)));
            e.Graphics.FillEllipse(dotBrush, dotBounds);

            e.Graphics.SmoothingMode = SmoothingMode.None;

            var textBounds = new Rectangle(
                dotBounds.Right + DesignTokens.Scale(8),
                bounds.Top,
                Math.Max(0, bounds.Right - dotBounds.Right - DesignTokens.Scale(16)),
                bounds.Height);

            TextRenderer.DrawText(
                e.Graphics,
                _statusText,
                Font,
                textBounds,
                ForeColor,
                TextFormatFlags.Left |
                TextFormatFlags.VerticalCenter |
                TextFormatFlags.EndEllipsis |
                TextFormatFlags.NoPrefix);
        }
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

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            pevent.Graphics.Clear(DesignTokens.Background);
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
            Margin = Padding.Empty;
            MinimumSize = Size.Empty;
            TabStop = false;
            Cursor = Cursors.Hand;
            FlatAppearance.BorderSize = 0;
            BackColor = DesignTokens.Background;
            ApplyDpiMetrics();
        }

        public void ApplyDpiMetrics()
        {
            Margin = new Padding(
                DesignTokens.Scale(2),
                DesignTokens.Scale(1),
                0,
                DesignTokens.Scale(7));
            MinimumSize = new Size(DesignTokens.TitleBarButtonWidth, DesignTokens.TitleBarButtonHeight);
            Invalidate();
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
            pevent.Graphics.Clear(DesignTokens.Background);
            pevent.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            pevent.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

            Color fill = ResolveFillColor();
            Color icon = Enabled
                ? DesignTokens.TextPrimary
                : DesignTokens.TextMuted;

            int horizontalInset = DesignTokens.Scale(3);
            int topInset = DesignTokens.Scale(1);
            int bottomInset = DesignTokens.Scale(5);
            Rectangle buttonBounds = new(
                horizontalInset,
                topInset,
                Math.Max(0, Width - (horizontalInset * 2)),
                Math.Max(0, Height - topInset - bottomInset));
            using var path = CreateRoundedRectanglePath(buttonBounds, DesignTokens.Scale(8));
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
