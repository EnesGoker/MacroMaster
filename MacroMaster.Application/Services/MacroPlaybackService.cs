using MacroMaster.Application.Abstractions;
using MacroMaster.Domain.Enums;
using MacroMaster.Domain.Models;

namespace MacroMaster.Application.Services;

public sealed class MacroPlaybackService : IMacroPlaybackService
{
    private readonly IInputPlaybackAdapter _inputPlaybackAdapter;
    private readonly IApplicationStateService _applicationStateService;

    private bool _isPaused;
    private bool _stopRequested;

    public MacroPlaybackService(
        IInputPlaybackAdapter inputPlaybackAdapter,
        IApplicationStateService applicationStateService)
    {
        _inputPlaybackAdapter = inputPlaybackAdapter;
        _applicationStateService = applicationStateService;
    }

    public bool IsPlaying => _applicationStateService.Is(AppState.Playing);
    public bool IsPaused => _isPaused;

    public event Action? PlaybackStarted;
    public event Action? PlaybackPaused;
    public event Action? PlaybackResumed;
    public event Action? PlaybackStopped;
    public event Action<MacroEvent>? EventPlayed;

    public async Task PlayAsync( MacroSession session, PlaybackSettings settings, CancellationToken cancellationToken = default)
    {
        if (session.Events.Count == 0 || IsPlaying)
        {
            return;
        }

        _stopRequested = false;
        _isPaused = false;

        if (settings.InitialDelayMs > 0)
        {
            await Task.Delay(settings.InitialDelayMs, cancellationToken);
        }

        _applicationStateService.SetState(AppState.Playing);
        PlaybackStarted?.Invoke();

        var repeatCount = settings.LoopIndefinitely
            ? int.MaxValue
            : Math.Max(settings.RepeatCount, 1);

        for (var iteration = 0; iteration < repeatCount; iteration++)
        {
            foreach (var macroEvent in session.Events)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (_stopRequested)
                {
                    _applicationStateService.SetState(AppState.Idle);
                    PlaybackStopped?.Invoke();
                    return;
                }

                while (_isPaused)
                {
                    await Task.Delay(50, cancellationToken);
                }

                var delayMs = settings.PreserveOriginalTiming
                    ? macroEvent.DelayMs
                    : ConvertDelayBySpeed(macroEvent.DelayMs, settings.SpeedMultiplier);

                if (delayMs > 0)
                {
                    await Task.Delay(delayMs, cancellationToken);
                }

                await _inputPlaybackAdapter.PlayEventAsync(macroEvent, cancellationToken);
                EventPlayed?.Invoke(macroEvent);
            }
        }

        _applicationStateService.SetState(AppState.Idle);
        PlaybackStopped?.Invoke();
    }

    public Task PauseAsync(CancellationToken cancellationToken = default)
    {
        if (!IsPlaying || _isPaused)
        {
            return Task.CompletedTask;
        }

        _isPaused = true;
        _applicationStateService.SetState(AppState.Paused);
        PlaybackPaused?.Invoke();

        return Task.CompletedTask;
    }

    public Task ResumeAsync(CancellationToken cancellationToken = default)
    {
        if (!_isPaused)
        {
            return Task.CompletedTask;
        }

        _isPaused = false;
        _applicationStateService.SetState(AppState.Playing);
        PlaybackResumed?.Invoke();

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        _stopRequested = true;
        _isPaused = false;
        _applicationStateService.SetState(AppState.Stopping);

        return Task.CompletedTask;
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
}