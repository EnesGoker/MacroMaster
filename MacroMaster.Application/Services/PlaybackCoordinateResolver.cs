using MacroMaster.Application.Abstractions;
using MacroMaster.Domain.Enums;
using MacroMaster.Domain.Models;

namespace MacroMaster.Application.Services;

internal sealed class PlaybackCoordinateResolver : IDisposable
{
    private readonly object _syncRoot = new();
    private readonly SemaphoreSlim _anchorGate = new(1, 1);
    private readonly bool _useRelativeCoordinates;
    private readonly bool _useScreenScaledCoordinates;
    private readonly CursorPosition? _recordedMouseAnchor;
    private readonly RecordedScreenInfo? _recordedScreen;
    private readonly RecordedScreenInfo? _playbackScreen;

    private CursorPosition? _playbackMouseAnchor;
    private int? _anchoredIteration;
    private bool _disposed;

    private PlaybackCoordinateResolver(
        bool useRelativeCoordinates,
        CursorPosition? recordedMouseAnchor,
        bool useScreenScaledCoordinates,
        RecordedScreenInfo? recordedScreen,
        RecordedScreenInfo? playbackScreen)
    {
        _useRelativeCoordinates = useRelativeCoordinates;
        _recordedMouseAnchor = recordedMouseAnchor;
        _useScreenScaledCoordinates = useScreenScaledCoordinates;
        _recordedScreen = recordedScreen;
        _playbackScreen = playbackScreen;
    }

    private bool CanResolveRelativeCoordinates =>
        _useRelativeCoordinates && _recordedMouseAnchor.HasValue;

    private bool CanResolveScreenScaledCoordinates =>
        _useScreenScaledCoordinates && _recordedScreen is not null && _playbackScreen is not null;

    public static PlaybackCoordinateResolver Create(
        MacroSession session,
        PlaybackSettings settings,
        IRecordedScreenProvider? recordedScreenProvider = null)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(settings);

        bool useRelativeCoordinates = settings.UseRelativeCoordinates
            && !settings.SimulationMode;
        CursorPosition? recordedMouseAnchor = useRelativeCoordinates
            ? GetRecordedMouseAnchor(session)
            : null;
        bool hasMouseCoordinates = HasMouseCoordinates(session);
        bool useScreenScaledCoordinates = settings.UseScreenScaledCoordinates
            && hasMouseCoordinates;
        RecordedScreenInfo? recordedScreen = useScreenScaledCoordinates
            ? ValidateRecordedScreen(session.RecordedScreen, "kayit")
            : null;
        RecordedScreenInfo? playbackScreen = useScreenScaledCoordinates
            ? ValidateRecordedScreen(
                (recordedScreenProvider ?? NullRecordedScreenProvider.Instance).GetRecordedScreen(),
                "mevcut")
            : null;

