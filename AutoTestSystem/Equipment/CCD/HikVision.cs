using AutoTestSystem.Base;
using MvCamCtrl.NET;
using MvCamCtrl.NET.CameraParams;
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
using System.Windows.Forms;
using static AutoTestSystem.BLL.Bd;


namespace AutoTestSystem.Equipment.CCD
{
    class HikVision : CCDBase
    {
        private string _strParamInfoPath;


        [Category("Params"), Description("Select CCD_Device"), TypeConverter(typeof(CCDDeviceList))]
        public string CCDDeviceName { get; set; }

        [Category("Parameter"), Description("Profile Path"), Editor(typeof(Manufacture.FileSelEditorRelPath), typeof(System.Drawing.Design.UITypeEditor))]
        public string Profile_path
        {
            get { return _strParamInfoPath; }
            set { _strParamInfoPath = value; }
        }


        CCamera m_MyCamera = null;
        static List<CCameraInfo> m_ltDeviceList = new List<CCameraInfo>();
        bool m_bGrabbing = false;
        bool IsDevice = true;
        Thread m_hReceiveThread = null;
        PixelFormat m_enBitmapPixelFormat = PixelFormat.DontCare;
        Bitmap m_pcBitmap = null;
        // ch:用于从驱动获取图像的缓存 | en:Buffer for getting image from driver
        private static Object BufForDriverLock = new Object();
        CImage m_pcImgForDriver;        // 图像信息
        CFrameSpecInfo m_pcImgSpecInfo; // 图像的水印信息
        CCameraInfo device = null;

        public override bool Capture(string strSavePath)
        {
            IsDevice = CameraDeviceConnect();
            if (!IsDevice)
                return false;

            CSaveImgToFileParam stSaveFileParam = new CSaveImgToFileParam();

            //string FullPath = strSavePath;
            string FullPath = "Image/LED_TEST/SN.bmp";
            string tmp = Path.GetDirectoryName(FullPath);
            DirectoryInfo di =null;
            if (!Directory.Exists(tmp))
            {
                if (tmp == "")
                    Logger.Warn("Image save in root DIR");
                else
                    di = Directory.CreateDirectory(tmp);
            }

            if (FullPath.EndsWith(".bmp"))
            {
                FullPath = FullPath.Replace(".bmp", "");
            }



            lock (BufForDriverLock)
            {
                if (m_pcImgForDriver.FrameLen == 0)
                {
                    Logger.Warn("Save Bmp Fail!");
                    return false;
                }
                stSaveFileParam.ImageType = MV_SAVE_IAMGE_TYPE.MV_IMAGE_BMP;
                stSaveFileParam.Image = m_pcImgForDriver;
                stSaveFileParam.MethodValue = 2;
                stSaveFileParam.ImagePath = FullPath + "Image_w" + stSaveFileParam.Image.Width.ToString() + "_h" + stSaveFileParam.Image.Height.ToString() + ".bmp";
                int nRet = m_MyCamera.SaveImageToFile(ref stSaveFileParam);
                if (CErrorDefine.MV_OK != nRet)
                {
                    Logger.Warn("Save Bmp Fail!");
                    return false;
                }
            }

            Logger.Debug("Save Succeed!");


            return true;
        }

