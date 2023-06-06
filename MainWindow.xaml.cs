// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using DesktopLine.Tool;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Diagnostics;
using WinRT;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace DesktopLine
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        // Windowsキーを長押しする時間
        private readonly int WINDOWS_KEY_HOOK_LONG_PRESS_MS = 500;

        // TODO Win11 のみになったらアクリルの後継の Mica が使えるはず
        WindowsSystemDispatcherQueueHelper wsdqHelper;
        Microsoft.UI.Composition.SystemBackdrops.DesktopAcrylicController acrylicController;
        Microsoft.UI.Composition.SystemBackdrops.SystemBackdropConfiguration configurationSource;

        /// <summary>
        /// キーボードの入力をフックする
        /// </summary>
        KeybordHookTool keybordHook = null;

        /// <summary>
        /// 現在のマウス座標
        /// </summary>
        Windows.Foundation.Point? currentPosition = null;

        /// <summary>
        /// 最初のマウス座標
        /// </summary>
        Windows.Foundation.Point? startPosition = null;

        public MainWindow()
        {
            InitializeComponent();

            // タイトルバーなどを消す
            ExtendsContentIntoTitleBar = true;
            // ウィンドウサイズ
            WindowTool.SetWindowSize(this, 500, 800);
            WindowTool.SetWindowTopMost(this);

            // アクリル素材（マイカは Win11 以降？）を適用する
            wsdqHelper = new WindowsSystemDispatcherQueueHelper();
            wsdqHelper.EnsureWindowsSystemDispatcherQueueController();
            SetBackdrop();

            // Hook
            keybordHook = new();
            SetupHook();

            // マウス操作移動時のイベント
            layoutRoot.PointerPressed += LayoutRoot_PointerPressed;
            layoutRoot.PointerMoved += LayoutRoot_PointerMoved;
            layoutRoot.PointerReleased += LayoutRoot_PointerReleased;
        }

        /// <summary>
        /// SetWindowsHookEx でフックしたイベントを処理する
        /// </summary>
        private void SetupHook()
        {
            var appWindow = WindowTool.GetAppWindow(this);

            // Windowsキーを最初に押した時間、押している時間。
            DateTime? startKeyDownTime = null;
            DateTime? currentKeyDownTime = null;

            // キーを押したら呼ばれる
            keybordHook.onKeybordHookEvent = (keyState, keyCode) =>
            {
                // Windowsキー の時だけ
                if (keyCode == WindowsApiTool.VK_LWIN)
                {
                    switch (keyState)
                    {
                        // キーを押している間呼ばれる
                        case KeybordHookTool.KeyState.DOWN:
                            currentKeyDownTime = DateTime.Now;
                            if (startKeyDownTime == null)
                            {
                                startKeyDownTime = DateTime.Now;
                            }
                            // 比較して超えていたら DesktopLine 呼び出す
                            if ((currentKeyDownTime.Value - startKeyDownTime.Value).TotalMilliseconds >= WINDOWS_KEY_HOOK_LONG_PRESS_MS)
                            {
                                // 表示されていない場合は出す
                                if (!appWindow.IsVisible)
                                {
                                    appWindow.Show();
                                }
                            }
                            break;

                        // キーを離したら呼ばれる
                        case KeybordHookTool.KeyState.UP:
                            if ((currentKeyDownTime.Value - startKeyDownTime.Value).TotalMilliseconds <= WINDOWS_KEY_HOOK_LONG_PRESS_MS)
                            {
                                // もし超えていなかったら大人しく Windowsキー を押す
                                ShortcutInputTool.SendWindowsKeyEvent();
                            }
                            // 次押した際のためにリセット
                            appWindow.Hide();
                            currentKeyDownTime = null;
                            startKeyDownTime = null;
                            break;
                    }
                    return true;
                }
                return false;
            };
        }

        private void SetBackdrop()
        {
            if (acrylicController != null)
            {
                acrylicController.Dispose();
                acrylicController = null;
            }
            configurationSource = null;
            TrySetAcrylicBackdrop();
        }

        private bool TrySetAcrylicBackdrop()
        {
            if (Microsoft.UI.Composition.SystemBackdrops.DesktopAcrylicController.IsSupported())
            {
                configurationSource = new Microsoft.UI.Composition.SystemBackdrops.SystemBackdropConfiguration();
                this.Activated += Window_Activated;
                this.Closed += Window_Closed;

                configurationSource.IsInputActive = true;
                switch (((FrameworkElement)this.Content).ActualTheme)
                {
                    case ElementTheme.Dark: configurationSource.Theme = Microsoft.UI.Composition.SystemBackdrops.SystemBackdropTheme.Dark; break;
                    case ElementTheme.Light: configurationSource.Theme = Microsoft.UI.Composition.SystemBackdrops.SystemBackdropTheme.Light; break;
                    case ElementTheme.Default: configurationSource.Theme = Microsoft.UI.Composition.SystemBackdrops.SystemBackdropTheme.Default; break;
                }

                acrylicController = new Microsoft.UI.Composition.SystemBackdrops.DesktopAcrylicController
                {
                    TintColor = new Windows.UI.Color() { A = 255, R = 32, G = 128, B = 64 },
                    TintOpacity = 0.25f,
                    LuminosityOpacity = 0.4f,
                    FallbackColor = new Windows.UI.Color() { A = 255, R = 16, G = 16, B = 64 }
                };

                acrylicController.AddSystemBackdropTarget(this.As<Microsoft.UI.Composition.ICompositionSupportsSystemBackdrop>());
                acrylicController.SetSystemBackdropConfiguration(configurationSource);
                return true;
            }
            return false;
        }

        private void Window_Activated(object sender, WindowActivatedEventArgs args)
        {
            configurationSource.IsInputActive = args.WindowActivationState != WindowActivationState.Deactivated;
        }

        private void Window_Closed(object sender, WindowEventArgs args)
        {
            if (acrylicController != null)
            {
                acrylicController.Dispose();
                acrylicController = null;
            }
            this.Activated -= Window_Activated;
            configurationSource = null;

            keybordHook.UnhookWindowsHookEx();
        }

        /// <summary>
        /// クリックを押したら呼ばれる
        /// </summary>
        private void LayoutRoot_PointerPressed(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            var point = e.GetCurrentPoint(layoutRoot);
            var position = point.Position;
            startPosition = position;
        }

        /// <summary>
        /// クリックを押し続けている間呼ばれる
        /// </summary>
        private void LayoutRoot_PointerMoved(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            // 線を描画する
            var point = e.GetCurrentPoint(layoutRoot);
            var position = point.Position;
            if (point.Properties.IsLeftButtonPressed && currentPosition != null)
            {
                var line = new Line()
                {
                    X1 = currentPosition.Value.X,
                    Y1 = currentPosition.Value.Y,
                    X2 = position.X,
                    Y2 = position.Y,
                    StrokeThickness = 4,
                    Stroke = new SolidColorBrush(Windows.UI.Color.FromArgb(0xFF, 0x00, 0x00, 0x00))
                };

                layoutRoot.Children.Add(line);
            }
            currentPosition = position;
        }

        /// <summary>
        /// クリックが離れたら呼ばれる
        /// </summary>
        private void LayoutRoot_PointerReleased(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            var point = e.GetCurrentPoint(layoutRoot);
            if (startPosition.Value.X < point.Position.X)
            {
                // 開始位置より右側
                ShortcutInputTool.SendSwitchKeyEvent(ShortcutInputTool.Direction.Right);
                Debug.WriteLine("[VirtualDesktop Switch] Right");
            }
            else
            {
                // 開始位置より左側
                ShortcutInputTool.SendSwitchKeyEvent(ShortcutInputTool.Direction.Left);
                Debug.WriteLine("[VirtualDesktop Switch] Left");
            }
            // 描いた内容を消す
            layoutRoot.Children.Clear();
        }

    }
}
