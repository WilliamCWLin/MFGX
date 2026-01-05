using AutoTestSystem.Base;
using AutoTestSystem.DUT;
using AutoTestSystem.Equipment.IO;
using AutoTestSystem.Model;
using Manufacture;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using static AutoTestSystem.Script.Script_Extra_Generic_Command;

namespace AutoTestSystem.Script
{
    internal class Script_IO_InstantAI_Continueous : ScriptIOBase
    {
        string strOutData = string.Empty;
        public enum Data_GetMode
        {
            Continueous,
        }

        [Category("Command"), Description("自訂顯示名稱")]
        public Data_GetMode P0_Mode { get; set; } = Data_GetMode.Continueous;

        [Category("Common Parameters"), Description("自訂顯示名稱")]
        public int Channel { get; set; }

        [Category("Continueous"), Description("Example->{\"loopCount\":\"500\",\"ch\":\"0\",\"interval_ms\":\"10\"}")]
        public string Params { get; set; }

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
            try
            {
                strOutData = string.Empty;
                bool ret = false;

                ret = Device.InstantAI(Params, ref strOutData);

                if (ret)
                    LogMessage($"Data:{strDataout}");
                else
                    LogMessage($"InstantAI Fail.{strOutData}",MessageLevel.Error);

                strDataout = strOutData;
                LogMessage($"Data:{strDataout}");

                return ret;
            }catch(Exception ex)
            {
                strDataout = $"{{\"Exception\":\"{ex.Message}}}\"";

                return false;
            }

        }

        public override bool PostProcess()
        {
            LogMessage($"Check Spec:{Spec}");

            string result = CheckRule(strOutData, Spec);
            
            if (result == "PASS" || Spec == "" || Spec == string.Empty)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
