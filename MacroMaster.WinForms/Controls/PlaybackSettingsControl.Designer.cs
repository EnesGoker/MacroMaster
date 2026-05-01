namespace MacroMaster.WinForms.Controls;

partial class PlaybackSettingsControl
{
    private System.ComponentModel.IContainer components = null;
    private TableLayoutPanel rootLayoutPanel = null!;
    private Label titleLabel = null!;
    private TableLayoutPanel settingsLayoutPanel = null!;
    private Label speedLabel = null!;
    private ComboBox speedComboBox = null!;
    private Label repeatCountLabel = null!;
    private NumericUpDown repeatCountNumericUpDown = null!;
    private Label initialDelayLabel = null!;
    private NumericUpDown initialDelayNumericUpDown = null!;
    private Panel dividerPanel = null!;
    private CheckBox preserveTimingCheckBox = null!;
    private CheckBox loopIndefinitelyCheckBox = null!;
    private CheckBox stopOnErrorCheckBox = null!;
    private CheckBox relativeCoordinatesCheckBox = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components is not null)
        {
            components.Dispose();
        }

        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        rootLayoutPanel = new TableLayoutPanel();
        titleLabel = new Label();
        settingsLayoutPanel = new TableLayoutPanel();
        speedLabel = new Label();
        speedComboBox = new ComboBox();
        repeatCountLabel = new Label();
        repeatCountNumericUpDown = new NumericUpDown();
        initialDelayLabel = new Label();
        initialDelayNumericUpDown = new NumericUpDown();
        dividerPanel = new Panel();
        preserveTimingCheckBox = new CheckBox();
        loopIndefinitelyCheckBox = new CheckBox();
        stopOnErrorCheckBox = new CheckBox();
        relativeCoordinatesCheckBox = new CheckBox();
        rootLayoutPanel.SuspendLayout();
        settingsLayoutPanel.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)repeatCountNumericUpDown).BeginInit();
        ((System.ComponentModel.ISupportInitialize)initialDelayNumericUpDown).BeginInit();
        SuspendLayout();
        // 
        // rootLayoutPanel
        // 
        rootLayoutPanel.ColumnCount = 1;
        rootLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        rootLayoutPanel.Controls.Add(titleLabel, 0, 0);
        rootLayoutPanel.Controls.Add(settingsLayoutPanel, 0, 1);
        rootLayoutPanel.Dock = DockStyle.Fill;
        rootLayoutPanel.Location = new Point(0, 0);
        rootLayoutPanel.Margin = Padding.Empty;
        rootLayoutPanel.Name = "rootLayoutPanel";
        rootLayoutPanel.Padding = new Padding(18, 12, 18, 14);
        rootLayoutPanel.RowCount = 2;
        rootLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
        rootLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        rootLayoutPanel.Size = new Size(938, 174);
        rootLayoutPanel.TabIndex = 0;
        // 
        // titleLabel
        // 
        titleLabel.Dock = DockStyle.Fill;
        titleLabel.Location = new Point(19, 12);
        titleLabel.Name = "titleLabel";
        titleLabel.Size = new Size(900, 30);
        titleLabel.TabIndex = 0;
        titleLabel.Text = "Oynatma Ayarlari";
        titleLabel.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // settingsLayoutPanel
        // 
        settingsLayoutPanel.ColumnCount = 4;
        settingsLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150F));
        settingsLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 48F));
        settingsLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 1F));
        settingsLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 52F));
        settingsLayoutPanel.Controls.Add(speedLabel, 0, 0);
        settingsLayoutPanel.Controls.Add(speedComboBox, 1, 0);
        settingsLayoutPanel.Controls.Add(repeatCountLabel, 0, 1);
        settingsLayoutPanel.Controls.Add(repeatCountNumericUpDown, 1, 1);
        settingsLayoutPanel.Controls.Add(initialDelayLabel, 0, 2);
        settingsLayoutPanel.Controls.Add(initialDelayNumericUpDown, 1, 2);
        settingsLayoutPanel.Controls.Add(dividerPanel, 2, 0);
        settingsLayoutPanel.Controls.Add(preserveTimingCheckBox, 3, 0);
        settingsLayoutPanel.Controls.Add(loopIndefinitelyCheckBox, 3, 1);
        settingsLayoutPanel.Controls.Add(stopOnErrorCheckBox, 3, 2);
        settingsLayoutPanel.Controls.Add(relativeCoordinatesCheckBox, 3, 3);
        settingsLayoutPanel.Dock = DockStyle.Fill;
        settingsLayoutPanel.Location = new Point(16, 42);
        settingsLayoutPanel.Margin = Padding.Empty;
        settingsLayoutPanel.Name = "settingsLayoutPanel";
        settingsLayoutPanel.RowCount = 4;
        settingsLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
        settingsLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
        settingsLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
        settingsLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
        settingsLayoutPanel.Size = new Size(906, 118);
        settingsLayoutPanel.TabIndex = 1;
        // 
        // speedLabel
        // 
        speedLabel.Dock = DockStyle.Fill;
        speedLabel.Location = new Point(3, 0);
        speedLabel.Name = "speedLabel";
        speedLabel.Size = new Size(144, 29);
        speedLabel.TabIndex = 0;
        speedLabel.Text = "Hiz";
        speedLabel.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // speedComboBox
        // 
        speedComboBox.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        speedComboBox.FormattingEnabled = true;
        speedComboBox.Location = new Point(153, 3);
        speedComboBox.Name = "speedComboBox";
        speedComboBox.Size = new Size(356, 23);
        speedComboBox.TabIndex = 1;
        // 
        // repeatCountLabel
        // 
        repeatCountLabel.Dock = DockStyle.Fill;
        repeatCountLabel.Location = new Point(3, 29);
        repeatCountLabel.Name = "repeatCountLabel";
        repeatCountLabel.Size = new Size(144, 29);
        repeatCountLabel.TabIndex = 2;
        repeatCountLabel.Text = "Tekrar";
        repeatCountLabel.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // repeatCountNumericUpDown
        // 
        repeatCountNumericUpDown.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        repeatCountNumericUpDown.Location = new Point(153, 32);
        repeatCountNumericUpDown.Maximum = new decimal(new int[] { 100, 0, 0, 0 });
        repeatCountNumericUpDown.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
        repeatCountNumericUpDown.Name = "repeatCountNumericUpDown";
        repeatCountNumericUpDown.Size = new Size(356, 23);
        repeatCountNumericUpDown.TabIndex = 3;
        repeatCountNumericUpDown.Value = new decimal(new int[] { 1, 0, 0, 0 });
        // 
        // initialDelayLabel
        // 
        initialDelayLabel.Dock = DockStyle.Fill;
        initialDelayLabel.Location = new Point(3, 58);
        initialDelayLabel.Name = "initialDelayLabel";
        initialDelayLabel.Size = new Size(144, 29);
        initialDelayLabel.TabIndex = 4;
        initialDelayLabel.Text = "Baslangic Gecikmesi";
        initialDelayLabel.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // initialDelayNumericUpDown
        // 
        initialDelayNumericUpDown.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        initialDelayNumericUpDown.Increment = new decimal(new int[] { 100, 0, 0, 0 });
        initialDelayNumericUpDown.Location = new Point(153, 61);
        initialDelayNumericUpDown.Maximum = new decimal(new int[] { 60000, 0, 0, 0 });
        initialDelayNumericUpDown.Name = "initialDelayNumericUpDown";
        initialDelayNumericUpDown.Size = new Size(356, 23);
        initialDelayNumericUpDown.TabIndex = 5;
        // 
        // dividerPanel
        // 
        dividerPanel.Dock = DockStyle.Fill;
        dividerPanel.Location = new Point(512, 0);
        dividerPanel.Margin = Padding.Empty;
        dividerPanel.Name = "dividerPanel";
        settingsLayoutPanel.SetRowSpan(dividerPanel, 4);
        dividerPanel.Size = new Size(1, 118);
        dividerPanel.TabIndex = 6;
        // 
        // preserveTimingCheckBox
        // 
        preserveTimingCheckBox.AutoSize = true;
        preserveTimingCheckBox.Dock = DockStyle.Fill;
        preserveTimingCheckBox.Location = new Point(528, 3);
        preserveTimingCheckBox.Margin = new Padding(15, 3, 3, 3);
        preserveTimingCheckBox.Name = "preserveTimingCheckBox";
        preserveTimingCheckBox.Size = new Size(375, 23);
        preserveTimingCheckBox.TabIndex = 7;
        preserveTimingCheckBox.Text = "Zamanlamayi Koru";
        preserveTimingCheckBox.UseVisualStyleBackColor = false;
        // 
        // loopIndefinitelyCheckBox
        // 
        loopIndefinitelyCheckBox.AutoSize = true;
        loopIndefinitelyCheckBox.Dock = DockStyle.Fill;
        loopIndefinitelyCheckBox.Location = new Point(528, 32);
        loopIndefinitelyCheckBox.Margin = new Padding(15, 3, 3, 3);
        loopIndefinitelyCheckBox.Name = "loopIndefinitelyCheckBox";
        loopIndefinitelyCheckBox.Size = new Size(375, 23);
        loopIndefinitelyCheckBox.TabIndex = 8;
        loopIndefinitelyCheckBox.Text = "Sonsuz Dongu";
        loopIndefinitelyCheckBox.UseVisualStyleBackColor = false;
        // 
        // stopOnErrorCheckBox
        // 
        stopOnErrorCheckBox.AutoSize = true;
        stopOnErrorCheckBox.Dock = DockStyle.Fill;
        stopOnErrorCheckBox.Location = new Point(528, 61);
        stopOnErrorCheckBox.Margin = new Padding(15, 3, 3, 3);
        stopOnErrorCheckBox.Name = "stopOnErrorCheckBox";
        stopOnErrorCheckBox.Size = new Size(375, 23);
        stopOnErrorCheckBox.TabIndex = 9;
        stopOnErrorCheckBox.Text = "Hatada Durdur";
        stopOnErrorCheckBox.UseVisualStyleBackColor = false;
        // 
        // relativeCoordinatesCheckBox
        // 
        relativeCoordinatesCheckBox.AutoSize = true;
        relativeCoordinatesCheckBox.Dock = DockStyle.Fill;
        relativeCoordinatesCheckBox.Location = new Point(528, 90);
        relativeCoordinatesCheckBox.Margin = new Padding(15, 3, 3, 3);
        relativeCoordinatesCheckBox.Name = "relativeCoordinatesCheckBox";
        relativeCoordinatesCheckBox.Size = new Size(375, 25);
        relativeCoordinatesCheckBox.TabIndex = 10;
        relativeCoordinatesCheckBox.Text = "Goreceli koordinat";
        relativeCoordinatesCheckBox.UseVisualStyleBackColor = false;
        // 
        // PlaybackSettingsControl
        // 
        AutoScaleMode = AutoScaleMode.Font;
        Controls.Add(rootLayoutPanel);
        Name = "PlaybackSettingsControl";
        Size = new Size(938, 174);
        rootLayoutPanel.ResumeLayout(false);
        settingsLayoutPanel.ResumeLayout(false);
        settingsLayoutPanel.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)repeatCountNumericUpDown).EndInit();
        ((System.ComponentModel.ISupportInitialize)initialDelayNumericUpDown).EndInit();
        ResumeLayout(false);
    }
}
