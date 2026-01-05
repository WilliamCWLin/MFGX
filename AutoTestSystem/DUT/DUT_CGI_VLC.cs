using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using AutoTestSystem.DAL;
using System.Text.RegularExpressions;
using static AutoTestSystem.BLL.Bd;
using AutoTestSystem.Base;
using System.ComponentModel;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net;
using LibVLCSharp.Shared;
using VideoLanClient;
using System.Drawing.Design;
using System.Windows.Forms.Design;
using static AutoTestSystem.Script.Script_Control_ClientTCPMessage;
using System.Drawing.Imaging;
using System.Security.Policy;
using System.Xml.Linq;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;

using static AutoTestSystem.DUT.DUT_Simu_VLC;
using AutoTestSystem.Model;

namespace AutoTestSystem.DUT
{
    public class DUT_CGI_VLC : DUT_BASE
    {

        [JsonIgnore]
        [Browsable(false)]
        int totalTimeOut = 0;

        [JsonIgnore]
        [Browsable(false)]
        private LibVLC _libVLC = null;
        [JsonIgnore]
        [Browsable(false)]
        private MediaPlayer _mediaPlayer = null;
        [JsonIgnore]
        [Browsable(false)]
        private Media _media = null;
        [Browsable(false)]
        private VideoLANClient _sourceProvider = null;

        [Category("Params"), Description("IP_addr")]
        public string IP_addr { get; set; }


        [Category("VLC"), Description("MediaOptions")]
        public string mediaOption { get; set; } = "--network-caching=300;--no-audio";
        [Category("VLC"), Description("Width")]
        public int Width { get; set; } = 2592;
        [Category("VLC"), Description("Height")]
        public int Height { get; set; } = 1944;
        [Category("VLC"), Description("ms")]
        public int PlayTimeout { get; set; } = 6000;

        public DUT_CGI_VLC()
        {
            IP_addr = "10.0.0.2";

            // 初始化 LibVLC
            
        }

        public override void Dispose()
        {
            throw new NotImplementedException();
        }

        public override bool Init(string strParamInfo)
        {
            // 建立 LibVLC 和 MediaPlayer
            _libVLC = new LibVLC(mediaOption.Split(';'));

            _mediaPlayer = new MediaPlayer(_libVLC);



            _sourceProvider = new VideoLANClient(_mediaPlayer, SynchronizationContext.Current);
            return true;
            //_vlcviewer = new VLCViewer(Width, Height, rtspUrl);
            //bool ret = _vlcviewer.Init(Width, Height, rtspUrl, mediaOption.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries));
            //return ret;
        }
        public override bool VideoInit(string rtspUrl)
        {
            try
            {
                _media?.Dispose();
                // 設置 RTSP 流的 URL
                _media = new Media(_libVLC, new Uri(rtspUrl));

            }
            catch (Exception ex) 
            {
                LogMessage($"VideoInit Exception{ex.Message}");
                return false; // 初始化解除失敗
            }
            return true;
        }

        public override bool VideoUnInit()
        {
            return true;
        }

        public override bool Status(ref string msg)
        {
            return true;
        }
        public override bool StartAction(string strItemName, string strParamIn, ref string strOutput)
        {
            return true;
        }

        public override bool OPEN()
        {
            return true;
        }

        public override bool UnInit()
        {
            try
            {
                if (_mediaPlayer != null)
                {
                    _mediaPlayer.Stop();
                }
               

                if (_sourceProvider != null)
                {
                    _sourceProvider.UnInit();
                    // 釋放資源
                    _sourceProvider.Dispose();
                    _sourceProvider = null;
                }
                if (_mediaPlayer != null)
                {
                    _mediaPlayer.Dispose();
                    _mediaPlayer = null;
                }
                if (_libVLC != null)
                {
                    _libVLC.Dispose();
                    _libVLC = null;
                }
                return true; // 初始化解除成功
            }
            catch (Exception ex)
            {
                LogMessage($"VideoUnInit Exception{ex.Message}");
                return false; // 初始化解除失敗
            }
        }

        public override bool SEND(string input)
        {
            return true;
        }

        public override bool SEND(byte[] input)
        {
            return true;
        }

        public override bool READ(string ParamIn, ref string output)
        {
            return true;
        }

        public override bool READNOJSON(string ParamIn , ref string output)
        {
            return true;
        }

        public override void SetTimeout(int timeout_comport, int timeout_total)
        {
            totalTimeOut = timeout_total;
        }
        enum E_request_type : ushort
        {
            GET = 0,
            POST = 1
        }

        public override bool SendCGICommand(int request_type, string Checkstr, string CGICMD, string input, ref string output)
        {
            string CGIURL = "http://" + IP_addr + CGICMD;
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(CGIURL);
            httpWebRequest.Timeout = totalTimeOut;

            httpWebRequest.ContentType = "application/json";
            if (request_type == (int)E_request_type.GET)
                httpWebRequest.Method = "Get";
            else
                httpWebRequest.Method = "POST";

            if (request_type != (int)E_request_type.GET)
            {
                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    streamWriter.Write(input);
                    LogMessage($" writedata  {input} ");
                }
            }

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();
                LogMessage($" result:  {result} ");
                output = result;
            }

