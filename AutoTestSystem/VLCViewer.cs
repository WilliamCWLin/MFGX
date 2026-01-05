using LibVLCSharp.Shared;
using LibVLCSharp.WinForms;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Vlc.DotNet.Core.Interops;

namespace AutoTestSystem
{
    public partial class VLCViewer : Form
    {
        [DllImport("kernel32.dll", EntryPoint = "RtlMoveMemory")]
        private static extern void CopyMemory(IntPtr Des, IntPtr Src, uint Length);

        private LibVLC _libVLC;
        private MediaPlayer _mediaPlayer;
        private VideoView _videoView;
        private Media _media;

        private IntPtr _buff;
        private int _width;
        private int _height;
        private int _pitch;
        private const int _pixelBytes = 1; // 每像素1字節，表示灰度圖像
        private byte[] _currentFrameBuffer;
        private System.Windows.Forms.Timer _updateTimer;
        private readonly object _bufferLock = new object();
        private MemoryMappedViewAccessor _memoryMappedView;
        private MemoryMappedFile _memoryMappedFile;
        private SynchronizationContext _synchronizationContext;
        private Bitmap _bitmap;
        private Graphics _graphics;
        private IntPtr bufferPtr;
        private int _buffersize;

        private byte[] _latestBuffer;
        // 初始化 Timer

        private System.Windows.Forms.Timer _saveTimer;
        private int _fileIndex = 0; // 用于生成文件名的递增数字
        private readonly VlcProvider _sourceProvider;
        public VLCViewer()
        {
            InitializeComponent();

            // 初始化 LibVLC
            Core.Initialize();

            // 建立 LibVLC 和 MediaPlayer
            var mediaOptions = new string[] { "--network-caching=300", "--no-audio" };
            _libVLC = new LibVLC(mediaOptions);
            _mediaPlayer = new MediaPlayer(_libVLC);
            _mediaPlayer.SetVideoFormatCallbacks(VideoFormat, Cleanup);
            //_mediaPlayer.SetVideoCallbacks(LockCallback, UnlockCallback, DisplayCallback);



            // 設置 RTSP 流的 URL
            var rtspUrl = "rtsp://10.0.0.2/stream1";
            _media = new Media(_libVLC, new Uri(rtspUrl));

            _saveTimer = new System.Windows.Forms.Timer();
            _saveTimer.Interval = 100; // 100 毫秒
            _saveTimer.Tick += SaveTimer_Tick;

            _sourceProvider = new VlcProvider(_mediaPlayer,
SynchronizationContext.Current);
            _sourceProvider.FrameRendered += OnFrameRendered;
            _sourceProvider.FrameChanged += OnFrameChanged;
        }


        protected void OnVideoPlayerLoad(object sender, EventArgs e)
        {
            //base.OnVideoPlayerLoad(sender, e);
            //pictureBoxVideo.SizeMode = PictureBoxSizeMode.StretchImage;
            //pictureBoxVideo.Dock = DockStyle.Fill;
        }

        protected void OnVideoPlayerDestroyed(object sender, EventArgs args)
        {
            //base.OnVideoPlayerDestroyed(sender, args);
            //_sourceProvider.UnInit();
        }
        private Bitmap _frameBitmap;
        private void OnFrameRendered(Graphics graphics)
        {


        }

