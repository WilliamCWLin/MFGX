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
    internal class Script_DUT_ATCmd_Pro : ScriptDUTBase
    {
        string strOutData = string.Empty;

        [Category("Common Parameters"), Description("自訂顯示名稱")]
        public string Send_Command { get; set; }

        [Category("Common Parameters"), Description("自訂顯示名稱")]
        public string Send_Parameter { get; set; }

        [Category("Common Parameters"), Description("自訂顯示名稱")]
        public int ReadTimeOut { get; set; }

        [Category("Common Parameters"), Description("自訂顯示名稱")]
        public int TotalTimeOut { get; set; }

        [Category("Common Parameters"), Description("0 = Normal ; 1 = Check ; 2 = I2CWR")]
        public int CommandType { get; set; }
        

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
            bool pass_fail = true;
            string end_data = string.Empty;

            DUTDevice.SetTimeout(ReadTimeOut, TotalTimeOut);
            DUTDevice.SEND(Send_Command + (char)(13));
            LogMessage($"Send:  {Send_Command}\n");

            DUTDevice.READ(ref end_data);
            LogMessage($"Read END:  {end_data}\n");
            strOutData = end_data;

            if (CommandType == 2)
            {
                if (Send_Parameter == string.Empty)
                {
                    LogMessage($"There is no paremeter for {Send_Command}", MessageLevel.Error);
                    pass_fail = false;
                }
                else
                {
                    DUTDevice.SEND(Send_Parameter + (char)(26));
                    LogMessage($"Send:  {Send_Parameter}\n");

                    DUTDevice.READ(ref end_data);
                    LogMessage($"Read END:  {end_data}\n");
                    strOutData += end_data;
                }
            }

            if (CommandType == 1)
            {
                output = strOutData;
            }
            else
            {

            }

            return pass_fail;
        }

        public override bool PostProcess()
        {
            string result = string.Empty;

            if (CommandType == 1)
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
            switch (Send_Command)
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
                        MessageBox.Show("...");
                        result = "PASS";
                    }
                    if (strOutData.Contains("#GPIO: 0,1,5") == true)
                    {
                        MessageBox.Show("INT");
                        result = "PASS";
                    }
                    break;

            }
        }

    }
}
