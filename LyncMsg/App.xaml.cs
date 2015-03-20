using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;

namespace LyncMsg
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        [DllImport("user32.DLL")]
        public static extern IntPtr FindWindow(string lpszClass, string lpszWindow);

        protected override void OnStartup(StartupEventArgs e)
        {
            IntPtr handle = FindWindow(null, "LyncMsg");
            if (handle.ToInt64() == 0)
            {
                base.OnStartup(e);
            }
            else
            {
                if (WinApi.IsWindowVisible(handle) && (WinApi.IsIconic(handle) || WinApi.GetForegroundWindow() != handle))
                    WinApi.SwitchToThisWindow(handle, true);
                else
                    WinApi.ShowWindow(handle, WinApi.WindowShowStyle.SwShow);
                Application.Current.Shutdown();
            }
        }
    }
}
