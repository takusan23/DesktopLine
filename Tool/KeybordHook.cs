using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace DesktopLine.Tool
{
    /// <summary>
    /// キーボードイベントを横取りする SetWindowsHookEx を使う
    /// もし暴走したら Ctrl + Alt + Delete で解除されるはず
    /// </summary>
    public class KeybordHook
    {
        /// <summary>
        /// GC対策
        /// </summary>
        private WindowsApiTool.HookProc keybordHookProc = null;

        /// <summary>
        /// Hook解除用
        /// </summary>
        private IntPtr hookId = IntPtr.Zero;

        /// <summary>
        /// キーボードを押したら呼ばれる
        /// </summary>
        public Action<int> onKeyDown;

        /// <summary>
        /// キーボードを離したら呼ばれる
        /// </summary>
        public Action<int> onKeyUp;

        public KeybordHook()
        {
            keybordHookProc = HookProc;
            hookId = WindowsApiTool.SetWindowsHookEx(WindowsApiTool.WH_KEYBOARD_LL, keybordHookProc, WindowsApiTool.GetModuleHandle(Process.GetCurrentProcess().MainModule.ModuleName), 0);
        }

        /// <summary>
        /// アプリケーション終了時に呼んでください。フックを解除します。
        /// </summary>
        public void UnhookWindowsHookEx() => WindowsApiTool.UnhookWindowsHookEx(hookId);

        private IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            var key = (WindowsApiTool.KBDLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(WindowsApiTool.KBDLLHOOKSTRUCT));
            switch ((int)wParam)
            {
                // キーを押したとき
                case WindowsApiTool.WM_KEYDOWN:
                    onKeyDown?.Invoke((int)key.vkCode);
                    break;

                // キーを離したとき
                case WindowsApiTool.WM_KEYUP:
                    onKeyUp?.Invoke((int)key.vkCode);
                    break;
            }
            return WindowsApiTool.CallNextHookEx(hookId, nCode, wParam, lParam);
        }
    }
}
