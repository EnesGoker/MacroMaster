using MacroMaster.Domain.Enums;
using MacroMaster.Domain.Models;
using MacroMaster.WinForms.Controls;

namespace MacroMaster.WinForms.Forms;

public partial class MainForm
{
    private void RefreshUiState(bool forceEventListReload = false)
    {
        MacroSession? displayedSession = GetSessionForPlayback();
        bool hasSession = displayedSession is { Events.Count: > 0 };
        PlaybackSettings playbackSettings = BuildPlaybackSettings();

        _eventListControl.SetSession(displayedSession, forceEventListReload);
        SelectActivePlaybackCursor(displayedSession, playbackSettings);

        bool isIdle = _applicationStateService.IsState(AppState.Idle);
        bool canRecord = !_shutdownInProgress
            && !_macroPlaybackService.IsPlaying
            && !_macroPlaybackService.IsPaused;
        bool canPlayback = !_shutdownInProgress
            && !_macroRecorderService.IsRecording
            && (_macroPlaybackService.IsPlaying || _macroPlaybackService.IsPaused || hasSession);
        bool canStop = !_shutdownInProgress
            && (_macroRecorderService.IsRecording || _macroPlaybackService.IsPlaying || _macroPlaybackService.IsPaused);
        bool canNavigatePlayback = CanNavigatePlayback(displayedSession, playbackSettings);
        bool canSaveSession = !_shutdownInProgress && isIdle && hasSession;
        bool canLoadSession = !_shutdownInProgress && isIdle;
        bool canEditHotkeys = !_shutdownInProgress && isIdle;
        bool playbackSettingsEnabled = !_shutdownInProgress
            && !_macroRecorderService.IsRecording
            && !_macroPlaybackService.IsPlaying
            && !_macroPlaybackService.IsPaused;
        _playbackSettingsControl.SetControlsEnabled(playbackSettingsEnabled);

        _toolbarControl.UpdateRecordButton(_macroRecorderService.IsRecording);
        _toolbarControl.UpdatePlaybackButton(
            _macroPlaybackService.IsPaused
                ? PlaybackButtonState.Resume
                : _macroPlaybackService.IsPlaying
                    ? PlaybackButtonState.Pause
                    : PlaybackButtonState.Play);
        _toolbarControl.SetButtonsEnabled(
            new ToolbarButtonState(
                canRecord,
                canStop,
                canPlayback,
                canSaveSession,
                canSaveSession,
                canSaveSession,
                canLoadSession,
                canLoadSession,
                canEditHotkeys));
        _playbackControl.UpdateState(
            BuildPlaybackControlState(
                displayedSession,
                playbackSettings,
                canPlayback,
                canStop,
                canNavigatePlayback));
        string statusText = ResolveStatusText(_applicationStateService.CurrentState, playbackSettings);
        var summaryState = BuildSessionSummaryState(displayedSession, statusText);
        _sessionSummaryControl.UpdateState(
            summaryState,
            displayedSession?.Events,
            _activePlaybackSourceIndex);
        UpdateMacroPreviewMapDialog(
            summaryState,
            displayedSession?.Events,
            _activePlaybackSourceIndex);
        _titleBarControl.SetStatus(
            statusText,
            ResolveTitleBarStatusColor(_applicationStateService.CurrentState));
        _titleBarControl.SetMaximized(WindowState == FormWindowState.Maximized);
    }

    private SessionSummaryState BuildSessionSummaryState(
        MacroSession? displayedSession,
        string statusText)
    {
        return new SessionSummaryState(
            statusText,
            displayedSession?.Name ?? "Oturum yok",
            displayedSession?.Events.Count ?? 0,
            displayedSession?.TotalDurationMs ?? 0,
            string.IsNullOrWhiteSpace(_lastSessionPath)
                ? "Kaydedilmedi"
                : Path.GetFileName(_lastSessionPath));
    }

    private void UpdateMacroPreviewMapDialog(
        SessionSummaryState summaryState,
        IReadOnlyList<MacroEvent>? events,
        int? activeSourceEventIndex)
    {
        if (_macroPreviewMapDialog is null
            || _macroPreviewMapDialog.IsDisposed
            || !_macroPreviewMapDialog.Visible)
        {
            return;
        }

        _macroPreviewMapDialog.UpdatePreview(
            summaryState,
            events,
            activeSourceEventIndex);
    }

