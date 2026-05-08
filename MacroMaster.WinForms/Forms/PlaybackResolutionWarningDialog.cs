using MacroMaster.Domain.Models;
using MacroMaster.WinForms.Platform;
using MacroMaster.WinForms.Theme;

namespace MacroMaster.WinForms.Forms;

internal enum PlaybackResolutionWarningChoice
{
    Cancel,
    NormalPlayback,
    ScaledPlayback
}

internal sealed class PlaybackResolutionWarningDialog : Form
{
    private readonly ThemedDialogButton _scaledPlaybackButton;

    internal PlaybackResolutionWarningDialog(
        RecordedScreenInfo recordedScreen,
        RecordedScreenInfo currentScreen)
    {
        Text = "Çözünürlük Uyarısı";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        AutoScaleMode = AutoScaleMode.None;
        BackColor = DesignTokens.Surface;
        ForeColor = DesignTokens.TextPrimary;
        Font = DesignTokens.FontUiNormal;
        ClientSize = new Size(DesignTokens.Scale(520), DesignTokens.Scale(210));
        MinimumSize = Size;

        _scaledPlaybackButton = BuildLayout(recordedScreen, currentScreen);
    }

    public static PlaybackResolutionWarningChoice Show(
        IWin32Window owner,
        RecordedScreenInfo recordedScreen,
        RecordedScreenInfo currentScreen)
    {
        using var dialog = new PlaybackResolutionWarningDialog(recordedScreen, currentScreen);
        DialogResult dialogResult = ModalDialogOverlay.ShowDialog(owner, dialog);

        return dialogResult switch
        {
            DialogResult.Yes => PlaybackResolutionWarningChoice.ScaledPlayback,
            DialogResult.No => PlaybackResolutionWarningChoice.NormalPlayback,
            _ => PlaybackResolutionWarningChoice.Cancel
        };
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
        _scaledPlaybackButton.Focus();
    }

    private ThemedDialogButton BuildLayout(
        RecordedScreenInfo recordedScreen,
        RecordedScreenInfo currentScreen)
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
        rootLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, DesignTokens.Scale(34)));
        rootLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, DesignTokens.Scale(62)));
        rootLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, DesignTokens.Scale(28)));
        rootLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

        var headingLabel = new Label
        {
            Dock = DockStyle.Fill,
            Text = "Makro farklı çözünürlükte kaydedilmiş",
            Font = DesignTokens.FontUiBold,
            ForeColor = DesignTokens.TextPrimary,
            BackColor = Color.Transparent,
            TextAlign = ContentAlignment.MiddleLeft,
            AutoEllipsis = true,
            UseMnemonic = false
        };

        var messageLabel = new Label
        {
            Dock = DockStyle.Fill,
            Text = $"Kayıt çözünürlüğü {FormatScreen(recordedScreen)}, mevcut ekran {FormatScreen(currentScreen)}. Normal oynatma mouse koordinatlarını kaydırabilir.",
            Font = DesignTokens.FontUiNormal,
            ForeColor = DesignTokens.TextSecondary,
            BackColor = Color.Transparent,
            TextAlign = ContentAlignment.MiddleLeft,
            AutoEllipsis = true,
            UseMnemonic = false
        };

        var detailLabel = new Label
        {
            Dock = DockStyle.Fill,
            Text = "Ölçekli oynatma önerilir.",
            Font = DesignTokens.FontUiSmall,
            ForeColor = DesignTokens.Accent,
            BackColor = Color.Transparent,
            TextAlign = ContentAlignment.MiddleLeft,
            AutoEllipsis = true,
            UseMnemonic = false
        };

        var buttonFooterPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            BackColor = Color.Transparent,
            Margin = Padding.Empty,
            Padding = new Padding(0, DesignTokens.Scale(10), 0, 0)
        };

        ThemedDialogButton scaledButton = CreateDialogButton(
            "Ölçekli Oynat",
            DialogResult.Yes,
            ThemedDialogButtonStyle.Primary,
            width: 132);
        ThemedDialogButton normalButton = CreateDialogButton(
            "Normal Oynat",
            DialogResult.No,
            ThemedDialogButtonStyle.Secondary,
            width: 124);
        ThemedDialogButton cancelButton = CreateDialogButton(
            "İptal",
            DialogResult.Cancel,
            ThemedDialogButtonStyle.Secondary,
            width: 96);

        AcceptButton = scaledButton;
        CancelButton = cancelButton;

        buttonFooterPanel.Controls.Add(scaledButton);
        buttonFooterPanel.Controls.Add(normalButton);
        buttonFooterPanel.Controls.Add(cancelButton);

        rootLayoutPanel.Controls.Add(headingLabel, 0, 0);
        rootLayoutPanel.Controls.Add(messageLabel, 0, 1);
        rootLayoutPanel.Controls.Add(detailLabel, 0, 2);
        rootLayoutPanel.Controls.Add(buttonFooterPanel, 0, 3);

        Controls.Add(rootLayoutPanel);
        return scaledButton;
    }

    private static ThemedDialogButton CreateDialogButton(
        string text,
        DialogResult dialogResult,
        ThemedDialogButtonStyle style,
        int width)
    {
        return new ThemedDialogButton(style)
        {
            Text = text,
            DialogResult = dialogResult,
            Width = DesignTokens.Scale(width),
            Height = DesignTokens.Scale(34),
            Margin = new Padding(DesignTokens.Scale(8), 0, 0, 0)
        };
    }

    private static string FormatScreen(RecordedScreenInfo screen)
    {
        return $"{screen.Width}x{screen.Height}";
    }
}
