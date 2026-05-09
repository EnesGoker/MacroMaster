using MacroMaster.Application.Abstractions;
using MacroMaster.Domain.Models;

namespace MacroMaster.WinForms.Platform;

internal sealed class WindowsRecordedScreenProvider : IRecordedScreenProvider
{
    public RecordedScreenInfo? GetRecordedScreen()
    {
        Rectangle virtualScreenBounds = SystemInformation.VirtualScreen;

        if (virtualScreenBounds.Width <= 0 || virtualScreenBounds.Height <= 0)
        {
            return null;
        }

        return new RecordedScreenInfo
        {
            Width = virtualScreenBounds.Width,
            Height = virtualScreenBounds.Height
        };
    }
}
