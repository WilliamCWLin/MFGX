using AutoTestSystem.Base;
using AutoTestSystem.DUT;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using static AutoTestSystem.BLL.Bd;


namespace AutoTestSystem.Script
{
    internal class Script_DUT_USB_ATCmd_Pro : ScriptDUTBase
    {
        string strOutData = string.Empty;

        [Category("Common Parameters"), Description("支援用%%方式做變數值取代")]
        public string Send_Command { get; set; }

        [Category("Common Parameters"), Description("支援用%%方式做變數值取代")]
        public string Send_Parameter { get; set; }

        [Category("Common Parameters"), Description("自訂顯示名稱")]
        public int ReadTimeOut { get; set; } = 3000;

        [Category("Common Parameters"), Description("自訂顯示名稱")]
        public int TotalTimeOut { get; set; } = 3000;

        [Category("Common Parameters"), Description("0 = Set ; 1 = Get ; 2 = Write")]
        public int CommandType { get; set; } = 0;


        public override void Dispose()
        {
            //throw new NotImplementedException();
        }

        public override bool PreProcess()
        {
            if (Send_Command == null || Send_Command == string.Empty)
            {
                LogMessage("Send_Command can not be null.", MessageLevel.Error);
                return false;
            }

            return true;
        }

        public override bool Process(DUT_BASE DUTDevice, ref string output)
        {
            string end_data = string.Empty;

            DUTDevice.SetTimeout(ReadTimeOut, TotalTimeOut);
            DUTDevice.SEND(ReplaceProp(Send_Command) + (char)(13));
            LogMessage($"Send:  {ReplaceProp(Send_Command)}\n");

            DUTDevice.READ(ref end_data);
            //LogMessage($"Read END:  {end_data}\n");
            strOutData = end_data;

            if (CommandType == 2)
            {
                if (Send_Parameter == null || Send_Parameter == string.Empty)
                {
                    LogMessage($"There is no paremeter for {ReplaceProp(Send_Command)}, \"Send_Paremeter\" is empty", MessageLevel.Error);
                    return false;
                }
                else
                {
                    DUTDevice.SEND(ReplaceProp(Send_Parameter) + (char)(26));
                    LogMessage($"Send:  {ReplaceProp(Send_Parameter)}\n");

                    DUTDevice.READ(ref end_data);
                    //LogMessage($"Read END:  {end_data}\n");
                    strOutData += end_data;
                }
            }

            output = ProcessData();
            LogMessage($"Read END:  {output}\n");

            return true;
        }

        public override bool PostProcess()
        {
            string result = CheckRule(strOutData, Spec);

            if (result == "PASS")
            {
                ExtraProcess();
                return true;
            }   
            else
                return false;
        }

        public string ProcessData()
        {
            var jsonOutData = new Dictionary<string, object> { };

            if (strOutData.Contains("OK") == true)
            {
                jsonOutData.Add("errorCode", "0");

                if (CommandType == 1)
                {
                    string data = Regex.Split(strOutData, "\r\r")[1];

                    if (data.Contains("<<<"))
                    {
                        string value = Regex.Split(Regex.Split(data, "<<<")[1], "\r")[0];
                        jsonOutData.Add("data", value);
                    }

                    if (data.Contains(": "))
                    {
                        string key = Regex.Split(data, ": ")[0];
                        string value = Regex.Split(data, ": ")[1];
                        if (key.Contains("#"))
                            jsonOutData.Add(key.Split('#')[1], value);
                        else if (key.Contains("+"))
                        {
                            switch (key)
                            {
                                case "+ICCID":
                                    jsonOutData.Add("length", value.Length);
                                    break;

                                default:
                                    jsonOutData.Add(key.Split('+')[1], value);
                                    break;
                            }
                        }
                        else
                            jsonOutData.Add(key, value);
                    }

                    if (data.Contains("-P0L."))
                    {
                        string[] ver = Regex.Split(data, "\r");
                        string value = $"{ver[0]}-{ver[1]}-{ver[3]}";
                        jsonOutData.Add("version", value);
                    }

                }

            }
            else
            {
                jsonOutData.Add("errorCode", "-1");
            }

            strOutData = JsonConvert.SerializeObject(jsonOutData);

            return strOutData;
        }

        public void ExtraProcess()
        {
            JObject item = JObject.Parse(strOutData);
            
            switch (Send_Command)
            {
                case "AT#I2CRD=9,10,31,28,1":
                    PushMoreData("Gsensor_X_L", item["I2CRD"].ToString());
                    break;

                case "AT#I2CRD=9,10,31,29,1":
                    PushMoreData("Gsensor_X_H", item["I2CRD"].ToString());
                    break;
 
                case "AT#I2CRD=9,10,31,2A,1":
                    PushMoreData("Gsensor_Y_L", item["I2CRD"].ToString());
                    break;

                case "AT#I2CRD=9,10,31,2B,1":
                    PushMoreData("Gsensor_Y_H", item["I2CRD"].ToString());
                    break;

                case "AT#I2CRD=9,10,31,2C,1":
                    PushMoreData("Gsensor_Z_L", item["I2CRD"].ToString());
                    break;

                case "AT#I2CRD=9,10,31,2D,1":
                    PushMoreData("Gsensor_Z_H", item["I2CRD"].ToString());
                    break;

            }
        }

    }
}
