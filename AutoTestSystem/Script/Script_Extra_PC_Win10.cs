
using AutoTestSystem.DAL;
using Manufacture;
using NAudio.Wave;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static AutoTestSystem.BLL.Bd;

namespace AutoTestSystem.Script
{
    internal class Script_Extra_PC_Win10 : Script_Extra_Base
    {

        string strOutData = string.Empty;

        //[Category("Common Parameters"), Description("自訂顯示名稱"), Editor(typeof(JsonEditor), typeof(UITypeEditor))]
        //public string Params { get; set; }


        [Category("Common Parameters"), Description("Select Parse Case"), TypeConverter(typeof(ParsingCaseList))]
        public string ParsingCase { get; set; }

        [Category("Common Parameters"), Description("自訂顯示名稱")]
        public string CheckString { get; set; }

        [Category("Common Parameters"), Description("自訂顯示名稱")]
        public int WaitTime { get; set; }

        [Category("Common Parameters"), Description("Command"), Editor(typeof(FileSelEditorRelPath), typeof(System.Drawing.Design.UITypeEditor))]
        public string Command { get; set; }

        [Category("Common Parameters"), Description("Exepath"), Editor(typeof(FileSelEditorRelPath), typeof(System.Drawing.Design.UITypeEditor))]
        public string Exepath { get; set; }

