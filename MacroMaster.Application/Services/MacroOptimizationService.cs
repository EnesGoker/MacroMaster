using MacroMaster.Application.Abstractions;
using MacroMaster.Domain.Enums;
using MacroMaster.Domain.Models;

namespace MacroMaster.Application.Services;

public sealed class MacroOptimizationService : IMacroOptimizationService
{
    public const int LongDelayThresholdMs = 250;
    public const int RedundantMouseMoveMaxDelayMs = 100;

    public MacroOptimizationPreview Preview(MacroSession session)
    {
        ArgumentNullException.ThrowIfNull(session);

        var optimizedEvents = new List<MacroEvent>(session.Events.Count);
        long absorbedDelayMs = 0;
        int removedEventCount = 0;

        for (int index = 0; index < session.Events.Count; index++)
        {
            MacroEvent macroEvent = session.Events[index];

            if (ShouldKeepEvent(session.Events, index))
            {
                MacroEvent keptEvent = CloneMacroEvent(macroEvent);
                keptEvent.DelayMs = AddDelaySafely(keptEvent.DelayMs, absorbedDelayMs);
                optimizedEvents.Add(keptEvent);
                absorbedDelayMs = 0;
                continue;
            }

            absorbedDelayMs += Math.Max(0, macroEvent.DelayMs);
            removedEventCount++;
        }

        if (absorbedDelayMs > 0 && optimizedEvents.Count > 0)
        {
            MacroEvent lastEvent = optimizedEvents[^1];
            lastEvent.DelayMs = AddDelaySafely(lastEvent.DelayMs, absorbedDelayMs);
        }

        int originalDurationMs = CalculateTotalDurationMs(session.Events);
        int optimizedDurationMs = CalculateTotalDurationMs(optimizedEvents);

        return new MacroOptimizationPreview(
            session.Name,
            session.Events.Count,
            optimizedEvents.Count,
            removedEventCount,
            originalDurationMs,
            optimizedDurationMs,
            optimizedEvents);
    }

    private static bool ShouldKeepEvent(
        IReadOnlyList<MacroEvent> events,
        int index)
    {
        MacroEvent macroEvent = events[index];

        if (!IsMouseMove(macroEvent))
        {
            return true;
        }

        if (!macroEvent.X.HasValue || !macroEvent.Y.HasValue)
        {
            return true;
        }

        if (macroEvent.DelayMs >= LongDelayThresholdMs)
        {
            return true;
        }

        if (macroEvent.DelayMs > RedundantMouseMoveMaxDelayMs)
        {
            return true;
        }

        bool previousIsMove = index > 0 && IsMouseMove(events[index - 1]);
        bool nextIsMove = index < events.Count - 1 && IsMouseMove(events[index + 1]);

        // Keep movement segment boundaries: the first point and the last point before
        // a click, wheel, keyboard event, or any other non-move action.
        return !previousIsMove || !nextIsMove;
    }

    private static bool IsMouseMove(MacroEvent macroEvent)
    {
        return macroEvent.EventType == MacroEventType.Mouse
            && macroEvent.MouseActionType == MouseActionType.Move;
    }

    private static int CalculateTotalDurationMs(IEnumerable<MacroEvent> events)
    {
        long totalDurationMs = events.Sum(macroEvent => (long)Math.Max(0, macroEvent.DelayMs));
        return totalDurationMs > int.MaxValue
            ? int.MaxValue
            : (int)totalDurationMs;
    }

    private static int AddDelaySafely(int originalDelayMs, long absorbedDelayMs)
    {
        long totalDelayMs = Math.Max(0, originalDelayMs) + Math.Max(0, absorbedDelayMs);
        return totalDelayMs > int.MaxValue
            ? int.MaxValue
            : (int)totalDelayMs;
    }

    private static MacroEvent CloneMacroEvent(MacroEvent source)
    {
        return new MacroEvent
        {
            Id = source.Id,
            EventType = source.EventType,
            KeyboardActionType = source.KeyboardActionType,
            MouseActionType = source.MouseActionType,
            DelayMs = source.DelayMs,
            TimestampUtc = source.TimestampUtc,
            KeyCode = source.KeyCode,
            ScanCode = source.ScanCode,
            IsExtendedKey = source.IsExtendedKey,
            KeyName = source.KeyName,
            X = source.X,
            Y = source.Y,
            WheelDelta = source.WheelDelta,
            Description = source.Description
        };
    }
}
