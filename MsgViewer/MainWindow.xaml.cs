using System;
using System.IO;
using System.Windows;
using MsgDao;

namespace MsgViewer
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly JsObject _browserObject = new JsObject();

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
        }

        public string GetViewsUrl()
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
