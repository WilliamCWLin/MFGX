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
    public abstract class Script_1MotBase : ScriptBase,IDisposable
    {
        public MotionBase MotorDevice = null;

        [Category("Common Parameters"), Description("自訂顯示名稱"), TypeConverter(typeof(Motion_DevList))]
        public string MotorDeviceSel { get; set; }


        public Script_1MotBase()
        {
            MotorDeviceSel = "";
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
                   
            try
            {
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
                    if (Process(MotorDevice, ref strDataOut))
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
        public virtual bool Process(MotionBase MotionDevice, ref string output)
        {
            return true;
        }
        public virtual bool PostProcess()
        {
            return true;
        }
        #endregion 新腳本使用

        public abstract bool PreProcess(string strParamInput);
        public abstract bool Process(MotionBase MotionDev);
        public abstract bool PostProcess(string TestKeyword, string strCheckSpec, ref string strDataout);
    }

}
