using AutoTestSystem.Base;
using AutoTestSystem.DUT;
using AutoTestSystem.Equipment.IO;
using AutoTestSystem.Equipment.Teach;
using AutoTestSystem.Model;
using DocumentFormat.OpenXml.Drawing.Diagrams;
using NAudio.Gui;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Design;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using static AutoTestSystem.BLL.Bd;
using static AutoTestSystem.MutiAIForm;
namespace AutoTestSystem.Script
{
    public enum IO_Mode
    {
        Set,
        Get,
        SetGet,
        Multi,
        GetAI,
        GetAllAI
    }
    internal class Script_IO_ControlTeach : Script_Extra_Base
    {
        string strOutData = string.Empty;


        [Category("Common Parameters")]
        [Description("自訂顯示名稱")]
        [TypeConverter(typeof(IOTeachList))]
        public string DeviceSel { get; set; }


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

        [Category("GetMutiAI Parameters"), Description("自訂顯示名稱"), Editor(typeof(Muti_AIEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public string MutiAI { get; set; } = "";

        [Category("GetAI Parameters"), Description("量測點位"), TypeConverter(typeof(SensorGetAIList))]
        public string GetAI_Name { get; set; } = "";
        [Category("GetAI Parameters"), Description("時間間隔")]
        public string Interval { get; set; } = "50";
        [Category("GetAI Parameters"), Description("平均次數")]
        public string Times { get; set; } = "1";

        [Category("SetMuti GetIO Parameters"), Description("Timeout(ms)")]
        public int TimeoutForMuti { get; set; } = 999999;

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
        public override bool Process(ref string output)
        {
            IOTeach ioTeach = null;
            if (GlobalNew.Devices.ContainsKey(DeviceSel))
            {
                ioTeach = GlobalNew.Devices[DeviceSel] as IOTeach;
            }
            else
            {
                LogMessage("IOTeach is null");
                return false;
            }

            bool Ret;
            string OutPut = string.Empty;
            try
            {
                switch (Mode)
                {
                    case IO_Mode.Set:
                        Ret = SetIO(ioTeach, ON_OFF, ref OutPut);
                        strOutData = JsonConvert.SerializeObject(OutPut, Formatting.Indented);
                        output = strOutData;
                        if (!Ret)
                            return false;
                        break;
                    case IO_Mode.Get:
                        Ret = GetIO(ioTeach, ref OutPut);
                        strOutData = JsonConvert.SerializeObject(OutPut, Formatting.Indented);
                        output = strOutData;
                        if (!Ret)
                            return false;
                        break;

                    case IO_Mode.GetAllAI:
                        Ret = GetAll_AI(ioTeach, ref OutPut);
                        strOutData = OutPut;
                        output = strOutData;
                        LogMessage(output, MessageLevel.Info);
                        if (!Ret)
                            return false;
                        break;

                    case IO_Mode.GetAI:
                        Ret = Get_AI(ioTeach, ref OutPut);
                        strOutData = OutPut;
                        output = strOutData;
                        LogMessage(output, MessageLevel.Info);
                        if (!Ret)
                            return false;
                        break;
                    case IO_Mode.SetGet:

                        string tmp_msg = string.Empty;
                        var Data = new Dictionary<string, object>();
                        bool bSuccess = true;

                        if (!SetIO(ioTeach, ON_OFF, ref tmp_msg))
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

                        if (!GetIO(ioTeach, ref tmp_msg))
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

                        if (!SetIO(ioTeach, !ON_OFF, ref tmp_msg))
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
                            var stopwatch = new Stopwatch();

                            stopwatch.Start();
                            LogMessage("Waiting...", MessageLevel.Debug);

                            while (stopwatch.ElapsedMilliseconds < TimeoutForMuti)
                            {
                                var allStatus = ioTeach.GetAllInputStatusFromCards();
                                bool allMatched = true;

                                foreach (var item in dataList)
                                {
                                    if (!allStatus.TryGetValue(item.IO_Name, out bool status))
                                    {
                                        LogMessage($"KeyName: {item.IO_Name} not found in IO map", MessageLevel.Error);
                                        return false;
                                    }

                                    LogMessage($"KeyName: {item.IO_Name}, Expected: {item.IO_Status}, Actual: {status}");

                                    if (status != bool.Parse(item.IO_Status))
                                    {
                                        LogMessage($"KeyName: {item.IO_Name}, Expected: {item.IO_Status}, Actual: {status}", MessageLevel.Error);
                                        allMatched = false;
                                        break;
                                    }
                                }

                                if (allMatched)
                                    break;

                                if (GlobalNew.g_shouldStop)
                                    return false;

                                Thread.Sleep(10);
                            }

                            stopwatch.Stop();

                            if (stopwatch.ElapsedMilliseconds >= TimeoutForMuti)
                            {
                                LogMessage($"GETIO TimeOut", MessageLevel.Error);
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

        public bool SetIO(IOTeach Device, bool ONOFF, ref string output)
        {
            try
            {
                string SensorKey = ReplaceProp(DO_SensorName);
                strOutData = string.Empty;
                if (!string.IsNullOrEmpty(SensorKey))
                {
                    bool Set_Flag = Device.SetIO(SensorKey, ONOFF);
                    if (!Set_Flag)
                    {
                        LogMessage($"SensorName:Set {DO_SensorName} ", MessageLevel.Error);
                        output = $"SensorName:Set {DO_SensorName} Fails";
                        return false;
                    }
                            //Thread.Sleep(SetIO_DelayTime);
                            
                        
                    LogMessage($"Set IO :({DO_SensorName}) = {ONOFF}. Success", MessageLevel.Info);
                    output = $"Set IO :({DO_SensorName}) = {ONOFF}. Success";

                    return true;

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
        }
        //public bool GetAll_AI(IOTeach Device, ref string output)
        //{
        //    string SensorKey = ReplaceProp(DI_SensorName);
        //    strOutData = string.Empty;

        //    if (string.IsNullOrEmpty(SensorKey))
        //    {
        //        LogMessage($"DI_SensorName Can not be Null or Empty", MessageLevel.Error);
        //        output = $"DI_SensorName Can not be Null or Empty";
        //        return false;
        //    }

        //    bool status = false;
        //    var stopwatch = new Stopwatch();
        //    stopwatch.Start();

        //    LogMessage($"Check({SensorKey} = {Check}), Waiting");

        //    while (stopwatch.ElapsedMilliseconds < Timeout)
        //    {
        //        bool getFlag = Device.GetIO(SensorKey, ref status);
        //        if (!getFlag)
        //        {
        //            LogMessage($"GETIO Fail: SensorName:{SensorKey}", MessageLevel.Error);
        //            output = $"GETIO Fail: SensorName:{SensorKey}";
        //            return false;
        //        }

        //        if (Check == status)
        //        {
        //            LogMessage($"CheckIOStatus Success : SensorName:{SensorKey}", MessageLevel.Info);
        //            output = $"CheckIOStatus Success : SensorName:{SensorKey}";
        //            return true;
        //        }

        //        if (GlobalNew.g_shouldStop)
        //        {
        //            output = $"Operation stopped by user.";
        //            return false;
        //        }

        //        Thread.Sleep(10);
        //    }

        //    LogMessage($"GETIO TimeOut", MessageLevel.Error);
        //    LogMessage($"CheckIOStatus Fail : SensorName:{SensorKey} Status:{status}", MessageLevel.Warn);
        //    output = $"CheckIOStatus Fail : SensorName:{SensorKey} Status:{status}";
        //    return false;
        //}


        public bool GetAll_AI(IOTeach Device, ref string output)
        {
            try
            {
                // === 基本驗證 ===
                if (Device == null)
                {
                    output = "Device == null";
                    LogMessage(output, MessageLevel.Error);
                    return false;
                }

                // 解析欲撈清單
                List<DeviceAIItem> list;
                try
                {
                    list = JsonConvert.DeserializeObject<List<DeviceAIItem>>(MutiAI ?? "[]") ?? new List<DeviceAIItem>();
                }
                catch (Exception exCfg)
                {
                    output = $"Config JSON parse error: {exCfg.Message}";
                    LogMessage(output, MessageLevel.Error);
                    return false;
                }

                // 清單為空：直接回傳
                var selectedNames = list.Where(x => !string.IsNullOrWhiteSpace(x?.AI_Name)).Select(x => x.AI_Name.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
                if (selectedNames.Count == 0)
                {
                    output = "{}"; // 空 JSON
                    LogMessage("AI list config is empty.", MessageLevel.Warn);
                    return true;
                }

                // 解析平均次數與間隔
                int times = 1;
                int intervalMs = 50;
                if (!int.TryParse(Times, out times) || times <= 0) times = 1;
                if (!int.TryParse(Interval, out intervalMs) || intervalMs < 0) intervalMs = 0;

                // 為每個點位建立 Samples 容器
                var samplesMap = new Dictionary<string, List<double>>(StringComparer.OrdinalIgnoreCase);
                foreach (var name in selectedNames) samplesMap[name] = new List<double>();

                // === 依 times 次數做輪詢取樣 ===
                for (int t = 0; t < times; t++)
                {
                    Dictionary<string, string> snapshot = null;

                    // 呼叫裝置：一次回來所有 AI 的電壓字串
                    try
                    {
                        snapshot = Device.GetAllAIInputVoltageFromCards();
                    }
                    catch (Exception exDev)
                    {
                        output = $"Device.GetAllAIInputVoltageFromCards exception: {exDev.Message}";
                        LogMessage(output, MessageLevel.Error);
                        return false;
                    }

                    if (snapshot == null)
                    {
                        output = "Device snapshot is null.";
                        LogMessage(output, MessageLevel.Error);
                        return false;
                    }

                    // 逐點位取值與解析
                    foreach (var name in selectedNames)
                    {
                        string raw = null;
                        snapshot.TryGetValue(name, out raw);

                        if (!GetAIHelper.TryExtractDouble(raw, out var v))
                        {
                            // 這筆失敗就略過；若要中止，可改成 return false
                            LogMessage($"Parse failed: AI='{name}', raw='{raw ?? "null"}' (skip).", MessageLevel.Warn);
                            continue;
                        }
                        samplesMap[name].Add(v);
                    }

                    // 間隔（最後一次不必睡）
                    if (t < times - 1 && intervalMs > 0)
                    {
                        try { Thread.Sleep(intervalMs); }
                        catch (ThreadInterruptedException tie)
                        {
                            output = $"Sleep interrupted: {tie.Message}";
                            LogMessage(output, MessageLevel.Error);
                            return false;
                        }
                    }
                }

                // === 產生扁平 JSON（每個點位用前綴 key） ===
                var outDict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                var logItems = new List<object>();

                foreach (var name in selectedNames)
                {
                    var samples = samplesMap[name];
                    if (samples.Count == 0)
                    {
                        // 沒有任何有效樣本，仍輸出空字串與 0；也可選擇不輸出該點位
                        outDict[$"{name}_Volt"] = 0.0;
                        outDict[$"{name}_Samples"] = "";
                        outDict[$"{name}_Max"] = 0.0;
                        outDict[$"{name}_Min"] = 0.0;

                        logItems.Add(new
                        {
                            AI_Name = name,
                            Times = times,
                            Interval_ms = intervalMs,
                            Samples = "",
                            Volt = 0.0,
                            Max = 0.0,
                            Min = 0.0
                        });
                        continue;
                    }

                    double avg = samples.Average();
                    double max = samples.Max();
                    double min = samples.Min();

                    double avgR = Math.Round(avg, 3);
                    double maxR = Math.Round(max, 3);
                    double minR = Math.Round(min, 3);

                    // 固定三位小數、逗號分隔、使用 '.' 作小數點
                    string samplesCsv = string.Join(
                        ",",
                        samples.Select(x => Math.Round(x, 3).ToString("0.000", CultureInfo.InvariantCulture))
                    );

                    outDict[$"{name}_Avg"] = avgR;
                    outDict[$"{name}_Samples"] = samplesCsv;
                    outDict[$"{name}_Max"] = maxR;
                    outDict[$"{name}_Min"] = minR;

                    //logItems.Add(new
                    //{
                    //    AI_Name = name,
                    //    Times = times,
                    //    Interval_ms = intervalMs,
                    //    Samples = samplesCsv,
                    //    Volt = avgR,
                    //    Max = maxR,
                    //    Min = minR
                    //});
                }

                output = JsonConvert.SerializeObject(outDict, Formatting.Indented);

                return true;
            }
            catch (Exception ex)
            {
                output = $"GetAll_AI unexpected error: {ex.Message}";
                LogMessage(output, MessageLevel.Error);
                return false;
            }
        }

        public bool Get_AI(IOTeach Device, ref string output)
        {
            try
            {
                // === 基本驗證 ===
                if (Device == null)
                {
                    output = "Device == null";
                    LogMessage(output, MessageLevel.Error);
                    return false;
                }

                string sensorKey = ReplaceProp(GetAI_Name);
                if (string.IsNullOrWhiteSpace(sensorKey))
                {
                    output = "GetAI_Name Can not be Null or Empty";
                    LogMessage(output, MessageLevel.Error);
                    return false;
                }

                // === 解析平均次數與間隔 ===
                int times = 1;
                int intervalMs = 50;
                if (!int.TryParse(Times, out times) || times <= 0) times = 1;
                if (!int.TryParse(Interval, out intervalMs) || intervalMs < 0) intervalMs = 0;

                var samples = new List<double>();

                // === 依 times 次數抓取數值並做間隔 ===
                for (int i = 0; i < times; i++)
                {
                    string oneShot = null;

                    // 裝置呼叫加細部防護，避免外部 API 丟例外
                    try
                    {
                        // 你的 IOTeach API：結果回填到 oneShot
                        Device.GetAI(sensorKey, ref oneShot);
                    }
                    catch (Exception exCall)
                    {
                        output = $"Device.GetAI exception: {exCall.Message}";
                        LogMessage(output, MessageLevel.Error);
                        return false;
                    }

                    // 解析值
                    if (!GetAIHelper.TryExtractDouble(oneShot, out var v))
                    {
                        output = $"Parse AI value failed. raw=\"{oneShot ?? "null"}\"";
                        LogMessage(output, MessageLevel.Error);
                        return false;

                        // 如果你要「略過失敗、繼續平均」，可改用：
                        // LogMessage($"Parse failed at sample #{i+1}, raw=\"{oneShot}\". Skip.", MessageLevel.Warn);
                        // continue;
                    }

                    samples.Add(v);

                    // 間隔（最後一次不必睡）
                    if (i < times - 1 && intervalMs > 0)
                    {
                        try { Thread.Sleep(intervalMs); }
                        catch (ThreadInterruptedException tie)
                        {
                            output = $"Sleep interrupted: {tie.Message}";
                            LogMessage(output, MessageLevel.Error);
                            return false;
                        }
                    }
                }


                // === 計算平均／最大／最小（單筆時平均即該值） ===
                double avg = samples.Count > 0 ? samples.Average() : 0.0;
                double maxValue = samples.Count > 0 ? samples.Max() : 0.0;
                double minValue = samples.Count > 0 ? samples.Min() : 0.0;

                // 四捨五入到小數 3 位
                double avgRounded = Math.Round(avg, 3);
                double maxRounded = Math.Round(maxValue, 3);
                double minRounded = Math.Round(minValue, 3);


                // === 逗號分隔的 Samples 字串（固定三位小數，使用 '.' 作小數點） ===
                string samplesCsv = string.Join(
                    ",",
                    samples.Select(x => Math.Round(x, 3).ToString("0.000", System.Globalization.CultureInfo.InvariantCulture))
                );

                // === 完整資訊寫入 Log（方便追溯） ===

                // 假設這些變數都已經算好
                // string sensorKey; double avgRounded, maxRounded, minRounded; string samplesCsv;

                // （可選）若 sensorKey 可能含空白或特殊符號，先淨化一下，避免 JSON key 長得怪
                string keyPrefix = sensorKey?.Trim() ?? "";
                // 例如只保留英數與底線：
                // keyPrefix = Regex.Replace(keyPrefix, @"[^\w]", "_");

                var jsonOutDict = new Dictionary<string, object>
                {
                    [$"{keyPrefix}_Avg"] = avgRounded,
                    [$"{keyPrefix}_Samples"] = samplesCsv,   // 例如 "2.000,2.111,2.333,2.123"
                    [$"{keyPrefix}_Max"] = maxRounded,
                    [$"{keyPrefix}_Min"] = minRounded
                };

                string jsonOut = JsonConvert.SerializeObject(jsonOutDict, Formatting.Indented);

                output = jsonOut;

                return true;
            }
            catch (ThreadAbortException tae)
            {
                // 若你的環境可能有 ThreadAbort，單獨抓住便於區分
                output = $"Thread aborted: {tae.Message}";
                LogMessage(output, MessageLevel.Error);
                return false;
            }
            catch (Exception ex)
            {
                // === 外層最後防線：任何未預期例外 ===
                output = $"Get_AI unexpected error: {ex.Message}";
                LogMessage(output, MessageLevel.Error);
                return false;
            }
        }


        public bool GetIO(IOTeach Device, ref string output)
        {
            string SensorKey = ReplaceProp(DI_SensorName);
            strOutData = string.Empty;

            if (string.IsNullOrEmpty(SensorKey))
            {
                LogMessage($"DI_SensorName Can not be Null or Empty", MessageLevel.Error);
                output = $"DI_SensorName Can not be Null or Empty";
                return false;
            }

            bool status = false;
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            LogMessage($"Check({SensorKey} = {Check}), Waiting");

            while (stopwatch.ElapsedMilliseconds < Timeout)
            {
                bool getFlag = Device.GetIO(SensorKey, ref status);
                if (!getFlag)
                {
                    LogMessage($"GETIO Fail: SensorName:{SensorKey}", MessageLevel.Error);
                    output = $"GETIO Fail: SensorName:{SensorKey}";
                    return false;
                }

                if (Check == status)
                {
                    LogMessage($"CheckIOStatus Success : SensorName:{SensorKey}", MessageLevel.Info);
                    output = $"CheckIOStatus Success : SensorName:{SensorKey}";
                    return true;
                }

                if (GlobalNew.g_shouldStop)
                {
                    output = $"Operation stopped by user.";
                    return false;
                }

                Thread.Sleep(10);
            }

            LogMessage($"GETIO TimeOut", MessageLevel.Error);
            LogMessage($"CheckIOStatus Fail : SensorName:{SensorKey} Status:{status}", MessageLevel.Warn);
            output = $"CheckIOStatus Fail : SensorName:{SensorKey} Status:{status}";
            return false;
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
                    
                    //if (!string.IsNullOrWhiteSpace(IO.))
                    //{
                        IOTeach ioteach = (IOTeach)GlobalNew.Devices[currentObject.DeviceSel];
                        TableList.AddRange(ioteach.GetDOKeys());
                        //sensors = JsonConvert.DeserializeObject<List<Sensor>>(IO.SetIO_List);

                        //foreach (var sensor in sensors)
                        //{
                        //    TableList.Add(sensor.SensorName);
                        //}
                    //}

                }

                //TableList.Add("ALL");
                return new StandardValuesCollection(TableList.ToArray());
            }

            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                return true;
            }

            public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
            {
                return false; // 表示允許手動輸入非選單中的值
            }

            public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
            {
                return sourceType == typeof(string);
            }

            public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
            {
                return value?.ToString();
            }

            public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
            {
                return destinationType == typeof(string);
            }

            public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
            {
                return value?.ToString();
            }
            class Sensor
            {
                public string SensorName { get; set; }
                public string Channel { get; set; }
            }

        }

        public class SensorGetIOList : TypeConverter
        {
            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                return true; // 表示支援下拉選單
            }

            public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
            {
                return false; // 表示允許手動輸入非選單中的值
            }

            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                List<string> tableList = new List<string>();

                try
                {
                    dynamic currentObject = context?.Instance;
                    if (currentObject == null || string.IsNullOrEmpty(currentObject.DeviceSel))
                        return new StandardValuesCollection(new string[] { });

                    if (GlobalNew.Devices.ContainsKey(currentObject.DeviceSel))
                    {
                        IOTeach ioTeach = GlobalNew.Devices[currentObject.DeviceSel] as IOTeach;
                        if (ioTeach != null)
                        {
                            tableList = ioTeach.GetGetIOKeys();
                        }
                    }
                }
                catch
                {
                    // 忽略錯誤，回傳空集合
                }

                return new StandardValuesCollection(tableList.ToArray());
            }

            public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
            {
                return sourceType == typeof(string);
            }

            public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
            {
                return value?.ToString();
            }

            public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
            {
                return destinationType == typeof(string);
            }

            public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
            {
                return value?.ToString();
            }
        }

        public class SensorGetAIList : TypeConverter
        {
            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                return true; // 表示支援下拉選單
            }

            public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
            {
                return false; // 表示允許手動輸入非選單中的值
            }

            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                List<string> tableList = new List<string>();

                try
                {
                    dynamic currentObject = context?.Instance;
                    if (currentObject == null || string.IsNullOrEmpty(currentObject.DeviceSel))
                        return new StandardValuesCollection(new string[] { });

                    if (GlobalNew.Devices.ContainsKey(currentObject.DeviceSel))
                    {
                        IOTeach ioTeach = GlobalNew.Devices[currentObject.DeviceSel] as IOTeach;
                        if (ioTeach != null)
                        {
                            tableList = ioTeach.GetAIKeys();
                        }
                    }
                }
                catch
                {
                    // 忽略錯誤，回傳空集合
                }

                return new StandardValuesCollection(tableList.ToArray());
            }

            public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
            {
                return sourceType == typeof(string);
            }

            public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
            {
                return value?.ToString();
            }

            public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
            {
                return destinationType == typeof(string);
            }

            public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
            {
                return value?.ToString();
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

        public class Muti_AIEditor : System.Drawing.Design.UITypeEditor
        {
            public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
            {
                dynamic currentObject = context.Instance;
                using (var MutiIOTable = new MutiAIForm(currentObject.DeviceSel))
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


    public class GetAIHelper // 放在你的類別同檔或同命名空間中即可
    {
        /// <summary>
        /// 從字串中嘗試抽出第一個數值（支援 "1.23"、"VOLT=1.23"、"value: 1.23V" 等）
        /// </summary>
        public static bool TryExtractDouble(string s, out double val)
        {
            val = 0;
            if (string.IsNullOrWhiteSpace(s)) return false;

            // 先嘗試 Invariant / 當地文化直接轉
            if (double.TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out val))
                return true;
            if (double.TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.CurrentCulture, out val))
                return true;

            // 再用 regex 抓第一個浮點數
            var m = Regex.Match(s, @"-?\d+(?:\.\d+)?");
            if (m.Success)
            {
                var token = m.Value;
                return double.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out val)
                    || double.TryParse(token, NumberStyles.Float, CultureInfo.CurrentCulture, out val);
            }
            return false;
        }
    }

}
