using AutoTestSystem.Base;
using AutoTestSystem.Equipment.ControlDevice;
using Manufacture;
using MvCamCtrl.NET;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static AutoTestSystem.BLL.Bd;

namespace AutoTestSystem.Script
{
    internal class Script_SSH_Command : Script_ControlDevice_Base
    {

        string strOutData = string.Empty;

        [Category("Common Parameters"), Description("Command")]
        public string Command { get; set; } = "";
        [Category("Common Parameters"), Description("Pattern")]
        public string Pattern { get; set; } = "";
        [Category("Common Parameters"), Description("DataKey")]
        public string DataKeyName { get; set; } = "Port";

        public override void Dispose()
        {
            //throw new NotImplementedException();
        }
        public override bool PreProcess()
        {
            strOutData = string.Empty;

            return true;
        }
        public override bool Process(ControlDeviceBase ControlDevice, ref string outputjsonStr)
        {
            bool ret = false;
            try
            {
                
                List<string> readInfos = new List<string>();
                int readCount = 1; // Number of times to read

                for (int i = 0; i < readCount; i++)
                {
                    string readinfo = "";
                    ret = ControlDevice.SEND(Command);
                    ret = ControlDevice.READ(ref readinfo);
                    readInfos.Add(readinfo);
                    Thread.Sleep(500);
                }

                var data = ParsePowerValues(readInfos, Pattern, DataKeyName);
                string jsonStr = JsonConvert.SerializeObject(data, Formatting.Indented);
                LogMessage(jsonStr, MessageLevel.Info);

                strOutData = jsonStr;
                outputjsonStr = strOutData;
            }
            catch (Exception ex)
            {
                LogMessage($"Exception: {ex.Message}", MessageLevel.Error);
            }

            return ret;
        }

        static Dictionary<string, string> ParsePowerValues(List<string> inputs, string pattern, string keyname)
        {
            Dictionary<string, List<double>> tempResults = new Dictionary<string, List<double>>();
            Dictionary<string, string> result = new Dictionary<string, string>();

            foreach (string input in inputs)
            {
                foreach (Match match in Regex.Matches(input, pattern))
                {
                    if (double.TryParse(match.Groups[3].Value, out double power))
                    {
                        string key = keyname + match.Groups[1].Value;
                        if (!tempResults.ContainsKey(key))
                        {
                            tempResults[key] = new List<double>();
                        }
                        tempResults[key].Add(power);
                    }
                }
            }

            foreach (var entry in tempResults)
            {
                double average = entry.Value.Average();
                result.Add(entry.Key, average.ToString());
            }

            return result;
        }
        public override bool PostProcess()
        {
            if (Spec != string.Empty && Spec != null)
            {
                string ret = string.Empty;              
                ret = CheckRule(strOutData, Spec);

                LogMessage($"CheckRule: {ret}", MessageLevel.Debug);
                if (ret == "PASS")
                    return true;
                else
                    return false;
            }
            return true;
        }

        static Dictionary<string, string> ParsePowerValues(string input,string pattern,string keyname)
        {
            List<double> powerValues = new List<double>();
            Dictionary<string, string> result = new Dictionary<string, string>();
            foreach (Match match in Regex.Matches(input, pattern))
            {              
                if (double.TryParse(match.Groups[3].Value, out double power))
                {
                    result.Add(keyname + match.Groups[1].Value, match.Groups[3].Value);
                }
            }

            return result;
        }
    }
}
