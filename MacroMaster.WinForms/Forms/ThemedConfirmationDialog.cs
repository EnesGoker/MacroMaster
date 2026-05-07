using MacroMaster.WinForms.Platform;
using MacroMaster.WinForms.Theme;
using System.Drawing.Drawing2D;

namespace MacroMaster.WinForms.Forms;

internal sealed class ThemedConfirmationDialog : Form
{
    private readonly RoundedDialogButton _cancelButton;

    private ThemedConfirmationDialog(
        string title,
        string heading,
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
        ClientSize = new Size(DesignTokens.Scale(420), DesignTokens.Scale(188));
        MinimumSize = Size;

        _cancelButton = CreateDialogButton(cancelText, destructive: false);
        BuildLayout(heading, message, detail, confirmText, destructive);
    }

    public static bool ConfirmMacroDelete(IWin32Window owner, string macroName)
    {
        string displayName = string.IsNullOrWhiteSpace(macroName)
            ? "Seçili makro"
            : macroName.Trim();

        using var dialog = new ThemedConfirmationDialog(
            "Makro Sil",
            "Makro silinsin mi?",
            $"{displayName} kütüphaneden silinecek.",
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

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        _cancelButton.Focus();
    }

    private void BuildLayout(
        string heading,
        string message,
        string detail,
        string confirmText,
        bool destructive)
    {
        var rootLayoutPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            BackColor = DesignTokens.Surface,
            Padding = new Padding(DesignTokens.Scale(18)),
            Margin = Padding.Empty
        };
        rootLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, DesignTokens.Scale(36)));
        rootLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, DesignTokens.Scale(34)));
        rootLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, DesignTokens.Scale(28)));
        rootLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

        var titleLabel = new Label
        {
            Dock = DockStyle.Fill,
            Text = heading,
            Font = DesignTokens.FontUiBold,
            ForeColor = DesignTokens.TextPrimary,
            BackColor = Color.Transparent,
            TextAlign = ContentAlignment.MiddleLeft,
            AutoEllipsis = true
        };

        var messageLabel = new Label
        {
            Dock = DockStyle.Fill,
            Text = message,
            Font = DesignTokens.FontUiNormal,
            ForeColor = DesignTokens.TextSecondary,
            BackColor = Color.Transparent,
            TextAlign = ContentAlignment.MiddleLeft,
            AutoEllipsis = true
        };

        var detailLabel = new Label
        {
            Dock = DockStyle.Fill,
            Text = detail,
            Font = DesignTokens.FontUiSmall,
            ForeColor = destructive ? DesignTokens.AccentRed : DesignTokens.TextSecondary,
            BackColor = Color.Transparent,
            TextAlign = ContentAlignment.MiddleLeft,
            AutoEllipsis = true
        };

        var buttonLayoutPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            BackColor = Color.Transparent,
            Margin = Padding.Empty,
            Padding = new Padding(0, DesignTokens.Scale(10), 0, 0)
        };

        var confirmButton = CreateDialogButton(confirmText, destructive);
        confirmButton.DialogResult = DialogResult.OK;
        _cancelButton.DialogResult = DialogResult.Cancel;

        AcceptButton = _cancelButton;
        CancelButton = _cancelButton;

        buttonLayoutPanel.Controls.Add(confirmButton);
        buttonLayoutPanel.Controls.Add(_cancelButton);

        rootLayoutPanel.Controls.Add(titleLabel, 0, 0);
        rootLayoutPanel.Controls.Add(messageLabel, 0, 1);
        rootLayoutPanel.Controls.Add(detailLabel, 0, 2);
        rootLayoutPanel.Controls.Add(buttonLayoutPanel, 0, 3);

        Controls.Add(rootLayoutPanel);
    }

    private static RoundedDialogButton CreateDialogButton(string text, bool destructive)
    {
        return new RoundedDialogButton(destructive)
        {
            Text = text,
            Width = DesignTokens.Scale(112),
            Height = DesignTokens.Scale(34),
            Margin = new Padding(DesignTokens.Scale(8), 0, 0, 0)
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
                : DesignTokens.BorderBright;
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
