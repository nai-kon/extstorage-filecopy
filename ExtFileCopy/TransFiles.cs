using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace ExtStorageTrans
{
    class TransFiles
    {
        public string title = null;
        public string srctype = null;
        public string srcvolname = null;
        public string srcdir = null;
        public string destdir = null;
        public string extension = null;
        private MainWindowData vdata = null;

        public TransFiles(MainWindowData vdata) {
            this.vdata = vdata;
        }

        // 0 success, -1 fail
        public int ExecTrans() {

            ICopyStorage storage = ICopyStorageFactory.Create(srctype);
            if (storage == null) return -1;

            // 同期元のフォルダパスを抽出
            string retryMsg;
            while (!storage.FindSrcDir(srcvolname, srcdir, out retryMsg, vdata)) {
                if (MessageBox.Show(retryMsg, App.appname, MessageBoxButton.OKCancel) == MessageBoxResult.Cancel) {
                    return -1;
                }
            }

            // コピー先に存在しない同期元ファイルを抽出
            var transFiles = storage.GetAllFiles(extension, vdata).Where(file => System.IO.File.Exists(destdir + file.fileName) == false);
            if (transFiles == null || transFiles.Count() == 0) {
                vdata.TotalTransCount = "同期対象のファイルはありません";
                return -1;
            }

            // コピー処理
            int ret = -1;
            vdata.TotalTransCount =String.Format("コピー中　{0}個の項目", transFiles.Count());
            vdata.DispInfo = vdata.TotalTransCount + "\n";
            string[] filedfile = storage.ExecCopyFile(destdir, transFiles, vdata);
            if (filedfile != null) {
                ret = 0;
                vdata.TotalTransCount = "コピー完了";
                vdata.DispInfo += String.Format("コピー完了 - 失敗{0}項目\n", filedfile.Count());
            }
            return ret;
        }
    }
}
