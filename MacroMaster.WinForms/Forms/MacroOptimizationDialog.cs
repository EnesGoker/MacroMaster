using MacroMaster.Application.Abstractions;
using MacroMaster.WinForms.Platform;
using MacroMaster.WinForms.Theme;
using System.Drawing.Drawing2D;
using System.Globalization;

namespace MacroMaster.WinForms.Forms;

internal sealed class MacroOptimizationDialog : Form
{
    private readonly RoundedDialogButton _secondaryButton;

    public MacroOptimizationDialog(MacroOptimizationPreview preview)
    {
        ArgumentNullException.ThrowIfNull(preview);

        Text = "Makro Optimize Et";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        AutoScaleMode = AutoScaleMode.None;
        BackColor = DesignTokens.Surface;
        ForeColor = DesignTokens.TextPrimary;
        Font = DesignTokens.FontUiNormal;
        ClientSize = new Size(DesignTokens.Scale(540), DesignTokens.Scale(330));
        MinimumSize = Size;

        _secondaryButton = CreateDialogButton("Iptal", accent: false);
        BuildPreviewLayout(preview);
    }

    private MacroOptimizationDialog()
    {
        Text = "Makro Optimize Et";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        AutoScaleMode = AutoScaleMode.None;
        BackColor = DesignTokens.Surface;
        ForeColor = DesignTokens.TextPrimary;
        Font = DesignTokens.FontUiNormal;
        ClientSize = new Size(DesignTokens.Scale(460), DesignTokens.Scale(210));
        MinimumSize = Size;

        _secondaryButton = CreateDialogButton("Tamam", accent: true);
        BuildNoChangesLayout();
    }

