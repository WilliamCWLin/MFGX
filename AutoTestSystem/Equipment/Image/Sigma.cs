using AutoTestSystem.Base;
using Newtonsoft.Json;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static AutoTestSystem.BLL.Bd;
using static System.ComponentModel.TypeConverter;
using static System.Windows.Forms.AxHost;
using static ZXing.QrCode.Internal.Version;

namespace AutoTestSystem.Equipment.Image
{

    internal class Sigma : Image_Base
    {


        [DllImport("ImgConverter.dll", EntryPoint = "FastRawToColor", CallingConvention = CallingConvention.StdCall)]
        public static extern void FastRawToColor(IntPtr src, int iW_pxl, int iH_pxl, int iAlignMode, int iBits, int iCh, IntPtr ppBMP);

        // C++ DLL 的 SFB_OpenStream 函式
        [DllImport("SigmaFrameBuffer.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int SFB_OpenStream(
            IntPtr hComm,
            string user_id,
            string user_pw,
            string url_path,
            bool plugin_stream);

        [DllImport("SigmaFrameBuffer.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int SFB_CloseStream(IntPtr hComm);

        [DllImport("SigmaFrameBuffer.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int SFB_getMFG3ErrCode(StringBuilder err_code, StringBuilder item_code);

        [DllImport("SigmaFrameBuffer.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern void SFB_BackupLogFile(string file_name);

        [DllImport("SigmaFrameBuffer.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int SFB_CaptureData(
            string ip,
            string port,
            int img_fmt,
            out IntPtr databuf,
            out uint data_size,
            string filepath,
            int count);


        [DllImport("SigmaFrameBuffer.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int SFB_SetICRState(IntPtr hComm, int state);


        [DllImport("SigmaFrameBuffer.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int SFB_SetExposure(string ip, string port, int exposure);

        IntPtr GrabBuffer = IntPtr.Zero;
        IntPtr m_TripleBuffer = IntPtr.Zero;
        IntPtr m_Raw8Buffer = IntPtr.Zero;
        IntPtr Raw_buffer = IntPtr.Zero;
        IntPtr BMP_buffer = IntPtr.Zero;
        IntPtr m_avgBuffer = IntPtr.Zero;
        uint data_size = 0;
        public Dictionary<string, string> _paramInfo = new Dictionary<string, string>();
        SerialPortHelper port = new SerialPortHelper();
        SerialPortHelper portA = new SerialPortHelper();
        SerialPortHelper portB = new SerialPortHelper();


        public enum Fun_enum
        {
            EXP,
            ICR,
        }


        [Category("Paramter"), Description("Choose the Bayer pattern"), TypeConverter(typeof(Bayer_Mode))]
        public string BayerPattern { get; set; } = "BGGR";
        [Category("Paramter"), Description("Define the User Name")]
        public string User_Name { get; set; } = "Administrator";
        [Category("Paramter"), Description("Define the User Password")]
        public string User_Pwd { get; set; } = "12345";
        [Category("Paramter"), Description("Define the URL Path")]
        public string URL_Path { get; set; } = "//192.168.2.123/730";
        [Category("Paramter"), Description("Define COM Port A")]
        public int COM_A { get; set; } = 3;
        [Category("Paramter"), Description("Define COM Port B")]
        public int COM_B { get; set; } = 5;
        [Category("Paramter"), Description("Choose the Single or Mutiple"), TypeConverter(typeof(Muti_COM))]
        public string Single_Mutiple_COM { get; set; } = "ALL";


        public Sigma()
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


            int Size = Image_Width * Image_Height * 3;
            int result;
            result = SFB_CaptureData(IQServerConfig.IPAddress, IQServerConfig.Port, 0, out m_TripleBuffer, out data_size, null, 1);
            if (result < 0)
            {
                StringBuilder errCode = new StringBuilder(256);
                StringBuilder itemCode = new StringBuilder(256);
                SFB_getMFG3ErrCode(errCode, itemCode);
                return false;
            }
            /// FOR RAW BUFFER to SaveImage
            Raw_buffer = m_TripleBuffer;


            ConvertRaw16ToRaw8(m_TripleBuffer, Image_Width, Image_Height, m_Raw8Buffer);
            FastRawToColor(m_Raw8Buffer, Image_Width, Image_Height, mod_bayer, 8, 1, GrabBuffer);
            string str_Address = $"0x{GrabBuffer.ToString("X")}";
            PtrBuffer = str_Address;

            GrabBuffer = ConvertRGBToBGR(GrabBuffer, Image_Height, Image_Width);

            unsafe
            {
                int bufferSize = Size;
                Buffer.MemoryCopy((void*)GrabBuffer, (void*)BMP_buffer, bufferSize, bufferSize);
            }


            return true;


        }

        public override bool SetParam(string Function, string jsonParamInfo)
        {
            switch (Function)
            {
                case "EXP":
                    try
                    {
                        _paramInfo = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonParamInfo);
                        List<string> valuesList = new List<string>();
                        foreach (var kvp in _paramInfo)
                        {
                            valuesList.Add(kvp.Value);
                        }


                        try
                        {
                            if (valuesList[0] != "")
                            {
                                if (Set_Exposure(Convert2UInt16(valuesList[0])))
                                {
                                    LogMessage("Write Exposure Success", MessageLevel.Debug);
                                    return true;
                                }
                                else
                                {
                                    LogMessage("Write Exposure Fail", MessageLevel.Error);
                                    return false;
                                }
                            }
                            else
                            {
                                LogMessage("Set_Exposure value is null", MessageLevel.Error);
                                return false;
                            }
                        }
                        catch (Exception ex)
                        {
                            LogMessage($"Write Exposure Exception :{ex.Message}", MessageLevel.Error);
                            return false;
                        }


                    }
                    catch (JsonException ex)
                    {
                        LogMessage($"Input string is not Json format :{ex.Message}", MessageLevel.Error);
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
                                LogMessage($"Write GPIO Exception :{ex.Message}", MessageLevel.Error);
                                return false;
                            }
                        }
                        else
                        {
                            LogMessage("GPIO High_Low input format is wrong", MessageLevel.Error);
                            return false;
                        }

                    }
                    catch (JsonException ex)
                    {
                        LogMessage($"Input string is not Json format :{ex.Message}", MessageLevel.Error);
                        return false;
                    }

                default:
                    return true;
            }
        }

        public override bool SaveImage(int Format_Mode, string strSavePath)
        {
            int bufferSize = 0;
            byte[] managedBuffer;
            if (Format_Mode == 0)
            {
                bufferSize = Image_Width * Image_Height * 2;
                managedBuffer = new byte[bufferSize];
                Marshal.Copy(Raw_buffer, managedBuffer, 0, bufferSize);
                File.WriteAllBytes(strSavePath, managedBuffer);
            } //Raw buffer to save image
            else if (Format_Mode == 1)
            {
                using (Bitmap checkbitmap = CreateBitmapFromIntPtr(BMP_buffer, Image_Width, Image_Height, PixelFormat.Format24bppRgb))
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
            Image_input_str = strParamInfo; 
            Image_Width = 2688;
            Image_Height = 1520;
            m_TripleBuffer = Marshal.AllocHGlobal(Image_Width * Image_Height * 3);
            GrabBuffer = Marshal.AllocHGlobal(Image_Width * Image_Height * 3);
            BMP_buffer = Marshal.AllocHGlobal(Image_Width * Image_Height * 3);
            m_avgBuffer = Marshal.AllocHGlobal(Image_Width * Image_Height * 3);
            int pixel_count = Image_Width * Image_Height;
            m_Raw8Buffer = Marshal.AllocHGlobal(pixel_count);



            if (Single_Mutiple_COM == "ALL")
            {

                if (portA.Open("COM" + COM_A.ToString()) && portB.Open("COM" + COM_B.ToString()))
                {
                    if (portA.Configure(baudRate: 115200) && portB.Configure(baudRate: 115200))
                    {
                        Console.WriteLine("COM Port ALL Setting OK");
                        return true;
                    }
                    else
                        return false;
                }
                else
                {

                    return false;
                }

                //if (portA.Open("COM" + COM_A.ToString()))
                //{
                //    if (portA.Configure(baudRate: 115200))
                //    {
                //        Console.WriteLine("COM Port ALL Setting OK");
                //        return true;
                //    }
                //    else
                //        return false;
                //}
                //else
                //{

                //    return false;
                //}


            }
            else if (Single_Mutiple_COM == "COMA")
            {
                if (portA.Open("COM" + COM_A.ToString()))
                {
                    if (portA.Configure(baudRate: 115200))
                    {
                        Console.WriteLine("COM PortA Setting OK");
                        return true;
                    }
                    else
                        return false;
                }
                else
                {

                    return false;
                }
            }
            else if (Single_Mutiple_COM == "COMB")
            {
                if (portB.Open("COM" + COM_B.ToString()))
                {
                    if (portB.Configure(baudRate: 115200))
                    {
                        Console.WriteLine("COM PortB Setting OK");
                        return true;
                    }
                    else
                        return false;
                }
                else
                {

                    return false;
                }
            }
            else { 
                
                return false; 
            
            }


        }


        public override bool Start()
        {
            int result = 0;
            //result = SFB_OpenStream(port.GetHandle(), User_Name, User_Pwd, URL_Path, true);

            if (Single_Mutiple_COM == "COMA")
            {
                result = SFB_OpenStream(portA.GetHandle(), User_Name, User_Pwd, URL_Path, true);

            }
            else if (Single_Mutiple_COM == "COMB")
            {
                result = SFB_OpenStream(portB.GetHandle(), User_Name, User_Pwd, URL_Path, true);
            }
            else
            {
                //if (Image_input_str == "A")
                //{
                //    result = SFB_OpenStream(portA.GetHandle(), User_Name, User_Pwd, URL_Path, true);
                //}
                //else if (Image_input_str == "B")
                //{
                //    result = SFB_OpenStream(portB.GetHandle(), User_Name, User_Pwd, URL_Path, true);
                //}

                // 從 Image_input_str 中擷取 & 符號前的部分
                string cameraIdentifier = !string.IsNullOrEmpty(Image_input_str) && Image_input_str.Contains("&")
                    ? Image_input_str.Split('&')[0]
                    : Image_input_str;

                if (cameraIdentifier == "A")
                {
                    result = SFB_OpenStream(portA.GetHandle(), User_Name, User_Pwd, URL_Path, true);
                }
                else if (cameraIdentifier == "B")
                {
                    result = SFB_OpenStream(portB.GetHandle(), User_Name, User_Pwd, URL_Path, true);
                }
            }


            Console.WriteLine($"SFB_OpenStream result: {result}");
            if (result < 0)
            {
                StringBuilder errCode = new StringBuilder(256);
                StringBuilder itemCode = new StringBuilder(256);
                SFB_getMFG3ErrCode(errCode, itemCode);
                return false;
            }


            return true;
        }

        public override bool UnInit()
        {
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


            if (Single_Mutiple_COM == "COMA")
            {
                SFB_CloseStream(portA.GetHandle());
                SFB_BackupLogFile(Image_input_str);
                portA.Close();

            }
            else if (Single_Mutiple_COM == "COMB")
            {
                SFB_CloseStream(portB.GetHandle());
                SFB_BackupLogFile(Image_input_str);
                portB.Close();
            }
            else
            {
                //if (Image_input_str == "A")
                //{
                //    SFB_CloseStream(portA.GetHandle());
                //    SFB_BackupLogFile(Image_input_str);
                //    portA.Close();
                //}
                //else if (Image_input_str == "B")
                //{
                //    SFB_CloseStream(portB.GetHandle());
                //    SFB_BackupLogFile(Image_input_str);
                //    portB.Close();
                //}
                // 從 Image_input_str 中擷取 & 符號前的部分
                string cameraIdentifier = !string.IsNullOrEmpty(Image_input_str) && Image_input_str.Contains("&")
                    ? Image_input_str.Split('&')[0]
                    : Image_input_str;

                if (cameraIdentifier == "A")
                {
                    SFB_CloseStream(portA.GetHandle());
                    SFB_BackupLogFile(Image_input_str);
                    portA.Close();
                }
                else if (cameraIdentifier == "B")
                {
                    SFB_CloseStream(portB.GetHandle());
                    SFB_BackupLogFile(Image_input_str);
                    portB.Close();
                }


                else
                {
                    portA.Close();
                    portB.Close();
                }
            }

            //SFB_CloseStream(port.GetHandle());
            //SFB_BackupLogFile(Image_input_str);
            //port.Close();




            return true;
        }


        public override bool GPIOWrite(int pin, bool bEnable)
        {
            int result = 0;
            int ICR_STATE = 0;
            if (!bEnable)
                ICR_STATE = 1;
            else
                ICR_STATE = 0;
            //result = SFB_SetICRState(port.GetHandle(), ICR_STATE);

            if (Single_Mutiple_COM == "COMA")
            {
                result = SFB_SetICRState(portA.GetHandle(), ICR_STATE);

            }
            else if (Single_Mutiple_COM == "COMB")
            {
                result = SFB_SetICRState(portB.GetHandle(), ICR_STATE);
            }
            else
            {
                //if (Image_input_str == "A")
                //{
                //    result = SFB_SetICRState(portA.GetHandle(), ICR_STATE);
                //}
                //else if (Image_input_str == "B")
                //{
                //    result = SFB_SetICRState(portB.GetHandle(), ICR_STATE);
                //}

                // 從 Image_input_str 中擷取 & 符號前的部分
                string cameraIdentifier = !string.IsNullOrEmpty(Image_input_str) && Image_input_str.Contains("&")
                    ? Image_input_str.Split('&')[0]
                    : Image_input_str;

                if (cameraIdentifier == "A")
                {
                    result = SFB_SetICRState(portA.GetHandle(), ICR_STATE);
                }
                else if (cameraIdentifier == "B")
                {
                    result = SFB_SetICRState(portB.GetHandle(), ICR_STATE);
                }
            }


            if (result < 0)
            {
                StringBuilder errCode = new StringBuilder(256);
                StringBuilder itemCode = new StringBuilder(256);
                SFB_getMFG3ErrCode(errCode, itemCode);
                return false;
            }
            return true;
        }

        public override bool Set_Exposure(int value)
        {
            int result = 0;
            result = SFB_SetExposure(IQServerConfig.IPAddress, IQServerConfig.Port, value);
            if (result < 0)
            {
                StringBuilder errCode = new StringBuilder(256);
                StringBuilder itemCode = new StringBuilder(256);
                SFB_getMFG3ErrCode(errCode, itemCode);
                return false;
            }

            return true;
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

        public class Muti_COM : TypeConverter  //下拉式選單
        {
            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                List<string> mutiple_com = new List<string>();

                mutiple_com.Add("COMA"); //1
                mutiple_com.Add("COMB"); //2
                mutiple_com.Add("ALL"); //3
                

                return new StandardValuesCollection(mutiple_com);
            }

            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                return true;
            }
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

    }


    public static class NativeMethods
    {
        public const uint GENERIC_READ = 0x80000000;
        public const uint GENERIC_WRITE = 0x40000000;
        public const uint FILE_SHARE_READ = 0x00000001;
        public const uint FILE_SHARE_WRITE = 0x00000002;
        public const uint OPEN_EXISTING = 3;
        public const uint FILE_ATTRIBUTE_NORMAL = 0x80;

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr CreateFile(
            string lpFileName,
            uint dwDesiredAccess,
            uint dwShareMode,
            IntPtr lpSecurityAttributes,
            uint dwCreationDisposition,
            uint dwFlagsAndAttributes,
            IntPtr hTemplateFile);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool GetCommState(IntPtr hFile, ref DCB lpDCB);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool SetCommState(IntPtr hFile, [In] ref DCB lpDCB);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool CloseHandle(IntPtr hObject);
    }


    [StructLayout(LayoutKind.Sequential)]
    public struct DCB
    {
        public uint DCBlength;
        public uint BaudRate;
        public uint Flags;
        public ushort wReserved;
        public ushort XonLim;
        public ushort XoffLim;
        public byte ByteSize;
        public byte Parity;
        public byte StopBits;
        public byte XonChar;
        public byte XoffChar;
        public byte ErrorChar;
        public byte EofChar;
        public byte EvtChar;
        public ushort wReserved1;
    }

    [Flags]
    public enum DCBFlags : uint
    {
        fBinary = 0x00000001,
        fParity = 0x00000002,
        fOutxCtsFlow = 0x00000004,
        fOutxDsrFlow = 0x00000008,
        fDtrControl = 0x00000030,
        fDsrSensitivity = 0x00000040,
        fTXContinueOnXoff = 0x00000080,
        fOutX = 0x00000100,
        fInX = 0x00000200,
        fErrorChar = 0x00000400,
        fNull = 0x00000800,
        fRtsControl = 0x00003000,
        fAbortOnError = 0x00004000
    }


    public class SerialPortHelper
    {
        private IntPtr _handle = IntPtr.Zero;
        public bool IsOpen => _handle != IntPtr.Zero && _handle.ToInt64() != -1;

        public bool Open(string portName)
        {

            if (IsOpen)
            {
                Console.WriteLine("串列埠已經開啟，無需重複開啟。");
                return true;
            }

            string formattedPortName = FormatPortName(portName);


            _handle = NativeMethods.CreateFile(
                formattedPortName,
                NativeMethods.GENERIC_READ | NativeMethods.GENERIC_WRITE,
                NativeMethods.FILE_SHARE_READ | NativeMethods.FILE_SHARE_WRITE,
                IntPtr.Zero,
                NativeMethods.OPEN_EXISTING,
                NativeMethods.FILE_ATTRIBUTE_NORMAL,
                IntPtr.Zero);

            if (!IsOpen)
            {
                int errorCode = Marshal.GetLastWin32Error();
                Console.WriteLine($"開啟串列埠失敗，錯誤碼: {errorCode}");
                return false;
            }

            return true;
        }

        public bool Configure(uint baudRate = 9600, byte byteSize = 8, byte parity = 0, byte stopBits = 0)
        {
            if (!IsOpen) return false;

            DCB dcb = new DCB
            {
                DCBlength = (uint)Marshal.SizeOf(typeof(DCB)),
                BaudRate = baudRate,
                ByteSize = byteSize,
                Parity = parity,
                StopBits = stopBits,
                Flags = (uint)(DCBFlags.fBinary | DCBFlags.fTXContinueOnXoff)
            };

            if (!NativeMethods.SetCommState(_handle, ref dcb))
            {
                int errorCode = Marshal.GetLastWin32Error();
                Console.WriteLine($"設定串列埠失敗，錯誤碼: {errorCode}");
                return false;
            }

            return true;
        }

        public void Close()
        {
            if (IsOpen)
            {
                NativeMethods.CloseHandle(_handle);
                _handle = IntPtr.Zero;
            }
        }


        public IntPtr GetHandle()
        {
            return _handle;
        }


        private string FormatPortName(string portName)
        {
            if (!portName.StartsWith(@"\\.\") && portName.StartsWith("COM"))
            {
                string numberPart = portName.Substring(3);
                if (int.TryParse(numberPart, out int portNumber) && portNumber >= 10)
                {
                    return @"\\.\" + portName;
                }
            }
            return portName;
        }

    }


    public static class IQServerConfig
    {
        public const string IPAddress = "192.168.2.1";
        public const string Port = "9876";
    }


}
