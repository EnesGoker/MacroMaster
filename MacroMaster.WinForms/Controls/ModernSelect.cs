using MacroMaster.WinForms.Theme;
using System.Drawing.Drawing2D;

namespace MacroMaster.WinForms.Controls;

internal sealed class ModernSelect : Control
{
    private readonly List<string> _items = [];
    private bool _isHovered;
    private bool _isPressed;
    private bool _isDropDownOpen;
    private int _selectedIndex = -1;

    public ModernSelect()
    {
        SetStyle(
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.ResizeRedraw |
            ControlStyles.Selectable |
            ControlStyles.UserPaint,
            true);

        Cursor = Cursors.Hand;
        BackColor = DesignTokens.SurfaceInset;
        ForeColor = DesignTokens.TextPrimary;
        Font = DesignTokens.FontUiNormal;
        MinimumSize = new Size(DesignTokens.Scale(120), DesignTokens.Scale(30));
        TabStop = true;
    }

    public event EventHandler? SelectedIndexChanged;

    public int SelectedIndex
    {
        get => _selectedIndex;
        set => SetSelectedIndex(value);
    }

    public object? SelectedItem
    {
        get => _selectedIndex >= 0 && _selectedIndex < _items.Count
            ? _items[_selectedIndex]
            : null;
        set
        {
            string? targetValue = value?.ToString();
            int index = targetValue is null
                ? -1
                : _items.FindIndex(item => string.Equals(item, targetValue, StringComparison.Ordinal));
            SetSelectedIndex(index);
        }
    }

    public void SetItems(IEnumerable<string> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        string? selectedValue = SelectedItem?.ToString();

        _items.Clear();
        _items.AddRange(items);

        int selectedIndex = selectedValue is null
            ? -1
            : _items.FindIndex(item => string.Equals(item, selectedValue, StringComparison.Ordinal));

        if (selectedIndex < 0 && _items.Count > 0)
        {
            selectedIndex = 0;
        }

        SetSelectedIndex(selectedIndex, raiseEvent: false);
        Invalidate();
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

    protected override void OnMouseDown(MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left && Enabled)
        {
            _isPressed = true;
            Focus();
            Invalidate();
        }

        base.OnMouseDown(e);
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        _isPressed = false;
        Invalidate();

        if (e.Button == MouseButtons.Left && ClientRectangle.Contains(e.Location) && Enabled)
        {
            ShowDropDown();
        }

        base.OnMouseUp(e);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (!Enabled)
        {
            base.OnKeyDown(e);
            return;
        }

        if (e.KeyCode is Keys.Space or Keys.Enter)
        {
            ShowDropDown();
            e.Handled = true;
            return;
        }

        if (e.KeyCode == Keys.Up)
        {
            SetSelectedIndex(Math.Max(0, _selectedIndex - 1));
            e.Handled = true;
            return;
        }

        if (e.KeyCode == Keys.Down)
        {
            SetSelectedIndex(Math.Min(_items.Count - 1, _selectedIndex + 1));
            e.Handled = true;
            return;
        }

        base.OnKeyDown(e);
    }

    protected override bool IsInputKey(Keys keyData)
    {
        Keys keyCode = keyData & Keys.KeyCode;
        return keyCode is Keys.Up or Keys.Down or Keys.Space or Keys.Enter || base.IsInputKey(keyData);
    }

    protected override void OnEnabledChanged(EventArgs e)
    {
        Cursor = Enabled ? Cursors.Hand : Cursors.Default;
        Invalidate();
        base.OnEnabledChanged(e);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics graphics = e.Graphics;
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        graphics.Clear(Parent?.BackColor ?? DesignTokens.Surface);

        Rectangle bounds = Rectangle.Inflate(ClientRectangle, -1, -1);
        using GraphicsPath path = CreateRoundPath(bounds, DesignTokens.Scale(7));
        using var fillBrush = new SolidBrush(ResolveFillColor());
        using var borderPen = new Pen(ResolveBorderColor(), Math.Max(1f, DesignTokens.DensityScale));
        graphics.FillPath(fillBrush, path);
        graphics.DrawPath(borderPen, path);

        Rectangle textBounds = new(
            bounds.Left + DesignTokens.Scale(11),
            bounds.Top,
            Math.Max(0, bounds.Width - DesignTokens.Scale(42)),
            bounds.Height);

        TextRenderer.DrawText(
            graphics,
            SelectedItem?.ToString() ?? string.Empty,
            Font,
            textBounds,
            Enabled ? ForeColor : DesignTokens.TextMuted,
            TextFormatFlags.Left |
            TextFormatFlags.VerticalCenter |
            TextFormatFlags.EndEllipsis |
            TextFormatFlags.NoPrefix);

        DrawArrow(graphics, bounds);
    }

    private void ShowDropDown()
    {
        if (_items.Count == 0 || _isDropDownOpen)
        {
            return;
        }

        var menu = new ContextMenuStrip
        {
            ShowCheckMargin = false,
            ShowImageMargin = false,
            MinimumSize = new Size(Width, 0)
        };
        AppToolStripRenderer.ApplyTo(menu);

        for (int index = 0; index < _items.Count; index++)
        {
            var item = new ToolStripMenuItem(_items[index])
            {
                Checked = index == _selectedIndex,
                Tag = index
            };
            item.Click += (_, _) =>
            {
                if (item.Tag is int itemIndex)
                {
                    SetSelectedIndex(itemIndex);
                }
            };
            menu.Items.Add(item);
        }

        _isDropDownOpen = true;
        Invalidate();
        menu.Closed += (_, _) =>
        {
            _isDropDownOpen = false;
            _isPressed = false;
            Invalidate();
            menu.Dispose();
        };

        menu.Show(this, new Point(0, Height + DesignTokens.Scale(4)));
    }

    private void SetSelectedIndex(int value, bool raiseEvent = true)
    {
        int normalizedValue = _items.Count == 0
            ? -1
            : Math.Clamp(value, 0, _items.Count - 1);

        if (_selectedIndex == normalizedValue)
        {
            return;
        }

        _selectedIndex = normalizedValue;
        Invalidate();

        if (raiseEvent)
        {
            SelectedIndexChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private Color ResolveFillColor()
    {
        if (!Enabled)
        {
            return Color.FromArgb(110, DesignTokens.SurfaceInset);
        }

        if (_isPressed || _isDropDownOpen)
        {
            return DesignTokens.Surface3;
        }

        return _isHovered
            ? DesignTokens.SurfaceHover
            : DesignTokens.SurfaceInset;
    }

    private Color ResolveBorderColor()
    {
        if (!Enabled)
        {
            return Color.FromArgb(90, DesignTokens.BorderSoft);
        }

        if (Focused || _isDropDownOpen)
        {
            return DesignTokens.Accent;
        }

        return _isHovered
            ? DesignTokens.BorderBright
            : DesignTokens.Border;
    }

    private static void DrawArrow(Graphics graphics, Rectangle bounds)
    {
        int arrowSize = DesignTokens.Scale(5);
        int centerX = bounds.Right - DesignTokens.Scale(18);
        int centerY = bounds.Top + bounds.Height / 2 + DesignTokens.Scale(1);

        Point[] points =
        [
            new(centerX - arrowSize, centerY - arrowSize / 2),
            new(centerX + arrowSize, centerY - arrowSize / 2),
            new(centerX, centerY + arrowSize / 2)
        ];

        using var arrowBrush = new SolidBrush(DesignTokens.TextSecondary);
        graphics.FillPolygon(arrowBrush, points);
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
