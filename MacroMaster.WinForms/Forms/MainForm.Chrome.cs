using MacroMaster.WinForms.Platform;
using MacroMaster.WinForms.Theme;

namespace MacroMaster.WinForms.Forms;

public partial class MainForm
{
    private static bool IsCustomChromeEnabled => false;

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);

        _titleBarControl.SetMaximized(WindowState == FormWindowState.Maximized);

        if (IsCustomChromeEnabled)
        {
            ApplyCustomChromePadding();
        }
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);

        if (IsCustomChromeEnabled)
        {
            ApplyCustomChromeDwmPolish();
        }
    }

    protected override void WndProc(ref Message m)
    {
        if (!IsCustomChromeEnabled)
        {
            base.WndProc(ref m);
            return;
        }

        if (m.Msg == WindowChromeNative.WmNcHitTest)
        {
            m.Result = (IntPtr)ResolveChromeHitTest(WindowChromeNative.GetPointFromLParam(m.LParam));
            return;
        }

        base.WndProc(ref m);
    }

    private void ApplyCustomChromeDwmPolish()
    {
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
            DesignTokens.Background);
        WindowChromeNative.TryApplyDwmColorAttribute(
            Handle,
            DwmWindowAttribute.TextColor,
            DesignTokens.TextPrimary);
    }

    private void ApplyCustomChromePadding()
    {
        Padding = WindowState == FormWindowState.Maximized
            ? new Padding(DesignTokens.WindowMaximizedPadding)
            : Padding.Empty;
    }

    private WindowHitTest ResolveChromeHitTest(Point screenPoint)
    {
        if (WindowState != FormWindowState.Maximized)
        {
            WindowHitTest resizeHitTest = WindowChromeNative.GetResizeHitTest(
                Bounds,
                screenPoint,
                DesignTokens.WindowResizeBorder);

            if (resizeHitTest != WindowHitTest.Client)
            {
                return resizeHitTest;
            }
        }

        Point clientPoint = PointToClient(screenPoint);
        if (_titleBarControl.Bounds.Contains(clientPoint))
        {
            return WindowHitTest.Caption;
        }

        return WindowHitTest.Client;
    }
}
