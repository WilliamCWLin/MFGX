
using AutoTestSystem.Base;
using AutoTestSystem.Model;
using Manufacture;
using NAudio.Wave;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static AutoTestSystem.BLL.Bd;

namespace AutoTestSystem.Script
{
    internal class Script_ControlDevice_AudioRecord_Pro : Script_ControlDevice_Base
    {
        string strOutData = string.Empty;
        string Status = string.Empty;
        private string _strParamInfoPath;
        string jsonStr = string.Empty;
        //int DelayTime;
        int errorcode;

        [Category("Choice Audio Mode"), Description("Choice Audio Mode")]
        public Audio_Mode AudioMode { get; set; } = Audio_Mode.Micophone;

        [Category("RecordTime"), Description("RecordTime")]
        public int RecordTime { get; set; } = 2000;

        [Category("SaveWavFile"), Description("選取存取的文件夾"), Editor(typeof(FolderSelEditorRelPath), typeof(System.Drawing.Design.UITypeEditor))]
        public string OutputWavPath { get; set; }        
        [Category("SaveWavFile"), Description("存取檔案名稱(預設為Output)支援用%%方式做變數值取代")]
        public string FileName { get; set; } = "Output";
        [Category("FileBackup"), Description("選取存取備份檔案的文件夾\r\n(若為網路磁盤(Z:\\ H:\\等)，需再填寫 \"網路磁碟IP指定路徑\")"), Editor(typeof(FolderSelEditorRelPath), typeof(System.Drawing.Design.UITypeEditor))]
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
            strOutData = string.Empty;
            Status = "Waiting";
            jsonStr = string.Empty;
            errorcode = -99;
            return true;
        }
        public override bool Process(ControlDeviceBase WaveDevice,ref string strOutData)
        {
            string BackupPath = string.Empty;
            string FileNameReplace = string.Empty;
            string NetworkDrvepath = string.Empty;
            string WavOutputPath = string.Empty;
            var outputdata = new Dictionary<string, object>();
            bool Audstatus = true;
            bool pass_fail = true;

            switch (AudioMode)
            {
                case Audio_Mode.Micophone:
                    outputdata.Add("RecordTime", RecordTime);

                    if (OutputWavPath == string.Empty || OutputWavPath == null)
                    {
                        Status = "SavePath_is_null";
                        LogMessage($"SavePath is not Empty or null!!", MessageLevel.Debug);
                        pass_fail = false;
                        errorcode = -1;
                    }
                    else
                    {
                        outputdata.Add("OutputWavPath", Path.GetFullPath(OutputWavPath));
                        if (!Directory.Exists(Path.GetFullPath(OutputWavPath)))
                        {
                            Status = "SavePath_not_exist";
                            LogMessage($"SavePath is not exist!!", MessageLevel.Warn);
                            pass_fail = false;
                            errorcode = -2;
                        }
                        else
                        {
                            if (FileName == null)
                            {
                                Status = "FileName_null";
                                errorcode = -3;
                                LogMessage($"FileName is Empty!!!", MessageLevel.Debug);
                                pass_fail = false;
                            }
                            else
                            {
                                FileNameReplace = ReplaceProp(FileName);
                                LogMessage($"{Description} The TestItem Name is {FileNameReplace}", MessageLevel.Debug);
                                WavOutputPath = OutputWavPath + "/" + FileNameReplace + ".wav";
                                Audstatus = WaveDevice.PerformAction(RecordTime.ToString(), WavOutputPath,true); //儲存WAV及設定計時器，在2秒後停止錄音                          

                                if (!Audstatus)
                                {
                                    Status = "AudioRecording_Error";
                                    errorcode = -5;
                                    outputdata.Add("Status(errorCode)", $"{Status}[{errorcode}]");
                                    strOutData = JsonConvert.SerializeObject(outputdata, Formatting.Indented);
                                    return false;
                                }
                                else
                                {

                                    // 使用NAudio套件來讀取WAV檔案
                                    using (var reader = new WaveFileReader(WavOutputPath))
                                    {
                                        TimeSpan duration = reader.TotalTime;
                                        LogMessage($"錄製時間長度: {duration}", MessageLevel.Debug);
                                        //檢查WAV檔案錄製時間長度
                                        if (duration.TotalMilliseconds < RecordTime)
                                        {
                                            Status = "Recording_time_is_not_enough";
                                            errorcode = -6;
                                            outputdata.Add("Status(errorCode)", $"{Status}[{errorcode}]");
                                            strOutData = JsonConvert.SerializeObject(outputdata, Formatting.Indented);
                                            LogMessage($"音檔實際錄製時間長度:{duration.TotalMilliseconds}ms 小於 設定錄製時間長度:{RecordTime}ms ", MessageLevel.Warn);
                                            return false;
                                        }
                                    }

                                    if (SaveBackupPath != null && SaveBackupPath != string.Empty)
                                    {
                                        string ReplaceBackupPath = ReplaceProp(SaveBackupPath);
                                        outputdata.Add("BackupPath", Path.GetFullPath(ReplaceBackupPath));
                                        //DirectoryInfo di = null;
                                        if (IsNetworkDrive(ReplaceBackupPath, ref NetworkDrvepath))
                                        {
                                            if (NetworkDrvepath != string.Empty)
                                            {
                                                if (!FileName.Contains("%"))
                                                    BackupPath = NetworkDrvepath + "/" + PopMoreData("ProductSN") + "_" + FileName + DateTime.Now.ToString("_yyyyMMdd_HHmmss") + ".wav";
                                                else
                                                    BackupPath = NetworkDrvepath + "/" + FileNameReplace + ".wav";
                                            }
                                            else
                                            {
                                                if (!FileName.Contains("%"))
                                                    BackupPath = ReplaceBackupPath + "/" + PopMoreData("ProductSN") + "_" + FileName + DateTime.Now.ToString("_yyyyMMdd_HHmmss") + ".wav";
                                                else
                                                    BackupPath = ReplaceBackupPath + "/" + FileNameReplace + ".wav";
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
                                                        File.Copy(WavOutputPath, BackupPath);
                                                    }, cancellationToken);
                                                    if (!copyTask.Wait(timeoutMilliseconds, cancellationToken))
                                                    {
                                                        cancellationTokenSource.Cancel(); // 如果超時，取消複製操作
                                                        LogMessage($"Copy File is Timout: [{timeoutMilliseconds / 1000}]s。", MessageLevel.Warn);
                                                    }
                                                    LogMessage($"Audio File.Copy to {BackupPath}", MessageLevel.Debug);
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                Status = "Audio_Exception";
                                                LogMessage($"Audio File.Copy Exception.{ex.Message}", MessageLevel.Error);
                                                outputdata.Add("Exception", ex.Message);
                                                pass_fail = false;
                                                errorcode = -9;
                                            }
                                        }
                                        else
                                        {
                                            //LogMessage($"Backup Path:{SaveBackupPath} is not exist or lossing,Please Re-select Backup Path and retry again!!!", MessageLevel.Debug);
                                            Status = "SaveBackupPath_NG";
                                            outputdata.Add("Status", Status);
                                            MessageBox.Show($"[{Description}] Backup Path:{ReplaceBackupPath} is not exist or lossing,Please Re-select Backup Path and retry again!!!", "Path Check", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                            pass_fail = false;
                                            errorcode = -7;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    break;
                case Audio_Mode.Sound:

                    Audstatus = WaveDevice.PerformAction("", "", false); //儲存WAV及設定計時器，在2秒後停止錄音                          

                    if (!Audstatus)
                    {
                        Status = "AudioSound_Error";
                        errorcode = -1;
                        outputdata.Add("Status(errorCode)", $"{Status}[{errorcode}]");
                        strOutData = JsonConvert.SerializeObject(outputdata, Formatting.Indented);
                        return false;
                    }

                    break;
                default:
                    break;
            }
            
            if(pass_fail)
            {
                Status = "Audio OK!!!";
                errorcode = 0;
            }
            outputdata.Add("Status(errorCode)", $"{Status}[{errorcode}]");
            outputdata.Add("errorCode", $"{errorcode}");
            strOutData = JsonConvert.SerializeObject(outputdata, Formatting.Indented);
            jsonStr = strOutData;

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

        public enum Audio_Mode
        {
            Micophone,
            Sound
        }
    }
}
