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

namespace AutoTestSystem.Script
{
    internal class Script_DUT_NONJSON_UART_Pro : ScriptDUTBase
    {
        string strOutData = string.Empty;

        [Category("Common Parameters"), Description("支援用%%方式做變數值取代")]
        public string Send_Command { get; set; }

        [Category("Common Parameters"), Description("自訂顯示名稱")]
        public int ReadTimeOut { get; set; } = 3000;

        [Category("Common Parameters"), Description("自訂顯示名稱")]
        public int TotalTimeOut { get; set; } = 3000;

        [Category("Common Parameters"), Description("自訂顯示名稱")]
        public string CheckCmd { get; set; }

        [Category("Common Parameters"), Description("0 = default 1 = passive")]
        public int ReadMode { get; set; } = 0;

        [Category("Common Parameters"), Description("自訂顯示名稱")]
        public string TestKeyword { get; set; } = "waitUARTCommand";

        public override void Dispose()
        {
            //throw new NotImplementedException();
        }
        
        public override bool PreProcess()
        {
            if (TestKeyword == "Reinit_COM")
            {
                return true;
            }
            
            if (Send_Command == null || Send_Command == string.Empty)
            {
                RowDataItem.OutputData = "Send_Command can not be null.";
                LogMessage("Send_Command can not be null.", MessageLevel.Error);
                return false;
            }

            if (CheckCmd == null || CheckCmd == string.Empty)
            {
                LogMessage("CheckCmd can not be null.", MessageLevel.Error);
                return false;
            }

            return true;
        }
        
        public override bool Process(DUT_BASE DUTDevice,ref string output)
        {
            DUTDevice.SetTimeout(ReadTimeOut, TotalTimeOut);
            
                if (ReadMode == 0)
                {
                    LogMessage($"Send:  {Send_Command}");
                    DUTDevice.SEND(ReplaceProp(Send_Command));
                    LogMessage($"Send:  {ReplaceProp(Send_Command)}");
                }

                DUTDevice.READ(ReplaceProp(CheckCmd), ref output);
                LogMessage($"Read END:  {output}");
            

            strOutData = output;

            return true;
        }

        public override bool PostProcess()
        {
            string result = CheckRule(strOutData, Spec);
            ExtraProcess(ref result);

            if (result == "PASS" || Spec == "")
            {
                return true;
            }             
            else
            {
                LogMessage($"{result}",MessageLevel.Error);
                return false;
            }
    
        }

        public void ExtraProcess(ref string output)
        {
            switch (CheckCmd)
            {
                case "TOF_Get":

                    string TOF_data = JsonConvert.SerializeObject(JObject.Parse(strOutData)["data"]);
                    PushMoreData("TOF_data", TOF_data);

                    if (!Directory.Exists(@"./TOF_data"))
                        Directory.CreateDirectory(@"./TOF_data");
                    if (PopMoreData("TOF_Calib") == "Done")
                        File.WriteAllText($@"./TOF_data/{PopMoreData("ProductSN")}_{DateTime.Now.ToString("MMddHHmmss")}_now.txt", TOF_data);
                    else
                        File.WriteAllText($@"./TOF_data/{PopMoreData("ProductSN")}_{DateTime.Now.ToString("MMddHHmmss")}_pre.txt", TOF_data);
                    break;

                case "TOF_Calib":

                    string CRC16_pre = JsonConvert.SerializeObject(JObject.Parse(strOutData)["CRC16_pre"]).Split('"')[1];
                    string CRC16_now = JsonConvert.SerializeObject(JObject.Parse(strOutData)["CRC16_now"]).Split('"')[1];

                    if (CRC16_now == CRC16_pre)
                    {
                        output = "TOF Calibration dosen't work!!";
                        PushMESData("CRC16_pre", Tuple.Create("CRC16_pre", CRC16_pre, "FAIL"));
                        PushMESData("CRC16_now", Tuple.Create("CRC16_now", CRC16_now, "FAIL"));
                    }
                    else
                    {
                        PushMoreData("TOF_Calib", "Done");
                        PushMESData("CRC16_pre", Tuple.Create("CRC16_pre", CRC16_pre, "PASS"));
                        PushMESData("CRC16_now", Tuple.Create("CRC16_now", CRC16_now, "PASS"));
                    }
                    break;

                case "Button_Get":

                    string Button_data = JsonConvert.SerializeObject(JObject.Parse(strOutData)["data"]);
                    PushMoreData("Button_data", Button_data);
                    break;
            }
        }

    }
}
