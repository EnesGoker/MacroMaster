using MacroMaster.WinForms.Theme;
using System.Drawing.Drawing2D;
using System.Globalization;

namespace MacroMaster.WinForms.Controls;

internal sealed class MacroLibraryControl : UserControl
{
    private readonly FlowLayoutPanel _macroListPanel;
    private readonly Label _totalMacroValueLabel;
    private readonly Label _totalEventValueLabel;

    private static readonly MacroLibraryItem[] DesignPreviewItems =
    [
        new("Otomatik_Rapor.macro", "15.05.2024 14:30", 152, true),
        new("Veri_Girisi.macro", "14.05.2024 09:15", 87, false),
        new("Haftalik_Takip.macro", "13.05.2024 16:45", 203, false),
        new("Mail_Gonder.macro", "12.05.2024 11:20", 45, false),
        new("Excel_Islemleri.macro", "11.05.2024 10:10", 310, false),
        new("Sistem_Bakim.macro", "10.05.2024 18:05", 126, false),
        new("Uygulama_Test.macro", "09.05.2024 21:35", 174, false)
    ];

    public MacroLibraryControl()
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

        _macroListPanel = new FlowLayoutPanel();
        _totalMacroValueLabel = CreateFooterValueLabel();
        _totalEventValueLabel = CreateFooterValueLabel();

