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
    internal class Script_HTTPMESCmd_Related : Script_ControlDevice_Base
    {
        string strActItem = string.Empty;
        string strParam = string.Empty;
        string strOutData = string.Empty;
        MESCmd mesCmd = null;
        [Category("Common Parameters"), Description("MES_Command"), Editor(typeof(JsonEditor), typeof(UITypeEditor))]
        public string MES_Command { get; set; }

        [Category("Common Parameters"), Description("Keyword")]
        public string Keyword { get; set; }

        public override void Dispose()
        {
            //throw new NotImplementedException();
        }
        
        public override bool PreProcess(string Actionitem, string strParamInput)
        {
            //設定要執行的ITEM及初始化一些參數
            strActItem = Actionitem;
            strParam = strParamInput;
            mesCmd = JsonConvert.DeserializeObject<MESCmd>(strParam);
            return true;
        }
        public override bool Process()
        {
            strOutData = "";
            string RP = ReplaceKeys(MES_Command,"");
            try
            {
                LogMessage($"MES_Command Replace string. {RP}", MessageLevel.Debug);
                bool ret = PreProcess(Keyword, RP);

                return Process(CtrlDevice);
            }
            catch(Exception ex)
            {
                LogMessage($"Process Exception. {ex.Message}", MessageLevel.Error);
                return false;
            }

        }
        public override bool Process(ControlDeviceBase Cmd)
        {
            bool pass_fail = true;
            string ack_data = "";
            string end_data = "";
            string send_data = "";
            string Popmes_data = "";
            string Failitem = PopMoreData("Failitem");

            Cmd.SetCheckstr(mesCmd.CheckStr);
            Cmd.SetMEStcmd(mesCmd.Cmd);
            
            if (string.Equals(mesCmd.Cmd, "C003") == true)
            {
                if (Failitem == string.Empty)
                {
                    Cmd.SetMEStdata($"{mesCmd.Data}OK");
                }
                else
                {
                    Cmd.SetMEStdata($"{mesCmd.Data}NG;{Failitem}");
                }
            }
            else if(string.Equals(mesCmd.Cmd, "C004") == true)
            {
                /*string testmes2 = PopOneMESData(mesCmd.Data);
                Cmd.SetMEStdata($"{mesCmd.Data}{testmes2}");*/

                Popmes_data = PopALLMESData();             
                Cmd.SetMEStdata($"{mesCmd.Data}{Popmes_data}");                
            }
            else
            {
                Cmd.SetMEStdata(mesCmd.Data);
            }

            Logger.Info($"Script_HTTPMESCmd_Related Send: {mesCmd.Cmd}\n");
            if (Cmd.SEND("") != true)
            {
                return false;
            }
            
            Cmd.READ(ref end_data);
            strOutData = end_data;
            Logger.Info($"Script_HTTPMESCmd_Related Read END: {end_data}");
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

        public class MESCmd
        {
            public string Cmd { get; set; }
            public string Data { get; set; }
            public string CheckStr { get; set;}
        }
    }
}
