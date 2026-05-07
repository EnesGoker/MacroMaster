using MacroMaster.WinForms.Platform;
using MacroMaster.WinForms.Theme;
using System.Drawing.Drawing2D;

namespace MacroMaster.WinForms.Forms;

internal sealed class ThemedConfirmationDialog : Form
{
    private readonly RoundedDialogButton _cancelButton;

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
        ClientSize = new Size(DesignTokens.Scale(468), DesignTokens.Scale(204));
        MinimumSize = Size;

        _cancelButton = CreateDialogButton(cancelText, destructive: false);
        BuildLayout(message, detail, confirmText, destructive);
    }

    public static bool ConfirmMacroDelete(IWin32Window owner, string macroName)
    {
        string displayName = string.IsNullOrWhiteSpace(macroName)
            ? "Seçili makro"
            : macroName.Trim();

        using var dialog = new ThemedConfirmationDialog(
            "Makro Sil",
            $"{displayName} kütüphaneden silinsin mi?",
            "Bu işlem geri alınamaz.",
            "Sil",
            "Vazgeç",
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
            RowCount = 2,
            BackColor = DesignTokens.Surface,
            Padding = new Padding(
                DesignTokens.Scale(22),
                DesignTokens.Scale(18),
                DesignTokens.Scale(22),
                DesignTokens.Scale(18)),
            Margin = Padding.Empty
        };
        rootLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
        rootLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, DesignTokens.Scale(52)));

        var contentLayoutPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = Color.Transparent,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        contentLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, DesignTokens.Scale(58)));
        contentLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));

        var warningBadge = new WarningBadgeControl(destructive)
        {
            Dock = DockStyle.Top,
            Width = DesignTokens.Scale(42),
            Height = DesignTokens.Scale(42),
            Margin = new Padding(0, DesignTokens.Scale(4), DesignTokens.Scale(16), 0)
        };

        var textLayoutPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            BackColor = Color.Transparent,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        textLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, DesignTokens.Scale(24)));
        textLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
        textLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, DesignTokens.Scale(26)));

        Label eyebrowLabel = CreateLabel(
            destructive ? "Kalıcı işlem" : "Onay gerekli",
            DesignTokens.FontUiSmall,
            destructive ? DesignTokens.AccentRed : DesignTokens.Accent,
            ContentAlignment.MiddleLeft);
        Label messageLabel = CreateLabel(
            message,
            DesignTokens.FontUiBold,
            DesignTokens.TextPrimary,
            ContentAlignment.MiddleLeft);
        Label detailLabel = CreateLabel(
            detail,
            DesignTokens.FontUiSmall,
            DesignTokens.TextSecondary,
            ContentAlignment.MiddleLeft);

        textLayoutPanel.Controls.Add(eyebrowLabel, 0, 0);
        textLayoutPanel.Controls.Add(messageLabel, 0, 1);
        textLayoutPanel.Controls.Add(detailLabel, 0, 2);
        contentLayoutPanel.Controls.Add(warningBadge, 0, 0);
        contentLayoutPanel.Controls.Add(textLayoutPanel, 1, 0);

        var buttonLayoutPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 1,
            BackColor = Color.Transparent,
            Margin = Padding.Empty,
            Padding = new Padding(0, DesignTokens.Scale(12), 0, 0)
        };
        buttonLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        buttonLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, DesignTokens.Scale(112)));
        buttonLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, DesignTokens.Scale(112)));
        buttonLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

        var confirmButton = CreateDialogButton(confirmText, destructive);
        confirmButton.DialogResult = DialogResult.OK;
        _cancelButton.DialogResult = DialogResult.Cancel;

        _cancelButton.Dock = DockStyle.Fill;
        confirmButton.Dock = DockStyle.Fill;
        _cancelButton.Margin = new Padding(0, 0, DesignTokens.Scale(8), 0);
        confirmButton.Margin = Padding.Empty;

        AcceptButton = _cancelButton;
        CancelButton = _cancelButton;

        buttonLayoutPanel.Controls.Add(_cancelButton, 1, 0);
        buttonLayoutPanel.Controls.Add(confirmButton, 2, 0);

        rootLayoutPanel.Controls.Add(contentLayoutPanel, 0, 0);
        rootLayoutPanel.Controls.Add(buttonLayoutPanel, 0, 1);
        Controls.Add(rootLayoutPanel);
    }

    private static Label CreateLabel(
        string text,
        Font font,
        Color foreColor,
        ContentAlignment textAlign)
    {
        return new Label
        {
            Dock = DockStyle.Fill,
            Text = text,
            Font = font,
            ForeColor = foreColor,
            BackColor = Color.Transparent,
            TextAlign = textAlign,
            UseMnemonic = false,
            AutoEllipsis = true,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
    }

    private static RoundedDialogButton CreateDialogButton(string text, bool destructive)
    {
        return new RoundedDialogButton(destructive)
        {
            Text = text,
            Width = DesignTokens.Scale(104),
            Height = DesignTokens.Scale(36)
        };
    }

    private static Color ResolvePaintBackColor(Control? control, Color fallback)
    {
        while (control is not null)
        {
            Color backColor = control.BackColor;
            if (backColor.A == byte.MaxValue)
            {
                return backColor;
            }

            control = control.Parent;
        }

        return fallback;
    }

    private sealed class WarningBadgeControl : Control
    {
        private readonly bool _destructive;

        public WarningBadgeControl(bool destructive)
        {
            _destructive = destructive;
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint,
                true);
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            pevent.Graphics.Clear(ResolvePaintBackColor(Parent, DesignTokens.Surface));
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Rectangle bounds = Rectangle.Inflate(ClientRectangle, -1, -1);
            if (bounds.Width <= 0 || bounds.Height <= 0)
            {
                return;
            }

            Color accentColor = _destructive ? DesignTokens.AccentRed : DesignTokens.AccentOrange;
            using GraphicsPath path = CreateRoundedRectanglePath(bounds, DesignTokens.Scale(11));
            using var fillBrush = new SolidBrush(Color.FromArgb(28, accentColor));
            using var borderPen = new Pen(Color.FromArgb(190, accentColor), Math.Max(1f, DesignTokens.DensityScale));
            using var textBrush = new SolidBrush(accentColor);
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

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.FillPath(fillBrush, path);
            e.Graphics.DrawPath(borderPen, path);
            e.Graphics.DrawString("!", textFont, textBrush, bounds, format);
            e.Graphics.SmoothingMode = SmoothingMode.None;
        }
    }

    private sealed class RoundedDialogButton : Control, IButtonControl
    {
        private readonly bool _destructive;
        private bool _isHovered;
        private bool _isPressed;
        private bool _isDefault;

        public RoundedDialogButton(bool destructive)
        {
            _destructive = destructive;
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.Selectable |
                ControlStyles.UserPaint,
                true);

            Cursor = Cursors.Hand;
            TabStop = true;
            Font = DesignTokens.FontUiBold;
            ForeColor = DesignTokens.TextPrimary;
            AccessibleRole = AccessibleRole.PushButton;
        }

        public DialogResult DialogResult { get; set; }

        public void NotifyDefault(bool value)
        {
            if (_isDefault == value)
            {
                return;
            }

            _isDefault = value;
            Invalidate();
        }

        public void PerformClick()
        {
            if (CanSelect)
            {
                OnClick(EventArgs.Empty);
            }
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);

            if (DialogResult == DialogResult.None)
            {
                return;
            }

            Form? ownerForm = FindForm();
            if (ownerForm is null)
            {
                return;
            }

            ownerForm.DialogResult = DialogResult;
            ownerForm.Close();
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            _isHovered = true;
            Invalidate();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            _isHovered = false;
            _isPressed = false;
            Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && Enabled)
            {
                _isPressed = true;
                Focus();
                Invalidate();
            }

            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            _isPressed = false;
            Invalidate();
            base.OnMouseUp(e);
        }

        protected override void OnGotFocus(EventArgs e)
        {
            Invalidate();
            base.OnGotFocus(e);
        }

        protected override void OnLostFocus(EventArgs e)
        {
            Invalidate();
            base.OnLostFocus(e);
        }

        protected override bool IsInputKey(Keys keyData)
        {
            Keys keyCode = keyData & Keys.KeyCode;
            return keyCode is Keys.Space or Keys.Enter || base.IsInputKey(keyData);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (Enabled && e.KeyCode is Keys.Space or Keys.Enter)
            {
                PerformClick();
                e.Handled = true;
                return;
            }

            base.OnKeyDown(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.Clear(ResolvePaintBackColor(Parent, DesignTokens.Surface));

            Rectangle bounds = Rectangle.Inflate(ClientRectangle, -1, -1);
            if (bounds.Width <= 0 || bounds.Height <= 0)
            {
                return;
            }

            using GraphicsPath path = CreateRoundedRectanglePath(bounds, DesignTokens.Scale(8));
            using var fillBrush = new SolidBrush(ResolveFillColor());
            using var borderPen = new Pen(ResolveBorderColor(), Math.Max(1f, DesignTokens.DensityScale));
            e.Graphics.FillPath(fillBrush, path);
            e.Graphics.DrawPath(borderPen, path);

            TextRenderer.DrawText(
                e.Graphics,
                Text,
                Font,
                bounds,
                Enabled ? DesignTokens.TextPrimary : DesignTokens.TextMuted,
                TextFormatFlags.HorizontalCenter |
                TextFormatFlags.VerticalCenter |
                TextFormatFlags.EndEllipsis |
                TextFormatFlags.NoPrefix);
        }

        private Color ResolveFillColor()
        {
            if (_isPressed)
            {
                return _destructive
                    ? Color.FromArgb(108, DesignTokens.AccentRedSoft)
                    : DesignTokens.Surface3;
            }

            if (_isHovered || Focused || _isDefault)
            {
                return _destructive
                    ? Color.FromArgb(88, DesignTokens.AccentRedSoft)
                    : DesignTokens.SurfaceHover;
            }

            return _destructive
                ? DesignTokens.AccentRedSoft
                : DesignTokens.Surface2;
        }

        private Color ResolveBorderColor()
        {
            if (_destructive)
            {
                return _isHovered || Focused || _isDefault
                    ? DesignTokens.AccentRed
                    : Color.FromArgb(180, DesignTokens.AccentRed);
            }

            return _isHovered || Focused || _isDefault
                ? DesignTokens.BorderBright
                : DesignTokens.Border;
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
