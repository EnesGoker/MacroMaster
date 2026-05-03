using MacroMaster.Application.Abstractions;
using MacroMaster.WinForms.Theme;
using System.Drawing.Drawing2D;
using System.Globalization;

namespace MacroMaster.WinForms.Controls;

internal sealed class MacroLibraryControl : UserControl
{
    private readonly FlowLayoutPanel _macroListPanel;
    private readonly Label _emptyStateLabel;
    private readonly Label _totalMacroValueLabel;
    private readonly Label _totalEventValueLabel;
    private readonly TextBox _searchTextBox;
    private IReadOnlyList<MacroLibraryEntry> _items = [];
    private string? _selectedFilePath;

    public event EventHandler? AddRequested;
    public event EventHandler<MacroLibraryItemEventArgs>? LoadRequested;
    public event EventHandler<MacroLibraryItemEventArgs>? RenameRequested;
    public event EventHandler<MacroLibraryItemEventArgs>? DeleteRequested;

    public MacroLibraryControl()
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

        _macroListPanel = new FlowLayoutPanel();
        _emptyStateLabel = CreateEmptyStateLabel();
        _totalMacroValueLabel = CreateFooterValueLabel();
        _totalEventValueLabel = CreateFooterValueLabel();
        _searchTextBox = CreateSearchTextBox();

