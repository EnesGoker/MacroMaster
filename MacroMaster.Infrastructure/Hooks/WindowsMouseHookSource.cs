using System.Diagnostics;
using System.Runtime.InteropServices;
using MacroMaster.Application.Abstractions;
using MacroMaster.Domain.Enums;
using MacroMaster.Infrastructure.Interop;

namespace MacroMaster.Infrastructure.Hooks;

public sealed class WindowsMouseHookSource : IMouseHookSource, IDisposable
{
    private readonly object _syncRoot = new();

    private NativeMethods.HookProc? _hookProc;
    private IntPtr _hookHandle = IntPtr.Zero;
    private bool _disposed;

    public bool IsRunning => _hookHandle != IntPtr.Zero;

    public event Action<MouseActionType, int?, int?, int?>? MouseActivityReceived;

    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        if (!OperatingSystem.IsWindows())
        {
            throw new PlatformNotSupportedException(
                "WindowsMouseHookSource yalnızca Windows ortamında çalışır.");
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
                ThrowWin32Exception("Mouse hook kaldırılamadı.");
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
            NativeMethods.WhMouseLl,
            hookProc,
            moduleHandle,
            0);

        if (hookHandle == IntPtr.Zero)
        {
            ThrowWin32Exception("Global mouse hook kurulamadı.");
        }

        return hookHandle;
    }

    private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0)
        {
            int message = unchecked((int)wParam.ToInt64());

            NativeMethods.MSLLHOOKSTRUCT hookData =
                Marshal.PtrToStructure<NativeMethods.MSLLHOOKSTRUCT>(lParam);

            MouseActionType actionType = message switch
            {
                NativeMethods.WmMouseMove => MouseActionType.Move,
                NativeMethods.WmLButtonDown => MouseActionType.LeftDown,
                NativeMethods.WmLButtonUp => MouseActionType.LeftUp,
                NativeMethods.WmRButtonDown => MouseActionType.RightDown,
                NativeMethods.WmRButtonUp => MouseActionType.RightUp,
                NativeMethods.WmMButtonDown => MouseActionType.MiddleDown,
                NativeMethods.WmMButtonUp => MouseActionType.MiddleUp,
                NativeMethods.WmMouseWheel => MouseActionType.Wheel,
                NativeMethods.WmLButtonDblClk => MouseActionType.DoubleClick,
                _ => MouseActionType.None
            };

            if (actionType != MouseActionType.None)
            {
                int? wheelDelta = actionType == MouseActionType.Wheel
                    ? ExtractWheelDelta(hookData.mouseData)
                    : null;

                MouseActivityReceived?.Invoke(
                    actionType,
                    hookData.pt.x,
                    hookData.pt.y,
                    wheelDelta);
            }
        }

        return NativeMethods.CallNextHookEx(_hookHandle, nCode, wParam, lParam);
    }

    private static int ExtractWheelDelta(uint mouseData)
    {
        short highWord = unchecked((short)((mouseData >> 16) & 0xffff));
        return highWord;
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