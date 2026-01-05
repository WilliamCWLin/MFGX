using AutoTestSystem.Base;
using AutoTestSystem.Equipment.Motion;
using AutoTestSystem.Model;
using MathNet.Numerics.Interpolation;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

///TEST PDF
using PdfSharp.Drawing;
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
using static AutoTestSystem.Model.IQ_SingleEntry;

///TEST PDF

namespace AutoTestSystem.Script
{
    internal class Script_Image_Focus_Tune_SFR_Gaussian : Script_Image_Base
    {
        private string strOutData = string.Empty;

        public MotionBase MotionDevice = null;

        //改成下面這個//
        [JsonIgnore]
        [Browsable(false)]
        private TCP MotionCtrlDevice = null;//先寫死

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

        // MTF 數據結構
        public class MTFData
        {
            public double CT_V_Top { get; set; }
            public double CT_H_Right { get; set; }
            public double CT_V_Bottom { get; set; }
            public double CT_H_Left { get; set; }
            public double TL_V_Top { get; set; }
            public double TL_H_Right { get; set; }
            public double TL_V_Bottom { get; set; }
            public double TL_H_Left { get; set; }
            public double TR_V_Top { get; set; }
            public double TR_H_Right { get; set; }
            public double TR_V_Bottom { get; set; }
            public double TR_H_Left { get; set; }
            public double BL_V_Top { get; set; }
            public double BL_H_Right { get; set; }
            public double BL_V_Bottom { get; set; }
            public double BL_H_Left { get; set; }
            public double BR_V_Top { get; set; }
            public double BR_H_Right { get; set; }
            public double BR_V_Bottom { get; set; }
            public double BR_H_Left { get; set; }

            // 計算屬性
            public double CT_V => (CT_V_Top + CT_V_Bottom) / 2;

            public double CT_H => (CT_H_Right + CT_H_Left) / 2;
            public double CenterMinMTF => Math.Min(CT_V, CT_H);
        }

        public class TestLoopData
        {
            public Dictionary<string, string> OutputData { get; set; } = new Dictionary<string, string>();
            public Dictionary<string, string> LF_INFO { get; set; } = new Dictionary<string, string>();
            public Dictionary<string, Dictionary<string, object>> OutputData2 { get; set; } = new Dictionary<string, Dictionary<string, object>>();
            public Dictionary<string, object> SfrData { get; set; } = new Dictionary<string, object>();
            public List<DrawElement> Elements { get; set; } = new List<DrawElement>();
            public IntPtr ImagePtr { get; set; }
            public bool HasValidData { get; set; } = false;
        }

        public class StepResult
        {
            public bool Success { get; set; }
            public MTFData MTFData { get; set; }
            public double MinScore { get; set; }
            public double CenterMinMTFSpec { get; set; }
            public double CornerMinMTFSpec { get; set; }
            public string ErrorMessage { get; set; }
            public IntPtr ImagePtr { get; set; }
            public string ImageAddress { get; set; }
        }

        public class AnalysisResult
        {
            public Dictionary<string, string> LF_INFO { get; set; } = new Dictionary<string, string>();
            public Dictionary<string, Dictionary<string, object>> OutputData2 { get; set; } = new Dictionary<string, Dictionary<string, object>>();
            public List<string> FocusCurveImages { get; set; } = new List<string>();
            public string JsonOutput { get; set; }
            public TimeSpan ProcessingTime { get; set; }
        }

        ///Best Postion apply pos to get sfr value param
        private double CTV_AP_Yvalue, CTH_AP_Yvalue, LTV_AP_Yvalue, LTH_AP_Yvalue, RTV_AP_Yvalue, RTH_AP_Yvalue, LBV_AP_Yvalue, LBH_AP_Yvalue, RBV_AP_Yvalue, RBH_AP_Yvalue;

        private double CTV_AP_Yvalue_GUS, CTH_AP_Yvalue_GUS, LTV_AP_Yvalue_GUS, LTH_AP_Yvalue_GUS, RTV_AP_Yvalue_GUS, RTH_AP_Yvalue_GUS, LBV_AP_Yvalue_GUS, LBH_AP_Yvalue_GUS, RBV_AP_Yvalue_GUS, RBH_AP_Yvalue_GUS;

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

        private static readonly Dictionary<string, string> MTFKeyMapping = new Dictionary<string, string>
        {
            ["CT_V_Top"] = "SFR_SFR_CT_Top",
            ["CT_H_Right"] = "SFR_SFR_CT_Right",
            ["CT_V_Bottom"] = "SFR_SFR_CT_Bottom",
            ["CT_H_Left"] = "SFR_SFR_CT_Left",
            ["TL_V_Top"] = "SFR_SFR_TL_Top",
            ["TL_H_Right"] = "SFR_SFR_TL_Right",
            ["TL_V_Bottom"] = "SFR_SFR_TL_Bottom",
            ["TL_H_Left"] = "SFR_SFR_TL_Left",
            ["TR_V_Top"] = "SFR_SFR_TR_Top",
            ["TR_H_Right"] = "SFR_SFR_TR_Right",
            ["TR_V_Bottom"] = "SFR_SFR_TR_Bottom",
            ["TR_H_Left"] = "SFR_SFR_TR_Left",
            ["BL_V_Top"] = "SFR_SFR_BL_Top",
            ["BL_H_Right"] = "SFR_SFR_BL_Right",
            ["BL_V_Bottom"] = "SFR_SFR_BL_Bottom",
            ["BL_H_Left"] = "SFR_SFR_BL_Left",
            ["BR_V_Top"] = "SFR_SFR_BR_Top",
            ["BR_H_Right"] = "SFR_SFR_BR_Right",
            ["BR_V_Bottom"] = "SFR_SFR_BR_Bottom",
            ["BR_H_Left"] = "SFR_SFR_BR_Left"
        };

        private void ApplyGaussianStepLogic(double resultMinScore)
        {
            if (R_F_miniscore_flag)
            {
                if (resultMinScore >= 0.30 && resultMinScore < 0.60)
                    Rotate_Step_Fine = Rotate_Step_Range1;
                else if (resultMinScore >= 0.60 && resultMinScore < 0.90)
                    Rotate_Step_Fine = Rotate_Step_Range2;
                else if (resultMinScore >= 0.90 && resultMinScore < 1.0)
                    Rotate_Step_Fine = Rotate_Step_Range3;
                else if (resultMinScore >= 1.0)
                    Rotate_Step_Fine = 20;
            }
        }

        private MTFData ParseMTFData(Dictionary<string, string> outputdata)
        {
            var mtf = new MTFData();
            var properties = typeof(MTFData).GetProperties()
                .Where(p => p.CanWrite && MTFKeyMapping.ContainsKey(p.Name))
                .ToArray();

            foreach (var property in properties)
            {
                var key = MTFKeyMapping[property.Name];
                if (outputdata.ContainsKey(key) &&
                    double.TryParse(outputdata[key], out double value))
                {
                    property.SetValue(mtf, value);
                }
            }

            return mtf;
        }

        private void ValidateAndResetMTFData(MTFData mtf, string focusField)
        {
            var valuesToCheck = GetValidationValues(mtf, focusField);

            if (valuesToCheck.Any(v => v == 0))
            {
                ResetMTFDataToZero(mtf);
                LogMessage("Rough start", MessageLevel.Info);
                LogMessage("SFR VALUE = 0  Keep Move position", MessageLevel.Info);
            }
        }

        private IEnumerable<double> GetValidationValues(MTFData mtf, string focusField)
        {
            var baseValues = new[] { mtf.CT_V_Top, mtf.CT_H_Right, mtf.CT_V_Bottom, mtf.CT_H_Left };

            var cornerValues = focusField == "Inter"
                ? new[] { mtf.TL_H_Right, mtf.TL_V_Bottom, mtf.TR_V_Bottom, mtf.TR_H_Left,
                 mtf.BL_V_Top, mtf.BL_H_Right, mtf.BR_V_Top, mtf.BR_H_Left }
                : new[] { mtf.TL_H_Left, mtf.TL_V_Top, mtf.TR_V_Top, mtf.TR_H_Right,
                 mtf.BL_V_Bottom, mtf.BL_H_Left, mtf.BR_V_Bottom, mtf.BR_H_Right };

            return baseValues.Concat(cornerValues);
        }

