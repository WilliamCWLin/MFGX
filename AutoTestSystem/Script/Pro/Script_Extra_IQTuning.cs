
using AutoTestSystem.Base;
using AutoTestSystem.DUT;
using AutoTestSystem.Equipment.ControlDevice;
using AutoTestSystem.Model;
using Manufacture;
using NAudio.Wave;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static AutoTestSystem.BLL.Bd;
using static AutoTestSystem.Script.Script_ControlDevice_Base;
using static AutoTestSystem.Script.Script_DUT_Camera_Control;
using static AutoTestSystem.Script.Script_Extra_SingleEntry;
using static AutoTestSystem.Script.ScriptDUTBase;
using VideoLanClient;
using AutoTestSystem.Equipment.Teach;
using static AutoTestSystem.Equipment.Teach.Teach_IQ_Tuning;
using PdfSharp.Drawing;

namespace AutoTestSystem.Script
{
    internal class Script_Extra_IQTuning : Script_Extra_Base
    {
        string strOutData = string.Empty;
        [JsonIgnore]
        [Browsable(false)]
        Dictionary<string, string> SFRData = new Dictionary<string, string>();

        [JsonIgnore]
        [Browsable(false)]
        byte[] Imagebuffer = null;

        [JsonIgnore]
        [Browsable(false)]
        TcpIpClient MotionCtrlDevice = null;//先寫死
        [JsonIgnore]
        [Browsable(false)]
        DUT_CGI_VLC DutCtrlDevice = null;//先寫死
        [JsonIgnore]
        [Browsable(false)]
        Teach_IQ_Tuning TeachDev = null;

        [Category("Devices"), Description("自訂顯示名稱"), TypeConverter(typeof(CTRL_DevList))]
        public string MotionDevice { get; set; }

        [Category("Devices"), Description("自訂顯示名稱"), TypeConverter(typeof(DUT_DevList))]
        public string VLCDevice { get; set; }

        [Category("Devices"), Description("自訂顯示名稱"), TypeConverter(typeof(Teach_DevList))]
        public string TeachParam { get; set; }

        [Category("TuningType"), Description("Camera Control Action Define")]
        public VLC_ACTION ActionMode { get; set; } = VLC_ACTION.IQTuning;
        [Category("Reverse"), Description("X reverse No = 0  Yes = 1")]
        public int X_rev { get; set; } = 0;
        [Category("Reverse"), Description("Y reverse No = 0  Yes = 1")]
        public int Y_rev { get; set; } = 0;

        [Category("IQ"), Description("ex:./IQ/SE_IVS.dll")]
        public string DLL_Path { get; set; }

        [Category("IQ"), Description("SE PIN Content.Use %address%"), Editor(typeof(PINEditor), typeof(UITypeEditor))]
        public string PIN { get; set; } = "";

        [Category("IQ_Limit"), Description("OC_X_Limit.")]
        public int OC_X_Limit { get; set; } = 150;

        [Category("IQ_Limit"), Description("OC_Y_Limit.")]
        public int OC_Y_Limit { get; set; } = 150;

        [Category("IQ_Limit"), Description("OC_X_Limit.")]
        public double OC_R_Limit { get; set; } = 5;

        [Category("IQ_Tuning_Spec"), Description("OC_R_SPEC.")]
        public double OC_R_Spec { get; set; } = 1;

        [Category("IQ_Tuning_Spec"), Description("OC_X_SPEC.")]
        public int OC_X_Spec { get; set; } = 10;

        [Category("IQ_Tuning_Spec"), Description("OC_Y_SPEC.")]
        public int OC_Y_Spec { get; set; } = 10;



        public string input;
        public string output;
        double X_ratio;
        double Y_ratio;
        string SE_PIN;
        double IQ_X_count;
        double IQ_Y_count;
        double IQ_U_count;
        double IQ_tcp_angle;
        double IQ_tcp_degreeX;
        double IQ_tcp_degreeY;
        double TL_atan_avg;
        double IQ_rotate_angle;
        string inputX;
        string inputY;
        string inputU;

