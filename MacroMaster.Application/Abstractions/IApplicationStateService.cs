using MacroMaster.Domain.Enums;

namespace MacroMaster.Application.Abstractions;

public interface IApplicationStateService
{
    AppState CurrentState { get; }

    bool Is(AppState state);

    void SetState(AppState state);

    event Action<AppState>? StateChanged;
}