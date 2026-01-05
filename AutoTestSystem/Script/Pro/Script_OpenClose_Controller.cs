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
    internal class Script_OpenClose_Controller : Script_ControlDevice_Base
    {
        private Dictionary<string, string> output_data = new Dictionary<string, string>();
        int errorCode;
        string strActItem = string.Empty;
        string strParam = string.Empty;
        string strOutData = string.Empty;
        string OutDataInfo = string.Empty;
        string ack_data = string.Empty;
        string end_data = string.Empty;

        public enum CMD_ACTION
        {
            open,
            close
        }

        [Category("Common Parameters"), Description("Timeout")]
        public int Timeout { get; set; } = 200;
        [Category("Common Parameters"), Description("OpenClose")]
        public CMD_ACTION OpenClose { get; set; } = CMD_ACTION.open;


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
            string strOutput = "";
            try
            {

                // Opne Close Function
                ControlDevice.SetTimeout(Timeout);
                switch (OpenClose)
                {
                    case CMD_ACTION.open:
                        ControlDevice.StartAction("open", "", ref strOutput);
                        break;
                    case CMD_ACTION.close:
                        ControlDevice.StartAction("close", "", ref strOutput);
                        break;
                }
            }
            catch (Exception ex)
            {
                outputdata.Add("ArduinoOpen", $"Error:{ex.Message}");
                LogMessage($"ArduinoOpen Error:{ex.Message}", MessageLevel.Error);
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
