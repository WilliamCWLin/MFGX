using AutoTestSystem.Base;
using AutoTestSystem.DUT;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Diagnostics;
using static AutoTestSystem.BLL.Bd;
using System.ComponentModel;
using System.Drawing.Design;
using Manufacture;

namespace AutoTestSystem.Script
{
    internal class Script_DUT_Do_Action : ScriptDUTBase
    {
        string strOutData = string.Empty;

        [Category("Command"), Description("")]
        public string ActionCommand { get; set; }

        public override void Dispose()
        {
            //throw new NotImplementedException();
        }
        
        public override bool PreProcess()
        {


            return true;
        }
        
        public override bool Process(DUT_BASE DUTDevice,ref string output)
        {
            string xx = string.Empty;
            DUTDevice.StartAction(ActionCommand, "",ref  xx);
            strOutData = output;

            return true;
        }

        public override bool PostProcess()
        {
            string result = CheckRule(strOutData, Spec);

            if (result == "PASS" || Spec == "")
            {
                return true;
            }             
            else
            {
                LogMessage($"{result}",MessageLevel.Error);
                return false;
            }
    
        }    
    }
}
