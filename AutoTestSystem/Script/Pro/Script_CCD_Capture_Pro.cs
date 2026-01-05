using AutoTestSystem.Base;
using AutoTestSystem.Equipment.CCD;
using AutoTestSystem.Equipment.ControlDevice;
using AutoTestSystem.Model;
using Manufacture;
using MvCamCtrl.NET;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Design;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static AutoTestSystem.BLL.Bd;

namespace AutoTestSystem.Script
{
    internal class Script_CCD_Capture_Pro : Script_CCD_Base
    {
        private Dictionary<string, string> outputdata = new Dictionary<string, string>();
        string strOutData = string.Empty;
        int Format;
        int errorcode;
        string SaveImagePath = string.Empty;
        private string _strParamInfoPath;
        private string _strFormat;
        private string _strFileName;
        string Status = string.Empty;
        string jsonStr = string.Empty;
        Stopwatch stopwatch = new Stopwatch();

        [Category("Exposure Time"), Description("ExposureEnable"), TypeConverter(typeof(ExposureFunction))]
        public string ExposureEnable { get; set; } = "OFF";

        [Category("Exposure Time"), Description("SetExposureTime 支援%%變數")]
        public string ExposureTime { get; set; } = "1000";

        [Category("DelayTime"), Description("DelayTime after SetExposureTime ")]
        public int ExposureDelayTime { get; set; } = 1000;

        [Category("FileCopyMethod"), Description("資料複製的方式"), TypeConverter(typeof(FileToCopy))]
        public string FileCopyMethod { get; set; } = "File.Copy";

        [Category("FileSave"), Description("存圖檔案名稱(不需要填寫副檔名)支援用%%方式做變數值取代")]
        //public string SaveFileName { get; set; } = "image01";
        public string SaveFileName
        {
            get {
                if (_strFileName == null)
                    return "image01";
                else
                    return _strFileName;                
            }
            set {
                _strFileName = value;
                CheckFormat();
            }
        }
        [Category("FileSave"), Description("存圖副檔名選擇"), TypeConverter(typeof(ImageFormate))]
        //public string SaveFormat { get; set; } = "jpg";
        public string SaveFormat
        {
            get
            {
                if (_strFormat == null)
                    return "jpg";
                else
                    return _strFormat;
            }
            set {               
                _strFormat = value;
                CheckFormat();
            }
        }
        [Category("FileSave"), Description("選擇存取影像文件夾"), Editor(typeof(FolderSelEditorRelPath), typeof(System.Drawing.Design.UITypeEditor))]
        public string SavePath { get; set; }
        [Category("DelayTime"), Description("DelayTime after FileSave")]
        public int FileSaveDelayTime { get; set; } = 500;

        [Category("FileBackup"), Description("備份檔案名稱(不需要填寫副檔名)(預設為%ProductSN%_%SaveFileName%_%StationName%_%FixtureName%_%NowTime%)支援用%%方式做變數值取代")]
        public string BackupFileName { get; set; } = "%ProductSN%_%SaveFileName%_%StationName%_%FixtureName%_%NowTime%";

            
        [Category("FileBackup"), Description("選擇存取備份影像文件夾\r\n(若為網路磁盤(Z:\\ H:\\等)，需再填寫 \"網路磁碟IP指定路徑\")"), Editor(typeof(FolderSelEditorRelPath), typeof(System.Drawing.Design.UITypeEditor))]
        public string SaveBackupPath { get; set; }
        
        [Category("FileBackup"), Description("網路磁碟IP指定路徑(\\\\xxx.xxx.xxx.xxx\\filename)"), Editor(typeof(NetWorkEditor), typeof(UITypeEditor))]
        public string Network_Path
        {
            get
            {
                GetNetworkpath(_strParamInfoPath);
                return _strParamInfoPath;
            }
            set { _strParamInfoPath = value; }
        }

        public override void Dispose()
        {
            //throw new NotImplementedException();
        }
        public override bool PreProcess()
        {
            GetNetworkpath(Network_Path);
            SaveImagePath = string.Empty;
            Format = 0;
            errorcode = 99;
            Status = "Waiting";
            jsonStr = string.Empty;
            outputdata.Clear();
            return true;
        }

