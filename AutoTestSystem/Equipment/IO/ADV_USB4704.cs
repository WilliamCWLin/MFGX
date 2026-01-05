using Automation.BDaq;
using AutoTestSystem.Base;
using AutoTestSystem.DevicesUI.IO;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using static AutoTestSystem.Script.Script_Extra_Generic_Command;

namespace AutoTestSystem.Equipment.IO
{
    public class ADV_USB4704 : IOBase
    {
        private InstantAiCtrl instantAIContrl;
        private InstantAoCtrl instantAOContrl;
        //private double[] m_dataScaled;
        //private  WaveformAiCtrl WaveformAiCtrl1 = new Automation.BDaq.WaveformAiCtrl();
        private InstantDoCtrl instantDoCtrl1 = new InstantDoCtrl();
        private InstantDiCtrl instantDiCtrl1 = new InstantDiCtrl();
        byte portData;
        //private readonly Queue<string> dataQueue = new Queue<string>();
        private string _strParamInfoPath;

        [JsonIgnore]
        [Browsable(false)]
        public InstantAiCtrl AICtrl
        {
            get { return instantAIContrl; }
            set { instantAIContrl = value; }
        }

        [JsonIgnore]
        [Browsable(false)]
        public InstantAoCtrl AOCtrl
        {
            get { return instantAOContrl; }
            set { instantAOContrl = value; }
        }
        [Category("Parameter"), Description("Devices"), TypeConverter(typeof(InstantAiCtrlDeviceList))]
        public string DeviceName
        {
            set; get;
        }

        [Category("Parameter"), Description("Profile Path"), Editor(typeof(Manufacture.FileSelEditorRelPath), typeof(System.Drawing.Design.UITypeEditor))]
        public string Profile_path
        {
            get { return _strParamInfoPath; }
            set { _strParamInfoPath = value; }
        }

