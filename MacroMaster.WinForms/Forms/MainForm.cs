using MacroMaster.Application.Abstractions;
using MacroMaster.Domain.Enums;
using MacroMaster.Domain.Models;
using MacroMaster.WinForms.Controls;
using MacroMaster.WinForms.Platform;
using MacroMaster.WinForms.Reporting;
using MacroMaster.WinForms.Theme;
using System.Globalization;
using System.Text;

namespace MacroMaster.WinForms.Forms;

public partial class MainForm : Form
{
    private const int PlaybackTelemetryRefreshIntervalMs = 33;
    private const int PlaybackSelectionRefreshIntervalMs = 80;

    private readonly IApplicationStateService _applicationStateService;
    private readonly IMacroRecorderService _macroRecorderService;
    private readonly IMacroPlaybackService _macroPlaybackService;
    private readonly IMacroOptimizationService _macroOptimizationService;
    private readonly IMacroStorageService _macroStorageService;
    private readonly IMacroLibraryService _macroLibraryService;
    private readonly IPlaybackSettingsStore _playbackSettingsStore;
    private readonly IHotkeySettingsStore _hotkeySettingsStore;
    private readonly IMacroLibraryUserStateStore _macroLibraryUserStateStore;
    private readonly IMutableHotkeyConfiguration _hotkeyConfiguration;
    private readonly IHotkeyService _hotkeyService;
    private readonly IRecordedScreenProvider _recordedScreenProvider;
    private readonly IAppLogger _logger;
    private readonly ToolbarControl _toolbarControl = new();
    private readonly TitleBarControl _titleBarControl = new();
    private readonly PlaybackSettingsControl _playbackSettingsControl = new();
    private readonly EventListControl _eventListControl = new();
    private readonly PlaybackControl _playbackControl = new();
    private readonly MacroLibraryControl _macroLibraryControl = new();
    private readonly SessionSummaryControl _sessionSummaryControl = new();
    private MacroPreviewMapDialog? _macroPreviewMapDialog;

    private MacroSession? _activeSession;
    private string? _lastSessionPath;
    private MacroLibraryUserState _macroLibraryUserState = new();
    private int _playedEventCount;
    private int _playedDurationMs;
    private int? _activePlaybackSourceIndex;
    private int _playbackNavigationInProgress;
    private bool _applyingPlaybackSettings;
    private bool _shutdownInProgress;
    private bool _shutdownCompleted;
    private UiRefreshThrottle? _recordingEventRefreshThrottle;
    private UiRefreshThrottle? _playbackTelemetryRefreshThrottle;
    private UiRefreshThrottle? _playbackSelectionRefreshThrottle;

    public MainForm(
        IApplicationStateService applicationStateService,
        IMacroRecorderService macroRecorderService,
        IMacroPlaybackService macroPlaybackService,
        IMacroOptimizationService macroOptimizationService,
        IMacroStorageService macroStorageService,
        IMacroLibraryService macroLibraryService,
        IPlaybackSettingsStore playbackSettingsStore,
        IHotkeySettingsStore hotkeySettingsStore,
        IMacroLibraryUserStateStore macroLibraryUserStateStore,
        IMutableHotkeyConfiguration hotkeyConfiguration,
        IHotkeyService hotkeyService,
        IRecordedScreenProvider recordedScreenProvider,
        IAppLogger logger)
    {
        _applicationStateService = applicationStateService;
        _macroRecorderService = macroRecorderService;
        _macroPlaybackService = macroPlaybackService;
        _macroOptimizationService = macroOptimizationService;
        _macroStorageService = macroStorageService;
        _macroLibraryService = macroLibraryService;
        _playbackSettingsStore = playbackSettingsStore;
        _hotkeySettingsStore = hotkeySettingsStore;
        _macroLibraryUserStateStore = macroLibraryUserStateStore;
        _hotkeyConfiguration = hotkeyConfiguration;
        _hotkeyService = hotkeyService;
        _recordedScreenProvider = recordedScreenProvider;
        _logger = logger;

        InitializeComponent();
        InitializeDynamicControls();

        Load += MainForm_Load;
        FormClosing += MainForm_FormClosing;
    }

    private void MainForm_Load(object? sender, EventArgs e)
    {
        SubscribeToServiceEvents();
        _ = ExecuteUiActionAsync(InitializeUiAsync, "Uygulama baslatma");
    }

