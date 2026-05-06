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
        ClientSize = new Size(DesignTokens.Scale(486), DesignTokens.Scale(218));
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
            RowCount = 2,
            BackColor = DesignTokens.Surface,
            Padding = new Padding(DesignTokens.Scale(20), DesignTokens.Scale(18), DesignTokens.Scale(20), DesignTokens.Scale(18)),
            Margin = Padding.Empty
        };
        rootLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
        rootLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, DesignTokens.Scale(52)));

        var messagePanel = new RoundedPanel
        {
            Dock = DockStyle.Fill,
            FillColor = DesignTokens.SurfaceInset,
            BorderColor = Color.FromArgb(92, DesignTokens.AccentRed),
            AccentColor = destructive ? DesignTokens.AccentRed : DesignTokens.Accent,
            Margin = new Padding(0, 0, 0, DesignTokens.Scale(12)),
            Padding = new Padding(
                DesignTokens.Scale(18),
                DesignTokens.Scale(16),
                DesignTokens.Scale(18),
                DesignTokens.Scale(16))
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
        messageLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, DesignTokens.Scale(64)));
        messageLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));

        var warningGlyph = new WarningGlyphControl
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 0, DesignTokens.Scale(16), 0)
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
        textLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, DesignTokens.Scale(28)));
        textLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
        textLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, DesignTokens.Scale(30)));

        var eyebrowLabel = new Label
        {
            Dock = DockStyle.Fill,
            Text = "Kalici islem",
            Font = DesignTokens.FontUiSmall,
            ForeColor = destructive ? DesignTokens.AccentRed : DesignTokens.Accent,
            BackColor = Color.Transparent,
            TextAlign = ContentAlignment.TopLeft,
            UseMnemonic = false,
            AutoEllipsis = true
        };

        var messageLabel = new Label
        {
            Dock = DockStyle.Fill,
            Text = message,
            Font = DesignTokens.FontUiBold,
            ForeColor = DesignTokens.TextPrimary,
            BackColor = Color.Transparent,
            TextAlign = ContentAlignment.MiddleLeft,
            UseMnemonic = false,
            AutoEllipsis = true
        };

        var detailLabel = new Label
        {
            Dock = DockStyle.Fill,
            Text = detail,
            Font = DesignTokens.FontUiSmall,
            ForeColor = DesignTokens.TextSecondary,
            BackColor = Color.Transparent,
            TextAlign = ContentAlignment.BottomLeft,
            UseMnemonic = false,
            AutoEllipsis = true
        };

        textLayoutPanel.Controls.Add(eyebrowLabel, 0, 0);
        textLayoutPanel.Controls.Add(messageLabel, 0, 1);
        textLayoutPanel.Controls.Add(detailLabel, 0, 2);
        messageLayoutPanel.Controls.Add(warningGlyph, 0, 0);
        messageLayoutPanel.Controls.Add(textLayoutPanel, 1, 0);
        messagePanel.Controls.Add(messageLayoutPanel);

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

        buttonLayoutPanel.Controls.Add(_cancelButton);
        buttonLayoutPanel.Controls.Add(confirmButton);

        rootLayoutPanel.Controls.Add(messagePanel, 0, 0);
        rootLayoutPanel.Controls.Add(buttonLayoutPanel, 0, 1);

        Controls.Add(rootLayoutPanel);
    }

    private static RoundedDialogButton CreateDialogButton(string text, bool destructive)
    {
        return new RoundedDialogButton(destructive)
        {
            Text = text,
            Width = DesignTokens.Scale(118),
            Height = DesignTokens.Scale(36),
            Margin = new Padding(DesignTokens.Scale(8), 0, 0, 0)
        };
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

            int size = Math.Min(DesignTokens.Scale(42), Math.Min(Width, Height) - DesignTokens.Scale(4));
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
            using var fillBrush = new SolidBrush(Color.FromArgb(28, DesignTokens.AccentOrange));
            using var borderPen = new Pen(DesignTokens.AccentOrange, Math.Max(1f, DesignTokens.DensityScale));
            using var textBrush = new SolidBrush(DesignTokens.AccentOrange);
            using var textFont = new Font(
                "Segoe UI",
                Math.Max(12f, DesignTokens.ScaleFont(18f)),
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

            using var accentPen = new Pen(Color.FromArgb(170, AccentColor), Math.Max(2f, DesignTokens.DensityScale * 2f));
            int accentX = bounds.Left + DesignTokens.Scale(1);
            e.Graphics.DrawLine(
                accentPen,
                accentX,
                bounds.Top + DesignTokens.Scale(14),
                accentX,
                bounds.Bottom - DesignTokens.Scale(14));
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
            e.Graphics.Clear(Parent?.BackColor ?? DesignTokens.Surface);

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
                ResolveTextColor(),
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
                    ? Color.FromArgb(112, DesignTokens.AccentRedSoft)
                    : DesignTokens.Surface3;
            }

            if (_isHovered || Focused || _isDefault)
            {
                return _destructive
                    ? Color.FromArgb(92, DesignTokens.AccentRedSoft)
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

        private Color ResolveTextColor()
        {
            if (!Enabled)
            {
                return DesignTokens.TextMuted;
            }

            return _destructive
                ? DesignTokens.TextPrimary
                : DesignTokens.TextPrimary;
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
