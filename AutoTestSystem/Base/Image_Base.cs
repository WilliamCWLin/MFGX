using AutoTestSystem.Equipment.Image;
using AutoTestSystem.Model;
using DocumentFormat.OpenXml.Drawing.Charts;
using Newtonsoft.Json;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutoTestSystem.Base
{
    public abstract class Image_Base : Manufacture.Equipment
    {
        private Dictionary<string, int> _paramInfo = new Dictionary<string, int>();
        [JsonIgnore]
        [Browsable(false)]
        public List<string> Function = new List<string>();
        public string IniPath;
        public byte[] imageBuffer = new byte[1];
        //public SensorTab _sensorTab = new SensorTab();
        public int Image_Width = 0;
        public int Image_Height = 0;
        public string Image_input_str ="";

        public abstract void Dispose();
        public virtual bool Init(string strParamInfo)
        {
            return true;
        }

        public virtual bool Capture(string strSavePath)
        {
            return true;
        }
        public virtual bool Capture(ref string PtrBuffer)
        {
            return true;
        }

        public virtual bool Capture(ref string PtrBuffer ,int AVG_Count)
        {
            return true;
        }

        public virtual bool Start()
        {
            return true;
        }

    
        public virtual bool Set_Exposure(int value)
        {
            return true;
        }
        public virtual bool Set_Gain(int value)
        {
            return true;
        }
        public virtual bool SaveImage(string strSavePath)
        {
            throw new NotImplementedException();
        }
        public virtual bool SaveImage(int Format_Mode, string strSavePath)
        {
            return true;
        }
      
        public virtual bool ReadParamFile(string strSavePath)
        {
            throw new NotImplementedException();
        }
        public virtual bool Priview(ImageData img)
        {
            throw new NotImplementedException();
        }
        public struct ImageData
        {
            public int Size;
            public int Width;
            public int Height;
            public ImageType type;
            public string ImgPath;
        }
        public enum ImageType
        {
            A = 1,
            B,
            C
        }
        //Dothink function
        public virtual bool GPIOWrite(int pin, bool bEnable)
        {
            return true;
        }

        public virtual bool PowerWrite(int avdd, int dovdd, int dvdd, int afvcc, int vpp)
        {
            return true;
        }

        public virtual bool WriteI2C(byte in_slave_id, ushort in_addr, ushort in_data, byte mode, int iDevID)
        {
            return true;
        }

        public virtual string GetParam(string paramName)
        {
            switch (paramName)
            {
                case "Width":
                    return JsonConvert.SerializeObject(new { Width = _paramInfo["Width"] });
                case "Height":
                    return JsonConvert.SerializeObject(new { Height = _paramInfo["Height"] });
                case "Dev_ID":
                    return JsonConvert.SerializeObject(new { Dev_ID = _paramInfo["Dev_ID"] });
                case "Exposure":
                    return JsonConvert.SerializeObject(new { Exposure = _paramInfo["Exposure"] });
                case "Gain":
                    return JsonConvert.SerializeObject(new { Gain = _paramInfo["Gain"] });
                default:
                    return null;
            }
        }
        public virtual bool SetParam(string Funciotn ,string jsonParamInfo)
        {
            _paramInfo = JsonConvert.DeserializeObject<Dictionary<string, int>>(jsonParamInfo);
            return true;
        }
        public virtual bool EnumerateDev()
        {
            return true;
        }

        public class FolderSelEditorRelPath : System.Drawing.Design.UITypeEditor
        {
            public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
            {
                string initialDirectory = AppDomain.CurrentDomain.BaseDirectory;
                using (var folderBrowserDialog = new FolderBrowserDialog())
                {
                    folderBrowserDialog.Description = "選擇資料夾";
                    //folderBrowserDialog.RootFolder = Environment.SpecialFolder.MyComputer;
                    folderBrowserDialog.SelectedPath = initialDirectory;

                    if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                    {
                        string selectedFolderPath = folderBrowserDialog.SelectedPath;//絕對路徑
                        try
                        {
                            DriveInfo driveInfo = new DriveInfo(selectedFolderPath);
                            if (driveInfo.DriveType == DriveType.Network && selectedFolderPath.StartsWith(driveInfo.Name))
                            {
                                if (GlobalNew.Network_Path == string.Empty || GlobalNew.Network_Path == null)
                                {
                                    MessageBox.Show($"The DriveType {selectedFolderPath} is Network Drive, Please make sure to set up Network_Path already", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    return null;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"The DriveType is Network Error:{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return null;
                        }
                        // 轉換為相對路徑
                        string relativePath = GetRelativePath(selectedFolderPath);
                        // 將反斜杠轉換為雙反斜杠
                        relativePath = relativePath.Replace("/", "\\");
                        return relativePath;

                    }

                }

                return value; // 如果用戶取消選擇，返回原始值
            }

            public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
            {
                return UITypeEditorEditStyle.Modal;
            }

            private string GetRelativePath(string selectedFolderPath)
            {
                string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                Uri baseUri = new Uri(baseDirectory);
                Uri selectedFolderUri = new Uri(selectedFolderPath);

                Uri relativeUri = baseUri.MakeRelativeUri(selectedFolderUri);

                return Uri.UnescapeDataString(relativeUri.ToString());
            }
        }

        public  IntPtr ConvertBitmapToIntPtr(Bitmap bmp)
        {
            IntPtr ptr = IntPtr.Zero;
            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            try
            {
                ptr = bmpData.Scan0;
            }
            finally
            {
                bmp.UnlockBits(bmpData);
            }
            return ptr;
        }


        public  byte[] ConvertIntPtrToByteArray(IntPtr ptr, int length)
        {
            // 創建 byte 陣列來存儲資料
            byte[] byteArray = new byte[length];

            // 使用 Marshal.Copy 方法將資料從 IntPtr 複製到 byte 陣列
            Marshal.Copy(ptr, byteArray, 0, length);

            return byteArray;
        }


        public  IntPtr ConvertByteArrayToIntPtr(byte[] byteArray)
        {
            // Allocate unmanaged memory for the byte buffer.
            IntPtr ptr = Marshal.AllocHGlobal(byteArray.Length);

            // Copy the byte buffer to the unmanaged memory.
            Marshal.Copy(byteArray, 0, ptr, byteArray.Length);

            return ptr;
        }

        public  Bitmap CreateBitmapFromIntPtr(IntPtr ptr, int width, int height, PixelFormat pixelFormat)
        {
            //// Calculate the stride (width * bytes per pixel)
            //int stride = width * Image.GetPixelFormatSize(pixelFormat) / 8;

            //// Create a Bitmap from the IntPtr
            //return new Bitmap(width, height, stride, pixelFormat, ptr);


            int stride = width * Image.GetPixelFormatSize(pixelFormat) / 8;
            Bitmap bmp = new Bitmap(width, height, pixelFormat);

            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, pixelFormat);
            try
            {
                // 複製 ptr 的資料到 bmp 的 buffer
                unsafe
                {
                    Buffer.MemoryCopy((void*)ptr, (void*)bmpData.Scan0, stride * height, stride * height);
                }
            }
            finally
            {
                bmp.UnlockBits(bmpData);
            }

            return bmp;


        }

        //public IntPtr ConvertRaw10ToRaw8(IntPtr raw10Buffer, int raw10Width, int height)
        //{
        //    ////計算 RAW8 寬度與總像素數
        //    //int raw8Width = (raw10Width * 4 / 5);

        //    //int pixelCount = raw8Width * height;

        //    //// 計算 RAW10 buffer 長度
        //    //int raw10Length = raw10Width * height;
        //    //byte[] raw10Data = new byte[raw10Length];
        //    //Marshal.Copy(raw10Buffer, raw10Data, 0, raw10Length);

        //    ////建立 RAW8 buffer
        //    //byte[] raw8Data = new byte[pixelCount];

        //    //for (int i = 0; i < pixelCount; i++)
        //    //{
        //    //    int index = (i >> 2) * 5 + (i % 4); // 等同於 (i / 4) * 5 + (i % 4)
        //    //    raw8Data[i] = raw10Data[index];
        //    //}


        //    ////將 byte[] 轉為 unmanaged IntPtr
        //    //IntPtr raw8Ptr = Marshal.AllocHGlobal(pixelCount);
        //    //Marshal.Copy(raw8Data, 0, raw8Ptr, pixelCount);

        //    //return raw8Ptr;

        //}

        public void ConvertRaw10ToRaw8(IntPtr raw10Buffer, int raw10Width, int height , IntPtr raw8Buffer)
        {

            int raw8Width = raw10Width * 4 / 5;
            int pixelCount = raw8Width * height;
            
            
            for (int i = 0; i < pixelCount; i++)
            {
                int index = (i >> 2) * 5 + (i % 4);
                byte value = Marshal.ReadByte(raw10Buffer, index);
                Marshal.WriteByte(raw8Buffer, i, value);

            }

        }


        public void ConvertRaw16ToRaw8(IntPtr raw16Buffer, int raw_Width, int raw_height, IntPtr raw8Buffer)
        {


            int pixelCount = raw_Width * raw_height;

            for (int i = 0; i < pixelCount; i++)
            {
                // 每個 RAW16 pixel 佔 2 bytes
                int byteIndex = i * 2;

                //byte lowByte = Marshal.ReadByte(raw16Buffer, byteIndex);
                byte highByte = Marshal.ReadByte(raw16Buffer, byteIndex + 1);

                // 根據參數選擇保留高位元或低位元
                byte raw8Value = highByte;

                Marshal.WriteByte(raw8Buffer, i, raw8Value);
            }
           

        }

        public IntPtr ConvertRGBToBGR(IntPtr Buffer3h, int Width, int height)
        {
            Mat mat = new Mat(height, Width, MatType.CV_8UC3, Buffer3h);
            // OpenCV 預設是 BGR，如果你的 buffer 是 RGB，可以轉換
            Cv2.CvtColor(mat, mat, ColorConversionCodes.RGB2BGR);

            return mat.Data;
        }



        public void AverageRawFrame(IntPtr srcBuffer, int raw10Width, int height, IntPtr avgBuffer ,int count)
        {

            int raw8Width = raw10Width * 4 / 5;
            int pixelCount = raw8Width * height;


            for (int i = 0; i < count; i++)
            {
                for (int j = 0; j < pixelCount; j++)
                {
                    
                   byte Srcvalue = Marshal.ReadByte(srcBuffer, j);
                   byte dstvalue = Marshal.ReadByte(srcBuffer, j);

                    Marshal.WriteByte(avgBuffer, j, Srcvalue);
                }
            }

        }

    }
}
