using AutoTestSystem.Model;
using LibVLCSharp.Shared;
using LibVLCSharp.WinForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using VideoLanClient;
using Vlc.DotNet.Core.Interops;
using ZXing;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Net.Sockets;
using System.Text;
using AutoTestSystem.Equipment.ControlDevice;
using AutoTestSystem.DUT;
using Newtonsoft.Json;
using System.Linq;
using System.Net;
using System.Web;
using static AutoTestSystem.Script.Script_DUT_Camera_Control;
using OpenCvSharp.Features2D;

namespace AutoTestSystem
{

    public partial class View : Form
    {
        [DllImport("kernel32.dll", EntryPoint = "RtlMoveMemory")]
        private static extern void CopyMemory(IntPtr Des, IntPtr Src, uint Length);

        private LibVLC _libVLC = null;
        private MediaPlayer _mediaPlayer = null;
        private Media _media;

        private int _Width = 0;
        private int _Height = 0;
        private float _Field = 0;
        // 初始化 Timer
        private System.Windows.Forms.Timer _statusUpdateTimer; // 用於定時更新狀態的計時器

        private int _fileIndex = 0;
        public VideoLANClient _sourceProvider = null;

        private readonly Font _font;
        private readonly Brush _brush;
        private readonly PointF _point;

        [JsonIgnore]
        [Browsable(false)]
        TcpIpClient MotionCtrlDevice = null;//先寫死
        DUT_CGI_VLC CGI_VLC = null;
        public string MotionDevice { get; set; }
        public string CGI_Device { get; set; }
        public string JSONData { get; set; }

        private string CachedSEPin = "";
        byte[] AlgoBuffer;

        bool success = true;
        string add_sub = "";
        string input = "";
        float X_shift_value;
        float Y_shift_value;
        float Z_shift_value;
        float U_shift_value;
        string plc_cmd = "";
        public string strJsonResult = string.Empty;
        double x_ratio = 0.0;
        double y_ratio = 0.0;
        int request_type;
        int flagMethod = (int)E_request_type.POST;
        string Checkstr = "result";
        string CGICMD;
        string strOutData = string.Empty;
        string output;
        float tcp_degree;
        double X_count = 0;
        double Y_count = 0;
        double U_count = 0;
        double tcp_angle = 0;
        double tcp_degreeX = 0;
        double tcp_degreeY = 0;

        public RichTextBox PIN_RichTextbox
        {
            get { return richTextBox_SEPin; }
            set { richTextBox_SEPin = value; }
        }
        public String PIN_Data
        {
            get { return CachedSEPin; }
            set { CachedSEPin = value; }
        }
        
        public enum Axis : ushort
        {
            X = 0,
            Y = 1,
            Z = 2,
            R = 3
        }
        public View()
        {

            InitializeComponent();
            textBox_w.Text = "2592";
            textBox_h.Text = "1944";
            _font = new Font("Arial", 18);
            _brush = new SolidBrush(Color.Yellow);
            _point = new PointF(10, 10);
            X_Shift.Text = "0";
            Y_Shift.Text = "0";
            Z_Shift.Text = "0";
            U_Shift.Text = "0";
            
            

            // 初始化 LibVLC
            Core.Initialize();
            

            // 建立 LibVLC 和 MediaPlayer
            var mediaOptions = new string[] { "--network-caching=100", "--no-audio" };
            _libVLC = new LibVLC(mediaOptions);
            _mediaPlayer = new MediaPlayer(_libVLC);
            comboBox_url.SelectedIndex = 0;

            // 設置 RTSP 流的 URL
            //var rtspUrl = "rtsp://10.0.0.2/stream1";
            var rtspUrl = comboBox_url.Text;


            _media = new Media(_libVLC, new Uri(rtspUrl));

            _sourceProvider = new VideoLANClient(_mediaPlayer, SynchronizationContext.Current);
            //_sourceProvider.FrameRendered += OnFrameRendered;
            _sourceProvider.FrameBufferRendered += OnBufferFrameRendered;
            _sourceProvider.FrameChanged += OnFrameChanged;

            // 初始化 Timer 控件
            _statusUpdateTimer = new System.Windows.Forms.Timer();
            _statusUpdateTimer.Interval = 50; // 每秒更新一次
            _statusUpdateTimer.Tick += StatusUpdateTimer_Tick;
            _statusUpdateTimer.Start();

            //初始化 Tcp
            LoadAvailableDevices();
            comboBox_Degree.SelectedIndex = 0;
            JSONData_comboBox.SelectedIndex = 0;
            //comboBox_devices.SelectedIndexChanged += ComboBox_devices_SelectedIndexChanged;
            CachedSEPin = richTextBox_SEPin.Text;
            try
            {
                InitTCP();
            }
            catch(Exception ex)
            {
                MessageBox.Show($"InitTCP->{ex.Message}");
            }
            try
            {
                CGI_SEND();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"CGI_SEND->{ex.Message}");
            }


        }
        enum E_request_type : ushort
        {
            GET = 0,
            POST = 1
        }

        public View(string rtsp, string mediaoption,int width,int height)
        {
            InitializeComponent();
            _font = new Font("Arial", 18);
            _brush = new SolidBrush(Color.Yellow);
            _point = new PointF(10, 10);
            _Width = width;
            _Height = height;
            // 初始化 LibVLC
            Core.Initialize();

            // 建立 LibVLC 和 MediaPlayer
            var mediaOptions = mediaoption.Split(';'); ;
            _libVLC = new LibVLC(mediaOptions);
            _mediaPlayer = new MediaPlayer(_libVLC);

            // 設置 RTSP 流的 URL

            _media = new Media(_libVLC, new Uri(rtsp));

            _sourceProvider = new VideoLANClient(_mediaPlayer, SynchronizationContext.Current);
            //_sourceProvider.FrameRendered += OnFrameRendered;
            _sourceProvider.FrameBufferRendered += OnBufferFrameRendered;
            _sourceProvider.FrameChanged += OnFrameChanged;

            // 初始化 Timer 控件
            _statusUpdateTimer = new System.Windows.Forms.Timer();
            _statusUpdateTimer.Interval = 50; // 每秒更新一次
            _statusUpdateTimer.Tick += StatusUpdateTimer_Tick;
            _statusUpdateTimer.Start();

           
        }

        private void StatusUpdateTimer_Tick(object sender, EventArgs e)
        {
            // 確保在 UI 執行緒上執行更新操作
            if (this.InvokeRequired)
            { 
                // 使用 BeginInvoke 將操作排入 UI 執行緒
                this.BeginInvoke(new Action(() =>
                {
                    UpdateStatus();
                }));
            }
            else
            {
                // 如果已經在 UI 執行緒上，直接更新
                UpdateStatus();
            }
        }

        private void UpdateStatus()
        {
            if (_sourceProvider == null)
                return;
            // 檢查 VideoLANClient 是否有影像
            bool isVideoPlaying = _sourceProvider.HasVideo();

            // 更新狀態標籤
            toolStripStatusLabel_videostatus.Text = isVideoPlaying
                ? "Video is playing" : "No video";

            // 如果有影像，顯示影像尺寸
            if (isVideoPlaying)
            {
                int videoWidth = _sourceProvider.iWidth;
                int videoHeight = _sourceProvider.iHeight;
                toolStripStatusLabel_videostatus.Text += $" | Video size: {videoWidth}x{videoHeight}  | Play Mode: {_sourceProvider.GetPlaybackMode()}";
            }
        }
        protected override void OnFormClosing(FormClosingEventArgs e)   
        {
            //Dictionary<string, string> Json = new Dictionary<string, string>();
            //Json.Add("X_ratio", X_ratio_box.Text);
            //Json.Add("Y_ratio", Y_ratio_box.Text);
            //strJsonResult = JsonConvert.SerializeObject(Json, Formatting.Indented);

            base.OnFormClosing(e);
        }

        //private void OnFrameRendered(Graphics graphics)
        //{
        //    // TODO: 處理影像繪製事件
        //    //
            
        //}
        private void OnBufferFrameRendered(object sender, FrameRenderedEventArgs e)
        {
            if(e.Buffer == null)
                return;
            Graphics graphics = e.Graphics;
            byte[] buffer = e.Buffer;

            DrawCrossAndCircle(graphics, _sourceProvider.iWidth, _sourceProvider.iHeight, _Field);

            DrawSFR_ROI(graphics, buffer, e.Width, e.Height);
        }
        private void OnFrameChanged(Bitmap bitmap)
        {
            var image = videoPictureBox.Image;
            videoPictureBox.Image = bitmap;
            image?.Dispose();
        }
 
        public void DrawY(Graphics graphics, byte[] input, int iWidth, int iHeight)
        {
            string output = CalculateY(input, iWidth, iHeight);
            graphics.DrawString(output, _font, _brush, _point);
        }

