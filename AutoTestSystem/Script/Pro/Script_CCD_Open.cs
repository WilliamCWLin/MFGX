using AutoTestSystem.Base;
using AutoTestSystem.Equipment.CCD;
using AutoTestSystem.Equipment.ControlDevice;
using Manufacture;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AutoTestSystem.BLL.Bd;

namespace AutoTestSystem.Script
{
    internal class Script_CCD_Open : Script_CCD_Base
    {
        [Category("Common Parameters"), Description("DelayTime")]
        public int DelayTime { get; set; } = 100;

        public override void Dispose()
        {
            //throw new NotImplementedException();
        }
        public override bool PreProcess()
        {
            return true;
        }

        public override bool Process(CCDBase CCD,ref string strOutData)
        {
            var outputdata = new Dictionary<string, object>();
            bool pass_fail = true;
            try
            {
                bool ret = CCD.Init("");
                if (ret)
                {                
                    outputdata.Add("CCD_Open", "Suceesed");
                    LogMessage($"CCD_Open Suceesed ", MessageLevel.Debug);
                }
                else
                {
                    outputdata.Add("CCD_Open", "Fail");
                    LogMessage($"CCD_Open Fail ", MessageLevel.Debug);
                    pass_fail = false;
                }                
            }
            catch (Exception ex)
            {
                outputdata.Add("CCD_Open", $"Error:{ex.Message}");
                LogMessage($"CCD_Open Error:{ex.Message}", MessageLevel.Error);
                pass_fail = false;
            }
            Sleep(DelayTime);
            // Convert the dictionary to a JSON string           
            strOutData = JsonConvert.SerializeObject(outputdata, Formatting.Indented);
            return pass_fail;
        }
        public override bool PostProcess()
        {
            return true;
        }     
    }
}