        private void ResetMTFDataToZero(MTFData mtf)
        {
            var properties = typeof(MTFData).GetProperties().Where(p => p.CanWrite);
            foreach (var property in properties)
            {
                if (property.PropertyType == typeof(double))
                {
                    property.SetValue(mtf, 0.0);
                }
            }
        }

        private double CalculateCornerMinMTF(MTFData mtf, string focusField, string rotateDirection)
        {
            var cornerValues = GetCornerValues(mtf, focusField, rotateDirection);
            return cornerValues.Any() ? cornerValues.Min() : 0;
        }

        private IEnumerable<double> GetCornerValues(MTFData mtf, string focusField, string rotateDirection)
        {
            var valueSelectors = new Dictionary<(string, string), Func<MTFData, double[]>>
            {
                [("Inter", "H")] = m => new[] { m.TL_H_Right, m.TR_H_Left, m.BL_H_Right, m.BR_H_Left },
                [("Inter", "V")] = m => new[] { m.TL_V_Bottom, m.TR_V_Bottom, m.BL_V_Top, m.BR_V_Top },
                [("Inter", "H&V")] = m => new[] { m.TL_H_Right, m.TL_V_Bottom, m.TR_V_Bottom, m.TR_H_Left,
                                        m.BL_V_Top, m.BL_H_Right, m.BR_V_Top, m.BR_H_Left },
                [("Outer", "H")] = m => new[] { m.TL_H_Left, m.TR_H_Right, m.BL_H_Left, m.BR_H_Right },
                [("Outer", "V")] = m => new[] { m.TL_V_Top, m.TR_V_Top, m.BL_V_Bottom, m.BR_V_Bottom },
                [("Outer", "H&V")] = m => new[] { m.TL_H_Left, m.TL_V_Top, m.TR_V_Top, m.TR_H_Right,
                                        m.BL_V_Bottom, m.BL_H_Left, m.BR_V_Bottom, m.BR_H_Right }
            };

            return valueSelectors.TryGetValue((focusField, rotateDirection), out var selector)
                ? selector(mtf)
                : new double[0];
        }

        private void RecordMTFData(MTFData mtf, string focusField)
        {
            var positionTuple = Tuple.Create(FineTune_Rel_Position, 0.0);

            // 中心數據始終記錄
            CTV.Add(Tuple.Create(FineTune_Rel_Position, mtf.CT_V));
            CTH.Add(Tuple.Create(FineTune_Rel_Position, mtf.CT_H));

            // 根據 Focus_Field 記錄不同的角落數據
            if (focusField == "Inter")
            {
                LTV.Add(Tuple.Create(FineTune_Rel_Position, mtf.TL_V_Bottom));
                LTH.Add(Tuple.Create(FineTune_Rel_Position, mtf.TL_H_Right));
                RTV.Add(Tuple.Create(FineTune_Rel_Position, mtf.TR_V_Bottom));
                RTH.Add(Tuple.Create(FineTune_Rel_Position, mtf.TR_H_Left));
                LBV.Add(Tuple.Create(FineTune_Rel_Position, mtf.BL_V_Top));
                LBH.Add(Tuple.Create(FineTune_Rel_Position, mtf.BL_H_Right));
                RBV.Add(Tuple.Create(FineTune_Rel_Position, mtf.BR_V_Top));
                RBH.Add(Tuple.Create(FineTune_Rel_Position, mtf.BR_H_Left));
            }
            else // "Outer"
            {
                LTV.Add(Tuple.Create(FineTune_Rel_Position, mtf.TL_V_Top));
                LTH.Add(Tuple.Create(FineTune_Rel_Position, mtf.TL_H_Left));
                RTV.Add(Tuple.Create(FineTune_Rel_Position, mtf.TR_V_Top));
                RTH.Add(Tuple.Create(FineTune_Rel_Position, mtf.TR_H_Right));
                LBV.Add(Tuple.Create(FineTune_Rel_Position, mtf.BL_V_Bottom));
                LBH.Add(Tuple.Create(FineTune_Rel_Position, mtf.BL_H_Left));
                RBV.Add(Tuple.Create(FineTune_Rel_Position, mtf.BR_V_Bottom));
                RBH.Add(Tuple.Create(FineTune_Rel_Position, mtf.BR_H_Right));
            }
        }

        private void UpdateOutputData(MTFData mtf, double resultMinScore, Dictionary<string, Dictionary<string, object>> outputdata2, Dictionary<string, object> sfr_data)
        {
            sfr_data.Clear();
            sfr_data["DAC"] = FineTune_Rel_Position;
            sfr_data["CTV"] = Math.Round(mtf.CT_V, 2);
            sfr_data["CTH"] = Math.Round(mtf.CT_H, 2);

            // 根據 Focus_Field 添加不同的數據
            if (Focus_Field == "Inter")
            {
                sfr_data["LTV"] = Math.Round(mtf.TL_V_Bottom, 2);
                sfr_data["LTH"] = Math.Round(mtf.TL_H_Right, 2);
                sfr_data["RTV"] = Math.Round(mtf.TR_V_Bottom, 2);
                sfr_data["RTH"] = Math.Round(mtf.TR_H_Left, 2);
                sfr_data["LBV"] = Math.Round(mtf.BL_V_Top, 2);
                sfr_data["LBH"] = Math.Round(mtf.BL_H_Right, 2);
                sfr_data["RBV"] = Math.Round(mtf.BR_V_Top, 2);
                sfr_data["RBH"] = Math.Round(mtf.BR_H_Left, 2);
            }
            else
            {
                sfr_data["LTV"] = Math.Round(mtf.TL_V_Top, 2);
                sfr_data["LTH"] = Math.Round(mtf.TL_H_Left, 2);
                sfr_data["RTV"] = Math.Round(mtf.TR_V_Top, 2);
                sfr_data["RTH"] = Math.Round(mtf.TR_H_Right, 2);
                sfr_data["LBV"] = Math.Round(mtf.BL_V_Bottom, 2);
                sfr_data["LBH"] = Math.Round(mtf.BL_H_Left, 2);
                sfr_data["RBV"] = Math.Round(mtf.BR_V_Bottom, 2);
                sfr_data["RBH"] = Math.Round(mtf.BR_H_Right, 2);
            }

            sfr_data["MS"] = Math.Round(resultMinScore, 2);
            outputdata2.Add(FineTune_Rel_Position.ToString(), new Dictionary<string, object>(sfr_data));
        }

        private void InitializeDataStructures()
        {
            CTV = new List<Tuple<double, double>>();
            CTH = new List<Tuple<double, double>>();
            LTV = new List<Tuple<double, double>>();
            LTH = new List<Tuple<double, double>>();
            RTV = new List<Tuple<double, double>>();
            RTH = new List<Tuple<double, double>>();
            LBV = new List<Tuple<double, double>>();
            LBH = new List<Tuple<double, double>>();
            RBV = new List<Tuple<double, double>>();
            RBH = new List<Tuple<double, double>>();
            MiniScore_List = new List<Tuple<double, double>>();
            MiniScore_List_Guss = new List<Tuple<double, double>>();
        }

        private TestLoopData ExecuteMainTestLoop(Image_Base image)
        {
            var loopData = new TestLoopData();

            // 創建保存目錄
            if (saveImgage == "YES" && !Directory.Exists(savepath))
            {
                Directory.CreateDirectory(savepath);
            }

            while (!stop_flag)
            {
                var stepResult = ExecuteSingleTestStep(image, loopData);
                if (!stepResult.Success)
                {
                    LogMessage($"Test step failed: {stepResult.ErrorMessage}", MessageLevel.Error);
                    return null;
                }

                ProcessStepResult(stepResult, loopData, image);

                if (ShouldStopLoop(stepResult, loopData))
                {
                    LogMessage("Loop stopped by conditions", MessageLevel.Info);
                    break;
                }

                MoveMotorToNextPosition(stepResult.MinScore);
                UpdatePositionCounters();
            }

            return loopData.HasValidData ? loopData : null;
        }

