using MacroMaster.Domain.Enums;
using MacroMaster.Domain.Models;
using MacroMaster.WinForms.Theme;
using System.Globalization;

namespace MacroMaster.WinForms.Controls;

internal sealed class EventListControl : UserControl
{
    private readonly DataGridView _eventGridView;
    private readonly Label _emptyStateLabel;
    private readonly Label _summaryLabel;
    private Guid? _displayedSessionId;
    private int _displayedEventCount;
    private int _displayedElapsedMs;

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
        _emptyStateLabel = new Label();
        _summaryLabel = new Label();

        BuildLayout();
        ConfigureGrid();
        ApplyTheme();
        SetSession(null);
    }

    public void SetSession(MacroSession? session)
    {
        if (session is null)
        {
            ClearRows();
            _emptyStateLabel.Visible = true;
            _eventGridView.Visible = false;
            _summaryLabel.Text = "Secili oturum yok";
            return;
        }

        bool canAppend = _displayedSessionId == session.Id
            && session.Events.Count >= _displayedEventCount;

        if (!canAppend)
        {
            ClearRows();
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

            _eventGridView.Rows.Add(
                (index + 1).ToString("000", CultureInfo.InvariantCulture),
                FormatElapsedTime(_displayedElapsedMs),
                FormatEventType(macroEvent.EventType),
                FormatAction(macroEvent),
                FormatPosition(macroEvent),
                FormattableString.Invariant($"{macroEvent.DelayMs} ms"),
                FormatDetail(macroEvent));
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
        rootLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 30f));

        var gridHostPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = DesignTokens.Background,
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
        _eventGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _eventGridView.BackgroundColor = DesignTokens.Background;
        _eventGridView.BorderStyle = BorderStyle.None;
        _eventGridView.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        _eventGridView.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
        _eventGridView.ColumnHeadersHeight = 38;
        _eventGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        _eventGridView.EnableHeadersVisualStyles = false;
        _eventGridView.GridColor = DesignTokens.Border;
        _eventGridView.MultiSelect = false;
        _eventGridView.ReadOnly = true;
        _eventGridView.RowHeadersVisible = false;
        _eventGridView.RowTemplate.Height = 34;
        _eventGridView.ScrollBars = ScrollBars.Vertical;
        _eventGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

        _eventGridView.Columns.Add(CreateTextColumn("#", "#", 58, 6));
        _eventGridView.Columns.Add(CreateTextColumn("Time", "Zaman", 120, 13));
        _eventGridView.Columns.Add(CreateTextColumn("Type", "Tur", 92, 10));
        _eventGridView.Columns.Add(CreateTextColumn("Action", "Aksiyon", 150, 17));
        _eventGridView.Columns.Add(CreateTextColumn("Position", "Konum", 170, 19));
        _eventGridView.Columns.Add(CreateTextColumn("Delay", "Gecikme", 96, 11));
        _eventGridView.Columns.Add(CreateTextColumn("Detail", "Detay", 220, 24));
    }

    private void ApplyTheme()
    {
        _emptyStateLabel.BackColor = DesignTokens.Background;
        _emptyStateLabel.ForeColor = DesignTokens.TextSecondary;
        _emptyStateLabel.Font = DesignTokens.FontUiNormal;

        _summaryLabel.BackColor = DesignTokens.Surface;
        _summaryLabel.ForeColor = DesignTokens.TextSecondary;
        _summaryLabel.Font = DesignTokens.FontUiNormal;
        _summaryLabel.Padding = new Padding(2, 6, 0, 0);

        _eventGridView.DefaultCellStyle.BackColor = DesignTokens.Background;
        _eventGridView.DefaultCellStyle.ForeColor = DesignTokens.TextPrimary;
        _eventGridView.DefaultCellStyle.Font = DesignTokens.FontUiNormal;
        _eventGridView.DefaultCellStyle.SelectionBackColor = Color.FromArgb(18, 82, 166);
        _eventGridView.DefaultCellStyle.SelectionForeColor = DesignTokens.TextPrimary;
        _eventGridView.DefaultCellStyle.Padding = new Padding(8, 0, 8, 0);

        _eventGridView.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(13, 17, 26);
        _eventGridView.AlternatingRowsDefaultCellStyle.ForeColor = DesignTokens.TextPrimary;

        _eventGridView.ColumnHeadersDefaultCellStyle.BackColor = DesignTokens.Surface2;
        _eventGridView.ColumnHeadersDefaultCellStyle.ForeColor = DesignTokens.TextPrimary;
        _eventGridView.ColumnHeadersDefaultCellStyle.Font = DesignTokens.FontUiBold;
        _eventGridView.ColumnHeadersDefaultCellStyle.SelectionBackColor = DesignTokens.Surface2;
        _eventGridView.ColumnHeadersDefaultCellStyle.SelectionForeColor = DesignTokens.TextPrimary;
        _eventGridView.ColumnHeadersDefaultCellStyle.Padding = new Padding(8, 0, 8, 0);
    }

    private static DataGridViewTextBoxColumn CreateTextColumn(
        string name,
        string headerText,
        int minimumWidth,
        int fillWeight)
    {
        return new DataGridViewTextBoxColumn
        {
            Name = name,
            HeaderText = headerText,
            MinimumWidth = minimumWidth,
            FillWeight = fillWeight,
            SortMode = DataGridViewColumnSortMode.NotSortable
        };
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
