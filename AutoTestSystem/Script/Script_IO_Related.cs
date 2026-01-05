using AutoTestSystem.Base;
using AutoTestSystem.DUT;
using AutoTestSystem.Equipment.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace AutoTestSystem.Script
{
    internal class Script_IO_Related : ScriptIOBase
    {
        string strActItem = string.Empty;
        string strParam = string.Empty;
        string strOutData = string.Empty;
        public override void Dispose()
        {
            //throw new NotImplementedException();
        }
        public override bool PreProcess(string Actionitem, string strParamInput)
        {
            strActItem = Actionitem;
            strParam = strParamInput;
            return true;
        }
        public override bool Process(IOBase Device)
        {
            double volt1 = -1;
            bool ret = Device.InstantAI(strParam, ref strOutData);

            return ret;
        }
        public override bool PostProcess(string strCheckSpec, ref string strDataout)
        {
            //
            //標準化輸出資料JSON
            //處理
            //DUTDevice.StartAction("Open");
            string result = CheckRule(strOutData, strCheckSpec);
            strDataout = strOutData;

            if (result == "PASS" || strCheckSpec == "")
                return true;
            else
                return false;

        }   
    }
}