        public override bool SaveImage(string strSavePath)
        {
            IsDevice = CameraDeviceConnect();
            if (!IsDevice)
                return false;

            CSaveImgToFileParam stSaveFileParam = new CSaveImgToFileParam();

            //string FullPath = strSavePath;
            string FullPath = "Image/LED_TEST/SN.bmp";
            string tmp = Path.GetDirectoryName(FullPath);
            DirectoryInfo di = null;
            if (!Directory.Exists(tmp))
            {
                if (tmp == "")
                    Logger.Warn("Image save in root DIR");
                else
                    di = Directory.CreateDirectory(tmp);
            }

            if (FullPath.EndsWith(".bmp"))
            {
                FullPath = FullPath.Replace(".bmp", "");
            }



            lock (BufForDriverLock)
            {
                if (m_pcImgForDriver.FrameLen == 0)
                {
                    Logger.Warn("Save Bmp Fail!");
                    return false;
                }
                stSaveFileParam.ImageType = MV_SAVE_IAMGE_TYPE.MV_IMAGE_BMP;
                stSaveFileParam.Image = m_pcImgForDriver;
                stSaveFileParam.MethodValue = 2;
                stSaveFileParam.ImagePath = FullPath + "Image_w" + stSaveFileParam.Image.Width.ToString() + "_h" + stSaveFileParam.Image.Height.ToString() + ".bmp";
                int nRet = m_MyCamera.SaveImageToFile(ref stSaveFileParam);
                if (CErrorDefine.MV_OK != nRet)
                {
                    Logger.Warn("Save Bmp Fail!");
                    return false;
                }
            }

            Logger.Debug("Save Succeed!");


            return true;
        }


        public override bool SaveImage(int Format_Mode,string strSavePath)
        {
            //Format_Mode 0:bmp 1:jpg 
            IsDevice = CameraDeviceConnect();
            if (!IsDevice)
                return false;

            CSaveImgToFileParam stSaveFileParam = new CSaveImgToFileParam();

            
            string FullPath = strSavePath;
            string tmp = Path.GetDirectoryName(FullPath);
            DirectoryInfo di = null;
            if (!Directory.Exists(tmp))
            {
                if (tmp == "")
                    Logger.Warn("Image save in root DIR");
                else
                    di = Directory.CreateDirectory(tmp);
            }

            if (Format_Mode == 0)
            {
                if (FullPath.EndsWith(".bmp"))
                {
                    FullPath = FullPath.Replace(".bmp", "");
                }
            }else if(Format_Mode == 1)
            {
                if (FullPath.EndsWith(".jpg"))
                {
                    FullPath = FullPath.Replace(".jpg", "");
                }
            }


            lock (BufForDriverLock)
            {
                if (m_pcImgForDriver.FrameLen == 0)
                {
                    Logger.Warn("Save Image Fail!");
                    return false;
                }
                if (Format_Mode == 0)
                {
                    stSaveFileParam.ImageType = MV_SAVE_IAMGE_TYPE.MV_IMAGE_BMP;
                    stSaveFileParam.Image = m_pcImgForDriver;
                    stSaveFileParam.MethodValue = 2;
                    stSaveFileParam.ImagePath = FullPath + ".bmp";
                }else if(Format_Mode == 1) 
                {

                    stSaveFileParam.ImageType = MV_SAVE_IAMGE_TYPE.MV_IMAGE_JPEG;
                    stSaveFileParam.Image = m_pcImgForDriver;
                    stSaveFileParam.Quality = 80;
                    stSaveFileParam.MethodValue = 2;
                    stSaveFileParam.ImagePath = FullPath + ".jpg";

                }
                int nRet = m_MyCamera.SaveImageToFile(ref stSaveFileParam);
                if (CErrorDefine.MV_OK != nRet)
                {
                    Logger.Warn("Save Image Fail!");
                    return false;
                }
            }

            Logger.Debug("Save Succeed!");


            return true;
        }

        public override void Dispose()
        {
            throw new NotImplementedException();
        }

        public override bool Status(ref string msg)
        {
            bool pass_fail = true;
            //int nRet = CSystem.EnumDevices(CSystem.MV_GIGE_DEVICE | CSystem.MV_USB_DEVICE, ref m_ltDeviceList);
            if(m_ltDeviceList.Count < 0 || null == m_MyCamera)
            {
                msg = "NO Camera Detect";
                return false;
            }
            else
            {
                for (int i = 0; i < m_ltDeviceList.Count; i++)
                {
                    CUSBCameraInfo usbInfo = (CUSBCameraInfo)m_ltDeviceList[i];
                    string tmp = "U3V: " + usbInfo.chManufacturerName + " " + usbInfo.chModelName + " (" + usbInfo.chSerialNumber + ")";
                    if (CCDDeviceName == tmp)
                        device = m_ltDeviceList[i];
                }
                IsDevice = CameraDeviceConnect();
                if (IsDevice)
                {
                    msg = $"{CCDDeviceName} Connected Sucessed";
                    pass_fail = true;
                }
                else
                {
                    msg = $"{CCDDeviceName} Connected Fail";
                    pass_fail = false;
                }

            }

            return pass_fail;
        }

