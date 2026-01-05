using AutoTestSystem.Base;
using AutoTestSystem.DUT;
using AutoTestSystem.Equipment.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using static AutoTestSystem.BLL.Bd;
namespace AutoTestSystem.Script
{
    internal class Script_IO_Related_GetIO_Pro : ScriptIOBase
    {
        string[] InputParamArr = null;
        string strOutData = string.Empty;

        [Category("Common Parameters"), Description("自訂顯示名稱")]
        public int GetBit { get; set; } = 0;

        [Category("GetIO Parameters"), Description("自訂顯示名稱")]
        public int portNum { get; set; } = 0;

        [Category("Common Parameters"), Description("自訂顯示名稱")]
        public bool Check { get; set; } = true;

        [Category("Common Parameters"), Description("自訂顯示名稱")]
        public int Timeout { get; set; } = 2000;
        public override void Dispose()
        {
            //throw new NotImplementedException();
        }
        public override bool PreProcess()
        { 
            strOutData = string.Empty;

            return true;
        }
        public override bool Process(IOBase Device,ref string output)
        {
            strOutData = string.Empty;
            int MAX_ELAPSE_MS = Timeout; 
            const int INTERVAL = 20; 
            bool InputSataus = false;
            bool ret = false;
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            LogMessage("Waiting...",MessageLevel.Debug);

            while (stopwatch.ElapsedMilliseconds < MAX_ELAPSE_MS)
            {
                ret = Device.GETIO(portNum,GetBit, ref InputSataus);
                if (ret)
                {
                    if (InputSataus == Check)
                    {
                        LogMessage($"Check IO Bit({GetBit}) = {Check} success");

                        var data = new Dictionary<string, object>
                        {
                            { "errorCode", 0 },
                            { "Message", $"Check IO Bit({GetBit}) = {Check} success" }
                        };

                        strOutData = JsonConvert.SerializeObject(data);

                        output = strOutData;
                        return true;
                    }
                }
                else // GETIO 函數返回失敗
                {
                    
                    var data = new Dictionary<string, object>
                    {
                        { "errorCode", -1 },
                        { "Message", $"Device.GETIO() return {ret}" }
                    };
                    strOutData = JsonConvert.SerializeObject(data);
                    output = strOutData;

                    LogMessage($"Device.GETIO() return {ret}");
                    return false; 
                }

                System.Threading.Thread.Sleep(INTERVAL);
            }

            LogMessage("GetIO Timeout...", MessageLevel.Error);

            // Timeout
            var timeoutData = new Dictionary<string, object>
            {
                { "errorCode", -2 }
            };

            strOutData = JsonConvert.SerializeObject(timeoutData);
            output = strOutData;

            return false; 
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
