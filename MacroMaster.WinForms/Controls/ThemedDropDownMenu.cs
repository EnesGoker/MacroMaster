using MacroMaster.WinForms.Theme;
using System.Drawing.Drawing2D;

namespace MacroMaster.WinForms.Controls;

internal sealed record ThemedDropDownMenuItem(
    string Text,
    Action? Action = null,
    bool Enabled = true,
    bool Checked = false)
{
    public bool IsSeparator { get; init; }

    public static ThemedDropDownMenuItem Separator() =>
        new(string.Empty) { IsSeparator = true, Enabled = false };
}

internal sealed class ThemedDropDownMenuOptions
{
    public int MinimumWidth { get; init; } = DesignTokens.Scale(156);
    public int MaximumWidth { get; init; } = DesignTokens.Scale(360);
    public int? FixedWidth { get; init; }
    public int ItemHeight { get; init; } = DesignTokens.Scale(44);
    public int SeparatorHeight { get; init; } = DesignTokens.Scale(14);
    public int VerticalPadding { get; init; } = DesignTokens.Scale(8);
    public int HorizontalPadding { get; init; } = DesignTokens.Scale(10);
    public bool ReserveCheckColumn { get; init; }
    public bool PreferAboveWhenOverflow { get; init; } = true;
}

/// <summary>
/// Lightweight owner-drawn WinForms drop-down menu.
///
/// ContextMenuStrip is intentionally avoided here because its internal image/check
/// margin calculations are difficult to control in high-DPI, dark-themed layouts.
/// This component keeps all menu geometry, text alignment, hover states and checked
/// indicators under the application's own design-token system.
/// </summary>
internal sealed class ThemedDropDownMenu : ToolStripDropDown
{
    private readonly ThemedDropDownMenuContent _content;

    private ThemedDropDownMenu(
        Control owner,
        IReadOnlyList<ThemedDropDownMenuItem> items,
        ThemedDropDownMenuOptions options)
    {
        ArgumentNullException.ThrowIfNull(owner);
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(options);

        AutoClose = true;
        DoubleBuffered = true;
        Padding = Padding.Empty;
        Margin = Padding.Empty;
        BackColor = DesignTokens.Surface;
        ForeColor = DesignTokens.TextPrimary;
        Font = DesignTokens.FontUiNormal;
        GripStyle = ToolStripGripStyle.Hidden;

        _content = new ThemedDropDownMenuContent(owner, items, options);
        _content.ItemInvoked += (_, item) =>
        {
            Close(ToolStripDropDownCloseReason.ItemClicked);
            item.Action?.Invoke();
        };
        _content.CloseRequested += (_, _) => Close(ToolStripDropDownCloseReason.CloseCalled);

        var host = new ToolStripControlHost(_content)
        {
            AutoSize = false,
            Margin = Padding.Empty,
            Padding = Padding.Empty,
            Size = _content.Size
        };

        Items.Add(host);
        Size = _content.Size;
    }

    public static ToolStripDropDown ShowFor(
        Control owner,
        IReadOnlyList<ThemedDropDownMenuItem> items,
        Point location,
        ThemedDropDownMenuOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(owner);
        ArgumentNullException.ThrowIfNull(items);

        options ??= new ThemedDropDownMenuOptions();
        var dropDown = new ThemedDropDownMenu(owner, items, options);
        Point adjustedLocation = ResolveOwnerLocation(owner, location, dropDown.Size, options);

        dropDown.Closed += (_, _) =>
        {
            if (dropDown.IsDisposed)
            {
                return;
            }

            try
            {
                dropDown.Dispose();
            }
            catch (ObjectDisposedException)
            {
                // The owner can be torn down while a menu closes.
            }
        };

        dropDown.Show(owner, adjustedLocation);
        dropDown.BeginInvoke((MethodInvoker)(() => dropDown._content.FocusFirstEnabledItem()));
        return dropDown;
    }

    private static Point ResolveOwnerLocation(
        Control owner,
        Point requestedLocation,
        Size menuSize,
        ThemedDropDownMenuOptions options)
    {
        Rectangle workingArea = Screen.FromControl(owner).WorkingArea;
        Point screenPoint = owner.PointToScreen(requestedLocation);
        int gap = DesignTokens.Scale(4);

        if (screenPoint.X + menuSize.Width > workingArea.Right)
        {
            screenPoint.X = Math.Max(
                workingArea.Left + gap,
                workingArea.Right - menuSize.Width - gap);
        }

        if (screenPoint.X < workingArea.Left)
        {
            screenPoint.X = workingArea.Left + gap;
        }

        if (screenPoint.Y + menuSize.Height > workingArea.Bottom)
        {
            if (options.PreferAboveWhenOverflow)
            {
                Point ownerTopLeft = owner.PointToScreen(Point.Empty);
                screenPoint.Y = ownerTopLeft.Y - menuSize.Height - gap;
            }

            if (screenPoint.Y < workingArea.Top)
            {
                screenPoint.Y = Math.Max(
                    workingArea.Top + gap,
                    workingArea.Bottom - menuSize.Height - gap);
            }
        }

        if (screenPoint.Y < workingArea.Top)
        {
            screenPoint.Y = workingArea.Top + gap;
        }

        return owner.PointToClient(screenPoint);
    }

