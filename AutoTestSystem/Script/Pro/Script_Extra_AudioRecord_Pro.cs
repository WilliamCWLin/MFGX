
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
    internal class Script_Extra_AudioRecord_Pro : Script_Extra_Base
    {
        private string _strParamInfoPath;
        private static WaveInEvent waveSource;
        private static WaveFileWriter waveFile;
        private System.Threading.Timer timer;
        string strOutData = string.Empty;    
        string Status = string.Empty;
        //int DelayTime;
        int errorcode;
        [Category("Common Parameters"), Description("RecordTime")]
        public int RecordTime { get; set; } = 2000;
        [Category("Common Parameters"), Description("Bit")]
        public int Bit { get; set; } = 16;
        [Category("Common Parameters"), Description("BandRate")]
        public int BandRate { get; set; } = 44100;
        [Category("Common Parameters"), Description("Channels")]
        public int Channels { get; set; } = 1;
        [Category("Common Parameters"), Description("選取存取的文件夾"), Editor(typeof(FolderSelEditorRelPath), typeof(System.Drawing.Design.UITypeEditor))]
        public string OutputWavPath { get; set; }
        [Category("Common Parameters"), Description("選取存取備份檔案的文件夾\r\n(若為網路磁盤(Z:\\ H:\\等)，需再填寫 \"網路磁碟IP指定路徑\")"), Editor(typeof(FolderSelEditorRelPath), typeof(System.Drawing.Design.UITypeEditor))]
        public string SaveBackupPath { get; set; }
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
        [Category("Common Parameters"), Description("存取檔案名稱(預設為Output)")]
        public string FileName { get; set; } = "Output";

        public override void Dispose()
        {
            //throw new NotImplementedException();
        }
        public override bool PreProcess()
        {
            GetNetworkpath(Network_Path);
            strOutData = string.Empty;
            Status = "Waiting";
            errorcode = -99;
            return true;
        }
        public override bool Process(ref string strOutData)
        {
            string BackupPath = string.Empty;
            string NetworkDrvepath = string.Empty;
            string WavOutputPath = string.Empty;
            var outputdata = new Dictionary<string, object>();
            outputdata.Add("BandRate", BandRate);
            outputdata.Add("Channels", Channels);
            outputdata.Add("RecordTime", RecordTime);
            
            
            if(OutputWavPath == string.Empty || OutputWavPath == null)
            {
                Status = "SavePath_is_null";
                outputdata.Add("Status", Status);
                strOutData = JsonConvert.SerializeObject(outputdata, Formatting.Indented);
                LogMessage($"SavePath is not Empty or null!!", MessageLevel.Debug);
                return false;
            }
            else
            {
                outputdata.Add("OutputWavPath", Path.GetFullPath(OutputWavPath));
                outputdata.Add("OutputWav_RelativePath", OutputWavPath);
            }
            if (!Directory.Exists(Path.GetFullPath(OutputWavPath)))
            {
                Status = "SavePath_not_exist";
                outputdata.Add("Status", Status);
                strOutData = JsonConvert.SerializeObject(outputdata, Formatting.Indented);
                LogMessage($"SavePath is not exist!!", MessageLevel.Warn);
                return false;
            }

            WavOutputPath = OutputWavPath + "/" + FileName + ".wav";
            
          
            try
            {
                waveSource = new WaveInEvent();
                if (WaveIn.DeviceCount > 0)
                {
                    for (int i = 0; i < WaveIn.DeviceCount; i++)
                    {
                        WaveInCapabilities capabilities = WaveIn.GetCapabilities(i);
                        waveSource.DeviceNumber = i;
                        LogMessage($"Device {i}: {capabilities.ProductName}", MessageLevel.Debug);
                    }
                }
                else
                {
                    Status = "No_Device!!";
                    outputdata.Add("Status", Status);
                    strOutData = JsonConvert.SerializeObject(outputdata, Formatting.Indented);
                    return false;
                }
                waveSource.WaveFormat = new WaveFormat(BandRate,Bit,Channels); // 44100 Hz, 16-bit, Mono
                // 設定錄音事件處理程序
                
                waveFile = new WaveFileWriter(WavOutputPath, waveSource.WaveFormat);
                //              
                waveSource.DataAvailable += WaveSourceDataAvailable;

                //指定停止錄製時間
                waveSource.StartRecording();
                Thread.Sleep(200);
                System.Threading.Thread.Sleep(RecordTime); // 將時間為毫秒                   
                waveSource.StopRecording();
                //關閉 WAV 寫入器
                waveFile.Close();
                waveSource.Dispose();

                // 設定計時器，先過2秒後再錄製RecordTime後停止錄音
                //DelayTime = RecordTime + 2000;
                //timer = new System.Threading.Timer(TimerCallback, null, DelayTime, Timeout.Infinite);
                //Thread.Sleep(2000);

                // 開始錄音
                //waveSource.StartRecording();
                //Thread.Sleep(DelayTime);

                // 使用NAudio套件來讀取WAV檔案
                using (var reader = new WaveFileReader(WavOutputPath))
                {
                    TimeSpan duration = reader.TotalTime;
                    LogMessage($"錄製時間長度: {duration}", MessageLevel.Debug);
                    //檢查WAV檔案錄製時間長度
                    if (duration.TotalMilliseconds < RecordTime)
                    {
                        Status = "Recording_time_is_not_enough";
                        outputdata.Add("Status", Status);
                        strOutData = JsonConvert.SerializeObject(outputdata, Formatting.Indented);
                        LogMessage($"音檔實際錄製時間長度:{duration.TotalMilliseconds}ms 小於 設定錄製時間長度:{RecordTime}ms ", MessageLevel.Warn);
                        return false;
                    }
                }               

            }
            catch (Exception e1)
            {
                LogMessage($"AudioRecord Exception.{e1.Message}", MessageLevel.Error);
                outputdata.Add("Exception", e1.Message);
                strOutData = JsonConvert.SerializeObject(outputdata, Formatting.Indented);
                return false;

            }            

            if (SaveBackupPath != null && SaveBackupPath != string.Empty)
            {
                outputdata.Add("BackupPath", Path.GetFullPath(SaveBackupPath)); 
                outputdata.Add("Backup_RelativePath",SaveBackupPath);
                //DirectoryInfo di = null;
                if (IsNetworkDrive(SaveBackupPath, ref NetworkDrvepath))
                {
                    if (NetworkDrvepath != string.Empty)
                        BackupPath = NetworkDrvepath + "/" + PopMoreData("ProductSN") + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + "_" + FileName + ".wav";
                    else
                        BackupPath = SaveBackupPath + "/" + PopMoreData("ProductSN") + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + "_" + FileName + ".wav";

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
                        LogMessage($"Audio File.Copy Exception.{ex.Message}", MessageLevel.Error);
                        outputdata.Add("Exception", ex.Message);
                        strOutData = JsonConvert.SerializeObject(outputdata, Formatting.Indented);
                        return false;
                    }
                }
                else
                {
                    //LogMessage($"Backup Path:{SaveBackupPath} is not exist or lossing,Please Re-select Backup Path and retry again!!!", MessageLevel.Debug);
                    Status = "SaveBackupPath_NG";
                    outputdata.Add("Status", Status);
                    strOutData = JsonConvert.SerializeObject(outputdata, Formatting.Indented);
                    MessageBox.Show($"[{Description}] Backup Path:{SaveBackupPath} is not exist or lossing,Please Re-select Backup Path and retry again!!!", "Path Check", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
            }
            Status = "Save_OK!!!";
            errorcode = 0;
            outputdata.Add("Status", Status);
            outputdata.Add("errorCode", errorcode);
            strOutData = JsonConvert.SerializeObject(outputdata, Formatting.Indented);

            return true;
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

        private void WaveSourceDataAvailable(object sender, WaveInEventArgs e)
        {
            Console.WriteLine($"Bytes Recorded: {e.BytesRecorded}");
            // 寫入錄音數據到文件
            waveFile?.Write(e.Buffer, 0, e.BytesRecorded);
        }
        private void TimerCallback(object state)
        {
            waveSource.StopRecording();

            waveFile.Close();
            waveFile.Dispose();
        }

    }
}
