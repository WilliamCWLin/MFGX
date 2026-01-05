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
    internal class Script_PC_Win10_Related : Script_ControlDevice_Base
    {
        string strActItem = string.Empty;
        string strParam = string.Empty;
        string strOutData = string.Empty;
        DosCmd dosCmd = null;

        [Category("Common Parameters"), Description("自訂顯示名稱"), Editor(typeof(JsonEditor), typeof(UITypeEditor))]
        public string Params { get; set; }

        [Category("Common Parameters"), Description("自訂顯示名稱")]
        public string ActionItem { get; set; }

        public override void Dispose()
        {
            //throw new NotImplementedException();
        }

        public override bool Process()
        {
            strOutData = "";
            string RP = ReplaceKeys(Params, "");
            dosCmd = JsonConvert.DeserializeObject<DosCmd>(RP);
            strActItem = ActionItem;
            return Process(CtrlDevice);
        }

        public override bool PreProcess(string Actionitem, string strParamInput)
        {
            //設定要執行的ITEM及初始化一些參數
            strActItem = Actionitem;
            strParam = strParamInput;
            dosCmd = JsonConvert.DeserializeObject<DosCmd>(strParam);
            return true;
        }
        public override bool Process(ControlDeviceBase PCCmd)
        {
            bool pass_fail = true;
            string ack_data = "";
            string end_data = "";

            PCCmd.SetTimeout(dosCmd.WaitTime);
            PCCmd.SetCheckstr(dosCmd.CheckStr);
            PCCmd.Send(dosCmd.Send_Command, strActItem);
            LogMessage($"Send:  {dosCmd.Send_Command}\n");
            PCCmd.READ(ref end_data);
            LogMessage($"Read END:  {end_data}\n");
            strOutData = end_data;
            return pass_fail;

        }
        public override bool PostProcess(string strCheckSpec, ref string strDataout)
        {
            strDataout =  strOutData;
            string result = CheckRule(strOutData, strCheckSpec);

            if (result == "PASS" || strCheckSpec == "")
                return true;
            else
                return false;

        }

        public class DosCmd
        {
            public string Send_Command { get; set; }
            public int WaitTime { get; set; }
            public string CheckStr { get; set;}
        }
    }
}