        //[Browsable(false)]
        public enum VLC_ACTION
        {
            IQTuning
        }
        public override void Dispose()
        {
            //throw new NotImplementedException();
        }
        public override bool PreProcess()
        {
            if (!string.IsNullOrEmpty(TeachParam))
            {
                TeachDev = GlobalNew.Devices[TeachParam] as Teach_IQ_Tuning;
                IQParameters _parameters = TeachDev.GetParametersFromJson<IQParameters>();
                if (_parameters == null)
                {
                    return false;
                }
                X_ratio = _parameters.X_ratio;
                Y_ratio = _parameters.Y_ratio;
                SE_PIN = _parameters.SE_PIN;
            }
            else
            {
                return false;
            }

            // 如果是相對路徑，轉換為絕對路徑
            string _dllPath = Path.IsPathRooted(DLL_Path) ? DLL_Path : Path.GetFullPath(DLL_Path);
            if (!File.Exists(_dllPath))
            {
                LogMessage($"DLL_Path:{_dllPath} is not exist", MessageLevel.Fatal);
                return false;
            }

            if (!string.IsNullOrEmpty(MotionDevice))
            {
                MotionCtrlDevice = GlobalNew.Devices[MotionDevice] as TcpIpClient;
                if (MotionCtrlDevice == null)
                {
                    LogMessage($"GlobalNew.Devices[{MotionDevice}] is not TcpIpClient type");
                    return false;
                }

            }
            else
            {
                LogMessage($"GlobalNew.Devices[{MotionDevice}] is null");
                return false;
            }


            if (!string.IsNullOrEmpty(VLCDevice))
            {
                DutCtrlDevice = GlobalNew.Devices[VLCDevice] as DUT_CGI_VLC;
                if (DutCtrlDevice == null)
                {
                    LogMessage($"GlobalNew.Devices[{VLCDevice}] is not VLCDevice");
                    return false;
                }

                if (Imagebuffer == null)
                {
                    Imagebuffer = new byte[DutCtrlDevice.Width * DutCtrlDevice.Height * 3];
                }
            }
            else
            {
                LogMessage($"GlobalNew.Devices[{VLCDevice}] is null");
                return false;
            }
            IQ_tcp_angle = 0;
            IQ_tcp_degreeX = 0;
            IQ_tcp_degreeY = 0;
            IQ_X_count=0;
            IQ_Y_count=0;
            IQ_U_count=0;
            return true;
        }
        public override bool Process(ref string strOutData)
        {

            try
            {
                using (Bitmap checkbitmap = DutCtrlDevice.CreateBitmapFromBuffer(Imagebuffer))
                {
                    // 清空 input 陣列
                    Array.Clear(Imagebuffer, 0, Imagebuffer.Length);

                    // 在 UI 線程上更新 PictureBox 圖像
                    DutCtrlDevice.DutDashboard.ImagePicturebox.Invoke((Action)(() =>
                    {
                        DutCtrlDevice.DutDashboard.ImagePicturebox.Image?.Dispose(); // 清理先前的圖片
                        DutCtrlDevice.DutDashboard.ImagePicturebox.Image = (Bitmap)checkbitmap.Clone();
                        DutCtrlDevice.DutDashboard.ImagePicturebox.Refresh();
                    }));
                }
                DutCtrlDevice.SwitchTabControlIndex(1);
                //string _dllPath = Path.IsPathRooted(DLL_Path) ? DLL_Path : Path.GetFullPath(DLL_Path);
                string _dllPath = ".\\IQ\\SE_IVS.dll";
                bool b_rotate = false;
                //==================

                //string imagePath = @"D:\10218_2mFocus_SFR_s608_0db_day_xy3_raw_2592x1944_5184.raw.bmp";
                //int width = 2592, height = 1944;
                //double format_mult = 3;
                //int raw_size = (int)(width * height * format_mult);
                //byte[] input = new byte[raw_size];

                // 讀取 BMP 文件到 byte[] 中
                //using (FileStream fs = new FileStream(imagePath, FileMode.Open, FileAccess.Read))
                //{
                //    fs.Seek(54, SeekOrigin.Begin); 
                //    fs.Read(input, 0, raw_size);
                //}

                // 獲取 byte[] 的指針
                //IntPtr start_ptr = Marshal.UnsafeAddrOfPinnedArrayElement(input, 0);

                // 創建一個新 byte[] 來存儲從 IntPtr 複製的數據
                //byte[] output = new byte[raw_size];
                //Marshal.Copy(start_ptr, output, 0, raw_size);
                //DutCtrlDevice.VideoInit("rtsp://10.0.0.2/stream1");

                //DutCtrlDevice.Play();
                //=========================
                for (int i = 0; i < 10; i++)
                //while (true)
                {
                    // 從 VideoLANClient 中獲取 RGB Buffer
                    //MotionCtrlDevice.SEND("TP");
                    //MotionCtrlDevice.READTimeout(ref output);
                    
                    //byte[] rgbBuffer = DutCtrlDevice.CaptureImage();
                    bool ret = DutCtrlDevice.CaptureImage(Imagebuffer);
                    if (Imagebuffer != null && ret)
                    {       
                        //*********************Calculate SFR && Draw ROI************************
                        SFRData.Clear();

                        string tmpSFR = IQ_SingleEntry.SFRCheck(_dllPath, SE_PIN, Imagebuffer, DutCtrlDevice.Width, DutCtrlDevice.Height, SFRData);
                        using (Bitmap checkbitmap = DutCtrlDevice.CreateBitmapFromBuffer(Imagebuffer))
                        {
                            if (!DrawRoisOnBitmap(checkbitmap, $"Rotate Angle:{IQ_rotate_angle}"))
                            {
                                LogMessage("Failed to draw ROIs on bitmap.");
                            }
                            // 清空 input 陣列
                            Array.Clear(Imagebuffer, 0, Imagebuffer.Length);

                            // 在 UI 線程上更新 PictureBox 圖像
                            DutCtrlDevice.DutDashboard.ImagePicturebox.Invoke((Action)(() =>
                            {
                                DutCtrlDevice.DutDashboard.ImagePicturebox.Image?.Dispose(); // 清理先前的圖片
                                DutCtrlDevice.DutDashboard.ImagePicturebox.Image = (Bitmap)checkbitmap.Clone();
                            }));
                        }


                        if (SFRData.ContainsKey("ShiftAndRotate_OC_Shift_X") &&
                            SFRData.ContainsKey("ShiftAndRotate_OC_Shift_Y") &&
                            SFRData.ContainsKey("ShiftAndRotate_Rotate_Shift_Y"))
                        {
                            if (SFRData == null || !SFRData.ContainsKey("ROI_SFR_SFR_Roi_Rule"))
                            {
                                // 字典不存在或不包含 ROI_SFR_SFR_Roi_Rule 的鍵時，輸出錯誤信息並返回
                                //MessageBox.Show("SFRData dictionary is null or does not contain the key 'ROI_SFR_SFR_Roi_Rule'.");
                                LogMessage("Can't find ROI Key.Check Dll path or Params", MessageLevel.Error);
                                break;
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
                                    LogMessage("Can't find ROI,Manual Fine Tune", MessageLevel.Error);
                                    continue;
                                }

                            }
                            else
                            {
                                LogMessage("Can't find ROI Key.Check Dll path or Params", MessageLevel.Error);
                                break;
                            }
                            //*********************Check Spec************************
                            int shiftX = int.Parse(SFRData["ShiftAndRotate_OC_Shift_X"]);
                            int shiftY = int.Parse(SFRData["ShiftAndRotate_OC_Shift_Y"]);
                            int rotateShiftY = int.Parse(SFRData["ShiftAndRotate_Rotate_Shift_Y"]);
                            SFRData_Value();
                            LogMessage($"OCX:{shiftX},OCY:{shiftY},R_Y:{IQ_rotate_angle}");
                            if ((Math.Abs(shiftX) <= OC_X_Spec) && (Math.Abs(shiftY) <= OC_Y_Spec) && (Math.Abs(IQ_rotate_angle)) <= OC_R_Spec)
                            {
                                return true;
                            }



                            //********************Motor Move*************************
                            

                            IQ_tcp_degreeX = shiftX * X_ratio * 100;
                            IQ_tcp_degreeY = shiftY * Y_ratio * 100;

                           
                            //U軸

                            if (IQ_rotate_angle >= OC_R_Spec)
                            {

                                if (TL_atan_avg > 0)
                                {
                                    inputU = "U+" + IQ_tcp_angle.ToString();
                                }
                                else if (TL_atan_avg < 0)
                                {
                                    inputU = "U-" + IQ_tcp_angle.ToString();
                                }
                                //bool sucess = View_Send(input, ref output);
                                //if (sucess != true)
                                //{
                                //    LogMessage("Out of U range, please click Reset and check the DUT", MessageLevel.Error);
                                //    return false;
                                //}
                            }
                            else
                            {
                                inputU = "U+0";
                            }
                            if (Math.Abs(shiftX) >= OC_X_Spec)
                            {
                                if (X_rev == 0)
                                {
                                    if (IQ_tcp_degreeX > 0 && IQ_tcp_degreeX < 800)
                                    {
                                        inputX = "X-" + IQ_tcp_degreeX.ToString();

                                    }
                                    else if (IQ_tcp_degreeX < 0 && IQ_tcp_degreeX > -800)
                                    {
                                        IQ_tcp_degreeX = Math.Abs(IQ_tcp_degreeX);
                                        inputX = "X+" + IQ_tcp_degreeX.ToString();

                                    }
                                    else if (IQ_tcp_degreeX > 800 || IQ_tcp_degreeX < -800)
                                    {
                                        LogMessage("將超出X軸運動極限", MessageLevel.Error);
                                        return false;
                                    }
                                }
                                else
                                {
                                    if (IQ_tcp_degreeX > 0 && IQ_tcp_degreeX < 800)
                                    {
                                        inputX = "X+" + IQ_tcp_degreeX.ToString();

                                    }
                                    else if (IQ_tcp_degreeX < 0 && IQ_tcp_degreeX > -800)
                                    {
                                        IQ_tcp_degreeX = Math.Abs(IQ_tcp_degreeX);
                                        inputX = "X-" + IQ_tcp_degreeX.ToString();

                                    }
                                    else if (IQ_tcp_degreeX > 800 || IQ_tcp_degreeX < -800)
                                    {
                                        LogMessage("將超出X軸運動極限", MessageLevel.Error);
                                        return false;
                                    }
                                }
                                

                                //bool sucess = View_Send(input, ref output);
                                //if (sucess != true)
                                //{
                                //    LogMessage("Out of X range, please click Reset and check the DUT", MessageLevel.Error);
                                //    return false;
                                //}
                            }
                            else
                            {
                                inputX = "X+0";
                            }
                            if (Math.Abs(shiftY) >= OC_Y_Spec)
                            {
                                if(Y_rev == 0)
                                {
                                    if (IQ_tcp_degreeY > 0 && IQ_tcp_degreeY < 700)
                                    {
                                        inputY = "Y-" + IQ_tcp_degreeY.ToString();
                                    }
                                    else if (IQ_tcp_degreeY < 0 && IQ_tcp_degreeY > -700)
                                    {
                                        IQ_tcp_degreeY = Math.Abs(IQ_tcp_degreeY);
                                        inputY = "Y+" + IQ_tcp_degreeY.ToString();
                                    }
                                    else if (IQ_tcp_degreeY > 700 || IQ_tcp_degreeY < -700)
                                    {
                                        LogMessage("將超出Y軸運動極限", MessageLevel.Error);
                                        return false;
                                    }
                                }
                                else
                                {
                                    if (IQ_tcp_degreeY > 0 && IQ_tcp_degreeY < 700)
                                    {
                                        inputY = "Y+" + IQ_tcp_degreeY.ToString();
                                    }
                                    else if (IQ_tcp_degreeY < 0 && IQ_tcp_degreeY > -700)
                                    {
                                        IQ_tcp_degreeY = Math.Abs(IQ_tcp_degreeY);
                                        inputY = "Y-" + IQ_tcp_degreeY.ToString();
                                    }
                                    else if (IQ_tcp_degreeY > 700 || IQ_tcp_degreeY < -700)
                                    {
                                        LogMessage("將超出Y軸運動極限", MessageLevel.Error);
                                        return false;
                                    }
                                }
                                

                                //bool sucess = View_Send(input, ref output);
                                //if (sucess != true)
                                //{
                                //    LogMessage("Out of Y range, please click Reset and check the DUT");
                                //    return false;
                                //}
                            }
                            else 
                            {
                                inputY = "Y+0";
                            }
                            //Thread.Sleep(1000);
                            if (!View_Send(inputX, inputY, inputU))
                            {
                                LogMessage("Out Moving Fail", MessageLevel.Error);
                                return false;
                            }
                            LogMessage($"X degree:{IQ_X_count},Y degree:{IQ_Y_count},U dregree:{IQ_U_count}");
                        }
                        System.Threading.Thread.Sleep(30); // 可根據需求調整延遲時間
                    }
                    // 模擬每次操作的延遲                    
                    else
                    {
                        DutCtrlDevice.DutDashboard.ImagePicturebox.Invoke((Action)(() =>
                        {
                            LogMessage("無法獲取 RGB 緩衝區！", MessageLevel.Warn);
                        }));
                    }
                    Thread.Sleep(500);
                }
                DutCtrlDevice.SwitchTabControlIndex(0);
                
            } 
            catch(Exception ex)
            { LogMessage(ex.ToString()); }

