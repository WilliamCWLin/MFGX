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
    internal class Script_CCD_SetParam_Pro : Script_CCD_Base
    {

        string strOutData = string.Empty;
        string Status = string.Empty;

        [Category("Common Parameters"), Description("曝光時間設定")]
        public int ExposureTime { get; set; }      

        public override void Dispose()
        {
            //throw new NotImplementedException();
        }
        public override bool PreProcess()
        {
            //strOutData = string.Empty;
            Status = "Waiting";
            return true;
        }

        public override bool Process(CCDBase CCD,ref string strOutData)
        {
            LogMessage($"{Description}:Set_Exposure({ExposureTime})", MessageLevel.Debug);
            if (!CCD.Set_Exposure(ExposureTime))
            {
                Status = "Set_Exposure_error";
                var NGOutData = new Dictionary<string, object>
                {
                    {"ExposureTime",ExposureTime},
                    {"CCD_SetParam_Status",Status}
                };
                strOutData = JsonConvert.SerializeObject(NGOutData, Formatting.Indented);
                LogMessage($"Script_CCD_SetParam:  {Status}");
                return false;
            }
            Status = "OK";
            var OutData = new Dictionary<string, object>
            {
                {"ExposureTime",ExposureTime},
                {"CCD_SetParam_Status",Status}
            };
            strOutData = JsonConvert.SerializeObject(OutData, Formatting.Indented);
            LogMessage($"Script_CCD_SetParam:  {strOutData}",MessageLevel.Debug);
            return true;
        }
        public override bool PostProcess()
        {
            return true;
        }     
    }
}
