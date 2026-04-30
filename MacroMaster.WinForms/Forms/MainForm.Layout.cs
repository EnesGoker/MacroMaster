using MacroMaster.WinForms.Controls;
using MacroMaster.WinForms.Theme;

namespace MacroMaster.WinForms.Forms;

public partial class MainForm
{
    private TableLayoutPanel _chromeLayout = null!;
    private Panel _contentHost = null!;
    private TableLayoutPanel _rootLayout = null!;
    private MenuStrip _menuStrip = null!;
    private ToolStrip _toolStrip = null!;
    private StatusStrip _statusStrip = null!;
    private ToolStripMenuItem _recordMenuItem = null!;
    private ToolStripMenuItem _playMenuItem = null!;
    private ToolStripMenuItem _stopMenuItem = null!;
    private ToolStripMenuItem _saveMenuItem = null!;
    private ToolStripMenuItem _loadMenuItem = null!;
    private ToolStripMenuItem _clearMenuItem = null!;
    private ToolStripButton _saveToolButton = null!;
    private ToolStripButton _loadToolButton = null!;
    private ToolStripButton _clearToolButton = null!;
    private ToolStripStatusLabel _statusStripStateLabel = null!;
    private ToolStripStatusLabel _statusStripEventCountLabel = null!;
    private ToolStripStatusLabel _statusStripSessionLabel = null!;
    private ToolStripStatusLabel _statusStripHotkeysLabel = null!;
    private NumericUpDown _speedNumeric = null!;
    private NumericUpDown _repeatCountNumeric = null!;
    private NumericUpDown _initialDelayNumeric = null!;
    private CheckBox _loopPlaybackCheckBox = null!;
    private CheckBox _relativeCoordinatesCheckBox = null!;
    private CheckBox _preserveTimingCheckBox = null!;
    private CheckBox _stopOnErrorCheckBox = null!;
    private Label _sessionStatusValueLabel = null!;
    private Label _totalEventsValueLabel = null!;
    private Label _totalDurationValueLabel = null!;
    private Label _sessionNameValueLabel = null!;
    private Label _controlHintLabel = null!;
    private Label _detailHeaderValueLabel = null!;
    private Label _detailTypeValueLabel = null!;
    private Label _detailActionValueLabel = null!;
    private Label _detailTimeValueLabel = null!;
    private Label _detailDelayValueLabel = null!;
    private Label _detailPositionValueLabel = null!;
    private Label _detailInputValueLabel = null!;
    private Label _playbackStatusValueLabel = null!;
    private Label _playbackModeValueLabel = null!;
    private Label _playbackSpeedValueLabel = null!;
    private Label _playbackRepeatValueLabel = null!;
    private Label _playbackDelayValueLabel = null!;
    private Label _playbackProgressValueLabel = null!;
    private Label _currentEventValueLabel = null!;
    private ModernButton _recordButton = null!;
    private ModernButton _playButton = null!;
    private ModernButton _stopButton = null!;
    private DataGridView _eventGrid = null!;
    private RichTextBox _eventDetailBox = null!;
    private ProgressBar _playbackProgressBar = null!;