    private sealed class ThemedDropDownMenuContent : Control
    {
        private readonly IReadOnlyList<ThemedDropDownMenuItem> _items;
        private readonly ThemedDropDownMenuOptions _options;
        private readonly int[] _itemTopPositions;
        private int _hoveredIndex = -1;
        private int _keyboardIndex = -1;

        public ThemedDropDownMenuContent(
            Control owner,
            IReadOnlyList<ThemedDropDownMenuItem> items,
            ThemedDropDownMenuOptions options)
        {
            _items = items;
            _options = options;
            _itemTopPositions = new int[items.Count];

            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.Selectable |
                ControlStyles.UserPaint,
                true);

            TabStop = true;
            Cursor = Cursors.Hand;
            BackColor = DesignTokens.Surface;
            ForeColor = DesignTokens.TextPrimary;
            Font = DesignTokens.FontUiNormal;
            Size = MeasureMenuSize();
            UpdateRegion();
        }

        public event EventHandler<ThemedDropDownMenuItem>? ItemInvoked;
        public event EventHandler? CloseRequested;

        public void FocusFirstEnabledItem()
        {
            _keyboardIndex = FindNextEnabledIndex(-1, 1);
            Focus();
            Invalidate();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            UpdateRegion();
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            // Painted in OnPaint for flicker-free rounded edges.
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.Clear(Color.Transparent);

            Rectangle bounds = Rectangle.Inflate(ClientRectangle, -1, -1);
            if (bounds.Width <= 0 || bounds.Height <= 0)
            {
                return;
            }

            using GraphicsPath backgroundPath = CreateRoundPath(bounds, DesignTokens.Scale(7));
            using var backgroundBrush = new SolidBrush(DesignTokens.Surface);
            using var borderPen = new Pen(DesignTokens.BorderSoft, Math.Max(1f, DesignTokens.DensityScale));
            e.Graphics.FillPath(backgroundBrush, backgroundPath);
            e.Graphics.DrawPath(borderPen, backgroundPath);

            for (int index = 0; index < _items.Count; index++)
            {
                DrawItem(e.Graphics, index);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            int nextHoveredIndex = GetItemIndexAt(e.Location);
            if (_hoveredIndex != nextHoveredIndex)
            {
                _hoveredIndex = nextHoveredIndex;
                if (nextHoveredIndex >= 0)
                {
                    _keyboardIndex = nextHoveredIndex;
                }

                Invalidate();
            }

            base.OnMouseMove(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            _hoveredIndex = -1;
            Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                InvokeItem(GetItemIndexAt(e.Location));
                return;
            }

            base.OnMouseUp(e);
        }

        protected override bool IsInputKey(Keys keyData)
        {
            Keys keyCode = keyData & Keys.KeyCode;
            return keyCode is Keys.Up or Keys.Down or Keys.Enter or Keys.Space or Keys.Escape
                || base.IsInputKey(keyData);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Up:
                    _keyboardIndex = FindNextEnabledIndex(_keyboardIndex, -1);
                    _hoveredIndex = -1;
                    Invalidate();
                    e.Handled = true;
                    return;

                case Keys.Down:
                    _keyboardIndex = FindNextEnabledIndex(_keyboardIndex, 1);
                    _hoveredIndex = -1;
                    Invalidate();
                    e.Handled = true;
                    return;

                case Keys.Enter:
                case Keys.Space:
                    InvokeItem(_keyboardIndex);
                    e.Handled = true;
                    return;

                case Keys.Escape:
                    CloseRequested?.Invoke(this, EventArgs.Empty);
                    e.Handled = true;
                    return;
            }

            base.OnKeyDown(e);
        }

        private Size MeasureMenuSize()
        {
            bool reserveCheckColumn = _options.ReserveCheckColumn || _items.Any(item => item.Checked);
            int checkColumnWidth = reserveCheckColumn ? DesignTokens.Scale(32) : 0;
            int textPadding = _options.HorizontalPadding + checkColumnWidth + DesignTokens.Scale(16);
            int maxTextWidth = 0;
            int height = _options.VerticalPadding * 2;

            for (int index = 0; index < _items.Count; index++)
            {
                ThemedDropDownMenuItem item = _items[index];
                _itemTopPositions[index] = height;

                if (item.IsSeparator)
                {
                    height += _options.SeparatorHeight;
                    continue;
                }

                Size textSize = TextRenderer.MeasureText(
                    item.Text,
                    DesignTokens.FontUiNormal,
                    new Size(int.MaxValue, int.MaxValue),
                    TextFormatFlags.NoPrefix |
                    TextFormatFlags.SingleLine);
                maxTextWidth = Math.Max(maxTextWidth, textSize.Width);
                height += _options.ItemHeight;
            }

            int measuredWidth = maxTextWidth + textPadding + _options.HorizontalPadding + DesignTokens.Scale(4);
            int width = _options.FixedWidth
                ?? Math.Clamp(measuredWidth, _options.MinimumWidth, _options.MaximumWidth);

            return new Size(Math.Max(1, width), Math.Max(1, height));
        }

