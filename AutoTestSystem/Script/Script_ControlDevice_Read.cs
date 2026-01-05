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
    internal class Script_ControlDevice_Read : Script_ControlDevice_Base
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
            int timeout=1500;
            if (string.IsNullOrEmpty(strParam))
                ControlDevice.SetTimeout(timeout);
            else
            {
                timeout = int.Parse(strParam.Substring(strParam.IndexOf(":") + 1));
                ControlDevice.SetTimeout(timeout);
            }

            bool pass_fail = ControlDevice.READ(ref strOutData);
            Logger.Info($"Script_ControlDevice_Read:  {strOutData}");
            return pass_fail;
        }
        public override bool PostProcess(string strCheckSpec, ref string strDataout)
        {
            //strDataout= ref strOutData;
            //string result = ProcessData(strOutData, strCheckSpec);

            strDataout =  strOutData;
            return true;

        }
    }
}
