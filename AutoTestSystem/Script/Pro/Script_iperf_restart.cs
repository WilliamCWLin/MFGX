using AutoTestSystem.Base;
using AutoTestSystem.Equipment.DosBase;
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
    internal class Script_iperf_restart : Script_ControlDevice_Base
    {
        string strActItem = string.Empty;
        string strOutData = string.Empty;


        public override void Dispose()
        {
            //throw new NotImplementedException();
        }     

        public override bool PreProcess()
        {
            return true;
        }
        public override bool Process(ControlDeviceBase iperf, ref string output)
        {
            iperf.UnInit();
            iperf.Init("Start");
            return true;

        }
        public override bool PostProcess()
        {
            string result = CheckRule(strOutData, Spec);

            if (result == "PASS" || Spec == "")
                return true;
            else
                return false;

        }

    }

}