    private async Task InitializeUiAsync()
    {
        try
        {
            await LoadPlaybackSettingsAsync();
        }
        catch (Exception ex)
        {
            ApplyPlaybackSettings(new PlaybackSettings());
            ShowError(
                "Oynatma ayarlari yuklenemedi. Varsayilan oynatma ayarlari kullanilacak.",
                ex);
        }

        try
        {
            await LoadHotkeySettingsAsync();
        }
        catch (Exception ex)
        {
            _hotkeyConfiguration.Apply(HotkeySettings.CreateDefault());
            ShowError(
                "Kisayol ayarlari yuklenemedi. Varsayilan kisayollar kullanilacak.",
                ex);
        }

        try
        {
            await LoadMacroLibraryUserStateAsync();
        }
        catch (Exception ex)
        {
            _macroLibraryUserState = new MacroLibraryUserState();
            ShowError(
                "Makro kutuphanesi kullanici tercihleri yuklenemedi. Favori ve son kullanilan bilgileri bu oturumda bos baslatilacak.",
                ex);
        }

        UpdateHotkeySummary();
        try
        {
            await RefreshMacroLibraryAsync();
        }
        catch (Exception ex)
        {
            ShowError(
                "Makro kutuphanesi yuklenemedi. Uygulama bos kutuphane ile devam edecek.",
                ex);
            _macroLibraryControl.SetItems([], null);
        }

        RefreshUiState();
        await InitializeHotkeysAsync();
    }

    private async void MainForm_FormClosing(object? sender, FormClosingEventArgs e)
    {
        if (_shutdownCompleted)
        {
            return;
        }

        if (_shutdownInProgress)
        {
            e.Cancel = true;
            return;
        }

        e.Cancel = true;
        _shutdownInProgress = true;

        try
        {
            await ShutdownAsync();
            _shutdownCompleted = true;
            Close();
        }
        catch (Exception ex)
        {
            _shutdownInProgress = false;
            ShowError("Uygulama kapatilirken bir hata olustu.", ex);
        }
    }

    private async Task ShutdownAsync()
    {
        _recordingEventRefreshThrottle?.Cancel();
        _playbackTelemetryRefreshThrottle?.Cancel();
        _playbackSelectionRefreshThrottle?.Cancel();

        _hotkeyService.RecordToggleRequested -= OnRecordToggleRequested;
        _hotkeyService.PlaybackToggleRequested -= OnPlaybackToggleRequested;
        _hotkeyService.StopRequested -= OnStopRequested;
        _hotkeyService.HotkeySettingsRequested -= OnHotkeySettingsRequested;
        UnsubscribeFromServiceEvents();

        if (_hotkeyService.IsRegistered)
        {
            await _hotkeyService.UnregisterAsync();
        }

        if (_macroRecorderService.IsRecording)
        {
            await _macroRecorderService.StopAsync();
        }

        if (_macroPlaybackService.IsPlaying || _macroPlaybackService.IsPaused)
        {
            await _macroPlaybackService.StopAsync();
        }

        await SavePlaybackSettingsAsync();
        await SaveHotkeySettingsAsync();
    }

    private void DisposeDynamicResources()
    {
        _recordingEventRefreshThrottle?.Dispose();
        _recordingEventRefreshThrottle = null;
        _playbackTelemetryRefreshThrottle?.Dispose();
        _playbackTelemetryRefreshThrottle = null;
        _playbackSelectionRefreshThrottle?.Dispose();
        _playbackSelectionRefreshThrottle = null;
    }

    private async Task ExecuteUiActionAsync(
        Func<Task> action,
        string operationName)
    {
        try
        {
            await action();
        }
        catch (Exception ex)
        {
            ShowError($"{operationName} islemi basarisiz oldu.", ex);
        }
    }

    private void SubscribeToServiceEvents()
    {
        _applicationStateService.StateChanged += OnApplicationStateChanged;
        _macroRecorderService.RecordingStarted += OnRecordingStarted;
        _macroRecorderService.EventRecorded += OnEventRecorded;
        _macroRecorderService.RecordingStopped += OnRecordingStopped;
        _macroPlaybackService.PlaybackStarted += OnPlaybackStarted;
        _macroPlaybackService.PlaybackPaused += OnPlaybackStateChanged;
        _macroPlaybackService.PlaybackResumed += OnPlaybackStateChanged;
        _macroPlaybackService.PlaybackStopped += OnPlaybackStopped;
        _macroPlaybackService.EventPlayed += OnPlaybackEventPlayed;

        _activeSession = _macroRecorderService.CurrentSession;
    }

