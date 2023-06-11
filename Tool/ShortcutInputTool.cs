using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace DesktopLine.Tool
{
    class ShortcutInputTool
    {

        /// <summary>
        /// 切り替える仮想デスクトップの方向
        /// </summary>
        public enum Direction { Left, Right }

        /// <summary>
        /// SetWindowHookExでクリックイベントを拾ってしまうので、関数でクリックイベントを発生させた場合はこの値を ExtraInfo に詰めてます。
        /// </summary>
        public static IntPtr SNED_INPUT_EXTRA_INFO = new(0607); // 2023/06/07

        /// <summary>
        /// 矢印キーを押す
        /// </summary>
        /// <param name="direction">方向。矢印キー</param>
        public static void SendArrowKeyInput(Direction direction)
        {
            short keyCode = (short)(direction == Direction.Left ? WindowsApiTool.VK_LEFT : WindowsApiTool.VK_RIGHT);
            SendMultipleKeyInput(keyCode);
        }

        /// <summary>
        /// 指定したキーを押す
        /// </summary>
        public static void SendMultipleKeyInput(params int[] keys)
        {
            // DOWN
            var keyDownList = keys.Select((key, index) =>
              {
                  var input = new WindowsApiTool.Input();
                  input.Type = 1;
                  input.ui.Keyboard.VirtualKey = (short)key;
                  input.ui.Keyboard.ScanCode = (short)WindowsApiTool.MapVirtualKey(key, 0);
                  input.ui.Keyboard.Flags = WindowsApiTool.KEYEVENTF_KEYDOWN;
                  input.ui.Keyboard.Time = 0;
                  input.ui.Keyboard.ExtraInfo = SNED_INPUT_EXTRA_INFO;
                  return input;
              });

            // UP
            var keyUpList = keys.Select((key, index) =>
            {
                var input = new WindowsApiTool.Input();
                input.Type = 1;
                input.ui.Keyboard.VirtualKey = (short)key;
                input.ui.Keyboard.ScanCode = (short)WindowsApiTool.MapVirtualKey(key, 0);
                input.ui.Keyboard.Flags = WindowsApiTool.KEYEVENTF_KEYUP;
                input.ui.Keyboard.Time = 0;
                input.ui.Keyboard.ExtraInfo = SNED_INPUT_EXTRA_INFO;
                return input;
            });

            var inputs = keyDownList.Concat(keyUpList).ToList();
            WindowsApiTool.SendInput(inputs.Count, inputs.ToArray(), Marshal.SizeOf(inputs[0]));
        }

    }
}