        public override void Dispose()
        {
            throw new NotImplementedException();
        }
        public override bool Status(ref string msg)
        {
            try
            {
                if (instantAIContrl.Initialized)
                {
                    foreach (DeviceTreeNode node in instantAIContrl.SupportedDevices)
                    {
                        if (node.Description.Contains(DeviceName))
                        {
                            msg = $"{DeviceName}(ON)";
                            return true;
                        }
                    }

                    msg = $"{DeviceName}(Not Exist)";

                    return false;
                }
                else
                {
                    msg = $"{DeviceName}(OFF)";
                    return false;
                }
            }
            catch(Exception ex)
            {
                msg = $"{DeviceName}{ex.Message}";
                return false;
            }

        }
        public override bool GETIO(int portNum, int pos, ref bool status)
        {
            try
            {
                if (!instantDoCtrl1.Initialized)
                {
                    LogMessage("No device be selected or device open failed!. ", MessageLevel.Error);
                    return false;
                }

                byte Data;
                ErrorCode err = ErrorCode.Success;
                err = instantDiCtrl1.Read(portNum, out Data);
                if (err != ErrorCode.Success)
                {
                    LogMessage("GETIO Exception." + err.ToString(), MessageLevel.Error);
                    return false;
                }

                status = ((Data >> pos) & 0x1) == 1;
                return true;
            }
            catch (Exception ex)
            {
                LogMessage("GETIO Exception." + ex.Message, MessageLevel.Error);
                return false;
            }

        }
        public override bool SETIO(int portNum, int bit, bool output)
        {
            if (!instantDoCtrl1.Initialized)
            {
                LogMessage("ADV_USB4704 No device be selected or device open failed!. ");
                return false;
            }

            ErrorCode err = ErrorCode.Success;
            BitArray bits = new BitArray(new byte[] { portData });
            bits[bit] = output;
            byte[] bytes = new byte[1];
            bits.CopyTo(bytes, 0);
            portData = bytes[0];

            err = instantDoCtrl1.Write(portNum, portData);
            LogMessage($"ADV_USB4704 SetIO bits = {bytes} portdata = {portData}  output = {output} bit = {bit}");
            if (err != ErrorCode.Success)
            {
                LogMessage("ADV_USB4704 SetIO Fail. " + err.ToString());
                return false;
            }

            return true;
        }
        public override bool Init(string strParamInfo)
        {
            try
            {
                if (DeviceName != "")
                {
                    DeviceInformation Dev = new DeviceInformation(DeviceName);
                    string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Profile_path);
                    instantAIContrl = new InstantAiCtrl();
                    try
                    {
                        instantAIContrl.SelectedDevice = Dev;

                        
                        instantAIContrl.LoadProfile(fullPath);

                        //EquipmentLogger.Error
                        LogMessage($"Init Success", MessageLevel.Info);

                        //return true;
                    }
                    catch (Exception e)
                    {
                        LogMessage($"Init Fail.{e.Message}", MessageLevel.Error);
                        return false;
                    }

                    //WaveformAiCtrl1 = new WaveformAiCtrl();

                    //try
                    //{
                    //    WaveformAiCtrl1.SelectedDevice = Dev;
                    //    WaveformAiCtrl1.LoadProfile(fullPath);
                    //    m_dataScaled = new double[2048];
                    //    WaveformAiCtrl1.DataReady += new System.EventHandler<Automation.BDaq.BfdAiEventArgs>(this.waveformAiCtrl1_DataReady);
                    //    LogMessage($"Init Success", MessageLevel.Info);
                    //}
                    //catch (Exception e)
                    //{
                    //    LogMessage($"Init WaveformAiCtrl1 Fail.{e.Message}", MessageLevel.Error);
                    //    return false;
                    //}

                    instantAOContrl = new InstantAoCtrl();
                    try
                    {
                        instantAOContrl.SelectedDevice = Dev;
                        LogMessage($"Init Success", MessageLevel.Info);

                        //return true;
                    }
                    catch (Exception e)
                    {
                        LogMessage($"Init Fail.{e.Message}", MessageLevel.Error);
                        return false;
                    }


                    if (instantDiCtrl1 == null)
                        instantDiCtrl1 = new InstantDiCtrl();

                    if (instantDiCtrl1.SupportedDevices.Count == 0)
                    {
                        LogMessage("SupportedDevices is empty. Init Fail", MessageLevel.Error);
                        return false;
                    }

                    if (instantDiCtrl1.SupportedDevices.Count == 1)
                    {
                        if (instantDiCtrl1.SupportedDevices[0].Description.Contains("Demo"))
                        {
                            LogMessage("SupportedDevices is Demo. Init Fail", MessageLevel.Error);
                            return false;
                        }
                    }

                    instantDiCtrl1.SelectedDevice = Dev;
                    
                    ErrorCode errDI = instantDiCtrl1.LoadProfile(fullPath);
                    if ((errDI >= ErrorCode.ErrorHandleNotValid) && (errDI != ErrorCode.Success))
                    {
                        LogMessage("Sorry ! Some errors happened, the error code is: " + errDI.ToString());
                    }
                    else
                    {
                        LogMessage($"DiCtrl_SelectedDevice {instantDiCtrl1.SelectedDevice.ToString()} and Load Profile {errDI.ToString()}");
                    }

                    if (instantDoCtrl1 == null)
                        instantDoCtrl1 = new InstantDoCtrl();

                    if (instantDoCtrl1.SupportedDevices.Count == 0)
                    {
                        LogMessage("SupportedDevices is empty.", MessageLevel.Error);
                        return false;
                    }
                    if (instantDoCtrl1.SupportedDevices.Count == 1)
                    {
                        if (instantDoCtrl1.SupportedDevices[0].Description.Contains("Demo"))
                        {
                            LogMessage("SupportedDevices is Demo. Init Fail", MessageLevel.Error);
                            return false;
                        }
                    }
                    instantDoCtrl1.SelectedDevice = Dev;
                    
                    ErrorCode errDo = instantDoCtrl1.LoadProfile(fullPath);
                    if ((errDo >= ErrorCode.ErrorHandleNotValid) && (errDo != ErrorCode.Success))
                    {
                        LogMessage("Sorry ! Some errors happened, the error code is: " + errDo.ToString());
                    }
                    else
                    {
                        LogMessage($"DoCtrl_SelectedDevice {instantDoCtrl1.SelectedDevice.ToString()}  and Load Profile {errDo.ToString()}");
                    }
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                LogMessage(ex.Message);
            }
            return true;


        }


        static public int  xxx = 0;
        public override bool InstantAI(string strParamIn, ref string Dataout)
        {
            var message = default(BindDataMessage);
            try
            {
                message = JsonConvert.DeserializeObject<BindDataMessage>(strParamIn);
                int loopCount = Int32.Parse(message.loopCount);
                int Interval = Int32.Parse(message.interval_ms); 
                List<double> voltages = new List<double>();

                for (int i = 0; i < loopCount; i++)
                {
                    double volt = -1;
                    ErrorCode ret = instantAIContrl.Read(Int32.Parse(message.ch), out volt);
                    

                    //double[] dd = new double[8];
                    //ErrorCode err2 = WaveformAiCtrlxx.Prepare();
                    //err2 = WaveformAiCtrlxx.Start();
                    //ErrorCode ret = WaveformAiCtrlxx.GetData(Int32.Parse(message.ch), dd); 
                    if (ret != ErrorCode.Success)
                    {
                        var err = new Dictionary<string, object>
                        {
                            { "Error", ret.ToString() }
                        };
                        Dataout = JsonConvert.SerializeObject(err);
                        return false;
                    }
                    voltages.Add(volt);

                    Thread.Sleep(Interval);
                }

                double maxVolt = voltages.Max();
                double minVolt = voltages.Min();
                double avgVolt = voltages.Average();
                string rawData = string.Join(",", voltages.Select(v => Math.Round(v, 3).ToString("F3").PadLeft(8)));

                var data = new Dictionary<string, object>
                {
                    { "Max", Math.Round(maxVolt, 3) },
                    { "Min", Math.Round(minVolt, 3) },
                    { "Average", Math.Round(avgVolt, 3) },
                    { "RawData", rawData }
                };
                Dataout = JsonConvert.SerializeObject(data,Formatting.Indented);

                LogMessage($"{Dataout}", MessageLevel.Debug);
            }
            catch (Exception ex)
            {
                LogMessage($"{Description} {ex.Message}", MessageLevel.Error);
            }

            return true;
        }

