using MacroMaster.Application.Abstractions;
using MacroMaster.Application.Services;
using MacroMaster.Infrastructure.Hooks;
using MacroMaster.Infrastructure.Interop;
using MacroMaster.Infrastructure.Persistence;
using MacroMaster.WinForms.Forms;
using MacroMaster.WinForms.Platform;

namespace MacroMaster.WinForms.Composition;

internal sealed class AppCompositionRoot
{
    public IApplicationStateService ApplicationStateService { get; }
    public IMacroRecorderService MacroRecorderService { get; }
    public IMacroPlaybackService MacroPlaybackService { get; }
    public IMacroStorageService MacroStorageService { get; }
    public IHotkeyService HotkeyService { get; }

    private AppCompositionRoot(
        IApplicationStateService applicationStateService,
        IMacroRecorderService macroRecorderService,
        IMacroPlaybackService macroPlaybackService,
        IMacroStorageService macroStorageService,
        IHotkeyService hotkeyService)
    {
        ApplicationStateService = applicationStateService;
        MacroRecorderService = macroRecorderService;
        MacroPlaybackService = macroPlaybackService;
        MacroStorageService = macroStorageService;
        HotkeyService = hotkeyService;
    }

    public static AppCompositionRoot Create()
    {
        IApplicationStateService applicationStateService = new ApplicationStateService();

        IHotkeyConfiguration hotkeyConfiguration = new DefaultHotkeyConfiguration();

        IKeyboardHookSource keyboardHookSource = new WindowsKeyboardHookSource();
        IMouseHookSource mouseHookSource = new WindowsMouseHookSource();
        IHotkeyService hotkeyService = new WindowsHotkeyService(hotkeyConfiguration);

        var jsonMacroStorageService = new JsonMacroStorageService();
        var xmlMacroStorageService = new XmlMacroStorageService();
        IMacroStorageService macroStorageService =
            new MacroStorageService(jsonMacroStorageService, xmlMacroStorageService);

        IInputPlaybackAdapter inputPlaybackAdapter = new WindowsInputPlaybackAdapter();

        IMacroRecorderService macroRecorderService = new MacroRecorderService(
            keyboardHookSource,
            mouseHookSource,
            applicationStateService,
            hotkeyConfiguration);

        IMacroPlaybackService macroPlaybackService = new MacroPlaybackService(
            inputPlaybackAdapter,
            applicationStateService);

        return new AppCompositionRoot(
            applicationStateService,
            macroRecorderService,
            macroPlaybackService,
            macroStorageService,
            hotkeyService);
    }

    public MainForm CreateMainForm()
    {
        return new MainForm(
            ApplicationStateService,
            MacroRecorderService,
            MacroPlaybackService,
            MacroStorageService,
            HotkeyService);
    }
}