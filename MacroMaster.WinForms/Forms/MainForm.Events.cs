using MacroMaster.Domain.Enums;
using MacroMaster.Domain.Models;

namespace MacroMaster.WinForms.Forms;

public partial class MainForm
{
    private void SubscribeFormEvents()
    {
        _applicationStateService.StateChanged += OnApplicationStateChanged;
        _macroRecorderService.RecordingStarted += OnRecordingStarted;
        _macroRecorderService.EventRecorded += OnEventRecorded;
        _macroRecorderService.RecordingStopped += OnRecordingStopped;
        _macroPlaybackService.PlaybackStarted += OnPlaybackStarted;
        _macroPlaybackService.PlaybackPaused += OnPlaybackPaused;
        _macroPlaybackService.PlaybackResumed += OnPlaybackResumed;
        _macroPlaybackService.PlaybackStopped += OnPlaybackStopped;
        _macroPlaybackService.EventPlayed += OnPlaybackEventPlayed;

        _hotkeyService.RecordToggleRequested += OnRecordToggleRequested;
        _hotkeyService.PlaybackToggleRequested += OnPlaybackToggleRequested;
        _hotkeyService.StopRequested += OnStopRequested;
    }

    private void UnsubscribeFormEvents()
    {
        _applicationStateService.StateChanged -= OnApplicationStateChanged;
        _macroRecorderService.RecordingStarted -= OnRecordingStarted;
        _macroRecorderService.EventRecorded -= OnEventRecorded;
        _macroRecorderService.RecordingStopped -= OnRecordingStopped;
        _macroPlaybackService.PlaybackStarted -= OnPlaybackStarted;
        _macroPlaybackService.PlaybackPaused -= OnPlaybackPaused;
        _macroPlaybackService.PlaybackResumed -= OnPlaybackResumed;
        _macroPlaybackService.PlaybackStopped -= OnPlaybackStopped;
        _macroPlaybackService.EventPlayed -= OnPlaybackEventPlayed;

        _hotkeyService.RecordToggleRequested -= OnRecordToggleRequested;
        _hotkeyService.PlaybackToggleRequested -= OnPlaybackToggleRequested;
        _hotkeyService.StopRequested -= OnStopRequested;
    }

    private Task RegisterHotkeysAsync()
    {
        return _hotkeyService.RegisterAsync();
    }

    private Task UnregisterHotkeysAsync()
    {
        return _hotkeyService.UnregisterAsync();
    }

    private async void OnRecordToggleRequested()
    {
        await ToggleRecordingAsync();
    }

    private async void OnPlaybackToggleRequested()
    {
        await TogglePlaybackAsync();
    }

    private async void OnStopRequested()
    {
        await StopAsync();
    }

    private void OnApplicationStateChanged(AppState state)
    {
        RunOnUiThread(RefreshUiState);
    }

    private void OnRecordingStarted()
    {
        RunOnUiThread(() =>
        {
            _activeSession = _macroRecorderService.CurrentSession;
            _activePlaybackEventId = null;
            _timelineEvents.Clear();
            RefreshEventGrid();
            ShowEventDetails(null);
            RefreshUiState();
        });
    }

    private void OnEventRecorded(MacroEvent macroEvent)
    {
        RunOnUiThread(() => AppendRecordedEvent(macroEvent));
    }

    private void OnRecordingStopped(MacroSession session)
    {
        RunOnUiThread(() => SetActiveSession(session));
    }

    private void OnPlaybackStarted()
    {
        RunOnUiThread(RefreshUiState);
    }

    private void OnPlaybackPaused()
    {
        RunOnUiThread(RefreshUiState);
    }

    private void OnPlaybackResumed()
    {
        RunOnUiThread(RefreshUiState);
    }

    private void OnPlaybackStopped()
    {
        RunOnUiThread(() =>
        {
            _activePlaybackEventId = null;
            RefreshUiState();
        });
    }

    private void OnPlaybackEventPlayed(MacroEvent macroEvent)
    {
        RunOnUiThread(() => UpdatePlaybackProgress(macroEvent));
    }

    private void RunOnUiThread(Action action)
    {
        if (!IsHandleCreated)
        {
            return;
        }

        if (InvokeRequired)
        {
            BeginInvoke(action);
            return;
        }

        action();
    }
}
