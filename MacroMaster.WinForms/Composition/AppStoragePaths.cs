namespace MacroMaster.WinForms.Composition;

internal sealed class AppStoragePaths
{
    public string RootDirectoryPath { get; }

    public string LogDirectoryPath { get; }

    public string PlaybackSettingsFilePath { get; }

    public string HotkeySettingsFilePath { get; }

    private AppStoragePaths(string rootDirectoryPath)
    {
        RootDirectoryPath = rootDirectoryPath;
        LogDirectoryPath = Path.Combine(rootDirectoryPath, "logs");
        PlaybackSettingsFilePath = Path.Combine(rootDirectoryPath, "playback-settings.json");
        HotkeySettingsFilePath = Path.Combine(rootDirectoryPath, "hotkey-settings.json");
    }

    public static AppStoragePaths CreateDefault()
    {
        string rootDirectoryPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "MacroMaster");

        return FromRootDirectory(rootDirectoryPath);
    }

    public static AppStoragePaths FromRootDirectory(string rootDirectoryPath)
    {
        if (string.IsNullOrWhiteSpace(rootDirectoryPath))
        {
            throw new ArgumentException("Uygulama veri klasor yolu bos olamaz.", nameof(rootDirectoryPath));
        }

        return new AppStoragePaths(Path.GetFullPath(rootDirectoryPath));
    }
}
