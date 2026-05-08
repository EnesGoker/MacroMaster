using MacroMaster.Domain.Enums;
using MacroMaster.Domain.Models;

namespace MacroMaster.WinForms.Forms;

internal static class PlaybackResolutionWarningPolicy
{
    public static bool ShouldWarn(
        MacroSession? session,
        PlaybackSettings settings,
        RecordedScreenInfo? currentScreen)
    {
        if (!ShouldInspectCurrentScreen(session, settings)
            || currentScreen is null
            || !HasValidScreen(currentScreen)
            || session!.RecordedScreen is null)
        {
            return false;
        }

        return session.RecordedScreen.Width != currentScreen.Width
            || session.RecordedScreen.Height != currentScreen.Height;
    }

    public static bool ShouldInspectCurrentScreen(
        MacroSession? session,
        PlaybackSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        return session is not null
            && !settings.SimulationMode
            && !settings.UseRelativeCoordinates
            && !settings.UseScreenScaledCoordinates
            && session.RecordedScreen is not null
            && HasValidScreen(session.RecordedScreen)
            && HasMouseCoordinates(session);
    }

    private static bool HasValidScreen(RecordedScreenInfo screen)
    {
        return screen.Width > 0 && screen.Height > 0;
    }

    private static bool HasMouseCoordinates(MacroSession session)
    {
        foreach (MacroEvent macroEvent in session.Events)
        {
            if (macroEvent.EventType == MacroEventType.Mouse
                && macroEvent.X.HasValue
                && macroEvent.Y.HasValue)
            {
                return true;
            }
        }

        return false;
    }
}
