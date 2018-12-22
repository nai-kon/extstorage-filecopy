using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using PortableDeviceApiLib;
using PortableDeviceTypesLib;
using _tagpropertykey = PortableDeviceApiLib._tagpropertykey;
using IPortableDeviceKeyCollection = PortableDeviceApiLib.IPortableDeviceKeyCollection;
using IPortableDeviceValues = PortableDeviceApiLib.IPortableDeviceValues;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;

namespace ExtStorageTrans
{

    class TransFromMTPDevice : ICopyStorage
    {
        public const string mediatype = "MTP";
        private PortableDeviceManager deviceManager;
        private string srcDevId = null;
        private string srcDirObjId = null;

        public TransFromMTPDevice() {
            deviceManager = new PortableDeviceManager();
        }

        public bool FindSrcDir(string volume, string dirpath, out string retryMsg, MainWindowData mainWnd) {
            
            const string defretryMsg = "機器を接続してOKを選択してください";
            retryMsg = defretryMsg;          

            // 接続中のデバイス数を取得
            uint count = 0;
            deviceManager.RefreshDeviceList();
            deviceManager.GetDevices(null, ref count);
            if (count == 0) {
                retryMsg = defretryMsg;
                return false;
            }

            // コピー元デバイス、フォルダの存在チェック
            string[] deviceIds = new string[count];
            PortableDeviceClass[] devices = new PortableDeviceClass[count];
            deviceManager.GetDevices(deviceIds, ref count);

            var clientInfo = (IPortableDeviceValues) new PortableDeviceValuesClass();
            bool existsSrcDir = true;
            foreach (var deviceId in deviceIds) {

                PortableDeviceClass device = new PortableDeviceClass();
                try {
                    // デバイス情報の取得
                    IPortableDeviceContent content;
                    IPortableDeviceProperties properties;
                    device.Open(deviceId, clientInfo);
                    device.Content(out content);
                    content.Properties(out properties);

                    IPortableDeviceValues propertyValues;
                    properties.GetValues("DEVICE", null, out propertyValues);

                    var property = new _tagpropertykey();
                    string devicename;
                    property.fmtid = new Guid(0xEF6B490D, 0x5CD8, 0x437A, 0xAF, 0xFC, 0xDA, 0x8B, 0x60, 0xEE, 0x4A, 0x3C);
                    property.pid = 4;
                    propertyValues.GetStringValue(property, out devicename);
                    //Console.WriteLine(devicename);

                    // 対象デバイスで無ければスキップ
                    if (devicename != volume) continue;

                    // コピー元デバイスのアクセス許可状態のチェック
                    var rootObjs = GetObjects("DEVICE", content); // ルートオブジェクトが見えるか
                    if(rootObjs.Count() <= 0) {
                        retryMsg = "対象機器へのアクセスが許可されていません\n"+
                                    "対象機器でMTP転送を有効後にOKを選択してください"; 
                        return false;
                    }

                    // コピー元フォルダのデバイスIDを取得
                    existsSrcDir = true;
                    string curDirId = "DEVICE";
                    foreach (string searchdir in dirpath.TrimStart('\\').TrimEnd('\\').Split('\\')){
                        //Console.WriteLine(searchdir);
                        bool existCurDir = false;
                        var dirObjs = GetObjects(curDirId, content, TransFileObject.ObjectKind.DIR);

                        foreach (var dirobj in dirObjs) {
                            if (dirobj.fileName == searchdir) {
                                existCurDir = true;
                                curDirId = dirobj.objId;
                                break;
                            }
                        }
                        if (!existCurDir) {
                            existsSrcDir = false;
                            break;
                        }
                    }
                    if (existsSrcDir) {
                        srcDirObjId = curDirId;
                        srcDevId = deviceId;
                        retryMsg = "";
                        break;
                    }               
                }
                catch (Exception e) {
                    mainWnd.DispInfo = string.Format("コピー元の検出失敗\n{0}", e.Message);
                    existsSrcDir = false;
                }
                finally {
                    device.Close();
                }
            }
            
            return existsSrcDir;
        }


