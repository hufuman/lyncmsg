using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using MsgDao;

namespace MsgViewer
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly JsObject _browserObject = new JsObject();
        private const int WM_HOTKEY = 0x312;
        private int showLyncHotKeyId = 0;
        public MainWindow()
        {
            InitializeComponent();

            if (!LyncDb.GetDb().Init())
            {
                MessageBox.Show("Failed to init message files");
                Close();
                return;
            }

            BrowserUtil.CoInternetSetFeatureEnabled(INTERNETFEATURELIST.FEATURE_LOCALMACHINE_LOCKDOWN,
                BrowserUtil.SET_FEATURE_ON_PROCESS, false);

            string url = GetViewsUrl();
            if (url == null)
            {
                MessageBox.Show("Failed to find views");
                Close();
                return;
            }

            WebMessages.Navigate(new Uri(url, UriKind.RelativeOrAbsolute));
            _browserObject.SetBrowser(WebMessages);
            WebMessages.ObjectForScripting = _browserObject;

            // register hot key
            Loaded += (sender, args) =>
            {
                var wndHelper = new WindowInteropHelper(this);
                var wndSource = HwndSource.FromHwnd(wndHelper.Handle);

                wndSource.AddHook((IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) =>
                {
                    switch (msg)
                    {
                        case WM_HOTKEY:
                            {
                                if (wParam.ToInt32() == showLyncHotKeyId)
                                {
                                    // ToggleLyncWindow();
                                    ToggleViewer();
                                    handled = true;
                                }
                                break;
                            }
                    }

                    return IntPtr.Zero;
                });

                showLyncHotKeyId = GlobalAddAtom("ShowLyncHotKey");
                // Ctrl+Alt+X
                RegisterHotKey(wndHelper.Handle, showLyncHotKeyId, ModifierKeys.Control | ModifierKeys.Alt, (int)88);
            };
        }

        private void ToggleViewer()
        {
            var wndHelper = new WindowInteropHelper(this);
            IntPtr handle = wndHelper.Handle;
            if (GetForegroundWindow() == handle && IsWindowVisible(handle))
            {
                ShowLyncWindow(false);
                ShowWindow(handle, WindowShowStyle.SW_MINIMIZE);
            }
            else
            {
                if (IsWindowVisible(handle) && (IsIconic(handle) || GetForegroundWindow() != handle))
                    SwitchToThisWindow(handle, true);
                else
                    ShowWindow(handle, WindowShowStyle.SW_SHOW);
            }
        }

        internal enum WindowShowStyle : uint
        {
            SW_HIDE = 0,
            SW_SHOW = 5,
            SW_MINIMIZE = 6
        }
        [DllImport("user32.DLL")]
        public static extern IntPtr FindWindow(string lpszClass, string lpszWindow);
        [DllImport("User32.dll")]
        private static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")]
        internal static extern bool ShowWindow(IntPtr hWnd, WindowShowStyle nCmdShow);
        [DllImport("user32")]
        static extern bool IsIconic(IntPtr hWnd);
        [DllImport("user32.dll")]
        public static extern void SwitchToThisWindow(IntPtr hwnd, bool fAltTab);
        [DllImport("kernel32", SetLastError = true)]
        public static extern short GlobalAddAtom(string lpString);
        [DllImport("user32", SetLastError = true)]
        public static extern bool IsWindowVisible(IntPtr hwnd);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool RegisterHotKey(
            IntPtr hWnd,
            int id,
            ModifierKeys fsModifiers,
            int vk
            );

        private void ShowLyncWindow(bool show)
        {
            IntPtr handle = FindWindow("CommunicatorMainWindowClass", null);
            if (handle == IntPtr.Zero)
                return;

            if (show)
            {
                if (IsWindowVisible(handle) && (IsIconic(handle) || GetForegroundWindow() != handle))
                    SwitchToThisWindow(handle, true);
                else
                    ShowWindow(handle, WindowShowStyle.SW_SHOW);
            }
            else
            {
                ShowWindow(handle, WindowShowStyle.SW_HIDE);
            }
        }

        private void ToggleLyncWindow()
        {
            IntPtr handle = FindWindow("CommunicatorMainWindowClass", null);
            if (handle != IntPtr.Zero)
            {
                if (GetForegroundWindow() == handle && IsWindowVisible(handle))
                {
                    ShowWindow(handle, WindowShowStyle.SW_HIDE);
                }
                else
                {
                    if (IsWindowVisible(handle) && (IsIconic(handle) || GetForegroundWindow() != handle))
                        SwitchToThisWindow(handle, true);
                    else
                        ShowWindow(handle, WindowShowStyle.SW_SHOW);
                }
            }
        }

        private string GetViewsUrl()
        {
            const string postfix = "\\views\\index.html";
            string dir = AppDomain.CurrentDomain.BaseDirectory;
            for (int i=0;i<4;++i)
            {
                string path = dir + postfix;
                path = path.Replace("\\\\", "\\");
                if (File.Exists(path))
                    return path;
                dir = Directory.GetParent(dir).FullName;
            }
            return null;
        }
    }
}
