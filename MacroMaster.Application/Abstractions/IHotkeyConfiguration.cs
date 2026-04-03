namespace MacroMaster.Application.Abstractions;

public interface IHotkeyConfiguration
{
    int RecordToggleVirtualKey { get; }
    int PlaybackToggleVirtualKey { get; }
    int StopVirtualKey { get; }
}