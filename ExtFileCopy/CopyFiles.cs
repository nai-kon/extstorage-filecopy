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

            // コピー元のフォルダパスを抽出
            string retryMsg;
            while (!storage.FindSrcDir(srcvolname, srcdir, out retryMsg, vdata)) {
                if (MessageBox.Show(retryMsg, App.appname, MessageBoxButton.OKCancel) == MessageBoxResult.Cancel) {
                    return -1;
                }
            }

            // コピー先に存在しないコピー元ファイルを抽出
            var copyfile = storage.GetAllFiles(extension, vdata).Where(fname => System.IO.File.Exists(destdir + fname.fileName) == false);
            if (copyfile == null || copyfile.Count() == 0) {
                vdata.TotalCopyCount = "同期対象のファイルはありません";
                return -1;
            }


            // コピー処理
            int ret = -1;
            vdata.TotalCopyCount =String.Format("コピー中　{0}個の項目", copyfile.Count());
            vdata.DispInfo = vdata.TotalCopyCount + "\n";
            string[] filedfile = storage.ExecCopyFile(destdir, copyfile, vdata);
            if (filedfile != null) {
                ret = 0;
                vdata.TotalCopyCount = "コピー完了";
                vdata.DispInfo += String.Format("コピー完了 - 失敗{0}項目\n", filedfile.Count());
            }
            return ret;
        }

    }
}
