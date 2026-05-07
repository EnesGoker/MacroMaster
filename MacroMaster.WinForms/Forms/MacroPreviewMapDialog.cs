using MacroMaster.Domain.Models;
using MacroMaster.WinForms.Controls;
using MacroMaster.WinForms.Platform;
using MacroMaster.WinForms.Theme;
using System.Drawing.Drawing2D;
using System.Globalization;

namespace MacroMaster.WinForms.Forms;

internal sealed class MacroPreviewMapDialog : Form
{
    private readonly Label _sessionNameLabel;
    private readonly Label _statusValueLabel;
    private readonly Label _eventCountValueLabel;
    private readonly Label _durationValueLabel;
    private readonly Label _fileNameValueLabel;
    private readonly MacroPreviewMapControl _mapControl;

    public MacroPreviewMapDialog()
    {
        Text = "Makro Onizleme Haritasi";
        StartPosition = FormStartPosition.Manual;
        FormBorderStyle = FormBorderStyle.None;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        KeyPreview = true;
        AutoScaleMode = AutoScaleMode.None;
        BackColor = DesignTokens.Surface;
        ForeColor = DesignTokens.TextPrimary;
        Font = DesignTokens.FontUiNormal;
        ClientSize = new Size(DesignTokens.Scale(780), DesignTokens.Scale(540));
        MinimumSize = Size;

        _sessionNameLabel = CreateHeaderTitleLabel();
        _statusValueLabel = CreateMetricValueLabel();
        _eventCountValueLabel = CreateMetricValueLabel();
        _durationValueLabel = CreateMetricValueLabel();
        _fileNameValueLabel = CreateMetricValueLabel();
        _mapControl = new MacroPreviewMapControl
        {
            AccessibleName = "Buyuk makro onizleme haritasi",
            AccessibleDescription = "Secili makronun fare rotasini genis harita olarak gosterir.",
            Cursor = Cursors.Default
        };

        BuildLayout();
    }

    public void PositionNear(
        Rectangle anchorScreenBounds,
        Rectangle ownerScreenBounds)
    {
        Rectangle referenceBounds = anchorScreenBounds.IsEmpty
            ? ownerScreenBounds
            : anchorScreenBounds;
        Rectangle workingArea = Screen.FromRectangle(referenceBounds).WorkingArea;
        int margin = DesignTokens.Scale(12);

        int preferredX = anchorScreenBounds.IsEmpty
            ? ownerScreenBounds.Left + (ownerScreenBounds.Width - Width) / 2
            : anchorScreenBounds.Left - Width - DesignTokens.Scale(16);

        if (preferredX < ownerScreenBounds.Left + margin)
        {
            preferredX = ownerScreenBounds.Left + (ownerScreenBounds.Width - Width) / 2;
        }

        int preferredY = anchorScreenBounds.IsEmpty
            ? ownerScreenBounds.Top + (ownerScreenBounds.Height - Height) / 2
            : anchorScreenBounds.Top + (anchorScreenBounds.Height - Height) / 2;

        Location = new Point(
            ClampScreenCoordinate(preferredX, workingArea.Left + margin, workingArea.Right - Width - margin),
            ClampScreenCoordinate(preferredY, workingArea.Top + margin, workingArea.Bottom - Height - margin));
    }

    public void UpdatePreview(
        SessionSummaryState state,
        IReadOnlyList<MacroEvent>? events,
        int? activeSourceEventIndex)
    {
        string sessionName = string.IsNullOrWhiteSpace(state.SessionName)
            ? "Oturum yok"
            : state.SessionName;
        string fileName = string.IsNullOrWhiteSpace(state.FileName)
            ? "Kaydedilmedi"
            : state.FileName;

        _sessionNameLabel.Text = sessionName;
        _statusValueLabel.Text = state.StatusText;
        _statusValueLabel.ForeColor = ResolveStatusColor(state.StatusText);
        _eventCountValueLabel.Text = Math.Max(0, state.EventCount).ToString(CultureInfo.InvariantCulture);
        _durationValueLabel.Text = FormattableString.Invariant($"{Math.Max(0, state.TotalDurationMs)} ms");
        _fileNameValueLabel.Text = fileName;
        _mapControl.UpdatePreview(
            state.EventCount,
            state.TotalDurationMs,
            state.StatusText,
            events,
            activeSourceEventIndex);
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);

