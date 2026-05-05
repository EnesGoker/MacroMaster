using MacroMaster.WinForms.Platform;
using MacroMaster.WinForms.Theme;
using System.Drawing.Drawing2D;

namespace MacroMaster.WinForms.Forms;

internal sealed class ThemedConfirmationDialog : Form
{
    private readonly Button _cancelButton;

    private ThemedConfirmationDialog(
        string title,
        string message,
        string detail,
        string confirmText,
        string cancelText,
        bool destructive)
    {
        Text = title;
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        AutoScaleMode = AutoScaleMode.None;
        BackColor = DesignTokens.Surface;
        ForeColor = DesignTokens.TextPrimary;
        Font = DesignTokens.FontUiNormal;
        ClientSize = new Size(DesignTokens.Scale(462), DesignTokens.Scale(214));
        MinimumSize = Size;

        _cancelButton = CreateDialogButton(cancelText, destructive: false);
        BuildLayout(message, detail, confirmText, destructive);
    }

    public static bool ConfirmMacroDelete(IWin32Window owner, string macroName)
    {
        string displayName = string.IsNullOrWhiteSpace(macroName)
            ? "Secili makro"
            : macroName.Trim();

        using var dialog = new ThemedConfirmationDialog(
            "Makro Sil",
            $"{displayName} kutuphaneden silinsin mi?",
            "Bu islem geri alinamaz.",
            "Evet",
            "Hayir",
            destructive: true);

        return dialog.ShowDialog(owner) == DialogResult.OK;
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
        _cancelButton.Focus();
    }

