using AutoTestSystem.Base;
using AutoTestSystem.Equipment.ControlDevice;
using Manufacture;
using MvCamCtrl.NET;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Renci.SshNet.Security;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows.Forms;
using static AutoTestSystem.BLL.Bd;

namespace AutoTestSystem.Script
{
    internal class Script_ControlDevice_ArduinoClose : Script_ControlDevice_Base
    {
        private Dictionary<string, string> output_data = new Dictionary<string, string>();
        int errorCode;
        string strActItem = string.Empty;
        string strParam = string.Empty;
        string strOutData = string.Empty;
        string OutDataInfo = string.Empty;
        string ack_data = string.Empty;
        string end_data = string.Empty;
       
        [Category("Common Parameters"), Description("Timeout")]
        public int Timeout { get; set; } = 1000;
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
        public override bool Process(ControlDeviceBase ControlDevice, ref string outputjsonStr)
        {
            var outputdata = new Dictionary<string, object>();
            bool pass_fail = true;
            try
            {
                ControlDevice.SetTimeout(Timeout);
                bool ret = ControlDevice.UnInit();            
                if (ret)
                {
                    outputdata.Add("ArduinoClose", "Suceesed");
                    LogMessage($"ArduinoClose Suceesed ",MessageLevel.Debug);                  
                }
                else
                {
                    outputdata.Add("ArduinoClose", "Fail");
                    LogMessage($"ArduinoClose Fail ", MessageLevel.Debug);
                    pass_fail =  false;
                }
            }catch(Exception ex)
            {
                outputdata.Add("ArduinoClose", $"Error:{ex.Message}");
                LogMessage($"ArduinoClose Error:{ex.Message}", MessageLevel.Error);
                pass_fail = false;
            }
            // Convert the dictionary to a JSON string           
            outputjsonStr = JsonConvert.SerializeObject(outputdata, Formatting.Indented);
            return pass_fail;
        }
        public override bool PostProcess()
        {
            return true;
        }     
    }
}