        private void OnFrameChanged(Bitmap bitmap)
        {
            var image = videoPictureBox.Image;
            videoPictureBox.Image = bitmap;
            image?.Dispose();
        }
        private void SaveTimer_Tick(object sender, EventArgs e)
        {
            // 停止定时器以防止多次触发
            _saveTimer.Stop();

            // 生成文件名
            string fileName = $"D:\\image_{_fileIndex}.bmp";
            SaveBufferAs24BitBmp(fileName);

            // 增加索引
            _fileIndex++;

            // 重新启动定时器
            _saveTimer.Start();
        }
        private uint VideoFormat(ref IntPtr userData, IntPtr chroma, ref uint width, ref uint height, ref uint pitches, ref uint lines)
        {
            FourCCConverter.ToFourCC("RV32", chroma);
            //Correct video width and height according to TrackInfo
            var media = _mediaPlayer.Media;
            if (media != null)
            {
                foreach (LibVLCSharp.Shared.MediaTrack track in media.Tracks)
                {
                    if (track.TrackType == TrackType.Video)
                    {
                        var trackInfo = track.Data.Video;
                        if (trackInfo.Width > 0 && trackInfo.Height > 0)
                        {
                            width = trackInfo.Width;
                            height = trackInfo.Height;
                            if (trackInfo.SarDen != 0)
                            {
                                width = width * trackInfo.SarNum / trackInfo.SarDen;
                            }
                        }

                        break;
                    }
                }
            }

            pitches = GetAlignedDimension((width * 32) / 8, 32);
            lines = GetAlignedDimension(height, 32);
            _pitch = (int)pitches;
            _width = (int)width;
            _height = (int)height;

            var size = pitches * lines;
            _memoryMappedFile = MemoryMappedFile.CreateNew(null, size);

            var args = new
            {
                // ReSharper disable once RedundantAnonymousTypePropertyName
                width = width,
                // ReSharper disable once RedundantAnonymousTypePropertyName
                height = height,
            };
            //_synchronizationContext.Post((state) =>
            //{
            _bitmap = new Bitmap((int)args.width, (int)args.height, PixelFormat.Format32bppRgb);
            _currentFrameBuffer = new byte[(int)args.width * (int)args.height * 4];
            _graphics = Graphics.FromImage(_bitmap);
            //}, null);

            _memoryMappedView = _memoryMappedFile.CreateViewAccessor();
            var viewHandle = _memoryMappedView.SafeMemoryMappedViewHandle.DangerousGetHandle();
            userData = viewHandle;
            return 1;
        }
        private uint GetAlignedDimension(uint dimension, uint mod)
        {
            var modResult = dimension % mod;
            if (modResult == 0)
            {
                return dimension;
            }

            return dimension + mod - (dimension % mod);
        }
        private void Cleanup(ref IntPtr opaque)
        {
            // 釋放視頻緩衝區
            Marshal.FreeHGlobal(_buff);
            _memoryMappedView?.Dispose();
            _memoryMappedView = null;
            _memoryMappedFile?.Dispose();
            _memoryMappedFile = null;

            _graphics?.Dispose();
            _graphics = null;
            _bitmap?.Dispose();
            _bitmap = null;
        }


        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _mediaPlayer.Stop();
            // 釋放資源
            _mediaPlayer.Dispose();
            _libVLC.Dispose();
        }

        private IntPtr LockCallback(IntPtr opaque, IntPtr planes)
        {
            // 將緩衝區指針寫入 planes
            Marshal.WriteIntPtr(planes, opaque);
            return opaque;
        }

        private void UnlockCallback(IntPtr opaque, IntPtr picture, IntPtr planes)
        {
            // 在這裡解鎖視頻幀或進行其他清理操作（如有必要）
        }
        private void UpdateImage(object sender, EventArgs e)
        {
            byte[] buffer = GetBuffer();
            if (buffer != null)
            {
                if (buffer.Length > _buffersize)
                {
                    Marshal.FreeHGlobal(bufferPtr);
                    _buffersize = buffer.Length;
                    bufferPtr = Marshal.AllocHGlobal(_buffersize);
                }
                // 使用 Bitmap 顯示視頻幀

                Marshal.Copy(buffer, 0, bufferPtr, buffer.Length);

                using (Bitmap bmp = new Bitmap(_width, _height, _pitch, PixelFormat.Format32bppRgb, bufferPtr))
                {
                    // 設置調色板為黑白
                    //ColorPalette palette = bmp.Palette;
                    //for (int i = 0; i < 256; i++)
                    //{
                    //    palette.Entries[i] = Color.FromArgb(i, i, i);
                    //}
                    //bmp.Palette = palette;

                    // 將 Bitmap 顯示在 PictureBox 中
                    videoPictureBox.Image = (Bitmap)bmp.Clone();
                }

                //Marshal.FreeHGlobal(bufferPtr);
            }
        }

