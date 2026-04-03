using MacroMaster.Application.Abstractions;

namespace MacroMaster.Infrastructure.Hooks;

public sealed class NullHotkeyService : IHotkeyService
{
    public bool IsRegistered { get; private set; }

    public event Action? RecordToggleRequested;
    public event Action? PlaybackToggleRequested;
    public event Action? StopRequested;

    public Task RegisterAsync(CancellationToken cancellationToken = default)
    {
        IsRegistered = true;
        return Task.CompletedTask;
    }

    public Task UnregisterAsync(CancellationToken cancellationToken = default)
    {
        IsRegistered = false;
        return Task.CompletedTask;
    }
}