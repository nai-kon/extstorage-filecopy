using System;
using System.Collections.Generic;


namespace ExtFileCopy
{
    public class CopyFileObject
    {
        public string fileName;
        public string objId;
        //public UInt64 size;
        //public FILETIME
        public CopyFileObject(string fname, string objid) {
            this.fileName = fname;
            this.objId = objid;
        }
    }

    interface ICopyStorage
    {
        bool FindSrcDir(string volume, string srcDirpath, out string retryMsg, MainWindowData mainWnd);
        IEnumerable<CopyFileObject> GetAllFiles(string ext, MainWindowData mainWnd);
        string[] ExecCopyFile(string destDirpath, IEnumerable<CopyFileObject> copyFiles, MainWindowData mainWnd);   

    }

    class ICopyStorageFactory
    {        
        public static ICopyStorage Create(string mediatype) {

            ICopyStorage inst = null;
            switch (mediatype) {
                case CopyFromRemovableStorage.mediatype:
                    inst = new CopyFromRemovableStorage();
                    break;
                case CopyFromPortableDevice.mediatype:
                    inst = new CopyFromPortableDevice();
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
