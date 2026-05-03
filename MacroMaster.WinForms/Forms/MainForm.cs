using MacroMaster.Application.Abstractions;
using MacroMaster.Domain.Enums;
using MacroMaster.Domain.Models;
using MacroMaster.WinForms.Controls;
using MacroMaster.WinForms.Theme;
using System.Globalization;

namespace MacroMaster.WinForms.Forms;

public partial class MainForm : Form
{
    private readonly IApplicationStateService _applicationStateService;
    private readonly IMacroRecorderService _macroRecorderService;
    private readonly IMacroPlaybackService _macroPlaybackService;
    private readonly IMacroStorageService _macroStorageService;
    private readonly IMacroLibraryService _macroLibraryService;
    private readonly IPlaybackSettingsStore _playbackSettingsStore;
    private readonly IHotkeySettingsStore _hotkeySettingsStore;
    private readonly IMutableHotkeyConfiguration _hotkeyConfiguration;
    private readonly IHotkeyService _hotkeyService;
    private readonly IAppLogger _logger;
    private readonly ToolbarControl _toolbarControl = new();
    private readonly PlaybackSettingsControl _playbackSettingsControl = new();
    private readonly EventListControl _eventListControl = new();
    private readonly PlaybackControl _playbackControl = new();
    private readonly MacroLibraryControl _macroLibraryControl = new();
    private readonly SessionSummaryControl _sessionSummaryControl = new();

    private MacroSession? _activeSession;
    private string? _lastSessionPath;
    private int _playedEventCount;
    private int _playedDurationMs;
    private bool _applyingPlaybackSettings;
    private bool _shutdownInProgress;
    private bool _shutdownCompleted;

