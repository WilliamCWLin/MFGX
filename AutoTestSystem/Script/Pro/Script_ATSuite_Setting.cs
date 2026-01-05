using Newtonsoft.Json;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.ComponentModel;
using static AutoTestSystem.BLL.Bd;
using AutoTestSystem.DAL;


namespace AutoTestSystem.Script
{
    internal class Script_ATSuite_Setting : Script_Extra_Base
    {
        string strOutData = string.Empty;

        [Category("Common Parameters"), Description("相對路徑，例如：Utility/EO5002_IQxel_BT")]
        public string Path { get; set; }

        DosCmd doscmd = new DosCmd();

        public override void Dispose()
        {
            //throw new NotImplementedException();
        }

        public override bool PreProcess()
        {
            if (Path == null || Path == string.Empty)
            {
                LogMessage("Path can not be null.", MessageLevel.Error);
                return false;
            }

            return true;
        }

        public override bool Process(ref string output)
        {
            string errorCode = "-1";
            string file_path = $"./{Path}/Setup/dut_setting.txt";

            // Read all lines from the file
            string[] lines = File.ReadAllLines(file_path);

            int index = 0;
            foreach (string line in lines)
            {
                if (line.Contains("DUT_COM_PORT"))
                {
                    string pattern = @"DUT_COM_PORT\s*=\s*(\d+)";
                    Match match = Regex.Match(line, pattern);
                    if (match.Success)
                    {
                        string caption = doscmd.SendCommand3($"wmic path Win32_PnPEntity where \"Caption like '%Auxiliary%'\" get Caption");
                        if (caption.Contains("Auxiliary") == true)
                        {
                            int header = caption.LastIndexOf("(") + 1;
                            int tail = caption.LastIndexOf(")");
                            string PortName = caption.Substring(header, tail - header);
                            PushMoreData("PortName", PortName);
                        }
                        
                        string DUT_COM_PORT = match.Groups[1].Value;
                        string NEW_COM_PORT = Regex.Split(PopMoreData("PortName"), "COM")[1];
                        if (DUT_COM_PORT.Equals(NEW_COM_PORT) != true)
                            lines[index] = line.Replace(DUT_COM_PORT, NEW_COM_PORT);
                        LogMessage($"[Setting LOG] {lines[index]}");
                        File.WriteAllLines(file_path, lines);
                        errorCode = "0";
                    }
                }
                index++;
            }

            var data = new Dictionary<string, object>
            {
                {"errorCode", errorCode},
            };
            output = JsonConvert.SerializeObject(data);
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
                LogMessage($"{result}", MessageLevel.Error);
                return false;
            }
        }

    }
}
