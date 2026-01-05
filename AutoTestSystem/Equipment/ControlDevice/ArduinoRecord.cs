using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AutoTestSystem.Base;
using AutoTestSystem.DAL;
using NAudio.Wave;
using static AutoTestSystem.BLL.Bd;

namespace AutoTestSystem.Equipment.ControlDevice
{
    class ArduinoRecord : ControlDeviceBase
    {
        private static WaveInEvent waveSource;
        private static WaveFileWriter waveFile;
        private System.Threading.Timer timer;

        [Category("Params"), Description("Select Wave Device"), TypeConverter(typeof(Wave_List))]
        public string DeviceName { get; set; }

        [Category("Params"), Description("Set WaveRate")]
        public int waverate { get; set; }

        [Category("Params"), Description("Set WaveRate Channels")]
        public int channels { get; set; }

        //Comport DeviceComport = null;


        public ArduinoRecord()
        {
            waverate = 44100;
            channels = 1;
        }



        public override bool Init(string strParamInfo)
        {
            //private static WaveInEvent waveSource;
            if (string.IsNullOrEmpty(DeviceName))
            {
                LogMessage("NO Wave DeviceName", MessageLevel.Warn);
                MessageBox.Show("NO Wave DeviceName", "Warning!!!");
                return false;
            }

            waveSource = new WaveInEvent();
            waveSource.WaveFormat = new WaveFormat(waverate, channels); // 44100 Hz, 16-bit, Mono
                  
            return true;

        }

        public override void OPEN()
        {
            throw new NotImplementedException();
        }


        public override bool UnInit()
        {
            if (DeviceName == null)
                return false;

            waveSource.StopRecording();
            waveSource.Dispose();
            //waveFile.Dispose();
            //waveFile.Close();

            return true;
        }

        public override bool SEND(string input)
        {
            return true;
        }

        public override bool READ(ref string output)
        {
            return true;
        }

        public override void Dispose()
        {
            throw new NotImplementedException();
        }

        public override void PerformAction(string strItemName,string strOutputPath)
        {
            if (strItemName == "Start")
                waveSource.StartRecording();
            else if (strItemName == "Stop")
            {
                // 設定計時器，在N秒後停止錄音
                timer = new System.Threading.Timer(TimerCallback, null,Int32.Parse(strOutputPath), Timeout.Infinite);               
            }
            else if (strItemName == "Save")
            {
                waveFile = new WaveFileWriter(strOutputPath, waveSource.WaveFormat);
                // 設定錄音事件處理程序
                waveSource.DataAvailable += WaveSourceDataAvailable;
            }
            else
                Logger.Info($"strItemName is not defind!!!");

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
        public override void SetTimeout(int time)
        {
            throw new NotImplementedException();
        }

        public class Wave_List : TypeConverter  //下拉式選單
        {
            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {

                string[] Wave_Device_Names = null;
                List<string> Device_Names = new List<string>(); // 存储设备名称的变量

                for (int i = 0; i < WaveIn.DeviceCount; i++)
                {
                    WaveInCapabilities capabilities = WaveIn.GetCapabilities(i);
                    Device_Names.Add(capabilities.ProductName);
                    Logger.Debug($"Device {i}: {capabilities.ProductName}");
                }
                Wave_Device_Names = Device_Names.ToArray();
                if (Wave_Device_Names.Length > 0)
                {
                    return new StandardValuesCollection(Wave_Device_Names.ToArray());
                }
                else
                {
                    return new StandardValuesCollection(new int[] { });
                }
            }

            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                return true;

            }
        }


    }
}
