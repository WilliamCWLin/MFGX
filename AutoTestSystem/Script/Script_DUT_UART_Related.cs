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


namespace AutoTestSystem.Script
{
    internal class Script_DUT_UART_Related : ScriptDUTBase
    {
        string strActItem = string.Empty;
        string strParam = string.Empty;
        string strOutData = string.Empty;
        Uart uart = null;

        public enum ReadMode
        {
            defult = 0,
            passive = 1,
        }

        public override void Dispose()
        {
            //throw new NotImplementedException();
        }
        
        public override bool PreProcess(string Actionitem, string strParamInput)
        {
            //設定要執行的ITEM及初始化一些參數
            strActItem = Actionitem;
            strParam = strParamInput;
            uart = JsonConvert.DeserializeObject<Uart>(strParam);
            return true;
        }
        
        public override bool Process(DUT_BASE DUTDevice)
        {
            bool pass_fail = true;
            string end_data = "";

            DUTDevice.SetTimeout(uart.ReadTimeOut, uart.TotalTimeOut);
            if (strActItem == "WaitNormalRespond")
            {
                DUTDevice.READNOJSON(uart.CheckCmd, ref end_data);
                Logger.Info($"Script_DUT_UART_Related Read END:  {end_data}\n");
                strOutData = end_data;
            }
            else
            {
                if (uart.ReadMode == (int)ReadMode.defult)
                {
                    DUTDevice.SEND(uart.Send_Command);
                    Logger.Info($"Script_DUT_UART_Related Send:  {uart.Send_Command}\n");
                }

                DUTDevice.READ(uart.CheckCmd, ref end_data);
                Logger.Info($"Script_DUT_UART_Related Read END:  {end_data}\n");
                strOutData = end_data;
            }
            return pass_fail;
        }

        public override bool PostProcess(string strCheckSpec, ref string strDataout)
        {
            strDataout = strOutData;
            string result = CheckRule(strOutData, strCheckSpec);
            ExtraProcess(ref result);

            if (result == "PASS" || strCheckSpec == "")
                return true;    
            else
                Logger.Warn($"{result}");
                return false;   
        }

        public void ExtraProcess(ref string result)
        {
            switch (uart.CheckCmd)
            {
                case "TOF_Get":

                    string data = JsonConvert.SerializeObject(JObject.Parse(strOutData)["data"]);
                    PushMoreData("TOF_data", data);

                    if (!Directory.Exists(@"./TOF_data"))
                        Directory.CreateDirectory(@"./TOF_data");                                
                    if (PopMoreData("TOF_Calib") == "Done") 
                        File.WriteAllText($@"./TOF_data/{PopMoreData("ProductSN")}_{DateTime.Now.ToString("MMddHHmmss")}_now.txt", data);
                    else           
                        File.WriteAllText($@"./TOF_data/{PopMoreData("ProductSN")}_{DateTime.Now.ToString("MMddHHmmss")}_pre.txt", data);
                    
                    break;

                case "TOF_Calib":
                  
                    string CRC16_pre = JsonConvert.SerializeObject(JObject.Parse(strOutData)["CRC16_pre"]).Split('"')[1];
                    string CRC16_now = JsonConvert.SerializeObject(JObject.Parse(strOutData)["CRC16_now"]).Split('"')[1];
                    
                    if (CRC16_now == CRC16_pre)
                    {
                        result = "FAIL";
                        PushMESData("CRC16_pre", Tuple.Create("CRC16_pre", CRC16_pre, "FAIL"));
                        PushMESData("CRC16_now", Tuple.Create("CRC16_now", CRC16_now, "FAIL"));
                    }
                    else
                    {
                        PushMESData("CRC16_pre", Tuple.Create("CRC16_pre", CRC16_pre, "PASS"));
                        PushMESData("CRC16_now", Tuple.Create("CRC16_now", CRC16_now, "PASS"));
                    }
                    
                    PushMoreData("TOF_Calib", "Done");
                    
                    break;
            }
        }

        public class Uart
        {
            public string Send_Command { get; set; }
            public int ReadTimeOut { get; set; }
            public int TotalTimeOut { get; set; }
            public string CheckCmd { get; set; }
            public int ReadMode { get; set;}
        }
    }
}
