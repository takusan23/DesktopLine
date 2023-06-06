using Microsoft.UI.Xaml;
using Microsoft.UI;
using Windows.Graphics;
using Microsoft.UI.Windowing;
using WinRT.Interop;

namespace DesktopLine.Tool
{
    public class WindowTool
    {

        /// <summary>
        /// AppWindow を返す
        /// </summary>
        /// <param name="window"></param>
        public static AppWindow GetAppWindow(Window window)
        {
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
            var myWndId = Win32Interop.GetWindowIdFromWindow(hWnd);
            return AppWindow.GetFromWindowId(myWndId);
        }

        /// <summary>
        /// ウィンドウサイズを変更する
        /// </summary>
        /// <param name="window"></param>
        /// <param name="height"></param>
        /// <param name="width"></param>
        public static void SetWindowSize(Window window, int height, int width)
        {
            GetAppWindow(window).Resize(new SizeInt32(width, height));
        }

        /// <summary>
        /// ウィンドウを最前面にする
        /// </summary>
        public static void SetWindowTopMost(Window window)
        {
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
            WindowsApiTool.SetWindowPos(hWnd, WindowsApiTool.HWND_TOPMOST, 0, 0, 0, 0, WindowsApiTool.SWP_NOMOVE | WindowsApiTool.SWP_NOSIZE);
        }

        /// <summary>
        /// UIスレッドで Window#Hide を呼び出す
        /// </summary>
        /// <param name="window"></param>
        public static void AsyncHide(Window window)
        {
            window.DispatcherQueue.TryEnqueue(() => GetAppWindow(window).Hide());
        }

        /// <summary>
        ///ウィンドウを真ん中に移動する
        /// </summary>
        /// <param name="window"></param>
        public static void SetCenterWindowPos(Window window)
        {
            var hWnd = WindowNative.GetWindowHandle(window);
            var windowId = Win32Interop.GetWindowIdFromWindow(hWnd);
            var appWindow = AppWindow.GetFromWindowId(windowId);
            var displayArea = DisplayArea.GetFromWindowId(windowId, DisplayAreaFallback.Nearest);
            // 真ん中にする
            var CenteredPosition = appWindow.Position;
            CenteredPosition.X = (displayArea.WorkArea.Width - appWindow.Size.Width) / 2;
            CenteredPosition.Y = (displayArea.WorkArea.Height - appWindow.Size.Height) / 2;
            appWindow.Move(CenteredPosition);
        }

    }
}