        BuildLayout();
        PopulateDesignPreviewItems();
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
            Padding = Padding.Empty
        };
        rootLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        rootLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 42f));
        rootLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 42f));
        rootLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
        rootLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 40f));

        var headerLayoutPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 1,
            BackColor = DesignTokens.Surface,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        headerLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 34f));
        headerLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        headerLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 38f));

        var iconLabel = new Label
        {
            Dock = DockStyle.Fill,
            Text = "M",
            Font = new Font(DesignTokens.FontUiLarge.FontFamily, 11f, FontStyle.Bold, GraphicsUnit.Point),
            ForeColor = DesignTokens.Accent,
            BackColor = DesignTokens.Surface,
            TextAlign = ContentAlignment.MiddleCenter
        };

        var titleLabel = new Label
        {
            Dock = DockStyle.Fill,
            Text = "Makro Kutuphanesi",
            Font = DesignTokens.FontUiLarge,
            ForeColor = DesignTokens.TextPrimary,
            BackColor = DesignTokens.Surface,
            TextAlign = ContentAlignment.MiddleLeft,
            AutoEllipsis = true
        };

        var addButton = new Button
        {
            Dock = DockStyle.Fill,
            Text = "+",
            BackColor = DesignTokens.Surface2,
            ForeColor = DesignTokens.TextPrimary,
            FlatStyle = FlatStyle.Flat,
            Font = new Font(DesignTokens.FontUiLarge.FontFamily, 16f, FontStyle.Bold, GraphicsUnit.Point),
            Margin = new Padding(4, 0, 0, 6),
            UseVisualStyleBackColor = false
        };
        addButton.FlatAppearance.BorderColor = DesignTokens.BorderBright;
        addButton.FlatAppearance.BorderSize = 1;

        headerLayoutPanel.Controls.Add(iconLabel, 0, 0);
        headerLayoutPanel.Controls.Add(titleLabel, 1, 0);
        headerLayoutPanel.Controls.Add(addButton, 2, 0);

        var searchPanel = new RoundedPanel
        {
            Dock = DockStyle.Fill,
            BackColor = DesignTokens.Background,
            BorderColor = DesignTokens.Border,
            Margin = new Padding(0, 2, 0, 8),
            Padding = new Padding(12, 0, 10, 0)
        };
        var searchLabel = new Label
        {
            Dock = DockStyle.Fill,
            Text = "Makro ara...",
            ForeColor = DesignTokens.TextMuted,
            Font = DesignTokens.FontUiNormal,
            BackColor = Color.Transparent,
            TextAlign = ContentAlignment.MiddleLeft
        };
        searchPanel.Controls.Add(searchLabel);

        _macroListPanel.Dock = DockStyle.Fill;
        _macroListPanel.FlowDirection = FlowDirection.TopDown;
        _macroListPanel.WrapContents = false;
        _macroListPanel.AutoScroll = true;
        _macroListPanel.BackColor = DesignTokens.Surface;
        _macroListPanel.Margin = Padding.Empty;
        _macroListPanel.Padding = Padding.Empty;
        _macroListPanel.Resize += (_, _) => ResizeLibraryRows();

        rootLayoutPanel.Controls.Add(headerLayoutPanel, 0, 0);
        rootLayoutPanel.Controls.Add(searchPanel, 0, 1);
        rootLayoutPanel.Controls.Add(_macroListPanel, 0, 2);
        rootLayoutPanel.Controls.Add(CreateFooterPanel(), 0, 3);

        Controls.Add(rootLayoutPanel);
    }

    private RoundedPanel CreateFooterPanel()
    {
        var footerPanel = new RoundedPanel
        {
            Dock = DockStyle.Fill,
            BackColor = DesignTokens.Background,
            BorderColor = DesignTokens.Border,
            Margin = new Padding(0, 8, 0, 0),
            Padding = new Padding(12, 0, 12, 0)
        };

        var footerLayoutPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            RowCount = 1,
            BackColor = Color.Transparent,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        footerLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35f));
        footerLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15f));
        footerLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35f));
        footerLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15f));

        footerLayoutPanel.Controls.Add(CreateFooterCaptionLabel("Toplam Makro"), 0, 0);
        footerLayoutPanel.Controls.Add(_totalMacroValueLabel, 1, 0);
        footerLayoutPanel.Controls.Add(CreateFooterCaptionLabel("Toplam Olay"), 2, 0);
        footerLayoutPanel.Controls.Add(_totalEventValueLabel, 3, 0);

        footerPanel.Controls.Add(footerLayoutPanel);
        return footerPanel;
    }

    private void PopulateDesignPreviewItems()
    {
        _macroListPanel.SuspendLayout();
        _macroListPanel.Controls.Clear();

        int totalEventCount = 0;
        foreach (MacroLibraryItem item in DesignPreviewItems)
        {
            totalEventCount += item.EventCount;
            _macroListPanel.Controls.Add(new MacroLibraryRow(item));
        }

        _totalMacroValueLabel.Text = DesignPreviewItems.Length.ToString(CultureInfo.InvariantCulture);
        _totalEventValueLabel.Text = totalEventCount.ToString("N0", CultureInfo.GetCultureInfo("tr-TR"));
        ResizeLibraryRows();
        _macroListPanel.ResumeLayout();
    }

    private void ResizeLibraryRows()
    {
        int rowWidth = Math.Max(
            160,
            _macroListPanel.ClientSize.Width - SystemInformation.VerticalScrollBarWidth - 8);

        foreach (Control control in _macroListPanel.Controls)
        {
            control.Width = rowWidth;
        }
    }

    private static Label CreateFooterCaptionLabel(string text)
    {
        return new Label
        {
            Dock = DockStyle.Fill,
            Text = text,
            Font = DesignTokens.FontUiNormal,
            ForeColor = DesignTokens.TextSecondary,
            BackColor = Color.Transparent,
            TextAlign = ContentAlignment.MiddleLeft,
            AutoEllipsis = true
        };
    }

    private static Label CreateFooterValueLabel()
    {
        return new Label
        {
            Dock = DockStyle.Fill,
            Font = DesignTokens.FontUiBold,
            ForeColor = DesignTokens.Accent,
            BackColor = Color.Transparent,
            TextAlign = ContentAlignment.MiddleRight,
            AutoEllipsis = true
        };
    }

    private sealed record MacroLibraryItem(
        string Name,
        string CreatedAtText,
        int EventCount,
        bool IsSelected);

    private sealed class MacroLibraryRow : RoundedPanel
    {
        private readonly MacroLibraryItem _item;

        public MacroLibraryRow(MacroLibraryItem item)
        {
            _item = item;
            Height = 76;
            Width = 320;
            Margin = new Padding(0, 0, 0, 8);
            Padding = new Padding(10, 9, 10, 9);
            BackColor = item.IsSelected
                ? Color.FromArgb(10, 47, 98)
                : Color.FromArgb(12, 20, 31);
            BorderColor = item.IsSelected
                ? DesignTokens.Accent
                : Color.FromArgb(16, 28, 44);

            BuildRow();
        }

        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);
            UpdateWidthFromParent();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            UpdateWidthFromParent();
        }

        private void UpdateWidthFromParent()
        {
            if (Parent is not null)
            {
                Width = Math.Max(
                    160,
                    Parent.ClientSize.Width - SystemInformation.VerticalScrollBarWidth - 8);
            }
        }

        private void BuildRow()
        {
            var layoutPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                BackColor = Color.Transparent,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            layoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 26f));
            layoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            layoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 50f));

            var iconLabel = new Label
            {
                Dock = DockStyle.Fill,
                Text = _item.IsSelected ? ">" : "-",
                Font = DesignTokens.FontUiBold,
                ForeColor = _item.IsSelected ? DesignTokens.Accent : DesignTokens.TextSecondary,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleCenter
            };

            var textLayoutPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = Color.Transparent,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            textLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 55f));
            textLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 45f));

            textLayoutPanel.Controls.Add(
                new Label
                {
                    Dock = DockStyle.Fill,
                    Text = _item.Name,
                    Font = DesignTokens.FontUiBold,
                    ForeColor = DesignTokens.TextPrimary,
                    BackColor = Color.Transparent,
                    TextAlign = ContentAlignment.MiddleLeft,
                    AutoEllipsis = true
                },
                0,
                0);
            textLayoutPanel.Controls.Add(
                new Label
                {
                    Dock = DockStyle.Fill,
                    Text = _item.CreatedAtText,
                    Font = DesignTokens.FontUiNormal,
                    ForeColor = DesignTokens.TextSecondary,
                    BackColor = Color.Transparent,
                    TextAlign = ContentAlignment.MiddleLeft,
                    AutoEllipsis = true
                },
                0,
                1);

            var countBadge = new RoundedPanel
            {
                Dock = DockStyle.Fill,
                BackColor = _item.IsSelected
                    ? Color.FromArgb(9, 68, 135)
                    : DesignTokens.Surface2,
                BorderColor = Color.Transparent,
                Margin = new Padding(6, 13, 0, 13),
                Padding = Padding.Empty
            };
            countBadge.Controls.Add(
                new Label
                {
                    Dock = DockStyle.Fill,
                    Text = _item.EventCount.ToString(CultureInfo.InvariantCulture),
                    Font = DesignTokens.FontUiNormal,
                    ForeColor = _item.IsSelected ? DesignTokens.Accent : DesignTokens.TextSecondary,
                    BackColor = Color.Transparent,
                    TextAlign = ContentAlignment.MiddleCenter
                });

            layoutPanel.Controls.Add(iconLabel, 0, 0);
            layoutPanel.Controls.Add(textLayoutPanel, 1, 0);
            layoutPanel.Controls.Add(countBadge, 2, 0);
            Controls.Add(layoutPanel);
        }
    }

    private class RoundedPanel : Panel
    {
        public Color BorderColor { get; set; } = DesignTokens.Border;

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

            using GraphicsPath path = CreateRoundPath(bounds, DesignTokens.Radius);
            using var fillBrush = new SolidBrush(BackColor);
            using var borderPen = new Pen(BorderColor);
            e.Graphics.FillPath(fillBrush, path);

            if (BorderColor.A > 0)
            {
                e.Graphics.DrawPath(borderPen, path);
            }
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
