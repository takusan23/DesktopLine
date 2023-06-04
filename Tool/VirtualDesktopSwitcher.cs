using System;
using System.Runtime.InteropServices;

namespace DesktopLine.Tool
{
    class VirtualDesktopSwitcher
    {

        /// <summary>
        /// 切り替える仮想デスクトップの方向
        /// </summary>
        public enum Direction { Left, Right }


        /// <summary>
        /// 仮想デスクトップ切り替えのショートカットキーを押す
        /// </summary>
        /// <param name="direction">方向。矢印キー</param>
        public static void sendSwitchKeyEvent(Direction direction)
        {
            short keyCode = (short)(direction == Direction.Left ? WindowsApiTool.VK_LEFT : WindowsApiTool.VK_RIGHT);
            var inputs = new WindowsApiTool.Input[6];

            // Windowsキーを押す
            inputs[0] = new WindowsApiTool.Input();
            inputs[0].Type = 1;
            inputs[0].ui.Keyboard.VirtualKey = WindowsApiTool.VK_LWIN;
            inputs[0].ui.Keyboard.ScanCode = (short)WindowsApiTool.MapVirtualKey(WindowsApiTool.VK_LWIN, 0);
            inputs[0].ui.Keyboard.Flags = WindowsApiTool.KEYEVENTF_KEYDOWN;
            inputs[0].ui.Keyboard.Time = 0;
            inputs[0].ui.Keyboard.ExtraInfo = IntPtr.Zero;

            // Ctrlキーを押す
            inputs[1] = new WindowsApiTool.Input();
            inputs[1].Type = 1;
            inputs[1].ui.Keyboard.VirtualKey = WindowsApiTool.VK_CONTROL;
            inputs[1].ui.Keyboard.ScanCode = (short)WindowsApiTool.MapVirtualKey(WindowsApiTool.VK_CONTROL, 0);
            inputs[1].ui.Keyboard.Flags = WindowsApiTool.KEYEVENTF_KEYDOWN;
            inputs[1].ui.Keyboard.Time = 0;
            inputs[1].ui.Keyboard.ExtraInfo = IntPtr.Zero;

            // 矢印キーを押す
            inputs[2] = new WindowsApiTool.Input();
            inputs[2].Type = 1;
            inputs[2].ui.Keyboard.VirtualKey = keyCode;
            inputs[2].ui.Keyboard.ScanCode = (short)WindowsApiTool.MapVirtualKey(keyCode, 0);
            inputs[2].ui.Keyboard.Flags = WindowsApiTool.KEYEVENTF_KEYDOWN;
            inputs[2].ui.Keyboard.Time = 0;
            inputs[2].ui.Keyboard.ExtraInfo = IntPtr.Zero;

            // Windowsキーを離す
            inputs[3] = new WindowsApiTool.Input();
            inputs[3].Type = 1;
            inputs[3].ui.Keyboard.VirtualKey = WindowsApiTool.VK_LWIN;
            inputs[3].ui.Keyboard.ScanCode = (short)WindowsApiTool.MapVirtualKey(WindowsApiTool.VK_LWIN, 0);
            inputs[3].ui.Keyboard.Flags = WindowsApiTool.KEYEVENTF_KEYUP;
            inputs[3].ui.Keyboard.Time = 0;
            inputs[3].ui.Keyboard.ExtraInfo = IntPtr.Zero;

            // Ctrlキーを離す
            inputs[4] = new WindowsApiTool.Input();
            inputs[4].Type = 1;
            inputs[4].ui.Keyboard.VirtualKey = WindowsApiTool.VK_CONTROL;
            inputs[4].ui.Keyboard.ScanCode = (short)WindowsApiTool.MapVirtualKey(WindowsApiTool.VK_CONTROL, 0);
            inputs[4].ui.Keyboard.Flags = WindowsApiTool.KEYEVENTF_KEYUP;
            inputs[4].ui.Keyboard.Time = 0;
            inputs[4].ui.Keyboard.ExtraInfo = IntPtr.Zero;

            // 矢印キーを離す
            inputs[5] = new WindowsApiTool.Input();
            inputs[5].Type = 1;
            inputs[5].ui.Keyboard.VirtualKey = keyCode;
            inputs[5].ui.Keyboard.ScanCode = (short)WindowsApiTool.MapVirtualKey(keyCode, 0);
            inputs[5].ui.Keyboard.Flags = WindowsApiTool.KEYEVENTF_KEYUP;
            inputs[5].ui.Keyboard.Time = 0;
            inputs[5].ui.Keyboard.ExtraInfo = IntPtr.Zero;

            WindowsApiTool.SendInput(inputs.Length, inputs, Marshal.SizeOf(inputs[0]));
        }
    }
}
