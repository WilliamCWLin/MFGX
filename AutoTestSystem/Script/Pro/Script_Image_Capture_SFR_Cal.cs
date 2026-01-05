using AutoTestSystem.Base;
using AutoTestSystem.Equipment.CCD;
using AutoTestSystem.Equipment.ControlDevice;
using AutoTestSystem.Model;
using Manufacture;
using MvCamCtrl.NET;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static AutoTestSystem.BLL.Bd;
using static AutoTestSystem.Model.IQ_SingleEntry;

namespace AutoTestSystem.Script
{
    internal class Script_Image_Capture_SFR_Cal : Script_Image_Base
    {
        string strOutData = string.Empty;

        [Category("SE Parameters"), Description("load content"), Editor(typeof(CommandEditor_MakeWriteLine), typeof(UITypeEditor))]
        public string PIN { get; set; } = "";

        [Category("SE Parameters"), Description("DLL Path")]
        public string DLLPath { get; set; } = "";

        [Category("Check"), Description("DLL Path")]
        public bool CheckROI { get; set; } = false;

        [Category("Draw"), Description("")]
        public bool DrawROI { get; set; } = false;

        [Category("Draw"), Description("")]
        public bool DrawResult { get; set; } = false;

        [Category("Draw"), Description("")]
        public bool DrawCross { get; set; } = false;

        [Category("Draw"), Description("")]
        public bool DrawDiagonal { get; set; } = false;

        [Category("Save"), Description("Save Image Option"), TypeConverter(typeof(SaveImage))]
        public string saveImgage { get; set; } = "YES";

        [Category("Save"), Description("Save path")]
        public string savepath { get; set; } = "";
        [Category("Save"), Description("Save path BMP File Name")]
        public string savepath_bmp_file { get; set; } = "";
        [Category("Save"), Description("Save path RAW File Name")]
        public string savepath_raw_file { get; set; } = "";
        [Category("SFR_SPEC"), Description("Define the SFR SPEC for Center")]
        public double SFR_SPEC_CT { get; set; } =60;
        [Category("SFR_SPEC"), Description("Define the SFR SPEC for Corner")]
        public double SFR_SPEC_CN { get; set; } = 40;

        

        [Category("TabShow"), Description("Show Image on Tab")]
        public bool Show_Tab { get; set; } = false;



        public string strstringoutput = "";




        private Rectangle? ParseRoiCoordinates(string roiValue, string[] roiRuleOrder)
        {
            // 根據解析的順序轉換成 Rectangle 需要的坐標
            var coords = roiValue.Split(',').Select(int.Parse).ToArray();

            if (coords.Length == 4 && roiRuleOrder.Length == 4)
            {
                int top = coords[Array.IndexOf(roiRuleOrder, "Top")];
                int left = coords[Array.IndexOf(roiRuleOrder, "Left")];
                int bottom = coords[Array.IndexOf(roiRuleOrder, "Bottom")];
                int right = coords[Array.IndexOf(roiRuleOrder, "Right")];
                // 如果任一坐標為 -1，則返回 null
                if (top < 0 || left < 0 || bottom < 0 || right < 0)
                {
                    return null;
                }
                int width = right - left;
                int height = bottom - top;
                if (width <= 0 || height <= 0)
                {
                    return null;
                }
                return new Rectangle(left, top, width, height);
            }

            return null;
        }

        public override void Dispose()
        {
            //throw new NotImplementedException();
        }
        public override bool PreProcess()
        {
            strOutData = string.Empty;
            return true;
        }

