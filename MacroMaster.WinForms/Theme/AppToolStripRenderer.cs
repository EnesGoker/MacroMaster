using System.Drawing.Drawing2D;

namespace MacroMaster.WinForms.Theme;

internal enum AppToolStripMenuDensity
{
    Standard,
    Comfortable
}

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

    public static void ApplyTo(
        ContextMenuStrip menu,
        AppToolStripMenuDensity density = AppToolStripMenuDensity.Standard)
    {
        ArgumentNullException.ThrowIfNull(menu);

        menu.BackColor = DropDownBackground;
        menu.ForeColor = DesignTokens.TextPrimary;
        menu.Font = DesignTokens.FontUiNormal;
        menu.Padding = ResolveMenuPadding(density);
        menu.RenderMode = ToolStripRenderMode.Professional;
        menu.Renderer = Instance;

        ApplyItems(menu.Items, density);
        menu.ItemAdded += (_, eventArgs) =>
        {
            if (eventArgs.Item is not null)
            {
                ApplyItem(eventArgs.Item, density);
            }
        };

        menu.Opening += (_, _) =>
        {
            ApplyItems(menu.Items, density);
            menu.PerformLayout();
        };
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

        Rectangle bounds = new(Point.Empty, e.Item.Size);
        using var backgroundBrush = new SolidBrush(DropDownBackground);
        e.Graphics.FillRectangle(backgroundBrush, bounds);

        if (!e.Item.Selected && !e.Item.Pressed)
        {
            return;
        }

        Rectangle itemBounds = ResolveItemBackgroundBounds(e.Item);

        using var path = CreateRoundedRectanglePath(itemBounds, DesignTokens.Scale(7));
        using var brush = new SolidBrush(e.Item.Pressed ? ItemPressedBackground : ItemHoverBackground);

        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        e.Graphics.FillPath(brush, path);
        using var borderPen = new Pen(Color.FromArgb(110, DesignTokens.Accent));
        e.Graphics.DrawPath(borderPen, path);
        e.Graphics.SmoothingMode = SmoothingMode.None;
    }

    protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);

        Color textColor = e.Item.Enabled
            ? DesignTokens.TextPrimary
            : DisabledText;

        Rectangle textBounds = ResolveTextBounds(e.Item, e.TextRectangle);

        TextRenderer.DrawText(
            e.Graphics,
            e.Text,
            e.TextFont,
            textBounds,
            textColor,
            TextFormatFlags.Left |
            TextFormatFlags.VerticalCenter |
            TextFormatFlags.EndEllipsis |
            TextFormatFlags.SingleLine |
            TextFormatFlags.NoPrefix);
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

    private static void ApplyItems(
        ToolStripItemCollection items,
        AppToolStripMenuDensity density)
    {
        foreach (ToolStripItem item in items)
        {
            ApplyItem(item, density);
        }
    }

    private static void ApplyItem(
        ToolStripItem item,
        AppToolStripMenuDensity density)
    {
        if (item is ToolStripSeparator)
        {
            item.Margin = ResolveSeparatorMargin(density);
            return;
        }

        item.BackColor = DropDownBackground;
        item.ForeColor = item.Enabled
            ? DesignTokens.TextPrimary
            : DisabledText;
        item.Font = DesignTokens.FontUiNormal;
        item.Margin = ResolveItemMargin(density);
        item.Padding = ResolveItemPadding(density);

        if (item is ToolStripDropDownItem dropDownItem)
        {
            dropDownItem.DropDown.BackColor = DropDownBackground;
            dropDownItem.DropDown.ForeColor = DesignTokens.TextPrimary;
            dropDownItem.DropDown.Font = DesignTokens.FontUiNormal;
            dropDownItem.DropDown.Padding = ResolveMenuPadding(density);
            dropDownItem.DropDown.RenderMode = ToolStripRenderMode.Professional;
            dropDownItem.DropDown.Renderer = Instance;
            ApplyItems(dropDownItem.DropDownItems, density);
        }
    }

    private static Padding ResolveMenuPadding(AppToolStripMenuDensity density)
    {
        return density == AppToolStripMenuDensity.Comfortable
            ? new Padding(DesignTokens.Scale(6))
            : new Padding(DesignTokens.Scale(4));
    }

    private static Padding ResolveItemMargin(AppToolStripMenuDensity density)
    {
        return density == AppToolStripMenuDensity.Comfortable
            ? new Padding(DesignTokens.Scale(2), DesignTokens.Scale(1), DesignTokens.Scale(2), DesignTokens.Scale(1))
            : Padding.Empty;
    }

    private static Padding ResolveItemPadding(AppToolStripMenuDensity density)
    {
        return density == AppToolStripMenuDensity.Comfortable
            ? new Padding(DesignTokens.Scale(12), DesignTokens.Scale(8), DesignTokens.Scale(16), DesignTokens.Scale(8))
            : new Padding(DesignTokens.Scale(10), DesignTokens.Scale(5), DesignTokens.Scale(10), DesignTokens.Scale(5));
    }

    private static Padding ResolveSeparatorMargin(AppToolStripMenuDensity density)
    {
        return density == AppToolStripMenuDensity.Comfortable
            ? new Padding(DesignTokens.Scale(8), DesignTokens.Scale(5), DesignTokens.Scale(8), DesignTokens.Scale(5))
            : new Padding(DesignTokens.Scale(4), DesignTokens.Scale(3), DesignTokens.Scale(4), DesignTokens.Scale(3));
    }

    private static Rectangle ResolveTextBounds(
        ToolStripItem item,
        Rectangle defaultTextRectangle)
    {
        int left = Math.Max(
            defaultTextRectangle.Left,
            item.Padding.Left + DesignTokens.Scale(2));
        int rightPadding = Math.Max(
            item.Padding.Right,
            DesignTokens.Scale(8));

        return new Rectangle(
            left,
            0,
            Math.Max(0, item.Width - left - rightPadding),
            item.Height);
    }

    private static Rectangle ResolveItemBackgroundBounds(ToolStripItem item)
    {
        bool isComfortableItem = item.Padding.Top >= DesignTokens.Scale(8);
        int horizontalInset = isComfortableItem
            ? DesignTokens.Scale(10)
            : DesignTokens.Scale(4);
        int verticalInset = isComfortableItem
            ? DesignTokens.Scale(5)
            : DesignTokens.Scale(2);
        int visibleWidth = item.Width;

        if (item.Owner is not null)
        {
            // ToolStripItem.Width can be wider than the visible drop-down client when
            // margins/padding are involved. Clamp to the owner viewport, but keep the
            // rectangle in item-local coordinates so the hover surface still spans the row.
            visibleWidth = Math.Min(
                visibleWidth,
                item.Owner.ClientSize.Width - DesignTokens.Scale(4));
        }

        Rectangle itemBounds = new(
            horizontalInset,
            verticalInset,
            Math.Max(0, visibleWidth - horizontalInset - DesignTokens.Scale(4)),
            Math.Max(0, item.Height - (verticalInset * 2)));

        if (itemBounds.Width <= 0 || itemBounds.Height <= 0)
        {
            return Rectangle.Empty;
        }

        return itemBounds;
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