        public override bool Process(CCDBase CCD,ref string strOutData)
        {
            /*if (!CCD.Start())
            {
                return false;
            }  */
            string strExOutData = string.Empty;
            string Backup_Path = string.Empty;
            string NetworkDrvepath = string.Empty;
            string FileNameReplace = string.Empty;
            string BackupFileNameReplace = string.Empty;
            string TotalBackupFileName = string.Empty;
            bool Ex_Ret = true;
            bool pass_fail = true;

            //SetExposureTime
            if (ExposureEnable == "ON")
            {
                Ex_Ret = SetExposureTime(CCD, ref strExOutData);
                if (!Ex_Ret)
                {
                    strOutData = strExOutData;
                    return false;
                }
                Sleep(ExposureDelayTime);
            }
                       
            if (SavePath !=string.Empty && SavePath != null) 
            {
                string ReplaceSavePath = ReplaceProp(SavePath);
                //SaveCCDImage
                outputdata.Add("SavePath", Path.GetFullPath(ReplaceSavePath));
                //檢查路徑是否存在
                if (!Directory.Exists(Path.GetFullPath(ReplaceSavePath)))
                {
                    Status = "SavePath_not_exist";
                    errorcode = -2;
                    LogMessage($"SavePath is not exist!!", MessageLevel.Warn);
                    pass_fail =  false;
                }else
                {
                    if (SaveFileName != null && SaveFileName != string.Empty)
                    {
                        FileNameReplace = ReplaceProp(SaveFileName);
                        //Check FileName Subfile is exist or not
                        if (FileNameReplace.Contains(SaveFormat))
                            PushMoreData("SaveFileName", FileNameReplace.Replace($".{SaveFormat}", ""));
                        else
                            PushMoreData("SaveFileName", FileNameReplace);

                        LogMessage($"{Description} The Backup FileName is {FileNameReplace}", MessageLevel.Debug);
                        outputdata.Add("FileName", FileNameReplace);
                        if (SaveFormat == "bmp")
                        {
                            Format = 0;
                        }
                        else if (SaveFormat == "jpg")
                        {
                            Format = 1;
                        }
                        else
                        {
                            LogMessage($"Format Error: Format Undefinded!!!", MessageLevel.Error);
                            return false;
                        }

                        

                        if (FileNameReplace.Contains(SaveFormat))
                            SaveImagePath = ReplaceSavePath + "/" + FileNameReplace;
                        else
                            SaveImagePath = ReplaceSavePath + "/" + FileNameReplace + "." + SaveFormat;


                        // 嘗試刪除舊檔案（如果存在）
                        try
                        {
                            if (System.IO.File.Exists(SaveImagePath))
                            {
                                System.IO.File.Delete(SaveImagePath);
                            }
                        }
                        catch (Exception ex)
                        {
                            LogMessage("Delete file failed: " + ex.Message, MessageLevel.Debug);
                        }


                        if (!CCD.SaveImage(Format, SaveImagePath))
                        {
                            Status = "CCD_SaveNG";
                            errorcode = -4;
                            LogMessage("CCD.SaveImage Fail", MessageLevel.Debug);
                            pass_fail = false;
                        }
                        else
                        {
                            Sleep(FileSaveDelayTime);
                            if (!File.Exists(SaveImagePath))//確認檔案是否存在
                            {
                                Status = $"ImageFileName:{Path.GetFileName(SaveImagePath)}_not_exist";
                                errorcode = -5;
                                LogMessage( Status, MessageLevel.Debug);
                                pass_fail = false;
                            }
                            else
                            {
                                if (SaveBackupPath != null && SaveBackupPath != string.Empty)
                                {
                                    string ReplaceSaveBackupPath = ReplaceProp(SaveBackupPath);
                                    outputdata.Add("BackupPath", Path.GetFullPath(ReplaceSaveBackupPath));

                                    if (BackupFileName == null || BackupFileName == string.Empty)
                                        TotalBackupFileName = PopMoreData("ProductSN") + DateTime.Now.ToString("_yyyyMMdd_HHmmss");
                                    else
                                    {
                                        BackupFileNameReplace = ReplaceProp(BackupFileName);
                                        //Check BackupFile Subfile is exist or not
                                        if (BackupFileNameReplace.Contains(SaveFormat))
                                            TotalBackupFileName = BackupFileNameReplace;
                                        else
                                            TotalBackupFileName = BackupFileNameReplace + "." + SaveFormat;
                                    }
                                    LogMessage($"{Description} The Backup FileName is {TotalBackupFileName}", MessageLevel.Debug);
                                    outputdata.Add("BackupFileName", TotalBackupFileName);

                                    //DirectoryInfo di = null;
                                    if (IsNetworkDrive(ReplaceSaveBackupPath, ref NetworkDrvepath))
                                    {
                                        if (NetworkDrvepath != string.Empty)
                                            Backup_Path = NetworkDrvepath + "/" + TotalBackupFileName;
                                        else
                                            Backup_Path = ReplaceSaveBackupPath + "/" + TotalBackupFileName;

                                        try
                                        {
                                            int timeoutMilliseconds = 10000;//10秒
                                                                           // 使用 Task.Run 來執行複製操作，並設定 CancellationTokenSource 來處理 timeout
                                            using (var cancellationTokenSource = new CancellationTokenSource())
                                            {
                                                var cancellationToken = cancellationTokenSource.Token;
                                                var copyTask = Task.Run(() =>
                                                {
                                                    FileCopy(SaveImagePath, Backup_Path, FileCopyMethod);
                                                }, cancellationToken);
                                                if (!copyTask.Wait(timeoutMilliseconds, cancellationToken))
                                                {
                                                    cancellationTokenSource.Cancel(); // 如果超時，取消複製操作
                                                    LogMessage($"Copy File is Timout: [{timeoutMilliseconds / 1000}]s。", MessageLevel.Warn);
                                                }
                                                LogMessage($"File.Copy to {Backup_Path}", MessageLevel.Debug);
                                            }

                                            //FileCopy(SaveImagePath, Backup_Path, FileCopyMethod);
                                            // 確認來源圖片與目標圖片是否相同
                                            bool areImagesIdentical = AreFilesIdentical(SaveImagePath, Backup_Path);
                                            if(!areImagesIdentical)
                                            {
                                                LogMessage($"The source image is not the same as the Backup destination Image!!", MessageLevel.Error);
                                                return false;
                                            }

                                        }
                                        catch (Exception ex)
                                        {
                                            LogMessage($"File.Copy Error => {ex.Message}, Please Check SavePath:{SaveImagePath} and BackupPath:{Backup_Path}", MessageLevel.Error);
                                            Status = "File.Copy_Error";
                                            outputdata.Add("Exception", ex.Message);
                                            errorcode = -9;
                                            pass_fail = false;
                                        }

                                    }
                                    else
                                    {
                                        //LogMessage($"Backup Path:{SaveBackupPath} is not exist or lossing,Please Re-select Backup Path and retry again!!!", MessageLevel.Debug);
                                        Status = "SaveBackupPath_NG";
                                        errorcode = -6;
                                        MessageBox.Show($"[{Description}] Backup Path:{ReplaceSaveBackupPath} is not exist or lossing,Please Re-select Backup Path and retry again!!!", "Path Check", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                        pass_fail = false;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        Status = "FileName_null";
                        errorcode = -3;
                        LogMessage($"FileName is Empty!!!", MessageLevel.Debug);
                        pass_fail = false;
                    }                  
                }
            }else
            {
                Status = "SavePath_is_empty";
                errorcode = -1;
                LogMessage($"SavePath is empty!!", MessageLevel.Warn);
                pass_fail = false;
            }

            if(pass_fail)
            {
                errorcode = 0;
                Status = "Save_OK";
            }
            outputdata.Add("CCD_SaveImage_Status[Errorcode]", $"{Status}[{errorcode}]");
            outputdata.Add("errorCode", $"{errorcode}");
            strOutData = JsonConvert.SerializeObject(outputdata, Formatting.Indented);
            jsonStr = strOutData;
            LogMessage($"Script_CCD_SaveImage:  {strOutData}");
            return pass_fail;
        }
        public override bool PostProcess()
        {
            if (Spec != string.Empty && Spec != null)
            {
                string ret = string.Empty;             
                ret = CheckRule(jsonStr, Spec);
                LogMessage($"CheckRule: {ret}", MessageLevel.Debug);
                if (ret == "PASS")
                    return true;
                else
                    return false;
            }
            return true;

        }

        public bool SetExposureTime(CCDBase CCD, ref string strExOutData)
        {
            bool ex_pass_fail = true;
            string ReplaceExposureTime = ReplaceProp(ExposureTime);
            if(string.IsNullOrEmpty(ReplaceExposureTime))
            {
                LogMessage($"ReplaceExposureTime is null or Empty", MessageLevel.Warn);
                return false;
            }
            PushMoreData("ExposureTime", ReplaceExposureTime);
            LogMessage($"{Description}:Set_Exposure({ReplaceExposureTime})", MessageLevel.Debug);
            outputdata.Add("ExposureTime", ReplaceExposureTime);
            if (!CCD.Set_Exposure(int.Parse(ReplaceExposureTime)))
            {
                Status = "Set_Exposure_error";                
                LogMessage($"Script_CCD_SetExParam:  {Status}");
                ex_pass_fail= false;
            }
            if(ex_pass_fail)
                Status = "Set_Exposure_OK";

            outputdata.Add("CCD_SetExParam_Status", Status);
            strExOutData = JsonConvert.SerializeObject(outputdata, Formatting.Indented);
            LogMessage($"Script_CCD_SetParam:  {strOutData}", MessageLevel.Debug);
            return ex_pass_fail;
        }
        public void FileCopy(string sourceFilePath, string targetFilePath,string FileCopyMethod)
        {
            stopwatch.Start();
            if (FileCopyMethod == "File.Copy")
                File.Copy(sourceFilePath, targetFilePath);
            else
            {
                using (var sourceStream = new FileStream(sourceFilePath, FileMode.Open))
                {
                    using (var targetStream = new FileStream(targetFilePath, FileMode.Create))
                    {
                        using (var bufferedSource = new BufferedStream(sourceStream))
                        {
                            using (var bufferedTarget = new BufferedStream(targetStream))
                            {
                                bufferedSource.CopyTo(bufferedTarget);
                            }
                        }
                    }
                }
            }
            stopwatch.Stop();
            LogMessage($"Copy File cost:{stopwatch.Elapsed}!!", MessageLevel.Debug);
            stopwatch.Reset();
        }

        public bool AreFilesIdentical(string filePath1, string filePath2)
        {
            try
            {
                if (!File.Exists(filePath1))
                {
                    LogMessage($"{filePath1} is not Exists!!,Please Check Soruce File", MessageLevel.Error);
                    return false;
                }
                if (!File.Exists(filePath2))
                {
                    LogMessage($"{filePath2} is not Exists!! Copy Fail", MessageLevel.Error);
                    return false;
                }
                using (var hashAlgorithm = SHA256.Create()) //SHA - 256 雜湊值
                {
                    byte[] hash1 = ComputeFileHash(filePath1, hashAlgorithm);
                    byte[] hash2 = ComputeFileHash(filePath2, hashAlgorithm);

                    return StructuralComparisons.StructuralEqualityComparer.Equals(hash1, hash2);
                }
            }catch(Exception File_ex)
            {
                LogMessage($"FilesIdentical Error : {File_ex.Message}", MessageLevel.Error);
                return false;
            }
        }

        static byte[] ComputeFileHash(string filePath, HashAlgorithm hashAlgorithm)
        {
            using (var stream = File.OpenRead(filePath))
            {
                return hashAlgorithm.ComputeHash(stream);
            }
        }

        public void CheckFormat()
        {
            //Check Filesubfile is eqeal with SaveFormat or not
            if (string.IsNullOrEmpty(SaveFileName))
                SaveFileName="image01";
            if (string.IsNullOrEmpty(SaveFormat))
                SaveFormat="jpg";
            else
            {
                string CheckFormat = $".{SaveFormat}";
                if (!CheckFormat.Contains(Path.GetExtension(SaveFileName)))
                {
                    MessageBox.Show($"Format Error: FileName subfile {Path.GetExtension(SaveFileName)} is not eqeal with SaveFormat {CheckFormat}!!!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }          
        }
        public class FileToCopy : TypeConverter  //下拉式選單
        {
            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {

                
                    List<string> CopyListKeys = new List<string>();
                 
                   CopyListKeys.Add("File.Copy");
                   CopyListKeys.Add("BufferedStream.Copy");

                    return new StandardValuesCollection(CopyListKeys);
                
            }

            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                return true;

            }

        }

        public class ImageFormate : TypeConverter  //下拉式選單
        {
            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {


                List<string> ImageFormatKeys = new List<string>();

                ImageFormatKeys.Add("bmp");
                ImageFormatKeys.Add("jpg");

                return new StandardValuesCollection(ImageFormatKeys);

            }

            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                return true;

            }

        }

        public class ExposureFunction : TypeConverter  //下拉式選單
        {
            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {


                List<string> ExposureFunction = new List<string>();

                ExposureFunction.Add("ON");
                ExposureFunction.Add("OFF");

                return new StandardValuesCollection(ExposureFunction);

            }

            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                return true;

            }

        }
    }
}
