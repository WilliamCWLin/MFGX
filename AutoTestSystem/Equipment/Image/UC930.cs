using AutoTestSystem.Base;
using DocumentFormat.OpenXml.Wordprocessing;
using Newtonsoft.Json;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static AutoTestSystem.BLL.Bd;
using static System.ComponentModel.TypeConverter;

namespace AutoTestSystem.Equipment.Image
{ 
    public struct FrameInfo
    {
        public byte byChannel;
        public ushort uWidth;
        public ushort uHeight;
        public uint uDataSize;
        public ulong uiTimeStamp;
    };
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct FrameInfoEx
    {
        public byte byChannel;     ///< 图像通道标识，只有UH920/DTLC2支持
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        //[MarshalAs(UnmanagedType.ByValArray)]
        public byte[] resvl;      ///< 保留3字节，填充0
        public byte byImgFormat;   ///< 图像格式，D_RAW8、D_RAW10...
        public ushort uWidth;          ///< 图像的宽度，单位字节
        public ushort uHeight;     ///< 图像的高度，单位字节
        public uint uDataSize;     ///< 数据量大小，单位字节
        public uint uFrameTag;     ///< 功能升级标识
        double fFSTimeStamp;    ///< 帧开始的时间戳
        double fFETimeStamp;    ///< 帧结束的时间戳
        public uint uEccErrorCnt;  ///< 每帧的ECC错误计数，只对MIPI接口有效
        public uint uCrcErrorCnt;  ///< 每帧的CRC错误计数，只对MIPI接口有效
        public uint uFrameID;      ///< 帧计数
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 60)]
        //[MarshalAs(UnmanagedType.ByValArray)]
        public uint[] resv;		///< 保留，填充0
    };
    /* MIPI ctrl 扩展结构体 */
    public struct MipiCtrlEx_t
    {
        /* mipi phy set (d-phy_1.5G/d-phy_2.5G,c-phy_2.0G */
        public byte byPhyType;//1个字节

        /* lane个数设置，d-phy有1/2/4lane,c-phy有1/2/3lane*/
        public byte byLaneCnt;//1个字节

        public byte a;
        public byte b;/*预留2个字节*/

        /* MIPI ctrl */
        //  DWORD dwCtrl;
        public System.UInt32 dwCtrl;//4个字节
        /* 设置接收的图像通道号，0/1/2/3 */
        public System.UInt32 uVc; //4个字节

        /* 使能过滤其他的虚拟通道 */
        public bool bVCFilterEn;//4个字节

        /* 使能输出的ID号 */
        public System.UInt32 uPackID;//4个字节

        /* 使能当前设置的ID号输出 */
        public bool bPackIDEn;//4个字节

        public byte byLp00MinTime;      /// LP00的最小时间，单位是：1个时钟周期1<<8使能手动设置值生效
        public byte byMipiMode;         /// 1是多VC模式（该模式下，可指定哪个VC是帧开始或帧结束），0是普通模式（检测到哪个VC短包先出，就作为帧开始和帧结束）
        public byte bySetFsForVc;       ///指定VC为FS(0,1,2,3)
        public byte bySetFeForVc;       /// 指定VC为FE(0,1,2,3)

        public ushort uLp01MaxTime;     /// LP01最大时间，超过这个时间，会卡控不出图,单位是mipi时钟的1/4

        public byte g;
        public byte h;/*预留2个字节*/

        public ushort uSettleTime;
        /* 保留，填充0 */
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 52)]
        public byte[] resv;

    };
    [StructLayout(LayoutKind.Sequential)]
    public struct ezSensorI2cRw_t
    {
        public System.UInt32 uCtrl;          ///< 控制码,SENSOR_I2C_xxx
        public byte bySlaveAddr;    ///< 从器件地址
        public byte a;
        public byte b;
        public byte c;/*预留3个字节*/
        public IntPtr pWrData;       ///< 写入数据块
       // public byte[] pWrData;
        public System.UInt32 uWrSize;        ///< 写入数据块的字节数
        public IntPtr pRdData;       ///< 读回数据块
       // public byte[] pRdData;
        public System.UInt32 uRdSize;        ///< 读出数据块字节数
           /* 保留，填充0 */
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public System.UInt32[] resv;
    };
    public enum boolean8 : uint
    {
        None = 0x00,
        MIPI_CTRL_LP_EN = 0x01,
        MIPI_CTRL_AUTO_START = 0x02,
        MIPI_CTRL_NON_CONT = 0x04,
        MIPI_CTRL_FULL_CAP = 0x08,
        MIPI_CTRL_CLK_LP_CHK = 0x10,
        MIPI_CTRL_CLK_LP01_CHK = 0x20,
        MIPI_CTRL_DAT_LP01_CHK = 0x40,
    };
    /* 用于配置FrameBuffer */
    // [StructLayout(LayoutKind.Sequential)]
    public struct FrameBufferConfig
    {
        public System.UInt32 uMode;            ///frame buffer模式选择,见BUF_MODE_XXXX
        public System.UInt32 uBufferSize;  /// 设备中的帧缓存大小(字节)，设备固定，用户设置无效
        public System.UInt32 uUpLimit;     /// 设备缓存上限设置(字节)，缓存量超过这个上限时，新的帧将被丢弃
        public System.UInt32 uBufferFrames;  /// 驱动的帧缓存数,只对BUF_MODE_NORMAL模式有效
        public bool bLite;          /// 是否使用紧凑的内存申请方式，ISP使用的内存将不预先申请
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 14)]
        public System.UInt32[] resv;
        //ULONG resv[14];     ///< 保留，填充0
    };
    //FrameFilter_t
    public struct FrameFilter
    {
        /* 对有crc错误的帧过滤 */
        public /*bool*/Byte bCrcErrorFilter;//bool是4字节 byte是一个字节

        /* 对size不匹配的帧过滤 */
        public /*bool*/ Byte bSizeErrorFilter;

        /* 备用 */
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        //[MarshalAs(UnmanagedType.ByValArray)]
        public uint[] resv;		///< 保留填充0
    };
    public enum SENSOR_POWER
    {
        /* A通道，或只有一个通道时 */
        POWER_AVDD = 0,     ///<A通道AVDD
        POWER_DOVDD,        ///<A通道DOVDD
        POWER_DVDD,         ///<A通道DVDD
        POWER_AFVCC,        ///<A通道AFVCC
        POWER_VPP,          ///<A通道VPP
        ////* B通道 */
        POWER_AVDD_B,       ///<B通道AVDD
        POWER_DOVDD_B,      ///<B通道DOVDD
        POWER_DVDD_B,       ///<B通道DVDD
        POWER_AFVCC_B,      ///<B通道AFVCC
        POWER_VPP_B,        ///<B通道VPP

        /* 新增加的电源通道定义 */
        POWER_OISVDD = 10,
        POWER_AVDD2 = 11,
        POWER_AUX1 = 12,
        POWER_AUX2 = 13,
        POWER_VPP2 = 14,

        /* CC16机型定义的电源 */
        POWER_SENSOR0 = 40,
        POWER_SENSOR1 = 41,
        POWER_SENSOR2 = 42,
        POWER_SENSOR3 = 43,
        POWER_VDDIO = 70

    };
    public enum CURRENT_RANGE
    {
        CURRENT_RANGE_MA = 0,    ///<电流测试量程为mA
        CURRENT_RANGE_UA,        ///<电流测试量程为uA
        CURRENT_RANGE_NA         ///<电流测试量程为nA
    };
    public enum RUNMODE
    {
        RUNMODE_PLAY = 0,
        RUNMODE_PAUSE,
        RUNMODE_STOP,
    };
    //SENSOR
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct SensorTab
    {
        public UInt16 aawidth;
        public UInt16 abheight;
        public Byte actype;
        public Byte adpin;
        public Byte aeslaveID;
        public Byte afmode;
        public UInt16 agflagReg;
        public UInt16 ahflagData;
        public UInt16 ajflagMask;
        public UInt16 akflagReg1;
        public UInt16 alflagData1;
        public UInt16 aoflagMask1;
        public UInt16 roi_x;
        public UInt16 roi_y;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string apname;
        [MarshalAs(UnmanagedType.ByValArray)]
        public UInt16[] aqparaList;
        //public UInt32[] aqparaList;
        //public ushort* ParaList;
        public UInt16 arparaListSize;
        [MarshalAs(UnmanagedType.ByValArray)]
        public UInt16[] assleepParaList;
        public UInt16 atsleepParaListSize;
        public UInt16[] aqexpparaList;
        public UInt16 aqexpparaListSize;
        public UInt16[] aqoriparaList;
        public UInt16 aqoriparaListSize;
        public Byte auoutformat;
        public int avmclk;
        public int awavdd;
        public int axdovdd;
        public int aydvdd;
        public int azvpp;
        public int azafvcc;
        public int awavdd2;
        
        public Byte baport;
        public UInt16 bbext0;
        public UInt16 bcext1;
        public UInt16 bdext2;
        [MarshalAs(UnmanagedType.ByValArray)]
        public UInt16[] beaf_InitParaList;
        public UInt16 bfaf_InitParaListSize;
        [MarshalAs(UnmanagedType.ByValArray)]
        public UInt16[] bgaf_AutoParaList;
        public UInt16 bhaf_AutoParaListSize;
        [MarshalAs(UnmanagedType.ByValArray)]
        public UInt16[] bjaf_FarParaList;
        public UInt16 bkaf_FarParaListSize;
        [MarshalAs(UnmanagedType.ByValArray)]
        public UInt16[] blaf_NearParaList;
        public UInt16 boaf_NearParaListSize;
        [MarshalAs(UnmanagedType.ByValArray)]
        public UInt16[] bpexposure_ParaList;
        public UInt16 bqexposure_ParaListSize;
        [MarshalAs(UnmanagedType.ByValArray)]
        public UInt16[] bmgain_ParaList;
        public UInt16 bngain_ParaListSize;
    }
    //  图像格式定义
    public enum IMAGE_FORMAT
    {
        FORMAT_RAW10 = 0x00,
        FORMAT_RAW8 = 0x01,
        FORMAT_YUV = 0x02,
        FORMAT_RAW16 = 0x03,
        FORMAT_RGB565 = 0x04,
        FORMAT_YUV_SPI = 0x05,
        FORMAT_MIPI_RAW10 = 0x06,           ///< 5bytes = 4 pixel...
        FORMAT_MIPI_RAW12 = 0x07,           ///< 3bytes = 2 pixel...
        FORMAT_YUV_MTK_S = 0x08,            ///< MTK output...
        FORMAT_YUV_10 = 0x09,
        FORMAT_YUV_12 = 0x0a,
        FORMAT_MIPI_RAW14 = 0x0b,           ///< 7bytes = 4 pixel...
        FORMAT_SAMSUNG_DVS = 0x010,

        FORMAT_BGR24 = 0x20,                ///< 排列顺序为B，G，R，各8bit
        FORMAT_BGR32 = 0x21,                ///< 排列顺序为B，G，R，0各8bit
        FORMAT_P10 = 0x24,                  ///< 一个像素占两个字节，LSB，0～1023，一般用于MIPI_RAW10转换
        FORMAT_P12 = 0x25,                  ///< 一个像素占两个字节，LSB，0～4095，一般用于MIPI_RAW12转换
        FORMAT_P14 = 0x26,                  ///< 一个像素占两个字节，LSB，0～16383，一般用于MIPI_RAW14转换
        FORMAT_G8 = 0x28,                   ///< 只取G值，8bit
        FORMAT_G10 = 0x29,                  ///< 只取G值，16bit
        FORMAT_GRAY8 = 0x2a,
    };
    //ROI结构体描述
    public struct DtRoi_t
    {
        public uint x;         ///< ROI起始点X坐标值
        public uint y;         ///< ROI起始点Y坐标值
        public uint w;         ///< ROI宽度
        public uint h;         ///< ROI高度
    }
    /// 支持的RAW格式。
    public enum RAW_FORMAT
    {
        RAW_RGGB = 0,   ///<RAW格式按RGGB排列
        RAW_GRBG,           ///<RAW格式按GRBG排列
        RAW_GBRG,           ///<RAW格式按GBRG排列
        RAW_BGGR,           ///<RAW格式按BGGR排列
    }
    /// 支持的YUV格式。
    public enum YUV_FORMAT
    {
        YUV_YCBYCR = 0,     ///<YUV格式按YCBYCR排列
        YUV_YCRYCB,         ///<YUV格式按YCRYCB排列
        YUV_CBYCRY,         ///<YUV格式按CBYCRY排列
        YUV_CRYCBY,         ///<YUV格式按CRYCBY排列
    };
    //图像数据结构体描述
    [StructLayout(LayoutKind.Sequential)]
    public struct DtImage_t
    {
        public IMAGE_FORMAT format;      ///< 图像格式
        public RAW_FORMAT rawFmt;      ///< RAW格式细节
        public YUV_FORMAT yuvFmt;      ///< YUV格式细节
        public uint width;       ///< 图像尺寸
        public uint height;         ///< 图像尺寸
        public IntPtr data;       ///< 图像数据
        public uint dataSize;    ///< buffer空间大小
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public uint[] resv;     ///< 保留32字节

        public DtImage_t(bool initArray) : this()
        {
            if (initArray)
            {
                InitArray();
            }
        }
        public void InitArray()
        {
            resv = new uint[8];
        }

    }
    internal class UC930 : Image_Base
    {


        [DllImport("ImgConverter.dll", EntryPoint = "FastRawToColor", CallingConvention = CallingConvention.StdCall)]
        public static extern void FastRawToColor(IntPtr src, int iW_pxl, int iH_pxl, int iAlignMode, int iBits, int iCh, IntPtr ppBMP);


        IntPtr GrabBuffer = IntPtr.Zero;
        IntPtr m_TripleBuffer = IntPtr.Zero;
        IntPtr m_Raw8Buffer = IntPtr.Zero;
        IntPtr Raw_buffer = IntPtr.Zero;
        IntPtr BMP_buffer = IntPtr.Zero;
        IntPtr m_avgBuffer = IntPtr.Zero;

        public int m_iDevID = 0;
        //public ushort m_uWidth = 56;
        //public ushort m_uHeight = 192;
        //public uint m_nMemSize = 0;
        //public uint m_um_FrameCnt = 0;
        //public uint m_uErrm_FrameCnt = 0;
        //public double m_fFramefps = 0;

        //public bool m_b_thread_start = false;
        //public bool m_b_display_thread_start = false;
        public FrameInfo m_FrameInfo = new FrameInfo();
        //public FrameInfoEx m_FrameEXInfo = new FrameInfoEx();

        //public uint m_FrameCnt = 0;
        //public uint m_PlayCnt = 0;

        //public byte[] m_TripleBuffer = new byte[1];
        //public IntPtr m_DisplyBuffer = new IntPtr();


        //public Boolean m_bDisply = false;
        //public Boolean m_bTriple = false;
        //public Boolean m_bGrab = false;

        private int _deviceID = -1;
        private const int DT_ERROR_FAILED = 0;
        private const int DT_ERROR_OK = 1;
        private const int DT_ERROR_TIMEOUT = 2;
        private const int DT_ERROR_IN_PROCESS = 2;
        private const int DT_ERROR_WAIT = 3;
        private const int DT_ERROR_BUSY = 4;

        private float[] _setpower = new float[5] { 0.0F, 0.0F, 0.0F, 0.0F, 0.0F };

        private SensorTab _sensorTab = new SensorTab();
        private uint grabSize = 0;

        private UInt16[] paraList;
        private ushort paraListSize = 0;
        private List<string> regSection;
        //private IntPtr _grabHandle = IntPtr.Zero;
        //private int m_RunMode = (int)RUNMODE.RUNMODE_STOP;
        //private bool flag = false;

        private string[] m_DevName = null;
        private static List<string> enumList = null;
        private static int DevNum = 0;
        public Dictionary<string, string> _paramInfo = new Dictionary<string, string>();
        IntPtr[] RcvArray = null;
        

        [Category("Params"), Description("Select Dothink_Device"), TypeConverter(typeof(Image_DevList))]
        public string DeviceName { get; set; }
        [Category("Parameter"), Description("Profile Path"), Editor(typeof(Manufacture.FileSelEditorRelPath), typeof(System.Drawing.Design.UITypeEditor))]
        public string IniPath { get; set; }

        [Category("Paramter"), Description("Choose the Bayer pattern"), TypeConverter(typeof(Bayer_Mode))]
        public string BayerPattern { get; set; } = "BGGR";


        public enum Fun_enum
        {
            I2C,
            ICR,
        }
        public UC930()
        {
            int enumCount = Enum.GetValues(typeof(Fun_enum)).Length;
            Function.Clear();
            foreach (var value in Enum.GetValues(typeof(Fun_enum)))
            {
                if (Function.Count < enumCount)
                {
                    Function.Add(value.ToString());
                }
            }
        }
        public override void Dispose()
        {
            throw new NotImplementedException();
        }
        public override bool Capture(ref string PtrBuffer)
        {

            int mod_bayer = 0;
            if (BayerPattern == "RGGB")
                mod_bayer = 1;
            else if (BayerPattern == "GRBG")
                mod_bayer = 2;
            else if (BayerPattern == "GBRG")
                mod_bayer = 3;
            else if (BayerPattern == "BGGR")
                mod_bayer = 4;


            DTCCM2_API.OpenVideo(Convert.ToUInt32(grabSize), _deviceID);
            int Size = _sensorTab.aawidth * _sensorTab.abheight * 3;
            //IntPtr m_TripleBuffer = Marshal.AllocHGlobal(Size);      //pBmp
            uint uGrabSize = 0;
            int bRet;
            if (DTCCM2_API.CalculateGrabSize(ref grabSize, _deviceID) == DT_ERROR_OK)
            {
                bRet = DTCCM2_API.GrabFrame(m_TripleBuffer, grabSize, ref uGrabSize, ref m_FrameInfo, _deviceID);
                if (bRet == 0)
                {
                    LogMessage($"Grab Fail, Error code = {DTCCM2_API.GrabFrame(m_TripleBuffer, grabSize, ref uGrabSize, ref m_FrameInfo, _deviceID)}", MessageLevel.Error);
                    return false;
                }
                if (bRet == 2)
                {
                    LogMessage($"Grab Time Out, Error code = {DTCCM2_API.GrabFrame(m_TripleBuffer, grabSize, ref uGrabSize, ref m_FrameInfo, _deviceID)}", MessageLevel.Error);
                    return false;
                }
                if (bRet == 1)
                {
                    LogMessage("Grab Success", MessageLevel.Debug);
                    //GrabBuffer = Marshal.AllocHGlobal(Size); // debug no use
                    //imageBuffer = new byte[Size];   //pBmp2 // debug no use

                    
                    /// FOR RAW BUFFER to SaveImage
                    Raw_buffer = m_TripleBuffer;

                    //m_Raw8Buffer = ConvertRaw10ToRaw8(m_TripleBuffer, (int)(grabSize / _sensorTab.abheight), _sensorTab.abheight);
                    ConvertRaw10ToRaw8(m_TripleBuffer, (int)(grabSize / _sensorTab.abheight), _sensorTab.abheight, m_Raw8Buffer);
                    FastRawToColor(m_Raw8Buffer, _sensorTab.aawidth, _sensorTab.abheight, mod_bayer, 8, 1, GrabBuffer);
                    string str_Address = $"0x{GrabBuffer.ToString("X")}";
                    PtrBuffer = str_Address;
                    //Marshal.Copy(GrabBuffer, imageBuffer, 0, Size);//拷贝数据

                    DTCCM2_API.CloseVideo(_deviceID);
                    /*DTCCM2_API.ImageProcess(m_Raw8Buffer, GrabBuffer, _sensorTab.aawidth, _sensorTab.abheight, ref m_FrameInfo, _deviceID);*/  //dothink method no use
                    GrabBuffer = ConvertRGBToBGR(GrabBuffer, _sensorTab.abheight, _sensorTab.aawidth);

                    /// FOR BMP BUFFER to SaveImage
                    /* BMP_buffer = GrabBuffer;*/  // use memory copy so mark this 

                    unsafe
                    {
                        int bufferSize = Size;
                        Buffer.MemoryCopy((void*)GrabBuffer, (void*)BMP_buffer, bufferSize, bufferSize);
                    }


                    return true;
                }
                else
                {
                    LogMessage("Grab Error", MessageLevel.Error);
                    return false;
                }
            }
            return false;
        }

        public override bool Capture(ref string PtrBuffer ,int Avg_count)
        {

            int mod_bayer = 0;
            if (BayerPattern == "RGGB")
                mod_bayer = 1;
            else if (BayerPattern == "GRBG")
                mod_bayer = 2;
            else if (BayerPattern == "GBRG")
                mod_bayer = 3;
            else if (BayerPattern == "BGGR")
                mod_bayer = 4;

            
            DTCCM2_API.OpenVideo(Convert.ToUInt32(grabSize), _deviceID);
            int Size = _sensorTab.aawidth * _sensorTab.abheight * 3;
            //IntPtr m_TripleBuffer = Marshal.AllocHGlobal(Size);      //pBmp
            uint uGrabSize = 0;
            int bRet;
            if (DTCCM2_API.CalculateGrabSize(ref grabSize, _deviceID) == DT_ERROR_OK)
            {
                for (int i = 0; i < Avg_count; i++)
                {
                    bRet = DTCCM2_API.GrabFrame(m_TripleBuffer, grabSize, ref uGrabSize, ref m_FrameInfo, _deviceID);
                    if (bRet == 0)
                    {
                        LogMessage($"Grab Fail, Error code = {DTCCM2_API.GrabFrame(m_TripleBuffer, grabSize, ref uGrabSize, ref m_FrameInfo, _deviceID)}", MessageLevel.Error);
                        return false;
                    }
                    if (bRet == 2)
                    {
                        LogMessage($"Grab Time Out, Error code = {DTCCM2_API.GrabFrame(m_TripleBuffer, grabSize, ref uGrabSize, ref m_FrameInfo, _deviceID)}", MessageLevel.Error);
                        return false;
                    }
                    if (bRet == 1)
                    {
                        LogMessage("Grab Success", MessageLevel.Debug);
                        
                    }
                    else
                    {
                        LogMessage($"Grab Error, Error code = {DTCCM2_API.GrabFrame(m_TripleBuffer, grabSize, ref uGrabSize, ref m_FrameInfo, _deviceID)}", MessageLevel.Error);
                        return false;
                    }
                }
                /// FOR RAW BUFFER to SaveImage
                Raw_buffer = m_TripleBuffer;
                /// FOR RAW BUFFER to SaveImage

                ConvertRaw10ToRaw8(m_TripleBuffer, (int)(grabSize / _sensorTab.abheight), _sensorTab.abheight, m_Raw8Buffer);
                FastRawToColor(m_Raw8Buffer, _sensorTab.aawidth, _sensorTab.abheight, mod_bayer, 8, 1, GrabBuffer);
                string str_Address = $"0x{GrabBuffer.ToString("X")}";
                PtrBuffer = str_Address;


                DTCCM2_API.CloseVideo(_deviceID);

                GrabBuffer = ConvertRGBToBGR(GrabBuffer, _sensorTab.abheight, _sensorTab.aawidth);

                /// FOR BMP BUFFER to SaveImage
                BMP_buffer = GrabBuffer;
                return true;
            }
            else
                return false;
        }

        public override bool SaveImage(int Format_Mode, string strSavePath)
        {
            int bufferSize = 0;
            byte[] managedBuffer;
            if (Format_Mode == 0)
            {
                bufferSize = (_sensorTab.aawidth * 5 / 4) * _sensorTab.abheight;
                managedBuffer = new byte[bufferSize];
                Marshal.Copy(Raw_buffer, managedBuffer, 0, bufferSize);
                File.WriteAllBytes(strSavePath, managedBuffer);
            } //Raw buffer to save image
            else if(Format_Mode ==1)
            {
                using (Bitmap checkbitmap = CreateBitmapFromIntPtr(BMP_buffer, _sensorTab.aawidth, _sensorTab.abheight, PixelFormat.Format24bppRgb))
                {
                    //checkbitmap.Save(strSavePath, ImageFormat.Bmp);
                    using (var fs = new FileStream(strSavePath, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        checkbitmap.Save(fs, ImageFormat.Bmp);
                    }
                }


            }

            return true;
        }

        public override bool Init(string strParamInfo)
        {
            EnumerateDev();
            if (DevNum>0)
            {
                if (ReadParamFile(IniPath))
                {
                    Image_Width = _sensorTab.aawidth;
                    Image_Height = _sensorTab.abheight;
                    m_TripleBuffer = Marshal.AllocHGlobal(Image_Width * Image_Height * 3);
                    //m_TripleBuffer = Marshal.AllocHGlobal(1920 * 1536);
                    GrabBuffer = Marshal.AllocHGlobal(Image_Width * Image_Height * 3);
                    BMP_buffer = Marshal.AllocHGlobal(Image_Width * Image_Height * 3);
                    m_avgBuffer = Marshal.AllocHGlobal(Image_Width * Image_Height * 3);
                    int pixel_count = Image_Width * Image_Height;
                    m_Raw8Buffer = Marshal.AllocHGlobal(pixel_count);
                    Image_input_str = strParamInfo;
                }
                else
                    return false;
                LogMessage("UC930 Init Success", MessageLevel.Debug);
                return true;
            }
            else
            {
                LogMessage("UC930 Init Fail", MessageLevel.Error);
                return false;
            }
        }
        public override bool Start()
        {
            if (DTCCM2_API.OpenDevice(m_DevName[0], ref _deviceID, DevNum) != DT_ERROR_OK)
            {
                Logger.Debug($"OpenDevice is faild, Error code = {DTCCM2_API.OpenDevice(m_DevName[0], ref _deviceID, DevNum)}");
                return false;
            }

            if (DTCCM2_API.SensorEnable(0, false, _deviceID) != DT_ERROR_OK)
            {
                Logger.Debug($"SensorEnable to false fail, Error code = {DTCCM2_API.SensorEnable(0, false, _deviceID)}");
                return false;

            }

            if (DTCCM2_API.SetSoftPinPullUp(false, _deviceID) != DT_ERROR_OK)
            {
                Logger.Debug($"SetSoftPinPullUp to down fail,  Error code = {DTCCM2_API.SetSoftPinPullUp(false, _deviceID)}");
                return false;
            }

            if (DTCCM2_API.SetSensorClock(false, (ushort)0, _deviceID) != DT_ERROR_OK)
            {
                Logger.Debug($"SetSensorClock to false fail, Error code = {DTCCM2_API.SetSensorClock(false, (ushort)0, _deviceID)}");
                return false;
            }
           
            SENSOR_POWER[] power = new SENSOR_POWER[5];
            power[0] = SENSOR_POWER.POWER_AVDD;
            power[1] = SENSOR_POWER.POWER_DOVDD;
            power[2] = SENSOR_POWER.POWER_DVDD;
            power[3] = SENSOR_POWER.POWER_AFVCC; 
            if (0x0590 == DTCCM2_API.GetKitType(_deviceID))
            {
                power[4] = SENSOR_POWER.POWER_VPP2;
            }
            else if (0x0591 == DTCCM2_API.GetKitType(_deviceID))
            {
                power[4] = SENSOR_POWER.POWER_VPP2;
            }
            else if (0x0593 == DTCCM2_API.GetKitType(_deviceID))
            {
                power[4] = SENSOR_POWER.POWER_VPP2;
            }
            else if (0x0594 == DTCCM2_API.GetKitType(_deviceID))
            {
                power[4] = SENSOR_POWER.POWER_VPP2;
            }
            else if (0x0534 == DTCCM2_API.GetKitType(_deviceID))
            {
                power[4] = SENSOR_POWER.POWER_VPP2;
            }
            else if (0x0595 == DTCCM2_API.GetKitType(_deviceID))
            {
                power[4] = SENSOR_POWER.POWER_VPP2;
            }
            else
            {
                power[4] = SENSOR_POWER.POWER_VPP2;
            }
            int[] volt = new int[5] { 0, 0, 0, 0, 0 };
            //int[] current = new int[5] { 300, 300, 300, 300, 400 };
            int[] current = new int[5] { 500, 500, 500, 500, 500 };
            bool[] onoff = new bool[5] { true, true, true, true, true };
            bool[] off = new bool[5] { false, false, false, false, false };
            CURRENT_RANGE[] range = new CURRENT_RANGE[10];
            range[0] = CURRENT_RANGE.CURRENT_RANGE_MA;
            range[1] = CURRENT_RANGE.CURRENT_RANGE_MA;
            range[2] = CURRENT_RANGE.CURRENT_RANGE_MA;
            range[3] = CURRENT_RANGE.CURRENT_RANGE_MA;
            range[4] = CURRENT_RANGE.CURRENT_RANGE_MA;

            if (DTCCM2_API.PmuSetCurrentRange(power, range, 5, _deviceID) != DT_ERROR_OK)
            {
                Logger.Debug($"PmuSetCurrentRange fail, Error code = {DTCCM2_API.PmuSetCurrentRange(power, range, 5, _deviceID)}");
                return false;
            }
            if (DTCCM2_API.PmuSetOcpCurrentLimit(power, current, 5, _deviceID) != DT_ERROR_OK)
            {
                Logger.Debug($"PmuSetOcpCurrentLimit fail, Erro code = {DTCCM2_API.PmuSetOcpCurrentLimit(power, current, 5, _deviceID)}");
                return false;
            }
            if (DTCCM2_API.PmuSetVoltage(power, volt, 5, _deviceID) != DT_ERROR_OK)
            {
                Logger.Debug($"PmuSetVoltage to 0 fail, Error code = {DTCCM2_API.PmuSetVoltage(power, volt, 5, _deviceID)}");
                return false;
            }
            Thread.Sleep(50);
            if (DTCCM2_API.PmuSetOnOff(power, off, 5, _deviceID) != DT_ERROR_OK)
            {
                Logger.Debug($"PmuSetOnOff to off fail, Error code = {DTCCM2_API.PmuSetOnOff(power, off, 5, _deviceID)}");
                return false;
            }

            //Thread.Sleep(10);
            //for (int i = 0; i < 5; i++)
            //{
            //    volt[i] = 0;
            //}
            //if (DTCCM2_API.PmuSetVoltage(power, volt, 5, _deviceID) != DT_ERROR_OK)
            //{
            //    Logger.Debug("PmuSetVoltage to on");
            //    return false;
            //}


            if (DTCCM2_API.PmuSetOnOff(power, onoff, 5, _deviceID) != DT_ERROR_OK)
            {
                Logger.Debug($"PmuSetOnOff to on fail, Error code = {DTCCM2_API.PmuSetOnOff(power, onoff, 5, _deviceID)}");
                return false;
            }


            Thread.Sleep(50);
            for (int i = 0; i < 5; i++)
            {
                volt[i] = Convert.ToInt32(_setpower[i] * 1000);
                if (DTCCM2_API.PmuSetVoltage(power, volt, 5, _deviceID) != DT_ERROR_OK)
                {
                    Logger.Debug($"PmuSetVoltage to on fail, Error code = {DTCCM2_API.PmuSetVoltage(power, volt, 5, _deviceID)}");
                    return false;
                }
                Thread.Sleep(50);
            }

            

            ///
            //if (DTCCM2_API.PmuSetVoltage(power, volt, 5, _deviceID) != DT_ERROR_OK)
            //{
            //    Logger.Debug("PmuSetVoltage to on fail");
            //    return false;
            //}


            //if (DTCCM2_API.PmuSetOnOff(power, onoff, 5, _deviceID) != DT_ERROR_OK)
            //{
            //    Logger.Debug("PmuSetOnOff to on");-
            //    return false;
            //}

            

            Thread.Sleep(50);
            uint Devicetype = DTCCM2_API.GetKitType(_deviceID);
            byte[] pinDef = new byte[40];
            if (_sensorTab.baport == 0 || _sensorTab.baport == 5)
            {
                pinDef[0] = 20;
                pinDef[1] = 0;
                pinDef[2] = 2;
                pinDef[3] = 1;
                pinDef[4] = 3;
                pinDef[5] = 4;
                pinDef[6] = 5;
                pinDef[7] = 6;
                pinDef[8] = 7;
                pinDef[9] = 8;
                pinDef[10] = 9;
                pinDef[11] = 20;
                pinDef[12] = 10;
                pinDef[13] = 11;
                pinDef[14] = 12;
                pinDef[15] = 20;
                pinDef[16] = 35;
                pinDef[17] = 13;
                pinDef[18] = 15;
                pinDef[19] = 14;
                pinDef[20] = 19;
                pinDef[21] = 18;
                pinDef[22] = 21;
                pinDef[23] = 17;
                pinDef[24] = 22;
                pinDef[25] = 23;
            }
            else if (_sensorTab.baport == 0x81 || _sensorTab.baport == 0x83)
            {
                pinDef[0] = 20;
                pinDef[1] = 0;
                pinDef[2] = 2;
                pinDef[3] = 1;
                pinDef[4] = 3;
                pinDef[5] = 4;
                pinDef[6] = 5;
                pinDef[7] = 6;
                pinDef[8] = 7;
                pinDef[9] = 8;
                pinDef[10] = 9;
                pinDef[11] = 20;
                pinDef[12] = 10;
                pinDef[13] = 11;
                pinDef[14] = 12;
                pinDef[15] = 20;
                pinDef[16] = 20;
                pinDef[17] = 13;
                pinDef[18] = 15;
                pinDef[19] = 14;
                pinDef[20] = 28;
                pinDef[21] = 31;
                pinDef[22] = 29;
                pinDef[23] = 30;
                pinDef[24] = 22;
                pinDef[25] = 23;
            }
            else if (Devicetype == 0x590 || Devicetype == 0x591 || Devicetype == 0x594)
            {
                pinDef[0] = 20;
                pinDef[1] = 20;
                pinDef[2] = 10;
                pinDef[3] = 11;
                pinDef[4] = 12;
                pinDef[5] = 5;
                pinDef[6] = 6;
                pinDef[7] = 7;
                pinDef[8] = 8;
                pinDef[9] = 9;
                pinDef[10] = 26;
                pinDef[11] = 27;
                pinDef[12] = 40;
                pinDef[13] = 41;
                pinDef[14] = 42;
                pinDef[15] = 43;
                pinDef[16] = 4;
                pinDef[17] = 20;
                pinDef[18] = 15;
                pinDef[19] = 14;
                pinDef[20] = 19;
                pinDef[21] = 18;
                pinDef[22] = 1;
                pinDef[23] = 0;
                pinDef[24] = 2;
                pinDef[25] = 3;
            }
            else  //standard parallel..
            {
                pinDef[0] = 16;
                pinDef[1] = 0;
                pinDef[2] = 2;
                pinDef[3] = 1;
                pinDef[4] = 3;
                pinDef[5] = 4;
                pinDef[6] = 5;
                pinDef[7] = 6;
                pinDef[8] = 7;
                pinDef[9] = 8;
                pinDef[10] = 9;
                pinDef[11] = 20;
                pinDef[12] = 10;
                pinDef[13] = 11;
                pinDef[14] = 12;
                pinDef[15] = 20;
                pinDef[16] = 20;
                pinDef[17] = 20;
                pinDef[18] = 20;
                pinDef[19] = 20;
                pinDef[20] = 13;
                pinDef[21] = 20;
                pinDef[22] = 14;
                pinDef[23] = 15;
                pinDef[24] = 18;
                pinDef[25] = 19;
            }
            if (DTCCM2_API.SetSoftPin(pinDef, _deviceID) != DT_ERROR_OK)
            {
                Logger.Debug($"SetSoftPin to on fail, Error code = {DTCCM2_API.SetSoftPin(pinDef, _deviceID)}");
                return false;
            }

            if (DTCCM2_API.EnableSoftPin(true, _deviceID) != DT_ERROR_OK)
            {
                Logger.Debug($"EnableSoftPin to on fail, Error code = {DTCCM2_API.EnableSoftPin(true, _deviceID)}");
                return false;
            }
          
            DTCCM2_API.EnableGpio(true, _deviceID);


            //// test EO5009 GPIO/////
            if (Image_input_str == "GPIO")
            {
                DTCCM2_API.SetGpioPinLevel(1, true, _deviceID);
                DTCCM2_API.SetGpioPinLevel(2, true, _deviceID);
            }
            //// test EO5009 GPIO/////


            if (DTCCM2_API.SetSensorClock(true, (ushort)(_sensorTab.avmclk/100), _deviceID) != DT_ERROR_OK)
            {
                Logger.Debug($"SetSensorClock to true fail, Error code = {DTCCM2_API.SetSensorClock(true, (ushort)(_sensorTab.avmclk / 100), _deviceID)}");
                return false;
            }


            //if (DTCCM2_API.SetSoftPinPullUp(true, _deviceID) != DT_ERROR_OK)
            //{
            //    Logger.Debug("SetSoftPinPullUp");
            //    return false;
            //}
            Thread.Sleep(10);
            DTCCM2_API.SetSensorI2cRate(false, _deviceID);
            if (DTCCM2_API.SetSensorI2cRate(true, _deviceID) != DT_ERROR_OK)
            {
                Logger.Debug($"SetSensorI2cRate fail, Error code = {DTCCM2_API.SetSensorI2cRate(true, _deviceID)}");
                return false;
            }
            if (DTCCM2_API.SetSensorI2cRapid(false, _deviceID) != DT_ERROR_OK)
            {
                Logger.Debug($"SetSensorI2cRapid fail, Error code = {DTCCM2_API.SetSensorI2cRapid(false, _deviceID)}");
                return false;
            }
            if (DTCCM2_API.SetI2CInterval(0, _deviceID) != DT_ERROR_OK)   //not support
            {
                Logger.Debug($"SetI2CInterval fail, Error code = {DTCCM2_API.SetSensorI2cRapid(false, _deviceID)}");
                return false;
            }
            if (DTCCM2_API.SetSensorI2cAckWait(1000, _deviceID) != DT_ERROR_OK)   //not support
            {
                return false;
            }
            UInt32 MIPI_CTRL_LP_EN = 0x01;//0000 0001
            UInt32 MIPI_CTRL_AUTO_START = 0x02; //0000 0010
            UInt32 MIPI_CTRL_NON_CONT = 0x04;//0000 0100
            UInt32 MIPI_CTRL_FULL_CAP = 0x08;//0000 1000
            UInt32 MIPI_CTRL_CLK_LP_CHK = 0x10;//0001 0000
            UInt32 MIPI_CTRL_CLK_LP01_CHK = 0x40;//0100 0000
            UInt32 MIPI_CTRL_DAT_LP01_CHK = 0x80;//1000 0000
            UInt32 test = ~MIPI_CTRL_LP_EN;
            test = ~MIPI_CTRL_AUTO_START;
            test = ~MIPI_CTRL_NON_CONT;
            test = ~MIPI_CTRL_FULL_CAP;
            test = ~MIPI_CTRL_CLK_LP_CHK;
            test = ~MIPI_CTRL_CLK_LP01_CHK;
            test = ~MIPI_CTRL_DAT_LP01_CHK;
            {
                MipiCtrlEx_t sMipiCtrlEx = new MipiCtrlEx_t();
                DTCCM2_API.GetMipiCtrlEx(ref sMipiCtrlEx, _deviceID);
                sMipiCtrlEx.byPhyType = 0;  //0是dphy，1是dphy deskew功能，2是cphy；
                sMipiCtrlEx.byLaneCnt = 4;  //lane个数设置，D-Phy支持1,2,4lane，C-Phy支持设置1,2,3     
                //sMipiCtrlEx.dwCtrl &= ~(MIPI_CTRL_LP_EN) & (~MIPI_CTRL_CLK_LP01_CHK) & (~MIPI_CTRL_DAT_LP01_CHK) & (~MIPI_CTRL_NON_CONT);
                sMipiCtrlEx.dwCtrl = 0x03;
                sMipiCtrlEx.byLp00MinTime = 10;
                //sMipiCtrlEx.dwCtrl |= MIPI_CTRL_CLK_LP_CHK | MIPI_CTRL_CLK_LP01_CHK | MIPI_CTRL_DAT_LP01_CHK | MIPI_CTRL_NON_CONT;
                //DTCCM2_API.SetMipiCtrlEx(sMipiCtrlEx, _deviceID);
            }
            DTCCM2_API.SetMipiImageVC(0, true, 0x1, _deviceID);
            if (DTCCM2_API.SensorEnable(Convert.ToByte((_sensorTab.adpin ^ 0x3)), true, _deviceID) != DT_ERROR_OK)
            {
                Logger.Debug($"SensorEnable fail, Error code = {DTCCM2_API.SensorEnable(Convert.ToByte((_sensorTab.adpin ^ 0x3)), true, _deviceID)}");
                return false;
            }
            Thread.Sleep(20);
            if (DTCCM2_API.SensorEnable(_sensorTab.adpin, true, _deviceID) != DT_ERROR_OK)
            {
                Logger.Debug($"SensorEnable fail, Error code = {DTCCM2_API.SensorEnable(_sensorTab.adpin, true, _deviceID)}");
                return false;
            }
            Thread.Sleep(50);

            if (DTCCM2_API.InitSensor(_sensorTab.aeslaveID, _sensorTab.aqparaList, _sensorTab.arparaListSize, _sensorTab.afmode, _deviceID) != DT_ERROR_OK)
            {
                Logger.Debug($"InitSensor fail, Error code = {DTCCM2_API.InitSensor(_sensorTab.aeslaveID, _sensorTab.aqparaList, _sensorTab.arparaListSize, _sensorTab.afmode, _deviceID)}");
                return false;
            }
            ushort reg, val;
            // int ParaListSize = sizeof(paraList) / sizeof(short);
            Byte addr = _sensorTab.aeslaveID;
            Byte reg_mode = _sensorTab.afmode;
            bool flag = true;//判断转义定义是否有效
            for (int i = 0; i < paraListSize; i += 2)
            {
                reg = paraList[i]; //if no mask, use ParaList[i] direclty.here have mask
                val = paraList[i + 1];//if no mask, use ParaList[i+1] direclty.here have mask

                if ((reg == 0xFFFF))
                {
                    if (val == 0xfef0)
                    {
                        flag = true;//开启转义定义
                        continue;
                    }
                    else if (val == 0xfef1)
                    {
                        flag = false;//关闭转义定义
                        continue;
                    }
                    if (flag == false)
                    {
                        DTCCM2_API.WriteSensorReg(addr, reg, val, reg_mode, _deviceID);
                        continue;
                    }
                    else
                    {
                        Thread.Sleep(50);
                        continue;
                    }
                }
                if (flag == true)
                {
                    if ((reg == 0xFFF8))
                    {
                        addr = (byte)(val & 0xff);
                        continue;
                    }
                    if ((reg == 0xFFFE))
                    {
                        reg_mode = (byte)val;
                        continue;
                    }
                }
                int iRet = DTCCM2_API.WriteSensorReg(addr, reg, val, reg_mode, _deviceID);
            }
            //ISX019_W();

            if (_sensorTab.actype == 2)
            {
                DTCCM2_API.SetYUV422Format(_sensorTab.auoutformat, _deviceID);
            }
            else
            {
                DTCCM2_API.SetRawFormat(_sensorTab.auoutformat, _deviceID);//设置输出raw格式
            }
            if (false)
            {
                //FrameBufferConfig config = new FrameBufferConfig(); //67108864
                //DTCCM2_API.GetFrameBufferConfig(ref config, this._deviceID);
                ////获取当前设备的BUFFER最大缓存深度（字节）
                //ulong m_uBufferSize = config.uBufferSize; 
                //config.uMode = 1;
                //config.uUpLimit = 16777216;
                //config.uBufferFrames = 3;
                //config.bLite = false;
                //DTCCM2_API.SetFrameBufferConfig(config, this._deviceID);
            }
#if false
            FrameFilter pFrameFilter=new FrameFilter();
            DTCCM2_API.GetErrFrameFilter(ref pFrameFilter, this._deviceID);
            pFrameFilter.bCrcErrorFilter = 1;
            pFrameFilter.bSizeErrorFilter = 1;
            DTCCM2_API.SetErrFrameFilter(pFrameFilter, this._deviceID);

            DTCCM2_API.GetErrFrameFilter(ref pFrameFilter, this._deviceID);
#endif 

            UInt32 PARA_PCLK_RVS = 0x08;//0000 1000
            //UInt32 PARA_VSYNC_RVS = 0x0a; //0001 0000
            //UInt32 PARA_HSYNC_RVS = 0x20;//0010 0000
            //UInt32 PARA_AUTO_POL = 0x40;//0100 0000

            ulong dwCrtl = 0;
            DTCCM2_API.GetParaCtrl(ref dwCrtl, _deviceID);
            DTCCM2_API.SetParaCtrl(dwCrtl | PARA_PCLK_RVS, _deviceID);//HS VS PCLK操作

            if (DTCCM2_API.InitRoi(_sensorTab.roi_x, _sensorTab.roi_y, _sensorTab.aawidth, _sensorTab.abheight, 0, 0, 1, 1, _sensorTab.actype, true, _deviceID) != DT_ERROR_OK)
            {
                Logger.Debug($"InitRoi fail, Error code {DTCCM2_API.InitRoi(_sensorTab.roi_x, _sensorTab.roi_y, _sensorTab.aawidth, _sensorTab.abheight, 0, 0, 1, 1, _sensorTab.actype, true, _deviceID)}");
                return false;
            }
            if (DTCCM2_API.SetSensorPort(_sensorTab.baport, _sensorTab.aawidth, _sensorTab.abheight, _deviceID) != DT_ERROR_OK)
            {
                Logger.Debug($"SetSensorPort fail, Erro code = {DTCCM2_API.SetSensorPort(_sensorTab.baport, _sensorTab.aawidth, _sensorTab.abheight, _deviceID)}");
                return false;
            }
            DTCCM2_API.SetMipiClkPhase(0, _deviceID);

            Thread.Sleep(10);

            if (DTCCM2_API.CalculateGrabSize(ref grabSize, _deviceID) != DT_ERROR_OK)
            {
                Logger.Debug($"CalculateGrabSize fail, Error code = {DTCCM2_API.CalculateGrabSize(ref grabSize, _deviceID)}");
                return false;
            }
            if (DTCCM2_API.InitIsp(_sensorTab.aawidth, _sensorTab.abheight, _sensorTab.actype, 0x01, _deviceID) != DT_ERROR_OK)
            {
                Logger.Debug($"InitIsp fail, Error code = {DTCCM2_API.InitIsp(_sensorTab.aawidth, _sensorTab.abheight, _sensorTab.actype, 0x01, _deviceID)}");
                return false;
            }
            //if (DTCCM2_API.OpenVideo(Convert.ToUInt32(grabSize), _deviceID) != DT_ERROR_OK)
            //{
            //    Logger.Debug("OpenVideo");
            //    return false;
            //}

            if (DTCCM2_API.SetGamma(100, _deviceID) != DT_ERROR_OK)
            {
                Logger.Debug($"SetGamma fail, Error code = {DTCCM2_API.SetGamma(100, _deviceID)}"); 
                return false;
            }
            if (DTCCM2_API.SetContrast(100, _deviceID) != DT_ERROR_OK)
            {
                Logger.Debug($"SetContrast, Error code = {DTCCM2_API.SetContrast(100, _deviceID)}");
                return false;
            }
            if (DTCCM2_API.SetSaturation(128, _deviceID) != DT_ERROR_OK)
            {
                Logger.Debug($"SetSaturation fail, Error code = {DTCCM2_API.SetSaturation(128, _deviceID)}");
                return false;
            }
            if (DTCCM2_API.SetDigitalGain(1.0F, 1.0F, 1.0F, _deviceID) != DT_ERROR_OK)
            {
                Logger.Debug($"SetDigitalGain fail, Error code = {DTCCM2_API.SetDigitalGain(1.0F, 1.0F, 1.0F, _deviceID)}");
                return false;
            }
            Thread.Sleep(100);
            return true;
        }
        public override bool UnInit()
        {
            if (DevNum > 0)
            {
                try
                {
                    DTCCM2_API.CloseDevice(_deviceID);
                    DTCCM2_API.CloseVideo(_deviceID);
                    DTCCM2_API.ResetSensorI2cBus(_deviceID);
                    DTCCM2_API.SensorEnable(Convert.ToByte(_sensorTab.adpin ^ 0x03), true, _deviceID);
                    Thread.Sleep(50);
                    DTCCM2_API.SetSensorClock(false,0, _deviceID);

                    SENSOR_POWER[] power = new SENSOR_POWER[10] { SENSOR_POWER.POWER_AVDD, SENSOR_POWER.POWER_DOVDD, SENSOR_POWER.POWER_DVDD, SENSOR_POWER.POWER_AFVCC, SENSOR_POWER.POWER_VPP2, 0, 0, 0, 0, 0 };
                    int[] volt = new int[10] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                    int[] current = new int[10] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                    bool[] onoff = new bool[10] { false, false, false, false, false, false, false, false, false, false };
                    CURRENT_RANGE[] range = new CURRENT_RANGE[10];
                    //使能柔性接口
                    DTCCM2_API.EnableSoftPin(false, _deviceID);
                    DTCCM2_API.EnableGpio(false, _deviceID);
                    DTCCM2_API.SetSoftPinPullUp(false, _deviceID);
                    DTCCM2_API.SensorEnable(0, true, _deviceID);
                    //设置5路电压值
                    DTCCM2_API.PmuSetVoltage(power, volt, 5, _deviceID);
                    DTCCM2_API.PmuSetCurrentRange(power, range, 5, _deviceID);
                    DTCCM2_API.PmuSetOcpCurrentLimit(power, current, 5, _deviceID);
                    Thread.Sleep(150);
                    //设置电压开关
                    DTCCM2_API.PmuSetOnOff(power, onoff, 5, _deviceID);
                    power = null;
                    volt = null;
                    current = null;
                    onoff = null;
                    range = null;
                    
                    //_sensorTab.aqparaList = null;
                    _setpower = new float[5] { 0.0F, 0.0F, 0.0F, 0.0F, 0.0F };

                    //free memory
                    if (m_TripleBuffer != IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(m_TripleBuffer);
                        m_TripleBuffer = IntPtr.Zero;
                    }
                    if (GrabBuffer != IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(GrabBuffer);
                        GrabBuffer = IntPtr.Zero;
                    }
                    if (m_Raw8Buffer != IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(m_Raw8Buffer);
                        m_Raw8Buffer = IntPtr.Zero;
                    }
                    
                    if (m_avgBuffer != IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(m_avgBuffer);
                        m_avgBuffer = IntPtr.Zero;
                    }
                    if (BMP_buffer != IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(BMP_buffer);
                        BMP_buffer = IntPtr.Zero;
                    }

                    return true;
                }
                catch (Exception e)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public override bool PowerWrite(int avdd, int dovdd, int dvdd, int afvcc, int vpp)
        {
            SENSOR_POWER[] power = new SENSOR_POWER[1];
            power[0] = SENSOR_POWER.POWER_VPP2;
            CURRENT_RANGE[] range = new CURRENT_RANGE[1];
            range[0] = CURRENT_RANGE.CURRENT_RANGE_MA;

            int[] volt = new int[1] { vpp };
            int[] volt_Z = new int[1] { 0 };
            int[] current = new int[1] { 400 };
            bool[] onoff = new bool[1] { true };
            bool[] off = new bool[1] { false };


            if (DTCCM2_API.PmuSetCurrentRange(power, range, 1, _deviceID) != DT_ERROR_OK)
            {
                Logger.Debug($"PmuSetCurrentRange fail, Error code = {DTCCM2_API.PmuSetCurrentRange(power, range, 1, _deviceID)}");
                return false;
            }
            if (DTCCM2_API.PmuSetOcpCurrentLimit(power, current, 1, _deviceID) != DT_ERROR_OK)
            {
                Logger.Debug($"PmuSetOcpCurrentLimit fail, Error code = {DTCCM2_API.PmuSetOcpCurrentLimit(power, current, 1, _deviceID)}");
                return false;
            }

            if (DTCCM2_API.PmuSetVoltage(power, volt_Z, 1, _deviceID) != DT_ERROR_OK)
            {
                Logger.Debug($"PmuSetVoltage to 0 fail, Error code = {DTCCM2_API.PmuSetVoltage(power, volt_Z, 1, _deviceID)}");
                return false;
            }
            Thread.Sleep(50);
            if (DTCCM2_API.PmuSetOnOff(power, off, 1, _deviceID) != DT_ERROR_OK)
            {
                Logger.Debug($"PmuSetOnOff to off fail, Error code = {DTCCM2_API.PmuSetOnOff(power, off, 1, _deviceID)}");
                return false;
            }
            if (DTCCM2_API.PmuSetVoltage(power, volt, 1, _deviceID) != DT_ERROR_OK)
            {
                Logger.Debug($"PmuSetVoltage to on fail, Error code = {DTCCM2_API.PmuSetVoltage(power, volt, 1, _deviceID)}");
                return false;
            }
            if (DTCCM2_API.PmuSetOnOff(power, onoff, 1, _deviceID) != DT_ERROR_OK)
            {
                Logger.Debug($"PmuSetOnOff to on fail, Error code = {DTCCM2_API.PmuSetOnOff(power, onoff, 1, _deviceID)}");
                return false;
            }



            return true;
        }
        public override bool ReadParamFile(string filename)
        {
            IniFile iReader = new IniFile(filename);
            _sensorTab.aawidth = Convert2UInt16(iReader.ReadKey("width", "Sensor"));
            _sensorTab.abheight = Convert2UInt16(iReader.ReadKey("height", "Sensor"));
            _sensorTab.actype = Convert2Byte(iReader.ReadKey("type", "Sensor"));
            _sensorTab.baport = Convert2Byte(iReader.ReadKey("port", "Sensor"));
            _sensorTab.adpin = Convert2Byte(iReader.ReadKey("pin", "Sensor"));
            _sensorTab.aeslaveID = Convert2Byte(iReader.ReadKey("SlaveID", "Sensor"));
            _sensorTab.afmode = Convert2Byte(iReader.ReadKey("mode", "Sensor"));
            _sensorTab.agflagReg = Convert2UInt16(iReader.ReadKey("FlagReg", "Sensor"));
            _sensorTab.ahflagData = Convert2UInt16(iReader.ReadKey("FlagData", "Sensor"));
            _sensorTab.ajflagMask = Convert2UInt16(iReader.ReadKey("FlagMask", "Sensor"));
            _sensorTab.akflagReg1 = Convert2UInt16(iReader.ReadKey("FlagReg1", "Sensor"));
            _sensorTab.alflagData1 = Convert2UInt16(iReader.ReadKey("FlagData1", "Sensor"));
            _sensorTab.aoflagMask1 = Convert2UInt16(iReader.ReadKey("FlagMask1", "Sensor"));
            _sensorTab.apname = (iReader.ReadKey("SensorName", "Sensor"));
            _sensorTab.auoutformat = Convert2Byte(iReader.ReadKey("outformat", "Sensor"));
            _sensorTab.avmclk = Convert.ToInt32(iReader.ReadKey("mclk", "Sensor"));
            _sensorTab.awavdd = Convert.ToInt32(iReader.ReadKey("avdd", "Sensor"));
            _sensorTab.axdovdd = Convert.ToInt32(iReader.ReadKey("dovdd", "Sensor"));
            _sensorTab.aydvdd = Convert.ToInt32(iReader.ReadKey("dvdd", "Sensor"));
            _sensorTab.azvpp = Convert.ToInt32(iReader.ReadKey("vpp", "Sensor"));
            if (iReader.ReadKey("afvcc", "Sensor") != "")
                _sensorTab.azafvcc = Convert.ToInt32(iReader.ReadKey("afvcc", "Sensor"));
            else
                _sensorTab.azafvcc = 0;
            if (iReader.ReadKey("avdd2", "Sensor") != "")
                _sensorTab.awavdd2 = Convert.ToInt32(iReader.ReadKey("avdd2", "Sensor"));
            else
                _sensorTab.awavdd2 = 0;
            if (iReader.ReadKey("ROI_x0", "ROI_Shift") != "")
                _sensorTab.roi_x = Convert2UInt16(iReader.ReadKey("ROI_x0", "ROI_Shift"));
            else
                _sensorTab.roi_x = 0;
            if (iReader.ReadKey("ROI_y0", "ROI_Shift") != "")
                _sensorTab.roi_y = Convert2UInt16(iReader.ReadKey("ROI_y0", "ROI_Shift"));
            else
                _sensorTab.roi_y = 0;


            _setpower = new float[5] { _sensorTab.awavdd / 1000F, _sensorTab.axdovdd / 1000F, _sensorTab.aydvdd / 1000F, _sensorTab.azafvcc / 1000F, _sensorTab.azvpp / 1000F };
            regSection = iReader.ReadIniSections(filename);

            //新增Ori_ParaList相關變數
            string regData = iReader.ReadSectionData("ParaList").Trim();
            string regData_exp = iReader.ReadSectionData("WhiteBoard_ParaList").Trim();
            string regData_ori = iReader.ReadSectionData("Ori_ParaList").Trim();

            string[] lines = regData.Split('\0');
            string[] lines_ori = regData_ori.Split('\0');
            string[] lines_exp = regData_exp.Split('\0');

            ushort offset = 0;
            ushort offset_exp = 0;
            ushort offset_ori = 0;
            _sensorTab.aqparaList = new UInt16[8192 * 4];
            for (int idx = 0; idx < lines.Length; idx++)
            {
                if (lines[idx][0].ToString() != "0" || lines[idx][1].ToString() != "x")
                {
                    continue;
                }
                string[] values = lines[idx].Split(',');
                _sensorTab.aqparaList[offset++] = Convert2UInt16(values[0]);
                _sensorTab.aqparaList[offset++] = Convert2UInt16(values[1]);
            }
            _sensorTab.arparaListSize = offset;
            paraList = _sensorTab.aqparaList;
            paraListSize = _sensorTab.arparaListSize;

            _sensorTab.aqexpparaList = new UInt16[8192 * 4];
            for (int idx = 0; idx < lines_exp.Length; idx++)
            {
                if (lines_exp[idx][0].ToString() != "0" || lines_exp[idx][1].ToString() != "x")
                {
                    continue;
                }
                string[] values_exp = lines_exp[idx].Split(',');
                _sensorTab.aqexpparaList[offset_exp++] = Convert2UInt16(values_exp[0]);
                _sensorTab.aqexpparaList[offset_exp++] = Convert2UInt16(values_exp[1]);
            }
            _sensorTab.aqexpparaListSize = offset_exp;

            _sensorTab.aqoriparaList = new UInt16[8192 * 4];
            for (int idx = 0; idx < lines_ori.Length; idx++)
            {
                if (lines_ori[idx][0].ToString() != "0" || lines_ori[idx][1].ToString() != "x")
                {
                    continue;
                }
                string[] values_ori = lines_ori[idx].Split(',');
                _sensorTab.aqoriparaList[offset_ori++] = Convert2UInt16(values_ori[0]);
                _sensorTab.aqoriparaList[offset_ori++] = Convert2UInt16(values_ori[1]);
            }
            _sensorTab.aqoriparaListSize = offset_ori;
            return true;
        }
        public UInt16 Convert2UInt16(string str)
        {
            if (str == null || str == "")
                return 0;
            str = str.Trim();
            if ((str.Length > 2) && (str[0] == '0') && (str[1] == 'x'))
            {
                return Convert.ToUInt16(str, 16);
            }
            else
            {
                return Convert.ToUInt16(str);
            }
        }
        public UInt32 Convert2UInt32(string str)
        {
            if (str == null || str == "")
                return 0;
            if ((str.Length > 2) && (str[0] == '0') && (str[1] == 'x'))
            {
                return Convert.ToUInt32(str, 16);
            }
            else
            {
                return Convert.ToUInt32(str);
            }
        }
        public Byte Convert2Byte(string str)
        {
            if (str == null || str == "")
                return 0;
            if ((str.Length > 2) && (str[0] == '0') && (str[1] == 'x'))
            {
                return Convert.ToByte(str, 16);
            }
            else
            {
                return Convert.ToByte(str);
            }
        }
        public void ISX019_W()
        {
            byte[] b = new byte[5];
            b[0] = 0x3C;
            b[1] = 0x00;
            b[2] = 0x00;
            b[3] = 0x01;
            b[4] = 0x4F;
            ezSensorI2cRw_t pSensorI2cRw = new ezSensorI2cRw_t();
            //# SENSOR_I2C_RD_NO_STOP       1 << 0         
            pSensorI2cRw.bySlaveAddr = 0x36;
            pSensorI2cRw.uCtrl = 0x01;
            byte[] a = new byte[9];
            a[0] = 0x09;
            a[1] = 0x01;
            a[2] = 0x06;
            a[3] = 0x02;
            a[4] = 0x3C;
            a[5] = 0x00;
            a[6] = 0x00;
            a[7] = 0x01;
            a[8] = 0x4F;

            IntPtr pWrData = Marshal.AllocHGlobal(9);
            IntPtr pRdData = Marshal.AllocHGlobal(5);
            // Marshal.Copy(pWrData, a, 0, 9);//拷贝数据
            Marshal.Copy(a, 0, pWrData, 9);
            Marshal.Copy(b, 0, pRdData, 5);
            pSensorI2cRw.pWrData = pWrData;
            pSensorI2cRw.pRdData = pRdData;
            pSensorI2cRw.uWrSize = 9;
            pSensorI2cRw.uRdSize = 5; //读 size 为 0，表示写
            int iRet = DTCCM2_API.ezSensorI2cRw(ref pSensorI2cRw, m_iDevID);

            //0x09,0x01,0x06,0x02,0x3c,0x00,0x01,0x00,0x4f
            b[0] = 0x3C;
            b[1] = 0x00;
            b[2] = 0x01;
            b[3] = 0x00;
            b[4] = 0x4F;
            //iRet = DTCCM2_API.WriteSensorI2cEx(0x36, uRegAddr, 4, b, 5, (int)m_iDevID);
            a[0] = 0x09;
            a[1] = 0x01;
            a[2] = 0x06;
            a[3] = 0x02;
            a[4] = 0x3C;
            a[5] = 0x00;
            a[6] = 0x01;
            a[7] = 0x00;
            a[8] = 0x4F;
            Marshal.Copy(a, 0, pWrData, 9);
            Marshal.Copy(b, 0, pRdData, 5);
            pSensorI2cRw.pWrData = pWrData;
            pSensorI2cRw.pRdData = pRdData;
            pSensorI2cRw.uWrSize = 9;
            pSensorI2cRw.uRdSize = 5; //读 size 为 0，表示写
            iRet = DTCCM2_API.ezSensorI2cRw(ref pSensorI2cRw, m_iDevID);

            //0x09,0x01,0x06,0x02,0x43,0x00,0x14,0x00,0x69
            b[0] = 0x43;
            b[1] = 0x00;
            b[2] = 0x14;
            b[3] = 0x00;
            b[4] = 0x69;
            // iRet = DTCCM2_API.WriteSensorI2cEx(0x36, uRegAddr, 4, b, 5, (int)m_iDevID);
            a[0] = 0x09;
            a[1] = 0x01;
            a[2] = 0x06;
            a[3] = 0x02;
            a[4] = 0x43;
            a[5] = 0x00;
            a[6] = 0x14;
            a[7] = 0x00;
            a[8] = 0x69;
            Marshal.Copy(a, 0, pWrData, 9);
            Marshal.Copy(b, 0, pRdData, 5);
            pSensorI2cRw.pWrData = pWrData;
            pSensorI2cRw.pRdData = pRdData;
            pSensorI2cRw.uWrSize = 9;
            pSensorI2cRw.uRdSize = 5; //读 size 为 0，表示写
            iRet = DTCCM2_API.ezSensorI2cRw(ref pSensorI2cRw, m_iDevID);

            //0x09,0x01,0x06,0x02,0x44,0x00,0x00,0x00,0x56
            b[0] = 0x44;
            b[1] = 0x00;
            b[2] = 0x00;
            b[3] = 0x00;
            b[4] = 0x56;
            //iRet = DTCCM2_API.WriteSensorI2cEx(0x36, uRegAddr, 4, b, 5, (int)m_iDevID);
            a[0] = 0x09;
            a[1] = 0x01;
            a[2] = 0x06;
            a[3] = 0x02;
            a[4] = 0x44;
            a[5] = 0x00;
            a[6] = 0x00;
            a[7] = 0x00;
            a[8] = 0x56;
            Marshal.Copy(a, 0, pWrData, 9);
            Marshal.Copy(b, 0, pRdData, 5);
            pSensorI2cRw.pWrData = pWrData;
            pSensorI2cRw.pRdData = pRdData;
            pSensorI2cRw.uWrSize = 9;
            pSensorI2cRw.uRdSize = 5; //读 size 为 0，表示写
            iRet = DTCCM2_API.ezSensorI2cRw(ref pSensorI2cRw, m_iDevID);
            //0x09,0x01,0x06,0x02,0x45,0x00,0x00,0x00,0x57
            b[0] = 0x45;
            b[1] = 0x00;
            b[2] = 0x00;
            b[3] = 0x00;
            b[4] = 0x57;
            //iRet = DTCCM2_API.WriteSensorI2cEx(0x36, uRegAddr, 4, b, 5, (int)m_iDevID);
            a[0] = 0x09;
            a[1] = 0x01;
            a[2] = 0x06;
            a[3] = 0x02;
            a[4] = 0x45;
            a[5] = 0x00;
            a[6] = 0x00;
            a[7] = 0x00;
            a[8] = 0x57;
            Marshal.Copy(a, 0, pWrData, 9);
            Marshal.Copy(b, 0, pRdData, 5);
            pSensorI2cRw.pWrData = pWrData;
            pSensorI2cRw.pRdData = pRdData;
            pSensorI2cRw.uWrSize = 9;
            pSensorI2cRw.uRdSize = 5; //读 size 为 0，表示写
            iRet = DTCCM2_API.ezSensorI2cRw(ref pSensorI2cRw, m_iDevID);
            //0x09,0x01,0x06,0x02,0x47,0x00,0x00,0x00,0x59
            b[0] = 0x47;
            b[1] = 0x00;
            b[2] = 0x00;
            b[3] = 0x00;
            b[4] = 0x59;
            //iRet = DTCCM2_API.WriteSensorI2cEx(0x36, uRegAddr, 4, b, 5, (int)m_iDevID);
            a[0] = 0x09;
            a[1] = 0x01;
            a[2] = 0x06;
            a[3] = 0x02;
            a[4] = 0x47;
            a[5] = 0x00;
            a[6] = 0x00;
            a[7] = 0x00;
            a[8] = 0x59;
            Marshal.Copy(a, 0, pWrData, 9);
            Marshal.Copy(b, 0, pRdData, 5);
            pSensorI2cRw.pWrData = pWrData;
            pSensorI2cRw.pRdData = pRdData;
            pSensorI2cRw.uWrSize = 9;
            pSensorI2cRw.uRdSize = 5; //读 size 为 0，表示写
            iRet = DTCCM2_API.ezSensorI2cRw(ref pSensorI2cRw, m_iDevID);
            //0x09,0x01,0x06,0x02,0x47,0x00,0x01,0x00,0x5a
            b[0] = 0x47;
            b[1] = 0x00;
            b[2] = 0x01;
            b[3] = 0x00;
            b[4] = 0x5a;
            //iRet = DTCCM2_API.WriteSensorI2cEx(0x36, uRegAddr, 4, b, 5, (int)m_iDevID);
            a[0] = 0x09;
            a[1] = 0x01;
            a[2] = 0x06;
            a[3] = 0x02;
            a[4] = 0x47;
            a[5] = 0x00;
            a[6] = 0x01;
            a[7] = 0x00;
            a[8] = 0x5a;
            Marshal.Copy(a, 0, pWrData, 9);
            Marshal.Copy(b, 0, pRdData, 5);
            pSensorI2cRw.pWrData = pWrData;
            pSensorI2cRw.pRdData = pRdData;
            pSensorI2cRw.uWrSize = 9;
            pSensorI2cRw.uRdSize = 5; //读 size 为 0，表示写
            iRet = DTCCM2_API.ezSensorI2cRw(ref pSensorI2cRw, m_iDevID);
            //0x09,0x01,0x06,0x02,0x47,0x00,0x02,0x00,0x5b
            b[0] = 0x47;
            b[1] = 0x00;
            b[2] = 0x02;
            b[3] = 0x00;
            b[4] = 0x5b;
            //iRet = DTCCM2_API.WriteSensorI2cEx(0x36, uRegAddr, 4, b, 5, (int)m_iDevID);
            a[0] = 0x09;
            a[1] = 0x01;
            a[2] = 0x06;
            a[3] = 0x02;
            a[4] = 0x47;
            a[5] = 0x00;
            a[6] = 0x02;
            a[7] = 0x00;
            a[8] = 0x5b;
            Marshal.Copy(a, 0, pWrData, 9);
            Marshal.Copy(b, 0, pRdData, 5);
            pSensorI2cRw.pWrData = pWrData;
            pSensorI2cRw.pRdData = pRdData;
            pSensorI2cRw.uWrSize = 9;
            pSensorI2cRw.uRdSize = 5; //读 size 为 0，表示写
            iRet = DTCCM2_API.ezSensorI2cRw(ref pSensorI2cRw, m_iDevID);
            //0x09,0x01,0x06,0x02,0x47,0x00,0x03,0x00,0x5c
            b[0] = 0x47;
            b[1] = 0x00;
            b[2] = 0x03;
            b[3] = 0x00;
            b[4] = 0x5c;
            //iRet = DTCCM2_API.WriteSensorI2cEx(0x36, uRegAddr, 4, b, 5, (int)m_iDevID);
            a[0] = 0x09;
            a[1] = 0x01;
            a[2] = 0x06;
            a[3] = 0x02;
            a[4] = 0x47;
            a[5] = 0x00;
            a[6] = 0x03;
            a[7] = 0x00;
            a[8] = 0x5c;
            Marshal.Copy(a, 0, pWrData, 9);
            Marshal.Copy(b, 0, pRdData, 5);
            pSensorI2cRw.pWrData = pWrData;
            pSensorI2cRw.pRdData = pRdData;
            pSensorI2cRw.uWrSize = 9;
            pSensorI2cRw.uRdSize = 5; //读 size 为 0，表示写
            iRet = DTCCM2_API.ezSensorI2cRw(ref pSensorI2cRw, m_iDevID);
            //0x09,0x01,0x06,0x02,0x48,0x00,0x00,0x00,0x5a
            b[0] = 0x48;
            b[1] = 0x00;
            b[2] = 0x00;
            b[3] = 0x00;
            b[4] = 0x5a;
            // iRet = DTCCM2_API.WriteSensorI2cEx(0x36, uRegAddr, 4, b, 5, (int)m_iDevID);
            a[0] = 0x09;
            a[1] = 0x01;
            a[2] = 0x06;
            a[3] = 0x02;
            a[4] = 0x48;
            a[5] = 0x00;
            a[6] = 0x00;
            a[7] = 0x00;
            a[8] = 0x5a;
            Marshal.Copy(a, 0, pWrData, 9);
            Marshal.Copy(b, 0, pRdData, 5);
            pSensorI2cRw.pWrData = pWrData;
            pSensorI2cRw.pRdData = pRdData;
            pSensorI2cRw.uWrSize = 9;
            pSensorI2cRw.uRdSize = 5; //读 size 为 0，表示写
            iRet = DTCCM2_API.ezSensorI2cRw(ref pSensorI2cRw, m_iDevID);
            //0x09,0x01,0x06,0x02,0x48,0x00,0x01,0x00,0x5b
            b[0] = 0x48;
            b[1] = 0x00;
            b[2] = 0x01;
            b[3] = 0x00;
            b[4] = 0x5b;
            // iRet = DTCCM2_API.WriteSensorI2cEx(0x36, uRegAddr, 4, b, 5, (int)m_iDevID);
            a[0] = 0x09;
            a[1] = 0x01;
            a[2] = 0x06;
            a[3] = 0x02;
            a[4] = 0x48;
            a[5] = 0x00;
            a[6] = 0x01;
            a[7] = 0x00;
            a[8] = 0x5b;
            Marshal.Copy(a, 0, pWrData, 9);
            Marshal.Copy(b, 0, pRdData, 5);
            pSensorI2cRw.pWrData = pWrData;
            pSensorI2cRw.pRdData = pRdData;
            pSensorI2cRw.uWrSize = 9;
            pSensorI2cRw.uRdSize = 5; //读 size 为 0，表示写
            iRet = DTCCM2_API.ezSensorI2cRw(ref pSensorI2cRw, m_iDevID);
            //0x09,0x01,0x06,0x02,0x48,0x00,0x02,0x00,0x5c
            b[0] = 0x48;
            b[1] = 0x00;
            b[2] = 0x02;
            b[3] = 0x00;
            b[4] = 0x5c;
            //iRet = DTCCM2_API.WriteSensorI2cEx(0x36, uRegAddr, 4, b, 5, (int)m_iDevID);
            a[0] = 0x09;
            a[1] = 0x01;
            a[2] = 0x06;
            a[3] = 0x02;
            a[4] = 0x48;
            a[5] = 0x00;
            a[6] = 0x02;
            a[7] = 0x00;
            a[8] = 0x5c;
            Marshal.Copy(a, 0, pWrData, 9);
            Marshal.Copy(b, 0, pRdData, 5);
            pSensorI2cRw.pWrData = pWrData;
            pSensorI2cRw.pRdData = pRdData;
            pSensorI2cRw.uWrSize = 9;
            pSensorI2cRw.uRdSize = 5; //读 size 为 0，表示写
            iRet = DTCCM2_API.ezSensorI2cRw(ref pSensorI2cRw, m_iDevID);
            //0x09,0x01,0x06,0x02,0x48,0x00,0x03,0x00,0x5d
            b[0] = 0x48;
            b[1] = 0x00;
            b[2] = 0x03;
            b[3] = 0x00;
            b[4] = 0x5d;
            //iRet = DTCCM2_API.WriteSensorI2cEx(0x36, uRegAddr, 4, b, 5, (int)m_iDevID);
            a[0] = 0x09;
            a[1] = 0x01;
            a[2] = 0x06;
            a[3] = 0x02;
            a[4] = 0x48;
            a[5] = 0x00;
            a[6] = 0x03;
            a[7] = 0x00;
            a[8] = 0x5d;
            Marshal.Copy(a, 0, pWrData, 9);
            Marshal.Copy(b, 0, pRdData, 5);
            pSensorI2cRw.pWrData = pWrData;
            pSensorI2cRw.pRdData = pRdData;
            pSensorI2cRw.uWrSize = 9;
            pSensorI2cRw.uRdSize = 5; //读 size 为 0，表示写
            iRet = DTCCM2_API.ezSensorI2cRw(ref pSensorI2cRw, m_iDevID);
            //0x09,0x01,0x06,0x02,0x34,0x00,0x01,0x80,0xc7
            b[0] = 0x34;
            b[1] = 0x00;
            b[2] = 0x01;
            b[3] = 0x80;
            b[4] = 0xc7;
            // iRet = DTCCM2_API.WriteSensorI2cEx(0x36, uRegAddr, 4, b, 5, (int)m_iDevID);
            a[0] = 0x09;
            a[1] = 0x01;
            a[2] = 0x06;
            a[3] = 0x02;
            a[4] = 0x34;
            a[5] = 0x00;
            a[6] = 0x01;
            a[7] = 0x80;
            a[8] = 0xc7;
            Marshal.Copy(a, 0, pWrData, 9);
            Marshal.Copy(b, 0, pRdData, 5);
            pSensorI2cRw.pWrData = pWrData;
            pSensorI2cRw.pRdData = pRdData;
            pSensorI2cRw.uWrSize = 9;
            pSensorI2cRw.uRdSize = 5; //读 size 为 0，表示写
            iRet = DTCCM2_API.ezSensorI2cRw(ref pSensorI2cRw, m_iDevID);
            //0x09,0x01,0x06,0x02,0x34,0x00,0x02,0x00,0x48
            b[0] = 0x34;
            b[1] = 0x00;
            b[2] = 0x02;
            b[3] = 0x00;
            b[4] = 0x48;
            //iRet = DTCCM2_API.WriteSensorI2cEx(0x36, uRegAddr, 4, b, 5, (int)m_iDevID);
            a[0] = 0x09;
            a[1] = 0x01;
            a[2] = 0x06;
            a[3] = 0x02;
            a[4] = 0x34;
            a[5] = 0x00;
            a[6] = 0x02;
            a[7] = 0x00;
            a[8] = 0x48;
            Marshal.Copy(a, 0, pWrData, 9);
            Marshal.Copy(b, 0, pRdData, 5);
            pSensorI2cRw.pWrData = pWrData;
            pSensorI2cRw.pRdData = pRdData;
            pSensorI2cRw.uWrSize = 9;
            pSensorI2cRw.uRdSize = 5; //读 size 为 0，表示写
            iRet = DTCCM2_API.ezSensorI2cRw(ref pSensorI2cRw, m_iDevID);
            //0x09,0x01,0x06,0x02,0x34,0x00,0x03,0x00,0x49
            b[0] = 0x34;
            b[1] = 0x00;
            b[2] = 0x03;
            b[3] = 0x00;
            b[4] = 0x49;
            //iRet = DTCCM2_API.WriteSensorI2cEx(0x36, uRegAddr, 4, b, 5, (int)m_iDevID);
            a[0] = 0x09;
            a[1] = 0x01;
            a[2] = 0x06;
            a[3] = 0x02;
            a[4] = 0x34;
            a[5] = 0x00;
            a[6] = 0x03;
            a[7] = 0x00;
            a[8] = 0x49;
            Marshal.Copy(a, 0, pWrData, 9);
            Marshal.Copy(b, 0, pRdData, 5);
            pSensorI2cRw.pWrData = pWrData;
            pSensorI2cRw.pRdData = pRdData;
            pSensorI2cRw.uWrSize = 9;
            pSensorI2cRw.uRdSize = 5; //读 size 为 0，表示写
            iRet = DTCCM2_API.ezSensorI2cRw(ref pSensorI2cRw, m_iDevID);
            //0x09,0x01,0x06,0x02,0x34,0x00,0x04,0x00,0x4a
            b[0] = 0x34;
            b[1] = 0x00;
            b[2] = 0x04;
            b[3] = 0x00;
            b[4] = 0x4a;
            //iRet = DTCCM2_API.WriteSensorI2cEx(0x36, uRegAddr, 4, b, 5, (int)m_iDevID);
            a[0] = 0x09;
            a[1] = 0x01;
            a[2] = 0x06;
            a[3] = 0x02;
            a[4] = 0x34;
            a[5] = 0x00;
            a[6] = 0x04;
            a[7] = 0x00;
            a[8] = 0x4a;
            Marshal.Copy(a, 0, pWrData, 9);
            Marshal.Copy(b, 0, pRdData, 5);
            pSensorI2cRw.pWrData = pWrData;
            pSensorI2cRw.pRdData = pRdData;
            pSensorI2cRw.uWrSize = 9;
            pSensorI2cRw.uRdSize = 5; //读 size 为 0，表示写
            iRet = DTCCM2_API.ezSensorI2cRw(ref pSensorI2cRw, m_iDevID);
            //0x09,0x01,0x06,0x02,0x34,0x00,0x05,0x80,0xcb
            b[0] = 0x34;
            b[1] = 0x00;
            b[2] = 0x05;
            b[3] = 0x80;
            b[4] = 0xcb;
            // iRet = DTCCM2_API.WriteSensorI2cEx(0x36, uRegAddr, 4, b, 5, (int)m_iDevID);
            a[0] = 0x09;
            a[1] = 0x01;
            a[2] = 0x06;
            a[3] = 0x02;
            a[4] = 0x34;
            a[5] = 0x00;
            a[6] = 0x05;
            a[7] = 0x80;
            a[8] = 0xcb;
            Marshal.Copy(a, 0, pWrData, 9);
            Marshal.Copy(b, 0, pRdData, 5);
            pSensorI2cRw.pWrData = pWrData;
            pSensorI2cRw.pRdData = pRdData;
            pSensorI2cRw.uWrSize = 9;
            pSensorI2cRw.uRdSize = 5; //读 size 为 0，表示写
            iRet = DTCCM2_API.ezSensorI2cRw(ref pSensorI2cRw, m_iDevID);

            //0x09,0x01,0x06,0x02,0x41,0x00,0x3a,0x03,0x90
            b[0] = 0x41;
            b[1] = 0x00;
            b[2] = 0x3a;
            b[3] = 0x03;
            b[4] = 0x90;

            a[0] = 0x09;
            a[1] = 0x01;
            a[2] = 0x06;
            a[3] = 0x02;
            a[4] = 0x41;
            a[5] = 0x00;
            a[6] = 0x3a;
            a[7] = 0x03;
            a[8] = 0x90;
            Marshal.Copy(a, 0, pWrData, 9);
            Marshal.Copy(b, 0, pRdData, 5);
            pSensorI2cRw.pWrData = pWrData;
            pSensorI2cRw.pRdData = pRdData;
            pSensorI2cRw.uWrSize = 9;
            pSensorI2cRw.uRdSize = 5; //读 size 为 0，表示写
            iRet = DTCCM2_API.ezSensorI2cRw(ref pSensorI2cRw, m_iDevID);
            //iRet = DTCCM2_API.WriteSensorI2cEx(0x36, uRegAddr, 4, b, 5, (int)m_iDevID);
            //0x09,0x01,0x06,0x02,0x42,0x00,0x00,0x00,0x54
            b[0] = 0x42;
            b[1] = 0x00;
            b[2] = 0x00;
            b[3] = 0x00;
            b[4] = 0x54;
            //iRet = DTCCM2_API.WriteSensorI2cEx(0x36, uRegAddr, 4, b, 5, (int)m_iDevID);
            a[0] = 0x09;
            a[1] = 0x01;
            a[2] = 0x06;
            a[3] = 0x02;
            a[4] = 0x42;
            a[5] = 0x00;
            a[6] = 0x00;
            a[7] = 0x00;
            a[8] = 0x54;
            Marshal.Copy(a, 0, pWrData, 9);
            Marshal.Copy(b, 0, pRdData, 5);
            pSensorI2cRw.pWrData = pWrData;
            pSensorI2cRw.pRdData = pRdData;
            pSensorI2cRw.uWrSize = 9;
            pSensorI2cRw.uRdSize = 5; //读 size 为 0，表示写
            iRet = DTCCM2_API.ezSensorI2cRw(ref pSensorI2cRw, m_iDevID);
            //0x09,0x01,0x06,0x02,0x42,0x00,0x01,0x00,0x55
            b[0] = 0x42;
            b[1] = 0x00;
            b[2] = 0x01;
            b[3] = 0x00;
            b[4] = 0x56;
            // iRet = DTCCM2_API.WriteSensorI2cEx(0x36, uRegAddr, 4, b, 5, (int)m_iDevID);
            a[0] = 0x09;
            a[1] = 0x01;
            a[2] = 0x06;
            a[3] = 0x02;
            a[4] = 0x42;
            a[5] = 0x00;
            a[6] = 0x01;
            a[7] = 0x00;
            a[8] = 0x56;
            Marshal.Copy(a, 0, pWrData, 9);
            Marshal.Copy(b, 0, pRdData, 5);
            pSensorI2cRw.pWrData = pWrData;
            pSensorI2cRw.pRdData = pRdData;
            pSensorI2cRw.uWrSize = 9;
            pSensorI2cRw.uRdSize = 5; //读 size 为 0，表示写
            iRet = DTCCM2_API.ezSensorI2cRw(ref pSensorI2cRw, m_iDevID);
            //0x09,0x01,0x06,0x02,0x40,0x00,0x00,0x00,0x52
            b[0] = 0x40;
            b[1] = 0x00;
            b[2] = 0x00;
            b[3] = 0x00;
            b[4] = 0x52;
            // iRet = DTCCM2_API.WriteSensorI2cEx(0x36, uRegAddr, 4, b, 5, (int)m_iDevID);
            a[0] = 0x09;
            a[1] = 0x01;
            a[2] = 0x06;
            a[3] = 0x02;
            a[4] = 0x40;
            a[5] = 0x00;
            a[6] = 0x00;
            a[7] = 0x00;
            a[8] = 0x52;
            Marshal.Copy(a, 0, pWrData, 9);
            Marshal.Copy(b, 0, pRdData, 5);
            pSensorI2cRw.pWrData = pWrData;
            pSensorI2cRw.pRdData = pRdData;
            pSensorI2cRw.uWrSize = 9;
            pSensorI2cRw.uRdSize = 5; //读 size 为 0，表示写
            iRet = DTCCM2_API.ezSensorI2cRw(ref pSensorI2cRw, m_iDevID);
            //0x09,0x01,0x06,0x02,0x46,0x00,0x00,0x00,0x58
            b[0] = 0x46;
            b[1] = 0x00;
            b[2] = 0x00;
            b[3] = 0x00;
            b[4] = 0x58;
            // iRet = DTCCM2_API.WriteSensorI2cEx(0x36, uRegAddr, 4, b, 5, (int)m_iDevID);
            a[0] = 0x09;
            a[1] = 0x01;
            a[2] = 0x06;
            a[3] = 0x02;
            a[4] = 0x46;
            a[5] = 0x00;
            a[6] = 0x00;
            a[7] = 0x00;
            a[8] = 0x58;
            Marshal.Copy(a, 0, pWrData, 9);
            Marshal.Copy(b, 0, pRdData, 5);
            pSensorI2cRw.pWrData = pWrData;
            pSensorI2cRw.pRdData = pRdData;
            pSensorI2cRw.uWrSize = 9;
            pSensorI2cRw.uRdSize = 5; //读 size 为 0，表示写
            iRet = DTCCM2_API.ezSensorI2cRw(ref pSensorI2cRw, m_iDevID);
            //0x09,0x01,0x06,0x02,0x1e,0x00,0x00,0x80,0xb0
            b[0] = 0x1e;
            b[1] = 0x00;
            b[2] = 0x00;
            b[3] = 0x80;
            b[4] = 0xb0;
            //iRet = DTCCM2_API.WriteSensorI2cEx(0x36, uRegAddr, 4, b, 5, (int)m_iDevID);
            a[0] = 0x09;
            a[1] = 0x01;
            a[2] = 0x06;
            a[3] = 0x02;
            a[4] = 0x1e;
            a[5] = 0x00;
            a[6] = 0x00;
            a[7] = 0x80;
            a[8] = 0xb0;
            Marshal.Copy(a, 0, pWrData, 9);
            Marshal.Copy(b, 0, pRdData, 5);
            pSensorI2cRw.pWrData = pWrData;
            pSensorI2cRw.pRdData = pRdData;
            pSensorI2cRw.uWrSize = 9;
            pSensorI2cRw.uRdSize = 5; //读 size 为 0，表示写
            iRet = DTCCM2_API.ezSensorI2cRw(ref pSensorI2cRw, m_iDevID);
            //0x09,0x01,0x06,0x02,0x0d,0x00,0x14,0x00,0x33
            b[0] = 0x0d;
            b[1] = 0x00;
            b[2] = 0x14;
            b[3] = 0x00;
            b[4] = 0x22;
            // iRet = DTCCM2_API.WriteSensorI2cEx(0x36, uRegAddr, 4, b, 5, (int)m_iDevID);
            a[0] = 0x09;
            a[1] = 0x01;
            a[2] = 0x06;
            a[3] = 0x02;
            a[4] = 0x0d;
            a[5] = 0x00;
            a[6] = 0x14;
            a[7] = 0x00;
            a[8] = 0x22;
            Marshal.Copy(a, 0, pWrData, 9);
            Marshal.Copy(b, 0, pRdData, 5);
            pSensorI2cRw.pWrData = pWrData;
            pSensorI2cRw.pRdData = pRdData;
            pSensorI2cRw.uWrSize = 9;
            pSensorI2cRw.uRdSize = 5; //读 size 为 0，表示写
            iRet = DTCCM2_API.ezSensorI2cRw(ref pSensorI2cRw, m_iDevID);
            //0x09,0x01,0x06,0x02,0x0d,0x00,0x6f,0x00,0x8E
            b[0] = 0x0d;
            b[1] = 0x00;
            b[2] = 0x6f;
            b[3] = 0x00;
            b[4] = 0x8e;
            // iRet = DTCCM2_API.WriteSensorI2cEx(0x36, uRegAddr, 4, b, 5, (int)m_iDevID);
            a[0] = 0x09;
            a[1] = 0x01;
            a[2] = 0x06;
            a[3] = 0x02;
            a[4] = 0x0d;
            a[5] = 0x00;
            a[6] = 0x6f;
            a[7] = 0x00;
            a[8] = 0x8e;
            Marshal.Copy(a, 0, pWrData, 9);
            Marshal.Copy(b, 0, pRdData, 5);
            pSensorI2cRw.pWrData = pWrData;
            pSensorI2cRw.pRdData = pRdData;
            pSensorI2cRw.uWrSize = 9;
            pSensorI2cRw.uRdSize = 5; //读 size 为 0，表示写
            iRet = DTCCM2_API.ezSensorI2cRw(ref pSensorI2cRw, m_iDevID);
            //0x09,0x01,0x06,0x02,0x1c,0x00,0x06,0xff,0x33
            b[0] = 0x1C;
            b[1] = 0x00;
            b[2] = 0x06;
            b[3] = 0xff;
            b[4] = 0x33;
            //iRet = DTCCM2_API.WriteSensorI2cEx(0x36, uRegAddr, 4, b, 5, (int)m_iDevID);
            a[0] = 0x09;
            a[1] = 0x01;
            a[2] = 0x06;
            a[3] = 0x02;
            a[4] = 0x1c;
            a[5] = 0x00;
            a[6] = 0x06;
            a[7] = 0xff;
            a[8] = 0x33;
            Marshal.Copy(a, 0, pWrData, 9);
            Marshal.Copy(b, 0, pRdData, 5);
            pSensorI2cRw.pWrData = pWrData;
            pSensorI2cRw.pRdData = pRdData;
            pSensorI2cRw.uWrSize = 9;
            pSensorI2cRw.uRdSize = 5; //读 size 为 0，表示写
            iRet = DTCCM2_API.ezSensorI2cRw(ref pSensorI2cRw, m_iDevID);
            //0x09,0x01,0x06,0x02,0x1c,0x00,0x07,0x0f,0x44
            b[0] = 0x1C;
            b[1] = 0x00;
            b[2] = 0x07;
            b[3] = 0x0f;
            b[4] = 0x44;
            // iRet = DTCCM2_API.WriteSensorI2cEx(0x36, uRegAddr, 4, b, 5, (int)m_iDevID);
            a[0] = 0x09;
            a[1] = 0x01;
            a[2] = 0x06;
            a[3] = 0x02;
            a[4] = 0x1c;
            a[5] = 0x00;
            a[6] = 0x07;
            a[7] = 0x0f;
            a[8] = 0x44;
            Marshal.Copy(a, 0, pWrData, 9);
            Marshal.Copy(b, 0, pRdData, 5);
            pSensorI2cRw.pWrData = pWrData;
            pSensorI2cRw.pRdData = pRdData;
            pSensorI2cRw.uWrSize = 9;
            pSensorI2cRw.uRdSize = 5; //读 size 为 0，表示写
            iRet = DTCCM2_API.ezSensorI2cRw(ref pSensorI2cRw, m_iDevID);
            //0x09,0x01,0x06,0x02,0x1c,0x00,0x84,0xff,0xb1
            b[0] = 0x1C;
            b[1] = 0x00;
            b[2] = 0x84;
            b[3] = 0xff;
            b[4] = 0xb1;
            //iRet = DTCCM2_API.WriteSensorI2cEx(0x36, uRegAddr, 4, b, 5, (int)m_iDevID);
            a[0] = 0x09;
            a[1] = 0x01;
            a[2] = 0x06;
            a[3] = 0x02;
            a[4] = 0x1c;
            a[5] = 0x00;
            a[6] = 0x84;
            a[7] = 0xff;
            a[8] = 0xb1;
            Marshal.Copy(a, 0, pWrData, 9);
            Marshal.Copy(b, 0, pRdData, 5);
            pSensorI2cRw.pWrData = pWrData;
            pSensorI2cRw.pRdData = pRdData;
            pSensorI2cRw.uWrSize = 9;
            pSensorI2cRw.uRdSize = 5; //读 size 为 0，表示写
            iRet = DTCCM2_API.ezSensorI2cRw(ref pSensorI2cRw, m_iDevID);
            //0x09,0x01,0x06,0x02,0x1c,0x00,0x85,0x0f,0xc2
            b[0] = 0x1C;
            b[1] = 0x00;
            b[2] = 0x85;
            b[3] = 0x0f;
            b[4] = 0xc2;
            //iRet = DTCCM2_API.WriteSensorI2cEx(0x36, uRegAddr, 4, b, 5, (int)m_iDevID);
            a[0] = 0x09;
            a[1] = 0x01;
            a[2] = 0x06;
            a[3] = 0x02;
            a[4] = 0x1c;
            a[5] = 0x00;
            a[6] = 0x85;
            a[7] = 0x0f;
            a[8] = 0xc2;
            Marshal.Copy(a, 0, pWrData, 9);
            Marshal.Copy(b, 0, pRdData, 5);
            pSensorI2cRw.pWrData = pWrData;
            pSensorI2cRw.pRdData = pRdData;
            pSensorI2cRw.uWrSize = 9;
            pSensorI2cRw.uRdSize = 5; //读 size 为 0，表示写
            iRet = DTCCM2_API.ezSensorI2cRw(ref pSensorI2cRw, m_iDevID);
            //0x09,0x01,0x06,0x02,0x15,0x00,0x00,0x02,0x29
            b[0] = 0x15;
            b[1] = 0x00;
            b[2] = 0x00;
            b[3] = 0x02;
            b[4] = 0x29;
            // iRet = DTCCM2_API.WriteSensorI2cEx(0x36, uRegAddr, 4, b, 5, (int)m_iDevID);
            a[0] = 0x09;
            a[1] = 0x01;
            a[2] = 0x06;
            a[3] = 0x02;
            a[4] = 0x15;
            a[5] = 0x00;
            a[6] = 0x00;
            a[7] = 0x02;
            a[8] = 0x29;
            Marshal.Copy(a, 0, pWrData, 9);
            Marshal.Copy(b, 0, pRdData, 5);
            pSensorI2cRw.pWrData = pWrData;
            pSensorI2cRw.pRdData = pRdData;
            pSensorI2cRw.uWrSize = 9;
            pSensorI2cRw.uRdSize = 5; //读 size 为 0，表示写
            iRet = DTCCM2_API.ezSensorI2cRw(ref pSensorI2cRw, m_iDevID);
        }
        public override bool GPIOWrite(int pin, bool bEnable)
        {
            int rtn = 0;
            rtn = DTCCM2_API.SetGpioPinLevel(pin, bEnable , _deviceID);
            if (rtn != 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public override bool WriteI2C(byte in_slave_id, ushort in_addr, ushort in_data, byte mode, int iDevID)
        {
            int rtn = 0;
            rtn = DTCCM2_API.WriteSensorReg(in_slave_id, in_addr, in_data, mode, iDevID);
            if (rtn != 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public override bool EnumerateDev()
        {
            try
            {
                if (m_DevName == null)
                    m_DevName = new string[8];
                if (RcvArray == null)
                    RcvArray = new IntPtr[m_DevName.Length];//Here can't use string
                DTCCM2_API.EnumerateDevice(RcvArray, 8, ref DevNum);
                if (enumList == null)
                    enumList = new List<string>(m_DevName.Length);
                for (int i = 0; i < DevNum; i++)
                {
                    m_DevName[i] = Marshal.PtrToStringAnsi(RcvArray[i]);
                    if (m_DevName[i] != null)
                    {
                        Marshal.FreeCoTaskMem(RcvArray[i]);
                        enumList.Add(m_DevName[i]);
                    }
                }
            }
            catch (Exception ex)
            {
                LogMessage($"Error For LoadImage Capture: {ex.Message}", MessageLevel.Error);
                return false;
            }
            return true;
        }
        public override bool SetParam(string Function,string jsonParamInfo)
        {
            switch (Function)
            {
                case "I2C":
                    if (jsonParamInfo == "")
                    {
                        return true;
                    }
                    else if (jsonParamInfo == "WhiteBoard_ParaList")
                    {
                        try
                        {
                            for (int i = 0; i < _sensorTab.aqexpparaListSize; i += 2)
                            {
                                if (WriteI2C(_sensorTab.aeslaveID, Convert2UInt16(_sensorTab.aqexpparaList[i].ToString()), Convert2UInt16(_sensorTab.aqexpparaList[i + 1].ToString()), _sensorTab.afmode, _deviceID))
                                {
                                    LogMessage("Write I2C Success", MessageLevel.Debug);                                    
                                }
                                else
                                {
                                    LogMessage("Write I2C Fail", MessageLevel.Error);
                                    return false;
                                }
                            }
                            return true;
                        }
                        catch(Exception ex)
                        {
                            LogMessage($"Write I2C Exception :{ ex.Message}", MessageLevel.Error);
                            return false;
                        }
                    }

                    //新增Ori_ParaList原始曝光直讀取
                    else if (jsonParamInfo == "Ori_ParaList")
                    {
                        try
                        {
                            for (int i = 0; i < _sensorTab.aqoriparaListSize; i += 2)
                            {
                                if (WriteI2C(_sensorTab.aeslaveID, Convert2UInt16(_sensorTab.aqoriparaList[i].ToString()), Convert2UInt16(_sensorTab.aqoriparaList[i + 1].ToString()), _sensorTab.afmode, _deviceID))
                                {
                                    LogMessage("Write I2C Success", MessageLevel.Debug);                                
                                }
                                else
                                {
                                    LogMessage("Write I2C Fail", MessageLevel.Error);
                                    return false;
                                }
                            }
                            return true;
                        }
                        catch (Exception ex)
                        {
                            LogMessage($"Write I2C Exception :{ ex.Message}", MessageLevel.Error);
                            return false;
                        }
                    }
                    else
                    {
                        LogMessage("Non-existent paralist", MessageLevel.Error);
                        return false;
                    }
                case "ICR":
                    try
                    {
                        _paramInfo = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonParamInfo);
                        List<string> valuesList = new List<string>();
                        foreach (var kvp in _paramInfo)
                        {
                            valuesList.Add(kvp.Value);
                        }
                        if (valuesList[1] == "true" || valuesList[1] == "false")
                        {
                            bool.TryParse(valuesList[1], out bool Enable);
                            try
                            {
                                if (valuesList[0] != "")
                                {
                                    if (GPIOWrite(Convert2UInt16(valuesList[0]), Enable))
                                    {
                                        LogMessage("Write GPIO Success", MessageLevel.Debug);
                                        return true;
                                    }
                                    else
                                    {
                                        LogMessage("Write GPIO Fail", MessageLevel.Error);
                                        return false;
                                    }
                                }
                                else
                                {
                                    LogMessage("GPIO pin is null", MessageLevel.Error);
                                    return false;
                                }
                            }
                            catch (Exception ex)
                            {
                                LogMessage($"Write GPIO Exception :{ ex.Message}", MessageLevel.Error);
                                return false;
                            }
                        }
                        else
                        {
                            LogMessage("GPIO High_Low input format is wrong", MessageLevel.Error);
                            return false;
                        }
                        
                    }
                    catch(JsonException ex)
                    {
                        LogMessage($"Input string is not Json format :{ ex.Message}", MessageLevel.Error);
                        return false;
                    }
                    
                default:
                    return true;
            }
        }
        public class Image_DevList : TypeConverter  //下拉式選單
        {
            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                if (DevNum > 0)
                {
                    return new StandardValuesCollection(enumList);
                }
                else
                {
                    return new StandardValuesCollection(new int[] { });
                }
            }
            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                return true;
            }

        }

        public class Bayer_Mode : TypeConverter  //下拉式選單
        {
            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                List<string> Bayer_mode = new List<string>();

                Bayer_mode.Add("RGGB"); //1
                Bayer_mode.Add("GRBG"); //2
                Bayer_mode.Add("GBRG"); //3
                Bayer_mode.Add("BGGR"); //4

                return new StandardValuesCollection(Bayer_mode);
            }

            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                return true;
            }
        }



        //public static IntPtr ConvertRaw10ToRaw8(IntPtr raw10Buffer, int width, int height)
        //{
        //    int pixelCount = width * height;
        //    int raw10Length = (pixelCount * 10 + 7) / 8;
        //    byte[] raw10Data = new byte[raw10Length];
        //    Marshal.Copy(raw10Buffer, raw10Data, 0, raw10Length);

        //    byte[] raw8Data = new byte[pixelCount];

        //    int raw10Index = 0;
        //    int raw8Index = 0;

        //    while (raw10Index + 5 <= raw10Length && raw8Index + 4 <= pixelCount)
        //    {
        //        raw8Data[raw8Index++] = raw10Data[raw10Index++];
        //        raw8Data[raw8Index++] = raw10Data[raw10Index++];
        //        raw8Data[raw8Index++] = raw10Data[raw10Index++];
        //        raw8Data[raw8Index++] = raw10Data[raw10Index++];
        //        raw10Index++; // skip the 5th byte (low bits)
        //    }

        //    // 分配 unmanaged 記憶體並複製資料
        //    IntPtr raw8Ptr = Marshal.AllocHGlobal(pixelCount);
        //    Marshal.Copy(raw8Data, 0, raw8Ptr, pixelCount);

        //    return raw8Ptr;
        //}


        //public IntPtr ConvertRaw10ToRaw8(IntPtr raw10Buffer, int raw10Width, int height)
        //{
        //    //計算 RAW8 寬度與總像素數
        //    int raw8Width = (raw10Width * 4 / 5);

        //    int pixelCount = raw8Width * height;

        //   // 計算 RAW10 buffer 長度
        //    int raw10Length = raw10Width * height;
        //    byte[] raw10Data = new byte[raw10Length];
        //    Marshal.Copy(raw10Buffer, raw10Data, 0, raw10Length);

        //    //建立 RAW8 buffer
        //    byte[] raw8Data = new byte[pixelCount];

        //    for (int i = 0; i < pixelCount; i++)
        //    {
        //        //int index = (i >> 2) * 5 + (i % 4); // 等同於 (i / 4) * 5 + (i % 4)
        //        //if (index < raw10Data.Length)
        //        //{
        //        //    raw8Data[i] = raw10Data[index];
        //        //}

        //        int index = (i >> 2) * 5 + (i % 4); // 等同於 (i / 4) * 5 + (i % 4)
        //        raw8Data[i] = raw10Data[index];


        //    }

        //    //將 byte[] 轉為 unmanaged IntPtr
        //    IntPtr raw8Ptr = Marshal.AllocHGlobal(pixelCount); 
        //    Marshal.Copy(raw8Data, 0, raw8Ptr, pixelCount);

        //    return raw8Ptr;
        //}

    }
    public class IniFile
        {
            public string path;             //INI文件名  

            [DllImport("kernel32", CharSet = CharSet.Ansi)]
            private static extern long WritePrivateProfileString(string section, string key,
                        string val, string filePath);

            [DllImport("kernel32", CharSet = CharSet.Ansi)]
            private static extern int GetPrivateProfileString(string section, string key, string def,
                        StringBuilder retVal, int size, string filePath);
            [DllImport("kernel32", CharSet = CharSet.Ansi)]
            private static extern int GetPrivateProfileSection(string lpAppName, byte[] lpszReturnBuffer, int nSize, string FilePath);
            //声明读写INI文件的API函数  
            public IniFile(string INIPath)
            {
                path = INIPath;
            }

            //类的构造函数，传递INI文件名  
            public void IniWriteValue(string Section, string Key, string Value)
            {
                WritePrivateProfileString(Section, Key, Value, this.path);
            }

            //写INI文件  
            public string ReadKey(string Key, string Section)
            {
                StringBuilder temp = new StringBuilder(255);
                int i = GetPrivateProfileString(Section, Key, "", temp, 255, this.path);
                return temp.ToString();
            }
            public string ReadSectionData(string Section)
            {
                byte[] buffer = new byte[32768];
                if (GetPrivateProfileSection(Section, buffer, buffer.Length, this.path) > 0)
                {
                    string str = Encoding.ASCII.GetString(buffer).Trim('\0');
                    return str;
                }
                return "";
            }
            public List<string> ReadIniSections(string filePath)
            {
                var sections = new List<string>();
                foreach (var line in File.ReadAllLines(filePath))
                {
                    if (line.StartsWith("[") && line.EndsWith("]"))
                    {
                        sections.Add(line.Trim('[', ']'));
                    }
                }
                return sections;
            }
        }
    public class DTCCM2_API
        {
            public const string str_dll_file = "dtccm2.dll";

            private const int DEFAULT_DEV_ID = 0;
            //在c#导入中可以定义成 int fun(ref byte abc,ref int edf, ref byte  tagdata)
            //在c#中可以这样调用.
            //先定义变量。
            //byte a;
            //int b;
            //byte[] c=new byte[4];
            //int d;
            //然后给他们赋值。…………
            //最后，可以在c#中可以这样调用
            //d= fun(ref a,ref b, ref c[0]);

            /// @brief 枚举设备，获得设备名及设备个数。
            /// @param DeviceName：枚举的设备名
            /// @param iDeviceNumMax：指定枚举设备的最大个数
            /// @param pDeviceNum：枚举的设备个数
            /// @retval DT_ERROR_OK：枚举操作成功
            /// @retval DT_ERROR_FAILED:枚举操作失败
            /// @retval DT_ERROR_INTERNAL_ERROR:内部错误
            /// @note 获取的设备名称字符串需要用户程序调用GlobalFree()逐个释放。
            [DllImport(str_dll_file, CallingConvention = CallingConvention.Cdecl, EntryPoint = "EnumerateDevice")]
            public static extern int EnumerateDevice(IntPtr[] DeviceName, int iDeviceNumMax, ref int pDeviceNum);

            //  [DllImport("dtccm2.dll", EntryPoint = "EnumerateDevice", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
            //  public static extern int EnumerateDevice(IntPtr[] deviceName, int deviceNumMax, ref int DeviceNum);
            /// @brief 关闭设备，关闭设备后，不能再操作。
            /// @retval DT_ERROR_OK：关闭设备成功
            /// @retval DT_ERROR_FAILD：关闭设备失败
            /// DTFPM_API int _DTCALL_ CloseDevice(int iDevID=DEFAULT_DEV_ID);
            [DllImport(str_dll_file, CallingConvention = CallingConvention.Cdecl, EntryPoint = "CloseDevice")]
            public static extern int CloseDevice(int iDevID = DEFAULT_DEV_ID);

            /// @brief 打开设备，只有打开成功后，设备才能操作;设备对象跟给的ID对应起来iDevID=1 则创建设备对象m_device[1]，iDevID=0 则创建设备对象m_device[0]；
            /// @param pszDeviceName：打开设备的名称
            /// @param pDevID：返回打开设备的ID号
            /// @retval DT_ERROR_OK：打开设备成功
            /// @retval DT_ERROR_FAILD：打开设备失败
            /// @retval DT_ERROR_INTERNAL_ERROR：内部错误
            /// @retval DT_ERROR_PARAMETER_INVALID：参数无效
            /// DTFPM_API int _DTCALL_ OpenDevice(const char *pszDeviceName,int *pDevID,int iDevID=DEFAULT_DEV_ID);
            [DllImport(str_dll_file, CallingConvention = CallingConvention.Cdecl, EntryPoint = "OpenDevice")]
            public static extern int OpenDevice(string DeviceName, ref int pDevID, int iDevID = DEFAULT_DEV_ID);

            /// @brief 判断设备是否打开。 
            /// @retval DT_ERROR_OK：设备已经连接打开
            /// @retval DT_ERROR_FAILED：设备没有连接成功
            /// @retval DT_ERROR_COMM_ERROR：通讯错误
            [DllImport(str_dll_file, CallingConvention = CallingConvention.Cdecl, EntryPoint = "IsDevConnect")]
            public static extern int IsDevConnect(int defDevID = DEFAULT_DEV_ID);

            /// @brief 设置柔性接口是否使能上拉电阻。
            /// @param bPullup：柔性接口上拉使能，bPullup=TRUE使能上拉电阻，bPullup=FALSE关闭上拉电阻
            /// @retval DT_ERROR_OK：设置成功
            /// @retval DT_ERROR_FAILED：设置失败
            /// @retval DT_ERROR_COMM_ERROR：通讯错误
            [DllImport(str_dll_file, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SetSoftPinPullUp")]
            public static extern int SetSoftPinPullUp(bool pullup, int defDevID = DEFAULT_DEV_ID);

            /// @brief 设置柔性接口。
            /// @param PinConfig：柔性接口配置定义
            /// @retval DT_ERROR_OK：柔性接口配置成功
            /// @retval DT_ERROR_FAILED：柔性接口配置失败
            /// @retval DT_ERROR_COMM_ERROR：通讯错误
            [DllImport(str_dll_file, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SetSoftPin")]
            public static extern int SetSoftPin(byte[] pinConfig, int defDevID = DEFAULT_DEV_ID);

            /// @brief 设置是否使能柔性接口，没使能时为高阻状态。
            /// @param bEnable：柔性接口使能
            /// @retval DT_ERROR_OK：设置成功
            /// @retval DT_ERROR_FAILED：设置失败
            /// @retval DT_ERROR_COMM_ERROR：通讯错误
            [DllImport(str_dll_file, CallingConvention = CallingConvention.Cdecl, EntryPoint = "EnableSoftPin")]
            public static extern int EnableSoftPin(bool enble, int defDevID = DEFAULT_DEV_ID);

            /// @brief 使能GPIO。
            /// @param bEnable：使能GPIO
            /// @retval DT_ERROR_OK：设置成功
            /// @retval DT_ERROR_FAILED：设置失败
            /// @retval DT_ERROR_COMM_ERROR：通讯错误
            [DllImport(str_dll_file, CallingConvention = CallingConvention.Cdecl, EntryPoint = "EnableGpio")]
            public static extern int EnableGpio(bool enble, int defDevID = DEFAULT_DEV_ID);

            /// @brief 设置SENSOR的输入时钟。
            /// @param bOnOff：使能SENSOR的输入时钟，为TRUE开启输入时钟，为FALSE关闭输入时钟
            /// @param uHundKhz：SENSOR的输入时钟值，单位为100Khz
            /// @retval DT_ERROR_OK：设置SENSOR输入时钟成功
            /// @retval DT_ERROR_FAILED：设置SENSOR输入时钟失败
            /// @retval DT_ERROR_COMM_ERROR：通讯错误
            [DllImport(str_dll_file, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SetSensorClock")]
            public static extern int SetSensorClock(bool onoff, ushort hundKhz, int defDevID = DEFAULT_DEV_ID);

            /// @brief 设置与SENSOR通讯的I2C总线速率，400Kbps或100Kbps。
            /// @param b400K：b400K=TURE，400Kbps；b400K=FALSE,100Kbps							 
            /// @retval DT_ERROR_OK：设置总线速率操作成功
            [DllImport(str_dll_file, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SetSensorI2cRate")]
            public static extern int SetSensorI2cRate(bool b400K, int defDevID = DEFAULT_DEV_ID);

            /// @brief 使能与SENSOR通讯的I2C总线为Rapid模式。
            /// @param  bRapid=1表示，强制灌电流输出高电平;=0，I2C管脚为输入状态，借助外部上拉变成高电平							 
            /// @retval DT_ERROR_OK：设置I2C总线Rapid模式成功
            [DllImport(str_dll_file, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SetSensorI2cRapid")]
            public static extern int SetSensorI2cRapid(bool brapid, int defDevID = DEFAULT_DEV_ID);

            /// @brief 设置I2C的字节间隔(HS系列,PE950支持)
            /// @brief uInterval：字节间隔设置,单位us
            [DllImport(str_dll_file, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SetI2CInterval")]
            public static extern int SetI2CInterval(UInt32 ctrl, int defDevID = DEFAULT_DEV_ID);

            /// @brief 通过Reset,PWDN管脚开启或关闭SENSOR。
            /// @param byPin：Reset，PWDN，PWDN2
            /// @param bEnable：开启或关闭SENSOR
            /// @retval DT_ERROR_OK：开启或关闭SENSOR操作成功
            /// @retval DT_ERROR_FAILED：开启或关闭SENSOR操作失败
            /// @retval DT_ERROR_COMM_ERROR：通讯错误
            [DllImport(str_dll_file, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SensorEnable")]
            public static extern int SensorEnable(byte pin, bool enable, int defDevID = DEFAULT_DEV_ID);

            /// @brief 复位与Sensor通讯的I2C总线。
            /// @retval DT_ERROR_OK：复位I2C操作成功
            /// @retval DT_ERROR_FAILED：复位I2C操作失败
            [DllImport(str_dll_file, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ResetSensorI2cBus")]
            public static extern int ResetSensorI2cBus(int defDevID = DEFAULT_DEV_ID);

            /// @brief 初始化SENSOR。
            /// @param uDevAddr：SENSOR器件地址
            /// @param pParaList：SENSOR的参数列表
            /// @param uLength：pParaList的大小
            /// @param byI2cMode：访问SENSOR的I2C模式，参见枚举类型I2CMODE
            /// @retval DT_ERROR_OK：初始化SENSOR成功
            /// @retval DT_ERROR_FAILED：初始化SENSOR失败
            [DllImport(str_dll_file, CallingConvention = CallingConvention.Cdecl, EntryPoint = "InitSensor")]
            public static extern int InitSensor(byte addr, ushort[] paraList, ushort length, byte i2cmode, int defDevID = DEFAULT_DEV_ID);

            /// @brief 设置MIPI接口接收器时钟相位。
            /// @param byPhase：MIPI接口接收器时钟相位（可以设置的值是0-7）
            /// @retval DT_ERROR_OK：设置MIPI接口接收器时钟相位成功
            /// @retval DT_ERROR_FAILED：设置MIPI接口接收器时钟相位失败
            /// @retval DT_ERROR_TIME_OUT：设置超时
            /// @retval DT_ERROR_COMM_ERROR：通讯错误
            [DllImport(str_dll_file, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SetMipiClkPhase")]
            public static extern int SetMipiClkPhase(byte phase, int defDevID = DEFAULT_DEV_ID);

            /// @brief 读SESNOR寄存器,I2C通讯模式byI2cMode的设置值见I2CMODE定义。
            /// @param uAddr：从器件地址
            /// @param uReg：寄存器地址
            /// @param pValue：读到的寄存器的值
            /// @param byMode：I2C模式
            /// @retval DT_ERROR_OK：读SENSOR寄存器操作成功
            /// @retval DT_ERROR_COMM_ERROR：通讯错误
            /// @retval DT_ERROR_PARAMETER_INVALID：byMode参数无效
            /// @retval DT_ERROR_TIME_OUT：通讯超时
            /// @retval DT_ERROR_INTERNAL_ERROR：内部错误
            [DllImport(str_dll_file, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ReadSensorReg")]
            public static extern int ReadSensorReg(byte devAddr, ushort regAddr, ref ushort data, byte mode, int defDevID = DEFAULT_DEV_ID);

            /// @brief 写SENSOR寄存器，支持向一个寄存器写入一个数据块（不超过255字节）。
            /// @param uDevAddr：从器件地址
            /// @param uRegAddr：寄存器地址
            /// @param uRegAddrSize：寄存器地址的字节数
            /// @param pData：写入寄存器的数据块
            /// @param uSize：写入寄存器的数据块的字节数（不超过255字节(HS300/HS300D/HV910/HV910D一次不能超过253字节)）
            /// @retval DT_ERROR_OK：完成写SENSOR寄存器块操作成功
            /// @retval DT_ERROR_COMM_ERROR：通讯错误
            /// @retval DT_ERROR_PARAMETER_INVALID：uSize参数无效
            /// @retval DT_ERROR_TIME_OUT：通讯超时
            /// @retval DT_ERROR_INTERNAL_ERROR：内部错误
            [DllImport(str_dll_file, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ReadSensorI2c")]
            public static extern int ReadSensorI2c(byte devAddr, ushort regAddr, byte regAddrSize, IntPtr lp, ushort size, bool noStop = false, int defDevID = DEFAULT_DEV_ID);

            /// @brief 获取电源电压，如果能获取检测到的，尽量使用检测到的数据，否则返回电压设置值。
            /// @param Power：电源类型，参见枚举类型“SENSOR_POWER”
            /// @param Voltage：获取的电源电压值，单位mV
            /// @param iCount：电源路数
            /// @retval DT_ERROR_OK：设置电源电压成功
            /// @retval DT_ERROR_FAILD：设置电源电压失败
            /// @retval DT_ERROR_COMM_ERROR：通讯错误
            /// @retval DT_ERROR_PARAMETER_OUT_OF_BOUND：参数超出了范围
            /// @see SENSOR_POWER
            [DllImport(str_dll_file, CallingConvention = CallingConvention.Cdecl, EntryPoint = "PmuGetVoltage")]
            public static extern int PmuGetVoltage(SENSOR_POWER[] power, int[] voltage, int count, int defDevID = DEFAULT_DEV_ID);

            /// @brief 设置电源电压。
            /// @param Power：电源类型，参见枚举类型“SENSOR_POWER”
            /// @param Voltage：设置的电源电压值，单位mV
            /// @param iCount：电源路数
            /// @retval DT_ERROR_OK：设置电源电压成功
            /// @retval DT_ERROR_FAILD：设置电源电压失败
            /// @retval DT_ERROR_COMM_ERROR：通讯错误
            /// @retval DT_ERROR_PARAMETER_OUT_OF_BOUND：参数超出了范围
            /// @see SENSOR_POWER
            [DllImport(str_dll_file, CallingConvention = CallingConvention.Cdecl, EntryPoint = "PmuSetVoltage")]
            public static extern int PmuSetVoltage(SENSOR_POWER[] power, int[] voltage, int count, int defDevID = DEFAULT_DEV_ID);

            /// @brief 设置电源开关状态。
            /// @param Power：电源类型，参见枚举类型“SENSOR_POWER”
            /// @param OnOff：设置电源开关状态，TRUE为开启，FALSE为关闭
            /// @param iCount：电源路数
            /// @retval DT_ERROR_OK：设置电源开关状态成功
            /// @retval DT_ERROR_FAILD：设置电源开关状态失败
            /// @retval DT_ERROR_COMM_ERROR：通讯错误
            /// @retval DT_ERROR_PARAMETER_OUT_OF_BOUND：参数超出了范围
            /// @see SENSOR_POWER
            [DllImport(str_dll_file, CallingConvention = CallingConvention.Cdecl, EntryPoint = "PmuSetOnOff")]
            public static extern int PmuSetOnOff(SENSOR_POWER[] power, bool[] onoff, int count, int defDevID = DEFAULT_DEV_ID);

            /// @brief 设置电源电流量程。
            /// @param Power：电源类型，参见枚举类型“SENSOR_POWER”
            /// @param Range：电源电流量程，参见枚举类型“CURRENT_RANGE”
            /// @param iCount：电源路数
            /// @retval DT_ERROR_OK：设置电源电流量程成功
            /// @retval DT_ERROR_FAILD：设置电源电流量程失败
            /// @see SENSOR_POWER
            /// @see CURRENT_RANGE
            /// @note 该函数仅UV910/DTLC2/UH910/UH920/UF920/PE350/PE950支持。
            [DllImport(str_dll_file, CallingConvention = CallingConvention.Cdecl, EntryPoint = "PmuSetCurrentRange")]
            public static extern int PmuSetCurrentRange(SENSOR_POWER[] power, CURRENT_RANGE[] range, int count, int defDevID = DEFAULT_DEV_ID);

            /// @brief 设置过流保护的电流限制,设定值(CurrentLimit)单位:mA。
            /// @param Power：电源类型，参见枚举类型“SENSOR_POWER”
            /// @param CurrentLimit：设置过流保护的电流限制值，单位mA
            /// @param iCount：电源路数
            /// @retval DT_ERROR_OK：设置过流保护的电流限制成功
            /// @retval DT_ERROR_FAILD：设置过流保护的电流限制失败
            /// @retval DT_ERROR_COMM_ERROR：通讯错误
            /// @retval DT_ERROR_PARAMETER_OUT_OF_BOUND：参数超出了范围
            /// @see SENSOR_POWER
            [DllImport(str_dll_file, CallingConvention = CallingConvention.Cdecl, EntryPoint = "PmuSetOcpCurrentLimit")]
            public static extern int PmuSetOcpCurrentLimit(SENSOR_POWER[] power, int[] currentlimit, int count, int defDevID = DEFAULT_DEV_ID);

            /// @brief 返回设备的型号，区分不同的测试板。
            ///
            /// @retval 0x0010：HS128测试板
            /// @retval 0x0020：HS230测试板
            /// @retval 0x0030：HS300测试板
            /// @retval 0x0031：HS300D测试板
            /// @retval 0x0092：HV910测试板
            /// @retval 0x0093：HV910D测试板
            /// @retval 0x0082：HV810测试板
            /// @retval 0x0083：HV810D测试板
            ///
            /// @retval 0x0130：PE300测试板
            /// @retval 0x0131：PE300D测试板
            /// @retval 0x0190：PE910测试板
            ///	@retval 0x0191：PE910D测试板
            /// @retval 0x0180：PE810测试板
            ///	@retval 0x0181：PE810D测试板
            /// @retval 0x0132：PE350测试板
            /// @retval 0x0192：PE950测试板
            /// @retval 0x0193：MP950测试板
            ///
            ///	@retval 0x0231：UT300测试板
            /// @retval 0x0232：UO300测试板
            /// @retval 0x0233: UM330测试板
            /// @retval 0x0295：UM900测试板
            /// @retval 0x0296：MU950测试板
            /// @retval 0x0297：DMU956测试板
            /// @retval 0x0239：ULV330测试板
            /// @retval 0x0299：ULV913测试板
            ///	@retval 0x0292：UV910测试板
            ///	@retval 0x0293：UH910测试板
            ///	@retval 0x02A1：DTLC2测试板
            /// @retval 0x0295：UF920测试板
            ///	@retval 0x0294：UH920测试板
            /// DTFPM_API DWORD _DTCALL_ GetKitType(int iDevID=DEFAULT_DEV_ID);
            [DllImport(str_dll_file, CallingConvention = CallingConvention.Cdecl, EntryPoint = "GetKitType")]
            public static extern uint GetKitType(int iDevID = 0);

            /// @brief OS测试参数配置。
            ///
            /// @param Voltage：测试电压，单位uV
            /// @param HighLimit：Open测试标准数组，测试之前应该把每一个测试pin的开路标准初始化好，单位uV
            /// @param LowLimit：Short测试标准数组，测试之前应该把每一个测试pin的开路标准初始化好，单位uV
            /// @param PinNum：管脚数，这个决定HighLimit、LowLimit数组大小
            /// @param PowerCurrent：电源pin电流，单位uA
            /// @param GpioCurrent：GPIOpin电流，单位uA
            ///
            /// @retval DT_ERROR_OK：OS测试参数配置成功
            /// @retval DT_ERROR_FAILD：OS测试参数配置失败
            /// @retval DT_ERROR_COMM_ERROR：通讯错误
            /// DTFPM_API int _DTCALL_ OS_Config(int Voltage, int HighLimit[], int LowLimit[], int PinNum, int PowerCurrent, int GpioCurrent, int iDevID=DEFAULT_DEV_ID);
            [DllImport(str_dll_file, CallingConvention = CallingConvention.Cdecl, EntryPoint = "OS_Config")]
            public static extern int OS_Config(int Voltage, int[] HighLimit, int[] LowLimit, int PinNum, int PowerCurrent, int GpioCurrent, int iDevID = 0);

            /// @brief LC/OS测试操作配置。
            /// 
            /// @param Command：操作码，参见宏定义“OS/LC测试配置定义”
            /// @param IoMask：有效管脚标识位，每字节的每bit对应一个管脚，如果这些bit为1，表示对应的管脚将参与测试
            /// @param PinNum：管脚数，这个决定IoMask数组大小，一般情况下IoMask的字节数为：PinNum/8+(PinNum%8!=0)
            ///
            /// @retval DT_ERROR_OK：LC/OS测试操作配置成功
            /// @retval DT_ERROR_FAILD：LC/OS测试操作配置失败
            /// @retval DT_ERROR_COMM_ERROR：通讯错误
            ///  DTFPM_API int _DTCALL_ LC_OS_CommandConfig(DWORD Command, UCHAR IoMask[], int PinNum, int iDevID=DEFAULT_DEV_ID);
            [DllImport(str_dll_file, CallingConvention = CallingConvention.Cdecl, EntryPoint = "LC_OS_CommandConfig")]
            public static extern int LC_OS_CommandConfig(uint Command, byte[] IoMask, int PinNum, int iDevID = 0);

            /// @brief OS测试结果读取。
            ///
            /// @param VoltageH：正向pos测试结果，单位uV
            /// @param VoltageL：反向pos测试结果，单位uV
            /// @param Result：开短路测试结果，参见宏定义“OS测试结果定义”
            /// @param PosEn：正向测试使能 
            /// @param NegEn：反向测试使能
            /// @param PinNum：管脚数，这个决定VoltageH、VoltageL，Result数组大小
            /// 
            /// @retval DT_ERROR_OK：OS测试结果读取成功
            /// @retval DT_ERROR_FAILD：OS测试结果读取失败
            /// @retval DT_ERROR_COMM_ERROR：通讯错误
            /// DTFPM_API int _DTCALL_ OS_Read(int VoltageH[], int VoltageL[], UCHAR Result[], BOOL PosEn, BOOL NegEn, int PinNum, int iDevID=DEFAULT_DEV_ID);
            [DllImport(str_dll_file, CallingConvention = CallingConvention.Cdecl, EntryPoint = "OS_Read")]
            public static extern int OS_Read(int[] VoltageH, int[] VoltageL, byte[] Result, Boolean PosEn, Boolean NegEn, int PinNum, int iDevID = 0);

            /// @brief 采集一帧图像，并且返回帧的一些信息，通过帧信息结构体可以获取帧的时间戳、当前帧的ECC错误计数、CRC错误计数等
            ///
            /// @param pInBuffer：采集图像BUFFER
            /// @param uBufferSize：采集图像BUFFER大小，单位字节
            /// @param pGrabSize：实际抓取的图像数据大小，单位字节
            /// @param pInfo：返回的图像数据信息
            ///
            /// @retval DT_ERROR_OK：采集一帧图像成功
            /// @retval DT_ERROR_FAILD：采集一帧图像失败，可能不是完整的一帧图像数据
            /// @retval DT_ERROR_TIME_OUT：采集超时
            /// @retval DT_ERROR_INTERNAL_ERROR：内部错误 
            /// 
            /// @note 调用该函数之前，请先根据图像大小获取到足够大的缓存区用于装载图像数据。\n
            /// 同时，缓存区的大小也需要作为参数传入到GrabFrameEx函数，以防止异常情况下导致的内存操作越界问题。 

            [DllImport(str_dll_file, CallingConvention = CallingConvention.Cdecl, EntryPoint = "GrabFrameEx")]
            public static extern int GrabFrameEx(IntPtr pInBuffer, uint uBufferSize, ref uint pGrabSize, ref FrameInfoEx pInfo, int iDevID = 0);
            /// @brief 采集一帧图像，并且返回帧的一些信息，A通道和B通道都是使用GrabFrame函数获取图像数据，通过帧信息可以区分图像数据所属的通道。
            /// @param pInBuffer：采集图像BUFFER
            /// @param uBufferSize：采集图像BUFFER大小，单位字节
            /// @param pGrabSize：实际抓取的图像数据大小，单位字节
            /// @param pInfo：返回的图像数据信息
            /// @retval DT_ERROR_OK：采集一帧图像成功
            /// @retval DT_ERROR_FAILD：采集一帧图像失败，可能不是完整的一帧图像数据
            /// @retval DT_ERROR_TIME_OUT：采集超时
            /// @retval DT_ERROR_INTERNAL_ERROR：内部错误 
            /// @note 调用该函数之前，请先根据图像大小获取到足够大的缓存区用于装载图像数据。\n
            /// 同时，缓存区的大小也需要作为参数传入到GrabFrame函数，以防止异常情况下导致的内存操作越界问题。
            [DllImport(str_dll_file, CallingConvention = CallingConvention.Cdecl, EntryPoint = "GrabFrame")]
            public static extern int GrabFrame(IntPtr pInBuffer, uint uBufferSize, ref uint pGrabSize, ref FrameInfo pInfo, int iDevID = 0);
            // unsafe public static extern int GrabFrame(byte* imagebuffer, uint buffersize, ref uint grabsize, ref FrameInfo info, int defDevID = DEFAULT_DEV_ID);
            // DTCCM_API int _DTCALL_ GrabFrame(BYTE* pInBuffer, ULONG uBufferSize, ULONG* pGrabSize, FrameInfo* pInfo, int iDevID = DEFAULT_DEV_ID);
            /// @brief 对RAW图像数据进行图像处理(MONO,WB,ColorChange,Gamma,Contrast)。
            ///
            /// @param pImage：RAW图像数据
            /// @param pBmp24：经过图像处理后的数据
            /// @param uWidth：图像数据宽度
            /// @param uHeight：图像数据高度
            /// @param pInfo：帧信息，参见结构体“FrameInfo”
            /// 
            /// @retval DT_ERROR_OK：图像处理成功
            /// @retval DT_ERROR_PARAMETER_INVALID：pData无效的参数
            /// @retval DT_ERROR_INTERNAL_ERROR：内部错误
            /// DTFPM_API int _DTCALL_ ImageProcess(BYTE *pImage, BYTE *pBmp24, int nWidth, int nHeight,FrameInfo *pInfo,int iDevID=DEFAULT_DEV_ID);
            [DllImport(str_dll_file, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ImageProcess")]
            public static extern int ImageProcess(IntPtr pImage, IntPtr pBmp24, int nWidth, int nHeight, ref FrameInfo pInfo, int iDevID = 0);


            /// @brief 显示RGB图像数据。
            ///
            /// @param pBmp24：待显示的RGB24格式的数据
            /// @param pInfo：帧信息，参见结构体“FrameInfo”
            ///
            /// @retval DT_ERROR_OK：显示RGB图像成功
            /// @retval DT_ERROR_FAILD：显示RGB图像失败 
            /// DTFPM_API int _DTCALL_ DisplayRGB24(BYTE *pBmp24,FrameInfo *pInfo=NULL,int iDevID=DEFAULT_DEV_ID);
            [DllImport(str_dll_file, CallingConvention = CallingConvention.Cdecl, EntryPoint = "DisplayRGB24")]
            public static extern int DisplayRGB24(IntPtr pBmp24, ref FrameInfo pInfo, int iDevID = 0);

            /// @brief 开启图像数据采集。
            ///
            /// @param uImgBytes：图像数据大小，单位字节
            ///
            /// @retval DT_ERROR_OK：开启图像数据采集成功
            /// @retval DT_ERROR_FAILD：开启图像数据采集失败
            /// @retval DT_ERROR_INTERNAL_ERROR：内部错误
            /// @retval DT_ERROR_COMM_ERROR：通讯错误
            /// DTFPM_API int _DTCALL_ OpenVideo(UINT uImgBytes,int iDevID=DEFAULT_DEV_ID);
            [DllImport(str_dll_file, CallingConvention = CallingConvention.Cdecl, EntryPoint = "OpenVideo")]
            public static extern int OpenVideo(uint uImgBytes, int iDevID = 0);
            /// @brief 关闭图像数据采集。
            ///
            /// @retval DT_ERROR_OK：关闭图像数据采集成功
            /// @retval DT_ERROR_FAILD：关闭图像数据采集失败
            /// @retval DT_ERROR_COMM_ERROR：通讯错误
            /// DTFPM_API int _DTCALL_ CloseVideo(int iDevID=DEFAULT_DEV_ID);
            [DllImport(str_dll_file, CallingConvention = CallingConvention.Cdecl, EntryPoint = "CloseVideo")]
            public static extern int CloseVideo(int iDevID = 0);

            /// @brief 初始化显示，支持2个窗口显示，如果使用2个sensor，须要使用hWndEx指定第二个窗口。
            /// @param hWnd：显示A通道图像的窗口句柄
            /// @param uImgWidth：图像数据宽度
            /// @param uHeight：图像数据高度
            /// @param byImgFormat：图像数据格式，如：RAW/YUV
            /// @param hWndEx：hWndEx：显示B通道图像的窗口句柄
            /// 
            [DllImport(str_dll_file, CallingConvention = CallingConvention.Cdecl, EntryPoint = "InitDisplay")]
            public static extern int InitDisplay(uint hwnd, ushort imgWidth, ushort imgHeight, byte imgFormat, byte channel, IntPtr hwndEx, int defDevID = DEFAULT_DEV_ID);

            /// @brief 初始化ISP
            /// @param uImgWidth：图像数据宽度
            /// @param uHeight：图像数据高度
            /// @param byImgFormat：图像数据格式，如：RAW/YUV
            /// @param byChannel：A/B通道选择
            [DllImport(str_dll_file, CallingConvention = CallingConvention.Cdecl, EntryPoint = "InitIsp")]
            public static extern int InitIsp(ushort imgWidth, ushort imgHeight, byte imgFormat, byte imgChannel, int defDevID = DEFAULT_DEV_ID);
            /// @brief 初始化设备，该函数主要用于初始化设备的SENSOR接口类型，图像格式，图像宽高信息，同时还要求用户传入用于视频显示的窗口句柄
            ///
            /// @param hWnd：显示A通道图像的窗口句柄
            /// @param uImgWidth，uImgHeight：设置SENSOR输出的宽高信息（单位：像素，可能ROI之后的结果）
            /// @param bySensorPortType：SENSOR输出接口类型，如：MIPI/并行
            /// @param byImgFormat：图像数据格式，sesor原始格式，如：RAW/YUV
            /// @param byChannel：A通道/B通道/AB同时工作
            /// @param hWndEx：显示B通道图像的窗口句柄
            ///
            /// @retval DT_ERROR_OK：初始化成功
            /// @retval DT_ERROR_FAILD：初始化失败
            /// @retval DT_ERROR_PARAMETER_INVALID：bySensorPort参数无效
            ///
            /// @note InitDevice函数支持初始化双通道测试板（如DTLC2/UH910），如果须要使用这类测试板的B通道，请做如下额外操作：
            /// @note byChannel参数传入CHANNEL_A|CHANNEL_B；hWndEx参数传入用于B通道视频显示的窗口句柄
            //  DTCCM_API int _DTCALL_ InitDevice(HWND hWnd,USHORT uImgWidth, USHORT uImgHeight,BYTE bySensorPortType,BYTE byImgFormat,BYTE byChannel = CHANNEL_A,HWND hWndEx = NULL,int iDevID = DEFAULT_DEV_ID);
            /// @param byChannel：A/B通道选择
            [DllImport(str_dll_file, CallingConvention = CallingConvention.Cdecl, EntryPoint = "InitDevice")]
            public static extern int InitDevice(IntPtr hwnd, ushort imgWidth, ushort imgHeight, byte bysensorportType, byte imgFormat, byte imgChannel, IntPtr hWndEx, int defDevID = DEFAULT_DEV_ID);
            /// @brief 设置ROI。
            /// @param roi_x0：起始点水平坐标，单位像素
            /// @param roi_y0：起始点垂直坐标，单位像素
            /// @param roi_hw：水平方向ROI图像宽度，单位像素
            /// @param roi_vw：垂直方向ROI图像高度，单位像素
            /// @param roi_hb：水平方向blank宽度，单位像素
            /// @param roi_vb：水平方向blank高度，单位像素
            /// @param roi_hnum：水平方向ROI数量，单位像素
            /// @param roi_vnum：垂直方向ROI数量，单位像素
            /// @param byImgFormat：图像数据格式，如：RAW/YUV
            /// @param roi_en：ROI使能
            /// @retval DT_ERROR_OK：ROI设置成功
            /// @retval DT_ERROR_FAILD：ROI设置失败
            /// @retval DT_ERROR_COMM_ERROR：通讯错误
            /// @note 该函数中指定宽度和水平位置是以像素为单位，并且要保证宽度转为字节后是16字节的整数倍。
            [DllImport(str_dll_file, CallingConvention = CallingConvention.Cdecl, EntryPoint = "InitRoi")]
            public static extern int InitRoi(ushort roi_x0, ushort roi_y0, ushort roi_hw, ushort roi_vw, ushort roi_hb, ushort roi_vb, ushort roi_hnum, ushort roi_vnum, byte imgFormat, bool enable, int defDevID = DEFAULT_DEV_ID);

            /// @brief 设置SENSOR图像数据接口类型。
            /// @param byPort：SENSOR图像数据接口类型，参见枚举类型“SENSOR_PORT”
            /// @param uWidth：图像数据宽度
            /// @param uHeight：图像数据高度
            /// @retval DT_ERROR_OK：设置SENSOR图像数据接口类型成功
            /// @retval DT_ERROR_FAILD：设置SENSOR图像数据接口类型失败 
            /// @retval DT_ERROR_PARAMETER_INVALID：无效的图像数据接口类型参数
            /// @see SENSOR_PORT
            [DllImport(str_dll_file, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SetSensorPort")]
            public static extern int SetSensorPort(byte port, ushort width, ushort height, int defDevID = DEFAULT_DEV_ID);

            /// @brief 返回实际抓取图像数据的大小（单位字节）。
            /// @param pGrabSize：返回实际抓取图像数据大小
            [DllImport(str_dll_file, CallingConvention = CallingConvention.Cdecl, EntryPoint = "CalculateGrabSize")]
            public static extern int CalculateGrabSize(ref uint grabSize, int defDevID = DEFAULT_DEV_ID);

            /// @brief 设置GAMMA值。
            /// @param iGamma：设置的GAMMA值
            [DllImport(str_dll_file, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SetGamma")]
            public static extern int SetGamma(int gamma, int defDevID = DEFAULT_DEV_ID);

            /// @brief 获取对比度设置值。
            /// @param pContrast：返回的对比度设置值
            [DllImport(str_dll_file, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SetContrast")]
            public static extern int SetContrast(int contrast, int defDevID = DEFAULT_DEV_ID);

            /// @brief 设置饱和度。
            /// @param iSaturation：设置饱和度值
            [DllImport(str_dll_file, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SetSaturation")]
            public static extern int SetSaturation(int saturation, int defDevID = DEFAULT_DEV_ID);

            /// @brief 设置RGB数字增益。
            /// @param fRGain：R增益值
            /// @param fGGain：G增益值
            /// @param fBGain：B增益值
            [DllImport(str_dll_file, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SetDigitalGain")]
            public static extern int SetDigitalGain(float gainR, float gainG, float gainB, int defDevID = DEFAULT_DEV_ID);

            /// @brief 设置RAW格式，参见枚举类型“RAW_FORMAT”。
            /// @param byRawMode：RAW格式设置
            /// @see RAW_FORMAT
            [DllImport(str_dll_file, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SetRawFormat")]
            public static extern int SetRawFormat(byte rawmode, int defDevID = DEFAULT_DEV_ID);

            [DllImport(str_dll_file, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SetYUV422Format")]
            public static extern int SetYUV422Format(byte rawmode, int defDevID = DEFAULT_DEV_ID);

            [DllImport(str_dll_file, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SetAWB")]
            public static extern int SetAWB(byte bAWBEn, int defDevID = DEFAULT_DEV_ID);

            [DllImport(str_dll_file, CallingConvention = CallingConvention.Cdecl, EntryPoint = "GetMipiCtrlEx")]
            public static extern int GetMipiCtrlEx(ref MipiCtrlEx_t pMipiCtrl, int defDevID = DEFAULT_DEV_ID);

            [DllImport(str_dll_file, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SetMipiCtrlEx")]
            public static extern int SetMipiCtrlEx(MipiCtrlEx_t pMipiCtrl, int defDevID = DEFAULT_DEV_ID);

            //DTCCM_API int _DTCALL_ SetMipiImageVC(UINT uVC, BOOL bVCFilterEn, BYTE byChannel = CHANNEL_A, int iDevID = DEFAULT_DEV_ID);
            [DllImport(str_dll_file, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SetMipiImageVC")]
            public static extern int SetMipiImageVC(uint uVC, bool bVCFilterEn, byte imgChannel, int defDevID = DEFAULT_DEV_ID);

            /// @brief 校准sensor接收，建议openvideo之后调用，校准成功再进行抓帧操作,建议超时时间大于1000ms
            /// 
            /// @param uTimeOut：校准超时时间设置，单位ms
            /// 
            /// @retval DT_ERROR_OK：校准成功，可以采集图像
            /// @retval DT_ERROR_TIME_OUT：校准超时
            // DTCCM_API int _DTCALL_ CalibrateSensorPort(ULONG uTimeOut, int iDevID = DEFAULT_DEV_ID);
            [DllImport(str_dll_file, CallingConvention = CallingConvention.Cdecl, EntryPoint = "CalibrateSensorPort")]
            public static extern int CalibrateSensorPort(uint uTimeOut, int defDevID = DEFAULT_DEV_ID);

            /// @brief 获取设备的唯一序列号
            ///
            /// @param pSN：返回的设备唯一序列号
            /// @param iBufferSize：设置要获取序列号字节的长度,最大支持32字节
            /// @param pRetLen：返回实际设备序列号字节长度
            /// 
            /// @retval DT_ERROR_OK：获取设备的序列号成功
            /// @retval DT_ERROR_FAILED：获取设备的序列号失败
            /// @retval DT_ERROR_COMM_ERROR：通讯错误
            ///     
            [DllImport(str_dll_file, CallingConvention = CallingConvention.Cdecl, EntryPoint = "GetDeviceSN")]
            //DTCCM_API int _DTCALL_ GetDeviceSN(BYTE* pSN, int iBufferSize, int* pRetLen, int iDevID = DEFAULT_DEV_ID);
            public static extern int GetDeviceSN(Byte[] pSN, int iBufferSize, ref int pRetLen, int iDevID = DEFAULT_DEV_ID);

            // @brief 获取FrameBuffer的配置信息
            /// 
            /// @param pConfig：FrameBuffer配置结构体,该结构体说明参见imagekit.h头文件
            [DllImport(str_dll_file, CallingConvention = CallingConvention.Cdecl, EntryPoint = "GetFrameBufferConfig")]
            //  DTCCM_API int _DTCALL_ GetFrameBufferConfig(FrameBufferConfig* pConfig, int iDevID = DEFAULT_DEV_ID);
            public static extern int GetFrameBufferConfig(ref FrameBufferConfig pConfig, int iDevID = DEFAULT_DEV_ID);
            /// @brief 配置FrameBuffer
            /// 
            /// @param pConfig：FrameBuffer配置结构体,该结构体说明参见imagekit.h头文件
            [DllImport(str_dll_file, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SetFrameBufferConfig")]
            //DTCCM_API int _DTCALL_ SetFrameBufferConfig(FrameBufferConfig* pConfig, int iDevID = DEFAULT_DEV_ID);
            public static extern int SetFrameBufferConfig(FrameBufferConfig pConfig, int iDevID = DEFAULT_DEV_ID);


            /// @brief 设置错误帧过滤是否使能
            ///
            /// @param pFrameFilter：帧过滤使能信息
            /// 
            /// @retval DT_ERROR_OK：设置成功
            // DTCCM_API int _DTCALL_ SetErrFrameFilter(FrameFilter_t* pFrameFilter, int iDevID = DEFAULT_DEV_ID);
            /// @param pConfig：FrameBuffer配置结构体,该结构体说明参见imagekit.h头文件
            [DllImport(str_dll_file, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SetErrFrameFilter")]
            //DTCCM_API int _DTCALL_ SetFrameBufferConfig(FrameBufferConfig* pConfig, int iDevID = DEFAULT_DEV_ID);
            public static extern int SetErrFrameFilter(FrameFilter pFrameFilter, int iDevID = DEFAULT_DEV_ID);
            /// @brief 获取错误帧过滤使能状态
            ///
            /// @param pFrameFilter：帧过滤使能信息
            /// 
            /// @retval DT_ERROR_OK：设置成功
            // DTCCM_API int _DTCALL_ GetErrFrameFilter(FrameFilter_t* pFrameFilter, int iDevID = DEFAULT_DEV_ID);
            /// @param pConfig：FrameBuffer配置结构体,该结构体说明参见imagekit.h头文件
            [DllImport(str_dll_file, CallingConvention = CallingConvention.Cdecl, EntryPoint = "GetErrFrameFilter")]
            //  DTCCM_API int _DTCALL_ GetFrameBufferConfig(FrameBufferConfig* pConfig, int iDevID = DEFAULT_DEV_ID);
            public static extern int GetErrFrameFilter(ref FrameFilter pFrameFilter, int iDevID = DEFAULT_DEV_ID);

            [DllImport(str_dll_file, CallingConvention = CallingConvention.Cdecl, EntryPoint = "GetParaCtrl")]
            //  DTCCM_API int _DTCALL_ GetFrameBufferConfig(FrameBufferConfig* pConfig, int iDevID = DEFAULT_DEV_ID);
            public static extern int GetParaCtrl(ref ulong pdwCtrl, int iDevID = DEFAULT_DEV_ID);
            [DllImport(str_dll_file, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SetParaCtrl")]
            //  DTCCM_API int _DTCALL_ GetFrameBufferConfig(FrameBufferConfig* pConfig, int iDevID = DEFAULT_DEV_ID);
            public static extern int SetParaCtrl(ulong pdwCtrl, int iDevID = DEFAULT_DEV_ID);


            //DTCCM_API int _DTCALL_ ImageTransform(DtImage_t* srcImg, DtImage_t* destImg, DtRoi_t roi[], int roiCount, int iDevID = DEFAULT_DEV_ID);

            //[DllImport(str_dll_file, EntryPoint = "ezImageTransform", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            [DllImport(str_dll_file, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ImageTransform")]
            public static extern int ImageTransform(ref DtImage_t srcImg, ref DtImage_t destImg, [In, Out] DtRoi_t[] pOsResult, int roiCount, int iDevID = DEFAULT_DEV_ID);

            [DllImport(str_dll_file, CallingConvention = CallingConvention.Cdecl, EntryPoint = "WriteSensorReg")]
            public static extern int WriteSensorReg(byte uAddr, ushort uReg, ushort uValue, byte byMode, int iDevID = DEFAULT_DEV_ID);

            /// @param uDevAddr：从器件地址
            /// @param uRegAddr：寄存器地址
            /// @param uRegAddrSize：寄存器地址的字节数
            /// @param pData：写入寄存器的数据块
            /// @param uSize：写入寄存器的数据块的字节数
            ///
            /// @retval DT_ERROR_OK：完成写SENSOR寄存器块操作成功
            /// @retval DT_ERROR_COMM_ERROR：通讯错误
            /// @retval DT_ERROR_PARAMETER_INVALID：uSize参数无效
            /// @retval DT_ERROR_TIME_OUT：通讯超时
            /// @retval DT_ERROR_INTERNAL_ERROR：内部错误
            //DTCCM_API int _DTCALL_ WriteSensorI2cEx(UCHAR uDevAddr, UINT uRegAddr, UCHAR uRegAddrSize, BYTE* pData, USHORT uSize, int iDevID = DEFAULT_DEV_ID);

            [DllImport(str_dll_file, CallingConvention = CallingConvention.Cdecl, EntryPoint = "WriteSensorI2cEx")]
            public static extern int WriteSensorI2cEx(byte uDevAddr, UInt32 uRegAddr, byte uRegAddrSize, byte[] pData, ushort uSize, int iDevID = DEFAULT_DEV_ID);

            [DllImport(str_dll_file, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SetGpioPinLevel")]
            public static extern int SetGpioPinLevel(int iPin, bool bLevel, int iDevID = DEFAULT_DEV_ID);

            [DllImport(str_dll_file, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ezSensorI2cRw")]
            public static extern int ezSensorI2cRw(ref ezSensorI2cRw_t pSensorI2cRw, int iDevID = DEFAULT_DEV_ID);
            //TCCM_API int _DTCALL_ ezSensorI2cRw(ezSensorI2cRw_t* pSensorI2cRw, int iDevID = DEFAULT_DEV_ID);
            [DllImport(str_dll_file, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SetSensorI2cAckWait")]
            public static extern int SetSensorI2cAckWait(UInt32 uAckWait, int iDevID = DEFAULT_DEV_ID);
        //DTCCM_API int _DTCALL_ SetSensorI2cAckWait(UINT uAckWait, int iDevID = DEFAULT_DEV_ID);



        


    }

}