        public bool CameraDeviceConnect()
        {
            bool IsReady = m_MyCamera.IsDeviceConnected();
            if (IsReady)
                return true;
            else
            {
                LogMessage("Camera Device is not Connected", MessageLevel.Warn);
                MessageBox.Show("Camera Device is not Connected", "Error !!!");
                return false;
            }
        }

        public override bool Init(string strParamInfo)
        {
            //if (strParamInfo == "")
            //    return false;
           try
            {
                //string tmp = string.Empty;
                int nRet = CSystem.EnumDevices(CSystem.MV_USB_DEVICE, ref m_ltDeviceList);
                //int nRet = CSystem.EnumDevices(CSystem.MV_GIGE_DEVICE | CSystem.MV_USB_DEVICE , ref m_ltDeviceList);
                for (int i = 0; i < m_ltDeviceList.Count; i++)
                {
                   CUSBCameraInfo usbInfo = (CUSBCameraInfo)m_ltDeviceList[i];
                      string tmp = "U3V: " + usbInfo.chManufacturerName + " " + usbInfo.chModelName + " (" + usbInfo.chSerialNumber + ")";
                     if (CCDDeviceName == tmp)
                         device = m_ltDeviceList[i];
                }

                if (null == m_MyCamera)
                {
                    m_MyCamera = new CCamera();
                    if (null == m_MyCamera)
                    {
                        LogMessage("Cant create Camera", MessageLevel.Warn);
                        return false;
                    }
                }


                nRet = m_MyCamera.CreateHandle(ref device);
                if (CErrorDefine.MV_OK != nRet)
                {
                    LogMessage("NO Camera Detect", MessageLevel.Warn);
                    MessageBox.Show("NO Camera Detect ", "Error !!!");
                    return false;
                }

                nRet = m_MyCamera.OpenDevice();
                if (CErrorDefine.MV_OK != nRet)
                {
                    m_MyCamera.DestroyHandle();
                    LogMessage("Camera OpenDevice Fail ", MessageLevel.Error);
                    MessageBox.Show("Camera OpenDevice Fail ", "Error !!!");
                    //ShowErrorMsg("Device open fail!", nRet);
                    return false;
                }


                m_MyCamera.SetEnumValue("AcquisitionMode", (uint)MV_CAM_ACQUISITION_MODE.MV_ACQ_MODE_CONTINUOUS);
                m_MyCamera.SetEnumValue("TriggerMode", (uint)MV_CAM_TRIGGER_MODE.MV_TRIGGER_MODE_OFF);
                m_MyCamera.SetEnumValue("ExposureAuto", 0);
                m_MyCamera.SetEnumValue("GainAuto", 0);


                /////////////////////////READ PARAM FILE//////////////////////
                if (!string.IsNullOrEmpty(Profile_path))
                {
                    if (File.Exists(Profile_path))
                    {
                        if (ReadParamFile(Profile_path))
                        {
                            LogMessage("Read Param File success!", MessageLevel.Info);

                        }
                        else
                            LogMessage("Read Param File Fail", MessageLevel.Warn);
                    }
                    else
                    {
                        LogMessage("Read Param File is not exists", MessageLevel.Warn);
                        return false;
                    }
                }

                /////////////////////////READ PARAM FILE//////////////////////


                /////////////// CCD START //////////////////////////////////
                if (Start())
                {
                    LogMessage("Streamimng success!", MessageLevel.Info);
                }
                else
                    MessageBox.Show("CCD Device Streaming abnormal", "Warning !!!");

                /////////////// CCD START //////////////////////////////////

            }
            catch(Exception ex)
            {
                LogMessage($"Init Fail.{ex.Message}", MessageLevel.Error);
                return false;
            }

            return true;
        }