        Dictionary<string, string> SFRData = new Dictionary<string, string>();
        public void DrawSFR_ROI(Graphics graphics, byte[] ImageBuffer, int iWidth, int iHeight)
        {
            try
            {
                ////======================模擬用(之後拿掉把引數input改命叫ImageBuffer)======================
                string _dllPath = ".\\IQ\\SE_IVS.dll";
                ////==================
                //string imagePath = @"D:\10218_2mFocus_SFR_s608_0db_day_xy3_raw_2592x1944_5184.raw.bmp";
                //int raw_size = (int)(2592 * 1944 * 3);
                //byte[] ImageBuffer = new byte[raw_size];

                //// 讀取 BMP 文件到 byte[] 中
                //using (FileStream fs = new FileStream(imagePath, FileMode.Open, FileAccess.Read))
                //{
                //    fs.Seek(54, SeekOrigin.Begin);
                //    fs.Read(ImageBuffer, 0, raw_size);
                //}

                //=============================================================
                SFRData.Clear();

                // 獲取 byte[] 的指針
                IntPtr start_ptr = Marshal.UnsafeAddrOfPinnedArrayElement(ImageBuffer, 0);

                // 創建一個新 byte[] 來存儲從 IntPtr 複製的數據
                string tmpSFR = IQ_SingleEntry.SFRCheck(_dllPath, CachedSEPin, ImageBuffer, iWidth, iHeight, SFRData);

                graphics.DrawString(tmpSFR, _font, _brush, _point);

                DrawRoisOnBitmap(graphics);
            }
            catch(Exception ex)
            {
                graphics.DrawString($"DrawSFR_ROI. {ex.Message}", _font, _brush, _point);
            }

        }

        public void DrawTX_TY_PixelRatio(Graphics graphics, byte[] input, int iWidth, int iHeight)
        {

        }
            
        Dictionary<string, string> YData = new Dictionary<string, string>();
        public string CalculateY(byte[] input, int iWidth, int iHeight)
        {
            if (input == null)
            {
                // 若 buffer 為 null，直接返回空字串
                return "";
            }

            GCHandle handle = GCHandle.Alloc(input, GCHandleType.Pinned); // 鎖定 input 緩衝區

            try
            {
                IntPtr start_ptr = handle.AddrOfPinnedObject(); // 獲取鎖定後的指標地址
                string str_Address = $"0x{start_ptr.ToString("X")}"; // 轉換為十六進制字符串

                // 替換參數 %buf_addr% 為實際的地址
                string SEPin = "[Action]\nItem=IntensityCheck_BMP\n[Image]\nPath=0\nAddress=%buf_addr%\nWidth=%width%\nHeight=%height%\nFormat=SE_BMP\n[Parameters]\nChannel=3\nImageMode=0\nRAW_BayerPattern=1\nRange_StartFromW=1\nRange_EndFromW=%line%\nDownsample=1\nBMPTarget_Channel_BGRY=4\nSearch_Left_Pixel=0\nSearch_Top_Pixel=0\nSearch_Right_Pixel=%width%\nSearch_Bottom_Pixel=%height%";
                string PIN_tmp = SEPin.Replace("%buf_addr%", str_Address);

                PIN_tmp = PIN_tmp.Replace("%width%", iWidth.ToString());
                PIN_tmp = PIN_tmp.Replace("%height%", iHeight.ToString());
                PIN_tmp = PIN_tmp.Replace("%line%", (iWidth * iHeight).ToString());
               
                YData.Clear();
                string dataout = string.Empty;
                IQ_SingleEntry.SE_StartAction("IQ\\SE_IVS.dll",PIN_tmp, ref dataout, YData); // 執行計算並獲取結果

                double? red = YData.GetFormattedDoubleValue("Result_Check_luminance_Red");
                double? green = YData.GetFormattedDoubleValue("Result_Check_luminance_Green");
                double? blue = YData.GetFormattedDoubleValue("Result_Check_luminance_Blue");
                double? y = YData.GetFormattedDoubleValue("Result_Check_luminance_Gray");

                string rtnOut = $"R({red})\nG({green})\nB({blue})\nY({y})";

                return rtnOut; // 返回計算結果
            }
            catch (Exception ex)
            {
                return $"Exception:{ex.Message}"; // 發生例外時返回空字串
            }
            finally
            {
                // 確保釋放 GCHandle
                if (handle.IsAllocated)
                {
                    handle.Free();
                }
            }
        }

        private void DrawCrossAndCircle(Graphics graphics, int width, int height, float roiRatio)
        {

            float centerX = width / 2f;
            float centerY = height / 2f;

            Pen crossPen = new Pen(Color.Yellow, 2);
            graphics.DrawLine(crossPen, 0, centerY, width, centerY);
            graphics.DrawLine(crossPen, centerX, 0, centerX, height);
            graphics.DrawLine(crossPen, 0, 0, width, height);
            graphics.DrawLine(crossPen, 0, height, width, 0);

            float roiRadius = (float)Math.Sqrt(Math.Pow(width / 2f, 2) + Math.Pow(height / 2f, 2)) * roiRatio;
            RectangleF circleRect = new RectangleF(centerX - roiRadius, centerY - roiRadius, roiRadius * 2, roiRadius * 2);
            Pen circlePen = new Pen(Color.Blue, 6);
            graphics.DrawEllipse(circlePen, circleRect);
        }
        //public void SaveBufferAs24BitBmp(string filePath)
        //{
        //    byte[] buffer = _sourceProvider.GetRgbBuffer();
        //    if (buffer != null)
        //    {
        //        MessageBox.Show("沒有可用的圖像數據！");
        //        return;
        //    }

        //    // 創建 24 位的緩衝區
        //    int bytesPerPixel = 3; // 24 位圖像每個像素佔用 3 字節
        //    byte[] buffer24 = new byte[_sourceProvider.iWidth * _sourceProvider.iHeight * bytesPerPixel];

        //    // 轉換 32 位緩衝區到 24 位緩衝區
        //    for (int i = 0, j = 0; i < buffer.Length; i += 4, j += 3)
        //    {
        //        // 複製 RGB 分量，忽略 Alpha
        //        buffer24[j] = buffer[i];       // 藍色分量
        //        buffer24[j + 1] = buffer[i + 1]; // 綠色分量
        //        buffer24[j + 2] = buffer[i + 2]; // 紅色分量
        //    }

        //    // 創建 24 位的 Bitmap
        //    int stride24 = _sourceProvider.iWidth * bytesPerPixel;

        //    // 使用 24 位緩衝區創建 Bitmap
        //    using (Bitmap bitmap24 = new Bitmap(_sourceProvider.iWidth, _sourceProvider.iHeight, stride24, PixelFormat.Format24bppRgb, Marshal.UnsafeAddrOfPinnedArrayElement(buffer24, 0)))
        //    {
        //        // 保存為 24 位 BMP 文件
        //        bitmap24.Save(filePath, ImageFormat.Bmp);
        //    }

        //    buffer = null;
        //}
        
