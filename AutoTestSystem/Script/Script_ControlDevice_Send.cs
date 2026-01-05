using AutoTestSystem.Base;
using AutoTestSystem.Equipment.ControlDevice;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTestSystem.Script
{
    internal class Script_ControlDevice_Send :Script_ControlDevice_Base
    {

        string strActItem = string.Empty;
        string strParam = string.Empty;
        string strOutData = string.Empty;
        public override void Dispose()
        {
            //throw new NotImplementedException();
        }
        public override bool PreProcess(string ActionItem, string Paraminput)
        {

            strActItem = ActionItem;
            strParam = Paraminput;

            return true;
        }
        public override bool Process(ControlDeviceBase ControlDevice)
        {
            
            return ControlDevice.SEND(strParam+"\r\n");
        }
        public override bool PostProcess(string strCheckSpec, ref string strDataout)
        {
            //string result = ProcessData(strOutData, strCheckSpec);

            return true;

        }
    }
}
