using AutoTestSystem.Base;
using AutoTestSystem.Equipment.CCD;
using AutoTestSystem.Equipment.ControlDevice;
using AutoTestSystem.Model;
using Manufacture;
using MvCamCtrl.NET;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Design;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static AutoTestSystem.BLL.Bd;

namespace AutoTestSystem.Script
{
    internal class Script_CCD_SaveImage_Pro : Script_CCD_Base
    {
        string strOutData = string.Empty;
        int Formate;
        int errorcode;
        string SaveImagePath = string.Empty;
        private string _strParamInfoPath;
        string Status = string.Empty;       
        Stopwatch stopwatch = new Stopwatch();

        [Category("Common Parameters"), Description("存圖檔案名稱(預設為image01)支援用%%方式做變數值取代")]
        public string FileName { get; set; } = "image01";
        [Category("Common Parameters"), Description("存圖格式選擇0:bmp 1:jpg"), TypeConverter(typeof(ImageFormate))]
        public string SaveFormat { get; set; } = "jpg";
        [Category("Common Parameters"), Description("選擇存取影像文件夾"), Editor(typeof(FolderSelEditorRelPath), typeof(System.Drawing.Design.UITypeEditor))]
        public string SavePath { get; set; }      
        [Category("Common Parameters"), Description("選擇存取備份影像文件夾\r\n(若為網路磁盤(Z:\\ H:\\等)，需再填寫 \"網路磁碟IP指定路徑\")"), Editor(typeof(FolderSelEditorRelPath), typeof(System.Drawing.Design.UITypeEditor))]
        public string SaveBackupPath { get; set; }
        [Category("Common Parameters"), Description("資料複製的方式"), TypeConverter(typeof(FileToCopy))]
        public string FileCopyMethod { get; set; } = "File.Copy";
        [Category("Common Parameters"), Description("網路磁碟IP指定路徑(\\\\xxx.xxx.xxx.xxx\\filename)"), Editor(typeof(NetWorkEditor), typeof(UITypeEditor))]
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
            Formate = 0;
            errorcode = 99;
            Status = "Waiting";        
            return true;
        }

