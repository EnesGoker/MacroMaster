using MacroMaster.WinForms.Theme;
using System.Drawing.Drawing2D;
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
        _statusValueLabel.ForeColor = state.StatusText.Equals("Bos", StringComparison.OrdinalIgnoreCase)
            ? DesignTokens.TextPrimary
            : DesignTokens.AccentGreen;
    }

    private void BuildLayout()
    {
        var rootLayoutPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 5,
            BackColor = DesignTokens.Surface,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        rootLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        rootLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, DesignTokens.Scale(64)));
        rootLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, DesignTokens.Scale(86)));
        rootLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, DesignTokens.Scale(78)));
        rootLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, DesignTokens.Scale(78)));
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
        statsLayoutPanel.Controls.Add(CreateStatTile(
            "Olay",
            _eventCountValueLabel,
            new Padding(0, DesignTokens.Scale(8), DesignTokens.Scale(5), DesignTokens.Scale(8))),
            0,
            0);
        statsLayoutPanel.Controls.Add(CreateStatTile(
            "Sure",
            _durationValueLabel,
            new Padding(DesignTokens.Scale(5), DesignTokens.Scale(8), 0, DesignTokens.Scale(8))),
            1,
            0);

        rootLayoutPanel.Controls.Add(CreateDetailCard("Durum", _statusValueLabel, new Padding(0, 0, 0, DesignTokens.Scale(8))), 0, 0);
        rootLayoutPanel.Controls.Add(statsLayoutPanel, 0, 1);
        rootLayoutPanel.Controls.Add(CreateDetailCard(
            "Oturum",
            _sessionNameValueLabel,
            new Padding(0, DesignTokens.Scale(8), 0, DesignTokens.Scale(8))),
            0,
            2);
        rootLayoutPanel.Controls.Add(CreateDetailCard(
            "Dosya",
            _fileNameValueLabel,
            new Padding(0, DesignTokens.Scale(8), 0, DesignTokens.Scale(8))),
            0,
            3);

        Controls.Add(rootLayoutPanel);
    }

    private static SoftPanel CreateStatTile(string caption, Label valueLabel, Padding margin)
    {
        var panel = new SoftPanel
        {
            Dock = DockStyle.Fill,
            BackColor = DesignTokens.SurfaceInset,
            BorderColor = DesignTokens.BorderSoft,
            Margin = margin,
            Padding = new Padding(
                DesignTokens.Scale(12),
                DesignTokens.Scale(8),
                DesignTokens.Scale(12),
                DesignTokens.Scale(8))
        };

        valueLabel.ForeColor = DesignTokens.TextPrimary;

        var layoutPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = Color.Transparent,
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

    private static SoftPanel CreateDetailCard(string caption, Label valueLabel, Padding margin)
    {
        var panel = new SoftPanel
        {
            Dock = DockStyle.Fill,
            BackColor = DesignTokens.SurfaceInset,
            BorderColor = DesignTokens.BorderSoft,
            Margin = margin,
            Padding = new Padding(
                DesignTokens.Scale(12),
                DesignTokens.Scale(7),
                DesignTokens.Scale(12),
                DesignTokens.Scale(7))
        };

        var layoutPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = Color.Transparent,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        layoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, DesignTokens.Scale(24)));
        layoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
        layoutPanel.Controls.Add(CreateCaptionLabel(caption), 0, 0);
        layoutPanel.Controls.Add(valueLabel, 0, 1);
        panel.Controls.Add(layoutPanel);
        return panel;
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

    private sealed class SoftPanel : Panel
    {
        public Color BorderColor { get; set; } = DesignTokens.BorderSoft;

        public SoftPanel()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint,
                true);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            e.Graphics.Clear(Parent?.BackColor ?? DesignTokens.Surface);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle bounds = Rectangle.Inflate(ClientRectangle, -1, -1);
            if (bounds.Width <= 0 || bounds.Height <= 0)
            {
                return;
            }

            using GraphicsPath path = CreateRoundPath(bounds, DesignTokens.Scale(10));
            using var fillBrush = new SolidBrush(BackColor);
            using var borderPen = new Pen(BorderColor);
            e.Graphics.FillPath(fillBrush, path);
            e.Graphics.DrawPath(borderPen, path);
        }
    }

    private static GraphicsPath CreateRoundPath(Rectangle bounds, int radius)
    {
        var path = new GraphicsPath();
        int diameter = Math.Min(radius * 2, Math.Min(bounds.Width, bounds.Height));

        if (diameter <= 1)
        {
            path.AddRectangle(bounds);
            return path;
        }

        var arc = new Rectangle(bounds.Left, bounds.Top, diameter, diameter);
        path.AddArc(arc, 180, 90);
        arc.X = bounds.Right - diameter;
        path.AddArc(arc, 270, 90);
        arc.Y = bounds.Bottom - diameter;
        path.AddArc(arc, 0, 90);
        arc.X = bounds.Left;
        path.AddArc(arc, 90, 90);
        path.CloseFigure();
        return path;
    }
}
