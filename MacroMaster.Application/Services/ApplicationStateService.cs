using MacroMaster.Application.Abstractions;
using MacroMaster.Domain.Enums;

namespace MacroMaster.Application.Services;

public sealed class ApplicationStateService : IApplicationStateService
{
    private AppState _currentState = AppState.Idle;

    public AppState CurrentState => _currentState;

    public event Action<AppState>? StateChanged;

    public bool Is(AppState state) => _currentState == state;

    public void SetState(AppState state)
    {
        if (_currentState == state)
        {
            return;
        }

        _currentState = state;
        StateChanged?.Invoke(_currentState);
    }
}