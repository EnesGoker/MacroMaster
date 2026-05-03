namespace MacroMaster.WinForms.Controls;

partial class PlaybackSettingsControl
{
    private System.ComponentModel.IContainer components = null;
    private TableLayoutPanel rootLayoutPanel = null!;
    private TableLayoutPanel settingsLayoutPanel = null!;
    private Label speedLabel = null!;
    private ComboBox speedComboBox = null!;
    private Label repeatCountLabel = null!;
    private NumericUpDown repeatCountNumericUpDown = null!;
    private Label initialDelayLabel = null!;
    private NumericUpDown initialDelayNumericUpDown = null!;
    private Label initialDelayUnitLabel = null!;
    private Panel dividerPanel = null!;
    private CheckBox preserveTimingCheckBox = null!;
    private CheckBox loopIndefinitelyCheckBox = null!;
    private CheckBox stopOnErrorCheckBox = null!;
    private CheckBox relativeCoordinatesCheckBox = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components is not null) components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        rootLayoutPanel = new TableLayoutPanel();
        settingsLayoutPanel = new TableLayoutPanel();
        speedLabel = new Label();
        speedComboBox = new ComboBox();
        repeatCountLabel = new Label();
        repeatCountNumericUpDown = new NumericUpDown();
        initialDelayLabel = new Label();
        initialDelayNumericUpDown = new NumericUpDown();
        initialDelayUnitLabel = new Label();
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

        // rootLayoutPanel
        rootLayoutPanel.ColumnCount = 1;
        rootLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        rootLayoutPanel.Controls.Add(settingsLayoutPanel, 0, 0);
        rootLayoutPanel.Dock = DockStyle.Fill;
        rootLayoutPanel.Margin = Padding.Empty;
        rootLayoutPanel.Name = "rootLayoutPanel";
        rootLayoutPanel.Padding = new Padding(18, 10, 18, 10);
        rootLayoutPanel.RowCount = 1;
        rootLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        // settingsLayoutPanel: [label | input | unit | divider | checkboxes]
        settingsLayoutPanel.ColumnCount = 5;
        settingsLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 28F));
        settingsLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
        settingsLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 32F));
        settingsLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 1F));
        settingsLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 52F));
        settingsLayoutPanel.RowCount = 4;
        settingsLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
        settingsLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
        settingsLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
        settingsLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
        settingsLayoutPanel.Dock = DockStyle.Fill;
        settingsLayoutPanel.Margin = Padding.Empty;
        settingsLayoutPanel.Name = "settingsLayoutPanel";

        // Row 0 — Hız
        settingsLayoutPanel.Controls.Add(speedLabel, 0, 0);
        settingsLayoutPanel.Controls.Add(speedComboBox, 1, 0);
        settingsLayoutPanel.Controls.Add(dividerPanel, 3, 0);
        settingsLayoutPanel.Controls.Add(preserveTimingCheckBox, 4, 0);
        // Row 1 — Tekrar
        settingsLayoutPanel.Controls.Add(repeatCountLabel, 0, 1);
        settingsLayoutPanel.Controls.Add(repeatCountNumericUpDown, 1, 1);
        settingsLayoutPanel.Controls.Add(loopIndefinitelyCheckBox, 4, 1);
        // Row 2 — Başlangıç Gecikmesi
        settingsLayoutPanel.Controls.Add(initialDelayLabel, 0, 2);
        settingsLayoutPanel.Controls.Add(initialDelayNumericUpDown, 1, 2);
        settingsLayoutPanel.Controls.Add(initialDelayUnitLabel, 2, 2);
        settingsLayoutPanel.Controls.Add(stopOnErrorCheckBox, 4, 2);
        // Row 3
        settingsLayoutPanel.Controls.Add(relativeCoordinatesCheckBox, 4, 3);

        speedLabel.Dock = DockStyle.Fill;
        speedLabel.Text = "Hız";
        speedLabel.TextAlign = ContentAlignment.MiddleLeft;
        speedLabel.AutoEllipsis = true;
        speedLabel.Name = "speedLabel";
        speedLabel.TabIndex = 0;

        speedComboBox.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        speedComboBox.FormattingEnabled = true;
        speedComboBox.Name = "speedComboBox";
        speedComboBox.TabIndex = 1;

        repeatCountLabel.Dock = DockStyle.Fill;
        repeatCountLabel.Text = "Tekrar";
        repeatCountLabel.TextAlign = ContentAlignment.MiddleLeft;
        repeatCountLabel.AutoEllipsis = true;
        repeatCountLabel.Name = "repeatCountLabel";
        repeatCountLabel.TabIndex = 2;

        repeatCountNumericUpDown.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        repeatCountNumericUpDown.Maximum = new decimal(new int[] { 100, 0, 0, 0 });
        repeatCountNumericUpDown.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
        repeatCountNumericUpDown.Value = new decimal(new int[] { 1, 0, 0, 0 });
        repeatCountNumericUpDown.Name = "repeatCountNumericUpDown";
        repeatCountNumericUpDown.TabIndex = 3;

        initialDelayLabel.Dock = DockStyle.Fill;
        initialDelayLabel.Text = "Başlangıç Gecikmesi";
        initialDelayLabel.TextAlign = ContentAlignment.MiddleLeft;
        initialDelayLabel.AutoEllipsis = true;
        initialDelayLabel.Name = "initialDelayLabel";
        initialDelayLabel.TabIndex = 4;

        initialDelayNumericUpDown.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        initialDelayNumericUpDown.Increment = new decimal(new int[] { 100, 0, 0, 0 });
        initialDelayNumericUpDown.Maximum = new decimal(new int[] { 60000, 0, 0, 0 });
        initialDelayNumericUpDown.Name = "initialDelayNumericUpDown";
        initialDelayNumericUpDown.TabIndex = 5;

        initialDelayUnitLabel.Dock = DockStyle.Fill;
        initialDelayUnitLabel.Text = "ms";
        initialDelayUnitLabel.TextAlign = ContentAlignment.MiddleLeft;
        initialDelayUnitLabel.Name = "initialDelayUnitLabel";
        initialDelayUnitLabel.TabIndex = 6;

        dividerPanel.Dock = DockStyle.Fill;
        dividerPanel.Margin = Padding.Empty;
        dividerPanel.Name = "dividerPanel";
        settingsLayoutPanel.SetRowSpan(dividerPanel, 4);

        preserveTimingCheckBox.Dock = DockStyle.Fill;
        preserveTimingCheckBox.Text = "Gerçek zamanlı";
        preserveTimingCheckBox.UseVisualStyleBackColor = false;
        preserveTimingCheckBox.Name = "preserveTimingCheckBox";
        preserveTimingCheckBox.TabIndex = 7;

        loopIndefinitelyCheckBox.Dock = DockStyle.Fill;
        loopIndefinitelyCheckBox.Text = "Sonsuza döngü";
        loopIndefinitelyCheckBox.UseVisualStyleBackColor = false;
        loopIndefinitelyCheckBox.Name = "loopIndefinitelyCheckBox";
        loopIndefinitelyCheckBox.TabIndex = 8;

        stopOnErrorCheckBox.Dock = DockStyle.Fill;
        stopOnErrorCheckBox.Text = "Hata'da durdur";
        stopOnErrorCheckBox.UseVisualStyleBackColor = false;
        stopOnErrorCheckBox.Name = "stopOnErrorCheckBox";
        stopOnErrorCheckBox.TabIndex = 9;

        relativeCoordinatesCheckBox.Dock = DockStyle.Fill;
        relativeCoordinatesCheckBox.Text = "Göreceli koordinat";
        relativeCoordinatesCheckBox.UseVisualStyleBackColor = false;
        relativeCoordinatesCheckBox.Name = "relativeCoordinatesCheckBox";
        relativeCoordinatesCheckBox.TabIndex = 10;

        AutoScaleMode = AutoScaleMode.None;
        Controls.Add(rootLayoutPanel);
        Name = "PlaybackSettingsControl";

        rootLayoutPanel.ResumeLayout(false);
        settingsLayoutPanel.ResumeLayout(false);
        settingsLayoutPanel.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)repeatCountNumericUpDown).EndInit();
        ((System.ComponentModel.ISupportInitialize)initialDelayNumericUpDown).EndInit();
        ResumeLayout(false);
    }
}