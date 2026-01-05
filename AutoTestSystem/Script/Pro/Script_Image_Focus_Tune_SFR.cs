using AutoTestSystem.Base;
using AutoTestSystem.Equipment.Motion;
using AutoTestSystem.Model;
using MathNet.Numerics.Interpolation;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenCvSharp.Aruco;
///TEST PDF
using PdfSharp.Drawing;
using PdfSharp.Drawing.Layout;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml.Linq;
using static AutoTestSystem.Model.IQ_SingleEntry;
///TEST PDF

namespace AutoTestSystem.Script
{
    internal class Script_Image_Focus_Tune_SFR : Script_Image_Base
    {
        string strOutData = string.Empty;

        public MotionBase MotionDevice = null;
        //改成下面這個//
        [JsonIgnore]
        [Browsable(false)]
        TCP MotionCtrlDevice = null;//先寫死

        [StructLayout(LayoutKind.Sequential)]
        public struct FitResultC
        {
            public double A;
            public double mu;
            public double sigma;
            public double baseline;
            public double fit_time_ms;
            [MarshalAs(UnmanagedType.I1)] public bool success;
        }

        
        [DllImport("gaussian_lib_cs.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool curve_fit_gaussian_c(
        double[] x_vals, double[] y_vals, int len, out FitResultC result);

        
        [Category("Device"), Description("自訂顯示名稱"), TypeConverter(typeof(MotionDeviceList))]
        public string MotionDeviceSel { get; set; }

        [Category("SE Parameters"), Description("load content"), Editor(typeof(CommandEditor_MakeWriteLine), typeof(UITypeEditor))]
        public string PIN { get; set; } = "";

        [Category("SE Parameters"), Description("DLL Path")]
        public string DLLPath { get; set; } = "";

        [Category("Check"), Description("DLL Path")]
        public bool CheckROI { get; set; } = false;

        [Category("Draw"), Description("")]
        public bool DrawROI { get; set; } = false;

        [Category("Draw"), Description("")]
        public bool DrawResult { get; set; } = false;

        [Category("Draw"), Description("")]
        public bool DrawCross { get; set; } = false;

        [Category("Draw"), Description("")]
        public bool DrawDiagonal { get; set; } = false;

        [Category("Focus_Tune_Deg"), Description("Define Rough Tune Rotate Deg")]
        public int Rotate_Step_Rough { get; set; } = 80;

        [Category("Focus_Tune_Deg"), Description("Define Fine Tune Rotate Deg")]
        public int Rotate_Step_Fine { get; set; } = 20;

        [Category("Focus_Tune_Deg"), Description("Define Reverse Rotate Deg")]
        public int Rotate_Step_Reverse { get; set; } = 0;

        [Category("Focus_Tune_Deg"), Description("Define Backlash Rotate Deg")]
        public int Rotate_Step_Backlash { get; set; } = 0;

        [Category("Focus_Tune_Deg"), Description("Define Final Offset Rotate Deg")]
        public int Rotate_Step_Offset { get; set; } = 0;

        [Category("Focus_Tune_Deg"), Description("Define Pixel Size (unit: um)")]
        public double Pixel_Size { get; set; } = 2;

        [Category("Focus_Tune_Deg"), Description("Define Rotate Step Position")]
        public double Rotate_Step_Position { get; set; } = 0.0014;

        [Category("Focus_Tune_Deg"), Description("Choose Final Offset Left or Right"), TypeConverter(typeof(Focus_Tune_Offset))]
        public string Rotate_Step_Offest_Dir { get; set; } = "Left_Offset";

        [Category("Focus_Tune_Deg"), Description("Disable/Enable Reverse Step Function"), TypeConverter(typeof(Focus_Tune_Reverse_Enable))]
        public string Rotate_Step_Reverse_Func { get; set; } = "Enable";

        [Category("Focus_Tune_Deg"), Description("Define Total Rotate Deg")]
        public int Rotate_Step_Total { get; set; } = 6000;

        [Category("Focus_Tune_Method"), Description("Define Focus Tune Curve Stop Percentage(%)")]
        public int Rotate_Stop_Value { get; set; } = 40;

        [Category("Focus_Tune_Method"), Description("Define SFR Direction of V / H / H&V"), TypeConverter(typeof(Focus_Tune_Direction_HV))]
        public string Rotate_Dir_HV { get; set; } = "H&V";

        [Category("Focus_Tune_Method"), Description("Define MiniScore CT (Center) SPEC")]
        public int Rotate_MS_CT_SPEC { get; set; } = 60;

        [Category("Focus_Tune_Method"), Description("Define MiniScore CN (Corner) SPEC")]
        public int Rotate_MS_CN_SPEC { get; set; } = 40;

        [Category("Focus_Tune_Method"), Description("Define MiniScore Method: Middle / Peak"), TypeConverter(typeof(Focus_Tune_MS_Method))]
        public string Rotate_MS_Mth { get; set; } = "Middle";

        [Category("Focus_Tune_Method"), Description("Define Focus Tune Best Position Method: Miniscore"), TypeConverter(typeof(Focus_Tune_BP_Method))]
        public string Rotate_BP_Mth { get; set; } = "MiniScore";

        [Category("Focus_Tune_Method"), Description("Define Rough to Fine Step from MiniScore Value")]
        public double Rotate_RF_MS_Value { get; set; } = 0.3;

        [Category("Focus_Tune_Method"), Description("Define Focus Tune Best Position Method: Miniscore"), TypeConverter(typeof(Focus_Tune_Field))]
        public string Focus_Field { get; set; } = "Inter";

        [Category("Focus_Tune_Method"), Description("Back to the Best position seperate by Fine Degree")]
        public bool Rotate_BP_Sep_Deg { get; set; } = true;



        [Category("Focus_Tune_Simu"), Description("If wnat to use Group Image to simulation, please disable motor function"), TypeConverter(typeof(Focus_Tune_Simu_Motor))]
        public string Rotate_Simu_Motor { get; set; } = "Disable";

        [Category("Save"), Description("Save Report Path Name")]
        public string save_report_path { get; set; } = "";

        [Category("Save"), Description("Save Report File Name")]
        public string save_report_file { get; set; } = "";


        [Category("Gussian_Method"), Description("Define Focus Tune Method using Gussian (NO/YES)"), TypeConverter(typeof(Focus_Tune_Gussian_Method))]
        public string Rotate_BP_Mth_Gus { get; set; } = "NO";
        [Category("Gussian_Method"), Description("Define Range 1 for MS(0 to 0.6)")]
        public int Rotate_Step_Range1 { get; set; } = 60;
        [Category("Gussian_Method"), Description("Define Range 1 for MS(0.6 to 0.8)")]
        public int Rotate_Step_Range2 { get; set; } = 50;
        [Category("Gussian_Method"), Description("Define Range 1 for MS(0.8 to 1)")]
        public int Rotate_Step_Range3 { get; set; } = 40;

        [Category("TabShow"), Description("Show Image on Tab")]
        public bool Show_Tab { get; set; } = false;


        [Category("Save"), Description("Save Image Option"), TypeConverter(typeof(SaveImage))]
        public string saveImgage { get; set; } = "NO";

        [Category("Save"), Description("Save path")]
        public string savepath { get; set; } = "";
        [Category("Save"), Description("Save path BMP File Name")]
        public string savepath_bmp_file { get; set; } = "";
        [Category("Save"), Description("Save path RAW File Name")]
        public string savepath_raw_file { get; set; } = "";


        public string strstringoutput = "";
        private List<Tuple<double, double>> CTV;
        private List<Tuple<double, double>> CTH; 
        private List<Tuple<double, double>> LTV;
        private List<Tuple<double, double>> LTH;
        private List<Tuple<double, double>> RTV;
        private List<Tuple<double, double>> RTH;
        private List<Tuple<double, double>> LBV;
        private List<Tuple<double, double>> LBH;
        private List<Tuple<double, double>> RBV;
        private List<Tuple<double, double>> RBH;
        private List<Tuple<double, double>> MiniScore_List;
        private List<Tuple<double, double>> MiniScore_List_Guss;
        private bool R_F_miniscore_flag;
        //private double Rough_Degree;
        //private double Fine_Degree;
        private double Total_Rel_Position;
        private double FineTune_Rel_Position;
        private bool stop_flag;
        private bool Total_stop_flag;
        private bool TCP_stop_flag;
        private double DOF_L_Pos;
        private double DOF_R_Pos;
        private double DOF_LR_Minus_Pos;
        private double MS_middle_Pos, MS_middle_Pos_GUS;
        private double MS_peak_Pos, MS_peak_Pos_GUS;
        private double MS_Best_val, MS_Best_val_GUS;
        private int Best_position;
        ///Best Postion apply pos to get sfr value param
        private double CTV_AP_Yvalue, CTH_AP_Yvalue, LTV_AP_Yvalue, LTH_AP_Yvalue, RTV_AP_Yvalue, RTH_AP_Yvalue, LBV_AP_Yvalue, LBH_AP_Yvalue, RBV_AP_Yvalue, RBH_AP_Yvalue;
        private double CTV_AP_Yvalue_GUS, CTH_AP_Yvalue_GUS, LTV_AP_Yvalue_GUS, LTH_AP_Yvalue_GUS, RTV_AP_Yvalue_GUS, RTH_AP_Yvalue_GUS, LBV_AP_Yvalue_GUS, LBH_AP_Yvalue_GUS, RBV_AP_Yvalue_GUS, RBH_AP_Yvalue_GUS;
        /// Tilt calculate parameter
        private double tilt_degree_x_v, tilt_degree_y_v, tilt_degree_x_h, tilt_degree_y_h;
        private double tilt_degree_x, tilt_degree_y;
        public Dictionary<string, Rectangle> ROI_INFO;

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
                // 如果任一坐標為 -1，則返回 null
                if (top < 0 || left < 0 || bottom < 0 || right < 0)
                {
                    return null;
                }
                int width = right - left;
                int height = bottom - top;
                if (width <= 0 || height <= 0)
                {
                    return null;
                }
                return new Rectangle(left, top, width, height);
            }

            return null;
        }

        public override void Dispose()
        {
            //throw new NotImplementedException();
        }
        public override bool PreProcess()
        {


            CTV = null;
            CTH = null;
            LTV = null;
            LTH = null;
            RTV = null;
            RTH = null;
            LBV = null;
            LBH = null;
            RBV = null;
            RBH = null;
            MiniScore_List = null;
            MiniScore_List_Guss = null;
            stop_flag = false;
            R_F_miniscore_flag = false;
            Total_stop_flag = false;
            TCP_stop_flag = false;
            Total_Rel_Position = 0;
            FineTune_Rel_Position = 0;
            Best_position = 0;
            DOF_LR_Minus_Pos = 0;
            //Rough_Degree = 0; //need add deg in properties
            //Fine_Degree = 0;  //need add deg in properties

            ///Best Postion apply pos to get sfr value param


            strOutData = string.Empty;

            //MotionDevice = (Image_Base)Devices[MotionDeviceSel];
            if (!string.IsNullOrEmpty(MotionDeviceSel))
            {
                MotionCtrlDevice = GlobalNew.Devices[MotionDeviceSel] as TCP;
                if (MotionCtrlDevice == null)
                {
                    LogMessage($"GlobalNew.Devices[{MotionDeviceSel}] is not TcpIpClient type");
                    return false;
                }

            }
            else
            {
                LogMessage($"GlobalNew.Devices[{MotionDevice}] is null");
                return false;
            }


            return true;
        }

