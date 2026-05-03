using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using MacroMaster.Application.Abstractions;
using MacroMaster.Application.Services;
using MacroMaster.Domain.Enums;
using MacroMaster.Domain.Models;
using MacroMaster.Infrastructure.Diagnostics;
using MacroMaster.Infrastructure.Hooks;
using MacroMaster.Infrastructure.Persistence;
using MacroMaster.WinForms;
using MacroMaster.WinForms.Composition;
using MacroMaster.WinForms.Forms;
using Xunit;
using Assert = global::MacroMaster.Tests.Expect;

namespace MacroMaster.Tests;

public sealed class MacroMasterTests
{
    [Fact(DisplayName = "ApplicationStateService allows valid transitions")]
    public Task ApplicationStateService_AllowsValidTransitions() => ApplicationStateService_AllowsValidTransitionsAsync();

    [Fact(DisplayName = "ApplicationStateService rejects invalid transitions")]
    public Task ApplicationStateService_RejectsInvalidTransitions() => ApplicationStateService_RejectsInvalidTransitionsAsync();

    [Fact(DisplayName = "JsonMacroStorageService round-trips a valid session")]
    public Task JsonMacroStorageService_RoundTripsSession() => JsonMacroStorageService_RoundTripsSessionAsync();

    [Fact(DisplayName = "XmlMacroStorageService round-trips a valid session")]
    public Task XmlMacroStorageService_RoundTripsSession() => XmlMacroStorageService_RoundTripsSessionAsync();

    [Fact(DisplayName = "JsonMacroStorageService rejects unsupported format versions")]
    public Task JsonMacroStorageService_RejectsUnsupportedVersion() => JsonMacroStorageService_RejectsUnsupportedVersionAsync();

    [Fact(DisplayName = "JsonMacroStorageService rejects system events during save")]
    public Task JsonMacroStorageService_RejectsSystemEventOnSave() => JsonMacroStorageService_RejectsSystemEventOnSaveAsync();

    [Fact(DisplayName = "JsonMacroStorageService rejects system events during load")]
    public Task JsonMacroStorageService_RejectsSystemEventOnLoad() => JsonMacroStorageService_RejectsSystemEventOnLoadAsync();

    [Fact(DisplayName = "JsonPlaybackSettingsStore returns defaults when no file exists")]
    public Task JsonPlaybackSettingsStore_ReturnsDefaultsWhenMissing() => JsonPlaybackSettingsStore_ReturnsDefaultsWhenMissingAsync();

    [Fact(DisplayName = "JsonPlaybackSettingsStore round-trips playback settings")]
    public Task JsonPlaybackSettingsStore_RoundTripsSettings() => JsonPlaybackSettingsStore_RoundTripsSettingsAsync();

    [Fact(DisplayName = "JsonPlaybackSettingsStore normalizes speed when original timing is preserved")]
    public Task JsonPlaybackSettingsStore_NormalizesPreservedTiming() => JsonPlaybackSettingsStore_NormalizesPreservedTimingAsync();

    [Fact(DisplayName = "JsonPlaybackSettingsStore rejects invalid persisted settings")]
    public Task JsonPlaybackSettingsStore_RejectsInvalidSettings() => JsonPlaybackSettingsStore_RejectsInvalidSettingsAsync();

    [Fact(DisplayName = "JsonHotkeySettingsStore returns defaults when no file exists")]
    public Task JsonHotkeySettingsStore_ReturnsDefaultsWhenMissing() => JsonHotkeySettingsStore_ReturnsDefaultsWhenMissingAsync();

    [Fact(DisplayName = "JsonHotkeySettingsStore round-trips hotkey settings")]
    public Task JsonHotkeySettingsStore_RoundTripsSettings() => JsonHotkeySettingsStore_RoundTripsSettingsAsync();

    [Fact(DisplayName = "JsonHotkeySettingsStore rejects duplicate bindings")]
    public Task JsonHotkeySettingsStore_RejectsDuplicateBindings() => JsonHotkeySettingsStore_RejectsDuplicateBindingsAsync();

    [Fact(DisplayName = "HotkeySettingsDialog exposes all controls within the dialog bounds")]
    public Task HotkeySettingsDialog_UsesStableLayout() => HotkeySettingsDialog_UsesStableLayoutAsync();

    [Fact(DisplayName = "AppCompositionRoot honors injected app storage paths")]
    public Task AppCompositionRoot_UsesInjectedStoragePaths() => AppCompositionRoot_UsesInjectedStoragePathsAsync();

    [Fact(DisplayName = "FileLogger writes entries to the daily log file")]
    public Task FileLogger_WritesEntries() => FileLogger_WritesEntriesAsync();

    [Fact(DisplayName = "WindowsKeyboardHookSource swallows subscriber exceptions in hook callbacks")]
    public Task WindowsKeyboardHookSource_SwallowsSubscriberExceptionsInCallback() => WindowsKeyboardHookSource_SwallowsSubscriberExceptionsInCallbackAsync();

    [Fact(DisplayName = "WindowsMouseHookSource swallows subscriber exceptions in hook callbacks")]
    public Task WindowsMouseHookSource_SwallowsSubscriberExceptionsInCallback() => WindowsMouseHookSource_SwallowsSubscriberExceptionsInCallbackAsync();

    [Fact(DisplayName = "GlobalExceptionHandlerRegistration logs UI thread exceptions")]
    public Task GlobalExceptionHandlerRegistration_LogsThreadExceptions() => GlobalExceptionHandlerRegistration_LogsThreadExceptionsAsync();

    [Fact(DisplayName = "GlobalExceptionHandlerRegistration logs unobserved task exceptions")]
    public Task GlobalExceptionHandlerRegistration_LogsUnobservedTaskExceptions() => GlobalExceptionHandlerRegistration_LogsUnobservedTaskExceptionsAsync();

    [Fact(DisplayName = "JsonMacroStorageService rejects mouse click events without coordinates")]
    public Task JsonMacroStorageService_RejectsMouseClickWithoutCoordinates() => JsonMacroStorageService_RejectsMouseClickWithoutCoordinatesAsync();

    [Fact(DisplayName = "JsonMacroStorageService rejects invalid event type values")]
    public Task JsonMacroStorageService_RejectsInvalidEventType() => JsonMacroStorageService_RejectsInvalidEventTypeAsync();

    [Fact(DisplayName = "JsonMacroStorageService rejects invalid keyboard action values")]
    public Task JsonMacroStorageService_RejectsInvalidKeyboardAction() => JsonMacroStorageService_RejectsInvalidKeyboardActionAsync();

    [Fact(DisplayName = "JsonMacroStorageService rejects invalid mouse action values")]
    public Task JsonMacroStorageService_RejectsInvalidMouseAction() => JsonMacroStorageService_RejectsInvalidMouseActionAsync();

    [Fact(DisplayName = "MacroRecorderService suppresses modifier leakage for reserved hotkeys")]
    public Task MacroRecorderService_SuppressesReservedHotkeyModifiers() => MacroRecorderService_SuppressesReservedHotkeyModifiersAsync();

    [Fact(DisplayName = "MacroRecorderService preserves legitimate modifier combinations")]
    public Task MacroRecorderService_PreservesLegitimateModifierSequences() => MacroRecorderService_PreservesLegitimateModifierSequencesAsync();

    [Fact(DisplayName = "MacroRecorderService collects concurrent input safely")]
    public Task MacroRecorderService_ConcurrentInput_CollectsEvents() => MacroRecorderService_ConcurrentInput_CollectsEventsAsync();

    [Fact(DisplayName = "MacroRecorderService retains the completed session until clear")]
    public Task MacroRecorderService_StopAsync_RetainsCompletedSession() => MacroRecorderService_StopAsync_RetainsCompletedSessionAsync();

    [Fact(DisplayName = "MacroRecorderService clear removes the active session")]
    public Task MacroRecorderService_Clear_RemovesActiveSession() => MacroRecorderService_Clear_RemovesActiveSessionAsync();

    [Fact(DisplayName = "MacroRecorderService clear is rejected while recording")]
    public Task MacroRecorderService_Clear_WhileRecording_Throws() => MacroRecorderService_Clear_WhileRecording_ThrowsAsync();

    [Fact(DisplayName = "MacroPlaybackService rebases mouse coordinates when relative playback is enabled")]
    public Task MacroPlaybackService_UseRelativeCoordinates_RebasesMouseCoordinates() => MacroPlaybackService_UseRelativeCoordinates_RebasesMouseCoordinatesAsync();