    public static void ShowNoChanges(IWin32Window owner)
    {
        using var dialog = new MacroOptimizationDialog();
        _ = dialog.ShowDialog(owner);
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
            DwmWindowCornerPreference.RoundSmall);
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

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        _secondaryButton.Focus();
    }

    private void BuildPreviewLayout(MacroOptimizationPreview preview)
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
        rootLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, DesignTokens.Scale(62)));
        rootLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, DesignTokens.Scale(104)));
        rootLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
        rootLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, DesignTokens.Scale(54)));

        rootLayoutPanel.Controls.Add(CreateHeaderPanel(preview), 0, 0);
        rootLayoutPanel.Controls.Add(CreateStatsPanel(preview), 0, 1);
        rootLayoutPanel.Controls.Add(CreateSafetyPanel(preview), 0, 2);
        rootLayoutPanel.Controls.Add(CreateButtonPanel(applyButtonText: "Uygula"), 0, 3);

        Controls.Add(rootLayoutPanel);
    }

    private void BuildNoChangesLayout()
    {
        var rootLayoutPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = DesignTokens.Surface,
            Margin = Padding.Empty,
            Padding = new Padding(
                DesignTokens.Scale(22),
                DesignTokens.Scale(18),
                DesignTokens.Scale(22),
                DesignTokens.Scale(18))
        };
        rootLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
        rootLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, DesignTokens.Scale(54)));

        var messagePanel = new RoundedPanel
        {
            Dock = DockStyle.Fill,
            FillColor = DesignTokens.SurfaceInset,
            BorderColor = DesignTokens.BorderSoft,
            AccentColor = DesignTokens.AccentGreen,
            Margin = new Padding(0, 0, 0, DesignTokens.Scale(12)),
            Padding = new Padding(DesignTokens.Scale(18))
        };

        var messageLabel = new Label
        {
            Dock = DockStyle.Fill,
            Text = "Bu oturumda kaldirilabilecek gereksiz fare hareketi bulunamadi.",
            Font = DesignTokens.FontUiBold,
            ForeColor = DesignTokens.TextPrimary,
            BackColor = Color.Transparent,
            TextAlign = ContentAlignment.MiddleLeft,
            UseMnemonic = false,
            AutoEllipsis = true
        };
        messagePanel.Controls.Add(messageLabel);

        rootLayoutPanel.Controls.Add(messagePanel, 0, 0);
        rootLayoutPanel.Controls.Add(CreateButtonPanel(applyButtonText: null), 0, 1);
        Controls.Add(rootLayoutPanel);
    }

    private static TableLayoutPanel CreateHeaderPanel(MacroOptimizationPreview preview)
    {
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = DesignTokens.Surface,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, DesignTokens.Scale(32)));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

        panel.Controls.Add(
            new Label
            {
                Dock = DockStyle.Fill,
                Text = "Optimizasyon onizlemesi",
                Font = DesignTokens.FontUiBold,
                ForeColor = DesignTokens.TextPrimary,
                BackColor = DesignTokens.Surface,
                TextAlign = ContentAlignment.MiddleLeft,
                AutoEllipsis = true,
                UseMnemonic = false
            },
            0,
            0);
        panel.Controls.Add(
            new Label
            {
                Dock = DockStyle.Fill,
                Text = $"{preview.SessionName} icin gereksiz fare hareketleri sadelestirilecek.",
                Font = DesignTokens.FontUiSmall,
                ForeColor = DesignTokens.TextSecondary,
                BackColor = DesignTokens.Surface,
                TextAlign = ContentAlignment.TopLeft,
                AutoEllipsis = true,
                UseMnemonic = false
            },
            0,
            1);

        return panel;
    }

    private static TableLayoutPanel CreateStatsPanel(MacroOptimizationPreview preview)
    {
        var statsPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            RowCount = 1,
            BackColor = DesignTokens.Surface,
            Margin = new Padding(0, 0, 0, DesignTokens.Scale(12)),
            Padding = Padding.Empty
        };
        statsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
        statsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
        statsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
        statsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));

        statsPanel.Controls.Add(CreateStatCard("Once", preview.OriginalEventCount.ToString(CultureInfo.InvariantCulture)), 0, 0);
        statsPanel.Controls.Add(CreateStatCard("Yeni", preview.OptimizedEventCount.ToString(CultureInfo.InvariantCulture)), 1, 0);
        statsPanel.Controls.Add(CreateStatCard("Silinen", preview.RemovedEventCount.ToString(CultureInfo.InvariantCulture)), 2, 0);
        statsPanel.Controls.Add(CreateStatCard("Azalma", preview.ReductionPercent.ToString("0.#", CultureInfo.InvariantCulture) + "%"), 3, 0);
        return statsPanel;
    }

    private static RoundedPanel CreateSafetyPanel(MacroOptimizationPreview preview)
    {
        var panel = new RoundedPanel
        {
            Dock = DockStyle.Fill,
            FillColor = DesignTokens.SurfaceInset,
            BorderColor = DesignTokens.BorderSoft,
            AccentColor = preview.PreservesDuration
                ? DesignTokens.AccentGreen
                : DesignTokens.AccentOrange,
            Margin = new Padding(0, 0, 0, DesignTokens.Scale(12)),
            Padding = new Padding(
                DesignTokens.Scale(16),
                DesignTokens.Scale(12),
                DesignTokens.Scale(16),
                DesignTokens.Scale(12))
        };

        string durationText = preview.PreservesDuration
            ? "Toplam sure korunacak."
            : "Toplam surede fark olustu; uygulamadan once kontrol edin.";
        string detailText = $"{durationText} Klavye, tiklama ve wheel olaylari korunur; yalnizca kisa ara mouse hareketleri kaldirilir.";

        panel.Controls.Add(
            new Label
            {
                Dock = DockStyle.Fill,
                Text = detailText,
                Font = DesignTokens.FontUiSmall,
                ForeColor = DesignTokens.TextSecondary,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleLeft,
                UseMnemonic = false,
                AutoEllipsis = true
            });
        return panel;
    }

    private static RoundedPanel CreateStatCard(string caption, string value)
    {
        var panel = new RoundedPanel
        {
            Dock = DockStyle.Fill,
            FillColor = DesignTokens.SurfaceInset,
            BorderColor = DesignTokens.BorderSoft,
            AccentColor = DesignTokens.Accent,
            Margin = new Padding(0, 0, DesignTokens.Scale(8), 0),
            Padding = new Padding(
                DesignTokens.Scale(12),
                DesignTokens.Scale(10),
                DesignTokens.Scale(12),
                DesignTokens.Scale(8))
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
        layoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, DesignTokens.Scale(22)));
        layoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
        layoutPanel.Controls.Add(
            new Label
            {
                Dock = DockStyle.Fill,
                Text = caption,
                Font = DesignTokens.FontUiSmall,
                ForeColor = DesignTokens.TextSecondary,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleLeft,
                AutoEllipsis = true,
                UseMnemonic = false
            },
            0,
            0);
        layoutPanel.Controls.Add(
            new Label
            {
                Dock = DockStyle.Fill,
                Text = value,
                Font = DesignTokens.FontUiBold,
                ForeColor = DesignTokens.TextPrimary,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleLeft,
                AutoEllipsis = true,
                UseMnemonic = false
            },
            0,
            1);
        panel.Controls.Add(layoutPanel);
        return panel;
    }

    private FlowLayoutPanel CreateButtonPanel(string? applyButtonText)
    {
        var buttonPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            BackColor = DesignTokens.Surface,
            Margin = Padding.Empty,
            Padding = new Padding(0, DesignTokens.Scale(10), 0, 0)
        };

        _secondaryButton.DialogResult = applyButtonText is null
            ? DialogResult.OK
            : DialogResult.Cancel;
        CancelButton = _secondaryButton;
        buttonPanel.Controls.Add(_secondaryButton);

        if (applyButtonText is not null)
        {
            RoundedDialogButton applyButton = CreateDialogButton(applyButtonText, accent: true);
            applyButton.DialogResult = DialogResult.OK;
            AcceptButton = applyButton;
            buttonPanel.Controls.Add(applyButton);
        }
        else
        {
            AcceptButton = _secondaryButton;
        }

        return buttonPanel;
    }

    private static RoundedDialogButton CreateDialogButton(string text, bool accent)
    {
        return new RoundedDialogButton(accent)
        {
            Text = text,
            Width = DesignTokens.Scale(118),
            Height = DesignTokens.Scale(36),
            Margin = new Padding(DesignTokens.Scale(8), 0, 0, 0)
        };
    }

    private sealed class RoundedPanel : Panel
    {
        public Color FillColor { get; set; } = DesignTokens.SurfaceInset;

        public Color BorderColor { get; set; } = DesignTokens.BorderSoft;

        public Color AccentColor { get; set; } = DesignTokens.Accent;

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
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle bounds = Rectangle.Inflate(ClientRectangle, -1, -1);

            if (bounds.Width <= 0 || bounds.Height <= 0)
            {
                return;
            }

            using GraphicsPath path = CreateRoundPath(bounds, DesignTokens.Scale(10));
            using var fillBrush = new SolidBrush(FillColor);
            using var borderPen = new Pen(BorderColor);
            e.Graphics.FillPath(fillBrush, path);
            e.Graphics.DrawPath(borderPen, path);

            var accentBounds = new Rectangle(
                bounds.Left,
                bounds.Top + DesignTokens.Scale(8),
                Math.Max(DesignTokens.Scale(3), 2),
                Math.Max(DesignTokens.Scale(18), bounds.Height - DesignTokens.Scale(16)));
            using GraphicsPath accentPath = CreateRoundPath(accentBounds, DesignTokens.Scale(2));
            using var accentBrush = new SolidBrush(AccentColor);
            e.Graphics.FillPath(accentBrush, accentPath);
        }
    }

    private sealed class RoundedDialogButton : Button
    {
        private readonly bool _accent;
        private bool _hovered;
        private bool _pressed;

        public RoundedDialogButton(bool accent)
        {
            _accent = accent;
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint,
                true);

            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            Font = DesignTokens.FontUiBold;
            Cursor = Cursors.Hand;
            UseVisualStyleBackColor = false;
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            _hovered = true;
            Invalidate();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            _hovered = false;
            _pressed = false;
            Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnMouseDown(MouseEventArgs mevent)
        {
            if (Enabled && mevent.Button == MouseButtons.Left)
            {
                _pressed = true;
                Invalidate();
            }

            base.OnMouseDown(mevent);
        }

        protected override void OnMouseUp(MouseEventArgs mevent)
        {
            _pressed = false;
            Invalidate();
            base.OnMouseUp(mevent);
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            pevent.Graphics.Clear(Parent?.BackColor ?? DesignTokens.Surface);
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            Graphics graphics = pevent.Graphics;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle bounds = Rectangle.Inflate(ClientRectangle, -1, -1);
            if (bounds.Width <= 0 || bounds.Height <= 0)
            {
                return;
            }

            Color fillColor = ResolveFillColor();
            Color borderColor = _accent
                ? DesignTokens.Accent
                : DesignTokens.BorderBright;

            using GraphicsPath path = CreateRoundPath(bounds, DesignTokens.Scale(6));
            using var fillBrush = new SolidBrush(fillColor);
            using var borderPen = new Pen(borderColor, Math.Max(1f, DesignTokens.DensityScale));
            graphics.FillPath(fillBrush, path);
            graphics.DrawPath(borderPen, path);

            TextRenderer.DrawText(
                graphics,
                Text,
                Font,
                bounds,
                DesignTokens.TextPrimary,
                TextFormatFlags.HorizontalCenter |
                TextFormatFlags.VerticalCenter |
                TextFormatFlags.EndEllipsis |
                TextFormatFlags.NoPrefix);
        }

        private Color ResolveFillColor()
        {
            if (_accent)
            {
                if (_pressed)
                {
                    return DesignTokens.AccentDeep;
                }

                return _hovered
                    ? Color.FromArgb(37, 117, 232)
                    : Color.FromArgb(34, 103, 218);
            }

            if (_pressed)
            {
                return DesignTokens.Surface3;
            }

            return _hovered
                ? DesignTokens.SurfaceHover
                : DesignTokens.Surface2;
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
