using AutoTestSystem.Base;
using AutoTestSystem.Equipment.ControlDevice;
using Manufacture;
using MvCamCtrl.NET;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Renci.SshNet.Security;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows.Forms;
using static AutoTestSystem.BLL.Bd;

namespace AutoTestSystem.Script
{
    internal class Script_JIGControl : Script_ControlDevice_Base
    {
        private Dictionary<string, string> output_data = new Dictionary<string, string>();
        int errorCode;
        string strActItem = string.Empty;
        string strParam = string.Empty;
        string strOutData = string.Empty;
        string ack_data = string.Empty;
        string end_data = string.Empty;

        [Category("Common Parameters"), Description("Arduino Mode")]
        public int Mode { get; set; } = 0;
        [Category("Common Parameters"), Description("Send Dut Command"), Editor(typeof(CommandEditor), typeof(UITypeEditor))]
        public string Send_Command { get; set; } = "";
        [Category("Common Parameters"), Description("CheckContentStr(The Dut Arduino Control Content is return \"Ready\" in MTE)")]
        public string StrContentCheck { get; set; } = "Ready";
        [Category("Common Parameters"), Description("Timeout")]
        public int Timeout { get; set; } = 10000;
        [Category("Common Parameters"), Description("DelayTime")]
        public int DelayTime { get; set; } = 500;

        public override void Dispose()
        {
            //throw new NotImplementedException();
        }
        public override bool PreProcess()
        {
            strOutData = "no_value";
            ack_data = "no_value";
            end_data = "no_value";
            errorCode = -99;
            output_data.Clear();
            return true;
        }
        public override bool Process(ControlDeviceBase controlDevice, ref string outputJsonStr)
        {
            bool processSucceeded = false;

            if (Mode == 0)
            {
                ProcessDataInfo("Timeout", Timeout.ToString());
                ProcessDataInfo("Send Command", Send_Command);

                try
                {
                    controlDevice.SetTimeout(Timeout);
                    //開始前先清掉buffer
                    controlDevice.ClearBuffer();

                    controlDevice.SEND(Send_Command);
                    LogMessage($"Script_ControlDevice_Arduino Send: {Send_Command}", MessageLevel.Debug);

                    bool hasProcessEnded = false;
                    bool isDeviceReady = false;

                    string message = string.Empty;

                    controlDevice.READ(ref message);
                    outputJsonStr += $"{message}";

                    if (!message.Contains($"Ack {Send_Command}"))
                    {
                        ProcessDataInfo("Output", outputJsonStr);
                        outputJsonStr = GetDataInfo();
                        return false;
                    }
                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();
                    while (!hasProcessEnded && stopwatch.ElapsedMilliseconds < Timeout)
                    {
                        try
                        {
                            message = string.Empty;
                            controlDevice.READ(ref message);
                            //if (string.IsNullOrEmpty(message)) continue;

                            outputJsonStr += $"{message}";
                            if (message.Contains("END\r"))
                            {
                                hasProcessEnded = true;
                                break;
                            }                          

                            if (message.Contains("Ready\r"))
                                isDeviceReady = true;

                            Sleep(10);
                        }
                        catch (Exception ex)
                        {
                            ProcessDataInfo("Exception", ex.Message);
                            LogMessage(ex.Message , MessageLevel.Error);
                            break;
                        }
                    }

                    processSucceeded = isDeviceReady && hasProcessEnded;
                }
                catch (Exception ex)
                {
                    LogMessage(ex.Message + " Please Check COM Port Correct or NOT", MessageLevel.Error);
                    ProcessDataInfo("Exception", ex.Message);
                    //outputJsonStr = GetDataInfo();
                }
            }
            ProcessDataInfo("Output", outputJsonStr);
            outputJsonStr = GetDataInfo();
            return processSucceeded;
        }
        public override bool PostProcess()
        {

            if (Spec != string.Empty && Spec != null)
            {
                string ret = string.Empty;
                string jsonStr = string.Empty;

                var output_data = new Dictionary<string, object>
                        {
                            { "errorCode", errorCode }
                        };
                try
                {
                    jsonStr = JsonConvert.SerializeObject(output_data,Formatting.Indented);
                    LogMessage($"output_data: {jsonStr}", MessageLevel.Debug);
                }
                catch (Exception e1)
                {
                    LogMessage($"Error: {jsonStr}=>{e1.Message}", MessageLevel.Error);
                    return false;
                }

                ret = CheckRule(jsonStr, Spec);
                LogMessage($"CheckRule: {ret}", MessageLevel.Debug);
                if (ret == "PASS")
                    return true;
                else
                    return false;
            }
            return true;
        }

        public void ProcessDataInfo(string dataKey, string dataInfo)
        {
            //string outputjsonStr = string.Empty;            
            if (output_data.ContainsKey(dataKey))
                output_data[dataKey] = dataInfo;
            else
                output_data.Add(dataKey, dataInfo);
        }
        public string GetData(string dataKey)
        {
            string jsonStr = string.Empty;
            if (output_data.TryGetValue(dataKey, out var value) && value != null)
            {
                return value.ToString(); // 如果值不為空，返回值的字符串表示形式
            }
            else
            {
                // 如果 key 不存在或者值為空，返回一個適當的預設值，或者拋出異常，視情況而定
                return "";
            }

        }

        public string GetDataInfo()
        {
             Dictionary<string, string> outputdata = new Dictionary<string, string>();
             string jsonStr = string.Empty;

            foreach (var item in output_data.Keys)
            {
                if (outputdata.ContainsKey(item))
                    outputdata[item] = GetData(item);
                else
                    outputdata.Add(item, GetData(item));
            }
            // Convert the dictionary to a JSON string           
            jsonStr = JsonConvert.SerializeObject(outputdata, Formatting.Indented);
            return jsonStr;
        }
    }
}
