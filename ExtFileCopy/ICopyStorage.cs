using System;
using System.Collections.Generic;


namespace ExtFileCopy
{
    interface ICopyStorage
    {
        string GetTrueDirpath(string volume, string dirpath);
        string[] GetFilenames(string dirpath, string ext);
        string[] ExecCopyFile(string srcDirpath, string destDirath, IEnumerable<string> CopyFilenames, MainWindowData mainWnd);   

    }

    class ICopyStorageFactory
    {        
        public static ICopyStorage Create(string mediatype) {

            ICopyStorage inst = null;
            switch (mediatype) {
                case CopyFromRemovableStorage.mediatype:
                    inst = new CopyFromRemovableStorage();
                    break;
                case CopyFromMTPDevice.mediatype:
                    //inst = new CopyFromMTPDevice();
                    break;
                default:
                    inst = null;
                    Console.WriteLine("パラメタエラー");
                    break;
            }

            return inst;
        }
    }
}
