

using AutoTestSystem.Base;
using AutoTestSystem.DUT;
using AutoTestSystem.Equipment.ControlDevice;
using AutoTestSystem.Equipment.Teach;
using AutoTestSystem.Model;
using Manufacture;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTestSystem.Script
{
   public class Script_Extra_Base : ScriptBase,IDisposable
    {
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

        public virtual void Dispose()
        {

        }
        public override bool Action(object obj)
        {
            string strDataOut = string.Empty;
            try
            {
                RowDataItem.TestResult = "FAIL";

                

                if (PreProcess())
                {
                    if (Process(ref strDataOut))
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
            catch (DumpException ex)
            {
                
                RowDataItem.TestResult = "PASS";
                RowDataItem.OutputData = strDataOut;
                LogMessage($"{strDataOut}");
                LogMessage($"Exception {ex.Message}");
                throw new DumpException(ex.Message);
            }
            catch (Exception ex)
            {
                
                RowDataItem.TestResult = "FAIL";         
                RowDataItem.OutputData = $"{Description} Action Exception. {ex.Message}";
                LogMessage($"{strDataOut}");
                LogMessage($"Exception {ex.Message}");
                return false;
            }

        }
        public virtual bool PreProcess(string ActionItem, string Paraminput)
        {
            return true;
        }
        public virtual bool PreProcess()
        {
            return true;
        }
        public virtual bool Process()
        {
            return true;
        }
        public virtual bool Process(ref string strDataout)
        {
            return true;
        }
        public virtual bool PostProcess()
        {

            return true;
        }



    }

    public class Extra_DevList : TypeConverter  //下拉式選單
    {
        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            if (GlobalNew.Devices.Count != 0)
            {
                List<string> hwListKeys = new List<string>();
                hwListKeys.Add("");
                hwListKeys.Add("Echo");
                hwListKeys.Add("OneTimeExe");
                hwListKeys.Add("OneTimeFile");
                hwListKeys.Add("OneTimeTcp");
                hwListKeys.AddRange(GlobalNew.Devices
                        .Where(item => item.Value is DUT_Simu || item.Value is DUT_SERIALPORT || item.Value is SerialPortDevice || item.Value is FILE ||item.Value is ExeProcess || item.Value is MES || item.Value is SSHDevice || item.Value is SecureShell || item.Value is MotionBase || item.Value is TcpIpClient || item.Value is OpenShort)
                        .Select(item => item.Key)
                        .ToList()
                        );

                string multiDeviceTable = string.Empty;
                foreach (var value in GlobalNew.Devices.Values)
                {
                    if (value is DUT_BASE)
                    {
                        if(((DUT_BASE)(value)).Enable)
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
                            if (GlobalNew.Devices[deviceObject] is DUT_Simu || GlobalNew.Devices[deviceObject] is DUT_SERIALPORT || GlobalNew.Devices[deviceObject] is SerialPortDevice)
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

    public class Extra_TeachList : TypeConverter  //下拉式選單
    {
        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            if (GlobalNew.Devices.Count != 0)
            {
                List<string> hwListKeys = new List<string>();
                hwListKeys.Add("");
                hwListKeys.AddRange(GlobalNew.Devices
                        .Where(item => item.Value is MotionTeach)
                        .Select(item => item.Key)
                        .ToList()
                        );
                
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

    public class RotaryControllerList : TypeConverter  //下拉式選單
    {
        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            if (GlobalNew.Devices.Count != 0)
            {
                List<string> hwListKeys = new List<string>();
                hwListKeys.Add("");
                hwListKeys.AddRange(GlobalNew.Devices
                        .Where(item => item.Value is RotaryTestController)
                        .Select(item => item.Key)
                        .ToList()
                        );

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
    public class IOTeachList : TypeConverter  //下拉式選單
    {
        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            if (GlobalNew.Devices.Count != 0)
            {
                List<string> hwListKeys = new List<string>();
                hwListKeys.Add("");
                hwListKeys.AddRange(GlobalNew.Devices
                        .Where(item => item.Value is IOTeach)
                        .Select(item => item.Key)
                        .ToList()
                        );

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