    [Fact(DisplayName = "MacroPlaybackService re-anchors relative mouse playback for each repeat iteration")]
    public Task MacroPlaybackService_UseRelativeCoordinates_ReanchorsEachIteration() => MacroPlaybackService_UseRelativeCoordinates_ReanchorsEachIterationAsync();

    [Fact(DisplayName = "MacroPlaybackService preserves absolute mouse coordinates when relative playback is disabled")]
    public Task MacroPlaybackService_UseRelativeCoordinatesFalse_PreservesAbsoluteCoordinates() => MacroPlaybackService_UseRelativeCoordinatesFalse_PreservesAbsoluteCoordinatesAsync();

    [Fact(DisplayName = "MacroPlaybackService honors repeat count")]
    public Task MacroPlaybackService_RepeatCount_ReplaysEntireSession() => MacroPlaybackService_RepeatCount_ReplaysEntireSessionAsync();

    [Fact(DisplayName = "MacroPlaybackService pauses until resume is requested")]
    public Task MacroPlaybackService_PauseResume_WaitsForResume() => MacroPlaybackService_PauseResume_WaitsForResumeAsync();

    [Fact(DisplayName = "MacroPlaybackService stops immediately on playback errors when StopOnError is enabled")]
    public Task MacroPlaybackService_StopOnErrorTrue_StopsImmediately() => MacroPlaybackService_StopOnErrorTrue_StopsImmediatelyAsync();

    [Fact(DisplayName = "MacroPlaybackService continues and reports failures when StopOnError is disabled")]
    public Task MacroPlaybackService_StopOnErrorFalse_ContinuesAndReports() => MacroPlaybackService_StopOnErrorFalse_ContinuesAndReportsAsync();

    private static Task ApplicationStateService_AllowsValidTransitionsAsync()
    {
        var stateService = new ApplicationStateService();
        List<AppState> observedStates = [];

        stateService.StateChanged += observedStates.Add;

        Assert.Equal(AppState.Idle, stateService.CurrentState, "Initial state should be Idle.");
        Assert.True(stateService.TryTransitionTo(AppState.Recording, AppState.Idle), "Idle -> Recording should succeed.");
        Assert.True(stateService.TryTransitionTo(AppState.Stopping, AppState.Recording), "Recording -> Stopping should succeed.");
        Assert.True(stateService.TryTransitionTo(AppState.Idle, AppState.Stopping), "Stopping -> Idle should succeed.");
        Assert.Equal(
            new[] { AppState.Recording, AppState.Stopping, AppState.Idle },
            observedStates,
            "StateChanged should publish each successful transition.");

        return Task.CompletedTask;
    }

    private static Task ApplicationStateService_RejectsInvalidTransitionsAsync()
    {
        var stateService = new ApplicationStateService();

        Assert.Throws<InvalidOperationException>(
            () => stateService.SetState(AppState.Paused),
            "Idle -> Paused should be rejected.");

        Assert.Equal(AppState.Idle, stateService.CurrentState, "State must remain Idle after a rejected transition.");
        Assert.False(
            stateService.TryTransitionTo(AppState.Playing, AppState.Recording),
            "Allowed current state guard should reject transitions when current state does not match.");

        return Task.CompletedTask;
    }

    private static async Task JsonMacroStorageService_RoundTripsSessionAsync()
    {
        string directoryPath = CreateTempDirectory();

        try
        {
            string filePath = Path.Combine(directoryPath, "roundtrip.json");
            var storageService = new JsonMacroStorageService();
            var session = new MacroSession
            {
                Name = "RoundTrip",
                Events =
                {
                    new MacroEvent
                    {
                        EventType = MacroEventType.Keyboard,
                        KeyboardActionType = KeyboardActionType.KeyDown,
                        DelayMs = 15,
                        KeyCode = 0x41,
                        ScanCode = 0x1E,
                        IsExtendedKey = false,
                        KeyName = "A",
                        Description = "Keyboard A down"
                    },
                    new MacroEvent
                    {
                        EventType = MacroEventType.Mouse,
                        MouseActionType = MouseActionType.Move,
                        DelayMs = 20,
                        X = 320,
                        Y = 240,
                        Description = "Mouse move"
                    }
                }
            };

            await storageService.SaveAsync(session, filePath);
            MacroSession loadedSession = await storageService.LoadAsync(filePath);

            Assert.Equal(MacroSessionFormat.CurrentVersion, loadedSession.FormatVersion, "Saved session should use the current format version.");
            Assert.Equal(session.Name, loadedSession.Name, "Session name should round-trip.");
            Assert.Equal(2, loadedSession.Events.Count, "Event count should round-trip.");
            Assert.Equal(0x1E, loadedSession.Events[0].ScanCode, "Keyboard scan code should round-trip.");
            Assert.Equal(320, loadedSession.Events[1].X, "Mouse X coordinate should round-trip.");
            Assert.Equal(240, loadedSession.Events[1].Y, "Mouse Y coordinate should round-trip.");
        }
        finally
        {
            DeleteDirectoryIfExists(directoryPath);
        }
    }

    private static async Task XmlMacroStorageService_RoundTripsSessionAsync()
    {
        string directoryPath = CreateTempDirectory();

        try
        {
            string filePath = Path.Combine(directoryPath, "roundtrip.xml");
            var storageService = new XmlMacroStorageService();
            var session = new MacroSession
            {
                Name = "XmlRoundTrip",
                Events =
                {
                    new MacroEvent
                    {
                        EventType = MacroEventType.Keyboard,
                        KeyboardActionType = KeyboardActionType.KeyDown,
                        DelayMs = 10,
                        KeyCode = 0x41,
                        ScanCode = 0x1E,
                        Description = "Keyboard A down"
                    },
                    new MacroEvent
                    {
                        EventType = MacroEventType.Mouse,
                        MouseActionType = MouseActionType.Move,
                        DelayMs = 25,
                        X = 640,
                        Y = 360,
                        Description = "Mouse move"
                    }
                }
            };

            await storageService.SaveAsync(session, filePath);
            MacroSession loadedSession = await storageService.LoadAsync(filePath);

            Assert.Equal(session.Name, loadedSession.Name, "XML session name should round-trip.");
            Assert.Equal(2, loadedSession.Events.Count, "XML event count should round-trip.");
            Assert.Equal(0x41, loadedSession.Events[0].KeyCode, "XML keyboard event data should round-trip.");
            Assert.Equal(640, loadedSession.Events[1].X, "XML mouse X coordinate should round-trip.");
            Assert.Equal(360, loadedSession.Events[1].Y, "XML mouse Y coordinate should round-trip.");
        }
        finally
        {
            DeleteDirectoryIfExists(directoryPath);
        }
    }

