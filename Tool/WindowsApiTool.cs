using System;
using System.Runtime.InteropServices;

namespace DesktopLine.Tool
{
    class WindowsApiTool
    {
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int y, int cx, int cy, int uFlags);

        [DllImport("user32.dll", SetLastError = true)]
        public extern static void SendInput(int nInputs, Input[] pInputs, int cbsize);

        [DllImport("user32.dll", EntryPoint = "MapVirtualKeyA")]
        public extern static int MapVirtualKey(int wCode, int wMapType);

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MouseInput
        {
            public int X;
            public int Y;
            public int Data;
            public int Flags;
            public int Time;
            public IntPtr ExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct KeyboardInput
        {
            public short VirtualKey;
            public short ScanCode;
            public int Flags;
            public int Time;
            public IntPtr ExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct HardwareInput
        {
            public int uMsg;
            public short wParamL;
            public short wParamH;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Input
        {
            public int Type;
            public InputUnion ui;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct InputUnion
        {
            [FieldOffset(0)]
            public MouseInput Mouse;
            [FieldOffset(0)]
            public KeyboardInput Keyboard;
            [FieldOffset(0)]
            public HardwareInput Hardware;
        }

        public const int INPUT_MOUSE = 0;

        public const int INPUT_KEYBOARD = 1;

        public const int INPUT_HARDWARE = 2;

        public const int KEYEVENTF_KEYDOWN = 0x0;

        public const int KEYEVENTF_KEYUP = 0x2;

        public const int KEYEVENTF_EXTENDEDKEY = 0x1;

        public const int VK_CONTROL = 0x11;

        public const int VK_LWIN = 0x5B;

        public const int VK_LEFT = 0x25;

        public const int VK_RIGHT = 0x27;

        public const int SWP_NOSIZE = 0x0001;

        public const int SWP_NOMOVE = 0x0002;

        public const int HWND_TOPMOST = -1;

    }

}
