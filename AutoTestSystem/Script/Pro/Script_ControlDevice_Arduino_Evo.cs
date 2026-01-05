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
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows.Forms;
using static AutoTestSystem.BLL.Bd;

namespace AutoTestSystem.Script
{
    internal class Script_ControlDevice_Arduino_Evo: Script_ControlDevice_Base
    {
        private Dictionary<string, string> output_data = new Dictionary<string, string>();
        int errorCode;
        int C_Channel, R_Channel, G_Channel, B_Channel;
        double RC_Ratio, GC_Ratio, BC_Ratio;
        string strActItem = string.Empty;
        string strParam = string.Empty;
        string strOutData = string.Empty;
        string RGBOutData = string.Empty;
        string OutDataInfo = string.Empty;
        string ack_data = string.Empty;
        string end_data = string.Empty;
        string jsonStr = string.Empty;
        string[] rgb_s = null;
        string[] rgb_ch = null;
        private bool click1 = false;
        //private bool click2 = false;

        public event EventHandler SettingsChanged;

        [Category("Common Parameters"), Description("Arduino Mode"), TypeConverter(typeof(ArduinoMode))]
        public string Mode { get; set; } = "Jig";
        [Category("Mode Select"), Description("Click"), TypeConverter(typeof(CommandMode))]
        public string Comand_Mode { get; set; } = "mode_1";
        [Category("Command Mode 1"), Description("Send Dut Command"), TypeConverter(typeof(SendCommandMode))]
        public string Send_Command_1 { get; set; } = "";
        [Category("Command Mode 1"), Description("CheckContentStr(The Dut Arduino Control Content is return \"Ready\" in MTE)")]
        public string StrContentCheck_1 { get; set; } = "Ready";
        //[Category("Command Set 2"), Description("Click2")]
        //public bool Click2 { get; set; } = false;
        protected void OnSettingsChanged()
        {
            SettingsChanged?.Invoke(this, EventArgs.Empty);
        }
        [Category("Command Mode 2"), Description("Send Dut Command"), TypeConverter(typeof(SendCommandMode))]
        public string Send_Command_2 { get; set; } = "";
        [Category("Command Mode 2"), Description("CheckContentStr(The Dut Arduino Control Content is return \"Ready\" in MTE)")]
        public string StrContentCheck_2 { get; set; } = "Ready";
        [Category("Command Mode 2"), Description("Input command index content."), Editor(typeof(CommandEditor), typeof(UITypeEditor))]
        public string Command_Index { get; set; } = "";
        [Category("Time Set"), Description("Timeout")]
        public int Timeout { get; set; } = 10000;
        [Category("Time Set"), Description("DelayTime")]
        public int DelayTime { get; set; } = 100;
        public string Send_Command = "";
        public string StrContentCheck = "";