    private static async Task JsonMacroStorageService_RejectsUnsupportedVersionAsync()
    {
        string directoryPath = CreateTempDirectory();

        try
        {
            string filePath = Path.Combine(directoryPath, "unsupported.json");

            string payload = JsonSerializer.Serialize(
                new
                {
                    Id = Guid.NewGuid(),
                    Name = "Unsupported",
                    CreatedAtUtc = DateTime.UtcNow,
                    FormatVersion = "9.9",
                    Events = Array.Empty<object>()
                });

            await File.WriteAllTextAsync(filePath, payload);

            var storageService = new JsonMacroStorageService();

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => storageService.LoadAsync(filePath),
                "Unsupported session versions must be rejected during load.");
        }
        finally
        {
            DeleteDirectoryIfExists(directoryPath);
        }
    }

    private static async Task JsonMacroStorageService_RejectsSystemEventOnSaveAsync()
    {
        string directoryPath = CreateTempDirectory();

        try
        {
            string filePath = Path.Combine(directoryPath, "system-event.json");
            var storageService = new JsonMacroStorageService();
            var session = new MacroSession
            {
                Name = "SystemEventSave",
                Events =
                {
                    new MacroEvent
                    {
                        EventType = MacroEventType.System,
                        DelayMs = 5,
                        Description = "System event"
                    }
                }
            };

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => storageService.SaveAsync(session, filePath),
                "System events must be rejected during save.");
        }
        finally
        {
            DeleteDirectoryIfExists(directoryPath);
        }
    }

    private static async Task JsonMacroStorageService_RejectsSystemEventOnLoadAsync()
    {
        string directoryPath = CreateTempDirectory();

        try
        {
            string filePath = Path.Combine(directoryPath, "system-event-load.json");

            string payload = JsonSerializer.Serialize(
                new
                {
                    Id = Guid.NewGuid(),
                    Name = "SystemEventLoad",
                    CreatedAtUtc = DateTime.UtcNow,
                    FormatVersion = MacroSessionFormat.CurrentVersion,
                    Events = new[]
                    {
                        new
                        {
                            Id = Guid.NewGuid(),
                            EventType = MacroEventType.System,
                            DelayMs = 10,
                            Description = "Unsupported system event"
                        }
                    }
                });

            await File.WriteAllTextAsync(filePath, payload);

            var storageService = new JsonMacroStorageService();

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => storageService.LoadAsync(filePath),
                "System events must be rejected during load.");
        }
        finally
        {
            DeleteDirectoryIfExists(directoryPath);
        }
    }

    private static async Task JsonPlaybackSettingsStore_ReturnsDefaultsWhenMissingAsync()
    {
        string directoryPath = CreateTempDirectory();

        try
        {
            string filePath = Path.Combine(directoryPath, "playback-settings.json");
            var settingsStore = new JsonPlaybackSettingsStore(filePath);

            PlaybackSettings settings = await settingsStore.LoadAsync();

            Assert.Equal(1.0, settings.SpeedMultiplier, "Missing settings files should fall back to the default speed multiplier.");
            Assert.Equal(1, settings.RepeatCount, "Missing settings files should fall back to the default repeat count.");
            Assert.Equal(0, settings.InitialDelayMs, "Missing settings files should fall back to zero initial delay.");
            Assert.False(settings.LoopIndefinitely, "Missing settings files should not enable looping by default.");
            Assert.False(settings.UseRelativeCoordinates, "Missing settings files should not enable relative coordinates by default.");
            Assert.True(settings.StopOnError, "Missing settings files should preserve the default stop-on-error behavior.");
            Assert.True(settings.PreserveOriginalTiming, "Missing settings files should preserve original timing by default.");
        }
        finally
        {
            DeleteDirectoryIfExists(directoryPath);
        }
    }

    private static async Task JsonPlaybackSettingsStore_RoundTripsSettingsAsync()
    {
        string directoryPath = CreateTempDirectory();

        try
        {
            string filePath = Path.Combine(directoryPath, "playback-settings.json");
            var settingsStore = new JsonPlaybackSettingsStore(filePath);
            var expectedSettings = new PlaybackSettings
            {
                SpeedMultiplier = 1.75,
                RepeatCount = 3,
                InitialDelayMs = 500,
                LoopIndefinitely = true,
                UseRelativeCoordinates = true,
                StopOnError = false,
                PreserveOriginalTiming = false
            };

            await settingsStore.SaveAsync(expectedSettings);
            PlaybackSettings loadedSettings = await settingsStore.LoadAsync();

            Assert.Equal(expectedSettings.SpeedMultiplier, loadedSettings.SpeedMultiplier, "Playback speed multiplier should round-trip.");
            Assert.Equal(expectedSettings.RepeatCount, loadedSettings.RepeatCount, "Playback repeat count should round-trip.");
            Assert.Equal(expectedSettings.InitialDelayMs, loadedSettings.InitialDelayMs, "Playback initial delay should round-trip.");
            Assert.Equal(expectedSettings.LoopIndefinitely, loadedSettings.LoopIndefinitely, "Loop-indefinitely should round-trip.");
            Assert.Equal(expectedSettings.UseRelativeCoordinates, loadedSettings.UseRelativeCoordinates, "Relative coordinate mode should round-trip.");
            Assert.Equal(expectedSettings.StopOnError, loadedSettings.StopOnError, "Stop-on-error should round-trip.");
            Assert.Equal(expectedSettings.PreserveOriginalTiming, loadedSettings.PreserveOriginalTiming, "Preserve-original-timing should round-trip.");
        }
        finally
        {
            DeleteDirectoryIfExists(directoryPath);
        }
    }

    private static async Task JsonPlaybackSettingsStore_NormalizesPreservedTimingAsync()
    {
        string directoryPath = CreateTempDirectory();

        try
        {
            string filePath = Path.Combine(directoryPath, "normalized-playback-settings.json");
            var settingsStore = new JsonPlaybackSettingsStore(filePath);
            var settings = new PlaybackSettings
            {
                SpeedMultiplier = 2.5,
                RepeatCount = 2,
                InitialDelayMs = 150,
                LoopIndefinitely = false,
                UseRelativeCoordinates = false,
                StopOnError = true,
                PreserveOriginalTiming = true
            };

            await settingsStore.SaveAsync(settings);
            PlaybackSettings loadedSettings = await settingsStore.LoadAsync();

            Assert.Equal(1.0, loadedSettings.SpeedMultiplier, "PreserveOriginalTiming should normalize the persisted speed multiplier to 1.0.");
            Assert.True(loadedSettings.PreserveOriginalTiming, "PreserveOriginalTiming should remain enabled after normalization.");
        }
        finally
        {
            DeleteDirectoryIfExists(directoryPath);
        }
    }

    private static async Task JsonPlaybackSettingsStore_RejectsInvalidSettingsAsync()
    {
        string directoryPath = CreateTempDirectory();

        try
        {
            string filePath = Path.Combine(directoryPath, "invalid-playback-settings.json");

            string payload = JsonSerializer.Serialize(
                new
                {
                    SpeedMultiplier = 0,
                    RepeatCount = 0,
                    InitialDelayMs = -1,
                    LoopIndefinitely = false,
                    UseRelativeCoordinates = false,
                    StopOnError = true,
                    PreserveOriginalTiming = true
                });

            await File.WriteAllTextAsync(filePath, payload);

            var settingsStore = new JsonPlaybackSettingsStore(filePath);

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => settingsStore.LoadAsync(),
                "Invalid playback settings files must be rejected during load.");
        }
        finally
        {
            DeleteDirectoryIfExists(directoryPath);
        }
    }

    private static async Task JsonHotkeySettingsStore_ReturnsDefaultsWhenMissingAsync()
    {
        string directoryPath = CreateTempDirectory();

        try
        {
            string filePath = Path.Combine(directoryPath, "hotkey-settings.json");
            var hotkeySettingsStore = new JsonHotkeySettingsStore(filePath);

            HotkeySettings settings = await hotkeySettingsStore.LoadAsync();

            Assert.Equal(
                HotkeySettings.DefaultRecordToggleHotkey,
                settings.RecordToggleHotkey,
                "Missing hotkey settings files should fall back to the default record hotkey.");
            Assert.Equal(
                HotkeySettings.DefaultPlaybackToggleHotkey,
                settings.PlaybackToggleHotkey,
                "Missing hotkey settings files should fall back to the default playback hotkey.");
            Assert.Equal(
                HotkeySettings.DefaultStopHotkey,
                settings.StopHotkey,
                "Missing hotkey settings files should fall back to the default stop hotkey.");
            Assert.Equal(
                HotkeySettings.DefaultHotkeySettingsHotkey,
                settings.HotkeySettingsHotkey,
                "Missing hotkey settings files should fall back to the default hotkey settings shortcut.");
        }
        finally
        {
            DeleteDirectoryIfExists(directoryPath);
        }
    }

    private static async Task JsonHotkeySettingsStore_RoundTripsSettingsAsync()
    {
        string directoryPath = CreateTempDirectory();

        try
        {
            string filePath = Path.Combine(directoryPath, "hotkey-settings.json");
            var hotkeySettingsStore = new JsonHotkeySettingsStore(filePath);
            var expectedSettings = new HotkeySettings
            {
                RecordToggleHotkey = new HotkeyBinding(0x70, HotkeyModifiers.Control),
                PlaybackToggleHotkey = new HotkeyBinding(0x71, HotkeyModifiers.Control | HotkeyModifiers.Shift),
                StopHotkey = new HotkeyBinding(0x72, HotkeyModifiers.Alt),
                HotkeySettingsHotkey = new HotkeyBinding(0x73, HotkeyModifiers.Control | HotkeyModifiers.Alt)
            };

            await hotkeySettingsStore.SaveAsync(expectedSettings);
            HotkeySettings loadedSettings = await hotkeySettingsStore.LoadAsync();

            Assert.Equal(expectedSettings.RecordToggleHotkey, loadedSettings.RecordToggleHotkey, "Record hotkey should round-trip.");
            Assert.Equal(expectedSettings.PlaybackToggleHotkey, loadedSettings.PlaybackToggleHotkey, "Playback hotkey should round-trip.");
            Assert.Equal(expectedSettings.StopHotkey, loadedSettings.StopHotkey, "Stop hotkey should round-trip.");
            Assert.Equal(expectedSettings.HotkeySettingsHotkey, loadedSettings.HotkeySettingsHotkey, "Hotkey settings shortcut should round-trip.");
        }
        finally
        {
            DeleteDirectoryIfExists(directoryPath);
        }
    }

    private static async Task JsonHotkeySettingsStore_RejectsDuplicateBindingsAsync()
    {
        string directoryPath = CreateTempDirectory();

        try
        {
            string filePath = Path.Combine(directoryPath, "invalid-hotkey-settings.json");

            string payload = JsonSerializer.Serialize(
                new HotkeySettings
                {
                    RecordToggleHotkey = new HotkeyBinding(0x70, HotkeyModifiers.Control),
                    PlaybackToggleHotkey = new HotkeyBinding(0x70, HotkeyModifiers.Control),
                    StopHotkey = new HotkeyBinding(0x72, HotkeyModifiers.Alt)
                });

            await File.WriteAllTextAsync(filePath, payload);

            var hotkeySettingsStore = new JsonHotkeySettingsStore(filePath);

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => hotkeySettingsStore.LoadAsync(),
                "Duplicate hotkey bindings must be rejected during load.");
        }
        finally
        {
            DeleteDirectoryIfExists(directoryPath);
        }
    }

    private static async Task HotkeySettingsDialog_UsesStableLayoutAsync()
    {
        _ = await RunOnStaThreadAsync(() =>
        {
            using var dialog = new HotkeySettingsDialog(HotkeySettings.CreateDefault());
            dialog.CreateControl();
            dialog.PerformLayout();

            Assert.True(
                dialog.ClientSize.Width >= 720,
                $"Hotkey settings dialog should allocate enough width to show all selector columns. Actual client size: {dialog.ClientSize.Width}x{dialog.ClientSize.Height}.");

            List<ComboBox> comboBoxes = GetDescendants<ComboBox>(dialog).ToList();
            Assert.Equal(8, comboBoxes.Count, "Hotkey settings dialog should expose eight combo boxes.");

            List<Button> buttons = GetDescendants<Button>(dialog).ToList();
            Assert.Equal(3, buttons.Count, "Hotkey settings dialog should expose three action buttons.");

            foreach (Control control in comboBoxes.Cast<Control>().Concat(buttons))
            {
                Rectangle bounds = GetBoundsRelativeToAncestor(control, dialog);

                Assert.True(
                    bounds.Left >= 0 && bounds.Top >= 0,
                    $"{DescribeControl(control)} should remain within the dialog origin.");
                Assert.True(
                    bounds.Right <= dialog.ClientSize.Width,
                    $"{DescribeControl(control)} should be fully visible within the dialog width.");
                Assert.True(
                    bounds.Bottom <= dialog.ClientSize.Height,
                    $"{DescribeControl(control)} should be fully visible within the dialog height.");
            }
        });
    }

    private static async Task FileLogger_WritesEntriesAsync()
    {
        string directoryPath = CreateTempDirectory();

        try
        {
            string logDirectoryPath = Path.Combine(directoryPath, "logs");
            var logger = new FileLogger(logDirectoryPath);
            var sampleException = new InvalidOperationException("Ornek hata ayrintisi");

            logger.Log(AppLogLevel.Information, "TestKaynak", "Bilgi kaydi");
            logger.Log(AppLogLevel.Error, "TestKaynak", "Hata kaydi", sampleException);
            logger.Dispose();

            string[] logFiles = Directory.GetFiles(logDirectoryPath, "macromaster-*.log");
            Assert.Equal(1, logFiles.Length, "Logger should create a single daily log file for the current date.");

            string logContents = await File.ReadAllTextAsync(logFiles[0]);
            Assert.True(logContents.Contains("[Bilgi]"), "Log output should include the localized information level.");
            Assert.True(logContents.Contains("Bilgi kaydi"), "Log output should include the information message.");
            Assert.True(logContents.Contains("Hata kaydi"), "Log output should include the error message.");
            Assert.True(logContents.Contains("Ornek hata ayrintisi"), "Log output should include exception details.");
        }
        finally
        {
            DeleteDirectoryIfExists(directoryPath);
        }
    }

    private static Task WindowsKeyboardHookSource_SwallowsSubscriberExceptionsInCallbackAsync()
    {
        var logger = new RecordingTestLogger();
        using var keyboardHookSource = new WindowsKeyboardHookSource(logger);
        keyboardHookSource.KeyActivityReceived += _ =>
            throw new InvalidOperationException("Klavye dinleyici hatasi");

        IntPtr hookDataPointer = Marshal.AllocHGlobal(Marshal.SizeOf<TestKeyboardHookStruct>());

        try
        {
            Marshal.StructureToPtr(
                new TestKeyboardHookStruct
                {
                    vkCode = 0x41,
                    scanCode = 0x1E,
                    flags = 0,
                    time = 0,
                    dwExtraInfo = IntPtr.Zero
                },
                hookDataPointer,
                fDeleteOld: false);

            _ = InvokeHookCallback(
                keyboardHookSource,
                nCode: 0,
                wParam: 0x0100,
                hookDataPointer);
        }
        finally
        {
            Marshal.FreeHGlobal(hookDataPointer);
        }

        Assert.True(
            logger.Entries.Any(entry =>
                entry.Source == nameof(WindowsKeyboardHookSource)
                && entry.Message.Contains("dinleyicilere iletilirken", StringComparison.Ordinal)),
            "Keyboard hook callbacks should log and swallow subscriber exceptions.");
        return Task.CompletedTask;
    }

    private static Task WindowsMouseHookSource_SwallowsSubscriberExceptionsInCallbackAsync()
    {
        var logger = new RecordingTestLogger();
        using var mouseHookSource = new WindowsMouseHookSource(logger);
        mouseHookSource.MouseActivityReceived += (_, _, _, _) =>
            throw new InvalidOperationException("Fare dinleyici hatasi");

        IntPtr hookDataPointer = Marshal.AllocHGlobal(Marshal.SizeOf<TestMouseHookStruct>());

        try
        {
            Marshal.StructureToPtr(
                new TestMouseHookStruct
                {
                    pt = new TestPoint { x = 320, y = 240 },
                    mouseData = 0,
                    flags = 0,
                    time = 0,
                    dwExtraInfo = IntPtr.Zero
                },
                hookDataPointer,
                fDeleteOld: false);

            _ = InvokeHookCallback(
                mouseHookSource,
                nCode: 0,
                wParam: 0x0200,
                hookDataPointer);
        }
        finally
        {
            Marshal.FreeHGlobal(hookDataPointer);
        }

        Assert.True(
            logger.Entries.Any(entry =>
                entry.Source == nameof(WindowsMouseHookSource)
                && entry.Message.Contains("dinleyicilere iletilirken", StringComparison.Ordinal)),
            "Mouse hook callbacks should log and swallow subscriber exceptions.");
        return Task.CompletedTask;
    }

    private static Task GlobalExceptionHandlerRegistration_LogsThreadExceptionsAsync()
    {
        var logger = new RecordingTestLogger();
        string? shownMessage = null;

        using var registration = new GlobalExceptionHandlerRegistration(
            logger,
            (message, _, _) => shownMessage = message,
            registerHandlers: false);

        registration.HandleThreadException(new InvalidOperationException("UI thread boom"));

        Assert.True(
            logger.Entries.Any(entry =>
                entry.Source == nameof(GlobalExceptionHandlerRegistration)
                && entry.Message.Contains("UI is parcaciginda", StringComparison.Ordinal)),
            "Global exception handler should log UI thread exceptions.");
        Assert.True(
            !string.IsNullOrWhiteSpace(shownMessage),
            "Global exception handler should surface a user-facing error message for UI thread exceptions.");
        return Task.CompletedTask;
    }

    private static Task GlobalExceptionHandlerRegistration_LogsUnobservedTaskExceptionsAsync()
    {
        var logger = new RecordingTestLogger();

        using var registration = new GlobalExceptionHandlerRegistration(
            logger,
            registerHandlers: false);

        registration.HandleUnobservedTaskException(
            new AggregateException(new InvalidOperationException("Task boom")));

        Assert.True(
            logger.Entries.Any(entry =>
                entry.Source == nameof(GlobalExceptionHandlerRegistration)
                && entry.Message.Contains("Gozlemlenmemis gorev hatasi", StringComparison.Ordinal)),
            "Global exception handler should log unobserved task exceptions.");
        return Task.CompletedTask;
    }

    private static async Task AppCompositionRoot_UsesInjectedStoragePathsAsync()
    {
        string directoryPath = CreateTempDirectory();

        try
        {
            AppStoragePaths storagePaths = AppStoragePaths.FromRootDirectory(directoryPath);

            using AppCompositionRoot compositionRoot = AppCompositionRoot.Create(storagePaths);

            await compositionRoot.PlaybackSettingsStore.SaveAsync(
                new PlaybackSettings
                {
                    SpeedMultiplier = 2.0,
                    RepeatCount = 2,
                    PreserveOriginalTiming = false
                });

            await compositionRoot.HotkeySettingsStore.SaveAsync(
                new HotkeySettings
                {
                    RecordToggleHotkey = new HotkeyBinding(0x70, HotkeyModifiers.Control),
                    PlaybackToggleHotkey = new HotkeyBinding(0x71, HotkeyModifiers.Shift),
                    StopHotkey = new HotkeyBinding(0x72, HotkeyModifiers.Alt)
                });

            Assert.True(
                File.Exists(storagePaths.PlaybackSettingsFilePath),
                "Playback settings should be persisted under the injected app storage root.");
            Assert.True(
                File.Exists(storagePaths.HotkeySettingsFilePath),
                "Hotkey settings should be persisted under the injected app storage root.");
            Assert.True(
                storagePaths.LogDirectoryPath.StartsWith(directoryPath, StringComparison.OrdinalIgnoreCase),
                "Derived log directory should remain within the injected app storage root.");
        }
        finally
        {
            DeleteDirectoryIfExists(directoryPath);
        }
    }

    private static async Task JsonMacroStorageService_RejectsMouseClickWithoutCoordinatesAsync()
    {
        string directoryPath = CreateTempDirectory();

        try
        {
            string filePath = Path.Combine(directoryPath, "missing-click-coordinates.json");

            string payload = JsonSerializer.Serialize(
                new
                {
                    Id = Guid.NewGuid(),
                    Name = "InvalidMouseClick",
                    CreatedAtUtc = DateTime.UtcNow,
                    FormatVersion = MacroSessionFormat.CurrentVersion,
                    Events = new[]
                    {
                        new
                        {
                            Id = Guid.NewGuid(),
                            EventType = MacroEventType.Mouse,
                            MouseActionType = MouseActionType.LeftDown,
                            DelayMs = 10,
                            Description = "Missing coordinates"
                        }
                    }
                });

            await File.WriteAllTextAsync(filePath, payload);

            var storageService = new JsonMacroStorageService();

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => storageService.LoadAsync(filePath),
                "Mouse click events without coordinates must be rejected during load.");
        }
        finally
        {
            DeleteDirectoryIfExists(directoryPath);
        }
    }

    private static async Task JsonMacroStorageService_RejectsInvalidEventTypeAsync()
    {
        string directoryPath = CreateTempDirectory();

        try
        {
            string filePath = Path.Combine(directoryPath, "invalid-event-type.json");

            string payload = JsonSerializer.Serialize(
                new
                {
                    Id = Guid.NewGuid(),
                    Name = "InvalidEventType",
                    CreatedAtUtc = DateTime.UtcNow,
                    FormatVersion = MacroSessionFormat.CurrentVersion,
                    Events = new[]
                    {
                        new
                        {
                            Id = Guid.NewGuid(),
                            EventType = 99,
                            DelayMs = 10,
                            Description = "Invalid event type"
                        }
                    }
                });

            await File.WriteAllTextAsync(filePath, payload);

            var storageService = new JsonMacroStorageService();

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => storageService.LoadAsync(filePath),
                "Invalid event type values must be rejected during load.");
        }
        finally
        {
            DeleteDirectoryIfExists(directoryPath);
        }
    }

    private static async Task JsonMacroStorageService_RejectsInvalidKeyboardActionAsync()
    {
        string directoryPath = CreateTempDirectory();

        try
        {
            string filePath = Path.Combine(directoryPath, "invalid-keyboard-action.json");

            string payload = JsonSerializer.Serialize(
                new
                {
                    Id = Guid.NewGuid(),
                    Name = "InvalidKeyboardAction",
                    CreatedAtUtc = DateTime.UtcNow,
                    FormatVersion = MacroSessionFormat.CurrentVersion,
                    Events = new[]
                    {
                        new
                        {
                            Id = Guid.NewGuid(),
                            EventType = MacroEventType.Keyboard,
                            KeyboardActionType = 99,
                            DelayMs = 10,
                            KeyCode = 0x41,
                            Description = "Invalid keyboard action"
                        }
                    }
                });

            await File.WriteAllTextAsync(filePath, payload);

            var storageService = new JsonMacroStorageService();

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => storageService.LoadAsync(filePath),
                "Invalid keyboard action values must be rejected during load.");
        }
        finally
        {
            DeleteDirectoryIfExists(directoryPath);
        }
    }

    private static async Task JsonMacroStorageService_RejectsInvalidMouseActionAsync()
    {
        string directoryPath = CreateTempDirectory();

        try
        {
            string filePath = Path.Combine(directoryPath, "invalid-mouse-action.json");

            string payload = JsonSerializer.Serialize(
                new
                {
                    Id = Guid.NewGuid(),
                    Name = "InvalidMouseAction",
                    CreatedAtUtc = DateTime.UtcNow,
                    FormatVersion = MacroSessionFormat.CurrentVersion,
                    Events = new[]
                    {
                        new
                        {
                            Id = Guid.NewGuid(),
                            EventType = MacroEventType.Mouse,
                            MouseActionType = 99,
                            DelayMs = 10,
                            X = 100,
                            Y = 200,
                            Description = "Invalid mouse action"
                        }
                    }
                });

            await File.WriteAllTextAsync(filePath, payload);

            var storageService = new JsonMacroStorageService();

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => storageService.LoadAsync(filePath),
                "Invalid mouse action values must be rejected during load.");
        }
        finally
        {
            DeleteDirectoryIfExists(directoryPath);
        }
    }

    private static async Task MacroRecorderService_SuppressesReservedHotkeyModifiersAsync()
    {
        var keyboardHookSource = new TestKeyboardHookSource();
        var mouseHookSource = new TestMouseHookSource();
        var recorder = new MacroRecorderService(
            keyboardHookSource,
            mouseHookSource,
            new ApplicationStateService(),
            new TestHotkeyConfiguration(
                new HotkeyBinding(0x77, HotkeyModifiers.Control),
                HotkeyBinding.None(0x78),
                HotkeyBinding.None(0x79)));

        await recorder.StartAsync("ReservedHotkey");

        keyboardHookSource.Emit(new KeyboardActivityInfo(0xA2, 0x1D, true, false, HotkeyModifiers.Control, true, HotkeyModifiers.Control));
        keyboardHookSource.Emit(new KeyboardActivityInfo(0x77, 0x42, true, false, HotkeyModifiers.Control, false, HotkeyModifiers.None));
        keyboardHookSource.Emit(new KeyboardActivityInfo(0x77, 0x42, false, false, HotkeyModifiers.Control, false, HotkeyModifiers.None));
        keyboardHookSource.Emit(new KeyboardActivityInfo(0xA2, 0x1D, false, false, HotkeyModifiers.None, true, HotkeyModifiers.Control));

        await recorder.StopAsync();

        Assert.Equal(
            0,
            recorder.CurrentSession?.Events.Count ?? -1,
            "Reserved hotkeys must not leak modifier events into the recorded macro.");
    }

    private static async Task MacroRecorderService_PreservesLegitimateModifierSequencesAsync()
    {
        var keyboardHookSource = new TestKeyboardHookSource();
        var mouseHookSource = new TestMouseHookSource();
        var recorder = new MacroRecorderService(
            keyboardHookSource,
            mouseHookSource,
            new ApplicationStateService(),
            new TestHotkeyConfiguration(
                HotkeyBinding.None(0x77),
                HotkeyBinding.None(0x78),
                HotkeyBinding.None(0x79)));

        await recorder.StartAsync("ModifierSequence");

        keyboardHookSource.Emit(new KeyboardActivityInfo(0xA2, 0x1D, true, false, HotkeyModifiers.Control, true, HotkeyModifiers.Control));
        keyboardHookSource.Emit(new KeyboardActivityInfo(0x41, 0x1E, true, false, HotkeyModifiers.Control, false, HotkeyModifiers.None));
        keyboardHookSource.Emit(new KeyboardActivityInfo(0x41, 0x1E, false, false, HotkeyModifiers.Control, false, HotkeyModifiers.None));
        keyboardHookSource.Emit(new KeyboardActivityInfo(0xA2, 0x1D, false, false, HotkeyModifiers.None, true, HotkeyModifiers.Control));

        await recorder.StopAsync();

        MacroSession? session = recorder.CurrentSession;
        Assert.True(session is not null, "The recorder should produce a completed session.");
        Assert.Equal(4, session!.Events.Count, "Legitimate modifier combinations must remain in the recorded macro.");
        Assert.Equal(0xA2, session.Events[0].KeyCode, "Control key-down should be preserved.");
        Assert.Equal(0x41, session.Events[1].KeyCode, "The primary key-down should be preserved.");
        Assert.Equal(0x41, session.Events[2].KeyCode, "The primary key-up should be preserved.");
        Assert.Equal(0xA2, session.Events[3].KeyCode, "Control key-up should be preserved.");
    }

    private static async Task MacroRecorderService_ConcurrentInput_CollectsEventsAsync()
    {
        var keyboardHookSource = new TestKeyboardHookSource();
        var mouseHookSource = new TestMouseHookSource();
        var recorder = new MacroRecorderService(
            keyboardHookSource,
            mouseHookSource,
            new ApplicationStateService(),
            new TestHotkeyConfiguration(
                HotkeyBinding.None(0x77),
                HotkeyBinding.None(0x78),
                HotkeyBinding.None(0x79)));

        const int eventCountPerSource = 50;

        await recorder.StartAsync("ConcurrentInput");

        Task keyboardTask = Task.Run(() =>
        {
            for (int index = 0; index < eventCountPerSource; index++)
            {
                keyboardHookSource.Emit(
                    new KeyboardActivityInfo(
                        0x41 + (index % 5),
                        0x1E + (index % 5),
                        true,
                        false,
                        HotkeyModifiers.None,
                        false,
                        HotkeyModifiers.None));
            }
        });

        Task mouseTask = Task.Run(() =>
        {
            for (int index = 0; index < eventCountPerSource; index++)
            {
                mouseHookSource.Emit(MouseActionType.Move, index, index + 10, null);
            }
        });

        await Task.WhenAll(keyboardTask, mouseTask);
        await recorder.StopAsync();

        MacroSession? session = recorder.CurrentSession;
        Assert.True(session is not null, "The recorder should produce a completed session after concurrent input.");
        Assert.Equal(
            eventCountPerSource * 2,
            session!.Events.Count,
            "Concurrent keyboard and mouse input should be recorded without event loss.");
    }

    private static async Task MacroRecorderService_StopAsync_RetainsCompletedSessionAsync()
    {
        var keyboardHookSource = new TestKeyboardHookSource();
        var mouseHookSource = new TestMouseHookSource();
        var recorder = new MacroRecorderService(
            keyboardHookSource,
            mouseHookSource,
            new ApplicationStateService(),
            new TestHotkeyConfiguration(
                HotkeyBinding.None(0x77),
                HotkeyBinding.None(0x78),
                HotkeyBinding.None(0x79)));

        await recorder.StartAsync("RetainedSession");
        keyboardHookSource.Emit(new KeyboardActivityInfo(0x41, 0x1E, true, false, HotkeyModifiers.None, false, HotkeyModifiers.None));

        await recorder.StopAsync();

        MacroSession? session = recorder.CurrentSession;
        Assert.True(session is not null, "Stop should retain the completed session for follow-up operations.");
        Assert.Equal("RetainedSession", session!.Name, "The retained session should remain available through CurrentSession.");
        Assert.Equal(1, session.Events.Count, "The retained completed session should expose the finalized events.");
    }

    private static async Task MacroRecorderService_Clear_RemovesActiveSessionAsync()
    {
        var keyboardHookSource = new TestKeyboardHookSource();
        var mouseHookSource = new TestMouseHookSource();
        var recorder = new MacroRecorderService(
            keyboardHookSource,
            mouseHookSource,
            new ApplicationStateService(),
            new TestHotkeyConfiguration(
                HotkeyBinding.None(0x77),
                HotkeyBinding.None(0x78),
                HotkeyBinding.None(0x79)));

        await recorder.StartAsync("ClearSession");
        keyboardHookSource.Emit(new KeyboardActivityInfo(0x41, 0x1E, true, false, HotkeyModifiers.None, false, HotkeyModifiers.None));
        await recorder.StopAsync();

        Assert.True(recorder.CurrentSession is not null, "The recorder should have a completed session before clear.");

        recorder.Clear();

        Assert.True(recorder.CurrentSession is null, "Clear should remove the active session reference completely.");
    }

    private static async Task MacroRecorderService_Clear_WhileRecording_ThrowsAsync()
    {
        var keyboardHookSource = new TestKeyboardHookSource();
        var mouseHookSource = new TestMouseHookSource();
        var recorder = new MacroRecorderService(
            keyboardHookSource,
            mouseHookSource,
            new ApplicationStateService(),
            new TestHotkeyConfiguration(
                HotkeyBinding.None(0x77),
                HotkeyBinding.None(0x78),
                HotkeyBinding.None(0x79)));

        await recorder.StartAsync("ClearWhileRecording");

        try
        {
            Assert.Throws<InvalidOperationException>(
                recorder.Clear,
                "Clear should be rejected while recording is in progress.");
        }
        finally
        {
            await recorder.StopAsync();
        }
    }

    private static async Task MacroPlaybackService_UseRelativeCoordinates_RebasesMouseCoordinatesAsync()
    {
        var inputPlaybackAdapter = new TestInputPlaybackAdapter
        {
            CurrentCursorPosition = new CursorPosition(500, 300)
        };
        var playbackService = new MacroPlaybackService(
            inputPlaybackAdapter,
            inputPlaybackAdapter,
            new ApplicationStateService());

        var session = new MacroSession
        {
            Name = "RelativeMouse",
            Events =
            {
                new MacroEvent
                {
                    EventType = MacroEventType.Mouse,
                    MouseActionType = MouseActionType.Move,
                    X = 100,
                    Y = 200,
                    Description = "Move anchor"
                },
                new MacroEvent
                {
                    EventType = MacroEventType.Mouse,
                    MouseActionType = MouseActionType.LeftDown,
                    X = 130,
                    Y = 240,
                    Description = "Click relative"
                }
            }
        };

        await playbackService.PlayAsync(
            session,
            new PlaybackSettings { UseRelativeCoordinates = true });

        Assert.Equal(1, inputPlaybackAdapter.CursorPositionReadCount, "Relative playback should read the current cursor position once per iteration.");
        Assert.Equal(2, inputPlaybackAdapter.PlayedEvents.Count, "Both mouse events should be played.");
        Assert.Equal(500, inputPlaybackAdapter.PlayedEvents[0].X, "The first mouse event should be rebased to the current cursor X position.");
        Assert.Equal(300, inputPlaybackAdapter.PlayedEvents[0].Y, "The first mouse event should be rebased to the current cursor Y position.");
        Assert.Equal(530, inputPlaybackAdapter.PlayedEvents[1].X, "Subsequent mouse X coordinates should preserve their relative delta.");
        Assert.Equal(340, inputPlaybackAdapter.PlayedEvents[1].Y, "Subsequent mouse Y coordinates should preserve their relative delta.");
    }

    private static async Task MacroPlaybackService_UseRelativeCoordinatesFalse_PreservesAbsoluteCoordinatesAsync()
    {
        var inputPlaybackAdapter = new TestInputPlaybackAdapter
        {
            CurrentCursorPosition = new CursorPosition(900, 900)
        };
        var playbackService = new MacroPlaybackService(
            inputPlaybackAdapter,
            inputPlaybackAdapter,
            new ApplicationStateService());

        var session = new MacroSession
        {
            Name = "AbsoluteMouse",
            Events =
            {
                new MacroEvent
                {
                    EventType = MacroEventType.Mouse,
                    MouseActionType = MouseActionType.Move,
                    X = 42,
                    Y = 64,
                    Description = "Absolute move"
                }
            }
        };

        await playbackService.PlayAsync(
            session,
            new PlaybackSettings { UseRelativeCoordinates = false });

        Assert.Equal(0, inputPlaybackAdapter.CursorPositionReadCount, "Absolute playback should not query the current cursor position.");
        Assert.Equal(1, inputPlaybackAdapter.PlayedEvents.Count, "The mouse event should be played once.");
        Assert.Equal(42, inputPlaybackAdapter.PlayedEvents[0].X, "Absolute playback should preserve the recorded X coordinate.");
        Assert.Equal(64, inputPlaybackAdapter.PlayedEvents[0].Y, "Absolute playback should preserve the recorded Y coordinate.");
    }

    private static async Task MacroPlaybackService_UseRelativeCoordinates_ReanchorsEachIterationAsync()
    {
        var inputPlaybackAdapter = new TestInputPlaybackAdapter();
        inputPlaybackAdapter.CursorPositions.Enqueue(new CursorPosition(500, 300));
        inputPlaybackAdapter.CursorPositions.Enqueue(new CursorPosition(1000, 200));

        var playbackService = new MacroPlaybackService(
            inputPlaybackAdapter,
            inputPlaybackAdapter,
            new ApplicationStateService());

        var session = new MacroSession
        {
            Name = "RelativeRepeat",
            Events =
            {
                new MacroEvent
                {
                    EventType = MacroEventType.Mouse,
                    MouseActionType = MouseActionType.Move,
                    X = 100,
                    Y = 200,
                    Description = "Anchor"
                },
                new MacroEvent
                {
                    EventType = MacroEventType.Mouse,
                    MouseActionType = MouseActionType.LeftUp,
                    X = 130,
                    Y = 240,
                    Description = "Delta"
                }
            }
        };

        await playbackService.PlayAsync(
            session,
            new PlaybackSettings
            {
                UseRelativeCoordinates = true,
                RepeatCount = 2
            });

        Assert.Equal(2, inputPlaybackAdapter.CursorPositionReadCount, "Relative playback should re-read the cursor anchor for each repeat.");
        Assert.Equal(4, inputPlaybackAdapter.PlayedEvents.Count, "Two events across two repeats should be played.");
        Assert.Equal(500, inputPlaybackAdapter.PlayedEvents[0].X, "The first iteration should use the first cursor anchor.");
        Assert.Equal(530, inputPlaybackAdapter.PlayedEvents[1].X, "The first iteration delta should be preserved.");
        Assert.Equal(1000, inputPlaybackAdapter.PlayedEvents[2].X, "The second iteration should use the second cursor anchor.");
        Assert.Equal(1030, inputPlaybackAdapter.PlayedEvents[3].X, "The second iteration delta should be preserved.");
    }

    private static async Task MacroPlaybackService_RepeatCount_ReplaysEntireSessionAsync()
    {
        var inputPlaybackAdapter = new TestInputPlaybackAdapter();
        var playbackService = new MacroPlaybackService(
            inputPlaybackAdapter,
            inputPlaybackAdapter,
            new ApplicationStateService());

        MacroSession session = CreatePlaybackSession();

        await playbackService.PlayAsync(
            session,
            new PlaybackSettings
            {
                RepeatCount = 3,
                StopOnError = true
            });

        Assert.Equal(6, inputPlaybackAdapter.PlayedEvents.Count, "RepeatCount should replay the entire session for each requested iteration.");
        Assert.Equal(6, inputPlaybackAdapter.SuccessfulEventIds.Count, "Every repeated event should complete successfully.");
    }

    private static async Task MacroPlaybackService_PauseResume_WaitsForResumeAsync()
    {
        var inputPlaybackAdapter = new TestInputPlaybackAdapter();
        var applicationStateService = new ApplicationStateService();
        var playbackService = new MacroPlaybackService(inputPlaybackAdapter, inputPlaybackAdapter, applicationStateService);
        var firstEventPlayed = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        var session = new MacroSession
        {
            Name = "DuraklatDevamEt",
            Events =
            {
                new MacroEvent
                {
                    Id = Guid.NewGuid(),
                    EventType = MacroEventType.Keyboard,
                    KeyboardActionType = KeyboardActionType.KeyDown,
                    KeyCode = 0x41,
                    ScanCode = 0x1E,
                    Description = "Ilk olay"
                },
                new MacroEvent
                {
                    Id = Guid.NewGuid(),
                    EventType = MacroEventType.Keyboard,
                    KeyboardActionType = KeyboardActionType.KeyUp,
                    KeyCode = 0x41,
                    ScanCode = 0x1E,
                    Description = "Ikinci olay"
                }
            }
        };

        playbackService.EventPlayed += macroEvent =>
        {
            if (macroEvent.Id != session.Events[0].Id)
            {
                return;
            }

            playbackService.PauseAsync().GetAwaiter().GetResult();
            firstEventPlayed.TrySetResult(true);
        };

        Task playTask = playbackService.PlayAsync(
            session,
            new PlaybackSettings
            {
                PreserveOriginalTiming = true,
                StopOnError = true
            });

        await firstEventPlayed.Task.WaitAsync(TimeSpan.FromSeconds(2));
        await Task.Delay(75);

        Assert.Equal(1, inputPlaybackAdapter.PlayedEvents.Count, "Playback should remain paused until resume is requested.");
        Assert.Equal(AppState.Paused, applicationStateService.CurrentState, "Playback state should be Paused while waiting for resume.");

        await playbackService.ResumeAsync();
        await playTask.WaitAsync(TimeSpan.FromSeconds(2));

        Assert.Equal(2, inputPlaybackAdapter.PlayedEvents.Count, "Playback should continue after resume is requested.");
        Assert.Equal(AppState.Idle, applicationStateService.CurrentState, "Playback should return to Idle after resume completes.");
    }

    private static async Task MacroPlaybackService_StopOnErrorTrue_StopsImmediatelyAsync()
    {
        var inputPlaybackAdapter = new TestInputPlaybackAdapter();
        var applicationStateService = new ApplicationStateService();
        var playbackService = new MacroPlaybackService(inputPlaybackAdapter, inputPlaybackAdapter, applicationStateService);
        List<AppState> observedStates = [];

        applicationStateService.StateChanged += observedStates.Add;

        MacroSession session = CreatePlaybackSession();
        inputPlaybackAdapter.FailOnEventIds.Add(session.Events[0].Id);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => playbackService.PlayAsync(session, new PlaybackSettings { StopOnError = true }),
            "Playback should stop on the first adapter failure when StopOnError is enabled.");

        Assert.Equal(1, inputPlaybackAdapter.AttemptedEventIds.Count, "Only the failing event should be attempted when StopOnError is enabled.");
        Assert.Equal(
            new[] { AppState.Playing, AppState.Error, AppState.Idle },
            observedStates,
            "Playback failures should surface through Error before returning to Idle.");
        Assert.Equal(AppState.Idle, applicationStateService.CurrentState, "Playback should finish in Idle after cleanup.");
    }

    private static async Task MacroPlaybackService_StopOnErrorFalse_ContinuesAndReportsAsync()
    {
        var inputPlaybackAdapter = new TestInputPlaybackAdapter();
        var applicationStateService = new ApplicationStateService();
        var playbackService = new MacroPlaybackService(inputPlaybackAdapter, inputPlaybackAdapter, applicationStateService);
        List<AppState> observedStates = [];

        applicationStateService.StateChanged += observedStates.Add;

        MacroSession session = CreatePlaybackSession();
        inputPlaybackAdapter.FailOnEventIds.Add(session.Events[0].Id);

        await Assert.ThrowsAsync<AggregateException>(
            () => playbackService.PlayAsync(session, new PlaybackSettings { StopOnError = false }),
            "Playback should continue past event failures and report them as an aggregate error when StopOnError is disabled.");

        Assert.Equal(2, inputPlaybackAdapter.AttemptedEventIds.Count, "Playback should continue to later events when StopOnError is disabled.");
        Assert.Equal(1, inputPlaybackAdapter.SuccessfulEventIds.Count, "Only successful events should be counted as completed.");
        Assert.Equal(
            new[] { AppState.Playing, AppState.Error, AppState.Idle },
            observedStates,
            "Playback failures should still transition through Error before returning to Idle.");
        Assert.Equal(AppState.Idle, applicationStateService.CurrentState, "Playback should finish in Idle after cleanup.");
    }

    private static MacroSession CreatePlaybackSession()
    {
        return new MacroSession
        {
            Name = "Playback",
            Events =
            {
                new MacroEvent
                {
                    Id = Guid.NewGuid(),
                    EventType = MacroEventType.Keyboard,
                    KeyboardActionType = KeyboardActionType.KeyDown,
                    KeyCode = 0x41,
                    ScanCode = 0x1E,
                    Description = "A down"
                },
                new MacroEvent
                {
                    Id = Guid.NewGuid(),
                    EventType = MacroEventType.Keyboard,
                    KeyboardActionType = KeyboardActionType.KeyUp,
                    KeyCode = 0x41,
                    ScanCode = 0x1E,
                    Description = "A up"
                }
            }
        };
    }

    private static Task<bool> RunOnStaThreadAsync(Action action)
    {
        var taskCompletionSource = new TaskCompletionSource<bool>(
            TaskCreationOptions.RunContinuationsAsynchronously);

        Thread staThread = new(() =>
        {
            try
            {
                action();
                taskCompletionSource.SetResult(true);
            }
            catch (Exception ex)
            {
                taskCompletionSource.SetException(ex);
            }
        });

        staThread.SetApartmentState(ApartmentState.STA);
        staThread.IsBackground = true;
        staThread.Start();

        return taskCompletionSource.Task;
    }

    private static IEnumerable<TControl> GetDescendants<TControl>(Control root)
        where TControl : Control
    {
        foreach (Control child in root.Controls)
        {
            if (child is TControl typedChild)
            {
                yield return typedChild;
            }

            foreach (TControl descendant in GetDescendants<TControl>(child))
            {
                yield return descendant;
            }
        }
    }

    private static Rectangle GetBoundsRelativeToAncestor(Control control, Control ancestor)
    {
        int left = control.Left;
        int top = control.Top;
        Control? current = control.Parent;

        while (current is not null && current != ancestor)
        {
            left += current.Left;
            top += current.Top;
            current = current.Parent;
        }

        if (current != ancestor)
        {
            throw new InvalidOperationException("The requested control is not inside the expected ancestor.");
        }

        return new Rectangle(left, top, control.Width, control.Height);
    }

    private static string DescribeControl(Control control)
    {
        string controlText = string.IsNullOrWhiteSpace(control.Text)
            ? control.GetType().Name
            : control.Text;

        return $"{control.GetType().Name} '{controlText}'";
    }

    private static IntPtr InvokeHookCallback(
        object hookSource,
        int nCode,
        int wParam,
        IntPtr hookDataPointer)
    {
        MethodInfo callbackMethod = hookSource.GetType().GetMethod(
            "HookCallback",
            BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("Hook callback methodi bulunamadi.");

        object? result = callbackMethod.Invoke(
            hookSource,
            [nCode, (IntPtr)wParam, hookDataPointer]);

        return result is IntPtr pointer
            ? pointer
            : IntPtr.Zero;
    }

    private static string CreateTempDirectory()
    {
        string path = Path.Combine(
            Path.GetTempPath(),
            "MacroMaster.Tests",
            Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(path);
        return path;
    }

    private static void DeleteDirectoryIfExists(string path)
    {
        if (Directory.Exists(path))
        {
            Directory.Delete(path, recursive: true);
        }
    }
}

internal static class Expect
{
    public static void True(bool condition, string message)
    {
        Xunit.Assert.True(condition, message);
    }

    public static void False(bool condition, string message)
    {
        Xunit.Assert.False(condition, message);
    }

    public static void Equal<T>(T expected, T actual, string message)
    {
        _ = message;
        Xunit.Assert.Equal(expected, actual);
    }

    public static void Equal<T>(IEnumerable<T> expected, IEnumerable<T> actual, string message)
    {
        _ = message;
        Xunit.Assert.Equal(expected, actual);
    }

    public static void Throws<TException>(Action action, string message)
        where TException : Exception
    {
        _ = message;
        Xunit.Assert.Throws<TException>(action);
    }

    public static async Task ThrowsAsync<TException>(Func<Task> action, string message)
        where TException : Exception
    {
        _ = message;
        await Xunit.Assert.ThrowsAsync<TException>(action);
    }
}

internal sealed class TestKeyboardHookSource : IKeyboardHookSource
{
    public bool IsRunning { get; private set; }

    public event Action<KeyboardActivityInfo>? KeyActivityReceived;

    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        IsRunning = true;
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        IsRunning = false;
        return Task.CompletedTask;
    }

    public void Emit(KeyboardActivityInfo keyboardActivity)
    {
        KeyActivityReceived?.Invoke(keyboardActivity);
    }
}

