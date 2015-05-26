using System;
using System.Windows;
using System.Windows.Threading;

namespace LyncMsg
{
    /// <summary>
    /// Interaction logic for MsgTipWnd.xaml
    /// </summary>
    public partial class MsgTipWnd : Window
    {
        private string _msgContent = "";
        private readonly DispatcherTimer _timer = new DispatcherTimer();
        private int _tickCount = 0;
        public MsgTipWnd()
        {
            _tickCount = 0;
            _timer.Interval = new TimeSpan(0, 0, 1);
            InitializeComponent();
            Loaded += (sender, args) =>
            {
                MsgContentLabel.Content = _msgContent;
            };
        }

        public void SetMsg(string msg)
        {
            _msgContent = msg;
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            _tickCount = 0;
            _timer.Stop();
            Hide();
            e.Cancel = true;
        }

        public void ShowTip()
        {
            _tickCount = 0;
            Left = SystemParameters.WorkArea.Right - Width;
            Top = SystemParameters.WorkArea.Bottom - Height;
            Show();
            if (MsgContentLabel != null)
                MsgContentLabel.Content = _msgContent;
            _timer.Tick += (sender, args) =>
            {
                ++ _tickCount;
                if (_tickCount >= 6)
                    Close();
            };
            _timer.Start();
        }
    }
}
