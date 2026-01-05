using AutoTestSystem.Base;
using AutoTestSystem.Equipment.DosBase;
using Manufacture;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AutoTestSystem.BLL.Bd;


namespace AutoTestSystem.Script
{
    internal class Script_PC_Win10_Pro : Script_ControlDevice_Base
    {
        string strActItem = string.Empty;
        string strOutData = string.Empty;

        [Category("Common Parameters"), Description("支援用%%方式做變數值取代")]
        public string Send_Command { get; set; }

        [Category("Common Parameters"), Description("自訂顯示名稱")]
        public int WaitTime { get; set; } = 3;

        [Category("Common Parameters"), Description("自訂顯示名稱")]
        public string CheckStr { get; set; }

        [Category("Common Parameters"), Description("自訂顯示名稱"), TypeConverter(typeof(ActionList))]
        public string ActionItem { get; set; } = "";

        [Category("Common Parameters"), Description("自訂顯示名稱"), TypeConverter(typeof(ModeList))]
        public string ModeItem { get; set; } = "Block";

        public override void Dispose()
        {
            //throw new NotImplementedException();
        }     

        public override bool PreProcess()
        {
            if (Send_Command == null || Send_Command == string.Empty)
            {
                LogMessage("Send_Command can not be null.", MessageLevel.Error);
                return false;
            }

            if (CheckStr == null || CheckStr == string.Empty)
            {
                LogMessage("CheckStr can not be null.", MessageLevel.Error);
                return false;
            }

            strActItem = ActionItem;

            return true;
        }
        public override bool Process(ControlDeviceBase PCCmd, ref string output)
        {
            //PCCmd.SetTimeout(WaitTime);
            if (ModeItem == "Non-Block")
            {
                PCCmd.SendNonblock(ReplaceProp(Send_Command), ref output);
                strOutData = output;
            }
            else
            {
                //PCCmd.SetCheckstr(ReplaceProp(CheckStr));
                LogMessage($"Send:  {ReplaceProp(Send_Command)}\n",MessageLevel.Info);
                PCCmd.MultiSend_Read(ReplaceProp(Send_Command), ReplaceProp(strActItem), ReplaceProp(CheckStr), WaitTime, ref output);
               
                //PCCmd.READ(ref output);
                LogMessage($"Read END:  {output}\n", MessageLevel.Info);
                strOutData = output;
            }
            return true;

        }
        public override bool PostProcess()
        {
            string result = CheckRule(strOutData, Spec);

            if (result == "PASS" || Spec == "")
                return true;
            else
                return false;

        }

    }

    public class ActionList : TypeConverter  //下拉式選單
    {
        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            List<string> typeList = new List<string>() { "", "audioAnalyse", "AnalyseDB", "LedCountRatio", "LedIntensity", "Camera_SFR", "Camera_RI", "Camera_DarkCornor", "Camera_AWB", "Camera_IRIS", "Camera_DUST","Camera_OC", "Camera_DP_WF", "JSON_Date","VKD_Camera_pkey","VKD_Camera_sign", "line_break_parser", "VKD_Vendor_JSON_DATA" };

            return new StandardValuesCollection(typeList);
        }

      
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;

        }
    }
    
    public class ModeList : TypeConverter  //下拉式選單
    {
        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            List<string> typeList = new List<string>() { "Block", "Non-Block" };

            return new StandardValuesCollection(typeList);
        }


        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;

        }
    }
}