internal sealed class TestMouseHookSource : IMouseHookSource
{
    public bool IsRunning { get; private set; }

    public event Action<MouseActionType, int?, int?, int?>? MouseActivityReceived;

    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        IsRunning = true;
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        IsRunning = false;
        return Task.CompletedTask;
    }

    public void Emit(MouseActionType mouseActionType, int? x, int? y, int? wheelDelta)
    {
        MouseActivityReceived?.Invoke(mouseActionType, x, y, wheelDelta);
    }
}

internal sealed class TestInputPlaybackAdapter : IInputPlaybackAdapter, ICursorPositionProvider
{
    public CursorPosition CurrentCursorPosition { get; set; } = new(0, 0);

    public int CursorPositionReadCount { get; private set; }

    public Queue<CursorPosition> CursorPositions { get; } = new();

    public List<Guid> AttemptedEventIds { get; } = [];

    public List<Guid> SuccessfulEventIds { get; } = [];

    public HashSet<Guid> FailOnEventIds { get; } = [];

    public List<MacroEvent> PlayedEvents { get; } = [];

    public Task<CursorPosition> GetCursorPositionAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        CursorPositionReadCount++;
        CursorPosition cursorPosition = CursorPositions.Count > 0
            ? CursorPositions.Dequeue()
            : CurrentCursorPosition;
        return Task.FromResult(cursorPosition);
    }

    public Task PlayEventAsync(MacroEvent macroEvent, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        AttemptedEventIds.Add(macroEvent.Id);

        if (FailOnEventIds.Contains(macroEvent.Id))
        {
            throw new InvalidOperationException(
                $"Simulated playback failure for event {macroEvent.Description}.");
        }

        SuccessfulEventIds.Add(macroEvent.Id);
        PlayedEvents.Add(CloneEvent(macroEvent));
        return Task.CompletedTask;
    }

    private static MacroEvent CloneEvent(MacroEvent source)
    {
        return new MacroEvent
        {
            Id = source.Id,
            EventType = source.EventType,
            KeyboardActionType = source.KeyboardActionType,
            MouseActionType = source.MouseActionType,
            DelayMs = source.DelayMs,
            TimestampUtc = source.TimestampUtc,
            KeyCode = source.KeyCode,
            ScanCode = source.ScanCode,
            IsExtendedKey = source.IsExtendedKey,
            KeyName = source.KeyName,
            X = source.X,
            Y = source.Y,
            WheelDelta = source.WheelDelta,
            Description = source.Description
        };
    }
}

