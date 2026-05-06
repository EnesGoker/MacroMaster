using MacroMaster.Domain.Enums;
using MacroMaster.Domain.Models;
using System.Globalization;

namespace MacroMaster.WinForms.Controls;

internal static class EventListFilterEngine
{
    internal const int LongDelayThresholdMs = 250;
    internal const int OptimizationCandidateMaxDelayMs = 100;

    public static EventListViewItem[] Apply(
        IReadOnlyList<MacroEvent> events,
        EventListFilterCriteria criteria)
    {
        ArgumentNullException.ThrowIfNull(events);

        var items = new List<EventListViewItem>(events.Count);
        int elapsedMs = 0;

        for (int index = 0; index < events.Count; index++)
        {
            MacroEvent macroEvent = events[index];
            elapsedMs += Math.Max(0, macroEvent.DelayMs);

            if (Matches(events, index, elapsedMs, criteria))
            {
                items.Add(new EventListViewItem(index, macroEvent, elapsedMs));
            }
        }

        return items.ToArray();
    }

    public static bool Matches(
        IReadOnlyList<MacroEvent> events,
        int sourceIndex,
        int elapsedMs,
        EventListFilterCriteria criteria)
    {
        ArgumentNullException.ThrowIfNull(events);

        if (sourceIndex < 0 || sourceIndex >= events.Count)
        {
            return false;
        }

        MacroEvent macroEvent = events[sourceIndex];

        return MatchesType(macroEvent, criteria.TypeFilter)
            && MatchesSmartFilter(events, sourceIndex, criteria.SmartFilter)
            && MatchesSearch(macroEvent, sourceIndex, elapsedMs, criteria.SearchTerm);
    }

    private static bool MatchesType(
        MacroEvent macroEvent,
        EventListTypeFilterKind typeFilter)
    {
        return typeFilter switch
        {
            EventListTypeFilterKind.All => true,
            EventListTypeFilterKind.Keyboard => macroEvent.EventType == MacroEventType.Keyboard,
            EventListTypeFilterKind.Mouse => macroEvent.EventType == MacroEventType.Mouse,
            EventListTypeFilterKind.System => macroEvent.EventType == MacroEventType.System,
            _ => true
        };
    }

    private static bool MatchesSmartFilter(
        IReadOnlyList<MacroEvent> events,
        int sourceIndex,
        EventListSmartFilterKind smartFilter)
    {
        MacroEvent macroEvent = events[sourceIndex];

        return smartFilter switch
        {
            EventListSmartFilterKind.All => true,
            EventListSmartFilterKind.KeyboardOnly => macroEvent.EventType == MacroEventType.Keyboard,
            EventListSmartFilterKind.MouseMoves => IsMouseMove(macroEvent),
            EventListSmartFilterKind.MouseClicks => IsMouseClick(macroEvent),
            EventListSmartFilterKind.LongDelays => macroEvent.DelayMs >= LongDelayThresholdMs,
            EventListSmartFilterKind.OptimizationCandidates => IsOptimizationCandidate(events, sourceIndex),
            EventListSmartFilterKind.InvalidOrIncomplete => IsInvalidOrIncomplete(macroEvent),
            _ => true
        };
    }

    private static bool MatchesSearch(
        MacroEvent macroEvent,
        int sourceIndex,
        int elapsedMs,
        string? searchTerm)
    {
        string[] searchTokens = Tokenize(searchTerm);

        if (searchTokens.Length == 0)
        {
            return true;
        }

        string rowNumber = (sourceIndex + 1).ToString("000", CultureInfo.InvariantCulture);
        string[] searchableValues =
        [
            rowNumber,
            (sourceIndex + 1).ToString(CultureInfo.InvariantCulture),
            FormatElapsedTime(elapsedMs),
            FormatEventType(macroEvent.EventType),
            macroEvent.EventType.ToString(),
            FormatAction(macroEvent),
            FormatPosition(macroEvent),
            macroEvent.DelayMs.ToString(CultureInfo.InvariantCulture),
            FormattableString.Invariant($"{macroEvent.DelayMs} ms"),
            macroEvent.Description ?? string.Empty,
            macroEvent.KeyName ?? string.Empty,
            macroEvent.KeyCode?.ToString(CultureInfo.InvariantCulture) ?? string.Empty,
            macroEvent.ScanCode?.ToString(CultureInfo.InvariantCulture) ?? string.Empty,
            macroEvent.X?.ToString(CultureInfo.InvariantCulture) ?? string.Empty,
            macroEvent.Y?.ToString(CultureInfo.InvariantCulture) ?? string.Empty,
            macroEvent.WheelDelta?.ToString(CultureInfo.InvariantCulture) ?? string.Empty
        ];

        return searchTokens.All(token => searchableValues.Any(value =>
            value.Contains(token, StringComparison.CurrentCultureIgnoreCase)));
    }

    private static bool IsOptimizationCandidate(
        IReadOnlyList<MacroEvent> events,
        int sourceIndex)
    {
        if (sourceIndex <= 0 || sourceIndex >= events.Count - 1)
        {
            return false;
        }

        MacroEvent macroEvent = events[sourceIndex];

        return IsMouseMove(macroEvent)
            && macroEvent.DelayMs <= OptimizationCandidateMaxDelayMs
            && IsMouseMove(events[sourceIndex - 1])
            && IsMouseMove(events[sourceIndex + 1]);
    }

    private static bool IsInvalidOrIncomplete(MacroEvent macroEvent)
    {
        return macroEvent.EventType switch
        {
            MacroEventType.Keyboard => macroEvent.KeyboardActionType == KeyboardActionType.None
                || (!macroEvent.KeyCode.HasValue && !macroEvent.ScanCode.HasValue),
            MacroEventType.Mouse => macroEvent.MouseActionType == MouseActionType.None
                || (RequiresCoordinates(macroEvent.MouseActionType)
                    && (!macroEvent.X.HasValue || !macroEvent.Y.HasValue))
                || (macroEvent.MouseActionType == MouseActionType.Wheel
                    && !macroEvent.WheelDelta.HasValue),
            _ => false
        };
    }

    private static bool IsMouseMove(MacroEvent macroEvent)
    {
        return macroEvent.EventType == MacroEventType.Mouse
            && macroEvent.MouseActionType == MouseActionType.Move;
    }

    private static bool IsMouseClick(MacroEvent macroEvent)
    {
        return macroEvent.EventType == MacroEventType.Mouse
            && macroEvent.MouseActionType is MouseActionType.LeftDown
                or MouseActionType.LeftUp
                or MouseActionType.RightDown
                or MouseActionType.RightUp
                or MouseActionType.MiddleDown
                or MouseActionType.MiddleUp
                or MouseActionType.DoubleClick;
    }

    private static bool RequiresCoordinates(MouseActionType mouseActionType)
    {
        return mouseActionType is MouseActionType.Move
            or MouseActionType.LeftDown
            or MouseActionType.LeftUp
            or MouseActionType.RightDown
            or MouseActionType.RightUp
            or MouseActionType.MiddleDown
            or MouseActionType.MiddleUp
            or MouseActionType.DoubleClick;
    }

    private static string[] Tokenize(string? searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return [];
        }

        return searchTerm
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(token => !string.IsNullOrWhiteSpace(token))
            .ToArray();
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
            _ => macroEvent.Description ?? string.Empty
        };
    }

    private static string FormatPosition(MacroEvent macroEvent)
    {
        if (macroEvent.X.HasValue && macroEvent.Y.HasValue)
        {
            return FormattableString.Invariant($"X: {macroEvent.X}, Y: {macroEvent.Y}");
        }

        return string.Empty;
    }
}
