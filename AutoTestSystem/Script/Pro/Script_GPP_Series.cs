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
    internal class Script_GPP_Series : Script_ControlDevice_Base
    {

        string strActItem = string.Empty;
        string strParam = string.Empty;
        string strOutData = string.Empty;
        string Status = string.Empty;

        [Category("Common Parameters"), Description("GPP Mode:0 Set Command and Check Feeback ")]
        public int Mode { get; set; }
        [Category("Common Parameters"), Description("Set Ch1:ISET1:1")]
        public string Set_Command { get; set; } = "ISET1:1";
        [Category("Common Parameters"), Description("Query Ch1:ISET1?")]
        public string Query_Command { get; set; } = "ISET1?";
        [Category("Common Parameters"), Description("Check Ch1")]
        public string CheckContent { get; set; } = "";
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
            Status = "Waiting";
            return true;
        }
        public override bool Process(ControlDeviceBase ControlDevice, ref string outputjsonStr)
        {
            if (Mode == 0)
            {
                try
                {
                    ControlDevice.SetTimeout(Timeout);

                    LogMessage($"{Description} Send:  {Set_Command}");
                    ControlDevice.SEND(Set_Command);

                    Sleep(DelayTime);

                    LogMessage($"{Description} SendQuery:  {Query_Command}");
                    ControlDevice.SEND(Query_Command);
                    

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

                            output += $"{message}";
                            if (output.Contains("\n"))
                            {
                                isTimeout = false;
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
                            {"Read",output },
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

                    var tmp = new Dictionary<string, object>
                    {
                        {"Mode",Mode },
                        {"SetCommand", Set_Command},
                        {"QueryCommand", Query_Command},
                        {"Timeout",Timeout},
                        {"Read",output },
                        {"errorCode","0"}
                    };

                    outputjsonStr = JsonConvert.SerializeObject(tmp, Formatting.Indented);
                    strOutData = outputjsonStr;
                    LogMessage($"strOutData:  {strOutData}");
                    
                    return true;
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
            }
            else if(Mode == 1)
            {
                bool ret = ControlDevice.CheckParam();

                if(ret == false)
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
    }
}