        public override bool Process(Image_Base Image,ref string strOutData)
        {




            // 紀錄測試開始時間
            DateTime startTime = DateTime.Now;
            LogMessage("FocusTune測試開始時間: " + startTime.ToString("HH:mm:ss.fff"));



            try
            {
                
                CTV =  new List<Tuple<double, double>>();
                CTH =  new List<Tuple<double, double>>();
                LTV =  new List<Tuple<double, double>>();
                LTH =  new List<Tuple<double, double>>();
                RTV =  new List<Tuple<double, double>>();
                RTH =  new List<Tuple<double, double>>();
                LBV =  new List<Tuple<double, double>>();
                LBH =  new List<Tuple<double, double>>();
                RBV =  new List<Tuple<double, double>>();
                RBH =  new List<Tuple<double, double>>();
                MiniScore_List = new List<Tuple<double, double>>();
                MiniScore_List_Guss = new List<Tuple<double, double>>();
                Dictionary<string, string> outputdata = new Dictionary<string, string>();
                Dictionary<string, string> LF_INFO = new Dictionary<string, string>();
                ROI_INFO = new Dictionary<string, Rectangle>();
                /// TestInfo data struct for json de-serialize//
                //List<ImageInfo> img_list = new List<ImageInfo>();
                Dictionary<string, Dictionary<string, object>> outputdata2 = new Dictionary<string, Dictionary<string, object>>();
                var sfr_data = new Dictionary<string, object>();
                /// TestInfo data struct for json de-serialize//


                int motor_move_thread_step = 0;
                //// save group image
                DirectoryInfo di_image = null;
                if (saveImgage == "YES")
                    if (!Directory.Exists(savepath))
                        di_image = Directory.CreateDirectory(savepath);


                while (!stop_flag) /////while Flag
                {
                    string str_Address = string.Empty;
                    if (!Image.Capture(ref str_Address))
                    {
                        return false;
                    }
                    long addressValue = Convert.ToInt64(str_Address.Substring(2), 16);
                    // Create an IntPtr from the long value
                    IntPtr ptr = new IntPtr(addressValue);

                    //// save image///
                    
                    if (saveImgage == "YES")
                    {
                        if (savepath_bmp_file == "")
                            LogMessage($"Igorne Save BMP File Name", MessageLevel.Debug);
                        else
                            Image.SaveImage(1, savepath + ReplaceProp(savepath_bmp_file));

                        if (savepath_raw_file == "")
                            LogMessage($"Igorne Save RAW File Name", MessageLevel.Debug);
                        else
                            Image.SaveImage(0, savepath + ReplaceProp(savepath_raw_file));
                    }
                    //// save image///

                    string PIN_tmp = PIN.Replace("%address%", str_Address);
                    

                    string oricontent = PIN_tmp.Replace("\\n", "\n");
                    //string oupt = "";
                    // 要注意這個地方檔案位置要是正確的，檔名要是正確的
                    string oricontent_Trans = ReplaceProp(oricontent);
                    //string replaceImagePath = ReplaceProp(ImagePath);
                    IQ_SingleEntry.SE_StartAction(DLLPath, oricontent_Trans, ref strOutData, outputdata);
                    LogMessage($"{strOutData}");

                    double CT_V_Top = 0, CT_H_Right = 0, CT_V_Bottom = 0, CT_H_Left = 0, TL_V_Top = 0, TL_H_Right = 0, TL_V_Bottom = 0, TL_H_Left = 0, TR_V_Top = 0, TR_H_Right = 0, TR_V_Bottom = 0, TR_H_Left = 0, BL_V_Top = 0,
                       BL_H_Right = 0, BL_V_Bottom = 0, BL_H_Left = 0, BR_V_Top = 0, BR_H_Right = 0, BR_V_Bottom = 0, BR_H_Left = 0;
                    if (outputdata.ContainsKey("SFR_SFR_CT_Top"))
                        CT_V_Top = double.Parse(outputdata["SFR_SFR_CT_Top"]);
                    if (outputdata.ContainsKey("SFR_SFR_CT_Right"))
                        CT_H_Right = double.Parse(outputdata["SFR_SFR_CT_Right"]);
                    if (outputdata.ContainsKey("SFR_SFR_CT_Bottom"))
                        CT_V_Bottom = double.Parse(outputdata["SFR_SFR_CT_Bottom"]);
                    if (outputdata.ContainsKey("SFR_SFR_CT_Left"))
                        CT_H_Left = double.Parse(outputdata["SFR_SFR_CT_Left"]);
                    if (outputdata.ContainsKey("SFR_SFR_TL_Top"))
                        TL_V_Top = double.Parse(outputdata["SFR_SFR_TL_Top"]);
                    if (outputdata.ContainsKey("SFR_SFR_TL_Right"))
                        TL_H_Right = double.Parse(outputdata["SFR_SFR_TL_Right"]);
                    if (outputdata.ContainsKey("SFR_SFR_TL_Bottom"))
                        TL_V_Bottom = double.Parse(outputdata["SFR_SFR_TL_Bottom"]);
                    if (outputdata.ContainsKey("SFR_SFR_TL_Left"))
                        TL_H_Left = double.Parse(outputdata["SFR_SFR_TL_Left"]);
                    if (outputdata.ContainsKey("SFR_SFR_TR_Top"))
                        TR_V_Top = double.Parse(outputdata["SFR_SFR_TR_Top"]);
                    if (outputdata.ContainsKey("SFR_SFR_TR_Right"))
                        TR_H_Right = double.Parse(outputdata["SFR_SFR_TR_Right"]);
                    if (outputdata.ContainsKey("SFR_SFR_TR_Bottom"))
                        TR_V_Bottom = double.Parse(outputdata["SFR_SFR_TR_Bottom"]);
                    if (outputdata.ContainsKey("SFR_SFR_TR_Left"))
                        TR_H_Left = double.Parse(outputdata["SFR_SFR_TR_Left"]);
                    if (outputdata.ContainsKey("SFR_SFR_BL_Top"))
                        BL_V_Top = double.Parse(outputdata["SFR_SFR_BL_Top"]);
                    if (outputdata.ContainsKey("SFR_SFR_BL_Right"))
                        BL_H_Right = double.Parse(outputdata["SFR_SFR_BL_Right"]);
                    if (outputdata.ContainsKey("SFR_SFR_BL_Bottom"))
                        BL_V_Bottom = double.Parse(outputdata["SFR_SFR_BL_Bottom"]);
                    if (outputdata.ContainsKey("SFR_SFR_BL_Left"))
                        BL_H_Left = double.Parse(outputdata["SFR_SFR_BL_Left"]);
                    if (outputdata.ContainsKey("SFR_SFR_BR_Top"))
                        BR_V_Top = double.Parse(outputdata["SFR_SFR_BR_Top"]);
                    if (outputdata.ContainsKey("SFR_SFR_BR_Right"))
                        BR_H_Right = double.Parse(outputdata["SFR_SFR_BR_Right"]);
                    if (outputdata.ContainsKey("SFR_SFR_BR_Bottom"))
                        BR_V_Bottom = double.Parse(outputdata["SFR_SFR_BR_Bottom"]);
                    if (outputdata.ContainsKey("SFR_SFR_BR_Left"))
                        BR_H_Left = double.Parse(outputdata["SFR_SFR_BR_Left"]);

                    if (Focus_Field == "Inter")
                    {
                        if (CT_V_Top == 0 || CT_H_Right == 0 || CT_V_Bottom == 0 || CT_H_Left == 0 || TL_H_Right == 0 || TL_V_Bottom == 0 || TR_V_Bottom == 0 || TR_H_Left == 0 || BL_V_Top == 0 || BL_H_Right == 0 || BR_V_Top == 0 || BR_H_Left == 0)
                        {
                            CT_V_Top = 0;
                            CT_H_Right = 0;
                            CT_V_Bottom = 0;
                            CT_H_Left = 0;
                            TL_H_Right = 0;
                            TL_V_Bottom = 0;
                            TR_V_Bottom = 0;
                            TR_H_Left = 0;
                            BL_V_Top = 0;
                            BL_H_Right = 0;
                            BR_V_Top = 0;
                            BR_H_Left = 0;
                            LogMessage("Rough start", MessageLevel.Info);
                            LogMessage("SFR VALUE = 0  Keep Move position", MessageLevel.Info);
                        }
                    }
                    else if (Focus_Field == "Outer") 
                    {
                        if (CT_V_Top == 0 || CT_H_Right == 0 || CT_V_Bottom == 0 || CT_H_Left == 0 || TL_H_Left == 0 || TL_V_Top == 0 || TR_V_Top == 0 || TR_H_Right == 0 || BL_V_Bottom == 0 || BL_H_Left == 0 || BR_V_Bottom == 0 || BR_H_Right == 0)
                        {
                            CT_V_Top = 0;
                            CT_H_Right = 0;
                            CT_V_Bottom = 0;
                            CT_H_Left = 0;
                            TL_H_Left = 0;
                            TL_V_Top = 0;
                            TR_V_Top = 0;
                            TR_H_Right = 0;
                            BL_V_Bottom = 0;
                            BL_H_Left = 0;
                            BR_V_Bottom = 0;
                            BR_H_Right = 0;
                            LogMessage("Rough start", MessageLevel.Info);
                            LogMessage("SFR VALUE = 0  Keep Move position", MessageLevel.Info);
                        }


                    }
                        double CT_V = 0;
                    double CT_H = 0;
                    double CenterMinMTF = 0;
                    double CornerMinMTF = 0;

                    CT_V = (CT_V_Top + CT_V_Bottom) / 2;
                    CT_H = (CT_H_Right + CT_H_Left) / 2;
                    CenterMinMTF = Math.Min(CT_V, CT_H);

                    if (Focus_Field == "Inter")
                    {

                        if (Rotate_Dir_HV == "H")
                        {
                            CornerMinMTF = Math.Min(TL_H_Right, TR_H_Left);
                            CornerMinMTF = Math.Min(CornerMinMTF, BL_H_Right);
                            CornerMinMTF = Math.Min(CornerMinMTF, BR_H_Left);
                        }
                        else if (Rotate_Dir_HV == "V")
                        {
                            CornerMinMTF = Math.Min(TL_V_Bottom, TR_V_Bottom);
                            CornerMinMTF = Math.Min(CornerMinMTF, BL_V_Top);
                            CornerMinMTF = Math.Min(CornerMinMTF, BR_V_Top);
                        }
                        else if (Rotate_Dir_HV == "H&V")
                        {
                            CornerMinMTF = Math.Min(TL_H_Right, TL_V_Bottom);
                            CornerMinMTF = Math.Min(CornerMinMTF, TR_V_Bottom);
                            CornerMinMTF = Math.Min(CornerMinMTF, TR_H_Left);
                            CornerMinMTF = Math.Min(CornerMinMTF, BL_V_Top);
                            CornerMinMTF = Math.Min(CornerMinMTF, BL_H_Right);
                            CornerMinMTF = Math.Min(CornerMinMTF, BR_V_Top);
                            CornerMinMTF = Math.Min(CornerMinMTF, BR_H_Left);
                        }

                    }else if(Focus_Field == "Outer") 
                    {
                        if (Rotate_Dir_HV == "H")
                        {
                            CornerMinMTF = Math.Min(TL_H_Left, TR_H_Right);
                            CornerMinMTF = Math.Min(CornerMinMTF, BL_H_Left);
                            CornerMinMTF = Math.Min(CornerMinMTF, BR_H_Right);
                        }
                        else if (Rotate_Dir_HV == "V")
                        {
                            CornerMinMTF = Math.Min(TL_V_Top, TR_V_Top);
                            CornerMinMTF = Math.Min(CornerMinMTF, BL_V_Bottom);
                            CornerMinMTF = Math.Min(CornerMinMTF, BR_V_Bottom);
                        }
                        else if (Rotate_Dir_HV == "H&V")
                        {
                            CornerMinMTF = Math.Min(TL_H_Left, TL_V_Top);
                            CornerMinMTF = Math.Min(CornerMinMTF, TR_V_Top);
                            CornerMinMTF = Math.Min(CornerMinMTF, TR_H_Right);
                            CornerMinMTF = Math.Min(CornerMinMTF, BL_V_Bottom);
                            CornerMinMTF = Math.Min(CornerMinMTF, BL_H_Left);
                            CornerMinMTF = Math.Min(CornerMinMTF, BR_V_Bottom);
                            CornerMinMTF = Math.Min(CornerMinMTF, BR_H_Right);
                        }



                    }

                    double CenterMinMTFSpec = CenterMinMTF / Rotate_MS_CT_SPEC;  //CT SPEC need to add properties
                    double CornerMinMTFSpec = CornerMinMTF / Rotate_MS_CN_SPEC;  //CN SPEC need to add properties
                    double Result_MinScore = Math.Min(CenterMinMTFSpec, CornerMinMTFSpec);

                    if (CenterMinMTFSpec > Rotate_RF_MS_Value) // add Center miniscore spec in properties for Rough to fine
                    {
                        R_F_miniscore_flag = true;
                        LogMessage("Rough to Fine CT Mini Score Done", MessageLevel.Info);
                    }
                    if (Result_MinScore > Rotate_RF_MS_Value) // add Center miniscore value in properties for Rough to fine.
                    {
                        //list.Add(Tuple.Create(3.14, 2.71));
                        //list.Add(Tuple.Create(1.23, 4.56));
                        if (Focus_Field == "Inter")
                        {
                            CTV.Add(Tuple.Create(FineTune_Rel_Position, CT_V));
                            CTH.Add(Tuple.Create(FineTune_Rel_Position, CT_H));
                            LTV.Add(Tuple.Create(FineTune_Rel_Position, TL_V_Bottom));
                            LTH.Add(Tuple.Create(FineTune_Rel_Position, TL_H_Right));
                            RTV.Add(Tuple.Create(FineTune_Rel_Position, TR_V_Bottom));
                            RTH.Add(Tuple.Create(FineTune_Rel_Position, TR_H_Left));
                            LBV.Add(Tuple.Create(FineTune_Rel_Position, BL_V_Top));
                            LBH.Add(Tuple.Create(FineTune_Rel_Position, BL_H_Right));
                            RBV.Add(Tuple.Create(FineTune_Rel_Position, BR_V_Top));
                            RBH.Add(Tuple.Create(FineTune_Rel_Position, BR_H_Left));
                            //MiniScore_List.Add(Tuple.Create(FineTune_Rel_Position, Result_MinScore));

                            sfr_data.Clear();
                            sfr_data["DAC"] = FineTune_Rel_Position;
                            sfr_data["CTV"] = Math.Round(CT_V, 2);
                            sfr_data["CTH"] = Math.Round(CT_H, 2);
                            sfr_data["LTV"] = Math.Round(TL_V_Bottom, 2);
                            sfr_data["LTH"] = Math.Round(TL_H_Right, 2);
                            sfr_data["RTV"] = Math.Round(TR_V_Bottom, 2);
                            sfr_data["RTH"] = Math.Round(TR_H_Left, 2);
                            sfr_data["LBV"] = Math.Round(BL_V_Top, 2);
                            sfr_data["LBH"] = Math.Round(BL_H_Right, 2);
                            sfr_data["RBV"] = Math.Round(BR_V_Top, 2);
                            sfr_data["RBH"] = Math.Round(BR_H_Left, 2);
                            sfr_data["MS"] = Math.Round(Result_MinScore, 2);
                            outputdata2.Add(FineTune_Rel_Position.ToString(), new Dictionary<string, object>(sfr_data));


                        }
                        else if (Focus_Field == "Outer") 
                        {
                            CTV.Add(Tuple.Create(FineTune_Rel_Position, CT_V));
                            CTH.Add(Tuple.Create(FineTune_Rel_Position, CT_H));
                            LTV.Add(Tuple.Create(FineTune_Rel_Position, TL_V_Top));
                            LTH.Add(Tuple.Create(FineTune_Rel_Position, TL_H_Left));
                            RTV.Add(Tuple.Create(FineTune_Rel_Position, TR_V_Top));
                            RTH.Add(Tuple.Create(FineTune_Rel_Position, TR_H_Right));
                            LBV.Add(Tuple.Create(FineTune_Rel_Position, BL_V_Bottom));
                            LBH.Add(Tuple.Create(FineTune_Rel_Position, BL_H_Left));
                            RBV.Add(Tuple.Create(FineTune_Rel_Position, BR_V_Bottom));
                            RBH.Add(Tuple.Create(FineTune_Rel_Position, BR_H_Right));
                            //MiniScore_List.Add(Tuple.Create(FineTune_Rel_Position, Result_MinScore));

                            sfr_data.Clear();
                            sfr_data["DAC"] = FineTune_Rel_Position;
                            sfr_data["CTV"] = Math.Round(CT_V, 2);
                            sfr_data["CTH"] = Math.Round(CT_H, 2);
                            sfr_data["LTV"] = Math.Round(TL_V_Top, 2);
                            sfr_data["LTH"] = Math.Round(TL_H_Left, 2);
                            sfr_data["RTV"] = Math.Round(TR_V_Top, 2);
                            sfr_data["RTH"] = Math.Round(TR_H_Right, 2);
                            sfr_data["LBV"] = Math.Round(BL_V_Bottom, 2);
                            sfr_data["LBH"] = Math.Round(BL_H_Left, 2);
                            sfr_data["RBV"] = Math.Round(BR_V_Bottom, 2);
                            sfr_data["RBH"] = Math.Round(BR_H_Right, 2);
                            sfr_data["MS"] = Math.Round(Result_MinScore, 2);
                            outputdata2.Add(FineTune_Rel_Position.ToString(), new Dictionary<string, object>(sfr_data));

                        }

                    }

                    /// add motion related (if Motion fail please break while loop)///


                    if (R_F_miniscore_flag)
                    {
                        motor_move_thread_step = Rotate_Step_Fine;
                    }
                    else
                    {
                        motor_move_thread_step = Rotate_Step_Rough; 
                    }

                   MotorStepMove(0, motor_move_thread_step, 0);  // TCP Write motor move
                 

                    /// add motion related (if Motion fail please break while loop)///
                    double max_pos_CTV = 0;
                    double max_pos_CTH = 0;
                    double current_max_val_CTV = 0;
                    double current_max_val_CTH = 0;
                    if (CTV.Count > 3)
                    {
                        HelperFindPeak(CTV, ref max_pos_CTV, ref current_max_val_CTV);
                        HelperFindPeak(CTV, ref max_pos_CTH, ref current_max_val_CTH);
                        double max_cur_ratio_v = CT_V / current_max_val_CTV;
                        double max_cur_ratio_h = CT_H / current_max_val_CTH;
                        if (max_cur_ratio_v < (1-((double)Rotate_Stop_Value /100)) || max_cur_ratio_h < (1 - ((double)Rotate_Stop_Value / 100))) 
                        {
                            stop_flag = true;
                        }
                    }
                    
                    if(Total_Rel_Position >= Rotate_Step_Total)
                    {
                        stop_flag = true;
                        Total_stop_flag = true;
                        LogMessage("Over Focus Tune Total Degree => while loop  break", MessageLevel.Error);
                    }

                    /////////Gussian method Sample aquire method////////////
                    if (Rotate_BP_Mth_Gus == "YES")
                    {
                        if (R_F_miniscore_flag)
                        {
                            //use adpative deg to each step
                            if (Result_MinScore >= 0.30 && Result_MinScore < 0.60)
                                Rotate_Step_Fine = Rotate_Step_Range1;  // 
                            else if (Result_MinScore >= 0.60 && Result_MinScore < 0.90)
                                Rotate_Step_Fine = Rotate_Step_Range2;  // 
                            else if (Result_MinScore >= 0.90 && Result_MinScore < 1.0)
                                Rotate_Step_Fine = Rotate_Step_Range3;  // 
                            else if (Result_MinScore >= 1.0)
                                Rotate_Step_Fine = 20;  // 



                        }
                    }
                    /////////Gussian method Sample aquire method////////////


                    if (R_F_miniscore_flag)
                    {
                       
                            FineTune_Rel_Position += Rotate_Step_Fine;  // need add fine degree 
                            Total_Rel_Position += Rotate_Step_Fine;  // need add fine degree 
                    }
                    else
                    {
                        Total_Rel_Position += Rotate_Step_Rough;
                    }

                    List<DrawElement> elements = new List<DrawElement>();
                    //if (DrawROI)  /// original version
                    //{

                    //    // 遍歷字典，找到所有的 ROI 鍵
                    //    foreach (var entry in outputdata)
                    //    {
                    //        if (entry.Key.StartsWith("ROI_") && entry.Key.EndsWith("_Roi"))
                    //        {
                    //            // 解析 ROI 坐標
                    //            var roiCoordinates = ParseRoiCoordinates(entry.Value, outputdata["ROI_SFR_SFR_Roi_Rule"].Split(','));
                    //            if (roiCoordinates != null)
                    //            {
                    //                    elements.Add(new DrawElement((Rectangle)roiCoordinates, "", Color.Yellow, 34, 6f, DrawElement.ElementType.Rectangle));  
                    //            }
                    //        }
                    //    }
                    //}
                    Color ROI_Chk_Spec = Color.Green;
                    if (DrawROI)  /// temp version to identify the spec for CT/CN With Color
                    {
                        // 遍歷字典，找到所有的 ROI 鍵
                        foreach (var entry in outputdata)
                        {
                            if (entry.Key.StartsWith("ROI_") && entry.Key.EndsWith("_Roi"))
                            {
                                if (entry.Key.Contains("CT_Top")) 
                                {
                                    if(CT_V_Top >= Rotate_MS_CT_SPEC)
                                        ROI_Chk_Spec = Color.Green;
                                    else
                                        ROI_Chk_Spec = Color.Red;
                                }
                                else if(entry.Key.Contains("CT_Right"))
                                {
                                    if (CT_H_Right >= Rotate_MS_CT_SPEC)
                                        ROI_Chk_Spec = Color.Green;
                                    else
                                        ROI_Chk_Spec = Color.Red;
                                }
                                else if (entry.Key.Contains("CT_Bottom"))
                                {
                                    if (CT_V_Bottom >= Rotate_MS_CT_SPEC)
                                        ROI_Chk_Spec = Color.Green;
                                    else
                                        ROI_Chk_Spec = Color.Red;
                                }
                                else if (entry.Key.Contains("CT_Left"))
                                {
                                    if (CT_H_Left >= Rotate_MS_CT_SPEC)
                                        ROI_Chk_Spec = Color.Green;
                                    else
                                        ROI_Chk_Spec = Color.Red;
                                }
                                else if (entry.Key.Contains("TL_Top"))
                                {
                                    if (TL_V_Top >= Rotate_MS_CN_SPEC)
                                        ROI_Chk_Spec = Color.Green;
                                    else
                                        ROI_Chk_Spec = Color.Red;
                                }
                                else if (entry.Key.Contains("TL_Right"))
                                {
                                    if (TL_H_Right >= Rotate_MS_CN_SPEC)
                                        ROI_Chk_Spec = Color.Green;
                                    else
                                        ROI_Chk_Spec = Color.Red;
                                }
                                else if (entry.Key.Contains("TL_Bottom"))
                                {
                                    if (TL_V_Bottom >= Rotate_MS_CN_SPEC)
                                        ROI_Chk_Spec = Color.Green;
                                    else
                                        ROI_Chk_Spec = Color.Red;
                                }
                                else if (entry.Key.Contains("TL_Left"))
                                {
                                    if (TL_H_Left >= Rotate_MS_CN_SPEC)
                                        ROI_Chk_Spec = Color.Green;
                                    else
                                        ROI_Chk_Spec = Color.Red;
                                }
                                else if (entry.Key.Contains("TR_Top"))
                                {
                                    if (TR_V_Top >= Rotate_MS_CN_SPEC)
                                        ROI_Chk_Spec = Color.Green;
                                    else
                                        ROI_Chk_Spec = Color.Red;
                                }
                                else if (entry.Key.Contains("TR_Right"))
                                {
                                    if (TR_H_Right >= Rotate_MS_CN_SPEC)
                                        ROI_Chk_Spec = Color.Green;
                                    else
                                        ROI_Chk_Spec = Color.Red;
                                }
                                else if (entry.Key.Contains("TR_Bottom"))
                                {
                                    if (TR_V_Bottom >= Rotate_MS_CN_SPEC)
                                        ROI_Chk_Spec = Color.Green;
                                    else
                                        ROI_Chk_Spec = Color.Red;
                                }
                                else if (entry.Key.Contains("TR_Left"))
                                {
                                    if (TR_H_Left >= Rotate_MS_CN_SPEC)
                                        ROI_Chk_Spec = Color.Green;
                                    else
                                        ROI_Chk_Spec = Color.Red;
                                }
                                else if (entry.Key.Contains("BL_Top"))
                                {
                                    if (BL_V_Top >= Rotate_MS_CN_SPEC)
                                        ROI_Chk_Spec = Color.Green;
                                    else
                                        ROI_Chk_Spec = Color.Red;
                                }
                                else if (entry.Key.Contains("BL_Right"))
                                {
                                    if (BL_H_Right >= Rotate_MS_CN_SPEC)
                                        ROI_Chk_Spec = Color.Green;
                                    else
                                        ROI_Chk_Spec = Color.Red;
                                }
                                else if (entry.Key.Contains("BL_Bottom"))
                                {
                                    if (BL_V_Bottom >= Rotate_MS_CN_SPEC)
                                        ROI_Chk_Spec = Color.Green;
                                    else
                                        ROI_Chk_Spec = Color.Red;
                                }
                                else if (entry.Key.Contains("BL_Left"))
                                {
                                    if (BL_H_Left >= Rotate_MS_CN_SPEC)
                                        ROI_Chk_Spec = Color.Green;
                                    else
                                        ROI_Chk_Spec = Color.Red;
                                }
                                else if (entry.Key.Contains("BR_Top"))
                                {
                                    if (BR_V_Top >= Rotate_MS_CN_SPEC)
                                        ROI_Chk_Spec = Color.Green;
                                    else
                                        ROI_Chk_Spec = Color.Red;
                                }
                                else if (entry.Key.Contains("BR_Right"))
                                {
                                    if (BR_H_Right >= Rotate_MS_CN_SPEC)
                                        ROI_Chk_Spec = Color.Green;
                                    else
                                        ROI_Chk_Spec = Color.Red;
                                }
                                else if (entry.Key.Contains("BR_Bottom"))
                                {
                                    if (BR_V_Bottom >= Rotate_MS_CN_SPEC)
                                        ROI_Chk_Spec = Color.Green;
                                    else
                                        ROI_Chk_Spec = Color.Red;
                                }
                                else if (entry.Key.Contains("BR_Left"))
                                {
                                    if (BR_H_Left >= Rotate_MS_CN_SPEC)
                                        ROI_Chk_Spec = Color.Green;
                                    else
                                        ROI_Chk_Spec = Color.Red;
                                }

                                // 解析 ROI 坐標
                                var roiCoordinates = ParseRoiCoordinates(entry.Value, outputdata["ROI_SFR_SFR_Roi_Rule"].Split(','));
                                if (roiCoordinates != null)
                                {
                                    elements.Add(new DrawElement((Rectangle)roiCoordinates, "", ROI_Chk_Spec, 34, 6f, DrawElement.ElementType.Rectangle));
                                    if(stop_flag == true)
                                        ROI_INFO.Add(entry.Key, (Rectangle)roiCoordinates);
                                }
                            }
                        }
                    }
                    if (DrawResult)
                    {
                        elements.Add(new DrawElement(
                            new Rectangle(0, 0, 1, 1),
                            strOutData,
                            Color.Blue,
                            20,
                            2.0f,
                            DrawElement.ElementType.Rectangle
                        ));
                    }
                    if (DrawCross)
                    {
                        elements.Add(new DrawElement(
                                    new Rectangle(0, 0, Image.Image_Width, Image.Image_Height),
                                    "Cross",
                                    Color.Blue,
                                    12,
                                    2.0f,
                                    DrawElement.ElementType.Cross
                                ));
                    }
                    if (DrawDiagonal)
                    {
                        elements.Add(new DrawElement(
                                    new Rectangle(0, 0, Image.Image_Width, Image.Image_Height),
                                    "Diagonal",
                                    Color.Blue,
                                    12,
                                    2.0f,
                                    DrawElement.ElementType.Diagonal
                                ));
                    }
                    if (CheckROI)
                    {
                        if (outputdata.ContainsKey("Pattern_Center_TL_Pattern_x_y") &&
                            outputdata.ContainsKey("Pattern_Center_TR_Pattern_x_y") &&
                            outputdata.ContainsKey("Pattern_Center_BL_Pattern_x_y") &&
                            outputdata.ContainsKey("Pattern_Center_BR_Pattern_x_y"))
                        {
                            if (outputdata["Pattern_Center_TL_Pattern_x_y"].Contains("-1") ||
                                outputdata["Pattern_Center_TR_Pattern_x_y"].Contains("-1") ||
                                outputdata["Pattern_Center_BL_Pattern_x_y"].Contains("-1") ||
                                outputdata["Pattern_Center_BR_Pattern_x_y"].Contains("-1"))
                            {
                                string OutputMSG = "ROI FAIL:\n" +
                                                   "TL_Pattern_x_y=" + outputdata["Pattern_Center_TL_Pattern_x_y"] + "\n" +
                                                   "TR_Pattern_x_y=" + outputdata["Pattern_Center_TR_Pattern_x_y"] + "\n" +
                                                   "BL_Pattern_x_y=" + outputdata["Pattern_Center_BL_Pattern_x_y"] + "\n" +
                                                   "BR_Pattern_x_y=" + outputdata["Pattern_Center_BR_Pattern_x_y"];
                                elements.Add(new DrawElement(
                                new Rectangle(0, 0, 1, 1),
                                OutputMSG,
                                Color.Blue,
                                52,
                                2.0f,
                                DrawElement.ElementType.Rectangle
                                ));
                                LogMessage("Can't find ROI", MessageLevel.Error);

                                if (elements.Count > 0)
                                {
                                    if (HandleDevice.DutDashboard != null)
                                    {
                                        HandleDevice.SwitchTabControlIndex(1);

                                        IQ_SingleEntry.DrawElementsOnImage(ptr, Image.Image_Width, Image.Image_Height, HandleDevice.DutDashboard.ImagePicturebox, elements);
                                    }
                                }

                                return false;
                            }

                        }
                        else
                        {
                            LogMessage("Can't find ROI Key.Check Dll path or Params", MessageLevel.Error);
                            return false;
                        }
                    }
                    if (elements.Count > 0)
                    {
                        if (HandleDevice.DutDashboard != null)
                        {
                            HandleDevice.SwitchTabControlIndex(1);
                            IQ_SingleEntry.DrawElementsOnImage(ptr, Image.Image_Width, Image.Image_Height, HandleDevice.DutDashboard.ImagePicturebox, elements);
                        }
                    }
                }

                ///LF INFO input
                LF_INFO.Add("Total Degree",  Total_Rel_Position.ToString());
                LF_INFO.Add("SFR Direction", Rotate_Dir_HV);
                LF_INFO.Add("Peak MODE", Rotate_MS_Mth);
                ///LF INFO input

                /////while Flag



                if (TCP_stop_flag)
                {
                    LogMessage("TCP Read Motor Move Done Fail", MessageLevel.Error);
                    return false;
                }

                if (Total_stop_flag)
                {
                    LogMessage("Over Focus Tune Total Degree", MessageLevel.Error);
                    return false;
                }



                ////////CT/CN/MS Curve find BP 
                //HelperFindPeak_MS(MiniScore_List, ref MS_peak_Pos, ref MS_middle_Pos, ref MS_Best_val);

                ////////CT/CN/MS Curve find BP  using dictionary
                MiniScore_List = new List<Tuple<double, double>>();

                foreach (var entry in outputdata2)
                {
                    var innerDict = entry.Value;

                    if (innerDict.ContainsKey("DAC") && innerDict.ContainsKey("MS"))
                    {
                        double dac = Convert.ToDouble(innerDict["DAC"]);
                        double ms = Convert.ToDouble(innerDict["MS"]);
                        MiniScore_List.Add(Tuple.Create(dac, ms));
                    }
                }

                if (Rotate_BP_Mth_Gus == "NO")
                    HelperFindPeak_MS(MiniScore_List, ref MS_peak_Pos, ref MS_middle_Pos, ref MS_Best_val);
                else if ((Rotate_BP_Mth_Gus == "YES"))
                {
                    HelperFindPeak_MS(MiniScore_List, ref MS_peak_Pos, ref MS_middle_Pos, ref MS_Best_val);
                    HelperFindPeak_MS_GUS(MiniScore_List, ref MS_peak_Pos_GUS, ref MS_middle_Pos_GUS, ref MS_Best_val_GUS);
                }
                ////////CT/CN/MS Curve find BP  using dictionary

                ///LF INFO input MS
                LF_INFO.Add("MS Pos(Peak,Middle)", "(" + ((int)MS_peak_Pos).ToString() + "," + ((int)MS_middle_Pos).ToString() + ")");
                if (Rotate_BP_Mth_Gus == "YES")
                    LF_INFO.Add("MS Pos Guss(Peak,Middle)", "(" +((int)MS_peak_Pos_GUS).ToString() + "," + ((int)MS_middle_Pos_GUS).ToString() + ")");
                LF_INFO.Add("DOF_Left_POS", DOF_L_Pos.ToString());
                LF_INFO.Add("DOF_Right_POS", DOF_R_Pos.ToString());
                LF_INFO.Add("DOF_RP_Minus_LP", DOF_LR_Minus_Pos.ToString());
                ///LF INFO input MS

                ///motor move to the best position//
                int motor_peak_angle = 0;

                if (Rotate_MS_Mth == "Middle")
                {
                    if (Rotate_BP_Mth_Gus == "YES")
                        Best_position = (int)MS_middle_Pos_GUS;
                    else if((Rotate_BP_Mth_Gus == "NO"))
                        Best_position = (int)MS_middle_Pos;
                }
                else
                {
                    if (Rotate_BP_Mth_Gus == "YES")
                        Best_position = (int)MS_peak_Pos_GUS;
                    else if ((Rotate_BP_Mth_Gus == "NO"))
                        Best_position = (int)MS_peak_Pos;
                }
                motor_peak_angle = (int)FineTune_Rel_Position - Best_position;
                LogMessage($"[Relative_Fine_Tune_Degree] {FineTune_Rel_Position}");
                
                //string read_tcp ="test"; // tmp test
                int count_angle = (int)(motor_peak_angle);
                int counter_record = count_angle / 20;

                if (Rotate_BP_Sep_Deg)
                {
                    for (int i = 0; i < counter_record + 1; i++)
                    {
                        MotorStepMove(1, 20, 0); // TCP Write
                    }
                }
                else
                {
                    MotorStepMove(1, count_angle, 0); // TCP Write
                }
                ///motor move to the best position//

                //// add backlash method => if backlash hvae value that can do mototr move
                MotorStepMove(1, Rotate_Step_Backlash, 0);
                MotorStepMove(0, Rotate_Step_Backlash, 0);
                //// add backlash method => if backlash hvae value that can do mototr move

                /// offset method////
                if(Rotate_Step_Offest_Dir == "Left_Offset")
                    MotorStepMove(1, Rotate_Step_Offset, 0);  //LEFT
                else
                    MotorStepMove(0, Rotate_Step_Offset, 0);  //RIGHT
                /// offset method////

                ////CT/CN List check the best postion simulation  correspond y value///////

                if (Rotate_MS_Mth == "Middle")
                {
                    CTV_AP_Yvalue = Math.Round(HelperFindPeak_Apply_Pos(CTV, (int)MS_middle_Pos), 2);
                    CTH_AP_Yvalue = Math.Round(HelperFindPeak_Apply_Pos(CTH, (int)MS_middle_Pos), 2);
                    LTV_AP_Yvalue = Math.Round(HelperFindPeak_Apply_Pos(LTV, (int)MS_middle_Pos), 2);
                    LTH_AP_Yvalue = Math.Round(HelperFindPeak_Apply_Pos(LTH, (int)MS_middle_Pos), 2);
                    RTV_AP_Yvalue = Math.Round(HelperFindPeak_Apply_Pos(RTV, (int)MS_middle_Pos), 2);
                    RTH_AP_Yvalue = Math.Round(HelperFindPeak_Apply_Pos(RTH, (int)MS_middle_Pos), 2);
                    LBV_AP_Yvalue = Math.Round(HelperFindPeak_Apply_Pos(LBV, (int)MS_middle_Pos), 2);
                    LBH_AP_Yvalue = Math.Round(HelperFindPeak_Apply_Pos(LBH, (int)MS_middle_Pos), 2);
                    RBV_AP_Yvalue = Math.Round(HelperFindPeak_Apply_Pos(RBV, (int)MS_middle_Pos), 2);
                    RBH_AP_Yvalue = Math.Round(HelperFindPeak_Apply_Pos(RBH, (int)MS_middle_Pos), 2);
                    ///LF INFO input 
                    LF_INFO.Add("CTV_Peak(Pos&Val)", "(" + ((int)MS_middle_Pos).ToString() + "," + CTV_AP_Yvalue.ToString() + ")");
                    LF_INFO.Add("CTH_Peak(Pos&Val)", "(" + ((int)MS_middle_Pos).ToString() + "," + CTH_AP_Yvalue.ToString() + ")");
                    LF_INFO.Add("LTV_Peak(Pos&Val)", "(" + ((int)MS_middle_Pos).ToString() + "," + LTV_AP_Yvalue.ToString() + ")");
                    LF_INFO.Add("LTH_Peak(Pos&Val)", "(" + ((int)MS_middle_Pos).ToString() + "," + LTH_AP_Yvalue.ToString() + ")");
                    LF_INFO.Add("RTV_Peak(Pos&Val)", "(" + ((int)MS_middle_Pos).ToString() + "," + RTV_AP_Yvalue.ToString() + ")");
                    LF_INFO.Add("RTH_Peak(Pos&Val)", "(" + ((int)MS_middle_Pos).ToString() + "," + RTH_AP_Yvalue.ToString() + ")");
                    LF_INFO.Add("LBV_Peak(Pos&Val)", "(" + ((int)MS_middle_Pos).ToString() + "," + LBV_AP_Yvalue.ToString() + ")");
                    LF_INFO.Add("LBH_Peak(Pos&Val)", "(" + ((int)MS_middle_Pos).ToString() + "," + LBH_AP_Yvalue.ToString() + ")");
                    LF_INFO.Add("RBV_Peak(Pos&Val)", "(" + ((int)MS_middle_Pos).ToString() + "," + RBV_AP_Yvalue.ToString() + ")");
                    LF_INFO.Add("RBH_Peak(Pos&Val)", "(" + ((int)MS_middle_Pos).ToString() + "," + RBH_AP_Yvalue.ToString() + ")");
                    ///LF INFO input 

                    if (Rotate_BP_Mth_Gus == "YES") 
                    {
                        CTV_AP_Yvalue_GUS = Math.Round(HelperFindPeak_Apply_Pos(CTV, (int)MS_middle_Pos_GUS), 2);
                        CTH_AP_Yvalue_GUS = Math.Round(HelperFindPeak_Apply_Pos(CTH, (int)MS_middle_Pos_GUS), 2);
                        LTV_AP_Yvalue_GUS = Math.Round(HelperFindPeak_Apply_Pos(LTV, (int)MS_middle_Pos_GUS), 2);
                        LTH_AP_Yvalue_GUS = Math.Round(HelperFindPeak_Apply_Pos(LTH, (int)MS_middle_Pos_GUS), 2);
                        RTV_AP_Yvalue_GUS = Math.Round(HelperFindPeak_Apply_Pos(RTV, (int)MS_middle_Pos_GUS), 2);
                        RTH_AP_Yvalue_GUS = Math.Round(HelperFindPeak_Apply_Pos(RTH, (int)MS_middle_Pos_GUS), 2);
                        LBV_AP_Yvalue_GUS = Math.Round(HelperFindPeak_Apply_Pos(LBV, (int)MS_middle_Pos_GUS), 2);
                        LBH_AP_Yvalue_GUS = Math.Round(HelperFindPeak_Apply_Pos(LBH, (int)MS_middle_Pos_GUS), 2);
                        RBV_AP_Yvalue_GUS = Math.Round(HelperFindPeak_Apply_Pos(RBV, (int)MS_middle_Pos_GUS), 2);
                        RBH_AP_Yvalue_GUS = Math.Round(HelperFindPeak_Apply_Pos(RBH, (int)MS_middle_Pos_GUS), 2);
                        ///LF INFO input 
                        LF_INFO.Add("CTV_Peak(Pos&Val)_GUS", "(" + ((int)MS_middle_Pos_GUS).ToString() + "," + CTV_AP_Yvalue_GUS.ToString() + ")");
                        LF_INFO.Add("CTH_Peak(Pos&Val)_GUS", "(" + ((int)MS_middle_Pos_GUS).ToString() + "," + CTH_AP_Yvalue_GUS.ToString() + ")");
                        LF_INFO.Add("LTV_Peak(Pos&Val)_GUS", "(" + ((int)MS_middle_Pos_GUS).ToString() + "," + LTV_AP_Yvalue_GUS.ToString() + ")");
                        LF_INFO.Add("LTH_Peak(Pos&Val)_GUS", "(" + ((int)MS_middle_Pos_GUS).ToString() + "," + LTH_AP_Yvalue_GUS.ToString() + ")");
                        LF_INFO.Add("RTV_Peak(Pos&Val)_GUS", "(" + ((int)MS_middle_Pos_GUS).ToString() + "," + RTV_AP_Yvalue_GUS.ToString() + ")");
                        LF_INFO.Add("RTH_Peak(Pos&Val)_GUS", "(" + ((int)MS_middle_Pos_GUS).ToString() + "," + RTH_AP_Yvalue_GUS.ToString() + ")");
                        LF_INFO.Add("LBV_Peak(Pos&Val)_GUS", "(" + ((int)MS_middle_Pos_GUS).ToString() + "," + LBV_AP_Yvalue_GUS.ToString() + ")");
                        LF_INFO.Add("LBH_Peak(Pos&Val)_GUS", "(" + ((int)MS_middle_Pos_GUS).ToString() + "," + LBH_AP_Yvalue_GUS.ToString() + ")");
                        LF_INFO.Add("RBV_Peak(Pos&Val)_GUS", "(" + ((int)MS_middle_Pos_GUS).ToString() + "," + RBV_AP_Yvalue_GUS.ToString() + ")");
                        LF_INFO.Add("RBH_Peak(Pos&Val)_GUS", "(" + ((int)MS_middle_Pos_GUS).ToString() + "," + RBH_AP_Yvalue_GUS.ToString() + ")");
                        ///LF INFO input 
                    }
                }
                else
                {
                    CTV_AP_Yvalue = Math.Round(HelperFindPeak_Apply_Pos(CTV, (int)MS_peak_Pos), 2);
                    CTH_AP_Yvalue = Math.Round(HelperFindPeak_Apply_Pos(CTH, (int)MS_peak_Pos), 2);
                    LTV_AP_Yvalue = Math.Round(HelperFindPeak_Apply_Pos(LTV, (int)MS_peak_Pos), 2);
                    LTH_AP_Yvalue = Math.Round(HelperFindPeak_Apply_Pos(LTH, (int)MS_peak_Pos), 2);
                    RTV_AP_Yvalue = Math.Round(HelperFindPeak_Apply_Pos(RTV, (int)MS_peak_Pos), 2);
                    RTH_AP_Yvalue = Math.Round(HelperFindPeak_Apply_Pos(RTH, (int)MS_peak_Pos), 2);
                    LBV_AP_Yvalue = Math.Round(HelperFindPeak_Apply_Pos(LBV, (int)MS_peak_Pos), 2);
                    LBH_AP_Yvalue = Math.Round(HelperFindPeak_Apply_Pos(LBH, (int)MS_peak_Pos), 2);
                    RBV_AP_Yvalue = Math.Round(HelperFindPeak_Apply_Pos(RBV, (int)MS_peak_Pos), 2);
                    RBH_AP_Yvalue = Math.Round(HelperFindPeak_Apply_Pos(RBH, (int)MS_peak_Pos), 2);
                    ///LF INFO input 
                    LF_INFO.Add("CTV_Peak(Pos&Val)", "(" + ((int)MS_peak_Pos).ToString() + "," + CTV_AP_Yvalue.ToString() + ")");
                    LF_INFO.Add("CTH_Peak(Pos&Val)", "(" + ((int)MS_peak_Pos).ToString() + "," + CTH_AP_Yvalue.ToString() + ")");
                    LF_INFO.Add("LTV_Peak(Pos&Val)", "(" + ((int)MS_peak_Pos).ToString() + "," + LTV_AP_Yvalue.ToString() + ")");
                    LF_INFO.Add("LTH_Peak(Pos&Val)", "(" + ((int)MS_peak_Pos).ToString() + "," + LTH_AP_Yvalue.ToString() + ")");
                    LF_INFO.Add("RTV_Peak(Pos&Val)", "(" + ((int)MS_peak_Pos).ToString() + "," + RTV_AP_Yvalue.ToString() + ")");
                    LF_INFO.Add("RTH_Peak(Pos&Val)", "(" + ((int)MS_peak_Pos).ToString() + "," + RTH_AP_Yvalue.ToString() + ")");
                    LF_INFO.Add("LBV_Peak(Pos&Val)", "(" + ((int)MS_peak_Pos).ToString() + "," + LBV_AP_Yvalue.ToString() + ")");
                    LF_INFO.Add("LBH_Peak(Pos&Val)", "(" + ((int)MS_peak_Pos).ToString() + "," + LBH_AP_Yvalue.ToString() + ")");
                    LF_INFO.Add("RBV_Peak(Pos&Val)", "(" + ((int)MS_peak_Pos).ToString() + "," + RBV_AP_Yvalue.ToString() + ")");
                    LF_INFO.Add("RBH_Peak(Pos&Val)", "(" + ((int)MS_peak_Pos).ToString() + "," + RBH_AP_Yvalue.ToString() + ")");
                    ///LF INFO input 
                    if (Rotate_BP_Mth_Gus == "YES")
                    {
                        CTV_AP_Yvalue_GUS = Math.Round(HelperFindPeak_Apply_Pos(CTV, (int)MS_peak_Pos_GUS), 2);
                        CTH_AP_Yvalue_GUS = Math.Round(HelperFindPeak_Apply_Pos(CTH, (int)MS_peak_Pos_GUS), 2);
                        LTV_AP_Yvalue_GUS = Math.Round(HelperFindPeak_Apply_Pos(LTV, (int)MS_peak_Pos_GUS), 2);
                        LTH_AP_Yvalue_GUS = Math.Round(HelperFindPeak_Apply_Pos(LTH, (int)MS_peak_Pos_GUS), 2);
                        RTV_AP_Yvalue_GUS = Math.Round(HelperFindPeak_Apply_Pos(RTV, (int)MS_peak_Pos_GUS), 2);
                        RTH_AP_Yvalue_GUS = Math.Round(HelperFindPeak_Apply_Pos(RTH, (int)MS_peak_Pos_GUS), 2);
                        LBV_AP_Yvalue_GUS = Math.Round(HelperFindPeak_Apply_Pos(LBV, (int)MS_peak_Pos_GUS), 2);
                        LBH_AP_Yvalue_GUS = Math.Round(HelperFindPeak_Apply_Pos(LBH, (int)MS_peak_Pos_GUS), 2);
                        RBV_AP_Yvalue_GUS = Math.Round(HelperFindPeak_Apply_Pos(RBV, (int)MS_peak_Pos_GUS), 2);
                        RBH_AP_Yvalue_GUS = Math.Round(HelperFindPeak_Apply_Pos(RBH, (int)MS_peak_Pos_GUS), 2);
                        ///LF INFO input 
                        LF_INFO.Add("CTV_Peak(Pos&Val)_GUS", "(" + ((int)MS_peak_Pos_GUS).ToString() + "," + CTV_AP_Yvalue_GUS.ToString() + ")");
                        LF_INFO.Add("CTH_Peak(Pos&Val)_GUS", "(" + ((int)MS_peak_Pos_GUS).ToString() + "," + CTH_AP_Yvalue_GUS.ToString() + ")");
                        LF_INFO.Add("LTV_Peak(Pos&Val)_GUS", "(" + ((int)MS_peak_Pos_GUS).ToString() + "," + LTV_AP_Yvalue_GUS.ToString() + ")");
                        LF_INFO.Add("LTH_Peak(Pos&Val)_GUS", "(" + ((int)MS_peak_Pos_GUS).ToString() + "," + LTH_AP_Yvalue_GUS.ToString() + ")");
                        LF_INFO.Add("RTV_Peak(Pos&Val)_GUS", "(" + ((int)MS_peak_Pos_GUS).ToString() + "," + RTV_AP_Yvalue_GUS.ToString() + ")");
                        LF_INFO.Add("RTH_Peak(Pos&Val)_GUS", "(" + ((int)MS_peak_Pos_GUS).ToString() + "," + RTH_AP_Yvalue_GUS.ToString() + ")");
                        LF_INFO.Add("LBV_Peak(Pos&Val)_GUS", "(" + ((int)MS_peak_Pos_GUS).ToString() + "," + LBV_AP_Yvalue_GUS.ToString() + ")");
                        LF_INFO.Add("LBH_Peak(Pos&Val)_GUS", "(" + ((int)MS_peak_Pos_GUS).ToString() + "," + LBH_AP_Yvalue_GUS.ToString() + ")");
                        LF_INFO.Add("RBV_Peak(Pos&Val)_GUS", "(" + ((int)MS_peak_Pos_GUS).ToString() + "," + RBV_AP_Yvalue_GUS.ToString() + ")");
                        LF_INFO.Add("RBH_Peak(Pos&Val)_GUS", "(" + ((int)MS_peak_Pos_GUS).ToString() + "," + RBH_AP_Yvalue_GUS.ToString() + ")");
                        ///LF INFO input 
                    }
                }

                ///// TILT Check /////
                HelperFindTilt_Labview();
                LF_INFO.Add("Tilt_X_V", Math.Round(tilt_degree_x_v,2).ToString());
                LF_INFO.Add("Tilt_X_H", Math.Round(tilt_degree_x_h, 2).ToString());
                LF_INFO.Add("Tilt_Y_V", Math.Round(tilt_degree_y_v, 2).ToString());
                LF_INFO.Add("Tilt_Y_H", Math.Round(tilt_degree_y_h, 2).ToString());
                LF_INFO.Add("Tilt_X", Math.Round(tilt_degree_x, 2).ToString());
                LF_INFO.Add("Tilt_Y", Math.Round(tilt_degree_y, 2).ToString());

                //// TEST PDF 畫DAC SFR 表格 用dictionary///////////
                DirectoryInfo di = null;
                if (!Directory.Exists(save_report_path))
                    di = Directory.CreateDirectory(save_report_path);

                string filePath = "OutputData2_SecondEntry.pdf";
                filePath = save_report_path+ReplaceProp(save_report_file);
                //WriteOutputDataToPdf(outputdata2, filePath);
                //// TEST PDF 畫DAC SFR 表格/////////


                ///TEST PDF
                // Set min/max boundary for PDF Picture

                string imagePath = "output.png";
                string imagePath2 = "secondImage.png";
                string imagePath3 = "thirdImage.png";
                string imagePath4 = "fourthImage.png";

                //// tab show foucs curve pre-process
                List<string> focus_show_curve = new List<string>();
                focus_show_curve.Add(imagePath);
                focus_show_curve.Add(imagePath2);
                focus_show_curve.Add(imagePath3);
                if (Rotate_BP_Mth_Gus == "YES")
                    focus_show_curve.Add(imagePath4);
                //// tab show foucs curve pre-process


                double minX = MiniScore_List.Min(p => p.Item1);
                double maxX = MiniScore_List.Max(p => p.Item1);

                double minY_MS = MiniScore_List.Min(p => p.Item2);
                double maxY_MS = MiniScore_List.Max(p => p.Item2);


                double minY_CTV = CTV.Min(p => p.Item2);
                double maxY_CTV = CTV.Max(p => p.Item2);
                double minY_CTH = CTH.Min(p => p.Item2);
                double maxY_CTH = CTH.Max(p => p.Item2);
                double minY_LTV = LTV.Min(p => p.Item2);
                double maxY_LTV = LTV.Max(p => p.Item2);
                double minY_LTH = LTH.Min(p => p.Item2);
                double maxY_LTH = LTH.Max(p => p.Item2);
                double minY_RTV = RTV.Min(p => p.Item2);
                double maxY_RTV = RTV.Max(p => p.Item2);
                double minY_RTH = RTH.Min(p => p.Item2);
                double maxY_RTH = RTH.Max(p => p.Item2);
                double minY_LBV = LBV.Min(p => p.Item2);
                double maxY_LBV = LBV.Max(p => p.Item2);
                double minY_LBH = LBH.Min(p => p.Item2);
                double maxY_LBH = LBH.Max(p => p.Item2);
                double minY_RBV = RBV.Min(p => p.Item2);
                double maxY_RBV = RBV.Max(p => p.Item2);
                double minY_RBH = RBH.Min(p => p.Item2);
                double maxY_RBH = RBH.Max(p => p.Item2);

                double max_CT_CN_H = Math.Max(maxY_CTH, maxY_LTH);
                max_CT_CN_H = Math.Max(max_CT_CN_H, maxY_LBH);
                max_CT_CN_H = Math.Max(max_CT_CN_H, maxY_RTH);
                max_CT_CN_H = Math.Max(max_CT_CN_H, maxY_RBH);

                double max_CT_CN_V = Math.Max(maxY_CTV, maxY_LTV);
                max_CT_CN_V = Math.Max(max_CT_CN_V, maxY_LBV);
                max_CT_CN_V = Math.Max(max_CT_CN_V, maxY_RTV);
                max_CT_CN_V = Math.Max(max_CT_CN_V, maxY_RBV);


                double min_CT_CN_H = Math.Min(minY_CTH, minY_LTH);
                min_CT_CN_H = Math.Min(min_CT_CN_H, minY_LBH);
                min_CT_CN_H = Math.Min(min_CT_CN_H, minY_RTH);
                min_CT_CN_H = Math.Min(min_CT_CN_H, minY_RBH);

                double min_CT_CN_V = Math.Min(minY_CTV, minY_LTV);
                min_CT_CN_V = Math.Min(min_CT_CN_V, minY_LBV);
                min_CT_CN_V = Math.Min(min_CT_CN_V, minY_RTV);
                min_CT_CN_V = Math.Min(min_CT_CN_V, minY_RBV);

                //mini-score plot
                // 建立 Bitmap 並畫圖
                int width = 600, height = 400;
                //int margin_I = 50;
                double margin = 80;
                float margin_F = 50;

                double scaleX = (width - 2 * margin) / (maxX - minX);
                //double scaleY_CTV = (height - 2 * margin) / (maxY_CTV - minY_CTV);
                //max/min plot
                double scaleY_max_min_V = (height - 2 * margin) / (max_CT_CN_V - min_CT_CN_V);
                double scaleY_max_min_H = (height - 2 * margin) / (max_CT_CN_H - min_CT_CN_H);

                double scaleY_MS = (height - 2 * margin) / (maxY_MS - minY_MS);
                float scaledx_bp = (float)((Best_position - minX) * scaleX + margin);
                float scaledx_dofL = (float)((DOF_L_Pos - minX) * scaleX + margin);
                float scaledx_dofR = (float)((DOF_R_Pos - minX) * scaleX + margin);


                List<PointF> scaledPoints = new List<PointF>();
                List<PointF> scaledPoints_CTV = new List<PointF>();
                List<PointF> scaledPoints_CTH = new List<PointF>();
                List<PointF> scaledPoints_LTV = new List<PointF>();
                List<PointF> scaledPoints_LTH = new List<PointF>();
                List<PointF> scaledPoints_RTV = new List<PointF>();
                List<PointF> scaledPoints_RTH = new List<PointF>();
                List<PointF> scaledPoints_LBV = new List<PointF>();
                List<PointF> scaledPoints_LBH = new List<PointF>();
                List<PointF> scaledPoints_RBV = new List<PointF>();
                List<PointF> scaledPoints_RBH = new List<PointF>();
                List<PointF> scaledPoints_MS = new List<PointF>();

                for (int i = 0; i < CTV.Count; i++)
                {

                    var CTV_pt = CTV[i];
                    var CTH_pt = CTH[i];
                    var LTV_pt = LTV[i];
                    var LTH_pt = LTH[i];
                    var RTV_pt = RTV[i];
                    var RTH_pt = RTH[i];
                    var LBV_pt = LBV[i];
                    var LBH_pt = LBH[i];
                    var RBV_pt = RBV[i];
                    var RBH_pt = RBH[i];
                    var MS_pt = MiniScore_List[i];
                    float x = (float)((CTV_pt.Item1 - minX) * scaleX + margin);
                    float y_CTV = (float)(height - ((CTV_pt.Item2 - min_CT_CN_V) * scaleY_max_min_V + margin));
                    float y_CTH = (float)(height - ((CTH_pt.Item2 - min_CT_CN_H) * scaleY_max_min_H + margin));
                    float y_LTV = (float)(height - ((LTV_pt.Item2 - min_CT_CN_V) * scaleY_max_min_V + margin));
                    float y_LTH = (float)(height - ((LTH_pt.Item2 - min_CT_CN_H) * scaleY_max_min_H + margin));
                    float y_RTV = (float)(height - ((RTV_pt.Item2 - min_CT_CN_V) * scaleY_max_min_V + margin));
                    float y_RTH = (float)(height - ((RTH_pt.Item2 - min_CT_CN_H) * scaleY_max_min_H + margin));
                    float y_LBV = (float)(height - ((LBV_pt.Item2 - min_CT_CN_V) * scaleY_max_min_V + margin));
                    float y_LBH = (float)(height - ((LBH_pt.Item2 - min_CT_CN_H) * scaleY_max_min_H + margin));
                    float y_RBV = (float)(height - ((RBV_pt.Item2 - min_CT_CN_V) * scaleY_max_min_V + margin));
                    float y_RBH = (float)(height - ((RBH_pt.Item2 - min_CT_CN_H) * scaleY_max_min_H + margin));
                    float y_MS = (float)(height - ((MS_pt.Item2 - minY_MS) * scaleY_MS + margin));
                    scaledPoints_CTV.Add(new PointF(x, y_CTV));
                    scaledPoints_CTH.Add(new PointF(x, y_CTH));
                    scaledPoints_LTV.Add(new PointF(x, y_LTV));
                    scaledPoints_LTH.Add(new PointF(x, y_LTH));
                    scaledPoints_RTV.Add(new PointF(x, y_RTV));
                    scaledPoints_RTH.Add(new PointF(x, y_RTH));
                    scaledPoints_LBV.Add(new PointF(x, y_LBV));
                    scaledPoints_LBH.Add(new PointF(x, y_LBH));
                    scaledPoints_RBV.Add(new PointF(x, y_RBV));
                    scaledPoints_RBH.Add(new PointF(x, y_RBH));
                    scaledPoints_MS.Add(new PointF(x, y_MS));
                }



                //// draw CT/CN V
                using (Bitmap bmp = new Bitmap(width, height))
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.Clear(Color.White);

                    for (int j = 0; j < scaledPoints_CTV.Count; j++)
                    {

                        g.FillEllipse(Brushes.Red, scaledPoints_CTV[j].X - 3, scaledPoints_CTV[j].Y - 3, 6, 6);
                        g.FillEllipse(Brushes.Green, scaledPoints_LTV[j].X - 3, scaledPoints_LTV[j].Y - 3, 6, 6);
                        g.FillEllipse(Brushes.SaddleBrown, scaledPoints_RTV[j].X - 3, scaledPoints_RTV[j].Y - 3, 6, 6);
                        g.FillEllipse(Brushes.RoyalBlue, scaledPoints_LBV[j].X - 3, scaledPoints_LBV[j].Y - 3, 6, 6);
                        g.FillEllipse(Brushes.Purple, scaledPoints_RBV[j].X - 3, scaledPoints_RBV[j].Y - 3, 6, 6);
                    }


                    if (scaledPoints_CTV.Count >= 3)
                    {
                        g.DrawCurve(Pens.Red, scaledPoints_CTV.ToArray(), 0.5f);
                        g.DrawCurve(Pens.Green, scaledPoints_LTV.ToArray(), 0.5f);
                        g.DrawCurve(Pens.SaddleBrown, scaledPoints_RTV.ToArray(), 0.5f);
                        g.DrawCurve(Pens.RoyalBlue, scaledPoints_LBV.ToArray(), 0.5f);
                        g.DrawCurve(Pens.Purple, scaledPoints_RBV.ToArray(), 0.5f);
                    }

                    RectangleF borderRect = new RectangleF(margin_F, margin_F, width - 2 * margin_F, height - 2 * margin_F);
                    g.DrawRectangle(Pens.Black, borderRect.X, borderRect.Y, borderRect.Width, borderRect.Height);


                    // 建立虛線畫筆
                    Pen dashedPen = new Pen(Color.Gray, 1);
                    dashedPen.DashStyle = DashStyle.Dash;

                    // 設定虛線的 X 位置（例如畫在資料區域的中間）
                    /*float dashedX = width / 2; */// 或者你可以用某個資料點的 X 值 =>example

                    // 畫從上到下的虛線
                    g.DrawLine(dashedPen, scaledx_bp, margin_F, scaledx_bp, height - margin_F);


                    // 加入左上角文字

                    string labelCTV = "CTV";
                    string labelLTV = "LTV";
                    string labelRTV = "RTV";
                    string labelLBV = "LBV";
                    string labelRBV = "RBV";
                    Font font = new Font("Arial", 5);
                    Brush brushCT = Brushes.Red;
                    Brush brushLT = Brushes.Green;
                    Brush brushRT = Brushes.SaddleBrown;
                    Brush brushLB = Brushes.RoyalBlue;
                    Brush brushRB = Brushes.Purple;

                    // 設定相同的 X 座標，讓字首對齊
                    float padding = 2f;
                    float alignedX = borderRect.X + padding;
                    PointF positionCT = new PointF(alignedX, borderRect.Y + padding);
                    PointF positionLT = new PointF(alignedX, borderRect.Y + padding + 10); // 與CTV垂直間距
                    PointF positionRT = new PointF(alignedX, borderRect.Y + padding + 20); // 與CTV垂直間距
                    PointF positionLB = new PointF(alignedX, borderRect.Y + padding + 30); // 與CTV垂直間距
                    PointF positionRB = new PointF(alignedX, borderRect.Y + padding + 40); // 與CTV垂直間距

                    // 繪製文字
                    g.DrawString(labelCTV, font, brushCT, positionCT);
                    g.DrawString(labelLTV, font, brushLT, positionLT);
                    g.DrawString(labelRTV, font, brushRT, positionRT);
                    g.DrawString(labelLBV, font, brushLB, positionLB);
                    g.DrawString(labelRBV, font, brushRB, positionRB);

                    Font labelFont = new Font("Arial", 6);
                    Brush labelBrush = Brushes.DarkGray;


                    // 設定要顯示的 X 軸刻度數量，例如 10 個
                    int xTickCount = 5;
                    double xStep = (maxX - minX) / (xTickCount - 1);

                    for (int i = 0; i < xTickCount; i++)
                    {
                        double xValue = minX + i * xStep;
                        float xPos = (float)((xValue - minX) * scaleX + margin);
                        string label = xValue.ToString("0");

                        // 顯示在圖像底部
                        PointF labelPos = new PointF(xPos - 10, height - margin_F + 2);
                        g.DrawString(label, labelFont, labelBrush, labelPos);

                        //// 可選：加上垂直輔助線
                        //Pen tickPen = new Pen(Color.LightGray, 1) { DashStyle = DashStyle.Dot };
                        //g.DrawLine(tickPen, xPos, margin_F, xPos, height - margin_F);
                    }




                    // 設定要顯示的 Y 軸刻度數量，例如 10 個
                    int yTickCount = 5;
                    double yStep = (max_CT_CN_V - min_CT_CN_V) / (yTickCount - 1);

                    for (int i = 0; i < yTickCount; i++)
                    {
                        double yValue = min_CT_CN_V + i * yStep;
                        float yPos = (float)(height - ((yValue - min_CT_CN_V) * scaleY_max_min_V + margin));
                        string label = yValue.ToString("0");


                        // 顯示在圖像左側
                        PointF labelPos = new PointF(margin_F - 25, yPos - 6);
                        g.DrawString(label, labelFont, labelBrush, labelPos);

                    }

                    // 加入 X 軸標籤文字 "(DAC)"
                    string xAxisLabel = "(DAC)";
                    Font xAxisFont = new Font("Arial", 5);
                    Brush xAxisBrush = Brushes.Black;

                    // 計算文字位置：置中於圖像底部
                    SizeF labelSize = g.MeasureString(xAxisLabel, xAxisFont);
                    float xLabelX = (width - labelSize.Width) / 2;
                    float xLabelY = height - margin_F + 15; // 可依需要微調 Y 值

                    g.DrawString(xAxisLabel, xAxisFont, xAxisBrush, new PointF(xLabelX, xLabelY));


                    // 加入 Y 軸標籤文字 "(MS)"
                    string yAxisLabel = "(SFR)";
                    Font yAxisFont = new Font("Arial", 5);
                    Brush yAxisBrush = Brushes.Black;

                    // 旋轉文字用的格式
                    StringFormat format = new StringFormat();
                    format.FormatFlags = StringFormatFlags.DirectionVertical;

                    // 計算文字位置：置中於 Y 軸左側
                    SizeF yLabelSize = g.MeasureString(yAxisLabel, yAxisFont);
                    float yLabelX = margin_F - 35; // 可依需要微調
                    float yLabelY = (height - yLabelSize.Width) / 2;

                    g.DrawString(yAxisLabel, yAxisFont, yAxisBrush, new PointF(yLabelX, yLabelY), format);

                    bmp.Save(imagePath, ImageFormat.Png);

                }
                
                //// draw CT/CN H
                using (Bitmap bmp = new Bitmap(width, height))
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.Clear(Color.White);

                    for (int j = 0; j < scaledPoints_CTV.Count; j++)
                    {

                        g.FillEllipse(Brushes.Red, scaledPoints_CTH[j].X - 3, scaledPoints_CTH[j].Y - 3, 6, 6);
                        g.FillEllipse(Brushes.Green, scaledPoints_LTH[j].X - 3, scaledPoints_LTH[j].Y - 3, 6, 6);
                        g.FillEllipse(Brushes.SaddleBrown, scaledPoints_RTH[j].X - 3, scaledPoints_RTH[j].Y - 3, 6, 6);
                        g.FillEllipse(Brushes.RoyalBlue, scaledPoints_LBH[j].X - 3, scaledPoints_LBH[j].Y - 3, 6, 6);
                        g.FillEllipse(Brushes.Purple, scaledPoints_RBH[j].X - 3, scaledPoints_RBH[j].Y - 3, 6, 6);
                    }


                    if (scaledPoints_CTV.Count >= 3)
                    {
                        g.DrawCurve(Pens.Red, scaledPoints_CTH.ToArray(), 0.5f);
                        g.DrawCurve(Pens.Green, scaledPoints_LTH.ToArray(), 0.5f);
                        g.DrawCurve(Pens.SaddleBrown, scaledPoints_RTH.ToArray(), 0.5f);
                        g.DrawCurve(Pens.RoyalBlue, scaledPoints_LBH.ToArray(), 0.5f);
                        g.DrawCurve(Pens.Purple, scaledPoints_RBH.ToArray(), 0.5f);
                    }

                    RectangleF borderRect = new RectangleF(margin_F, margin_F, width - 2 * margin_F, height - 2 * margin_F);
                    g.DrawRectangle(Pens.Black, borderRect.X, borderRect.Y, borderRect.Width, borderRect.Height);


                    // 建立虛線畫筆
                    Pen dashedPen = new Pen(Color.Gray, 1);
                    dashedPen.DashStyle = DashStyle.Dash;

                    // 設定虛線的 X 位置（例如畫在資料區域的中間）
                    //float dashedX = width / 2; // 或者你可以用某個資料點的 X 值

                    // 畫從上到下的虛線
                    g.DrawLine(dashedPen, scaledx_bp, margin_F, scaledx_bp, height - margin_F);



                    // 加入左上角文字

                    string labelCTH = "CTH";
                    string labelLTH = "LTH";
                    string labelRTH = "RTH";
                    string labelLBH = "LBH";
                    string labelRBH = "RBH";
                    Font font = new Font("Arial", 5);
                    Brush brushCT = Brushes.Red;
                    Brush brushLT = Brushes.Green;
                    Brush brushRT = Brushes.SaddleBrown;
                    Brush brushLB = Brushes.RoyalBlue;
                    Brush brushRB = Brushes.Purple;

                    // 設定相同的 X 座標，讓字首對齊
                    float padding = 2f;
                    float alignedX = borderRect.X + padding;
                    PointF positionCT = new PointF(alignedX, borderRect.Y + padding);
                    PointF positionLT = new PointF(alignedX, borderRect.Y + padding + 10); // 與CTV垂直間距
                    PointF positionRT = new PointF(alignedX, borderRect.Y + padding + 20); // 與CTV垂直間距
                    PointF positionLB = new PointF(alignedX, borderRect.Y + padding + 30); // 與CTV垂直間距
                    PointF positionRB = new PointF(alignedX, borderRect.Y + padding + 40); // 與CTV垂直間距

                    // 繪製文字
                    g.DrawString(labelCTH, font, brushCT, positionCT);
                    g.DrawString(labelLTH, font, brushLT, positionLT);
                    g.DrawString(labelRTH, font, brushRT, positionRT);
                    g.DrawString(labelLBH, font, brushLB, positionLB);
                    g.DrawString(labelRBH, font, brushRB, positionRB);

                    Font labelFont = new Font("Arial", 6);
                    Brush labelBrush = Brushes.DarkGray;


                    // 設定要顯示的 X 軸刻度數量，例如 10 個
                    int xTickCount = 5;
                    double xStep = (maxX - minX) / (xTickCount - 1);

                    for (int i = 0; i < xTickCount; i++)
                    {
                        double xValue = minX + i * xStep;
                        float xPos = (float)((xValue - minX) * scaleX + margin);
                        string label = xValue.ToString("0");

                        // 顯示在圖像底部
                        PointF labelPos = new PointF(xPos - 10, height - margin_F + 2);
                        g.DrawString(label, labelFont, labelBrush, labelPos);

                        //// 可選：加上垂直輔助線
                        //Pen tickPen = new Pen(Color.LightGray, 1) { DashStyle = DashStyle.Dot };
                        //g.DrawLine(tickPen, xPos, margin_F, xPos, height - margin_F);
                    }




                    // 設定要顯示的 Y 軸刻度數量，例如 10 個
                    int yTickCount = 5;
                    double yStep = (max_CT_CN_H - min_CT_CN_H) / (yTickCount - 1);

                    for (int i = 0; i < yTickCount; i++)
                    {
                        double yValue = min_CT_CN_H + i * yStep;
                        float yPos = (float)(height - ((yValue - min_CT_CN_H) * scaleY_max_min_H + margin));
                        string label = yValue.ToString("0");


                        // 顯示在圖像左側
                        PointF labelPos = new PointF(margin_F - 25, yPos - 6);
                        g.DrawString(label, labelFont, labelBrush, labelPos);

                    }

                    // 加入 X 軸標籤文字 "(DAC)"
                    string xAxisLabel = "(DAC)";
                    Font xAxisFont = new Font("Arial", 5);
                    Brush xAxisBrush = Brushes.Black;

                    // 計算文字位置：置中於圖像底部
                    SizeF labelSize = g.MeasureString(xAxisLabel, xAxisFont);
                    float xLabelX = (width - labelSize.Width) / 2;
                    float xLabelY = height - margin_F + 15; // 可依需要微調 Y 值

                    g.DrawString(xAxisLabel, xAxisFont, xAxisBrush, new PointF(xLabelX, xLabelY));


                    // 加入 Y 軸標籤文字 "(MS)"
                    string yAxisLabel = "(SFR)";
                    Font yAxisFont = new Font("Arial", 5);
                    Brush yAxisBrush = Brushes.Black;

                    // 旋轉文字用的格式
                    StringFormat format = new StringFormat();
                    format.FormatFlags = StringFormatFlags.DirectionVertical;

                    // 計算文字位置：置中於 Y 軸左側
                    SizeF yLabelSize = g.MeasureString(yAxisLabel, yAxisFont);
                    float yLabelX = margin_F - 35; // 可依需要微調
                    float yLabelY = (height - yLabelSize.Width) / 2;

                    g.DrawString(yAxisLabel, yAxisFont, yAxisBrush, new PointF(yLabelX, yLabelY), format);



                    bmp.Save(imagePath2, ImageFormat.Png);
                }