    private void UnsubscribeFromServiceEvents()
    {
        _applicationStateService.StateChanged -= OnApplicationStateChanged;
        _macroRecorderService.RecordingStarted -= OnRecordingStarted;
        _macroRecorderService.EventRecorded -= OnEventRecorded;
        _macroRecorderService.RecordingStopped -= OnRecordingStopped;
        _macroPlaybackService.PlaybackStarted -= OnPlaybackStarted;
        _macroPlaybackService.PlaybackPaused -= OnPlaybackStateChanged;
        _macroPlaybackService.PlaybackResumed -= OnPlaybackStateChanged;
        _macroPlaybackService.PlaybackStopped -= OnPlaybackStopped;
        _macroPlaybackService.EventPlayed -= OnPlaybackEventPlayed;
    }

    private void OnApplicationStateChanged(AppState state)
    {
        _ = state;
        RequestUiRefresh();
    }

    private void OnPlaybackStarted()
    {
        _playedEventCount = 0;
        _playedDurationMs = 0;
        _activePlaybackSourceIndex = null;
        CancelPendingPlaybackTelemetryRefresh();
        CancelPendingPlaybackSelectionRefresh();
        RequestUiRefresh();
    }

    private void OnPlaybackStateChanged()
    {
        CancelPendingPlaybackTelemetryRefresh();
        CancelPendingPlaybackSelectionRefresh();
        RequestUiRefresh();
    }

    private void OnPlaybackEventPlayed(MacroEvent macroEvent)
    {
        _activePlaybackSourceIndex = ResolveCurrentPlaybackSourceIndex();
        _playedEventCount = SaturatingAdd(_playedEventCount, 1);
        _playedDurationMs = SaturatingAdd(_playedDurationMs, macroEvent.DelayMs);
        RequestPlaybackTelemetryRefresh();
        RequestPlaybackSelectionRefresh();
    }

    private int? ResolveCurrentPlaybackSourceIndex()
    {
        MacroSession? session = GetSessionForPlayback();

        if (session is not { Events.Count: > 0 })
        {
            return null;
        }

        return Math.Clamp(
            _playedEventCount % session.Events.Count,
            0,
            session.Events.Count - 1);
    }

    private void OnPlaybackStopped()
    {
        _playedEventCount = 0;
        _playedDurationMs = 0;
        _activePlaybackSourceIndex = null;
        CancelPendingPlaybackTelemetryRefresh();
        CancelPendingPlaybackSelectionRefresh();
        RequestUiRefresh();
    }

    private void recordToggleButton_Click(object? sender, EventArgs e)
    {
        _ = sender;
        _ = e;
        OnRecordToggleRequested();
    }

    private void playbackToggleButton_Click(object? sender, EventArgs e)
    {
        _ = sender;
        _ = e;
        OnPlaybackToggleRequested();
    }

    private void stopButton_Click(object? sender, EventArgs e)
    {
        _ = sender;
        _ = e;
        OnStopRequested();
    }

    private void playbackSkipBackButton_Click(object? sender, EventArgs e)
    {
        _ = sender;
        _ = e;
        _ = ExecuteUiActionAsync(ResetPlaybackCursorAsync, "Oynatma basa alma");
    }

    private void playbackStepBackButton_Click(object? sender, EventArgs e)
    {
        _ = sender;
        _ = e;
        _ = ExecuteUiActionAsync(() => StepPlaybackAsync(stepDirection: -1), "Oynatma geri adim");
    }

    private void playbackStepForwardButton_Click(object? sender, EventArgs e)
    {
        _ = sender;
        _ = e;
        _ = ExecuteUiActionAsync(() => StepPlaybackAsync(stepDirection: 1), "Oynatma ileri adim");
    }

    private void saveJsonButton_Click(object? sender, EventArgs e)
    {
        _ = sender;
        _ = e;
        _ = ExecuteUiActionAsync(SaveJsonAsync, "JSON kaydetme");
    }

    private void saveLibraryButton_Click(object? sender, EventArgs e)
    {
        _ = sender;
        _ = e;
        _ = ExecuteUiActionAsync(SaveToLibraryAsync, "Makro kutuphanesine kaydetme");
    }

    private void loadJsonButton_Click(object? sender, EventArgs e)
    {
        _ = sender;
        _ = e;
        _ = ExecuteUiActionAsync(LoadJsonAsync, "JSON yukleme");
    }

