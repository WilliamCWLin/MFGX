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

namespace AutoTestSystem.Script
{
    internal class Script_IO_InstantAI : ScriptIOBase
    {
        string strActItem = string.Empty;
        string strParam = string.Empty;
        string strOutData = string.Empty;

        [Category("Common Parameters"), Description("自訂顯示名稱")]
        public int Channel { get; set; }

        public override void Dispose()
        {
            //throw new NotImplementedException();
        }

        public override bool Process()
        {
            strOutData = string.Empty;
            bool ret = IODevice.InstantAI(Channel, ref strOutData);
            string result = CheckRule(strOutData, Spec);
                     
            if (result == "PASS" || Spec == "")
            {
                LogMessage(Spec, MessageLevel.Debug);

                return true;
            }           
            else
            {
                LogMessage(Spec, MessageLevel.Error);
                return false;
            }
                
        }
    }
}
