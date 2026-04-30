namespace MacroMaster.Application.Abstractions;

public interface IMutableHotkeyConfiguration : IHotkeyConfiguration
{
    new int RecordToggleVirtualKey { get; set; }

    new int PlaybackToggleVirtualKey { get; set; }

    new int StopVirtualKey { get; set; }
}
