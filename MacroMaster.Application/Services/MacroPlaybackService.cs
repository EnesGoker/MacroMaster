using MacroMaster.Application.Abstractions;
using MacroMaster.Domain.Enums;
using MacroMaster.Domain.Models;

namespace MacroMaster.Application.Services;

public sealed class MacroPlaybackService : IMacroPlaybackService
{
    private readonly IInputPlaybackAdapter _inputPlaybackAdapter;
    private readonly ICursorPositionProvider _cursorPositionProvider;
    private readonly IApplicationStateService _applicationStateService;
    private readonly IAppLogger _logger;
    private readonly object _syncRoot = new();

    private bool _stopRequested;
    private CancellationTokenSource? _playbackCancellationTokenSource;
    private TaskCompletionSource<bool> _resumeSignal = CreateResumeSignal(signaled: true);

    public MacroPlaybackService(
        IInputPlaybackAdapter inputPlaybackAdapter,
        ICursorPositionProvider cursorPositionProvider,
        IApplicationStateService applicationStateService,
        IAppLogger? logger = null)
    {
        _inputPlaybackAdapter = inputPlaybackAdapter;
        _cursorPositionProvider = cursorPositionProvider;
        _applicationStateService = applicationStateService;
        _logger = logger ?? NullAppLogger.Instance;
    }

    public bool IsPlaying => _applicationStateService.IsState(AppState.Playing);
    public bool IsPaused => _applicationStateService.IsState(AppState.Paused);

    public event Action? PlaybackStarted;
    public event Action? PlaybackPaused;
    public event Action? PlaybackResumed;
    public event Action? PlaybackStopped;
    public event Action<MacroEvent>? EventPlayed;