        public override void Dispose()
        {
            //throw new NotImplementedException();
        }
        public override bool PreProcess()
        {
            strOutData = "no_value";
            RGBOutData = "no_value";
            C_Channel = -99;
            R_Channel = -99;
            G_Channel = -99;
            B_Channel = -99;
            RC_Ratio = -99;
            GC_Ratio = -99;
            BC_Ratio = -99;
            rgb_s = null;
            rgb_ch = null;
            OutDataInfo = "no_value";
            ack_data = "no_value";
            end_data = "no_value";
            errorCode = -99;
            jsonStr = string.Empty;
            output_data.Clear();
            return true;
        }
        public override bool Process(ControlDeviceBase ControlDevice, ref string outputjsonStr)
        {        
            bool pass_fail = true;
            bool isReady = true;
            bool isEnd = true;
            string status = string.Empty;

            if (Comand_Mode == "mode_1")
            {
                Send_Command = Send_Command_1;
                StrContentCheck = StrContentCheck_1;
            }
            else if (Comand_Mode == "mode_2")
            {
                Send_Command = Send_Command_2;
                StrContentCheck = StrContentCheck_2;
            }


            ProcessDataInfo("Timeout", Timeout.ToString());
            ProcessDataInfo("Send Command", Send_Command);
            ProcessDataInfo("Arduino Mode", Mode.ToString());

            
                if (Mode == "Jig")
                {
                    if (StrContentCheck != string.Empty && StrContentCheck != null)
                    {
                        try
                        {
                            ControlDevice.ClearBuffer();
                            ControlDevice.SetTimeout(Timeout);
                            ControlDevice.SEND(Send_Command);
                            Sleep(DelayTime); // 將時間為毫秒
                            ControlDevice.READ(ref ack_data);
                        }
                        catch (Exception ack)
                        {
                            LogMessage($"CMD:{Send_Command} Ack_Response:{ack_data} => {ack.Message} Please Check COM Port Correct or NOT", MessageLevel.Error);
                            ProcessDataInfo("Exception", $"CMD:{Send_Command} Ack_Response:{ack_data} => {ack.Message} Please Check COM Port Correct or NOT");
                            outputjsonStr = GetDataInfo();
                            return false;
                        }
                        //if (ack_data.Contains($"Ack {Send_Command}\r"))
                        if (ack_data != $"Ack {Send_Command}\r")
                        {
                            MessageBox.Show($"NO Ack ,Please Check Arduino", "Wrning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            errorCode = -2;//NO Ack
                            ProcessDataInfo("Status(ErrorCode)", $"NO_Ack({errorCode})");
                            ProcessDataInfo("Output", $"[{ack_data}][{strOutData}]");
                            outputjsonStr = GetDataInfo();
                            return false;
                        }
                        else
                        {
                            Stopwatch stopwatch = new Stopwatch();
                            stopwatch.Start();

                            string output = string.Empty;
                            bool isTimeout = false;
                            bool Checkcmd = true;
                            bool ex_ret;
                            int Ex_count = 0;
                            int count = 0;
                            while (!output.Contains("END"))
                            {
                                try
                                {
                                    Sleep(DelayTime); // 將時間為毫秒
                                    string message = string.Empty;
                                    if (output.Contains("Time Out") || stopwatch.ElapsedMilliseconds > Timeout * 2)
                                    {
                                        isTimeout = true;
                                        break;
                                    }
                                    pass_fail = ControlDevice.READ(ref message);
                                    output += $"{message}";
                                    strOutData = output;

                                    if (output.Contains("END"))
                                        Checkcmd = false;

                                    // 使用正規表達式找出所有 "Ready" 的出現次數
                                    count = Regex.Matches(output, $"{StrContentCheck}").Count;
                                    //判斷Content次數
                                    if (count > 0)
                                    {
                                        string FinalCheckContent = string.Empty;
                                        for (int i = 0; i < count; i++)
                                            FinalCheckContent += $"{StrContentCheck}\r";

                                        if (Checkcmd)
                                        {
                                            LogMessage($"The checking string Content contains \"{count}\" \"{StrContentCheck}\" ", MessageLevel.Info);
                                            //確認CMD包含ContentCheck設定的字串
                                            if (output != FinalCheckContent)
                                            {
                                                LogMessage($"\r\nCMDStatus=>\r\n{output}\r\nNot eqeal with=>\r\n{FinalCheckContent}\r\n", MessageLevel.Warn);
                                                errorCode = -3;//CMD Incorrect
                                                status = $" {output}({errorCode})";
                                                isReady = false;
                                                break;
                                            }
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    LogMessage($"{ex.Message} Enter COM Port Reset [{Ex_count}] Times", MessageLevel.Error);
                                    ProcessDataInfo("Exception", $"{ex.Message} Enter COM Port Reset [{Ex_count}] Times");
                                    Ex_count++;
                                    //Reset -Start//
                                    ControlDevice.SetTimeout(Timeout);
                                    ex_ret = ControlDevice.UnInit();
                                    Thread.Sleep(1000);
                                    ex_ret &= ControlDevice.Init("");
                                    Thread.Sleep(1000);

                                    if (ex_ret == false || Ex_count > 5)
                                    {
                                        MessageBox.Show($"Reset Fail!! Please Check COM Port Correct or NOT", "Exception Info", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                        return false;
                                    }
                                    //Reset -END//
                                }
                            }

                            if (isTimeout == true)
                            {
                                LogMessage($"Script_ControlDevice_Arduino TimeOut", MessageLevel.Warn);
                                errorCode = -5;//TimeOut
                                status = $" Jigs_TimeOut({errorCode})";
                                isReady = false;
                                isEnd = false;
                            }
                            LogMessage($"\r\nThe Jigs Arduino Set=>\r\n{Send_Command}\r\nArduino_Response=>\r\n{ack_data}{strOutData}\r\n", MessageLevel.Info);

                        }
                    }
                    else
                    {
                        LogMessage($"StrContentCheck is Empty or null!!!", MessageLevel.Warn);
                        errorCode = -1;//NO ContentStr
                        ProcessDataInfo("Status(ErrorCode)", $"StrContentCheck_Empty({errorCode})");
                        outputjsonStr = GetDataInfo();
                        return false;
                    }

                }
                else if (Mode == "RGBSensor")
                {
                    try
                    {
                        ControlDevice.ClearBuffer();
                        ControlDevice.SetTimeout(Timeout);
                        ControlDevice.SEND(Send_Command);
                        LogMessage($"Script_ControlDevice_Arduino Send:{Send_Command}", MessageLevel.Info);
                        Sleep(DelayTime); // 將時間為毫秒
                        ControlDevice.READ(ref ack_data);
                        LogMessage($"Script_ControlDevice_Arduino Read Ack:  {ack_data}", MessageLevel.Info);
                        if (ack_data != $"Ack {Send_Command}\r")
                        {
                            MessageBox.Show($"NO Ack ,Please Check Arduino", "Wrning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            errorCode = -2;//NO Ack
                            ProcessDataInfo("Status(ErrorCode)", $"NO_Ack({errorCode})");
                            ProcessDataInfo("Output", $"[{ack_data}][{RGBOutData}][{end_data}]");
                            outputjsonStr = GetDataInfo();
                            return false;
                        }
                        else
                        {
                            Sleep(DelayTime); // 將時間為毫秒
                            pass_fail = ControlDevice.READ(ref RGBOutData);//R:(Int) G:(Int) B:(Int)
                            LogMessage($"Script_ControlDevice_Arduino Read Test RGB Value:  {RGBOutData}", MessageLevel.Info);
                            strOutData = RGBOutData;

                            if (RGBOutData != null && RGBOutData != "RGB Sensor Error!!\r")
                            {
                                RGBOutData = RGBOutData.Replace(" ", ";");//R:(Int);G:(Int);B:(Int);
                                rgb_s = RGBOutData.TrimEnd('\r').Split(';');

                                for (int i = 0; i < rgb_s.Count(); i++)
                                {
                                    rgb_ch = rgb_s[i].Split(':');
                                    output_data.Add(rgb_ch[0], rgb_ch[1]);

                                    if (rgb_ch[0] == "C")
                                        C_Channel = int.Parse(rgb_ch[1]);
                                    else if (rgb_ch[0] == "R")
                                        R_Channel = int.Parse(rgb_ch[1]);
                                    else if (rgb_ch[0] == "G")
                                        G_Channel = int.Parse(rgb_ch[1]);
                                    else if (rgb_ch[0] == "B")
                                        B_Channel = int.Parse(rgb_ch[1]);
                                }

                                RC_Ratio = (double)R_Channel / C_Channel;
                                GC_Ratio = (double)G_Channel / C_Channel;
                                BC_Ratio = (double)B_Channel / C_Channel;
                                output_data.Add("RC_Ratio", RC_Ratio.ToString("F3"));
                                output_data.Add("GC_Ratio", GC_Ratio.ToString("F3"));
                                output_data.Add("BC_Ratio", BC_Ratio.ToString("F3"));
                            }
                            else
                            {
                                LogMessage($"RGBSensor_Error!!!", MessageLevel.Warn);
                                errorCode = -3;//RGBSensor_Error
                                status = $"{RGBOutData}({errorCode})";
                                isReady = false;
                            }

                            Sleep(DelayTime);  // 將時間為毫秒
                            ControlDevice.READ(ref end_data);
                            LogMessage($"Script_ControlDevice_Arduino Read END:  {end_data}", MessageLevel.Info);
                            if (!end_data.Contains("END"))
                            {
                                LogMessage($"Script_ControlDevice_Arduino Read END Fails:  {end_data}", MessageLevel.Warn);
                                errorCode = -4;//NO END
                                status = $" NO_END({errorCode})";
                                isEnd = false;
                            }
                        }
                    }
                    catch (Exception ex)
                    {

                        LogMessage(ex.Message + "Please Check COM Port Correct or NOT", MessageLevel.Error);
                        ProcessDataInfo("Exception", ex.Message);
                        pass_fail = false;
                    }
                }
                else
                {
                    LogMessage($"Arduino Mode not defind!!!", MessageLevel.Warn);
                    errorCode = -30;//Mode_not_defind
                    ProcessDataInfo("Status(ErrorCode)", $"Mode_not_defind({errorCode})");
                    outputjsonStr = GetDataInfo();
                    return false;
                }

                if (!isReady || !isEnd)
                    pass_fail = false;
                else
                {
                    errorCode = 0;//ok
                    status = $"OK({errorCode})";
                }

                if (Mode == "RGBSensor")
                    OutDataInfo = $"[{ack_data}][{strOutData}][{end_data}]";
                else
                    OutDataInfo = $"[{ack_data}][{strOutData}]";
                ProcessDataInfo("Status(ErrorCode)", status);
                ProcessDataInfo("errorCode", $"{errorCode}");
                ProcessDataInfo("Output", OutDataInfo);
                outputjsonStr = GetDataInfo();
                jsonStr = outputjsonStr;

                return pass_fail;
            

            
            
        }
        public override bool PostProcess()
        {

            if (Spec != string.Empty && Spec != null)
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

        public class ArduinoMode : TypeConverter  //下拉式選單
        {
            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {


                List<string> CopyListKeys = new List<string>();

                CopyListKeys.Add("Jig");
                CopyListKeys.Add("RGBSensor");

                return new StandardValuesCollection(CopyListKeys);

            }

            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                return true;

            }

        }
        public class SendCommandMode : TypeConverter  //下拉式選單
        {
            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {


                List<string> CopyListKeys = new List<string>();

                CopyListKeys.Add("Jig_Status");
                CopyListKeys.Add("PowerOn");
                CopyListKeys.Add("PowerOff");
                CopyListKeys.Add("PowerRST");
                CopyListKeys.Add("Button1_Press");
                CopyListKeys.Add("Button1_Release");
                CopyListKeys.Add("GetCurrent_1");
                CopyListKeys.Add("GetCurrent_2");
                CopyListKeys.Add("GetCurrent_3");
                CopyListKeys.Add("GetCurrent_4");
                CopyListKeys.Add("GetRGB_1_RGB");
                CopyListKeys.Add("GetRGB_2_RGB");
                CopyListKeys.Add("GetRGB_3_RGB");
                CopyListKeys.Add("I2C_Scanner");
                CopyListKeys.Add("IoStatus");
                CopyListKeys.Add("FW_Version");
                CopyListKeys.Add("Change_Description");
                CopyListKeys.Add("?");

                return new StandardValuesCollection(CopyListKeys);

            }

            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                return true;

            }

        }
        public class CommandFunctionMode : TypeConverter  //下拉式選單
        {
            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {


                List<string> CopyListKeys = new List<string>();

                CopyListKeys.Add("One Command");
                CopyListKeys.Add("Multi Command");

                return new StandardValuesCollection(CopyListKeys);

            }

            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                return true;

            }

        }
        public class CommandMode : TypeConverter  //下拉式選單
        {
            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {


                List<string> CopyListKeys = new List<string>();

                CopyListKeys.Add("Mode_1");
                CopyListKeys.Add("Mode_2");

                return new StandardValuesCollection(CopyListKeys);

            }

            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                return true;

            }

        }
    }
}
