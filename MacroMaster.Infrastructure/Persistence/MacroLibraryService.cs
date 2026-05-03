using MacroMaster.Application.Abstractions;
using MacroMaster.Domain.Models;

namespace MacroMaster.Infrastructure.Persistence;

public sealed class MacroLibraryService : IMacroLibraryService
{
    private static readonly string[] SupportedSearchPatterns = ["*.json", "*.xml"];

    private readonly IMacroStorageService _macroStorageService;
    private readonly IAppLogger _logger;
    private readonly string _libraryDirectoryPath;

    public MacroLibraryService(
        IMacroStorageService macroStorageService,
        IAppLogger logger,
        string libraryDirectoryPath)
    {
        ArgumentNullException.ThrowIfNull(macroStorageService);
        ArgumentNullException.ThrowIfNull(logger);

        if (string.IsNullOrWhiteSpace(libraryDirectoryPath))
        {
            throw new ArgumentException("Makro kutuphane klasoru bos olamaz.", nameof(libraryDirectoryPath));
        }

        _macroStorageService = macroStorageService;
        _logger = logger;
        _libraryDirectoryPath = Path.GetFullPath(libraryDirectoryPath);
    }

    public async Task<IReadOnlyList<MacroLibraryEntry>> ListAsync(
        CancellationToken cancellationToken = default)
    {
        EnsureLibraryDirectoryExists();

        var entries = new List<MacroLibraryEntry>();

        foreach (string filePath in EnumerateMacroFiles())
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                MacroSession session = await LoadByExtensionAsync(filePath, cancellationToken);
                FileInfo fileInfo = new(filePath);

                entries.Add(
                    new MacroLibraryEntry(
                        string.IsNullOrWhiteSpace(session.Name)
                            ? Path.GetFileNameWithoutExtension(filePath)
                            : session.Name,
                        fileInfo.FullName,
                        fileInfo.LastWriteTimeUtc,
                        session.Events.Count,
                        ResolveFormat(fileInfo.FullName)));
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.Log(
                    AppLogLevel.Warning,
                    nameof(MacroLibraryService),
                    $"Makro kutuphanesindeki dosya okunamadi ve listeden atlandi: {filePath}",
                    ex);
            }
        }

        return entries
            .OrderByDescending(entry => entry.LastModifiedUtc)
            .ThenBy(entry => entry.Name, StringComparer.CurrentCultureIgnoreCase)
            .ToArray();
    }

    public Task<MacroSession> LoadAsync(
        string filePath,
        CancellationToken cancellationToken = default)
    {
        string resolvedFilePath = ResolveLibraryFilePath(filePath);
        return LoadByExtensionAsync(resolvedFilePath, cancellationToken);
    }

    public async Task<string> SaveAsync(
        MacroSession session,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(session);
        EnsureLibraryDirectoryExists();

        string filePath = Path.Combine(
            _libraryDirectoryPath,
            BuildSafeFileName(session.Name, ".json"));

        await _macroStorageService.SaveAsJsonAsync(session, filePath, cancellationToken);
        return filePath;
    }

    public async Task<string> RenameAsync(
        string filePath,
        string newName,
        CancellationToken cancellationToken = default)
    {
        string resolvedFilePath = ResolveLibraryFilePath(filePath);
        string normalizedName = NormalizeSessionName(newName);
        MacroLibraryFileFormat format = ResolveFormat(resolvedFilePath);
        string extension = Path.GetExtension(resolvedFilePath);
        string targetFilePath = Path.Combine(
            _libraryDirectoryPath,
            BuildSafeFileName(normalizedName, extension));

        if (!IsSamePath(resolvedFilePath, targetFilePath) && File.Exists(targetFilePath))
        {
            throw new InvalidOperationException(
                $"Bu isimde bir makro zaten var: {Path.GetFileName(targetFilePath)}");
        }

        MacroSession session = await LoadByExtensionAsync(resolvedFilePath, cancellationToken);
        session.Name = normalizedName;

        await SaveByFormatAsync(session, targetFilePath, format, cancellationToken);

        if (!IsSamePath(resolvedFilePath, targetFilePath))
        {
            File.Delete(resolvedFilePath);
        }

        return targetFilePath;
    }

    public Task DeleteAsync(
        string filePath,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        string resolvedFilePath = ResolveLibraryFilePath(filePath);

        if (File.Exists(resolvedFilePath))
        {
            File.Delete(resolvedFilePath);
        }

        return Task.CompletedTask;
    }

    private IEnumerable<string> EnumerateMacroFiles()
    {
        foreach (string searchPattern in SupportedSearchPatterns)
        {
            foreach (string filePath in Directory.EnumerateFiles(
                         _libraryDirectoryPath,
                         searchPattern,
                         SearchOption.TopDirectoryOnly))
            {
                yield return filePath;
            }
        }
    }

    private async Task<MacroSession> LoadByExtensionAsync(
        string filePath,
        CancellationToken cancellationToken)
    {
        return ResolveFormat(filePath) switch
        {
            MacroLibraryFileFormat.Json => await _macroStorageService.LoadFromJsonAsync(filePath, cancellationToken),
            MacroLibraryFileFormat.Xml => await _macroStorageService.LoadFromXmlAsync(filePath, cancellationToken),
            _ => throw new NotSupportedException($"Desteklenmeyen makro dosya formati: {filePath}")
        };
    }

    private async Task SaveByFormatAsync(
        MacroSession session,
        string filePath,
        MacroLibraryFileFormat format,
        CancellationToken cancellationToken)
    {
        switch (format)
        {
            case MacroLibraryFileFormat.Json:
                await _macroStorageService.SaveAsJsonAsync(session, filePath, cancellationToken);
                break;
            case MacroLibraryFileFormat.Xml:
                await _macroStorageService.SaveAsXmlAsync(session, filePath, cancellationToken);
                break;
            default:
                throw new NotSupportedException($"Desteklenmeyen makro dosya formati: {filePath}");
        }
    }

    private string ResolveLibraryFilePath(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("Makro dosya yolu bos olamaz.", nameof(filePath));
        }

        string resolvedFilePath = Path.GetFullPath(filePath);
        string libraryRoot = _libraryDirectoryPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            + Path.DirectorySeparatorChar;

        if (!resolvedFilePath.StartsWith(libraryRoot, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Makro kutuphanesi disindaki dosyalar bu islem icin kullanilamaz: {resolvedFilePath}");
        }

        return resolvedFilePath;
    }

    private static MacroLibraryFileFormat ResolveFormat(string filePath)
    {
        string extension = Path.GetExtension(filePath);

        if (extension.Equals(".json", StringComparison.OrdinalIgnoreCase))
        {
            return MacroLibraryFileFormat.Json;
        }

        if (extension.Equals(".xml", StringComparison.OrdinalIgnoreCase))
        {
            return MacroLibraryFileFormat.Xml;
        }

        throw new NotSupportedException($"Desteklenmeyen makro dosya uzantisi: {extension}");
    }

    private void EnsureLibraryDirectoryExists()
    {
        Directory.CreateDirectory(_libraryDirectoryPath);
    }

    private static string BuildSafeFileName(string sessionName, string extension)
    {
        char[] invalidCharacters = Path.GetInvalidFileNameChars();
        string sanitizedName = new(
            sessionName
                .Where(character => !invalidCharacters.Contains(character))
                .ToArray());

        if (string.IsNullOrWhiteSpace(sanitizedName))
        {
            sanitizedName = "MakroOturumu";
        }

        return sanitizedName + extension;
    }

    private static string NormalizeSessionName(string sessionName)
    {
        if (string.IsNullOrWhiteSpace(sessionName))
        {
            throw new ArgumentException("Makro adi bos olamaz.", nameof(sessionName));
        }

        string normalizedName = sessionName.Trim();

        if (normalizedName.EndsWith(".json", StringComparison.OrdinalIgnoreCase)
            || normalizedName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
        {
            normalizedName = Path.GetFileNameWithoutExtension(normalizedName);
        }

        char[] invalidCharacters = Path.GetInvalidFileNameChars();
        normalizedName = new string(
            normalizedName
                .Where(character => !invalidCharacters.Contains(character))
                .ToArray());

        normalizedName = normalizedName.Trim();

        if (string.IsNullOrWhiteSpace(normalizedName))
        {
            throw new ArgumentException("Makro adi gecerli bir dosya adi olmalidir.", nameof(sessionName));
        }

        return normalizedName;
    }

    private static bool IsSamePath(string left, string right)
    {
        return string.Equals(
            Path.GetFullPath(left),
            Path.GetFullPath(right),
            StringComparison.OrdinalIgnoreCase);
    }
}
