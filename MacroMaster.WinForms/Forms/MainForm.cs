using MacroMaster.Application.Abstractions;
using MacroMaster.Domain.Models;

namespace MacroMaster.WinForms.Forms;

public partial class MainForm : Form
{
    private readonly IApplicationStateService _applicationStateService;
    private readonly IMacroRecorderService _macroRecorderService;
    private readonly IMacroPlaybackService _macroPlaybackService;
    private readonly IMacroStorageService _macroStorageService;
    private readonly IHotkeyService _hotkeyService;

    public MainForm(
        IApplicationStateService applicationStateService,
        IMacroRecorderService macroRecorderService,
        IMacroPlaybackService macroPlaybackService,
        IMacroStorageService macroStorageService,
        IHotkeyService hotkeyService)
    {
        _applicationStateService = applicationStateService;
        _macroRecorderService = macroRecorderService;
        _macroPlaybackService = macroPlaybackService;
        _macroStorageService = macroStorageService;
        _hotkeyService = hotkeyService;

        InitializeComponent();

        Load += MainForm_Load;
        FormClosed += MainForm_FormClosed;
    }

    private async void MainForm_Load(object? sender, EventArgs e)
    {
        _hotkeyService.RecordToggleRequested += OnRecordToggleRequested;
        _hotkeyService.PlaybackToggleRequested += OnPlaybackToggleRequested;
        _hotkeyService.StopRequested += OnStopRequested;

        await _hotkeyService.RegisterAsync();
    }

    private async void MainForm_FormClosed(object? sender, FormClosedEventArgs e)
    {
        _hotkeyService.RecordToggleRequested -= OnRecordToggleRequested;
        _hotkeyService.PlaybackToggleRequested -= OnPlaybackToggleRequested;
        _hotkeyService.StopRequested -= OnStopRequested;

        await _hotkeyService.UnregisterAsync();
    }

    private async void OnRecordToggleRequested()
    {
        if (_macroPlaybackService.IsPlaying || _macroPlaybackService.IsPaused)
        {
            return;
        }

        if (_macroRecorderService.IsRecording)
        {
            await _macroRecorderService.StopAsync();
            return;
        }

        await _macroRecorderService.StartAsync();
    }

    private async void OnPlaybackToggleRequested()
    {
        if (_macroRecorderService.IsRecording)
        {
            return;
        }

        if (_macroPlaybackService.IsPlaying)
        {
            if (_macroPlaybackService.IsPaused)
            {
                await _macroPlaybackService.ResumeAsync();
            }
            else
            {
                await _macroPlaybackService.PauseAsync();
            }

            return;
        }

        if (_macroRecorderService.CurrentSession is { Events.Count: > 0 } currentSession)
        {
            await _macroPlaybackService.PlayAsync(currentSession, new PlaybackSettings());
        }
    }

    private async void OnStopRequested()
    {
        if (_macroRecorderService.IsRecording)
        {
            await _macroRecorderService.StopAsync();
        }

        if (_macroPlaybackService.IsPlaying || _macroPlaybackService.IsPaused)
        {
            await _macroPlaybackService.StopAsync();
        }
    }
}