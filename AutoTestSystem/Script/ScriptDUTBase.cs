using AutoTestSystem.Base;
using AutoTestSystem.DUT;
using AutoTestSystem.Equipment.Motion;
using AutoTestSystem.Model;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTestSystem.Script
{
    public  class ScriptDUTBase : ScriptBase,IDisposable
    {
        [Category("Common Parameters"), Description("自訂顯示名稱"), TypeConverter(typeof(DUT_DevList))]
        public string DeviceSel { get; set; }

        public ScriptDUTBase()
        {
            DeviceSel = "";
        }
        public override void LogMessage(string message, MessageLevel mode = MessageLevel.Debug)
        {
            if (GlobalNew.FormMode == "1")
            {
                base.LogMessage(message, mode);
                //switch (mode)
                //{
                //    case MessageLevel.Debug:
                //        HandleDevice?.LogMessage($"[{Description}] {message}", MessageLevel.Debug);
                //        break;
                //    case MessageLevel.Info:
                //        HandleDevice?.LogMessage($"[{Description}] {message}", MessageLevel.Info);
                //        break;
                //    case MessageLevel.Warn:
                //        HandleDevice?.LogMessage($"[{Description}] {message}", MessageLevel.Warn);
                //        break;
                //    case MessageLevel.Error:
                //        HandleDevice?.LogMessage($"[{Description}] {message}", MessageLevel.Error);
                //        break;
                //    case MessageLevel.Fatal:
                //        HandleDevice?.LogMessage($"[{Description}] {message}", MessageLevel.Fatal);
                //        break;
                //}
            }
            else
            {
                //switch (mode)
                //{
                //    case MessageLevel.Debug:
                //        LogManager.GetLogger("DUTLogger").Debug($"[{Description}] {message}");                       
                //        break;
                //    case MessageLevel.Info:
                //        LogManager.GetLogger("DUTLogger").Info($"[{Description}] {message}");
                //        break;
                //    case MessageLevel.Warn:
                //        LogManager.GetLogger("DUTLogger").Warn($"[{Description}] {message}");
                //        break;
                //    case MessageLevel.Error:
                //        LogManager.GetLogger("DUTLogger").Error($"[{Description}] {message}");
                //        break;
                //    case MessageLevel.Fatal:
                //        LogManager.GetLogger("DUTLogger").Fatal($"[{Description}] {message}");
                //        break;
                //}
            }

        }

        public virtual void Dispose()
        {

        }
        #region 舊腳本使用

        public virtual bool PreProcess(string ActionItem, string Paraminput)
        {
            return true;
        }

        public virtual bool PostProcess(string strCheckSpec, ref string strDataout)
        {
            return true;
        }

        #endregion 舊腳本使用

        #region 新腳本使用
        public virtual bool PreProcess()
        {
            return true;
        }
        public virtual bool PostProcess()
        {
            return true;
        }
        public override bool Action(object DictDevices)
        {

            DUT_BASE DUT = null;
            try
            {
                Dictionary<string, object> Devices = DictDevices as Dictionary<string, object>;

                if (Devices == null)
                {
                    LogMessage($"DictDevices is null", MessageLevel.Error);
                    return false; // 操作成功
                }

                if (DeviceSel == string.Empty)
                {
                    LogMessage("DeviceSel == string.Empty", MessageLevel.Error);
                    return false;
                }

                if (DeviceSel == "MULTI_THREAD")
                {
                    DUT = HandleDevice;
                }
                else
                {
                    if (Devices.ContainsKey(DeviceSel) == false)
                    {
                        string ref_Dev = FindMultiDeviceName(DeviceSel);

                        if (Devices.ContainsKey(ref_Dev))
                        {
                            LogMessage($"FindMultiDevice({ref_Dev})");
                            DUT = (DUT_BASE)Devices[ref_Dev];
                        }
                        else
                        {
                            LogMessage($"{ref_Dev} not found device");
                            return false;
                        }
                    }
                    else
                        DUT = (DUT_BASE)Devices[DeviceSel];
                }

                RowDataItem.TestResult = "FAIL";

                string strDataOut = string.Empty;

                if (PreProcess())
                {
                    if (Process(DUT, ref strDataOut))
                    {
                        if (PostProcess())
                        {
                            RowDataItem.TestResult = "PASS";
                        }
                    }
                }

                RowDataItem.OutputData = !string.IsNullOrEmpty(RowDataItem.OutputData)? RowDataItem.OutputData + Environment.NewLine + strDataOut: strDataOut;
                return RowDataItem.TestResult == "PASS";
 
            }
            catch(Exception ex)
            {

                RowDataItem.TestResult = "FAIL";
                LogMessage($"{this.Description}: Action Exception. {ex.Message}", MessageLevel.Error);
                RowDataItem.OutputData = $"{Description} Action Exception. {ex.Message}";

                return false;
            }
        }

        public virtual bool Process(DUT_BASE DUTDevice,ref string OutData)
        {
            return true;
        }

        #endregion 新腳本使用

        #region 新舊腳本共用
        public virtual bool Process(DUT_BASE DUTDevice)
        {
            return true;
        }

        #endregion 新舊腳本共用

        public class DUT_DevList : TypeConverter  //下拉式選單
        {
            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                if (GlobalNew.Devices.Count != 0)
                {
                    List<string> hwListKeys = new List<string>();

                    hwListKeys.Add("");
                    hwListKeys.Add("MULTI_THREAD");
                    hwListKeys.AddRange(GlobalNew.Devices
                            .Where(item =>item.Value is DUT_BASE)
                            .Select(item => item.Key)
                            .ToList()
                            );
                    string multiDeviceTable = string.Empty;
                    foreach (var value in GlobalNew.Devices.Values)
                    {
                        if (value is DUT_BASE)
                        {
                            if (((DUT_BASE)(value)).Enable)
                            {
                                multiDeviceTable = ((DUT_BASE)(value)).MultiDeviceTable;
                                break;
                            }
                        }
                    }
                    if (!string.IsNullOrEmpty(multiDeviceTable))
                    {
                        // 解析 JSON 字符串
                        JArray data = JArray.Parse(multiDeviceTable);

                        // 找到 DeviceObject 欄中的值是否在 GlobalNew.Devices 中，並將對應的 SharedName 值列到 hwListKeys 中
                        foreach (var item in data)
                        {
                            string deviceObject = (string)item["DeviceObject"];
                            if (GlobalNew.Devices.ContainsKey(deviceObject))
                            {
                                if (GlobalNew.Devices[deviceObject] is DUT_BASE)
                                    hwListKeys.Add($"@{(string)item["SharedName"]}@");
                            }
                        }
                    }

                    return new StandardValuesCollection(hwListKeys);
                }
                else
                {
                    return new StandardValuesCollection(new string[] { "" });
                }
            }

            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                return true;

            }
        }

        public class DataTypeList : TypeConverter  //下拉式選單
        {
            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                List<string> typeList = new List<string>() { "BIN", "OCT", "DEX", "HEX" };

                return new StandardValuesCollection(typeList);
            }

            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                return true;

            }
        }

        public class DataTypeList2 : TypeConverter  //下拉式選單
        {
            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                List<string> typeList = new List<string>() { "DEX", "HEX" };

                return new StandardValuesCollection(typeList);
            }

            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                return true;

            }
        }

        public class CRCTypeList : TypeConverter  //下拉式選單
        {
            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                List<string> typeList = new List<string>() { "NO", "CRC_ITU" };

                return new StandardValuesCollection(typeList);
            }

            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                return true;

            }
        }

        public class YES_NO_TypeList : TypeConverter  //下拉式選單
        {
            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                List<string> typeList = new List<string>() { "NO", "YES" };

                return new StandardValuesCollection(typeList);
            }

            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                return true;

            }
        }
    }
}
