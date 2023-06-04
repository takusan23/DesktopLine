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
        // TODO Win11 �݂̂ɂȂ�����A�N�����̌�p�� Mica ���g����͂�
        WindowsSystemDispatcherQueueHelper wsdqHelper;
        Microsoft.UI.Composition.SystemBackdrops.DesktopAcrylicController acrylicController;
        Microsoft.UI.Composition.SystemBackdrops.SystemBackdropConfiguration configurationSource;

        /// <summary>
        /// ���݂̃}�E�X���W
        /// </summary>
        Windows.Foundation.Point? currentPosition = null;

        /// <summary>
        /// �ŏ��̃}�E�X���W
        /// </summary>
        Windows.Foundation.Point? startPosition = null;

        public MainWindow()
        {
            InitializeComponent();
            // �^�C�g���o�[�Ȃǂ�����
            ExtendsContentIntoTitleBar = true;
            // �E�B���h�E�T�C�Y
            WindowTool.SetWindowSize(this, 500, 800);
            // �A�N�����f�ށi�}�C�J�� Win11 �ȍ~�H�j��K�p����
            wsdqHelper = new WindowsSystemDispatcherQueueHelper();
            wsdqHelper.EnsureWindowsSystemDispatcherQueueController();
            SetBackdrop();

            // �}�E�X����ړ����̃C�x���g
            layoutRoot.PointerPressed += LayoutRoot_PointerPressed;
            layoutRoot.PointerMoved += LayoutRoot_PointerMoved;
            layoutRoot.PointerReleased += LayoutRoot_PointerReleased;
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
        /// �N���b�N����������Ă΂��
        /// </summary>
        private void LayoutRoot_PointerPressed(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            var point = e.GetCurrentPoint(layoutRoot);
            var position = point.Position;
            startPosition = position;
        }

        /// <summary>
        /// �N���b�N�����������Ă���ԌĂ΂��
        /// </summary>
        private void LayoutRoot_PointerMoved(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            // ����`�悷��
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
        /// �N���b�N�����ꂽ��Ă΂��
        /// </summary>
        private void LayoutRoot_PointerReleased(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            var point = e.GetCurrentPoint(layoutRoot);
            if (startPosition.Value.X < point.Position.X)
            {
                // �J�n�ʒu���E��
                VirtualDesktopSwitcher.sendSwitchKeyEvent(VirtualDesktopSwitcher.Direction.Right);
            }
            else
            {
                // �J�n�ʒu��荶��
                VirtualDesktopSwitcher.sendSwitchKeyEvent(VirtualDesktopSwitcher.Direction.Left);
            }
        }

    }
}
