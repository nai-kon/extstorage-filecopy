using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace ExtFileCopy
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>

    public partial class App : Application
    {
        public static string appname = "外部メディアコピー";
        public static string settingxml = "";

        private void Application_Startup(object sender, StartupEventArgs e){

            if (e.Args.Count() < 1) {
                MessageBox.Show("アプリケーションは直接実行できません", appname);
                Shutdown();
            }
            else {
                settingxml = e.Args.ElementAt(0);
            }
        }

    }
}
