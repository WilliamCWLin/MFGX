using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AutoTestSystem.Base;
using AutoTestSystem.DAL;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using static AutoTestSystem.BLL.Bd;

namespace AutoTestSystem.Equipment.ControlDevice
{
    class AudioRecord : ControlDeviceBase
    {
        private string _DeviceInfo;
        private float Mic_DeviceVolume;
        private float Sound_DeviceVolume;
        private static WaveInEvent waveSource;
        private static WaveFileWriter waveFile;
        private string MicrophoneName;
        private string SoundName;
        private float Microphonevolume = 1;
        private float Soundvolume = 1;
        //private System.Threading.Timer timer;

        //[ReadOnly(true)]
        //[Category("Microphone Devices Record"), Description("ALL Microphone Devices")]
        //public string All_MicrophoneDevices
        //{
        //    get
        //    {
        //         return GetAllMicrophoneDevices();
        //    }
        //    set
        //    {
        //        _MicrophoneDevice = value;
        //    }
        //}
        /*[ReadOnly(true)]
        [Category("Microphone Params Record"), Description("Microphone volume"), TypeConverter(typeof(Wave_List))]
        public float Microphonevolume { get; set; } = 1;*/

        [Category("Sound Audio Params"), Description("Select Wave Audio Device"), TypeConverter(typeof(Sound_List))]
        public string SoundDeviceName { get; set; }

        [Category("Microphone Audio Params"), Description("Select Wave Microphone Device"), TypeConverter(typeof(Wave_List))]
        public string MicrophoneDeviceName
        {
            get
            {
                return _DeviceInfo;
            }
            set
            {
                _DeviceInfo = value;
            }
        }

        [Category("Common Params"), Description("Set WaveRate")]
        public int waverate { get; set; }

        [Category("Common Params"), Description("Set Channels")]
        public int channels { get; set; }

        [Category("Common Params"), Description("Set Bits")]
        public int Bits { get; set; }

        [Category("Choice Audio Device Mode"), Description("Choice Audio Device Mode")]
        public AudioDevice Audio_Mode { get; set; } = AudioDevice.Default;


        [Category("Microphone Audio Params"), Description("Set volume(0~1)")]
        public float Micophone_volume
        {
            get
            {
                if (Mic_DeviceVolume == 0)
                    return 1;
                else
                    return Mic_DeviceVolume;
            }
            set {
                Mic_DeviceVolume = CheckMicrophoneVolume(value);
            }
        }

        [Category("Sound Audio Params"), Description("Set volume(0~1)")]
        public float Sound_volume
        {
            get
            {
                if (Sound_DeviceVolume == 0)
                    return 1;
                else
                    return Sound_DeviceVolume;
            }
            set
            {
                Sound_DeviceVolume = CheckSoundVolume(value);
            }
        }

        private bool isInitial;
        private bool devicestatus;
        private bool Mic_Flag;
        private bool Sound_Flag;

        public AudioRecord()
        {
            waverate = 44100;
            channels = 1;
            Bits = 16;            
            isInitial = false;
            Mic_Flag = true;
            Sound_Flag = true;
        }

        private float CheckMicrophoneVolume(float device_volume)
        {
            if (device_volume < 0 || device_volume > 1)
            {
                MessageBox.Show($"Microphon Volume Range is \"0\" to \"1\"", "Microphone Volume Note", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return Microphonevolume;
            }
            return device_volume;
        }

        private float CheckSoundVolume(float device_volume)
        {
            if (device_volume < 0 || device_volume > 1)
            {
                MessageBox.Show($"Sound Volume Range is \"0\" to \"1\"", "Sound Volume Note", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return Soundvolume;
            }
            return device_volume;
        }

        private void SetMicrophoneVolume(float volume)
        {
            var enumerator = new MMDeviceEnumerator();
            var devices = enumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active);
            int deviceNumber = 0;
            foreach (var device in devices)
            {
                if (MicrophoneDeviceName == device.FriendlyName)
                {
                    device.AudioEndpointVolume.Mute = false;
                    MicrophoneName = MicrophoneDeviceName;
                    Microphonevolume = volume;
                    device.AudioEndpointVolume.MasterVolumeLevelScalar = volume;
                    LogMessage($"Set Microphone({deviceNumber})[{MicrophoneName}] volume: {device.AudioEndpointVolume.MasterVolumeLevelScalar}(%)", MessageLevel.Info);
                    break;
                }
                deviceNumber++;
            }
        }

        private float GetMicrophoneVolume()
        {
            float _Volume = 0;
            var enumerator = new MMDeviceEnumerator();
            var devices = enumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active);
            int deviceNumber = 0;
            foreach (var device in devices)
            {
                if (!string.IsNullOrEmpty(MicrophoneDeviceName))
                {
                    if (MicrophoneDeviceName == device.FriendlyName)
                    {
                        if (device.AudioEndpointVolume.Mute != false)
                        {
                            device.AudioEndpointVolume.Mute = false;
                        }
                        _Volume = device.AudioEndpointVolume.MasterVolumeLevelScalar;
                        Microphonevolume = (float)Math.Round(_Volume,2);
                        LogMessage($"Get Microphone({deviceNumber})[{device.FriendlyName}] volume: {Microphonevolume}(%)", MessageLevel.Info);
                        break;
                    }
                    deviceNumber++;
                }else
                    LogMessage($"GetMicrophoneVolume => Microphone DeviceName NULL or Empty", MessageLevel.Warn);
            }

            return Microphonevolume;
        }

