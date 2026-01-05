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
    internal class Script_DUT_UART_Half_Control : ScriptDUTBase
    {
        string strOutData = string.Empty;

        [Category("Comport Mode"), Description("on/off")]
        public string Comport_mode { get; set; } = "";


        public override bool PreProcess()
        {
            strOutData = string.Empty;
            return true;
        }

        public override bool Process(DUT_BASE DUTDevice, ref string output)
        {
            bool status_check = false;
            string output_x = "";
            if (Comport_mode == "on")
            {
                string Status_msg = string.Empty;
                status_check = DUTDevice.Status(ref Status_msg);
                LogMessage(Status_msg);
                if (status_check != true)
                    status_check = DUTDevice.Init(string.Empty);

                //string TOF_data = JsonConvert.SerializeObject(JObject.Parse(strOutData)["data"]);
                if (status_check)
                {
                    output_x = "{\"Status\":\"pass\"}";
                }
                else
                {
                    output_x = "{\"Status\":\"fail\"}";
                }

                LogMessage($"Read END:  {status_check}\n");
            }
            else if (Comport_mode == "off")
            {
                status_check = DUTDevice.UnInit();

                if (status_check)
                {
                    output_x = "{\"Status\":\"pass\"}";
                }
                else
                {
                    output_x = "{\"Status\":\"fail\"}";
                }

            }

            LogMessage($"Read END:  {output}");

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
