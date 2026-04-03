using MacroMaster.Application.Abstractions;
using System.Runtime.InteropServices;

namespace MacroMaster.WinForms.Platform;

public sealed class WindowsHotkeyService : IHotkeyService, IDisposable
{
    private const int RecordToggleHotkeyId = 1001;
    private const int PlaybackToggleHotkeyId = 1002;
    private const int StopHotkeyId = 1003;

    private const uint ModNone = 0x0000;

    private const uint VkF8 = 0x77;
    private const uint VkF9 = 0x78;
    private const uint VkF10 = 0x79;

    private readonly HotkeyMessageWindow _messageWindow;
    private bool _disposed;

    public WindowsHotkeyService()
    {
        _messageWindow = new HotkeyMessageWindow(this);
    }

    public bool IsRegistered { get; private set; }

    public event Action? RecordToggleRequested;
    public event Action? PlaybackToggleRequested;
    public event Action? StopRequested;

    public Task RegisterAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        if (!OperatingSystem.IsWindows())
        {
            throw new PlatformNotSupportedException(
                "WindowsHotkeyService yalnızca Windows ortamında çalışır.");
        }

        if (IsRegistered)
        {
            return Task.CompletedTask;
        }

        _messageWindow.EnsureHandleCreated();

        RegisterSingleHotkey(RecordToggleHotkeyId, ModNone, VkF8);
        RegisterSingleHotkey(PlaybackToggleHotkeyId, ModNone, VkF9);
        RegisterSingleHotkey(StopHotkeyId, ModNone, VkF10);

        IsRegistered = true;
        return Task.CompletedTask;
    }

    public Task UnregisterAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!IsRegistered)
        {
            return Task.CompletedTask;
        }

        UnregisterSingleHotkey(RecordToggleHotkeyId);
        UnregisterSingleHotkey(PlaybackToggleHotkeyId);
        UnregisterSingleHotkey(StopHotkeyId);

        IsRegistered = false;
        return Task.CompletedTask;
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
            ThrowWin32Exception($"Hotkey kaydı başarısız oldu. Id: {id}");
        }
    }

    private void UnregisterSingleHotkey(int id)
    {
        _ = HotkeyNativeMethods.UnregisterHotKey(_messageWindow.Handle, id);
    }

    private static void ThrowWin32Exception(string message)
    {
        int errorCode = Marshal.GetLastWin32Error();
        throw new InvalidOperationException($"{message} Win32 Error Code: {errorCode}");
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
            UnregisterAsync().GetAwaiter().GetResult();
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