        public override void Dispose()
        {
            //throw new NotImplementedException();
        }
        public override bool PreProcess(string ActionItem, string Paraminput)
        {
            return true;
        }
        public override bool Process()
        {
            strOutData = string.Empty;
            string recvStr = "";
            DosCmd doscmd = new DosCmd();
            string rp_ExePath = ReplaceKeys(Exepath,"");
            string rp_Command = ReplaceKeys(Command, "");
            bool result = doscmd.SendCommand($"{rp_ExePath} {rp_Command}", ref recvStr, CheckString, WaitTime);
            LogMessage($"SendCommand({rp_ExePath} {rp_Command}) Checkstring({CheckString}) WaitTime({WaitTime})");
            if (result == true)
            {
                if (Enum.TryParse<ParsingRule>(ParsingCase, out ParsingRule rule))
                {
                    LogMessage($"case {rule}");
                    switch (rule)
                    {
                        case ParsingRule.AudioAnalyse:
                            string[] strResult = Regex.Split(recvStr, "Detected peak at");
                            List<string> hzValue = new List<string>();
                            List<int> dBValue = new List<int>();
                            foreach (var lineitem in strResult)
                            {
                                if (lineitem.Contains("dB"))
                                {

                                    int tmp_hzValue = Convert.ToInt32((lineitem.Substring(0, 5)));
                                    hzValue.Add("Hz_" + tmp_hzValue);

                                    int dBindex = lineitem.IndexOf("dB");
                                    int tmp_dBValue = Convert.ToInt32((lineitem.Substring(dBindex - 6, 2)));
                                    dBValue.Add(tmp_dBValue);
                                }
                            }
                            var audio_data = new Dictionary<string, object>
                        {
                                { "STATUS", "PASS" }
                        };
                            for (int item = 0; item < hzValue.Count; item++)
                            {
                                audio_data.Add(hzValue[item], dBValue[item]);
                            }
                            strOutData = JsonConvert.SerializeObject(audio_data);

                            break;

                        case ParsingRule.LedCountRatio:
                            string[] ledstrResult = Regex.Split(recvStr, "\r");
                            int count = 0;
                            List<string> BPName = new List<string>();
                            List<float> BPValue = new List<float>();
                            List<string> BGName = new List<string>();
                            List<float> BGValue = new List<float>();
                            List<string> RGName = new List<string>();
                            List<float> RGValue = new List<float>();
                            foreach (var lineitem in ledstrResult)
                            {
                                if (lineitem.Contains("DetectCount"))
                                {
                                    string[] ledCount = Regex.Split(lineitem, "=");
                                    count = Convert.ToInt32(ledCount[1]);
                                }

                                if (lineitem.Contains("BrighterPixel_MeanG"))
                                {
                                    string[] BPdata = Regex.Split(lineitem, "=");
                                    BPName.Add("BrighterPixel_MeanG_" + (BPName.Count + 1));
                                    float tmp_BPValue = (float)Convert.ToDouble(BPdata[1]);
                                    BPValue.Add(tmp_BPValue);
                                }

                                if (lineitem.Contains("Sample_BG_Ratio"))
                                {
                                    string[] BGdata = Regex.Split(lineitem, "=");
                                    BGName.Add("Sample_BG_Ratio_" + (BGName.Count + 1));
                                    float tmp_BGValue = (float)Convert.ToDouble(BGdata[1]);
                                    BGValue.Add(tmp_BGValue);
                                }

                                if (lineitem.Contains("Sample_RG_Ratio"))
                                {
                                    string[] RGdata = Regex.Split(lineitem, "=");
                                    RGName.Add("Sample_RG_Ratio_" + (RGName.Count + 1));
                                    float tmp_RGValue = (float)Convert.ToDouble(RGdata[1]);
                                    RGValue.Add(tmp_RGValue);
                                }

                            }
                            var ledCountdata = new Dictionary<string, object>
                        {
                            { "STATUS", "PASS" }
                        };
                            ledCountdata.Add("DetectCount", count);
                            for (int item = 0; item < BPName.Count; item++)
                            {
                                ledCountdata.Add(BPName[item], BPValue[item]);

                            }

                            for (int item = 0; item < BGName.Count; item++)
                            {
                                ledCountdata.Add(BGName[item], BGValue[item]);
                            }

                            for (int item = 0; item < RGName.Count; item++)
                            {
                                ledCountdata.Add(RGName[item], RGValue[item]);
                            }


                            strOutData = JsonConvert.SerializeObject(ledCountdata);
                            break;

                        case ParsingRule.LedIntensity:
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
                                if (lineitem.Contains("Check_luminance_Blue"))
                                {
                                    string[] b_lum_str = Regex.Split(lineitem, "=");
                                    b_luminance = (float)Convert.ToDouble(b_lum_str[1]);
                                }

                                if (lineitem.Contains("Check_luminance_Green"))
                                {
                                    string[] g_lum_str = Regex.Split(lineitem, "=");
                                    g_luminance = (float)Convert.ToDouble(g_lum_str[1]);
                                }

                                if (lineitem.Contains("Check_luminance_Red"))
                                {
                                    string[] r_lum_str = Regex.Split(lineitem, "=");
                                    r_luminance = (float)Convert.ToDouble(r_lum_str[1]);
                                }

                                if (lineitem.Contains("Check_luminance_Gray"))
                                {
                                    string[] gray_lum_str = Regex.Split(lineitem, "=");
                                    gray_luminance = (float)Convert.ToDouble(gray_lum_str[1]);
                                }

                                if (lineitem.Contains("Max_Value_Blue"))
                                {
                                    string[] max_b_str = Regex.Split(lineitem, "=");
                                    b_max = Convert.ToInt32(max_b_str[1]);
                                }

                                if (lineitem.Contains("Max_Value_Green"))
                                {
                                    string[] max_g_str = Regex.Split(lineitem, "=");
                                    g_max = Convert.ToInt32(max_g_str[1]);
                                }

                                if (lineitem.Contains("Max_Value_Red"))
                                {
                                    string[] max_r_str = Regex.Split(lineitem, "=");
                                    r_max = Convert.ToInt32(max_r_str[1]);
                                }

                                if (lineitem.Contains("Max_Value_Gray"))
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

                            strOutData = JsonConvert.SerializeObject(intensitydata);
                            break;

                        default:

                            strOutData = "{ \"STATUS\", \"PASS\" }";
                            break;
                    }
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
                strOutData = JsonConvert.SerializeObject(data); 
                return false;
            }

        }
        public override bool PostProcess()
        {
            string result = CheckRule(strOutData, Spec);

            if (result == "PASS" || Spec == "")
                return true;
            else
                return false;
        }

        public enum ParsingRule
        {
            Default,
            AudioAnalyse,
            LedCountRatio,
            LedIntensity
        }
        public class FileSelEditorRelPath : UITypeEditor
        {
            public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
            {
                using (var openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.Title = "選擇檔案";
                    openFileDialog.Filter = "所有檔案 (*.*)|*.*";

                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        string selectedFilePath = openFileDialog.FileName;

                        // 转换为相对路径或绝对路径
                        string filePath = GetRelativeOrAbsolutePath(selectedFilePath);
                        return filePath.Replace("/", "\\"); ;
                    }
                }

                return value; // 如果用户取消选择，返回原始值
            }

            public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
            {
                return UITypeEditorEditStyle.Modal;
            }

            private string GetRelativeOrAbsolutePath(string selectedFilePath)
            {
                string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string selectedFileFullPath = Path.GetFullPath(selectedFilePath);

                if (selectedFileFullPath.StartsWith(baseDirectory, StringComparison.OrdinalIgnoreCase))
                {
                    // 文件在当前执行文件的目录下，使用相对路径
                    Uri baseUri = new Uri(baseDirectory);
                    Uri selectedFileUri = new Uri(selectedFileFullPath);
                    Uri relativeUri = baseUri.MakeRelativeUri(selectedFileUri);
                    return Uri.UnescapeDataString(relativeUri.ToString());
                }
                else
                {
                    // 文件不在当前执行文件的目录下，使用绝对路径
                    return selectedFileFullPath;
                }
            }
        }
        public class ParsingCaseList : TypeConverter  //下拉式選單
        {
            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                // Get enum values array
                Array enumValues = Enum.GetValues(typeof(ParsingRule));
                List<string> enumList = new List<string>(enumValues.Length);

                if (enumValues.Length > 0)
                {
                    foreach (var enumValue in enumValues)
                    {
                        enumList.Add(enumValue.ToString());
                    }

                    return new StandardValuesCollection(enumList);
                }
                else
                {
                    return new StandardValuesCollection(new string[] { });
                }
            }

            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                return true;
            }

        }

    }
}
