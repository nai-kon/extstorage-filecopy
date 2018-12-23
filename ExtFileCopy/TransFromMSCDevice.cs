using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace ExtStorageTrans { 

    class TransFromMSCDevice : ICopyStorage
    {
        public const string mediatype = "MSC";
        private string srcdir = null;


        public bool FindSrcDir(string volume, string srcDirpath, out string retryMsg, MainWindowData mainWnd) {

            bool ret = false;
            retryMsg = "メディアを挿入してOKを選択してください";

            // ボリューム名が一致するドライブのパスを取得
            DriveInfo[] drives = DriveInfo.GetDrives();
            foreach (DriveInfo drive in drives){

                if (drive.IsReady && drive.VolumeLabel == volume) {
                    
                    string temp = drive.Name.TrimEnd('\\') + srcDirpath;
                    if (Directory.Exists(temp)) {
                        ret = true;
                        srcdir = temp;
                        break;
                    }
                }
            }
            
            return ret;
        }

        public IEnumerable<TransFileObject> GetAllFiles(string ext, MainWindowData mainWnd) {

            if (srcdir == null) return null;

            var ret = new List<TransFileObject>();
            foreach(var filename in Directory.GetFiles(srcdir, ext, SearchOption.AllDirectories)) {
                // MSCモードでは更新日時は使わないので、ひとまず現在時刻にする
                ret.Add(new TransFileObject(Path.GetFileName(filename), null, DateTime.Now, TransFileObject.ObjectKind.FILE));
            }

            return ret;
        }


        public string[] ExecCopyFile(string destDirpath, IEnumerable<TransFileObject> copyFiles, MainWindowData mainWnd){

            if (srcdir == null) return null;

            var filedfile = new List<string>();

            int cnt = 0;
            int total = copyFiles.Count();
            foreach(var file in copyFiles) {
                try {
                    File.Copy(srcdir + file.fileName, destDirpath + file.fileName, false);
                    mainWnd.DispInfo += String.Format("成功：{0}\n", file.fileName);
                    cnt++;
                }
                catch (Exception e) {
                    filedfile.Add(file.fileName);
                    mainWnd.DispInfo += String.Format("失敗：{0} [{1}]\n", e.Message, file.fileName);
                }

                mainWnd.Progress = (100 * cnt) / total;
            }

            return filedfile.ToArray();
        }
    }
}
