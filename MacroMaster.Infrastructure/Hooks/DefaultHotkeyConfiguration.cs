using MacroMaster.Application.Abstractions;

namespace MacroMaster.Infrastructure.Hooks;

public sealed class DefaultHotkeyConfiguration : IHotkeyConfiguration
{
    public int RecordToggleVirtualKey => 0x77;   // F8
    public int PlaybackToggleVirtualKey => 0x78; // F9
    public int StopVirtualKey => 0x79;           // F10
}