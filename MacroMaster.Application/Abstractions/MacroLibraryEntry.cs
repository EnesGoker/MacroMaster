using MacroMaster.Domain.Models;

namespace MacroMaster.Application.Abstractions;

public sealed record MacroLibraryEntry(
    string Name,
    string FilePath,
    DateTime LastModifiedUtc,
    int EventCount,
    MacroLibraryFileFormat Format);

public enum MacroLibraryFileFormat
{
    Json = 0,
    Xml = 1
}

public interface IMacroLibraryService
{
    Task<IReadOnlyList<MacroLibraryEntry>> ListAsync(CancellationToken cancellationToken = default);

    Task<MacroSession> LoadAsync(
        string filePath,
        CancellationToken cancellationToken = default);

    Task<string> SaveAsync(
        MacroSession session,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        string filePath,
        CancellationToken cancellationToken = default);
}
