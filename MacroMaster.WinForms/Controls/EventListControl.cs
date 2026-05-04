using MacroMaster.Domain.Enums;
using MacroMaster.Domain.Models;
using MacroMaster.WinForms.Theme;
using System.Drawing.Drawing2D;
using System.Globalization;

namespace MacroMaster.WinForms.Controls;

internal sealed class EventListControl : UserControl
{
    private readonly DataGridView _eventGridView;
    private readonly ContextMenuStrip _eventContextMenu;
    private readonly Label _emptyStateLabel;
    private readonly Label _summaryLabel;
    private MacroSession? _displayedSession;
    private Guid? _displayedSessionId;
    private int _displayedEventCount;
    private int _displayedElapsedMs;

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
        _eventContextMenu = BuildEventContextMenu();
        _emptyStateLabel = new Label();
        _summaryLabel = new Label();

        BuildLayout();
        ConfigureGrid();
        ApplyTheme();
        SetSession(null);
    }

    public void SetSession(MacroSession? session, bool forceReload = false)
    {
        if (session is null)
        {
            ClearRows();
            _emptyStateLabel.Visible = true;
            _eventGridView.Visible = false;
            _summaryLabel.Text = "Secili oturum yok";
            return;
        }

        _displayedSession = session;

        bool canAppend = !forceReload
            && _displayedSessionId == session.Id
            && session.Events.Count >= _displayedEventCount;

        if (!canAppend)
        {
            ClearRows();
            _displayedSession = session;
            _displayedSessionId = session.Id;
        }

        AppendRows(session, _displayedEventCount);

        bool hasEvents = session.Events.Count > 0;
        _eventGridView.Visible = hasEvents;
        _emptyStateLabel.Visible = !hasEvents;
        _summaryLabel.Text = FormattableString.Invariant(
            $"Toplam olay: {session.Events.Count}  |  Toplam sure: {session.TotalDurationMs} ms");

        if (hasEvents)
        {
            int rowIndex = _eventGridView.Rows.Count - 1;
            if (rowIndex >= 0)
            {
                _eventGridView.ClearSelection();
                _eventGridView.Rows[rowIndex].Selected = true;
                _eventGridView.FirstDisplayedScrollingRowIndex = Math.Max(0, rowIndex);
            }
        }
    }

    private void ClearRows()
    {
        _eventGridView.Rows.Clear();
        _displayedSession = null;
        _displayedSessionId = null;
        _displayedEventCount = 0;
        _displayedElapsedMs = 0;
    }

    private void AppendRows(MacroSession session, int startIndex)
    {
        if (startIndex >= session.Events.Count)
        {
            return;
        }

        _eventGridView.SuspendLayout();

        for (int index = startIndex; index < session.Events.Count; index++)
        {
            MacroEvent macroEvent = session.Events[index];
            _displayedElapsedMs += Math.Max(0, macroEvent.DelayMs);

            int rowIndex = _eventGridView.Rows.Add(
                (index + 1).ToString("000", CultureInfo.InvariantCulture),
                FormatElapsedTime(_displayedElapsedMs),
                FormatEventType(macroEvent.EventType),
                FormatAction(macroEvent),
                FormatPosition(macroEvent),
                FormattableString.Invariant($"{macroEvent.DelayMs} ms"),
                FormatDetail(macroEvent));
            _eventGridView.Rows[rowIndex].Tag = index;
        }

        _displayedEventCount = session.Events.Count;
        _eventGridView.ResumeLayout();
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
        rootLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
        rootLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, DesignTokens.Scale(34)));

        var gridHostPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = DesignTokens.SurfaceInset,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };

        _eventGridView.Dock = DockStyle.Fill;
        _emptyStateLabel.Dock = DockStyle.Fill;
        _emptyStateLabel.Text = "Secili bir oturum yok. Yeni bir makro kaydedin veya diskten yukleyin.";
        _emptyStateLabel.TextAlign = ContentAlignment.MiddleCenter;

        _summaryLabel.Dock = DockStyle.Fill;
        _summaryLabel.TextAlign = ContentAlignment.MiddleLeft;

        gridHostPanel.Controls.Add(_eventGridView);
        gridHostPanel.Controls.Add(_emptyStateLabel);
        rootLayoutPanel.Controls.Add(gridHostPanel, 0, 0);
        rootLayoutPanel.Controls.Add(_summaryLabel, 0, 1);

        Controls.Add(rootLayoutPanel);
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
        _eventGridView.ScrollBars = ScrollBars.Both;
        _eventGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _eventGridView.Resize += (_, _) => ApplyColumnWidths();
        _eventGridView.CellMouseDown += EventGridView_CellMouseDown;
        _eventGridView.CellPainting += EventGridView_CellPainting;

        _eventGridView.Columns.Add(CreateTextColumn("#", "#", 32, 5));
        _eventGridView.Columns.Add(CreateTextColumn("Time", "Zaman", 70, 12));
        _eventGridView.Columns.Add(CreateTextColumn("Type", "Tur", 50, 9));
        _eventGridView.Columns.Add(CreateTextColumn("Action", "Aksiyon", 60, 15));
        _eventGridView.Columns.Add(CreateTextColumn("Position", "Konum", 80, 18));
        _eventGridView.Columns.Add(CreateTextColumn("Delay", "Gecikme", 55, 10));
        _eventGridView.Columns.Add(CreateTextColumn("Detail", "Detay", 80, 31));
        ApplyColumnWidths();
    }

    private ContextMenuStrip BuildEventContextMenu()
    {
        var contextMenu = new ContextMenuStrip
        {
            ShowImageMargin = false
        };
        AppToolStripRenderer.ApplyTo(contextMenu);

        var editItem = new ToolStripMenuItem("Duzenle");
        editItem.Click += (_, _) =>
        {
            if (_eventContextMenu.Tag is int rowIndex)
            {
                RequestEditForRow(rowIndex);
            }
        };
        contextMenu.Items.Add(editItem);

        return contextMenu;
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
        _eventContextMenu.Tag = e.RowIndex;
        _eventContextMenu.Show(_eventGridView, _eventGridView.PointToClient(Cursor.Position));
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

        _summaryLabel.BackColor = DesignTokens.Surface;
        _summaryLabel.ForeColor = DesignTokens.TextSecondary;
        _summaryLabel.Font = DesignTokens.FontUiNormal;
        _summaryLabel.Padding = new Padding(DesignTokens.Scale(10), DesignTokens.Scale(7), 0, 0);

        _eventGridView.DefaultCellStyle.BackColor = DesignTokens.SurfaceInset;
        _eventGridView.DefaultCellStyle.ForeColor = DesignTokens.TextPrimary;
        _eventGridView.DefaultCellStyle.Font = DesignTokens.FontUiNormal;
        _eventGridView.DefaultCellStyle.SelectionBackColor = Color.FromArgb(24, 93, 188);
        _eventGridView.DefaultCellStyle.SelectionForeColor = DesignTokens.TextPrimary;
        _eventGridView.DefaultCellStyle.Padding = new Padding(DesignTokens.Scale(10), 0, DesignTokens.Scale(10), 0);

        _eventGridView.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(12, 17, 27);
        _eventGridView.AlternatingRowsDefaultCellStyle.ForeColor = DesignTokens.TextPrimary;

        _eventGridView.ColumnHeadersDefaultCellStyle.BackColor = DesignTokens.Surface2;
        _eventGridView.ColumnHeadersDefaultCellStyle.ForeColor = DesignTokens.TextPrimary;
        _eventGridView.ColumnHeadersDefaultCellStyle.Font = DesignTokens.FontUiBold;
        _eventGridView.ColumnHeadersDefaultCellStyle.SelectionBackColor = DesignTokens.Surface2;
        _eventGridView.ColumnHeadersDefaultCellStyle.SelectionForeColor = DesignTokens.TextPrimary;
        _eventGridView.ColumnHeadersDefaultCellStyle.Padding = new Padding(
            DesignTokens.Scale(10),
            0,
            DesignTokens.Scale(10),
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

        int availableWidth = _eventGridView.ClientSize.Width
            - DesignTokens.GridScrollbarReserveWidth;

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