        BuildLayout();
        SetItems([], null);
    }

    public void SetItems(
        IReadOnlyList<MacroLibraryEntry> items,
        string? selectedFilePath)
    {
        ArgumentNullException.ThrowIfNull(items);

        _items = items;
        _selectedFilePath = selectedFilePath;
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        string searchTerm = _searchTextBox.Text.Trim();
        IReadOnlyList<MacroLibraryEntry> filteredItems = string.IsNullOrWhiteSpace(searchTerm)
            ? _items
            : _items
                .Where(item => item.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                .ToList();

        _macroListPanel.SuspendLayout();
        _macroListPanel.Controls.Clear();

        if (filteredItems.Count == 0)
        {
            _emptyStateLabel.Text = _items.Count == 0
                ? "Kayitli makro yok. Kaydet butonu ile kutuphaneye ekleyebilirsin."
                : "Arama ile eslesen makro bulunamadi.";
            _macroListPanel.Controls.Add(_emptyStateLabel);
        }
        else
        {
            foreach (MacroLibraryEntry item in filteredItems)
            {
                bool isSelected = IsSamePath(item.FilePath, _selectedFilePath);
                var row = new MacroLibraryRow(item, isSelected);
                row.Activated += (_, _) => LoadRequested?.Invoke(this, new MacroLibraryItemEventArgs(item));
                row.RenameRequested += (_, _) => RenameRequested?.Invoke(this, new MacroLibraryItemEventArgs(item));
                row.DeleteRequested += (_, _) => DeleteRequested?.Invoke(this, new MacroLibraryItemEventArgs(item));
                _macroListPanel.Controls.Add(row);
            }
        }

        int totalEventCount = filteredItems.Sum(item => Math.Max(0, item.EventCount));
        _totalMacroValueLabel.Text = filteredItems.Count.ToString(CultureInfo.InvariantCulture);
        _totalEventValueLabel.Text = totalEventCount.ToString("N0", CultureInfo.GetCultureInfo("tr-TR"));
        ResizeLibraryRows();
        _macroListPanel.ResumeLayout();
    }

    private void BuildLayout()
    {
        var rootLayoutPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            BackColor = DesignTokens.Surface,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        rootLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        rootLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, DesignTokens.Scale(42)));
        rootLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, DesignTokens.Scale(44)));
        rootLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
        rootLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, DesignTokens.Scale(50)));

        var headerLayoutPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = DesignTokens.Surface,
            Margin = new Padding(0, 0, GetScrollbarReserveWidth(), 0),
            Padding = Padding.Empty
        };
        headerLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        headerLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, DesignTokens.Scale(38)));

        var titleLabel = new Label
        {
            Dock = DockStyle.Fill,
            Text = "Makro Kutuphanesi",
            Font = DesignTokens.FontUiBold,
            ForeColor = DesignTokens.TextPrimary,
            BackColor = DesignTokens.Surface,
            TextAlign = ContentAlignment.MiddleLeft,
            AutoEllipsis = true
        };

        var addButton = new Button
        {
            Dock = DockStyle.Fill,
            Text = "+",
            BackColor = DesignTokens.Surface2,
            ForeColor = DesignTokens.TextPrimary,
            FlatStyle = FlatStyle.Flat,
            Font = DesignTokens.FontUiBold,
            Margin = new Padding(DesignTokens.Scale(8), DesignTokens.Scale(3), 0, DesignTokens.Scale(7)),
            UseVisualStyleBackColor = false
        };
        addButton.FlatAppearance.BorderColor = DesignTokens.BorderSoft;
        addButton.FlatAppearance.BorderSize = 1;
        addButton.Click += (_, _) => AddRequested?.Invoke(this, EventArgs.Empty);

        headerLayoutPanel.Controls.Add(titleLabel, 0, 0);
        headerLayoutPanel.Controls.Add(addButton, 1, 0);

        var searchPanel = new RoundedPanel
        {
            Dock = DockStyle.Fill,
            BackColor = DesignTokens.SurfaceInset,
            BorderColor = DesignTokens.BorderSoft,
            Margin = new Padding(
                0,
                DesignTokens.Scale(3),
                GetScrollbarReserveWidth(),
                DesignTokens.Scale(8)),
            Padding = new Padding(DesignTokens.Scale(14), 0, DesignTokens.Scale(10), 0)
        };
        searchPanel.Click += (_, _) => _searchTextBox.Focus();
        searchPanel.Controls.Add(_searchTextBox);

        _macroListPanel.Dock = DockStyle.Fill;
        _macroListPanel.FlowDirection = FlowDirection.TopDown;
        _macroListPanel.WrapContents = false;
        _macroListPanel.AutoScroll = true;
        _macroListPanel.BackColor = DesignTokens.Surface;
        _macroListPanel.Margin = Padding.Empty;
        _macroListPanel.Padding = new Padding(0, 2, 0, 0);
        _macroListPanel.Resize += (_, _) => ResizeLibraryRows();

        rootLayoutPanel.Controls.Add(headerLayoutPanel, 0, 0);
        rootLayoutPanel.Controls.Add(searchPanel, 0, 1);
        rootLayoutPanel.Controls.Add(_macroListPanel, 0, 2);
        rootLayoutPanel.Controls.Add(CreateFooterPanel(), 0, 3);

        Controls.Add(rootLayoutPanel);
    }

    private RoundedPanel CreateFooterPanel()
    {
        var footerPanel = new RoundedPanel
        {
            Dock = DockStyle.Fill,
            BackColor = DesignTokens.SurfaceInset,
            BorderColor = DesignTokens.BorderSoft,
            Margin = new Padding(0, DesignTokens.Scale(8), GetScrollbarReserveWidth(), 0),
            Padding = new Padding(DesignTokens.Scale(12), DesignTokens.Scale(5), DesignTokens.Scale(12), DesignTokens.Scale(5))
        };

        var footerLayoutPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            RowCount = 1,
            BackColor = Color.Transparent,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        footerLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38f));
        footerLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 12f));
        footerLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34f));
        footerLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16f));

        footerLayoutPanel.Controls.Add(CreateFooterCaptionLabel("Toplam Makro"), 0, 0);
        footerLayoutPanel.Controls.Add(_totalMacroValueLabel, 1, 0);
        footerLayoutPanel.Controls.Add(CreateFooterCaptionLabel("Toplam Olay"), 2, 0);
        footerLayoutPanel.Controls.Add(_totalEventValueLabel, 3, 0);

        footerPanel.Controls.Add(footerLayoutPanel);
        return footerPanel;
    }

    private void ResizeLibraryRows()
    {
        int rowWidth = Math.Max(
            180,
            _macroListPanel.ClientSize.Width - GetScrollbarReserveWidth());

        foreach (Control control in _macroListPanel.Controls)
        {
            control.Width = rowWidth;
        }
    }

    private static Label CreateFooterCaptionLabel(string text)
    {
        return new Label
        {
            Dock = DockStyle.Fill,
            Text = text,
            Font = DesignTokens.FontUiNormal,
            ForeColor = DesignTokens.TextSecondary,
            BackColor = Color.Transparent,
            TextAlign = ContentAlignment.MiddleLeft,
            AutoEllipsis = true
        };
    }

    private static Label CreateFooterValueLabel()
    {
        return new Label
        {
            Dock = DockStyle.Fill,
            Font = DesignTokens.FontUiBold,
            ForeColor = DesignTokens.Accent,
            BackColor = Color.Transparent,
            TextAlign = ContentAlignment.MiddleRight,
            AutoEllipsis = true
        };
    }

    private static Label CreateEmptyStateLabel()
    {
        return new Label
        {
            Height = DesignTokens.Scale(96),
            Font = DesignTokens.FontUiNormal,
            ForeColor = DesignTokens.TextMuted,
            BackColor = Color.Transparent,
            Text = "Kayitli makro yok. Kaydet butonu ile kutuphaneye ekleyebilirsin.",
            TextAlign = ContentAlignment.MiddleCenter,
            AutoEllipsis = true
        };
    }

    private TextBox CreateSearchTextBox()
    {
        var textBox = new TextBox
        {
            BorderStyle = BorderStyle.None,
            Dock = DockStyle.Fill,
            PlaceholderText = "Makro ara...",
            BackColor = DesignTokens.SurfaceInset,
            ForeColor = DesignTokens.TextPrimary,
            Font = DesignTokens.FontUiNormal,
            Margin = Padding.Empty
        };

        textBox.TextChanged += (_, _) => ApplyFilter();
        return textBox;
    }

    private sealed class MacroLibraryRow : RoundedPanel
    {
        private readonly MacroLibraryEntry _item;
        private readonly bool _isSelected;

        public event EventHandler? Activated;
        public event EventHandler? RenameRequested;
        public event EventHandler? DeleteRequested;

        public MacroLibraryRow(MacroLibraryEntry item, bool isSelected)
        {
            _item = item;
            _isSelected = isSelected;
            Height = DesignTokens.Scale(68);
            Width = 320;
            Margin = new Padding(0, 0, 0, DesignTokens.Scale(8));
            Padding = new Padding(
                DesignTokens.Scale(14),
                DesignTokens.Scale(9),
                DesignTokens.Scale(10),
                DesignTokens.Scale(9));
            BackColor = isSelected
                ? Color.FromArgb(20, 56, 98)
                : DesignTokens.SurfaceInset;
            BorderColor = isSelected
                ? Color.FromArgb(125, DesignTokens.Accent)
                : DesignTokens.BorderSoft;

            BuildRow();
            BuildContextMenu();
            WireActivation(this);
        }

        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);
            UpdateWidthFromParent();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            UpdateWidthFromParent();
        }

        private void UpdateWidthFromParent()
        {
            if (Parent is not null)
            {
                Width = Math.Max(
                    180,
                    Parent.ClientSize.Width - GetScrollbarReserveWidth());
            }
        }

        private void BuildRow()
        {
            var layoutPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 1,
                BackColor = Color.Transparent,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            layoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));

            var textLayoutPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = Color.Transparent,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            textLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, DesignTokens.Scale(31)));
            textLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            textLayoutPanel.Controls.Add(
                new Label
                {
                    Dock = DockStyle.Fill,
                    Text = _item.Name,
                    Font = DesignTokens.FontUiBold,
                    ForeColor = DesignTokens.TextPrimary,
                    BackColor = Color.Transparent,
                    TextAlign = ContentAlignment.BottomLeft,
                    AutoEllipsis = true
                },
                0,
                0);
            textLayoutPanel.Controls.Add(
                new Label
                {
                    Dock = DockStyle.Fill,
                    Text = FormatLastModified(_item.LastModifiedUtc),
                    Font = DesignTokens.FontUiNormal,
                    ForeColor = DesignTokens.TextSecondary,
                    BackColor = Color.Transparent,
                    TextAlign = ContentAlignment.TopLeft,
                    AutoEllipsis = true
                },
                0,
                1);

            layoutPanel.Controls.Add(textLayoutPanel, 0, 0);
            Controls.Add(layoutPanel);
        }

        private void BuildContextMenu()
        {
            var contextMenu = new ContextMenuStrip
            {
                BackColor = DesignTokens.Surface2,
                ForeColor = DesignTokens.TextPrimary,
                ShowImageMargin = false
            };
            var renameItem = new ToolStripMenuItem("Isim Duzenle");
            renameItem.Click += (_, _) => RenameRequested?.Invoke(this, EventArgs.Empty);

            var deleteItem = new ToolStripMenuItem("Sil");
            deleteItem.Click += (_, _) => DeleteRequested?.Invoke(this, EventArgs.Empty);
            contextMenu.Items.Add(renameItem);
            contextMenu.Items.Add(new ToolStripSeparator());
            contextMenu.Items.Add(deleteItem);
            ContextMenuStrip = contextMenu;
        }

        private void WireActivation(Control control)
        {
            control.Click += (_, _) => Activated?.Invoke(this, EventArgs.Empty);
            control.ContextMenuStrip = ContextMenuStrip;

            foreach (Control child in control.Controls)
            {
                WireActivation(child);
            }
        }

        private static string FormatLastModified(DateTime lastModifiedUtc)
        {
            return lastModifiedUtc
                .ToLocalTime()
                .ToString("dd.MM.yyyy HH:mm", CultureInfo.GetCultureInfo("tr-TR"));
        }
    }

    private class RoundedPanel : Panel
    {
        public Color BorderColor { get; set; } = DesignTokens.Border;

        public RoundedPanel()
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

            using GraphicsPath path = CreateRoundPath(bounds, DesignTokens.Radius);
            using var fillBrush = new SolidBrush(BackColor);
            using var borderPen = new Pen(BorderColor);
            e.Graphics.FillPath(fillBrush, path);

            if (BorderColor.A > 0)
            {
                e.Graphics.DrawPath(borderPen, path);
            }
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

    private static bool IsSamePath(string left, string? right)
    {
        return !string.IsNullOrWhiteSpace(right)
            && string.Equals(
                Path.GetFullPath(left),
                Path.GetFullPath(right),
                StringComparison.OrdinalIgnoreCase);
    }

    private static int GetScrollbarReserveWidth()
    {
        return SystemInformation.VerticalScrollBarWidth + DesignTokens.Scale(10);
    }
}

internal sealed class MacroLibraryItemEventArgs : EventArgs
{
    public MacroLibraryItemEventArgs(MacroLibraryEntry item)
    {
        Item = item;
    }

    public MacroLibraryEntry Item { get; }
}
