using System.Xml.Serialization;
using MacroMaster.Application.Abstractions;
using MacroMaster.Domain.Models;

namespace MacroMaster.Infrastructure.Persistence;

public sealed class XmlMacroStorageService : IMacroStorageService
{
    private static readonly XmlSerializer Serializer = new(typeof(MacroSession));

    public Task SaveAsJsonAsync(
        MacroSession session,
        string filePath,
        CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException(
            $"{nameof(XmlMacroStorageService)} JSON kaydetme işlemini desteklemez.");
    }

    public Task<MacroSession> LoadFromJsonAsync(
        string filePath,
        CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException(
            $"{nameof(XmlMacroStorageService)} JSON yükleme işlemini desteklemez.");
    }

    public async Task SaveAsXmlAsync(
        MacroSession session,
        string filePath,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(session);
        ValidateFilePath(filePath);

        EnsureDirectoryExists(filePath);

        await using FileStream stream = File.Create(filePath);

        Serializer.Serialize(stream, session);

        await stream.FlushAsync(cancellationToken);
    }

    public async Task<MacroSession> LoadFromXmlAsync(
        string filePath,
        CancellationToken cancellationToken = default)
    {
        ValidateFilePath(filePath);
        EnsureFileExists(filePath);

        await using FileStream stream = File.OpenRead(filePath);

        object? result = Serializer.Deserialize(stream);

        await Task.CompletedTask;

        return result as MacroSession
               ?? throw new InvalidOperationException(
                   $"XML dosyasından geçerli bir MacroSession okunamadı: {filePath}");
    }

    private static void ValidateFilePath(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("Dosya yolu boş olamaz.", nameof(filePath));
        }
    }

    private static void EnsureDirectoryExists(string filePath)
    {
        string? directory = Path.GetDirectoryName(filePath);

        if (string.IsNullOrWhiteSpace(directory))
        {
            return;
        }

        Directory.CreateDirectory(directory);
    }

    private static void EnsureFileExists(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException(
                $"Dosya bulunamadı: {filePath}",
                filePath);
        }
    }
}