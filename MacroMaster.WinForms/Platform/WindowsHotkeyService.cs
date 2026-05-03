using System.Runtime.InteropServices;
using MacroMaster.Application.Abstractions;
using MacroMaster.Application.Services;

namespace MacroMaster.WinForms.Platform;

public sealed class WindowsHotkeyService : IHotkeyService, IDisposable
{
    private const int RecordToggleHotkeyId = 1001;
    private const int PlaybackToggleHotkeyId = 1002;
    private const int StopHotkeyId = 1003;
    private const int HotkeySettingsHotkeyId = 1004;

    private const uint ModNone = 0x0000;
    private const uint ModAlt = 0x0001;
    private const uint ModControl = 0x0002;
    private const uint ModShift = 0x0004;
    private const uint ModWin = 0x0008;

    private readonly HotkeyMessageWindow _messageWindow;
    private readonly IHotkeyConfiguration _hotkeyConfiguration;
    private readonly IAppLogger _logger;
    private bool _disposed;

    public WindowsHotkeyService(
        IHotkeyConfiguration hotkeyConfiguration,
        IAppLogger? logger = null)
    {
        _hotkeyConfiguration = hotkeyConfiguration;
        _logger = logger ?? NullAppLogger.Instance;
        _messageWindow = new HotkeyMessageWindow(this);
    }

    public bool IsRegistered { get; private set; }

    public event Action? RecordToggleRequested;
    public event Action? PlaybackToggleRequested;
    public event Action? StopRequested;
    public event Action? HotkeySettingsRequested;

    public Task RegisterAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        if (!OperatingSystem.IsWindows())
        {
            PlatformNotSupportedException exception = new(
                "WindowsHotkeyService yalnizca Windows ortaminda calisir.");
            _logger.Log(
                AppLogLevel.Error,
                nameof(WindowsHotkeyService),
                "Global kisayol servisi Windows disindaki bir ortamda baslatilamadi.",
                exception);
            throw exception;
        }

        if (IsRegistered)
        {
            return Task.CompletedTask;
        }

        _messageWindow.EnsureHandleCreated();

        List<int> registeredHotkeyIds = [];

        try
        {
            RegisterSingleHotkey(
                RecordToggleHotkeyId,
                MapModifiers(_hotkeyConfiguration.RecordToggleHotkey.Modifiers),
                unchecked((uint)_hotkeyConfiguration.RecordToggleHotkey.VirtualKeyCode));
            registeredHotkeyIds.Add(RecordToggleHotkeyId);

            RegisterSingleHotkey(
                PlaybackToggleHotkeyId,
                MapModifiers(_hotkeyConfiguration.PlaybackToggleHotkey.Modifiers),
                unchecked((uint)_hotkeyConfiguration.PlaybackToggleHotkey.VirtualKeyCode));
            registeredHotkeyIds.Add(PlaybackToggleHotkeyId);

            RegisterSingleHotkey(
                StopHotkeyId,
                MapModifiers(_hotkeyConfiguration.StopHotkey.Modifiers),
                unchecked((uint)_hotkeyConfiguration.StopHotkey.VirtualKeyCode));
            registeredHotkeyIds.Add(StopHotkeyId);

            RegisterSingleHotkey(
                HotkeySettingsHotkeyId,
                MapModifiers(_hotkeyConfiguration.HotkeySettingsHotkey.Modifiers),
                unchecked((uint)_hotkeyConfiguration.HotkeySettingsHotkey.VirtualKeyCode));
            registeredHotkeyIds.Add(HotkeySettingsHotkeyId);
        }
        catch
        {
            for (int index = registeredHotkeyIds.Count - 1; index >= 0; index--)
            {
                UnregisterSingleHotkey(registeredHotkeyIds[index]);
            }

            IsRegistered = false;
            throw;
        }

