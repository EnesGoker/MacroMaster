using MacroMaster.Application.Abstractions;

namespace MacroMaster.Infrastructure.Hooks;

public sealed class DefaultHotkeyConfiguration : IMutableHotkeyConfiguration
{
    public int RecordToggleVirtualKey { get; set; } = 0x77;   // F8
    public int PlaybackToggleVirtualKey { get; set; } = 0x78; // F9
    public int StopVirtualKey { get; set; } = 0x79;           // F10
}
