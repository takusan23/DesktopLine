using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace DesktopLine.Tool
{
    /// <summary>
    /// キーボードイベントを横取りする SetWindowsHookEx を使う
    /// もし暴走したら Ctrl + Alt + Delete で解除されるはず
    /// </summary>
    public class KeybordHookTool
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
        /// キー入力をフックしたときに呼ばれる。
        /// パラメータの KeyState は enum 参照。int はキーコードです。
        /// 返り値は true の場合はキーイベントを送らない、false の場合はキーイベントを送る（デフォルト操作）。
        /// </summary>
        public Func<KeyState, int, bool> onKeybordHookEvent = null;

        public KeybordHookTool()
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
            var keyCode = (int)key.vkCode;

            // 自分で SendInput した場合は return CallNextHookEx にする
            var sendInputFromThisApp = key.dwExtraInfo == ShortcutInputTool.SNED_INPUT_EXTRA_INFO;
            if (sendInputFromThisApp)
            {
                return WindowsApiTool.CallNextHookEx(hookId, nCode, wParam, lParam);
            }

            // 呼び出す
            var keyState = ((int)wParam == WindowsApiTool.WM_KEYUP) ? KeyState.UP : KeyState.DOWN;
            var isComsumeEvent = onKeybordHookEvent(keyState, keyCode);
            // キーイベントを送らない場合 true
            return isComsumeEvent ? new IntPtr(1) : WindowsApiTool.CallNextHookEx(hookId, nCode, wParam, lParam);
        }

        /// <summary>
        /// キーの状態
        /// </summary>
        public enum KeyState
        {
            /// <summary>
            /// 押している
            /// </summary>
            DOWN,

            /// <summary>
            /// 離した
            /// </summary>
            UP
        }

    }
}
