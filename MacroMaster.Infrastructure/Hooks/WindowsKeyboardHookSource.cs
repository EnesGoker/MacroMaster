using System.Diagnostics;
using System.Runtime.InteropServices;
using MacroMaster.Application.Abstractions;
using MacroMaster.Infrastructure.Interop;

namespace MacroMaster.Infrastructure.Hooks;

public sealed class WindowsKeyboardHookSource : IKeyboardHookSource, IDisposable
{
    private readonly object _syncRoot = new();

    private NativeMethods.HookProc? _hookProc;
    private IntPtr _hookHandle = IntPtr.Zero;
    private bool _disposed;

    public bool IsRunning => _hookHandle != IntPtr.Zero;

    public event Action<int, bool>? KeyActivityReceived;

    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        if (!OperatingSystem.IsWindows())
        {
            throw new PlatformNotSupportedException(
                "WindowsKeyboardHookSource yalnızca Windows ortamında çalışır.");
        }

        lock (_syncRoot)
        {
            if (IsRunning)
            {
                return Task.CompletedTask;
            }

            _hookProc = HookCallback;
            _hookHandle = InstallHook(_hookProc);
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_syncRoot)
        {
            if (!IsRunning)
            {
                return Task.CompletedTask;
            }

            if (!NativeMethods.UnhookWindowsHookEx(_hookHandle))
            {
                ThrowWin32Exception("Keyboard hook kaldırılamadı.");
            }

            _hookHandle = IntPtr.Zero;
            _hookProc = null;
        }

        return Task.CompletedTask;
    }

    private static IntPtr InstallHook(NativeMethods.HookProc hookProc)
    {
        using Process currentProcess = Process.GetCurrentProcess();
        ProcessModule? mainModule = currentProcess.MainModule;

        IntPtr moduleHandle = IntPtr.Zero;

        if (mainModule is not null)
        {
            moduleHandle = NativeMethods.GetModuleHandle(mainModule.ModuleName);
        }

        IntPtr hookHandle = NativeMethods.SetWindowsHookEx(
            NativeMethods.WhKeyboardLl,
            hookProc,
            moduleHandle,
            0);

        if (hookHandle == IntPtr.Zero)
        {
            ThrowWin32Exception("Global keyboard hook kurulamadı.");
        }

        return hookHandle;
    }

    private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0)
        {
            int message = unchecked((int)wParam.ToInt64());

            if (message is NativeMethods.WmKeyDown
                or NativeMethods.WmKeyUp
                or NativeMethods.WmSysKeyDown
                or NativeMethods.WmSysKeyUp)
            {
                NativeMethods.KBDLLHOOKSTRUCT hookData =
                    Marshal.PtrToStructure<NativeMethods.KBDLLHOOKSTRUCT>(lParam);

                bool isKeyDown = message is NativeMethods.WmKeyDown
                    or NativeMethods.WmSysKeyDown;

                KeyActivityReceived?.Invoke(unchecked((int)hookData.vkCode), isKeyDown);
            }
        }

        return NativeMethods.CallNextHookEx(_hookHandle, nCode, wParam, lParam);
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
            StopAsync().GetAwaiter().GetResult();
        }
        catch
        {
            // no-op
        }

        _disposed = true;
        GC.SuppressFinalize(this);
    }
}