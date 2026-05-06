using MacroMaster.Domain.Enums;
using MacroMaster.Domain.Models;
using MacroMaster.WinForms.Theme;
using System.Drawing.Drawing2D;
using System.Globalization;

namespace MacroMaster.WinForms.Controls;

internal sealed class EventListControl : UserControl
{
    private static readonly TypeFilterOption[] TypeFilterOptions =
    [
        new(EventListTypeFilterKind.All, "Tür: Tümü"),
        new(EventListTypeFilterKind.Keyboard, "Tür: Klavye"),
        new(EventListTypeFilterKind.Mouse, "Tür: Fare"),
        new(EventListTypeFilterKind.System, "Tür: Sistem")
    ];

    private static readonly SmartFilterOption[] SmartFilterOptions =
    [
        new(EventListSmartFilterKind.All, "Akıllı: Tümü"),
        new(EventListSmartFilterKind.KeyboardOnly, "Sadece klavye"),
        new(EventListSmartFilterKind.MouseMoves, "Mouse hareketleri"),
        new(EventListSmartFilterKind.MouseClicks, "Tıklamalar"),
        new(EventListSmartFilterKind.LongDelays, "Uzun beklemeler"),
        new(EventListSmartFilterKind.OptimizationCandidates, "Optimize adayları"),
        new(EventListSmartFilterKind.InvalidOrIncomplete, "Hatalı/eksik")
    ];

    private readonly DataGridView _eventGridView;
    private readonly ModernScrollBar _eventScrollBar;
    private readonly RoundedGridHostPanel _gridHostPanel;
    private readonly Label _emptyStateLabel;
    private readonly TextBox _filterSearchTextBox;
    private readonly ModernSelect _typeFilterSelect;
    private readonly ModernSelect _smartFilterSelect;
    private readonly CompactTextButton _clearFilterButton;
    private readonly Label _filterStatusLabel;
    private MacroSession? _displayedSession;
    private Guid? _displayedSessionId;
    private int _displayedEventCount;
    private int _displayedElapsedMs;
    private bool _updatingFilterControls;
    private EventListTypeFilterKind _selectedTypeFilterKind = EventListTypeFilterKind.All;
    private EventListSmartFilterKind _selectedSmartFilterKind = EventListSmartFilterKind.All;

    public event EventHandler<EventEditRequestedEventArgs>? EventEditRequested;

    public EventListControl()
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

        _eventGridView = new DataGridView();
        _eventScrollBar = new ModernScrollBar();
        _gridHostPanel = new RoundedGridHostPanel();
        _emptyStateLabel = new Label();
        _filterSearchTextBox = CreateFilterSearchTextBox();
        _typeFilterSelect = new ModernSelect();
        _smartFilterSelect = new ModernSelect();
        _clearFilterButton = new CompactTextButton();
        _filterStatusLabel = new Label();

