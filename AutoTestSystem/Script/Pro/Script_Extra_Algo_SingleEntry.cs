
using AutoTestSystem.Model;
using Manufacture;
using NAudio.Wave;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Data;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static AutoTestSystem.BLL.Bd;
using AutoTestSystem.DAL;
using System.Drawing;
using static AutoTestSystem.Model.IQ_SingleEntry;
using DocumentFormat.OpenXml.Drawing.Diagrams;

namespace AutoTestSystem.Script
{
    internal class Script_Extra_Algo_SingleEntry : Script_Extra_Base
    {
        [Category("SE Parameters"), Description("load content"), Editor(typeof(CommandEditor_MakeWriteLine), typeof(UITypeEditor))]
        public string PIN { get; set; } = "";

        [Category("SE Parameters"), Description("DLL Path")]
        public string DLLPath { get; set; } = "";

        [Category("Image"), Description("Image Path")]
        public int Width { get; set; } = 0;

        [Category("Image"), Description("Image Path")]
        public int Height { get; set; } = 0;

        [Category("Image"), Description("Image Path")]
        public string ImagePath { get; set; } = "";

        [Category("Check"), Description("DLL Path")]
        public bool CheckROI { get; set; } = false;

        [Category("Draw"), Description("")]
        public bool DrawROI { get; set; } = false;

        [Category("Draw"), Description("")]
        public bool DrawImage { get; set; } = false;

        [Category("Draw"), Description("")]
        public bool DrawResult { get; set; } = false;

        [Category("Draw"), Description("")]
        public bool DrawCross { get; set; } = false;

        [Category("Draw"), Description("")]
        public bool DrawDiagonal { get; set; } = false;

        [Category("Z.File Backup Queue"), Description("Copy and Dst")]
        public string FileName { get; set; } = "%ProductSN%_%Description%-%RetryCount%_%NowTimeHH_mm_ss%";

        [Category("Z.File Backup Queue"), Description("Subfolder for backup")]
        public string SubFolder { get; set; } = "%ProductSN%";
        [Category("Z.File Backup Queue"), Description("Backup or not")]
        public bool Backup { get; set; } = false;


        public string strstringoutput = "";
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
        public override bool PreProcess()
        {
            strstringoutput = string.Empty;
            return true;
        }
        public override bool Process(ref string strOutData)
        {
            Dictionary<string, string> outputdata = new Dictionary<string, string>();

            string oricontent = PIN.Replace("\\n", "\n");
            //string oupt = "";
            // 要注意這個地方檔案位置要是正確的，檔名要是正確的
            string oricontent_Trans = ReplaceProp(oricontent);
            string replaceImagePath = ReplaceProp(ImagePath);
            IQ_SingleEntry.SE_StartAction(DLLPath, oricontent_Trans, ref strOutData, outputdata);
            LogMessage($"{strOutData}");
            List<DrawElement> elements = new List<DrawElement>();
            if (DrawROI)
            {

                // 遍歷字典，找到所有的 ROI 鍵
                foreach (var entry in outputdata)
                {
                    if (entry.Key.StartsWith("ROI_") && entry.Key.EndsWith("_Roi"))
                    {
                        // 解析 ROI 坐標
                        var roiCoordinates = ParseRoiCoordinates(entry.Value, outputdata["ROI_SFR_SFR_Roi_Rule"].Split(','));
                        if (roiCoordinates != null)
                        {
                            elements.Add(new DrawElement((Rectangle)roiCoordinates, "", Color.Yellow, 34, 6f, DrawElement.ElementType.Rectangle));
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
                    40,
                    4.0f,
                    DrawElement.ElementType.Rectangle
                ));
            }
            if (DrawCross)
            {
                elements.Add(new DrawElement(
                            new Rectangle(0, 0, Width, Height),
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
                            new Rectangle(0, 0, Width, Height),
                            "Diagonal",
                            Color.Blue,
                            12,
                            2.0f,
                            DrawElement.ElementType.Diagonal
                        ));
            }
            if (DrawDiagonal)
            {
                int canvasWidth = Width;
                int canvasHeight = Height;
                double diagonal = Math.Sqrt(canvasWidth * canvasWidth + canvasHeight * canvasHeight);

                double[] ratios = { 0.7, 0.4 };
                Color[] colors = { Color.Red, Color.Blue };

                for (int i = 0; i < ratios.Length; i++)
                {
                    double radius = (diagonal * ratios[i]) / 2;
                    int centerX = canvasWidth / 2;
                    int centerY = canvasHeight / 2;

                    Rectangle boundingRect = new Rectangle(
                        (int)(centerX - radius),
                        (int)(centerY - radius),
                        (int)(radius * 2),
                        (int)(radius * 2)
                    );

                    elements.Add(new DrawElement(
                        boundingRect,
                        $"FOV {ratios[i]}",
                        colors[i],
                        2,
                        2.0f,
                        DrawElement.ElementType.Circle
                    ));
                }
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
                                IQ_SingleEntry.DrawElementsImage(replaceImagePath, HandleDevice.DutDashboard.ImagePicturebox, elements);
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
                    IQ_SingleEntry.DrawElementsImage(replaceImagePath, HandleDevice.DutDashboard.ImagePicturebox, elements);
                }
            }

            if (HandleDevice.DutDashboard != null)
            {
                if(DrawImage)
                {
                    List<string> imagePaths = new List<string>();

                    var fileList = new[] {
                            ReplaceProp(replaceImagePath),

                        };
                    var validFiles = fileList
                    .Where(f => !string.IsNullOrWhiteSpace(f) && File.Exists(f))
                    .ToList();

                    imagePaths.AddRange(validFiles);

                    if (imagePaths.Count == 0)
                    {
                        LogMessage("未找到任何圖片。");
                        return false;
                    }

                    RowDataItem.ImagePath = replaceImagePath;
                    HandleDevice.DutDashboard.ShowImagesInTab($"{Description}", imagePaths);
                }
            }

            String jsonStr = JsonConvert.SerializeObject(outputdata, Formatting.Indented);

            strOutData = jsonStr;
            strstringoutput = strOutData;

            return true;
        }
        public override bool PostProcess()
        {
            if(Backup)
            {
                string dst, err;
                string replaceImagePath = ReplaceProp(ImagePath);
                BackupQueue(replaceImagePath, RetryCount, out dst, out err);
            }

                
            string result = CheckRule(strstringoutput, Spec);
            if (result == "PASS" || Spec == "")
            {
                return true;
            }
            else
            {
                LogMessage($"{result}", MessageLevel.Error);
                return false;
            }
        }

