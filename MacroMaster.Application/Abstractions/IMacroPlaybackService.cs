using MacroMaster.Domain.Models;

namespace MacroMaster.Application.Abstractions;

public interface IMacroPlaybackService
{
    bool IsPlaying { get; }
    bool IsPaused { get; }

    Task PlayAsync(
        MacroSession session,
        PlaybackSettings settings,
        CancellationToken cancellationToken = default);

    Task PauseAsync(CancellationToken cancellationToken = default);

    Task ResumeAsync(CancellationToken cancellationToken = default);

    Task StopAsync(CancellationToken cancellationToken = default);

    event Action? PlaybackStarted;
    event Action? PlaybackPaused;
    event Action? PlaybackResumed;
    event Action? PlaybackStopped;
    event Action<MacroEvent>? EventPlayed;
}