    private void importLibraryMacroButton_Click(object? sender, EventArgs e)
    {
        _ = sender;
        _ = e;
        _ = ExecuteUiActionAsync(ImportLibraryMacroAsync, "Makro kutuphanesine ice aktarma");
    }

    private void saveXmlButton_Click(object? sender, EventArgs e)
    {
        _ = sender;
        _ = e;
        _ = ExecuteUiActionAsync(SaveXmlAsync, "XML kaydetme");
    }

    private void saveHtmlReportButton_Click(object? sender, EventArgs e)
    {
        _ = sender;
        _ = e;
        _ = ExecuteUiActionAsync(SaveHtmlReportAsync, "HTML rapor olusturma");
    }

    private void saveTextReportButton_Click(object? sender, EventArgs e)
    {
        _ = sender;
        _ = e;
        _ = ExecuteUiActionAsync(SaveTextReportAsync, "TXT rapor olusturma");
    }

    private void loadXmlButton_Click(object? sender, EventArgs e)
    {
        _ = sender;
        _ = e;
        _ = ExecuteUiActionAsync(LoadXmlAsync, "XML yukleme");
    }

    private void clearSessionButton_Click(object? sender, EventArgs e)
    {
        _ = sender;
        _ = e;
        _ = ExecuteUiActionAsync(ClearSessionAsync, "Oturum temizleme");
    }

    private void editHotkeysButton_Click(object? sender, EventArgs e)
    {
        _ = sender;
        _ = e;
        _ = ExecuteUiActionAsync(EditHotkeysAsync, "Kisayol ayarlari");
    }

    private void macroLibraryControl_LoadRequested(object? sender, MacroLibraryItemEventArgs e)
    {
        _ = sender;
        _ = ExecuteUiActionAsync(() => LoadLibraryMacroAsync(e.Item), "Makro kutuphanesi yukleme");
    }

    private void macroLibraryControl_RenameRequested(object? sender, MacroLibraryItemEventArgs e)
    {
        _ = sender;
        _ = ExecuteUiActionAsync(() => RenameLibraryMacroAsync(e.Item), "Makro kutuphanesi isim duzenleme");
    }

    private void macroLibraryControl_DeleteRequested(object? sender, MacroLibraryItemEventArgs e)
    {
        _ = sender;
        _ = ExecuteUiActionAsync(() => DeleteLibraryMacroAsync(e.Item), "Makro kutuphanesi silme");
    }

    private void macroLibraryControl_FavoriteToggled(object? sender, MacroLibraryItemEventArgs e)
    {
        _ = sender;
        _ = ExecuteUiActionAsync(() => ToggleLibraryFavoriteAsync(e.Item), "Makro favori degistirme");
    }

    private void macroLibraryControl_OptimizeRequested(object? sender, MacroLibraryItemEventArgs e)
    {
        _ = sender;
        _ = ExecuteUiActionAsync(() => OptimizeLibraryMacroAsync(e.Item), "Makro kutuphanesi optimizasyonu");
    }

    private void eventListControl_EventEditRequested(object? sender, EventEditRequestedEventArgs e)
    {
        _ = sender;
        _ = ExecuteUiActionAsync(() => EditEventAsync(e), "Olay duzenleme");
    }

    private void preserveTimingCheckBox_CheckedChanged(object? sender, EventArgs e)
    {
        _ = sender;
        _ = e;
        if (_applyingPlaybackSettings)
        {
            return;
        }

        RefreshUiState();
    }

    private void loopIndefinitelyCheckBox_CheckedChanged(object? sender, EventArgs e)
    {
        _ = sender;
        _ = e;
        if (_applyingPlaybackSettings)
        {
            return;
        }

        RefreshUiState();
    }

    private void playbackSettingControl_ValueChanged(object? sender, EventArgs e)
    {
        _ = sender;
        _ = e;
        if (_applyingPlaybackSettings)
        {
            return;
        }

        RefreshUiState();
    }

    private async Task ClearSessionAsync()
    {
        EnsureSessionMutationAllowed();

        _macroRecorderService.Clear();
        _activeSession = null;
        _lastSessionPath = null;
        _playedEventCount = 0;
        _playedDurationMs = 0;
        _activePlaybackSourceIndex = null;
        await RefreshMacroLibraryAsync();
        RefreshUiState();
    }

