using MacroMaster.Domain.Models;

namespace MacroMaster.WinForms.Forms;

public partial class MainForm
{
    private async Task HandleRecordToggleAsync()
    {
        if (_shutdownInProgress || _macroPlaybackService.IsPlaying || _macroPlaybackService.IsPaused)
        {
            return;
        }

        if (_macroRecorderService.IsRecording)
        {
            await _macroRecorderService.StopAsync();
            return;
        }

        _lastSessionPath = null;
        await _macroRecorderService.StartAsync();
        RefreshUiState();
    }

    private void OnRecordingStarted()
    {
        _activeSession = _macroRecorderService.CurrentSession;
        _lastSessionPath = null;
        _playedEventCount = 0;
        _playedDurationMs = 0;
        _activePlaybackSourceIndex = null;
        RequestUiRefresh();
    }

    private void OnEventRecorded(MacroEvent macroEvent)
    {
        _ = macroEvent;
        RequestRecordingEventRefresh();
    }

    private void OnRecordingStopped(MacroSession session)
    {
        _activeSession = session;
        _recordingEventRefreshThrottle?.Cancel();
        RequestUiRefresh();
    }
}

