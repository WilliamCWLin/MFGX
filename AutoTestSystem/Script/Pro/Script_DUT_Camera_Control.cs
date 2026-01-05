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
using System.Windows.Forms;
using System.Drawing;
using AutoTestSystem.Model;
using System.Runtime.InteropServices;
using System.Web.UI.WebControls;
using PdfSharp.Drawing;
using static AutoTestSystem.Script.Script_ControlDevice_Base;
using AutoTestSystem.Equipment.ControlDevice;
using System.Drawing.Imaging;
using OpenCvSharp.Flann;
using System.Threading;

namespace AutoTestSystem.Script
{
    internal class Script_DUT_Camera_Control : ScriptDUTBase
    {
        string strOutData = string.Empty;
        int Format = 0;
        int errorcode;
        string SaveImagePath = string.Empty;
        [JsonIgnore]
        [Browsable(false)]
        byte[] Imagebuffer = null;

        [Category("Camera Control Parameter"), Description("Camera Control Action Define")]
        public VIDEO_ACTION ControlMode { get; set; } = VIDEO_ACTION.Init;

        [Category("Camera Control Parameter"), Description("存圖檔案名稱(不需要填寫副檔名)支援用%%方式做變數值取代")]
        public string URL_Name { get; set; } = "";

        [Category("ImageSave"), Description("存圖檔案名稱(不需要填寫副檔名)支援用%%方式做變數值取代")]
        public string SaveFileName { get; set; } = "image01";

        [Category("ImageSave"), Description("存圖副檔名選擇"), TypeConverter(typeof(ImageFormat))]
        public string SaveFormat { get; set; } = "jpg";

        [Category("ImageSave"), Description("選擇存取影像文件夾"), Editor(typeof(FolderSelEditorRelPath), typeof(System.Drawing.Design.UITypeEditor))]
        public string SavePath { get; set; }

        [Category("Preview"), Description("顯示張數")]
        public int Count { get; set; } = 200;

        [Category("Preview"), Description("取張間隔(ms)")]
        public int Interval { get; set; } = 30;

        [Category("Plugin Player"), Description("<URL> [快取時間] [寬度] [高度]")]
        public string Arguments { get; set; } = "rtsp://10.0.0.2/stream1 300 1296 972";
        [Category("Plugin Player"), Description("exe path")]
        public string ExePath { get; set; } = "Utility/vlcplayer/VLCPlayerApp.exe";

        public enum VIDEO_ACTION
        {
            Init,
            Play,
            Preview,
            SaveImage,
            Stop,
            Uninit,
            Reset,
            PluginPlayer
        }

        public class Camera_control_mode : TypeConverter  //下拉式選單
        {
            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {


                List<string> ChromasKeys = new List<string>();

                ChromasKeys.Add("Init");
                ChromasKeys.Add("Uninit");
                ChromasKeys.Add("SaveImage");
                ChromasKeys.Add("Play");
                ChromasKeys.Add("Stop");
                return new StandardValuesCollection(ChromasKeys);

            }

            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                return true;

            }

        }

