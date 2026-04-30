using MacroMaster.Domain.Enums;
using MacroMaster.Domain.Models;
using MacroMaster.WinForms.Controls;

namespace MacroMaster.WinForms.Forms;

public partial class MainForm
{
    private readonly List<MacroEvent> _timelineEvents = new();
    private MacroSession? _activeSession;
    private Guid? _activePlaybackEventId;

    private void RefreshUiState()
    {
        var totalEvents = _timelineEvents.Count;
        var totalDuration = _timelineEvents.Sum(e => e.DelayMs);
        var currentState = _applicationStateService.CurrentState;

        _sessionStatusValueLabel.Text = GetStateDisplayText(currentState);
        _totalEventsValueLabel.Text = totalEvents.ToString();
        _totalDurationValueLabel.Text = $"{totalDuration} ms";
        _sessionNameValueLabel.Text = _activeSession?.Name ?? "Hazir oturum yok";

        _playbackStatusValueLabel.Text = currentState switch
        {
            AppState.Playing => "Oynatiliyor",
            AppState.Paused => "Duraklatildi",
            AppState.Recording => "Kayit aliniyor",
            AppState.Stopping => "Durduruluyor",
            AppState.Error => "Hata",
            _ => "Hazir"
        };

        _currentEventValueLabel.Text = _activePlaybackEventId.HasValue
            ? $"Secili oynatma olayi: {_activePlaybackEventId}"
            : totalEvents > 0
                ? $"Toplam olay: {totalEvents}"
                : "Henuz olay kaydi yok";

        _playbackProgressBar.Maximum = Math.Max(totalEvents, 1);
        var progressValue = _activePlaybackEventId.HasValue
            ? Math.Min(GetPlaybackProgressIndex(_activePlaybackEventId.Value), _playbackProgressBar.Maximum)
            : Math.Min(totalEvents, _playbackProgressBar.Maximum);
        _playbackProgressBar.Value = Math.Max(0, progressValue);
        _playbackProgressValueLabel.Text = totalEvents > 0
            ? $"{Math.Min(progressValue, totalEvents)} / {totalEvents}"
            : "0 / 0";

        var hasSession = _activeSession is { Events.Count: > 0 } || _timelineEvents.Count > 0;
        var canRecord = !_macroPlaybackService.IsPlaying && !_macroPlaybackService.IsPaused;
        var canPlay = !_macroRecorderService.IsRecording && hasSession;
        var canStop = _macroRecorderService.IsRecording || _macroPlaybackService.IsPlaying || _macroPlaybackService.IsPaused;

        _recordButton.Text = _macroRecorderService.IsRecording ? "Kaydi Durdur" : "Kaydi Baslat";
        _recordButton.Enabled = canRecord;
        _recordButton.Variant = _macroRecorderService.IsRecording
            ? ModernButtonVariant.Danger
            : ModernButtonVariant.Primary;

        _playButton.Text = _macroPlaybackService.IsPaused
            ? "Devam Et"
            : _macroPlaybackService.IsPlaying
                ? "Duraklat"
                : "Oynat";
        _playButton.Enabled = canPlay;
        _playButton.Variant = _macroPlaybackService.IsPaused
            ? ModernButtonVariant.Success
            : ModernButtonVariant.Secondary;

        _stopButton.Enabled = canStop;
        _saveButton.Enabled = hasSession && !_macroRecorderService.IsRecording;
        _loadButton.Enabled = !_macroRecorderService.IsRecording && !_macroPlaybackService.IsPlaying;
        _clearButton.Enabled = hasSession && !_macroRecorderService.IsRecording && !_macroPlaybackService.IsPlaying && !_macroPlaybackService.IsPaused;

        _recordToolButton.Text = _macroRecorderService.IsRecording ? "Durdur" : "Kayit";
        _recordToolButton.Enabled = canRecord;
        _recordMenuItem.Text = _macroRecorderService.IsRecording ? "Kaydi Durdur" : "Kaydi Baslat";
        _recordMenuItem.Enabled = canRecord;

        _playToolButton.Text = _macroPlaybackService.IsPaused
            ? "Devam"
            : _macroPlaybackService.IsPlaying
                ? "Duraklat"
                : "Oynat";
        _playToolButton.Enabled = canPlay;
        _playMenuItem.Text = _macroPlaybackService.IsPaused
            ? "Devam Et"
            : _macroPlaybackService.IsPlaying
                ? "Duraklat"
                : "Oynat";
        _playMenuItem.Enabled = canPlay;

        _stopToolButton.Enabled = canStop;
        _stopMenuItem.Enabled = canStop;
        _saveToolButton.Enabled = _saveButton.Enabled;
        _saveMenuItem.Enabled = _saveButton.Enabled;
        _loadToolButton.Enabled = _loadButton.Enabled;
        _loadMenuItem.Enabled = _loadButton.Enabled;
        _clearToolButton.Enabled = _clearButton.Enabled;
        _clearMenuItem.Enabled = _clearButton.Enabled;

        _statusStripStateLabel.Text = $"Durum: {GetStateDisplayText(currentState)}";
        _statusStripEventCountLabel.Text = $"Olay: {totalEvents}";
        _statusStripSessionLabel.Text = $"Oturum: {(_activeSession?.Name ?? "Hazir oturum yok")}";
        _statusStripHotkeysLabel.Text = "Kisayol: F8 / F9 / F10";

        var canEditPlaybackSettings = !_macroRecorderService.IsRecording
            && !_macroPlaybackService.IsPlaying
            && !_macroPlaybackService.IsPaused;
        _speedNumeric.Enabled = canEditPlaybackSettings;
        _repeatCountNumeric.Enabled = canEditPlaybackSettings && !_loopPlaybackCheckBox.Checked;
        _initialDelayNumeric.Enabled = canEditPlaybackSettings;
        _loopPlaybackCheckBox.Enabled = canEditPlaybackSettings;
        _relativeCoordinatesCheckBox.Enabled = canEditPlaybackSettings;
        _preserveTimingCheckBox.Enabled = canEditPlaybackSettings;
        _stopOnErrorCheckBox.Enabled = canEditPlaybackSettings;
    }