        public override bool Start()
        {
            IsDevice = CameraDeviceConnect();
            if (!IsDevice)
                return false;
            // pre-operation
            int nRet = NecessaryOperBeforeGrab();
            if (CErrorDefine.MV_OK != nRet)
            {
                Logger.Warn("HikVision CCD pre-operation fail");
                return false;
            }

            // ch:标志位置位true | en:Set position bit true
            m_bGrabbing = true;

            m_hReceiveThread = new Thread(ReceiveThreadProcess);
            m_hReceiveThread.Start();

            // ch:开始采集 | en:Start Grabbing
            nRet = m_MyCamera.StartGrabbing();
            if (CErrorDefine.MV_OK != nRet)
            {
                m_bGrabbing = false;
                m_hReceiveThread.Join();
                Logger.Warn("Start Grabbing Fail!");
                return true;
            }


           

            return true;
        }

        public override bool UnInit()
        {

           try
            {
                // ch:取流标志位清零 | en:Reset flow flag bit
                if (m_bGrabbing == true)
                {
                    m_bGrabbing = false;
                    m_hReceiveThread.Join();
                }

                if (m_MyCamera == null)
                    return false;

                // ch:关闭设备 | en:Close Device
                m_MyCamera.CloseDevice();
                m_MyCamera.DestroyHandle();

            }
            catch (Exception ex)
            {
                LogMessage($"UnInit Fail.{ex.Message}", MessageLevel.Error);
                return false;
            }


            return true;
        }

        public override bool Set_Exposure(int value)
        {
            bool pass_fail_flag = true;
            IsDevice = CameraDeviceConnect();
            if (!IsDevice)
                return false;

            int nRet = m_MyCamera.SetFloatValue("ExposureTime", value);
            if (nRet == 0)
                pass_fail_flag = true;
            else
                pass_fail_flag = false;


            return pass_fail_flag;
        }

        public override bool ReadParamFile(string strSavePath)
        {
            int nRet = 0;
            if (strSavePath != string.Empty) 
            {
                nRet = m_MyCamera.FeatureLoad(strSavePath);
                if(CErrorDefine.MV_OK != nRet)
                {
                    Logger.Warn("Open Parameter File Name Fail !!!");
                    return false;
                }
               
            }else 
            {
                Logger.Warn("NO parameter file Name !!!");
                return false;

            }
            //CFloatValue sss = new CFloatValue();
            //m_MyCamera.GetFloatValue("ExposureTime", ref sss);
            //float aaa = sss.CurValue;
            

                return true;
        }


        private Int32 NecessaryOperBeforeGrab()
        {
            // ch:取图像宽 | en:Get Iamge Width
            CIntValue pcWidth = new CIntValue();
            int nRet = m_MyCamera.GetIntValue("Width", ref pcWidth);
            if (CErrorDefine.MV_OK != nRet)
            {
                Logger.Warn("Get Width Info Fail!");
                return nRet;
            }
            // ch:取图像高 | en:Get Iamge Height
            CIntValue pcHeight = new CIntValue();
            nRet = m_MyCamera.GetIntValue("Height", ref pcHeight);
            if (CErrorDefine.MV_OK != nRet)
            {
                Logger.Warn("Get Height Info Fail!");
                return nRet;
            }
            // ch:取像素格式 | en:Get Pixel Format
            CEnumValue pcPixelFormat = new CEnumValue();
            nRet = m_MyCamera.GetEnumValue("PixelFormat", ref pcPixelFormat);
            if (CErrorDefine.MV_OK != nRet)
            {
                Logger.Warn("Get Pixel Format Fail!");
                return nRet;
            }

            // ch:设置bitmap像素格式
           
            m_enBitmapPixelFormat = PixelFormat.Format24bppRgb;
         

            if (null != m_pcBitmap)
            {
                m_pcBitmap.Dispose();
                m_pcBitmap = null;
            }
            m_pcBitmap = new Bitmap((Int32)pcWidth.CurValue, (Int32)pcHeight.CurValue, m_enBitmapPixelFormat);

            // ch:Mono8格式，设置为标准调色板 | en:Set Standard Palette in Mono8 Format
            if (PixelFormat.Format8bppIndexed == m_enBitmapPixelFormat)
            {
                ColorPalette palette = m_pcBitmap.Palette;
                for (int i = 0; i < palette.Entries.Length; i++)
                {
                    palette.Entries[i] = Color.FromArgb(i, i, i);
                }
                m_pcBitmap.Palette = palette;
            }

            return CErrorDefine.MV_OK;
        }


