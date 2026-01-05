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
    internal class Script_IO_Related_GetIO : ScriptIOBase
    {
        int iGetBit = -1;
        string strCheck;
        int itimeout = -1;
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

                if (InputParamArr.Length < 3)
                {
                    Logger.Error("Parameter format error.");
                    return false;
                }
                else
                {
                    iGetBit = Int32.Parse(InputParamArr[0]);
                    strCheck = InputParamArr[1];
                    itimeout = Int32.Parse(InputParamArr[2]);
                }

                return true;
            }
            catch (Exception ex)
            {
                Logger.Info($"PreProcess Exception: {ex.Message}");
                return false;
            }

        }
        public override bool Process(IOBase Device)
        {
            strOutData = string.Empty;
            int MAX_ELAPSE_MS = itimeout; 
            const int INTERVAL = 20; 

            bool InputSataus = false;
            bool ret = false;
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            LogMessage("Waiting...",MessageLevel.Debug);
            while (stopwatch.ElapsedMilliseconds < MAX_ELAPSE_MS)
            {
                ret = Device.GETIO(0,iGetBit, ref InputSataus);
                if (ret)
                {
                    if (InputSataus.ToString().ToLower() == strCheck)
                    {
                        LogMessage($"Check IO Bit({iGetBit}) = {strCheck} success");
                        Logger.Debug($"Check IO Bit({iGetBit}) = {strCheck} success");
                        var data = new Dictionary<string, object>
                        {
                            { "errorCode", 0 },
                            { "InputStatus", InputSataus.ToString().ToLower() }
                        };
                        strOutData = JsonConvert.SerializeObject(data);
                        return true;
                    }
                }
                else // GETIO 函数返回失败时
                {
                    
                    var data = new Dictionary<string, object>
                    {
                        { "errorCode", -1 }
                    };
                    strOutData = JsonConvert.SerializeObject(data);
                    return false; // 返回 false 表示出现了错误
                }

                System.Threading.Thread.Sleep(INTERVAL);
            }
            LogMessage("Timeout...", MessageLevel.Error);
            // 如果运行到这里，说明已经达到了最大超时时间
            var timeoutData = new Dictionary<string, object>
            {
                { "errorCode", -2 }
            };
            strOutData = JsonConvert.SerializeObject(timeoutData);
            return true; // 返回 false 表示出现了错误
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
            if( PreProcess("", Input))
            {
                return Process(IODevice);
            }
            return false;



        }
    }
}
