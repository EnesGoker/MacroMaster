using MacroMaster.Application.Abstractions;

namespace MacroMaster.Application.Services;

public sealed class MutableHotkeyConfiguration : IMutableHotkeyConfiguration
{
    public MutableHotkeyConfiguration()
        : this(HotkeySettings.CreateDefault())
    {
    }

    public MutableHotkeyConfiguration(HotkeySettings hotkeySettings)
    {
        Apply(hotkeySettings);
    }

    public HotkeyBinding RecordToggleHotkey { get; private set; } = HotkeySettings.DefaultRecordToggleHotkey;

    public HotkeyBinding PlaybackToggleHotkey { get; private set; } = HotkeySettings.DefaultPlaybackToggleHotkey;

    public HotkeyBinding StopHotkey { get; private set; } = HotkeySettings.DefaultStopHotkey;

    public void Apply(HotkeySettings settings)
    {
        HotkeySettingsValidator.Validate(settings, "Hotkey configuration update");

        RecordToggleHotkey = settings.RecordToggleHotkey;
        PlaybackToggleHotkey = settings.PlaybackToggleHotkey;
        StopHotkey = settings.StopHotkey;
    }

    public HotkeySettings Snapshot()
    {
        return HotkeySettings.FromConfiguration(this);
    }
}
