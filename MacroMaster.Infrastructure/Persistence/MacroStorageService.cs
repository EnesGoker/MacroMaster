using MacroMaster.Application.Abstractions;
using MacroMaster.Domain.Models;

namespace MacroMaster.Infrastructure.Persistence;

public sealed class MacroStorageService : IMacroStorageService
{
    private readonly JsonMacroStorageService _jsonMacroStorageService;
    private readonly XmlMacroStorageService _xmlMacroStorageService;

    public MacroStorageService(
        JsonMacroStorageService jsonMacroStorageService,
        XmlMacroStorageService xmlMacroStorageService)
    {
        _jsonMacroStorageService = jsonMacroStorageService;
        _xmlMacroStorageService = xmlMacroStorageService;
    }

    public Task SaveAsJsonAsync(
        MacroSession session,
        string filePath,
        CancellationToken cancellationToken = default)
    {
        return _jsonMacroStorageService.SaveAsJsonAsync(
            session,
            filePath,
            cancellationToken);
    }

    public Task<MacroSession> LoadFromJsonAsync(
        string filePath,
        CancellationToken cancellationToken = default)
    {
        return _jsonMacroStorageService.LoadFromJsonAsync(
            filePath,
            cancellationToken);
    }

    public Task SaveAsXmlAsync(
        MacroSession session,
        string filePath,
        CancellationToken cancellationToken = default)
    {
        return _xmlMacroStorageService.SaveAsXmlAsync(
            session,
            filePath,
            cancellationToken);
    }

    public Task<MacroSession> LoadFromXmlAsync(
        string filePath,
        CancellationToken cancellationToken = default)
    {
        return _xmlMacroStorageService.LoadFromXmlAsync(
            filePath,
            cancellationToken);
    }
}