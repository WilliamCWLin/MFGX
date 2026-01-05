using Newtonsoft.Json;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel;
using AutoTestSystem.Model;
using AutoTestSystem.DAL;
using static AutoTestSystem.BLL.Bd;


namespace AutoTestSystem.Script
{
    internal class Script_ATSuite_Pro : Script_Extra_Base
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
            string log = $"./{Path}/Log/Log_Current.txt";
            string process = "ATSuite.exe";
            string content = string.Empty;

            // Thread 1
            Task startProcess = new Task(() =>
            {
                string res = doscmd.SendCommand3($"cd {Path} && {process}");
            });

            // Thread 2
            Task killProcess = new Task(() =>
            {
                int retryTimes = 20;
                for (int i = 0; i < retryTimes; i++)
                {
                    Thread.Sleep(3000);
                    if (File.Exists(log))
                    {
                        content = File.ReadAllText(log);
                        if (content.Contains("**** P A S S ****") || content.Contains("**** F A I L ****"))
                        {
                            string res = doscmd.SendCommand3($"taskkill /F /IM {process}");
                            LogMessage($"[Task Manager] {res}");
                            break;
                        }
                    }
                }
            });

            startProcess.Start();
            killProcess.Start();
            Task.WhenAny(killProcess).Wait();

            var data = new Dictionary<string, object> { };
            if (content == string.Empty || content.Contains("[ERROR]"))
                data.Add("errorCode", "-1");
            else
                data.Add("errorCode", "0");
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