        private StepResult ExecuteSingleTestStep(Image_Base image, TestLoopData loopData)
        {
            var stepResult = new StepResult();

            try
            {
                // 1. 影像擷取
                if (!CaptureAndProcessImage(image, out string address, out IntPtr ptr))
                {
                    stepResult.Success = false;
                    stepResult.ErrorMessage = "Image capture failed";
                    return stepResult;
                }

                stepResult.ImagePtr = ptr;
                stepResult.ImageAddress = address;

                // 2. 保存影像 (如果需要)
                SaveImageIfRequired(image);

                // 3. SFR 分析
                if (!PerformSFRAnalysis(address, loopData.OutputData))
                {
                    stepResult.Success = false;
                    stepResult.ErrorMessage = "SFR analysis failed";
                    return stepResult;
                }

                // 4. 解析 MTF 數據
                stepResult.MTFData = ParseMTFData(loopData.OutputData);
                ValidateAndResetMTFData(stepResult.MTFData, Focus_Field);

                // 5. 計算分數
                CalculateScores(stepResult);

                stepResult.Success = true;
                return stepResult;
            }
            catch (Exception ex)
            {
                stepResult.Success = false;
                stepResult.ErrorMessage = ex.Message;
                return stepResult;
            }
        }

        private bool CaptureAndProcessImage(Image_Base image, out string address, out IntPtr ptr)
        {
            address = string.Empty;
            ptr = IntPtr.Zero;

            if (!image.Capture(ref address))
            {
                return false;
            }

            long addressValue = Convert.ToInt64(address.Substring(2), 16);
            ptr = new IntPtr(addressValue);
            return true;
        }

        private void SaveImageIfRequired(Image_Base image)
        {
            if (saveImgage != "YES") return;

            if (!string.IsNullOrEmpty(savepath_bmp_file))
            {
                image.SaveImage(1, savepath + ReplaceProp(savepath_bmp_file));
            }
            else
            {
                LogMessage("Ignore Save BMP File Name", MessageLevel.Debug);
            }

            if (!string.IsNullOrEmpty(savepath_raw_file))
            {
                image.SaveImage(0, savepath + ReplaceProp(savepath_raw_file));
            }
            else
            {
                LogMessage("Ignore Save RAW File Name", MessageLevel.Debug);
            }
        }

        private bool PerformSFRAnalysis(string address, Dictionary<string, string> outputdata)
        {
            try
            {
                string pinTmp = PIN.Replace("%address%", address);
                string oricontent = pinTmp.Replace("\\n", "\n");
                string oricontentTrans = ReplaceProp(oricontent);

                IQ_SingleEntry.SE_StartAction(DLLPath, oricontentTrans, ref strOutData, outputdata);
                LogMessage($"{strOutData}");

                return true;
            }
            catch (Exception ex)
            {
                LogMessage($"SFR Analysis failed: {ex.Message}", MessageLevel.Error);
                return false;
            }
        }

        private void CalculateScores(StepResult stepResult)
        {
            double centerMinMTF = stepResult.MTFData.CenterMinMTF;
            double cornerMinMTF = CalculateCornerMinMTF(stepResult.MTFData, Focus_Field, Rotate_Dir_HV);

            stepResult.CenterMinMTFSpec = centerMinMTF / Rotate_MS_CT_SPEC;
            stepResult.CornerMinMTFSpec = cornerMinMTF / Rotate_MS_CN_SPEC;
            stepResult.MinScore = Math.Min(stepResult.CenterMinMTFSpec, stepResult.CornerMinMTFSpec);
        }

        private void ProcessStepResult(StepResult stepResult, TestLoopData loopData, Image_Base image)
        {
            // 檢查是否切換到細調模式
            if (stepResult.MinScore > Rotate_RF_MS_Value && !R_F_miniscore_flag)
            {
                R_F_miniscore_flag = true;
                LogMessage("Rough to Fine CT Mini Score Done", MessageLevel.Info);

                // 立即應用高斯邏輯
                if (Rotate_BP_Mth_Gus == "YES")
                {
                    ApplyGaussianStepLogic(stepResult.MinScore);
                }
            }

            // 記錄數據 (如果達到閾值)
            if (stepResult.MinScore > Rotate_RF_MS_Value)
            {
                RecordMTFData(stepResult.MTFData, Focus_Field);
                UpdateOutputData(stepResult.MTFData, stepResult.MinScore, loopData.OutputData2, loopData.SfrData);
                loopData.HasValidData = true;
            }

            // 處理繪圖元素 (傳入 image 參數)
            ProcessDrawingElements(stepResult, loopData, image);
        }

        private void ProcessDrawingElements(StepResult stepResult, TestLoopData loopData, Image_Base image)
        {
            loopData.Elements.Clear();

            // ROI 繪製
            if (DrawROI)
            {
                AddROIElements(loopData.OutputData, stepResult.MTFData, loopData.Elements);
            }

            // 其他繪圖元素 (傳入 image 參數)
            AddOtherDrawingElements(stepResult, loopData.Elements, image);

            // 檢查 ROI
            if (CheckROI && !ValidateROI(loopData.OutputData, loopData.Elements))
            {
                stop_flag = true;
                return;
            }

            // 繪製到界面
            if (loopData.Elements.Count > 0 && HandleDevice.DutDashboard != null)
            {
                HandleDevice.SwitchTabControlIndex(1);
                IQ_SingleEntry.DrawElementsOnImage(stepResult.ImagePtr, image.Image_Width, image.Image_Height,
                    HandleDevice.DutDashboard.ImagePicturebox, loopData.Elements);
            }
        }

        private void AddROIElements(Dictionary<string, string> outputdata, MTFData mtfData, List<DrawElement> elements)
        {
            foreach (var entry in outputdata.Where(e => e.Key.StartsWith("ROI_") && e.Key.EndsWith("_Roi")))
            {
                var roiColor = GetROIColor(entry.Key, mtfData);
                var roiCoordinates = ParseRoiCoordinates(entry.Value, outputdata["ROI_SFR_SFR_Roi_Rule"].Split(','));

                if (roiCoordinates != null)
                {
                    elements.Add(new DrawElement((Rectangle)roiCoordinates, "", roiColor, 34, 6f, DrawElement.ElementType.Rectangle));
                }
            }
        }

