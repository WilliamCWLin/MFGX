using AutoTestSystem.Base;
using AutoTestSystem.Equipment.ControlDevice;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AutoTestSystem.BLL.Bd;

namespace AutoTestSystem.Script
{
    internal class Script_ControlDevice_Open : Script_ControlDevice_Base
    {

        string strActItem = string.Empty;
        string strParam = string.Empty;
        string strOutData = string.Empty;
        public override void Dispose()
        {
            //throw new NotImplementedException();
        }
        public override bool PreProcess()
        {


            return true;
        }
        public override bool Process(ControlDeviceBase ControlDevice,ref string output)
        {
            if (ControlDevice.Init(string.Empty))
            {
                LogMessage("Init sucess !!", MessageLevel.Info);
                return true;
            }
            LogMessage("Init fail !!", MessageLevel.Info);
            return false;
        }
        public override bool PostProcess()
        {
            //strDataout= ref strOutData;
            //string result = ProcessData(strOutData, strCheckSpec);

            return true;

        }
    }
}
