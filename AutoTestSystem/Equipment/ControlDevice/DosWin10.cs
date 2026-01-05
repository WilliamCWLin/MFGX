using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AutoTestSystem.DAL;
using static AutoTestSystem.BLL.Bd;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using AutoTestSystem.Base;
using Newtonsoft.Json.Linq;
using OpenCvSharp;

namespace AutoTestSystem.Equipment.ControlDevice
{
    class DosWin10: ControlDeviceBase
    {
        DosCmd doscmd = new DosCmd();
        int waitTime = 0;
       
       
        
        public override bool SendNonblock(string input ,ref string output)
        {
            bool result = doscmd.SendNonBlockCommand(input, waitTime);
            var data = new Dictionary<string, object>
                {
                    { "STATUS", "PASS" }
                };
            output = CreateDataString(data);
            return true;
        }
        public override bool MultiSend_Read(string input,string strActItem, string checkStr, int waitTime,ref string strOutData)
        {
            string pattern = string.Empty;
            string recvStr = string.Empty;
            bool result = doscmd.SendCommand(input, ref recvStr, checkStr, waitTime);
            Logger.Info($" strActItem  {strActItem} ");
            if (result == true)
            {
                switch(strActItem)
                {
                    case "audioAnalyse":
                        string[] strResult = Regex.Split(recvStr, "\r\n");
                        List<string> hzValue = new List<string>();
                        List<int> dBValue = new List<int>();
                        foreach (var lineitem in strResult)
                        {
                            if(lineitem.Contains("dB"))
                            {
                                pattern = @"Detected peak at (\d+\.\d+) Hz of (\d+\.\d+) dB";
                                Match match = Regex.Match(lineitem, pattern);
                                if (match.Success)
                                {
                                    int tmp_hzValue = Convert.ToInt32(double.Parse(match.Groups[1].Value));
                                    hzValue.Add("Hz_" + tmp_hzValue);

                                    int tmp_dBValue = Convert.ToInt32(double.Parse(match.Groups[2].Value));
                                    dBValue.Add(tmp_dBValue);
                                }
                            }
                        }
                        var audio_data = new Dictionary<string, object>
                        {
                                { "STATUS", "PASS" }
                        };
                        for (int item = 0; item< hzValue.Count; item++ )
                        {
                            audio_data.Add(hzValue[item], dBValue[item]);
                        }
                        strOutData = CreateDataString(audio_data);
                    break;
                    
                    case "AnalyseDB":
                        int headerCount = 0;
                        int tailCount = 0;
                        foreach (char c in recvStr)
                        {
                            if (c == '{') headerCount++;
                            if (c == '}') tailCount++;
                        }
                        
                        if ((headerCount == 0) || (tailCount == 0))
                        {
                            var faildata = new Dictionary<string, object>
                            {
                                { "STATUS", "FAIL" },
                                {" Respond", recvStr}
                            };
                            strOutData = CreateDataString(faildata);
                            return false;
                        }

                        recvStr = Regex.Replace(recvStr, @"[\r\n]", "");
                        recvStr = Regex.Replace(recvStr, @"[\r]", "");
                        recvStr = Regex.Replace(recvStr, @"[\n]", "");
                        recvStr = Regex.Replace(recvStr, @"[\t]", "");
                        int header = recvStr.IndexOf("{");
                        int tail = recvStr.LastIndexOf("}");
                        if (header == -1 || tail == -1)
                        {
                            var faildata = new Dictionary<string, object>
                            {
                                { "STATUS", "FAIL" },
                                {" Respond", recvStr}
                            };
                            strOutData = CreateDataString(faildata);
                            return false;
                        }
                  
                        int capture_lehgth = tail - header + 1;
                        recvStr = recvStr.Substring(header, capture_lehgth);
                        Logger.Info($" recvStr  {recvStr} ");

                        strOutData = recvStr;
                    break;
                    
                    case "LedCountRatio":
                        string[] ledstrResult = Regex.Split(recvStr, "\r");
                        int count = 0;
                        List<string> BPName = new List<string>();
                        List<float> BPValue = new List<float>();
                        List<string> AreaName = new List<string>();
                        List<float> AreaValue = new List<float>();
                        List<string> MRName = new List<string>();
                        List<float> MRValue = new List<float>();
                        List<string> MGName = new List<string>();
                        List<float> MGValue = new List<float>();
                        List<string> MBName = new List<string>();
                        List<float> MBValue = new List<float>();
                        List<string> BGName = new List<string>();
                        List<float> BGValue = new List<float>();
                        List<string> RGName = new List<string>();
                        List<float> RGValue = new List<float>();
                        List<string> WidthName = new List<string>();
                        List<float> WidthValue = new List<float>();
                        List<string> HeightName = new List<string>();
                        List<float> HeightValue = new List<float>();
                        List<string> ContourCountName = new List<string>();
                        List<float> ContourCountValue = new List<float>();
                        List<string> ContourWidthName = new List<string>();
                        List<float> ContourWidthValue = new List<float>();
                        List<string> ContourHeightName = new List<string>();
                        List<float> ContourHeightValue = new List<float>();
                        List<string> ContourAreaName = new List<string>();
                        List<float> ContourAreaValue = new List<float>();
                        List<string> HypotenuseName = new List<string>();
                        List<float> HypotenuseValue = new List<float>();
                        List<string> Uniformity_X_Name = new List<string>();
                        List<string> Uniformity_Y_Name = new List<string>();
                        List<float> Uniformity_X_Value = new List<float>();
                        List<float> Uniformity_Y_Value = new List<float>();

                        foreach (var lineitem in ledstrResult)
                        {
                            if(lineitem.Contains("DetectCount"))
                            {
                                string[] ledCount = Regex.Split(lineitem, "=");
                                count = Convert.ToInt32(ledCount[1]);
                            }

                            if (lineitem.Contains("Area"))
                            {
                                string[] Areadata = Regex.Split(lineitem, "=");
                                AreaName.Add("Area_" + (AreaName.Count + 1));
                                float tmp_AreaValue = (float)Convert.ToDouble(Areadata[1]);
                                AreaValue.Add(tmp_AreaValue);
                            }

                            if (lineitem.Contains("BrighterPixel_MeanG"))
                            {
                                string[] BPdata = Regex.Split(lineitem, "=");
                                BPName.Add("BrighterPixel_MeanG_"+(BPName.Count+1));
                                float tmp_BPValue = (float) Convert.ToDouble(BPdata[1]);
                                BPValue.Add(tmp_BPValue);
                            }

                            if (lineitem.Contains("Sample_MeanR"))
                            {
                                string[] MRdata = Regex.Split(lineitem, "=");
                                MRName.Add("Sample_MeanR_" + (MRName.Count + 1));
                                float tmp_MRValue = (float)Convert.ToDouble(MRdata[1]);
                                MRValue.Add(tmp_MRValue);
                            }

                            if (lineitem.Contains("Sample_MeanG"))
                            {
                                string[] MGdata = Regex.Split(lineitem, "=");
                                MGName.Add("Sample_MeanG_" + (MGName.Count + 1));
                                float tmp_MGValue = (float)Convert.ToDouble(MGdata[1]);
                                MGValue.Add(tmp_MGValue);
                            }

                            if (lineitem.Contains("Sample_MeanB"))
                            {
                                string[] MBdata = Regex.Split(lineitem, "=");
                                MBName.Add("Sample_MeanB_" + (MBName.Count + 1));
                                float tmp_MBValue = (float)Convert.ToDouble(MBdata[1]);
                                MBValue.Add(tmp_MBValue);
                            }

                            if (lineitem.Contains("Sample_BG_Ratio"))
                            {
                                string[] BGdata = Regex.Split(lineitem, "=");
                                BGName.Add("Sample_BG_Ratio_"+(BGName.Count+1));
                                float tmp_BGValue = (float) Convert.ToDouble(BGdata[1]);
                                BGValue.Add(tmp_BGValue);
                            }

                            if(lineitem.Contains("Sample_RG_Ratio"))
                            {
                                string[] RGdata = Regex.Split(lineitem, "=");
                                RGName.Add("Sample_RG_Ratio_"+(RGName.Count+1));
                                float tmp_RGValue = (float) Convert.ToDouble(RGdata[1]);
                                RGValue.Add(tmp_RGValue);
                            }

                            if (lineitem.Contains("Rect_Width"))
                            {
                                string[] Widthdata = Regex.Split(lineitem, "=");
                                WidthName.Add("Rect_Width_" + (WidthName.Count + 1));
                                float tmp_Widthdata = (float)Convert.ToDouble(Widthdata[1]);
                                WidthValue.Add(tmp_Widthdata);
                            }

                            if (lineitem.Contains("Rect_Height"))
                            {
                                string[] Heightdata = Regex.Split(lineitem, "=");
                                HeightName.Add("Rect_Height_" + (HeightName.Count + 1));
                                float tmp_Heightdata = (float)Convert.ToDouble(Heightdata[1]);
                                HeightValue.Add(tmp_Heightdata);
                            }

                            if (lineitem.Contains("ContourCount"))
                            {
                                string[] ContourCountdata = Regex.Split(lineitem, "=");
                                ContourCountName.Add("ContourCount_" + (ContourCountName.Count + 1));
                                float tmp_ContourCountdata = (float)Convert.ToDouble(ContourCountdata[1]);
                                ContourCountValue.Add(tmp_ContourCountdata);
                            }

                            if (lineitem.Contains("Contour_Width"))
                            {
                                string[] ContourWidthdata = Regex.Split(lineitem, "=");
                                ContourWidthName.Add("ContourWidth_" + (ContourWidthName.Count + 1));
                                float tmp_ContourWidthdata = (float)Convert.ToDouble(ContourWidthdata[1]);
                                ContourWidthValue.Add(tmp_ContourWidthdata);
                            }

                            if (lineitem.Contains("Contour_Height"))
                            {
                                string[] ContourHeightdata = Regex.Split(lineitem, "=");
                                ContourHeightName.Add("ContourHeight_" + (ContourHeightName.Count + 1));
                                float tmp_ContourHeightdata = (float)Convert.ToDouble(ContourHeightdata[1]);
                                ContourHeightValue.Add(tmp_ContourHeightdata);
                            }

                            if (lineitem.Contains("Contour_Area"))
                            {
                                string[] ContourAreadata = Regex.Split(lineitem, "=");
                                ContourAreaName.Add("ContourArea_" + (ContourAreaName.Count + 1));
                                float tmp_ContourAreadata = (float)Convert.ToDouble(ContourAreadata[1]);
                                ContourAreaValue.Add(tmp_ContourAreadata);
                            }

                            if (lineitem.Contains("Hypotenuse"))
                            {
                                string[] Hypotenusedata = Regex.Split(lineitem, "=");
                                HypotenuseName.Add("Hypotenuse_" + (HypotenuseName.Count + 1));
                                float tmp_Hypotenusedata = (float)Convert.ToDouble(Hypotenusedata[1]);
                                HypotenuseValue.Add(tmp_Hypotenusedata);
                            }

                            if (lineitem.Contains("Uniformity_X"))
                            {
                                string[] Uniformity_Xdata = Regex.Split(lineitem, "=");
                                Uniformity_X_Name.Add("Uniformity_X_" + (Uniformity_X_Name.Count + 1));
                                float tmp_Uniformity_Xdata = (float)Convert.ToDouble(Uniformity_Xdata[1]);
                                Uniformity_X_Value.Add(tmp_Uniformity_Xdata);
                            }

                            if (lineitem.Contains("Uniformity_Y"))
                            {
                                string[] Uniformity_Ydata = Regex.Split(lineitem, "=");
                                Uniformity_Y_Name.Add("Uniformity_Y_" + (Uniformity_Y_Name.Count + 1));
                                float tmp_Uniformity_Ydata = (float)Convert.ToDouble(Uniformity_Ydata[1]);
                                Uniformity_Y_Value.Add(tmp_Uniformity_Ydata);
                            }

                        }
                        
                        var ledCountdata = new Dictionary<string, object>
                        {
                            { "STATUS", "PASS" }
                        };
                        ledCountdata.Add("DetectCount", count);

                        for (int item = 0; item < AreaName.Count; item++)
                        {
                            ledCountdata.Add(AreaName[item], AreaValue[item]);
                        }

                        for (int item = 0; item< BPName.Count; item++ )
                        {
                            ledCountdata.Add(BPName[item], BPValue[item]);      
                        }

                        for (int item = 0; item < MRName.Count; item++)
                        {
                            ledCountdata.Add(MRName[item], MRValue[item]);
                        }

                        for (int item = 0; item < MGName.Count; item++)
                        {
                            ledCountdata.Add(MGName[item], MGValue[item]);
                        }

                        for (int item = 0; item < MBName.Count; item++)
                        {
                            ledCountdata.Add(MBName[item], MBValue[item]);
                        }

                        for (int item = 0; item< BGName.Count; item++ )
                        {
                            ledCountdata.Add(BGName[item], BGValue[item]);   
                        }

                        for (int item = 0; item< RGName.Count; item++ )
                        {
                            ledCountdata.Add(RGName[item], RGValue[item]);   
                        }

                        for (int item = 0; item < WidthName.Count; item++)
                        {
                            ledCountdata.Add(WidthName[item], WidthValue[item]);
                        }

                        for (int item = 0; item < HeightName.Count; item++)
                        {
                            ledCountdata.Add(HeightName[item], HeightValue[item]);
                        }

                        for (int item = 0; item < ContourCountName.Count; item++)
                        {
                            ledCountdata.Add(ContourCountName[item], ContourCountValue[item]);
                        }

                        for (int item = 0; item < ContourWidthName.Count; item++)
                        {
                            ledCountdata.Add(ContourWidthName[item], ContourWidthValue[item]);
                        }

                        for (int item = 0; item < ContourHeightName.Count; item++)
                        {
                            ledCountdata.Add(ContourHeightName[item], ContourHeightValue[item]);
                        }

                        for (int item = 0; item < ContourAreaName.Count; item++)
                        {
                            ledCountdata.Add(ContourAreaName[item], ContourAreaValue[item]);
                        }

                        for (int item = 0; item < HypotenuseName.Count; item++)
                        {
                            ledCountdata.Add(HypotenuseName[item], HypotenuseValue[item]);
                        }

                        for (int item = 0; item < Uniformity_X_Name.Count; item++)
                        {
                            ledCountdata.Add(Uniformity_X_Name[item], Uniformity_X_Value[item]);
                        }

                        for (int item = 0; item < Uniformity_Y_Name.Count; item++)
                        {
                            ledCountdata.Add(Uniformity_Y_Name[item], Uniformity_Y_Value[item]);
                        }

                        strOutData = CreateDataString(ledCountdata);

                    break;

                    case "LedIntensity":
                        string[] intensitystrResult = Regex.Split(recvStr, "\r");
                        float b_luminance = 0;
                        float g_luminance = 0;
                        float r_luminance = 0;
                        float gray_luminance = 0;
                        int b_max = 0;
                        int g_max = 0;
                        int r_max = 0;
                        int gray_max = 0;
                        
                        foreach (var lineitem in intensitystrResult)
                        {
                            //Logger.Info(lineitem);
                            if(lineitem.Contains("Check_luminance_Blue"))
                            {
                                string[] b_lum_str = Regex.Split(lineitem, "=");
                                b_luminance = (float)Convert.ToDouble(b_lum_str[1]);
                            }
                            
                            if(lineitem.Contains("Check_luminance_Green"))
                            {
                                string[] g_lum_str = Regex.Split(lineitem, "=");
                                g_luminance = (float) Convert.ToDouble(g_lum_str[1]);
                            }
                            
                            if(lineitem.Contains("Check_luminance_Red"))
                            {
                                string[] r_lum_str = Regex.Split(lineitem, "=");
                                r_luminance = (float) Convert.ToDouble(r_lum_str[1]);
                            }
                            
                            if(lineitem.Contains("Check_luminance_Gray"))
                            {
                                string[] gray_lum_str = Regex.Split(lineitem, "=");
                                gray_luminance = (float) Convert.ToDouble(gray_lum_str[1]);
                            }
                            
                            if(lineitem.Contains("Max_Value_Blue"))
                            {
                                string[] max_b_str = Regex.Split(lineitem, "=");
                                b_max = Convert.ToInt32(max_b_str[1]);
                            }
                            
                            if(lineitem.Contains("Max_Value_Green"))
                            {
                                string[] max_g_str = Regex.Split(lineitem, "=");
                                g_max = Convert.ToInt32(max_g_str[1]);
                            }
                            
                            if(lineitem.Contains("Max_Value_Red"))
                            {
                                string[] max_r_str = Regex.Split(lineitem, "=");
                                r_max = Convert.ToInt32(max_r_str[1]);
                            }
                            
                            if(lineitem.Contains("Max_Value_Gray"))
                            {
                                string[] max_gray_str = Regex.Split(lineitem, "=");
                                gray_max = Convert.ToInt32(max_gray_str[1]);
                            }

                        }
                        var intensitydata = new Dictionary<string, object>
                        {
                            { "STATUS", "PASS" }
                        };
                        intensitydata.Add("Check_luminance_Blue", b_luminance);
                        intensitydata.Add("Check_luminance_Green", g_luminance);
                        intensitydata.Add("Check_luminance_Red", r_luminance);
                        intensitydata.Add("Check_luminance_Gray", gray_luminance);
                        intensitydata.Add("Max_Value_Blue", b_max);
                        intensitydata.Add("Max_Value_Green", g_max);
                        intensitydata.Add("Max_Value_Red", r_max);
                        intensitydata.Add("Max_Value_Gray", gray_max);
                        
                        strOutData = CreateDataString(intensitydata);
                    break;
                    case "Camera_DarkCornor":
                        LogMessage($"Camera_DarkCornor recvStr  {recvStr} ");
                        string[] DCRResult = Regex.Split(recvStr, "\r\n");
                        float RI_TL_Gr = 0;
                        float RI_TR_Gr = 0;
                        float RI_BL_Gr = 0;
                        float RI_BR_Gr = 0;
                        

                        foreach (var lineitem in DCRResult)
                        {
                            LogMessage($"lineitem   {lineitem} ");
                            if (lineitem.Contains("RI_TL_Gr"))
                            {
                                string[] RI_TL_Gr_str = Regex.Split(lineitem, "=");
                                RI_TL_Gr = (float)Convert.ToDouble(RI_TL_Gr_str[1]);
                            }

                            if (lineitem.Contains("RI_TR_Gr"))
                            {
                                string[] RI_TR_Gr_str = Regex.Split(lineitem, "=");
                                RI_TR_Gr = (float)Convert.ToDouble(RI_TR_Gr_str[1]);
                            }

                            if (lineitem.Contains("RI_BL_Gr"))
                            {
                                string[] RI_BL_Gr_str = Regex.Split(lineitem, "=");
                                RI_BL_Gr = (float)Convert.ToDouble(RI_BL_Gr_str[1]);
                            }

                            if (lineitem.Contains("RI_BR_Gr"))
                            {
                                string[] RI_BR_Gr_str = Regex.Split(lineitem, "=");
                                RI_BR_Gr = (float)Convert.ToDouble(RI_BR_Gr_str[1]);
                            }


                        }

                        var DCRdata = new Dictionary<string, object>
                            {
                                { "STATUS", "PASS" }
                            };
                        DCRdata.Add("RI_TL_Gr", RI_TL_Gr);
                        DCRdata.Add("RI_TR_Gr", RI_TR_Gr);
                        DCRdata.Add("RI_BL_Gr", RI_BL_Gr);
                        DCRdata.Add("RI_BR_Gr", RI_BR_Gr);
                        

                        strOutData = CreateDataString(DCRdata);
                        break;
                    case "Camera_AWB":
                        LogMessage($"Camera_AWB recvStr  {recvStr} ");
                        string[] AWBResult = Regex.Split(recvStr, "\r\n");
                        float rgRatio_TL = 0;
                        float rgRatio_TR = 0;
                        float rgRatio_BL = 0;
                        float rgRatio_BR = 0;
                        float rgRatio_Center = 0;

                        foreach (var lineitem in AWBResult)
                        {
                            LogMessage($"lineitem   {lineitem} ");
                            if (lineitem.Contains("RG_Ratio_TL"))
                            {
                                string[] rgRatio_TL_str = Regex.Split(lineitem, "=");
                                rgRatio_TL = (float)Convert.ToDouble(rgRatio_TL_str[1]);
                            }

                            if (lineitem.Contains("RG_Ratio_TR"))
                            {
                                string[] rgRatio_TR_str = Regex.Split(lineitem, "=");
                                rgRatio_TR = (float)Convert.ToDouble(rgRatio_TR_str[1]);
                            }

                            if (lineitem.Contains("RG_Ratio_BL"))
                            {
                                string[] rgRatio_BL_str = Regex.Split(lineitem, "=");
                                rgRatio_BL = (float)Convert.ToDouble(rgRatio_BL_str[1]);
                            }

                            if (lineitem.Contains("RG_Ratio_BR"))
                            {
                                string[] rgRatio_BR_str = Regex.Split(lineitem, "=");
                                rgRatio_BR = (float)Convert.ToDouble(rgRatio_BR_str[1]);
                            }

                            if (lineitem.Contains("RG_Ratio_Center"))
                            {
                                string[] rgRatio_C_str = Regex.Split(lineitem, "=");
                                rgRatio_Center = (float)Convert.ToDouble(rgRatio_C_str[1]);
                            }

                        }

                        var AWBdata = new Dictionary<string, object>
                            {
                                { "STATUS", "PASS" }
                            };
                        AWBdata.Add("RG_Ratio_TL", rgRatio_TL);
                        AWBdata.Add("RG_Ratio_TR", rgRatio_TR);
                        AWBdata.Add("RG_Ratio_BL", rgRatio_BL);
                        AWBdata.Add("RG_Ratio_BR", rgRatio_BR);
                        AWBdata.Add("RG_Ratio_Center", rgRatio_Center);

                        strOutData = CreateDataString(AWBdata);
                        break;

                    case "Camera_IRIS":
                        LogMessage($"Camera_IRIS recvStr  {recvStr} ");
                        string[] IRISResult = Regex.Split(recvStr, "\r\n");
                        float Check_luminance_Gr = 0;

                        foreach (var lineitem in IRISResult)
                        {
                            LogMessage($"lineitem   {lineitem} ");
                            if (lineitem.Contains("Check_luminance_Gr"))
                            {
                                string[] Check_luminance_Gr_str = Regex.Split(lineitem, "=");
                                Check_luminance_Gr = (float)Convert.ToDouble(Check_luminance_Gr_str[1]);
                            }

                        }

                        var IRISdata = new Dictionary<string, object>
                            {
                                { "STATUS", "PASS" }
                            };
                        IRISdata.Add("Check_luminance_Gr", Check_luminance_Gr);

                        strOutData = CreateDataString(IRISdata);
                        break;
                    case "Camera_SFR":
                        LogMessage($"Camera_IRIS recvStr  {recvStr} ");
                        string[] SFRResult = Regex.Split(recvStr, "\r\n");
                        float SFR_TL_V = 0;
                        float SFR_TR_V = 0;
                        float SFR_BL_V = 0;
                        float SFR_BR_V = 0;
                        float SFR_SFR_CT_Average = 0;

                        foreach (var lineitem in SFRResult)
                        {
                            LogMessage($"lineitem   {lineitem} ");
                            if (lineitem.Contains("SFR_TL_V+H_Average_Field1"))
                            {
                                string[] SFR_TL_V_str = Regex.Split(lineitem, "=");
                                SFR_TL_V = (float)Convert.ToDouble(SFR_TL_V_str[1]);
                            }
                            if (lineitem.Contains("SFR_TR_V+H_Average_Field1"))
                            {
                                string[] SFR_TR_V_str = Regex.Split(lineitem, "=");
                                SFR_TR_V = (float)Convert.ToDouble(SFR_TR_V_str[1]);
                            }
                            if (lineitem.Contains("SFR_BL_V+H_Average_Field1"))
                            {
                                string[] SFR_BL_V_str = Regex.Split(lineitem, "=");
                                SFR_BL_V = (float)Convert.ToDouble(SFR_BL_V_str[1]);
                            }
                            if (lineitem.Contains("SFR_BR_V+H_Average_Field1"))
                            {
                                string[] SFR_BR_V_str = Regex.Split(lineitem, "=");
                                SFR_BR_V = (float)Convert.ToDouble(SFR_BR_V_str[1]);
                            }
                            if (lineitem.Contains("SFR_SFR_CT_Average"))
                            {
                                string[] SFR_SFR_CT_Average_str = Regex.Split(lineitem, "=");
                                SFR_SFR_CT_Average = (float)Convert.ToDouble(SFR_SFR_CT_Average_str[1]);
                            }

                        }

                        var SFRdata = new Dictionary<string, object>
                            {
                                { "STATUS", "PASS" }
                            };

                        SFRdata.Add("SFR_TL_V+H_Average_Field1", SFR_TL_V);
                        SFRdata.Add("SFR_TR_V+H_Average_Field1", SFR_TR_V);
                        SFRdata.Add("SFR_BL_V+H_Average_Field1", SFR_BL_V);
                        SFRdata.Add("SFR_BR_V+H_Average_Field1", SFR_BR_V);
                        SFRdata.Add("SFR_SFR_CT_Average", SFR_SFR_CT_Average);
                        strOutData = CreateDataString(SFRdata);
                        break;

                    case "Camera_DUST":
                        LogMessage($"Camera_DUST recvStr  {recvStr} ");
                        string[] DUSTResult = Regex.Split(recvStr, "\r\n");
                        float BRGHT_BLMSHS = 0;
                        

                        foreach (var lineitem in DUSTResult)
                        {
                            LogMessage($"lineitem   {lineitem} ");
                            if (lineitem.Contains("BRGHT_BLMSHS"))
                            {
                                string[] BRGHT_BLMSHS_str = Regex.Split(lineitem, "=");
                                BRGHT_BLMSHS = (float)Convert.ToDouble(BRGHT_BLMSHS_str[1]);
                            }

                        }

                        var Dustdata = new Dictionary<string, object>
                            {
                                { "STATUS", "PASS" }
                            };

                        Dustdata.Add("BRGHT_BLMSHS", BRGHT_BLMSHS);
                        strOutData = CreateDataString(Dustdata);
                        break;
                    case "Camera_DP_WF":
                        LogMessage($"Camera_DP_WF recvStr  {recvStr} ");
                        string[] DP_WFResult = Regex.Split(recvStr, "\r\n");
                        float BRGHT_DEFECTS = 0;
                        float BRGHT_CLSTRS = 0;


                        foreach (var lineitem in DP_WFResult)
                        {
                            LogMessage($"lineitem   {lineitem} ");
                            if (lineitem.Contains("BRGHT_DEFECTS"))
                            {
                                string[] BRGHT_DEFECTS_str = Regex.Split(lineitem, "=");
                                BRGHT_DEFECTS = (float)Convert.ToDouble(BRGHT_DEFECTS_str[1]);
                            }
                            if (lineitem.Contains("BRGHT_CLSTRS"))
                            {
                                string[] BRGHT_CLSTRS_str = Regex.Split(lineitem, "=");
                                BRGHT_CLSTRS = (float)Convert.ToDouble(BRGHT_CLSTRS_str[1]);
                            }

                        }

                        var DP_WFdata = new Dictionary<string, object>
                            {
                                { "STATUS", "PASS" }
                            };

                        DP_WFdata.Add("BRGHT_DEFECTS", BRGHT_DEFECTS);
                        DP_WFdata.Add("BRGHT_CLSTRS", BRGHT_CLSTRS);
                        strOutData = CreateDataString(DP_WFdata);
                        break;
                    case "Camera_RI":
                        LogMessage($"Camera_RI recvStr  {recvStr} ");
                        string[] RIResult = Regex.Split(recvStr, "\r\n");
                        float RI_WRSE_CRNR_R = 0;


                        foreach (var lineitem in RIResult)
                        {
                            LogMessage($"lineitem   {lineitem} ");
                            if (lineitem.Contains("RI_WRSE_CRNR_R"))
                            {
                                string[] RI_WRSE_CRNR_R_str = Regex.Split(lineitem, "=");
                                RI_WRSE_CRNR_R = (float)Convert.ToDouble(RI_WRSE_CRNR_R_str[1]);
                            }

                        }

                        var RI_WFdata = new Dictionary<string, object>
                            {
                                { "STATUS", "PASS" }
                            };

                        RI_WFdata.Add("RI_WRSE_CRNR_R", RI_WRSE_CRNR_R);
                        strOutData = CreateDataString(RI_WFdata);
                        break;
                    case "Camera_OC":
                        LogMessage($"Camera_OC recvStr  {recvStr} ");
                        string[] OCResult = Regex.Split(recvStr, "\r\n");
                        float Original_OC_shift_x_um = 0;
                        float Original_OC_shift_y_um = 0;

                        foreach (var lineitem in OCResult)
                        {
                            LogMessage($"lineitem   {lineitem} ");
                            if (lineitem.Contains("Original_OC_shift_x_um"))
                            {
                                string[] Original_OC_shift_x_um_str = Regex.Split(lineitem, "=");
                                Original_OC_shift_x_um = (float)Convert.ToDouble(Original_OC_shift_x_um_str[1]);
                            }
                            if (lineitem.Contains("Original_OC_shift_y_um"))
                            {
                                string[] Original_OC_shift_y_um_str = Regex.Split(lineitem, "=");
                                Original_OC_shift_y_um = (float)Convert.ToDouble(Original_OC_shift_y_um_str[1]);
                            }
                        }

                        var OC_WFdata = new Dictionary<string, object>
                            {
                                { "STATUS", "PASS" }
                            };

                        OC_WFdata.Add("Original_OC_shift_x_um", Original_OC_shift_x_um);
                        OC_WFdata.Add("Original_OC_shift_y_um", Original_OC_shift_y_um);
                        strOutData = CreateDataString(OC_WFdata);
                        break;
                    case "JSON_Date":
                        int JSONheader = recvStr.IndexOf("{");
                        int JSONtail = recvStr.LastIndexOf("}");
                        if (JSONheader == -1 || JSONtail == -1)
                        {
                            LogMessage($"[READ] header == -1 || tail == -1", MessageLevel.Error);
                            return false;
                        }

                        int JSON_capture_length = JSONtail - JSONheader + 1;
                        recvStr = recvStr.Substring(JSONheader, JSON_capture_length);
                        strOutData = recvStr;
                    
                    break;
                    case "VKD_Camera_pkey":
                        LogMessage($"VKD_Camera_pkey recvStr  {recvStr} ",MessageLevel.Info);
                        string[] C_pkey = Regex.Split(recvStr, "\r\n");
                        string vkd_C_pkey = string.Empty;


                        foreach (var lineitem in C_pkey)
                        {
                            LogMessage($"lineitem   {lineitem} ",MessageLevel.Info);
                            if (lineitem.Contains("Public key hex"))
                            {
                                string[] VKD_RESPOND_PKEY = Regex.Split(lineitem, ":");
                                vkd_C_pkey = VKD_RESPOND_PKEY[2].Replace(" ","");
                            }

                        }

                        var Camera_pkey_data = new Dictionary<string, object>
                            {
                                { "STATUS", "PASS" }
                            };

                        Camera_pkey_data.Add("pkey_hex", vkd_C_pkey);
                        strOutData = CreateDataString(Camera_pkey_data);
                        break;
                    
                    break;
                    case "VKD_Camera_sign":
                        LogMessage($"VKD_Camera_sign recvStr  {recvStr} ",MessageLevel.Info);
                        string[] C_sign = Regex.Split(recvStr, "\r\n");
                        string vkd_C_sign = string.Empty;


                        foreach (var lineitem in C_sign)
                        {
                            LogMessage($"lineitem   {lineitem} ",MessageLevel.Info);
                            if (lineitem.Contains("Signature out hex"))
                            {
                                string[] VKD_RESPOND_SIGN = Regex.Split(lineitem, ":");
                                vkd_C_sign = VKD_RESPOND_SIGN[1].Replace(" ","");
                            }

                        }

                        var Camera_sign_data = new Dictionary<string, object>
                            {
                                { "STATUS", "PASS" }
                            };

                        Camera_sign_data.Add("sign_hex", vkd_C_sign);
                        strOutData = CreateDataString(Camera_sign_data);
                        break;
                    
                    break;
                    case "line_break_parser":
                        LogMessage($"line_break_parser recvStr  {recvStr} ",MessageLevel.Info);
                        string[] linevalue = Regex.Split(recvStr, "\r\n");
                        string[] linekey = new string[128];
                        int line_count = 0;

                        foreach (var lineitem in linevalue)
                        {
                            if (lineitem.Length > 1)
                            {
                                LogMessage($"lineitem   {lineitem} ", MessageLevel.Info);
                                linekey[line_count] = "Result_" + (line_count + 1);
                                line_count++;
                            }
                        }
                        line_count = 0;
                        var Line_break_data = new Dictionary<string, object>
                            {
                                { "STATUS", "PASS" }
                            };
                        foreach (var lineitem in linevalue)
                        {
                            if (lineitem.Length > 1)
                            {
                                Line_break_data.Add(linekey[line_count], lineitem);
                                line_count++;
                            }
                        }
                        strOutData = CreateDataString(Line_break_data);
                        break;
                    
                    break;
                    case "VKD_JSON_DATA":
                        int VKD_JSONheader = recvStr.IndexOf("{");
                        int VKD_JSONtail = recvStr.LastIndexOf("}");
                        if (VKD_JSONheader == -1 || VKD_JSONtail == -1)
                        {
                            LogMessage($"[READ] header == -1 || tail == -1", MessageLevel.Error);
                            return false;
                        }

                        int VKD_JSON_capture_length = VKD_JSONtail - VKD_JSONheader + 1;
                        recvStr = recvStr.Substring(VKD_JSONheader, VKD_JSON_capture_length).Replace("'","\"");
                        strOutData = recvStr;
                  
                    
                    break;
                    case "VKD_Vendor_JSON_DATA":
                        int VKD_VendorJSONheader = recvStr.IndexOf("{");
                        int VKD_VendorJSONtail = recvStr.LastIndexOf("}");
                        if (VKD_VendorJSONheader == -1 || VKD_VendorJSONtail == -1)
                        {
                            LogMessage($"[READ] header == -1 || tail == -1", MessageLevel.Error);
                            return false;
                        }

                        int VKD_Vendor_JSON_capture_length = VKD_VendorJSONtail - VKD_VendorJSONheader + 1;
                        recvStr = recvStr.Substring(VKD_VendorJSONheader, VKD_Vendor_JSON_capture_length).Replace("'", "").Replace("{{", "{").Replace("}}", "}");
                        //recvStr = recvStr.Substring(VKD_VendorJSONheader, VKD_Vendor_JSON_capture_length);
                        //recvStr = recvStr.Substring(VKD_VendorJSONheader, VKD_Vendor_JSON_capture_length);
                        strOutData = recvStr;
                        break;

                 
                    default:
                        var data = new Dictionary<string, object>
                        {
                            { "STATUS", "PASS" }
                        };
                        strOutData = CreateDataString(data);
                    break;
                }
                return true;
            }
            else
            {
                var data = new Dictionary<string, object>
                {
                    { "STATUS", "FAIL" },
                    {" Respond", recvStr}
                };
                strOutData = CreateDataString(data);
                return false;
            }

        }

        public override bool READ(ref string output)
        {
            //output = strOutData;
            return true;
        }

        public override void Dispose()
        {
            throw new NotImplementedException();
        }

        public override bool Init(string strParamInfo)
        {
            return true;
        }

        public override bool UnInit()
        {
            return true;
        }

        public override void SetTimeout(int time)
        {
            //waitTime = time;
        }
        public override void SetCheckstr(string str)
        {
            //checkStr = str;
        }

        public string CreateDataString(Dictionary<string, object> data)
        {
            try
            {
                string jsonStr = JsonConvert.SerializeObject(data, Formatting.Indented);
                return jsonStr;
            }
            catch (Exception ex)
            {
                // 處理轉換錯誤
                return $"轉換為 JSON 字串時出現錯誤: {ex.Message}";
            }
        }

        public override bool SEND(string input)
        {
            throw new NotImplementedException();
        }

        public override bool Send(string input, string strActItem)
        {
            return true;
        }
    }
}

