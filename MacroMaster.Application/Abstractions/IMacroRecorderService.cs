using MacroMaster.Domain.Models;

namespace MacroMaster.Application.Abstractions;

public interface IMacroRecorderService
{
    bool IsRecording { get; }

    MacroSession? CurrentSession { get; }

    Task StartAsync(
        string? sessionName = null,
        CancellationToken cancellationToken = default);

    Task StopAsync(CancellationToken cancellationToken = default);

    void Clear();

    event Action? RecordingStarted;
    event Action<MacroEvent>? EventRecorded;
    event Action<MacroSession>? RecordingStopped;
}