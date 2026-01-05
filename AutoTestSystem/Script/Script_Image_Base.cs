
using AutoTestSystem.Base;
using AutoTestSystem.Equipment.CCD;
using AutoTestSystem.Equipment.ControlDevice;
using AutoTestSystem.Model;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.ComponentModel.TypeConverter;

namespace AutoTestSystem.Script
{
    public class Script_Image_Base : ScriptBase, IDisposable
    {
        public Image_Base ImageDevice = null;

        [Category("Device"), Description("自訂顯示名稱"), TypeConverter(typeof(Image_DevList))]
        public string DeviceSel { get; set; }


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
        public override bool Action(object DictDevices)
        {
            try
            {
                Dictionary<string, object> Devices = DictDevices as Dictionary<string, object>;

                if (Devices == null)
                {
                    LogMessage($"DictDevices is null");
                    return false; // 操作成功
                }

                RowDataItem.TestResult = "FAIL";

                if (DeviceSel == string.Empty)
                {
                    LogMessage($"Image Device is null");
                    return false;
                }

                if (Devices.ContainsKey(DeviceSel) == false)
                {
                    LogMessage($"Devices.ContainsKey(DeviceSel)  is null");
                    return false;
                }

                if (Devices.ContainsKey(DeviceSel) == false)
                {
                    string ref_Dev = FindMultiDeviceName(DeviceSel);

                    if (Devices.ContainsKey(ref_Dev))
                    {
                        LogMessage($"FindMultiDevice({ref_Dev})");
                        ImageDevice = (Image_Base)Devices[ref_Dev];
                    }
                    else
                    {
                        LogMessage($"{ref_Dev} not found device");
                        return false;
                    }
                }
                else
                    ImageDevice = (Image_Base)Devices[DeviceSel];

                string strDataOut = string.Empty;

                if (PreProcess())
                {
                    if (Process(ImageDevice, ref strDataOut))
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
        #region 新腳本使用
        public virtual bool PreProcess()
        {
            return true;
        }
        public virtual bool Process(Image_Base CCD, ref string strDataout)
        {
            return true;
        }
        public virtual bool PostProcess()
        {

            return true;
        }
        #endregion 新腳本使用


        public class Image_DevList : TypeConverter  //下拉式選單
        {
            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {

                if (GlobalNew.Devices.Count != 0)
                {
                    List<string> hwListKeys = new List<string>();

                    hwListKeys.Add("");
                    hwListKeys.AddRange(GlobalNew.Devices
                        .Where(item => item.Value is Image_Base)
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
                                if (GlobalNew.Devices[deviceObject] is Image_Base)
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