                //// draw MS
                using (Bitmap bmp = new Bitmap(width, height))
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.Clear(Color.White);

                    for (int j = 0; j < scaledPoints_CTV.Count; j++)
                    {

                        
                        g.FillEllipse(Brushes.Black, scaledPoints_MS[j].X - 3, scaledPoints_MS[j].Y - 3, 6, 6);
                       
                    }


                    if (scaledPoints_CTV.Count >= 3)
                    {
                       
                        g.DrawCurve(Pens.Black, scaledPoints_MS.ToArray(), 0.5f);
                        
                    }

                    RectangleF borderRect = new RectangleF(margin_F, margin_F, width - 2 * margin_F, height - 2 * margin_F);
                    g.DrawRectangle(Pens.Black, borderRect.X, borderRect.Y, borderRect.Width, borderRect.Height);


                    // 建立虛線畫筆
                    Pen dashedPen = new Pen(Color.Gray, 1);
                    dashedPen.DashStyle = DashStyle.Dash;

                    // 設定虛線的 X 位置（例如畫在資料區域的中間）
                    //float dashedX = width / 2; // 或者你可以用某個資料點的 X 值

                    // 畫從上到下的虛線
                    g.DrawLine(dashedPen, scaledx_bp, margin_F, scaledx_bp, height - margin_F);

