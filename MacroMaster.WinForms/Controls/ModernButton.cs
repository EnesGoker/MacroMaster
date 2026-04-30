using System.ComponentModel;
using System.Drawing.Drawing2D;
using MacroMaster.WinForms.Theme;

namespace MacroMaster.WinForms.Controls;

internal enum ModernButtonVariant
{
    Primary,
    Danger,
    Secondary,
    Ghost,
    Success
}

internal sealed class ModernButton : Button
{
    private ModernButtonVariant _variant = ModernButtonVariant.Primary;
    private int _cornerRadius = AppRadii.Sm;
    private bool _isHovered;
    private bool _isPressed;

    public ModernButton()
    {
        SetStyle(
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.ResizeRedraw |
            ControlStyles.UserPaint,
            true);

        DoubleBuffered = true;
        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize = 0;
        UseVisualStyleBackColor = false;
        BackColor = AppColors.Primary;
        ForeColor = AppColors.TextPrimary;
        Font = AppFonts.Button;
        Height = 40;
        Width = 132;
        Padding = new Padding(AppSpacing.Md, AppSpacing.Sm, AppSpacing.Md, AppSpacing.Sm);
        Cursor = Cursors.Hand;
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public ModernButtonVariant Variant
    {
        get => _variant;
        set
        {
            if (_variant == value)
            {
                return;
            }

            _variant = value;
            Invalidate();
        }
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int CornerRadius
    {
        get => _cornerRadius;
        set
        {
            var sanitizedValue = Math.Max(0, value);
            if (_cornerRadius == sanitizedValue)
            {
                return;
            }

            _cornerRadius = sanitizedValue;
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
        _isHovered = true;
        Invalidate();
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        _isHovered = false;
        _isPressed = false;
        Invalidate();
    }

    protected override void OnMouseDown(MouseEventArgs mevent)
    {
        base.OnMouseDown(mevent);
        if (mevent.Button == MouseButtons.Left)
        {
            _isPressed = true;
            Invalidate();
        }
    }

    protected override void OnMouseUp(MouseEventArgs mevent)
    {
        base.OnMouseUp(mevent);
        _isPressed = false;
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs pevent)
    {
        pevent.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        pevent.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

        var bounds = Rectangle.Inflate(ClientRectangle, -1, -1);
        if (bounds.Width <= 0 || bounds.Height <= 0)
        {
            return;
        }

        var palette = ResolvePalette();
        using var path = CreateRoundedRectanglePath(bounds, _cornerRadius);
        using var backgroundBrush = new SolidBrush(palette.Background);
        using var borderPen = new Pen(palette.Border, 1) { Alignment = PenAlignment.Inset };

        pevent.Graphics.FillPath(backgroundBrush, path);
        pevent.Graphics.DrawPath(borderPen, path);

        TextRenderer.DrawText(
            pevent.Graphics,
            Text,
            Font,
            bounds,
            palette.Foreground,
            TextFormatFlags.HorizontalCenter |
            TextFormatFlags.VerticalCenter |
            TextFormatFlags.EndEllipsis);
    }

    private (Color Background, Color Border, Color Foreground) ResolvePalette()
    {
        if (!Enabled)
        {
            return (
                Background: Color.FromArgb(24, 34, 58),
                Border: AppColors.CardBorder,
                Foreground: AppColors.TextMuted);
        }

        var accentColor = _variant switch
        {
            ModernButtonVariant.Primary => AppColors.Primary,
            ModernButtonVariant.Danger => AppColors.Danger,
            ModernButtonVariant.Success => AppColors.Success,
            ModernButtonVariant.Secondary => Color.FromArgb(30, 41, 59),
            ModernButtonVariant.Ghost => Color.FromArgb(0, 0, 0, 0),
            _ => AppColors.Primary
        };

        var background = _variant == ModernButtonVariant.Ghost
            ? Color.Transparent
            : accentColor;

        var border = _variant == ModernButtonVariant.Ghost
            ? AppColors.CardBorder
            : accentColor;

        var foreground = AppColors.TextPrimary;

        if (_variant == ModernButtonVariant.Ghost)
        {
            foreground = _isHovered ? AppColors.TextPrimary : AppColors.TextSecondary;
            background = _isPressed
                ? Color.FromArgb(40, 54, 84)
                : _isHovered
                    ? Color.FromArgb(20, 28, 48)
                    : Color.Transparent;
        }
        else if (_isPressed)
        {
            background = ControlPaint.Dark(accentColor, 0.18f);
            border = ControlPaint.Dark(accentColor, 0.25f);
        }
        else if (_isHovered)
        {
            background = ControlPaint.Light(accentColor, 0.08f);
            border = ControlPaint.Light(accentColor, 0.03f);
        }

        return (background, border, foreground);
    }

    private static GraphicsPath CreateRoundedRectanglePath(Rectangle bounds, int radius)
    {
        var sanitizedRadius = Math.Max(0, radius);
        var diameter = Math.Min(sanitizedRadius * 2, Math.Min(bounds.Width, bounds.Height));

        if (diameter <= 0)
        {
            var rectanglePath = new GraphicsPath();
            rectanglePath.AddRectangle(bounds);
            rectanglePath.CloseFigure();
            return rectanglePath;
        }

        var arc = new Rectangle(bounds.Location, new Size(diameter, diameter));
        var path = new GraphicsPath();

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