    private void UpdateActivePreviewMapCursor(int? activeSourceEventIndex)
    {
        _sessionSummaryControl.UpdatePreviewActiveSourceEventIndex(activeSourceEventIndex);

        if (_macroPreviewMapDialog is null
            || _macroPreviewMapDialog.IsDisposed
            || !_macroPreviewMapDialog.Visible)
        {
            return;
        }

        _macroPreviewMapDialog.UpdateActiveSourceEventIndex(activeSourceEventIndex);
    }

    private void SelectActivePlaybackCursor(
        MacroSession? displayedSession,
        PlaybackSettings playbackSettings)
    {
        if (!ShouldFollowActivePlaybackCursor(displayedSession, playbackSettings))
        {
            return;
        }

        _eventListControl.TrySelectSourceEvent(_activePlaybackSourceIndex.GetValueOrDefault());
    }

    private bool ShouldFollowActivePlaybackCursor(
        MacroSession? displayedSession,
        PlaybackSettings playbackSettings)
    {
        if (displayedSession is not { Events.Count: > 0 }
            || !_activePlaybackSourceIndex.HasValue)
        {
            return false;
        }

        bool shouldFollowIdleDebugCursor = _applicationStateService.IsState(AppState.Idle);
        bool shouldFollowPausedDebugCursor = _applicationStateService.IsState(AppState.Paused);
        bool shouldFollowSimulationPlayback = playbackSettings.SimulationMode
            && _applicationStateService.IsState(AppState.Playing);

        return shouldFollowIdleDebugCursor
            || shouldFollowPausedDebugCursor
            || shouldFollowSimulationPlayback;
    }

    private void RequestUiRefresh()
    {
        if (!CanRunDeferredUiRefresh())
        {
            return;
        }

        if (InvokeRequired)
        {
            TryBeginInvokeOnUiThread(() => RefreshUiState());
            return;
        }

        RefreshUiState();
    }

    private bool CanRunDeferredUiRefresh()
    {
        return !_shutdownInProgress && IsHandleCreated && !IsDisposed;
    }

    private void TryBeginInvokeOnUiThread(Action action)
    {
        try
        {
            BeginInvoke(action);
        }
        catch (ObjectDisposedException)
        {
        }
        catch (InvalidOperationException)
        {
        }
    }

    private void RequestPlaybackTelemetryRefresh()
    {
        if (_playbackTelemetryRefreshThrottle is null)
        {
            RequestUiRefresh();
            return;
        }

        _playbackTelemetryRefreshThrottle.Request();
    }

    private void RefreshPlaybackTelemetry()
    {
        MacroSession? displayedSession = GetSessionForPlayback();
        PlaybackSettings playbackSettings = BuildPlaybackSettings();
        _playbackControl.UpdateTelemetry(BuildPlaybackTelemetrySnapshot(displayedSession, playbackSettings));
    }

    private void CancelPendingPlaybackTelemetryRefresh()
    {
        _playbackTelemetryRefreshThrottle?.Cancel();
    }

    private void RequestPlaybackSelectionRefresh()
    {
        if (_playbackSelectionRefreshThrottle is null)
        {
            RequestUiRefresh();
            return;
        }

        _playbackSelectionRefreshThrottle.Request();
    }

    private void RefreshPlaybackSelection()
    {
        MacroSession? displayedSession = GetSessionForPlayback();
        PlaybackSettings playbackSettings = BuildPlaybackSettings();
        SelectActivePlaybackCursor(displayedSession, playbackSettings);

        if (ShouldFollowActivePlaybackCursor(displayedSession, playbackSettings))
        {
            UpdateActivePreviewMapCursor(_activePlaybackSourceIndex);
        }
    }

    private void CancelPendingPlaybackSelectionRefresh()
    {
        _playbackSelectionRefreshThrottle?.Cancel();
    }

    private void RequestRecordingEventRefresh()
    {
        if (_recordingEventRefreshThrottle is null)
        {
            RequestUiRefresh();
            return;
        }

        _recordingEventRefreshThrottle.Request();
    }
}

