using System;
using System.Xml;
using System.Threading;
using System.Windows;
using System.Threading.Tasks;

namespace ExtFileCopy
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    /// 

    public partial class MainWindow : Window
    {
        private MainWindowData vdata;
        private CopyFiles copyInst;

        public MainWindow()
        {
            InitializeComponent();
            vdata = new MainWindowData();
            DataContext = vdata;
            LoadSetting();
        }

        private void LoadSetting() {
            var xml = new XmlDocument();

            try {
                xml.Load(App.settingxml);
                var settings = xml.SelectNodes("copy/setting");
                foreach(XmlNode setting in settings) {
                    copyInst = new CopyFiles(vdata);
                    copyInst.title = vdata.CopySettingTitle = setting.Attributes["title"].InnerText;
                    copyInst.srctype = setting.SelectSingleNode("mediatype").InnerText;
                    copyInst.srcvolname = setting.SelectSingleNode("srcvolname").InnerText;
                    copyInst.srcdir = setting.SelectSingleNode("srcdir").InnerText;
                    copyInst.destdir = setting.SelectSingleNode("destdir").InnerText;
                    copyInst.extension = setting.SelectSingleNode("extension").InnerText;
                    break;
                }
            }
            catch (Exception e) {
                copyInst = null;
                MessageBox.Show("設定エラー", App.appname);
            }
        }

        private async Task AsncExecCopy(){
            if (copyInst == null) return;

            ExecCopy.IsEnabled = false;
            int res = await Task.Run(() =>
            {
                return copyInst.ExecCopy();
            });
            if (res == 0) {
                // コピー先をエクスプローラで開く
                System.Diagnostics.Process.Start(copyInst.destdir);
            }
            ExecCopy.IsEnabled = true;
        }

        private async void ExecCopy_Click(object sender, RoutedEventArgs e)
        {
            await AsncExecCopy();
        }
    }
}
