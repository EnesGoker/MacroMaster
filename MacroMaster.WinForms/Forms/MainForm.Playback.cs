using MacroMaster.Domain.Enums;
using MacroMaster.Domain.Models;
using MacroMaster.Application.Abstractions;

namespace MacroMaster.WinForms.Forms;

public partial class MainForm
{
    private async Task HandlePlaybackToggleAsync()
    {
        if (_shutdownInProgress || _macroRecorderService.IsRecording)
        {
            return;
        }

        if (_macroPlaybackService.IsPaused)
        {
            await _macroPlaybackService.ResumeAsync();
            return;
        }

        if (_macroPlaybackService.IsPlaying)
        {
            await _macroPlaybackService.PauseAsync();
            return;
        }

        if (GetSessionForPlayback() is { Events.Count: > 0 } currentSession)
        {
            PlaybackSettings? playbackSettings = ResolvePlaybackSettingsForStart(currentSession);

            if (playbackSettings is null)
            {
                return;
            }

            await _macroPlaybackService.PlayAsync(currentSession, playbackSettings);
        }
    }

    private async Task HandleStopAsync()
    {
        if (_macroRecorderService.IsRecording)
        {
            await _macroRecorderService.StopAsync();
        }

        if (_macroPlaybackService.IsPlaying || _macroPlaybackService.IsPaused)
        {
            await _macroPlaybackService.StopAsync();
        }
    }

    private async Task ResetPlaybackCursorAsync()
    {
        if (Interlocked.Exchange(ref _playbackNavigationInProgress, 1) == 1)
        {
            return;
        }

        try
        {
            MacroSession? session = GetSessionForPlayback();
            PlaybackSettings playbackSettings = BuildPlaybackSettings();

            if (!CanNavigatePlayback(session, playbackSettings))
            {
                return;
            }

            if (_macroPlaybackService.IsPaused)
            {
                await _macroPlaybackService.WaitForPlaybackNavigationReadyAsync();
            }

            _playedEventCount = 0;
            _playedDurationMs = 0;
            _activePlaybackSourceIndex = session is { Events.Count: > 0 }
                ? 0
                : null;

            if (_macroPlaybackService.IsPaused)
            {
                await _macroPlaybackService.SeekPlaybackCursorAsync(_playedEventCount);
            }

            RefreshUiState();
        }
        finally
        {
            Interlocked.Exchange(ref _playbackNavigationInProgress, 0);
        }
    }

    private async Task StepPlaybackAsync(int stepDirection)
    {
        if (Interlocked.Exchange(ref _playbackNavigationInProgress, 1) == 1)
        {
            return;
        }

        try
        {
            MacroSession? session = GetSessionForPlayback();
            PlaybackSettings playbackSettings = BuildPlaybackSettings();

            if (session is null || !CanNavigatePlayback(session, playbackSettings))
            {
                return;
            }

            if (_macroPlaybackService.IsPaused)
            {
                await _macroPlaybackService.WaitForPlaybackNavigationReadyAsync();
            }

            int totalPlaybackEvents = GetTotalPlaybackEventCount(session, playbackSettings);

            if (stepDirection <= 0)
            {
                StepPlaybackCursorBack(session);

                if (_macroPlaybackService.IsPaused)
                {
                    await _macroPlaybackService.SeekPlaybackCursorAsync(_playedEventCount);
                }

                return;
            }

            int targetLogicalIndex = _playedEventCount;

            if (targetLogicalIndex >= totalPlaybackEvents)
            {
                return;
            }

            int sourceEventIndex = targetLogicalIndex % session.Events.Count;
            await _macroPlaybackService.PlayEventAtAsync(
                session,
                playbackSettings,
                sourceEventIndex,
                targetLogicalIndex);

            _playedEventCount = targetLogicalIndex + 1;
            _playedDurationMs = CalculatePlayedDurationMs(session, targetLogicalIndex);
            _activePlaybackSourceIndex = sourceEventIndex;

            if (_macroPlaybackService.IsPaused)
            {
                await _macroPlaybackService.SeekPlaybackCursorAsync(_playedEventCount);
            }

            RefreshUiState();
        }
        finally
        {
            Interlocked.Exchange(ref _playbackNavigationInProgress, 0);
        }
    }

    private void StepPlaybackCursorBack(MacroSession session)
    {
        if (_playedEventCount <= 0)
        {
            return;
        }

        int previousPlayedEventCount = Math.Max(0, _playedEventCount - 1);
        _playedEventCount = previousPlayedEventCount;
        _playedDurationMs = previousPlayedEventCount == 0
            ? 0
            : CalculatePlayedDurationMs(session, previousPlayedEventCount - 1);
        _activePlaybackSourceIndex = previousPlayedEventCount == 0
            ? 0
            : (previousPlayedEventCount - 1) % session.Events.Count;
        RefreshUiState();
    }

    private MacroSession? GetSessionForPlayback()
    {
        if (_macroRecorderService.IsRecording)
        {
            return _macroRecorderService.CurrentSession;
        }

        return _activeSession ?? _macroRecorderService.CurrentSession;
    }

