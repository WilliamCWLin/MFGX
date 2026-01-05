using AutoTestSystem.Base;
using AutoTestSystem.Equipment.DosBase;
using Manufacture;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.UI.WebControls.WebParts;
using static AutoTestSystem.BLL.Bd;


namespace AutoTestSystem.Script
{
    internal class Script_HTTPMESCmd_Pro : Script_ControlDevice_Base
    {
        string strSPECstring = string.Empty;
        string strOutData = string.Empty;

        [Category("Common Parameters"), Description("自訂顯示名稱")]
        public string MESCmd { get; set; }

        [Category("Common Parameters"), Description("支援用%%方式做變數值取代")]
        public string Data { get; set; } = "%WorkID%;%ProductSN%;";

        [Category("Common Parameters"), Description("自訂顯示名稱")]
        public string CheckStr { get; set; } = "OK";

        [Category("Common Parameters"), Description("自訂顯示名稱")]
        public string PrefixStr { get; set; }

        public override void Dispose()
        {
            //throw new NotImplementedException();
        }

        public override bool PreProcess()
        {
            strSPECstring = string.Empty;
            strOutData = string.Empty;
            if (MESCmd == null || MESCmd == string.Empty)
            {
                LogMessage("MESCmd can not be null.", MessageLevel.Error);
                return false;
            }

            if (Data == null || Data == string.Empty)
            {
                LogMessage("Data can not be null.", MessageLevel.Error);
                return false;
            }

            if (CheckStr == null || CheckStr == string.Empty)
            {
                LogMessage("CheckStr can not be null.", MessageLevel.Error);
                return false;
            }

            return true;
        }

        public override bool Process(ControlDeviceBase Cmd, ref string output)
        {
            string Failitem = PopMoreData("Failitem");

            //Cmd.SetCheckstr(ReplaceProp(CheckStr));
            //Cmd.SetMEStcmd(MESCmd);
            //MESDataprocess(string MEStCMD, string MEStData, string checkStr, ref string strOutData)
            Boolean result = false;
            switch (MESCmd)
            {
                case "C003":
                    if (Failitem == string.Empty)
                        result = Cmd.MESDataprocess(MESCmd, $"{ReplaceProp(Data)}OK", ReplaceProp(CheckStr), ref output);
                    else
                        result = Cmd.MESDataprocess(MESCmd, $"{ReplaceProp(Data)}NG;{Failitem}", ReplaceProp(CheckStr), ref output);
                    break;

                case "C004":
                    try
                    {
                        if (Failitem == string.Empty)
                        {
                            string meta = ReplaceProp("%METALOGPATH%").Replace("%CheckFailitem%", "PASS");
                            string testlog = ReplaceProp("%TESTLOGPATH%");

                            if(!string.IsNullOrEmpty(meta))
                                MESUpload("METALOGPATH", ReplaceProp(meta), "PASS");
                            if (!string.IsNullOrEmpty(testlog))
                                MESUpload("TESTLOGPATH", ReplaceProp(testlog), "PASS");

                            //if (RetryCount == 0)
                            //    HandleDevice?.DutDashboard.TestSummary(HandleDevice, true);
                        }
                        else
                        {
                            string meta = ReplaceProp("%METALOGPATH%").Replace("%CheckFailitem%", "FAIL");
                            string testlog = ReplaceProp("%TESTLOGPATH%");
                            if (!string.IsNullOrEmpty(meta))
                                MESUpload("METALOGPATH", ReplaceProp(meta), "PASS");
                            if (!string.IsNullOrEmpty(testlog))
                                MESUpload("TESTLOGPATH", ReplaceProp(testlog), "PASS");

                            //if (RetryCount == 0)
                            //    HandleDevice?.DutDashboard.TestSummary(HandleDevice, false);
                        }
                    }
                    catch { }

                    result = Cmd.MESDataprocess(MESCmd,$"{ReplaceProp(Data)}{PopALLMESData()}", ReplaceProp(CheckStr), ref output);
                    break;

                default:
                    result =  Cmd.MESDataprocess(MESCmd,ReplaceProp(Data), ReplaceProp(CheckStr), ref output);
                    break;
            }

            LogMessage($"Send: {MESCmd}\nDATA:{ReplaceProp(Data)}",MessageLevel.Info);
            if (result != true)
            {
                return false;
            }

           // Cmd.READ(ref output);
 /*           switch (MESCmd)
            {
                case "C022":
                    string Dome_type = string.Empty;
                    string Product_type = string.Empty;
                    
                    if(output.Contains("Indoor") == true)
                    {
                        Dome_type = "Indoor"
                    }
                    else if(output.Contains("Outdoor") == true)
                    {
                        Dome_type = "Outdoor"
                    }
                    else
                    {
                        Dome_type = "Unknown"
                    }
                    if(output.Contains("5005") == true)
                    {
                        Product_type = "VD5005"
                    }
                    else if(output.Contains("5006") == true)
                    {
                        Product_type = "5006"
                    }
                    else
                    {
                        Product_type = "Unknown"
                    }
                    var typedata = new Dictionary<string, object>
                    {
                        { "STATUS", "PASS" }
                    };
                    typedata.Add("Dome_type", Dome_type);
                    typedata.Add("Product_type", Product_type);
                    output = CreateDataString(typedata);

                    LogMessage($"Read END: {output}");
                    break;
                
        
            }*/
            LogMessage($"Read END: {output}");
            LogMessage($"Checkstring: {ReplaceProp(CheckStr)}");
            strOutData = output;

            return true;
        }