                    dashedPen = new Pen(Color.Blue, 1);
                    dashedPen.DashStyle = DashStyle.Dash;

                    // 畫從左到右的虛線（水平線）
                    float y_MS_1value = (float)(height - ((1 - minY_MS) * scaleY_MS + margin));
                    g.DrawLine(dashedPen, margin_F, y_MS_1value, width - margin_F, y_MS_1value);

                    dashedPen = new Pen(Color.Green, 1);
                    dashedPen.DashStyle = DashStyle.Dash;
                    g.DrawLine(dashedPen, scaledx_dofL, margin_F, scaledx_dofL, height - margin_F);
                    g.DrawLine(dashedPen, scaledx_dofR, margin_F, scaledx_dofR, height - margin_F);

                    // 加入左上角文字
                    string MS = "MS";
                    string MS_1value = "MS=1";
                    string MS_Range = "DOF_Range";

                    Font font = new Font("Arial", 5);
                    Brush brush = Brushes.Black;
                    Brush brush_MS1 = Brushes.Blue;
                    Brush brush_MS_Range = Brushes.Green;

                    // 設定相同的 X 座標，讓字首對齊
                    float padding = 2f;
                    float alignedX = borderRect.X + padding;
                    PointF position = new PointF(alignedX, borderRect.Y + padding);
                    PointF position_MS1 = new PointF(alignedX, borderRect.Y + padding+10);
                    PointF position_MSRange = new PointF(alignedX, borderRect.Y + padding+20);

