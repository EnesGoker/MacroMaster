using MacroMaster.Application.Abstractions;

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
        InitializeLayout();
        ApplyTheme();
        InitializeEventGrid();
        RefreshUiState();

        Load += MainForm_Load;
        FormClosed += MainForm_FormClosed;
    }

    private async void MainForm_Load(object? sender, EventArgs e)
    {
        SubscribeFormEvents();
        await RegisterHotkeysAsync();
    }

    private async void MainForm_FormClosed(object? sender, FormClosedEventArgs e)
    {
        UnsubscribeFormEvents();
        await UnregisterHotkeysAsync();
    }
}
