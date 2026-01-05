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
using System.Threading.Tasks;
using System.Windows.Forms;
using static AutoTestSystem.BLL.Bd;

namespace AutoTestSystem.Script
{
    internal class Script_PPX_Pro : Script_ControlDevice_Base
    {

        string strActItem = string.Empty;
        string strParam = string.Empty;
        string strOutData = string.Empty;
        string Status = string.Empty;

        [Category("Common Parameters"), Description("PPX Mode:0 Set Command and Check Feeback ")]
        public int Mode { get; set; }

        [Category("Select Work mode"), Description("Output Status (ON/OFF)"), TypeConverter(typeof(OutputModeList))]
        public string ON_OFF { get; set; } = "ON";

        [Category("Select Work mode"), Description("Set Output mode (CVHS:0 CCHS:1 CVLS:2 CCLS:3)"), TypeConverter(typeof(WorkmodeList))]
        public string ModeList { get; set; } = "CVHS";

        [Category("Common Parameters"), Description("Set Command")]
        public string Set_Command { get; set; } = ":APPLY 3.6,1.0";
        [Category("Common Parameters"), Description("Query Command")]
        public string Query_Command { get; set; } = ":APPLY?";
        [Category("Common Parameters"), Description("Check Content")]
        public string CheckContent { get; set; } = "";

        [Category("TimeSet"), Description("Timeout")]
        public int Timeout { get; set; } = 5000;
        [Category("TimeSet"), Description("DelayTime")]
        public int DelayTime { get; set; } = 500;

        public override void Dispose()
        {
            //throw new NotImplementedException();
        }
        public override bool PreProcess()
        {
            strOutData = string.Empty;
            Status = "Waiting";
            return true;
        }
        public override bool Process(ControlDeviceBase ControlDevice, ref string outputjsonStr)
        {
            bool Check_WorkMode = true;
            bool Check_OutputMode = true;
            string CheckOutput = string.Empty;
            var tmp = new Dictionary<string, object>();
            try
            {
                ControlDevice.SetTimeout(Timeout);

                Check_WorkMode = WorkModeSet(ControlDevice, ModeList);
                if (!Check_WorkMode)
                {
                    LogMessage("Check_WorkMode Fail", MessageLevel.Error);
                    return false;
                }
                Check_OutputMode = OutModeSet(ControlDevice, ON_OFF);
                if (!Check_OutputMode)
                {
                    LogMessage("Check_OutputMode Fail", MessageLevel.Error);
                    return false;
                }

                if (Mode == 0)
                {

                    ControlDevice.SetTimeout(Timeout);

                    Check_WorkMode = WorkModeSet(ControlDevice, ModeList);
                    if (!Check_WorkMode)
                    {
                        LogMessage("Check_WorkMode Fail", MessageLevel.Error);
                        return false;
                    }
                    Check_OutputMode = OutModeSet(ControlDevice, ON_OFF);
                    if (!Check_OutputMode)
                    {
                        LogMessage("Check_OutputMode Fail", MessageLevel.Error);
                        return false;
                    }

                    LogMessage($"{Description} Send:  {Set_Command}");
                    ControlDevice.SEND(Set_Command);

                    Sleep(DelayTime);

                    LogMessage($"{Description} SendQuery:  {Query_Command}");
                    ControlDevice.SEND(Query_Command);

                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();

                    string output = string.Empty;
                    string[] Output_Value = null;
                    bool isTimeout = true;
                    while (stopwatch.ElapsedMilliseconds < Timeout)
                    {
                        try
                        {
                            string message = string.Empty;

                            ControlDevice.READ(ref message);

                            output += $"{message}";
                            if (output.Contains("\n"))
                            {
                                isTimeout = false;
                                /*if (output.Contains(","))
                                {
                                    Output_Value = output.Split(',');
                                }*/
                                break;
                            }

                            Sleep(10);
                        }
                        catch (Exception ex)
                        {
                            LogMessage(ex.Message, MessageLevel.Error);
                            break;
                        }
                    }

                    if (isTimeout == true)
                    {
                        LogMessage($"Read is Timeout, Data is (\"{output}\")", MessageLevel.Error);
                        var error = new Dictionary<string, object>
                        {
                            {"Send",Mode },
                            {"SendCommand", Set_Command},
                            {"ErrorMsg","Timeout"},
                            {"OutputData",output },
                            {"errorCode","-1"}
                        };
                        outputjsonStr = JsonConvert.SerializeObject(error, Formatting.Indented);
                        return false;
                    }

                    if (string.IsNullOrEmpty(output))
                    {
                        LogMessage($"Output is null - > Fail", MessageLevel.Error);
                        return false;
                    }
                    else
                    {
                        if (!output.Contains(CheckContent))
                        {
                            LogMessage($"Check {output} contains {CheckContent} - > Fail", MessageLevel.Error);
                            return false;
                        }

                    }

                    tmp.Add("Mode", Mode);
                    tmp.Add("SetCommand", Set_Command);
                    tmp.Add("QueryCommand", Query_Command);
                    tmp.Add("Timeout", Timeout);

                    /*if (output.Contains(","))
                    {
                        tmp.Add("Volt", Output_Value[0]);
                        tmp.Add("Current", Output_Value[1]);
                        tmp.Add("Power", Output_Value[2]);
                    }
                    else*/
                    {
                        tmp.Add("OutputData", output);
                    }
                    tmp.Add("errorCode", "0");

                    outputjsonStr = JsonConvert.SerializeObject(tmp, Formatting.Indented);
                    strOutData = outputjsonStr;
                    LogMessage($"strOutData:  {strOutData}");

                    return true;
                }
                else if (Mode == 1)
                {
                    bool ret = ControlDevice.CheckParam();

                    if (ret == false)
                    {
                        var Exceptiondata = new Dictionary<string, object>
                    {
                        {"Mode",Mode },
                        {"Action", "CheckParam"},
                        {"errorCode",-1 },
                    };
                        outputjsonStr = JsonConvert.SerializeObject(Exceptiondata, Formatting.Indented);
                    }
                    else
                    {
                        var Exceptiondata = new Dictionary<string, object>
                    {
                        {"Mode",Mode },
                        {"Action", "CheckParam"},
                        {"errorCode",0 },
                    };
                        outputjsonStr = JsonConvert.SerializeObject(Exceptiondata, Formatting.Indented);
                        return true;
                    }

                }
            }
            catch (Exception ex)
            {
                Logger.Warn(ex.Message + "Please Check COM Port Correct or NOT");
                var Exceptiondata = new Dictionary<string, object>
                    {
                        {"Mode",Mode },
                        {"SendCommand", Set_Command},
                        {"Timeout",Timeout},
                        {"errorCode",-2 },
                        {"ErrorMsg", $"{ ex.Message}"}
                    };
                outputjsonStr = JsonConvert.SerializeObject(Exceptiondata, Formatting.Indented);
                return false;
            }


            return false;
        }
        public override bool PostProcess()
        {
            if (Spec != string.Empty && Spec != null)
            {
                string ret = string.Empty;

                ret = CheckRule(strOutData, Spec);
                LogMessage($"CheckRule: {ret}", MessageLevel.Debug);
                if (ret == "PASS")
                    return true;
                else
                    return false;
            }
            return true;
        }