                    // 繪製文字
                    g.DrawString(MS, font, brush, position);
                    g.DrawString(MS_1value, font, brush_MS1, position_MS1);
                    g.DrawString(MS_Range, font, brush_MS_Range, position_MSRange);



                    Font labelFont = new Font("Arial", 6);
                    Brush labelBrush = Brushes.DarkGray;


                    // 設定要顯示的 X 軸刻度數量，例如 10 個
                    int xTickCount = 5;
                    double xStep = (maxX - minX) / (xTickCount - 1);

                    for (int i = 0; i < xTickCount; i++)
                    {
                        double xValue = minX + i * xStep;
                        float xPos = (float)((xValue - minX) * scaleX + margin);
                        string label = xValue.ToString("0");

                        // 顯示在圖像底部
                        PointF labelPos = new PointF(xPos - 10, height - margin_F + 2);
                        g.DrawString(label, labelFont, labelBrush, labelPos);

                        //// 可選：加上垂直輔助線
                        //Pen tickPen = new Pen(Color.LightGray, 1) { DashStyle = DashStyle.Dot };
                        //g.DrawLine(tickPen, xPos, margin_F, xPos, height - margin_F);
                    }




                    int minIndexY = MiniScore_List.FindIndex(p => p.Item2 == MiniScore_List.Min(q => q.Item2));
                    int maxIndexY = MiniScore_List.FindIndex(p => p.Item2 == MiniScore_List.Max(q => q.Item2));


