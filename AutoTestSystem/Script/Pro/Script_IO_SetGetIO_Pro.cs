using AutoTestSystem.Base;
using AutoTestSystem.DUT;
using AutoTestSystem.Equipment.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using static AutoTestSystem.BLL.Bd;
namespace AutoTestSystem.Script
{
    internal class Script_IO_SetGetIO_Pro : ScriptIOBase
    {
        string strOutData = string.Empty;

        [Category("GetIO Parameters"), Description("自訂顯示名稱")]
        public int GetBit { get; set; } = 0;

        [Category("GetIO Parameters"), Description("自訂顯示名稱")]
        public int GetportNum { get; set; } = 0;

        [Category("GetIO Parameters"), Description("自訂顯示名稱")]
        public bool Check { get; set; } = true;

        [Category("GetIO Parameters"), Description("Timeout(ms)")]
        public int Timeout { get; set; } = 8000;

        [Category("SetIO Parameters"), Description("設定ONOFF")]
        public bool ON_OFF { get; set; }

        [Category("SetIO Parameters"), Description("自訂顯示名稱")]
        public int SetBit { get; set; }

        [Category("SetIO Parameters"), Description("自訂顯示名稱")]
        public int SetportNum { get; set; } = 0;

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
            string tmp_msg = string.Empty;
            var data = new Dictionary<string, object>();
            bool bSuccess = true;

            if (!SetIO(Device, ON_OFF,ref tmp_msg))
            {            
                data.Add("StepOne", tmp_msg);
                data.Add("errorCode", -1);
                strOutData = JsonConvert.SerializeObject(data, Formatting.Indented);
                output = strOutData;
                LogMessage(tmp_msg);
                return false;
            }
            data.Add("StepOne", tmp_msg);

            Thread.Sleep(10);

            if(!GetIO(Device, ref tmp_msg))
            {          
                data.Add("StepTwo", tmp_msg);
                data.Add("errorCode", -2);
                strOutData = JsonConvert.SerializeObject(data, Formatting.Indented);
                output = strOutData;
                LogMessage(tmp_msg);
                bSuccess = false;
            }
            data.Add("StepTwo", tmp_msg);

            if(!SetIO(Device, !ON_OFF, ref tmp_msg))
            {
                data.Add("errorCode", -3);
                data.Add("StepThree", tmp_msg);

                strOutData = JsonConvert.SerializeObject(data, Formatting.Indented);
                output = strOutData;
                LogMessage(tmp_msg);
                return false;
            }

            data.Add("StepThree", tmp_msg);
            data.Add("errorCode", 0);
            strOutData = JsonConvert.SerializeObject(data, Formatting.Indented);
            output = strOutData;
            LogMessage(strOutData);

            if (bSuccess)
                return true;
            else
                return false;
        }
        public override bool PostProcess()
        {
            string result = CheckRule(strOutData, Spec);

            if (result == "PASS")
                return true;
            else
                return false;
        }

        public bool SetIO(IOBase Device,bool ONOFF, ref string output)
        {
            strOutData = string.Empty;
            LogMessage($"Set IO Bit({SetBit}) = {ONOFF}");
            bool ret = Device.SETIO(SetportNum, SetBit, ONOFF);

            if (ret)
            {
                LogMessage($"Set IO Bit({SetBit}) = {ONOFF}. Success");
                output = $"Set IO Bit({SetBit}) = {ONOFF}. Success";
                return true;           
            }
            else
            {
                output = $"Device.SETIO() return {ret}";
                LogMessage($"Device.SETIO() Fail. return {ret}");
            }

            return ret;
        }

        public bool GetIO(IOBase Device, ref string output)
        {
            strOutData = string.Empty;
            int MAX_ELAPSE_MS = Timeout;
            const int INTERVAL = 20;
            bool InputSataus = false;
            bool ret = false;
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            LogMessage("Waiting...", MessageLevel.Debug);

            while (stopwatch.ElapsedMilliseconds < MAX_ELAPSE_MS)
            {
                ret = Device.GETIO(GetportNum, GetBit, ref InputSataus);
                if (ret)
                {
                    if (InputSataus == Check)
                    {
                        LogMessage($"Check IO Bit({GetBit}) = {Check} success");

                        output = $"Check IO Bit({GetBit}) = {Check} success";

                        return true;
                    }
                }
                else // GETIO 函數返回失敗
                {
                    output = $"Device.GETIO() return {ret}";
                    LogMessage($"Device.GETIO() return {ret}");
                    return false;
                }

                System.Threading.Thread.Sleep(INTERVAL);
            }

            LogMessage("GetIO Timeout...", MessageLevel.Error);

            // Timeout
            output = $"Check IO Bit({ GetBit}) = {Check} Fail. Timeout";

            return false;
        }
    }
}
