using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace ExtFileCopy
{
    class CopyFiles
    {

        public string title = null;
        public string srctype = null;
        public string srcvolname = null;
        public string srcdir = null;
        public string destdir = null;
        public string extension = null;
        private MainWindowData vdata = null;
        

        public CopyFiles(MainWindowData vdata) {
            this.vdata = vdata;
        }

        // 0 success, -1 fail
        public int ExecCopy() {

            ICopyStorage storage = ICopyStorageFactory.Create(srctype);
            if (storage == null) return -1;

            // コピー元のフォルダパスを取得
            string truesrcdir = null;
            while ((truesrcdir = storage.GetTrueDirpath(srcvolname, srcdir)) == null) {
                if (MessageBox.Show("メディアを挿入してOKを選択してください", App.appname, MessageBoxButton.OKCancel) == MessageBoxResult.Cancel) {
                    return -1;
                }
            }

            // コピー先に存在しないコピー元ファイルを抽出            
            IEnumerable<string> copyfname = storage.GetFilenames(truesrcdir, extension).Where(fname => System.IO.File.Exists(destdir + fname) == false);
            if (copyfname.Count() == 0) {
                vdata.TotalCopyCount = "同期対象のファイルはありません";
                return -1;
            }


            // コピー処理
            vdata.TotalCopyCount = String.Format("コピー中　{0}個の項目", copyfname.Count());
            string[] filedfile = storage.ExecCopyFile(truesrcdir, destdir, copyfname, vdata);
            vdata.TotalCopyCount = "コピー完了";
            vdata.DispInfo += String.Format("コピー完了 - 失敗{0}項目\n", filedfile.Count());
            return 0;
        }

    }
}
