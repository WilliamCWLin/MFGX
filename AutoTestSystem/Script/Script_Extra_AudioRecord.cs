
using NAudio.Wave;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static AutoTestSystem.BLL.Bd;

namespace AutoTestSystem.Script
{
    internal class Script_Extra_AudioRecord : Script_Extra_Base
    {
        private static WaveInEvent waveSource;
        private static WaveFileWriter waveFile;
        private System.Threading.Timer timer;
        string strActItem = string.Empty;
        string strOutputPath = string.Empty;
        string strOutData = string.Empty;
        int RecordTime = 2000;

        public override void Dispose()
        {
            //throw new NotImplementedException();
        }
        public override bool PreProcess(string ActionItem, string Paraminput)
        {
            try
            {
                string[] InputParamArr = null;
                InputParamArr = Paraminput.Split(',');

                if (InputParamArr.Length < 2)
                {
                    Logger.Error("Parameter format error.");
                    return false;
                }
                else
                {
                    RecordTime = Int32.Parse(InputParamArr[0]);
                    strOutputPath = InputParamArr[1];
                    //Logger.Info($"SetBit:{strSetBit},Output:{bOutput}");
                }

                return true;
            }
            catch (Exception ex)
            {
                Logger.Info($"Script_IO_Related_SetIO PreProcess Exception: {ex.Message}");
                return false;
            }

        }
        public override bool Process()
        {
            for (int i = 0; i < WaveIn.DeviceCount; i++)
            {
                WaveInCapabilities capabilities = WaveIn.GetCapabilities(i);
                Logger.Debug($"Device {i}: {capabilities.ProductName}");
            }
            
            waveSource = new WaveInEvent();
            //waveSource.DeviceNumber = selectedDeviceNumber;
            waveSource.WaveFormat = new WaveFormat(44100, 1); // 44100 Hz, 16-bit, Mono
            waveFile = new WaveFileWriter(strOutputPath, waveSource.WaveFormat);
            //Thread.Sleep(5000);
            // 設定錄音事件處理程序
            waveSource.DataAvailable += WaveSourceDataAvailable;

            // 設定計時器，在20秒後停止錄音
            timer = new System.Threading.Timer(TimerCallback, null, RecordTime, Timeout.Infinite);

            // 開始錄音
            waveSource.StartRecording();
            Thread.Sleep(RecordTime);


            try
            {
                DirectoryInfo di = null;
                string Backup_Path = Path.GetDirectoryName(strOutputPath) + "/Backup";
                if (!Directory.Exists(Backup_Path))
                    di = Directory.CreateDirectory(Backup_Path);

                Backup_Path = Backup_Path + "/" + DateTime.Now.ToString("yyyyMMdd_HHmmss_") + PopMoreData("ProductSN") + "_" + Path.GetFileName(strOutputPath);
                File.Copy(strOutputPath, Backup_Path);
                Logger.Debug($"Audio File.Copy to {Backup_Path}");
            }
            catch(Exception ex)
            {
                Logger.Debug($"Audio File.Copy Exception.{ex.Message}");
            }

            var data = new Dictionary<string, object>
                        {
                            { "errorCode", 0 }
                        };
            strOutData = JsonConvert.SerializeObject(data);


            return true;
        }
        public override bool PostProcess()
        {
            //string result = ProcessData(strOutData, strCheckSpec);
            //strDataout = strOutData;

            //if (result == "PASS" || strCheckSpec == "")
            //    return true;
            //else
            //    return false;
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

        public class Delay
        {
           
            public int DelayTime { get; set; }
            

        }

    }
}
