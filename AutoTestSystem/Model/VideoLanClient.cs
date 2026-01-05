using LibVLCSharp.Shared;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Vlc.DotNet.Core.Interops;

namespace VideoLanClient
{
    public class VideoLANClient
    {
        public event Action<Graphics> FrameRendered = (_) => { };
        public event Action<Bitmap> FrameChanged = (_) => { };

        public event EventHandler<FrameRenderedEventArgs> FrameBufferRendered;

        /// <summary>
        /// The memory mapped file that contains the picture data
        /// </summary>
        private MemoryMappedFile _memoryMappedFile;

        /// <summary>
        /// The view that contains the pointer to the buffer that contains the picture data
        /// </summary>
        private MemoryMappedViewAccessor _memoryMappedView;

        private int _videoWidth;
        private int _videoHeight;

        private Bitmap _bitmap;
        private Graphics _graphics;
        private byte[] _bitmapBuffer;
        private byte[] _rgbBuffer;
        private readonly MediaPlayer _mediaPlayer;
        private readonly SynchronizationContext _synchronizationContext;
        private readonly object _bufferLock = new object();
        // 新增播放模式標識
        private bool _useFrameEvents = true;
        private bool _display = false;
        // 切換播放模式的方法
        public void SetPlaybackMode(bool useFrameEvents)
        {
            _useFrameEvents = useFrameEvents;
        }
        public string GetPlaybackMode()
        {
            if (_useFrameEvents)
                return "Display";
            else
                return "MFG";
        }
        public int iWidth
        {
            set { _videoWidth = value; }
            get { return _videoWidth; }
        }

        public int iHeight
        {
            set { _videoHeight = value; }
            get { return _videoHeight; }
        }
        public VideoLANClient(
            MediaPlayer mediaPlayer,
            SynchronizationContext synchronizationContext
        )
        {

            _mediaPlayer = mediaPlayer;
            _mediaPlayer.SetVideoFormatCallbacks(VideoFormat, CleanupVideo);
            _mediaPlayer.SetVideoCallbacks(LockVideo, null, DisplayVideo);
            _synchronizationContext = synchronizationContext;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                RemoveVideo(); // 釋放托管資源
            }

            // 釋放非托管資源（如果有的話）
        }

        ~VideoLANClient()
        {
            Dispose(false);
        }
        public void UnInit()
        {
            RemoveVideo();
        }

        public void StopVideo()
        {
            _mediaPlayer.Stop();
            lock (_bufferLock)
            {
                if (_rgbBuffer != null)
                {
                    Array.Clear(_rgbBuffer, 0, _rgbBuffer.Length);
                }
            }
            _display = false;
        }


        //private void EnsureRgbBufferSize(int width, int height)
        //{
        //    int requiredSize = width * height * 3; // 24-bit, 3 bytes per pixel

        //    if (_rgbBuffer == null || _rgbBuffer.Length != requiredSize)
        //    {
        //        _rgbBuffer = new byte[requiredSize];
        //    }
        //}
        //private void ConvertRV32ToRGB24(byte[] rv32Buffer, byte[] rgbBuffer)
        //{
        //    for (int i = 0, j = 0; i < rv32Buffer.Length; i += 3, j += 3)
        //    {
        //        // RV32: R = rv32Buffer[i], G = rv32Buffer[i+1], B = rv32Buffer[i+2]
        //        rgbBuffer[j] = rv32Buffer[i];     // B
        //        rgbBuffer[j + 1] = rv32Buffer[i + 1]; // G
        //        rgbBuffer[j + 2] = rv32Buffer[i + 2];     // R
        //    }
        //}
        #region Vlc video callbacks