        //public override bool WaveAiCtrl(string strParam, ref string Dataout)
        //{
        //    dataQueue.Clear();
        //    var message = default(BindDataMessage);
        //    message = JsonConvert.DeserializeObject<BindDataMessage>(strParam);
        //    int loopCount = Int32.Parse(message.loopCount);
        //    int Interval = Int32.Parse(message.interval_ms);
        //    ErrorCode err2 = WaveformAiCtrl1.Prepare();
        //    err2 = WaveformAiCtrl1.Start();


        //    Stopwatch stopwatch = new Stopwatch();
        //    stopwatch.Start();
        //    string sectiontemp = string.Empty;

        //    while (stopwatch.ElapsedMilliseconds <= loopCount)
        //    {
        //        Thread.Sleep(3); // 短暫等待以減少 CPU 資源佔用
        //    }
        //    WaveformAiCtrl1.Stop();
        //    return true;
        //}
        //private void waveformAiCtrl1_DataReady(object sender, BfdAiEventArgs args)
        //{
        //    try
        //    {
        //        //The WaveformAiCtrl has been disposed.
        //        if (WaveformAiCtrl1.State == ControlState.Idle)
        //        {
        //            return;
        //        }
        //        if (m_dataScaled.Length < args.Count)
        //        {
        //            m_dataScaled = new double[args.Count];
        //        }

        //        ErrorCode err = ErrorCode.Success;
        //        int chanCount = WaveformAiCtrl1.Conversion.ChannelCount;
        //        int sectionLength = WaveformAiCtrl1.Record.SectionLength;
                
        //        err = WaveformAiCtrl1.GetData(args.Count, m_dataScaled);
        //        string rawData = string.Join(",", m_dataScaled.Select(v => Math.Round(v, 3).ToString("F3").PadLeft(8)));
        //        dataQueue.Enqueue(rawData);
        //        if (err != ErrorCode.Success && err != ErrorCode.WarningRecordEnd)
        //        {
        //            return;
        //        }

        //    }
        //    catch (System.Exception) { }
        //}


        public override bool InstantAI(int channel, ref string Dataout)
        {
            try
            {
                double volt = -1;
                ErrorCode ret = instantAIContrl.Read(channel, out volt);
                if (ret != ErrorCode.Success)
                {
                    var err = new Dictionary<string, object>
                    {
                        { "Error", ret.ToString()}
                    };
                    Dataout = JsonConvert.SerializeObject(err);
                    return false;
                }

                var data = new Dictionary<string, object>
                    {
                        { "VOLT", Math.Round(volt, 3) }
                    };
                Dataout = JsonConvert.SerializeObject(data,Formatting.Indented);
            }
            catch (Exception ex)
            {
                LogMessage($"{Description} [InstantAI] Error: {ex.Message}");
            }

            return true;
        }

        public override bool InstantAO(int channel,double volt, ref string Dataout)
        {
            try
            {
                //
                if (volt > 5 || volt < -5)
                {
                    var err = new Dictionary<string, object>
                    {
                        { "Error", $"{volt} over limit"}
                    };
                    Dataout = JsonConvert.SerializeObject(err);
                    return false;
                }
                ErrorCode ret = instantAOContrl.Write(channel, volt);

                if (ret != ErrorCode.Success)
                {
                    var err = new Dictionary<string, object>
                    {
                        { "Error", ret.ToString()}
                    };
                    Dataout = JsonConvert.SerializeObject(err);
                    return false;
                }

                var data = new Dictionary<string, object>
                    {
                        { "SET_VOLT", volt }
                    };
                Dataout = JsonConvert.SerializeObject(data, Formatting.Indented);
            }
            catch (Exception ex)
            {
                LogMessage($"{Description} [InstantAI] Error: {ex.Message}");
            }

            return true;
        }