        public override bool Process(Image_Base Image,ref string strOutData)
        {

            try
            {

                string str_Address = string.Empty;

                //Dictionary<int, string> imageDictionary = new Dictionary<int, string>();
                //Image.Capture(ref imageDictionary);
                //str_Address = imageDictionary[1];

                if (!Image.Capture(ref str_Address))
                {
                    return false;
                }



                long addressValue = Convert.ToInt64(str_Address.Substring(2), 16);
                // Create an IntPtr from the long value
                IntPtr ptr = new IntPtr(addressValue);

                DirectoryInfo di = null;
                if (saveImgage == "YES")
                {
                    if (!Directory.Exists(savepath))
                        di = Directory.CreateDirectory(savepath);

                    if (savepath_bmp_file == "")
                        LogMessage($"Igorne Save BMP File Name", MessageLevel.Debug);
                    else
                        Image.SaveImage(1, savepath + ReplaceProp(savepath_bmp_file));

                    if (savepath_raw_file == "")
                        LogMessage($"Igorne Save RAW File Name", MessageLevel.Debug);
                    else
                        Image.SaveImage(0, savepath + ReplaceProp(savepath_raw_file));
               

                }




                string PIN_tmp = PIN.Replace("%address%", str_Address);
                Dictionary<string, string> outputdata = new Dictionary<string, string>();

                string oricontent = PIN_tmp.Replace("\\n", "\n");
                //string oupt = "";
                // 要注意這個地方檔案位置要是正確的，檔名要是正確的
                string oricontent_Trans = ReplaceProp(oricontent);
                //string replaceImagePath = ReplaceProp(ImagePath);
                IQ_SingleEntry.SE_StartAction(DLLPath, oricontent_Trans, ref strOutData, outputdata);
                double CT_V_Top = 0,CT_H_Right = 0,CT_V_Bottom = 0,CT_H_Left = 0,TL_V_Top = 0,TL_H_Right = 0,TL_V_Bottom = 0,TL_H_Left = 0,TR_V_Top = 0,TR_H_Right = 0,TR_V_Bottom = 0,TR_H_Left = 0,BL_V_Top = 0,
                       BL_H_Right = 0,BL_V_Bottom = 0, BL_H_Left = 0, BR_V_Top = 0, BR_H_Right = 0, BR_V_Bottom = 0, BR_H_Left = 0;
                 if(outputdata.ContainsKey("SFR_SFR_CT_Top"))
                    CT_V_Top = double.Parse(outputdata["SFR_SFR_CT_Top"]);
                if (outputdata.ContainsKey("SFR_SFR_CT_Right"))
                    CT_H_Right = double.Parse(outputdata["SFR_SFR_CT_Right"]);
                if (outputdata.ContainsKey("SFR_SFR_CT_Bottom"))
                    CT_V_Bottom = double.Parse(outputdata["SFR_SFR_CT_Bottom"]);
                if (outputdata.ContainsKey("SFR_SFR_CT_Left"))
                    CT_H_Left = double.Parse(outputdata["SFR_SFR_CT_Left"]);
                if (outputdata.ContainsKey("SFR_SFR_TL_Top"))
                    TL_V_Top = double.Parse(outputdata["SFR_SFR_TL_Top"]);
                if (outputdata.ContainsKey("SFR_SFR_TL_Right"))
                    TL_H_Right = double.Parse(outputdata["SFR_SFR_TL_Right"]);
                if (outputdata.ContainsKey("SFR_SFR_TL_Bottom"))
                    TL_V_Bottom = double.Parse(outputdata["SFR_SFR_TL_Bottom"]);
                if (outputdata.ContainsKey("SFR_SFR_TL_Left"))
                    TL_H_Left = double.Parse(outputdata["SFR_SFR_TL_Left"]);
                if (outputdata.ContainsKey("SFR_SFR_TR_Top"))
                    TR_V_Top = double.Parse(outputdata["SFR_SFR_TR_Top"]);
                if (outputdata.ContainsKey("SFR_SFR_TR_Right"))
                    TR_H_Right = double.Parse(outputdata["SFR_SFR_TR_Right"]);
                if (outputdata.ContainsKey("SFR_SFR_TR_Bottom"))
                    TR_V_Bottom = double.Parse(outputdata["SFR_SFR_TR_Bottom"]);
                if (outputdata.ContainsKey("SFR_SFR_TR_Left"))
                    TR_H_Left = double.Parse(outputdata["SFR_SFR_TR_Left"]);
                if (outputdata.ContainsKey("SFR_SFR_BL_Top"))
                    BL_V_Top = double.Parse(outputdata["SFR_SFR_BL_Top"]);
                if (outputdata.ContainsKey("SFR_SFR_BL_Right"))
                    BL_H_Right = double.Parse(outputdata["SFR_SFR_BL_Right"]);
                if (outputdata.ContainsKey("SFR_SFR_BL_Bottom"))
                    BL_V_Bottom = double.Parse(outputdata["SFR_SFR_BL_Bottom"]);
                if (outputdata.ContainsKey("SFR_SFR_BL_Left"))
                    BL_H_Left = double.Parse(outputdata["SFR_SFR_BL_Left"]);
                if (outputdata.ContainsKey("SFR_SFR_BR_Top"))
                    BR_V_Top = double.Parse(outputdata["SFR_SFR_BR_Top"]);
                if (outputdata.ContainsKey("SFR_SFR_BR_Right"))
                    BR_H_Right = double.Parse(outputdata["SFR_SFR_BR_Right"]);
                if (outputdata.ContainsKey("SFR_SFR_BR_Bottom"))
                    BR_V_Bottom = double.Parse(outputdata["SFR_SFR_BR_Bottom"]);
                if (outputdata.ContainsKey("SFR_SFR_BR_Left"))
                    BR_H_Left = double.Parse(outputdata["SFR_SFR_BR_Left"]);



                LogMessage($"{strOutData}");
                List<DrawElement> elements = new List<DrawElement>();

                Color ROI_Chk_Spec = Color.Green;
                if (DrawROI)
                {

                    // 遍歷字典，找到所有的 ROI 鍵
                    foreach (var entry in outputdata)
                    {
                        if (entry.Key.StartsWith("ROI_") && entry.Key.EndsWith("_Roi"))
                        {

                            if (entry.Key.Contains("CT_Top"))
                            {
                                if (CT_V_Top >= SFR_SPEC_CT)
                                    ROI_Chk_Spec = Color.Green;
                                else
                                    ROI_Chk_Spec = Color.Red;
                            }
                            else if (entry.Key.Contains("CT_Right"))
                            {
                                if (CT_H_Right >= SFR_SPEC_CT)
                                    ROI_Chk_Spec = Color.Green;
                                else
                                    ROI_Chk_Spec = Color.Red;
                            }
                            else if (entry.Key.Contains("CT_Bottom"))
                            {
                                if (CT_V_Bottom >= SFR_SPEC_CT)
                                    ROI_Chk_Spec = Color.Green;
                                else
                                    ROI_Chk_Spec = Color.Red;
                            }
                            else if (entry.Key.Contains("CT_Left"))
                            {
                                if (CT_H_Left >= SFR_SPEC_CT)
                                    ROI_Chk_Spec = Color.Green;
                                else
                                    ROI_Chk_Spec = Color.Red;
                            }
                            else if (entry.Key.Contains("TL_Top"))
                            {
                                if (TL_V_Top >= SFR_SPEC_CN)
                                    ROI_Chk_Spec = Color.Green;
                                else
                                    ROI_Chk_Spec = Color.Red;
                            }
                            else if (entry.Key.Contains("TL_Right"))
                            {
                                if (TL_H_Right >= SFR_SPEC_CN)
                                    ROI_Chk_Spec = Color.Green;
                                else
                                    ROI_Chk_Spec = Color.Red;
                            }
                            else if (entry.Key.Contains("TL_Bottom"))
                            {
                                if (TL_V_Bottom >= SFR_SPEC_CN)
                                    ROI_Chk_Spec = Color.Green;
                                else
                                    ROI_Chk_Spec = Color.Red;
                            }
                            else if (entry.Key.Contains("TL_Left"))
                            {
                                if (TL_H_Left >= SFR_SPEC_CN)
                                    ROI_Chk_Spec = Color.Green;
                                else
                                    ROI_Chk_Spec = Color.Red;
                            }
                            else if (entry.Key.Contains("TR_Top"))
                            {
                                if (TR_V_Top >= SFR_SPEC_CN)
                                    ROI_Chk_Spec = Color.Green;
                                else
                                    ROI_Chk_Spec = Color.Red;
                            }
                            else if (entry.Key.Contains("TR_Right"))
                            {
                                if (TR_H_Right >= SFR_SPEC_CN)
                                    ROI_Chk_Spec = Color.Green;
                                else
                                    ROI_Chk_Spec = Color.Red;
                            }
                            else if (entry.Key.Contains("TR_Bottom"))
                            {
                                if (TR_V_Bottom >= SFR_SPEC_CN)
                                    ROI_Chk_Spec = Color.Green;
                                else
                                    ROI_Chk_Spec = Color.Red;
                            }
                            else if (entry.Key.Contains("TR_Left"))
                            {
                                if (TR_H_Left >= SFR_SPEC_CN)
                                    ROI_Chk_Spec = Color.Green;
                                else
                                    ROI_Chk_Spec = Color.Red;
                            }
                            else if (entry.Key.Contains("BL_Top"))
                            {
                                if (BL_V_Top >= SFR_SPEC_CN)
                                    ROI_Chk_Spec = Color.Green;
                                else
                                    ROI_Chk_Spec = Color.Red;
                            }
                            else if (entry.Key.Contains("BL_Right"))
                            {
                                if (BL_H_Right >= SFR_SPEC_CN)
                                    ROI_Chk_Spec = Color.Green;
                                else
                                    ROI_Chk_Spec = Color.Red;
                            }
                            else if (entry.Key.Contains("BL_Bottom"))
                            {
                                if (BL_V_Bottom >= SFR_SPEC_CN)
                                    ROI_Chk_Spec = Color.Green;
                                else
                                    ROI_Chk_Spec = Color.Red;
                            }
                            else if (entry.Key.Contains("BL_Left"))
                            {
                                if (BL_H_Left >= SFR_SPEC_CN)
                                    ROI_Chk_Spec = Color.Green;
                                else
                                    ROI_Chk_Spec = Color.Red;
                            }
                            else if (entry.Key.Contains("BR_Top"))
                            {
                                if (BR_V_Top >= SFR_SPEC_CN)
                                    ROI_Chk_Spec = Color.Green;
                                else
                                    ROI_Chk_Spec = Color.Red;
                            }
                            else if (entry.Key.Contains("BR_Right"))
                            {
                                if (BR_H_Right >= SFR_SPEC_CN)
                                    ROI_Chk_Spec = Color.Green;
                                else
                                    ROI_Chk_Spec = Color.Red;
                            }
                            else if (entry.Key.Contains("BR_Bottom"))
                            {
                                if (BR_V_Bottom >= SFR_SPEC_CN)
                                    ROI_Chk_Spec = Color.Green;
                                else
                                    ROI_Chk_Spec = Color.Red;
                            }
                            else if (entry.Key.Contains("BR_Left"))
                            {
                                if (BR_H_Left >= SFR_SPEC_CN)
                                    ROI_Chk_Spec = Color.Green;
                                else
                                    ROI_Chk_Spec = Color.Red;
                            }


                            // 解析 ROI 坐標
                            var roiCoordinates = ParseRoiCoordinates(entry.Value, outputdata["ROI_SFR_SFR_Roi_Rule"].Split(','));
                            if (roiCoordinates != null)
                            {
                                elements.Add(new DrawElement((Rectangle)roiCoordinates, "", ROI_Chk_Spec, 34, 6f, DrawElement.ElementType.Rectangle));
                            }
                        }
                    }


                }
                if (DrawResult)
                {
                    elements.Add(new DrawElement(
                        new Rectangle(0, 0, 1, 1),
                        strOutData,
                        Color.Blue,
                        20,
                        2.0f,
                        DrawElement.ElementType.Rectangle
                    ));
                }
                if (DrawCross)
                {
                    elements.Add(new DrawElement(
                                new Rectangle(0, 0, Image.Image_Width, Image.Image_Height),
                                "Cross",
                                Color.Blue,
                                12,
                                2.0f,
                                DrawElement.ElementType.Cross
                            ));
                }
                if (DrawDiagonal)
                {
                    elements.Add(new DrawElement(
                                new Rectangle(0, 0, Image.Image_Width, Image.Image_Height),
                                "Diagonal",
                                Color.Blue,
                                12,
                                2.0f,
                                DrawElement.ElementType.Diagonal
                            ));
                }


                if (CheckROI)
                {
                    if (outputdata.ContainsKey("Pattern_Center_TL_Pattern_x_y") &&
                        outputdata.ContainsKey("Pattern_Center_TR_Pattern_x_y") &&
                        outputdata.ContainsKey("Pattern_Center_BL_Pattern_x_y") &&
                        outputdata.ContainsKey("Pattern_Center_BR_Pattern_x_y"))
                    {
                        if (outputdata["Pattern_Center_TL_Pattern_x_y"].Contains("-1") ||
                            outputdata["Pattern_Center_TR_Pattern_x_y"].Contains("-1") ||
                            outputdata["Pattern_Center_BL_Pattern_x_y"].Contains("-1") ||
                            outputdata["Pattern_Center_BR_Pattern_x_y"].Contains("-1"))
                        {
                            string OutputMSG = "ROI FAIL:\n" +
                                               "TL_Pattern_x_y=" + outputdata["Pattern_Center_TL_Pattern_x_y"] + "\n" +
                                               "TR_Pattern_x_y=" + outputdata["Pattern_Center_TR_Pattern_x_y"] + "\n" +
                                               "BL_Pattern_x_y=" + outputdata["Pattern_Center_BL_Pattern_x_y"] + "\n" +
                                               "BR_Pattern_x_y=" + outputdata["Pattern_Center_BR_Pattern_x_y"];
                            elements.Add(new DrawElement(
                            new Rectangle(0, 0, 1, 1),
                            OutputMSG,
                            Color.Blue,
                            52,
                            2.0f,
                            DrawElement.ElementType.Rectangle
                            ));
                            LogMessage("Can't find ROI", MessageLevel.Error);

                            if (elements.Count > 0)
                            {
                                if (HandleDevice.DutDashboard != null)
                                {
                                    if (!Show_Tab)
                                    {
                                        HandleDevice.SwitchTabControlIndex(1);
                                        IQ_SingleEntry.DrawElementsOnImage(ptr, Image.Image_Width, Image.Image_Height, HandleDevice.DutDashboard.ImagePicturebox, elements);
                                    }else if(Show_Tab)
                                        IQ_SingleEntry.DrawElementsOnImage(ptr, Image.Image_Width, Image.Image_Height, HandleDevice.DutDashboard, elements,Description);
                                }
                            }

                            return false;
                        }

                    }
                    else
                    {
                        LogMessage("Can't find ROI Key.Check Dll path or Params", MessageLevel.Error);
                        return false;
                    }
                }

                if (elements.Count > 0)
                {
                    if (HandleDevice.DutDashboard != null)
                    {
                        if (!Show_Tab)
                        {
                            HandleDevice.SwitchTabControlIndex(1);
                            IQ_SingleEntry.DrawElementsOnImage(ptr, Image.Image_Width, Image.Image_Height, HandleDevice.DutDashboard.ImagePicturebox, elements);
                        }
                        else if (Show_Tab)
                            IQ_SingleEntry.DrawElementsOnImage(ptr, Image.Image_Width, Image.Image_Height, HandleDevice.DutDashboard, elements, Description);
                    }
                }

                String jsonStr = JsonConvert.SerializeObject(outputdata, Formatting.Indented);

                strOutData = jsonStr;
                strstringoutput = strOutData;
            }
            catch (Exception ex)
            {
                LogMessage($"Error For SFR Capture Calculate: {ex.Message}", MessageLevel.Error);
                return false;
            }


            return true;
        }
        public override bool PostProcess()
        {
            if (Spec != string.Empty && Spec != null)
            {
                string ret = string.Empty;             
                ret = CheckRule(strstringoutput, Spec);
                LogMessage($"CheckRule: {ret}", MessageLevel.Debug);
                if (ret == "PASS")
                    return true;
                else
                    return false;
            }
            return true;

        }




