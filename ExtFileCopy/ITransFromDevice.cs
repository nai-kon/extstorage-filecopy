using System;
using System.Collections.Generic;


namespace ExtStorageTrans
{
    public class TransFileObject
    {
        public enum ObjectKind { ALL, DIR, FILE };
        public string fileName;
        public string objId;
        public DateTime updateTime;
        public ObjectKind kind;
        //public UInt64 size;
        public TransFileObject(string fname, string objid, DateTime updatetime, ObjectKind kind) {
            this.fileName = fname;
            this.objId = objid;
            this.updateTime = updatetime;
            this.kind = kind;
        }
    }
    
    interface ICopyStorage
    {
        bool FindSrcDir(string volume, string srcDirpath, out string retryMsg, MainWindowData mainWnd);
        IEnumerable<TransFileObject> GetAllFiles(string ext, MainWindowData mainWnd);
        string[] ExecCopyFile(string destDirpath, IEnumerable<TransFileObject> copyFiles, MainWindowData mainWnd);   

    }

    class ICopyStorageFactory
    {        
        public static ICopyStorage Create(string mediatype) {

            ICopyStorage inst = null;
            switch (mediatype) {
                case TransFromMTPDevice.mediatype:
                    inst = new TransFromMTPDevice();
                    break;
                case TransFromMSCDevice.mediatype:
                    inst = new TransFromMSCDevice();
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
