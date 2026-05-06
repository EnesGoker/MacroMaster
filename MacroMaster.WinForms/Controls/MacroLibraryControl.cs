using MacroMaster.Application.Abstractions;
using MacroMaster.WinForms.Theme;
using System.Drawing.Drawing2D;
using System.Globalization;

namespace MacroMaster.WinForms.Controls;

internal sealed class MacroLibraryControl : UserControl
{
    private static readonly MacroLibraryFilterOption[] FilterOptions =
    [
        new(MacroLibraryFilterKind.All, "Tum makrolar"),
        new(MacroLibraryFilterKind.Favorites, "Favoriler"),
        new(MacroLibraryFilterKind.Recent, "Son kullanilanlar"),
        new(MacroLibraryFilterKind.Json, "JSON dosyalari"),
        new(MacroLibraryFilterKind.Xml, "XML dosyalari"),
        new(MacroLibraryFilterKind.Short, "Kisa makrolar"),
        new(MacroLibraryFilterKind.Long, "Uzun makrolar")
    ];

    private readonly Panel _listViewportPanel;
    private readonly FlowLayoutPanel _macroListPanel;
    private readonly ModernScrollBar _listScrollBar;
    private readonly Label _emptyStateLabel;
    private readonly Label _totalMacroValueLabel;
    private readonly Label _totalEventValueLabel;
    private readonly TextBox _searchTextBox;
    private readonly FilterIconButton _filterButton;
    private IReadOnlyList<MacroLibraryViewItem> _items = [];
    private MacroLibraryFilterKind _selectedFilterKind = MacroLibraryFilterKind.All;
    private string? _selectedFilePath;

    public event EventHandler? AddRequested;
    public event EventHandler<MacroLibraryItemEventArgs>? LoadRequested;
    public event EventHandler<MacroLibraryItemEventArgs>? RenameRequested;
    public event EventHandler<MacroLibraryItemEventArgs>? DeleteRequested;
    public event EventHandler<MacroLibraryItemEventArgs>? FavoriteToggled;

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

        _listViewportPanel = new Panel();
        _macroListPanel = new FlowLayoutPanel();
        _listScrollBar = new ModernScrollBar();
        _emptyStateLabel = CreateEmptyStateLabel();
        _totalMacroValueLabel = CreateFooterValueLabel();
        _totalEventValueLabel = CreateFooterValueLabel();
        _searchTextBox = CreateSearchTextBox();
        _filterButton = new FilterIconButton();