    private void BuildLayout(
        string message,
        string detail,
        string confirmText,
        bool destructive)
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
        rootLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, DesignTokens.Scale(40)));
        rootLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
        rootLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, DesignTokens.Scale(52)));

        var titleLabel = new Label
        {
            Dock = DockStyle.Fill,
            Text = "Makro silme onayi",
            Font = DesignTokens.FontUiBold,
            ForeColor = DesignTokens.TextPrimary,
            BackColor = Color.Transparent,
            TextAlign = ContentAlignment.MiddleLeft
        };

        var messagePanel = new RoundedPanel
        {
            Dock = DockStyle.Fill,
            FillColor = DesignTokens.SurfaceInset,
            BorderColor = DesignTokens.BorderSoft,
            Margin = new Padding(0, 0, 0, DesignTokens.Scale(10)),
            Padding = new Padding(DesignTokens.Scale(16), DesignTokens.Scale(12), DesignTokens.Scale(16), DesignTokens.Scale(12))
        };

        var messageLayoutPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = Color.Transparent,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        messageLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, DesignTokens.Scale(54)));
        messageLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));

        var warningGlyph = new WarningGlyphControl
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 0, DesignTokens.Scale(12), 0)
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
        textLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 62f));
        textLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 38f));

        var messageLabel = new Label
        {
            Dock = DockStyle.Fill,
            Text = message,
            Font = DesignTokens.FontUiBold,
            ForeColor = DesignTokens.TextPrimary,
            BackColor = Color.Transparent,
            TextAlign = ContentAlignment.BottomLeft,
            UseMnemonic = false
        };

        var detailLabel = new Label
        {
            Dock = DockStyle.Fill,
            Text = detail,
            Font = DesignTokens.FontUiSmall,
            ForeColor = DesignTokens.TextSecondary,
            BackColor = Color.Transparent,
            TextAlign = ContentAlignment.TopLeft,
            UseMnemonic = false
        };

        textLayoutPanel.Controls.Add(messageLabel, 0, 0);
        textLayoutPanel.Controls.Add(detailLabel, 0, 1);
        messageLayoutPanel.Controls.Add(warningGlyph, 0, 0);
        messageLayoutPanel.Controls.Add(textLayoutPanel, 1, 0);
        messagePanel.Controls.Add(messageLayoutPanel);

        var buttonLayoutPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            BackColor = Color.Transparent,
            Margin = Padding.Empty,
            Padding = new Padding(0, DesignTokens.Scale(12), 0, 0)
        };

        var confirmButton = CreateDialogButton(confirmText, destructive);
        confirmButton.DialogResult = DialogResult.OK;
        _cancelButton.DialogResult = DialogResult.Cancel;

        AcceptButton = _cancelButton;
        CancelButton = _cancelButton;

        buttonLayoutPanel.Controls.Add(_cancelButton);
        buttonLayoutPanel.Controls.Add(confirmButton);

        rootLayoutPanel.Controls.Add(titleLabel, 0, 0);
        rootLayoutPanel.Controls.Add(messagePanel, 0, 1);
        rootLayoutPanel.Controls.Add(buttonLayoutPanel, 0, 2);

        Controls.Add(rootLayoutPanel);
    }

    private static Button CreateDialogButton(string text, bool destructive)
    {
        Color fillColor = destructive
            ? DesignTokens.AccentRedSoft
            : DesignTokens.Surface2;
        Color borderColor = destructive
            ? DesignTokens.AccentRed
            : DesignTokens.BorderBright;
        Color hoverColor = destructive
            ? Color.FromArgb(104, DesignTokens.AccentRedSoft)
            : DesignTokens.SurfaceHover;
        Color pressedColor = destructive
            ? Color.FromArgb(132, DesignTokens.AccentRedSoft)
            : DesignTokens.Surface3;

        var button = new Button
        {
            Text = text,
            Width = DesignTokens.Scale(112),
            Height = DesignTokens.Scale(34),
            Margin = new Padding(DesignTokens.Scale(8), 0, 0, 0),
            FlatStyle = FlatStyle.Flat,
            UseVisualStyleBackColor = false,
            Font = DesignTokens.FontUiBold,
            BackColor = fillColor,
            ForeColor = DesignTokens.TextPrimary
        };

        button.FlatAppearance.BorderColor = borderColor;
        button.FlatAppearance.BorderSize = 1;
        button.FlatAppearance.MouseOverBackColor = hoverColor;
        button.FlatAppearance.MouseDownBackColor = pressedColor;
        return button;
    }

    private sealed class WarningGlyphControl : Control
    {
        public WarningGlyphControl()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint,
                true);
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            pevent.Graphics.Clear(Parent?.BackColor ?? DesignTokens.SurfaceInset);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            int size = Math.Min(DesignTokens.Scale(32), Math.Min(Width, Height) - DesignTokens.Scale(4));
            if (size <= 0)
            {
                return;
            }

            var bounds = new Rectangle(
                (Width - size) / 2,
                (Height - size) / 2,
                size,
                size);

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using var fillBrush = new SolidBrush(Color.FromArgb(36, DesignTokens.AccentOrange));
            using var borderPen = new Pen(DesignTokens.AccentOrange, Math.Max(1f, DesignTokens.DensityScale));
            using var textBrush = new SolidBrush(DesignTokens.AccentOrange);
            using var textFont = new Font(
                "Segoe UI",
                Math.Max(12f, DesignTokens.ScaleFont(16f)),
                FontStyle.Bold,
                GraphicsUnit.Pixel);
            using var format = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

            e.Graphics.FillEllipse(fillBrush, bounds);
            e.Graphics.DrawEllipse(borderPen, bounds);
            e.Graphics.DrawString("!", textFont, textBrush, bounds, format);
            e.Graphics.SmoothingMode = SmoothingMode.None;
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

            Rectangle bounds = Rectangle.Inflate(ClientRectangle, -1, -1);
            if (bounds.Width <= 0 || bounds.Height <= 0)
            {
                return;
            }

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using GraphicsPath path = CreateRoundedRectanglePath(bounds, DesignTokens.Scale(9));
            using var fillBrush = new SolidBrush(FillColor);
            using var borderPen = new Pen(BorderColor);

            e.Graphics.FillPath(fillBrush, path);
            e.Graphics.DrawPath(borderPen, path);
            e.Graphics.SmoothingMode = SmoothingMode.None;
        }
    }

    private static GraphicsPath CreateRoundedRectanglePath(Rectangle bounds, int radius)
    {
        var path = new GraphicsPath();

        if (bounds.Width <= 0 || bounds.Height <= 0)
        {
            return path;
        }

        int diameter = Math.Min(radius * 2, Math.Min(bounds.Width, bounds.Height));
        var arc = new Rectangle(bounds.Location, new Size(diameter, diameter));

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