        WindowChromeNative.TryApplyDwmBoolAttribute(
            Handle,
            DwmWindowAttribute.UseImmersiveDarkMode,
            enabled: true);
        WindowChromeNative.TryApplyDwmCornerPreference(
            Handle,
            DwmWindowCornerPreference.Round);
        WindowChromeNative.TryApplyDwmColorAttribute(
            Handle,
            DwmWindowAttribute.BorderColor,
            DesignTokens.Border);
        WindowChromeNative.TryApplyDwmColorAttribute(
            Handle,
            DwmWindowAttribute.CaptionColor,
            DesignTokens.Surface);
        WindowChromeNative.TryApplyDwmColorAttribute(
            Handle,
            DwmWindowAttribute.TextColor,
            DesignTokens.TextPrimary);
    }

    protected override void OnDeactivate(EventArgs e)
    {
        base.OnDeactivate(e);

        if (!IsDisposed && Visible)
        {
            Close();
        }
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (e.KeyCode == Keys.Escape)
        {
            e.Handled = true;
            Close();
        }
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        UpdateRoundedRegion();
    }

    private void UpdateRoundedRegion()
    {
        if (ClientSize.Width <= 0 || ClientSize.Height <= 0)
        {
            return;
        }

        using GraphicsPath path = CreateRoundPath(
            new Rectangle(Point.Empty, ClientSize),
            DesignTokens.Scale(14));
        Region? previousRegion = Region;
        Region = new Region(path);
        previousRegion?.Dispose();
    }

    private void BuildLayout()
    {
        var rootLayoutPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            BackColor = DesignTokens.Surface,
            Margin = Padding.Empty,
            Padding = new Padding(
                DesignTokens.Scale(22),
                DesignTokens.Scale(18),
                DesignTokens.Scale(22),
                DesignTokens.Scale(18))
        };
        rootLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        rootLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, DesignTokens.Scale(70)));
        rootLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, DesignTokens.Scale(86)));
        rootLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
        rootLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, DesignTokens.Scale(38)));

        rootLayoutPanel.Controls.Add(CreateHeaderPanel(), 0, 0);
        rootLayoutPanel.Controls.Add(CreateMetricsPanel(), 0, 1);
        rootLayoutPanel.Controls.Add(CreateMapHost(), 0, 2);
        rootLayoutPanel.Controls.Add(CreateLegendPanel(), 0, 3);

        Controls.Add(rootLayoutPanel);
    }

    private TableLayoutPanel CreateHeaderPanel()
    {
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = DesignTokens.Surface,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, DesignTokens.Scale(70)));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));

        var iconPanel = new PreviewIconPanel
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 0, DesignTokens.Scale(14), DesignTokens.Scale(12))
        };

        var textPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = DesignTokens.Surface,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        textPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 60f));
        textPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 40f));
        textPanel.Controls.Add(_sessionNameLabel, 0, 0);
        textPanel.Controls.Add(CreateSubtitleLabel(), 0, 1);

        panel.Controls.Add(iconPanel, 0, 0);
        panel.Controls.Add(textPanel, 1, 0);
        return panel;
    }

    private TableLayoutPanel CreateMetricsPanel()
    {
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            RowCount = 1,
            BackColor = DesignTokens.Surface,
            Margin = new Padding(0, 0, 0, DesignTokens.Scale(12)),
            Padding = Padding.Empty
        };

        for (int index = 0; index < 4; index++)
        {
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
        }

        panel.Controls.Add(new MetricCard("Durum", _statusValueLabel), 0, 0);
        panel.Controls.Add(new MetricCard("Olay", _eventCountValueLabel), 1, 0);
        panel.Controls.Add(new MetricCard("Sure", _durationValueLabel), 2, 0);
        panel.Controls.Add(new MetricCard("Dosya", _fileNameValueLabel), 3, 0);
        return panel;
    }

    private RoundedPanel CreateMapHost()
    {
        var panel = new RoundedPanel
        {
            Dock = DockStyle.Fill,
            BackColor = DesignTokens.SurfaceInset,
            FillColor = DesignTokens.SurfaceInset,
            BorderColor = DesignTokens.BorderSoft,
            Margin = Padding.Empty,
            Padding = new Padding(DesignTokens.Scale(10))
        };
        panel.Controls.Add(_mapControl);
        return panel;
    }

    private static FlowLayoutPanel CreateLegendPanel()
    {
        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            BackColor = DesignTokens.Surface,
            Margin = new Padding(0, DesignTokens.Scale(10), 0, 0),
            Padding = Padding.Empty
        };
        panel.Controls.Add(new LegendItem("Baslangic", DesignTokens.AccentGreen));
        panel.Controls.Add(new LegendItem("Aktif nokta", DesignTokens.AccentOrange));
        panel.Controls.Add(new LegendItem("Tiklama", DesignTokens.AccentRed));
        panel.Controls.Add(new LegendItem("Wheel", DesignTokens.AccentPurple));
        return panel;
    }

    private static Label CreateHeaderTitleLabel()
    {
        return new Label
        {
            Dock = DockStyle.Fill,
            Text = "Oturum yok",
            Font = DesignTokens.FontUiLarge,
            ForeColor = DesignTokens.TextPrimary,
            BackColor = DesignTokens.Surface,
            TextAlign = ContentAlignment.BottomLeft,
            AutoEllipsis = true,
            UseMnemonic = false
        };
    }

    private static Label CreateSubtitleLabel()
    {
        return new Label
        {
            Dock = DockStyle.Fill,
            Text = "Makro fare rotasi ve aktif olay onizlemesi",
            Font = DesignTokens.FontUiSmall,
            ForeColor = DesignTokens.TextSecondary,
            BackColor = DesignTokens.Surface,
            TextAlign = ContentAlignment.TopLeft,
            AutoEllipsis = true,
            UseMnemonic = false
        };
    }

    private static Label CreateMetricValueLabel()
    {
        return new Label
        {
            Dock = DockStyle.Fill,
            Font = DesignTokens.FontUiBold,
            ForeColor = DesignTokens.TextPrimary,
            BackColor = DesignTokens.SurfaceInset,
            TextAlign = ContentAlignment.MiddleLeft,
            AutoEllipsis = true,
            UseMnemonic = false
        };
    }

    private static Color ResolveStatusColor(string statusText)
    {
        return statusText.Equals("Bos", StringComparison.OrdinalIgnoreCase)
            ? DesignTokens.TextPrimary
            : DesignTokens.AccentGreen;
    }

    private static int ClampScreenCoordinate(
        int value,
        int minimum,
        int maximum)
    {
        return minimum > maximum
            ? minimum
            : Math.Clamp(value, minimum, maximum);
    }

    private sealed class MetricCard : UserControl
    {
        private readonly Label _valueLabel;

        public MetricCard(string caption, Label valueLabel)
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint,
                true);

            _valueLabel = valueLabel;

            Dock = DockStyle.Fill;
            Margin = new Padding(0, 0, DesignTokens.Scale(10), 0);
            Padding = new Padding(
                DesignTokens.Scale(14),
                DesignTokens.Scale(8),
                DesignTokens.Scale(14),
                DesignTokens.Scale(8));
            BackColor = DesignTokens.SurfaceInset;

            var layoutPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = DesignTokens.SurfaceInset,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            layoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 45f));
            layoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 55f));
            layoutPanel.Controls.Add(CreateCaptionLabel(caption), 0, 0);
            layoutPanel.Controls.Add(_valueLabel, 0, 1);
            Controls.Add(layoutPanel);
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
            using var fillBrush = new SolidBrush(DesignTokens.SurfaceInset);
            using var borderPen = new Pen(DesignTokens.BorderSoft);
            e.Graphics.FillPath(fillBrush, path);
            e.Graphics.DrawPath(borderPen, path);
        }

        private static Label CreateCaptionLabel(string text)
        {
            return new Label
            {
                Dock = DockStyle.Fill,
                Text = text,
                Font = DesignTokens.FontUiSmall,
                ForeColor = DesignTokens.TextSecondary,
                BackColor = DesignTokens.SurfaceInset,
                TextAlign = ContentAlignment.MiddleLeft,
                AutoEllipsis = true,
                UseMnemonic = false
            };
        }
    }

    private sealed class LegendItem : Control
    {
        private readonly string _text;
        private readonly Color _dotColor;

        public LegendItem(string text, Color dotColor)
        {
            _text = text;
            _dotColor = dotColor;
            Width = DesignTokens.Scale(132);
            Height = DesignTokens.Scale(28);
            Margin = new Padding(0, 0, DesignTokens.Scale(8), 0);
            BackColor = DesignTokens.Surface;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            int radius = DesignTokens.Scale(4);
            int centerY = ClientRectangle.Top + ClientRectangle.Height / 2;
            using var brush = new SolidBrush(_dotColor);
            e.Graphics.FillEllipse(
                brush,
                DesignTokens.Scale(2),
                centerY - radius,
                radius * 2,
                radius * 2);
            TextRenderer.DrawText(
                e.Graphics,
                _text,
                DesignTokens.FontUiSmall,
                new Rectangle(
                    DesignTokens.Scale(16),
                    0,
                    Math.Max(0, Width - DesignTokens.Scale(16)),
                    Height),
                DesignTokens.TextSecondary,
                TextFormatFlags.VerticalCenter |
                TextFormatFlags.Left |
                TextFormatFlags.EndEllipsis |
                TextFormatFlags.NoPrefix);
        }
    }

    private sealed class PreviewIconPanel : Control
    {
        public PreviewIconPanel()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint,
                true);
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

            using GraphicsPath path = CreateRoundPath(bounds, DesignTokens.Scale(12));
            using var fillBrush = new SolidBrush(DesignTokens.AccentDeep);
            using var borderPen = new Pen(Color.FromArgb(150, DesignTokens.Accent));
            e.Graphics.FillPath(fillBrush, path);
            e.Graphics.DrawPath(borderPen, path);

            PointF[] cursor =
            [
                new(bounds.Left + bounds.Width * 0.33f, bounds.Top + bounds.Height * 0.28f),
                new(bounds.Left + bounds.Width * 0.70f, bounds.Top + bounds.Height * 0.56f),
                new(bounds.Left + bounds.Width * 0.52f, bounds.Top + bounds.Height * 0.60f),
                new(bounds.Left + bounds.Width * 0.62f, bounds.Top + bounds.Height * 0.80f),
                new(bounds.Left + bounds.Width * 0.50f, bounds.Top + bounds.Height * 0.84f),
                new(bounds.Left + bounds.Width * 0.40f, bounds.Top + bounds.Height * 0.62f),
                new(bounds.Left + bounds.Width * 0.33f, bounds.Top + bounds.Height * 0.70f)
            ];

            using var cursorBrush = new SolidBrush(Color.FromArgb(235, Color.White));
            using var cursorPen = new Pen(DesignTokens.Accent, Math.Max(1.5f, DesignTokens.DensityScale));
            e.Graphics.FillPolygon(cursorBrush, cursor);
            e.Graphics.DrawPolygon(cursorPen, cursor);
        }
    }

    private sealed class RoundedPanel : Panel
    {
        public Color FillColor { get; set; } = DesignTokens.SurfaceInset;
        public Color BorderColor { get; set; } = DesignTokens.BorderSoft;

        public RoundedPanel()
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

            using GraphicsPath path = CreateRoundPath(bounds, DesignTokens.Scale(12));
            using var fillBrush = new SolidBrush(FillColor);
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
