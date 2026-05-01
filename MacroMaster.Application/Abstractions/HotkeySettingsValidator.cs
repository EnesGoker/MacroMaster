namespace MacroMaster.Application.Abstractions;

public static class HotkeySettingsValidator
{
    private const HotkeyModifiers SupportedModifiers =
        HotkeyModifiers.Alt
        | HotkeyModifiers.Control
        | HotkeyModifiers.Shift
        | HotkeyModifiers.Windows;

    private static readonly HashSet<int> UnsupportedPrimaryVirtualKeySet =
    [
        0x01,
        0x02,
        0x04,
        0x05,
        0x06,
        0x10,
        0x11,
        0x12,
        0x5B,
        0x5C,
        0xA0,
        0xA1,
        0xA2,
        0xA3,
        0xA4,
        0xA5
    ];

    public static IReadOnlySet<int> UnsupportedPrimaryVirtualKeys => UnsupportedPrimaryVirtualKeySet;

    public static void Validate(
        HotkeySettings settings,
        string operationDescription)
    {
        ArgumentNullException.ThrowIfNull(settings);

        string context = string.IsNullOrWhiteSpace(operationDescription)
            ? "Kisayol ayari dogrulamasi"
            : operationDescription;

        ValidateBinding(settings.RecordToggleHotkey, "Kayit degistirme kisayolu", context);
        ValidateBinding(settings.PlaybackToggleHotkey, "Oynatma degistirme kisayolu", context);
        ValidateBinding(settings.StopHotkey, "Durdurma kisayolu", context);

        EnsureDistinct(
            settings.RecordToggleHotkey,
            "Kayit degistirme kisayolu",
            settings.PlaybackToggleHotkey,
            "Oynatma degistirme kisayolu",
            context);
        EnsureDistinct(
            settings.RecordToggleHotkey,
            "Kayit degistirme kisayolu",
            settings.StopHotkey,
            "Durdurma kisayolu",
            context);
        EnsureDistinct(
            settings.PlaybackToggleHotkey,
            "Oynatma degistirme kisayolu",
            settings.StopHotkey,
            "Durdurma kisayolu",
            context);
    }

    private static void ValidateBinding(
        HotkeyBinding? hotkeyBinding,
        string bindingName,
        string context)
    {
        if (hotkeyBinding is null)
        {
            throw CreateValidationException(
                context,
                $"{bindingName} zorunludur.");
        }

        if ((hotkeyBinding.Modifiers & ~SupportedModifiers) != 0)
        {
            throw CreateValidationException(
                context,
                $"{bindingName} desteklenmeyen degistirici bayraklari iceriyor: {hotkeyBinding.Modifiers}.");
        }

        if (hotkeyBinding.VirtualKeyCode <= 0 || hotkeyBinding.VirtualKeyCode > 0xFE)
        {
            throw CreateValidationException(
                context,
                $"{bindingName} gecerli bir sanal tus kodu kullanmalidir. Gecerli deger: {hotkeyBinding.VirtualKeyCode}.");
        }

        if (UnsupportedPrimaryVirtualKeySet.Contains(hotkeyBinding.VirtualKeyCode))
        {
            throw CreateValidationException(
                context,
                $"{bindingName} ana tus olarak fare dugmesi veya degistirici tus kullanamaz. Gecerli sanal tus: {hotkeyBinding.VirtualKeyCode}.");
        }
    }

    private static void EnsureDistinct(
        HotkeyBinding firstBinding,
        string firstName,
        HotkeyBinding secondBinding,
        string secondName,
        string context)
    {
        if (firstBinding == secondBinding)
        {
            throw CreateValidationException(
                context,
                $"{firstName} ve {secondName} ayni kisayolu kullanamaz ({Format(firstBinding)}).");
        }
    }

    private static InvalidOperationException CreateValidationException(
        string context,
        string message)
    {
        return new InvalidOperationException($"{context} basarisiz oldu. {message}");
    }

    private static string Format(HotkeyBinding hotkeyBinding)
    {
        List<string> parts = [];

        if (hotkeyBinding.Modifiers.HasFlag(HotkeyModifiers.Control))
        {
            parts.Add("Ctrl");
        }

        if (hotkeyBinding.Modifiers.HasFlag(HotkeyModifiers.Shift))
        {
            parts.Add("Shift");
        }

        if (hotkeyBinding.Modifiers.HasFlag(HotkeyModifiers.Alt))
        {
            parts.Add("Alt");
        }

        if (hotkeyBinding.Modifiers.HasFlag(HotkeyModifiers.Windows))
        {
            parts.Add("Win");
        }

        parts.Add(hotkeyBinding.VirtualKeyCode.ToString(System.Globalization.CultureInfo.InvariantCulture));
        return string.Join("+", parts);
    }
}