                    // 設定要顯示的 Y 軸刻度數量，例如 10 個
                    int yTickCount = 5;
                    double yStep = (maxY_MS - minY_MS) / (yTickCount - 1);

                    for (int i = 0; i < yTickCount; i++)
                    {
                        double yValue = minY_MS + i * yStep;
                        float yPos = (float)(height - ((yValue - minY_MS) * scaleY_MS + margin));
                        string label = yValue.ToString("0.##");
                        

                        // 顯示在圖像左側
                        PointF labelPos = new PointF(margin_F - 25, yPos - 6);
                        g.DrawString(label, labelFont, labelBrush, labelPos);

                        //// 可選：加上水平輔助線
                        //Pen tickPen = new Pen(Color.LightGray, 1) { DashStyle = DashStyle.Dot };
                        //g.DrawLine(tickPen, margin_F, yPos, width - margin_F, yPos);
                    }

                    // 加入 X 軸標籤文字 "(DAC)"
                    string xAxisLabel = "(DAC)";
                    Font xAxisFont = new Font("Arial", 5);
                    Brush xAxisBrush = Brushes.Black;

                    // 計算文字位置：置中於圖像底部
                    SizeF labelSize = g.MeasureString(xAxisLabel, xAxisFont);
                    float xLabelX = (width - labelSize.Width) / 2;
                    float xLabelY = height - margin_F + 15; // 可依需要微調 Y 值

                    g.DrawString(xAxisLabel, xAxisFont, xAxisBrush, new PointF(xLabelX, xLabelY));



                    // 加入 Y 軸標籤文字 "(MS)"
                    string yAxisLabel = "(MS)";
                    Font yAxisFont = new Font("Arial", 5);
                    Brush yAxisBrush = Brushes.Black;

                    // 旋轉文字用的格式
                    StringFormat format = new StringFormat();
                    format.FormatFlags = StringFormatFlags.DirectionVertical;

                    // 計算文字位置：置中於 Y 軸左側
                    SizeF yLabelSize = g.MeasureString(yAxisLabel, yAxisFont);
                    float yLabelX = margin_F - 35; // 可依需要微調
                    float yLabelY = (height - yLabelSize.Width) / 2;

                    g.DrawString(yAxisLabel, yAxisFont, yAxisBrush, new PointF(yLabelX, yLabelY), format);

