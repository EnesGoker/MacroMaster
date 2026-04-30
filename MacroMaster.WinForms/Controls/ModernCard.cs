using System.ComponentModel;
using System.Drawing.Drawing2D;
using MacroMaster.WinForms.Theme;

namespace MacroMaster.WinForms.Controls;

internal sealed class ModernCard : Panel
{
    private int _cornerRadius = AppRadii.Md;
    private int _borderThickness = 1;
    private Color _borderColor = AppColors.CardBorder;
    private Color _cardBackColor = AppColors.Card;

    public ModernCard()
    {
        SetStyle(
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.ResizeRedraw |
            ControlStyles.UserPaint |
            ControlStyles.SupportsTransparentBackColor,
            true);

        DoubleBuffered = true;
        BackColor = Color.Transparent;
        ForeColor = AppColors.TextPrimary;
        Padding = AppSpacing.CardPadding;
        Margin = new Padding(AppSpacing.Lg);
        Size = new Size(240, 160);

        UpdateCardRegion();
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
            UpdateCardRegion();
            Invalidate();
        }
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int BorderThickness
    {
        get => _borderThickness;
        set
        {
            var sanitizedValue = Math.Max(1, value);
            if (_borderThickness == sanitizedValue)
            {
                return;
            }

            _borderThickness = sanitizedValue;
            Invalidate();
        }
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Color BorderColor
    {
        get => _borderColor;
        set
        {
            if (_borderColor == value)
            {
                return;
            }

            _borderColor = value;
            Invalidate();
        }
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Color CardBackColor
    {
        get => _cardBackColor;
        set
        {
            if (_cardBackColor == value)
            {
                return;
            }

            _cardBackColor = value;
            Invalidate();
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

        var surfaceBounds = ClientRectangle;
        if (surfaceBounds.Width <= 0 || surfaceBounds.Height <= 0)
        {
            return;
        }

        var paintBounds = Rectangle.Inflate(surfaceBounds, -1, -1);
        using var path = CreateRoundedRectanglePath(paintBounds, _cornerRadius);
        using var backgroundBrush = new SolidBrush(_cardBackColor);
        using var borderPen = new Pen(_borderColor, _borderThickness)
        {
            Alignment = PenAlignment.Inset
        };

        e.Graphics.FillPath(backgroundBrush, path);
        e.Graphics.DrawPath(borderPen, path);
    }

    protected override void OnSizeChanged(EventArgs e)
    {
        base.OnSizeChanged(e);
        UpdateCardRegion();
    }

    private void UpdateCardRegion()
    {
        if (Width <= 0 || Height <= 0)
        {
            return;
        }

        using var path = CreateRoundedRectanglePath(ClientRectangle, _cornerRadius);
        var previousRegion = Region;
        Region = new Region(path);
        previousRegion?.Dispose();
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
