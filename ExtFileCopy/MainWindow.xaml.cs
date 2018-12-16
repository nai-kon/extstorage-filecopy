using System;
using System.Xml;
using System.Threading;
using System.Windows;
using System.Threading.Tasks;

namespace ExtStorageTrans
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    /// 

    public partial class MainWindow : Window
    {
        private MainWindowData vdata;
        private TransFiles transInst;

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
                var settings = xml.SelectNodes("trans/setting");
                foreach(XmlNode setting in settings) {
                    transInst = new TransFiles(vdata);
                    transInst.title = vdata.TransSettingTitle = setting.Attributes["title"].InnerText;
                    transInst.srctype = setting.SelectSingleNode("mediatype").InnerText;
                    transInst.srcvolname = setting.SelectSingleNode("srcvolname").InnerText;
                    transInst.srcdir = setting.SelectSingleNode("srcdir").InnerText;
                    transInst.destdir = setting.SelectSingleNode("destdir").InnerText;
                    transInst.extension = setting.SelectSingleNode("extension").InnerText;
                    break;
                }
            }
            catch (Exception e) {
                transInst = null;
                MessageBox.Show("設定エラー", App.appname);
            }
        }

        private async Task AsncExecTrans(){
            if (transInst == null) return;

            ExecTrans.IsEnabled = false;
            int res = await Task.Run(() =>
            {
                return transInst.ExecTrans();
            });
            if (res == 0) {
                // コピー先をエクスプローラで開く
                System.Diagnostics.Process.Start(transInst.destdir);
            }
            ExecTrans.IsEnabled = true;
        }

        private async void ExecTrans_Click(object sender, RoutedEventArgs e)
        {
            await AsncExecTrans();
        }
    }
}