            return false;
        }
        public override bool PostProcess()
        {
            return true;

        }

        // 獨立的繪製 ROI 的函式
        private bool DrawRoisOnBitmap(Bitmap bitmap,string extramsg)
        {
            string[] roiRuleOrder;

            // 檢查字典是否存在且包含數據
            if (SFRData == null || !SFRData.ContainsKey("ROI_SFR_SFR_Roi_Rule"))
            {
                // 字典不存在或不包含 ROI_SFR_SFR_Roi_Rule 的鍵時，輸出錯誤信息並返回
                LogMessage("SFRData dictionary is null or does not contain the key 'ROI_SFR_SFR_Roi_Rule'.");
                return false;
            }

            // 解析 ROI_Rule 順序
            roiRuleOrder = SFRData["ROI_SFR_SFR_Roi_Rule"].Split(',');

            using (Graphics g = Graphics.FromImage(bitmap))
            {
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
                    int centerX = bitmap.Width / 2;
                    int centerY = bitmap.Height / 2;

                    // 構建要顯示的文本
                    string shiftAndRotateText = $"Shift X: {shiftX}\nShift Y: {shiftY}\nRotate Shift Y: {rotateShiftY}\n{extramsg}";

                    // 設置文本格式
                    Font font = new Font("Arial", 56, FontStyle.Bold);
                    Brush brush = Brushes.YellowGreen;
                    StringFormat format = new StringFormat
                    {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Center
                    };

                    // 繪製文本在圖像的中心位置
                    g.DrawString(shiftAndRotateText, font, brush, new PointF(centerX, centerY), format);


                    Pen crossPen = new Pen(Color.Yellow, 2);
                    g.DrawLine(crossPen, 0, centerY, bitmap.Width, centerY);
                    g.DrawLine(crossPen, centerX, 0, centerX, bitmap.Height);
                    g.DrawLine(crossPen, 0, 0, bitmap.Width, bitmap.Height);
                    g.DrawLine(crossPen, 0, bitmap.Height, bitmap.Width, 0);

                    //float roiRadius = (float)Math.Sqrt(Math.Pow(bitmap.Width / 2f, 2) + Math.Pow(bitmap.Height / 2f, 2)) * 0.7F;
                    //RectangleF circleRect = new RectangleF(centerX - roiRadius, centerY - roiRadius, roiRadius * 2, roiRadius * 2);
                    //Pen circlePen = new Pen(Color.Blue, 6);
                    //g.DrawEllipse(circlePen, circleRect);
                }
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

        private void DrawCrossAndCircle(Graphics graphics, int width, int height, float roiRatio)
        {


        }

        private void InitTCP()
        {
            //MotionDevice = comboBox_plcdevices.SelectedItem.ToString();

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

        public void SFRData_Value()
        {

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
            TL_atan_avg = (TL_X_atan + BL_X_atan) / 2;

            //轉換成角度
            double X_angle = Math.Round(Math.Abs(TL_X_atan) * 180 / Math.PI, 2);
            double Y_angle = Math.Round(Math.Abs(BL_X_atan) * 180 / Math.PI, 2);
            IQ_rotate_angle = (X_angle + Y_angle) / 2;
            IQ_tcp_angle = Math.Round(Math.Abs((X_angle + Y_angle) / 2), 2) * 100;
        }

        public bool View_Send(string inputX,string inputY,string inputU)
        {
            if (inputU.Contains("U+"))
            {
                IQ_U_count += IQ_tcp_angle;

            }
            if (inputU.Contains("U-"))
            {
                IQ_U_count -= IQ_tcp_angle;

            }
            if (inputX.Contains("X+"))
            {
                IQ_X_count += IQ_tcp_degreeX;

            }
            if (inputX.Contains("X-"))
            {
                IQ_X_count -= IQ_tcp_degreeX;

            }
            if (inputY.Contains("Y+"))
            {
                IQ_Y_count += IQ_tcp_degreeY;

            }
            if (inputY.Contains("Y-"))
            {
                IQ_Y_count -= IQ_tcp_degreeY;

            }

            if (IQ_U_count < -2000 || IQ_U_count > 2000)
            {
                return false;
            }
            else if (IQ_X_count < -800 || IQ_X_count > 800)
            {
                return false;
            }
            else if (IQ_Y_count < -700 || IQ_Y_count > 700)
            {
                return false;
            }
            else
            {
                if (!MotionCtrlDevice.SEND_ASCII(inputX, inputY, inputU, ref output))
                {
                    return false;
                }
                else
                {
                    if (output.Contains("OK"))
                    {
                        Thread.Sleep(600);
                        return true;
                    }
                    else if (output.Contains("Error"))
                    {
                        return false;
                    }
                    else
                    {
                        LogMessage("View_Send Moving Fail", MessageLevel.Error);
                        return false;
                    }
                }

            }
        }
    }

    public class Teach_DevList : TypeConverter  //下拉式選單
    {
        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            if (GlobalNew.Devices.Count != 0)
            {
                List<string> hwListKeys = new List<string>();

                hwListKeys.Add("");
                hwListKeys.AddRange(GlobalNew.Devices
                        .Where(item => item.Value is TeachBase)
                        .Select(item => item.Key)
                        .ToList()
                        );

                try
                {
                    string multiDeviceTable = string.Empty;
                    foreach (var value in GlobalNew.Devices.Values)
                    {
                        if (value is DUT_BASE)
                        {
                            if (((DUT_BASE)(value)).Enable)
                            {
                                multiDeviceTable = ((DUT_BASE)(value)).MultiDeviceTable;
                                break;
                            }
                        }
                    }
                    if (!string.IsNullOrEmpty(multiDeviceTable))
                    {
                        // 解析 JSON 字符串
                        JArray data = JArray.Parse(multiDeviceTable);

                        // 找到 DeviceObject 欄中的值是否在 GlobalNew.Devices 中，並將對應的 SharedName 值列到 hwListKeys 中
                        foreach (var item in data)
                        {
                            string deviceObject = (string)item["DeviceObject"];
                            if (GlobalNew.Devices.ContainsKey(deviceObject))
                            {
                                if (GlobalNew.Devices[deviceObject] is TeachBase)
                                    hwListKeys.Add($"@{(string)item["SharedName"]}@");
                            }
                        }
                    }
                }
                catch
                {
                    return new StandardValuesCollection(new string[] { "" });
                }

                return new StandardValuesCollection(hwListKeys);
            }
            else
            {
                return new StandardValuesCollection(new string[] { "" });
            }
        }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;

        }
    }

}
