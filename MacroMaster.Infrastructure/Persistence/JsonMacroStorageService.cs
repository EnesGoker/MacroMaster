using System.Text.Json;
using MacroMaster.Application.Abstractions;
using MacroMaster.Domain.Models;

namespace MacroMaster.Infrastructure.Persistence;

public sealed class JsonMacroStorageService : IMacroStorageService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public async Task SaveAsJsonAsync(
        MacroSession session,
        string filePath,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(session);
        ValidateFilePath(filePath);

        EnsureDirectoryExists(filePath);

        await using FileStream stream = File.Create(filePath);

        await JsonSerializer.SerializeAsync(
            stream,
            session,
            JsonOptions,
            cancellationToken);
    }

    public async Task<MacroSession> LoadFromJsonAsync(
        string filePath,
        CancellationToken cancellationToken = default)
    {
        ValidateFilePath(filePath);
        EnsureFileExists(filePath);

        await using FileStream stream = File.OpenRead(filePath);

        MacroSession? session = await JsonSerializer.DeserializeAsync<MacroSession>(
            stream,
            JsonOptions,
            cancellationToken);

        return session ?? throw new InvalidOperationException(
            $"JSON dosyasından geçerli bir MacroSession okunamadı: {filePath}");
    }

    public Task SaveAsXmlAsync(
        MacroSession session,
        string filePath,
        CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException(
            $"{nameof(JsonMacroStorageService)} XML kaydetme işlemini desteklemez.");
    }

    public Task<MacroSession> LoadFromXmlAsync(
        string filePath,
        CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException(
            $"{nameof(JsonMacroStorageService)} XML yükleme işlemini desteklemez.");
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