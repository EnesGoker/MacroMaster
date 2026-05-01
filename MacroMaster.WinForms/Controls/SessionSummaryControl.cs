using MacroMaster.WinForms.Theme;
using System.Globalization;

namespace MacroMaster.WinForms.Controls;

public readonly record struct SessionSummaryState(
    string StatusText,
    string SessionName,
    int EventCount,
    int TotalDurationMs,
    string FileName);

internal sealed class SessionSummaryControl : UserControl
{
    private readonly Label _statusValueLabel;
    private readonly Label _eventCountValueLabel;
    private readonly Label _durationValueLabel;
    private readonly Label _sessionNameValueLabel;
    private readonly Label _fileNameValueLabel;

    public SessionSummaryControl()
    {
        SetStyle(
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.ResizeRedraw,
            true);

        DoubleBuffered = true;
        BackColor = DesignTokens.Surface;
        ForeColor = DesignTokens.TextPrimary;
        Font = DesignTokens.FontUiNormal;

        _statusValueLabel = CreateValueLabel();
        _eventCountValueLabel = CreateValueLabel();
        _durationValueLabel = CreateValueLabel();
        _sessionNameValueLabel = CreateValueLabel();
        _fileNameValueLabel = CreateValueLabel();

        BuildLayout();
        UpdateState(new SessionSummaryState("Bos", "Oturum yok", 0, 0, "Kaydedilmedi"));
    }

    public void UpdateState(SessionSummaryState state)
    {
        _statusValueLabel.Text = state.StatusText;
        _eventCountValueLabel.Text = state.EventCount.ToString(CultureInfo.InvariantCulture);
        _durationValueLabel.Text = FormattableString.Invariant($"{Math.Max(0, state.TotalDurationMs)} ms");
        _sessionNameValueLabel.Text = state.SessionName;
        _fileNameValueLabel.Text = state.FileName;
    }

    private void BuildLayout()
    {
        var rootLayoutPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            BackColor = DesignTokens.Surface,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        rootLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        rootLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 78f));
        rootLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 68f));
        rootLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

        var statsLayoutPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = DesignTokens.Surface,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        statsLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
        statsLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
        statsLayoutPanel.Controls.Add(CreateStatTile("Olay", _eventCountValueLabel), 0, 0);
        statsLayoutPanel.Controls.Add(CreateStatTile("Sure", _durationValueLabel), 1, 0);

        rootLayoutPanel.Controls.Add(CreateDetailRow("Durum", _statusValueLabel), 0, 0);
        rootLayoutPanel.Controls.Add(statsLayoutPanel, 0, 1);
        rootLayoutPanel.Controls.Add(CreateStackedDetailPanel(), 0, 2);

        Controls.Add(rootLayoutPanel);
    }

    private TableLayoutPanel CreateStackedDetailPanel()
    {
        var detailLayoutPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = DesignTokens.Surface,
            Margin = new Padding(0, 10, 0, 0),
            Padding = Padding.Empty
        };
        detailLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        detailLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));
        detailLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));
        detailLayoutPanel.Controls.Add(CreateDetailRow("Oturum", _sessionNameValueLabel), 0, 0);
        detailLayoutPanel.Controls.Add(CreateDetailRow("Dosya", _fileNameValueLabel), 0, 1);
        return detailLayoutPanel;
    }

    private static Panel CreateStatTile(string caption, Label valueLabel)
    {
        var panel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = DesignTokens.Background,
            Margin = new Padding(0, 0, 8, 0),
            Padding = new Padding(10, 8, 10, 8)
        };

        var layoutPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = DesignTokens.Background,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        layoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 45f));
        layoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 55f));
        layoutPanel.Controls.Add(CreateCaptionLabel(caption), 0, 0);
        layoutPanel.Controls.Add(valueLabel, 0, 1);
        panel.Controls.Add(layoutPanel);
        return panel;
    }

    private static TableLayoutPanel CreateDetailRow(string caption, Label valueLabel)
    {
        var layoutPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = DesignTokens.Surface,
            Margin = Padding.Empty,
            Padding = new Padding(0, 0, 0, 8)
        };
        layoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 24f));
        layoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
        layoutPanel.Controls.Add(CreateCaptionLabel(caption), 0, 0);
        layoutPanel.Controls.Add(valueLabel, 0, 1);
        return layoutPanel;
    }

    private static Label CreateCaptionLabel(string text)
    {
        return new Label
        {
            Dock = DockStyle.Fill,
            Text = text,
            Font = DesignTokens.FontUiNormal,
            ForeColor = DesignTokens.TextSecondary,
            BackColor = Color.Transparent,
            TextAlign = ContentAlignment.BottomLeft,
            AutoEllipsis = true
        };
    }

    private static Label CreateValueLabel()
    {
        return new Label
        {
            Dock = DockStyle.Fill,
            Font = DesignTokens.FontUiBold,
            ForeColor = DesignTokens.TextPrimary,
            BackColor = Color.Transparent,
            TextAlign = ContentAlignment.TopLeft,
            AutoEllipsis = true
        };
    }
}