    private Task EditEventAsync(EventEditRequestedEventArgs editRequest)
    {
        ArgumentNullException.ThrowIfNull(editRequest);
        EnsureSessionMutationAllowed();

        MacroSession session = GetRequiredSession();

        if (editRequest.EventIndex < 0 || editRequest.EventIndex >= session.Events.Count)
        {
            throw new InvalidOperationException("Duzenlenecek olay aktif oturumda bulunamadi.");
        }

        MacroEvent macroEvent = session.Events[editRequest.EventIndex];

        using var dialog = new EventEditDialog(editRequest.EventIndex, macroEvent);

        if (ModalDialogOverlay.ShowDialog(this, dialog) != DialogResult.OK)
        {
            return Task.CompletedTask;
        }

        macroEvent.DelayMs = dialog.EditResult.DelayMs;

        if (dialog.EditResult.X.HasValue && dialog.EditResult.Y.HasValue)
        {
            macroEvent.X = dialog.EditResult.X;
            macroEvent.Y = dialog.EditResult.Y;
        }

        RefreshUiState(forceEventListReload: true);
        return Task.CompletedTask;
    }

    private void AdoptLoadedSession(MacroSession session, string filePath)
    {
        _activeSession = session;
        _lastSessionPath = filePath;
        _playedEventCount = 0;
        _playedDurationMs = 0;
        _activePlaybackSourceIndex = null;
        RefreshUiState();
    }

    private async Task LoadPlaybackSettingsAsync()
    {
        PlaybackSettings playbackSettings = await _playbackSettingsStore.LoadAsync();
        ApplyPlaybackSettings(playbackSettings);
    }

    private async Task SavePlaybackSettingsAsync()
    {
        await _playbackSettingsStore.SaveAsync(BuildPlaybackSettings());
    }

    private async Task LoadHotkeySettingsAsync()
    {
        HotkeySettings hotkeySettings = await _hotkeySettingsStore.LoadAsync();
        _hotkeyConfiguration.Apply(hotkeySettings);
    }

    private async Task SaveHotkeySettingsAsync()
    {
        await _hotkeySettingsStore.SaveAsync(_hotkeyConfiguration.Snapshot());
    }

    private void ApplyPlaybackSettings(PlaybackSettings playbackSettings)
    {
        ArgumentNullException.ThrowIfNull(playbackSettings);

        _applyingPlaybackSettings = true;

        try
        {
            _playbackSettingsControl.ApplySettings(playbackSettings);
        }
        finally
        {
            _applyingPlaybackSettings = false;
        }
    }

    private void EnsureSessionMutationAllowed()
    {
        if (_shutdownInProgress
            || _applicationStateService.IsAny(AppState.Recording, AppState.Playing, AppState.Paused, AppState.Stopping))
        {
            throw new InvalidOperationException(
                "Oturum kaydetme ve yukleme islemleri yalnizca uygulama bostayken kullanilabilir.");
        }
    }

    private void EnsureHotkeyMutationAllowed()
    {
        if (_shutdownInProgress || !_applicationStateService.IsState(AppState.Idle))
        {
            throw new InvalidOperationException(
                "Kisayol degisiklikleri yalnizca uygulama bostayken yapilabilir.");
        }
    }

    private static string FormatAppState(AppState appState)
    {
        return appState switch
        {
            AppState.Idle => "Bos",
            AppState.Recording => "Kayit",
            AppState.Playing => "Oynatma",
            AppState.Paused => "Duraklatildi",
            AppState.Stopping => "Durduruluyor",
            AppState.Error => "Hata",
            _ => appState.ToString()
        };
    }

    private static string ResolveStatusText(
        AppState appState,
        PlaybackSettings playbackSettings)
    {
        if (playbackSettings.SimulationMode)
        {
            return appState switch
            {
                AppState.Playing => "Simülasyon",
                AppState.Paused => "Simülasyon duraklatıldı",
                _ => FormatAppState(appState)
            };
        }

        return FormatAppState(appState);
    }

    private PlaybackControlState BuildPlaybackControlState(
        MacroSession? session,
        PlaybackSettings playbackSettings,
        bool canPlayback,
        bool canStop,
        bool canNavigatePlayback)
    {
        PlaybackTelemetrySnapshot telemetry = BuildPlaybackTelemetrySnapshot(session, playbackSettings);

        return new PlaybackControlState(
            telemetry,
            canPlayback,
            canStop,
            canNavigatePlayback,
            _macroPlaybackService.IsPaused
                ? PlaybackButtonState.Resume
                : _macroPlaybackService.IsPlaying
                    ? PlaybackButtonState.Pause
                    : PlaybackButtonState.Play);
    }

