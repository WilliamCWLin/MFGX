using LibVLCSharp.Shared;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Threading;
using System;
using Vlc.DotNet.Core.Interops;
using System.Diagnostics;

internal class VlcProvider
{
    public event Action<Graphics> FrameRendered = (_) => { };
    public event Action<Bitmap> FrameChanged = (_) => { };

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

    private readonly MediaPlayer _mediaPlayer;
    private readonly SynchronizationContext _synchronizationContext;

    public VlcProvider(
        MediaPlayer mediaPlayer,
        SynchronizationContext synchronizationContext
    )
    {
        _mediaPlayer = mediaPlayer;
        _mediaPlayer.SetVideoFormatCallbacks(VideoFormat, CleanupVideo);
        _mediaPlayer.SetVideoCallbacks(LockVideo, null, DisplayVideo);
        _synchronizationContext = synchronizationContext;
    }

    public void UnInit()
    {
        RemoveVideo();
    }

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

        _videoWidth = (int)width;
        _videoHeight = (int)height;

        var size = pitches * lines;
        _memoryMappedFile = MemoryMappedFile.CreateNew(null, size);

        var args = new
        {
            // ReSharper disable once RedundantAnonymousTypePropertyName
            width = width,
            // ReSharper disable once RedundantAnonymousTypePropertyName
            height = height,
        };
        _synchronizationContext.Post((state) =>
        {
            _bitmap = new Bitmap((int)args.width, (int)args.height, PixelFormat.Format32bppRgb);
            _bitmapBuffer = new byte[(int)args.width * (int)args.height * 4];
            _graphics = Graphics.FromImage(_bitmap);
        }, null);

        _memoryMappedView = _memoryMappedFile.CreateViewAccessor();
        var viewHandle = _memoryMappedView.SafeMemoryMappedViewHandle.DangerousGetHandle();
        userData = viewHandle;
        return 1;
    }

    /// <summary>
    /// Called by Vlc when it requires a cleanup
    /// </summary>
    /// <param name="userData">The parameter is not used</param>
    private void CleanupVideo(ref IntPtr userData)
    {
        // This callback may be called by Dispose in the Dispatcher thread, in which case it deadlocks if we call RemoveVideo again in the same thread.
        _synchronizationContext.Post((state) => { RemoveVideo(); }, null);
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
            var sw = new Stopwatch();
            sw.Start();
            var bitmapData = _bitmap.LockBits(
                new Rectangle(0, 0, _videoWidth, _videoHeight),
                ImageLockMode.WriteOnly, PixelFormat.Format32bppRgb
            );

            byte* ptr = (byte*)0;
            _memoryMappedView.SafeMemoryMappedViewHandle.AcquirePointer(ref ptr);
            Marshal.Copy(IntPtr.Add(new IntPtr(ptr), 0), _bitmapBuffer, 0, _bitmapBuffer.Length);
            Marshal.Copy(_bitmapBuffer, 0, bitmapData.Scan0, _bitmapBuffer.Length); 

            _bitmap.UnlockBits(bitmapData);

            sw.Stop();
            Debug.WriteLine($"{sw.ElapsedMilliseconds}ms");
            DrawCrossAndCircle(_graphics, _videoWidth, _videoHeight, 0.7f);
            FrameRendered.Invoke(_graphics);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }

        Task.Run(() =>
        {
            var image = _bitmap.Clone(
                new Rectangle(0, 0, _bitmap.Width, _bitmap.Height),
                PixelFormat.Format32bppRgb);

            _synchronizationContext.Post((state) =>
            {
                FrameChanged.Invoke(image); //帧图像已修改
            }, null);
        });
    }

    #endregion
    private void DrawCrossAndCircle(Graphics graphics, int width, int height, float roiRatio)
    {

        float centerX = width / 2f;
        float centerY = height / 2f;

        Pen crossPen = new Pen(Color.Yellow, 5); // 你可以调整颜色和宽度
        graphics.DrawLine(crossPen, centerX - 60, centerY, centerX + 60, centerY); // 水平线
        graphics.DrawLine(crossPen, centerX, centerY - 60, centerX, centerY + 60); // 垂直线


        float roiRadius = (float)Math.Sqrt(Math.Pow(width / 2f, 2) + Math.Pow(height / 2f, 2)) * roiRatio;
        RectangleF circleRect = new RectangleF(centerX - roiRadius, centerY - roiRadius, roiRadius * 2, roiRadius * 2);
        Pen circlePen = new Pen(Color.Blue, 6); // 你可以调整颜色和宽度
        graphics.DrawEllipse(circlePen, circleRect);
    }
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

        _graphics?.Dispose();
        _graphics = null;
        _bitmap?.Dispose();
        _bitmap = null;
    }
}