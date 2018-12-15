using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace ExtFileCopy { 

    class CopyFromRemovableStorage : ICopyStorage
    {

        public const string mediatype = "Removable";

        public string GetTrueDirpath(string volume, string dirpath){

            string TrueDirpath = null;

            // ボリューム名が一致するドライブのパスを取得
            DriveInfo[] drives = DriveInfo.GetDrives();
            foreach (DriveInfo drive in drives){

                if (drive.IsReady && drive.VolumeLabel == volume) {
                    
                    string temp = drive.Name.TrimEnd('\\') + dirpath;
                    if (Directory.Exists(temp)) {
                        TrueDirpath = temp;
                        break;
                    }
                }
            }
            
            return TrueDirpath;
        }

        public string[] GetFilenames(string dirpath, string ext){
            
            return Array.ConvertAll(Directory.GetFiles(dirpath, ext, SearchOption.AllDirectories),
                                    fname => Path.GetFileName(fname)); 
        }


        public string[] ExecCopyFile(string srcDirpath, string destDirath, IEnumerable<string> CopyFilenames, MainWindowData mainWnd){

            var filedfile = new List<string>();

            int cnt = 0;
            int total = CopyFilenames.Count();
            mainWnd.DispInfo += String.Format("コピーファイル数:{0}\n", total);
            foreach(string fname in CopyFilenames) {
                try {
                    File.Copy(srcDirpath + fname, destDirath + fname, false);
                    mainWnd.DispInfo += String.Format("成功：{0}\n", fname);
                    cnt++;
                }
                catch (Exception e) {
                    filedfile.Add(fname);
                    mainWnd.DispInfo += String.Format("失敗：{0} [{1}]\n", e.Message, fname);
                }

                mainWnd.Progress = (100 * cnt) / total;
            }

            return filedfile.ToArray();
        }
    }
}
