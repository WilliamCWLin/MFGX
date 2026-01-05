using AutoTestSystem.Base;
using AutoTestSystem.DUT;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Diagnostics;
using static AutoTestSystem.BLL.Bd;
using System.ComponentModel;
using System.Drawing.Design;
using Manufacture;
using static AutoTestSystem.Model.IQ_SingleEntry;
using System.Drawing;
using System.Xml.Linq;
using System.Threading;

namespace AutoTestSystem.Script
{

    internal class Script_DUT_CommandCheck : ScriptDUTBase
    {
        public enum BOOT_ACTION
        {
            SEND,
            READ,
            SENDREAD,
            READSEND,
        }

        string strOutData = string.Empty;

        [Category("Command"), Description("自訂顯示名稱")]
        public string P1_Send_NewLine { get; set; } = "CRLF";

        [Category("Command"), Description("自訂顯示名稱")]
        public BOOT_ACTION P0_MODE { get; set; } = BOOT_ACTION.SENDREAD;

        [Category("Command"), Description("支援用%%方式做變數值取代")]
        public string P2_Send_Command { get; set; } = string.Empty;

        [Category("Command"), Description("自訂顯示名稱")]
        public int P4_TotalTimeOut { get; set; } = 10000;

        [Category("Command"), Description("自訂顯示名稱"), Editor(typeof(CheckPattern), typeof(UITypeEditor))]
        public string P3_Check_Response { get; set; } = string.Empty;
        [Category("Command"), Description("load content"), Editor(typeof(ParserPatterns), typeof(UITypeEditor))]
        public string P5_ParserPatterns { get; set; } = "";
        public override void Dispose()
        {
            //throw new NotImplementedException();
        }

        public override bool PreProcess()
        {

            strOutData = string.Empty;

            return true;
        }

        public override bool Process(DUT_BASE DUTDevice, ref string output)
        {
            string ReadOutput = string.Empty;
            string SendCmd = ReplaceProp(P2_Send_Command);
            bool ret = false;
            try
            {
                DUTDevice.ClearBuffer();

                switch (P0_MODE)
                {
                    case BOOT_ACTION.SEND:

                        ret = SEND_COMMAND(DUTDevice, SendCmd);

                        if (!ret)
                        {
                            output = "Send Command Fail";
                        }

                        break;
                    case BOOT_ACTION.READSEND:

                        ret = ReadAndCheck(DUTDevice, ref ReadOutput);
                        LogMessage($"\n>>>>>>>>>>>>>>>>>>>>>>ReadInfo<<<<<<<<<<<<<<<<<<<<<\n{ReadOutput.Replace("\r\r\n", "\r\n")}");
                        if (ret)
                        {

                            if(!SEND_COMMAND(DUTDevice, SendCmd))
                                output = "Send Command Fail";
                            //LogMessage("Send Command Fail");
                        }                            
                        else
                        {
                            output = "Read Check Timeout";
                        }
                            //LogMessage("Read Check Timeout");

                        break;
                    case BOOT_ACTION.SENDREAD:

                        ret = SEND_COMMAND(DUTDevice, SendCmd);

                        if(ret == false)
                        {
                            output = "Send Command Fail";
                            //LogMessage("Send Command Fail");
                        }                   
                        else
                        {
                            ret = ReadAndCheck(DUTDevice, ref ReadOutput);
                            if (!ret)
                            {
                                output = "Read Check Timeout";
                                //LogMessage("Read Check Timeout");
                            }
                            else
                                LogMessage($"\n>>>>>>>>>>>>>>>>>>>>>>ReadInfo<<<<<<<<<<<<<<<<<<<<<\n{ReadOutput.Replace("\r\r\n", "\r\n")}");

                        }                      

                        break;
                    case BOOT_ACTION.READ:
                        ret = ReadAndCheck(DUTDevice, ref ReadOutput);

                        if(ret == false)
                        {
                            output = "Read Check Timeout";
                        }
                        else
                            LogMessage($"\n>>>>>>>>>>>>>>>>>>>>>>ReadInfo<<<<<<<<<<<<<<<<<<<<<\n{ReadOutput.Replace("\r\r\n", "\r\n")}");

                        break;

                }
                if (ret && !string.IsNullOrEmpty(P5_ParserPatterns)) // 如果讀取成功並且有指定匹配模式，繼續執行
                {
                    var result = ExtractDataFromPatterns(ReadOutput, P5_ParserPatterns);
                    if (result.Count == 0) // 若無法匹配模式，則認定失敗
                    {
                        output = "ExtractData Fail.";
                        LogMessage($"\n==============ExtractData Fail==============\n>>>>>>>>>>>>>>>>>>>>>>ReadInfo<<<<<<<<<<<<<<<<<<<<<\n{ReadOutput.Replace("\r\r\n", "\r\n")}\n>>>>>>>>>>>>>>>>>>>>>Patterns<<<<<<<<<<<<<<<<<<<<<\n{P5_ParserPatterns.Replace("\r\r\n", "\r\n")}");
                        ret = false;
                    }
                    else
                    {
                        output = JsonConvert.SerializeObject(result, Formatting.Indented);
                        strOutData = output;
                        LogMessage("\n==============Extract Data==============");
                    }
                }

                LogMessage(output+"\n");
                DUTDevice.ClearBuffer();

                return ret;
            }
            catch (Exception ex)
            {
                output = ex.Message;
                LogMessage(output);
                strOutData = output;
                DUTDevice.ClearBuffer();
                return false;
            }
        }

