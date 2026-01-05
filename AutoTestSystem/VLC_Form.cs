using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Imaging;
using OpenCvSharp;
using LibVLCSharp.Shared;
using System.Runtime.InteropServices;
using AutoTestSystem.DUT;

namespace AutoTestSystem
{
    public partial class VLC_Form : Form
    {
        public VLC_Form()
        {
            InitializeComponent();
        }

        public VLC_Form(DUT_Simu_VLC Param)
        {
            InitializeComponent();
            width = Param.width;
            height = Param.height;
            DoubleBuffered = true;
        }


        private byte[] DataFrame;
        private Mat Vlc_Buffer;
        private int width = 1920;
        private int height = 1080;
        private int stride = 0;
        private PixelFormat pixelFormat = PixelFormat.Format24bppRgb;
        private Bitmap bp;
        private LibVLC vlc;
        private MediaPlayer mp;
        private bool Lock_FLAG = true;
        private IntPtr VLC_ptr;
        private byte[] frameData2;
        private BitmapData bdata;
        private string URL;
        private string field;



        private void button1_Click(object sender, EventArgs e)
        {

            if (textBox1.Text == string.Empty)
            {
                MessageBox.Show("Please write URL in box");
                return;
            }

            Lock_FLAG = true;
            frameData2 = new byte[width * height * 4];
            //Vlc_Buffer = new Mat(1080, 1920, MatType.CV_8UC4);

            bp = new Bitmap(width, height, PixelFormat.Format32bppRgb);
            bdata = bp.LockBits(new Rectangle(0, 0, bp.Width, bp.Height), ImageLockMode.ReadWrite, bp.PixelFormat);
            bp.UnlockBits(bdata);
            vlc = new LibVLC();
            mp = new MediaPlayer(vlc);

            mp.SetVideoFormat("RV32", (uint)width, (uint)height, (uint)width * 4);
            mp.SetVideoCallbacks(LockCallback, UnlockCallback, DisplayCallback);
            //mp.Play(new Media(vlc, new Uri("rtsp://root:ba2c465bddd9@192.168.1.1:554/api/GetStream?streamID=0")));
            mp.Play(new Media(vlc, new Uri(URL)));
            
            timer1.Start();
        }

        private IntPtr LockCallback(IntPtr opaque, IntPtr planes)
        {
            //IntPtr buffer = Marshal.AllocHGlobal(1920 * 1080 * 3);
            IntPtr buffer = Marshal.AllocHGlobal(width * height * 4);
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




        private void button2_Click(object sender, EventArgs e)
        {
            timer1.Stop();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Lock_FLAG = true;
            while (Lock_FLAG)
            {
            }
            DrawCrosshair(bp);
            pictureBox1.Image = bp;
            DrawCircle(bp);
            pictureBox1.Image = bp;

        }
        private void DrawCrosshair(Bitmap bitmap)
        {
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                // Clear the bitmap with a white background
                //g.Clear(Color.White);

                // Define the pen to draw the crosshair
                using (Pen pen = new Pen(Color.Blue, 10))
                {
                    // Draw horizontal line
                    g.DrawLine(pen, 0, bitmap.Height / 2, bitmap.Width, bitmap.Height / 2);

                    // Draw vertical line
                    g.DrawLine(pen, bitmap.Width / 2, 0, bitmap.Width / 2, bitmap.Height);
                }
            }
        }

        private void DrawCircle(Bitmap bitmap)
        {
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                using (Pen pen = new Pen(Color.Blue, 10))
                {
                    field = TextBox_Field.Text;
                    //Draw Circle
                    double r = (width / 2) * (width / 2) + (height / 2) * (height / 2);
                    r = Math.Sqrt(r);
                    //int r = height / 2;
                    //field = (double)(field);
                    double F = Convert.ToDouble(field);
                    int R = Convert.ToInt32(F * 2 * r);
                    int X = (width / 2) - (R / 2);
                    int Y = (height / 2) - (R / 2);
                    g.DrawEllipse(pen, X, Y, R, R);

                }
            }
        }




        private void VLC_Form_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (timer1.Enabled != true)
                timer1.Stop();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            URL = textBox1.Text;
        }

        private void TextBox_Field_TextChanged(object sender, EventArgs e)
        {
            field = TextBox_Field.Text;
        }
    }
}