        /// <summary>
        /// Called by vlc when the video format is needed. This method allocats the picture buffers for vlc and tells it to set the chroma to RV32
        /// </summary>
        /// <param name="userData">The user data that will be given to the <see cref="LockVideo"/> callback. It contains the pointer to the buffer</param>
        /// <param name="chroma">The chroma</param>
        /// <param name="width">The visible width</param>
        /// <param name="height">The visible height</param>
        /// <param name="pitches">The buffer width</param>
        /// <param name="lines">The buffer height</param>
        /// <returns>The number of buffers allocated</returns>
        private uint VideoFormat(
            // ReSharper disable RedundantAssignment
            ref IntPtr userData,
            IntPtr chroma,
            ref uint width,
            ref uint height,
            ref uint pitches,
            ref uint lines
        ) // ReSharper restore RedundantAssignment
        {
            FourCCConverter.ToFourCC("RV24", chroma);

            width = (uint)_videoWidth;
            height = (uint)_videoHeight;

            pitches = GetAlignedDimension((width * 24) / 8, 32);
            lines = GetAlignedDimension(height, 32);

            var size = pitches * lines;

            var args = new
            {
                width = width,
                height = height,
            };

            // 假設args.width和args.height是新的寬高值
            int newWidth = (int)args.width;
            int newHeight = (int)args.height;

            // 計算新buffer需要的大小
            int newBufferSize = newWidth * newHeight * 3;
            //_synchronizationContext.Post((state) =>
            //{
            try
            {
                if(_bitmap == null)
                {
                    _bitmap = new Bitmap(newWidth, newHeight, PixelFormat.Format24bppRgb);
                    _graphics = Graphics.FromImage(_bitmap);
                }

                // 如果_bitmapBuffer為null或大小不同，則重新分配
                if (_bitmapBuffer == null || _bitmapBuffer.Length != newBufferSize)
                {
                    _bitmapBuffer = new byte[newBufferSize];
                }



                if (_memoryMappedFile == null || _memoryMappedView == null)
                {
                    _memoryMappedFile = MemoryMappedFile.CreateNew(null, size);
                    _memoryMappedView = _memoryMappedFile.CreateViewAccessor();
                    var viewHandle = _memoryMappedView.SafeMemoryMappedViewHandle.DangerousGetHandle();
                    userData = viewHandle;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}bmp null");
            }
            //}, null);
               
            return 1;
        }

        public bool HasVideo()
        {
            if (_mediaPlayer == null)
                return false;

            // 檢查播放器的狀態是否正在播放，並且視頻軌道數量大於0
            if (_mediaPlayer.State == VLCState.Playing && _display == true )
            {
                return true;
            }

            return false;
        }
        public byte[] GetRgbBuffer()
        {
            lock (_bufferLock)
            {
                // 確保 _rgbBuffer 已經初始化並返回一個複製的 buffer
                return _rgbBuffer != null ? (byte[])_rgbBuffer.Clone() : null;
            }
        }

        public bool GetRgbBuffer(byte[] image)
        {
            lock (_bufferLock)
            {
                if(_rgbBuffer != null)
                {
                    Array.Clear(image,0,_rgbBuffer.Length);
                    Array.Copy(_rgbBuffer, image, _rgbBuffer.Length);
                    Array.Clear(_rgbBuffer, 0, _rgbBuffer.Length);
                    return true;
                }

                return false;
            }
        }
        public ReadOnlyMemory<byte> GetRgbBuffer2()
        {
            return _rgbBuffer.AsMemory();
        }
        
        /// <summary>
        /// Called by Vlc when it requires a cleanup
        /// </summary>
        /// <param name="userData">The parameter is not used</param>
        private void CleanupVideo(ref IntPtr userData)
        {
            // This callback may be called by Dispose in the Dispatcher thread, in which case it deadlocks if we call RemoveVideo again in the same thread.
            //_synchronizationContext.Post((state) => { RemoveVideo(); }, null);
        }

        /// <summary>
        /// Called by libvlc when it wants to acquire a buffer where to write
        /// </summary>
        /// <param name="userData">The pointer to the buffer (the out parameter of the <see cref="VideoFormat"/> callback)</param>
        /// <param name="planes">The pointer to the planes array. Since only one plane has been allocated, the array has only one value to be allocated.</param>
        /// <returns>The pointer that is passed to the other callbacks as a picture identifier, this is not used</returns>
        private IntPtr LockVideo(IntPtr userData, IntPtr planes)
        {
            Marshal.WriteIntPtr(planes, userData);
            return userData;
        }

