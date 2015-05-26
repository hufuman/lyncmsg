using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using MsgDao;
using LyncMsg.Demon;

namespace LyncMsg
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly JsObject _browserObject = new JsObject();
        private const int WmHotKey = 0x312;
        private int _showLyncHotKeyId;
        private readonly int _showTipWndMsg = WinApi.RegisterWindowMessage("showTipWndMsg");
        private readonly MsgTipWnd _msgTipWnd = new MsgTipWnd();
        private IntPtr _mainWndPtr;
        public MainWindow()
        {
            InitializeComponent();

            if (!LyncDb.GetDb().Init())
            {
                MessageBox.Show("Failed to init message files", "LyncMsg", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
                return;
            }

            BrowserUtil.CoInternetSetFeatureEnabled(INTERNETFEATURELIST.FEATURE_LOCALMACHINE_LOCKDOWN,
                BrowserUtil.SET_FEATURE_ON_PROCESS, false);

            string url = GetViewsUrl();
            if (url == null)
            {
                MessageBox.Show("Failed to find views", "LyncMsg", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
                return;
            }

            var conversationManager = new LConversationManager();
            conversationManager.Init(LClient.GetClient().LyncClient.ConversationManager, OnMsgArrived);

            WebMessages.Navigate(new Uri(url, UriKind.RelativeOrAbsolute));
            _browserObject.SetBrowser(WebMessages);
            WebMessages.ObjectForScripting = _browserObject;

            // register hot key
            Loaded += (sender, args) =>
            {
                var wndHelper = new WindowInteropHelper(this);
                _mainWndPtr = wndHelper.Handle;
                var wndSource = HwndSource.FromHwnd(wndHelper.Handle);
                if (wndSource == null)
                    return;

                wndSource.AddHook((IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) =>
                {
                    switch (msg)
                    {
                        case WmHotKey:
                            {
                                if (wParam.ToInt32() == _showLyncHotKeyId)
                                {
                                    // ToggleLyncWindow();
                                    ToggleViewer();
                                    handled = true;
                                }
                                break;
                            }
                    }
                    if (msg == _showTipWndMsg)
                    {
                        _msgTipWnd.ShowTip();
                    }

                    return IntPtr.Zero;
                });

                _showLyncHotKeyId = WinApi.GlobalAddAtom("ShowLyncHotKey");
                // Ctrl+Alt+X
                WinApi.RegisterHotKey(wndHelper.Handle, _showLyncHotKeyId, ModifierKeys.Control | ModifierKeys.Alt, 88);
            };
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            Application.Current.Shutdown();
            e.Cancel = false;
        }

        private void ToggleViewer()
        {
            var wndHelper = new WindowInteropHelper(this);
            IntPtr handle = wndHelper.Handle;
            if (WinApi.GetForegroundWindow() == handle && WinApi.IsWindowVisible(handle))
            {
                ShowLyncWindow(false);
                WinApi.ShowWindow(handle, WinApi.WindowShowStyle.SwMinimize);
            }
            else
            {
                _browserObject.CallOnShow();
                if (WinApi.IsWindowVisible(handle) && (WinApi.IsIconic(handle) || WinApi.GetForegroundWindow() != handle))
                    WinApi.SwitchToThisWindow(handle, true);
                else
                    WinApi.ShowWindow(handle, WinApi.WindowShowStyle.SwShow);
            }
        }

        private void ShowLyncWindow(bool show)
        {
            IntPtr handle = WinApi.FindWindow("CommunicatorMainWindowClass", null);
            if (handle == IntPtr.Zero)
                return;

            if (show)
            {
                if (WinApi.IsWindowVisible(handle) && (WinApi.IsIconic(handle) || WinApi.GetForegroundWindow() != handle))
                    WinApi.SwitchToThisWindow(handle, true);
                else
                    WinApi.ShowWindow(handle, WinApi.WindowShowStyle.SwShow);
            }
            else
            {
                WinApi.ShowWindow(handle, WinApi.WindowShowStyle.SwHide);
            }
        }

        private string GetViewsUrl()
        {
            const string postfix = "\\views\\index.html";
            string dir = AppDomain.CurrentDomain.BaseDirectory;
            for (int i = 0; i < 4; ++i)
            {
                string path = dir + postfix;
                path = path.Replace("\\\\", "\\");
                if (File.Exists(path))
                    return path;
                dir = Directory.GetParent(dir).FullName;
            }
            return null;
        }

        private void OnMsgArrived(long chatId, long userId, string htmlMsg, string plainMsg)
        {
            string msg = UserDb.GetDb().GetUserInfoById(userId).Name + "：" + plainMsg;
            _msgTipWnd.SetMsg(msg);
            WinApi.PostMessage(_mainWndPtr, _showTipWndMsg, (IntPtr)0, (IntPtr)0);
        }
    }
}