        public void ReceiveThreadProcess()
        {
            CFrameout pcFrameInfo = new CFrameout();
            CDisplayFrameInfo pcDisplayInfo = new CDisplayFrameInfo();
            CPixelConvertParam pcConvertParam = new CPixelConvertParam();
            int nRet = CErrorDefine.MV_OK;

            while (m_bGrabbing)
            {
                nRet = m_MyCamera.GetImageBuffer(ref pcFrameInfo, 1000);
                if (nRet == CErrorDefine.MV_OK)
                {
                    lock (BufForDriverLock)
                    {
                        m_pcImgForDriver = pcFrameInfo.Image.Clone() as CImage;
                        m_pcImgSpecInfo = pcFrameInfo.FrameSpec;

                        pcConvertParam.InImage = pcFrameInfo.Image;
                        if (PixelFormat.Format8bppIndexed == m_pcBitmap.PixelFormat)
                        {
                            pcConvertParam.OutImage.PixelType = MvGvspPixelType.PixelType_Gvsp_Mono8;
                            m_MyCamera.ConvertPixelType(ref pcConvertParam);
                        }
                        else
                        {
                            pcConvertParam.OutImage.PixelType = MvGvspPixelType.PixelType_Gvsp_BGR8_Packed;
                            m_MyCamera.ConvertPixelType(ref pcConvertParam);
                        }

                        // ch:保存Bitmap数据 | en:Save Bitmap Data
                        BitmapData m_pcBitmapData = m_pcBitmap.LockBits(new Rectangle(0, 0, pcConvertParam.InImage.Width, pcConvertParam.InImage.Height), ImageLockMode.ReadWrite, m_pcBitmap.PixelFormat);
                        Marshal.Copy(pcConvertParam.OutImage.ImageData, 0, m_pcBitmapData.Scan0, (Int32)pcConvertParam.OutImage.ImageData.Length);
                        m_pcBitmap.UnlockBits(m_pcBitmapData);
                    }

                    //pcDisplayInfo.WindowHandle = pictureBox1.Handle;
                    pcDisplayInfo.Image = pcFrameInfo.Image;
                    m_MyCamera.DisplayOneFrame(ref pcDisplayInfo);

                    m_MyCamera.FreeImageBuffer(ref pcFrameInfo);
                }
               
            }
        }



       



        public class CCDDeviceList : TypeConverter  //下拉式選單
        {
            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {


                //int nRet = CSystem.EnumDevices(CSystem.MV_GIGE_DEVICE | CSystem.MV_USB_DEVICE , ref m_ltDeviceList);
                int nRet = CSystem.EnumDevices(CSystem.MV_USB_DEVICE, ref m_ltDeviceList);
                if (nRet == 1)
                {
                    return new StandardValuesCollection(new int[] { });
                }

                string[] CCD_Device_Names =new string[m_ltDeviceList.Count];
                //string tmp = string.Empty;

                for (int i = 0; i < m_ltDeviceList.Count; i++)
                {

                    CUSBCameraInfo usbInfo = (CUSBCameraInfo)m_ltDeviceList[i];
                    string tmp = "U3V: " + usbInfo.chManufacturerName + " " + usbInfo.chModelName + " (" + usbInfo.chSerialNumber + ")";
                    CCD_Device_Names[i] = tmp;               

                }

                
                if (CCD_Device_Names.Length > 0)
                {
                    return new StandardValuesCollection(CCD_Device_Names.ToArray());
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




    }
}
