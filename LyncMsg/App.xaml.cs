using System;
using System.Text;
using System.Windows;
using LyncMsg.Util;
using MsgDao;

namespace LyncMsg
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            var handle = IntPtr.Zero;
            var className = new StringBuilder(1024);
            for(;;)
            {
                handle = WinApi.FindWindowEx(IntPtr.Zero, handle, null, "LyncMsg");
                if (handle == IntPtr.Zero)
                    break;

                if (WinApi.GetClassName(handle, className, 1024) <= 0)
                    continue;

                if (className.ToString().IndexOf("[LyncMsg.exe") >= 0)
                    break;
            }

            if (handle == IntPtr.Zero)
            {
                base.OnStartup(e);
                return;
            }

            // confirm lync has started
            if (!LClient.GetClient().Init())
            {
                MessageBox.Show("Start Lync First please.", "LyncMsg");
                return;
            }

            if (WinApi.IsWindowVisible(handle) && (WinApi.IsIconic(handle) || WinApi.GetForegroundWindow() != handle))
                WinApi.SwitchToThisWindow(handle, true);
            else
                WinApi.ShowWindow(handle, WinApi.WindowShowStyle.SwShow);
            Current.Shutdown();
        }
    }
}