        private void DrawItem(Graphics graphics, int index)
        {
            ThemedDropDownMenuItem item = _items[index];
            int top = _itemTopPositions[index];

            if (item.IsSeparator)
            {
                int y = top + (_options.SeparatorHeight / 2);
                using var separatorPen = new Pen(DesignTokens.BorderSoft);
                graphics.DrawLine(
                    separatorPen,
                    DesignTokens.Scale(16),
                    y,
                    Width - DesignTokens.Scale(16),
                    y);
                return;
            }

            Rectangle rowBounds = new(
                DesignTokens.Scale(1),
                top,
                Math.Max(0, Width - DesignTokens.Scale(2)),
                _options.ItemHeight);

            bool isHighlighted = item.Enabled && (index == _hoveredIndex || (Focused && index == _keyboardIndex));
            if (isHighlighted)
            {
                Rectangle highlightBounds = Rectangle.Inflate(
                    rowBounds,
                    -DesignTokens.Scale(8),
                    -DesignTokens.Scale(5));
                using GraphicsPath highlightPath = CreateRoundPath(highlightBounds, DesignTokens.Scale(7));
                using var highlightBrush = new SolidBrush(DesignTokens.SurfaceHover);
                using var highlightBorderPen = new Pen(Color.FromArgb(120, DesignTokens.Accent));
                graphics.FillPath(highlightBrush, highlightPath);
                graphics.DrawPath(highlightBorderPen, highlightPath);
            }

            bool reserveCheckColumn = _options.ReserveCheckColumn || _items.Any(menuItem => menuItem.Checked);
            int textLeft = rowBounds.Left + _options.HorizontalPadding + DesignTokens.Scale(8);

            if (reserveCheckColumn)
            {
                int checkColumnWidth = DesignTokens.Scale(32);
                if (item.Checked)
                {
                    int dotSize = DesignTokens.Scale(7);
                    Rectangle dotBounds = new(
                        rowBounds.Left + _options.HorizontalPadding + ((checkColumnWidth - dotSize) / 2),
                        rowBounds.Top + ((rowBounds.Height - dotSize) / 2),
                        dotSize,
                        dotSize);
                    using var dotBrush = new SolidBrush(DesignTokens.Accent);
                    graphics.FillEllipse(dotBrush, dotBounds);
                }

                textLeft += checkColumnWidth;
            }

            Rectangle textBounds = new(
                textLeft,
                rowBounds.Top,
                Math.Max(0, rowBounds.Right - textLeft - _options.HorizontalPadding - DesignTokens.Scale(8)),
                rowBounds.Height);

            TextRenderer.DrawText(
                graphics,
                item.Text,
                DesignTokens.FontUiNormal,
                textBounds,
                item.Enabled ? DesignTokens.TextPrimary : DesignTokens.TextMuted,
                TextFormatFlags.Left |
                TextFormatFlags.VerticalCenter |
                TextFormatFlags.EndEllipsis |
                TextFormatFlags.SingleLine |
                TextFormatFlags.NoPrefix);
        }

        private int GetItemIndexAt(Point location)
        {
            for (int index = 0; index < _items.Count; index++)
            {
                ThemedDropDownMenuItem item = _items[index];
                if (item.IsSeparator || !item.Enabled)
                {
                    continue;
                }

                int top = _itemTopPositions[index];
                Rectangle bounds = new(0, top, Width, _options.ItemHeight);
                if (bounds.Contains(location))
                {
                    return index;
                }
            }

            return -1;
        }

        private int FindNextEnabledIndex(int currentIndex, int direction)
        {
            if (_items.Count == 0)
            {
                return -1;
            }

            int index = currentIndex;
            for (int attempt = 0; attempt < _items.Count; attempt++)
            {
                index = (index + direction + _items.Count) % _items.Count;
                ThemedDropDownMenuItem item = _items[index];
                if (!item.IsSeparator && item.Enabled)
                {
                    return index;
                }
            }

            return -1;
        }

        private void InvokeItem(int index)
        {
            if (index < 0 || index >= _items.Count)
            {
                return;
            }

            ThemedDropDownMenuItem item = _items[index];
            if (item.IsSeparator || !item.Enabled)
            {
                return;
            }

            ItemInvoked?.Invoke(this, item);
        }

        private void UpdateRegion()
        {
            if (Width <= 0 || Height <= 0)
            {
                return;
            }

            using GraphicsPath path = CreateRoundPath(
                new Rectangle(0, 0, Width, Height),
                DesignTokens.Scale(8));
            Region = new Region(path);
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