        public bool WorkModeSet(ControlDeviceBase ControlDevice, string mode)
        {
            bool CheckMode = true;
            switch (mode)
            {
                case "CVHS":                   
                      LogMessage($"Send CVHS", MessageLevel.Info);
                      ControlDevice.SEND(":OUTP:MODE 0");
                      //CheckMode = ControlDevice.CommandCheckForScript(":OUTP:MODE?","0");
                    break;
                case "CCHS":
                    LogMessage($"Send CCHS", MessageLevel.Info);
                    ControlDevice.SEND(":OUTP:MODE 1");
                    //CheckMode = ControlDevice.CommandCheckForScript(":OUTP:MODE?", "1");
                    break;
                case "CVLS":
                    LogMessage($"Send CVLS", MessageLevel.Info);
                    ControlDevice.SEND(":OUTP:MODE 2");
                    //CheckMode = ControlDevice.CommandCheckForScript(":OUTP:MODE?", "2");
                    break;
                case "CCLS":
                    LogMessage($"Send CCLS", MessageLevel.Info);
                    ControlDevice.SEND(":OUTP:MODE 3");
                    //CheckMode = ControlDevice.CommandCheckForScript(":OUTP:MODE?", "3");
                    break;
                default:
                    LogMessage($"WorkMode is not definde", MessageLevel.Error);
                    break;
            }
            return CheckMode;
        }

        public bool OutModeSet(ControlDeviceBase ControlDevice, string ON_OFF)
        {
            bool OutMode = true;
            switch (ON_OFF)
            {
                case "ON":
                    LogMessage($"Output ON", MessageLevel.Info);
                    ControlDevice.SEND(":OUTP 1");
                    //OutMode = ControlDevice.CommandCheckForScript(":OUTP?","1");
                    break;
                case "OFF":
                    LogMessage($"Output OFF", MessageLevel.Info);
                    ControlDevice.SEND(":OUTP 0");
                    //OutMode = ControlDevice.CommandCheckForScript(":OUTP?", "0");
                    break;                
                default:
                    LogMessage($"Output is not definde", MessageLevel.Error);
                    break;
            }
            return OutMode;
        }

        public class WorkmodeList : TypeConverter  //下拉式選單
        {
            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {


                List<string> WorkmodeList = new List<string>();

                WorkmodeList.Add("CVHS");
                WorkmodeList.Add("CCHS");
                WorkmodeList.Add("CVLS");
                WorkmodeList.Add("CCLS");

                return new StandardValuesCollection(WorkmodeList);

            }

            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                return true;

            }

        }

        public class OutputModeList : TypeConverter  //下拉式選單
        {
            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {


                List<string> OutputMode = new List<string>();

                OutputMode.Add("ON");
                OutputMode.Add("OFF");

                return new StandardValuesCollection(OutputMode);

            }

            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                return true;

            }

        }
    }
}