        private void SetSoundVolume(float volume)
        {
            var enumerator = new MMDeviceEnumerator();
            var devices = enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);
            int deviceNumber = 0;
            foreach (var device in devices)
            {
                if (SoundDeviceName == device.FriendlyName)
                {
                    device.AudioEndpointVolume.Mute = false;
                    SoundName = SoundDeviceName;
                    Soundvolume = volume;
                    device.AudioEndpointVolume.MasterVolumeLevelScalar = volume;
                    LogMessage($"Set Sound({deviceNumber})[{SoundName}] volume: {device.AudioEndpointVolume.MasterVolumeLevelScalar}(%)", MessageLevel.Info);
                    break;
                }
                deviceNumber++;
            }
        }

        private float GetSoundVolume()
        {
            float _Volume = 0;
            var enumerator = new MMDeviceEnumerator();
            var devices = enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);
            int deviceNumber = 0;
            foreach (var device in devices)
            {
                if (!string.IsNullOrEmpty(SoundDeviceName))
                {
                    if (SoundDeviceName == device.FriendlyName)
                    {
                        if (device.AudioEndpointVolume.Mute != false)
                        {
                            device.AudioEndpointVolume.Mute = false;
                        }
                        _Volume = device.AudioEndpointVolume.MasterVolumeLevelScalar;
                        Soundvolume = (float)Math.Round(_Volume, 2);
                        LogMessage($"Get Sound({deviceNumber})[{device.FriendlyName}] volume: {Soundvolume}(%)", MessageLevel.Info);
                        break;
                    }
                    deviceNumber++;
                }
                else
                    LogMessage($"GetSoundvolume => Sound DeviceName NULL or Empty", MessageLevel.Warn);
            }

