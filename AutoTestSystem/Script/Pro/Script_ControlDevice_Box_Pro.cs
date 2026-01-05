using AutoTestSystem.Base;
using AutoTestSystem.Equipment.ControlDevice;
using Manufacture;
using MvCamCtrl.NET;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static AutoTestSystem.BLL.Bd;

namespace AutoTestSystem.Script
{
    internal class Script_ControlDevice_Box_Pro : Script_ControlDevice_Base
    {

        string strActItem = string.Empty;
        string strParam = string.Empty;
        string strOutData = string.Empty;
        string Status = string.Empty;
        string jsonStr = string.Empty;
        int errorCode;
        //BoxPropterty BoxProp = null;

        [Category("Common Parameters"), Description("Arduino Mode")]
        public int Mode { get; set; }
        [Category("Common Parameters"), Description("Send Dut Command"), Editor(typeof(CommandEditor), typeof(UITypeEditor))]
        public string Send_Command { get; set; } = "";
        [Category("Common Parameters"), Description("CheckContentStr")]
        public string StrContentCheck { get; set; } = "";
        [Category("Common Parameters"), Description("Timeout")]
        public int Timeout { get; set; } = 5000;
        [Category("Common Parameters"), Description("DelayTime")]
        public int DelayTime { get; set; } = 500;

        public override void Dispose()
        {
            //throw new NotImplementedException();
        }
        public override bool PreProcess()
        {
            strOutData = string.Empty;
            errorCode = -99;
            Status = "Waiting";
            jsonStr = string.Empty;
            return true;
        }
        public override bool Process(ControlDeviceBase ControlDevice,ref string outputjsonStr)
        {            
            bool pass_fail = true;
            var outputdata = new Dictionary<string, object>();
            outputdata.Add("ArduinoMode",Mode);
            outputdata.Add("SendCommand", Send_Command);
            outputdata.Add("Timeout", Timeout);

            if (Mode == 0)
            {
                try
                {
                    ControlDevice.SetTimeout(Timeout);
                    ControlDevice.SEND(Send_Command);

                    Sleep(DelayTime);
                    LogMessage($"{Description} Send:  {Send_Command}. WaitTime: {DelayTime}");
                    //Logger.Debug($"{Description} Send:  {Send_Command}. WaitTime: {DelayTime}");
                    
                    ControlDevice.READ(ref strOutData);
                    outputdata.Add("CMDStatus", strOutData);
                    LogMessage($"{Description} Read Test Value:  {strOutData}");
                    //Logger.Debug($"{Description} Read Test Value:  {strOutData}");
                    //Sleep(BoxProp.DelayTime);
                }
                catch (Exception ex)
                {
                    Logger.Warn(ex.Message + "Please Check COM Port Correct or NOT");
                    outputdata.Add("Exception", $"{ ex.Message}");
                    pass_fail = false;
                }
            }
            else if (Mode == 1)
            {
                try
                {
                    if (!Send_Command.EndsWith(Environment.NewLine))
                    {
                        // 在文字後面加上換行符
                        Send_Command += Environment.NewLine;
                    }
                        
                    ControlDevice.SetTimeout(Timeout);
                    ControlDevice.SEND(Send_Command);
                    LogMessage($"{Description} Send:  {Send_Command}");
                    //Logger.Debug($"{Description} Send:  {Send_Command}");


                    ControlDevice.READ(ref strOutData);
                    outputdata.Add("CMDStatus", strOutData);
                    LogMessage($"{Description} Read Test Value:  {strOutData}");
                    //Logger.Debug($"{Description} Read Test Value:  {strOutData}");

                    Sleep(DelayTime);
                }
                catch (Exception ex)
                {
                    Logger.Warn(ex.Message + "Please Check COM Port Correct or NOT");
                    outputdata.Add("Exception", $"{ ex.Message}");
                    pass_fail = false;
                }             
            }
            if (Mode == 2)
            {
                try
                {
                    ControlDevice.SetTimeout(Timeout);
                    ControlDevice.SEND(Send_Command);

                    Sleep(DelayTime);
                    LogMessage($"Send:  {Send_Command}. WaitTime: {DelayTime}");
                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();

                    string output = string.Empty;
                    bool isTimeout = true;
                    while (stopwatch.ElapsedMilliseconds < Timeout)
                    {
                        try
                        {
                            string message = string.Empty;

                            ControlDevice.READ(ref message);

                            strOutData += $"{message}";
                            if (strOutData.Contains(StrContentCheck))
                            {
                                isTimeout = false;
                                break;
                            }

                            Thread.Sleep(10);
                        }
                        catch (Exception ex)
                        {
                            LogMessage(ex.Message, MessageLevel.Error);
                            outputdata.Add("Exception1", $"{ ex.Message}");
                            pass_fail = false;
                            break;
                        }
                    }


                    outputdata.Add("CMDStatus", strOutData);
                    LogMessage($"{Description} Read:  {strOutData}");

                }
                catch (Exception ex)
                {
                    Logger.Warn(ex.Message + "Please Check COM Port Correct or NOT");
                    outputdata.Add("Exception", $"{ ex.Message}");
                    pass_fail = false;
                }
            }
                
            if (!pass_fail)
            {
                errorCode = -9;
                Status = "Exception_Error";
            }
            else
            {
                if (!string.IsNullOrEmpty(StrContentCheck))
                {
                    outputdata.Add("StrContentCheck", StrContentCheck);
                    if (strOutData.Contains(StrContentCheck))
                    {
                        errorCode = 0;
                        Status = "OK";
                    }
                    else
                    {
                        errorCode = -1;//ContentCheck incorrect
                        pass_fail = false;
                        Status = $"StrContentCheck_Fail({errorCode})";
                        LogMessage($"CheckString =>{strOutData} is not included {StrContentCheck}", MessageLevel.Debug);
                    }
                }
                else
                {
                    errorCode = -2;//NO ContentCheck
                    Status = "Checkstring is NULL";
                    pass_fail = false;
                }
            }

            outputdata.Add("Status", Status);
            outputdata.Add("errorCode",$"{errorCode}");
            outputjsonStr = JsonConvert.SerializeObject(outputdata, Formatting.Indented);
            jsonStr = outputjsonStr;

            return pass_fail;
        }
        public override bool PostProcess()
        {
            if (Spec != string.Empty && Spec != null)
            {
                string ret = string.Empty;              
                ret = CheckRule(jsonStr, Spec);
                LogMessage($"CheckRule: {ret}", MessageLevel.Debug);
                if (ret == "PASS")
                    return true;
                else
                    return false;
            }
            return true;
        }
    }
}
