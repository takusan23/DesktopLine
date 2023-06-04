using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace DesktopLine.Tool
{
    /// <summary>
    /// マウスのイベントを横取りする
    /// </summary>
    public class MouseHook
    {
        /// <summary>
        /// GC対策
        /// </summary>
        private WindowsApiTool.HookProc mouseHookProc = null;

        /// <summary>
        /// Hook解除用
        /// </summary>
        private IntPtr hookId = IntPtr.Zero;

        /// <summary>
        /// マウスを押したら呼ばれる
        /// </summary>
        public Action onMouseDown;

        /// <summary>
        /// マウスを離したら呼ばれる
        /// </summary>
        public Action onMouseUp;

        public MouseHook()
        {
            mouseHookProc = HookProc;
            hookId = WindowsApiTool.SetWindowsHookEx(WindowsApiTool.WH_MOUSE_LL, mouseHookProc, WindowsApiTool.GetModuleHandle(Process.GetCurrentProcess().MainModule.ModuleName), 0);
        }

        /// <summary>
        /// アプリケーション終了時に呼んでください。フックを解除します。
        /// </summary>
        public void UnhookWindowsHookEx() => WindowsApiTool.UnhookWindowsHookEx(hookId);

        private IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            var mouseHookStruct = (WindowsApiTool.MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(WindowsApiTool.MSLLHOOKSTRUCT));
            switch ((int)wParam)
            {
                case WindowsApiTool.WM_LBUTTONDOWN:
                    onMouseDown?.Invoke();
                    break;
                case WindowsApiTool.WM_LBUTTONUP:
                    onMouseUp?.Invoke();
                    break;
            }
            return WindowsApiTool.CallNextHookEx(hookId, nCode, wParam, lParam);
        }
    }
}