                    bmp.Save(imagePath3, ImageFormat.Png);
                }


                ///// gus_setting from ms
                if (Rotate_BP_Mth_Gus == "YES")
                {

                    double minX_g = MiniScore_List_Guss.Min(p => p.Item1);
                    double maxX_g = MiniScore_List_Guss.Max(p => p.Item1);
                    double minY_MS_g = MiniScore_List_Guss.Min(p => p.Item2);
                    double maxY_MS_g = MiniScore_List_Guss.Max(p => p.Item2);
                    double scaleX_g = (width - 2 * margin) / (maxX_g - minX_g);
                    double scaleY_MS_g = (height - 2 * margin) / (maxY_MS_g - minY_MS_g);
                    float scaledx_bp_g = (float)((Best_position - minX_g) * scaleX_g + margin);
                    float scaledx_dofL_g = (float)((DOF_L_Pos - minX_g) * scaleX_g + margin);
                    float scaledx_dofR_g = (float)((DOF_R_Pos - minX_g) * scaleX_g + margin);
                    List<PointF> scaledPoints_MS_g = new List<PointF>();
                    for (int i = 0; i < MiniScore_List_Guss.Count; i++)
                    {
                        var MS_pt_g = MiniScore_List_Guss[i];
                        float x_g = (float)((MS_pt_g.Item1 - minX_g) * scaleX_g + margin);
                        float y_MS_g = (float)(height - ((MS_pt_g.Item2 - minY_MS_g) * scaleY_MS_g + margin));
                        scaledPoints_MS_g.Add(new PointF(x_g, y_MS_g));
                    }
                    ///// gus_setting from ms

                    //// draw MS_GUS
                    using (Bitmap bmp = new Bitmap(width, height))
                    using (Graphics g = Graphics.FromImage(bmp))
                    {
                        g.SmoothingMode = SmoothingMode.AntiAlias;
                        g.Clear(Color.White);

                        for (int j = 0; j < scaledPoints_CTV.Count; j++)
                        {


                            g.FillEllipse(Brushes.Black, scaledPoints_MS[j].X - 3, scaledPoints_MS[j].Y - 3, 6, 6);

                        }


                        if (scaledPoints_CTV.Count >= 3)
                        {

                            g.DrawCurve(Pens.Black, scaledPoints_MS_g.ToArray(), 0.5f);

                        }

                        RectangleF borderRect = new RectangleF(margin_F, margin_F, width - 2 * margin_F, height - 2 * margin_F);
                        g.DrawRectangle(Pens.Black, borderRect.X, borderRect.Y, borderRect.Width, borderRect.Height);


                        // 建立虛線畫筆
                        Pen dashedPen = new Pen(Color.Gray, 1);
                        dashedPen.DashStyle = DashStyle.Dash;

                        // 設定虛線的 X 位置（例如畫在資料區域的中間）
                        //float dashedX = width / 2; // 或者你可以用某個資料點的 X 值

                        // 畫從上到下的虛線
                        g.DrawLine(dashedPen, scaledx_bp_g, margin_F, scaledx_bp_g, height - margin_F);

                        dashedPen = new Pen(Color.Blue, 1);
                        dashedPen.DashStyle = DashStyle.Dash;

                        // 畫從左到右的虛線（水平線）
                        float y_MS_1value = (float)(height - ((1 - minY_MS_g) * scaleY_MS_g + margin));
                        g.DrawLine(dashedPen, margin_F, y_MS_1value, width - margin_F, y_MS_1value);

                        dashedPen = new Pen(Color.Green, 1);
                        dashedPen.DashStyle = DashStyle.Dash;
                        g.DrawLine(dashedPen, scaledx_dofL_g, margin_F, scaledx_dofL_g, height - margin_F);
                        g.DrawLine(dashedPen, scaledx_dofR_g, margin_F, scaledx_dofR_g, height - margin_F);

                        // 加入左上角文字
                        string MS = "MS_Guss";
                        string MS_1value = "MS=1";
                        string MS_Range = "DOF_Range";

                        Font font = new Font("Arial", 5);
                        Brush brush = Brushes.Black;
                        Brush brush_MS1 = Brushes.Blue;
                        Brush brush_MS_Range = Brushes.Green;

                        // 設定相同的 X 座標，讓字首對齊
                        float padding = 2f;
                        float alignedX = borderRect.X + padding;
                        PointF position = new PointF(alignedX, borderRect.Y + padding);
                        PointF position_MS1 = new PointF(alignedX, borderRect.Y + padding + 10);
                        PointF position_MSRange = new PointF(alignedX, borderRect.Y + padding + 20);

                        // 繪製文字
                        g.DrawString(MS, font, brush, position);
                        g.DrawString(MS_1value, font, brush_MS1, position_MS1);
                        g.DrawString(MS_Range, font, brush_MS_Range, position_MSRange);





                        Font labelFont = new Font("Arial", 6);
                        Brush labelBrush = Brushes.DarkGray;

                        //// 第一筆資料
                        //int firstIndex = 0;
                        //var firstPt = scaledPoints_MS[firstIndex];
                        //string firstLabel = MiniScore_List[firstIndex].Item1.ToString("0.##");
                        //PointF firstLabelPos = new PointF(firstPt.X - 10, height - margin_F + 2);
                        //g.DrawString(firstLabel, labelFont, labelBrush, firstLabelPos);

                        //// 最後一筆資料
                        //int lastIndex = scaledPoints_MS.Count-1 ;
                        //var lastPt = scaledPoints_MS[lastIndex];
                        //string lastLabel = MiniScore_List[lastIndex].Item1.ToString("0.##");
                        //PointF lastLabelPos = new PointF(lastPt.X - 10, height - margin_F + 2);
                        //g.DrawString(lastLabel, labelFont, labelBrush, lastLabelPos);


                        // 設定要顯示的 X 軸刻度數量，例如 10 個
                        int xTickCount = 5;
                        double xStep = (maxX_g - minX_g) / (xTickCount - 1);

                        for (int i = 0; i < xTickCount; i++)
                        {
                            double xValue = minX_g + i * xStep;
                            float xPos = (float)((xValue - minX_g) * scaleX_g + margin);
                            string label = xValue.ToString("0");

                            // 顯示在圖像底部
                            PointF labelPos = new PointF(xPos - 10, height - margin_F + 2);
                            g.DrawString(label, labelFont, labelBrush, labelPos);

                            //// 可選：加上垂直輔助線
                            //Pen tickPen = new Pen(Color.LightGray, 1) { DashStyle = DashStyle.Dot };
                            //g.DrawLine(tickPen, xPos, margin_F, xPos, height - margin_F);
                        }




                        int minIndexY = MiniScore_List.FindIndex(p => p.Item2 == MiniScore_List.Min(q => q.Item2));
                        int maxIndexY = MiniScore_List.FindIndex(p => p.Item2 == MiniScore_List.Max(q => q.Item2));


                        // Y 軸標示（最小與最大）
                        //PointF ptMinY = scaledPoints_MS[minIndexY];
                        //PointF ptMaxY = scaledPoints_MS[maxIndexY];
                        //string labelMinY = MiniScore_List[minIndexY].Item2.ToString("0.##");
                        //string labelMaxY = MiniScore_List[maxIndexY].Item2.ToString("0.##");
                        //g.DrawString(labelMinY, labelFont, labelBrush, new PointF(margin_F - 25, ptMinY.Y - 6));
                        //g.DrawString(labelMaxY, labelFont, labelBrush, new PointF(margin_F - 25, ptMaxY.Y - 6));



                        // 設定要顯示的 Y 軸刻度數量，例如 10 個
                        int yTickCount = 5;
                        double yStep = (maxY_MS_g - minY_MS_g) / (yTickCount - 1);

                        for (int i = 0; i < yTickCount; i++)
                        {
                            double yValue = minY_MS_g + i * yStep;
                            float yPos = (float)(height - ((yValue - minY_MS_g) * scaleY_MS_g + margin));
                            string label = yValue.ToString("0.##");

                            // 顯示在圖像左側
                            PointF labelPos = new PointF(margin_F - 25, yPos - 6);
                            g.DrawString(label, labelFont, labelBrush, labelPos);

                            //// 可選：加上水平輔助線
                            //Pen tickPen = new Pen(Color.LightGray, 1) { DashStyle = DashStyle.Dot };
                            //g.DrawLine(tickPen, margin_F, yPos, width - margin_F, yPos);
                        }

                        // 加入 X 軸標籤文字 "(DAC)"
                        string xAxisLabel = "(DAC)";
                        Font xAxisFont = new Font("Arial", 5);
                        Brush xAxisBrush = Brushes.Black;

                        // 計算文字位置：置中於圖像底部
                        SizeF labelSize = g.MeasureString(xAxisLabel, xAxisFont);
                        float xLabelX = (width - labelSize.Width) / 2;
                        float xLabelY = height - margin_F + 15; // 可依需要微調 Y 值

                        g.DrawString(xAxisLabel, xAxisFont, xAxisBrush, new PointF(xLabelX, xLabelY));



                        // 加入 Y 軸標籤文字 "(MS)"
                        string yAxisLabel = "(MS)";
                        Font yAxisFont = new Font("Arial", 5);
                        Brush yAxisBrush = Brushes.Black;

                        // 旋轉文字用的格式
                        StringFormat format = new StringFormat();
                        format.FormatFlags = StringFormatFlags.DirectionVertical;

                        // 計算文字位置：置中於 Y 軸左側
                        SizeF yLabelSize = g.MeasureString(yAxisLabel, yAxisFont);
                        float yLabelX = margin_F - 35; // 可依需要微調
                        float yLabelY = (height - yLabelSize.Width) / 2;

                        g.DrawString(yAxisLabel, yAxisFont, yAxisBrush, new PointF(yLabelX, yLabelY), format);


                        bmp.Save(imagePath4, ImageFormat.Png);
                    }
                }

                //// output sfr curve temp test///
                //string test1 ="";
                // foreach (var entry in outputdata2)
                // {
                //     string dacKey = entry.Key;
                //     string line = string.Join(", ", entry.Value.Select(kvp => $"{kvp.Key}: {kvp.Value}"));
                //     test1 += line+"\n\n";
                //     Console.WriteLine($"DAC {dacKey} => {line}");
                // }
                // //string json_data =$"\n{ \}";
                // var data = new Dictionary<string, object>
                //         {
                //             { "STATUS", $"\n{test1}\n" }
                //         };
                //string output = JsonConvert.SerializeObject(data, Formatting.Indented);
                // String jsonStr = output;
                //// output sfr curve temp test///

                /////ADD Process END TIME//////


                //// tab show focus curve
                if(Show_Tab)
                    HandleDevice.DutDashboard.ShowImagesInTab("Focus_Curve", focus_show_curve);

                // 紀錄測試結束時間
                DateTime endTime = DateTime.Now;
                LogMessage("FocusTune測試結束時間: " + endTime.ToString("HH:mm:ss.fff"));

                // 計算時間差
                TimeSpan duration = endTime - startTime;
                LogMessage("FocusTune測試耗時: " + duration.TotalSeconds + " sec");
                LF_INFO.Add("FT_Time(s)", ((int)duration.TotalSeconds).ToString());
                /////ADD Process END TIME//////

                String jsonStr = JsonConvert.SerializeObject(outputdata2, Formatting.Indented);
                String jsonStr_LFinfo = JsonConvert.SerializeObject(LF_INFO, Formatting.Indented);
                ////test pdf json/////

                PdfDocument document = new PdfDocument();
                if (outputdata2.Count > 0)
                {
                    // write cuve data
                    document = WriteOutputDataToPdf_Json(document, jsonStr);

                    /// write related  LF info
                    document = DrawVerticalSectionsToPdf(document, LF_INFO);

                    // draw curve data
                    XGraphics gfx = XGraphics.FromPdfPage(document.AddPage());
                    XImage image = XImage.FromFile(imagePath);
                    XImage image2 = XImage.FromFile(imagePath2);
                    XImage image3 = XImage.FromFile(imagePath3);
                    XImage image4 = XImage.FromFile(imagePath4);
                    double yOffset = image.PixelHeight; // 第一張圖的高度
                    gfx.DrawImage(image, 0, 0, image.PixelWidth, image.PixelHeight);
                    gfx.DrawImage(image2, 0, yOffset, image2.PixelWidth, image2.PixelHeight);
                    gfx = XGraphics.FromPdfPage(document.AddPage());
                    gfx.DrawImage(image3, 0, 0, image3.PixelWidth, image3.PixelHeight);
                    if(Rotate_BP_Mth_Gus == "YES")
                        gfx.DrawImage(image4, 0, yOffset, image3.PixelWidth, image4.PixelHeight);

                    document.Save(filePath);
                    
                }
                document.Close();
                ////test pdf json to draw/////
                strOutData = jsonStr_LFinfo;  // add param in report without DAC & SFR Value;
                strstringoutput = strOutData;
            }
            catch (Exception ex)
            {
                LogMessage($"Error For SFR Capture Calculate: {ex.Message}", MessageLevel.Error);
                return false;
            }


            return true;
        }
        public override bool PostProcess()
        {
            if (Spec != string.Empty && Spec != null)
            {
                string ret = string.Empty;             
                ret = CheckRule(strstringoutput, Spec);
                LogMessage($"CheckRule: {ret}", MessageLevel.Debug);
                if (ret == "PASS")
                    return true;
                else
                    return false;
            }
            return true;

        }




        //public static IntPtr ConvertBitmapToIntPtr(Bitmap bitmap)
        //{
        //    // 鎖定 Bitmap 的像素資料
        //    BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);

        //    // 取得指向像素資料的指標
        //    IntPtr ptr = bitmapData.Scan0;

        //    // 解鎖 Bitmap 的像素資料
        //    bitmap.UnlockBits(bitmapData);

        //    return ptr;
        //}


        //public static IntPtr ConvertByteArrayToIntPtr(byte[] byteArray)
        //{
        //    // Allocate unmanaged memory for the byte buffer.
        //    IntPtr ptr = Marshal.AllocHGlobal(byteArray.Length);

        //    // Copy the byte buffer to the unmanaged memory.
        //    Marshal.Copy(byteArray, 0, ptr, byteArray.Length);

        //    return ptr;
        //}



        //public static byte[] ConvertIntPtrToByteArray(IntPtr ptr, int length)
        //{
        //    // 創建 byte 陣列來存儲資料
        //    byte[] byteArray = new byte[length];

        //    // 使用 Marshal.Copy 方法將資料從 IntPtr 複製到 byte 陣列
        //    Marshal.Copy(ptr, byteArray, 0, length);

        //    return byteArray;
        //}


        //public static Bitmap CreateBitmapFromIntPtr(IntPtr ptr, int width, int height, PixelFormat pixelFormat)
        //{
        //    // Calculate the stride (width * bytes per pixel)
        //    int stride = width * Image.GetPixelFormatSize(pixelFormat) / 8;

        //    // Create a Bitmap from the IntPtr
        //    return new Bitmap(width, height, stride, pixelFormat, ptr);
        //}

        public class MotionDeviceList : TypeConverter  //下拉式選單
        {
            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {

                if (GlobalNew.Devices.Count != 0)
                {
                    List<string> hwListKeys = new List<string>();

                    hwListKeys.Add("");
                    hwListKeys.AddRange(GlobalNew.Devices
                        .Where(item => item.Value is MotionBase)
                        .Select(item => item.Key)
                        .ToList()
                        );
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
                                if (GlobalNew.Devices[deviceObject] is MotionBase)
                                    hwListKeys.Add($"@{(string)item["SharedName"]}@");
                            }
                        }
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

        public class Focus_Tune_Offset : TypeConverter  //下拉式選單
        {
            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                List<string> FT_Offset = new List<string>();

                FT_Offset.Add("Left_Offset");
                FT_Offset.Add("Right_Offset");

                return new StandardValuesCollection(FT_Offset);
            }

            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                return true;
            }
        }

        public class Focus_Tune_Direction_HV : TypeConverter  //下拉式選單
        {
            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                List<string> FT_Dir_HV = new List<string>();

                FT_Dir_HV.Add("H&V");
                FT_Dir_HV.Add("V");
                FT_Dir_HV.Add("H");
                return new StandardValuesCollection(FT_Dir_HV);
            }

            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                return true;
            }
        }

        public class Focus_Tune_BP_Method : TypeConverter  //下拉式選單
        {
            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                List<string> FT_BP_Mth = new List<string>();
                FT_BP_Mth.Add("MiniScore");
                return new StandardValuesCollection(FT_BP_Mth);
            }

            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                return true;
            }
        }

        public class Focus_Tune_MS_Method : TypeConverter  //下拉式選單
        {
            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                List<string> FT_MS_Mth = new List<string>();
                FT_MS_Mth.Add("Middle");
                FT_MS_Mth.Add("Peak");
                return new StandardValuesCollection(FT_MS_Mth);
            }

            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                return true;
            }
        }

        public class Focus_Tune_Reverse_Enable : TypeConverter  //下拉式選單
        {
            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                List<string> FT_Rev_En = new List<string>();
                FT_Rev_En.Add("Disable");
                FT_Rev_En.Add("Enable");
                return new StandardValuesCollection(FT_Rev_En);
            }

            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                return true;
            }
        }

        public class Focus_Tune_Simu_Motor : TypeConverter  //下拉式選單
        {
            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                List<string> FT_Simu_M = new List<string>();
                FT_Simu_M.Add("Disable");
                FT_Simu_M.Add("Enable");
                return new StandardValuesCollection(FT_Simu_M);
            }

            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                return true;
            }
        }

        public class Focus_Tune_Gussian_Method : TypeConverter  //下拉式選單
        {
            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                List<string> FT_Guss_M = new List<string>();
                FT_Guss_M.Add("NO");
                FT_Guss_M.Add("YES");
                return new StandardValuesCollection(FT_Guss_M);
            }

            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                return true;
            }
        }

        public class SaveImage : TypeConverter  //下拉式選單
        {
            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                List<string> Img_Save = new List<string>();

                Img_Save.Add("NO");
                Img_Save.Add("YES");

                return new StandardValuesCollection(Img_Save);
            }

            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                return true;
            }
        }

        public class Focus_Tune_Field : TypeConverter  //下拉式選單
        {
            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                List<string> FT_Field = new List<string>();
                FT_Field.Add("Inter");
                FT_Field.Add("Outer");
                return new StandardValuesCollection(FT_Field);
            }

            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                return true;
            }
        }

        public void HelperFindPeak(List<Tuple<double, double>> Data, ref double max_position ,ref double max_value)
        {
            if (Data.Count < 3)
            {
                LogMessage("Warn! There is no enough data in the input list", MessageLevel.Warn);
                return;
            }

            double i = 0;
            var x = Data.Select(p => p.Item1).ToArray();
            var y = Data.Select(p => p.Item2).ToArray();
            var spline = CubicSpline.InterpolateNatural(x, y);

            max_position = 0;
            max_value = 0;
            for (i = Data.First().Item1; i< Data.Last().Item1;i++ ) 
            {
                if(spline.Interpolate(i)> max_value)
                {
                    max_position = i;
                    max_value = spline.Interpolate(i);
                }
            }
            //double interpolatedValue = spline.Interpolate(81);
        }
        public void HelperFindPeak_MS(List<Tuple<double, double>> Data, ref double max_position_peak, ref double max_position_middle, ref double max_value)
        {
            if (Data.Count < 3)
            {
                LogMessage("Warn! There is no enough data in the input list", MessageLevel.Warn);
                return;
            }

            double i = 0;
            var x = Data.Select(p => p.Item1).ToArray();
            var y = Data.Select(p => p.Item2).ToArray();
            var spline = CubicSpline.InterpolateNatural(x, y);

            int dof_count = 0;
            double Left_POS =  0;
            double Right_POS = 0;
            double max_position=0;
            max_position_peak = 0;
            max_position_middle = 0;
            

            max_value = 0;
            for (i = Data.First().Item1; i < Data.Last().Item1; i++)
            {
                if (spline.Interpolate(i) > max_value)
                {
                    max_position = i;
                    max_value = spline.Interpolate(i);
                }
                
                if (spline.Interpolate(i) >= 1) 
                {
                    max_position_middle += i;
                    dof_count++;
                }
                //Record L R Pos
                if (dof_count == 1)
                    Left_POS = max_position_middle;
            }

            if (dof_count > 0)
            {
                Right_POS = Left_POS + dof_count - 1;
                max_position_middle = (int)(max_position_middle / dof_count);
                max_position_peak = max_position;
            }
            else
            {
                max_position_peak = max_position;
                max_position_middle = max_position;
            }
            DOF_L_Pos = Left_POS;
            DOF_R_Pos = Right_POS;
            DOF_LR_Minus_Pos = Right_POS - Left_POS;

        }

        public void HelperFindPeak_MS_GUS(List<Tuple<double, double>> Data, ref double max_position_peak, ref double max_position_middle, ref double max_value)
        {
            if (Data.Count < 3)
            {
                LogMessage("Warn! There is no enough data in the input list", MessageLevel.Warn);
                return;
            }


            //guss method//
            FitResultC Best_Position;
            int gus_len = Data.Select(t => t.Item2).ToArray().Length;
            bool BP_Chk = curve_fit_gaussian_c(Data.Select(t => t.Item1).ToArray(), Data.Select(t => t.Item2).ToArray(), gus_len, out Best_Position);
           
            double A = Best_Position.A;
            double mu = Best_Position.mu;
            double sigma = Best_Position.sigma;
            double b = Best_Position.baseline;
            double gus_x = mu;
            double gus_y = 0;


            // guss method//

            double i = 0;
            int dof_count = 0;
            double Left_POS = 0;
            double Right_POS = 0;
            double max_position = 0;
            max_position_peak = 0;
            max_position_middle = 0;


           

           
            max_value = 0;
            for (i = Data.First().Item1; i < Data.Last().Item1; i++)
            {
                gus_y = A * Math.Exp(-Math.Pow(i - mu, 2) / (2 * Math.Pow(sigma, 2))) + b;
                MiniScore_List_Guss.Add(Tuple.Create(i, gus_y));
                if (gus_y > max_value)
                {
                    max_position = i;
                    max_value = gus_y;
                }

                if (gus_y >= 1)
                {
                    max_position_middle += i;
                    dof_count++;
                }
                //Record L R Pos
                if (dof_count == 1)
                    Left_POS = max_position_middle;
            }

            if (dof_count > 0)
            {
                Right_POS = Left_POS + dof_count - 1;
                max_position_middle = (int)(max_position_middle / dof_count);
                max_position_peak = max_position;
            }
            else
            {
                max_position_peak = max_position;
                max_position_middle = max_position;
            }
            DOF_L_Pos = Left_POS;
            DOF_R_Pos = Right_POS;
            DOF_LR_Minus_Pos = Right_POS - Left_POS;

        }
        public double HelperFindPeak_Apply_Pos(List<Tuple<double, double>> Data,  double Position)
        {
            if (Data.Count < 3)
            {
                LogMessage("Warn! There is no enough data in the input list", MessageLevel.Warn);
                return 0;
            }

            double i = 0;
            var x = Data.Select(p => p.Item1).ToArray();
            var y = Data.Select(p => p.Item2).ToArray();
            var spline = CubicSpline.InterpolateNatural(x, y);

            double pos_value = spline.Interpolate(Position);

            return pos_value;
           
        }

        public double HelperFindPeak_Apply_Pos_GUS(List<Tuple<double, double>> Data, double Position)
        {
            if (Data.Count < 3)
            {
                LogMessage("Warn! There is no enough data in the input list", MessageLevel.Warn);
                return 0;
            }

            //guss method//
            FitResultC Best_Position;
            int gus_len = Data.Select(t => t.Item2).ToArray().Length;
            bool BP_Chk = curve_fit_gaussian_c(Data.Select(t => t.Item1).ToArray(), Data.Select(t => t.Item2).ToArray(), gus_len, out Best_Position);

            double A = Best_Position.A;
            double mu = Best_Position.mu;
            double sigma = Best_Position.sigma;
            double b = Best_Position.baseline;
            double gus_x = mu;
            double gus_y = 0;
            // guss method//


            gus_y = A * Math.Exp(-Math.Pow(Position - mu, 2) / (2 * Math.Pow(sigma, 2))) + b;
            double pos_value = gus_y;

            return pos_value;

        }

        public void MotorStepMove (int Direction, int step_deg, int rev_step_deg)
        {
            string motor_move = null;

            if (Direction == 0)
                motor_move = "++" + step_deg.ToString();
            else if(Direction == 1)
                motor_move = "--" + step_deg.ToString();

            if (Rotate_Simu_Motor == "Enable")
            {
                MotionCtrlDevice.SetCommand(motor_move); // TCP Write
                MotorStepMove_Done();
            }
        }

        public void MotorStepMove_Done()
        {

            string TCP_Read = "";
            MotionCtrlDevice.READ(ref TCP_Read);
            if (TCP_Read.Contains("OK"))
                stop_flag = false;
            else
            {
                stop_flag = true;
                TCP_stop_flag = true;
            }
        }


        public static void WriteOutputDataToPdf(Dictionary<string, Dictionary<string, object>> outputdata2, string filePath)
        {
            if (outputdata2.Count == 0)
                return;

            PdfDocument document = new PdfDocument();
            document.Info.Title = "SFR Data Table";

            PdfPage page = document.AddPage();
            XGraphics gfx = XGraphics.FromPdfPage(page);
            XFont headerFont = new XFont("Verdana", 9, XFontStyle.Bold); // 粗體字
            XFont dataFont = new XFont("Verdana", 9, XFontStyle.Regular);

            double marginLeft = 40;
            double marginTop = 40;
            double lineHeight = 20;
            double pageWidth = page.Width.Point - marginLeft * 2;

            var headers = outputdata2.First().Value.Keys.ToList();
            int columnCount = headers.Count;
            double columnWidth = pageWidth / columnCount;

            double x = marginLeft;
            double y = marginTop;

            // 第一行：藍色背景 + 藍色框線 + 粗體字
            foreach (var header in headers)
            {
                // 填滿藍色背景
                gfx.DrawRectangle(XBrushes.LightBlue, x, y, columnWidth, lineHeight);
                gfx.DrawRectangle(new XPen(XColors.Black, 1), x, y, columnWidth, lineHeight);
                // 畫文字（粗體）
                gfx.DrawString(header, headerFont, XBrushes.Black, new XRect(x + 2, y + 2, columnWidth - 4, lineHeight - 4), XStringFormats.TopLeft);
                x += columnWidth;
            }

            y += lineHeight;
            x = marginLeft;

            // 資料列：黑色框線 + 一般字體
            foreach (var entry in outputdata2)
            {
                foreach (var header in headers)
                {
                    var value = entry.Value.ContainsKey(header) ? entry.Value[header].ToString() : "";
                    gfx.DrawRectangle(XPens.Black, x, y, columnWidth, lineHeight);
                    gfx.DrawString(value, dataFont, XBrushes.Black, new XRect(x + 2, y + 2, columnWidth - 4, lineHeight - 4), XStringFormats.TopLeft);
                    x += columnWidth;
                }

                y += lineHeight;
                x = marginLeft;

                // 換頁判斷
                if (y + lineHeight > page.Height.Point - marginTop)
                {
                    page = document.AddPage();
                    gfx = XGraphics.FromPdfPage(page);
                    y = marginTop;
                }
            }

            document.Save(filePath);
            document.Close();
        }


        public static PdfDocument WriteOutputDataToPdf_Json(PdfDocument document, string jsonStr)
        {
            // 解析 JSON 字串為 JObject
            var root = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, object>>>(jsonStr);
            if (root == null || root.Count == 0)
                return null;

            //PdfDocument document = new PdfDocument();
            document.Info.Title = "SFR Data Table from JSON";

            PdfPage page =  document.AddPage();
            XGraphics gfx = XGraphics.FromPdfPage(page);
            XFont titleFont = new XFont("Verdana", 14, XFontStyle.Bold); // 粗體標題
            XFont headerFont = new XFont("Verdana", 9, XFontStyle.Bold);
            XFont dataFont = new XFont("Verdana", 9, XFontStyle.Regular);

            double marginLeft = 40;
            double marginTop = 40;
            double lineHeight = 20;
            double pageWidth = page.Width.Point - marginLeft * 2;

            var headers = root.First().Value.Keys;
            int columnCount = headers.Count;
            double columnWidth = pageWidth / columnCount;

            double x = marginLeft;
            double y = marginTop;


            // 畫出標題 LF DATA
            gfx.DrawString("LF DATA", titleFont, XBrushes.Black, new XRect(marginLeft, y, page.Width - marginLeft * 2, lineHeight), XStringFormats.TopLeft);
            y += lineHeight + 10; // 標題下方留空間


            // 第一行：藍色背景 + 藍色框線 + 粗體字
            foreach (var header in headers)
            {
                gfx.DrawRectangle(XBrushes.LightBlue, x, y, columnWidth, lineHeight);
                gfx.DrawRectangle(new XPen(XColors.Black, 1), x, y, columnWidth, lineHeight);
                gfx.DrawString(header, headerFont, XBrushes.Black, new XRect(x + 2, y + 2, columnWidth - 4, lineHeight - 4), XStringFormats.TopLeft);
                x += columnWidth;
            }

            y += lineHeight;
            x = marginLeft;

            // 資料列
            foreach (var entry in root)
            {
                foreach (var header in headers)
                {
                    var value = entry.Value.ContainsKey(header) ? entry.Value[header]?.ToString() ?? "" : "";
                    gfx.DrawRectangle(XPens.Black, x, y, columnWidth, lineHeight);
                    gfx.DrawString(value, dataFont, XBrushes.Black, new XRect(x + 2, y + 2, columnWidth - 4, lineHeight - 4), XStringFormats.TopLeft);
                    x += columnWidth;
                }

                y += lineHeight;
                x = marginLeft;

                // 換頁判斷
                if (y + lineHeight > page.Height.Point - marginTop)
                {
                    page = document.AddPage();
                    gfx = XGraphics.FromPdfPage(page);
                    y = marginTop;
                }
            }
            return document;

        }



        ///寫取資料進入pdf 方法(表格型式)
        public PdfDocument DrawVerticalSectionsToPdf(PdfDocument document,Dictionary<string, string> sections)
        {
            
            document.Info.Title = "Vertical Layout Report";

            PdfPage page = document.AddPage();
            XGraphics gfx = XGraphics.FromPdfPage(page);
            XFont font = new XFont("Arial", 12, XFontStyle.Regular);

            //float marginTop = 50;
            //float marginLeft = 50;
            //float lineHeight = 20;
            //float y = marginTop;
            //float bottomMargin = 50;
            //float pageHeight = (float)page.Height;


            //foreach (var section in sections)
            //{


            //    // 預估這段會用到的高度（最多 5 行）
            //    int lines = 3 + (string.IsNullOrWhiteSpace(section.Value) ? 0 : 1) + 1;
            //    float requiredHeight = lines * lineHeight;

            //    // 若超出頁面高度，則換頁
            //    if (y + requiredHeight > pageHeight - bottomMargin)
            //    {
            //        page = document.AddPage();
            //        gfx = XGraphics.FromPdfPage(page);
            //        y = marginTop;
            //    }

            //    gfx.DrawString("-------------------", font, XBrushes.Black, new XPoint(marginLeft, y));
            //    y += lineHeight;

            //    gfx.DrawString(section.Key, font, XBrushes.Black, new XPoint(marginLeft, y));
            //    y += lineHeight;

            //    gfx.DrawString("-------------------", font, XBrushes.Black, new XPoint(marginLeft, y));
            //    y += lineHeight;

            //    if (!string.IsNullOrWhiteSpace(section.Value))
            //    {
            //        gfx.DrawString(section.Value, font, XBrushes.Black, new XPoint(marginLeft, y));
            //        y += lineHeight;
            //    }

            //    gfx.DrawString("-------------------", font, XBrushes.Black, new XPoint(marginLeft, y));
            //    y += lineHeight;
            //}






            XFont fontRegular = new XFont("Arial", 12, XFontStyle.Regular);
            XFont fontBold = new XFont("Arial", 12, XFontStyle.Bold);

            float marginLeft = 50;
            float marginTop = 50;
            float rowHeight = 30;
            float colWidthTitle = 150;
            float colWidthValue = 300;
            float bottomMargin = 50;
            float y = marginTop;

            XPen borderPen = new XPen(XColors.Black, 0.5);
            borderPen.DashStyle = XDashStyle.Solid;

            // 畫表格標題列
            gfx.DrawRectangle(XBrushes.LightBlue, marginLeft, y, colWidthTitle + colWidthValue, rowHeight);
            gfx.DrawRectangle(borderPen, marginLeft, y, colWidthTitle, rowHeight);
            gfx.DrawRectangle(borderPen, marginLeft + colWidthTitle, y, colWidthValue, rowHeight);
            
            gfx.DrawString("Section", fontBold, XBrushes.Black,
            new XRect(marginLeft, y, colWidthTitle, rowHeight), XStringFormats.Center);
            gfx.DrawString("Value", fontBold, XBrushes.Black,
            new XRect(marginLeft + colWidthTitle, y, colWidthValue, rowHeight), XStringFormats.Center);
            y += rowHeight;

            int rowIndex = 0;
            foreach (var section in sections)
            {
                // 換頁判斷
                if (y + rowHeight > page.Height - bottomMargin)
                {
                    page = document.AddPage();
                    gfx = XGraphics.FromPdfPage(page);
                    y = marginTop;

                    // 重畫表頭
                    gfx.DrawRectangle(XBrushes.LightBlue, marginLeft, y, colWidthTitle + colWidthValue, rowHeight);
                    gfx.DrawRectangle(borderPen, marginLeft, y, colWidthTitle, rowHeight);
                    gfx.DrawRectangle(borderPen, marginLeft + colWidthTitle, y, colWidthValue, rowHeight);
                    gfx.DrawString("Section", fontBold, XBrushes.Black,
                    new XRect(marginLeft, y, colWidthTitle, rowHeight), XStringFormats.Center);
                    gfx.DrawString("Value", fontBold, XBrushes.Black,
                    new XRect(marginLeft + colWidthTitle, y, colWidthValue, rowHeight), XStringFormats.Center);
                    y += rowHeight;
                }

                // 交錯背景色
                //XBrush background = (rowIndex % 2 == 0) ? XBrushes.LightBlue : new XSolidBrush(XColor.FromArgb(240, 240, 240));
                XBrush background = XBrushes.White;
                gfx.DrawRectangle(background, marginLeft, y, colWidthTitle + colWidthValue, rowHeight);

                // 畫儲存格邊框
                gfx.DrawRectangle(borderPen, marginLeft, y, colWidthTitle, rowHeight);
                gfx.DrawRectangle(borderPen, marginLeft + colWidthTitle, y, colWidthValue, rowHeight);

                // 畫文字
                gfx.DrawString(section.Key, fontRegular, XBrushes.Black,
         new XRect(marginLeft, y, colWidthTitle, rowHeight), XStringFormats.Center);
                gfx.DrawString(section.Value ?? "", fontRegular, XBrushes.Black,
                new XRect(marginLeft + colWidthTitle, y, colWidthValue, rowHeight), XStringFormats.Center);

                y += rowHeight;
                rowIndex++;
            }










            return document;
            
        }

        /// 文字換行方法////
        public List<string> WrapText(XGraphics gfx, string text, XFont font, float maxWidth)
        {
            List<string> lines = new List<string>();
            string[] words = text.Split(' ');
            string currentLine = "";

            foreach (var word in words)
            {
                string testLine = string.IsNullOrEmpty(currentLine) ? word : currentLine + " " + word;
                XSize size = gfx.MeasureString(testLine, font);
                if (size.Width > maxWidth)
                {
                    if (!string.IsNullOrEmpty(currentLine))
                        lines.Add(currentLine);
                    currentLine = word;
                }
                else
                {
                    currentLine = testLine;
                }
            }

            if (!string.IsNullOrEmpty(currentLine))
                lines.Add(currentLine);

            return lines;
        }


        /// <summary>
        /// Tilt計算方法，基於C++版本的HelperFindTilt_Labview移植
        /// </summary>
        public void HelperFindTilt_Labview()
        {
            try
            {
               

                // 取得各區域的peak位置和值
                double max_lb_pos_V = 0, max_lt_pos_V  =0, max_rt_pos_V = 0, max_rb_pos_V = 0;
                double max_lb_pos_H = 0, max_lt_pos_H  =0, max_rt_pos_H = 0, max_rb_pos_H = 0;
                double max_lb_val_V = 0 , max_lt_val_V =0, max_rt_val_V = 0, max_rb_val_V = 0;
                double max_lb_val_H = 0, max_lt_val_H  =0, max_rt_val_H = 0, max_rb_val_H = 0;
                
                Rectangle LT_V =new Rectangle() , LT_H = new Rectangle(), RT_V = new Rectangle(), RT_H = new Rectangle(),LB_V = new Rectangle(),LB_H = new Rectangle(),RB_V = new Rectangle(),RB_H = new Rectangle();


                // 從現有的SFR資料中找出各區域的peak位置
                if (LBV.Count > 0) HelperFindPeak(LBV, ref max_lb_pos_V, ref max_lb_val_V);
                if (LTV.Count > 0) HelperFindPeak(LTV, ref max_lt_pos_V, ref max_lt_val_V);
                if (RTV.Count > 0) HelperFindPeak(RTV, ref max_rt_pos_V, ref max_rt_val_V);
                if (RBV.Count > 0) HelperFindPeak(RBV, ref max_rb_pos_V, ref max_rb_val_V);

                if (LBH.Count > 0) HelperFindPeak(LBH, ref max_lb_pos_H, ref max_lb_val_H);
                if (LTH.Count > 0) HelperFindPeak(LTH, ref max_lt_pos_H, ref max_lt_val_H);
                if (RTH.Count > 0) HelperFindPeak(RTH, ref max_rt_pos_H, ref max_rt_val_H);
                if (RBH.Count > 0) HelperFindPeak(RBH, ref max_rb_pos_H, ref max_rb_val_H);

                // 從 ROI_INFO 中解析各區域的 Rectangle
                if (ROI_INFO != null && ROI_INFO.Count > 0)
                {
                    foreach (var entry in ROI_INFO)
                    {
                        string key = entry.Key;
                        Rectangle roi = entry.Value;

                        // 根據 Focus_Field 設定選擇對應的 ROI
                        if (Focus_Field == "Inter")
                        {
                            if (key.Contains("TL_Bottom"))
                                LT_V = roi;
                            else if (key.Contains("TL_Right"))
                                LT_H = roi;
                            else if (key.Contains("TR_Bottom"))
                                RT_V = roi;
                            else if ( key.Contains("TR_Left"))
                                RT_H = roi;
                            else if (key.Contains("BL_Top"))
                                LB_V = roi;
                            else if (key.Contains("BL_Right"))
                                LB_H = roi;
                            else if (key.Contains("BR_Top"))
                                RB_V = roi;
                            else if (key.Contains("BR_Left"))
                                RB_H = roi;
                        }
                        else if (Focus_Field == "Outer")
                        {
                            if (key.Contains("TL_Top"))
                                LT_V = roi;
                            else if ( key.Contains("TL_Left"))
                                LT_H = roi;
                            else if (key.Contains("TR_Top"))
                                RT_V = roi;
                            else if (key.Contains("TR_Right"))
                                RT_H = roi;
                            else if ( key.Contains("BL_Bottom"))
                                LB_V = roi;
                            else if ( key.Contains("BL_Left"))
                                LB_H = roi;
                            else if ( key.Contains("BR_Bottom"))
                                RB_V = roi;
                            else if (key.Contains("BR_Right"))
                                RB_H = roi;
                        }
                    }
                }


                var ct_lb_pixel_V = ParseCoordinates(LB_V);
                var ct_lt_pixel_V = ParseCoordinates(LT_V);
                var ct_rt_pixel_V = ParseCoordinates(RT_V);
                var ct_rb_pixel_V = ParseCoordinates(RB_V);

                var ct_lb_pixel_H = ParseCoordinates(LB_H);
                var ct_lt_pixel_H = ParseCoordinates(LT_H);
                var ct_rt_pixel_H = ParseCoordinates(RT_H);
                var ct_rb_pixel_H = ParseCoordinates(RB_H);


                

                // 像素轉實際距離 (mm)
                double pixel_size_um = Pixel_Size;
                double motor_a_pulsemm = Rotate_Step_Position; // (mm/deg)

                // V方向計算
                var ct_lb_real_V = new Tuple<double, double>(
                    ct_lb_pixel_V.Item1 * pixel_size_um / 1000.0,
                    ct_lb_pixel_V.Item2 * pixel_size_um / 1000.0);
                var ct_lt_real_V = new Tuple<double, double>(
                    ct_lt_pixel_V.Item1 * pixel_size_um / 1000.0,
                    ct_lt_pixel_V.Item2 * pixel_size_um / 1000.0);
                var ct_rt_real_V = new Tuple<double, double>(
                    ct_rt_pixel_V.Item1 * pixel_size_um / 1000.0,
                    ct_rt_pixel_V.Item2 * pixel_size_um / 1000.0);
                var ct_rb_real_V = new Tuple<double, double>(
                    ct_rb_pixel_V.Item1 * pixel_size_um / 1000.0,
                    ct_rb_pixel_V.Item2 * pixel_size_um / 1000.0);

                double L_T_B_VX = (ct_lt_real_V.Item1 + ct_lb_real_V.Item1) / 2;
                double L_T_B_VY = (ct_lt_real_V.Item2 + ct_lb_real_V.Item2) / 2;
                double L_T_B_VZ = ((max_lt_pos_V / 10.0) * motor_a_pulsemm + (max_lb_pos_V / 10.0) * motor_a_pulsemm) / 2;

                double R_T_B_VX = (ct_rt_real_V.Item1 + ct_rb_real_V.Item1) / 2;
                double R_T_B_VY = (ct_rt_real_V.Item2 + ct_rb_real_V.Item2) / 2;
                double R_T_B_VZ = ((max_rt_pos_V / 10.0) * motor_a_pulsemm + (max_rb_pos_V / 10.0) * motor_a_pulsemm) / 2;

                double T_L_R_VX = (ct_lt_real_V.Item1 + ct_rt_real_V.Item1) / 2;
                double T_L_R_VY = (ct_lt_real_V.Item2 + ct_rt_real_V.Item2) / 2;
                double T_L_R_VZ = ((max_lt_pos_V / 10.0) * motor_a_pulsemm + (max_rt_pos_V / 10.0) * motor_a_pulsemm) / 2;

                double B_L_R_VX = (ct_lb_real_V.Item1 + ct_rb_real_V.Item1) / 2;
                double B_L_R_VY = (ct_lb_real_V.Item2 + ct_rb_real_V.Item2) / 2;
                double B_L_R_VZ = ((max_lb_pos_V / 10.0) * motor_a_pulsemm + (max_rb_pos_V / 10.0) * motor_a_pulsemm) / 2;

                double VTX = Math.Atan((T_L_R_VZ - B_L_R_VZ) / (B_L_R_VY - T_L_R_VY)) * (180.0 / Math.PI);
                double VTY = Math.Atan((L_T_B_VZ - R_T_B_VZ) / (R_T_B_VX - L_T_B_VX)) * (180.0 / Math.PI);

                // H方向計算
                var ct_lb_real_H = new Tuple<double, double>(
                    ct_lb_pixel_H.Item1 * pixel_size_um / 1000.0,
                    ct_lb_pixel_H.Item2 * pixel_size_um / 1000.0);
                var ct_lt_real_H = new Tuple<double, double>(
                    ct_lt_pixel_H.Item1 * pixel_size_um / 1000.0,
                    ct_lt_pixel_H.Item2 * pixel_size_um / 1000.0);
                var ct_rt_real_H = new Tuple<double, double>(
                    ct_rt_pixel_H.Item1 * pixel_size_um / 1000.0,
                    ct_rt_pixel_H.Item2 * pixel_size_um / 1000.0);
                var ct_rb_real_H = new Tuple<double, double>(
                    ct_rb_pixel_H.Item1 * pixel_size_um / 1000.0,
                    ct_rb_pixel_H.Item2 * pixel_size_um / 1000.0);

                double L_T_B_HX = (ct_lt_real_H.Item1 + ct_lb_real_H.Item1) / 2;
                double L_T_B_HY = (ct_lt_real_H.Item2 + ct_lb_real_H.Item2) / 2;
                double L_T_B_HZ = ((max_lt_pos_H / 10.0) * motor_a_pulsemm + (max_lb_pos_H / 10.0) * motor_a_pulsemm) / 2;

                double R_T_B_HX = (ct_rt_real_H.Item1 + ct_rb_real_H.Item1) / 2;
                double R_T_B_HY = (ct_rt_real_H.Item2 + ct_rb_real_H.Item2) / 2;
                double R_T_B_HZ = ((max_rt_pos_H / 10.0) * motor_a_pulsemm + (max_rb_pos_H / 10.0) * motor_a_pulsemm) / 2;

                double T_L_R_HX = (ct_lt_real_H.Item1 + ct_rt_real_H.Item1) / 2;
                double T_L_R_HY = (ct_lt_real_H.Item2 + ct_rt_real_H.Item2) / 2;
                double T_L_R_HZ = ((max_lt_pos_H / 10.0) * motor_a_pulsemm + (max_rt_pos_H / 10.0) * motor_a_pulsemm) / 2;

                double B_L_R_HX = (ct_lb_real_H.Item1 + ct_rb_real_H.Item1) / 2;
                double B_L_R_HY = (ct_lb_real_H.Item2 + ct_rb_real_H.Item2) / 2;
                double B_L_R_HZ = ((max_lb_pos_H / 10.0) * motor_a_pulsemm + (max_rb_pos_H / 10.0) * motor_a_pulsemm) / 2;

                double HTX = Math.Atan((T_L_R_HZ - B_L_R_HZ) / (B_L_R_HY - T_L_R_HY)) * (180.0 / Math.PI);
                double HTY = Math.Atan((L_T_B_HZ - R_T_B_HZ) / (R_T_B_HX - L_T_B_HX)) * (180.0 / Math.PI);

                // 保存結果
                tilt_degree_x_v = VTX;
                tilt_degree_y_v = VTY;
                tilt_degree_x_h = HTX;
                tilt_degree_y_h = HTY;
                tilt_degree_x = (tilt_degree_x_v + tilt_degree_x_h) / 2;
                tilt_degree_y = (tilt_degree_y_v + tilt_degree_y_h) / 2;

                LogMessage($"Tilt calculation completed: X={tilt_degree_x:F3}°, Y={tilt_degree_y:F3}°", MessageLevel.Info);
            }
            catch (Exception ex)
            {
                LogMessage($"Error in tilt calculation: {ex.Message}", MessageLevel.Error);
            }
        }

     
        

        private Tuple<double, double> ParseCoordinates(Rectangle rect)
{
    try
    {
        // 計算 Rectangle 的中心點
        double centerX = rect.X + rect.Width / 2.0;
        double centerY = rect.Y + rect.Height / 2.0;
        
        LogMessage($"ParseCoordinates: Rectangle({rect.X}, {rect.Y}, {rect.Width}, {rect.Height}) -> Center({centerX:F1}, {centerY:F1})", MessageLevel.Debug);
        
        return new Tuple<double, double>(centerX, centerY);
    }
    catch (Exception ex)
    {
        LogMessage($"Error parsing Rectangle coordinates: {ex.Message}", MessageLevel.Error);
        return new Tuple<double, double>(0, 0);
    }
}


    }



}






    //class ImageTestValue
    //{
    //    public object value;
       
    //}

    //class ImageInfo 
    //{
    //    public string DAC;
    //    Dictionary<string, ImageTestValue> SFR_Info;

    //}