    public async Task PlayAsync(
        MacroSession session,
        PlaybackSettings settings,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(settings);

        if (session.Events.Count == 0)
        {
            return;
        }

        if (!_applicationStateService.TryTransitionTo(AppState.Playing, AppState.Idle))
        {
            return;
        }

        PlaybackSettings effectiveSettings = NormalizeSettingsForExecution(settings);
        CancellationTokenSource? playbackCancellationTokenSource = null;
        bool playbackStarted = false;
        List<Exception> playbackErrors = [];
        CursorPosition? recordedMouseAnchor = effectiveSettings.UseRelativeCoordinates
            ? GetRecordedMouseAnchor(session)
            : null;

        try
        {
            lock (_syncRoot)
            {
                if (_playbackCancellationTokenSource is not null)
                {
                    _applicationStateService.TryTransitionTo(AppState.Idle, AppState.Playing);
                    return;
                }

                _stopRequested = false;
                _resumeSignal = CreateResumeSignal(signaled: true);
                _playbackCancellationTokenSource =
                    CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                playbackCancellationTokenSource = _playbackCancellationTokenSource;
            }

            playbackStarted = true;
            _logger.Log(
                AppLogLevel.Information,
                nameof(MacroPlaybackService),
                $"Oynatma baslatildi. Oturum: {session.Name}, olay sayisi: {session.Events.Count}.");
            SafeInvokePlaybackStarted();

            CancellationToken playbackCancellationToken =
                playbackCancellationTokenSource.Token;

            if (effectiveSettings.InitialDelayMs > 0)
            {
                await Task.Delay(effectiveSettings.InitialDelayMs, playbackCancellationToken);
            }

            playbackCancellationToken.ThrowIfCancellationRequested();

            int repeatCount = effectiveSettings.LoopIndefinitely
                ? int.MaxValue
                : Math.Max(effectiveSettings.RepeatCount, 1);

            for (int iteration = 0; iteration < repeatCount; iteration++)
            {
                CursorPosition? playbackMouseAnchor = recordedMouseAnchor.HasValue
                    ? await _cursorPositionProvider.GetCursorPositionAsync(playbackCancellationToken)
                    : null;

                for (int eventIndex = 0; eventIndex < session.Events.Count; eventIndex++)
                {
                    MacroEvent macroEvent = session.Events[eventIndex];
                    MacroEvent playbackEvent = ResolvePlaybackEvent(
                        macroEvent,
                        recordedMouseAnchor,
                        playbackMouseAnchor);

                    playbackCancellationToken.ThrowIfCancellationRequested();
                    ThrowIfStopRequested();
                    await WaitIfPausedAsync(playbackCancellationToken);

                    int delayMs = ResolveDelayMs(playbackEvent, effectiveSettings);

                    if (delayMs > 0)
                    {
                        await Task.Delay(delayMs, playbackCancellationToken);
                    }

                    playbackCancellationToken.ThrowIfCancellationRequested();
                    ThrowIfStopRequested();

                    try
                    {
                        await _inputPlaybackAdapter.PlayEventAsync(
                            playbackEvent,
                            playbackCancellationToken);

                        SafeInvokeEventPlayed(playbackEvent);
                    }
                    catch (OperationCanceledException)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        Exception playbackException = CreatePlaybackException(
                            iteration,
                            eventIndex,
                            playbackEvent,
                            ex);

                        _logger.Log(
                            AppLogLevel.Error,
                            nameof(MacroPlaybackService),
                            "Olay oynatilirken hata olustu.",
                            playbackException);

                        if (effectiveSettings.StopOnError)
                        {
                            throw playbackException;
                        }

                        playbackErrors.Add(playbackException);
                    }
                }
            }

            if (playbackErrors.Count > 0)
            {
                throw new AggregateException(
                    "Oynatma bir veya daha fazla olay hatasiyla tamamlandi.",
                    playbackErrors);
            }
        }
        catch (OperationCanceledException) when (_stopRequested || cancellationToken.IsCancellationRequested)
        {
            _logger.Log(
                AppLogLevel.Information,
                nameof(MacroPlaybackService),
                _stopRequested
                    ? "Oynatma kullanici istegiyle durduruldu."
                    : "Oynatma cagiran iptal belirteci tarafindan iptal edildi.");
        }
        catch (Exception ex)
        {
            TrySetErrorState();
            _logger.Log(
                AppLogLevel.Error,
                nameof(MacroPlaybackService),
                "Oynatma hata nedeniyle sonlandirildi.",
                ex);
            throw;
        }
        finally
        {
            lock (_syncRoot)
            {
                if (ReferenceEquals(
                        _playbackCancellationTokenSource,
                        playbackCancellationTokenSource))
                {
                    _playbackCancellationTokenSource = null;
                }

                _stopRequested = false;
                _resumeSignal.TrySetResult(true);
                _resumeSignal = CreateResumeSignal(signaled: true);
            }

            playbackCancellationTokenSource?.Dispose();

            if (playbackStarted
                || _applicationStateService.IsAny(
                    AppState.Playing,
                    AppState.Paused,
                    AppState.Stopping,
                    AppState.Error))
            {
                _applicationStateService.TryTransitionTo(
                    AppState.Idle,
                    AppState.Playing,
                    AppState.Paused,
                    AppState.Stopping,
                    AppState.Error);
                SafeInvokePlaybackStopped();
            }
        }
    }

    public Task PauseAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_syncRoot)
        {
            if (_playbackCancellationTokenSource is null)
            {
                return Task.CompletedTask;
            }
        }

        if (!_applicationStateService.TryTransitionTo(AppState.Paused, AppState.Playing))
        {
            return Task.CompletedTask;
        }

        lock (_syncRoot)
        {
            if (_resumeSignal.Task.IsCompleted)
            {
                _resumeSignal = CreateResumeSignal(signaled: false);
            }
        }

        _logger.Log(
            AppLogLevel.Information,
            nameof(MacroPlaybackService),
            "Oynatma duraklatildi.");
        SafeInvokePlaybackPaused();
        return Task.CompletedTask;
    }

    public Task ResumeAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        TaskCompletionSource<bool>? resumeSignal;

        lock (_syncRoot)
        {
            if (_playbackCancellationTokenSource is null)
            {
                return Task.CompletedTask;
            }

            resumeSignal = _resumeSignal;
        }

        if (!_applicationStateService.TryTransitionTo(AppState.Playing, AppState.Paused))
        {
            return Task.CompletedTask;
        }

        resumeSignal.TrySetResult(true);
        _logger.Log(
            AppLogLevel.Information,
            nameof(MacroPlaybackService),
            "Oynatma devam ettirildi.");
        SafeInvokePlaybackResumed();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        CancellationTokenSource? playbackCancellationTokenSource;
        TaskCompletionSource<bool>? resumeSignal;

        lock (_syncRoot)
        {
            if (_playbackCancellationTokenSource is null
                && !_applicationStateService.IsAny(AppState.Playing, AppState.Paused))
            {
                return Task.CompletedTask;
            }

            _stopRequested = true;
            playbackCancellationTokenSource = _playbackCancellationTokenSource;
            resumeSignal = _resumeSignal;
        }

        _applicationStateService.TryTransitionTo(
            AppState.Stopping,
            AppState.Playing,
            AppState.Paused);

        resumeSignal?.TrySetResult(true);
        playbackCancellationTokenSource?.Cancel();
        _logger.Log(
            AppLogLevel.Information,
            nameof(MacroPlaybackService),
            "Oynatma durdurma istegi alindi.");

        return Task.CompletedTask;
    }

    private async Task WaitIfPausedAsync(CancellationToken cancellationToken)
    {
        Task waitTask;

        lock (_syncRoot)
        {
            if (!_applicationStateService.IsState(AppState.Paused))
            {
                return;
            }

            waitTask = _resumeSignal.Task;
        }

        await waitTask.WaitAsync(cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfStopRequested();
    }

    private void TrySetErrorState()
    {
        try
        {
            _applicationStateService.TryTransitionTo(
                AppState.Error,
                AppState.Playing,
                AppState.Paused,
                AppState.Stopping);
        }
        catch (Exception ex)
        {
            _logger.Log(
                AppLogLevel.Error,
                nameof(MacroPlaybackService),
                "Oynatma hata durumuna gecirilirken ek bir hata olustu.",
                ex);
        }
    }

    private void ThrowIfStopRequested()
    {
        if (_stopRequested)
        {
            throw new OperationCanceledException("Oynatma icin durdurma istegi alindi.");
        }
    }

    private static PlaybackSettings NormalizeSettingsForExecution(PlaybackSettings settings)
    {
        return new PlaybackSettings
        {
            SpeedMultiplier = settings.PreserveOriginalTiming
                ? 1.0
                : settings.SpeedMultiplier > 0
                    ? settings.SpeedMultiplier
                    : 1.0,
            RepeatCount = Math.Max(settings.RepeatCount, 1),
            InitialDelayMs = Math.Max(settings.InitialDelayMs, 0),
            LoopIndefinitely = settings.LoopIndefinitely,
            UseRelativeCoordinates = settings.UseRelativeCoordinates,
            StopOnError = settings.StopOnError,
            PreserveOriginalTiming = settings.PreserveOriginalTiming
        };
    }

    private static int ResolveDelayMs(
        MacroEvent playbackEvent,
        PlaybackSettings settings)
    {
        return settings.PreserveOriginalTiming
            ? Math.Max(playbackEvent.DelayMs, 0)
            : ConvertDelayBySpeed(playbackEvent.DelayMs, settings.SpeedMultiplier);
    }

    private static int ConvertDelayBySpeed(int originalDelayMs, double speedMultiplier)
    {
        if (originalDelayMs <= 0)
        {
            return 0;
        }

        if (speedMultiplier <= 0)
        {
            speedMultiplier = 1.0;
        }

        return (int)Math.Max(originalDelayMs / speedMultiplier, 0);
    }

    private static InvalidOperationException CreatePlaybackException(
        int iteration,
        int eventIndex,
        MacroEvent macroEvent,
        Exception innerException)
    {
        string eventDescription = string.IsNullOrWhiteSpace(macroEvent.Description)
            ? FormatEventType(macroEvent.EventType)
            : macroEvent.Description;

        return new InvalidOperationException(
            $"Oynatma {iteration + 1}. tekrarin {eventIndex + 1}. olayinda basarisiz oldu: {eventDescription}.",
            innerException);
    }

    private static string FormatEventType(MacroEventType eventType)
    {
        return eventType switch
        {
            MacroEventType.Keyboard => "klavye",
            MacroEventType.Mouse => "fare",
            MacroEventType.System => "sistem",
            _ => eventType.ToString()
        };
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

    private static MacroEvent ResolvePlaybackEvent(
        MacroEvent macroEvent,
        CursorPosition? recordedMouseAnchor,
        CursorPosition? playbackMouseAnchor)
    {
        if (recordedMouseAnchor is null
            || playbackMouseAnchor is null
            || macroEvent.EventType != MacroEventType.Mouse
            || !macroEvent.X.HasValue
            || !macroEvent.Y.HasValue)
        {
            return macroEvent;
        }

        int resolvedX = ResolveRelativeCoordinate(
            macroEvent.X.Value,
            recordedMouseAnchor.Value.X,
            playbackMouseAnchor.Value.X);
        int resolvedY = ResolveRelativeCoordinate(
            macroEvent.Y.Value,
            recordedMouseAnchor.Value.Y,
            playbackMouseAnchor.Value.Y);

        return CloneMacroEventWithCoordinates(macroEvent, resolvedX, resolvedY);
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

    private static TaskCompletionSource<bool> CreateResumeSignal(bool signaled)
    {
        TaskCompletionSource<bool> taskCompletionSource = new(
            TaskCreationOptions.RunContinuationsAsynchronously);

        if (signaled)
        {
            taskCompletionSource.TrySetResult(true);
        }

        return taskCompletionSource;
    }

    private void SafeInvokePlaybackStarted()
    {
        try
        {
            PlaybackStarted?.Invoke();
        }
        catch (Exception ex)
        {
            _logger.Log(
                AppLogLevel.Error,
                nameof(MacroPlaybackService),
                "Oynatma basladi bildirimi yayinlanirken hata olustu.",
                ex);
        }
    }

    private void SafeInvokePlaybackPaused()
    {
        try
        {
            PlaybackPaused?.Invoke();
        }
        catch (Exception ex)
        {
            _logger.Log(
                AppLogLevel.Error,
                nameof(MacroPlaybackService),
                "Oynatma duraklatildi bildirimi yayinlanirken hata olustu.",
                ex);
        }
    }

    private void SafeInvokePlaybackResumed()
    {
        try
        {
            PlaybackResumed?.Invoke();
        }
        catch (Exception ex)
        {
            _logger.Log(
                AppLogLevel.Error,
                nameof(MacroPlaybackService),
                "Oynatma devam etti bildirimi yayinlanirken hata olustu.",
                ex);
        }
    }

    private void SafeInvokePlaybackStopped()
    {
        try
        {
            PlaybackStopped?.Invoke();
        }
        catch (Exception ex)
        {
            _logger.Log(
                AppLogLevel.Error,
                nameof(MacroPlaybackService),
                "Oynatma durdu bildirimi yayinlanirken hata olustu.",
                ex);
        }
    }

    private void SafeInvokeEventPlayed(MacroEvent playbackEvent)
    {
        try
        {
            EventPlayed?.Invoke(playbackEvent);
        }
        catch (Exception ex)
        {
            _logger.Log(
                AppLogLevel.Error,
                nameof(MacroPlaybackService),
                "Olay oynatildi bildirimi yayinlanirken hata olustu.",
                ex);
        }
    }
}
