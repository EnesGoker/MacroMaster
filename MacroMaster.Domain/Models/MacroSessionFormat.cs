namespace MacroMaster.Domain.Models;

public static class MacroSessionFormat
{
    public const string CurrentVersion = "1.2";

    public static IReadOnlyCollection<string> SupportedVersions { get; } =
        ["1.0", "1.1", CurrentVersion];
}
