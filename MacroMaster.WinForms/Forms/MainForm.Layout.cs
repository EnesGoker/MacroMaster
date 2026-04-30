using MacroMaster.WinForms.Controls;
using MacroMaster.WinForms.Theme;

namespace MacroMaster.WinForms.Forms;

public partial class MainForm
{
    private TableLayoutPanel _rootLayout = null!;
    private Label _sessionStatusValueLabel = null!;
    private Label _totalEventsValueLabel = null!;
    private Label _totalDurationValueLabel = null!;
    private Label _sessionNameValueLabel = null!;
    private Label _playbackStatusValueLabel = null!;
    private Label _playbackProgressValueLabel = null!;
    private Label _currentEventValueLabel = null!;
    private ModernButton _recordButton = null!;
    private ModernButton _playButton = null!;
    private ModernButton _stopButton = null!;
    private ModernButton _saveButton = null!;
    private ModernButton _loadButton = null!;
    private ModernButton _clearButton = null!;
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

        Controls.Add(_rootLayout);

        ResumeLayout(performLayout: true);
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
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60f));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40f));

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
            RowCount = 3,
            BackColor = Color.Transparent,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 44f));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 28f));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 28f));

        var detailCard = CreateDetailCard();
        detailCard.Margin = new Padding(0, 0, 0, AppSpacing.Lg / 2);

        var playbackCard = CreatePlaybackCard();
        playbackCard.Margin = new Padding(0, AppSpacing.Lg / 2, 0, AppSpacing.Lg / 2);

        var hotkeysCard = CreateHotkeysCard();
        hotkeysCard.Margin = new Padding(0, AppSpacing.Lg / 2, 0, 0);

        layout.Controls.Add(detailCard, 0, 0);
        layout.Controls.Add(playbackCard, 0, 1);
        layout.Controls.Add(hotkeysCard, 0, 2);
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
            RowCount = 2,
            BackColor = Color.Transparent
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28f));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
        layout.Controls.Add(CreateSectionLabel("Hizli Eylemler"), 0, 0);

        var buttonsLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 2,
            BackColor = Color.Transparent,
            Margin = new Padding(0, AppSpacing.Sm, 0, 0)
        };
        for (var column = 0; column < 3; column++)
        {
            buttonsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
        }
        buttonsLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));
        buttonsLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));

        _recordButton = CreateActionButton("Kaydi Baslat", ModernButtonVariant.Primary, async () => await ToggleRecordingAsync());
        _playButton = CreateActionButton("Oynat", ModernButtonVariant.Secondary, async () => await TogglePlaybackAsync());
        _stopButton = CreateActionButton("Durdur", ModernButtonVariant.Danger, async () => await StopAsync());
        _saveButton = CreateActionButton("Kaydet", ModernButtonVariant.Secondary, async () => await SaveSessionAsync());
        _loadButton = CreateActionButton("Yukle", ModernButtonVariant.Secondary, async () => await LoadSessionAsync());
        _clearButton = CreateActionButton("Temizle", ModernButtonVariant.Ghost, ClearCurrentSession);

        buttonsLayout.Controls.Add(_recordButton, 0, 0);
        buttonsLayout.Controls.Add(_playButton, 1, 0);
        buttonsLayout.Controls.Add(_stopButton, 2, 0);
        buttonsLayout.Controls.Add(_saveButton, 0, 1);
        buttonsLayout.Controls.Add(_loadButton, 1, 1);
        buttonsLayout.Controls.Add(_clearButton, 2, 1);

        layout.Controls.Add(buttonsLayout, 0, 1);
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
            RowCount = 2,
            BackColor = Color.Transparent
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28f));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
        layout.Controls.Add(CreateSectionLabel("Olay Detayi"), 0, 0);

        _eventDetailBox = new RichTextBox
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(0, AppSpacing.Sm, 0, 0),
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
            RowCount = 5,
            BackColor = Color.Transparent
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28f));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 24f));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 24f));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28f));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

        layout.Controls.Add(CreateSectionLabel("Oynatma Durumu"), 0, 0);

        _playbackStatusValueLabel = CreateLabel("-", AppFonts.BodyStrong, AppColors.TextPrimary);
        layout.Controls.Add(_playbackStatusValueLabel, 0, 1);

        _currentEventValueLabel = CreateLabel("Hazir", AppFonts.Caption, AppColors.TextSecondary);
        layout.Controls.Add(_currentEventValueLabel, 0, 2);

        _playbackProgressBar = new ProgressBar
        {
            Dock = DockStyle.Fill,
            Height = 18,
            Margin = new Padding(0, AppSpacing.Sm, 0, AppSpacing.Sm),
            Style = ProgressBarStyle.Continuous
        };
        layout.Controls.Add(_playbackProgressBar, 0, 3);

        _playbackProgressValueLabel = CreateLabel("0 / 0", AppFonts.Caption, AppColors.TextSecondary);
        _playbackProgressValueLabel.TextAlign = ContentAlignment.TopRight;
        layout.Controls.Add(_playbackProgressValueLabel, 0, 4);

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
