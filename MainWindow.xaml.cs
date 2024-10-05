// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using DesktopLine.Tool;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
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
        // TODO Win11 のみになったらアクリルの後継の Mica が使えるはず
        WindowsSystemDispatcherQueueHelper wsdqHelper;
        Microsoft.UI.Composition.SystemBackdrops.DesktopAcrylicController acrylicController;
        Microsoft.UI.Composition.SystemBackdrops.SystemBackdropConfiguration configurationSource;

        /// <summary>
        /// キーボードの入力をフックする
        /// </summary>
        KeybordHookTool keybordHook = null;

        /// <summary>
        /// 1 秒を表す ticks。1ms が TimeSpan.TicksPerMillisecond なので、1000 かけて 1sec。
        /// </summary>
        const long ONE_SECONDS_TICKS = TimeSpan.TicksPerMillisecond * 1_000;

        public MainWindow()
        {
            InitializeComponent();

            // タイトルバーなどを消す
            ExtendsContentIntoTitleBar = true;
            // ウィンドウサイズ
            WindowTool.SetWindowSize(this, 500, 800);
            WindowTool.SetWindowTopMost(this);
            WindowTool.AsyncHide(this);
            WindowTool.SetCenterWindowPos(this);

            // アクリル素材（マイカは Win11 以降？）を適用する
            wsdqHelper = new WindowsSystemDispatcherQueueHelper();
            wsdqHelper.EnsureWindowsSystemDispatcherQueueController();
            SetBackdrop();

            // Hook
            keybordHook = new();
            SetupHook();

            // マウス操作移動時のイベント
            SetupCanvas();
        }

        /// <summary>
        /// SetWindowsHookEx でフックしたイベントを処理する
        /// </summary>
        private void SetupHook()
        {
            var appWindow = WindowTool.GetAppWindow(this);
            // 左のCtrlキーを押しているか
            var isCtrlKeyDown = false;
            // Windowsキーを押しているか
            var isWindowsKeyDown = false;
            // それぞれのキーを押した時の時間。単位は Tick で、1ms が 10_000 ticks になる。
            var ctrlKeyDownTime = 0L;
            var windowsKeyDownTime = 0L;

            // キーを押したら呼ばれる
            keybordHook.onKeybordHookEvent = (keyState, keyCode) =>
            {
                Debug.WriteLine($"onKeybordHookEvent {keyState} = {keyCode}");
                switch (keyState)
                {
                    // キーを押している
                    case KeybordHookTool.KeyState.DOWN:
                        if (keyCode == WindowsApiTool.VK_LCONTROL)
                        {
                            isCtrlKeyDown = true;
                            ctrlKeyDownTime = DateTime.Now.Ticks;
                        }
                        if (keyCode == WindowsApiTool.VK_LWIN)
                        {
                            isWindowsKeyDown = true;
                            windowsKeyDownTime = DateTime.Now.Ticks;
                        }
                        // パソコンが高負荷状態だと、キーを押して無いのに表示されたりするため、
                        // キーを押した時間がかけ離れていないことを確認。かけ離れている場合は押してないことにする。
                        if (ONE_SECONDS_TICKS < Math.Abs(ctrlKeyDownTime - ctrlKeyDownTime))
                        {
                            isCtrlKeyDown = false;
                            isWindowsKeyDown = false;
                        }
                        // 両方とも押していれば出す
                        if (isCtrlKeyDown && isWindowsKeyDown)
                        {
                            // 仮想デスクトップ切替時は切り替えたあとのデスクトップで表示されるよう
                            if (!appWindow.IsVisible)
                            {
                                appWindow.Show();
                            }
                        }
                        break;
                    // キーを離した
                    case KeybordHookTool.KeyState.UP:
                        if (keyCode == WindowsApiTool.VK_LCONTROL)
                        {
                            isCtrlKeyDown = false;
                            ctrlKeyDownTime = 0;
                        }
                        if (keyCode == WindowsApiTool.VK_LWIN)
                        {
                            isWindowsKeyDown = false;
                            windowsKeyDownTime = 0;
                        }
                        // もし表示中なら消す
                        if (appWindow.IsVisible)
                        {
                            appWindow.Hide();
                        }
                        break;
                }
                return false;
            };
        }

        /// <summary>
        /// マウスで Win3 UI Canvas に線を描く処理
        /// </summary>
        private void SetupCanvas()
        {
            // 最初のマウス座標
            Windows.Foundation.Point? startPosition = null;
            // 現在のマウス座標
            Windows.Foundation.Point? currentPosition = null;

            // クリックを押したら呼ばれる
            layoutRoot.PointerPressed += (sender, e) =>
            {
                var point = e.GetCurrentPoint(layoutRoot);
                var position = point.Position;
                startPosition = position;
            };

            // クリックを押してる間呼ばれる
            layoutRoot.PointerMoved += (sender, e) =>
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
            };

            // クリックを離したら呼ばれる
            // スマホのスワイプで画面を切り替えるのと同じ感じの操作
            // 仮想デスクトップ切り替えは Win+Ctrl+矢印キー ですが、すでに Win+Ctrl は押している状態（DesktopLine 表示中）なので、矢印キーを押すだけ
            layoutRoot.PointerReleased += (sender, e) =>
            {
                var point = e.GetCurrentPoint(layoutRoot);
                if (startPosition.Value.X > point.Position.X)
                {
                    // 開始位置より左側
                    // 左から仮想デスクトップを移動させてくる イメージ
                    ShortcutInputTool.SendArrowKeyInput(ShortcutInputTool.Direction.Right);
                    Debug.WriteLine("[VirtualDesktop Switch] Right");
                }
                else
                {
                    // 開始位置より右側
                    // 右から仮想デスクトップを移動させてくる イメージ
                    ShortcutInputTool.SendArrowKeyInput(ShortcutInputTool.Direction.Left);
                    Debug.WriteLine("[VirtualDesktop Switch] Left");
                }
                // 描いた内容を消す
                layoutRoot.Children.Clear();
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
        /// タスクトレイの終了を押したら呼ばれる
        /// </summary>
        private void TaskTrayIconItemClose_Click(Microsoft.UI.Xaml.Input.XamlUICommand sender, Microsoft.UI.Xaml.Input.ExecuteRequestedEventArgs args) => Close();

        /// <summary>
        /// タスクトレイのGitHubを押したら呼ばれる
        /// </summary>
        private void TaskTrayIconItemGitHub_Click(Microsoft.UI.Xaml.Input.XamlUICommand sender, Microsoft.UI.Xaml.Input.ExecuteRequestedEventArgs args) => OpenGitHubTool.OpenGitHubWebPage();

        private async void TaskTrayIconItemRegisterStartup_Click(Microsoft.UI.Xaml.Input.XamlUICommand sender, Microsoft.UI.Xaml.Input.ExecuteRequestedEventArgs args)
        {
            // ダイアログ表示のため、一時的に Show
            var appWindow = WindowTool.GetAppWindow(this);
            appWindow.Show();

            // スタートアップに登録、もしくは解除
            bool isRegisterStartup;
            if (!StartupTool.IsRegisterStartup())
            {
                StartupTool.RegisterStartup();
                isRegisterStartup = true;
            }
            else
            {
                StartupTool.UnRegisterStartup();
                isRegisterStartup = false;
            }

            // ダイアログを出す
            ContentDialog startupResultDialog = new()
            {
                Title = "スタートアップ",
                Content = isRegisterStartup ? "スタートアップに登録しました" : "スタートアップの登録を解除しました。",
                CloseButtonText = "閉じる",
                // MSDN にはないがこれが必要
                XamlRoot = Content.XamlRoot
            };
            await startupResultDialog.ShowAsync();

            // ウィンドウ戻す
            appWindow.Hide();
        }
    }
}
