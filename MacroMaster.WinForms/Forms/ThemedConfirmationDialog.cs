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
        string subjectLabel,
        string subjectValue,
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
        ClientSize = new Size(DesignTokens.Scale(580), DesignTokens.Scale(318));
        MinimumSize = Size;

        _cancelButton = CreateDialogButton(cancelText, destructive: false);
        BuildLayout(
            heading,
            message,
            detail,
            subjectLabel,
            subjectValue,
            confirmText,
            destructive);
    }

    public static bool ConfirmMacroDelete(IWin32Window owner, string macroName)
    {
        string displayName = string.IsNullOrWhiteSpace(macroName)
            ? "Seçili makro"
            : macroName.Trim();

        using var dialog = new ThemedConfirmationDialog(
            "Makro Sil",
            $"{displayName} kütüphaneden silinsin mi?",
            "Kayıtlı makro dosyası kütüphaneden kaldırılacak.",
            "Bu işlem geri alınamaz.",
            "Silinecek makro",
            displayName,
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
        string heading,
        string message,
        string detail,
        string subjectLabel,
        string subjectValue,
        string confirmText,
        bool destructive)
    {
        var rootLayoutPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            BackColor = DesignTokens.Surface,
            Padding = new Padding(
                DesignTokens.Scale(24),
                DesignTokens.Scale(22),
                DesignTokens.Scale(24),
                DesignTokens.Scale(20)),
            Margin = Padding.Empty
        };
        rootLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, DesignTokens.Scale(76)));
        rootLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
        rootLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, DesignTokens.Scale(62)));

        var headerLayoutPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = Color.Transparent,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        headerLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, DesignTokens.Scale(64)));
        headerLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));

        var warningGlyph = new WarningGlyphControl(destructive)
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 0, DesignTokens.Scale(16), DesignTokens.Scale(8))
        };

        var headerTextLayoutPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            BackColor = Color.Transparent,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        headerTextLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, DesignTokens.Scale(22)));
        headerTextLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, DesignTokens.Scale(29)));
        headerTextLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

        Label eyebrowLabel = CreateLabel(
            destructive ? "Kalıcı işlem" : "Onay gerekli",
            DesignTokens.FontUiSmall,
            destructive ? DesignTokens.AccentRed : DesignTokens.Accent,
            ContentAlignment.MiddleLeft);
        Label titleLabel = CreateLabel(
            heading,
            DesignTokens.FontUiLarge,
            DesignTokens.TextPrimary,
            ContentAlignment.MiddleLeft);
        Label messageLabel = CreateLabel(
            message,
            DesignTokens.FontUiNormal,
            DesignTokens.TextSecondary,
            ContentAlignment.TopLeft);

        headerTextLayoutPanel.Controls.Add(eyebrowLabel, 0, 0);
        headerTextLayoutPanel.Controls.Add(titleLabel, 0, 1);
        headerTextLayoutPanel.Controls.Add(messageLabel, 0, 2);
        headerLayoutPanel.Controls.Add(warningGlyph, 0, 0);
        headerLayoutPanel.Controls.Add(headerTextLayoutPanel, 1, 0);

        var messagePanel = new RoundedPanel
        {
            Dock = DockStyle.Fill,
            FillColor = DesignTokens.SurfaceInset,
            BorderColor = Color.FromArgb(112, destructive ? DesignTokens.AccentRed : DesignTokens.Accent),
            AccentColor = destructive ? DesignTokens.AccentRed : DesignTokens.Accent,
            Margin = new Padding(0, DesignTokens.Scale(2), 0, DesignTokens.Scale(14)),
            Padding = new Padding(
                DesignTokens.Scale(22),
                DesignTokens.Scale(18),
                DesignTokens.Scale(22),
                DesignTokens.Scale(16))
        };

        var messageLayoutPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            BackColor = Color.Transparent,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        messageLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, DesignTokens.Scale(22)));
        messageLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, DesignTokens.Scale(34)));
        messageLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, DesignTokens.Scale(16)));
        messageLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

        Label subjectCaptionLabel = CreateLabel(
            subjectLabel,
            DesignTokens.FontUiSmall,
            DesignTokens.TextSecondary,
            ContentAlignment.MiddleLeft);
        Label subjectValueLabel = CreateLabel(
            subjectValue,
            DesignTokens.FontUiBold,
            DesignTokens.TextPrimary,
            ContentAlignment.MiddleLeft);
        var dividerLine = new DividerLineControl
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(0, DesignTokens.Scale(7), 0, DesignTokens.Scale(8))
        };
        Label detailLabel = CreateLabel(
            detail,
            DesignTokens.FontUiSmall,
            DesignTokens.TextSecondary,
            ContentAlignment.TopLeft);

        messageLayoutPanel.Controls.Add(subjectCaptionLabel, 0, 0);
        messageLayoutPanel.Controls.Add(subjectValueLabel, 0, 1);
        messageLayoutPanel.Controls.Add(dividerLine, 0, 2);
        messageLayoutPanel.Controls.Add(detailLabel, 0, 3);
        messagePanel.Controls.Add(messageLayoutPanel);

        var buttonLayoutPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 1,
            BackColor = Color.Transparent,
            Margin = Padding.Empty,
            Padding = new Padding(0, DesignTokens.Scale(14), 0, 0)
        };
        buttonLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        buttonLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, DesignTokens.Scale(134)));
        buttonLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, DesignTokens.Scale(134)));
        buttonLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

        var confirmButton = CreateDialogButton(confirmText, destructive);
        confirmButton.DialogResult = DialogResult.OK;
        _cancelButton.DialogResult = DialogResult.Cancel;

        confirmButton.Dock = DockStyle.Fill;
        _cancelButton.Dock = DockStyle.Fill;
        confirmButton.Margin = new Padding(DesignTokens.Scale(8), 0, 0, 0);
        _cancelButton.Margin = new Padding(DesignTokens.Scale(8), 0, 0, 0);

        AcceptButton = _cancelButton;
        CancelButton = _cancelButton;

        buttonLayoutPanel.Controls.Add(_cancelButton, 1, 0);
        buttonLayoutPanel.Controls.Add(confirmButton, 2, 0);

        rootLayoutPanel.Controls.Add(headerLayoutPanel, 0, 0);
        rootLayoutPanel.Controls.Add(messagePanel, 0, 1);
        rootLayoutPanel.Controls.Add(buttonLayoutPanel, 0, 2);

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
            Width = DesignTokens.Scale(126),
            Height = DesignTokens.Scale(40)
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

    private sealed class DividerLineControl : Control
    {
        public DividerLineControl()
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
            pevent.Graphics.Clear(ResolvePaintBackColor(Parent, DesignTokens.SurfaceInset));
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            int y = Math.Max(0, Height / 2);
            using var pen = new Pen(DesignTokens.BorderSoft);
            e.Graphics.DrawLine(pen, 0, y, Width, y);
        }
    }

    private sealed class WarningGlyphControl : Control
    {
        private readonly bool _destructive;

        public WarningGlyphControl(bool destructive)
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

            int size = Math.Min(DesignTokens.Scale(52), Math.Min(Width, Height) - DesignTokens.Scale(2));
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
            Color accentColor = _destructive ? DesignTokens.AccentRed : DesignTokens.AccentOrange;
            using GraphicsPath tilePath = CreateRoundedRectanglePath(bounds, DesignTokens.Scale(14));
            using var fillBrush = new SolidBrush(Color.FromArgb(34, accentColor));
            using var borderPen = new Pen(Color.FromArgb(174, accentColor), Math.Max(1f, DesignTokens.DensityScale));
            e.Graphics.FillPath(fillBrush, tilePath);
            e.Graphics.DrawPath(borderPen, tilePath);

            DrawTrashGlyph(e.Graphics, bounds, accentColor);
            e.Graphics.SmoothingMode = SmoothingMode.None;
        }

        private static void DrawTrashGlyph(Graphics graphics, Rectangle bounds, Color accentColor)
        {
            int stroke = Math.Max(2, DesignTokens.Scale(2));
            int centerX = bounds.Left + bounds.Width / 2;
            int lidY = bounds.Top + DesignTokens.Scale(17);
            int bodyTop = lidY + DesignTokens.Scale(5);
            int bodyHeight = DesignTokens.Scale(19);
            int bodyWidth = DesignTokens.Scale(21);

            Rectangle bodyBounds = new(
                centerX - bodyWidth / 2,
                bodyTop,
                bodyWidth,
                bodyHeight);

            using var glyphPen = new Pen(accentColor, stroke)
            {
                StartCap = LineCap.Round,
                EndCap = LineCap.Round,
                LineJoin = LineJoin.Round
            };

            graphics.DrawLine(
                glyphPen,
                centerX - DesignTokens.Scale(13),
                lidY,
                centerX + DesignTokens.Scale(13),
                lidY);
            graphics.DrawLine(
                glyphPen,
                centerX - DesignTokens.Scale(5),
                lidY - DesignTokens.Scale(5),
                centerX + DesignTokens.Scale(5),
                lidY - DesignTokens.Scale(5));
            graphics.DrawRectangle(glyphPen, bodyBounds);
            graphics.DrawLine(
                glyphPen,
                centerX - DesignTokens.Scale(5),
                bodyTop + DesignTokens.Scale(5),
                centerX - DesignTokens.Scale(5),
                bodyTop + bodyHeight - DesignTokens.Scale(5));
            graphics.DrawLine(
                glyphPen,
                centerX + DesignTokens.Scale(5),
                bodyTop + DesignTokens.Scale(5),
                centerX + DesignTokens.Scale(5),
                bodyTop + bodyHeight - DesignTokens.Scale(5));
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
            e.Graphics.Clear(ResolvePaintBackColor(Parent, DesignTokens.Surface));
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