        public class ImageFormat : TypeConverter  //下拉式選單
        {
            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {


                List<string> ImageFormateKeys = new List<string>();

                ImageFormateKeys.Add("bmp");
                ImageFormateKeys.Add("jpg");
                ImageFormateKeys.Add("png");

                return new StandardValuesCollection(ImageFormateKeys);

            }

            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                return true;

            }

        }

        public override void Dispose()
        {
            // 釋放 Imagebuffer
            if (Imagebuffer != null)
            {
                Array.Clear(Imagebuffer, 0, Imagebuffer.Length);
                Imagebuffer = null;
            }
        }
        
        public override bool PreProcess()
        {
            SaveImagePath = string.Empty;

            // 釋放 Imagebuffer
            if (Imagebuffer != null)
            {
                Array.Clear(Imagebuffer, 0, Imagebuffer.Length);

            }
            return true;
        }

        public override bool Process(DUT_BASE DUTDevices, ref string output)
        {
            try
            {

                LogMessage($"Action:{ControlMode}");

                switch (ControlMode)
                {
                    case VIDEO_ACTION.Init:
                        return DUTDevices.VideoInit(ReplaceProp(URL_Name));

                    case VIDEO_ACTION.Uninit:
                        return DUTDevices.VideoUnInit();
                    case VIDEO_ACTION.Preview:
                        // 切換到 Tab 1
                        DUTDevices.SwitchTabControlIndex(1);
                        bool ret = true;
                        if (Imagebuffer == null)
                        {
                            Imagebuffer = new byte[((DUT_CGI_VLC)DUTDevices).Width * ((DUT_CGI_VLC)DUTDevices).Height * 3];
                        }
                        
                        for (int i = 0; i < Count; i++)
                        {
                            Array.Clear(Imagebuffer, 0, Imagebuffer.Length);
                            ret = DUTDevices.CaptureImage(Imagebuffer);
                            if (ret == false)
                            {                              
                                ret = false;
                                LogMessage($"Preview Fail.({i})");
                            }
                            else
                            {
                                DUTDevices.Preview(Imagebuffer);
                            }
                                
                            Thread.Sleep(Interval);
                        }

                        // 循環結束後切換回 Tab 0
                        DUTDevices.SwitchTabControlIndex(0);
                        return ret;

                    case VIDEO_ACTION.SaveImage:
                        string fileName = ReplaceProp(SaveFileName);
                        SaveImagePath = fileName.Contains(SaveFormat) ?
                            Path.Combine(SavePath, fileName) :
                            Path.Combine(SavePath, fileName + "." + SaveFormat);
                        try
                        {
                            if (Imagebuffer == null)
                            {
                                Imagebuffer = new byte[((DUT_CGI_VLC)DUTDevices).Width * ((DUT_CGI_VLC)DUTDevices).Height * 3];
                            }
                            Array.Clear(Imagebuffer, 0, Imagebuffer.Length);
                            ret = DUTDevices.CaptureImage(Imagebuffer);
                            IQ_SingleEntry.SaveImage(Imagebuffer, ((DUT_CGI_VLC)DUTDevices).Width, ((DUT_CGI_VLC)DUTDevices).Height, SaveImagePath);
                            return true; // 儲存成功

                        }
                        catch (Exception ex)
                        {
                            LogMessage($"Save VLC Fail.{ex.Message}");
                            return false;
                        }
                        //if (!DUTDevices.SaveImage(SaveImagePath))
                        //{
                        //    return false;
                        //}
                        //break;

                    case VIDEO_ACTION.Play:
                        return DUTDevices.Play();

                    case VIDEO_ACTION.Stop:
                        return DUTDevices.Stop();
                    case VIDEO_ACTION.Reset:
                        bool retReset = DUTDevices.UnInit();
                        Thread.Sleep(1500);
                        retReset &= DUTDevices.Init("");
                        return retReset;
                    case VIDEO_ACTION.PluginPlayer:
                        try
                        {
                            // 確認執行檔是否存在
                            if (!File.Exists(ExePath))
                            {
                                LogMessage($"Can't Find: {ExePath}");
                                return false;
                            }

                            // 創建 ProcessStartInfo
                            ProcessStartInfo startInfo = new ProcessStartInfo
                            {
                                FileName = ExePath,
                                Arguments = Arguments,
                                UseShellExecute = false, // 不使用 Shell，便於等待程序結束
                                CreateNoWindow = false // 可設為 true 以隱藏呼叫程式的視窗
                            };
                            // 啟動 VLCPlayerApp 程式
                            using (Process process = System.Diagnostics.Process.Start(startInfo))
                            {
                                LogMessage("EXE Start，Waiting End...");

                                // 等待程式結束
                                process.WaitForExit();

                                // 程式結束後，執行後續邏輯
                                LogMessage($"EXE Closed");

                                return true;
                            }
                        }
                        catch (Exception ex)
                        {
                            LogMessage($"EXE Start Fail: {ex.Message}");
                            return false;
                        }

                    default:
                        LogMessage($"未知的 ControlMode: {ControlMode}");

                        return false;
                }

                strOutData = output;
            }
            catch (Exception ex)
            {
                LogMessage(ex.Message);
                return false;
            }

            return true;
        }





        public override bool PostProcess()
        {
            //string result = CheckRule(strOutData, Spec);
            ////ExtraProcess(ref result);

            //if (result == "PASS" || Spec == "")
            //{
            //    return true;
            //}             
            //else
            //{
            //    LogMessage($"{result}",MessageLevel.Error);
            //    return false;
            //}
            return true;
        }

        //public void ExtraProcess(ref string output)
        //{
        //    switch (CheckCmd)
        //    {
        //        case "TOF_Get":

        //            string TOF_data = JsonConvert.SerializeObject(JObject.Parse(strOutData)["data"]);
        //            PushMoreData("TOF_data", TOF_data);

        //            if (!Directory.Exists(@"./TOF_data"))
        //                Directory.CreateDirectory(@"./TOF_data");
        //            if (PopMoreData("TOF_Calib") == "Done")
        //                File.WriteAllText($@"./TOF_data/{PopMoreData("ProductSN")}_{DateTime.Now.ToString("MMddHHmmss")}_now.txt", TOF_data);
        //            else
        //                File.WriteAllText($@"./TOF_data/{PopMoreData("ProductSN")}_{DateTime.Now.ToString("MMddHHmmss")}_pre.txt", TOF_data);
        //            break;

        //        case "TOF_Calib":

        //            string CRC16_pre = JsonConvert.SerializeObject(JObject.Parse(strOutData)["CRC16_pre"]).Split('"')[1];
        //            string CRC16_now = JsonConvert.SerializeObject(JObject.Parse(strOutData)["CRC16_now"]).Split('"')[1];

        //            if (CRC16_now == CRC16_pre)
        //            {
        //                output = "TOF Calibration dosen't work!!";
        //                PushMESData("CRC16_pre", Tuple.Create("CRC16_pre", CRC16_pre, "FAIL"));
        //                PushMESData("CRC16_now", Tuple.Create("CRC16_now", CRC16_now, "FAIL"));
        //            }
        //            else
        //            {
        //                PushMoreData("TOF_Calib", "Done");
        //                PushMESData("CRC16_pre", Tuple.Create("CRC16_pre", CRC16_pre, "PASS"));
        //                PushMESData("CRC16_now", Tuple.Create("CRC16_now", CRC16_now, "PASS"));
        //            }
        //            break;

        //        case "Button_Get":

        //            string Button_data = JsonConvert.SerializeObject(JObject.Parse(strOutData)["data"]);
        //            PushMoreData("Button_data", Button_data);
        //            break;
        //    }
        //}

    }
}
