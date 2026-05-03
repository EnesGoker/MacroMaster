namespace MacroMaster.WinForms.Forms;

partial class MainForm
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    ///  Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        titleLabel = new Label();
        hotkeySummaryLabel = new Label();
        statusTableLayoutPanel = new TableLayoutPanel();
        stateTitleLabel = new Label();
        stateValueLabel = new Label();
        sessionNameTitleLabel = new Label();
        sessionNameValueLabel = new Label();
        eventCountTitleLabel = new Label();
        eventCountValueLabel = new Label();
        durationTitleLabel = new Label();
        durationValueLabel = new Label();
        sessionFileTitleLabel = new Label();
        sessionFileValueLabel = new Label();
        actionsFlowLayoutPanel = new FlowLayoutPanel();
        recordToggleButton = new Button();
        playbackToggleButton = new Button();
        stopButton = new Button();
        saveJsonButton = new Button();
        loadJsonButton = new Button();
        saveXmlButton = new Button();
        loadXmlButton = new Button();
        clearSessionButton = new Button();
        relativeCoordinatesCheckBox = new CheckBox();
        playbackSettingsPanel = new FlowLayoutPanel();
        playbackSettingsTitleLabel = new Label();
        stopOnErrorCheckBox = new CheckBox();
        preserveTimingCheckBox = new CheckBox();
        speedMultiplierLabel = new Label();
        speedMultiplierNumericUpDown = new NumericUpDown();
        repeatCountLabel = new Label();
        repeatCountNumericUpDown = new NumericUpDown();
        loopIndefinitelyCheckBox = new CheckBox();
        initialDelayLabel = new Label();
        initialDelayNumericUpDown = new NumericUpDown();
        sessionPreviewTextBox = new TextBox();
        statusTableLayoutPanel.SuspendLayout();
        actionsFlowLayoutPanel.SuspendLayout();
        playbackSettingsPanel.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)speedMultiplierNumericUpDown).BeginInit();
        ((System.ComponentModel.ISupportInitialize)repeatCountNumericUpDown).BeginInit();
        ((System.ComponentModel.ISupportInitialize)initialDelayNumericUpDown).BeginInit();
        SuspendLayout();
        // 
        // titleLabel
        // 
        titleLabel.AutoSize = true;
        titleLabel.Font = new Font("Segoe UI", 15.75F, FontStyle.Bold, GraphicsUnit.Point, 162);
        titleLabel.Location = new Point(20, 16);
        titleLabel.Name = "titleLabel";
        titleLabel.Size = new Size(278, 30);
        titleLabel.TabIndex = 0;
        titleLabel.Text = "MacroMaster Kontrol Merkezi";
        // 
        // hotkeySummaryLabel
        // 
        hotkeySummaryLabel.AutoSize = true;
        hotkeySummaryLabel.Location = new Point(23, 55);
        hotkeySummaryLabel.Name = "hotkeySummaryLabel";
        hotkeySummaryLabel.Size = new Size(115, 15);
        hotkeySummaryLabel.TabIndex = 1;
        hotkeySummaryLabel.Text = "Kisayollar yukleniyor...";
        // 
        // statusTableLayoutPanel
        // 
        statusTableLayoutPanel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        statusTableLayoutPanel.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
        statusTableLayoutPanel.ColumnCount = 2;
        statusTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170F));
        statusTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        statusTableLayoutPanel.Controls.Add(stateTitleLabel, 0, 0);
        statusTableLayoutPanel.Controls.Add(stateValueLabel, 1, 0);
        statusTableLayoutPanel.Controls.Add(sessionNameTitleLabel, 0, 1);
        statusTableLayoutPanel.Controls.Add(sessionNameValueLabel, 1, 1);
        statusTableLayoutPanel.Controls.Add(eventCountTitleLabel, 0, 2);
        statusTableLayoutPanel.Controls.Add(eventCountValueLabel, 1, 2);
        statusTableLayoutPanel.Controls.Add(durationTitleLabel, 0, 3);
        statusTableLayoutPanel.Controls.Add(durationValueLabel, 1, 3);
        statusTableLayoutPanel.Controls.Add(sessionFileTitleLabel, 0, 4);
        statusTableLayoutPanel.Controls.Add(sessionFileValueLabel, 1, 4);
        statusTableLayoutPanel.Location = new Point(23, 86);
        statusTableLayoutPanel.Name = "statusTableLayoutPanel";
        statusTableLayoutPanel.RowCount = 5;
        statusTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
        statusTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
        statusTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
        statusTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
        statusTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
        statusTableLayoutPanel.Size = new Size(938, 176);
        statusTableLayoutPanel.TabIndex = 2;
        // 
        // stateTitleLabel
        // 
        stateTitleLabel.Anchor = AnchorStyles.Left;
        stateTitleLabel.AutoSize = true;
        stateTitleLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 162);
        stateTitleLabel.Location = new Point(4, 10);
        stateTitleLabel.Name = "stateTitleLabel";
        stateTitleLabel.Size = new Size(95, 15);
        stateTitleLabel.TabIndex = 0;
        stateTitleLabel.Text = "Uygulama durumu";
        // 
        // stateValueLabel
        // 
        stateValueLabel.Anchor = AnchorStyles.Left;
        stateValueLabel.AutoSize = true;
        stateValueLabel.Location = new Point(175, 10);
        stateValueLabel.Name = "stateValueLabel";
        stateValueLabel.Size = new Size(23, 15);
        stateValueLabel.TabIndex = 1;
        stateValueLabel.Text = "Bos";
        // 
        // sessionNameTitleLabel
        // 
        sessionNameTitleLabel.Anchor = AnchorStyles.Left;
        sessionNameTitleLabel.AutoSize = true;
        sessionNameTitleLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 162);
        sessionNameTitleLabel.Location = new Point(4, 45);
        sessionNameTitleLabel.Name = "sessionNameTitleLabel";
        sessionNameTitleLabel.Size = new Size(83, 15);
        sessionNameTitleLabel.TabIndex = 2;
        sessionNameTitleLabel.Text = "Oturum adi";
        // 
        // sessionNameValueLabel
        // 
        sessionNameValueLabel.Anchor = AnchorStyles.Left;
        sessionNameValueLabel.AutoEllipsis = true;
        sessionNameValueLabel.AutoSize = true;
        sessionNameValueLabel.Location = new Point(175, 45);
        sessionNameValueLabel.Name = "sessionNameValueLabel";
        sessionNameValueLabel.Size = new Size(66, 15);
        sessionNameValueLabel.TabIndex = 3;
        sessionNameValueLabel.Text = "Oturum yok";
        // 
        // eventCountTitleLabel
        // 
        eventCountTitleLabel.Anchor = AnchorStyles.Left;
        eventCountTitleLabel.AutoSize = true;
        eventCountTitleLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 162);
        eventCountTitleLabel.Location = new Point(4, 80);
        eventCountTitleLabel.Name = "eventCountTitleLabel";
        eventCountTitleLabel.Size = new Size(71, 15);
        eventCountTitleLabel.TabIndex = 4;
        eventCountTitleLabel.Text = "Olay sayisi";
        // 
        // eventCountValueLabel
        // 
        eventCountValueLabel.Anchor = AnchorStyles.Left;
        eventCountValueLabel.AutoSize = true;
        eventCountValueLabel.Location = new Point(175, 80);
        eventCountValueLabel.Name = "eventCountValueLabel";
        eventCountValueLabel.Size = new Size(13, 15);
        eventCountValueLabel.TabIndex = 5;
        eventCountValueLabel.Text = "0";
        // 
        // durationTitleLabel
        // 
        durationTitleLabel.Anchor = AnchorStyles.Left;
        durationTitleLabel.AutoSize = true;
        durationTitleLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 162);
        durationTitleLabel.Location = new Point(4, 115);
        durationTitleLabel.Name = "durationTitleLabel";
        durationTitleLabel.Size = new Size(54, 15);
        durationTitleLabel.TabIndex = 6;
        durationTitleLabel.Text = "Sure";
        // 
        // durationValueLabel
        // 
        durationValueLabel.Anchor = AnchorStyles.Left;
        durationValueLabel.AutoSize = true;
        durationValueLabel.Location = new Point(175, 115);
        durationValueLabel.Name = "durationValueLabel";
        durationValueLabel.Size = new Size(31, 15);
        durationValueLabel.TabIndex = 7;
        durationValueLabel.Text = "0 ms";
        // 
        // sessionFileTitleLabel
        // 
        sessionFileTitleLabel.Anchor = AnchorStyles.Left;
        sessionFileTitleLabel.AutoSize = true;
        sessionFileTitleLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 162);
        sessionFileTitleLabel.Location = new Point(4, 151);
        sessionFileTitleLabel.Name = "sessionFileTitleLabel";
        sessionFileTitleLabel.Size = new Size(67, 15);
        sessionFileTitleLabel.TabIndex = 8;
        sessionFileTitleLabel.Text = "Oturum dosyasi";
        // 
        // sessionFileValueLabel
        // 
        sessionFileValueLabel.Anchor = AnchorStyles.Left;
        sessionFileValueLabel.AutoEllipsis = true;
        sessionFileValueLabel.AutoSize = true;
        sessionFileValueLabel.Location = new Point(175, 151);
        sessionFileValueLabel.Name = "sessionFileValueLabel";
        sessionFileValueLabel.Size = new Size(48, 15);
        sessionFileValueLabel.TabIndex = 9;
        sessionFileValueLabel.Text = "Kaydedilmedi";
        // 
        // actionsFlowLayoutPanel
        // 
        actionsFlowLayoutPanel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        actionsFlowLayoutPanel.Controls.Add(recordToggleButton);
        actionsFlowLayoutPanel.Controls.Add(playbackToggleButton);
        actionsFlowLayoutPanel.Controls.Add(stopButton);
        actionsFlowLayoutPanel.Controls.Add(saveJsonButton);
        actionsFlowLayoutPanel.Controls.Add(loadJsonButton);
        actionsFlowLayoutPanel.Controls.Add(saveXmlButton);
        actionsFlowLayoutPanel.Controls.Add(loadXmlButton);
        actionsFlowLayoutPanel.Controls.Add(clearSessionButton);
        actionsFlowLayoutPanel.Controls.Add(relativeCoordinatesCheckBox);
        actionsFlowLayoutPanel.Location = new Point(23, 277);
        actionsFlowLayoutPanel.Name = "actionsFlowLayoutPanel";
        actionsFlowLayoutPanel.Size = new Size(938, 38);
        actionsFlowLayoutPanel.TabIndex = 3;
        // 
        // recordToggleButton
        // 
        recordToggleButton.AutoSize = true;
        recordToggleButton.Location = new Point(3, 3);
        recordToggleButton.Name = "recordToggleButton";
        recordToggleButton.Size = new Size(109, 27);
        recordToggleButton.TabIndex = 0;
        recordToggleButton.Text = "Kayit Baslat";
        recordToggleButton.UseVisualStyleBackColor = true;
        recordToggleButton.Click += recordToggleButton_Click;
        // 
        // playbackToggleButton
        // 
        playbackToggleButton.AutoSize = true;
        playbackToggleButton.Location = new Point(118, 3);
        playbackToggleButton.Name = "playbackToggleButton";
        playbackToggleButton.Size = new Size(49, 27);
        playbackToggleButton.TabIndex = 1;
        playbackToggleButton.Text = "Oynat";
        playbackToggleButton.UseVisualStyleBackColor = true;
        playbackToggleButton.Click += playbackToggleButton_Click;
        // 
        // stopButton
        // 
        stopButton.AutoSize = true;
        stopButton.Location = new Point(173, 3);
        stopButton.Name = "stopButton";
        stopButton.Size = new Size(51, 27);
        stopButton.TabIndex = 2;
        stopButton.Text = "Durdur";
        stopButton.UseVisualStyleBackColor = true;
        stopButton.Click += stopButton_Click;
        // 
        // saveJsonButton
        // 
        saveJsonButton.AutoSize = true;
        saveJsonButton.Location = new Point(230, 3);
        saveJsonButton.Name = "saveJsonButton";
        saveJsonButton.Size = new Size(79, 27);
        saveJsonButton.TabIndex = 3;
        saveJsonButton.Text = "JSON Kaydet";
        saveJsonButton.UseVisualStyleBackColor = true;
        saveJsonButton.Click += saveJsonButton_Click;
        // 
        // loadJsonButton
        // 
        loadJsonButton.AutoSize = true;
        loadJsonButton.Location = new Point(315, 3);
        loadJsonButton.Name = "loadJsonButton";
        loadJsonButton.Size = new Size(80, 27);
        loadJsonButton.TabIndex = 4;
        loadJsonButton.Text = "JSON Yukle";
        loadJsonButton.UseVisualStyleBackColor = true;
        loadJsonButton.Click += loadJsonButton_Click;
        // 
        // saveXmlButton
        // 
        saveXmlButton.AutoSize = true;
        saveXmlButton.Location = new Point(401, 3);
        saveXmlButton.Name = "saveXmlButton";
        saveXmlButton.Size = new Size(72, 27);
        saveXmlButton.TabIndex = 5;
        saveXmlButton.Text = "XML Kaydet";
        saveXmlButton.UseVisualStyleBackColor = true;
        saveXmlButton.Click += saveXmlButton_Click;
        // 
        // loadXmlButton
        // 
        loadXmlButton.AutoSize = true;
        loadXmlButton.Location = new Point(479, 3);
        loadXmlButton.Name = "loadXmlButton";
        loadXmlButton.Size = new Size(73, 27);
        loadXmlButton.TabIndex = 6;
        loadXmlButton.Text = "XML Yukle";
        loadXmlButton.UseVisualStyleBackColor = true;
        loadXmlButton.Click += loadXmlButton_Click;
        // 
        // clearSessionButton
        // 
        clearSessionButton.AutoSize = true;
        clearSessionButton.Location = new Point(558, 3);
        clearSessionButton.Name = "clearSessionButton";
        clearSessionButton.Size = new Size(90, 27);
        clearSessionButton.TabIndex = 7;
        clearSessionButton.Text = "Oturumu Temizle";
        clearSessionButton.UseVisualStyleBackColor = true;
        clearSessionButton.Click += clearSessionButton_Click;
        // 
        // relativeCoordinatesCheckBox
        // 
        relativeCoordinatesCheckBox.AutoSize = true;
        relativeCoordinatesCheckBox.Location = new Point(654, 6);
        relativeCoordinatesCheckBox.Margin = new Padding(3, 6, 3, 3);
        relativeCoordinatesCheckBox.Name = "relativeCoordinatesCheckBox";
        relativeCoordinatesCheckBox.Size = new Size(176, 19);
        relativeCoordinatesCheckBox.TabIndex = 8;
        relativeCoordinatesCheckBox.Text = "Goreli Fare Oynatimi";
        relativeCoordinatesCheckBox.UseVisualStyleBackColor = true;
        relativeCoordinatesCheckBox.CheckedChanged += playbackSettingControl_ValueChanged;
        // 
        // playbackSettingsPanel
        // 
        playbackSettingsPanel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        playbackSettingsPanel.BorderStyle = BorderStyle.FixedSingle;
        playbackSettingsPanel.Controls.Add(playbackSettingsTitleLabel);
        playbackSettingsPanel.Controls.Add(stopOnErrorCheckBox);
        playbackSettingsPanel.Controls.Add(preserveTimingCheckBox);
        playbackSettingsPanel.Controls.Add(speedMultiplierLabel);
        playbackSettingsPanel.Controls.Add(speedMultiplierNumericUpDown);
        playbackSettingsPanel.Controls.Add(repeatCountLabel);
        playbackSettingsPanel.Controls.Add(repeatCountNumericUpDown);
        playbackSettingsPanel.Controls.Add(loopIndefinitelyCheckBox);
        playbackSettingsPanel.Controls.Add(initialDelayLabel);
        playbackSettingsPanel.Controls.Add(initialDelayNumericUpDown);
        playbackSettingsPanel.Location = new Point(23, 326);
        playbackSettingsPanel.Name = "playbackSettingsPanel";
        playbackSettingsPanel.Padding = new Padding(8, 7, 8, 7);
        playbackSettingsPanel.Size = new Size(938, 62);
        playbackSettingsPanel.TabIndex = 4;
        // 
        // playbackSettingsTitleLabel
        // 
        playbackSettingsTitleLabel.Anchor = AnchorStyles.Left;
        playbackSettingsTitleLabel.AutoSize = true;
        playbackSettingsTitleLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 162);
        playbackSettingsTitleLabel.Location = new Point(11, 19);
        playbackSettingsTitleLabel.Margin = new Padding(3, 5, 10, 0);
        playbackSettingsTitleLabel.Name = "playbackSettingsTitleLabel";
        playbackSettingsTitleLabel.Size = new Size(97, 15);
        playbackSettingsTitleLabel.TabIndex = 0;
        playbackSettingsTitleLabel.Text = "Oynatma Ayarlari";
        // 
        // stopOnErrorCheckBox
        // 
        stopOnErrorCheckBox.AutoSize = true;
        stopOnErrorCheckBox.Checked = true;
        stopOnErrorCheckBox.CheckState = CheckState.Checked;
        stopOnErrorCheckBox.Location = new Point(121, 15);
        stopOnErrorCheckBox.Name = "stopOnErrorCheckBox";
        stopOnErrorCheckBox.Size = new Size(96, 19);
        stopOnErrorCheckBox.TabIndex = 1;
        stopOnErrorCheckBox.Text = "Hatada Durdur";
        stopOnErrorCheckBox.UseVisualStyleBackColor = true;
        stopOnErrorCheckBox.CheckedChanged += playbackSettingControl_ValueChanged;
        // 
        // preserveTimingCheckBox
        // 
        preserveTimingCheckBox.AutoSize = true;
        preserveTimingCheckBox.Checked = true;
        preserveTimingCheckBox.CheckState = CheckState.Checked;
        preserveTimingCheckBox.Location = new Point(223, 15);
        preserveTimingCheckBox.Name = "preserveTimingCheckBox";
        preserveTimingCheckBox.Size = new Size(111, 19);
        preserveTimingCheckBox.TabIndex = 2;
        preserveTimingCheckBox.Text = "Zamanlamayi Koru";
        preserveTimingCheckBox.UseVisualStyleBackColor = true;
        preserveTimingCheckBox.CheckedChanged += preserveTimingCheckBox_CheckedChanged;
        // 
        // speedMultiplierLabel
        // 
        speedMultiplierLabel.Anchor = AnchorStyles.Left;
        speedMultiplierLabel.AutoSize = true;
        speedMultiplierLabel.Location = new Point(340, 17);
        speedMultiplierLabel.Margin = new Padding(3, 7, 3, 0);
        speedMultiplierLabel.Name = "speedMultiplierLabel";
        speedMultiplierLabel.Size = new Size(38, 15);
        speedMultiplierLabel.TabIndex = 3;
        speedMultiplierLabel.Text = "Hiz";
        // 
        // speedMultiplierNumericUpDown
        // 
        speedMultiplierNumericUpDown.DecimalPlaces = 2;
        speedMultiplierNumericUpDown.Increment = new decimal(new int[] { 25, 0, 0, 131072 });
        speedMultiplierNumericUpDown.Location = new Point(384, 12);
        speedMultiplierNumericUpDown.Maximum = new decimal(new int[] { 10, 0, 0, 0 });
        speedMultiplierNumericUpDown.Minimum = new decimal(new int[] { 25, 0, 0, 131072 });
        speedMultiplierNumericUpDown.Name = "speedMultiplierNumericUpDown";
        speedMultiplierNumericUpDown.Size = new Size(62, 23);
        speedMultiplierNumericUpDown.TabIndex = 4;
        speedMultiplierNumericUpDown.Value = new decimal(new int[] { 1, 0, 0, 0 });
        speedMultiplierNumericUpDown.ValueChanged += playbackSettingControl_ValueChanged;
        // 
        // repeatCountLabel
        // 
        repeatCountLabel.Anchor = AnchorStyles.Left;
        repeatCountLabel.AutoSize = true;
        repeatCountLabel.Location = new Point(452, 17);
        repeatCountLabel.Margin = new Padding(3, 7, 3, 0);
        repeatCountLabel.Name = "repeatCountLabel";
        repeatCountLabel.Size = new Size(42, 15);
        repeatCountLabel.TabIndex = 5;
        repeatCountLabel.Text = "Tekrar";
        // 
        // repeatCountNumericUpDown
        // 
        repeatCountNumericUpDown.Location = new Point(500, 12);
        repeatCountNumericUpDown.Maximum = new decimal(new int[] { 100, 0, 0, 0 });
        repeatCountNumericUpDown.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
        repeatCountNumericUpDown.Name = "repeatCountNumericUpDown";
        repeatCountNumericUpDown.Size = new Size(56, 23);
        repeatCountNumericUpDown.TabIndex = 6;
        repeatCountNumericUpDown.Value = new decimal(new int[] { 1, 0, 0, 0 });
        repeatCountNumericUpDown.ValueChanged += playbackSettingControl_ValueChanged;
        // 
        // loopIndefinitelyCheckBox
        // 
        loopIndefinitelyCheckBox.AutoSize = true;
        loopIndefinitelyCheckBox.Location = new Point(562, 15);
        loopIndefinitelyCheckBox.Name = "loopIndefinitelyCheckBox";
        loopIndefinitelyCheckBox.Size = new Size(113, 19);
        loopIndefinitelyCheckBox.TabIndex = 7;
        loopIndefinitelyCheckBox.Text = "Sonsuz Dongu";
        loopIndefinitelyCheckBox.UseVisualStyleBackColor = true;
        loopIndefinitelyCheckBox.CheckedChanged += loopIndefinitelyCheckBox_CheckedChanged;
        // 
        // initialDelayLabel
        // 
        initialDelayLabel.Anchor = AnchorStyles.Left;
        initialDelayLabel.AutoSize = true;
        initialDelayLabel.Location = new Point(681, 17);
        initialDelayLabel.Margin = new Padding(3, 7, 3, 0);
        initialDelayLabel.Name = "initialDelayLabel";
        initialDelayLabel.Size = new Size(69, 15);
        initialDelayLabel.TabIndex = 8;
        initialDelayLabel.Text = "Baslangic Gecikmesi";
        // 
        // initialDelayNumericUpDown
        // 
        initialDelayNumericUpDown.Increment = new decimal(new int[] { 100, 0, 0, 0 });
        initialDelayNumericUpDown.Location = new Point(756, 12);
        initialDelayNumericUpDown.Maximum = new decimal(new int[] { 60000, 0, 0, 0 });
        initialDelayNumericUpDown.Name = "initialDelayNumericUpDown";
        initialDelayNumericUpDown.Size = new Size(76, 23);
        initialDelayNumericUpDown.TabIndex = 9;
        initialDelayNumericUpDown.ValueChanged += playbackSettingControl_ValueChanged;
        // 
        // sessionPreviewTextBox
        // 
        sessionPreviewTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        sessionPreviewTextBox.Font = new Font("Consolas", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
        sessionPreviewTextBox.Location = new Point(23, 401);
        sessionPreviewTextBox.Multiline = true;
        sessionPreviewTextBox.Name = "sessionPreviewTextBox";
        sessionPreviewTextBox.ReadOnly = true;
        sessionPreviewTextBox.ScrollBars = ScrollBars.Vertical;
        sessionPreviewTextBox.Size = new Size(938, 102);
        sessionPreviewTextBox.TabIndex = 5;
        // 
        // MainForm
        // 
        AutoScaleMode = AutoScaleMode.None;
        ClientSize = new Size(984, 521);
        Controls.Add(sessionPreviewTextBox);
        Controls.Add(playbackSettingsPanel);
        Controls.Add(actionsFlowLayoutPanel);
        Controls.Add(statusTableLayoutPanel);
        Controls.Add(hotkeySummaryLabel);
        Controls.Add(titleLabel);
        MinimumSize = new Size(1000, 560);
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "MacroMaster Kontrol Merkezi";
        statusTableLayoutPanel.ResumeLayout(false);
        statusTableLayoutPanel.PerformLayout();
        actionsFlowLayoutPanel.ResumeLayout(false);
        actionsFlowLayoutPanel.PerformLayout();
        playbackSettingsPanel.ResumeLayout(false);
        playbackSettingsPanel.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)speedMultiplierNumericUpDown).EndInit();
        ((System.ComponentModel.ISupportInitialize)repeatCountNumericUpDown).EndInit();
        ((System.ComponentModel.ISupportInitialize)initialDelayNumericUpDown).EndInit();
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion
    private Label titleLabel;
    private Label hotkeySummaryLabel;
    private TableLayoutPanel statusTableLayoutPanel;
    private Label stateTitleLabel;
    private Label stateValueLabel;
    private Label sessionNameTitleLabel;
    private Label sessionNameValueLabel;
    private Label eventCountTitleLabel;
    private Label eventCountValueLabel;
    private Label durationTitleLabel;
    private Label durationValueLabel;
    private Label sessionFileTitleLabel;
    private Label sessionFileValueLabel;
    private FlowLayoutPanel actionsFlowLayoutPanel;
    private Button recordToggleButton;
    private Button playbackToggleButton;
    private Button stopButton;
    private Button saveJsonButton;
    private Button loadJsonButton;
    private Button saveXmlButton;
    private Button loadXmlButton;
    private Button clearSessionButton;
    private CheckBox relativeCoordinatesCheckBox;
    private FlowLayoutPanel playbackSettingsPanel;
    private Label playbackSettingsTitleLabel;
    private CheckBox stopOnErrorCheckBox;
    private CheckBox preserveTimingCheckBox;
    private Label speedMultiplierLabel;
    private NumericUpDown speedMultiplierNumericUpDown;
    private Label repeatCountLabel;
    private NumericUpDown repeatCountNumericUpDown;
    private CheckBox loopIndefinitelyCheckBox;
    private Label initialDelayLabel;
    private NumericUpDown initialDelayNumericUpDown;
    private TextBox sessionPreviewTextBox;
}