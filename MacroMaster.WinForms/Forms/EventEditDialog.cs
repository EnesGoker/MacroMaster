using MacroMaster.Domain.Enums;
using MacroMaster.Domain.Models;
using MacroMaster.WinForms.Theme;

namespace MacroMaster.WinForms.Forms;

internal sealed class EventEditDialog : Form
{
    private readonly NumericUpDown _delayNumericUpDown = new();
    private readonly NumericUpDown _xNumericUpDown = new();
    private readonly NumericUpDown _yNumericUpDown = new();
    private readonly bool _coordinatesEditable;

    public EventEditDialog(int eventIndex, MacroEvent macroEvent)
    {
        ArgumentNullException.ThrowIfNull(macroEvent);

        _coordinatesEditable = macroEvent.EventType == MacroEventType.Mouse
            || macroEvent.X.HasValue
            || macroEvent.Y.HasValue;

        Text = $"Olay #{eventIndex + 1:000} Duzenle";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        AutoScaleMode = AutoScaleMode.None;
        BackColor = DesignTokens.Surface;
        ForeColor = DesignTokens.TextPrimary;
        Font = DesignTokens.FontUiNormal;
        ClientSize = new Size(DesignTokens.Scale(420), DesignTokens.Scale(250));
        MinimumSize = Size;

        BuildLayout(macroEvent);
    }

    public EventEditResult EditResult { get; private set; } = new(0, null, null);

    private void BuildLayout(MacroEvent macroEvent)
    {
        var rootLayoutPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            BackColor = DesignTokens.Surface,
            Padding = new Padding(DesignTokens.Scale(18)),
            Margin = Padding.Empty
        };
        rootLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, DesignTokens.Scale(42)));
        rootLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
        rootLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, DesignTokens.Scale(46)));

        var titleLabel = new Label
        {
            Dock = DockStyle.Fill,
            Text = "Olay ayarlarini duzenle",
            Font = DesignTokens.FontUiBold,
            ForeColor = DesignTokens.TextPrimary,
            BackColor = Color.Transparent,
            TextAlign = ContentAlignment.MiddleLeft
        };

        var fieldsLayoutPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 3,
            BackColor = Color.Transparent,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        fieldsLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, DesignTokens.Scale(142)));
        fieldsLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        fieldsLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, DesignTokens.Scale(42)));
        fieldsLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, DesignTokens.Scale(42)));
        fieldsLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, DesignTokens.Scale(42)));

        ConfigureNumericInput(_delayNumericUpDown, 0, 600000, macroEvent.DelayMs);
        ConfigureNumericInput(_xNumericUpDown, -100000, 100000, macroEvent.X ?? 0);
        ConfigureNumericInput(_yNumericUpDown, -100000, 100000, macroEvent.Y ?? 0);
        _xNumericUpDown.Enabled = _coordinatesEditable;
        _yNumericUpDown.Enabled = _coordinatesEditable;

        fieldsLayoutPanel.Controls.Add(CreateFieldLabel("Gecikme (ms)"), 0, 0);
        fieldsLayoutPanel.Controls.Add(_delayNumericUpDown, 1, 0);
        fieldsLayoutPanel.Controls.Add(CreateFieldLabel("X koordinati"), 0, 1);
        fieldsLayoutPanel.Controls.Add(_xNumericUpDown, 1, 1);
        fieldsLayoutPanel.Controls.Add(CreateFieldLabel("Y koordinati"), 0, 2);
        fieldsLayoutPanel.Controls.Add(_yNumericUpDown, 1, 2);

        var buttonLayoutPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            BackColor = Color.Transparent,
            Margin = Padding.Empty,
            Padding = new Padding(0, DesignTokens.Scale(10), 0, 0)
        };

        var saveButton = CreateDialogButton("Kaydet", isPrimary: true);
        var cancelButton = CreateDialogButton("Iptal", isPrimary: false);

        saveButton.Click += (_, _) => SaveAndClose();
        cancelButton.Click += (_, _) =>
        {
            DialogResult = DialogResult.Cancel;
            Close();
        };

        AcceptButton = saveButton;
        CancelButton = cancelButton;
        buttonLayoutPanel.Controls.Add(saveButton);
        buttonLayoutPanel.Controls.Add(cancelButton);

        rootLayoutPanel.Controls.Add(titleLabel, 0, 0);
        rootLayoutPanel.Controls.Add(fieldsLayoutPanel, 0, 1);
        rootLayoutPanel.Controls.Add(buttonLayoutPanel, 0, 2);

        Controls.Add(rootLayoutPanel);
    }

    private void SaveAndClose()
    {
        EditResult = new EventEditResult(
            Decimal.ToInt32(_delayNumericUpDown.Value),
            _coordinatesEditable ? Decimal.ToInt32(_xNumericUpDown.Value) : null,
            _coordinatesEditable ? Decimal.ToInt32(_yNumericUpDown.Value) : null);

        DialogResult = DialogResult.OK;
        Close();
    }

    private static void ConfigureNumericInput(
        NumericUpDown numericUpDown,
        decimal minimum,
        decimal maximum,
        decimal value)
    {
        numericUpDown.Dock = DockStyle.Fill;
        numericUpDown.Minimum = minimum;
        numericUpDown.Maximum = maximum;
        numericUpDown.Value = Math.Min(Math.Max(value, minimum), maximum);
        numericUpDown.BorderStyle = BorderStyle.FixedSingle;
        numericUpDown.BackColor = DesignTokens.SurfaceInset;
        numericUpDown.ForeColor = DesignTokens.TextPrimary;
        numericUpDown.Font = DesignTokens.FontUiNormal;
        numericUpDown.TextAlign = HorizontalAlignment.Right;
        numericUpDown.Margin = new Padding(0, DesignTokens.Scale(4), 0, DesignTokens.Scale(4));
    }

    private static Label CreateFieldLabel(string text)
    {
        return new Label
        {
            Dock = DockStyle.Fill,
            Text = text,
            Font = DesignTokens.FontUiNormal,
            ForeColor = DesignTokens.TextSecondary,
            BackColor = Color.Transparent,
            TextAlign = ContentAlignment.MiddleLeft
        };
    }

    private static Button CreateDialogButton(string text, bool isPrimary)
    {
        var button = new Button
        {
            Text = text,
            Width = DesignTokens.Scale(112),
            Height = DesignTokens.Scale(34),
            Margin = new Padding(DesignTokens.Scale(8), 0, 0, 0),
            FlatStyle = FlatStyle.Flat,
            UseVisualStyleBackColor = false,
            Font = DesignTokens.FontUiBold,
            BackColor = isPrimary ? DesignTokens.AccentDeep : DesignTokens.Surface2,
            ForeColor = DesignTokens.TextPrimary
        };

        button.FlatAppearance.BorderColor = isPrimary
            ? DesignTokens.Accent
            : DesignTokens.BorderBright;
        button.FlatAppearance.BorderSize = 1;
        return button;
    }
}

internal sealed record EventEditResult(int DelayMs, int? X, int? Y);
