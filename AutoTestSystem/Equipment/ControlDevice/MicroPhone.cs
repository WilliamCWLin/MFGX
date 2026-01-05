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
    class MicroPhone : ControlDeviceBase
    {
        private static WaveInEvent waveSource;
        private static WaveFileWriter waveFile;
        private System.Threading.Timer timer;
        private bool isRecordingStoppedSubscribed = false;
        [Category("Device"), Description("Select Wave Device"), TypeConverter(typeof(Wave_List))]
        public string DeviceName { get; set; }

        [Category("Params"), Description("Set WaveRate")]
        public int waverate { get; set; }

        [Category("Params"), Description("Set Channels")]
        public int channels { get; set; }

        [Category("Params"), Description("Set Bits")]
        public int Bits { get; set; }

        WaveFileWriter waveFileWriter = null;

        private bool isInitial;

        public MicroPhone()
        {
            waverate = 16000;
            channels = 1;
            Bits = 16;
            isInitial = false;
        }


        public override bool Init(string strParamInfo)
        {
            //private static WaveInEvent waveSource;
            if (string.IsNullOrEmpty(DeviceName))
            {
                LogMessage("NO Wave DeviceName", MessageLevel.Error);
                MessageBox.Show("NO Wave DeviceName", "Warning!!!");
                return false;
            }

            waveSource = new WaveInEvent();

            int deviceCount = WaveInEvent.DeviceCount;

            if (deviceCount > 0)
            {
                for (int deviceNumber = 0; deviceNumber < deviceCount; deviceNumber++)
                {
                    WaveInCapabilities deviceInfo = WaveInEvent.GetCapabilities(deviceNumber);
                    

                    if (DeviceName == deviceInfo.ProductName)
                    {
                        waveSource.WaveFormat = new WaveFormat(waverate, Bits, channels);
                        waveSource.DeviceNumber = deviceNumber;
                        LogMessage($"Device {deviceNumber}: {deviceInfo.ProductName}");
                        LogMessage($"Use Device({deviceNumber}).Set WaveFormat  {waverate}(hz) {Bits}(bits) {channels}(ch)", MessageLevel.Debug);

                        isInitial = true;
                        break;
                    }
                }
                waveSource.DataAvailable += WaveSourceDataAvailable;

            }
            else
            {
                LogMessage($"Init Fail.DeviceCount {deviceCount}", MessageLevel.Error);
                MessageBox.Show($"{Description} DeviceCount {deviceCount}.\"Please confirm if the recording device is properly connected.\"", "Error!!!");
                return false;
            }
            return true;

        }

        public override void OPEN()
        {
            throw new NotImplementedException();
        }


        public override bool UnInit()
        {
            if (waveSource != null)
            {
                waveSource.StopRecording();
                waveSource.Dispose();
                isInitial = false;
            }
            waveFile?.Close();
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
        public override bool Send(string strActItem, string param)
        {
            if (isInitial == false)
            {
                if(!Init(""))
                    return false;
            }
                
            strActItem = strActItem.ToLower();

            switch (strActItem)
            {
                case "setsavefile":

                    waveFile?.Close();

                    waveFile = new WaveFileWriter(param, waveSource.WaveFormat);
                    LogMessage($"Set outupt path({param}).");
                    break;

                case "startrecording":
                    try
                    {
                        waveSource.StartRecording();
                        LogMessage($"Start Record. Wait Time({param}).");
                        System.Threading.Thread.Sleep(Int32.Parse(param) * 1000);
                        waveSource.StopRecording();
                        waveFile?.Close();
                    }
                    catch (Exception ex)
                    {
                        isInitial = false;
                    }

                    break;

                default:
                    LogMessage($"Unknown action: {strActItem}", MessageLevel.Error);
                    return false;
            }

            return true;
        }

        private void WaveSourceDataAvailable(object sender, WaveInEventArgs e)
        {
            waveFile?.Write(e.Buffer, 0, e.BytesRecorded);
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
                Device_Names.Add("");
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