            return Soundvolume;
        }
      
        public override bool Init(string strParamInfo)
        {
            //private static WaveInEvent waveSource;
            bool Mic_isInitial = false;
            bool Sound_isInitial = false;
            MMDeviceEnumerator enumerator = null;

            waveSource = new WaveInEvent();
            enumerator = new MMDeviceEnumerator();

            int deviceCount = WaveInEvent.DeviceCount;

            if (deviceCount > 0)
            {
                switch (Audio_Mode)
                {
                    case AudioDevice.Default:
                        //Microphone
                        if (string.IsNullOrEmpty(MicrophoneDeviceName) && string.IsNullOrEmpty(SoundDeviceName))
                        {
                            LogMessage("NO Wave DeviceName", MessageLevel.Warn);
                            MessageBox.Show("NO Wave DeviceName", "Warning!!!");
                            return false;
                        }
                        //var enumerator = new MMDeviceEnumerator();
                        var Micdevices = enumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active);
                        int MicrophonedeviceNumber = 0;
                        foreach (var device in Micdevices)
                        {
                            if (MicrophoneDeviceName == device.FriendlyName)
                            {
                                waveSource.WaveFormat = new WaveFormat(waverate, Bits, channels);
                                waveSource.DeviceNumber = MicrophonedeviceNumber;
                                SetMicrophoneVolume(Micophone_volume);
                                LogMessage($"Device {MicrophonedeviceNumber}: {device.FriendlyName}", MessageLevel.Info);
                                LogMessage($"Use Device({MicrophonedeviceNumber}).Set WaveFormat  {waverate}(hz) {Bits}(bits) {channels}(ch)", MessageLevel.Info);
                                Mic_isInitial = true;
                                break;
                         }
                        MicrophonedeviceNumber++;
                        }       
                        //Sound 
                        var Sounddevices = enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);
                        int SounddeviceNumber = 0;
                        foreach (var device in Sounddevices)
                        {
                            if (SoundDeviceName == device.FriendlyName)
                            {
                            waveSource.WaveFormat = new WaveFormat(waverate, Bits, channels);
                            waveSource.DeviceNumber = SounddeviceNumber;
                            SetSoundVolume(Sound_volume);
                            LogMessage($"Device {SounddeviceNumber}: {device.FriendlyName}", MessageLevel.Info);
                            LogMessage($"Use Device({SounddeviceNumber}).Set WaveFormat  {waverate}(hz) {Bits}(bits) {channels}(ch)", MessageLevel.Info);
                            Sound_isInitial = true;
                            break;
                        }
                            SounddeviceNumber++;
                        }

                        if (Mic_isInitial == true && Sound_isInitial == true)
                        {
                            isInitial = true;
                        }

                        if (!isInitial)
                        {
                            LogMessage($"Init Fail,{MicrophoneDeviceName} && {SoundDeviceName} is not exist!!!", MessageLevel.Error);
                            return false;
                        }

                        break;
                    case AudioDevice.Only_Micophone:

                        if (string.IsNullOrEmpty(MicrophoneDeviceName))
                        {
                            LogMessage("NO Wave DeviceName", MessageLevel.Warn);
                            MessageBox.Show("NO Wave DeviceName", "Warning!!!");
                            return false;
                        }

                        Sound_Flag = false;
                        var Mic_devices = enumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active);
                        int Microphonedevice_Number = 0;
                        foreach (var device in Mic_devices)
                        {
                            if (MicrophoneDeviceName == device.FriendlyName)
                            {
                                waveSource.WaveFormat = new WaveFormat(waverate, Bits, channels);
                                waveSource.DeviceNumber = Microphonedevice_Number;
                                SetMicrophoneVolume(Micophone_volume);
                                LogMessage($"Device {Microphonedevice_Number}: {device.FriendlyName}", MessageLevel.Info);
                                LogMessage($"Use Device({Microphonedevice_Number}).Set WaveFormat  {waverate}(hz) {Bits}(bits) {channels}(ch)", MessageLevel.Info);
                                isInitial = true;
                                break;
                            }
                            Microphonedevice_Number++;
                        }

                        if (!isInitial)
                        {
                            LogMessage($"Init Fail,{MicrophoneDeviceName} is not exist!!!", MessageLevel.Error);
                            return false;
                        }
                        break;
                    case AudioDevice.Only_Sound:

                        if (string.IsNullOrEmpty(SoundDeviceName))
                        {
                            LogMessage("NO Wave DeviceName", MessageLevel.Warn);
                            MessageBox.Show("NO Wave DeviceName", "Warning!!!");
                            return false;
                        }

                        Mic_Flag = false;
                        var Sound_devices = enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);
                        int Sound_deviceNumber = 0;
                        foreach (var device in Sound_devices)
                        {
                            if (SoundDeviceName == device.FriendlyName)
                            {
                                waveSource.WaveFormat = new WaveFormat(waverate, Bits, channels);
                                waveSource.DeviceNumber = Sound_deviceNumber;
                                SetSoundVolume(Sound_volume);
                                LogMessage($"Device {Sound_deviceNumber}: {device.FriendlyName}", MessageLevel.Info);
                                LogMessage($"Use Device({Sound_deviceNumber}).Set WaveFormat  {waverate}(hz) {Bits}(bits) {channels}(ch)", MessageLevel.Info);
                                isInitial = true;
                                break;
                            }
                            Sound_deviceNumber++;
                        }

                        if (!isInitial)
                        {
                            LogMessage($"Init Fail,{SoundDeviceName} is not exist!!!", MessageLevel.Error);
                            return false;
                        }

                        break;
                    default:
                        break;
            }
            
                // 設定錄音事件處理程序
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

        public override bool Status(ref string msg)
        {            
            if (isInitial == true)
            {
                bool Mic_isInitial = false;
                bool Sound_isInitial = false;
                devicestatus = false;
                MMDeviceEnumerator enumerator = null;
                enumerator = new MMDeviceEnumerator();

                if (Mic_Flag)
                {
                    var Micdevices = enumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active);
                    int MicdeviceNumber = 0;
                    foreach (var device in Micdevices)
                    {
                        if (MicrophoneDeviceName == device.FriendlyName)
                        {
                            Mic_isInitial = true;
                            break;
                        }
                        MicdeviceNumber++;
                    }
                }

                if (Sound_Flag)
                {
                    var Sounddevices = enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);
                    int SounddeviceNumber = 0;
                    foreach (var device in Sounddevices)
                    {
                        if (SoundDeviceName == device.FriendlyName)
                        {
                            Sound_isInitial = true;
                            break;
                        }
                        SounddeviceNumber++;
                    }
                }


                    if (Mic_isInitial == true && Sound_isInitial == true)
                    {
                        devicestatus = true;
                        msg = $"{MicrophoneDeviceName} and {SoundDeviceName}[ON]";
                        return true;
                    }else if(Mic_isInitial == true && Sound_isInitial == false)
                    {
                        devicestatus = true;
                        msg = $"{MicrophoneDeviceName}[ON]";
                        return true;
                    }
                    else if(Mic_isInitial == false && Sound_isInitial == true)
                    {
                        devicestatus = true;
                        msg = $"{SoundDeviceName}[ON]";
                        return true;
                    }else
                    {
                        devicestatus = false;
                        msg = $"{MicrophoneDeviceName} and {SoundDeviceName}[Not_Exist]";
                       return false;
                    }
            }
            msg = $"Device Init Fail";
            return false;
        }

        public override bool UnInit()
        {
            if (waveSource != null)
            {
                waveSource.StopRecording();
                waveSource.Dispose();
                isInitial = false;
                //waveFile.Dispose();
                //waveFile.Close();
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

        public override bool PerformAction(string StopTime, string strOutputPath,bool mode)
        {
            string msg = string.Empty;
            if (devicestatus == false)
            {
                bool isStatus = Status(ref msg);
                if(!isStatus)
                    return false;              
            }
            if (mode)
            {
                try
                {
                    if(string.IsNullOrEmpty(StopTime))
                    {
                        LogMessage($"Record StopTime can not be Empty or null",MessageLevel.Warn);
                        return false;
                    }
                    waveFile = new WaveFileWriter(strOutputPath, waveSource.WaveFormat);
                    if (!File.Exists(strOutputPath))
                    {
                        LogMessage($"AudioRecord OutputPath is not Exists!!", MessageLevel.Error);
                        return false;
                    }
                    if (Mic_Flag)
                    {
                        //Check  Microphone Volume Setting
                        if (GetMicrophoneVolume() != Micophone_volume)
                            SetMicrophoneVolume(Micophone_volume);
                    }
                    if (Sound_Flag)
                    {
                        //Check  Sound Volume Setting
                        if (GetSoundVolume() != Sound_volume)
                            SetSoundVolume(Sound_volume);
                    }


                    //指定停止錄製時間
                    waveSource.StartRecording();
                    Thread.Sleep(200);
                    System.Threading.Thread.Sleep(Int32.Parse(StopTime)); // 將時間為毫秒                   
                    waveSource.StopRecording();
                    //關閉 WAV 寫入器
                    waveFile.Close();
                    waveSource.Dispose();

                    // 設定計時器，在N秒後停止錄音
                    //timer = new System.Threading.Timer(TimerCallback, null, Int32.Parse(strOutputPath), Timeout.Infinite);

                }
                catch (Exception ex)
                {
                    isInitial = false;
                    devicestatus = false;
                    Status(ref msg);
                    LogMessage($"AudioRecord PerformAction Error:{ex.Message}!!!", MessageLevel.Error);
                    return false;
                }
            }
            else
            {
              //Check  Sound Volume Setting
              if (GetSoundVolume() != Sound_volume)
                 SetSoundVolume(Sound_volume);
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

                var enumerator = new MMDeviceEnumerator();
                var devices = enumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active);
                int deviceNumber = 0;
                foreach (var device in devices)
                {
                    Device_Names.Add(device.FriendlyName);
                    deviceNumber++;
                    Logger.Debug($"Device {deviceNumber}: {device.FriendlyName}");
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

        public class Sound_List : TypeConverter  //下拉式選單
        {
            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {

                string[] Wave_Device_Names = null;
                List<string> Device_Names = new List<string>(); // 存储设备名称的变量

                var enumerator = new MMDeviceEnumerator();
                var devices = enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);
                int deviceNumber = 0;
                foreach (var device in devices)
                {
                    Device_Names.Add(device.FriendlyName);
                    deviceNumber++;
                    Logger.Debug($"Device {deviceNumber}: {device.FriendlyName}");
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

        public enum AudioDevice
        {
            Default,
            Only_Micophone,
            Only_Sound
        }

    }
}
