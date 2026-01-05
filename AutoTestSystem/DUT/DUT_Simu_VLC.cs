using System;
using System.Windows.Forms;
using System.Threading;
using AutoTestSystem.DAL;
using System.Text.RegularExpressions;
using static AutoTestSystem.BLL.Bd;
using AutoTestSystem.Base;
using System.ComponentModel;
using Manufacture;
using System.Drawing.Design;
using LibVLCSharp.Shared;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.Runtime.InteropServices;
using AutoTestSystem.Model;

namespace AutoTestSystem.DUT
{
    public class DUT_Simu_VLC : DUT_BASE
    {

        [Category("ChromasType"), Description("Chromas Defination"), TypeConverter(typeof(VLC_ChromasMode))]
        public string ChromasMode { get; set; } = "RV32";


        [Category("Resolution"), Description("Set Width")]
        public int width { get; set; } = 1920;

        [Category("Resolution"), Description("Set Heighy")]
        public int height { get; set; } = 1080;


        //[Category("URL"), Description("Input VLC URL Name")]
        //public string url { get; set; } = "URL";
        [Category("Timeout(ms)"), Description("Timeout for VLC Connect(ms)")]
        public int Timeout_ms { get; set; } = 20000;

        public class VLC_ChromasMode : TypeConverter  //下拉式選單
        {
            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {


                List<string> ChromasKeys = new List<string>();

                ChromasKeys.Add("RV32");
                ChromasKeys.Add("RV24");


                return new StandardValuesCollection(ChromasKeys);

            }

            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                return true;

            }

        }

        [Category("Image Calibration"), Description("VLC SHOW"), Editor(typeof(CommandEditor_MakeWriteLine), typeof(UITypeEditor))]
        public string content { get; set; } = "";

      

        public class CommandEditor_MakeWriteLine : UITypeEditor
        {

            public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
            {
                DUT_Simu_VLC temps =new DUT_Simu_VLC ();
                foreach (var valuess in GlobalNew.Devices.Values)
                {
                    if (valuess is DUT_Simu_VLC)
                    {
                        temps = (DUT_Simu_VLC)valuess;

                    }
                }

                using (VLC_Form form = new VLC_Form(temps))
                {
                    
                    form.ShowDialog();
                    return null;
                }
            }

            public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
            {
                return UITypeEditorEditStyle.Modal;
            }
        }

        int stride = 4;
        bool Lock_FLAG = true;
        byte[] frameData2;
        BitmapData bdata;
        Bitmap bp;
        LibVLC vlc = null;
        MediaPlayer mp = null;
        int Timeout_cur = 0;

        string url = string.Empty;



        public DUT_Simu_VLC()
        {
        
        }

        public override void Dispose()
        {
            throw new NotImplementedException();
        }

        public override bool Init(string strParamInfo)
        {

            url = strParamInfo;


            if (ChromasMode == "RV24")
            {
                stride = 3;
                bp = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            }
            else if (ChromasMode == "RV32")
            {
                stride = 4;
                bp = new Bitmap(width, height, PixelFormat.Format32bppRgb);
            }



            //bdata = bp.LockBits(new Rectangle(0, 0, bp.Width, bp.Height), ImageLockMode.ReadWrite, bp.PixelFormat);
            vlc = new LibVLC();
            mp = new MediaPlayer(vlc);

            mp.SetVideoFormat(ChromasMode, (uint)width, (uint)height, (uint)width * (uint)stride);
            mp.SetVideoCallbacks(LockCallback, UnlockCallback, DisplayCallback);




            return true;
        }


        public override bool Play() 
        {


            

            bdata = bp.LockBits(new Rectangle(0, 0, bp.Width, bp.Height), ImageLockMode.ReadWrite, bp.PixelFormat);
            bp.UnlockBits(bdata);
            frameData2 = new byte[width * height * stride];
            mp.Play(new Media(vlc, new Uri(url)));


            return true;
        }



        public override bool StartAction(string strItemName, string strParamIn, ref string strOutput)
        {
            throw new NotImplementedException();
        }
        
        public override bool OPEN()
        {


            return true;
        }

        public override bool UnInit()
        {


            if (mp != null)
            {
                mp.Stop();

                //mp.Dispose();
                //mp = null;

            }
            if (vlc != null)
            {
                //vlc.Dispose();
                //vlc = null;

            }



            return true;
        }

        public override bool SEND(string input)
        {
            return true;
        }

        public override bool SEND(byte[] input)
        {
            return true;
        }



        public override bool SaveImage(int Format_Mode, string strSavePath)
        {


            Lock_FLAG = true;

            //bdata = bp.LockBits(new Rectangle(0, 0, bp.Width, bp.Height), ImageLockMode.ReadWrite, bp.PixelFormat);
            //bp.UnlockBits(bdata);
            //frameData2 = new byte[width * height * stride];
            //mp.Play(new Media(vlc, new Uri(url)));

            while (Lock_FLAG)
            {
                LogMessage($"VLC callback Lock status ", MessageLevel.Debug);
                Thread.Sleep(1000);
                Timeout_cur++;

                if ((Timeout_cur * 1000) >= Timeout_ms)
                {
                    LogMessage($"VLC Connect Timeout ", MessageLevel.Debug);
                    return false;
                }
            }

            Timeout_cur = 0;
            bp.Save(strSavePath);
          


            return true;
        }



        private IntPtr LockCallback(IntPtr opaque, IntPtr planes)
        {
            IntPtr buffer = Marshal.AllocHGlobal(width * height * stride);
            Marshal.WriteIntPtr(planes, buffer);
            Lock_FLAG = true;
            return IntPtr.Zero;
        }

        private void UnlockCallback(IntPtr opaque, IntPtr picture, IntPtr planes)
        {
            IntPtr buffer = Marshal.ReadIntPtr(planes);
            Marshal.Copy(buffer, frameData2, 0, frameData2.Length);
            Marshal.Copy(frameData2, 0, bdata.Scan0, frameData2.Length);

            //Marshal.Copy(frameData2, 0, Vlc_Buffer.Data, frameData2.Length); //for opencv


            Lock_FLAG = false;

            Marshal.FreeHGlobal(buffer);
        }

        private void DisplayCallback(IntPtr opaque, IntPtr picture)
        {
            // 更新 UI 或執行其他操作

        }


    }
}