internal sealed class TestHotkeyConfiguration : IHotkeyConfiguration
{
    public TestHotkeyConfiguration(
        HotkeyBinding recordToggleHotkey,
        HotkeyBinding playbackToggleHotkey,
        HotkeyBinding stopHotkey,
        HotkeyBinding? hotkeySettingsHotkey = null)
    {
        RecordToggleHotkey = recordToggleHotkey;
        PlaybackToggleHotkey = playbackToggleHotkey;
        StopHotkey = stopHotkey;
        HotkeySettingsHotkey = hotkeySettingsHotkey ?? HotkeySettings.DefaultHotkeySettingsHotkey;
    }

    public HotkeyBinding RecordToggleHotkey { get; }

    public HotkeyBinding PlaybackToggleHotkey { get; }

    public HotkeyBinding StopHotkey { get; }

    public HotkeyBinding HotkeySettingsHotkey { get; }
}

internal sealed class RecordingTestLogger : IAppLogger
{
    public List<RecordedLogEntry> Entries { get; } = [];

    public void Log(
        AppLogLevel logLevel,
        string source,
        string message,
        Exception? exception = null)
    {
        Entries.Add(new RecordedLogEntry(logLevel, source, message, exception));
    }
}

internal readonly record struct RecordedLogEntry(
    AppLogLevel LogLevel,
    string Source,
    string Message,
    Exception? Exception);

[StructLayout(LayoutKind.Sequential)]
internal struct TestKeyboardHookStruct
{
    public uint vkCode;
    public uint scanCode;
    public uint flags;
    public uint time;
    public IntPtr dwExtraInfo;
}

[StructLayout(LayoutKind.Sequential)]
internal struct TestPoint
{
    public int x;
    public int y;
}

[StructLayout(LayoutKind.Sequential)]
internal struct TestMouseHookStruct
{
    public TestPoint pt;
    public uint mouseData;
    public uint flags;
    public uint time;
    public IntPtr dwExtraInfo;
}