        public override bool PostProcess()
        {
            string result = CheckRule(strOutData, Spec);

            if (result == "PASS" || Spec == "")
            {
                ExtraProcess();
                return true;
            }
            else
                return false;

        }

        public void ExtraProcess()
        {
            JObject jsonOutData = JObject.Parse(strOutData);
            string data = jsonOutData["data"].ToString();

            switch (MESCmd)
            {
                case "C022":
                    if (Data.Contains("SPEC1") == true)
                    {
                        strSPECstring = string.Empty;
                        string tmpSPEC = data.Split(';')[1];
                        strSPECstring = tmpSPEC;
                        PushMoreData("MES_SPEC_DATA", strSPECstring);
                        PushMoreData("MES_SPEC1_DATA", strSPECstring);
                        LogMessage($"MES Data: SPEC1 -> {strSPECstring}", MessageLevel.Info);
                    }

                    if (Data.Contains("SPEC2") == true)
                    {
                        strSPECstring = PopMoreData("MES_SPEC_DATA");
                        LogMessage($"POP MES Data: SPEC1 -> {strSPECstring}", MessageLevel.Info);
                        string tmpSPEC = data.Split(';')[1];
                        strSPECstring = strSPECstring + tmpSPEC;
                        LogMessage($"MES Data: SPEC1 + SPEC2 -> {strSPECstring}", MessageLevel.Info);
                        PushMoreData("MES_SPEC2_DATA", tmpSPEC);
                        PushMoreData("MES_SPEC_DATA_ALL", strSPECstring);
                        if (strSPECstring.Contains("VD5005") == true)
                        {
                            PushMoreData("PROUCT_TYPE","VD5005");
                            LogMessage($"Push Data: PROUCT_TYPE -> VD5005", MessageLevel.Info);
                        }
                        else if(strSPECstring.Contains("VD5006") == true)
                        {
                            PushMoreData("PROUCT_TYPE","VD5006");
                            LogMessage($"Push Data: PROUCT_TYPE -> VD5006", MessageLevel.Info);
                        }
                        else if(strSPECstring.Contains("VD5001") == true)
                        {
                            PushMoreData("PROUCT_TYPE","VD5001");
                            LogMessage($"Push Data: PROUCT_TYPE -> VD5001", MessageLevel.Info);
                        }
                        else
                        {
                            PushMoreData("PROUCT_TYPE","Unknown");
                            LogMessage($"Push Data: PROUCT_TYPE -> Unknown", MessageLevel.Info);
                        }

                        if((strSPECstring.Contains("Indoor") == true)||(strSPECstring.Contains("_IN_") == true))
                        {
                            PushMoreData("DOME_TYPE","Indoor");
                            LogMessage($"Push Data: DOME_TYPE -> Indoor", MessageLevel.Info);
                        }
                        else if((strSPECstring.Contains("Outdoor") == true)|| (strSPECstring.Contains("_OUT_") == true))
                        {
                            PushMoreData("DOME_TYPE","Outdoor");
                            LogMessage($"Push Data: DOME_TYPE -> Outdoor", MessageLevel.Info);
                        }
                        else
                        {
                            PushMoreData("DOME_TYPE","Unknown");
                            LogMessage($"Push Data: DOME_TYPE -> Unknown", MessageLevel.Info);
                        }
                       
                        if (strSPECstring.Contains("128/0") == true)
                        {
                            PushMoreData("SD1_SIZE","128");
                            LogMessage($"Push Data: SD1_SIZE -> 128", MessageLevel.Info);
                            PushMoreData("SD2_SIZE", "0");
                            LogMessage($"Push Data: SD2_SIZE -> 0", MessageLevel.Info);
                        }
                        else if (strSPECstring.Contains("128/128") == true)
                        {
                            PushMoreData("SD1_SIZE", "128");
                            LogMessage($"Push Data: SD1_SIZE -> 128", MessageLevel.Info);
                            PushMoreData("SD2_SIZE", "128");
                            LogMessage($"Push Data: SD2_SIZE -> 128", MessageLevel.Info);
                        }
                        else if (strSPECstring.Contains("256/0") == true)
                        {
                            PushMoreData("SD1_SIZE", "256");
                            LogMessage($"Push Data: SD1_SIZE -> 256", MessageLevel.Info);
                            PushMoreData("SD2_SIZE", "0");
                            LogMessage($"Push Data: SD2_SIZE -> 0", MessageLevel.Info);
                        }
                        else if (strSPECstring.Contains("256/128") == true)
                        {
                            PushMoreData("SD1_SIZE", "256");
                            LogMessage($"Push Data: SD1_SIZE -> 256", MessageLevel.Info);
                            PushMoreData("SD2_SIZE", "128");
                            LogMessage($"Push Data: SD2_SIZE -> 128", MessageLevel.Info);
                        }
                        else if (strSPECstring.Contains("256/256") == true)
                        {
                            PushMoreData("SD1_SIZE", "256");
                            LogMessage($"Push Data: SD1_SIZE -> 256", MessageLevel.Info);
                            PushMoreData("SD2_SIZE", "256");
                            LogMessage($"Push Data: SD2_SIZE -> 256", MessageLevel.Info);
                        }
                        else if (strSPECstring.Contains("512/0") == true)
                        {
                            PushMoreData("SD1_SIZE", "512");
                            LogMessage($"Push Data: SD1_SIZE -> 512", MessageLevel.Info);
                            PushMoreData("SD2_SIZE", "0");
                            LogMessage($"Push Data: SD2_SIZE -> 0", MessageLevel.Info);
                        }
                        else if (strSPECstring.Contains("512/128") == true)
                        {
                            PushMoreData("SD1_SIZE", "512");
                            LogMessage($"Push Data: SD1_SIZE -> 512", MessageLevel.Info);
                            PushMoreData("SD2_SIZE", "128");
                            LogMessage($"Push Data: SD2_SIZE -> 128", MessageLevel.Info);
                        }
                        else if (strSPECstring.Contains("512/256") == true)
                        {
                            PushMoreData("SD1_SIZE", "512");
                            LogMessage($"Push Data: SD1_SIZE -> 512", MessageLevel.Info);
                            PushMoreData("SD2_SIZE", "256");
                            LogMessage($"Push Data: SD2_SIZE -> 256", MessageLevel.Info);
                        }
                        else if (strSPECstring.Contains("512/512") == true)
                        {
                            PushMoreData("SD1_SIZE", "512");
                            LogMessage($"Push Data: SD1_SIZE -> 512", MessageLevel.Info);
                            PushMoreData("SD2_SIZE", "512");
                            LogMessage($"Push Data: SD2_SIZE -> 512", MessageLevel.Info);
                        }
                        else if ((strSPECstring.Contains("1TB/0") == true)||(strSPECstring.Contains("1T/0") == true))
                        {
                            PushMoreData("SD1_SIZE", "1024");
                            LogMessage($"Push Data: SD1_SIZE -> 1024", MessageLevel.Info);
                            PushMoreData("SD2_SIZE", "0");
                            LogMessage($"Push Data: SD2_SIZE -> 0", MessageLevel.Info);
                        }
                        else if ((strSPECstring.Contains("1TB/1TB") == true)||(strSPECstring.Contains("1T/1T") == true))
                        {
                            PushMoreData("SD1_SIZE", "1024");
                            LogMessage($"Push Data: SD1_SIZE -> 1024", MessageLevel.Info);
                            PushMoreData("SD2_SIZE", "1024");
                            LogMessage($"Push Data: SD2_SIZE -> 1024", MessageLevel.Info);
                        }
                        else if ((strSPECstring.Contains("1.5TB/1.5TB") == true) || (strSPECstring.Contains("1.5T/1.5T") == true))
                        {
                            PushMoreData("SD1_SIZE", "1536");
                            LogMessage($"Push Data: SD1_SIZE -> 1536", MessageLevel.Info);
                            PushMoreData("SD2_SIZE", "1536");
                            LogMessage($"Push Data: SD2_SIZE -> 1536", MessageLevel.Info);
                        }
                        else
                        {
                            PushMoreData("SD_SIZE","Unknown");
                            LogMessage($"Push Data: SD_SIZE -> Unknown", MessageLevel.Info);
                        }

                    }

                    if (Data.Contains("FW_VERSION") == true)
                    {
                        string MES_FW_VERSION = data.Split(';')[1];
                        PushMoreData("MES_FW_VERSION", MES_FW_VERSION);
                    }

                    if (Data.Contains("CUSTOMER_VERSION") == true)
                    {
                        string MES_CUSTOMER_VERSION = data.Split(';')[1];
                        PushMoreData("MES_CUSTOMER_VERSION", MES_CUSTOMER_VERSION);
                    }

                    if (Data.Contains("CONFIG_NO") == true)
                    {
                        string MES_CONFIG_NO = data.Split(';')[1];
                        PushMoreData("MES_CONFIG_NO", MES_CONFIG_NO);
                    }

                    if (Data.Contains("PCBA_VERSION") == true)
                    {
                        string MES_PCBA_VERSION = data.Split(';')[1];
                        PushMoreData("MES_PCBA_VERSION", MES_PCBA_VERSION);
                    }

                    break;

                case "C038":
                    if (Data.Contains("KEYPART SN") == true)
                    {
                        /*
                        string MES_KEYPART_SN = data.Split(';')[1];
                        PushMoreData("MES_KEYPART_SN", MES_KEYPART_SN);
                        LogMessage($"Push Data: MES_KEYPART_SN ->{MES_KEYPART_SN}", MessageLevel.Info);
                        */

                        /*
                        string[] MES_KEYPART_SN_LIST = data.Split(';');
                        string MES_KEYPART_NAME = string.Empty;
                        int keypart_count = 0;
                        foreach (string keypart in MES_KEYPART_SN_LIST)
                        {
                            
                            if (keypart_count == 0)
                            {
                                keypart_count++;
                                continue;
                            }
                            else if (keypart_count == 1)
                            {
                                PushMoreData("MES_KEYPART_SN", keypart);
                                LogMessage($"Push Data: MES_KEYPART_SN ->{keypart}", MessageLevel.Info);
                            }
                            else{
                                MES_KEYPART_NAME = "MES_KEYPART_SN" + keypart_count;
                                PushMoreData(MES_KEYPART_NAME, keypart);
                                LogMessage($"Push Data: {MES_KEYPART_NAME}->{keypart}", MessageLevel.Info);
                            }
                            keypart_count++;
                        }
                        */
                       
                        
                        string[] MES_KEYPART_SN = data.Split(';');
                        LogMessage($"Prefix : {PrefixStr}", MessageLevel.Info);


                        foreach (string keypart in MES_KEYPART_SN)
                        {
                            if(keypart.Contains(PrefixStr) == true && keypart.Length>0)
                            {
                                PushMoreData("MES_KEYPART_SN", keypart);
                                LogMessage($"Push Prefix Data: MES_KEYPART_SN ->{keypart}", MessageLevel.Info);
                                break;
                            }
                            else
                            {
                                PushMoreData("MES_KEYPART_SN", keypart);
                                LogMessage($"Push Data: MES_KEYPART_SN ->{keypart}", MessageLevel.Info);
                            }
                        }                      
                       
                    }

                    if (Data.Contains("Customer SN") == true)
                    {
                        string MES_CUSTOMER_SN = data.Split(';')[1];
                        PushMoreData("MES_CUSTOMER_SN", MES_CUSTOMER_SN);
                    }

                    if (Data.Contains("MAC") == true)
                    {
                        string MES_MAC = data.Split(';')[1];
                        PushMoreData("MES_MAC", MES_MAC);
                    }

                    break;
                case "C045":

                    string[] parts = data.Split(';');
                    if (parts.Length > 1)
                    {
                        string MES_SN = parts[1];
                        PushMoreData("MES_SN", MES_SN);
                        LogMessage($"Push Data: MES_SN ->{MES_SN}",MessageLevel.Info);
                    }
                    
                    break;

                case "VN014":
                case "VN021":

                    string[] IP_parts = data.Split(';');
                    if (IP_parts.Length > 1)
                    {
                        string STATIC_IP = IP_parts[1];
                        PushMoreData("STATIC_IP", STATIC_IP);
                        LogMessage($"Push Data: STATIC_IP ->{STATIC_IP}", MessageLevel.Info);
                    }

                    break;
                case "C029":
                    int count = 0;
                    //string[] C029_respond = data.Split(';');
                    //if (C029_respond.Length > 1)
                    {
                        string[] C029_Data = data.Split(',');
                        //string[] C029_Data = C029_respond[1].Split(',');
                        if (C029_Data.Length > 1)
                        {
                            foreach (string C029_RESULT in C029_Data)
                            {
                                string C029_str = "RESULT" + count;
                                PushMoreData(C029_str, C029_RESULT);
                                LogMessage($"Push Data: C029_RESULT ->{C029_RESULT}", MessageLevel.Info);
                                count++;
                            }

                        }

                    }

                    break;
                case "VN020":
                    string MES_SAFETY_CODE = data.Split(';')[1];
                    PushMoreData("MES_SAFETY_CODE", MES_SAFETY_CODE);
                    break;
            }
        }
    }
}