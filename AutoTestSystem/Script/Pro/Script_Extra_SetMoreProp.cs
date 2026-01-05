
using AutoTestSystem.Model;
using Manufacture;
using NAudio.Wave;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static AutoTestSystem.BLL.Bd;

namespace AutoTestSystem.Script
{
    internal class Script_Extra_SetMoreProp : Script_Extra_Base
    {
        [Category("Common Parameters"), Description("Key")]
        public string Key { get; set; } = "";
        [Category("Common Parameters"), Description("Value")]
        public string Value { get; set; } = "";



        public override void Dispose()
        {
            //throw new NotImplementedException();
        }
        public override bool PreProcess()
        {

            return true;
        }
        public override bool Process(ref string strOutData)
        {
            string ReplaceValue = ReplaceProp(Value);
            bool ret = PushMoreData(Prefix+Key, ReplaceValue);
            
            if (ret)
            {
                JObject json = new JObject();
                json[Key] = ReplaceValue;
                strOutData = json.ToString();
            }             
            else
            {
                JObject json = new JObject();
                json["Err"] = $"Set {Key} = {ReplaceValue} Fail.";
                strOutData = json.ToString();
            }

            LogMessage(strOutData);

            return true;
        }
        public override bool PostProcess()
        {
            return true;

        }


    }
}
