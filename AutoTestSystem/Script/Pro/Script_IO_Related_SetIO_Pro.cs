using AutoTestSystem.Base;
using AutoTestSystem.DUT;
using AutoTestSystem.Equipment.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using static AutoTestSystem.BLL.Bd;
namespace AutoTestSystem.Script
{
    internal class Script_IO_Related_SetIO_Pro : ScriptIOBase
    {
        string strOutData = string.Empty;

        [Category("Common Parameters"), Description("設定ONOFF")]
        public bool ON_OFF { get; set; }

        [Category("Common Parameters"), Description("自訂顯示名稱")]
        public int SetBit { get; set; }

        [Category("Common Parameters"), Description("自訂顯示名稱")]
        public int portNum { get; set; } = 0;

        [Category("Support Replace Param(s)"), Description("支援取代%%可輸入字串")]
        public string strportNum { get; set; } = string.Empty;

        [Category("Support Replace Param(s)"), Description("支援取代%%可輸入字串")]
        public string strSetBit { get; set; } = string.Empty;
        public override void Dispose()
        {
            //throw new NotImplementedException();
        }
        public override bool PreProcess()
        {
            strOutData = string.Empty;
            return true;

        }
        public override bool Process(IOBase Device, ref string strDataout)
        {
            strOutData = string.Empty;
            bool ret = false;
            if (string.IsNullOrEmpty(strSetBit))
            {
                LogMessage($"Set IO Bit({SetBit}) = {ON_OFF}");
                ret = Device.SETIO(portNum, SetBit, ON_OFF);
            }
            else
            {
                int bitnum;
                int portnum;
                if (!int.TryParse(ReplaceProp(strSetBit), out bitnum))
                {
                    string msg = ReplaceProp(strSetBit);
                    //LogMessage($"Replace  {strSetBit} to {(msg=="")?msg:"NULL"} Fail or INT.TryParse fail");
                    var data = new Dictionary<string, object>
                    {
                        { "errorCode", -2 },
                         { "Message", $"Input Params Fail [{ReplaceProp(strSetBit)}]" }
                    };
                    strOutData = JsonConvert.SerializeObject(data,Formatting.Indented);
                    strDataout = strOutData;
                    return false;
                }

                if (!int.TryParse(ReplaceProp(strportNum), out portnum))
                {
                    LogMessage($"{strportNum} int.TryParse fail");
                    var data = new Dictionary<string, object>
                    {
                        { "errorCode", -3 },
                         { "Message", $"Input Params Fail [{ReplaceProp(strportNum)}]" }
                    };
                    strOutData = JsonConvert.SerializeObject(data,Formatting.Indented);
                    strDataout = strOutData;
                    return false;
                }

                LogMessage($"Set IO Bit[{portnum}][{bitnum}] = {ON_OFF}");
                ret = Device.SETIO(portnum, bitnum, ON_OFF);
            }

            if(ret)
            {
                var data = new Dictionary<string, object>
                    {
                        { "errorCode", 0 }
                    };
                strOutData = JsonConvert.SerializeObject(data, Formatting.Indented);
                LogMessage($"Set IO Bit Success. Data({strOutData}) ");
            }
            else
            {
                var data = new Dictionary<string, object>
                    {
                        { "errorCode", -1 },
                         { "Message", $"Device.SETIO() return {ret}" }
                    };
                strOutData = JsonConvert.SerializeObject(data, Formatting.Indented);
                LogMessage($"Device.SETIO() Fail. return {ret}");
            }

            strDataout = strOutData;

            return ret;
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
}
