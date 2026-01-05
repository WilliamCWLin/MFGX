using AutoTestSystem.Base;
using AutoTestSystem.DUT;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using static AutoTestSystem.BLL.Bd;
using System.ComponentModel;


namespace AutoTestSystem.Script
{
    internal class Script_DUT_ATCmd_Related : ScriptDUTBase
    {
        string strActItem = string.Empty;
        string strParam = string.Empty;
        string strOutData = string.Empty;
        
        Uart uart = null;

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
            string end_data = string.Empty;

            DUTDevice.SetTimeout(uart.ReadTimeOut, uart.TotalTimeOut);
            DUTDevice.SEND(uart.Send_Command + (char)(13));
            LogMessage($"Send:  {uart.Send_Command}\n");

            DUTDevice.READ(ref end_data);
            LogMessage($"Read END:  {end_data}\n");
            strOutData = end_data;

            if (uart.CommandType == 2)
            {
                if (uart.Send_Parameter == string.Empty)
                {
                    LogMessage($"There is no paremeter for {uart.Send_Command}", MessageLevel.Error);
                    return false;
                }
                else
                {
                    DUTDevice.SEND(uart.Send_Parameter + (char)(26));
                    LogMessage($"Send:  {uart.Send_Parameter}\n");

                    DUTDevice.READ(ref end_data);
                    LogMessage($"END:  {end_data}\n");
                    strOutData += end_data;
                }
            }

            return true;
        }

        public override bool PostProcess(string strCheckSpec, ref string strDataout)
        {
            string result = string.Empty;
            strDataout = strOutData;

            if (uart.CommandType == 1)
            {
                ExtraProcess(ref result);
                if (result == "PASS")
                    return true;
                else
                    return false;
            }
            else
                return true;

        }

        public void ExtraProcess(ref string result)
        {
            switch (uart.Send_Command)
            {
                case "AT#GPIO=1,2":                    
                    if (strOutData.Contains("#GPIO: 1,1") == true)
                    {
                        MessageBox.Show("LTE W1 LED ON");
                        result = "PASS";
                    }
                    if (strOutData.Contains("#GPIO: 1,0") == true)
                    {
                        MessageBox.Show("LTE W1 LED OFF");
                    }
                    break;

                case "AT#GPIO=2,2":
                    if (strOutData.Contains("#GPIO: 1,1") == true)
                    {
                        MessageBox.Show("LTE W2 LED ON");
                        result = "PASS";
                    }
                    if (strOutData.Contains("#GPIO: 1,0") == true)
                    {
                        MessageBox.Show("LTE W2 LED OFF");
                    }
                    break;
                
                case "AT#GPIO=3,2":
                    if (strOutData.Contains("#GPIO: 1,1") == true)
                    {
                        MessageBox.Show("LTE W3 LED ON");
                        result = "PASS";
                    }
                    if (strOutData.Contains("#GPIO: 1,0") == true)
                    {
                        MessageBox.Show("LTE W3 LED OFF");
                    }
                    break;

                case "AT#GPIO=4,2":
                    if (strOutData.Contains("#GPIO: 1,1") == true)
                    {
                        MessageBox.Show("LTE W4 LED ON");
                        result = "PASS";
                    }
                    if (strOutData.Contains("#GPIO: 1,0") == true)
                    {
                        MessageBox.Show("LTE W4 LED OFF");
                    }
                    break;

                case "AT#GPIO=5,2":
                    if (strOutData.Contains("#GPIO: 1,0") == true)
                    {
                        MessageBox.Show("Status Orange LED ON");
                        result = "PASS";
                    }
                    if (strOutData.Contains("#GPIO: 1,1") == true)
                    {
                        MessageBox.Show("Status Orange LED OFF");
                    }
                    break;
                
                case "AT#GPIO=6,2":
                    if (strOutData.Contains("#GPIO: 1,1") == true)
                    {
                        MessageBox.Show("Status Blue LED ON");
                        result = "PASS";
                    }
                    if (strOutData.Contains("#GPIO: 1,0") == true)
                    {
                        MessageBox.Show("Status Blue LED OFF");
                    }
                    break;
                
                case "AT#GPIO=7,2":
                    if (strOutData.Contains("#GPIO: 0,0,5") == true)
                    {
                        MessageBox.Show("Tamper Switch Pressed");
                        result = "PASS";
                    }
                    if (strOutData.Contains("#GPIO: 0,1,5") == true)
                    {
                        MessageBox.Show("Tamper Switch released");
                        result = "PASS";
                    }
                    break;

            }
        }

        public class Uart
        {
            public string Send_Command { get; set; }
            public string Send_Parameter { get; set; }
            public int ReadTimeOut { get; set; }
            public int TotalTimeOut { get; set; }
            public int CommandType { get; set; }
        }
    }
}
