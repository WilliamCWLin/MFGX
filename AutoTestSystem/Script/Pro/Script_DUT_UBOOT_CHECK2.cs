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

namespace AutoTestSystem.Script
{

    internal class Script_DUT_UBOOT_CHECK : ScriptDUTBase
    {
        public enum BOOT_ACTION
        {
            SendCMD,
            WaitForResponse,
            WaitForKeySend,
            SendWaitForResponse,
        }

        string strOutData = string.Empty;
        string strReceiveData = string.Empty;

        [Category("Common Parameters"), Description("支援用%%方式做變數值取代")]
        public string Send_Command { get; set; }

        [Category("Common Parameters"), Description("自訂顯示名稱")]
        public int ReadTimeOut { get; set; } = 10000;

        [Category("BOOT_ACTION"), Description("自訂顯示名稱")]
        public BOOT_ACTION ACTION_MODE { get; set; } = BOOT_ACTION.WaitForKeySend;

        [Category("Common Parameters"), Description("自訂顯示名稱")]
        public int TotalTimeOut { get; set; } = 10000;

        [Category("Common Parameters"), Description("自訂顯示名稱")]
        public string CheckCmd { get; set; }

        public override void Dispose()
        {
            //throw new NotImplementedException();
        }
        
        public override bool PreProcess()
        {

            strReceiveData = string.Empty;

            return true;
        }

        public override bool Process(DUT_BASE DUTDevice,ref string output)
        {
            DUTDevice.SetTimeout(ReadTimeOut, TotalTimeOut);
            string ReadOutput = string.Empty;
            string SendCmd = ReplaceProp(Send_Command);
            bool ret = false;
            try
            {
                var Jsondata = new Dictionary<string, object>
                {
                    { "STATUS", "FAIL" },  // 預設為 FAIL
                    { "READ", string.Empty },
                    { "SEND", SendCmd },
                    { "CHECK", CheckCmd }
                };
                switch (ACTION_MODE)
                {
                    case BOOT_ACTION.SendCMD:

                        ret = DUTDevice.SEND(SendCmd);

                        break;
                    case BOOT_ACTION.WaitForKeySend:

                        ret = DUTDevice.READ(CheckCmd, ref ReadOutput);
                        if (ret)
                            DUTDevice.SEND(SendCmd);

                        Jsondata["READ"] = ReadOutput;

                        break;
                    case BOOT_ACTION.SendWaitForResponse:

                        ret = DUTDevice.SEND(SendCmd);

                        ret &= DUTDevice.READ(CheckCmd, ref ReadOutput);
                   
                        Jsondata["READ"] = ReadOutput;

                        break;
                    case BOOT_ACTION.WaitForResponse:
                        ret = DUTDevice.READ(CheckCmd, ref ReadOutput);
                        Jsondata["READ"] = ReadOutput;

                        break;
                }

                if (ret)
                    Jsondata["STATUS"] = "PASS";
                else
                    Jsondata["STATUS"] = "FAIL";

                output = JsonConvert.SerializeObject(Jsondata,Formatting.Indented);
                strOutData = output;
                LogMessage(strOutData);
                return ret;
            }
            catch (Exception ex)
            {
                var data = new Dictionary<string, object>
                        {
                            { "STATUS", "FAIL" },
                            { "Exception", ex.Message }
                        };
                output = JsonConvert.SerializeObject(data, Formatting.Indented);

                strOutData = output;
                LogMessage(strOutData);
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