    private MacroSession GetRequiredSession()
    {
        return GetSessionForPlayback() is { Events.Count: > 0 } session
            ? session
            : throw new InvalidOperationException("Kullanilacak kayitli veya yuklenmis bir oturum yok.");
    }

    private static decimal ClampDecimal(
        decimal value,
        decimal minimum,
        decimal maximum)
    {
        return Math.Min(Math.Max(value, minimum), maximum);
    }

    private PlaybackSettings BuildPlaybackSettings()
    {
        return _playbackSettingsControl.GetCurrentSettings();
    }

    private PlaybackSettings? ResolvePlaybackSettingsForStart(MacroSession session)
    {
        PlaybackSettings playbackSettings = BuildPlaybackSettings();

        if (!PlaybackResolutionWarningPolicy.ShouldInspectCurrentScreen(session, playbackSettings))
        {
            return playbackSettings;
        }

        RecordedScreenInfo? currentScreen = TryGetCurrentScreenForResolutionWarning();

        if (!PlaybackResolutionWarningPolicy.ShouldWarn(session, playbackSettings, currentScreen))
        {
            return playbackSettings;
        }

        PlaybackResolutionWarningChoice warningChoice =
            PlaybackResolutionWarningDialog.Show(this, session.RecordedScreen!, currentScreen!);

        return warningChoice switch
        {
            PlaybackResolutionWarningChoice.ScaledPlayback =>
                ClonePlaybackSettings(playbackSettings, useScreenScaledCoordinates: true),
            PlaybackResolutionWarningChoice.NormalPlayback => playbackSettings,
            _ => null
        };
    }

    private RecordedScreenInfo? TryGetCurrentScreenForResolutionWarning()
    {
        try
        {
            return _recordedScreenProvider.GetRecordedScreen();
        }
        catch (Exception ex)
        {
            _logger.Log(
                AppLogLevel.Warning,
                nameof(MainForm),
                "Oynatma cozunurluk uyarisi icin mevcut ekran bilgisi alinamadi.",
                ex);
            return null;
        }
    }

    private static PlaybackSettings ClonePlaybackSettings(
        PlaybackSettings source,
        bool useScreenScaledCoordinates)
    {
        return new PlaybackSettings
        {
            SpeedMultiplier = source.SpeedMultiplier,
            RepeatCount = source.RepeatCount,
            InitialDelayMs = source.InitialDelayMs,
            LoopIndefinitely = source.LoopIndefinitely,
            UseRelativeCoordinates = useScreenScaledCoordinates
                ? false
                : source.UseRelativeCoordinates,
            UseScreenScaledCoordinates = useScreenScaledCoordinates,
            SimulationMode = source.SimulationMode,
            StopOnError = source.StopOnError,
            PreserveOriginalTiming = source.PreserveOriginalTiming
        };
    }

    private bool CanNavigatePlayback(
        MacroSession? session,
        PlaybackSettings playbackSettings)
    {
        return !_shutdownInProgress
            && session is { Events.Count: > 0 }
            && !_macroRecorderService.IsRecording
            && !_macroPlaybackService.IsPlaying
            && _applicationStateService.IsAny(AppState.Idle, AppState.Paused)
            && GetTotalPlaybackEventCount(session, playbackSettings) > 0;
    }

    private static int GetTotalPlaybackEventCount(
        MacroSession session,
        PlaybackSettings playbackSettings)
    {
        if (session.Events.Count == 0)
        {
            return 0;
        }

        int repeatCount = playbackSettings.LoopIndefinitely
            ? 1
            : Math.Max(playbackSettings.RepeatCount, 1);

        return SaturatingMultiply(session.Events.Count, repeatCount);
    }

    private static int SaturatingMultiply(int value, int multiplier)
    {
        long result = (long)Math.Max(0, value) * Math.Max(0, multiplier);
        return result > int.MaxValue ? int.MaxValue : (int)result;
    }

    private static int SaturatingAdd(int left, int right)
    {
        long result = (long)Math.Max(0, left) + Math.Max(0, right);
        return result > int.MaxValue ? int.MaxValue : (int)result;
    }

    private static int CalculatePlayedDurationMs(
        MacroSession session,
        int inclusiveLogicalIndex)
    {
        if (session.Events.Count == 0 || inclusiveLogicalIndex < 0)
        {
            return 0;
        }

        int completedCycles = inclusiveLogicalIndex / session.Events.Count;
        int partialEventIndex = inclusiveLogicalIndex % session.Events.Count;
        long playedDurationMs = (long)completedCycles * Math.Max(0, session.TotalDurationMs);

        for (int eventIndex = 0; eventIndex <= partialEventIndex; eventIndex++)
        {
            playedDurationMs += Math.Max(0, session.Events[eventIndex].DelayMs);
        }

        return playedDurationMs > int.MaxValue
            ? int.MaxValue
            : (int)playedDurationMs;
    }
}

