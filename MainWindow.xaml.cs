// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using DesktopLine.Tool;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using System.Diagnostics;
using System.Drawing;
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
        /// 一つ前のマウス座標
        /// </summary>
        Windows.Foundation.Point? currentPoint = null;

        public MainWindow()
        {
            InitializeComponent();
            // タイトルバーなどを消す
            ExtendsContentIntoTitleBar = true;
            // ウィンドウサイズ
            WindowTool.SetWindowSize(this, 300, 300);
            // アクリル素材（マイカは Win11 以降？）を適用する
            wsdqHelper = new WindowsSystemDispatcherQueueHelper();
            wsdqHelper.EnsureWindowsSystemDispatcherQueueController();
            SetBackdrop();
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
        }


        /// <summary>
        /// クリックを押し続けている間呼ばれる
        /// </summary>
        private void DrawLineCanavs_PointerMoved(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            var pointer = e.Pointer;
            var point = e.GetCurrentPoint(layoutRoot);
            var position = point.Position;
            if (point.Properties.IsLeftButtonPressed && currentPoint != null)
            {
                var line = new Line()
                {
                    X1 = currentPoint.Value.X,
                    Y1 = currentPoint.Value.Y,
                    X2 = position.X,
                    Y2 = position.Y,
                    StrokeThickness = 4,
                    Stroke = new SolidColorBrush(Windows.UI.Color.FromArgb(0xFF, 0x00, 0x00, 0x00))
                };

                layoutRoot.Children.Add(line);
                Debug.WriteLine("DrawLineCanavs.Children.Add(line);");
            }
            currentPoint = position;
        }

    }

}