        public override bool Process(CCDBase CCD,ref string strOutData)
        {
            /*if (!CCD.Start())
            {
                return false;
            }  */
            string BackupPath = string.Empty;
            string NetworkDrvepath = string.Empty;
            string FileNameReplace = string.Empty;
            var outputdata = new Dictionary<string, object>();
            bool pass_fail = true;
                      
            if (SavePath !=string.Empty && SavePath != null) 
            {
                outputdata.Add("SavePath", Path.GetFullPath(SavePath));
                //檢查路徑是否存在
                if (!Directory.Exists(Path.GetFullPath(SavePath)))
                {
                    Status = "SavePath_not_exist";
                    errorcode = -2;
                    LogMessage($"SavePath is not exist!!", MessageLevel.Warn);
                    pass_fail =  false;
                }else
                {
                    if (FileName != null)
                    {
                        FileNameReplace = ReplaceProp(FileName);
                        LogMessage($"{Description} The TestItem Name is {FileNameReplace}", MessageLevel.Debug);
                        outputdata.Add("FileName", FileNameReplace);

                        if (SaveFormat == "bmp")
                        {
                            Formate = 0;
                        }
                        else if (SaveFormat == "jpg")
                        {
                            Formate = 1;
                        }
                        else
                        {
                            LogMessage($"Formate Error!!!", MessageLevel.Error);
                            return false;
                        }
                        SaveImagePath = SavePath + "/" + FileNameReplace + "." + SaveFormat;

                        if (!CCD.SaveImage(Formate, SaveImagePath))
                        {
                            Status = "CCD_SaveNG";
                            errorcode = -4;
                            LogMessage("CCD.SaveImage Fail", MessageLevel.Debug);
                            pass_fail = false;
                        }
                        else
                        {
                            Sleep(800);
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
                                    outputdata.Add("BackupPath", Path.GetFullPath(SaveBackupPath));
                                    //DirectoryInfo di = null;
                                    if (IsNetworkDrive(SaveBackupPath, ref NetworkDrvepath))
                                    {
                                        if (NetworkDrvepath != string.Empty)
                                        {
                                            if(!FileName.Contains("%"))
                                                BackupPath = NetworkDrvepath + "/" + PopMoreData("ProductSN") + "_" + FileName + DateTime.Now.ToString("_yyyyMMdd_HHmmss") + "." + SaveFormat;
                                            else
                                                BackupPath = NetworkDrvepath + "/" + FileNameReplace + "." + SaveFormat;
                                        }
                                        else
                                        {
                                            if (!FileName.Contains("%"))
                                                BackupPath = SaveBackupPath + "/" + PopMoreData("ProductSN") + "_" + FileName + DateTime.Now.ToString("_yyyyMMdd_HHmmss") + "." + SaveFormat;
                                            else
                                                BackupPath = SaveBackupPath + "/" + FileNameReplace + "." + SaveFormat;
                                        }

                                        try
                                        {
                                            int timeoutMilliseconds = 5000;//5秒
                                                                           // 使用 Task.Run 來執行複製操作，並設定 CancellationTokenSource 來處理 timeout
                                            using (var cancellationTokenSource = new CancellationTokenSource())
                                            {
                                                var cancellationToken = cancellationTokenSource.Token;
                                                var copyTask = Task.Run(() =>
                                                {
                                                    FileCopy(SaveImagePath, BackupPath, FileCopyMethod);
                                                }, cancellationToken);
                                                if (!copyTask.Wait(timeoutMilliseconds, cancellationToken))
                                                {
                                                    cancellationTokenSource.Cancel(); // 如果超時，取消複製操作
                                                    LogMessage($"Copy File is Timout: [{timeoutMilliseconds / 1000}]s。", MessageLevel.Warn);
                                                }
                                                LogMessage($"File.Copy to {BackupPath}", MessageLevel.Debug);
                                            }


                                        }
                                        catch (Exception ex)
                                        {
                                            LogMessage($"File.Copy Error => {ex.Message}, Please Check SavePath:{SaveImagePath} and BackupPath:{BackupPath}", MessageLevel.Error);
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
                                        MessageBox.Show($"[{Description}] Backup Path:{SaveBackupPath} is not exist or lossing,Please Re-select Backup Path and retry again!!!", "Path Check", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
            outputdata.Add("CCD_SaveImage_Status(errorCode)", $"{Status}[{errorcode}]");
            strOutData = JsonConvert.SerializeObject(outputdata, Formatting.Indented);
            LogMessage($"Script_CCD_SaveImage:  {strOutData}");
            return pass_fail;
        }
        public override bool PostProcess()
        {
            if (Spec != string.Empty && Spec != null)
            {
                string ret = string.Empty;
                string jsonStr = string.Empty;

                var output_data = new Dictionary<string, object>
                        {
                            { "errorCode", errorcode }
                        };
                try
                {
                    jsonStr = JsonConvert.SerializeObject(output_data, Formatting.Indented);
                    LogMessage($"output_data: {jsonStr}", MessageLevel.Debug);
                }
                catch (Exception e1)
                {
                    LogMessage($"Error: {jsonStr}=>{e1.Message}", MessageLevel.Error);
                    return false;
                }

                ret = CheckRule(jsonStr, Spec);
                LogMessage($"CheckRule: {ret}", MessageLevel.Debug);
                if (ret == "PASS")
                    return true;
                else
                    return false;
            }
            return true;

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


                List<string> CopyListKeys = new List<string>();

                CopyListKeys.Add("bmp");
                CopyListKeys.Add("jpg");

                return new StandardValuesCollection(CopyListKeys);

            }

            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                return true;

            }

        }     
    }
}
