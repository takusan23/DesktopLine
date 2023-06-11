// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using DesktopLine.Tool;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using System.Diagnostics;
using System.Threading.Tasks;
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
        /// �L�[�{�[�h�̓��͂��t�b�N����
        /// </summary>
        KeybordHookTool keybordHook = null;

        public MainWindow()
        {
            InitializeComponent();

            // �^�C�g���o�[�Ȃǂ�����
            ExtendsContentIntoTitleBar = true;
            // �E�B���h�E�T�C�Y
            WindowTool.SetWindowSize(this, 500, 800);
            WindowTool.SetWindowTopMost(this);
            WindowTool.AsyncHide(this);
            WindowTool.SetCenterWindowPos(this);

            // �A�N�����f�ށi�}�C�J�� Win11 �ȍ~�H�j��K�p����
            wsdqHelper = new WindowsSystemDispatcherQueueHelper();
            wsdqHelper.EnsureWindowsSystemDispatcherQueueController();
            SetBackdrop();

            // Hook
            keybordHook = new();
            SetupHook();

            // �}�E�X����ړ����̃C�x���g
            SetupCanvas();
        }

        /// <summary>
        /// SetWindowsHookEx �Ńt�b�N�����C�x���g����������
        /// </summary>
        private void SetupHook()
        {
            var appWindow = WindowTool.GetAppWindow(this);
            // ����Ctrl�L�[�������Ă��邩
            var isCtrlKeyDown = false;
            // Windows�L�[�������Ă��邩
            var isWindowsKeyDown = false;

            // �L�[����������Ă΂��
            keybordHook.onKeybordHookEvent = (keyState, keyCode) =>
            {
                Debug.WriteLine($"onKeybordHookEvent {keyState} = {keyCode}");
                switch (keyState)
                {
                    // �L�[�������Ă���
                    case KeybordHookTool.KeyState.DOWN:
                        if (keyCode == WindowsApiTool.VK_LCONTROL)
                        {
                            isCtrlKeyDown = true;
                        }
                        if (keyCode == WindowsApiTool.VK_LWIN)
                        {
                            isWindowsKeyDown = true;
                        }
                        // �����Ƃ������Ă���Ώo��
                        if (isCtrlKeyDown && isWindowsKeyDown)
                        {
                            // ���z�f�X�N�g�b�v�ؑ֎��͐؂�ւ������Ƃ̃f�X�N�g�b�v�ŕ\�������悤
                            if (!appWindow.IsVisible)
                            {
                                appWindow.Show();
                            }
                        }
                        break;
                    // �L�[�𗣂���
                    case KeybordHookTool.KeyState.UP:
                        if (keyCode == WindowsApiTool.VK_LCONTROL)
                        {
                            isCtrlKeyDown = false;
                        }
                        if (keyCode == WindowsApiTool.VK_LWIN)
                        {
                            isWindowsKeyDown = false;
                        }
                        // �����\�����Ȃ����
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
        /// �}�E�X�� Win3 UI Canvas �ɐ���`������
        /// </summary>
        private void SetupCanvas()
        {
            // �ŏ��̃}�E�X���W
            Windows.Foundation.Point? startPosition = null;
            // ���݂̃}�E�X���W
            Windows.Foundation.Point? currentPosition = null;

            // �N���b�N����������Ă΂��
            layoutRoot.PointerPressed += (sender, e) =>
            {
                var point = e.GetCurrentPoint(layoutRoot);
                var position = point.Position;
                startPosition = position;
            };

            // �N���b�N�������Ă�ԌĂ΂��
            layoutRoot.PointerMoved += (sender, e) =>
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
            };

            // �N���b�N�𗣂�����Ă΂��
            // �X�}�z�̃X���C�v�ŉ�ʂ�؂�ւ���̂Ɠ��������̑���
            // ���z�f�X�N�g�b�v�؂�ւ��� Win+Ctrl+���L�[ �ł����A���ł� Win+Ctrl �͉����Ă����ԁiDesktopLine �\�����j�Ȃ̂ŁA���L�[����������
            layoutRoot.PointerReleased += (sender, e) =>
            {
                var point = e.GetCurrentPoint(layoutRoot);
                if (startPosition.Value.X > point.Position.X)
                {
                    // �J�n�ʒu��荶��
                    // �����牼�z�f�X�N�g�b�v���ړ������Ă��� �C���[�W
                    ShortcutInputTool.SendArrowKeyInput(ShortcutInputTool.Direction.Right);
                    Debug.WriteLine("[VirtualDesktop Switch] Right");
                }
                else
                {
                    // �J�n�ʒu���E��
                    // �E���牼�z�f�X�N�g�b�v���ړ������Ă��� �C���[�W
                    ShortcutInputTool.SendArrowKeyInput(ShortcutInputTool.Direction.Left);
                    Debug.WriteLine("[VirtualDesktop Switch] Left");
                }
                // �`�������e������
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
        /// �^�X�N�g���C�̏I������������Ă΂��
        /// </summary>
        private void TaskTrayIconItemClose_Click(Microsoft.UI.Xaml.Input.XamlUICommand sender, Microsoft.UI.Xaml.Input.ExecuteRequestedEventArgs args) => Close();

        /// <summary>
        /// �^�X�N�g���C��GitHub����������Ă΂��
        /// </summary>
        private void TaskTrayIconItemGitHub_Click(Microsoft.UI.Xaml.Input.XamlUICommand sender, Microsoft.UI.Xaml.Input.ExecuteRequestedEventArgs args) => OpenGitHubTool.OpenGitHubWebPage();

    }
}