    private void InitializeLayout()
    {
        SuspendLayout();

        Controls.Clear();
        AutoScaleMode = AutoScaleMode.Dpi;
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(1260, 820);
        ClientSize = new Size(1440, 920);
        Text = "MacroMaster Kontrol Merkezi";

        _menuStrip = CreateMenuStrip();
        _toolStrip = CreateToolStrip();
        _statusStrip = CreateStatusStrip();

        _rootLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            BackColor = Color.Transparent,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        _rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 112f));
        _rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 172f));
        _rootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

        _rootLayout.Controls.Add(CreateHeaderCard(), 0, 0);
        _rootLayout.Controls.Add(CreateSummaryAndActionsRow(), 0, 1);
        _rootLayout.Controls.Add(CreateContentRow(), 0, 2);

        _contentHost = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.Transparent,
            Padding = AppSpacing.PagePadding
        };
        _contentHost.Controls.Add(_rootLayout);

        _chromeLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            BackColor = Color.Transparent,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        _chromeLayout.RowStyles.Add(new RowStyle());
        _chromeLayout.RowStyles.Add(new RowStyle());
        _chromeLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
        _chromeLayout.RowStyles.Add(new RowStyle());

        _chromeLayout.Controls.Add(_menuStrip, 0, 0);
        _chromeLayout.Controls.Add(_toolStrip, 0, 1);
        _chromeLayout.Controls.Add(_contentHost, 0, 2);
        _chromeLayout.Controls.Add(_statusStrip, 0, 3);

        MainMenuStrip = _menuStrip;
        Controls.Add(_chromeLayout);

        ResumeLayout(performLayout: true);
    }

    private MenuStrip CreateMenuStrip()
    {
        var menuStrip = new MenuStrip
        {
            Dock = DockStyle.Fill,
            Stretch = true
        };

        var fileMenu = new ToolStripMenuItem("&Dosya");
        _loadMenuItem = CreateMenuActionItem("Makro Yukle...", async () => await LoadSessionAsync());
        _saveMenuItem = CreateMenuActionItem("Makro Kaydet...", async () => await SaveSessionAsync());
        var exitMenuItem = new ToolStripMenuItem("Cikis");
        exitMenuItem.Click += (_, _) => Close();
        fileMenu.DropDownItems.AddRange(
            [
                _loadMenuItem,
                _saveMenuItem,
                new ToolStripSeparator(),
                exitMenuItem
            ]);

        var macroMenu = new ToolStripMenuItem("&Makro");
        _recordMenuItem = CreateMenuActionItem("Kaydi Baslat / Durdur", async () => await ToggleRecordingAsync());
        _recordMenuItem.ShortcutKeyDisplayString = "F8";
        _clearMenuItem = CreateMenuActionItem("Oturumu Temizle", ClearCurrentSession);
        macroMenu.DropDownItems.AddRange(
            [
                _recordMenuItem,
                new ToolStripSeparator(),
                _clearMenuItem
            ]);

        var playbackMenu = new ToolStripMenuItem("&Oynatma");
        _playMenuItem = CreateMenuActionItem("Oynat / Duraklat", async () => await TogglePlaybackAsync());
        _playMenuItem.ShortcutKeyDisplayString = "F9";
        _stopMenuItem = CreateMenuActionItem("Durdur", async () => await StopAsync());
        _stopMenuItem.ShortcutKeyDisplayString = "F10";
        playbackMenu.DropDownItems.AddRange(
            [
                _playMenuItem,
                _stopMenuItem
            ]);

        var settingsMenu = new ToolStripMenuItem("&Ayarlar");
        var hotkeysMenuItem = new ToolStripMenuItem("Kisayol Bilgileri");
        hotkeysMenuItem.Click += (_, _) => ShowHotkeyReference();
        settingsMenu.DropDownItems.Add(hotkeysMenuItem);

        var helpMenu = new ToolStripMenuItem("&Yardim");
        var aboutMenuItem = new ToolStripMenuItem("Hakkinda");
        aboutMenuItem.Click += (_, _) => ShowAboutDialog();
        helpMenu.DropDownItems.Add(aboutMenuItem);

        menuStrip.Items.AddRange(
            [
                fileMenu,
                macroMenu,
                playbackMenu,
                settingsMenu,
                helpMenu
            ]);

        return menuStrip;
    }

    private ToolStrip CreateToolStrip()
    {
        var toolStrip = new ToolStrip
        {
            Dock = DockStyle.Fill,
            GripStyle = ToolStripGripStyle.Hidden,
            Stretch = true,
            AutoSize = false,
            Height = 42,
            Padding = new Padding(AppSpacing.Sm, AppSpacing.Xs, AppSpacing.Sm, AppSpacing.Xs)
        };

        _saveToolButton = CreateToolButton("Kaydet", async () => await SaveSessionAsync());
        _loadToolButton = CreateToolButton("Yukle", async () => await LoadSessionAsync());
        _clearToolButton = CreateToolButton("Temizle", ClearCurrentSession);
        var hotkeysToolButton = CreateToolButton("Kisayollar", ShowHotkeyReference);
        var aboutToolButton = CreateToolButton("Hakkinda", ShowAboutDialog);
        var sectionLabel = new ToolStripLabel("Dosya ve Yardim")
        {
            ForeColor = AppColors.TextSecondary,
            Font = AppFonts.BodyStrong,
            Margin = new Padding(0, 0, AppSpacing.Md, 0)
        };
        var hintLabel = new ToolStripLabel("Ana kontrol kartlari asagida")
        {
            ForeColor = AppColors.TextMuted,
            Margin = new Padding(AppSpacing.Md, 0, 0, 0)
        };

        toolStrip.Items.AddRange(
            [
                sectionLabel,
                new ToolStripSeparator(),
                _saveToolButton,
                _loadToolButton,
                _clearToolButton,
                new ToolStripSeparator(),
                hotkeysToolButton,
                aboutToolButton,
                new ToolStripSeparator(),
                hintLabel
            ]);

        return toolStrip;
    }

    private StatusStrip CreateStatusStrip()
    {
        _statusStripStateLabel = new ToolStripStatusLabel("Durum: Hazir");
        _statusStripEventCountLabel = new ToolStripStatusLabel("Olay: 0");
        _statusStripSessionLabel = new ToolStripStatusLabel("Oturum: -")
        {
            Spring = true,
            TextAlign = ContentAlignment.MiddleLeft
        };
        _statusStripHotkeysLabel = new ToolStripStatusLabel("Kisayol: F8 / F9 / F10");

        return new StatusStrip
        {
            Dock = DockStyle.Fill,
            SizingGrip = false,
            Items =
            {
                _statusStripStateLabel,
                _statusStripEventCountLabel,
                _statusStripSessionLabel,
                _statusStripHotkeysLabel
            }
        };
    }

    private Control CreateHeaderCard()
    {
        var card = CreateCard();
        card.Margin = new Padding(0, 0, 0, AppSpacing.Lg);

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = Color.Transparent
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 58f));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 42f));

        layout.Controls.Add(CreateLabel("Macro Master", AppFonts.Title, AppColors.TextPrimary), 0, 0);
        layout.Controls.Add(
            CreateLabel(
                "Kayit, oynatim ve otomasyon akislarini tek panelden yonetin.",
                AppFonts.Body,
                AppColors.TextSecondary),
            0,
            1);

        card.Controls.Add(layout);
        return card;
    }

    private Control CreateSummaryAndActionsRow()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = Color.Transparent,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 64f));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 36f));

        var summaryCard = CreateSummaryCard();
        summaryCard.Margin = new Padding(0, 0, AppSpacing.Lg / 2, AppSpacing.Lg);

        var actionsCard = CreateActionsCard();
        actionsCard.Margin = new Padding(AppSpacing.Lg / 2, 0, 0, AppSpacing.Lg);

        layout.Controls.Add(summaryCard, 0, 0);
        layout.Controls.Add(actionsCard, 1, 0);
        return layout;
    }

    private Control CreateContentRow()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = Color.Transparent,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 66f));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34f));

        var eventCard = CreateEventListCard();
        eventCard.Margin = new Padding(0, 0, AppSpacing.Lg / 2, 0);

        var sideBar = CreateSidebarLayout();
        sideBar.Margin = new Padding(AppSpacing.Lg / 2, 0, 0, 0);

        layout.Controls.Add(eventCard, 0, 0);
        layout.Controls.Add(sideBar, 1, 0);
        return layout;
    }

    private Control CreateSidebarLayout()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            BackColor = Color.Transparent,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 34f));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 18f));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 28f));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 20f));

        var detailCard = CreateDetailCard();
        detailCard.Margin = new Padding(0, 0, 0, AppSpacing.Lg / 2);

        var playbackCard = CreatePlaybackCard();
        playbackCard.Margin = new Padding(0, AppSpacing.Lg / 2, 0, AppSpacing.Lg / 2);

        var playbackSettingsCard = CreatePlaybackSettingsCard();
        playbackSettingsCard.Margin = new Padding(0, AppSpacing.Lg / 2, 0, AppSpacing.Lg / 2);

        var hotkeysCard = CreateHotkeysCard();
        hotkeysCard.Margin = new Padding(0, AppSpacing.Lg / 2, 0, 0);

        layout.Controls.Add(detailCard, 0, 0);
        layout.Controls.Add(playbackCard, 0, 1);
        layout.Controls.Add(playbackSettingsCard, 0, 2);
        layout.Controls.Add(hotkeysCard, 0, 3);
        return layout;
    }

    private ModernCard CreateSummaryCard()
    {
        var card = CreateCard();

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            BackColor = Color.Transparent
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28f));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 56f));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

        layout.Controls.Add(CreateSectionLabel("Oturum Ozeti"), 0, 0);

        var metrics = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 1,
            BackColor = Color.Transparent,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        metrics.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
        metrics.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
        metrics.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.34f));
        metrics.Controls.Add(CreateMetricBlock("Durum", out _sessionStatusValueLabel), 0, 0);
        metrics.Controls.Add(CreateMetricBlock("Toplam Olay", out _totalEventsValueLabel), 1, 0);
        metrics.Controls.Add(CreateMetricBlock("Toplam Sure", out _totalDurationValueLabel), 2, 0);

        layout.Controls.Add(metrics, 0, 1);

        var sessionPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.Transparent,
            Margin = new Padding(0, AppSpacing.Sm, 0, 0)
        };
        sessionPanel.Controls.Add(CreateLabel("Secili Oturum", AppFonts.Caption, AppColors.TextSecondary));

        _sessionNameValueLabel = CreateLabel("-", AppFonts.SectionTitle, AppColors.TextPrimary);
        _sessionNameValueLabel.Dock = DockStyle.Bottom;
        _sessionNameValueLabel.Height = 30;
        sessionPanel.Controls.Add(_sessionNameValueLabel);

        layout.Controls.Add(sessionPanel, 0, 2);

        card.Controls.Add(layout);
        return card;
    }

    private ModernCard CreateActionsCard()
    {
        var card = CreateCard();

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            BackColor = Color.Transparent
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28f));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 62f));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
        layout.Controls.Add(CreateSectionLabel("Canli Kontroller"), 0, 0);

        var buttonsLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 1,
            BackColor = Color.Transparent,
            Margin = new Padding(0, AppSpacing.Sm, 0, 0)
        };
        for (var column = 0; column < 3; column++)
        {
            buttonsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
        }
        buttonsLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

        _recordButton = CreateActionButton("Kaydi Baslat", ModernButtonVariant.Primary, async () => await ToggleRecordingAsync());
        _playButton = CreateActionButton("Oynat", ModernButtonVariant.Secondary, async () => await TogglePlaybackAsync());
        _stopButton = CreateActionButton("Durdur", ModernButtonVariant.Danger, async () => await StopAsync());

        buttonsLayout.Controls.Add(_recordButton, 0, 0);
        buttonsLayout.Controls.Add(_playButton, 1, 0);
        buttonsLayout.Controls.Add(_stopButton, 2, 0);

        layout.Controls.Add(buttonsLayout, 0, 1);

        _controlHintLabel = CreateLabel(
            "Kayit ve oynatma burada. Kaydet, yukle ve temizle ust cubukta.",
            AppFonts.Caption,
            AppColors.TextSecondary);
        _controlHintLabel.Margin = new Padding(0, AppSpacing.Sm, 0, 0);
        layout.Controls.Add(_controlHintLabel, 0, 2);

        card.Controls.Add(layout);
        return card;
    }

    private ModernCard CreateEventListCard()
    {
        var card = CreateCard();

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = Color.Transparent
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28f));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
        layout.Controls.Add(CreateSectionLabel("Olay Listesi"), 0, 0);

        _eventGrid = new DataGridView
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(0, AppSpacing.Sm, 0, 0),
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AllowUserToResizeRows = false,
            MultiSelect = false,
            ReadOnly = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect
        };

        layout.Controls.Add(_eventGrid, 0, 1);
        card.Controls.Add(layout);
        return card;
    }

    private ModernCard CreateDetailCard()
    {
        var card = CreateCard();

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            BackColor = Color.Transparent
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28f));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 26f));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 144f));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
        layout.Controls.Add(CreateSectionLabel("Olay Detayi"), 0, 0);

        _detailHeaderValueLabel = CreateLabel("Olay secilmedi", AppFonts.BodyStrong, AppColors.TextPrimary);
        _detailHeaderValueLabel.Margin = new Padding(0, AppSpacing.Sm, 0, 0);
        layout.Controls.Add(_detailHeaderValueLabel, 0, 1);

        var detailGrid = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 6,
            BackColor = Color.Transparent,
            Margin = new Padding(0, AppSpacing.Sm, 0, AppSpacing.Sm)
        };
        detailGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34f));
        detailGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 66f));
        for (var row = 0; row < 6; row++)
        {
            detailGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 24f));
        }

        detailGrid.Controls.Add(CreateSettingsLabel("Tur"), 0, 0);
        detailGrid.Controls.Add(CreateDetailValueLabel(out _detailTypeValueLabel), 1, 0);
        detailGrid.Controls.Add(CreateSettingsLabel("Aksiyon"), 0, 1);
        detailGrid.Controls.Add(CreateDetailValueLabel(out _detailActionValueLabel), 1, 1);
        detailGrid.Controls.Add(CreateSettingsLabel("Zaman"), 0, 2);
        detailGrid.Controls.Add(CreateDetailValueLabel(out _detailTimeValueLabel), 1, 2);
        detailGrid.Controls.Add(CreateSettingsLabel("Gecikme"), 0, 3);
        detailGrid.Controls.Add(CreateDetailValueLabel(out _detailDelayValueLabel), 1, 3);
        detailGrid.Controls.Add(CreateSettingsLabel("Konum"), 0, 4);
        detailGrid.Controls.Add(CreateDetailValueLabel(out _detailPositionValueLabel), 1, 4);
        detailGrid.Controls.Add(CreateSettingsLabel("Tus / Teker"), 0, 5);
        detailGrid.Controls.Add(CreateDetailValueLabel(out _detailInputValueLabel), 1, 5);
        layout.Controls.Add(detailGrid, 0, 2);

        _eventDetailBox = new RichTextBox
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(0),
            ReadOnly = true,
            BorderStyle = BorderStyle.None,
            BackColor = Color.FromArgb(10, 15, 28),
            ForeColor = AppColors.TextPrimary,
            Font = AppFonts.Monospace,
            ScrollBars = RichTextBoxScrollBars.Vertical
        };

        layout.Controls.Add(_eventDetailBox, 0, 1);
        card.Controls.Add(layout);
        return card;
    }

    private ModernCard CreatePlaybackCard()
    {
        var card = CreateCard();

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 6,
            BackColor = Color.Transparent
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28f));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 24f));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 72f));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 24f));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28f));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

        layout.Controls.Add(CreateSectionLabel("Oynatma Durumu"), 0, 0);

        _playbackStatusValueLabel = CreateLabel("-", AppFonts.BodyStrong, AppColors.TextPrimary);
        layout.Controls.Add(_playbackStatusValueLabel, 0, 1);

        var playbackMetrics = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 2,
            BackColor = Color.Transparent,
            Margin = new Padding(0, AppSpacing.Sm, 0, AppSpacing.Sm)
        };
        playbackMetrics.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
        playbackMetrics.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
        playbackMetrics.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));
        playbackMetrics.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));
        playbackMetrics.Controls.Add(CreateCompactMetricBlock("Mod", out _playbackModeValueLabel), 0, 0);
        playbackMetrics.Controls.Add(CreateCompactMetricBlock("Hiz", out _playbackSpeedValueLabel), 1, 0);
        playbackMetrics.Controls.Add(CreateCompactMetricBlock("Tekrar", out _playbackRepeatValueLabel), 0, 1);
        playbackMetrics.Controls.Add(CreateCompactMetricBlock("Gecikme", out _playbackDelayValueLabel), 1, 1);
        layout.Controls.Add(playbackMetrics, 0, 2);

        _currentEventValueLabel = CreateLabel("Hazir", AppFonts.Caption, AppColors.TextSecondary);
        layout.Controls.Add(_currentEventValueLabel, 0, 3);

        _playbackProgressBar = new ProgressBar
        {
            Dock = DockStyle.Fill,
            Height = 18,
            Margin = new Padding(0, AppSpacing.Sm, 0, AppSpacing.Sm),
            Style = ProgressBarStyle.Continuous
        };
        layout.Controls.Add(_playbackProgressBar, 0, 4);

        _playbackProgressValueLabel = CreateLabel("0 / 0", AppFonts.Caption, AppColors.TextSecondary);
        _playbackProgressValueLabel.TextAlign = ContentAlignment.TopRight;
        layout.Controls.Add(_playbackProgressValueLabel, 0, 5);

        card.Controls.Add(layout);
        return card;
    }

    private ModernCard CreateHotkeysCard()
    {
        var card = CreateCard();

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            BackColor = Color.Transparent
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28f));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 24f));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 24f));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 24f));

        layout.Controls.Add(CreateSectionLabel("Global Kisayollar"), 0, 0);
        layout.Controls.Add(CreateLabel("Kayit: F8", AppFonts.Body, AppColors.TextSecondary), 0, 1);
        layout.Controls.Add(CreateLabel("Oynat / Duraklat: F9", AppFonts.Body, AppColors.TextSecondary), 0, 2);
        layout.Controls.Add(CreateLabel("Acil Durdur: F10", AppFonts.Body, AppColors.TextSecondary), 0, 3);

        card.Controls.Add(layout);
        return card;
    }

    private ModernCard CreatePlaybackSettingsCard()
    {
        var card = CreateCard();

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = Color.Transparent,
            AutoScroll = true
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28f));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
        layout.Controls.Add(CreateSectionLabel("Oynatma Ayarlari"), 0, 0);

        var settingsLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 5,
            BackColor = Color.Transparent,
            Margin = new Padding(0, AppSpacing.Sm, 0, 0)
        };
        settingsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 54f));
        settingsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 46f));
        settingsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34f));
        settingsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34f));
        settingsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34f));
        settingsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32f));
        settingsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32f));

        _speedNumeric = CreateNumericInput(1.00m, 0.25m, 4.00m, 0.25m, 2);
        _repeatCountNumeric = CreateNumericInput(1, 1, 999, 1, 0);
        _initialDelayNumeric = CreateNumericInput(0, 0, 60000, 100, 0);
        _loopPlaybackCheckBox = CreateSettingsCheckBox("Sonsuz dongu");
        _relativeCoordinatesCheckBox = CreateSettingsCheckBox("Goreceli koordinat");
        _preserveTimingCheckBox = CreateSettingsCheckBox("Orijinal zaman", isChecked: true);
        _stopOnErrorCheckBox = CreateSettingsCheckBox("Hatada durdur", isChecked: true);

        settingsLayout.Controls.Add(CreateSettingsLabel("Hiz"), 0, 0);
        settingsLayout.Controls.Add(_speedNumeric, 1, 0);
        settingsLayout.Controls.Add(CreateSettingsLabel("Tekrar"), 0, 1);
        settingsLayout.Controls.Add(_repeatCountNumeric, 1, 1);
        settingsLayout.Controls.Add(CreateSettingsLabel("Baslangic gecikmesi"), 0, 2);
        settingsLayout.Controls.Add(_initialDelayNumeric, 1, 2);
        settingsLayout.Controls.Add(_preserveTimingCheckBox, 0, 3);
        settingsLayout.Controls.Add(_stopOnErrorCheckBox, 1, 3);
        settingsLayout.Controls.Add(_loopPlaybackCheckBox, 0, 4);
        settingsLayout.Controls.Add(_relativeCoordinatesCheckBox, 1, 4);

        _speedNumeric.ValueChanged += (_, _) => RefreshUiState();
        _repeatCountNumeric.ValueChanged += (_, _) => RefreshUiState();
        _initialDelayNumeric.ValueChanged += (_, _) => RefreshUiState();
        _loopPlaybackCheckBox.CheckedChanged += (_, _) => RefreshUiState();
        _relativeCoordinatesCheckBox.CheckedChanged += (_, _) => RefreshUiState();
        _preserveTimingCheckBox.CheckedChanged += (_, _) => RefreshUiState();
        _stopOnErrorCheckBox.CheckedChanged += (_, _) => RefreshUiState();

        layout.Controls.Add(settingsLayout, 0, 1);
        card.Controls.Add(layout);
        return card;
    }

    private ModernCard CreateCard()
    {
        return new ModernCard
        {
            Dock = DockStyle.Fill
        };
    }

    private Control CreateMetricBlock(string title, out Label valueLabel)
    {
        var panel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.Transparent,
            Padding = new Padding(0, 0, AppSpacing.Sm, 0)
        };

        var titleLabel = CreateLabel(title, AppFonts.Caption, AppColors.TextSecondary);
        titleLabel.Dock = DockStyle.Top;
        panel.Controls.Add(titleLabel);

        valueLabel = CreateLabel("-", AppFonts.SectionTitle, AppColors.TextPrimary);
        valueLabel.Dock = DockStyle.Bottom;
        valueLabel.Height = 28;
        panel.Controls.Add(valueLabel);

        return panel;
    }

    private Control CreateCompactMetricBlock(string title, out Label valueLabel)
    {
        var panel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(10, 15, 28),
            Padding = new Padding(AppSpacing.Sm),
            Margin = new Padding(AppSpacing.Xs)
        };

        var titleLabel = CreateLabel(title, AppFonts.Caption, AppColors.TextMuted);
        titleLabel.Dock = DockStyle.Top;
        panel.Controls.Add(titleLabel);

        valueLabel = CreateLabel("-", AppFonts.BodyStrong, AppColors.TextPrimary);
        valueLabel.Dock = DockStyle.Bottom;
        valueLabel.Height = 20;
        panel.Controls.Add(valueLabel);

        return panel;
    }

    private ModernButton CreateActionButton(string text, ModernButtonVariant variant, Action onClick)
    {
        var button = new ModernButton
        {
            Text = text,
            Variant = variant,
            Dock = DockStyle.Fill,
            Margin = new Padding(AppSpacing.Xs)
        };

        button.Click += (_, _) => onClick();
        return button;
    }

    private ModernButton CreateActionButton(string text, ModernButtonVariant variant, Func<Task> onClickAsync)
    {
        var button = new ModernButton
        {
            Text = text,
            Variant = variant,
            Dock = DockStyle.Fill,
            Margin = new Padding(AppSpacing.Xs)
        };

        button.Click += async (_, _) => await onClickAsync();
        return button;
    }

    private ToolStripMenuItem CreateMenuActionItem(string text, Action onClick)
    {
        var item = new ToolStripMenuItem(text);
        item.Click += (_, _) => onClick();
        return item;
    }

    private ToolStripMenuItem CreateMenuActionItem(string text, Func<Task> onClickAsync)
    {
        var item = new ToolStripMenuItem(text);
        item.Click += async (_, _) => await onClickAsync();
        return item;
    }

    private ToolStripButton CreateToolButton(string text, Action onClick)
    {
        var button = new ToolStripButton(text)
        {
            AutoSize = false,
            Width = 82,
            DisplayStyle = ToolStripItemDisplayStyle.Text,
            Margin = new Padding(AppSpacing.Xs, 0, AppSpacing.Xs, 0)
        };

        button.Click += (_, _) => onClick();
        return button;
    }

    private ToolStripButton CreateToolButton(string text, Func<Task> onClickAsync)
    {
        var button = new ToolStripButton(text)
        {
            AutoSize = false,
            Width = 82,
            DisplayStyle = ToolStripItemDisplayStyle.Text,
            Margin = new Padding(AppSpacing.Xs, 0, AppSpacing.Xs, 0)
        };

        button.Click += async (_, _) => await onClickAsync();
        return button;
    }

    private NumericUpDown CreateNumericInput(decimal value, decimal minimum, decimal maximum, decimal increment, int decimalPlaces)
    {
        return new NumericUpDown
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(AppSpacing.Sm, 2, 0, 2),
            Value = value,
            Minimum = minimum,
            Maximum = maximum,
            Increment = increment,
            DecimalPlaces = decimalPlaces,
            BorderStyle = BorderStyle.FixedSingle,
            BackColor = Color.FromArgb(10, 15, 28),
            ForeColor = AppColors.TextPrimary,
            Font = AppFonts.Body,
            TextAlign = HorizontalAlignment.Right
        };
    }

    private CheckBox CreateSettingsCheckBox(string text, bool isChecked = false)
    {
        return new CheckBox
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 2, AppSpacing.Sm, 2),
            Text = text,
            Checked = isChecked,
            ForeColor = AppColors.TextSecondary,
            Font = AppFonts.Body,
            BackColor = Color.Transparent,
            TextAlign = ContentAlignment.MiddleLeft,
            AutoEllipsis = true
        };
    }

    private Label CreateSettingsLabel(string text)
    {
        return CreateLabel(text, AppFonts.Body, AppColors.TextSecondary);
    }

    private Control CreateDetailValueLabel(out Label valueLabel)
    {
        valueLabel = CreateLabel("-", AppFonts.BodyStrong, AppColors.TextPrimary);
        return valueLabel;
    }

    private Label CreateSectionLabel(string text)
    {
        return CreateLabel(text, AppFonts.SectionTitle, AppColors.Primary);
    }

    private Label CreateLabel(string text, Font font, Color color)
    {
        return new Label
        {
            AutoSize = false,
            Dock = DockStyle.Fill,
            Text = text,
            Font = font,
            ForeColor = color,
            BackColor = Color.Transparent,
            TextAlign = ContentAlignment.MiddleLeft
        };
    }
}