        /// <summary>
        /// Called by libvlc when the picture has to be displayed.
        /// </summary>
        /// <param name="userData">The pointer to the buffer (the out parameter of the <see cref="VideoFormat"/> callback)</param>
        /// <param name="picture">The pointer returned by the <see cref="LockVideo"/> callback. This is not used.</param>
        private unsafe void DisplayVideo(IntPtr userData, IntPtr picture)
        {
            if (_bitmap == null)
            {
                return;
            }
            try
            {

                var bitmapData = _bitmap.LockBits(
                    new Rectangle(0, 0, _videoWidth, _videoHeight),
                    ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb
                );

                byte* ptr = (byte*)0;
                _memoryMappedView.SafeMemoryMappedViewHandle.AcquirePointer(ref ptr);
                Marshal.Copy(IntPtr.Add(new IntPtr(ptr), 0), _bitmapBuffer, 0, _bitmapBuffer.Length);
                //EnsureRgbBufferSize(_videoWidth, _videoHeight);
                lock (_bufferLock)
                {

                    if (_rgbBuffer == null || _rgbBuffer.Length != _bitmapBuffer.Length)
                    {
                        _rgbBuffer = new byte[_bitmapBuffer.Length];
                    }
                    // BGR to RGB conversion
                    for (int i = 0; i < _bitmapBuffer.Length; i += 3)
                    {
                        byte blue = _bitmapBuffer[i];
                        byte green = _bitmapBuffer[i + 1];
                        byte red = _bitmapBuffer[i + 2];

                        _bitmapBuffer[i] = red;
                        _bitmapBuffer[i + 1] = green;
                        _bitmapBuffer[i + 2] = blue;

                        _rgbBuffer[i] = red;
                        _rgbBuffer[i + 1] = green;
                        _rgbBuffer[i + 2] = blue;

                    }

                    Marshal.Copy(_bitmapBuffer, 0, bitmapData.Scan0, _bitmapBuffer.Length);

                    _bitmap.UnlockBits(bitmapData);

                    if (_useFrameEvents)
                    {
                        FrameBufferRendered.Invoke(this, new FrameRenderedEventArgs(_graphics, _rgbBuffer, _videoWidth, _videoHeight));
                    }
                }
                _display = true;
            }
            catch (Exception e)
            {
                MessageBox.Show($"{e.Message}");
            }
            if (_useFrameEvents)
            {
                Task.Run(() =>
                {
                    var image = _bitmap.Clone(
                        new Rectangle(0, 0, _bitmap.Width, _bitmap.Height),
                        PixelFormat.Format24bppRgb);

                    _synchronizationContext.Post((state) =>
                    {

                        FrameChanged.Invoke(image);
                    }, null);
                });
            }
        }

        #endregion

        /// <summary>
        /// Aligns dimension to the next multiple of mod
        /// </summary>
        /// <param name="dimension">The dimension to be aligned</param>
        /// <param name="mod">The modulus</param>
        /// <returns>The aligned dimension</returns>
        private uint GetAlignedDimension(uint dimension, uint mod)
        {
            var modResult = dimension % mod;
            if (modResult == 0)
            {
                return dimension;
            }

            return dimension + mod - (dimension % mod);
        }

        /// <summary>
        /// Removes the video (must be called from the Dispatcher thread)
        /// </summary>
        private void RemoveVideo()
        {
            _memoryMappedView?.Dispose();
            _memoryMappedView = null;
            _memoryMappedFile?.Dispose();
            _memoryMappedFile = null;

            //_synchronizationContext.Post((state) =>
            //{
                _graphics?.Dispose();
                _graphics = null;
                _bitmap?.Dispose();
                _bitmap = null;
            //}, null);

            _bitmapBuffer = null;
            _rgbBuffer = null;
        }
    }

    public class FrameRenderedEventArgs : EventArgs
    {
        public Graphics Graphics { get; }
        public byte[] Buffer { get; }
        public int Width { get; }
        public int Height { get; }
        public FrameRenderedEventArgs(Graphics graphics, byte[] buffer, int width, int height)
        {
            Graphics = graphics;
            Buffer = buffer;
            Width = width;
            Height = height;
        }
    }
}