        public void PlayBTN_Click(object sender, EventArgs e)
        {

            if (int.TryParse(textBox_h.Text, out int result))
            {
                _sourceProvider.iHeight = result;
            }
            if (int.TryParse(textBox_w.Text, out int resultw))
            {
                _sourceProvider.iWidth = resultw;
            }
            // 檢查寬高是否有效
            if (_Width <= 0 || _Height <= 0)
            {
                return ; // 如果寬高無效，則返回 false
            }
            _sourceProvider.SetPlaybackMode(true);

            if (!_sourceProvider.HasVideo())
            {
                // 播放媒體
                bool success = PlayMedia();

                if (!success)
                {
                    MessageBox.Show("播放媒體時發生錯誤。", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            

        }
        public bool Play()
        {

            if (int.TryParse(textBox_h.Text, out int result))
            {
                _sourceProvider.iHeight = result;
            }
            if (int.TryParse(textBox_w.Text, out int resultw))
            {
                _sourceProvider.iWidth = resultw;
            }
            // 檢查寬高是否有效
            if (_Width <= 0 || _Height <= 0)
            {
                return false; // 如果寬高無效，則返回 false
            }
            _sourceProvider.SetPlaybackMode(true);

            if (!_sourceProvider.HasVideo())
            {
                // 播放媒體
                bool success = PlayMedia();

                if (!success)
                {
                    MessageBox.Show("播放媒體時發生錯誤。", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            return true;

        }

        public void Stop()
        {
            bool success = StopMedia();
            if (!success)
            {
                MessageBox.Show("停止媒體時發生錯誤。", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void StopBTN_Click(object sender, EventArgs e)
        {
            bool success = StopMedia();
            if (!success)
            {
                MessageBox.Show("停止媒體時發生錯誤。", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UninitBTN_Click(object sender, EventArgs e)
        {
            bool success = Uninitialize();
            if (!success)
            {
                MessageBox.Show("初始化解除時發生錯誤。", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveBTN_Click(object sender, EventArgs e)
        {
            try
            {
                // 選擇檔案儲存的位置和名稱
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "JPEG Image|*.jpg|Bitmap Image|*.bmp",
                    Title = "儲存圖片為",
                    FileName = $"Image_{DateTime.Now:yyyyMMdd_HHmmssfff}.jpg" // 預設檔名為時間戳
                };

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    // 呼叫 SaveImage 函式，並將選擇的路徑傳入
                    bool success = SaveImage(saveFileDialog.FileName);
                    if (!success)
                    {
                        MessageBox.Show("儲存圖片時發生錯誤。", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                }


            }
            catch
            {
               
            }

        }

        private void DrawY_BTN_Click(object sender, EventArgs e)
        {
            bool success = DrawY();
            if (!success)
            {
                MessageBox.Show("處理Y數據時發生錯誤。", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void X_P_BTN_Click (object sender, EventArgs e)
        {
            if (XY_Change.Checked)
            {
                if (XrevCB.Checked)
                {
                    add_sub = "--TY";
                    input = add_sub + tcp_degree.ToString();
                    Y_shift_value = Convert.ToSingle(Y_Shift.Text) - Convert.ToSingle(comboBox_Degree.Text);
                    if (Y_shift_value < -7)
                    {
                        MessageBox.Show("超出極限", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else if (Y_shift_value == -7)
                    { 
                        Y_shift_value = -7;
                        Y_Shift.Text = Y_shift_value.ToString();
                        success = MotionCtrlDevice.SEND(input);
                    }
                    else
                    {
                        Y_Shift.Text = Y_shift_value.ToString();
                        success = MotionCtrlDevice.SEND(input);
                    }         
                }
                else
                {
                    add_sub = "++TY";
                    input = add_sub + tcp_degree.ToString();
                    Y_shift_value = Convert.ToSingle(Y_Shift.Text) + Convert.ToSingle(comboBox_Degree.Text);
                    if (Y_shift_value >= 7)
                    {
                        MessageBox.Show("超出極限", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else if(Y_shift_value == 7)
                    {
                        Y_shift_value = 7;
                        Y_Shift.Text = Y_shift_value.ToString();
                        success = MotionCtrlDevice.SEND(input);
                    }
                    else
                    {
                        Y_Shift.Text = Y_shift_value.ToString();
                        success = MotionCtrlDevice.SEND(plc_cmd);
                    }        
                }
            }
            else
            {
                if (XrevCB.Checked)
                {
                    add_sub = "--TX";
                    input = add_sub + tcp_degree.ToString();
                    X_shift_value = Convert.ToSingle(X_Shift.Text) - Convert.ToSingle(comboBox_Degree.Text);
                    if (X_shift_value < -8)
                    {
                        MessageBox.Show("超出極限", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else if(X_shift_value == -8)
                    { 
                        X_shift_value = -8;
                        X_Shift.Text = X_shift_value.ToString();
                        success = MotionCtrlDevice.SEND(input);
                    }
                    else
                    {
                        X_Shift.Text = X_shift_value.ToString();
                        success = MotionCtrlDevice.SEND(input);
                    }                  
                }
                else
                {
                    add_sub = "++TX";
                    input = add_sub + tcp_degree.ToString();
                    X_shift_value = Convert.ToSingle(X_Shift.Text) + Convert.ToSingle(comboBox_Degree.Text);
                    if (X_shift_value > 8)
                    {
                        MessageBox.Show("超出極限", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else if(X_shift_value == 8)
                    { 
                        X_shift_value = 8;
                        X_Shift.Text = X_shift_value.ToString();
                        success = MotionCtrlDevice.SEND(input);
                    }
                    else
                    {
                        X_Shift.Text = X_shift_value.ToString();
                        success = MotionCtrlDevice.SEND(input);
                    }
                }
            }

            if (!success)
            {
                MessageBox.Show("連線發生錯誤。", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void X_N_BTN_Click (object sender, EventArgs e)
        { 
            if (XY_Change.Checked)
            {
                if (XrevCB.Checked)
                {
                    add_sub = "++TY";
                    input = add_sub + tcp_degree.ToString();
                    Y_shift_value = Convert.ToSingle(Y_Shift.Text) + Convert.ToSingle(comboBox_Degree.Text);
                    if (Y_shift_value > 7)
                    {
                        MessageBox.Show("超出極限", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else if(Y_shift_value == 7)
                    { 
                        Y_shift_value = 7;
                        Y_Shift.Text = Y_shift_value.ToString();
                        success = MotionCtrlDevice.SEND(input);
                    }
                    else
                    {
                        Y_Shift.Text = Y_shift_value.ToString();
                        success = MotionCtrlDevice.SEND(input);
                    }
                }
                else
                {
                    add_sub = "--TY";
                    input = add_sub + tcp_degree.ToString();
                    Y_shift_value = Convert.ToSingle(Y_Shift.Text) - Convert.ToSingle(comboBox_Degree.Text);
                    if (Y_shift_value < -7)
                    {
                        MessageBox.Show("超出極限", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else if(Y_shift_value == -7)
                    { 
                        Y_shift_value = -7;
                        Y_Shift.Text = Y_shift_value.ToString();
                        success = MotionCtrlDevice.SEND(input);
                    }
                    else
                    {
                        Y_Shift.Text = Y_shift_value.ToString();
                        success = MotionCtrlDevice.SEND(input);
                    }
                }
            }
            else
            {
                if (XrevCB.Checked)
                {
                    add_sub = "++TX";
                    input = add_sub + tcp_degree.ToString();
                    X_shift_value = Convert.ToSingle(X_Shift.Text) + Convert.ToSingle(comboBox_Degree.Text);
                    if (X_shift_value > 8)
                    {
                        MessageBox.Show("超出極限", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else if(X_shift_value == 8)
                    { 
                        X_shift_value = 8;
                        X_Shift.Text = X_shift_value.ToString();
                        success = MotionCtrlDevice.SEND(input);
                    }
                    else
                    {
                        X_Shift.Text = X_shift_value.ToString();
                        success = MotionCtrlDevice.SEND(input);
                    }   
                }
                else
                {
                    add_sub = "--TX";
                    input = add_sub + tcp_degree.ToString();
                    X_shift_value = Convert.ToSingle(X_Shift.Text) - Convert.ToSingle(comboBox_Degree.Text);
                    if (X_shift_value < -8)
                    {
                        MessageBox.Show("超出極限", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else if(X_shift_value == -8)
                    { 
                        X_shift_value = -8;
                        X_Shift.Text = X_shift_value.ToString();
                        success = MotionCtrlDevice.SEND(input);
                    }
                    else
                    {
                        X_Shift.Text = X_shift_value.ToString();
                        success = MotionCtrlDevice.SEND(input);
                    }
                }
            }
            if (!success)
            {
                MessageBox.Show("連線發生錯誤。", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void Y_P_BTN_Click(object sender, EventArgs e)
        {
            if (XY_Change.Checked)
            {
                if (YrevCB.Checked)
                {
                    add_sub = "--TX";
                    input = add_sub + tcp_degree.ToString();
                    X_shift_value = Convert.ToSingle(X_Shift.Text) - Convert.ToSingle(comboBox_Degree.Text);
                    if (X_shift_value < -8)
                    {
                        MessageBox.Show("超出極限", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else if(X_shift_value == -8)
                    { 
                        X_shift_value = -8;
                        X_Shift.Text = X_shift_value.ToString();
                        success = MotionCtrlDevice.SEND(input);
                    }
                    else
                    {
                        X_Shift.Text = X_shift_value.ToString();
                        success = MotionCtrlDevice.SEND(input);
                    }                    
                }
                else
                {
                    add_sub = "++TX";
                    input = add_sub + tcp_degree.ToString();
                    X_shift_value = Convert.ToSingle(X_Shift.Text) + Convert.ToSingle(comboBox_Degree.Text);
                    if (X_shift_value > 8)
                    {
                        MessageBox.Show("超出極限", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else if (X_shift_value == 8)
                    { 
                        X_shift_value = 8;
                        X_Shift.Text = X_shift_value.ToString();
                        success = MotionCtrlDevice.SEND(input);
                    }
                    else
                    {
                        X_Shift.Text = X_shift_value.ToString();
                        success = MotionCtrlDevice.SEND(input);
                    }                    
                }
            }
            else
            {
                if (YrevCB.Checked)
                {
                    add_sub = "--TY";
                    input = add_sub + tcp_degree.ToString();
                    Y_shift_value = Convert.ToSingle(Y_Shift.Text) - Convert.ToSingle(comboBox_Degree.Text);
                    if (Y_shift_value < -7)
                    {
                        MessageBox.Show("超出極限", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else if (Y_shift_value == -7)
                    { 
                        Y_shift_value = -7;
                        Y_Shift.Text = Y_shift_value.ToString();
                        success = MotionCtrlDevice.SEND(input);
                    }
                    else
                    {
                        Y_Shift.Text = Y_shift_value.ToString();
                        success = MotionCtrlDevice.SEND(input);
                    }
                }
                else
                {
                    add_sub = "++TY";
                    input = add_sub + tcp_degree.ToString();
                    Y_shift_value = Convert.ToSingle(Y_Shift.Text) + Convert.ToSingle(comboBox_Degree.Text);
                    if (Y_shift_value > 7)
                    {
                        MessageBox.Show("超出極限", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else if (Y_shift_value == 7) 
                    { 
                        Y_shift_value = 7;
                        Y_Shift.Text = Y_shift_value.ToString();
                        success = MotionCtrlDevice.SEND(input);
                    }
                    else
                    {
                        Y_Shift.Text = Y_shift_value.ToString();
                        success = MotionCtrlDevice.SEND(input);
                    }
                }
            }
            if (!success)
            {
                MessageBox.Show("連線發生錯誤。。", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void Y_N_BTN_Click(object sender, EventArgs e)
        {
            
            if (XY_Change.Checked)
            {
                if (YrevCB.Checked)
                {
                    add_sub = "++TX";
                    input = add_sub + tcp_degree.ToString();
                    X_shift_value = Convert.ToSingle(X_Shift.Text) + Convert.ToSingle(comboBox_Degree.Text);
                    if (X_shift_value > 8)
                    {
                        MessageBox.Show("超出極限", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else if (X_shift_value == 8)
                    { 
                        X_shift_value = 8;
                        X_Shift.Text = X_shift_value.ToString();
                        success = MotionCtrlDevice.SEND(input);
                    }
                    else
                    {
                        X_Shift.Text = X_shift_value.ToString();
                        success = MotionCtrlDevice.SEND(input);
                    }
                }
                else
                {
                    add_sub = "--TX";
                    input = add_sub + tcp_degree.ToString();
                    X_shift_value = Convert.ToSingle(X_Shift.Text) - Convert.ToSingle(comboBox_Degree.Text);
                    if (X_shift_value < -8)
                    {
                        MessageBox.Show("超出極限", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else if (X_shift_value == -8)
                    {
                        X_shift_value = -8;
                        X_Shift.Text = X_shift_value.ToString();
                        success = MotionCtrlDevice.SEND(input);
                    }
                    else
                    {
                        X_Shift.Text = X_shift_value.ToString();
                        success = MotionCtrlDevice.SEND(input);
                    }
                }
            }
            else
            {
                if (YrevCB.Checked)
                {
                    add_sub = "++TY";
                    input = add_sub + tcp_degree.ToString();
                    Y_shift_value = Convert.ToSingle(Y_Shift.Text) + Convert.ToSingle(comboBox_Degree.Text);
                    if (Y_shift_value > 7)
                    {
                        MessageBox.Show("超出極限", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else if(Y_shift_value == 7)
                    {
                        Y_shift_value = 7;
                        Y_Shift.Text = Y_shift_value.ToString();
                        success = MotionCtrlDevice.SEND(input);
                    }
                    else
                    {
                        Y_Shift.Text = Y_shift_value.ToString();
                        success = MotionCtrlDevice.SEND(input);
                    }
                }
                else
                {
                    add_sub = "--TY";
                    input = add_sub + tcp_degree.ToString();
                    Y_shift_value = Convert.ToSingle(Y_Shift.Text) - Convert.ToSingle(comboBox_Degree.Text);
                    if (Y_shift_value < -7)
                    {
                        MessageBox.Show("超出極限", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);    
                    }
                    else if (Y_shift_value == -7)
                    {
                        Y_shift_value = -7;
                        Y_Shift.Text = Y_shift_value.ToString();
                        success = MotionCtrlDevice.SEND(input);
                    }
                    else
                    {
                        Y_Shift.Text = Y_shift_value.ToString();
                        success = MotionCtrlDevice.SEND(input);
                    }
                }
            }
            if (!success)
            {
                MessageBox.Show("連線發生錯誤。。", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void Z_P_BTN_Click(object sender, EventArgs e)
        {
            add_sub = "++TZ";
            input = add_sub + tcp_degree.ToString();
            Z_shift_value = Convert.ToSingle(Z_Shift.Text) + Convert.ToSingle(comboBox_Degree.Text);
            Z_Shift.Text = Z_shift_value.ToString();
            success = MotionCtrlDevice.SEND(input);
            if (!success)
            {
                MessageBox.Show("連線發生錯誤。。", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void Z_N_BTN_Click(object sender, EventArgs e)
        {
            add_sub = "--TZ";
            input = add_sub + tcp_degree.ToString();
            Z_shift_value = Convert.ToSingle(Z_Shift.Text) - Convert.ToSingle(comboBox_Degree.Text);
            Z_Shift.Text = Z_shift_value.ToString();
            success = MotionCtrlDevice.SEND(input);
            if (!success)
            {
                MessageBox.Show("連線發生錯誤。。", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void U_P_BTN_Click(object sender, EventArgs e)
        {


            add_sub = "++TU";
            input = add_sub + tcp_degree.ToString();
            U_shift_value = Convert.ToSingle(U_Shift.Text) + Convert.ToSingle(comboBox_Degree.Text);
            U_Shift.Text = U_shift_value.ToString();
            success = MotionCtrlDevice.SEND(input);
            if (!success)
            {
                MessageBox.Show("連線發生錯誤。。", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void U_N_BTN_Click(object sender, EventArgs e)
        {
            add_sub = "--TU";
            input = add_sub + tcp_degree.ToString();
            U_shift_value = Convert.ToSingle(U_Shift.Text) - Convert.ToSingle(comboBox_Degree.Text);
            U_Shift.Text = U_shift_value.ToString();
            success = MotionCtrlDevice.SEND(input);
            if (!success)
            {
                MessageBox.Show("連線發生錯誤。。", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Save_BTN_Click(object sender, EventArgs e)
        {
            //X_ratio_box.Text = x_ratio.ToString();
            //Y_ratio_box.Text = y_ratio.ToString();
            Dictionary<string, object> Json = new Dictionary<string, object>();
            if (double.TryParse(X_ratio_box.Text, out double xRatio))
            {
                Json.Add("X_ratio", xRatio);
            }
            else
            {
                MessageBox.Show("請輸入有效的 X_ratio 值。");
            }

            if (double.TryParse(Y_ratio_box.Text, out double yRatio))
            {
                Json.Add("Y_ratio", yRatio);
            }
            else
            {
                MessageBox.Show("請輸入有效的 Y_ratio 值。");
            }
            Json.Add("SE_PIN", richTextBox_SEPin.Text);
            strJsonResult = JsonConvert.SerializeObject(Json, Formatting.Indented);

        }
        private void Tele_BTN_Click(object sender, EventArgs e)
        {
            plc_cmd = "MTF/N";
            MotionCtrlDevice.SEND(plc_cmd);
        }
        private void Wide_BTN_Click(object sender, EventArgs e)
        {
            plc_cmd = "MTF/F";
            MotionCtrlDevice.SEND(plc_cmd);
        }
        private void Reset_BTN_Click(object sender, EventArgs e)
        {
            //MotionCtrlDevice.UnInit();
            MotionCtrlDevice.Init();
            plc_cmd = "END";
            MotionCtrlDevice.SEND(plc_cmd);
            X_Shift.Text = "0";
            Y_Shift.Text = "0";
            Z_Shift.Text = "0";
            U_Shift.Text = "0";
            X_count = 0;
            Y_count = 0;
            U_count = 0;

        }
        

        public bool PlayMedia()
        {
            //Show();
            // 播放媒體
            _mediaPlayer.Play(_media);

            //_updateTimer.Start();
            return true; // 播放成功
        }

        public bool StopMedia()
        {
            try
            {
                _statusUpdateTimer.Stop();
                _sourceProvider.StopVideo();
                return true; // 停止成功
            }
            catch
            {
                return false; // 停止失敗
            }
        }

        public bool Uninitialize()
        {
            try
            {
                _statusUpdateTimer.Stop();
                _mediaPlayer.Stop();
                _sourceProvider.UnInit();

                if(_sourceProvider != null)
                {
                    _sourceProvider.FrameChanged -= OnFrameChanged;
                    _sourceProvider.FrameBufferRendered -= OnBufferFrameRendered;
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
            catch
            {
                return false; // 初始化解除失敗
            }
        }

        public bool SaveImage(string path)
        {
            try
            {
                // 呼叫 SaveImage 函式，並將選擇的路徑傳入
                IQ_SingleEntry.SaveImage(_sourceProvider.GetRgbBuffer(), _sourceProvider.iWidth, _sourceProvider.iHeight, path);
                return true; // 儲存成功

            }
            catch
            {
                return false; // 儲存過程發生錯誤
            }
        }

        private bool DrawY()
        {
            try
            {
                // 目前沒有實作任何功能
                return true; // 成功
            }
            catch
            {
                return false; // 發生錯誤
            }
        }

        private void richTextBox_SEPin_TextChanged(object sender, EventArgs e)
        {
            CachedSEPin = richTextBox_SEPin.Text;
        }

        private void View_Load(object sender, EventArgs e)
        {
            //richTextBox_SEPin.Text = "[Action]\nItem=Cal_SFRFor4Field_LabviewFormat_BMP\n[Image]\nPath=0\nAddress=%address%\nWidth=%width%\nHeight=%height%\nFormat=SE_BMP\nRawFormat=0\n[Parameters]\nPixelSizeum=2\nOBValue=30\nROI_Width=68\nROI_Height=52\nSpecFreq=0.125\nField1=0.56\nAngle1=30\nField2=-1\nAngle2=37\nField3=-1\nAngle3=37\nField4=-1\nAngle4=37\nBrightGround=1\nAutoPosition=1\nCorner_Type=0\nCenter_Type=13\nCenter_ROI_Num=1\nChannel=3\nInput_Order=0\nSearch_Pct_x=70\nSearch_Pct_y=70\nBorder_Pct_x=30\nBorder_Pct_y=30\nCorner_Differ_Type=1\nOutputType=1\nDebugMode=0\nOutput_Method=1\n[Others]\nShowLog=0";
            //richTextBox_SEPin.Text = "[Action]\nItem=Cal_SFRFor4Field_LabviewFormat_BMP\n[Image]\nPath=0\nAddress=%address%\nWidth=%width%\nHeight=%height%\nFormat=SE_BMP\n[Parameters]\nPixelSizeum=2\nOBValue=30\nROI_Width=68\nROI_Height=52\nSpecFreq=0.125\nField1=0.6\nAngle1=37\nField2=-1\nAngle2=37\nField3=-1\nAngle3=37\nField4=-1\nAngle4=37\nBrightGround=1\nAutoPosition=1\nCorner_Type=0\nCenter_Type=6\nCenter_ROI_Num=1\nChannel=3\nInput_Order=0\nSearch_Pct_x=85\nSearch_Pct_y=85\nBorder_Pct_x=15\nBorder_Pct_y=15\nCorner_Differ_Type=1\nOutputType=1\nDebugMode=0\nOutput_Method=1\n[Others]\nShowLog=0\n";


            textBox_f.Text = "0.7";
        }

        public bool View_Send(string input,ref string output)
        {
            if (input.Contains("++TU"))
            {
                U_count += tcp_angle;

            }
            else if (input.Contains("--TU"))
            {
                U_count -= tcp_angle;

            }
            else if (input.Contains("++TX"))
            {
                X_count += tcp_degreeX;

            }
            else if (input.Contains("--TX"))
            {
                X_count -= tcp_degreeX;

            }
            else if (input.Contains("++TY"))
            {
                Y_count += tcp_degreeY;

            }
            else if (input.Contains("--TY"))
            {
                Y_count -= tcp_degreeY;

            }

            if (U_count < -2000 || U_count > 2000)
            {
                return false;
            }
            else if (X_count < -800 || X_count >800)
            {
                return false;
            }
            else if (Y_count < -700 || Y_count > 700)
            {
                return false;
            }
            else
            {

                MotionCtrlDevice.SEND(input);
                
                if (!MotionCtrlDevice.READ(ref output))
                {
                    return false;
                }
                else
                {
                    if (output.Contains("OK"))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }

        private async void Action_Click(object sender, EventArgs e)
        {
            
            //
            // 使用 Task 來執行 1000 次迴圈
            await Task.Run(() =>
            {
                ActionTest();
            });
            // 完成後重新啟用按鈕
            //Action.Visible = false;
        }


        public void ActionTest()
        {
            
            while (true)
            {  
                try
                {
                    _sourceProvider.SetPlaybackMode(false);
                    // 確保按鈕點擊後不可用，避免重複點擊
                    Bitmap bitmap = null;
                    // 從 VideoLANClient 中獲取 RGB Buffer
                    byte[] rgbBuffer = _sourceProvider.GetRgbBuffer();
                    Thread.Sleep(500);
                    if (rgbBuffer != null)
                    {
                        string tmpY = CalculateY(rgbBuffer, _Width, _Height);

                        // 創建 Bitmap 並顯示
                        if (bitmap != null)
                        {
                            bitmap.Dispose();
                            bitmap = null;
                        }

                        // 創建 Bitmap
                        bitmap = CreateBitmapFromBuffer(rgbBuffer);

                        using (Graphics g = Graphics.FromImage(bitmap))
                        {
                            g.DrawString($"{tmpY}", new Font("Arial", 18), Brushes.Yellow, new PointF(10, 10));

                            DrawSFR_ROI(g, rgbBuffer, _Width, _Height);
                            //g.DrawString($"這是第 {i + 1} 次示例文字", new Font("Arial", 20), Brushes.Yellow, new PointF(1500, 10));
                        }

                        //進行Tuning測試
                        if (SFRData.ContainsKey("ShiftAndRotate_OC_Shift_X") &&
                            SFRData.ContainsKey("ShiftAndRotate_OC_Shift_Y") &&
                            SFRData.ContainsKey("ShiftAndRotate_Rotate_Shift_Y"))
                            {


                            if (SFRData == null || !SFRData.ContainsKey("ROI_SFR_SFR_Roi_Rule"))
                            {
                                // 字典不存在或不包含 ROI_SFR_SFR_Roi_Rule 的鍵時，輸出錯誤信息並返回
                                //MessageBox.Show("SFRData dictionary is null or does not contain the key 'ROI_SFR_SFR_Roi_Rule'.");
                                Action_Status.Text = "Rule null";
                                continue;
                            }

                            if (SFRData.ContainsKey("Pattern_Center_TL_Pattern_x_y") &&
                               SFRData.ContainsKey("Pattern_Center_TR_Pattern_x_y") &&
                               SFRData.ContainsKey("Pattern_Center_BL_Pattern_x_y") &&
                               SFRData.ContainsKey("Pattern_Center_BR_Pattern_x_y"))
                            {
                                if (SFRData["Pattern_Center_TL_Pattern_x_y"].Contains("-1") ||
                                   SFRData["Pattern_Center_TR_Pattern_x_y"].Contains("-1") ||
                                   SFRData["Pattern_Center_BL_Pattern_x_y"].Contains("-1") ||
                                   SFRData["Pattern_Center_BR_Pattern_x_y"].Contains("-1"))
                                {
                                    MessageBox.Show("Can't find ROI,Manual Fine Tune");
                                    continue;
                                }

                            }
                            else
                            {
                                MessageBox.Show("Can't find ROI Key");
                                break;
                            }

                            float shiftX = int.Parse(SFRData["ShiftAndRotate_OC_Shift_X"]);
                            float shiftY = int.Parse(SFRData["ShiftAndRotate_OC_Shift_Y"]);
                            int rotateShiftY = int.Parse(SFRData["ShiftAndRotate_Rotate_Shift_Y"]);

                            //取出四個角的x,y座標
                            int[] TL_arr = Regex.Split(SFRData["Pattern_Center_TL_Pattern_x_y"], ",").Select(int.Parse).ToArray();
                            int[] TR_arr = Regex.Split(SFRData["Pattern_Center_TR_Pattern_x_y"], ",").Select(int.Parse).ToArray();
                            int[] BL_arr = Regex.Split(SFRData["Pattern_Center_BL_Pattern_x_y"], ",").Select(int.Parse).ToArray();
                            int[] BR_arr = Regex.Split(SFRData["Pattern_Center_BR_Pattern_x_y"], ",").Select(int.Parse).ToArray();

                            //取出TL x,y值
                            double TL_X = TL_arr[0];
                            double TL_Y = TL_arr[1];

                            //取出TR x,y值
                            double TR_X = TR_arr[0];
                            double TR_Y = TR_arr[1];

                            //取出BL x,y值
                            double BL_X = BL_arr[0];
                            double BL_Y = BL_arr[1];

                            double BR_X = BR_arr[0];
                            double BR_Y = BR_arr[1];

                            //計算TL TR atan 與 TL BL atan 
                            double TL_X_atan = Math.Atan(((TL_Y - TR_Y) / (TL_X - TR_X)));
                            double BL_X_atan = Math.Atan(((BL_Y - BR_Y) / (BL_X - BR_X)));
                            double TL_atan_avg =(TL_X_atan + BL_X_atan)/2;
                            //轉換成角度
                            double X_angle = Math.Round((TL_X_atan) * 180 / Math.PI, 2);
                            double Y_angle = Math.Round((BL_X_atan) * 180 / Math.PI, 2);
                            tcp_angle = Math.Round(Math.Abs((X_angle + Y_angle) / 2), 2) * 100;
                            //X Y 偏移量cmd = shift值 * ratio值
                            string X_shift_ratio="";
                            string Y_shift_ratio="";
                            X_ratio_box.Invoke(new Action(() =>
                            {
                                X_shift_ratio = X_ratio_box.Text.ToString();
                            }));
                            Y_ratio_box.Invoke(new Action(() =>
                            {
                                Y_shift_ratio = Y_ratio_box.Text.ToString();
                            }));

                            //float tcp_degreeX = (shiftX / 20) * 100 ;
                            //float tcp_degreeY = (shiftY / 20) * 100 ;
                            
                            tcp_degreeX = shiftX * Math.Abs(double.Parse(X_shift_ratio)) * 100;
                            tcp_degreeY = shiftY * Math.Abs(double.Parse(Y_shift_ratio)) * 100;
                            
                            
                            //double U_count;
                            if (tcp_angle > -2000 && tcp_angle < 2000)
                            {
                                //U軸
                                if (rotateShiftY < -4 || rotateShiftY > 4)
                                {
                                    Invoke((Action)(() =>
                                    {
                                        Action_Status.Text = "U Tuning...";
                                    }));

                                    if (TL_atan_avg > 0)
                                    {
                                        input = "++TU" + tcp_angle.ToString();
                                        
                                    }
                                    else if (TL_atan_avg < 0)
                                    {
                                        input = "--TU" + tcp_angle.ToString();
                                        
                                    }
                                    bool sucess = View_Send(input, ref output);
                                    if ( sucess != true)
                                    {
                                        MessageBox.Show("Out of U range, please click Reset and check the DUT");
                                        break;
                                    }

                                }
                                else
                                {
                                    if(tcp_degreeX > -800 && tcp_degreeX<800)
                                    {
                                        if (shiftX < -4 || shiftX > 4)
                                        {
                                            //X軸
                                            Invoke((Action)(() =>
                                            {
                                                Action_Status.Text = "X Tuning...";
                                            }));
                                            //tcp_degreeX = (shiftX / 20) * 100;
                                            if (XrevCB.Checked)
                                            {
                                                if (tcp_degreeX > 0 && tcp_degreeX < 800)
                                                {
                                                    input = "++TX" + tcp_degreeX.ToString();
                                                    
                                                }
                                                else if (tcp_degreeX < 0 && tcp_degreeX > -800)
                                                {
                                                    tcp_degreeX = Math.Abs(tcp_degreeX);
                                                    input = "--TX" + tcp_degreeX.ToString();
                                                    
                                                }
                                                else if (tcp_degreeX > 800 || tcp_degreeX < -800)
                                                {
                                                    MessageBox.Show("將超出X軸運動極限", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                                }
                                            }
                                            else
                                            {
                                                if (tcp_degreeX > 0 && tcp_degreeX < 800)
                                                {
                                                    input = "--TX" + tcp_degreeX.ToString();
                                                    
                                                }
                                                else if (tcp_degreeX < 0 && tcp_degreeX > -800)
                                                {
                                                    tcp_degreeX = Math.Abs(tcp_degreeX);
                                                    input = "++TX" + tcp_degreeX.ToString();
                                                    
                                                }
                                                else if (tcp_degreeX > 800 || tcp_degreeX < -800)
                                                {
                                                    MessageBox.Show("將超出X軸運動極限", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                                }
                                            }

                                            bool sucess = View_Send(input, ref output);
                                            if ( sucess!= true)
                                            {
                                                MessageBox.Show("Out of X range, please click Reset and check the DUT");
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            if (tcp_degreeY > -700 && tcp_degreeY < 700)
                                            {
                                                if (shiftY < -4 || shiftY > 4)
                                                {
                                                    //Y軸
                                                    Invoke((Action)(() =>
                                                    {
                                                        Action_Status.Text = "Y Tuning...";
                                                    }));
                                                    //tcp_degreeX = (shiftX / 20) * 100;
                                                    if (YrevCB.Checked)
                                                    {
                                                        if (tcp_degreeY > 0 && tcp_degreeY < 700)
                                                        {
                                                            input = "++TY" + tcp_degreeY.ToString();
                                                            
                                                        }
                                                        else if (tcp_degreeY < 0 && tcp_degreeY > -700)
                                                        {
                                                            tcp_degreeY = Math.Abs(tcp_degreeY);
                                                            input = "--TY" + tcp_degreeY.ToString();
                                                            
                                                        }
                                                        else if (tcp_degreeY > 700 || tcp_degreeY < -700)
                                                        {
                                                            MessageBox.Show("將超出Y軸運動極限", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (tcp_degreeY > 0 && tcp_degreeY < 700)
                                                        {
                                                            input = "--TY" + tcp_degreeY.ToString();                                                           
                                                        }
                                                        else if (tcp_degreeY < 0 && tcp_degreeY > -700)
                                                        {
                                                            tcp_degreeY = Math.Abs(tcp_degreeY);
                                                            input = "++TY" + tcp_degreeY.ToString();
                                                        }
                                                        else if (tcp_degreeY > 700 || tcp_degreeY < -700)
                                                        {
                                                            MessageBox.Show("將超出Y軸運動極限", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                                        }
                                                    }

                                                    bool sucess =View_Send(input, ref output);
                                                    if(sucess != true)
                                                    {
                                                        MessageBox.Show("Out of Y range, please click Reset and check the DUT");
                                                        break;
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                MessageBox.Show("Over the PLC Y move limit");
                                                break;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        MessageBox.Show("Over the PLC X move limit");
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                MessageBox.Show("Over the PLC U move limit");
                                break;
                            }
                            Thread.Sleep(500);
                            SFRData.Clear();


                            string _dllPath = ".\\IQ\\SE_IVS.dll";
                            string tmpSFR = IQ_SingleEntry.SFRCheck(_dllPath, CachedSEPin, rgbBuffer, _Width, _Height, SFRData);
                            tmpSFR = IQ_SingleEntry.SFRCheck(_dllPath, CachedSEPin, rgbBuffer, _Width, _Height, SFRData);
                            shiftX = int.Parse(SFRData["ShiftAndRotate_OC_Shift_X"]);
                            shiftY = int.Parse(SFRData["ShiftAndRotate_OC_Shift_Y"]);
                            rotateShiftY = int.Parse(SFRData["ShiftAndRotate_Rotate_Shift_Y"]);
                            if ((rotateShiftY < 6 && rotateShiftY > -6) && (shiftX < 6 && shiftX > -6) && (shiftY < 6 && shiftY > -6))
                            {
                                break;  
                            }

                        }
                        // 清空 input 陣列
                        Array.Clear(rgbBuffer, 0, rgbBuffer.Length);
                        // 在 UI 線程上更新 PictureBox 圖像
                        Invoke((Action)(() =>
                        {
                            videoPictureBox.Image?.Dispose(); // 清理先前的圖片
                            videoPictureBox.Image = bitmap;

                            //bitmap.Dispose();
                        }));

                        // 模擬每次操作的延遲
                        Thread.Sleep(1000); // 可根據需求調整延遲時間
                    }
                    else
                    {

                        throw new Exception("無法獲取 RGB 緩衝區！");
                        //this.Close();
                    }
                    // 確保在循環結束後釋放 Bitmap
                    if (bitmap != null)
                    {
                        bitmap.Dispose();
                        bitmap = null;
                    }
                }
                catch (Exception ex)
                {
                    Invoke((Action)(() =>
                    {
                        Action_Status.Text = "Ready";
                    }));
                    break;
                }
                
               
            }

            _sourceProvider.SetPlaybackMode(true);

            Invoke((Action)(() =>
            {
                Action_Status.Text = "Ready";
            }));
            
        }
        //private Image ByteArrayToImage(byte[] byteArray)
        //{
        //    var bitmapData = bitmap.LockBits(
        //        new Rectangle(0, 0, _sourceProvider.iWidth, _sourceProvider.iHeight),
        //        ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb

        //    );
        //    int stride = bitmapData.Stride;
        //    IntPtr ptr = bitmapData.Scan0;
        //    for(int y = 0;y< _sourceProvider.iHeight; y++)
        //    {
        //        Marshal.Copy(byteArray,y* _sourceProvider.iWidth * 3,ptr+y*stride, _sourceProvider.iWidth*3);
        //    }

        //    bitmap.UnlockBits(bitmapData);
        //    return bitmap;
        //}

        public Bitmap CreateBitmapFromBuffer(byte[] rgbBuffer)
        {
            int width = _sourceProvider.iWidth;
            int height = _sourceProvider.iHeight;

            // 確保緩衝區大小正確
            int expectedBufferSize = width * height * 3; // 每個像素24位（3字節）
            if (rgbBuffer.Length != expectedBufferSize)
            {
                throw new ArgumentException($"RGB buffer size ({rgbBuffer.Length}) does not match expected size ({expectedBufferSize}).");
            }

            Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format24bppRgb);
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

        private void textBox_w_TextChanged(object sender, EventArgs e)
        {
            if (int.TryParse(textBox_w.Text, out int result))
            {
                _Width = result;

            }
            else
            {
                // 如果無法轉換，您可以選擇設定變數為預設值或顯示錯誤消息
                _Width = 0; // 設定預設值
                _sourceProvider.iWidth = 0;
            }

        }

        private void textBox_h_TextChanged(object sender, EventArgs e)
        {
            if (int.TryParse(textBox_h.Text, out int result))
            {

                _Height = result;
            }
            else
            {
                // 如果無法轉換，您可以選擇設定變數為預設值或顯示錯誤消息
                _Height = 0; // 設定預設值
                _sourceProvider.iHeight = 0;
            }

        }

        private void textBox_f_TextChanged(object sender, EventArgs e)
        {
            if (float.TryParse(textBox_f.Text, out float result))
            {
                _Field = result;
            }
            else
            {
                // 如果無法轉換，您可以選擇設定變數為預設值或顯示錯誤消息
                _Field = 0.7F; // 設定預設值
            }
        }

        private void LoadAvailableDevices()
        {
            //comboBox_plcdevices.Items.Clear();
            //comboBox_CGI.Items.Clear();
            foreach(var devicekay in GlobalNew.Devices.Keys)
            {
                if (GlobalNew.Devices[devicekay] is TcpIpClient)
                {
                    comboBox_plcdevices.Items.Add(devicekay);
                }
                if (GlobalNew.Devices[devicekay] is DUT_CGI_VLC)
                {
                    comboBox_CGI.Items.Add(devicekay);
                }
            }

            if (comboBox_plcdevices.Items.Count > 0)
            {
                comboBox_plcdevices.SelectedIndex = 0;
            }
            if (comboBox_CGI.Items.Count > 0)
            {
                comboBox_CGI.SelectedIndex = 0;
                CGI_CMDcomboBox1.SelectedIndex = 0;
            }
        }
        
        private void InitTCP()
        {
            MotionDevice = comboBox_plcdevices.SelectedItem.ToString();

            if (!string.IsNullOrEmpty(MotionDevice))
            {
                if (GlobalNew.Devices.ContainsKey(MotionDevice))
                {
                    var device = GlobalNew.Devices[MotionDevice];
                    if (device is TcpIpClient client)
                    {
                        MotionCtrlDevice = client;
                    }
                    else
                    {
                        MessageBox.Show("Not TCP connect..", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Can't find MotionDevice in GlobalNew.Device。", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }               
            }
            else
            {
                MessageBox.Show("MotionDevices沒有選擇。", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void CGI_SEND()
        {
            CGI_Device = comboBox_CGI.SelectedItem.ToString();
            
            if (!string.IsNullOrEmpty(CGI_Device))
            {
                if (GlobalNew.Devices.ContainsKey(CGI_Device))
                {
                    var device = GlobalNew.Devices[CGI_Device];
                    if (device is DUT_CGI_VLC dut)
                    {
                        CGI_VLC = dut;
                        JSONData = JSONData_comboBox.Text.ToString();
                        CGICMD = CGI_CMDcomboBox1.Text.ToString();
                        //SendCGICommand(1, Checkstr, CGICMD, JSONData);
                    }
                    else
                    {
                        MessageBox.Show("Not VLC connect..", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            
        }
        private bool DrawRoisOnBitmap(Graphics g)
        {
            string[] roiRuleOrder;

            //檢查字典是否存在且包含數據
            if (SFRData == null || !SFRData.ContainsKey("ROI_SFR_SFR_Roi_Rule"))
            {
                // 字典不存在或不包含 ROI_SFR_SFR_Roi_Rule 的鍵時，輸出錯誤信息並返回
                //MessageBox.Show("SFRData dictionary is null or does not contain the key 'ROI_SFR_SFR_Roi_Rule'.");
                return false;
            }

            // 解析 ROI_Rule 順序
            roiRuleOrder = SFRData["ROI_SFR_SFR_Roi_Rule"].Split(',');


            Pen roiPen = new Pen(Color.Yellow, 5);

            // 遍歷字典，找到所有的 ROI 鍵
            foreach (var entry in SFRData)
            {
                if (entry.Key.StartsWith("ROI_") && entry.Key.EndsWith("_Roi"))
                {
                    // 解析 ROI 坐標
                    var roiCoordinates = ParseRoiCoordinates(entry.Value, roiRuleOrder);
                    if (roiCoordinates != null)
                    {
                        // 繪製 ROI
                        g.DrawRectangle(roiPen, roiCoordinates.Value);
                    }
                }
            }


            // 顯示 ShiftAndRotate 相關值在圖像正中間
            if (SFRData.ContainsKey("ShiftAndRotate_OC_Shift_X") &&
                SFRData.ContainsKey("ShiftAndRotate_OC_Shift_Y") &&
                SFRData.ContainsKey("ShiftAndRotate_Rotate_Shift_Y"))
            {
                int shiftX = int.Parse(SFRData["ShiftAndRotate_OC_Shift_X"]);
                int shiftY = int.Parse(SFRData["ShiftAndRotate_OC_Shift_Y"]);
                int rotateShiftY = int.Parse(SFRData["ShiftAndRotate_Rotate_Shift_Y"]);

                // 計算圖像的中心位置
                int centerX = _Width / 2;
                int centerY = _Height / 2;

                // 構建要顯示的文本

                // 設置文本格式
                Font font = new Font("Arial", 30, FontStyle.Bold);
                Brush brush = Brushes.Green;
                StringFormat format = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };

                //模擬用
                //int X_shift_value = 2;
                //int Y_shift_value = 2;

                x_ratio = (double)X_shift_value / shiftX;
                y_ratio = (double)Y_shift_value / shiftY;
                x_ratio = Math.Round(x_ratio,2);
                y_ratio = Math.Round(y_ratio,2);

                string shiftAndRotateText = $"Shift X: {shiftX}\nShift Y: {shiftY}\nRotate Shift Y: {rotateShiftY}  \n x_ratio(d/p):{x_ratio} \n y_ratio(d/p):{y_ratio}";

                //if (X_ratio_box.InvokeRequired)
                //{
                //    // 使用 BeginInvoke 將操作排入 UI 執行緒
                //    X_ratio_box.BeginInvoke(new Action(() =>
                //    {
                //        X_ratio_box.Text = x_ratio.ToString();
                //    }));
                //}
                //else
                //{
                //    // 如果已經在 UI 執行緒上，直接更新
                //    X_ratio_box.Text = x_ratio.ToString();
                //}
                //if (Y_ratio_box.InvokeRequired)
                //{
                //    // 使用 BeginInvoke 將操作排入 UI 執行緒
                //    Y_ratio_box.BeginInvoke(new Action(() =>
                //    {
                //        Y_ratio_box.Text = y_ratio.ToString();
                //    }));
                //}
                //else
                //{
                //    // 如果已經在 UI 執行緒上，直接更新
                //    Y_ratio_box.Text = y_ratio.ToString();
                //}
                
                // 繪製文本在圖像的中心位置
                g.DrawString(shiftAndRotateText, font, brush, new PointF(centerX, centerY), format);
            }
            

            return true;
        }
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

                int width = right - left;
                int height = bottom - top;
                return new Rectangle(left, top, width, height);
            }

            return null;
        }

        private async void Send_CGIcmd_Click(object sender, EventArgs e)
        {
            CGI_SEND();
            await Task.Run(() =>
            {
                try
                {
                    SendCGICommand(1, Checkstr, CGICMD, JSONData, ref output);
                    strOutData = output;
                    //return true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"CGI_SEND->{ex.Message}");
                }
            });      
        }
        public bool SendCGICommand(int request_type, string Checkstr, string CGICMD, string input,ref string output)
        {
            string CGIURL = "http://" + "10.0.0.2" + CGICMD;
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(CGIURL);
            httpWebRequest.ContentType = "application/json";
            if (request_type == (int)E_request_type.GET)
                httpWebRequest.Method = "GET";
            else
                httpWebRequest.Method = "POST";


            if (request_type != (int)E_request_type.GET)
            {
                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    streamWriter.Write(input);
                    //LogMessage($" writedata  {input} ");
                }
            }

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();
                //LogMessage($" result:  {result} ");
                output = result;
            }

            if (output.Contains(Checkstr))
                return true;
            else
                return false;


        }

        private void comboBox_Degree_TextChange(object sender, EventArgs e)
        {
            tcp_degree = Convert.ToSingle(comboBox_Degree.Text) * 100;
        }

        private async void TeachBTN_Click(object sender, EventArgs e)
        {
            await Task.Run(() =>
            {
                Teach();
            });
        }

        public void Teach()
        {
            double tx_dpRatio = 0.0;
            double ty_dpRatio = 0.0;
            try
            {
                _sourceProvider.SetPlaybackMode(false);
                // 確保按鈕點擊後不可用，避免重複點擊
                Bitmap bitmap = null;
                // 從 VideoLANClient 中獲取 RGB Buffer
                int count = 0;
                double firstOCX = 0.0;
                double firstOCY = 0.0;
                for (int i = 1;i<5;i++)
                {


                    
                    // 創建 Bitmap 並顯示
                    if (bitmap != null)
                    {
                        bitmap.Dispose();
                        bitmap = null;
                    }
                    float tcp_degreeX = i * 50;

                    if (tcp_degreeX > 0 && tcp_degreeX < 800)
                    {
                        input = "--TX" + tcp_degreeX.ToString();
                    }
                    else if (tcp_degreeX < 0 && tcp_degreeX > -800)
                    {
                        tcp_degreeX = Math.Abs(tcp_degreeX);
                        input = "++TX" + tcp_degreeX.ToString();
                    }
                    else if (tcp_degreeX > 800 || tcp_degreeX < -800)
                    {
                        MessageBox.Show("將超出X軸運動極限", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                    if (i > 1)
                    {
                        View_Send(input, ref output);
                    }

                    Thread.Sleep(1000);

                    byte[] rgbBuffer = _sourceProvider.GetRgbBuffer();

                    if (rgbBuffer != null)
                    {
                       
                        //float tcp_degreeY = (shiftY / 20) * 100;
                        //X軸


                        // 創建 Bitmap 並顯示
                        if (bitmap != null)
                        {
                            bitmap.Dispose();
                            bitmap = null;
                        }
                        SFRData.Clear();
                        // 創建 Bitmap
                        bitmap = CreateBitmapFromBuffer(rgbBuffer);

                        //IQ_SingleEntry.SaveImage(rgbBuffer, _Width, _Height, $"D:\\{i}(d)");
                        using (Graphics g = Graphics.FromImage(bitmap))
                        {
                            DrawSFR_ROI(g, rgbBuffer, _Width, _Height);                          
                        }

                        //進行Tuning測試
                        if (SFRData.ContainsKey("ShiftAndRotate_OC_Shift_X") &&
                            SFRData.ContainsKey("ShiftAndRotate_OC_Shift_Y") &&
                            SFRData.ContainsKey("ShiftAndRotate_Rotate_Shift_Y"))
                        {



                            float shiftX = int.Parse(SFRData["ShiftAndRotate_OC_Shift_X"]);
                            float shiftY = int.Parse(SFRData["ShiftAndRotate_OC_Shift_Y"]);

                            if (i == 1)
                            {
                                firstOCX = shiftX;
                                
                                continue;
                                
                            }
                            double tx_dpRatio_test = (double)i * 0.5 / (shiftX - firstOCX);
                            tx_dpRatio += tx_dpRatio_test;

                            count++;
                            //X Y 偏移量cmd = shift值 * ratio值


                            //float tcp_degreeX = shiftX / int.Parse(X_ratio_box.Text) * 100;
                            //float tcp_degreeY = shiftY / int.Parse(Y_ratio_box.Text) * 100;

                            SFRData.Clear();


                        }
                        // 清空 input 陣列
                        Array.Clear(rgbBuffer, 0, rgbBuffer.Length);
                        // 在 UI 線程上更新 PictureBox 圖像
                        Invoke((Action)(() =>
                        {
                            videoPictureBox.Image?.Dispose(); // 清理先前的圖片
                            videoPictureBox.Image = bitmap;

                            //bitmap.Dispose();
                        }));

                        // 模擬每次操作的延遲
                        tcp_degreeX = -1 * i * 50;

                        //X軸
                        if (tcp_degreeX > 0 && tcp_degreeX < 800)
                        {
                            input = "--TX" + tcp_degreeX.ToString();
                        }
                        else if (tcp_degreeX < 0 && tcp_degreeX > -800)
                        {
                            tcp_degreeX = Math.Abs(tcp_degreeX);
                            input = "++TX" + tcp_degreeX.ToString();
                        }
                        else if (tcp_degreeX > 800 || tcp_degreeX < -800)
                        {
                            MessageBox.Show("將超出X軸運動極限", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }

                        
                        View_Send(input, ref output);
                        Thread.Sleep(500);

                    }
                    else
                    {

                        throw new Exception("無法獲取 RGB 緩衝區！");
                        //this.Close();
                    }


                }

                tx_dpRatio = (double)tx_dpRatio / count;
                tx_dpRatio = Math.Round(tx_dpRatio, 2);
                tx_dpRatio = Math.Abs(tx_dpRatio);
                X_ratio_box.BeginInvoke(new Action(() =>
                {
                    X_ratio_box.Text = tx_dpRatio.ToString();
                }));

                count = 0;
                for (int i = 1; i < 5; i++)
                {
                    float tcp_degreeY = i * 50;

                    //Y軸
                    if (tcp_degreeY > 0 && tcp_degreeY < 700)
                    {
                        input = "--TY" + tcp_degreeY.ToString();
                    }
                    else if (tcp_degreeY < 0 && tcp_degreeY > -700)
                    {
                        tcp_degreeY = Math.Abs(tcp_degreeY);
                        input = "++TY" + tcp_degreeY.ToString();
                    }
                    else if (tcp_degreeY > 700 || tcp_degreeY < -700)
                    {
                        MessageBox.Show("將超出Y軸運動極限", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    if (i > 1)
                    {
                        View_Send(input, ref output);                       
                    }


                    Thread.Sleep(1000);

                    byte[] rgbBuffer = _sourceProvider.GetRgbBuffer();
                    if (rgbBuffer != null)
                    {


                        
                        // 創建 Bitmap 並顯示
                        if (bitmap != null)
                        {
                            bitmap.Dispose();
                            bitmap = null;
                        }

                        // 創建 Bitmap
                        bitmap = CreateBitmapFromBuffer(rgbBuffer);

                        using (Graphics g = Graphics.FromImage(bitmap))
                        {
                            DrawSFR_ROI(g, rgbBuffer, _Width, _Height);
                        }

                        //進行Tuning測試
                        if (SFRData.ContainsKey("ShiftAndRotate_OC_Shift_X") &&
                            SFRData.ContainsKey("ShiftAndRotate_OC_Shift_Y") &&
                            SFRData.ContainsKey("ShiftAndRotate_Rotate_Shift_Y"))
                        {


                            float shiftY = int.Parse(SFRData["ShiftAndRotate_OC_Shift_Y"]);
                            if (i == 1)
                            {
                                firstOCY = shiftY;
                                continue;
                            }
                            ty_dpRatio += (double)i*0.5 / (shiftY - firstOCY);

                            count++;

                            SFRData.Clear();
                        }
                        // 清空 input 陣列
                        Array.Clear(rgbBuffer, 0, rgbBuffer.Length);
                        // 在 UI 線程上更新 PictureBox 圖像
                        Invoke((Action)(() =>
                        {
                            videoPictureBox.Image?.Dispose(); // 清理先前的圖片
                            videoPictureBox.Image = bitmap;

                            //bitmap.Dispose();
                        }));

                        tcp_degreeY = -1 * i * 50;

                        //Y軸
                        if (tcp_degreeY > 0 && tcp_degreeY < 700)
                        {
                            input = "--TY" + tcp_degreeY.ToString();
                        }
                        else if (tcp_degreeY < 0 && tcp_degreeY > -700)
                        {
                            tcp_degreeY = Math.Abs(tcp_degreeY);
                            input = "++TY" + tcp_degreeY.ToString();
                        }
                        else if (tcp_degreeY > 700 || tcp_degreeY < -700)
                        {
                            MessageBox.Show("將超出Y軸運動極限", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }

                        View_Send(input, ref output);

                        Thread.Sleep(100);
                    }
                    else
                    {

                        throw new Exception("無法獲取 RGB 緩衝區！");
                        //this.Close();
                    }



                }
                ty_dpRatio = (double)ty_dpRatio / count;
                ty_dpRatio = Math.Round(ty_dpRatio, 2);
                ty_dpRatio = Math.Abs(ty_dpRatio);
                Y_ratio_box.BeginInvoke(new Action(() =>
                {
                    Y_ratio_box.Text = ty_dpRatio.ToString();
                }));
                // 確保在循環結束後釋放 Bitmap
                if (bitmap != null)
                {
                    bitmap.Dispose();
                    bitmap = null;
                }
            }
            catch (Exception ex)
            {
                Action_Status.Invoke(new Action(() =>
                {
                    Action_Status.Text = "Ready";
                }));
                
                return;
            }


            

            _sourceProvider.SetPlaybackMode(true);



        }



        

    }



    public static class DictionaryExtensions
    {
        /// <summary>
        /// 安全地取用字典中的值，如果找不到鍵則返回預設值。
        /// </summary>
        /// <typeparam name="TKey">字典的鍵類型</typeparam>
        /// <typeparam name="TValue">字典的值類型</typeparam>
        /// <param name="dictionary">要查詢的字典</param>
        /// <param name="key">要查詢的鍵</param>
        /// <param name="defaultValue">找不到鍵時返回的預設值</param>
        /// <returns>對應的值或預設值</returns>
        public static TValue GetValueOrDefault<TKey, TValue>(
            this Dictionary<TKey, TValue> dictionary,
            TKey key,
            TValue defaultValue = default)
        {
            if (dictionary == null)
            {
                throw new ArgumentNullException(nameof(dictionary), "字典不能為空");
            }

            if (dictionary.TryGetValue(key, out TValue value))
            {
                return value;
            }

            return defaultValue;
        }

        public static double? GetFormattedDoubleValue(this Dictionary<string, string> dictionary, string key)
        {
            if (dictionary.TryGetValue(key, out string value))
            {
                // 嘗試將字串轉換為 double
                if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out double number))
                {
                    // 回傳保留兩位小數的 double 值
                    return Math.Round(number, 2);
                }
            }

            return null; // 若無法解析，返回 null
        }


    }

}