        public IEnumerable<TransFileObject> GetAllFiles(string ext, MainWindowData mainWnd) {

            if (srcDevId == null || srcDirObjId == null) return null;

            var files = new List<TransFileObject>();
            PortableDeviceClass device = new PortableDeviceClass();
            try {
                IPortableDeviceContent content;
                IPortableDeviceProperties properties;
                var clientInfo = (IPortableDeviceValues)new PortableDeviceValuesClass();
                device.Open(srcDevId, clientInfo);
                device.Content(out content);
                content.Properties(out properties);
                
                // コピー対象の拡張子と一致するファイルを抽出
                var patExt = new Regex("." + ext, RegexOptions.Compiled);
                var srcfiles = GetObjects(srcDirObjId, content, TransFileObject.ObjectKind.FILE);                               
                foreach (var srcfile in srcfiles) {

                    if (patExt.IsMatch(srcfile.fileName)) {
                        files.Add(new TransFileObject(srcfile.fileName, srcfile.objId, srcfile.updateTime, srcfile.kind));
                    }
                }
            }
            catch (Exception e) {
                mainWnd.DispInfo = string.Format("エラーが発生しました\n{0}", e.Message);
            }
            finally {
                device.Close();
            }
            return files;
        }


        public string[] ExecCopyFile(string destDirpath, IEnumerable<TransFileObject> copyFiles, MainWindowData mainWnd) {

            if (srcDevId == null || srcDirObjId == null) return null;

            var filedfile = new List<string>();
            int cnt = 0;
            int total = copyFiles.Count();
            PortableDeviceClass device = new PortableDeviceClass();

            IPortableDeviceContent content;
            var clientInfo = (IPortableDeviceValues) new PortableDeviceValuesClass();
            device.Open(srcDevId, clientInfo);
            device.Content(out content);

            foreach (var file in copyFiles) {
                try {
                    DownloadFile(file, destDirpath + file.fileName, content);
                    mainWnd.DispInfo += String.Format("成功：{0}\n", file.fileName);
                    cnt++;
                }
                catch (Exception e) {
                    filedfile.Add(file.fileName);
                    mainWnd.DispInfo += String.Format("失敗：{0} [{1}]\n", e.Message, file.fileName);
                }

                mainWnd.Progress = (100 * cnt) / total;
            }

            device.Close();
            return filedfile.ToArray();
        }


        private List<TransFileObject> GetObjects(string parentDirId, IPortableDeviceContent content, TransFileObject.ObjectKind kindFilter = TransFileObject.ObjectKind.ALL) {

            var retObjs = new List<TransFileObject>();

            IPortableDeviceProperties properties;
            content.Properties(out properties);
            
            IEnumPortableDeviceObjectIDs objectIDs;
            content.EnumObjects(0, parentDirId, null, out objectIDs);

            // オブジェクトを取得
            string objectID;
            uint fetched = 0;
            while (true) {
                objectIDs.Next(1, out objectID, ref fetched);
                if (fetched <= 0) break;

                TransFileObject currentObject = WrapObject(properties, objectID);
                if (kindFilter == TransFileObject.ObjectKind.ALL || currentObject.kind == kindFilter) {
                    retObjs.Add(currentObject);
                }
            }

            return retObjs;
        }


