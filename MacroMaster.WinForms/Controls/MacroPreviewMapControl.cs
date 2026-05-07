using MacroMaster.WinForms.Theme;
using System.Drawing.Drawing2D;

namespace MacroMaster.WinForms.Controls;

internal readonly record struct MacroPreviewMapState(
    int EventCount,
    int TotalDurationMs,
    string StatusText);

internal sealed class MacroPreviewMapControl : Control
{
    private static readonly PointF[] PlaceholderPathPoints =
    [
        new(0.12f, 0.72f),
        new(0.26f, 0.54f),
        new(0.44f, 0.62f),
        new(0.58f, 0.38f),
        new(0.74f, 0.46f),
        new(0.86f, 0.24f)
    ];

    private MacroPreviewMapState _state;

    public event EventHandler? PreviewRequested;

    public MacroPreviewMapControl()
    {
        SetStyle(
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.ResizeRedraw |
            ControlStyles.UserPaint |
            ControlStyles.Selectable,
            true);

        Dock = DockStyle.Fill;
        Margin = Padding.Empty;
        BackColor = DesignTokens.SurfaceInset;
        Cursor = Cursors.Hand;
        TabStop = true;
        AccessibleName = "Makro onizleme haritasi";
        AccessibleDescription = "Secili makronun fare rotasini kucuk harita olarak gosterir.";
    }

    public void UpdatePreview(
        int eventCount,
        int durationMs,
        string statusText)
    {
        UpdatePreview(new MacroPreviewMapState(
            Math.Max(0, eventCount),
            Math.Max(0, durationMs),
            statusText));
    }

    public void UpdatePreview(MacroPreviewMapState state)
    {
        _state = new MacroPreviewMapState(
            Math.Max(0, state.EventCount),
            Math.Max(0, state.TotalDurationMs),
            state.StatusText ?? string.Empty);
        Invalidate();
    }

    protected override void OnClick(EventArgs e)
    {
        base.OnClick(e);
        PreviewRequested?.Invoke(this, EventArgs.Empty);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (e.KeyCode is Keys.Enter or Keys.Space)
        {
            e.Handled = true;
            PreviewRequested?.Invoke(this, EventArgs.Empty);
        }
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

        if (_state.EventCount <= 0)
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
        PointF[] resolved = new PointF[PlaceholderPathPoints.Length];

        for (int index = 0; index < PlaceholderPathPoints.Length; index++)
        {
            resolved[index] = new PointF(
                bounds.Left + bounds.Width * PlaceholderPathPoints[index].X,
                bounds.Top + bounds.Height * PlaceholderPathPoints[index].Y);
        }

        return resolved;
    }

    private PointF ResolveActivePoint(PointF[] resolvedPath)
    {
        uint seed = unchecked((uint)HashCode.Combine(
            _state.EventCount,
            _state.TotalDurationMs,
            _state.StatusText));
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
