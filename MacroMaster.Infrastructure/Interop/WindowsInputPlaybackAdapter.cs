using System.Runtime.InteropServices;
using MacroMaster.Application.Abstractions;
using MacroMaster.Domain.Enums;
using MacroMaster.Domain.Models;

namespace MacroMaster.Infrastructure.Interop;

public sealed class WindowsInputPlaybackAdapter : IInputPlaybackAdapter
{
    public Task PlayEventAsync(MacroEvent macroEvent, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(macroEvent);
        cancellationToken.ThrowIfCancellationRequested();

        if (!OperatingSystem.IsWindows())
        {
            throw new PlatformNotSupportedException(
                "WindowsInputPlaybackAdapter yalnızca Windows ortamında çalışır.");
        }

        return macroEvent.EventType switch
        {
            MacroEventType.Keyboard => PlayKeyboardEventAsync(macroEvent, cancellationToken),
            MacroEventType.Mouse => PlayMouseEventAsync(macroEvent, cancellationToken),
            _ => Task.CompletedTask
        };
    }

    private static Task PlayKeyboardEventAsync(
        MacroEvent macroEvent,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!macroEvent.KeyCode.HasValue)
        {
            throw new InvalidOperationException(
                "Keyboard event oynatılırken KeyCode boş olamaz.");
        }

        bool isKeyUp = macroEvent.KeyboardActionType == KeyboardActionType.KeyUp;

        SendKeyboardInput((ushort)macroEvent.KeyCode.Value, isKeyUp);

        return Task.CompletedTask;
    }

    private static Task PlayMouseEventAsync(
        MacroEvent macroEvent,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        switch (macroEvent.MouseActionType)
        {
            case MouseActionType.Move:
                MoveCursor(macroEvent.X, macroEvent.Y);
                break;

            case MouseActionType.LeftDown:
                SendMouseInput(NativeMethods.MouseEventFLeftDown);
                break;

            case MouseActionType.LeftUp:
                SendMouseInput(NativeMethods.MouseEventFLeftUp);
                break;

            case MouseActionType.RightDown:
                SendMouseInput(NativeMethods.MouseEventFRightDown);
                break;

            case MouseActionType.RightUp:
                SendMouseInput(NativeMethods.MouseEventFRightUp);
                break;

            case MouseActionType.MiddleDown:
                SendMouseInput(NativeMethods.MouseEventFMiddleDown);
                break;

            case MouseActionType.MiddleUp:
                SendMouseInput(NativeMethods.MouseEventFMiddleUp);
                break;

            case MouseActionType.Wheel:
                SendMouseInput(
                    NativeMethods.MouseEventFWheel,
                    unchecked((uint)(macroEvent.WheelDelta ?? 0)));
                break;

            case MouseActionType.DoubleClick:
                SendMouseInput(NativeMethods.MouseEventFLeftDown);
                SendMouseInput(NativeMethods.MouseEventFLeftUp);
                SendMouseInput(NativeMethods.MouseEventFLeftDown);
                SendMouseInput(NativeMethods.MouseEventFLeftUp);
                break;

            case MouseActionType.None:
            default:
                break;
        }

        return Task.CompletedTask;
    }

    private static void MoveCursor(int? x, int? y)
    {
        if (!x.HasValue || !y.HasValue)
        {
            throw new InvalidOperationException(
                "Mouse move event oynatılırken X ve Y boş olamaz.");
        }

        bool result = NativeMethods.SetCursorPos(x.Value, y.Value);

        if (!result)
        {
            ThrowWin32Exception("Fare imleci taşınamadı.");
        }
    }

    private static void SendKeyboardInput(ushort virtualKeyCode, bool isKeyUp)
    {
        NativeMethods.INPUT[] inputs =
        [
            new NativeMethods.INPUT
            {
                type = NativeMethods.InputKeyboard,
                U = new NativeMethods.InputUnion
                {
                    ki = new NativeMethods.KEYBDINPUT
                    {
                        wVk = virtualKeyCode,
                        wScan = 0,
                        dwFlags = isKeyUp ? NativeMethods.KeyEventFKeyUp : 0,
                        time = 0,
                        dwExtraInfo = IntPtr.Zero
                    }
                }
            }
        ];

        uint sent = NativeMethods.SendInput(
            (uint)inputs.Length,
            inputs,
            Marshal.SizeOf<NativeMethods.INPUT>());

        if (sent == 0)
        {
            ThrowWin32Exception("Klavye girdisi gönderilemedi.");
        }
    }

    private static void SendMouseInput(uint flags, uint mouseData = 0)
    {
        NativeMethods.INPUT[] inputs =
        [
            new NativeMethods.INPUT
            {
                type = NativeMethods.InputMouse,
                U = new NativeMethods.InputUnion
                {
                    mi = new NativeMethods.MOUSEINPUT
                    {
                        dx = 0,
                        dy = 0,
                        mouseData = mouseData,
                        dwFlags = flags,
                        time = 0,
                        dwExtraInfo = IntPtr.Zero
                    }
                }
            }
        ];

        uint sent = NativeMethods.SendInput(
            (uint)inputs.Length,
            inputs,
            Marshal.SizeOf<NativeMethods.INPUT>());

        if (sent == 0)
        {
            ThrowWin32Exception("Fare girdisi gönderilemedi.");
        }
    }

    private static void ThrowWin32Exception(string message)
    {
        int errorCode = Marshal.GetLastWin32Error();
        throw new InvalidOperationException($"{message} Win32 Error Code: {errorCode}");
    }
}