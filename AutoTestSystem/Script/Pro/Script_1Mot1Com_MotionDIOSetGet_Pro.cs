using AutoTestSystem.Base;
using AutoTestSystem.DAL;
using AutoTestSystem.Equipment.ControlDevice;
using AutoTestSystem.Equipment.Motion;
using AutoTestSystem.Model;
using Manufacture;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Design;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static AutoTestSystem.Base.MotionBase;
using static AutoTestSystem.BLL.Bd;
using static AutoTestSystem.Equipment.Motion.Motion_DIOparam;

namespace AutoTestSystem.Script
{
    internal class Script_1Mot1Com_MotionDIOSetGet_Pro : Script_1Mot1ComBase
    {

        string strOutData = string.Empty;
        /*[Category("Torque Parameters"), Description("自訂顯示名稱")]
        public ushort Port_count { get; set; }*/

        [Category("SetIO Parameters"), Description("SetIO Bit")]
        public ushort SetBit { get; set; }
        [Category("SetIO Parameters"), Description("SetIO功能開啟")]
        public bool DO_Status_ONOFF { get; set; }
        [Category("GetIO Parameters"), Description("GetIO Bit")]
        public ushort GetBit { get; set; }

        [Category("GetIO Parameters"), Description("自訂顯示名稱")]
        public bool Check { get; set; } = true;

        [Category("GetIO Parameters"), Description("Timeout(ms)")]
        public int Timeout { get; set; } = 8000;

        string jsonStr = string.Empty;


        public override void Dispose()
        {
            throw new NotImplementedException();
        }

        public enum DIOMode
        {
            Single_Transmit,
            Multiple_Transmit,
            Motion_Status
        }

        public override bool PreProcess()
        {
            strOutData = string.Empty;
            return true;
        }

        public override bool Process(ControlDeviceBase ComportDev, MotionBase MotionDev, ref string outputjsonStr)
        {
            string tmp_msg = string.Empty;
            var data = new Dictionary<string, object>();
            bool bSuccess = true;

            if (!SetIO(MotionDev, DO_Status_ONOFF, ref tmp_msg))
            {
                data.Add("StepOne", tmp_msg);
                data.Add("errorCode", -1);
                strOutData = JsonConvert.SerializeObject(data, Formatting.Indented);
                outputjsonStr = strOutData;
                LogMessage(tmp_msg);
                return false;
            }
            data.Add("StepOne", tmp_msg);

            Thread.Sleep(10);

            if (!GetIO(MotionDev, ref tmp_msg))
            {
                data.Add("StepTwo", tmp_msg);
                data.Add("errorCode", -2);
                strOutData = JsonConvert.SerializeObject(data, Formatting.Indented);
                outputjsonStr = strOutData;
                LogMessage(tmp_msg);
                bSuccess = false;
            }
            if(bSuccess)
                data.Add("StepTwo", tmp_msg);

            if (!SetIO(MotionDev, !DO_Status_ONOFF, ref tmp_msg))
            {
                data.Add("errorCode", -3);
                data.Add("StepThree", tmp_msg);

                strOutData = JsonConvert.SerializeObject(data, Formatting.Indented);
                outputjsonStr = strOutData;
                LogMessage(tmp_msg);
                return false;
            }

            data.Add("StepThree", tmp_msg);
            if (bSuccess)
                data.Add("errorCode", 0);
            strOutData = JsonConvert.SerializeObject(data, Formatting.Indented);
            outputjsonStr = strOutData;
            LogMessage(strOutData);

            if (bSuccess)
                return true;
            else
                return false;
        }

        public override bool PostProcess()
        {
            if (!string.IsNullOrEmpty(Spec))
            {
                string ret = string.Empty;
                ret = CheckRule(jsonStr, Spec);
                LogMessage($"CheckRule: {ret}", MessageLevel.Info);
                if (ret == "PASS")
                    return true;
                else
                    return false;
            }
            return true;
        }

        public bool SetIO(MotionBase Device, bool ONOFF, ref string output)
        {
            int port_DIOstatus = 0;
            int myInt = ONOFF ? 1 : 0;
            //strOutData = string.Empty;
            LogMessage($"Set IO Bit({SetBit}) = {myInt}");
            bool ret = Device.SetGet_IO(SetBit, myInt, ref port_DIOstatus);

            if (ret)
            {
                LogMessage($"Set IO Bit({SetBit}) = {myInt}. Success");
                output = $"Set IO Bit({SetBit}) = {myInt}. Success";
                return true;
            }
            else
            {
                output = $"Device.SETIO() return {ret}";
                LogMessage($"Device.SETIO() Fail. return {ret}");
            }

            return ret;
        }

        public bool GetIO(MotionBase Device, ref string output)
        {
            //strOutData = string.Empty;
            int port_DIstatus = 0;
            int MAX_ELAPSE_MS = Timeout;
            const int INTERVAL = 20;
            bool ret = false;
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            LogMessage("Waiting...", MessageLevel.Debug);
            int INTCheck = Check ? 1 : 0;
            while (stopwatch.ElapsedMilliseconds < MAX_ELAPSE_MS)
            {
                ret = Device.Get_IO_Status(ref port_DIstatus, GetBit);
                if (ret)
                {
                    if (port_DIstatus == INTCheck)
                    {
                        LogMessage($"Check IO Bit({GetBit}) = {INTCheck} success");

                        output = $"Check IO Bit({GetBit}) = {INTCheck} success";

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
            output = $"Check IO Bit({ GetBit}) = {INTCheck} Fail. Timeout";

            return false;
        }
    
    public override bool PreProcess(string strParamInput)
        {
            throw new NotImplementedException();
        }

        public override bool Process(ControlDeviceBase comport, MotionBase MotionDev)
        {
            throw new NotImplementedException();
        }

        public override bool PostProcess(string TestKeyword, string strCheckSpec, ref string strDataout)
        {
            throw new NotImplementedException();
        }
    }
}
