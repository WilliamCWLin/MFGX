using AutoTestSystem.Base;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;


namespace AutoTestSystem.Equipment.Image
{
    internal class LoadImage : Image_Base
    {

        
        [Category("Image"), Description("Load Mode Option"), TypeConverter(typeof(ImageLoad))]
        public string LoadMode { get; set; } = "Load_Single_Image";

        [Category("Image"), Description("Image File"), Editor(typeof(Manufacture.FileSelEditorRelPath), typeof(System.Drawing.Design.UITypeEditor))]
        public string Single_Image_File{ get; set; }

        [Category("Image"), Description("Image Path"), Editor(typeof(FolderSelEditorRelPath), typeof(System.Drawing.Design.UITypeEditor))]
        public string Group_Image_Path { get; set; }


        private List<string> imageFilePaths =null;
        private byte[] bufferWithoutHeader = null;
        private byte[] buffer = null;
        private MemoryStream ms = null;
        private IntPtr SFR_Buffer = IntPtr.Zero;

        public override bool Init(string strParamInfo)
        {
            string[] imageFiles = {};
            //imageFilePaths = null;

            //Load Single Image
            string File_Path = Single_Image_File;

            //Load Group Image
            //string folderPath = "./Image"; // 替換成你的資料夾路徑
            string folderPath = Group_Image_Path; // 替換成你的資料夾路徑

            if (LoadMode == "Load_Single_Image")
            {
                if (File_Path == "" || File_Path ==null)
                {
                    return false;
                }
                
                imageFiles = imageFiles.Concat(new string[] { File_Path }).ToArray();

            }
            else if (LoadMode == "Load_Group_Image")
            {
                if (folderPath == "" || folderPath == null)
                {
                    return false;
                }
                imageFiles = Directory.GetFiles(folderPath, "*.bmp"); //// 取得資料夾內所有圖檔的路徑， 你可以根據需要更改檔案類型     
            }

                                                             
            Array.Sort(imageFiles); // 依照檔案名稱排序

            if (imageFilePaths == null)
            {
                imageFilePaths = new List<string>(imageFiles);
            }

            return true;
        }


        public override void Dispose()
        {
            throw new NotImplementedException();
        }

        public override bool Capture(string strSavePath)
        {
            return true;
        }

        public override bool Capture(ref string PtrBuffer)
        {
            try
            {
                if (imageFilePaths.Count > 0)
                {

                    using (Bitmap bmp = new Bitmap(imageFilePaths[0]))
                    {

                        Image_Width = bmp.Width;
                        Image_Height = bmp.Height;
                        IntPtr SFR_Buffer = ConvertBitmapToIntPtr(bmp);
                        //Byte[] buffer;
                        ms = new MemoryStream();
                        bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                        buffer = ms.ToArray();
                        

                        int headerSize = 54;
                        // delete bmp header, 54 byte
                        if (bufferWithoutHeader == null)
                            bufferWithoutHeader = new byte[buffer.Length - headerSize];

                        bufferWithoutHeader = ConvertIntPtrToByteArray(SFR_Buffer, bufferWithoutHeader.Length);
                        SFR_Buffer = ConvertByteArrayToIntPtr(bufferWithoutHeader);
                        string str_Address = $"0x{SFR_Buffer.ToString("X")}";
                        string removedFilePath = imageFilePaths[0];
                        imageFilePaths.RemoveAt(0);
                        PtrBuffer = str_Address;

                    }

                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                LogMessage($"Error For LoadImage Capture: {ex.Message}", MessageLevel.Error);
                return false;
            }
            return true;
        }


        //public IntPtr ConvertBitmapToIntPtr(Bitmap bmp)
        //{
        //    IntPtr ptr = IntPtr.Zero;
        //    BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
        //    try
        //    {
        //        ptr = bmpData.Scan0;
        //    }
        //    finally
        //    {
        //        bmp.UnlockBits(bmpData);
        //    }
        //    return ptr;
        //}


        //public byte[] ConvertIntPtrToByteArray(IntPtr ptr, int length)
        //{
        //    // 創建 byte 陣列來存儲資料
        //    byte[] byteArray = new byte[length];

        //    // 使用 Marshal.Copy 方法將資料從 IntPtr 複製到 byte 陣列
        //    Marshal.Copy(ptr, byteArray, 0, length);

        //    return byteArray;
        //}


        //public IntPtr ConvertByteArrayToIntPtr(byte[] byteArray)
        //{
        //    // Allocate unmanaged memory for the byte buffer.
        //    IntPtr ptr = Marshal.AllocHGlobal(byteArray.Length);

        //    // Copy the byte buffer to the unmanaged memory.
        //    Marshal.Copy(byteArray, 0, ptr, byteArray.Length);

        //    return ptr;
        //}

        public class ImageLoad : TypeConverter  //下拉式選單
        {
            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                List<string> ImageLoad = new List<string>();

                ImageLoad.Add("Load_Single_Image");
                ImageLoad.Add("Load_Group_Image");
                
                return new StandardValuesCollection(ImageLoad);
            }

            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                return true;
            }
        }

        public override bool UnInit()
        {
            imageFilePaths = null;
            bufferWithoutHeader = null;
            buffer = null;
            ms = null;
            SFR_Buffer = IntPtr.Zero;


            // 强制垃圾回收
            GC.Collect();
            GC.WaitForPendingFinalizers();


            return true;
        }




        }
}