        BuildLayout();
        ConfigureGrid();
        ApplyTheme();
        SetSession(null);
    }

    public void SetSession(MacroSession? session, bool forceReload = false)
    {
        if (session is null)
        {
            ResetRowsForSession(null);
            UpdateVisibleState();
            UpdateEventScrollBar();
            return;
        }

        _displayedSession = session;

        bool canAppend = !forceReload
            && _displayedSessionId == session.Id
            && session.Events.Count >= _displayedEventCount;

        if (!canAppend)
        {
            ResetRowsForSession(session);
        }

        AppendRows(session, _displayedEventCount);
        UpdateVisibleState();
        SelectLastVisibleRow();
        UpdateEventScrollBar();
    }

    private void ResetRowsForSession(MacroSession? session)
    {
        _eventGridView.Rows.Clear();
        _displayedSession = session;
        _displayedSessionId = session?.Id;
        _displayedEventCount = 0;
        _displayedElapsedMs = 0;
    }

    private void RebuildRowsForCurrentSession()
    {
        MacroSession? session = _displayedSession;

        if (session is null)
        {
            ResetRowsForSession(null);
            UpdateVisibleState();
            UpdateEventScrollBar();
            return;
        }

        ResetRowsForSession(session);
        AppendRows(session, 0);
        UpdateVisibleState();
        SelectLastVisibleRow();
        UpdateEventScrollBar();
    }

    private void AppendRows(MacroSession session, int startIndex)
    {
        if (startIndex >= session.Events.Count)
        {
            return;
        }

        EventListFilterCriteria criteria = BuildFilterCriteria();
        _eventGridView.SuspendLayout();
        try
        {
            for (int index = startIndex; index < session.Events.Count; index++)
            {
                MacroEvent macroEvent = session.Events[index];
                _displayedElapsedMs += Math.Max(0, macroEvent.DelayMs);

                if (!EventListFilterEngine.Matches(session.Events, index, _displayedElapsedMs, criteria))
                {
                    continue;
                }

                AddEventRow(new EventListViewItem(index, macroEvent, _displayedElapsedMs));
            }
        }
        finally
        {
            _displayedEventCount = session.Events.Count;
            _eventGridView.ResumeLayout();
        }
    }

    private void AddEventRow(EventListViewItem viewItem)
    {
        MacroEvent macroEvent = viewItem.Event;
        int rowIndex = _eventGridView.Rows.Add(
            (viewItem.SourceIndex + 1).ToString("000", CultureInfo.InvariantCulture),
            FormatElapsedTime(viewItem.ElapsedMs),
            FormatEventType(macroEvent.EventType),
            FormatAction(macroEvent),
            FormatPosition(macroEvent),
            FormattableString.Invariant($"{macroEvent.DelayMs} ms"),
            FormatDetail(macroEvent));
        _eventGridView.Rows[rowIndex].Tag = viewItem.SourceIndex;
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
        rootLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, DesignTokens.Scale(42)));
        rootLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

        _gridHostPanel.Dock = DockStyle.Fill;
        _gridHostPanel.BackColor = DesignTokens.SurfaceInset;
        _gridHostPanel.Margin = Padding.Empty;
        _gridHostPanel.Padding = Padding.Empty;
        _gridHostPanel.Resize += (_, _) => LayoutGridViewport();

        _eventGridView.Dock = DockStyle.None;
        _emptyStateLabel.Dock = DockStyle.None;
        _emptyStateLabel.Text = "Secili bir oturum yok. Yeni bir makro kaydedin veya diskten yukleyin.";
        _emptyStateLabel.TextAlign = ContentAlignment.MiddleCenter;

        _eventScrollBar.Dock = DockStyle.None;
        _eventScrollBar.BackColor = DesignTokens.SurfaceInset;
        _eventScrollBar.ValueChanged += (_, _) => ScrollGridTo(_eventScrollBar.Value);

        _gridHostPanel.Controls.Add(_eventGridView);
        _gridHostPanel.Controls.Add(_eventScrollBar);
        _gridHostPanel.Controls.Add(_emptyStateLabel);
        rootLayoutPanel.Controls.Add(CreateFilterToolbar(), 0, 0);
        rootLayoutPanel.Controls.Add(_gridHostPanel, 0, 1);

        Controls.Add(rootLayoutPanel);
    }

    private TableLayoutPanel CreateFilterToolbar()
    {
        var toolbarPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 5,
            RowCount = 1,
            BackColor = DesignTokens.Surface,
            Margin = Padding.Empty,
            Padding = new Padding(0, 0, 0, DesignTokens.Scale(8))
        };
        toolbarPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42f));
        toolbarPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, DesignTokens.Scale(124)));
        toolbarPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, DesignTokens.Scale(168)));
        toolbarPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, DesignTokens.Scale(84)));
        toolbarPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 18f));
        toolbarPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

        var searchPanel = new RoundedInputHostPanel
        {
            Dock = DockStyle.Fill,
            BackColor = DesignTokens.SurfaceInset,
            BorderColor = DesignTokens.BorderSoft,
            Margin = new Padding(0, 0, DesignTokens.Scale(8), 0),
            Padding = new Padding(
                DesignTokens.Scale(12),
                DesignTokens.Scale(6),
                DesignTokens.Scale(10),
                DesignTokens.Scale(5))
        };
        searchPanel.Click += (_, _) => _filterSearchTextBox.Focus();
        searchPanel.Controls.Add(_filterSearchTextBox);

        _typeFilterSelect.Dock = DockStyle.Fill;
        _typeFilterSelect.Margin = new Padding(0, 0, DesignTokens.Scale(8), 0);
        _typeFilterSelect.SetItems(TypeFilterOptions.Select(option => option.Label));
        _typeFilterSelect.SelectedIndexChanged += (_, _) => UpdateFiltersFromControls();

        _smartFilterSelect.Dock = DockStyle.Fill;
        _smartFilterSelect.Margin = new Padding(0, 0, DesignTokens.Scale(8), 0);
        _smartFilterSelect.SetItems(SmartFilterOptions.Select(option => option.Label));
        _smartFilterSelect.SelectedIndexChanged += (_, _) => UpdateFiltersFromControls();

        _clearFilterButton.Dock = DockStyle.Fill;
        _clearFilterButton.Text = "Temizle";
        _clearFilterButton.Margin = new Padding(0, 0, DesignTokens.Scale(8), 0);
        _clearFilterButton.Click += (_, _) => ClearFilters();

        _filterStatusLabel.Dock = DockStyle.Fill;
        _filterStatusLabel.TextAlign = ContentAlignment.MiddleRight;
        _filterStatusLabel.AutoEllipsis = true;
        _filterStatusLabel.BackColor = DesignTokens.Surface;
        _filterStatusLabel.ForeColor = DesignTokens.TextSecondary;
        _filterStatusLabel.Font = DesignTokens.FontUiSmall;

        toolbarPanel.Controls.Add(searchPanel, 0, 0);
        toolbarPanel.Controls.Add(_typeFilterSelect, 1, 0);
        toolbarPanel.Controls.Add(_smartFilterSelect, 2, 0);
        toolbarPanel.Controls.Add(_clearFilterButton, 3, 0);
        toolbarPanel.Controls.Add(_filterStatusLabel, 4, 0);

        return toolbarPanel;
    }

    private void LayoutGridViewport()
    {
        int inset = DesignTokens.Scale(1);
        int scrollWidth = _eventScrollBar.Visible ? DesignTokens.Scale(14) : 0;
        int scrollGap = _eventScrollBar.Visible ? DesignTokens.Scale(6) : 0;
        int contentWidth = Math.Max(0, _gridHostPanel.ClientSize.Width - (inset * 2));
        int gridWidth = Math.Max(0, contentWidth - scrollWidth - scrollGap);
        int gridHeight = Math.Max(0, _gridHostPanel.ClientSize.Height - (inset * 2));

        _eventGridView.Bounds = new Rectangle(inset, inset, gridWidth, gridHeight);
        _emptyStateLabel.Bounds = new Rectangle(inset, inset, contentWidth, gridHeight);
        _eventScrollBar.Bounds = new Rectangle(
            inset + gridWidth + scrollGap,
            inset,
            scrollWidth,
            gridHeight);
        ApplyColumnWidths();
    }

    private void ConfigureGrid()
    {
        _eventGridView.AllowUserToAddRows = false;
        _eventGridView.AllowUserToDeleteRows = false;
        _eventGridView.AllowUserToResizeRows = false;
        _eventGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
        _eventGridView.BackgroundColor = DesignTokens.SurfaceInset;
        _eventGridView.BorderStyle = BorderStyle.None;
        _eventGridView.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        _eventGridView.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
        _eventGridView.ColumnHeadersHeight = DesignTokens.Scale(42);
        _eventGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        _eventGridView.EnableHeadersVisualStyles = false;
        _eventGridView.GridColor = DesignTokens.BorderSoft;
        _eventGridView.MultiSelect = false;
        _eventGridView.ReadOnly = true;
        _eventGridView.RowHeadersVisible = false;
        _eventGridView.RowTemplate.Height = DesignTokens.Scale(38);
        _eventGridView.ScrollBars = ScrollBars.None;
        _eventGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _eventGridView.Resize += (_, _) => ApplyColumnWidths();
        _eventGridView.Resize += (_, _) => UpdateEventScrollBar();
        _eventGridView.Scroll += (_, _) => SyncEventScrollBarFromGrid();
        _eventGridView.MouseWheel += EventGridView_MouseWheel;
        _eventGridView.CellMouseDown += EventGridView_CellMouseDown;
        _eventGridView.CellPainting += EventGridView_CellPainting;

        _eventGridView.Columns.Add(CreateTextColumn("#", "#", 58, 6));
        _eventGridView.Columns.Add(CreateTextColumn("Time", "Zaman", 126, 15));
        _eventGridView.Columns.Add(CreateTextColumn("Type", "Tur", 78, 10));
        _eventGridView.Columns.Add(CreateTextColumn("Action", "Aksiyon", 92, 15));
        _eventGridView.Columns.Add(CreateTextColumn("Position", "Konum", 138, 22));
        _eventGridView.Columns.Add(CreateTextColumn("Delay", "Gecikme", 86, 11));
        _eventGridView.Columns.Add(CreateTextColumn("Detail", "Detay", 132, 21));
        ApplyColumnWidths();
    }

    private TextBox CreateFilterSearchTextBox()
    {
        var textBox = new TextBox
        {
            BorderStyle = BorderStyle.None,
            Dock = DockStyle.Fill,
            PlaceholderText = "Olay ara...",
            BackColor = DesignTokens.SurfaceInset,
            ForeColor = DesignTokens.TextPrimary,
            Font = DesignTokens.FontUiNormal,
            Margin = Padding.Empty
        };

        textBox.TextChanged += (_, _) => UpdateFiltersFromControls();
        return textBox;
    }

    private void UpdateFiltersFromControls()
    {
        if (_updatingFilterControls)
        {
            return;
        }

        _selectedTypeFilterKind = GetSelectedTypeFilter();
        _selectedSmartFilterKind = GetSelectedSmartFilter();
        _clearFilterButton.IsActive = HasActiveFilters();
        RebuildRowsForCurrentSession();
    }

    private void ClearFilters()
    {
        if (!HasActiveFilters())
        {
            return;
        }

        _updatingFilterControls = true;
        try
        {
            _filterSearchTextBox.Clear();
            _typeFilterSelect.SelectedIndex = 0;
            _smartFilterSelect.SelectedIndex = 0;
            _selectedTypeFilterKind = EventListTypeFilterKind.All;
            _selectedSmartFilterKind = EventListSmartFilterKind.All;
        }
        finally
        {
            _updatingFilterControls = false;
        }

        _clearFilterButton.IsActive = false;
        RebuildRowsForCurrentSession();
    }

    private EventListFilterCriteria BuildFilterCriteria()
    {
        return new EventListFilterCriteria(
            _filterSearchTextBox.Text,
            _selectedTypeFilterKind,
            _selectedSmartFilterKind);
    }

    private bool HasActiveFilters()
    {
        return !string.IsNullOrWhiteSpace(_filterSearchTextBox.Text)
            || _selectedTypeFilterKind != EventListTypeFilterKind.All
            || _selectedSmartFilterKind != EventListSmartFilterKind.All;
    }

    private EventListTypeFilterKind GetSelectedTypeFilter()
    {
        int selectedIndex = _typeFilterSelect.SelectedIndex;
        return selectedIndex >= 0 && selectedIndex < TypeFilterOptions.Length
            ? TypeFilterOptions[selectedIndex].Kind
            : EventListTypeFilterKind.All;
    }

    private EventListSmartFilterKind GetSelectedSmartFilter()
    {
        int selectedIndex = _smartFilterSelect.SelectedIndex;
        return selectedIndex >= 0 && selectedIndex < SmartFilterOptions.Length
            ? SmartFilterOptions[selectedIndex].Kind
            : EventListSmartFilterKind.All;
    }

    private void UpdateVisibleState()
    {
        int totalEventCount = _displayedSession?.Events.Count ?? 0;
        int visibleEventCount = _eventGridView.Rows.Count;
        bool hasVisibleRows = visibleEventCount > 0;

        _eventGridView.Visible = hasVisibleRows;
        _emptyStateLabel.Visible = !hasVisibleRows;

        if (!hasVisibleRows)
        {
            _emptyStateLabel.Text = ResolveEmptyStateText(totalEventCount);
        }

        _filterStatusLabel.Text = FormattableString.Invariant($"{visibleEventCount} / {totalEventCount} olay");
    }

    private string ResolveEmptyStateText(int totalEventCount)
    {
        if (_displayedSession is null)
        {
            return "Secili bir oturum yok. Yeni bir makro kaydedin veya diskten yukleyin.";
        }

        if (totalEventCount == 0)
        {
            return "Secili oturumda olay yok.";
        }

        return HasActiveFilters()
            ? "Filtreyle eslesen olay bulunamadi."
            : "Secili oturumda gosterilecek olay yok.";
    }

    private void SelectLastVisibleRow()
    {
        int rowIndex = _eventGridView.Rows.Count - 1;

        if (rowIndex < 0)
        {
            return;
        }

        _eventGridView.ClearSelection();
        _eventGridView.Rows[rowIndex].Selected = true;

        try
        {
            _eventGridView.FirstDisplayedScrollingRowIndex = Math.Max(0, rowIndex);
        }
        catch (InvalidOperationException)
        {
            // DataGridView can reject the scroll while it is rebuilding rows.
        }
    }

    private void EventGridView_MouseWheel(object? sender, MouseEventArgs e)
    {
        _ = sender;

        int wheelStep = Math.Max(1, SystemInformation.MouseWheelScrollLines);
        _eventScrollBar.Value -= Math.Sign(e.Delta) * wheelStep;
    }

    private void EventGridView_CellMouseDown(object? sender, DataGridViewCellMouseEventArgs e)
    {
        _ = sender;

        if (e.Button != MouseButtons.Right || e.RowIndex < 0 || _displayedSession is null)
        {
            return;
        }

        _eventGridView.ClearSelection();
        _eventGridView.Rows[e.RowIndex].Selected = true;
        _eventGridView.CurrentCell = _eventGridView.Rows[e.RowIndex].Cells[Math.Max(0, e.ColumnIndex)];

        int rowIndex = e.RowIndex;
        ThemedDropDownMenu.ShowFor(
            _eventGridView,
            [
                new ThemedDropDownMenuItem(
                    "Düzenle",
                    () => RequestEditForRow(rowIndex))
            ],
            _eventGridView.PointToClient(Cursor.Position),
            new ThemedDropDownMenuOptions
            {
                MinimumWidth = DesignTokens.Scale(126),
                MaximumWidth = DesignTokens.Scale(220),
                ItemHeight = DesignTokens.Scale(42),
                VerticalPadding = DesignTokens.Scale(8),
                HorizontalPadding = DesignTokens.Scale(14)
            });
    }

    private void EventGridView_CellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
    {
        _ = sender;

        if (e.RowIndex < 0 || e.ColumnIndex != _eventGridView.Columns["Type"].Index)
        {
            return;
        }

        string eventType = e.Value?.ToString() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(eventType))
        {
            return;
        }

        Graphics? graphics = e.Graphics;
        Font font = e.CellStyle?.Font ?? _eventGridView.Font;
        if (graphics is null)
        {
            return;
        }

        e.Paint(
            e.CellBounds,
            DataGridViewPaintParts.Background |
            DataGridViewPaintParts.Border |
            DataGridViewPaintParts.SelectionBackground);

        Color accentColor = eventType switch
        {
            "Fare" => Color.FromArgb(79, 158, 255),
            "Klavye" => Color.FromArgb(123, 95, 255),
            "Sistem" => Color.FromArgb(52, 199, 89),
            _ => DesignTokens.TextSecondary
        };
        Color pillBackColor = Color.FromArgb(
            _eventGridView.Rows[e.RowIndex].Selected ? 44 : 26,
            accentColor);

        int horizontalPadding = DesignTokens.Scale(8);
        int verticalPadding = DesignTokens.Scale(7);
        Size textSize = TextRenderer.MeasureText(eventType, font);
        int pillWidth = Math.Min(
            e.CellBounds.Width - horizontalPadding * 2,
            Math.Max(DesignTokens.Scale(54), textSize.Width + DesignTokens.Scale(18)));
        int pillHeight = Math.Min(
            e.CellBounds.Height - verticalPadding * 2,
            Math.Max(DesignTokens.Scale(22), textSize.Height + DesignTokens.Scale(4)));

        var pillBounds = new Rectangle(
            e.CellBounds.Left + (e.CellBounds.Width - pillWidth) / 2,
            e.CellBounds.Top + (e.CellBounds.Height - pillHeight) / 2,
            Math.Max(1, pillWidth),
            Math.Max(1, pillHeight));

        using GraphicsPath pillPath = CreateRoundedRectanglePath(pillBounds, DesignTokens.Scale(10));
        using var backgroundBrush = new SolidBrush(pillBackColor);
        using var borderPen = new Pen(Color.FromArgb(120, accentColor));
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        graphics.FillPath(backgroundBrush, pillPath);
        graphics.DrawPath(borderPen, pillPath);

        TextRenderer.DrawText(
            graphics,
            eventType,
            font,
            pillBounds,
            accentColor,
            TextFormatFlags.HorizontalCenter |
            TextFormatFlags.VerticalCenter |
            TextFormatFlags.EndEllipsis |
            TextFormatFlags.NoPrefix);

        e.Handled = true;
    }

    private void RequestEditForRow(int rowIndex)
    {
        if (_displayedSession is null
            || rowIndex < 0
            || rowIndex >= _eventGridView.Rows.Count
            || _eventGridView.Rows[rowIndex].Tag is not int eventIndex
            || eventIndex < 0
            || eventIndex >= _displayedSession.Events.Count)
        {
            return;
        }

        EventEditRequested?.Invoke(
            this,
            new EventEditRequestedEventArgs(eventIndex, _displayedSession.Events[eventIndex]));
    }

    private void ApplyTheme()
    {
        _emptyStateLabel.BackColor = DesignTokens.SurfaceInset;
        _emptyStateLabel.ForeColor = DesignTokens.TextSecondary;
        _emptyStateLabel.Font = DesignTokens.FontUiNormal;

        _filterSearchTextBox.BackColor = DesignTokens.SurfaceInset;
        _filterSearchTextBox.ForeColor = DesignTokens.TextPrimary;
        _filterSearchTextBox.Font = DesignTokens.FontUiNormal;

        _filterStatusLabel.BackColor = DesignTokens.Surface;
        _filterStatusLabel.ForeColor = DesignTokens.TextSecondary;
        _filterStatusLabel.Font = DesignTokens.FontUiSmall;

        _eventGridView.DefaultCellStyle.BackColor = DesignTokens.SurfaceInset;
        _eventGridView.DefaultCellStyle.ForeColor = DesignTokens.TextPrimary;
        _eventGridView.DefaultCellStyle.Font = DesignTokens.FontUiNormal;
        _eventGridView.DefaultCellStyle.SelectionBackColor = Color.FromArgb(24, 93, 188);
        _eventGridView.DefaultCellStyle.SelectionForeColor = DesignTokens.TextPrimary;
        _eventGridView.DefaultCellStyle.Padding = new Padding(DesignTokens.Scale(6), 0, DesignTokens.Scale(6), 0);

        _eventGridView.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(12, 17, 27);
        _eventGridView.AlternatingRowsDefaultCellStyle.ForeColor = DesignTokens.TextPrimary;

        _eventGridView.ColumnHeadersDefaultCellStyle.BackColor = DesignTokens.Surface2;
        _eventGridView.ColumnHeadersDefaultCellStyle.ForeColor = DesignTokens.TextPrimary;
        _eventGridView.ColumnHeadersDefaultCellStyle.Font = DesignTokens.FontUiBold;
        _eventGridView.ColumnHeadersDefaultCellStyle.SelectionBackColor = DesignTokens.Surface2;
        _eventGridView.ColumnHeadersDefaultCellStyle.SelectionForeColor = DesignTokens.TextPrimary;
        _eventGridView.ColumnHeadersDefaultCellStyle.Padding = new Padding(
            DesignTokens.Scale(8),
            0,
            DesignTokens.Scale(8),
            0);
    }

    private static DataGridViewTextBoxColumn CreateTextColumn(
        string name,
        string headerText,
        int minimumWidth,
        int fillWeight)
    {
        int scaledMinimumWidth = DesignTokens.Scale(minimumWidth);
        int scaledWidth = DesignTokens.Scale(Math.Max(minimumWidth, fillWeight * 10));

        return new DataGridViewTextBoxColumn
        {
            Name = name,
            HeaderText = headerText,
            AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
            MinimumWidth = scaledMinimumWidth,
            Width = scaledWidth,
            FillWeight = fillWeight,
            SortMode = DataGridViewColumnSortMode.NotSortable
        };
    }

    private void ApplyColumnWidths()
    {
        if (_eventGridView.Columns.Count == 0)
        {
            return;
        }

        int availableWidth = _eventGridView.ClientSize.Width;

        if (availableWidth <= 0)
        {
            return;
        }

        float totalWeight = 0f;

        foreach (DataGridViewColumn column in _eventGridView.Columns)
        {
            totalWeight += Math.Max(1f, column.FillWeight);
        }

        int assignedWidth = 0;

        for (int index = 0; index < _eventGridView.Columns.Count; index++)
        {
            DataGridViewColumn column = _eventGridView.Columns[index];
            bool isLastColumn = index == _eventGridView.Columns.Count - 1;
            int targetWidth = isLastColumn
                ? availableWidth - assignedWidth
                : (int)Math.Round(availableWidth * Math.Max(1f, column.FillWeight) / totalWeight);

            column.Width = Math.Max(column.MinimumWidth, targetWidth);
            assignedWidth += column.Width;
        }
    }

    private void UpdateEventScrollBar()
    {
        if (_eventGridView.Rows.Count == 0 || !_eventGridView.Visible)
        {
            _eventScrollBar.SetRange(0, 1, 0);
            _eventScrollBar.Visible = false;
            LayoutGridViewport();
            return;
        }

        int visibleRows = Math.Max(1, _eventGridView.DisplayedRowCount(false));
        int maximum = Math.Max(0, _eventGridView.Rows.Count - visibleRows);
        int currentIndex = GetFirstDisplayedRowIndex();

        _eventScrollBar.SetRange(maximum, visibleRows, Math.Min(currentIndex, maximum));
        _eventScrollBar.Visible = maximum > 0;
        LayoutGridViewport();
        if (_eventScrollBar.Visible)
        {
            _eventScrollBar.BringToFront();
        }
    }

    private void SyncEventScrollBarFromGrid()
    {
        if (_eventGridView.Rows.Count == 0)
        {
            _eventScrollBar.SetValueSilently(0);
            return;
        }

        _eventScrollBar.SetValueSilently(GetFirstDisplayedRowIndex());
    }

    private void ScrollGridTo(int rowIndex)
    {
        if (_eventGridView.Rows.Count == 0)
        {
            return;
        }

        int targetRowIndex = Math.Clamp(rowIndex, 0, _eventGridView.Rows.Count - 1);
        try
        {
            _eventGridView.FirstDisplayedScrollingRowIndex = targetRowIndex;
        }
        catch (InvalidOperationException)
        {
            // DataGridView can reject the scroll while it is rebuilding rows.
        }
    }

    private int GetFirstDisplayedRowIndex()
    {
        try
        {
            return _eventGridView.FirstDisplayedScrollingRowIndex < 0
                ? 0
                : _eventGridView.FirstDisplayedScrollingRowIndex;
        }
        catch (InvalidOperationException)
        {
            return 0;
        }
    }

    private sealed record TypeFilterOption(
        EventListTypeFilterKind Kind,
        string Label);

    private sealed record SmartFilterOption(
        EventListSmartFilterKind Kind,
        string Label);

    private sealed class RoundedInputHostPanel : Panel
    {
        public Color BorderColor { get; set; } = DesignTokens.BorderSoft;

        public RoundedInputHostPanel()
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

            Rectangle bounds = Rectangle.Inflate(ClientRectangle, -1, -1);
            if (bounds.Width <= 0 || bounds.Height <= 0)
            {
                return;
            }

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using GraphicsPath path = CreateRoundedRectanglePath(bounds, DesignTokens.Scale(7));
            using var fillBrush = new SolidBrush(BackColor);
            using var borderPen = new Pen(BorderColor, Math.Max(1f, DesignTokens.DensityScale));
            e.Graphics.FillPath(fillBrush, path);
            e.Graphics.DrawPath(borderPen, path);
        }
    }

    private sealed class CompactTextButton : Control
    {
        private bool _isHovered;
        private bool _isPressed;
        private bool _isActive;

        public CompactTextButton()
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
            Font = DesignTokens.FontUiBold;
            ForeColor = DesignTokens.TextSecondary;
            BackColor = DesignTokens.Surface2;
            MinimumSize = new Size(DesignTokens.Scale(76), DesignTokens.Scale(30));
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

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.Clear(Parent?.BackColor ?? DesignTokens.Surface);

            Rectangle bounds = Rectangle.Inflate(ClientRectangle, -1, -1);
            if (bounds.Width <= 0 || bounds.Height <= 0)
            {
                return;
            }

            using GraphicsPath path = CreateRoundedRectanglePath(bounds, DesignTokens.Scale(7));
            using var fillBrush = new SolidBrush(ResolveFillColor());
            using var borderPen = new Pen(ResolveBorderColor(), Math.Max(1f, DesignTokens.DensityScale));
            e.Graphics.FillPath(fillBrush, path);
            e.Graphics.DrawPath(borderPen, path);

            TextRenderer.DrawText(
                e.Graphics,
                Text,
                Font,
                bounds,
                ResolveTextColor(),
                TextFormatFlags.HorizontalCenter |
                TextFormatFlags.VerticalCenter |
                TextFormatFlags.EndEllipsis |
                TextFormatFlags.NoPrefix);
        }

        private Color ResolveFillColor()
        {
            if (!Enabled)
            {
                return Color.FromArgb(88, DesignTokens.SurfaceInset);
            }

            if (_isPressed)
            {
                return DesignTokens.Surface3;
            }

            if (_isActive)
            {
                return Color.FromArgb(24, DesignTokens.Accent);
            }

            return _isHovered
                ? DesignTokens.SurfaceHover
                : DesignTokens.Surface2;
        }

        private Color ResolveBorderColor()
        {
            if (!Enabled)
            {
                return Color.FromArgb(70, DesignTokens.BorderSoft);
            }

            if (_isActive || Focused)
            {
                return DesignTokens.Accent;
            }

            return _isHovered
                ? DesignTokens.BorderBright
                : DesignTokens.BorderSoft;
        }

        private Color ResolveTextColor()
        {
            if (!Enabled)
            {
                return DesignTokens.TextMuted;
            }

            return _isActive
                ? DesignTokens.Accent
                : DesignTokens.TextSecondary;
        }
    }

    private sealed class RoundedGridHostPanel : Panel
    {
        private Size _lastRegionSize;
        private int _lastRegionRadius = -1;

        public RoundedGridHostPanel()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint,
                true);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            UpdateRegion();
            Invalidate();
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            e.Graphics.Clear(Parent?.BackColor ?? DesignTokens.Surface);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Rectangle bounds = Rectangle.Inflate(ClientRectangle, -1, -1);
            if (bounds.Width <= 0 || bounds.Height <= 0)
            {
                return;
            }

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            using GraphicsPath path = CreateRoundedRectanglePath(bounds, DesignTokens.Scale(8));
            using var fillBrush = new SolidBrush(DesignTokens.SurfaceInset);
            using var borderPen = new Pen(DesignTokens.BorderSoft);

            e.Graphics.FillPath(fillBrush, path);
            e.Graphics.DrawPath(borderPen, path);
            e.Graphics.SmoothingMode = SmoothingMode.None;
        }

        private void UpdateRegion()
        {
            Rectangle bounds = ClientRectangle;
            int radius = DesignTokens.Scale(8);

            if (bounds.Width <= 0 || bounds.Height <= 0)
            {
                Region? emptyRegion = Region;
                Region = null;
                emptyRegion?.Dispose();
                _lastRegionSize = Size.Empty;
                _lastRegionRadius = -1;
                return;
            }

            if (Region is not null
                && _lastRegionSize == bounds.Size
                && _lastRegionRadius == radius)
            {
                return;
            }

            using GraphicsPath path = CreateRoundedRectanglePath(
                Rectangle.Inflate(bounds, -1, -1),
                radius);
            Region? previousRegion = Region;
            Region = new Region(path);
            previousRegion?.Dispose();
            _lastRegionSize = bounds.Size;
            _lastRegionRadius = radius;
        }
    }

    private static GraphicsPath CreateRoundedRectanglePath(Rectangle bounds, int radius)
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

    private static string FormatElapsedTime(int elapsedMs)
    {
        TimeSpan elapsed = TimeSpan.FromMilliseconds(elapsedMs);
        return FormattableString.Invariant(
            $"{(int)elapsed.TotalHours:00}:{elapsed.Minutes:00}:{elapsed.Seconds:00}.{elapsed.Milliseconds:000}");
    }

    private static string FormatEventType(MacroEventType eventType)
    {
        return eventType switch
        {
            MacroEventType.Keyboard => "Klavye",
            MacroEventType.Mouse => "Fare",
            MacroEventType.System => "Sistem",
            _ => eventType.ToString()
        };
    }

    private static string FormatAction(MacroEvent macroEvent)
    {
        return macroEvent.EventType switch
        {
            MacroEventType.Keyboard => macroEvent.KeyboardActionType.ToString(),
            MacroEventType.Mouse => macroEvent.MouseActionType.ToString(),
            MacroEventType.System => "Sistem",
            _ => macroEvent.Description
        };
    }

    private static string FormatPosition(MacroEvent macroEvent)
    {
        if (macroEvent.X.HasValue && macroEvent.Y.HasValue)
        {
            return FormattableString.Invariant($"X: {macroEvent.X}, Y: {macroEvent.Y}");
        }

        return "-";
    }

    private static string FormatDetail(MacroEvent macroEvent)
    {
        return macroEvent.EventType switch
        {
            MacroEventType.Keyboard when !string.IsNullOrWhiteSpace(macroEvent.KeyName) => macroEvent.KeyName,
            MacroEventType.Mouse when macroEvent.MouseActionType == MouseActionType.Wheel => FormattableString.Invariant(
                $"{macroEvent.Description} ({macroEvent.WheelDelta ?? 0})"),
            _ => string.IsNullOrWhiteSpace(macroEvent.Description)
                ? "-"
                : macroEvent.Description
        };
    }
}

internal sealed class EventEditRequestedEventArgs : EventArgs
{
    public EventEditRequestedEventArgs(int eventIndex, MacroEvent macroEvent)
    {
        EventIndex = eventIndex;
        MacroEvent = macroEvent;
    }

    public int EventIndex { get; }

    public MacroEvent MacroEvent { get; }
}