        private TransFileObject WrapObject(IPortableDeviceProperties properties, string objectId) {
            
            IPortableDeviceKeyCollection keys;
            properties.GetSupportedProperties(objectId, out keys);

            IPortableDeviceValues values;
            properties.GetValues(objectId, keys, out values);

            // Get the name of the object
            string name;
            var property = new _tagpropertykey();
            property.fmtid = new Guid(0xEF6B490D, 0x5CD8, 0x437A, 0xAF, 0xFC, 0xDA, 0x8B, 0x60, 0xEE, 0x4A, 0x3C);
            property.pid = 4;

            try {
                values.GetStringValue(property, out name);
            }
            catch (COMException e) {
                name = "(non name)";
            }

            // Get the original name of the object
            string OriginalName;
            property = new _tagpropertykey();
            property.fmtid = new Guid(0xEF6B490D, 0x5CD8, 0x437A, 0xAF, 0xFC, 0xDA, 0x8B, 0x60, 0xEE, 0x4A, 0x3C);
            property.pid = 12;
            try {
                values.GetStringValue(property, out OriginalName);
            }
            catch (COMException e) {
                OriginalName = "";
            }

            // Get last write time
            DateTime updatetime = DateTime.Now;
            property = new _tagpropertykey();
            property.fmtid = new Guid(0xEF6B490D, 0x5CD8, 0x437A, 0xAF, 0xFC, 0xDA, 0x8B, 0x60, 0xEE, 0x4A, 0x3C);
            property.pid = 19;
            try {
                float value;
                values.GetFloatValue(property, out value);
                updatetime = DateTime.FromOADate(value);
            }
            catch (COMException e) {
                //updatetime
            }

            // Get the type of the object
            Guid contentType;
            property = new _tagpropertykey();
            property.fmtid = new Guid(0xEF6B490D, 0x5CD8, 0x437A, 0xAF, 0xFC, 0xDA, 0x8B, 0x60, 0xEE, 0x4A, 0x3C);
            property.pid = 7;
            try {
                values.GetGuidValue(property, out contentType);
            }
            catch (COMException e) {
                return new TransFileObject(name, null, updatetime, TransFileObject.ObjectKind.DIR);
            }
            
            Guid folderType = new Guid(0x27E2E392, 0xA111, 0x48E0, 0xAB, 0x0C, 0xE1, 0x77, 0x05, 0xA0, 0x5F, 0x85);
            Guid functionalType = new Guid(0x99ED0160, 0x17FF, 0x4C44, 0x9D, 0x98, 0x1D, 0x7A, 0x6F, 0x94, 0x19, 0x21);

            if (contentType == folderType || contentType == functionalType) {
                return new TransFileObject(name, objectId, updatetime, TransFileObject.ObjectKind.DIR);
            }

            if (OriginalName.CompareTo("") != 0) {
                name = OriginalName;
            }

            return new TransFileObject(name, objectId, updatetime, TransFileObject.ObjectKind.FILE);
        }
        

        private void DownloadFile(TransFileObject file, string destPath, IPortableDeviceContent content) {

            IPortableDeviceProperties properties;
            content.Properties(out properties);

            var downloadFileObj = WrapObject(properties, file.objId);

            IPortableDeviceResources resources;
            content.Transfer(out resources);

            PortableDeviceApiLib.IStream wpdStream;
            uint optimalTransferSize = 0;

            var property = new _tagpropertykey();
            property.fmtid = new Guid(0xE81E79BE, 0x34F0, 0x41BF, 0xB5, 0x3F, 0xF1, 0xA0, 0x6A, 0xE8, 0x78, 0x42);
            property.pid = 0;

            resources.GetStream(file.objId, ref property, 0, ref optimalTransferSize, out wpdStream);
            System.Runtime.InteropServices.ComTypes.IStream sourceStream = (System.Runtime.InteropServices.ComTypes.IStream)wpdStream;

            FileStream targetStream = new FileStream(destPath, FileMode.Create, FileAccess.Write);

            unsafe {
                var buffer = new byte[10240];
                int bytesRead;
                do {
                    sourceStream.Read(buffer, 10240, new IntPtr(&bytesRead));
                    if (bytesRead <= 0)
                        break;
                    targetStream.Write(buffer, 0, bytesRead);
                } while (true/*bytesRead > 0*/);

                targetStream.Close();
            }

            Marshal.ReleaseComObject(sourceStream);
            Marshal.ReleaseComObject(wpdStream);

            // ファイルの更新日時を更新
            File.SetLastWriteTime(destPath, file.updateTime);
        }

    }
}