    public MainForm(
        IApplicationStateService applicationStateService,
        IMacroRecorderService macroRecorderService,
        IMacroPlaybackService macroPlaybackService,
        IMacroStorageService macroStorageService,
        IMacroLibraryService macroLibraryService,
        IPlaybackSettingsStore playbackSettingsStore,
        IHotkeySettingsStore hotkeySettingsStore,
        IMutableHotkeyConfiguration hotkeyConfiguration,
        IHotkeyService hotkeyService,
        IAppLogger logger)
    {
        _applicationStateService = applicationStateService;
        _macroRecorderService = macroRecorderService;
        _macroPlaybackService = macroPlaybackService;
        _macroStorageService = macroStorageService;
        _macroLibraryService = macroLibraryService;
        _playbackSettingsStore = playbackSettingsStore;
        _hotkeySettingsStore = hotkeySettingsStore;
        _hotkeyConfiguration = hotkeyConfiguration;
        _hotkeyService = hotkeyService;
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

    private async Task InitializeHotkeysAsync()
    {
        _hotkeyService.RecordToggleRequested += OnRecordToggleRequested;
        _hotkeyService.PlaybackToggleRequested += OnPlaybackToggleRequested;
        _hotkeyService.StopRequested += OnStopRequested;

        try
        {
            await _hotkeyService.RegisterAsync();
        }
        catch
        {
            _hotkeyService.RecordToggleRequested -= OnRecordToggleRequested;
            _hotkeyService.PlaybackToggleRequested -= OnPlaybackToggleRequested;
            _hotkeyService.StopRequested -= OnStopRequested;
            throw;
        }
    }

    private void OnRecordToggleRequested()
    {
        _ = ExecuteUiActionAsync(HandleRecordToggleAsync, "Kayit degistirme");
    }

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

    private void OnPlaybackToggleRequested()
    {
        _ = ExecuteUiActionAsync(HandlePlaybackToggleAsync, "Oynatma degistirme");
    }

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
            await _macroPlaybackService.PlayAsync(currentSession, BuildPlaybackSettings());
        }
    }

    private void OnStopRequested()
    {
        _ = ExecuteUiActionAsync(HandleStopAsync, "Durdurma istegi");
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

    private async Task ShutdownAsync()
    {
        _hotkeyService.RecordToggleRequested -= OnRecordToggleRequested;
        _hotkeyService.PlaybackToggleRequested -= OnPlaybackToggleRequested;
        _hotkeyService.StopRequested -= OnStopRequested;
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

    private void ShowError(string message, Exception exception)
    {
        if (IsDisposed)
        {
            return;
        }

        string detail = string.IsNullOrWhiteSpace(exception.Message)
            ? exception.GetType().Name
            : exception.Message;

        _logger.Log(
            AppLogLevel.Error,
            nameof(MainForm),
            message,
            exception);

        MessageBox.Show(
            this,
            $"{message}{Environment.NewLine}{Environment.NewLine}{detail}",
            "MacroMaster",
            MessageBoxButtons.OK,
            MessageBoxIcon.Error);
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

    private void OnRecordingStarted()
    {
        _activeSession = _macroRecorderService.CurrentSession;
        _lastSessionPath = null;
        RequestUiRefresh();
    }

    private void OnEventRecorded(MacroEvent macroEvent)
    {
        _ = macroEvent;
        RequestUiRefresh();
    }

    private void OnRecordingStopped(MacroSession session)
    {
        _activeSession = session;
        RequestUiRefresh();
    }

    private void OnPlaybackStarted()
    {
        _playedEventCount = 0;
        _playedDurationMs = 0;
        RequestUiRefresh();
    }

    private void OnPlaybackStateChanged()
    {
        RequestUiRefresh();
    }

    private void OnPlaybackEventPlayed(MacroEvent macroEvent)
    {
        _ = macroEvent;
        _playedEventCount++;
        _playedDurationMs += Math.Max(0, macroEvent.DelayMs);
        RequestUiRefresh();
    }

    private void OnPlaybackStopped()
    {
        _playedEventCount = 0;
        _playedDurationMs = 0;
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

    private void saveXmlButton_Click(object? sender, EventArgs e)
    {
        _ = sender;
        _ = e;
        _ = ExecuteUiActionAsync(SaveXmlAsync, "XML kaydetme");
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

    private async Task SaveJsonAsync()
    {
        MacroSession session = GetRequiredSession();
        EnsureSessionMutationAllowed();

        using var dialog = new SaveFileDialog
        {
            Filter = "JSON makro (*.json)|*.json|Tum dosyalar (*.*)|*.*",
            FileName = BuildDefaultFileName(session.Name, ".json"),
            AddExtension = true,
            DefaultExt = "json",
            RestoreDirectory = true
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        await _macroStorageService.SaveAsJsonAsync(session, dialog.FileName);
        _lastSessionPath = dialog.FileName;
        await RefreshMacroLibraryAsync();
        RefreshUiState();
    }

    private async Task SaveToLibraryAsync()
    {
        MacroSession session = GetRequiredSession();
        EnsureSessionMutationAllowed();

        string filePath = await _macroLibraryService.SaveAsync(session);
        _lastSessionPath = filePath;
        await RefreshMacroLibraryAsync();
        RefreshUiState();
    }

    private async Task LoadJsonAsync()
    {
        EnsureSessionMutationAllowed();

        using var dialog = new OpenFileDialog
        {
            Filter = "JSON makro (*.json)|*.json|Tum dosyalar (*.*)|*.*",
            RestoreDirectory = true
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        MacroSession session = await _macroStorageService.LoadFromJsonAsync(dialog.FileName);
        AdoptLoadedSession(session, dialog.FileName);
        await RefreshMacroLibraryAsync();
    }

    private async Task SaveXmlAsync()
    {
        MacroSession session = GetRequiredSession();
        EnsureSessionMutationAllowed();

        using var dialog = new SaveFileDialog
        {
            Filter = "XML makro (*.xml)|*.xml|Tum dosyalar (*.*)|*.*",
            FileName = BuildDefaultFileName(session.Name, ".xml"),
            AddExtension = true,
            DefaultExt = "xml",
            RestoreDirectory = true
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        await _macroStorageService.SaveAsXmlAsync(session, dialog.FileName);
        _lastSessionPath = dialog.FileName;
        await RefreshMacroLibraryAsync();
        RefreshUiState();
    }

    private async Task LoadXmlAsync()
    {
        EnsureSessionMutationAllowed();

        using var dialog = new OpenFileDialog
        {
            Filter = "XML makro (*.xml)|*.xml|Tum dosyalar (*.*)|*.*",
            RestoreDirectory = true
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        MacroSession session = await _macroStorageService.LoadFromXmlAsync(dialog.FileName);
        AdoptLoadedSession(session, dialog.FileName);
        await RefreshMacroLibraryAsync();
    }

    private async Task ClearSessionAsync()
    {
        EnsureSessionMutationAllowed();

        _macroRecorderService.Clear();
        _activeSession = null;
        _lastSessionPath = null;
        _playedEventCount = 0;
        _playedDurationMs = 0;
        await RefreshMacroLibraryAsync();
        RefreshUiState();
    }

    private async Task LoadLibraryMacroAsync(MacroLibraryEntry item)
    {
        ArgumentNullException.ThrowIfNull(item);
        EnsureSessionMutationAllowed();

        MacroSession session = await _macroLibraryService.LoadAsync(item.FilePath);
        AdoptLoadedSession(session, item.FilePath);
        await RefreshMacroLibraryAsync();
    }

    private async Task RenameLibraryMacroAsync(MacroLibraryEntry item)
    {
        ArgumentNullException.ThrowIfNull(item);
        EnsureSessionMutationAllowed();

        using var dialog = new MacroNameEditDialog(item.Name);

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        string previousFilePath = item.FilePath;
        string renamedFilePath = await _macroLibraryService.RenameAsync(
            previousFilePath,
            dialog.MacroName);

        if (IsSamePath(_lastSessionPath, previousFilePath))
        {
            _lastSessionPath = renamedFilePath;

            if (_activeSession is not null)
            {
                _activeSession.Name = Path.GetFileNameWithoutExtension(renamedFilePath);
            }
        }

        await RefreshMacroLibraryAsync();
        RefreshUiState();
    }

    private async Task DeleteLibraryMacroAsync(MacroLibraryEntry item)
    {
        ArgumentNullException.ThrowIfNull(item);
        EnsureSessionMutationAllowed();

        DialogResult confirmation = MessageBox.Show(
            this,
            $"{item.Name} kutuphaneden silinsin mi?",
            "Makro Sil",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning,
            MessageBoxDefaultButton.Button2);

        if (confirmation != DialogResult.Yes)
        {
            return;
        }

        await _macroLibraryService.DeleteAsync(item.FilePath);

        if (IsSamePath(_lastSessionPath, item.FilePath))
        {
            _lastSessionPath = null;
        }

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

        if (dialog.ShowDialog(this) != DialogResult.OK)
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

    private async Task RefreshMacroLibraryAsync()
    {
        IReadOnlyList<MacroLibraryEntry> entries = await _macroLibraryService.ListAsync();
        _macroLibraryControl.SetItems(entries, _lastSessionPath);
    }

    private void AdoptLoadedSession(MacroSession session, string filePath)
    {
        _activeSession = session;
        _lastSessionPath = filePath;
        _playedEventCount = 0;
        _playedDurationMs = 0;
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

    private void RefreshUiState(bool forceEventListReload = false)
    {
        MacroSession? displayedSession = GetSessionForPlayback();
        bool hasSession = displayedSession is { Events.Count: > 0 };
        PlaybackSettings playbackSettings = BuildPlaybackSettings();

        _eventListControl.SetSession(displayedSession, forceEventListReload);

        bool isIdle = _applicationStateService.IsState(AppState.Idle);
        bool canRecord = !_shutdownInProgress
            && !_macroPlaybackService.IsPlaying
            && !_macroPlaybackService.IsPaused;
        bool canPlayback = !_shutdownInProgress
            && !_macroRecorderService.IsRecording
            && (_macroPlaybackService.IsPlaying || _macroPlaybackService.IsPaused || hasSession);
        bool canStop = !_shutdownInProgress
            && (_macroRecorderService.IsRecording || _macroPlaybackService.IsPlaying || _macroPlaybackService.IsPaused);
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
                canStop));
        _sessionSummaryControl.UpdateState(
            new SessionSummaryState(
                FormatAppState(_applicationStateService.CurrentState),
                displayedSession?.Name ?? "Oturum yok",
                displayedSession?.Events.Count ?? 0,
                displayedSession?.TotalDurationMs ?? 0,
                string.IsNullOrWhiteSpace(_lastSessionPath)
                    ? "Kaydedilmedi"
                    : Path.GetFileName(_lastSessionPath)));
    }

    private void RequestUiRefresh()
    {
        if (!IsHandleCreated || IsDisposed)
        {
            return;
        }

        if (InvokeRequired)
        {
            BeginInvoke(new Action(() => RefreshUiState()));
            return;
        }

        RefreshUiState();
    }

    private void InitializeDynamicControls()
    {
        BuildResponsiveHostLayout();

        _toolbarControl.Name = "toolbarControl";
        _toolbarControl.Dock = DockStyle.Fill;
        _toolbarControl.RecordToggleClicked += recordToggleButton_Click;
        _toolbarControl.PlaybackToggleClicked += playbackToggleButton_Click;
        _toolbarControl.StopClicked += stopButton_Click;
        _toolbarControl.SaveLibraryClicked += saveLibraryButton_Click;
        _toolbarControl.SaveJsonClicked += saveJsonButton_Click;
        _toolbarControl.SaveXmlClicked += saveXmlButton_Click;
        _toolbarControl.LoadJsonClicked += loadJsonButton_Click;
        _toolbarControl.LoadXmlClicked += loadXmlButton_Click;
        _toolbarControl.HotkeysClicked += editHotkeysButton_Click;

        _playbackSettingsControl.Name = "playbackSettingsControl";
        _playbackSettingsControl.Dock = DockStyle.Fill;
        _playbackSettingsControl.SettingsChanged += playbackSettingsControl_SettingsChanged;

        _eventListControl.Name = "eventListControl";
        _eventListControl.Dock = DockStyle.Fill;
        _eventListControl.EventEditRequested += eventListControl_EventEditRequested;

        _macroLibraryControl.Name = "macroLibraryControl";
        _macroLibraryControl.Dock = DockStyle.Fill;
        _macroLibraryControl.AddRequested += loadJsonButton_Click;
        _macroLibraryControl.LoadRequested += macroLibraryControl_LoadRequested;
        _macroLibraryControl.RenameRequested += macroLibraryControl_RenameRequested;
        _macroLibraryControl.DeleteRequested += macroLibraryControl_DeleteRequested;

        _sessionSummaryControl.Name = "sessionSummaryControl";
        _sessionSummaryControl.Dock = DockStyle.Fill;

        _playbackControl.Name = "playbackControl";
        _playbackControl.Dock = DockStyle.Fill;
        _playbackControl.PlaybackClicked += playbackToggleButton_Click;
        _playbackControl.StopClicked += stopButton_Click;
    }

    private void BuildResponsiveHostLayout()
    {
        SuspendLayout();

        BackColor = DesignTokens.Background;
        ForeColor = DesignTokens.TextPrimary;
        AutoScaleMode = AutoScaleMode.None;
        ClientSize = new Size(DesignTokens.Scale(1280), DesignTokens.Scale(760));
        MinimumSize = new Size(DesignTokens.Scale(640), DesignTokens.Scale(480));
        Padding = Padding.Empty;

        titleLabel.Font = DesignTokens.FontUiLarge;
        titleLabel.ForeColor = DesignTokens.TextPrimary;
        titleLabel.Dock = DockStyle.Fill;
        titleLabel.TextAlign = ContentAlignment.MiddleLeft;

        var rootLayoutPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            BackColor = DesignTokens.Background,
            Padding = new Padding(
                DesignTokens.Scale(18),
                DesignTokens.Scale(14),
                DesignTokens.Scale(18),
                DesignTokens.Scale(16)),
            Margin = Padding.Empty
        };
        rootLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, DesignTokens.Scale(54)));
        rootLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, DesignTokens.ToolbarHeight + DesignTokens.Scale(18)));
        rootLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 72f));
        rootLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 28f));

        var headerLayoutPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 1,
            BackColor = Color.Transparent,
            Margin = Padding.Empty,
            Padding = new Padding(DesignTokens.Scale(4), 0, DesignTokens.Scale(4), DesignTokens.Scale(8))
        };
        headerLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
        headerLayoutPanel.Controls.Add(titleLabel, 0, 0);

        var toolbarHostPanel = CreateCard();
        toolbarHostPanel.ContentPadding = new Padding(
            DesignTokens.Scale(18),
            DesignTokens.Scale(7),
            DesignTokens.Scale(18),
            DesignTokens.Scale(7));
        toolbarHostPanel.Body.Controls.Add(_toolbarControl);

        var mainLayoutPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 1,
            BackColor = Color.Transparent,
            Margin = new Padding(0, DesignTokens.Scale(8), 0, DesignTokens.Scale(10)),
            Padding = Padding.Empty
        };
        mainLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25.5f));
        mainLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 56.5f));
        mainLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 18f));

        var libraryHostPanel = CreateCard();
        libraryHostPanel.Margin = new Padding(0, 0, DesignTokens.GapMedium / 2, 0);
        libraryHostPanel.ContentPadding = new Padding(DesignTokens.CardPadding);
        libraryHostPanel.Body.Controls.Add(_macroLibraryControl);

        var previewHostPanel = CreateSectionCard("Olay / Oturum Onizleme");
        previewHostPanel.Margin = new Padding(DesignTokens.GapMedium / 2, 0, DesignTokens.GapMedium / 2, 0);
        previewHostPanel.Body.Controls.Add(_eventListControl);

        var sessionHostPanel = CreateSectionCard("Oturum Ozeti");
        sessionHostPanel.Margin = new Padding(DesignTokens.GapMedium / 2, 0, DesignTokens.Scale(6), 0);
        sessionHostPanel.Body.Controls.Add(_sessionSummaryControl);

        mainLayoutPanel.Controls.Add(libraryHostPanel, 0, 0);
        mainLayoutPanel.Controls.Add(previewHostPanel, 1, 0);
        mainLayoutPanel.Controls.Add(sessionHostPanel, 2, 0);

        var bottomLayoutPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = Color.Transparent,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        bottomLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 48f));
        bottomLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 52f));

        var playbackControlCard = CreateSectionCard("Oynatma Kontrolu");
        playbackControlCard.Margin = new Padding(0, 0, DesignTokens.GapMedium / 2, 0);
        playbackControlCard.Body.Controls.Add(_playbackControl);

        var playbackSettingsHostPanel = CreateSectionCard("Oynatma Ayarlari");
        playbackSettingsHostPanel.Margin = new Padding(DesignTokens.GapMedium / 2, 0, 0, 0);
        playbackSettingsHostPanel.ContentPadding = Padding.Empty;
        playbackSettingsHostPanel.Body.Controls.Add(_playbackSettingsControl);

        bottomLayoutPanel.Controls.Add(playbackControlCard, 0, 0);
        bottomLayoutPanel.Controls.Add(playbackSettingsHostPanel, 1, 0);

        rootLayoutPanel.Controls.Add(headerLayoutPanel, 0, 0);
        rootLayoutPanel.Controls.Add(toolbarHostPanel, 0, 1);
        rootLayoutPanel.Controls.Add(mainLayoutPanel, 0, 2);
        rootLayoutPanel.Controls.Add(bottomLayoutPanel, 0, 3);

        Controls.Clear();
        Controls.Add(rootLayoutPanel);

        ResumeLayout(performLayout: true);
    }

    private static DashboardCard CreateCard()
    {
        return new DashboardCard
        {
            Dock = DockStyle.Fill,
            ShowHeader = false,
            Margin = Padding.Empty
        };
    }

    private static DashboardCard CreateSectionCard(string title)
    {
        var card = CreateCard();
        card.ShowHeader = true;
        card.Title = title;
        card.ContentPadding = new Padding(
            DesignTokens.CardPadding,
            DesignTokens.Scale(16),
            DesignTokens.CardPadding,
            DesignTokens.CardPadding);
        return card;
    }

    private void playbackSettingsControl_SettingsChanged(object? sender, EventArgs e)
    {
        _ = sender;
        _ = e;

        if (_applyingPlaybackSettings)
        {
            return;
        }

        RefreshUiState();
    }

    private async Task EditHotkeysAsync()
    {
        EnsureHotkeyMutationAllowed();

        using var dialog = new HotkeySettingsDialog(_hotkeyConfiguration.Snapshot());

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        await ApplyHotkeySettingsAsync(dialog.SelectedHotkeySettings);
    }

    private async Task ApplyHotkeySettingsAsync(HotkeySettings hotkeySettings)
    {
        ArgumentNullException.ThrowIfNull(hotkeySettings);

        EnsureHotkeyMutationAllowed();
        HotkeySettingsValidator.Validate(hotkeySettings, "Kisayol ayari uygulama");

        HotkeySettings previousSettings = _hotkeyConfiguration.Snapshot();
        bool wasRegistered = _hotkeyService.IsRegistered;

        try
        {
            if (wasRegistered)
            {
                await _hotkeyService.UnregisterAsync();
            }

            _hotkeyConfiguration.Apply(hotkeySettings);

            if (wasRegistered)
            {
                await _hotkeyService.RegisterAsync();
            }

            await _hotkeySettingsStore.SaveAsync(_hotkeyConfiguration.Snapshot());
        }
        catch (Exception applyException)
        {
            List<Exception> recoveryErrors = [];

            try
            {
                if (_hotkeyService.IsRegistered)
                {
                    await _hotkeyService.UnregisterAsync();
                }
            }
            catch (Exception unregisterException)
            {
                recoveryErrors.Add(
                    new InvalidOperationException(
                        "Kismen uygulanan kisayol kaydi temiz bir sekilde geri alinamadi.",
                        unregisterException));
            }

            try
            {
                _hotkeyConfiguration.Apply(previousSettings);
            }
            catch (Exception restoreConfigurationException)
            {
                recoveryErrors.Add(
                    new InvalidOperationException(
                        "Onceki kisayol yapilandirmasi geri yuklenemedi.",
                        restoreConfigurationException));
            }

            if (wasRegistered)
            {
                try
                {
                    await _hotkeyService.RegisterAsync();
                }
                catch (Exception restoreRegistrationException)
                {
                    recoveryErrors.Add(
                        new InvalidOperationException(
                            "Onceki kisayol kaydi yeniden yuklenemedi.",
                            restoreRegistrationException));
                }
            }

            UpdateHotkeySummary();
            RefreshUiState();

            if (recoveryErrors.Count > 0)
            {
                List<Exception> aggregateErrors = [applyException];
                aggregateErrors.AddRange(recoveryErrors);
                throw new AggregateException(
                    "Kisayol ayari guncellemesi basarisiz oldu ve geri alma islemi tam olarak tamamlanamadi.",
                    aggregateErrors);
            }

            throw;
        }

        UpdateHotkeySummary();
        RefreshUiState();
    }

    private void UpdateHotkeySummary()
    {
        string recordHotkey = FormatHotkey(_hotkeyConfiguration.RecordToggleHotkey);
        string playbackHotkey = FormatHotkey(_hotkeyConfiguration.PlaybackToggleHotkey);
        string stopHotkey = FormatHotkey(_hotkeyConfiguration.StopHotkey);

        _toolbarControl.SetHotkeyHints(recordHotkey, stopHotkey, playbackHotkey, "F12");
    }

    private static string FormatHotkey(HotkeyBinding hotkeyBinding)
    {
        List<string> parts = [];

        if (hotkeyBinding.Modifiers.HasFlag(HotkeyModifiers.Control))
        {
            parts.Add("Ctrl");
        }

        if (hotkeyBinding.Modifiers.HasFlag(HotkeyModifiers.Shift))
        {
            parts.Add("Shift");
        }

        if (hotkeyBinding.Modifiers.HasFlag(HotkeyModifiers.Alt))
        {
            parts.Add("Alt");
        }

        if (hotkeyBinding.Modifiers.HasFlag(HotkeyModifiers.Windows))
        {
            parts.Add("Win");
        }

        parts.Add(((Keys)hotkeyBinding.VirtualKeyCode).ToString());
        return string.Join("+", parts);
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

    private static string BuildDefaultFileName(string sessionName, string extension)
    {
        char[] invalidCharacters = Path.GetInvalidFileNameChars();
        string sanitizedName = new string(sessionName
            .Where(character => !invalidCharacters.Contains(character))
            .ToArray());

        if (string.IsNullOrWhiteSpace(sanitizedName))
        {
            sanitizedName = "MakroOturumu";
        }

        return sanitizedName + extension;
    }

    private static bool IsSamePath(string? left, string right)
    {
        return !string.IsNullOrWhiteSpace(left)
            && string.Equals(
                Path.GetFullPath(left),
                Path.GetFullPath(right),
                StringComparison.OrdinalIgnoreCase);
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

    private PlaybackControlState BuildPlaybackControlState(
        MacroSession? session,
        PlaybackSettings playbackSettings,
        bool canPlayback,
        bool canStop)
    {
        int repeatCount = Math.Max(playbackSettings.RepeatCount, 1);
        int sessionEventCount = session?.Events.Count ?? 0;
        int totalEventCount = playbackSettings.LoopIndefinitely
            ? sessionEventCount
            : sessionEventCount * repeatCount;
        int totalDurationMs = playbackSettings.LoopIndefinitely
            ? session?.TotalDurationMs ?? 0
            : (session?.TotalDurationMs ?? 0) * repeatCount + playbackSettings.InitialDelayMs;
        int playedEventCount = _macroPlaybackService.IsPlaying || _macroPlaybackService.IsPaused
            ? _playedEventCount
            : 0;
        int playedDurationMs = _macroPlaybackService.IsPlaying || _macroPlaybackService.IsPaused
            ? _playedDurationMs
            : 0;

        return new PlaybackControlState(
            FormatAppState(_applicationStateService.CurrentState),
            playedEventCount,
            totalEventCount,
            playedDurationMs,
            totalDurationMs,
            playbackSettings.PreserveOriginalTiming ? 1.0 : playbackSettings.SpeedMultiplier,
            repeatCount,
            playbackSettings.LoopIndefinitely,
            playbackSettings.InitialDelayMs,
            canPlayback,
            canStop,
            _macroPlaybackService.IsPaused
                ? PlaybackButtonState.Resume
                : _macroPlaybackService.IsPlaying
                    ? PlaybackButtonState.Pause
                    : PlaybackButtonState.Play);
    }
}
