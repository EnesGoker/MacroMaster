namespace MacroMaster.Application.Abstractions;

public sealed class HotkeySettings
{
    public static HotkeyBinding DefaultRecordToggleHotkey { get; } = HotkeyBinding.None(0x77);     // F8
    public static HotkeyBinding DefaultPlaybackToggleHotkey { get; } = HotkeyBinding.None(0x78);   // F9
    public static HotkeyBinding DefaultStopHotkey { get; } = HotkeyBinding.None(0x79);             // F10

    public HotkeyBinding RecordToggleHotkey { get; set; } = DefaultRecordToggleHotkey;

    public HotkeyBinding PlaybackToggleHotkey { get; set; } = DefaultPlaybackToggleHotkey;

    public HotkeyBinding StopHotkey { get; set; } = DefaultStopHotkey;

    public static HotkeySettings CreateDefault()
    {
        return new HotkeySettings
        {
            RecordToggleHotkey = DefaultRecordToggleHotkey,
            PlaybackToggleHotkey = DefaultPlaybackToggleHotkey,
            StopHotkey = DefaultStopHotkey
        };
    }

    public static HotkeySettings FromConfiguration(IHotkeyConfiguration hotkeyConfiguration)
    {
        ArgumentNullException.ThrowIfNull(hotkeyConfiguration);

        return new HotkeySettings
        {
            RecordToggleHotkey = hotkeyConfiguration.RecordToggleHotkey,
            PlaybackToggleHotkey = hotkeyConfiguration.PlaybackToggleHotkey,
            StopHotkey = hotkeyConfiguration.StopHotkey
        };
    }

    public HotkeySettings Clone()
    {
        return new HotkeySettings
        {
            RecordToggleHotkey = RecordToggleHotkey,
            PlaybackToggleHotkey = PlaybackToggleHotkey,
            StopHotkey = StopHotkey
        };
    }
}
