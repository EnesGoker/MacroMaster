using MacroMaster.WinForms.Controls;
using MacroMaster.WinForms.Theme;

namespace MacroMaster.WinForms.Forms;

public partial class MainForm
{
    private void InitializeDynamicControls()
    {
        BuildResponsiveHostLayout(applyInitialWindowMetrics: true);

        _recordingEventRefreshThrottle = new UiRefreshThrottle(
            this,
            intervalMs: 50,
            refreshAction: () => RefreshUiState(),
            canRun: CanRunDeferredUiRefresh);
        _playbackTelemetryRefreshThrottle = new UiRefreshThrottle(
            this,
            PlaybackTelemetryRefreshIntervalMs,
            RefreshPlaybackTelemetry,
            CanRunDeferredUiRefresh);
        _playbackSelectionRefreshThrottle = new UiRefreshThrottle(
            this,
            PlaybackSelectionRefreshIntervalMs,
            RefreshPlaybackSelection,
            CanRunDeferredUiRefresh);

        _toolbarControl.Name = "toolbarControl";
        _toolbarControl.Dock = DockStyle.Fill;
        _toolbarControl.RecordToggleClicked += recordToggleButton_Click;
        _toolbarControl.PlaybackToggleClicked += playbackToggleButton_Click;
        _toolbarControl.StopClicked += stopButton_Click;
        _toolbarControl.SaveLibraryClicked += saveLibraryButton_Click;
        _toolbarControl.SaveJsonClicked += saveJsonButton_Click;
        _toolbarControl.SaveXmlClicked += saveXmlButton_Click;
        _toolbarControl.SaveHtmlReportClicked += saveHtmlReportButton_Click;
        _toolbarControl.SaveTextReportClicked += saveTextReportButton_Click;
        _toolbarControl.LoadJsonClicked += loadJsonButton_Click;
        _toolbarControl.LoadXmlClicked += loadXmlButton_Click;
        _toolbarControl.HotkeysClicked += editHotkeysButton_Click;

        _titleBarControl.Name = "titleBarControl";
        _titleBarControl.Dock = DockStyle.Fill;
        _titleBarControl.SetTitle("MacroMaster Kontrol Merkezi");
        _titleBarControl.DragRequested += titleBarControl_DragRequested;
        _titleBarControl.MinimizeRequested += titleBarControl_MinimizeRequested;
        _titleBarControl.MaximizeRestoreRequested += titleBarControl_MaximizeRestoreRequested;
        _titleBarControl.CloseRequested += titleBarControl_CloseRequested;

        _playbackSettingsControl.Name = "playbackSettingsControl";
        _playbackSettingsControl.Dock = DockStyle.Fill;
        _playbackSettingsControl.SettingsChanged += playbackSettingsControl_SettingsChanged;

        _eventListControl.Name = "eventListControl";
        _eventListControl.Dock = DockStyle.Fill;
        _eventListControl.EventEditRequested += eventListControl_EventEditRequested;

        _macroLibraryControl.Name = "macroLibraryControl";
        _macroLibraryControl.Dock = DockStyle.Fill;
        _macroLibraryControl.AddRequested += importLibraryMacroButton_Click;
        _macroLibraryControl.LoadRequested += macroLibraryControl_LoadRequested;
        _macroLibraryControl.RenameRequested += macroLibraryControl_RenameRequested;
        _macroLibraryControl.DeleteRequested += macroLibraryControl_DeleteRequested;
        _macroLibraryControl.FavoriteToggled += macroLibraryControl_FavoriteToggled;
        _macroLibraryControl.OptimizeRequested += macroLibraryControl_OptimizeRequested;

        _sessionSummaryControl.Name = "sessionSummaryControl";
        _sessionSummaryControl.Dock = DockStyle.Fill;
        _sessionSummaryControl.PreviewMapRequested += sessionSummaryControl_PreviewMapRequested;

        _playbackControl.Name = "playbackControl";
        _playbackControl.Dock = DockStyle.Fill;
        _playbackControl.SkipBackClicked += playbackSkipBackButton_Click;
        _playbackControl.StepBackClicked += playbackStepBackButton_Click;
        _playbackControl.PlaybackClicked += playbackToggleButton_Click;
        _playbackControl.StepForwardClicked += playbackStepForwardButton_Click;
        _playbackControl.StopClicked += stopButton_Click;
    }