    private void SetActiveSession(MacroSession? session)
    {
        _activeSession = session;
        _timelineEvents.Clear();

        if (session is not null)
        {
            _timelineEvents.AddRange(session.Events);
        }

        _activePlaybackEventId = null;
        RefreshEventGrid();
        ShowEventDetails(_timelineEvents.FirstOrDefault());
        RefreshUiState();
    }

    private void AppendRecordedEvent(MacroEvent macroEvent)
    {
        _timelineEvents.Add(macroEvent);
        AddEventRow(macroEvent, _timelineEvents.Count - 1);
        RefreshUiState();
    }

    private void UpdatePlaybackProgress(MacroEvent macroEvent)
    {
        _activePlaybackEventId = macroEvent.Id;
        SelectEvent(macroEvent);
        RefreshUiState();
    }

    private void ResetUiTimeline()
    {
        _activeSession = null;
        _activePlaybackEventId = null;
        _timelineEvents.Clear();
        RefreshEventGrid();
        ShowEventDetails(null);
        RefreshUiState();
    }

    private int GetPlaybackProgressIndex(Guid eventId)
    {
        var index = _timelineEvents.FindIndex(macroEvent => macroEvent.Id == eventId);
        return index < 0 ? 0 : index + 1;
    }

    private static string GetStateDisplayText(AppState state)
    {
        return state switch
        {
            AppState.Recording => "Kayit",
            AppState.Playing => "Oynatma",
            AppState.Paused => "Durakladi",
            AppState.Stopping => "Duruyor",
            AppState.Error => "Hata",
            _ => "Bos"
        };
    }
}
