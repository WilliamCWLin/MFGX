using AutoTestSystem.Base;
using AutoTestSystem.DUT;
using AutoTestSystem.Equipment.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using static AutoTestSystem.BLL.Bd;
namespace AutoTestSystem.Script
{
    internal class Script_IO_Related_SetIO : ScriptIOBase
    {
        string strActItem = string.Empty;
        int strSetBit = -1;
        bool bOutput = false;
        string[] InputParamArr = null;
        string strOutData = string.Empty;

        [Category("Common Parameters"), Description("自訂顯示名稱")]
        public string Input { get; set; }
        public override void Dispose()
        {
            //throw new NotImplementedException();
        }
        public override bool PreProcess(string Actionitem, string strParamInput)
        {
            try
            {
                InputParamArr = strParamInput.Split(',');

                if (InputParamArr.Length < 2)
                {
                    Logger.Error($"Parameter({strParamInput}) format error.");
                    return false;
                }
                else
                {
                    strSetBit = Int32.Parse(InputParamArr[0]);
                    bOutput = bool.Parse(InputParamArr[1]);
                    //Logger.Info($"SetBit:{strSetBit},Output:{bOutput}");
                }
                    
                return true;
            }
            catch (Exception ex)
            {
                Logger.Info($"Script_IO_Related_SetIO PreProcess Exception: {ex.Message}");
                return false;
            }
        }
        public override bool Process(IOBase Device)
        {
            strOutData = string.Empty;
            LogMessage($"Set IO Bit({strSetBit}) = {bOutput}");
            bool ret = Device.SETIO(0,strSetBit, bOutput);
            if(ret)
            {
                var data = new Dictionary<string, object>
                    {
                        { "errorCode", 0 }
                    };
                strOutData = JsonConvert.SerializeObject(data);
                LogMessage($"Set IO Bit Success. Data({strOutData}) ");
            }
            else
            {
                var data = new Dictionary<string, object>
                    {
                        { "errorCode", -1 }
                    };
                strOutData = JsonConvert.SerializeObject(data);
            }

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

        public override bool Process()
        {
            if (PreProcess("", Input))
            {
                return Process(IODevice);
            }
            return false;



        }
    }
}
