using System.Drawing.Drawing2D;

namespace MacroMaster.WinForms.Theme;

internal sealed class AppToolStripRenderer : ToolStripProfessionalRenderer
{
    public static readonly AppToolStripRenderer Instance = new();

    private static readonly Color DropDownBackground = DesignTokens.Surface;
    private static readonly Color DropDownBorder = DesignTokens.Border;
    private static readonly Color ItemBackground = DesignTokens.Surface2;
    private static readonly Color ItemHoverBackground = DesignTokens.SurfaceHover;
    private static readonly Color ItemPressedBackground = DesignTokens.Surface3;
    private static readonly Color DisabledText = DesignTokens.TextMuted;

    private AppToolStripRenderer()
        : base(new AppToolStripColorTable())
    {
        RoundedEdges = false;
    }

    public static void ApplyTo(ContextMenuStrip menu)
    {
        ArgumentNullException.ThrowIfNull(menu);

        menu.BackColor = DropDownBackground;
        menu.ForeColor = DesignTokens.TextPrimary;
        menu.Font = DesignTokens.FontUiNormal;
        menu.Padding = new Padding(DesignTokens.Scale(4));
        menu.RenderMode = ToolStripRenderMode.Professional;
        menu.Renderer = Instance;

        ApplyItems(menu.Items);

        menu.Opening += (_, _) => ApplyItems(menu.Items);
    }

    protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);

        using var brush = new SolidBrush(DropDownBackground);
        e.Graphics.FillRectangle(brush, e.AffectedBounds);
    }

    protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);

        using var borderPen = new Pen(DropDownBorder);
        e.Graphics.DrawRectangle(borderPen, 0, 0, e.ToolStrip.Width - 1, e.ToolStrip.Height - 1);
    }

    protected override void OnRenderImageMargin(ToolStripRenderEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);

        using var brush = new SolidBrush(DropDownBackground);
        e.Graphics.FillRectangle(brush, e.AffectedBounds);
    }

    protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);

        int y = e.Item.Height / 2;
        using var pen = new Pen(DesignTokens.BorderSoft);
        e.Graphics.DrawLine(pen, DesignTokens.Scale(8), y, e.Item.Width - DesignTokens.Scale(8), y);
    }

    protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);

        Color fill = e.Item.Pressed
            ? ItemPressedBackground
            : e.Item.Selected
                ? ItemHoverBackground
                : ItemBackground;

        Rectangle bounds = new(Point.Empty, e.Item.Size);
        Rectangle itemBounds = Rectangle.Inflate(bounds, -DesignTokens.Scale(3), -DesignTokens.Scale(2));

        using var path = CreateRoundedRectanglePath(itemBounds, DesignTokens.Scale(7));
        using var brush = new SolidBrush(fill);

        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        e.Graphics.FillPath(brush, path);

        if (e.Item.Selected || e.Item.Pressed)
        {
            using var borderPen = new Pen(Color.FromArgb(110, DesignTokens.Accent));
            e.Graphics.DrawPath(borderPen, path);
        }

        e.Graphics.SmoothingMode = SmoothingMode.None;
    }

    protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);

        e.TextColor = e.Item.Enabled
            ? DesignTokens.TextPrimary
            : DisabledText;

        base.OnRenderItemText(e);
    }

    protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);

        e.ArrowColor = e.Item?.Enabled != false
            ? DesignTokens.TextSecondary
            : DisabledText;

        base.OnRenderArrow(e);
    }

    protected override void OnRenderItemCheck(ToolStripItemImageRenderEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);

        Rectangle checkBounds = Rectangle.Inflate(e.ImageRectangle, DesignTokens.Scale(2), DesignTokens.Scale(2));
        using var path = CreateRoundedRectanglePath(checkBounds, DesignTokens.Scale(4));
        using var fillBrush = new SolidBrush(Color.FromArgb(40, DesignTokens.Accent));
        using var borderPen = new Pen(DesignTokens.Accent);

        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        e.Graphics.FillPath(fillBrush, path);
        e.Graphics.DrawPath(borderPen, path);

        if (e.Image is not null)
        {
            e.Graphics.DrawImage(e.Image, e.ImageRectangle);
        }

        e.Graphics.SmoothingMode = SmoothingMode.None;
    }

    private static void ApplyItems(ToolStripItemCollection items)
    {
        foreach (ToolStripItem item in items)
        {
            if (item is ToolStripSeparator)
            {
                item.Margin = new Padding(DesignTokens.Scale(4), DesignTokens.Scale(3), DesignTokens.Scale(4), DesignTokens.Scale(3));
                continue;
            }

            item.BackColor = ItemBackground;
            item.ForeColor = item.Enabled
                ? DesignTokens.TextPrimary
                : DisabledText;
            item.Font = DesignTokens.FontUiNormal;
            item.Padding = new Padding(DesignTokens.Scale(10), DesignTokens.Scale(5), DesignTokens.Scale(10), DesignTokens.Scale(5));

            if (item is ToolStripDropDownItem dropDownItem)
            {
                dropDownItem.DropDown.BackColor = DropDownBackground;
                dropDownItem.DropDown.ForeColor = DesignTokens.TextPrimary;
                dropDownItem.DropDown.Font = DesignTokens.FontUiNormal;
                dropDownItem.DropDown.RenderMode = ToolStripRenderMode.Professional;
                dropDownItem.DropDown.Renderer = Instance;
                ApplyItems(dropDownItem.DropDownItems);
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

    private sealed class AppToolStripColorTable : ProfessionalColorTable
    {
        public override Color ToolStripDropDownBackground => DropDownBackground;
        public override Color ImageMarginGradientBegin => DropDownBackground;
        public override Color ImageMarginGradientMiddle => DropDownBackground;
        public override Color ImageMarginGradientEnd => DropDownBackground;
        public override Color MenuBorder => DropDownBorder;
        public override Color MenuItemBorder => DesignTokens.Accent;
        public override Color MenuItemSelected => ItemHoverBackground;
        public override Color MenuItemSelectedGradientBegin => ItemHoverBackground;
        public override Color MenuItemSelectedGradientEnd => ItemHoverBackground;
        public override Color MenuItemPressedGradientBegin => ItemPressedBackground;
        public override Color MenuItemPressedGradientMiddle => ItemPressedBackground;
        public override Color MenuItemPressedGradientEnd => ItemPressedBackground;
        public override Color SeparatorDark => DesignTokens.BorderSoft;
        public override Color SeparatorLight => DropDownBackground;
    }
}
