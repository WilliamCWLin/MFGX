using AutoTestSystem.Base;
using AutoTestSystem.DUT;
using AutoTestSystem.Equipment.IO;
using AutoTestSystem.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Design;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using static AutoTestSystem.BLL.Bd;
namespace AutoTestSystem.Script
{
    internal class Script_IO_ModBusSetGetIO_Pro : ScriptIOBase
    {
        string strOutData = string.Empty;

        [Category("Common Parameters"), Description("自訂顯示名稱")]
        public IO_Mode Mode { get; set; }

        [Category("GetIO Parameters"), Description("自訂顯示名稱"), TypeConverter(typeof(SensorGetIOList))]
        public string DI_SensorName { get; set; } = "";

        [Category("GetIO Parameters"), Description("自訂顯示名稱")]
        public bool Check { get; set; } = true;

        [Category("GetIO Parameters"), Description("Timeout(ms)")]
        public int Timeout { get; set; } = 8000;
        /*[Category("GetIO Parameters"), Description("DelayTime for GetIO")]
        public int GetIO_DelayTime { get; set; } = 10;*/

        [Category("SetMuti GetIO Parameters"), Description("自訂顯示名稱"), Editor(typeof(Muti_IOEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public string MutiIO_From { get; set; } = "";
        [Category("SetMuti GetIO Parameters"), Description("Timeout(ms)")]
        public int TimeoutForMuti { get; set; } = 8000;

        [Category("SetIO Parameters"), Description("設定ONOFF")]
        public bool ON_OFF { get; set; }

        [Category("SetIO Parameters"), Description("自訂顯示名稱"), TypeConverter(typeof(SensorSetIOList))]
        public string DO_SensorName { get; set; } = "";
        [Browsable(false)]
        [Category("SetIO Parameters"), Description("DelayTime for SetIO")]
        public int SetIO_DelayTime { get; set; } = 10;


        public Dictionary<string, string> DI_Data = new Dictionary<string, string>();
        public Dictionary<string, string> DO_Data = new Dictionary<string, string>();

        public override void Dispose()
        {
            //throw new NotImplementedException();
        }
        public override bool PreProcess()
        {
            strOutData = string.Empty;
            DI_Data = null;
            DO_Data = null;

            return true;
        }
        public override bool Process(IOBase Device, ref string output)
        {
            bool Ret;
            string OutPut = string.Empty;
            try
            {
                switch (Mode)
                {
                    case IO_Mode.Set:
                        Ret = SetIO(Device, ON_OFF, ref OutPut);
                        strOutData = JsonConvert.SerializeObject(OutPut, Formatting.Indented);
                        output = strOutData;
                        if (!Ret)
                            return false;
                        break;
                    case IO_Mode.Get:
                        Ret = GetIO(Device, ref OutPut);
                        strOutData = JsonConvert.SerializeObject(OutPut, Formatting.Indented);
                        output = strOutData;
                        if (!Ret)
                            return false;
                        break;
                    case IO_Mode.SetGet:

                        string tmp_msg = string.Empty;
                        var Data = new Dictionary<string, object>();
                        bool bSuccess = true;

                        if (!SetIO(Device, ON_OFF, ref tmp_msg))
                        {
                            Data.Add("StepOne", tmp_msg);
                            Data.Add("errorCode", -1);
                            strOutData = JsonConvert.SerializeObject(Data, Formatting.Indented);
                            output = strOutData;
                            LogMessage(tmp_msg);
                            return false;
                        }
                        Data.Add("StepOne", tmp_msg);

                        Thread.Sleep(10);

                        if (!GetIO(Device, ref tmp_msg))
                        {
                            Data.Add("StepTwo", tmp_msg);
                            Data.Add("errorCode", -2);
                            strOutData = JsonConvert.SerializeObject(Data, Formatting.Indented);
                            output = strOutData;
                            LogMessage(tmp_msg);
                            bSuccess = false;
                        }
                        if (bSuccess)
                            Data.Add("StepTwo", tmp_msg);

                        if (!SetIO(Device, !ON_OFF, ref tmp_msg))
                        {
                            Data.Add("errorCode", -3);
                            Data.Add("StepThree", tmp_msg);

                            strOutData = JsonConvert.SerializeObject(Data, Formatting.Indented);
                            output = strOutData;
                            LogMessage(tmp_msg);
                            return false;
                        }

                        Data.Add("StepThree", tmp_msg);
                        if (bSuccess)
                            Data.Add("errorCode", 0);
                        strOutData = JsonConvert.SerializeObject(Data, Formatting.Indented);
                        output = strOutData;
                        LogMessage(strOutData);

                        if (!bSuccess)
                            return false;
                        break;
                    case IO_Mode.Multi:
                        if (!string.IsNullOrEmpty(MutiIO_From))
                        {

                            List<IOData> dataList = JsonConvert.DeserializeObject<List<IOData>>(MutiIO_From);
                            bool status = false;
                            bool Check_Done = false;
                            bool Time_out = true;
                            int Count_Num = 0;
                            var stopwatch = new Stopwatch();
                            
                            stopwatch.Start();
                            LogMessage("Waiting...", MessageLevel.Debug);
                            while (stopwatch.ElapsedMilliseconds < TimeoutForMuti && !Check_Done)
                            {

                                    foreach (var item in dataList)
                                    {

                                            Count_Num++;
                                            //bool Get_Flag = Device.GETIO(int.Parse(data.Value), GetCHNumForMuti, ref status);
                                            bool Get_Flag = GetIOParse(Device, item.IO_Name, ref status);
                                            Console.WriteLine($"KeyName: {item.IO_Name}, Value: {item.IO_Status}");
                                            if (!Get_Flag)
                                                return false;

                                            if (status != bool.Parse(item.IO_Status))
                                            {
                                                Count_Num = 0;
                                                break;
                                            }
                                    }
                                    if (dataList.Count() == Count_Num)
                                    {
                                        Check_Done = true;
                                        Time_out = false;
                                        break;
                                    }

                                //}

                            }
                            if (Time_out)
                            {
                                LogMessage($"GETIO TimeOut", MessageLevel.Error);
                                stopwatch.Stop();
                                stopwatch.Reset();
                                return false;
                            }

                        }
                        break;
                }

            }
            catch (Exception ex)
            {
                LogMessage($"Script_IO_ModBusSetGetIO_Pro Exception:{ex.Message}", MessageLevel.Error);
                return false;
            }
            return true;
        }
        public override bool PostProcess()
        {
            string result = CheckRule(strOutData, Spec);

            if (result == "PASS")
                return true;
            else
                return false;
        }

        public bool SetIO(IOBase Device, bool ONOFF, ref string output)
        {
            try
            {
                strOutData = string.Empty;
                if (!string.IsNullOrEmpty(DO_SensorName))
                {
                    DO_Data = Device.GetDOForm();
                    if (DO_Data != null)
                    {
                        foreach (var data in DO_Data)
                        {
                            string KeyName = data.Key;
                            string bitValue = data.Value;
                            if (KeyName == DO_SensorName)// || DO_SensorName == "ALL")
                            {
                                bool SetFlag = Device.SETIO(int.Parse(bitValue), ONOFF);
                                if (!SetFlag)
                                {
                                    LogMessage($"SensorName:{DO_SensorName} Channel:{bitValue} Status:{ONOFF}", MessageLevel.Error);
                                    output = $"SensorName:{DO_SensorName} Channel:{bitValue} Status:{ONOFF} Fails";
                                    return false;
                                }
                                //Thread.Sleep(SetIO_DelayTime);
                            }
                        }
                        LogMessage($"Set IO :({DO_SensorName}) = {ONOFF}. Success", MessageLevel.Info);
                        output = $"Set IO :({DO_SensorName}) = {ONOFF}. Success";
                    }
                    else
                    {
                        LogMessage($"DO_Data Can not be Null or Empty", MessageLevel.Error);
                        output = $"Set IO :({DO_SensorName}) = {ONOFF}. Fails";
                        return false;
                    }
                }
                else
                {
                    LogMessage($"DO_SensorName Can not be Null or Empty", MessageLevel.Error);
                    output = $"Set IO :({DO_SensorName}) = {ONOFF}. Fails";
                    return false;
                }
            }
            catch (Exception ex)
            {
                LogMessage($"SetIO Error:{ex.Message}", MessageLevel.Error);
                output = $"Set IO :({DO_SensorName}) = {ONOFF}.Exception Error";
                return false;
            }

            return true;
        }
        public bool GetIOParse(IOBase Device, string sensorName, ref bool status)
        {
            string value = string.Empty;           
            DI_Data = Device.GetDIForm();
            if (DI_Data.TryGetValue(sensorName, out value))
            {
                return Device.GETIO(int.Parse(value), ref status);
            }
            throw new KeyNotFoundException($"Sensor '{sensorName}' not found in GetIO map.");
        }

        public bool GetIO(IOBase Device, ref string output)
        {
            strOutData = string.Empty;
            if (!string.IsNullOrEmpty(DI_SensorName))
            {
                bool status = false;
                bool Check_Done = false;
                bool Time_out = true;
                var stopwatch = new Stopwatch();

                DI_Data = Device.GetDIForm();
                if (DI_Data != null)
                {                   
                    stopwatch.Start();
                    LogMessage("Waiting...", MessageLevel.Debug);

                    while (stopwatch.ElapsedMilliseconds < Timeout && !Check_Done)
                    {
                        int Count_num = 0;
                        foreach (var data in DI_Data)
                        {
                            string KeyName = data.Key;
                            string bitValue = data.Value;
                            if (KeyName == DI_SensorName)
                            {
                                Count_num++;
                                bool Get_Flag = Device.GETIO(int.Parse(bitValue), ref status);
                                if (!Get_Flag)
                                {
                                    LogMessage($"GETIO Fail:SensorName:{DI_SensorName} Channel:{bitValue} Status:{status}", MessageLevel.Error);
                                    output = $"GETIO Fail:SensorName:{DI_SensorName} Channel:{bitValue} Status:{status}";
                                    stopwatch.Stop();
                                    stopwatch.Reset();
                                    return false;
                                }

                                if (Check == status)
                                {
                                    Check_Done = true;
                                    Time_out = false;
                                    LogMessage($"CheckIOStatus Success : SensorName:{DI_SensorName} Channel:{bitValue} Status:{status}", MessageLevel.Info);
                                    output = $"CheckIOStatus Success : SensorName:{DI_SensorName} Channel:{bitValue} Status:{status}";
                                    break;
                                }
                            }
                        }
                        Thread.Sleep(10);
                    }
                    if (Time_out)
                    {
                        LogMessage($"GETIO TimeOut", MessageLevel.Error);
                        LogMessage($"CheckIOStatus Fail : SensorName:{DI_SensorName} Status:{status}", MessageLevel.Warn);
                        output = $"CheckIOStatus Fail : SensorName:{DI_SensorName}  Status:{status}";
                        stopwatch.Stop();
                        stopwatch.Reset();
                        return false;
                    }
                }
            }
            else
            {
                LogMessage($"DI_SensorName Can not be Null or Empty", MessageLevel.Error);
                output = $"DI_SensorName Can not be Null or Empty";
                return false;
            }

            return true;
        }

        public class IOData
        {
            public string IO_Name { get; set; }
            public string IO_Status { get; set; }
        }

        public class SensorSetIOList : TypeConverter
        {

            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                dynamic currentObject = context.Instance;
                List<string> TableList = new List<string>();
                List<Sensor> sensors = new List<Sensor>();


                if (string.IsNullOrEmpty(currentObject.DeviceSel))
                {
                    return new StandardValuesCollection(new string[] { });
                }


                if (GlobalNew.Devices.ContainsKey(currentObject.DeviceSel) == true)
                {
                    IOBase IO = (IOBase)GlobalNew.Devices[currentObject.DeviceSel];
                    if (!string.IsNullOrWhiteSpace(IO.SetIO_List))
                    {

                        sensors = JsonConvert.DeserializeObject<List<Sensor>>(IO.SetIO_List);

                        foreach (var sensor in sensors)
                        {
                            TableList.Add(sensor.SensorName);
                        }
                    }

                }

                //TableList.Add("ALL");
                return new StandardValuesCollection(TableList.ToArray());
            }

            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                return true;
            }
            class Sensor
            {
                public string SensorName { get; set; }
                public string Channel { get; set; }
            }

        }

        public class SensorGetIOList : TypeConverter
        {

            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                dynamic currentObject = context.Instance;
                List<string> TableList = new List<string>();
                List<Sensor> sensors = new List<Sensor>();


                if (string.IsNullOrEmpty(currentObject.DeviceSel))
                {
                    return new StandardValuesCollection(new string[] { });
                }


                if (GlobalNew.Devices.ContainsKey(currentObject.DeviceSel) == true)
                {
                    IOBase IO = (IOBase)GlobalNew.Devices[currentObject.DeviceSel];
                    if (!string.IsNullOrWhiteSpace(IO.GetIO_List))
                    {

                        sensors = JsonConvert.DeserializeObject<List<Sensor>>(IO.GetIO_List);

                        foreach (var sensor in sensors)
                        {
                            TableList.Add(sensor.SensorName);
                        }
                    }

                }

                //TableList.Add("ALL");
                return new StandardValuesCollection(TableList.ToArray());
            }

            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                return true;
            }
            class Sensor
            {
                public string SensorName { get; set; }
                public string Channel { get; set; }
            }

        }

        public class Muti_IOEditor : System.Drawing.Design.UITypeEditor
        {
            public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
            {
                dynamic currentObject = context.Instance;
                using (var MutiIOTable = new MutiIOSelect(currentObject.DeviceSel))
                {
                    // 如果有現有的值，將其加載到表單中
                    if (value != null)
                    {
                        MutiIOTable.LoadDataGridViewFromJson(value.ToString());
                    }

                    var result = MutiIOTable.ShowDialog();
                    string json = MutiIOTable.GetDataGridViewAsJson();
                    return MutiIOTable.GetDataGridViewAsJson();

                }

            }


            public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
            {
                return UITypeEditorEditStyle.Modal;
            }
        }

    }
}
