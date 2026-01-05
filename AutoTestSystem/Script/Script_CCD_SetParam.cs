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
    internal class Script_CCD_SetParam : Script_CCD_Base
    {

        string strActItem = string.Empty;
        string strParam = string.Empty;
        string strOutData = string.Empty;

        CCD ccd_param = null;

        [Category("Common Parameters"), Description("自訂顯示名稱"), Editor(typeof(JsonEditor), typeof(UITypeEditor))]
        public string Params { get; set; }

        public override void Dispose()
        {
            //throw new NotImplementedException();
        }
        public override bool PreProcess(string ActionItem, string Paraminput)
        {

            strActItem = ActionItem;
            strParam = Paraminput;

            ccd_param = JsonConvert.DeserializeObject<CCD>(strParam);

            return true;
        }

        public override bool Process()
        {
            ccd_param = JsonConvert.DeserializeObject<CCD>(Params);
            return Process(CCDDevice);
        }

        public override bool Process(CCDBase CCD)
        {
            LogMessage($"{Description}:Set_Exposure({ccd_param.ExposureTime})", MessageLevel.Debug);
            if (!CCD.Set_Exposure(ccd_param.ExposureTime))
            {
                strOutData = "Set Exposure error";
                LogMessage($"Script_CCD_SetParam:  {strOutData}");
                return false;
            }
            strOutData = "OK";
            LogMessage($"Script_CCD_SetParam:  {strOutData}",MessageLevel.Debug);
            return true;
        }
        public override bool PostProcess(string strCheckSpec, ref string strDataout)
        {
            //strDataout= ref strOutData;
            //string result = ProcessData(strOutData, strCheckSpec);

            strDataout =  strOutData;
            return true;

        }     

        public class CCD
        {
           
            public int ExposureTime { get; set; }
            

        }

    }
}
