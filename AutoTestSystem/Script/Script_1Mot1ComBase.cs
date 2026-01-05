using AutoTestSystem.Base;
using AutoTestSystem.DAL;
using AutoTestSystem.DUT;
using AutoTestSystem.Equipment.ControlDevice;
using AutoTestSystem.Equipment.Motion;
using AutoTestSystem.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static AutoTestSystem.Script.Script_ControlDevice_Base;
using static AutoTestSystem.Script.ScriptIOBase;

namespace AutoTestSystem.Script
{
    public abstract class Script_1Mot1ComBase : ScriptBase,IDisposable
    {
        public ControlDeviceBase CtrlDevice = null;
        public MotionBase MotorDevice = null;

        [Category("Common Parameters"), Description("自訂顯示名稱"), TypeConverter(typeof(Motion_DevList))]
        public string MotorDeviceSel { get; set; }

        [Category("Common Parameters"), Description("自訂顯示名稱"), TypeConverter(typeof(CTRL_DevList))]
        public string ControlDeviceSel { get; set; }

        public Script_1Mot1ComBase()
        {
            MotorDeviceSel = "";
            ControlDeviceSel = "";
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
            Dictionary<string, object> Devices = DictDevices as Dictionary<string, object>;

            if (Devices == null)
            {
                LogMessage($"DictDevices is null", MessageLevel.Error);
                return false; // 操作成功
            }
            RowDataItem.TestResult = "FAIL";

            if (MotorDeviceSel == string.Empty)
            {
                LogMessage($"ControlDevice Device is null", MessageLevel.Error);
                return false;
            }
            if (ControlDeviceSel == string.Empty)
            {
                LogMessage($"ControlDevice Device is null", MessageLevel.Error);
                return false;
            }
                   
            try
            {

                if (Devices.ContainsKey(ControlDeviceSel) == false)
                {
                    string ref_Dev = FindMultiDeviceName(ControlDeviceSel);

                    if (Devices.ContainsKey(ref_Dev))
                    {
                        LogMessage($"FindMultiDevice({ref_Dev})");
                        CtrlDevice = (ControlDeviceBase)Devices[ref_Dev];
                    }
                    else
                    {
                        LogMessage($"{ref_Dev} not found multidevice");
                        return false;
                    }
                }
                else
                {
                    if (Devices.ContainsKey(ControlDeviceSel) == false)
                    {
                        LogMessage($"Devices.ContainsKey({ControlDeviceSel})  is null", MessageLevel.Error);

                        return false;
                    }
                    CtrlDevice = (ControlDeviceBase)Devices[ControlDeviceSel];
                }


                if (Devices.ContainsKey(MotorDeviceSel) == false)
                {
                    string ref_Dev = FindMultiDeviceName(MotorDeviceSel);

                    if (Devices.ContainsKey(ref_Dev))
                    {
                        LogMessage($"FindMultiDevice({ref_Dev})");
                        MotorDevice = (MotionBase)Devices[ref_Dev];
                    }
                    else
                    {
                        LogMessage($"{ref_Dev} not found multidevice");
                        return false;
                    }
                }
                else
                {
                    if (Devices.ContainsKey(MotorDeviceSel) == false)
                    {
                        LogMessage($"Devices.ContainsKey({MotorDeviceSel})  is null", MessageLevel.Error);

                        return false;
                    }
                    MotorDevice = (MotionBase)Devices[MotorDeviceSel];
                }

                string strDataOut = string.Empty;
                string fullJson = string.Empty;

                if (PreProcess())
                {
                    if (Process(CtrlDevice, MotorDevice, ref strDataOut))
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
                RowDataItem.OutputData = $"{Description} Action Exception. {ex.Message}";
                LogMessage($"Exception {ex.Message}", MessageLevel.Error);

                return false;
            }
        }

        public virtual bool PreProcess()
        {
            return true;
        }
        public virtual bool Process(ControlDeviceBase ControlDevice,MotionBase MotionDevice, ref string output)
        {
            return true;
        }
        public virtual bool PostProcess()
        {

            return true;
        }
        #endregion 新腳本使用

        public abstract bool PreProcess(string strParamInput);
        public abstract bool Process(ControlDeviceBase comport, MotionBase MotionDev);
        public abstract bool PostProcess(string TestKeyword,string strCheckSpec, ref string strDataout);
        
    }

    public class Motion_DevList : TypeConverter  //下拉式選單
    {
        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {

            if (GlobalNew.Devices.Count != 0)
            {
                List<string> hwListKeys = new List<string>();

                hwListKeys.Add("");
                hwListKeys.AddRange(GlobalNew.Devices
                    .Where(item => item.Value is MotionBase)
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
                            if (GlobalNew.Devices[deviceObject] is MotionBase)
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
    public class DIOEditor : System.Drawing.Design.UITypeEditor
    {
        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            using (var DIODialog = new Motion_DIOparam())
            {
                if (value == null)
                    value = string.Empty;

                DIODialog.SetParam(value.ToString());
                if (DIODialog.ShowDialog() == DialogResult.OK)
                {
                    return DIODialog.GetParam();
                }
                else
                {
                    MessageBox.Show($"The DIO param key or value exist \"Empty\",Please Check DIOparam From Setting", "SetDIOparam Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            return value; // 如果用戶取消選擇，返回原始值
        }

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }
    }
}
