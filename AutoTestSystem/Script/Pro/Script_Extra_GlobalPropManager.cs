using AutoTestSystem.Model;
using Manufacture;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Windows.Forms;

namespace AutoTestSystem.Script
{
    internal class Script_Extra_GlobalPropManager : Script_Extra_Base
    {
        [JsonIgnore]
        [Browsable(false)]
        public static Dictionary<string, string> GlobalProperties { get; private set; } = new Dictionary<string, string>();

        public enum OperationMode
        {
            Get,
            Set
        }

        [Category("Common Parameters"), Description("Directly use Keyname, without adding %%")]
        public string Key { get; set; } = "";

        [Category("Common Mode Select"), Description("Set or Get the value")]
        public OperationMode Mode { get; set; } = OperationMode.Get;

        [Category("Common Parameters"), Description("Value to set (only used in Set mode)")]
        public string Value { get; set; } = "";

        string jsonresult = "";

        public override void Dispose()
        {
        }

        public override bool PreProcess()
        {
            jsonresult = string.Empty;
            return true;
        }

        public override bool Process(ref string strOutData)
        {
            string ret = "";

            if (Mode == OperationMode.Set)
            {
                Script_Extra_GlobalPropManager.GlobalProperties[Key] = ReplaceProp(Value);
                ret = $"Set {Key} = {ReplaceProp(Value)}";
            }
            else if (Mode == OperationMode.Get)
            {
                ret = Script_Extra_GlobalPropManager.GlobalProperties.ContainsKey(Key) ? GlobalProperties[Key] : string.Empty;
            }

            var dict = new Dictionary<string, string>
            {
                { Key, ret }
            };

            jsonresult = JsonConvert.SerializeObject(dict, Formatting.Indented);
            strOutData = jsonresult;
            LogMessage(jsonresult);

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
                LogMessage($"{result}", MessageLevel.Error);
                return false;
            }
        }


        public static void ClearSyncProp()
        {
            GlobalProperties.Clear();
        }

    }
}