        public override bool UnInit()
        {
            try
            {
                if(instantAIContrl == null)
                {
                    LogMessage("UnInit instantAIContrl is null", MessageLevel.Error);
                    return false;
                }
                if (instantAIContrl.SelectedDevice.Description == null)
                {
                    LogMessage("UnInit SelectedDevice Description is null", MessageLevel.Error);
                    return false;
                }
                if (!instantAIContrl.SupportedDevices.Any(node => node.Description == DeviceName))
                {
                    LogMessage($"{DeviceName} not exist", MessageLevel.Error);
                    return false;
                }

                instantAIContrl.Cleanup();
                instantAIContrl.Dispose();
                instantAIContrl = null;


                if (instantAOContrl != null)
                {
                    if (instantAOContrl.SelectedDevice.Description == null)
                    {
                        LogMessage("UnInit SelectedDevice Description is null", MessageLevel.Error);
                        return false;
                    }
                    if (!instantAOContrl.SupportedDevices.Any(node => node.Description == DeviceName))
                    {
                        LogMessage($"{DeviceName} not exist", MessageLevel.Error);
                        return false;
                    }

                    instantAOContrl.Cleanup();
                    instantAOContrl.Dispose();
                    instantAOContrl = null;
                }
                if (instantDoCtrl1 != null)
                {

                    if (instantDoCtrl1.SupportedDevices.Count == 0)
                    {
                        LogMessage("SupportedDevices is empty.", MessageLevel.Error);
                        return false;
                    }
                    if (instantDoCtrl1.SupportedDevices.Count == 1)
                    {
                        if (instantDoCtrl1.SupportedDevices[0].Description.Contains("Demo"))
                        {
                            LogMessage("SupportedDevices is Demo. UnInit Fail", MessageLevel.Error);
                            return false;
                        }
                    }


                    if (!instantDoCtrl1.SupportedDevices.Any(node => node.Description == DeviceName))
                    {
                        LogMessage($"{DeviceName} not exist", MessageLevel.Error);
                        return false;

                    }

                    if (!instantDoCtrl1.Initialized)
                    {
                        LogMessage("instantDoCtrl1 is not Initialized!. ");
                        return false;
                    }

                    instantDoCtrl1.Cleanup();
                    instantDoCtrl1 = null;
                }
                else
                {
                    LogMessage("ADV_USB4704 instantDoCtrl1 is not Initialized!. Can't Uninit .return true");
                }


                if (instantDiCtrl1 != null)
                {
                    if (instantDiCtrl1.SupportedDevices.Count == 0)
                    {
                        LogMessage("SupportedDevices is empty.", MessageLevel.Error);
                        return false;
                    }

                    if (instantDiCtrl1.SupportedDevices.Count == 1)
                    {
                        if (instantDiCtrl1.SupportedDevices[0].Description.Contains("Demo"))
                        {
                            LogMessage("SupportedDevices is Demo. UnInit Fail", MessageLevel.Error);
                            return false;
                        }
                    }

                    if (!instantDiCtrl1.SupportedDevices.Any(node => node.Description == DeviceName))
                    {
                        LogMessage($"{DeviceName} not exist", MessageLevel.Error);
                        return false;

                    }

                    if (!instantDiCtrl1.Initialized)
                    {
                        LogMessage("instantDiCtrl1 is not Initialized!. ");
                        return false;
                    }

                    instantDiCtrl1.Cleanup();
                    instantDiCtrl1 = null;
                }
                else
                {
                    LogMessage("instantDiCtrl1 is not Initialized!. Can't Uninit .return true");
                }

                return true;
            }
            catch (Exception ex)
            {
                LogMessage("ADV_USB4704 Dispose Exception." + ex.Message,MessageLevel.Error);
                return false;
            }

        }

        public override bool Show()
        {
            UnInit();

            bool ret = Init("");

            if (ret)
            {
                UI_USB4704 form = new UI_USB4704(this);
                form.StartPosition = FormStartPosition.CenterScreen; // 設置表單置中
                form.ShowDialog();

                //UI_DemoIO form = new UI_DemoIO(this);
                //form.StartPosition = FormStartPosition.CenterScreen; // 設置表單置中
                //form.ShowDialog();
            }

            return true;
        }

        public class InstantAiCtrlDeviceList : TypeConverter  //下拉式選單
        {
            public static readonly InstantAiCtrl AiCtrl = new InstantAiCtrl();
            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                if (AiCtrl.SupportedDevices.Count > 0)
                {
                    List<string> hwList = new List<string>();
                    foreach (DeviceTreeNode node in AiCtrl.SupportedDevices)
                    {
                        if (node.Description.Contains("Demo") == false)
                            hwList.Add(node.Description);
                    }

                    return new StandardValuesCollection(hwList.ToArray());
                }
                else
                {
                    return new StandardValuesCollection(new int[] { 0 });
                }
            }

            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                return true;

            }
        }
    }
}