            if (output.Contains(Checkstr))
                return true;
            else
                return false;

        }

        public override bool Play()
        {
            try
            {
                //
                _sourceProvider.iWidth = Width;
                _sourceProvider.iHeight = Height;
                _sourceProvider.SetPlaybackMode(false);
                _mediaPlayer.Play(_media);

                var stopwatch = Stopwatch.StartNew();  // 計時器開始計時
                int timeout = PlayTimeout; 
                //return true;
                while (true)
                {
                    bool isVideoPlaying = _sourceProvider.HasVideo();

                    if (isVideoPlaying)
                    {
                        return true;                                            
                    }

                    if (stopwatch.ElapsedMilliseconds >= timeout)
                    {
                        return false;  // 如果超過超時時間，退出循環
                    }

                    Thread.Sleep(10);  // 每次迴圈休息10毫秒
                }
            }
            catch (Exception ex)
            {
                LogMessage($"Play VLC Fail.{ex.Message}");
                return false;
            }
        }
        public override bool Stop()
        {
            try
            {
                _sourceProvider.StopVideo();
                return true;
            }
            catch (Exception ex)
            {
                LogMessage($"Stop VLC Fail.{ex.Message}");
                return false;
            }
        }

        public override byte[] CaptureImage()
        {
            byte[] buffer = null;
            int maxRetry = 2; 
            int retryCount = 0;

            while (retryCount <= maxRetry)
            {
                try
                {
                    buffer = _sourceProvider.GetRgbBuffer();

                    if (buffer != null)
                    {
                        return buffer;
                    }
                    else
                    {
                        retryCount++;
                        LogMessage($"CaptureImage Fail. 無法獲取 RGB 緩衝區！重試次數: {retryCount}");
                    }
                }
                catch (Exception ex)
                {
                    retryCount++;
                    LogMessage($"CaptureImage Fail. 重試次數: {retryCount}, Error: {ex.Message}");
                }
            }

            return null;
        }

        public bool IsAllZero(byte[] array)
        {
            return array.AsParallel().All(b => b==0);
        }
        public override bool CaptureImage(byte[] image)
        {
            int maxRetry = 2;
            int retryCount = 0;

            while (retryCount <= maxRetry)
            {
                try
                {
                    bool get_result = _sourceProvider.GetRgbBuffer(image);

                    if (image != null)
                    {
                        if (IsAllZero(image))
                        {
                            LogMessage($"CaptureImage Fail. buffer zero: Retry: {retryCount}");
                            return false;
                        }
                            
                        else
                            return true;
                    }
                    else
                    {
                        retryCount++;
                        LogMessage($"CaptureImage Fail. Can't Get RGB Buffer！Retry: {retryCount}");
                    }
                }
                catch (Exception ex)
                {
                    retryCount++;
                    LogMessage($"CaptureImage Fail. Retry: {retryCount}, Error: {ex.Message}");
                }
            }

            return false;
        }

        public override bool Preview(byte[] rgbBuffer)
        {
            try
            {
                if (rgbBuffer != null)
                {
                    // 創建 Bitmap 並顯示，使用 using 管理 Bitmap 的生命周期
                    using (Bitmap bitmap = CreateBitmapFromBuffer(rgbBuffer))
                    {
                        string tmpY = IQ_SingleEntry.IntensityCheck(rgbBuffer, Width, Height);
                        // 使用 Graphics 在 Bitmap 上繪製文字
                        using (Graphics g = Graphics.FromImage(bitmap))
                        {
                            g.DrawString($"{tmpY}", new Font("Arial", 18), Brushes.Yellow, new PointF(10, 10));
                        }
                        // 清空 input 陣列
                        Array.Clear(rgbBuffer, 0, rgbBuffer.Length);
                        // 使用 Clone 方法來確保 bitmap 的副本
                        Bitmap clonedBitmap = (Bitmap)bitmap.Clone();

                        // 在 UI 線程上更新 PictureBox 圖像
                        DutDashboard.ImagePicturebox.Invoke((Action)(() =>
                        {
                            // 清理先前的圖片
                            DutDashboard.ImagePicturebox.Image?.Dispose();
                            // 設置新的圖片
                            DutDashboard.ImagePicturebox.Image = clonedBitmap;
                        }));

                        return true; // 成功返回
                    }
                }

            }
            catch (Exception ex)
            {
                LogMessage($"PreviewImage Fail. {ex.Message} ");
            }

            return false;
        }


        public Bitmap CreateBitmapFromBuffer(byte[] rgbBuffer)
        {
            // 確保緩衝區大小正確
            int expectedBufferSize = Width * Height * 3; // 每個像素24位（3字節）
            if (rgbBuffer.Length != expectedBufferSize)
            {
                throw new ArgumentException($"RGB buffer size ({rgbBuffer.Length}) does not match expected size ({expectedBufferSize}).");
            }

            Bitmap bitmap = new Bitmap(Width, Height, PixelFormat.Format24bppRgb);
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
        public override bool SaveImage(string strSavePath)
        {
            try
            {
                IQ_SingleEntry.SaveImage(_sourceProvider.GetRgbBuffer(), _sourceProvider.iWidth, _sourceProvider.iHeight, strSavePath);
                return true; // 儲存成功

            }
            catch (Exception ex)
            {
                LogMessage($"SaveImage Fail.{ex.Message}");
                return false;
            }
        }

        public override bool SaveImage(byte[] image,string strSavePath)
        {
            try
            {
                IQ_SingleEntry.SaveImage(image, _sourceProvider.iWidth, _sourceProvider.iHeight, strSavePath);
                return true; // 儲存成功

            }
            catch (Exception ex)
            {
                LogMessage($"Stop VLC Fail.{ex.Message}");
                return false;
            }
        }
    }

}
