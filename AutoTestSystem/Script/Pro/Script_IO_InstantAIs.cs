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

namespace AutoTestSystem.Script
{
    internal class Script_IO_InstantAIs : ScriptIOBase
    {
        string strOutData = string.Empty;

        [Category("Common Parameters"), Description("自訂顯示名稱")]
        public string Channel { get; set; }

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
                int ch = int.Parse(ReplaceProp(Channel));
                bool ret = Device.InstantAI(ch, ref strOutData);

                if(ret)
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