        public override bool PostProcess()
        {
            string result = CheckRule(strOutData, Spec);

            if (result == "PASS" || Spec == "")
            {
                return true;
            }
            else
            {
                LogMessage($"{result}", MessageLevel.Error);

                LogMessage($"\n==============ExtractData Fail==============\n>>>>>>>>>>>>>>>>>>>>>>ReadInfo<<<<<<<<<<<<<<<<<<<<<\n{strOutData.Replace("\r\r\n", "\r\n")}\n>>>>>>>>>>>>>>>>>>>>>Patterns<<<<<<<<<<<<<<<<<<<<<\n{Spec.Replace("\r\r\n", "\r\n")}");
                return false;
            }

        }

        public bool SEND_COMMAND(DUT_BASE DUTDevice, string cmd)
        {
            cmd = cmd + P1_Send_NewLine;
            cmd = cmd.Replace("\\r\\n", "\r\n")
                     .Replace("\\r", "\r")
                     .Replace("\\n", "\n")
                     .Replace("CRLF", "\r\n")
                     .Replace("CR", "\r")
                     .Replace("LF", "\n");

            return DUTDevice.SEND(cmd);
        }
        public bool ValidateSectionOutput(string sectionOutput)
        {
            string[] lines = P3_Check_Response.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            
            foreach (var pattern in lines)
            {
                if (!sectionOutput.Contains(pattern))
                {
                    return false;
                }
            }
            return true;
            //switch (CheckMode)
            //{
            //    case ValidationMode.All:
            //        foreach (var pattern in _patterns)
            //        {
            //            if (!Regex.IsMatch(sectionOutput, pattern))
            //            {
            //                return false;
            //            }
            //        }
            //        return true;

            //    case ValidationMode.Any:
            //        foreach (var pattern in _patterns)
            //        {
            //            if (Regex.IsMatch(sectionOutput, pattern))
            //            {
            //                return true;
            //            }
            //        }
            //        return false;

            //    case ValidationMode.Contains:
            //        foreach (var pattern in _patterns)
            //        {
            //            if (!sectionOutput.Contains(pattern))
            //            {
            //                return false;
            //            }
            //        }
            //        return true;

            //    case ValidationMode.NoCheck:

            //        return true;

            //    default:
            //        throw new ArgumentOutOfRangeException();
            //}
        }
        public bool ReadAndCheck(DUT_BASE dutDevice, ref string outputString)
        {
            // 用於儲存輸出內容
            StringBuilder outputBuilder = new StringBuilder();
            string sectionOutput = string.Empty;
            bool isCheckSuccessful = false;

            // 設置計時器
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            string sectiontemp = string.Empty;
            while (stopwatch.ElapsedMilliseconds <= P4_TotalTimeOut)
            {
                string tempBuffer = string.Empty;

                // 嘗試從設備中讀取數據
                dutDevice.READ(ref tempBuffer);

                if (!string.IsNullOrEmpty(tempBuffer))
                {
                    sectiontemp += tempBuffer;
                    outputBuilder.Append(tempBuffer); // 將新數據添加到輸出
                    sectionOutput = outputBuilder.ToString();

                    // 將 tempBuffer 按行分割，逐行處理
                    //string[] lines = sectiontemp.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                    
                    //if (sectionOutput.Contains(CheckCmd))
                    if(ValidateSectionOutput(sectionOutput))
                    {
                        isCheckSuccessful = true;
                        break;
                    }

                }

                //if (isCheckSuccessful)
                //{
                //    break; // 匹配到目標後跳出循環
                //}
                Thread.Sleep(3); // 短暫等待以減少 CPU 資源佔用
            }
            outputString = outputBuilder.ToString();
            //sectionOutput = outputBuilder.ToString();

            //if (!string.IsNullOrEmpty(sectionOutput))
            //{
            //    string output = string.Empty;
            //    // 根據自定義換行符分割數據
            //    string[] lines = sectionOutput.Split(new[] { "\r\n" }, StringSplitOptions.None);

            //    // 將完整行加入隊列，最後一部分可能是未完成行
            //    for (int i = 0; i < lines.Length - 1; i++)
            //    {
            //        if (lines[i].Contains(CheckCmd))
            //        {
            //            dutDevice.StartAction("WriteLog", "{color:green}" + lines[i]+ "\r\n", ref output);
            //        }
            //        else
            //            dutDevice.StartAction("WriteLog", lines[i]+ "\r\n", ref output);
            //    }

            //    // 保留最後一部分（可能是未完成的行）在 tempBuffer 中
            //    dutDevice.StartAction("WriteLog", lines[lines.Length - 1], ref output);
            //}

            // 若超時，記錄超時訊息
            if (!isCheckSuccessful)
            {
                sectionOutput = $"Timeout after {P4_TotalTimeOut} ms";
            }

            // 停止計時器並釋放資源
            stopwatch.Stop();
            outputBuilder.Clear();

            return isCheckSuccessful;
        }
    }
}