        public static double EvaluateExpression(string expression)
        {
            DataTable table = new DataTable();
            table.Columns.Add("expression", string.Empty.GetType(), expression);
            DataRow row = table.NewRow();
            table.Rows.Add(row);
            return double.Parse((string)row["expression"]);
        }


        /// <summary>
        /// 複製來源檔到同一目錄，檔名加上 (RetryCount)；若重名則再加 _1、_2… 避免覆蓋
        /// </summary>
        /// <param name="sourcePath">來源檔路徑（可相對或絕對）</param>
        /// <param name="retryCount">重試次數，目的檔名會加上 (xx)（兩碼補零）</param>
        /// <param name="destPath">輸出：實際複製後的目的完整路徑</param>
        /// <param name="error">輸出：失敗時的錯誤訊息</param>
        /// <returns>true=成功；false=失敗</returns>
        public bool BackupQueue(string sourcePath, int retryCount,
                                                              out string destPath, out string error)
        {
            destPath = null;
            error = null;

            try
            {
                // 轉為完整路徑（同時處理相對/絕對路徑）
                string fullSource = Path.GetFullPath(sourcePath);

                // 1) 檢查來源檔是否存在
                if (!File.Exists(fullSource))
                {
                    LogMessage("來源檔不存在：" + fullSource);
                    return false;
                }

                // 2) 拆出目錄/檔名/副檔名
                string dir = Path.GetDirectoryName(fullSource);
                string name = Path.GetFileNameWithoutExtension(fullSource);
                string ext = Path.GetExtension(fullSource);

                // 3) 目的檔名：加上 (RetryCount)（兩碼補零）
                string retryTag = "(" + Math.Abs(retryCount).ToString("D2") + ")"; // 負數也會顯示 (-01)
                string destName = name + " " +retryTag + ext;
                string candidate = Path.Combine(dir, destName);

                File.Copy(fullSource, candidate, true);

                destPath = candidate;

                AddTestFileInfo(destPath);
                return true;
            }
            catch (Exception ex)
            {
                LogMessage("複製失敗：" + ex.Message);

                return false;
            }
        }

        // 添加方法來創建並添加 TestFileInfo
        private void AddTestFileInfo(string sourcePath)
        {
            TestFileInfo fileInfo = new TestFileInfo
            {
                SourcePath = ReplaceProp(sourcePath),
                FileName = ReplaceProp(FileName),
                SubFolder = ReplaceProp(SubFolder),
                DeleteSourceAfterBackup = true
            };

            HandleDevice.FileManager?.AddFile(fileInfo);
        }
    }

    public class CommandEditor_MakeWriteLine : UITypeEditor
    {
        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            using (Form form = new Form())
            {
                // 設定表單屬性
                form.StartPosition = FormStartPosition.CenterScreen; // 居中顯示
                form.Size = new Size(800, 800); // 設定表單大小
                form.Text = "編輯文字"; // 可選：設定表單標題
                form.MinimumSize = new Size(400, 300); // 可選：設置最小大小

                TextBox textBox = new TextBox
                {
                    Multiline = true,
                    Dock = DockStyle.Fill,
                    ScrollBars = ScrollBars.Both, // 開啟水平與垂直滾動條
                    WordWrap = false, // 可選：關閉自動換行，讓水平方向也能滾動
                    Text = value?.ToString().Replace("\\n", Environment.NewLine)
                };
                form.Controls.Add(textBox);
                form.ShowDialog();
                //return textBox.Text.Replace(Environment.NewLine, "\\n");
                return textBox.Text;
            }
        }

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }
    }
}
