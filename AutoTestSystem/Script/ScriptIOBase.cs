using AutoTestSystem.Base;
using AutoTestSystem.DUT;
using AutoTestSystem.Equipment.IO;
using AutoTestSystem.Equipment.Motion;
using AutoTestSystem.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTestSystem.Script
{
    public  class ScriptIOBase : ScriptBase,IDisposable
    {
        [JsonIgnore]
        public IOBase IODevice = null;

        [Category("Common Parameters"), Description("自訂顯示名稱"), TypeConverter(typeof(IO_DevList))]
        public string DeviceSel { get; set; }

        public ScriptIOBase()
        {
            DeviceSel = "";
        }

        public virtual void Dispose()
        {

        }
        public override void LogMessage(string message, MessageLevel mode = MessageLevel.Debug)
        {
            if (GlobalNew.FormMode == "1")
            {
                base.LogMessage(message, mode);
            }
            //else
            //{
            //    switch (mode)
            //    {
            //        case MessageLevel.Debug:
            //            Manufacture.Equipment.EquipmentLogger.Debug($"[{Description}] {message}");
            //            break;
            //        case MessageLevel.Info:
            //            Manufacture.Equipment.EquipmentLogger.Info($"[{Description}] {message}");
            //            break;
            //        case MessageLevel.Warn:
            //            Manufacture.Equipment.EquipmentLogger.Warn($"[{Description}] {message}");
            //            break;
            //        case MessageLevel.Error:
            //            Manufacture.Equipment.EquipmentLogger.Error($"[{Description}] {message}");
            //            break;
            //        case MessageLevel.Fatal:
            //            Manufacture.Equipment.EquipmentLogger.Fatal($"[{Description}] {message}");
            //            break;
            //    }
            //}
        }

        #region 新腳本使用
        public override bool Action(object DictDevices)
        {
            try
            {
                Dictionary<string, object> Devices = DictDevices as Dictionary<string, object>;

                if (Devices == null)
                {
                    LogMessage($"DictDevices is null", MessageLevel.Error);
                    return false; // 操作成功
                }

                RowDataItem.TestResult = "FAIL";

                if (DeviceSel == string.Empty)
                {
                    LogMessage($"[IOBase] IO Device is null", MessageLevel.Error);
                    return false;
                }

                if (Devices.ContainsKey(DeviceSel) == false)
                {
                    string ref_Dev = FindMultiDeviceName(DeviceSel);

                    if (Devices.ContainsKey(ref_Dev))
                    {
                        LogMessage($"FindMultiDevice({ref_Dev})");
                        IODevice = (IOBase)Devices[ref_Dev];
                    }
                    else
                    {
                        LogMessage($"{ref_Dev} not found multidevice");
                        return false;
                    }
                }
                else
                    IODevice = (IOBase)Devices[DeviceSel];


                string strDataOut = string.Empty;

                if (PreProcess())
                {
                    if (Process(IODevice, ref strDataOut))
                    {
                        if (PostProcess())
                        {
                            RowDataItem.TestResult = "PASS";
                        }
                    }
                }

                RowDataItem.OutputData = strDataOut;
                return RowDataItem.TestResult == "PASS";

            }
            catch (Exception ex)
            {
                RowDataItem.TestResult = "FAIL";
                LogMessage($"Exception {ex.Message}", MessageLevel.Error);
                RowDataItem.OutputData = $"{Description} Action Exception. {ex.Message}";
                return false;
            }
        }

        public virtual bool Process(IOBase Device, ref string strDataout)
        {
            return true;
        }
        public virtual bool Process()
        {
            return true;
        }

        public virtual bool PostProcess()
        {
            return true;
        }
        #endregion 新腳本使用

        public virtual bool PreProcess(string ActionItem, string Paraminput)
        {
            return true;
        }
        public virtual bool Process(IOBase Device)
        {
            return true;
        }

        public virtual bool PreProcess()
        {
            return true;
        }


        public virtual bool PostProcess(string strCheckSpec,ref string strDataout)
        {
            return true;
        }

        public enum IO_Mode
        {
            Set,
            Get,
            SetGet,
            Multi
        }

        public class IO_DevList : TypeConverter  //下拉式選單
        {
            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {

                if (GlobalNew.Devices.Count != 0)
                {
                    List<string> hwListKeys = new List<string>();

                    hwListKeys.Add("");
                    hwListKeys.AddRange(GlobalNew.Devices
                        .Where(item => item.Value is IOBase)
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
                                if (GlobalNew.Devices[deviceObject] is IOBase)
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
    }
}
