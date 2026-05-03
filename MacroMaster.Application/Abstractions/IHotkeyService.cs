using MacroMaster.Domain.Models;

namespace MacroMaster.Application.Abstractions;

public interface IHotkeyService
{
    bool IsRegistered { get; }

    Task RegisterAsync(CancellationToken cancellationToken = default);

    Task UnregisterAsync(CancellationToken cancellationToken = default);

    event Action? RecordToggleRequested;
    event Action? PlaybackToggleRequested;
    event Action? StopRequested;
    event Action? HotkeySettingsRequested;
}