    private PlaybackTelemetrySnapshot BuildPlaybackTelemetrySnapshot(
        MacroSession? session,
        PlaybackSettings playbackSettings)
    {
        int repeatCount = Math.Max(playbackSettings.RepeatCount, 1);
        int sessionEventCount = session?.Events.Count ?? 0;
        int sessionDurationMs = Math.Max(0, session?.TotalDurationMs ?? 0);
        int initialDelayMs = Math.Max(0, playbackSettings.InitialDelayMs);
        int totalEventCount = playbackSettings.LoopIndefinitely
            ? Math.Max(0, sessionEventCount)
            : SaturatingMultiply(sessionEventCount, repeatCount);
        int totalDurationMs = playbackSettings.LoopIndefinitely
            ? sessionDurationMs
            : SaturatingAdd(SaturatingMultiply(sessionDurationMs, repeatCount), initialDelayMs);
        int playedEventCount = Math.Clamp(_playedEventCount, 0, Math.Max(0, totalEventCount));
        int playedDurationMs = Math.Clamp(_playedDurationMs, 0, Math.Max(0, totalDurationMs));
        double speedMultiplier = playbackSettings.PreserveOriginalTiming || playbackSettings.SpeedMultiplier <= 0
            ? 1.0
            : playbackSettings.SpeedMultiplier;

        return new PlaybackTelemetrySnapshot(
            ResolveStatusText(_applicationStateService.CurrentState, playbackSettings),
            playedEventCount,
            totalEventCount,
            playedDurationMs,
            totalDurationMs,
            speedMultiplier,
            repeatCount,
            playbackSettings.LoopIndefinitely,
            initialDelayMs);
    }

    private sealed class UiRefreshThrottle : IDisposable
    {
        private readonly Control _owner;
        private readonly System.Windows.Forms.Timer _timer;
        private readonly Action _refreshAction;
        private readonly Func<bool> _canRun;
        private volatile bool _pending;
        private bool _disposed;

        public UiRefreshThrottle(
            Control owner,
            int intervalMs,
            Action refreshAction,
            Func<bool> canRun)
        {
            ArgumentNullException.ThrowIfNull(owner);
            ArgumentNullException.ThrowIfNull(refreshAction);
            ArgumentNullException.ThrowIfNull(canRun);

            if (intervalMs <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(intervalMs),
                    intervalMs,
                    "Refresh throttle interval must be greater than zero.");
            }

            _owner = owner;
            _refreshAction = refreshAction;
            _canRun = canRun;
            _timer = new System.Windows.Forms.Timer
            {
                Interval = intervalMs
            };
            _timer.Tick += OnTick;
        }

        public void Request()
        {
            if (_disposed)
            {
                return;
            }

            if (!CanAccessOwner())
            {
                return;
            }

            if (_owner.InvokeRequired)
            {
                TryBeginInvoke(RequestCore);
                return;
            }

            RequestCore();
        }

        public void Cancel()
        {
            _pending = false;

            if (_disposed)
            {
                return;
            }

            if (!_owner.IsHandleCreated || _owner.IsDisposed)
            {
                return;
            }

            if (_owner.InvokeRequired)
            {
                TryBeginInvoke(CancelCore);
                return;
            }

            CancelCore();
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _pending = false;
            _timer.Tick -= OnTick;
            _timer.Dispose();
        }

        private void RequestCore()
        {
            if (_disposed)
            {
                return;
            }

            if (!CanAccessOwner())
            {
                CancelCore();
                return;
            }

            _pending = true;

            if (!_timer.Enabled)
            {
                _timer.Start();
            }
        }

        private void CancelCore()
        {
            if (_disposed)
            {
                return;
            }

            _pending = false;
            _timer.Stop();
        }

        private void OnTick(object? sender, EventArgs e)
        {
            _ = sender;
            _ = e;

            if (_disposed)
            {
                return;
            }

            _timer.Stop();

            if (!_pending || !CanAccessOwner())
            {
                _pending = false;
                return;
            }

            _pending = false;
            _refreshAction();
        }

        private bool CanAccessOwner()
        {
            return !_disposed && _owner.IsHandleCreated && !_owner.IsDisposed && _canRun();
        }

        private void TryBeginInvoke(Action action)
        {
            try
            {
                _owner.BeginInvoke(action);
            }
            catch (ObjectDisposedException)
            {
            }
            catch (InvalidOperationException)
            {
            }
        }
    }

}
