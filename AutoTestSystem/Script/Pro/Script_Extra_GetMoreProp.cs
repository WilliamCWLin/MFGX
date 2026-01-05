
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
    internal class Script_Extra_GetMoreProp : Script_Extra_Base
    {
        [Category("Common Parameters"), Description("Directly use Keyname, without adding %%")]
        public string Key { get; set; } = "";

        string jsonresult = "";


        public override void Dispose()
        {
            //throw new NotImplementedException();
        }
        public override bool PreProcess()
        {
            jsonresult = string.Empty;
            return true;
        }
        public override bool Process(ref string strOutData)
        {

            string ret = PopMoreData(Key);
            //MessageBox.Show(ret);

            var dict = new Dictionary<string, string>
            {
                {Key, ret }
            };
            jsonresult = JsonConvert.SerializeObject(dict, Formatting.Indented);
            strOutData = jsonresult;
            LogMessage(jsonresult);
            //string jsonresult = $"{Key}: {ret}";
            //if (ret)
            //{
            //    JObject json = new JObject();
            //    json[Key] = ReplaceValue;
            //    strOutData = json.ToString();
            //}             
            //else
            //{
            //    JObject json = new JObject();
            //    json["Err"] = $"Set {Key} = {ReplaceValue} Fail.";
            //    strOutData = json.ToString();

            //}

            return true;
        }
        public override bool PostProcess()
        {
            string result = CheckRule(jsonresult, Spec);
            if (result == "PASS" || Spec == "")
            {
                return true;
            }
            else
            {
                LogMessage($"{result}", MessageLevel.Warn);
                return false;
            }

        }


    }
}
