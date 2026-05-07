using MacroMaster.WinForms.Theme;
using System.Drawing.Drawing2D;
using System.Globalization;

namespace MacroMaster.WinForms.Controls;

public readonly record struct SessionSummaryState(
    string StatusText,
    string SessionName,
    int EventCount,
    int TotalDurationMs,
    string FileName);

internal sealed class SessionSummaryControl : UserControl
{
    private readonly Label _statusValueLabel;
    private readonly Label _eventCountValueLabel;
    private readonly Label _durationValueLabel;
    private readonly Label _sessionNameValueLabel;
    private readonly Label _fileNameValueLabel;
    private readonly PreviewMapPanel _previewMapPanel;

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
        _previewMapPanel = new PreviewMapPanel();

        BuildLayout();
        UpdateState(new SessionSummaryState("Bos", "Oturum yok", 0, 0, "Kaydedilmedi"));
    }

    public void UpdateState(SessionSummaryState state)
    {
        string sessionName = string.IsNullOrWhiteSpace(state.SessionName)
            ? "Oturum yok"
            : state.SessionName;
        string fileName = string.IsNullOrWhiteSpace(state.FileName)
            ? "Kaydedilmedi"
            : state.FileName;

        _statusValueLabel.Text = state.StatusText;
        _eventCountValueLabel.Text = state.EventCount.ToString(CultureInfo.InvariantCulture);
        _durationValueLabel.Text = FormattableString.Invariant($"{Math.Max(0, state.TotalDurationMs)} ms");
        _sessionNameValueLabel.Text = sessionName;
        _fileNameValueLabel.Text = fileName;
        _previewMapPanel.UpdatePreview(state.EventCount, state.TotalDurationMs, state.StatusText);
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
            RowCount = 2,
            BackColor = DesignTokens.Surface,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        rootLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        rootLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, DesignTokens.Scale(168)));
        rootLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

        rootLayoutPanel.Controls.Add(CreateCompactDetailsPanel(), 0, 0);
        rootLayoutPanel.Controls.Add(CreateMapSection(), 0, 1);

        Controls.Add(rootLayoutPanel);
    }

    private SoftPanel CreateCompactDetailsPanel()
    {
        var panel = new SoftPanel
        {
            Dock = DockStyle.Fill,
            BackColor = DesignTokens.SurfaceInset,
            BorderColor = DesignTokens.BorderSoft,
            Margin = new Padding(0, 0, 0, DesignTokens.Scale(14)),
            Padding = new Padding(
                DesignTokens.Scale(14),
                DesignTokens.Scale(12),
                DesignTokens.Scale(14),
                DesignTokens.Scale(12))
        };

        var layoutPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 5,
            BackColor = Color.Transparent,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        layoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, DesignTokens.Scale(76)));
        layoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));

        for (int index = 0; index < 5; index++)
        {
            layoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 20f));
        }

        AddDetailRow(layoutPanel, 0, "Durum", _statusValueLabel);
        AddDetailRow(layoutPanel, 1, "Olay", _eventCountValueLabel);
        AddDetailRow(layoutPanel, 2, "Sure", _durationValueLabel);
        AddDetailRow(layoutPanel, 3, "Oturum", _sessionNameValueLabel);
        AddDetailRow(layoutPanel, 4, "Dosya", _fileNameValueLabel);

        panel.Controls.Add(layoutPanel);
        return panel;
    }

    private TableLayoutPanel CreateMapSection()
    {
        var mapLayoutPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = DesignTokens.Surface,
            Margin = new Padding(0, 0, 0, DesignTokens.Scale(10)),
            Padding = Padding.Empty
        };
        mapLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        mapLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, DesignTokens.Scale(28)));
        mapLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
        mapLayoutPanel.Controls.Add(CreateSectionCaptionLabel("Ekran Onizleme"), 0, 0);
        mapLayoutPanel.Controls.Add(_previewMapPanel, 0, 1);
        return mapLayoutPanel;
    }

    private static void AddDetailRow(
        TableLayoutPanel layoutPanel,
        int rowIndex,
        string caption,
        Label valueLabel)
    {
        valueLabel.Font = DesignTokens.FontUiBold;
        valueLabel.ForeColor = DesignTokens.TextPrimary;
        valueLabel.Margin = new Padding(DesignTokens.Scale(4), 0, 0, 0);
        valueLabel.TextAlign = ContentAlignment.MiddleLeft;

        layoutPanel.Controls.Add(CreateCaptionLabel(caption), 0, rowIndex);
        layoutPanel.Controls.Add(valueLabel, 1, rowIndex);
    }

    private static Label CreateSectionCaptionLabel(string text)
    {
        return new Label
        {
            Dock = DockStyle.Fill,
            Text = text,
            Font = DesignTokens.FontUiBold,
            ForeColor = DesignTokens.TextPrimary,
            BackColor = Color.Transparent,
            TextAlign = ContentAlignment.MiddleLeft,
            AutoEllipsis = true
        };
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

    private sealed class PreviewMapPanel : Control
    {
        private static readonly PointF[] PathPoints =
        [
            new(0.12f, 0.72f),
            new(0.26f, 0.54f),
            new(0.44f, 0.62f),
            new(0.58f, 0.38f),
            new(0.74f, 0.46f),
            new(0.86f, 0.24f)
        ];

        private int _eventCount;
        private int _durationMs;
        private string _statusText = string.Empty;

        public PreviewMapPanel()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint,
                true);

            Dock = DockStyle.Fill;
            Margin = Padding.Empty;
            BackColor = DesignTokens.SurfaceInset;
        }

        public void UpdatePreview(
            int eventCount,
            int durationMs,
            string statusText)
        {
            _eventCount = Math.Max(0, eventCount);
            _durationMs = Math.Max(0, durationMs);
            _statusText = statusText;
            Invalidate();
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            pevent.Graphics.Clear(Parent?.BackColor ?? DesignTokens.Surface);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics graphics = e.Graphics;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle bounds = Rectangle.Inflate(ClientRectangle, -1, -1);
            if (bounds.Width <= 0 || bounds.Height <= 0)
            {
                return;
            }

            using GraphicsPath path = CreateRoundPath(bounds, DesignTokens.Scale(9));
            using var fillBrush = new SolidBrush(DesignTokens.SurfaceInset);
            using var borderPen = new Pen(DesignTokens.BorderSoft);
            graphics.FillPath(fillBrush, path);
            graphics.DrawPath(borderPen, path);

            Rectangle plotBounds = Rectangle.Inflate(bounds, -DesignTokens.Scale(10), -DesignTokens.Scale(10));
            DrawGrid(graphics, plotBounds);

            if (_eventCount <= 0)
            {
                DrawEmptyState(graphics, plotBounds);
                return;
            }

            PointF[] resolvedPath = ResolvePath(plotBounds);
            using var routePen = new Pen(Color.FromArgb(140, DesignTokens.Accent), Math.Max(2f, DesignTokens.Scale(2)))
            {
                StartCap = LineCap.Round,
                EndCap = LineCap.Round,
                LineJoin = LineJoin.Round
            };
            graphics.DrawCurve(routePen, resolvedPath, 0.35f);

            DrawPoint(graphics, resolvedPath[0], DesignTokens.AccentGreen, DesignTokens.Scale(5));
            DrawPoint(graphics, resolvedPath[^1], DesignTokens.AccentOrange, DesignTokens.Scale(5));
            DrawPoint(graphics, resolvedPath[2], DesignTokens.AccentRed, DesignTokens.Scale(4));
            DrawPoint(graphics, resolvedPath[4], DesignTokens.AccentPurple, DesignTokens.Scale(4));

            PointF activePoint = ResolveActivePoint(resolvedPath);
            using var glowBrush = new SolidBrush(Color.FromArgb(42, DesignTokens.Accent));
            float glowRadius = DesignTokens.Scale(18);
            graphics.FillEllipse(
                glowBrush,
                activePoint.X - glowRadius,
                activePoint.Y - glowRadius,
                glowRadius * 2,
                glowRadius * 2);
            DrawPoint(graphics, activePoint, DesignTokens.Accent, DesignTokens.Scale(9));
        }

        private static void DrawGrid(Graphics graphics, Rectangle bounds)
        {
            if (bounds.Width <= 0 || bounds.Height <= 0)
            {
                return;
            }

            using var minorPen = new Pen(Color.FromArgb(22, DesignTokens.BorderBright));
            using var majorPen = new Pen(Color.FromArgb(44, DesignTokens.BorderBright));
            int minorStep = Math.Max(DesignTokens.Scale(10), 6);
            int majorStep = minorStep * 4;

            for (int x = bounds.Left; x <= bounds.Right; x += minorStep)
            {
                bool isMajor = (x - bounds.Left) % majorStep == 0;
                graphics.DrawLine(isMajor ? majorPen : minorPen, x, bounds.Top, x, bounds.Bottom);
            }

            for (int y = bounds.Top; y <= bounds.Bottom; y += minorStep)
            {
                bool isMajor = (y - bounds.Top) % majorStep == 0;
                graphics.DrawLine(isMajor ? majorPen : minorPen, bounds.Left, y, bounds.Right, y);
            }
        }

        private static void DrawEmptyState(Graphics graphics, Rectangle bounds)
        {
            TextRenderer.DrawText(
                graphics,
                "Harita icin makro secin",
                DesignTokens.FontUiSmall,
                bounds,
                DesignTokens.TextMuted,
                TextFormatFlags.HorizontalCenter |
                TextFormatFlags.VerticalCenter |
                TextFormatFlags.EndEllipsis |
                TextFormatFlags.NoPrefix);
        }

        private static PointF[] ResolvePath(Rectangle bounds)
        {
            PointF[] resolved = new PointF[PathPoints.Length];

            for (int index = 0; index < PathPoints.Length; index++)
            {
                resolved[index] = new PointF(
                    bounds.Left + bounds.Width * PathPoints[index].X,
                    bounds.Top + bounds.Height * PathPoints[index].Y);
            }

            return resolved;
        }

        private PointF ResolveActivePoint(PointF[] resolvedPath)
        {
            uint seed = unchecked((uint)HashCode.Combine(_eventCount, _durationMs, _statusText));
            int index = (int)(seed % (uint)resolvedPath.Length);
            return resolvedPath[index];
        }

        private static void DrawPoint(
            Graphics graphics,
            PointF point,
            Color color,
            int radius)
        {
            using var brush = new SolidBrush(color);
            using var borderPen = new Pen(Color.FromArgb(180, DesignTokens.TextPrimary), Math.Max(1f, DesignTokens.DensityScale));
            var bounds = new RectangleF(
                point.X - radius,
                point.Y - radius,
                radius * 2f,
                radius * 2f);
            graphics.FillEllipse(brush, bounds);
            graphics.DrawEllipse(borderPen, bounds);
        }
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