        private void DisplayCallback(IntPtr opaque, IntPtr picture)
        {
            // 创建 Bitmap，使用现有的缓冲区数据
            var bitmap = new Bitmap((int)_width, (int)_height, (int)_pitch, PixelFormat.Format32bppRgb, picture);

            lock (_bufferLock)
            {

                if (_latestBuffer == null || _latestBuffer.Length != _height * _pitch)
                {
                    _latestBuffer = new byte[_height * _pitch];
                }
                Marshal.Copy(picture, _latestBuffer, 0, _latestBuffer.Length);
            }
            videoPictureBox.Image = bitmap;
            //// 更新 PictureBox 上的图像
            //if (videoPictureBox.InvokeRequired)
            //{
            //    videoPictureBox.Invoke(new Action(() =>
            //    {
            //        // 先释放之前的图片，避免内存泄漏
            //        if (videoPictureBox.Image != null)
            //        {
            //            videoPictureBox.Image.Dispose();
            //        }
            //        videoPictureBox.Image = bitmap;
            //    }));
            //}
            //else
            //{
            //    // 先释放之前的图片，避免内存泄漏
            //    if (videoPictureBox.Image != null)
            //    {
            //        videoPictureBox.Image.Dispose();
            //    }
            //    videoPictureBox.Image = bitmap;
            //}
        }

        public byte[] GetBuffer()
        {
            lock (_bufferLock)
            {
                if (_latestBuffer == null)
                    return null;

                // 返回缓冲区的副本，以避免外部修改
                return (byte[])_latestBuffer.Clone();
            }
        }
        public void SaveBufferAs24BitBmp(string filePath)
        {
            byte[] buffer = GetBuffer();

            if (buffer == null || buffer.Length == 0)
            {
                MessageBox.Show("没有可用的图像数据！");
                return;
            }

            // 创建 24 位的缓冲区
            int bytesPerPixel = 3; // 24 位图像每个像素占用 3 字节
            byte[] buffer24 = new byte[_width * _height * bytesPerPixel];

            // 转换 32 位缓冲区到 24 位缓冲区
            for (int i = 0, j = 0; i < buffer.Length; i += 4, j += 3)
            {
                // 复制 RGB 分量，忽略 Alpha
                buffer24[j] = buffer[i];       // 蓝色分量
                buffer24[j + 1] = buffer[i + 1]; // 绿色分量
                buffer24[j + 2] = buffer[i + 2]; // 红色分量
            }

            // 创建 24 位的 Bitmap
            lock (_bufferLock)
            {
                // Calculate the stride (pitch) for the 24-bit image
                int stride24 = _width * bytesPerPixel;

                // Create a bitmap using the 24-bit buffer
                using (Bitmap bitmap24 = new Bitmap(_width, _height, stride24, PixelFormat.Format24bppRgb, Marshal.UnsafeAddrOfPinnedArrayElement(buffer24, 0)))
                {
                    // 保存为 24 位 BMP 文件
                    bitmap24.Save(filePath, ImageFormat.Bmp);
                }
            }

            //MessageBox.Show("图像已成功保存为 24 位 BMP 到: " + filePath);
        }
        public void SaveBufferAsBmp(string filePath)
        {
            // 获取最新的缓冲区
            byte[] buffer = GetBuffer();

            if (buffer == null || buffer.Length == 0)
            {
                MessageBox.Show("没有可用的图像数据！");
                return;
            }

            // 锁定缓冲区，防止在操作时数据被修改
            lock (_bufferLock)
            {
                // 创建 Bitmap
                using (Bitmap bitmap = new Bitmap((int)_width, (int)_height, (int)_pitch, PixelFormat.Format32bppRgb, Marshal.UnsafeAddrOfPinnedArrayElement(buffer, 0)))
                {
                    // 保存为 BMP 文件
                    bitmap.Save(filePath, ImageFormat.Bmp);
                }
            }

            //MessageBox.Show("图像已成功保存到: " + filePath);
        }


        private void PlayBTN_Click(object sender, EventArgs e)
        {
            // 播放媒體
            _mediaPlayer.Play(_media);

            //_updateTimer.Start();
        }

        private void StopBTN_Click(object sender, EventArgs e)
        {

            _mediaPlayer.Pause();
            _mediaPlayer.Stop();

        }

        private void DisplayBTN_Click(object sender, EventArgs e)
        {
            //SaveBufferAsBmp("D:\\xx.bmp");
            //SaveBufferAs24BitBmp("D:\\xx.bmp");
            _saveTimer.Start();
        }

        private void UninitBTN_Click(object sender, EventArgs e)
        {
            _sourceProvider.UnInit();
        }
    }
}