        return new PlaybackCoordinateResolver(
            useRelativeCoordinates,
            recordedMouseAnchor,
            useScreenScaledCoordinates,
            recordedScreen,
            playbackScreen);
    }

    public async Task PrepareForIterationAsync(
        int iteration,
        ICursorPositionProvider cursorPositionProvider,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(cursorPositionProvider);
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        if (!CanResolveRelativeCoordinates)
        {
            return;
        }

        int safeIteration = Math.Max(iteration, 0);

        await _anchorGate.WaitAsync(cancellationToken);

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            lock (_syncRoot)
            {
                if (_anchoredIteration == safeIteration
                    && _playbackMouseAnchor.HasValue)
                {
                    return;
                }
            }

            CursorPosition playbackMouseAnchor =
                await cursorPositionProvider.GetCursorPositionAsync(cancellationToken);

            lock (_syncRoot)
            {
                _playbackMouseAnchor = playbackMouseAnchor;
                _anchoredIteration = safeIteration;
            }
        }
        finally
        {
            _anchorGate.Release();
        }
    }

    public Task PrepareForLogicalEventAsync(
        int logicalEventIndex,
        int eventCount,
        ICursorPositionProvider cursorPositionProvider,
        CancellationToken cancellationToken)
    {
        if (eventCount <= 0)
        {
            return Task.CompletedTask;
        }

        int safeLogicalEventIndex = Math.Max(logicalEventIndex, 0);
        int iteration = safeLogicalEventIndex / eventCount;

        return PrepareForIterationAsync(
            iteration,
            cursorPositionProvider,
            cancellationToken);
    }

    public MacroEvent Resolve(MacroEvent macroEvent)
    {
        ArgumentNullException.ThrowIfNull(macroEvent);
        ThrowIfDisposed();

        CursorPosition? playbackMouseAnchor;

        lock (_syncRoot)
        {
            playbackMouseAnchor = _playbackMouseAnchor;
        }

        if (macroEvent.EventType != MacroEventType.Mouse
            || !macroEvent.X.HasValue
            || !macroEvent.Y.HasValue)
        {
            return macroEvent;
        }

        if (CanResolveRelativeCoordinates && playbackMouseAnchor is not null)
        {
            int resolvedRelativeX = ResolveRelativeCoordinate(
                macroEvent.X.Value,
                _recordedMouseAnchor!.Value.X,
                playbackMouseAnchor.Value.X);
            int resolvedRelativeY = ResolveRelativeCoordinate(
                macroEvent.Y.Value,
                _recordedMouseAnchor.Value.Y,
                playbackMouseAnchor.Value.Y);

            return CloneMacroEventWithCoordinates(macroEvent, resolvedRelativeX, resolvedRelativeY);
        }

        if (CanResolveScreenScaledCoordinates)
        {
            int resolvedScaledX = ResolveScreenScaledCoordinate(
                macroEvent.X.Value,
                _recordedScreen!.Width,
                _playbackScreen!.Width,
                "X");
            int resolvedScaledY = ResolveScreenScaledCoordinate(
                macroEvent.Y.Value,
                _recordedScreen.Height,
                _playbackScreen.Height,
                "Y");

            return CloneMacroEventWithCoordinates(macroEvent, resolvedScaledX, resolvedScaledY);
        }

        return macroEvent;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _anchorGate.Dispose();
        _disposed = true;
    }

    private static CursorPosition? GetRecordedMouseAnchor(MacroSession session)
    {
        foreach (MacroEvent macroEvent in session.Events)
        {
            if (macroEvent.EventType == MacroEventType.Mouse
                && macroEvent.X.HasValue
                && macroEvent.Y.HasValue)
            {
                return new CursorPosition(macroEvent.X.Value, macroEvent.Y.Value);
            }
        }

        return null;
    }

    private static bool HasMouseCoordinates(MacroSession session)
    {
        foreach (MacroEvent macroEvent in session.Events)
        {
            if (macroEvent.EventType == MacroEventType.Mouse
                && macroEvent.X.HasValue
                && macroEvent.Y.HasValue)
            {
                return true;
            }
        }

        return false;
    }

    private static RecordedScreenInfo ValidateRecordedScreen(
        RecordedScreenInfo? recordedScreenInfo,
        string sourceName)
    {
        if (recordedScreenInfo is null
            || recordedScreenInfo.Width <= 0
            || recordedScreenInfo.Height <= 0)
        {
            throw new InvalidOperationException(
                $"Ekrana gore koordinat olcekleme icin {sourceName} ekran boyutu gecersiz veya eksik.");
        }

        return new RecordedScreenInfo
        {
            Width = recordedScreenInfo.Width,
            Height = recordedScreenInfo.Height
        };
    }

    private static int ResolveRelativeCoordinate(
        int recordedCoordinate,
        int recordedAnchor,
        int playbackAnchor)
    {
        long relativeOffset = (long)recordedCoordinate - recordedAnchor;
        long resolvedCoordinate = playbackAnchor + relativeOffset;

        if (resolvedCoordinate < int.MinValue || resolvedCoordinate > int.MaxValue)
        {
            throw new InvalidOperationException(
                $"Goreli fare oynatimi gecersiz bir koordinat uretti: {resolvedCoordinate}.");
        }

        return (int)resolvedCoordinate;
    }

    private static int ResolveScreenScaledCoordinate(
        int recordedCoordinate,
        int recordedDimension,
        int playbackDimension,
        string axisName)
    {
        if (recordedCoordinate < 0 || recordedCoordinate >= recordedDimension)
        {
            throw new InvalidOperationException(
                $"Ekrana gore koordinat olcekleme kayit {axisName} koordinatini ekran sinirlari disinda buldu: {recordedCoordinate}. Kayit boyutu: {recordedDimension}.");
        }

        double resolvedCoordinate = Math.Floor(
            (double)recordedCoordinate * playbackDimension / recordedDimension);

        if (double.IsNaN(resolvedCoordinate)
            || double.IsInfinity(resolvedCoordinate)
            || resolvedCoordinate < 0
            || resolvedCoordinate >= playbackDimension
            || resolvedCoordinate > int.MaxValue)
        {
            throw new InvalidOperationException(
                $"Ekrana gore koordinat olcekleme mevcut ekran sinirlari disinda {axisName} koordinati uretti: {resolvedCoordinate}. Mevcut boyut: {playbackDimension}.");
        }

        return (int)resolvedCoordinate;
    }

    private static MacroEvent CloneMacroEventWithCoordinates(
        MacroEvent source,
        int x,
        int y)
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
            X = x,
            Y = y,
            WheelDelta = source.WheelDelta,
            Description = source.Description
        };
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }
}
