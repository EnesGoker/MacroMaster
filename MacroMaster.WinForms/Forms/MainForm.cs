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
    private readonly IPlaybackSettingsStore _playbackSettingsStore;
    private readonly IHotkeySettingsStore _hotkeySettingsStore;
    private readonly IMutableHotkeyConfiguration _hotkeyConfiguration;
    private readonly IHotkeyService _hotkeyService;
    private readonly IAppLogger _logger;
    private readonly Button _editHotkeysButton = new();
    private readonly ToolbarControl _toolbarControl = new();
    private readonly PlaybackSettingsControl _playbackSettingsControl = new();
    private readonly EventListControl _eventListControl = new();

    private MacroSession? _activeSession;
    private string? _lastSessionPath;
    private bool _applyingPlaybackSettings;
    private bool _shutdownInProgress;
    private bool _shutdownCompleted;

    public MainForm(
        IApplicationStateService applicationStateService,
        IMacroRecorderService macroRecorderService,
        IMacroPlaybackService macroPlaybackService,
        IMacroStorageService macroStorageService,
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
        _macroPlaybackService.PlaybackStarted += OnPlaybackStateChanged;
        _macroPlaybackService.PlaybackPaused += OnPlaybackStateChanged;
        _macroPlaybackService.PlaybackResumed += OnPlaybackStateChanged;
        _macroPlaybackService.PlaybackStopped += OnPlaybackStopped;

        _activeSession = _macroRecorderService.CurrentSession;
    }

    private void UnsubscribeFromServiceEvents()
    {
        _applicationStateService.StateChanged -= OnApplicationStateChanged;
        _macroRecorderService.RecordingStarted -= OnRecordingStarted;
        _macroRecorderService.EventRecorded -= OnEventRecorded;
        _macroRecorderService.RecordingStopped -= OnRecordingStopped;
        _macroPlaybackService.PlaybackStarted -= OnPlaybackStateChanged;
        _macroPlaybackService.PlaybackPaused -= OnPlaybackStateChanged;
        _macroPlaybackService.PlaybackResumed -= OnPlaybackStateChanged;
        _macroPlaybackService.PlaybackStopped -= OnPlaybackStopped;
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

    private void OnPlaybackStateChanged()
    {
        RequestUiRefresh();
    }

    private void OnPlaybackStopped()
    {
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
    }

    private Task ClearSessionAsync()
    {
        EnsureSessionMutationAllowed();

        _macroRecorderService.Clear();
        _activeSession = null;
        _lastSessionPath = null;
        RefreshUiState();

        return Task.CompletedTask;
    }

    private void AdoptLoadedSession(MacroSession session, string filePath)
    {
        _activeSession = session;
        _lastSessionPath = filePath;
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

    private void RefreshUiState()
    {
        MacroSession? displayedSession = GetSessionForPlayback();
        bool hasSession = displayedSession is { Events.Count: > 0 };

        stateValueLabel.Text = FormatAppState(_applicationStateService.CurrentState);
        sessionNameValueLabel.Text = displayedSession?.Name ?? "Oturum yok";
        eventCountValueLabel.Text = displayedSession?.Events.Count.ToString(CultureInfo.InvariantCulture) ?? "0";
        durationValueLabel.Text = displayedSession is null
            ? "0 ms"
            : FormattableString.Invariant($"{displayedSession.TotalDurationMs} ms");
        sessionFileValueLabel.Text = string.IsNullOrWhiteSpace(_lastSessionPath)
            ? "Kaydedilmedi"
            : Path.GetFileName(_lastSessionPath);
        playbackToggleButton.Text = _macroPlaybackService.IsPaused
            ? "Devam Et"
            : _macroPlaybackService.IsPlaying
                ? "Duraklat"
                : "Oynat";
        recordToggleButton.Text = _macroRecorderService.IsRecording
            ? "Kaydi Durdur"
            : "Kayit Baslat";
        _eventListControl.SetSession(displayedSession);

        bool isIdle = _applicationStateService.IsState(AppState.Idle);

        recordToggleButton.Enabled = !_shutdownInProgress
            && !_macroPlaybackService.IsPlaying
            && !_macroPlaybackService.IsPaused;
        playbackToggleButton.Enabled = !_shutdownInProgress
            && !_macroRecorderService.IsRecording
            && (_macroPlaybackService.IsPlaying || _macroPlaybackService.IsPaused || hasSession);
        stopButton.Enabled = !_shutdownInProgress
            && (_macroRecorderService.IsRecording || _macroPlaybackService.IsPlaying || _macroPlaybackService.IsPaused);
        saveJsonButton.Enabled = !_shutdownInProgress && isIdle && hasSession;
        saveXmlButton.Enabled = !_shutdownInProgress && isIdle && hasSession;
        loadJsonButton.Enabled = !_shutdownInProgress && isIdle;
        loadXmlButton.Enabled = !_shutdownInProgress && isIdle;
        clearSessionButton.Enabled = !_shutdownInProgress && isIdle
            && (displayedSession is not null || !string.IsNullOrWhiteSpace(_lastSessionPath));
        _editHotkeysButton.Enabled = !_shutdownInProgress && isIdle;
        bool playbackSettingsEnabled = !_shutdownInProgress
            && !_macroRecorderService.IsRecording
            && !_macroPlaybackService.IsPlaying
            && !_macroPlaybackService.IsPaused;
        relativeCoordinatesCheckBox.Enabled = playbackSettingsEnabled;
        stopOnErrorCheckBox.Enabled = playbackSettingsEnabled;
        preserveTimingCheckBox.Enabled = playbackSettingsEnabled;
        loopIndefinitelyCheckBox.Enabled = playbackSettingsEnabled;
        initialDelayNumericUpDown.Enabled = playbackSettingsEnabled;
        speedMultiplierNumericUpDown.Enabled = playbackSettingsEnabled
            && !preserveTimingCheckBox.Checked;
        repeatCountNumericUpDown.Enabled = playbackSettingsEnabled
            && !loopIndefinitelyCheckBox.Checked;
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
                recordToggleButton.Enabled,
                stopButton.Enabled,
                playbackToggleButton.Enabled,
                saveJsonButton.Enabled,
                loadJsonButton.Enabled,
                _editHotkeysButton.Enabled));
    }

    private void RequestUiRefresh()
    {
        if (!IsHandleCreated || IsDisposed)
        {
            return;
        }

        if (InvokeRequired)
        {
            BeginInvoke(new Action(RefreshUiState));
            return;
        }

        RefreshUiState();
    }

    private void InitializeDynamicControls()
    {
        actionsFlowLayoutPanel.Visible = false;
        playbackSettingsPanel.Visible = false;
        BuildResponsiveHostLayout();

        _toolbarControl.Name = "toolbarControl";
        _toolbarControl.Dock = DockStyle.Fill;
        _toolbarControl.RecordToggleClicked += recordToggleButton_Click;
        _toolbarControl.PlaybackToggleClicked += playbackToggleButton_Click;
        _toolbarControl.StopClicked += stopButton_Click;
        _toolbarControl.SaveClicked += saveJsonButton_Click;
        _toolbarControl.LoadClicked += loadJsonButton_Click;
        _toolbarControl.HotkeysClicked += editHotkeysButton_Click;

        _playbackSettingsControl.Name = "playbackSettingsControl";
        _playbackSettingsControl.Dock = DockStyle.Fill;
        _playbackSettingsControl.SettingsChanged += playbackSettingsControl_SettingsChanged;

        _eventListControl.Name = "eventListControl";
        _eventListControl.Dock = DockStyle.Fill;

        _editHotkeysButton.AutoSize = true;
        _editHotkeysButton.Name = "editHotkeysButton";
        _editHotkeysButton.Text = "Kisayol Ayarlari";
        _editHotkeysButton.UseVisualStyleBackColor = true;
        _editHotkeysButton.Click += editHotkeysButton_Click;

        actionsFlowLayoutPanel.Controls.Add(_editHotkeysButton);

        int relativeCoordinatesIndex = actionsFlowLayoutPanel.Controls.IndexOf(relativeCoordinatesCheckBox);
        if (relativeCoordinatesIndex >= 0)
        {
            actionsFlowLayoutPanel.Controls.SetChildIndex(_editHotkeysButton, relativeCoordinatesIndex);
        }
    }

    private void BuildResponsiveHostLayout()
    {
        SuspendLayout();

        BackColor = DesignTokens.Background;
        ForeColor = DesignTokens.TextPrimary;
        ClientSize = new Size(1280, 760);
        MinimumSize = new Size(1000, 620);
        Padding = Padding.Empty;

        titleLabel.Font = DesignTokens.FontUiLarge;
        titleLabel.ForeColor = DesignTokens.TextPrimary;
        titleLabel.Dock = DockStyle.Fill;
        titleLabel.TextAlign = ContentAlignment.BottomLeft;

        hotkeySummaryLabel.Font = DesignTokens.FontUiNormal;
        hotkeySummaryLabel.ForeColor = DesignTokens.TextSecondary;
        hotkeySummaryLabel.Dock = DockStyle.Fill;
        hotkeySummaryLabel.TextAlign = ContentAlignment.TopLeft;

        statusTableLayoutPanel.Dock = DockStyle.Fill;
        statusTableLayoutPanel.Margin = Padding.Empty;

        ApplyLegacyDashboardTheme();

        var rootLayoutPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            BackColor = DesignTokens.Background,
            Padding = new Padding(14),
            Margin = Padding.Empty
        };
        rootLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 58f));
        rootLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, DesignTokens.ToolbarHeight + 16f));
        rootLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
        rootLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, DesignTokens.BottomPanelHeight));

        var headerLayoutPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = Color.Transparent,
            Margin = Padding.Empty,
            Padding = new Padding(4, 0, 4, 8)
        };
        headerLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 58f));
        headerLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 42f));
        headerLayoutPanel.Controls.Add(titleLabel, 0, 0);
        headerLayoutPanel.Controls.Add(hotkeySummaryLabel, 0, 1);

        var toolbarHostPanel = CreateCard();
        toolbarHostPanel.ContentPadding = new Padding(18, 8, 18, 8);
        toolbarHostPanel.Body.Controls.Add(_toolbarControl);

        var mainLayoutPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = Color.Transparent,
            Margin = new Padding(0, 6, 0, 8),
            Padding = Padding.Empty
        };
        mainLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30f));
        mainLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70f));

        var sessionHostPanel = CreateSectionCard("Oturum Ozeti");
        sessionHostPanel.Body.Controls.Add(statusTableLayoutPanel);

        var previewHostPanel = CreateSectionCard("Olay / Oturum Onizleme");
        previewHostPanel.Body.Controls.Add(_eventListControl);

        mainLayoutPanel.Controls.Add(sessionHostPanel, 0, 0);
        mainLayoutPanel.Controls.Add(previewHostPanel, 1, 0);

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

        var playbackPlaceholderPanel = CreatePlaybackPlaceholderPanel();
        playbackPlaceholderPanel.Margin = new Padding(0, 0, DesignTokens.GapMedium / 2, 0);

        var playbackSettingsHostPanel = CreateCard();
        playbackSettingsHostPanel.Margin = new Padding(DesignTokens.GapMedium / 2, 0, 0, 0);
        playbackSettingsHostPanel.ContentPadding = Padding.Empty;
        playbackSettingsHostPanel.Body.Controls.Add(_playbackSettingsControl);

        bottomLayoutPanel.Controls.Add(playbackPlaceholderPanel, 0, 0);
        bottomLayoutPanel.Controls.Add(playbackSettingsHostPanel, 1, 0);

        rootLayoutPanel.Controls.Add(headerLayoutPanel, 0, 0);
        rootLayoutPanel.Controls.Add(toolbarHostPanel, 0, 1);
        rootLayoutPanel.Controls.Add(mainLayoutPanel, 0, 2);
        rootLayoutPanel.Controls.Add(bottomLayoutPanel, 0, 3);

        Controls.Clear();
        Controls.Add(rootLayoutPanel);

        ResumeLayout(performLayout: true);
    }

    private void ApplyLegacyDashboardTheme()
    {
        ApplyStatusTableTheme(statusTableLayoutPanel);
    }

    private static void ApplyStatusTableTheme(TableLayoutPanel tableLayoutPanel)
    {
        tableLayoutPanel.CellBorderStyle = TableLayoutPanelCellBorderStyle.None;
        tableLayoutPanel.BackColor = DesignTokens.Surface;
        tableLayoutPanel.Padding = Padding.Empty;

        foreach (Control control in tableLayoutPanel.Controls)
        {
            control.BackColor = DesignTokens.Surface;
            control.ForeColor = tableLayoutPanel.GetColumn(control) == 0
                ? DesignTokens.TextPrimary
                : DesignTokens.TextSecondary;
            control.Font = tableLayoutPanel.GetColumn(control) == 0
                ? DesignTokens.FontUiBold
                : DesignTokens.FontUiNormal;
            control.Margin = Padding.Empty;
            control.Padding = new Padding(4, 0, 4, 0);

            if (control is Label label)
            {
                label.AutoSize = false;
                label.Dock = DockStyle.Fill;
                label.TextAlign = ContentAlignment.MiddleLeft;
            }
        }
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
            12,
            DesignTokens.CardPadding,
            DesignTokens.CardPadding);
        return card;
    }

    private static DashboardCard CreatePlaybackPlaceholderPanel()
    {
        var panel = CreateSectionCard("Oynatma Kontrolu");
        var placeholderLabel = new Label
        {
            Dock = DockStyle.Fill,
            Text = "PlaybackControl siradaki adimda bu alana yerlesecek.",
            Font = DesignTokens.FontUiNormal,
            ForeColor = DesignTokens.TextSecondary,
            BackColor = DesignTokens.Surface,
            TextAlign = ContentAlignment.MiddleCenter
        };

        panel.Body.Controls.Add(placeholderLabel);
        return panel;
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

        hotkeySummaryLabel.Text =
            $"Kisayollar: Kayit {recordHotkey}  |  " +
            $"Oynatma {playbackHotkey}  |  " +
            $"Durdur {stopHotkey}";
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
}
