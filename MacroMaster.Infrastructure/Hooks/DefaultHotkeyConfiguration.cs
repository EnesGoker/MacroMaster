using MacroMaster.Application.Abstractions;

namespace MacroMaster.Infrastructure.Hooks;

public sealed class DefaultHotkeyConfiguration : IHotkeyConfiguration
{
    public DefaultHotkeyConfiguration(
        HotkeyBinding? recordToggle = null,
        HotkeyBinding? playbackToggle = null,
        HotkeyBinding? stop = null)
    {
        RecordToggleHotkey = recordToggle ?? HotkeySettings.DefaultRecordToggleHotkey;
        PlaybackToggleHotkey = playbackToggle ?? HotkeySettings.DefaultPlaybackToggleHotkey;
        StopHotkey = stop ?? HotkeySettings.DefaultStopHotkey;
    }

    public HotkeyBinding RecordToggleHotkey { get; }

    public HotkeyBinding PlaybackToggleHotkey { get; }

    public HotkeyBinding StopHotkey { get; }
}
