using MacroMaster.Domain.Enums;
using MacroMaster.Domain.Models;
using MacroMaster.WinForms.Theme;

namespace MacroMaster.WinForms.Forms;

public partial class MainForm
{
    private void InitializeEventGrid()
    {
        _eventGrid.BackgroundColor = Color.FromArgb(10, 15, 28);
        _eventGrid.BorderStyle = BorderStyle.None;
        _eventGrid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        _eventGrid.EnableHeadersVisualStyles = false;
        _eventGrid.GridColor = Color.FromArgb(24, 33, 56);
        _eventGrid.RowHeadersVisible = false;
        _eventGrid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
        _eventGrid.ColumnHeadersHeight = 38;
        _eventGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        _eventGrid.RowTemplate.Height = 36;
        _eventGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _eventGrid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
        _eventGrid.ScrollBars = ScrollBars.Vertical;
        _eventGrid.DefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = Color.FromArgb(10, 15, 28),
            ForeColor = AppColors.TextPrimary,
            SelectionBackColor = Color.FromArgb(37, 99, 235),
            SelectionForeColor = AppColors.TextPrimary,
            Font = AppFonts.Body,
            Padding = new Padding(4, 0, 4, 0)
        };
        _eventGrid.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle(_eventGrid.DefaultCellStyle)
        {
            BackColor = Color.FromArgb(12, 18, 33)
        };
        _eventGrid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = Color.FromArgb(16, 24, 45),
            ForeColor = AppColors.TextPrimary,
            SelectionBackColor = Color.FromArgb(16, 24, 45),
            SelectionForeColor = AppColors.TextPrimary,
            Font = AppFonts.BodyStrong,
            Alignment = DataGridViewContentAlignment.MiddleLeft
        };

        _eventGrid.Columns.Clear();
        _eventGrid.Columns.Add(CreateTextColumn("Index", "#", 56f));
        _eventGrid.Columns.Add(CreateTextColumn("Time", "Zaman", 96f));
        _eventGrid.Columns.Add(CreateTextColumn("Type", "Tur", 82f));
        _eventGrid.Columns.Add(CreateTextColumn("Action", "Aksiyon", 118f));
        _eventGrid.Columns.Add(CreateTextColumn("Position", "Konum", 124f));
        _eventGrid.Columns.Add(CreateTextColumn("Delay", "Gecikme", 82f));
        _eventGrid.Columns.Add(CreateTextColumn("Detail", "Detay", 180f));

        _eventGrid.SelectionChanged += EventGrid_SelectionChanged;
        RefreshEventGrid();
    }

    private DataGridViewTextBoxColumn CreateTextColumn(string name, string headerText, float fillWeight)
    {
        return new DataGridViewTextBoxColumn
        {
            Name = name,
            HeaderText = headerText,
            ReadOnly = true,
            FillWeight = fillWeight,
            SortMode = DataGridViewColumnSortMode.NotSortable
        };
    }

    private void RefreshEventGrid()
    {
        _eventGrid.Rows.Clear();

        for (var index = 0; index < _timelineEvents.Count; index++)
        {
            AddEventRow(_timelineEvents[index], index);
        }
    }

    private void AddEventRow(MacroEvent macroEvent, int index)
    {
        var rowIndex = _eventGrid.Rows.Add(
            (index + 1).ToString("D3"),
            FormatElapsed(CalculateElapsedMs(index)),
            GetEventTypeText(macroEvent),
            GetActionText(macroEvent),
            GetLocationText(macroEvent),
            $"{macroEvent.DelayMs} ms",
            GetDetailText(macroEvent));

        _eventGrid.Rows[rowIndex].Tag = macroEvent;
    }

    private void EventGrid_SelectionChanged(object? sender, EventArgs e)
    {
        var selectedEvent = _eventGrid.SelectedRows.Count > 0
            ? _eventGrid.SelectedRows[0].Tag as MacroEvent
            : null;

        ShowEventDetails(selectedEvent);
    }

    private void ShowEventDetails(MacroEvent? macroEvent)
    {
        if (macroEvent is null)
        {
            _eventDetailBox.Text = "Bir olay secildiginde detaylar burada gorunecek.";
            return;
        }

        _eventDetailBox.Text = string.Join(
            Environment.NewLine,
            [
                $"Olay: {GetEventDisplayName(macroEvent)}",
                $"Tur: {GetEventTypeText(macroEvent)}",
                $"Aksiyon: {GetActionText(macroEvent)}",
                $"Gecikme: {macroEvent.DelayMs} ms",
                $"Zaman (UTC): {macroEvent.TimestampUtc:HH:mm:ss.fff}",
                $"Konum: {GetLocationText(macroEvent)}",
                $"Tus: {macroEvent.KeyName ?? "-"}",
                $"Aciklama: {GetDetailText(macroEvent)}"
            ]);
    }

    private void SelectEvent(MacroEvent macroEvent)
    {
        foreach (DataGridViewRow row in _eventGrid.Rows)
        {
            if (row.Tag is MacroEvent rowEvent && rowEvent.Id == macroEvent.Id)
            {
                _eventGrid.ClearSelection();
                row.Selected = true;
                if (row.Index >= 0)
                {
                    _eventGrid.FirstDisplayedScrollingRowIndex = row.Index;
                }
                break;
            }
        }
    }

    private int CalculateElapsedMs(int index)
    {
        var elapsed = 0;

        for (var i = 0; i <= index && i < _timelineEvents.Count; i++)
        {
            elapsed += _timelineEvents[i].DelayMs;
        }

        return elapsed;
    }

    private static string FormatElapsed(int totalMilliseconds)
    {
        return TimeSpan.FromMilliseconds(Math.Max(totalMilliseconds, 0)).ToString(@"mm\:ss\.fff");
    }

    private static string GetEventDisplayName(MacroEvent macroEvent)
    {
        return macroEvent.EventType switch
        {
            MacroEventType.Keyboard => macroEvent.KeyName is { Length: > 0 }
                ? $"{macroEvent.KeyboardActionType} ({macroEvent.KeyName})"
                : macroEvent.KeyboardActionType.ToString(),
            MacroEventType.Mouse => macroEvent.MouseActionType.ToString(),
            _ => "Sistem"
        };
    }

    private static string GetEventTypeText(MacroEvent macroEvent)
    {
        return macroEvent.EventType switch
        {
            MacroEventType.Keyboard => "Klavye",
            MacroEventType.Mouse => "Fare",
            _ => "Sistem"
        };
    }

    private static string GetActionText(MacroEvent macroEvent)
    {
        return macroEvent.EventType switch
        {
            MacroEventType.Keyboard => macroEvent.KeyboardActionType.ToString(),
            MacroEventType.Mouse => macroEvent.MouseActionType.ToString(),
            _ => macroEvent.Description
        };
    }

    private static string GetLocationText(MacroEvent macroEvent)
    {
        return macroEvent.X.HasValue && macroEvent.Y.HasValue
            ? $"X: {macroEvent.X}, Y: {macroEvent.Y}"
            : "-";
    }

    private static string GetDetailText(MacroEvent macroEvent)
    {
        if (!string.IsNullOrWhiteSpace(macroEvent.Description))
        {
            return macroEvent.Description;
        }

        if (macroEvent.EventType == MacroEventType.Keyboard)
        {
            return macroEvent.KeyName ?? "-";
        }

        if (macroEvent.WheelDelta.HasValue)
        {
            return $"Teker: {macroEvent.WheelDelta.Value}";
        }

        return "-";
    }
}
