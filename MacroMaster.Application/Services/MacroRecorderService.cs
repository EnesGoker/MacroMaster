using MacroMaster.Application.Abstractions;
using MacroMaster.Domain.Enums;
using MacroMaster.Domain.Models;

namespace MacroMaster.Application.Services;

public sealed class MacroRecorderService : IMacroRecorderService
{
    private readonly IKeyboardHookSource _keyboardHookSource;
    private readonly IMouseHookSource _mouseHookSource;
    private readonly IApplicationStateService _applicationStateService;

    private readonly List<MacroEvent> _recordedEvents = new();
    private DateTime _lastEventTimestampUtc;

    public MacroRecorderService(
        IKeyboardHookSource keyboardHookSource,
        IMouseHookSource mouseHookSource,
        IApplicationStateService applicationStateService)
    {
        _keyboardHookSource = keyboardHookSource;
        _mouseHookSource = mouseHookSource;
        _applicationStateService = applicationStateService;
    }

    public bool IsRecording => _applicationStateService.Is(AppState.Recording);

    public MacroSession? CurrentSession { get; private set; }

    public event Action? RecordingStarted;
    public event Action<MacroEvent>? EventRecorded;
    public event Action<MacroSession>? RecordingStopped;

    public async Task StartAsync(
        string? sessionName = null,
        CancellationToken cancellationToken = default)
    {
        if (IsRecording)
        {
            return;
        }

        CurrentSession = new MacroSession
        {
            Name = string.IsNullOrWhiteSpace(sessionName)
                ? $"Macro_{DateTime.UtcNow:yyyyMMdd_HHmmss}"
                : sessionName.Trim()
        };

        _recordedEvents.Clear();
        _lastEventTimestampUtc = DateTime.UtcNow;

        _keyboardHookSource.KeyActivityReceived += OnKeyboardActivityReceived;
        _mouseHookSource.MouseActivityReceived += OnMouseActivityReceived;

        await _keyboardHookSource.StartAsync(cancellationToken);
        await _mouseHookSource.StartAsync(cancellationToken);

        _applicationStateService.SetState(AppState.Recording);
        RecordingStarted?.Invoke();
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        if (!IsRecording || CurrentSession is null)
        {
            return;
        }

        _applicationStateService.SetState(AppState.Stopping);

        _keyboardHookSource.KeyActivityReceived -= OnKeyboardActivityReceived;
        _mouseHookSource.MouseActivityReceived -= OnMouseActivityReceived;

        await _keyboardHookSource.StopAsync(cancellationToken);
        await _mouseHookSource.StopAsync(cancellationToken);

        CurrentSession.Events = _recordedEvents.ToList();

        _applicationStateService.SetState(AppState.Idle);
        RecordingStopped?.Invoke(CurrentSession);
    }

    public void Clear()
    {
        _recordedEvents.Clear();

        if (CurrentSession is not null)
        {
            CurrentSession.Events.Clear();
        }
    }

    private void OnKeyboardActivityReceived(int keyCode, bool isKeyDown)
    {
        if (!IsRecording)
        {
            return;
        }

        var nowUtc = DateTime.UtcNow;

        var macroEvent = new MacroEvent
        {
            EventType = MacroEventType.Keyboard,
            KeyboardActionType = isKeyDown
                ? KeyboardActionType.KeyDown
                : KeyboardActionType.KeyUp,
            KeyCode = keyCode,
            KeyName = keyCode.ToString(),
            TimestampUtc = nowUtc,
            DelayMs = CalculateDelayMs(nowUtc),
            Description = $"{(isKeyDown ? "KeyDown" : "KeyUp")} - {keyCode}"
        };

        _recordedEvents.Add(macroEvent);
        EventRecorded?.Invoke(macroEvent);
    }

    private void OnMouseActivityReceived(
        MouseActionType mouseActionType,
        int? x,
        int? y,
        int? wheelDelta)
    {
        if (!IsRecording)
        {
            return;
        }

        var nowUtc = DateTime.UtcNow;

        var macroEvent = new MacroEvent
        {
            EventType = MacroEventType.Mouse,
            MouseActionType = mouseActionType,
            X = x,
            Y = y,
            WheelDelta = wheelDelta,
            TimestampUtc = nowUtc,
            DelayMs = CalculateDelayMs(nowUtc),
            Description = mouseActionType.ToString()
        };

        _recordedEvents.Add(macroEvent);
        EventRecorded?.Invoke(macroEvent);
    }

    private int CalculateDelayMs(DateTime currentTimestampUtc)
    {
        var delay = (int)(currentTimestampUtc - _lastEventTimestampUtc).TotalMilliseconds;
        _lastEventTimestampUtc = currentTimestampUtc;

        return Math.Max(delay, 0);
    }
}