        //public static IntPtr ConvertBitmapToIntPtr(Bitmap bitmap)
        //{
        //    // 鎖定 Bitmap 的像素資料
        //    BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);

        //    // 取得指向像素資料的指標
        //    IntPtr ptr = bitmapData.Scan0;

        //    // 解鎖 Bitmap 的像素資料
        //    bitmap.UnlockBits(bitmapData);

        //    return ptr;
        //}


        //public static IntPtr ConvertByteArrayToIntPtr(byte[] byteArray)
        //{
        //    // Allocate unmanaged memory for the byte buffer.
        //    IntPtr ptr = Marshal.AllocHGlobal(byteArray.Length);

        //    // Copy the byte buffer to the unmanaged memory.
        //    Marshal.Copy(byteArray, 0, ptr, byteArray.Length);

        //    return ptr;
        //}



        //public static byte[] ConvertIntPtrToByteArray(IntPtr ptr, int length)
        //{
        //    // 創建 byte 陣列來存儲資料
        //    byte[] byteArray = new byte[length];

        //    // 使用 Marshal.Copy 方法將資料從 IntPtr 複製到 byte 陣列
        //    Marshal.Copy(ptr, byteArray, 0, length);

        //    return byteArray;
        //}


        //public static Bitmap CreateBitmapFromIntPtr(IntPtr ptr, int width, int height, PixelFormat pixelFormat)
        //{
        //    // Calculate the stride (width * bytes per pixel)
        //    int stride = width * Image.GetPixelFormatSize(pixelFormat) / 8;

        //    // Create a Bitmap from the IntPtr
        //    return new Bitmap(width, height, stride, pixelFormat, ptr);
        //}


        public class SaveImage : TypeConverter  //下拉式選單
        {
            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                List<string> Img_Save = new List<string>();

                Img_Save.Add("NO");
                Img_Save.Add("YES");

                return new StandardValuesCollection(Img_Save);
            }

            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                return true;
            }
        }

        
    }
}