    private void BuildResponsiveHostLayout(bool applyInitialWindowMetrics)
    {
        SuspendLayout();
        Control? previousRoot = Controls.Count > 0 ? Controls[0] : null;
        if (previousRoot is not null)
        {
            DetachReusableDashboardControls(previousRoot);
        }

        BackColor = DesignTokens.Background;
        ForeColor = DesignTokens.TextPrimary;
        AutoScaleMode = AutoScaleMode.None;
        if (applyInitialWindowMetrics)
        {
            ClientSize = new Size(DesignTokens.Scale(1280), DesignTokens.Scale(760));
        }

        MinimumSize = new Size(DesignTokens.Scale(640), DesignTokens.Scale(480));
        Padding = Padding.Empty;
        ApplyWindowChromeConfiguration();

        var rootLayoutPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            BackColor = DesignTokens.Background,
            Padding = new Padding(
                DesignTokens.Scale(18),
                0,
                DesignTokens.Scale(18),
                DesignTokens.Scale(16)),
            Margin = Padding.Empty
        };
        rootLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, DesignTokens.TitleBarHeight - DesignTokens.Scale(8)));
        rootLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, DesignTokens.ToolbarHeight + DesignTokens.Scale(24)));
        rootLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 72f));
        rootLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 28f));

        var headerLayoutPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 1,
            BackColor = Color.Transparent,
            Margin = Padding.Empty,
            Padding = new Padding(0, 0, DesignTokens.Scale(4), 0)
        };
        headerLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
        headerLayoutPanel.Controls.Add(_titleBarControl, 0, 0);

        var toolbarHostPanel = CreateCard();
        toolbarHostPanel.Margin = new Padding(0, DesignTokens.Scale(16), 0, DesignTokens.Scale(4));
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

        var previewHostPanel = CreateCard();
        previewHostPanel.Margin = new Padding(DesignTokens.GapMedium / 2, 0, DesignTokens.GapMedium / 2, 0);
        previewHostPanel.ContentPadding = new Padding(DesignTokens.CardPadding);
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
        playbackSettingsHostPanel.Body.Controls.Add(_playbackSettingsControl);

        bottomLayoutPanel.Controls.Add(playbackControlCard, 0, 0);
        bottomLayoutPanel.Controls.Add(playbackSettingsHostPanel, 1, 0);

        rootLayoutPanel.Controls.Add(headerLayoutPanel, 0, 0);
        rootLayoutPanel.Controls.Add(toolbarHostPanel, 0, 1);
        rootLayoutPanel.Controls.Add(mainLayoutPanel, 0, 2);
        rootLayoutPanel.Controls.Add(bottomLayoutPanel, 0, 3);

        Controls.Clear();
        Controls.Add(rootLayoutPanel);
        previousRoot?.Dispose();

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

    private void DetachReusableDashboardControls(Control root)
    {
        DetachReusableControl(root, _titleBarControl);
        DetachReusableControl(root, _toolbarControl);
        DetachReusableControl(root, _playbackSettingsControl);
        DetachReusableControl(root, _eventListControl);
        DetachReusableControl(root, _macroLibraryControl);
        DetachReusableControl(root, _sessionSummaryControl);
        DetachReusableControl(root, _playbackControl);
    }

    private static void DetachReusableControl(Control root, Control reusableControl)
    {
        Control? parent = reusableControl.Parent;
        if (parent is null)
        {
            return;
        }

        Control? cursor = parent;
        bool parentBelongsToRoot = false;
        while (cursor is not null)
        {
            if (ReferenceEquals(cursor, root))
            {
                parentBelongsToRoot = true;
                break;
            }

            cursor = cursor.Parent;
        }

        if (!parentBelongsToRoot)
        {
            return;
        }

        parent.Controls.Remove(reusableControl);
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
}