        IsRegistered = true;
        _logger.Log(
            AppLogLevel.Information,
            nameof(WindowsHotkeyService),
            "Global kisayollar basariyla kaydedildi.");
        return Task.CompletedTask;
    }

    public Task UnregisterAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        UnregisterCore();
        return Task.CompletedTask;
    }

    private void UnregisterCore()
    {
        if (!IsRegistered)
        {
            return;
        }

        UnregisterSingleHotkey(RecordToggleHotkeyId);
        UnregisterSingleHotkey(PlaybackToggleHotkeyId);
        UnregisterSingleHotkey(StopHotkeyId);
        UnregisterSingleHotkey(HotkeySettingsHotkeyId);

        IsRegistered = false;
        _logger.Log(
            AppLogLevel.Information,
            nameof(WindowsHotkeyService),
            "Global kisayollar kaldirildi.");
    }

    internal void HandleHotkeyPressed(int hotkeyId)
    {
        switch (hotkeyId)
        {
            case RecordToggleHotkeyId:
                RecordToggleRequested?.Invoke();
                break;

            case PlaybackToggleHotkeyId:
                PlaybackToggleRequested?.Invoke();
                break;

            case StopHotkeyId:
                StopRequested?.Invoke();
                break;

            case HotkeySettingsHotkeyId:
                HotkeySettingsRequested?.Invoke();
                break;
        }
    }

    private void RegisterSingleHotkey(int id, uint modifiers, uint virtualKey)
    {
        bool result = HotkeyNativeMethods.RegisterHotKey(
            _messageWindow.Handle,
            id,
            modifiers,
            virtualKey);

        if (!result)
        {
            ThrowWin32Exception($"Kisayol kaydedilemedi. Kimlik: {id}.");
        }
    }

    private void UnregisterSingleHotkey(int id)
    {
        _ = HotkeyNativeMethods.UnregisterHotKey(_messageWindow.Handle, id);
    }

    private static uint MapModifiers(HotkeyModifiers modifiers)
    {
        uint nativeModifiers = ModNone;

        if (modifiers.HasFlag(HotkeyModifiers.Alt))
        {
            nativeModifiers |= ModAlt;
        }

        if (modifiers.HasFlag(HotkeyModifiers.Control))
        {
            nativeModifiers |= ModControl;
        }

        if (modifiers.HasFlag(HotkeyModifiers.Shift))
        {
            nativeModifiers |= ModShift;
        }

        if (modifiers.HasFlag(HotkeyModifiers.Windows))
        {
            nativeModifiers |= ModWin;
        }

        return nativeModifiers;
    }

    private void ThrowWin32Exception(string message)
    {
        int errorCode = Marshal.GetLastWin32Error();
        InvalidOperationException exception = new(
            FormattableString.Invariant($"{message} Win32 hata kodu: {errorCode}"));
        _logger.Log(
            AppLogLevel.Error,
            nameof(WindowsHotkeyService),
            message,
            exception);
        throw exception;
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        try
        {
            UnregisterCore();
        }
        catch
        {
            // no-op
        }

        _messageWindow.Release();
        _disposed = true;
        GC.SuppressFinalize(this);
    }

    private sealed class HotkeyMessageWindow : NativeWindow
    {
        private const int WmHotkey = 0x0312;

        private readonly WindowsHotkeyService _owner;

        public HotkeyMessageWindow(WindowsHotkeyService owner)
        {
            _owner = owner;
        }

        public void EnsureHandleCreated()
        {
            if (Handle != IntPtr.Zero)
            {
                return;
            }

            CreateHandle(new CreateParams
            {
                Caption = "MacroMasterHotkeyMessageWindow"
            });
        }

        public void Release()
        {
            if (Handle != IntPtr.Zero)
            {
                DestroyHandle();
            }
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WmHotkey)
            {
                _owner.HandleHotkeyPressed(m.WParam.ToInt32());
            }

            base.WndProc(ref m);
        }
    }

    private static class HotkeyNativeMethods
    {
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool RegisterHotKey(
            IntPtr hWnd,
            int id,
            uint fsModifiers,
            uint vk);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool UnregisterHotKey(
            IntPtr hWnd,
            int id);
    }
}