        BuildLayout();
        SetItems([], null);
    }

    public void SetItems(
        IReadOnlyList<MacroLibraryViewItem> items,
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
        MacroLibraryViewItem[] filteredItems =
            MacroLibraryFilterEngine.Apply(_items, searchTerm, _selectedFilterKind);

        _macroListPanel.SuspendLayout();
        _macroListPanel.Controls.Clear();

        if (filteredItems.Length == 0)
        {
            _emptyStateLabel.Text = _items.Count == 0
                ? "Kayitli makro yok. Kaydet butonu ile kutuphaneye ekleyebilirsin."
                : "Filtreyle eslesen makro bulunamadi.";
            _macroListPanel.Controls.Add(_emptyStateLabel);
        }
        else
        {
            foreach (MacroLibraryViewItem item in filteredItems)
            {
                bool isSelected = IsSamePath(item.Entry.FilePath, _selectedFilePath);
                var row = new MacroLibraryRow(item, isSelected);
                row.Activated += (_, _) => LoadRequested?.Invoke(this, new MacroLibraryItemEventArgs(item));
                row.RenameRequested += (_, _) => RenameRequested?.Invoke(this, new MacroLibraryItemEventArgs(item));
                row.DeleteRequested += (_, _) => DeleteRequested?.Invoke(this, new MacroLibraryItemEventArgs(item));
                row.FavoriteToggled += (_, _) => FavoriteToggled?.Invoke(this, new MacroLibraryItemEventArgs(item));
                WireMouseWheelForwarding(row);
                _macroListPanel.Controls.Add(row);
            }
        }

        int totalEventCount = filteredItems.Sum(item => Math.Max(0, item.Entry.EventCount));
        _totalMacroValueLabel.Text = filteredItems.Length.ToString(CultureInfo.InvariantCulture);
        _totalEventValueLabel.Text = totalEventCount.ToString("N0", CultureInfo.GetCultureInfo("tr-TR"));
        ResizeLibraryRows();
        UpdateListScrollLayout();
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
            ColumnCount = 3,
            RowCount = 1,
            BackColor = DesignTokens.Surface,
            Margin = new Padding(0, 0, DesignTokens.Scale(12), 0),
            Padding = Padding.Empty
        };
        headerLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        headerLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, DesignTokens.Scale(38)));
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

        _filterButton.Dock = DockStyle.Fill;
        _filterButton.Margin = new Padding(DesignTokens.Scale(8), DesignTokens.Scale(3), 0, DesignTokens.Scale(7));
        _filterButton.Click += (_, _) => ShowFilterMenu();

        headerLayoutPanel.Controls.Add(titleLabel, 0, 0);
        headerLayoutPanel.Controls.Add(_filterButton, 1, 0);
        headerLayoutPanel.Controls.Add(addButton, 2, 0);

        var searchPanel = new RoundedPanel
        {
            Dock = DockStyle.Fill,
            BackColor = DesignTokens.SurfaceInset,
            BorderColor = DesignTokens.BorderSoft,
            Margin = new Padding(
                0,
                DesignTokens.Scale(3),
                DesignTokens.Scale(12),
                DesignTokens.Scale(8)),
            Padding = new Padding(
                DesignTokens.Scale(14),
                DesignTokens.Scale(5),
                DesignTokens.Scale(10),
                DesignTokens.Scale(4))
        };
        searchPanel.Click += (_, _) => _searchTextBox.Focus();
        searchPanel.Controls.Add(_searchTextBox);

        var listHostPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = DesignTokens.Surface,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        listHostPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        listHostPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, DesignTokens.Scale(20)));

        _listViewportPanel.Dock = DockStyle.Fill;
        _listViewportPanel.BackColor = DesignTokens.Surface;
        _listViewportPanel.Margin = Padding.Empty;
        _listViewportPanel.Padding = Padding.Empty;
        _listViewportPanel.Resize += (_, _) => UpdateListScrollLayout();
        _listViewportPanel.MouseWheel += ListViewportPanel_MouseWheel;

        _macroListPanel.Dock = DockStyle.None;
        _macroListPanel.FlowDirection = FlowDirection.TopDown;
        _macroListPanel.WrapContents = false;
        _macroListPanel.AutoScroll = false;
        _macroListPanel.BackColor = DesignTokens.Surface;
        _macroListPanel.Margin = Padding.Empty;
        _macroListPanel.Padding = new Padding(0, 2, 0, 0);
        _macroListPanel.Resize += (_, _) => ResizeLibraryRows();
        _macroListPanel.MouseWheel += ListViewportPanel_MouseWheel;

        _listScrollBar.Dock = DockStyle.Fill;
        _listScrollBar.Margin = new Padding(DesignTokens.Scale(4), 0, 0, 0);
        _listScrollBar.ValueChanged += (_, _) => ScrollListTo(_listScrollBar.Value);

        _listViewportPanel.Controls.Add(_macroListPanel);
        listHostPanel.Controls.Add(_listViewportPanel, 0, 0);
        listHostPanel.Controls.Add(_listScrollBar, 1, 0);

        rootLayoutPanel.Controls.Add(headerLayoutPanel, 0, 0);
        rootLayoutPanel.Controls.Add(searchPanel, 0, 1);
        rootLayoutPanel.Controls.Add(listHostPanel, 0, 2);
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
            Margin = new Padding(0, DesignTokens.Scale(8), DesignTokens.Scale(12), 0),
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
            _listViewportPanel.ClientSize.Width - DesignTokens.Scale(4));

        foreach (Control control in _macroListPanel.Controls)
        {
            control.Width = rowWidth;
        }
    }

    private void UpdateListScrollLayout()
    {
        if (_listViewportPanel.ClientSize.Width <= 0 || _listViewportPanel.ClientSize.Height <= 0)
        {
            return;
        }

        ResizeLibraryRows();

        int contentHeight = CalculateListContentHeight();
        int viewportHeight = _listViewportPanel.ClientSize.Height;
        int maximum = Math.Max(0, contentHeight - viewportHeight);
        int nextValue = Math.Min(_listScrollBar.Value, maximum);

        _listScrollBar.SetRange(maximum, viewportHeight, nextValue);
        _listScrollBar.Visible = maximum > 0;

        _macroListPanel.Bounds = new Rectangle(
            0,
            -nextValue,
            Math.Max(0, _listViewportPanel.ClientSize.Width - DesignTokens.Scale(4)),
            Math.Max(viewportHeight, contentHeight));
    }

    private int CalculateListContentHeight()
    {
        int contentHeight = _macroListPanel.Padding.Vertical;

        foreach (Control control in _macroListPanel.Controls)
        {
            contentHeight += control.Height + control.Margin.Vertical;
        }

        return contentHeight;
    }

    private void ScrollListTo(int value)
    {
        _macroListPanel.Top = -Math.Max(0, value);
    }

    private void ListViewportPanel_MouseWheel(object? sender, MouseEventArgs e)
    {
        _ = sender;

        int wheelStep = Math.Max(DesignTokens.Scale(48), SystemInformation.MouseWheelScrollLines * DesignTokens.Scale(18));
        _listScrollBar.Value -= Math.Sign(e.Delta) * wheelStep;
    }

    private void WireMouseWheelForwarding(Control control)
    {
        control.MouseWheel += ListViewportPanel_MouseWheel;

        foreach (Control child in control.Controls)
        {
            WireMouseWheelForwarding(child);
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

    private void ShowFilterMenu()
    {
        var menu = new ContextMenuStrip
        {
            ShowCheckMargin = true,
            ShowImageMargin = false,
            MinimumSize = new Size(DesignTokens.Scale(184), 0)
        };
        AppToolStripRenderer.ApplyTo(menu, AppToolStripMenuDensity.Comfortable);

        foreach (MacroLibraryFilterOption option in FilterOptions)
        {
            var menuItem = new ToolStripMenuItem(option.Label)
            {
                Checked = option.Kind == _selectedFilterKind,
                Tag = option.Kind
            };
            menuItem.Click += (_, _) =>
            {
                if (menuItem.Tag is MacroLibraryFilterKind filterKind)
                {
                    SetFilter(filterKind);
                }
            };
            menu.Items.Add(menuItem);
        }

        menu.Closed += (_, _) => DisposeMenuAfterCurrentMessage(menu);
        menu.Show(_filterButton, new Point(0, _filterButton.Height + DesignTokens.Scale(4)));
    }

    private void SetFilter(MacroLibraryFilterKind filterKind)
    {
        if (_selectedFilterKind == filterKind)
        {
            return;
        }

        _selectedFilterKind = filterKind;
        _filterButton.IsActive = filterKind != MacroLibraryFilterKind.All;
        ApplyFilter();
    }

    private void DisposeMenuAfterCurrentMessage(ContextMenuStrip menu)
    {
        if (menu.IsDisposed)
        {
            return;
        }

        if (IsDisposed || Disposing || !IsHandleCreated)
        {
            menu.Dispose();
            return;
        }

        try
        {
            BeginInvoke((MethodInvoker)(() =>
            {
                if (!menu.IsDisposed)
                {
                    menu.Dispose();
                }
            }));
        }
        catch (ObjectDisposedException)
        {
            // The owner can be torn down while the drop-down is closing.
        }
        catch (InvalidOperationException)
        {
            if (!menu.IsDisposed)
            {
                menu.Dispose();
            }
        }
    }

    private sealed class FilterIconButton : Control
    {
        private bool _isHovered;
        private bool _isPressed;
        private bool _isActive;

        public FilterIconButton()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.Selectable |
                ControlStyles.UserPaint,
                true);

            Cursor = Cursors.Hand;
            TabStop = true;
            AccessibleName = "Makro filtresi";
            AccessibleRole = AccessibleRole.PushButton;
        }

        public bool IsActive
        {
            get => _isActive;
            set
            {
                if (_isActive == value)
                {
                    return;
                }

                _isActive = value;
                Invalidate();
            }
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
            base.OnMouseUp(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.Clear(Parent?.BackColor ?? DesignTokens.Surface);

            Rectangle bounds = Rectangle.Inflate(ClientRectangle, -1, -1);

            if (bounds.Width <= 0 || bounds.Height <= 0)
            {
                return;
            }

            using GraphicsPath backgroundPath = CreateRoundPath(bounds, DesignTokens.Scale(4));
            using var fillBrush = new SolidBrush(ResolveFillColor());
            using var borderPen = new Pen(ResolveBorderColor());
            e.Graphics.FillPath(fillBrush, backgroundPath);
            e.Graphics.DrawPath(borderPen, backgroundPath);

            DrawFilterGlyph(e.Graphics, bounds);
        }

        protected override bool IsInputKey(Keys keyData)
        {
            Keys keyCode = keyData & Keys.KeyCode;
            return keyCode is Keys.Space or Keys.Enter || base.IsInputKey(keyData);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (Enabled && e.KeyCode is Keys.Space or Keys.Enter)
            {
                OnClick(EventArgs.Empty);
                e.Handled = true;
                return;
            }

            base.OnKeyDown(e);
        }

        private Color ResolveFillColor()
        {
            if (_isPressed)
            {
                return DesignTokens.Surface3;
            }

            if (_isActive)
            {
                return Color.FromArgb(30, DesignTokens.Accent);
            }

            return _isHovered
                ? DesignTokens.SurfaceHover
                : DesignTokens.Surface2;
        }

        private Color ResolveBorderColor()
        {
            if (_isActive || Focused)
            {
                return DesignTokens.Accent;
            }

            return _isHovered
                ? DesignTokens.BorderBright
                : DesignTokens.BorderSoft;
        }

        private void DrawFilterGlyph(Graphics graphics, Rectangle bounds)
        {
            int iconWidth = DesignTokens.Scale(14);
            int iconHeight = DesignTokens.Scale(13);
            int left = bounds.Left + (bounds.Width - iconWidth) / 2;
            int top = bounds.Top + (bounds.Height - iconHeight) / 2;
            int right = left + iconWidth;
            int bottom = top + iconHeight;
            int centerX = left + iconWidth / 2;
            int neckY = top + DesignTokens.Scale(6);
            int stemBottom = bottom - DesignTokens.Scale(1);

            using var path = new GraphicsPath();
            path.AddLine(left, top, right, top);
            path.AddLine(right, top, centerX + DesignTokens.Scale(3), neckY);
            path.AddLine(centerX + DesignTokens.Scale(3), neckY, centerX + DesignTokens.Scale(1), neckY);
            path.AddLine(centerX + DesignTokens.Scale(1), neckY, centerX + DesignTokens.Scale(1), stemBottom);
            path.AddLine(centerX + DesignTokens.Scale(1), stemBottom, centerX - DesignTokens.Scale(1), stemBottom);
            path.AddLine(centerX - DesignTokens.Scale(1), stemBottom, centerX - DesignTokens.Scale(1), neckY);
            path.AddLine(centerX - DesignTokens.Scale(1), neckY, centerX - DesignTokens.Scale(3), neckY);
            path.CloseFigure();

            using var iconBrush = new SolidBrush(_isActive ? DesignTokens.Accent : DesignTokens.TextSecondary);
            graphics.FillPath(iconBrush, path);
        }
    }

    private sealed class MacroLibraryRow : RoundedPanel
    {
        private readonly MacroLibraryViewItem _item;
        private readonly bool _isSelected;

        public event EventHandler? Activated;
        public event EventHandler? RenameRequested;
        public event EventHandler? DeleteRequested;
        public event EventHandler? FavoriteToggled;

        public MacroLibraryRow(MacroLibraryViewItem item, bool isSelected)
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
                    Parent.ClientSize.Width - DesignTokens.Scale(4));
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
                    Text = _item.Entry.Name,
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
                    Text = FormatLastModified(_item.Entry.LastModifiedUtc),
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
                ShowImageMargin = false
            };
            AppToolStripRenderer.ApplyTo(
                contextMenu,
                AppToolStripMenuDensity.Comfortable);

            var favoriteItem = new ToolStripMenuItem(_item.IsFavorite ? "Favoriden Cikar" : "Favoriye Ekle");
            favoriteItem.Click += (_, _) => FavoriteToggled?.Invoke(this, EventArgs.Empty);

            var renameItem = new ToolStripMenuItem("Isim Duzenle");
            renameItem.Click += (_, _) => RenameRequested?.Invoke(this, EventArgs.Empty);

            var deleteItem = new ToolStripMenuItem("Sil");
            deleteItem.Click += (_, _) => DeleteRequested?.Invoke(this, EventArgs.Empty);
            contextMenu.Items.Add(favoriteItem);
            contextMenu.Items.Add(new ToolStripSeparator());
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

}

internal sealed class MacroLibraryItemEventArgs : EventArgs
{
    public MacroLibraryItemEventArgs(MacroLibraryViewItem viewItem)
    {
        ViewItem = viewItem;
        Item = viewItem.Entry;
    }

    public MacroLibraryEntry Item { get; }

    public MacroLibraryViewItem ViewItem { get; }
}