        private void AddOtherDrawingElements(StepResult stepResult, List<DrawElement> elements, Image_Base image)
        {
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
                    new Rectangle(0, 0, image.Image_Width, image.Image_Height),
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
                    new Rectangle(0, 0, image.Image_Width, image.Image_Height),
                    "Diagonal",
                    Color.Blue,
                    12,
                    2.0f,
                    DrawElement.ElementType.Diagonal
                ));
            }
        }

        private bool ValidateROI(Dictionary<string, string> outputdata, List<DrawElement> elements)
        {
            var roiKeys = new[] { "Pattern_Center_TL_Pattern_x_y", "Pattern_Center_TR_Pattern_x_y",
                         "Pattern_Center_BL_Pattern_x_y", "Pattern_Center_BR_Pattern_x_y" };

            if (!roiKeys.All(key => outputdata.ContainsKey(key)))
            {
                LogMessage("Can't find ROI Key. Check Dll path or Params", MessageLevel.Error);
                return false;
            }

            if (roiKeys.Any(key => outputdata[key].Contains("-1")))
            {
                string outputMsg = "ROI FAIL:\n" + string.Join("\n",
                    roiKeys.Select(key => $"{key.Replace("Pattern_Center_", "").Replace("_Pattern_x_y", "")}={outputdata[key]}"));

                elements.Add(new DrawElement(
                    new Rectangle(0, 0, 1, 1),
                    outputMsg,
                    Color.Blue,
                    52,
                    2.0f,
                    DrawElement.ElementType.Rectangle
                ));

                LogMessage("Can't find ROI", MessageLevel.Error);
                return false;
            }

            return true;
        }

        private bool ShouldStopLoop(StepResult stepResult, TestLoopData loopData)
        {
            // 檢查 TCP 停止標誌
            if (TCP_stop_flag)
            {
                LogMessage("TCP Read Motor Move Done Fail", MessageLevel.Error);
                return true;
            }

            // 檢查總位置限制
            if (Total_Rel_Position >= Rotate_Step_Total)
            {
                stop_flag = true;
                Total_stop_flag = true;
                LogMessage("Over Focus Tune Total Degree => while loop break", MessageLevel.Error);
                return true;
            }

            // 早停機制 - 基於峰值衰減
            if (CTV.Count > 3)
            {
                return CheckEarlyStopCondition(stepResult);
            }

            return false;
        }

        private bool CheckEarlyStopCondition(StepResult stepResult)
        {
            double max_pos_CTV = 0, max_pos_CTH = 0;
            double current_max_val_CTV = 0, current_max_val_CTH = 0;

            HelperFindPeak(CTV, ref max_pos_CTV, ref current_max_val_CTV);
            HelperFindPeak(CTH, ref max_pos_CTH, ref current_max_val_CTH);

            double max_cur_ratio_v = stepResult.MTFData.CT_V / current_max_val_CTV;
            double max_cur_ratio_h = stepResult.MTFData.CT_H / current_max_val_CTH;
            double threshold = 1 - ((double)Rotate_Stop_Value / 100);

            // 傳統早停條件
            if (max_cur_ratio_v < threshold || max_cur_ratio_h < threshold)
            {
                return true;
            }

            // 高斯模式的額外早停條件
            if (Rotate_BP_Mth_Gus == "YES" && R_F_miniscore_flag && stepResult.MinScore < Rotate_RF_MS_Value)
            {
                LogMessage("Early stop: MinScore below threshold in fine mode", MessageLevel.Info);
                return true;
            }

            return false;
        }

        private void MoveMotorToNextPosition(double minScore)
        {
            int motorStep;

            if (R_F_miniscore_flag)
            {
                // 應用高斯邏輯調整步進
                if (Rotate_BP_Mth_Gus == "YES")
                {
                    ApplyGaussianStepLogic(minScore);
                }
                motorStep = Rotate_Step_Fine;
            }
            else
            {
                motorStep = Rotate_Step_Rough;
            }

            MotorStepMove(0, motorStep, 0);
        }

        private void UpdatePositionCounters()
        {
            if (R_F_miniscore_flag)
            {
                FineTune_Rel_Position += Rotate_Step_Fine;
                Total_Rel_Position += Rotate_Step_Fine;
            }
            else
            {
                Total_Rel_Position += Rotate_Step_Rough;
            }
        }

        private AnalysisResult AnalyzeTestData(TestLoopData testData)
        {
            var result = new AnalysisResult
            {
                LF_INFO = testData.LF_INFO,
                OutputData2 = testData.OutputData2
            };

            // 檢查錯誤條件
            if (TCP_stop_flag || Total_stop_flag)
            {
                return result; // 返回空結果
            }

            // 重建 MiniScore_List
            RebuildMiniScoreList(testData.OutputData2);

            // 執行峰值分析
            PerformPeakAnalysis(result);

            // 移動到最佳位置
            MoveToOptimalPosition();

            // 計算最佳位置的 SFR 值
            CalculateOptimalPositionValues(result);

            // 添加 LF_INFO 數據
            PopulateLFInfo(result);

            return result;
        }

        private void RebuildMiniScoreList(Dictionary<string, Dictionary<string, object>> outputData2)
        {
            MiniScore_List.Clear();

            foreach (var entry in outputData2)
            {
                var innerDict = entry.Value;
                if (innerDict.ContainsKey("DAC") && innerDict.ContainsKey("MS"))
                {
                    double dac = Convert.ToDouble(innerDict["DAC"]);
                    double ms = Convert.ToDouble(innerDict["MS"]);
                    MiniScore_List.Add(Tuple.Create(dac, ms));
                }
            }
        }

        private void PerformPeakAnalysis(AnalysisResult result)
        {
            if (Rotate_BP_Mth_Gus == "NO")
            {
                HelperFindPeak_MS(MiniScore_List, ref MS_peak_Pos, ref MS_middle_Pos, ref MS_Best_val);
                LogMessage($"Standard peak analysis: Peak={MS_peak_Pos}, Middle={MS_middle_Pos}, Best={MS_Best_val:F3}", MessageLevel.Info);
            }
            else
            {
                // 先執行標準分析
                HelperFindPeak_MS(MiniScore_List, ref MS_peak_Pos, ref MS_middle_Pos, ref MS_Best_val);

                // 🔧 確保在生成高斯數據前檢查原始數據
                if (MiniScore_List == null || MiniScore_List.Count < 3)
                {
                    LogMessage("Insufficient data for Gaussian fitting", MessageLevel.Warn);
                    return;
                }

                // 再執行高斯分析
                HelperFindPeak_MS_GUS(MiniScore_List, ref MS_peak_Pos_GUS, ref MS_middle_Pos_GUS, ref MS_Best_val_GUS);

                // 🔧 驗證高斯數據生成結果
                if (MiniScore_List_Guss != null && MiniScore_List_Guss.Count > 0)
                {
                    LogMessage($"Gaussian analysis completed: {MiniScore_List_Guss.Count} points generated", MessageLevel.Info);
                    LogMessage($"Gaussian peak analysis: Peak={MS_peak_Pos_GUS}, Middle={MS_middle_Pos_GUS}, Best={MS_Best_val_GUS:F3}", MessageLevel.Info);
                }
                else
                {
                    LogMessage("Gaussian fitting failed - no data generated", MessageLevel.Error);
                }
            }
        }

        private void MoveToOptimalPosition()
        {
            // 確定最佳位置
            if (Rotate_MS_Mth == "Middle")
            {
                Best_position = Rotate_BP_Mth_Gus == "YES" ? (int)MS_middle_Pos_GUS : (int)MS_middle_Pos;
            }
            else
            {
                Best_position = Rotate_BP_Mth_Gus == "YES" ? (int)MS_peak_Pos_GUS : (int)MS_peak_Pos;
            }

            // 計算需要移動的角度
            int motorPeakAngle = (int)FineTune_Rel_Position - Best_position;
            LogMessage($"[Relative_Fine_Tune_Degree] {FineTune_Rel_Position}");

            // 分批移動到最佳位置
            int countAngle = Math.Abs(motorPeakAngle);
            int counterRecord = countAngle / 20;
            if (Rotate_BP_Sep_Deg)
            {
                for (int i = 0; i < counterRecord + 1; i++)
                {
                    MotorStepMove(1, 20, 0);
                }
            }
            else {

                MotorStepMove(1, countAngle, 0);


            }
            // 背隙補償
            if (Rotate_Step_Backlash > 0)
            {
                MotorStepMove(1, Rotate_Step_Backlash, 0);
                MotorStepMove(0, Rotate_Step_Backlash, 0);
            }

            // 偏移調整
            if (Rotate_Step_Offset > 0)
            {
                int direction = Rotate_Step_Offest_Dir == "Left_Offset" ? 1 : 0;
                MotorStepMove(direction, Rotate_Step_Offset, 0);
            }
        }

        private void CalculateOptimalPositionValues(AnalysisResult result)
        {
            int targetPosition = Rotate_MS_Mth == "Middle" ?
                (Rotate_BP_Mth_Gus == "YES" ? (int)MS_middle_Pos_GUS : (int)MS_middle_Pos) :
                (Rotate_BP_Mth_Gus == "YES" ? (int)MS_peak_Pos_GUS : (int)MS_peak_Pos);

            // 計算各區域的 SFR 值
            CalculateRegionSFRValues(targetPosition, false); // 標準方法

            if (Rotate_BP_Mth_Gus == "YES")
            {
                CalculateRegionSFRValues(Rotate_MS_Mth == "Middle" ? (int)MS_middle_Pos_GUS : (int)MS_peak_Pos_GUS, true); // 高斯方法
            }
        }

        private void CalculateRegionSFRValues(int position, bool isGaussian)
        {
            var regions = new[] { CTV, CTH, LTV, LTH, RTV, RTH, LBV, LBH, RBV, RBH };
            var valueNames = new[] { "CTV", "CTH", "LTV", "LTH", "RTV", "RTH", "LBV", "LBH", "RBV", "RBH" };

            for (int i = 0; i < regions.Length; i++)
            {
                double value = Math.Round(HelperFindPeak_Apply_Pos(regions[i], position), 2);

                if (isGaussian)
                {
                    // 設置高斯值 (這裡可能需要對應的高斯計算方法)
                    SetGaussianValue(valueNames[i], value);
                }
                else
                {
                    SetStandardValue(valueNames[i], value);
                }
            }
        }

        private void SetStandardValue(string regionName, double value)
        {
            switch (regionName)
            {
                case "CTV": CTV_AP_Yvalue = value; break;
                case "CTH": CTH_AP_Yvalue = value; break;
                case "LTV": LTV_AP_Yvalue = value; break;
                case "LTH": LTH_AP_Yvalue = value; break;
                case "RTV": RTV_AP_Yvalue = value; break;
                case "RTH": RTH_AP_Yvalue = value; break;
                case "LBV": LBV_AP_Yvalue = value; break;
                case "LBH": LBH_AP_Yvalue = value; break;
                case "RBV": RBV_AP_Yvalue = value; break;
                case "RBH": RBH_AP_Yvalue = value; break;
            }
        }

        private void SetGaussianValue(string regionName, double value)
        {
            switch (regionName)
            {
                case "CTV": CTV_AP_Yvalue_GUS = value; break;
                case "CTH": CTH_AP_Yvalue_GUS = value; break;
                case "LTV": LTV_AP_Yvalue_GUS = value; break;
                case "LTH": LTH_AP_Yvalue_GUS = value; break;
                case "RTV": RTV_AP_Yvalue_GUS = value; break;
                case "RTH": RTH_AP_Yvalue_GUS = value; break;
                case "LBV": LBV_AP_Yvalue_GUS = value; break;
                case "LBH": LBH_AP_Yvalue_GUS = value; break;
                case "RBV": RBV_AP_Yvalue_GUS = value; break;
                case "RBH": RBH_AP_Yvalue_GUS = value; break;
            }
        }

        private void PopulateLFInfo(AnalysisResult result)
        {
            result.LF_INFO.Add("Total Degree", Total_Rel_Position.ToString());
            result.LF_INFO.Add("SFR Direction", Rotate_Dir_HV);
            result.LF_INFO.Add("Peak MODE", Rotate_MS_Mth);

            // MS 位置信息
            result.LF_INFO.Add("MS Pos(Peak,Middle)", $"({(int)MS_peak_Pos},{(int)MS_middle_Pos})");
            if (Rotate_BP_Mth_Gus == "YES")
            {
                result.LF_INFO.Add("MS Pos Guss(Peak,Middle)", $"({(int)MS_peak_Pos_GUS},{(int)MS_middle_Pos_GUS})");
            }

            // DOF 信息
            result.LF_INFO.Add("DOF_Left_POS", DOF_L_Pos.ToString());
            result.LF_INFO.Add("DOF_Right_POS", DOF_R_Pos.ToString());
            result.LF_INFO.Add("DOF_RP_Minus_LP", DOF_LR_Minus_Pos.ToString());

            // 峰值信息
            AddPeakInfoToLFInfo(result.LF_INFO, false);
            if (Rotate_BP_Mth_Gus == "YES")
            {
                AddPeakInfoToLFInfo(result.LF_INFO, true);
            }
        }

        private void GenerateCharts(AnalysisResult analysisResult)
        {
            var chartImages = new List<string>
            {
                "output.png",      // CT/CN V
                "secondImage.png", // CT/CN H
                "thirdImage.png",  // MS
                "fourthImage.png"  // MS Gaussian (如果啟用)
            };

            // 準備圖表數據
            var chartData = PrepareChartData();

            // 生成垂直方向圖表
            GenerateVerticalChart(chartImages[0], chartData, ChartType.Vertical);

            // 生成水平方向圖表
            GenerateHorizontalChart(chartImages[1], chartData, ChartType.Horizontal);

            // 生成 MinScore 圖表
            GenerateMinScoreChart(chartImages[2], chartData, false);

            // 生成高斯 MinScore 圖表 (如果啟用)
            if (Rotate_BP_Mth_Gus == "YES")
            {
                GenerateMinScoreChart(chartImages[3], chartData, true);
                analysisResult.FocusCurveImages.AddRange(chartImages);
            }
            else
            {
                analysisResult.FocusCurveImages.AddRange(chartImages.Take(3));
            }
        }

        private enum ChartType
        {
            Vertical,
            Horizontal,
            MinScore,
            MinScoreGaussian
        }

        private ChartDataContainer PrepareChartData()
        {
            // X 軸範圍計算
            double minX = MiniScore_List.Min(p => p.Item1);
            double maxX = MiniScore_List.Max(p => p.Item1);

            // === 修正：MinScore Y 軸範圍計算 ===
            // 🔧 包含原始和高斯數據的範圍
            double minY_MS = MiniScore_List.Min(p => p.Item2);
            double maxY_MS = MiniScore_List.Max(p => p.Item2);

            // 🔧 如果有高斯數據，也要考慮進去
            if (Rotate_BP_Mth_Gus == "YES" && MiniScore_List_Guss != null && MiniScore_List_Guss.Count > 0)
            {
                double minY_Guss = MiniScore_List_Guss.Min(p => p.Item2);
                double maxY_Guss = MiniScore_List_Guss.Max(p => p.Item2);

                minY_MS = Math.Min(minY_MS, minY_Guss);  // 取兩者中的最小值
                maxY_MS = Math.Max(maxY_MS, maxY_Guss);  // 取兩者中的最大值

                LogMessage($"MinScore range: Original({MiniScore_List.Min(p => p.Item2):F3}-{MiniScore_List.Max(p => p.Item2):F3}), " +
                          $"Gaussian({minY_Guss:F3}-{maxY_Guss:F3}), " +
                          $"Combined({minY_MS:F3}-{maxY_MS:F3})", MessageLevel.Info);
            }

            // 垂直和水平方向 SFR 範圍計算
            double minY_V = new[] { CTV, LTV, RTV, LBV, RBV }.SelectMany(list => list).Min(p => p.Item2);
            double maxY_V = new[] { CTV, LTV, RTV, LBV, RBV }.SelectMany(list => list).Max(p => p.Item2);
            double minY_H = new[] { CTH, LTH, RTH, LBH, RBH }.SelectMany(list => list).Min(p => p.Item2);
            double maxY_H = new[] { CTH, LTH, RTH, LBH, RBH }.SelectMany(list => list).Max(p => p.Item2);

            // 添加緩衝區
            double bufferRatio = 0.05;

            double rangeY_V = maxY_V - minY_V;
            double bufferY_V = rangeY_V * bufferRatio;

            double rangeY_H = maxY_H - minY_H;
            double bufferY_H = rangeY_H * bufferRatio;

            double rangeY_MS = maxY_MS - minY_MS;
            double bufferY_MS = rangeY_MS * bufferRatio;

            return new ChartDataContainer
            {
                MinX = minX,
                MaxX = maxX,
                MinY_MS = minY_MS - bufferY_MS,    // 🔧 包含高斯數據的範圍
                MaxY_MS = maxY_MS + bufferY_MS,    // 🔧 包含高斯數據的範圍
                MinY_V = minY_V - bufferY_V,
                MaxY_V = maxY_V + bufferY_V,
                MinY_H = minY_H - bufferY_H,
                MaxY_H = maxY_H + bufferY_H,
                BestPosition = Best_position,
                DOF_L_Pos = this.DOF_L_Pos,
                DOF_R_Pos = this.DOF_R_Pos
            };
        }

        private class ChartDataContainer
        {
            public double MinX { get; set; }
            public double MaxX { get; set; }
            public double MinY_MS { get; set; }
            public double MaxY_MS { get; set; }
            public double MinY_V { get; set; }
            public double MaxY_V { get; set; }
            public double MinY_H { get; set; }
            public double MaxY_H { get; set; }
            public int BestPosition { get; set; }
            public double DOF_L_Pos { get; set; }
            public double DOF_R_Pos { get; set; }
        }

        private void GenerateVerticalChart(string imagePath, ChartDataContainer data, ChartType chartType)
        {
            const int width = 600, height = 400;
            const double margin = 50;

            using (var bmp = new Bitmap(width, height))
            using (var g = Graphics.FromImage(bmp))
            {
                SetupGraphics(g);

                var scaleX = (width - 2 * margin) / (data.MaxX - data.MinX);
                var scaleY = (height - 2 * margin) / (data.MaxY_V - data.MinY_V);

                // 繪製數據點和曲線
                var seriesData = new[]
                {
                    (CTV, "CTV", Brushes.Red, Pens.Red),
                    (LTV, "LTV", Brushes.Green, Pens.Green),
                    (RTV, "RTV", Brushes.SaddleBrown, Pens.SaddleBrown),
                    (LBV, "LBV", Brushes.RoyalBlue, Pens.RoyalBlue),
                    (RBV, "RBV", Brushes.Purple, Pens.Purple)
                };

                DrawSeriesData(g, seriesData, data.MinX, data.MinY_V, scaleX, scaleY, margin);
                DrawChartFrame(g, margin, width, height);
                DrawVerticalLine(g, data.BestPosition, data.MinX, scaleX, margin, height);
                DrawLabels(g, seriesData.Select(s => (s.Item2, s.Item3)).ToArray(), margin);
                DrawAxes(g, data.MinX, data.MaxX, data.MinY_V, data.MaxY_V, scaleX, scaleY, margin, width, height, "(DAC)", "(SFR)");

                bmp.Save(imagePath, ImageFormat.Png);
            }
        }

        private void GenerateHorizontalChart(string imagePath, ChartDataContainer data, ChartType chartType)
        {
            const int width = 600, height = 400;
            const double margin = 50;

            using (var bmp = new Bitmap(width, height))
            using (var g = Graphics.FromImage(bmp))
            {
                SetupGraphics(g);

                var scaleX = (width - 2 * margin) / (data.MaxX - data.MinX);
                var scaleY = (height - 2 * margin) / (data.MaxY_H - data.MinY_H);

                // 繪製數據點和曲線
                var seriesData = new[]
                {
            (CTH, "CTH", Brushes.Red, Pens.Red),
            (LTH, "LTH", Brushes.Green, Pens.Green),
            (RTH, "RTH", Brushes.SaddleBrown, Pens.SaddleBrown),
            (LBH, "LBH", Brushes.RoyalBlue, Pens.RoyalBlue),
            (RBH, "RBH", Brushes.Purple, Pens.Purple)
        };

                DrawSeriesData(g, seriesData, data.MinX, data.MinY_H, scaleX, scaleY, margin);
                DrawChartFrame(g, margin, width, height);
                DrawVerticalLine(g, data.BestPosition, data.MinX, scaleX, margin, height);
                DrawLabels(g, seriesData.Select(s => (s.Item2, s.Item3)).ToArray(), margin);
                DrawAxes(g, data.MinX, data.MaxX, data.MinY_H, data.MaxY_H, scaleX, scaleY, margin, width, height, "(DAC)", "(SFR)");

                bmp.Save(imagePath, ImageFormat.Png);
            }
        }

        private void GenerateMinScoreChart(string imagePath, ChartDataContainer data, bool isGaussian)
        {
            const int width = 600, height = 400;
            const double margin = 50;

            using (var bmp = new Bitmap(width, height))
            using (var g = Graphics.FromImage(bmp))
            {
                SetupGraphics(g);

                // 🔧 修正：點始終來自原始數據，線根據類型選擇數據源
                var pointDataSource = MiniScore_List;                                    // 點：始終使用原始數據
                var lineDataSource = isGaussian ? MiniScore_List_Guss : MiniScore_List;  // 線：根據類型選擇

                var minY = data.MinY_MS;
                var maxY = data.MaxY_MS;

                // 🔧 檢查原始數據（點數據）是否存在
                if (pointDataSource == null || pointDataSource.Count == 0)
                {
                    LogMessage("No original MinScore data available for points", MessageLevel.Error);

                    // 繪製空圖表框架
                    DrawChartFrame(g, margin, width, height);
                    DrawAxes(g, data.MinX, data.MaxX, minY, maxY,
                        (width - 2 * margin) / (data.MaxX - data.MinX),
                        (height - 2 * margin) / (maxY - minY),
                        margin, width, height, "(DAC)", "(MS)");

                    // 添加 "No Data" 標籤
                    using (var font = new Font("Arial", 12, FontStyle.Bold))
                    {
                        var noDataText = "No MinScore Data";
                        var textSize = g.MeasureString(noDataText, font);
                        var textX = (width - textSize.Width) / 2;
                        var textY = (height - textSize.Height) / 2;
                        g.DrawString(noDataText, font, Brushes.Red, new PointF(textX, textY));
                    }

                    bmp.Save(imagePath, ImageFormat.Png);
                    return;
                }

                var scaleX = (width - 2 * margin) / (data.MaxX - data.MinX);
                var scaleY = (height - 2 * margin) / (maxY - minY);

                // === 第1步：繪製數據點 (始終來自原始 MiniScore_List) ===
                var pointScaledPoints = pointDataSource.Select(pt => new PointF(
                    (float)((pt.Item1 - data.MinX) * scaleX + margin),
                    (float)(height - ((pt.Item2 - minY) * scaleY + margin))
                )).ToArray();

                // 繪製原始數據點
                foreach (var point in pointScaledPoints)
                {
                    // 檢查點是否在有效範圍內
                    if (point.X >= margin && point.X <= width - margin &&
                        point.Y >= margin && point.Y <= height - margin)
                    {
                        g.FillEllipse(Brushes.Black, point.X - 3, point.Y - 3, 6, 6);
                    }
                }

                // === 第2步：繪製曲線 (根據 isGaussian 選擇數據源) ===
                if (isGaussian)
                {
                    // 🔧 高斯模式：檢查高斯數據是否存在
                    if (MiniScore_List_Guss == null || MiniScore_List_Guss.Count == 0)
                    {
                        LogMessage("Gaussian data is empty, only showing original data points", MessageLevel.Warn);
                        // 只顯示點，不繪製高斯曲線
                    }
                    else
                    {
                        // 繪製高斯擬合曲線
                        var gaussScaledPoints = MiniScore_List_Guss.Select(pt => new PointF(
                            (float)((pt.Item1 - data.MinX) * scaleX + margin),
                            (float)(height - ((pt.Item2 - minY) * scaleY + margin))
                        )).ToArray();

                        if (gaussScaledPoints.Length >= 3)
                        {
                            // 高斯曲線：紅色平滑曲線
                            using (var gaussPen = new Pen(Color.Black, 1))
                            {
                                g.DrawCurve(gaussPen, gaussScaledPoints, 0.5f);
                            }
                        }

                        LogMessage($"Drew Gaussian curve with {gaussScaledPoints.Length} points", MessageLevel.Info);
                    }
                }
                else
                {
                    // 🔧 原始模式：連接原始數據點
                    if (pointScaledPoints.Length >= 3)
                    {
                        // 原始數據曲線：黑色實線
                        g.DrawCurve(Pens.Black, pointScaledPoints, 0.5f);
                    }
                }

                // === 第3步：繪製圖表框架和參考線 ===
                DrawChartFrame(g, margin, width, height);
                DrawVerticalLine(g, data.BestPosition, data.MinX, scaleX, margin, height);
                DrawHorizontalLine(g, 1.0, minY, scaleY, margin, width, height, Color.Blue); // MS = 1 線
                DrawDOFLines(g, data, scaleX, margin, height);

                // === 第4步：添加標籤 ===
                var labels = new List<(string, Brush)>
                {
                    ("MS_Points", Brushes.Black),      // 原始數據點
                    ("MS=1 Spec", Brushes.Blue),       // 規格線
                    ("DOF_Range", Brushes.Green)       // DOF 範圍
                };

                // 根據模式添加曲線標籤
                if (isGaussian)
                {
                    if (MiniScore_List_Guss != null && MiniScore_List_Guss.Count > 0)
                    {
                        labels.Insert(1, ("MS_Gaussian", Brushes.Red));  // 高斯擬合曲線
                    }
                }
                else
                {
                    labels.Insert(1, ("MS_Original", Brushes.Black));    // 原始連線
                }

                DrawLabels(g, labels.ToArray(), margin);

                // === 第5步：繪製座標軸 ===
                DrawAxes(g, data.MinX, data.MaxX, minY, maxY, scaleX, scaleY, margin, width, height, "(DAC)", "(MS)");

                bmp.Save(imagePath, ImageFormat.Png);

                // 記錄繪製信息
                LogMessage($"MinScore chart saved: {imagePath}, Mode: {(isGaussian ? "Gaussian" : "Original")}, " +
                          $"Points: {pointScaledPoints.Length}, " +
                          $"Gaussian data: {(MiniScore_List_Guss?.Count ?? 0)}", MessageLevel.Info);
            }
        }

        private void SetupGraphics(Graphics g)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.White);
        }

        private void DrawSeriesData(Graphics g, (List<Tuple<double, double>>, string, Brush, Pen)[] seriesData,
            double minX, double minY, double scaleX, double scaleY, double margin)
        {
            foreach (var (data, name, brush, pen) in seriesData)
            {
                var scaledPoints = data.Select(pt => new PointF(
                    (float)((pt.Item1 - minX) * scaleX + margin),
                    (float)(400 - ((pt.Item2 - minY) * scaleY + margin))
                )).ToArray();

                // 繪製點
                foreach (var point in scaledPoints)
                {
                    g.FillEllipse(brush, point.X - 3, point.Y - 3, 6, 6);
                }

                // 繪製曲線
                if (scaledPoints.Length >= 3)
                {
                    g.DrawCurve(pen, scaledPoints, 0.5f);
                }
            }
        }

        private void DrawChartFrame(Graphics g, double margin, int width, int height)
        {
            var borderRect = new RectangleF((float)margin, (float)margin,
                width - 2 * (float)margin, height - 2 * (float)margin);
            g.DrawRectangle(Pens.Black, borderRect.X, borderRect.Y, borderRect.Width, borderRect.Height);
        }

        private void DrawVerticalLine(Graphics g, double xValue, double minX, double scaleX, double margin, int height)
        {
            using (var dashedPen = new Pen(Color.Gray, 1))
            {
                dashedPen.DashStyle = DashStyle.Dash;
                float x = (float)((xValue - minX) * scaleX + margin);

                float startY = (float)margin + 1;
                float endY = height - (float)margin - 1;

                g.DrawLine(dashedPen, x, startY, x, endY);
            }
        }

        private void DrawHorizontalLine(Graphics g, double yValue, double minY, double scaleY,
            double margin, int width, int height, Color color)
        {
            using (var dashedPen = new Pen(color, 1))
            {
                dashedPen.DashStyle = DashStyle.Dash;
                float y = (float)(height - ((yValue - minY) * scaleY + margin));

                float startX = (float)margin + 1;
                float endX = width - (float)margin - 1;

                g.DrawLine(dashedPen, startX, y, endX, y);
            }
        }

        private void DrawDOFLines(Graphics g, ChartDataContainer data, double scaleX, double margin, int height)
        {
            using (var dofPen = new Pen(Color.Green, 1))
            {
                dofPen.DashStyle = DashStyle.Dash;

                float leftX = (float)((data.DOF_L_Pos - data.MinX) * scaleX + margin);
                float rightX = (float)((data.DOF_R_Pos - data.MinX) * scaleX + margin);
                float startY = (float)margin + 1;
                float endY = height - (float)margin - 1;

                g.DrawLine(dofPen, leftX, startY, leftX, endY);
                g.DrawLine(dofPen, rightX, startY, rightX, endY);
            }
        }

        private void DrawLabels(Graphics g, (string text, Brush brush)[] labels, double margin)
        {
            using (var font = new Font("Arial", 5))
            {
                float padding = 2f;
                float alignedX = (float)margin + padding;

                for (int i = 0; i < labels.Length; i++)
                {
                    var position = new PointF(alignedX, (float)margin + 5 + padding + i * 10);
                    g.DrawString(labels[i].text, font, labels[i].brush, position);
                }
            }
        }

        private void DrawAxes(Graphics g, double minX, double maxX, double minY, double maxY,
            double scaleX, double scaleY, double margin, int width, int height, string xLabel, string yLabel)
        {
            using (var labelFont = new Font("Arial", 6))
            using (var axisFont = new Font("Arial", 5))
            {
                var labelBrush = Brushes.DarkGray;
                var axisBrush = Brushes.Black;

                // X 軸刻度
                int xTickCount = 5;
                double xStep = (maxX - minX) / (xTickCount - 1);
                for (int i = 0; i < xTickCount; i++)
                {
                    double xValue = minX + i * xStep;
                    float xPos = (float)((xValue - minX) * scaleX + margin);
                    string label = xValue.ToString("0");
                    var labelPos = new PointF(xPos - 10, height - (float)margin + 5);
                    g.DrawString(label, labelFont, labelBrush, labelPos);
                }

                // Y 軸刻度
                int yTickCount = 5;
                double yStep = (maxY - minY) / (yTickCount - 1);
                for (int i = 0; i < yTickCount; i++)
                {
                    double yValue = minY + i * yStep;
                    float yPos = (float)(height - ((yValue - minY) * scaleY + margin));
                    string label = yValue.ToString("0.##");
                    var labelPos = new PointF((float)margin - 35, yPos - 6);
                    g.DrawString(label, labelFont, labelBrush, labelPos);
                }

                // 軸標籤
                var xLabelSize = g.MeasureString(xLabel, axisFont);
                float xLabelX = (width - xLabelSize.Width) / 2;
                float xLabelY = height - (float)margin + 20;
                g.DrawString(xLabel, axisFont, axisBrush, new PointF(xLabelX, xLabelY));

                using (var format = new StringFormat())
                {
                    format.FormatFlags = StringFormatFlags.DirectionVertical;
                    var yLabelSize = g.MeasureString(yLabel, axisFont);
                    float yLabelX = (float)margin - 45;
                    float yLabelY = (height - yLabelSize.Width) / 2;
                    g.DrawString(yLabel, axisFont, axisBrush, new PointF(yLabelX, yLabelY), format);
                }
            }
        }

        private void GeneratePDFReport(AnalysisResult analysisResult)
        {
            string filePath = save_report_path + ReplaceProp(save_report_file);

            var document = new PdfDocument();

            if (analysisResult.OutputData2.Count > 0)
            {
                // 寫入曲線數據
                string jsonStr = JsonConvert.SerializeObject(analysisResult.OutputData2, Formatting.Indented);
                document = WriteOutputDataToPdf_Json(document, jsonStr);

                // 寫入相關 LF 信息
                document = DrawVerticalSectionsToPdf(document, analysisResult.LF_INFO);

                // 繪制曲線數據
                AddChartsToDocument(document, analysisResult.FocusCurveImages);

                document.Save(filePath);
            }

            document.Close();
        }

        private void AddChartsToDocument(PdfDocument document, List<string> chartImages)
        {
            if (chartImages.Count == 0) return;

            var page1 = document.AddPage();
            var gfx1 = XGraphics.FromPdfPage(page1);

            // 第一頁：前兩張圖
            if (chartImages.Count > 0)
            {
                var image1 = XImage.FromFile(chartImages[0]);
                gfx1.DrawImage(image1, 0, 0, image1.PixelWidth, image1.PixelHeight);

                if (chartImages.Count > 1)
                {
                    var image2 = XImage.FromFile(chartImages[1]);
                    double yOffset = image1.PixelHeight;
                    gfx1.DrawImage(image2, 0, yOffset, image2.PixelWidth, image2.PixelHeight);
                }
            }

            // 第二頁：後兩張圖
            if (chartImages.Count > 2)
            {
                var page2 = document.AddPage();
                var gfx2 = XGraphics.FromPdfPage(page2);

                var image3 = XImage.FromFile(chartImages[2]);
                gfx2.DrawImage(image3, 0, 0, image3.PixelWidth, image3.PixelHeight);

                if (chartImages.Count > 3)
                {
                    var image4 = XImage.FromFile(chartImages[3]);
                    double yOffset = image3.PixelHeight;
                    gfx2.DrawImage(image4, 0, yOffset, image4.PixelWidth, image4.PixelHeight);
                }
            }
        }

        private void AddPeakInfoToLFInfo(Dictionary<string, string> lfInfo, bool isGaussian)
        {
            string suffix = isGaussian ? "_GUS" : "";
            int position = isGaussian ?
                (Rotate_MS_Mth == "Middle" ? (int)MS_middle_Pos_GUS : (int)MS_peak_Pos_GUS) :
                (Rotate_MS_Mth == "Middle" ? (int)MS_middle_Pos : (int)MS_peak_Pos);

            var values = isGaussian ?
                new[] { CTV_AP_Yvalue_GUS, CTH_AP_Yvalue_GUS, LTV_AP_Yvalue_GUS, LTH_AP_Yvalue_GUS,
                RTV_AP_Yvalue_GUS, RTH_AP_Yvalue_GUS, LBV_AP_Yvalue_GUS, LBH_AP_Yvalue_GUS,
                RBV_AP_Yvalue_GUS, RBH_AP_Yvalue_GUS } :
                new[] { CTV_AP_Yvalue, CTH_AP_Yvalue, LTV_AP_Yvalue, LTH_AP_Yvalue,
                RTV_AP_Yvalue, RTH_AP_Yvalue, LBV_AP_Yvalue, LBH_AP_Yvalue,
                RBV_AP_Yvalue, RBH_AP_Yvalue };

            var names = new[] { "CTV", "CTH", "LTV", "LTH", "RTV", "RTH", "LBV", "LBH", "RBV", "RBH" };

            for (int i = 0; i < names.Length; i++)
            {
                lfInfo.Add($"{names[i]}_Peak(Pos&Val){suffix}", $"({position},{values[i]})");
            }
        }

        private void GenerateReports(AnalysisResult analysisResult)
        {
            if (analysisResult.OutputData2.Count == 0) return;

            try
            {
                // 創建報告目錄
                if (!Directory.Exists(save_report_path))
                {
                    Directory.CreateDirectory(save_report_path);
                }

                // 生成圖表
                GenerateCharts(analysisResult);

                // 生成 PDF 報告
                GeneratePDFReport(analysisResult);

                // 顯示焦點曲線 (如果啟用)
                if (Show_Tab)
                {
                    HandleDevice.DutDashboard.ShowImagesInTab("Focus_Curve", analysisResult.FocusCurveImages);
                }
            }
            catch (Exception ex)
            {
                LogMessage($"Report generation failed: {ex.Message}", MessageLevel.Error);
            }
        }

        private void LogProcessingTime(DateTime startTime)
        {
            DateTime endTime = DateTime.Now;
            TimeSpan duration = endTime - startTime;

            LogMessage($"FocusTune測試開始時間: {startTime:HH:mm:ss.fff}");
            LogMessage($"FocusTune測試結束時間: {endTime:HH:mm:ss.fff}");
            LogMessage($"FocusTune測試耗時: {duration.TotalSeconds} sec");
        }

        private static readonly Dictionary<string, Func<MTFData, double>> RoiValueSelector = new Dictionary<string, Func<MTFData, double>>
        {
            ["CT_Top"] = mtf => mtf.CT_V_Top,
            ["CT_Right"] = mtf => mtf.CT_H_Right,
            ["CT_Bottom"] = mtf => mtf.CT_V_Bottom,
            ["CT_Left"] = mtf => mtf.CT_H_Left,
            ["TL_Top"] = mtf => mtf.TL_V_Top,
            ["TL_Right"] = mtf => mtf.TL_H_Right,
            ["TL_Bottom"] = mtf => mtf.TL_V_Bottom,
            ["TL_Left"] = mtf => mtf.TL_H_Left,
            ["TR_Top"] = mtf => mtf.TR_V_Top,
            ["TR_Right"] = mtf => mtf.TR_H_Right,
            ["TR_Bottom"] = mtf => mtf.TR_V_Bottom,
            ["TR_Left"] = mtf => mtf.TR_H_Left,
            ["BL_Top"] = mtf => mtf.BL_V_Top,
            ["BL_Right"] = mtf => mtf.BL_H_Right,
            ["BL_Bottom"] = mtf => mtf.BL_V_Bottom,
            ["BL_Left"] = mtf => mtf.BL_H_Left,
            ["BR_Top"] = mtf => mtf.BR_V_Top,
            ["BR_Right"] = mtf => mtf.BR_H_Right,
            ["BR_Bottom"] = mtf => mtf.BR_V_Bottom,
            ["BR_Left"] = mtf => mtf.BR_H_Left
        };

        private Color GetROIColor(string roiKey, MTFData mtfData)
        {
            var selector = RoiValueSelector.FirstOrDefault(kvp => roiKey.Contains(kvp.Key));
            if (selector.Key == null) return Color.Yellow;

            var value = selector.Value(mtfData);
            var spec = roiKey.StartsWith("CT_") ? Rotate_MS_CT_SPEC : Rotate_MS_CN_SPEC;

            return value >= spec ? Color.Green : Color.Red;
        }

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

        public void HelperFindPeak(List<Tuple<double, double>> Data, ref double max_position, ref double max_value)
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
            for (i = Data.First().Item1; i < Data.Last().Item1; i++)
            {
                if (spline.Interpolate(i) > max_value)
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
            double Left_POS = 0;
            double Right_POS = 0;
            double max_position = 0;
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

        public double HelperFindPeak_Apply_Pos(List<Tuple<double, double>> Data, double Position)
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

        public void MotorStepMove(int Direction, int step_deg, int rev_step_deg)
        {
            string motor_move = null;

            if (Direction == 0)
                motor_move = "++" + step_deg.ToString();
            else if (Direction == 1)
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

            PdfPage page = document.AddPage();
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
        public PdfDocument DrawVerticalSectionsToPdf(PdfDocument document, Dictionary<string, string> sections)
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

        public override bool Process(Image_Base Image, ref string strOutData)
        {
            var startTime = DateTime.Now;
            LogMessage($"FocusTune測試開始時間: {startTime:HH:mm:ss.fff}");

            try
            {
                // 1. 初始化數據結構
                InitializeDataStructures();

                // 2. 執行主要測試循環
                var testData = ExecuteMainTestLoop(Image);
                if (testData == null)
                {
                    LogMessage("Main test loop failed", MessageLevel.Error);
                    return false;
                }

                // 3. 分析測試數據
                var analysisResult = AnalyzeTestData(testData);

                //Record Time
                LogProcessingTime(startTime);
                analysisResult.LF_INFO.Add("FT_Time(s)", ((float)(DateTime.Now - startTime).TotalSeconds).ToString("F2"));

                // 4. 生成報告
                GenerateReports(analysisResult);

                // 6. 設置輸出
                string jsonStr = JsonConvert.SerializeObject(analysisResult.OutputData2, Formatting.Indented);
                string jsonStrLFinfo = JsonConvert.SerializeObject(analysisResult.LF_INFO, Formatting.Indented);

                strOutData = jsonStrLFinfo;
                strstringoutput = strOutData;

                return true;
            }
            catch (Exception ex)
            {
                LogMessage($"Error For SFR Capture Calculate: {ex.Message}", MessageLevel.Error);
                return false;
            }
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
    }
}