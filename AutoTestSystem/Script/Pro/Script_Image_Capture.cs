using AutoTestSystem.Base;
using AutoTestSystem.Equipment.Image;
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
using static AutoTestSystem.Equipment.Image.UC930;

namespace AutoTestSystem.Script
{
    internal class Script_Image_Capture : Script_Image_Base
    {
        string strOutData = string.Empty;
        [Category("Save"), Description("Save path")]
        public string savepath { get; set; } = "";

        [Category("Save"), Description("Save Image Option"), TypeConverter(typeof(SaveImage))]
        public string saveImgage { get; set; } = "YES";
        [Category("Save"), Description("Save path BMP File Name")]
        public string savepath_bmp_file { get; set; } = "";
        [Category("Save"), Description("Save path RAW File Name")]
        public string savepath_raw_file { get; set; } = "";

        [Category("TabShow"), Description("Show Image on Tab")]
        public bool Show_Tab { get; set; } = false;



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
                if (!Image.Capture(ref str_Address))
                {
                    return false;
                }

                long addressValue = Convert.ToInt64(str_Address.Substring(2), 16);
                // Create an IntPtr from the long value
                IntPtr ptr = new IntPtr(addressValue);
                //Image.Capture(savepath);
                
                DirectoryInfo di = null;
                if (saveImgage == "YES")
                {
                    if (!Directory.Exists(savepath))
                        di = Directory.CreateDirectory(savepath);

                    if(savepath_bmp_file == "")
                        LogMessage($"Igorne Save BMP File Name", MessageLevel.Debug);
                    else 
                        Image.SaveImage(1, savepath+ ReplaceProp(savepath_bmp_file));

                    if (savepath_raw_file == "")
                        LogMessage($"Igorne Save RAW File Name", MessageLevel.Debug);
                    else
                        Image.SaveImage(0, savepath + ReplaceProp(savepath_raw_file));
                   
                }
                using (Bitmap checkbitmap = Image.CreateBitmapFromIntPtr(ptr, Image.Image_Width, Image.Image_Height, PixelFormat.Format24bppRgb))
                {

                    if (!Show_Tab)
                    {
                        HandleDevice.SwitchTabControlIndex(1);
                        HandleDevice.DutDashboard.ImagePicturebox.Invoke((Action)(() =>
                        {
                            HandleDevice.DutDashboard.ImagePicturebox.Image?.Dispose(); // 清理先前的图片
                            HandleDevice.DutDashboard.ImagePicturebox.Image = (Bitmap)checkbitmap.Clone(); // 设置新的 Bitmap 对象
                            HandleDevice.DutDashboard.ImagePicturebox.Refresh();
                        }));
                    }else if (Show_Tab)
                        HandleDevice.DutDashboard.ShowSingleImageInTab(Description, (Bitmap)checkbitmap.Clone());
                }
                LogMessage($"Capture Suceesed ", MessageLevel.Debug);
                ptr = IntPtr.Zero;

                return true;
            }
            catch(Exception ex)
            {

                LogMessage($"Capture Fail :{ ex.Message}", MessageLevel.Error);
                return false;
            }
                 
        }
        public override bool PostProcess()
        {
            //bCloseCamera();
            if (Spec != string.Empty && Spec != null)
            {
                string ret = string.Empty;             
                ret = CheckRule(strOutData, Spec);
                LogMessage($"CheckRule: {ret}", MessageLevel.Debug);
                if (ret == "PASS")
                    return true;
                else
                    return false;
            }
            return true;

        }

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


        public Bitmap CreateBitmapFromBuffer(Image_Base Image,byte[] rgbBuffer)
        {
            // 確保緩衝區大小正確
            int expectedBufferSize = Image.Image_Width * Image.Image_Height * 3; // 每個像素24位（3字節）
            if (rgbBuffer.Length != expectedBufferSize)
            {
                throw new ArgumentException($"RGB buffer size ({rgbBuffer.Length}) does not match expected size ({expectedBufferSize}).");
            }

            Bitmap bitmap = new Bitmap(Image.Image_Width, Image.Image_Height, PixelFormat.Format24bppRgb);
            BitmapData bitmapData = null;
            try
            {
                // 鎖定位圖的位元組，準備寫入
                bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                    ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

                // 將RGB緩衝區數據複製到位圖
                Marshal.Copy(rgbBuffer, 0, bitmapData.Scan0, rgbBuffer.Length);
            }
            finally
            {
                // 解鎖位圖
                if (bitmapData != null)
                {
                    bitmap.UnlockBits(bitmapData);
                }
            }

            return bitmap;
        }
    }
}
