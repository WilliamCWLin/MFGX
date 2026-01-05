using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;
using PdfSharp.Drawing;
using PdfSharp.Drawing.Layout;
using PdfSharp.Pdf;
using static AutoTestSystem.Base.ScriptBase;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Net;
using OpenCvSharp;
using System.Drawing.Printing;
using System.Xml.Linq;

namespace AutoTestSystem.Model
{
    

    public class TestInfoRecorder
    {
        private Dictionary<string, List<object>> testSections;
        public string currentRawPath;
        public string currentPDFPath;

        public TestInfoRecorder()
        {
            currentRawPath = string.Empty;
            currentPDFPath = string.Empty;
            testSections = new Dictionary<string, List<object>>();
        }
        private string FormatStep(string step)
        {
            // 在这里执行您的格式化逻辑，例如添加缩进、换行等
            // 这里仅作为示例，您可以根据实际需求进行调整

            // 使用制表符 \t 来添加缩进
            string indentedStep = $"\t{step}";

            return indentedStep;
        }

        public string DataFilter(object Data)
        {
            DataItem Item = (DataItem)Data;
            string outputData = $"{{ \n\"Data\": {Item.OutputData} \n,\n\"Spec\": {Item.Spec} \n}}";
            string timestamp = GlobalNew.stopwatch.Elapsed.ToString("hh\\:mm\\:ss\\.fff");

            string retryText = Item.RetryTimes > 0 ? $"({Item.RetryTimes})" : "";
            string exceptionText = outputData.Contains("Action Exception") ? "(Exception)" : "";

            string FinalOutput = outputData.Contains("Exception Handling") ? outputData : $" ==========[{Item.Item}]{exceptionText}{retryText}========== \n{outputData}\n\n{Item.TestResult}!! {timestamp}";
            
            return FinalOutput;
        }
        public void AddTestStep(string section, object step)
        {
            if (section == "DetailItem" || section == "ExHanling")
            {
                if (!testSections.ContainsKey("DetailItem"))
                    testSections["DetailItem"] = new List<object>();

                if (section == "ExHanling")
                {
                    testSections["DetailItem"].Add(step);
                }
                else
                {
                    // 原本：把文字轉成一段
                    testSections[section].Add(DataFilter(step));

                    // ← 新增：若這個步驟帶有圖片，就緊接著插入一個 PdfImageStep
                    var di = step as DataItem;
                    if (di != null)
                    {
                        // 單張
                        if (!string.IsNullOrWhiteSpace(di.ImagePath))
                        {
                            testSections["DetailItem"].Add(new PdfImageStep
                            {
                                Path = di.ImagePath,
                                Caption = di.Item  // 例如用測項名當 caption，可自行調整
                            });
                        }

                        // 若你改成多張 List<string>：
                        // if (di.ImagePaths != null)
                        // {
                        //     foreach (var p in di.ImagePaths)
                        //     {
                        //         if (string.IsNullOrWhiteSpace(p)) continue;
                        //         testSections["DetailItem"].Add(new PdfImageStep { Path = p, Caption = di.Item });
                        //     }
                        // }
                    }
                }
            }
            else
            {
                if (!testSections.ContainsKey(section))
                    testSections[section] = new List<object>();

                testSections[section].Add(step);
            }
        }


        public void ClearTestSteps()
        {
            currentRawPath = string.Empty;
            testSections.Clear();
        }
        public Dictionary<string, List<object>> GetTestSections()
        {
            return testSections;
        }
        //public void ShowCurrentJsonFile()
        //{
        //    if(currentRawPath != string.Empty)
        //    {
        //        // 從文件中讀取JSON字符串
        //        string json = File.ReadAllText(currentRawPath);

        //        // 將JSON字符串反序列化為字典
        //        var testSections = JsonConvert.DeserializeObject<Dictionary<string, List<object>>>(json);
        //        string absolutePath = Path.GetFullPath(currentRawPath);
        //        ExportToPdf(absolutePath.Replace(".json",".pdf"), testSections);
        //    }


        //}

        public void ShowCurrentPDF()
        {
            if (currentPDFPath != string.Empty)
            {
                Process.Start("msedge.exe", currentPDFPath);
            }

            
        }
        public void ExportToJSON(string filePath)
        {
            try
            {
                // 獲取文件的目錄路徑
                string directory = Path.GetDirectoryName(filePath);

                // 新的子目錄路徑
                string subdirectoryPath = Path.Combine(directory, "RAW");
                // 確保資料夾存在
                Directory.CreateDirectory(subdirectoryPath);

                // 獲取文件名（不包括擴展名）
                string filenameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
                // 組合新的路徑
                string newPath = Path.Combine(subdirectoryPath, filenameWithoutExtension + ".json");

                // 將字典序列化為JSON字符串
                string json = JsonConvert.SerializeObject(testSections, Formatting.Indented);

                currentRawPath = newPath;
                // 將JSON字符串寫入文件
                File.WriteAllText(newPath, json);
            }
            catch(Exception ex)
            {
                
            }
        }
        public void ExportToFile(string filePath)
        {
            try
            {
                // 獲取目錄路徑
                string directoryPath = Path.GetDirectoryName(filePath);

                // 如果目錄不存在，則創建它
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    foreach (string step in testSections[$"Title"])
                    {
                        writer.WriteLine(step);
                    }

                    writer.WriteLine("");

                    writer.WriteLine($"======================= {"Test Items"} =======================");
                    foreach (string step in testSections["Table"])
                    {
                        writer.WriteLine(step);
                    }

                    writer.WriteLine("\n");


                    writer.WriteLine($"======================= {"Test Summary"} =======================");
                    foreach (string step in testSections["Test Summary"])
                    {
                        writer.WriteLine(step);
                    }

                    writer.WriteLine("\n\n");
                    

                    foreach (var kvp in testSections)
                    {
                        if (kvp.Key == "MES" || kvp.Key == "Variable" || kvp.Key == "Title" || kvp.Key == "Table" || kvp.Key == "Test Summary" || kvp.Key == "Test Items")
                            continue;

                        foreach (string step in kvp.Value)
                        {
                            writer.WriteLine($"{step}\n\n");
                        }

                    }

                    writer.WriteLine($"======================= {"Variable Table"} =======================");
                    foreach (string step in testSections["Variable"])
                    {
                        writer.WriteLine(step);
                    }

                    writer.WriteLine("\n");

                    writer.WriteLine($"======================= {"MES Table"} =======================");
                    foreach (string step in testSections["MES"])
                    {
                        writer.WriteLine(step);
                    }

                    writer.WriteLine("\n");
                }

                
                Console.WriteLine("Test results exported to file successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error exporting test results to file: {ex.Message}");
            }
            ExportToJSON(filePath);
            
        }

        //public void ExportToPdf(string filePath, Dictionary<string, List<object>> testSections)
        //{
        //    // Create a new PDF document
        //    PdfDocument document = new PdfDocument();
        //    document.Options.CompressContentStreams = true;
        //    // Add a page to the document
        //    PdfPage page = document.AddPage();
        //    XGraphics gfx = XGraphics.FromPdfPage(page);

        //    System.Drawing.Text.PrivateFontCollection pfcFonts = new System.Drawing.Text.PrivateFontCollection();
        //    string strFontPath = @"bHEI00M.ttf";//字体设置为微软雅黑
        //    pfcFonts.AddFontFile(strFontPath);

        //    XPdfFontOptions options = new XPdfFontOptions(PdfFontEncoding.Unicode, PdfFontEmbedding.Always);
        //    XFont font = new XFont("Arial", 12, XFontStyle.Regular);

        //    //XFont font = new XFont("Arial", 11, XFontStyle.Regular);
        //    XFont Titlefont = new XFont("Arial", 18, XFontStyle.Bold);
        //    XFont Headerfont = new XFont("Arial", 22, XFontStyle.Bold);
        //    // Set page dimensions and margins
        //    double margin = 40;
        //    double pageWidth = page.Width.Point - margin * 2;
        //    double pageHeight = page.Height.Point - margin * 2;

        //    // Set the starting position for writing text
        //    double yPos = margin;

        //    // Write section header
        //    if (testSections.ContainsKey("Title"))
        //    {
        //        gfx.DrawString("MFGX Test Report ", Headerfont, XBrushes.Blue, margin, yPos);
        //        yPos += font.Height + 10;
        //        gfx.DrawString("SLA Dept. Liteon Group", Headerfont, XBrushes.Blue, margin, yPos);
        //        yPos += font.Height + 25;

        //        foreach (string step in testSections["Title"])
        //        {
        //            string[] Headerlines = step.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

        //            foreach (string line in Headerlines)
        //            {
        //                // Determine the color based on the first line content
        //                XBrush textBrush = XBrushes.Black; // Default color for other lines
        //                                                   // Calculate space needed for the line
        //                XSize lineSize = gfx.MeasureString(line, font);

        //                // Check if there's enough space for the line
        //                if (yPos + lineSize.Height > pageHeight - margin)
        //                {
        //                    // Add a new page
        //                    page = document.AddPage();
        //                    gfx = XGraphics.FromPdfPage(page);
        //                    yPos = margin; // Reset yPos for the new page
        //                }

        //                // Draw the line text with the determined color
        //                gfx.DrawString(line, font, textBrush, margin, yPos);
        //                yPos += lineSize.Height + 10; // Update yPos based on the height of the line
        //            }
        //        }
        //    }
        //    if (testSections.ContainsKey("Test Summary"))
        //    {
        //        yPos += font.Height + 5; // Update yPos based on the height of the line
        //        DrawCenteredLineWithEquals(ref gfx, page, Titlefont, "TEST SUMMARY", XBrushes.DarkSeaGreen, margin, ref yPos);
        //        foreach (string step in testSections["Test Summary"])
        //        {
        //            string[] lines = step.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
        //            XBrush textBrush = XBrushes.Black; // Default color for other lines

        //            foreach (string line in lines)
        //            {
        //                XSize lineSize = gfx.MeasureString(line, font);
        //                XFont ResultFont = new XFont("Arial", 14, XFontStyle.Bold);

        //                if (line.Contains("TestResult:"))
        //                    lineSize = gfx.MeasureString(line, ResultFont);
        //                // Check if there's enough space for the line
        //                if (yPos + lineSize.Height > pageHeight - margin)
        //                {
        //                    // Add a new page
        //                    page = document.AddPage();
        //                    gfx = XGraphics.FromPdfPage(page);
        //                    yPos = margin; // Reset yPos for the new page
        //                }

        //                if (line.Contains("TestResult:"))
        //                {
        //                    if (line.Contains("FAIL"))
        //                        gfx.DrawString(line, ResultFont, XBrushes.Red, margin, yPos);
        //                    else
        //                        gfx.DrawString(line, ResultFont, XBrushes.Green, margin, yPos);
        //                }
        //                else
        //                    gfx.DrawString(line, font, textBrush, margin, yPos);

        //                yPos += lineSize.Height + 10; // Update yPos based on the height of the line
        //            }
        //        }
        //    }

        //    if (testSections.ContainsKey("Table"))
        //    {
        //        yPos += font.Height + 10; // Update yPos based on the height of the line
        //        DrawCenteredLineWithEquals(ref gfx, page, Titlefont, "ITEM TABLE", XBrushes.DarkSeaGreen, margin, ref yPos);

        //        // Convert your data into a list of string arrays
        //        List<string[]> tableData = new List<string[]>();

        //        foreach (string step in testSections["Table"])
        //        {
        //            // Split the step into columns based on multiple spaces
        //            string[] columns = System.Text.RegularExpressions.Regex.Split(step, @"\s{2,}").Where(s => !string.IsNullOrEmpty(s)).ToArray();

        //            // Add the columns array to the tableData list
        //            tableData.Add(columns);
        //        }
        //        // Draw the table
        //        page = DrawTable(document, page, ref gfx, font, margin, ref yPos, tableData);

        //    }
        //    // Check if there's enough space for the Test Items section
        //    if (yPos + font.Height + 20 > pageHeight)
        //    {
        //        // Add a new page
        //        page = document.AddPage();
        //        gfx = XGraphics.FromPdfPage(page);
        //        yPos = margin; // Reset yPos for the new page
        //    }
            
        //    if (testSections.ContainsKey("DetailItem"))
        //    {
        //        yPos += font.Height + 20; // Update yPos based on the height of the line

        //        DrawCenteredLineWithEquals(ref gfx, page, Titlefont, "ITEM DATA", XBrushes.DarkSeaGreen, margin, ref yPos);

        //        yPos += font.Height + 6; // Update yPos based on the height of the line
        //        foreach (string step in testSections["DetailItem"])
        //        {
        //            // Split the step into lines
        //            string[] lines = step.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
        //            bool isFirstLine = true;
        //            XBrush textBrush = XBrushes.Black; // Default color for other lines
        //            int isPassorFail = 0;
        //            int count = 0;
        //            foreach (string line in lines)
        //            {
        //                count++;

        //                // Calculate space needed for the line
        //                XSize lineSize = gfx.MeasureString(line, font);
        //                CheckForPageOverflow(document, ref yPos, lineSize.Height, pageHeight, margin, ref page, ref gfx);

        //                if (count == lines.Count())
        //                {
        //                    if (line.Contains("PASS!!"))
        //                        isPassorFail = 1;
        //                    else if (line.Contains("FAIL"))
        //                        isPassorFail = 0;
        //                    else if (line.Contains("Exception Handling"))
        //                        isPassorFail = 2;

        //                    XFont Resultfont = new XFont("Arial", 16, XFontStyle.Bold);

        //                    if (isPassorFail == 0)
        //                    {
        //                        gfx.DrawString(line, Resultfont, XBrushes.Red, margin, yPos);
        //                        yPos += font.Height + 30; // Update yPos based on the height of the line
        //                    }

        //                    else if (isPassorFail == 2)
        //                    {
        //                        DrawCenteredLineWithEquals(ref gfx, page, Titlefont, line.Replace("*", ""), XBrushes.Red, margin, ref yPos);
        //                        yPos += font.Height + 5; // Update yPos based on the height of the line
        //                    }
        //                    else
        //                    {
        //                        gfx.DrawString(line, Resultfont, XBrushes.Green, margin, yPos);
        //                        yPos += font.Height + 30;
        //                    }
        //                }
        //                else
        //                {
        //                    if (isFirstLine)
        //                    {
        //                        if (line.Contains("Exception"))
        //                            textBrush = XBrushes.Red; // Default color for other lines
        //                        else
        //                            textBrush = XBrushes.Black; // Default color for other lines
        //                        //DrawSectionSeparator(ref gfx, font, ref yPos, margin);
        //                        gfx.DrawString(line, Titlefont, textBrush, margin, yPos);
        //                        yPos += lineSize.Height + 6; // Update yPos based on the height of the line
        //                        isFirstLine = false; // Only check the first line                          
        //                    }
        //                    else
        //                    {
        //                        textBrush = XBrushes.Black; // Default color for other lines
        //                        gfx.DrawString(line, font, textBrush, margin, yPos);
        //                        yPos += lineSize.Height + 6; // Update yPos based on the height of the line
        //                    }

        //                }
        //            }
        //        }
        //    }

        //    // Add some spacing between sections
        //    yPos += 22;

        //    // Save the document to the specified file path
        //    document.Save(filePath);

        //    // 將PDF文檔保存到MemoryStream
        //    // 創建一個HTTP監聽器
        //    //var listener = new HttpListener();
        //    //listener.Prefixes.Add("http://localhost:8000/");
        //    //listener.Start();

        //    //    // 在另一個線程中處理請求
        //    //    Task.Run(() =>
        //    //{
        //    //    while (true)
        //    //    {
        //    //        var context = listener.GetContext();
        //    //        var response = context.Response;

        //    //        // 假設您已經有了PDF數據的byte數組
        //    //        byte[] pdfBuffer; // 從您的應用程序中獲取PDF數據
        //    //        using (MemoryStream stream = new MemoryStream())
        //    //        {
        //    //            // 保存文檔到流
        //    //            document.Save(stream, false);

        //    //            // 將流轉換為字節數組
        //    //            pdfBuffer = stream.ToArray();
        //    //        }
        //    //        // 將PDF數據寫入HTTP響應
        //    //        response.ContentType = "application/pdf";
        //    //        response.OutputStream.Write(pdfBuffer, 0, pdfBuffer.Length);
        //    //        response.OutputStream.Close();
        //    //    }
        //    //});

        //    //// 使用Edge打開PDF
            

        //    document.Close();
        //    Process.Start("msedge.exe", filePath);

        //}
        public void ExportToPdf(string filePath/*, Dictionary<string, List<string>> testSections*/)
        {
            // Create a new PDF document
            PdfDocument document = new PdfDocument();
            document.Options.CompressContentStreams = true;
            // Add a page to the document
            PdfPage page = document.AddPage();
            XGraphics gfx = XGraphics.FromPdfPage(page);

            //System.Drawing.Text.PrivateFontCollection pfcFonts = new System.Drawing.Text.PrivateFontCollection();
            //string strFontPath = @"bHEI00M.ttf";//字体设置为微软雅黑
            //pfcFonts.AddFontFile(strFontPath);

            XPdfFontOptions options = new XPdfFontOptions(PdfFontEncoding.Unicode, PdfFontEmbedding.Always);

            //XFont font = new XFont(pfcFonts.Families[0], 12, XFontStyle.Bold, options);
            XFont font = new XFont("Arial", 10, XFontStyle.Regular);
            XFont smallfont = new XFont("Calibri", 9, XFontStyle.Regular);
            //XFont font = new XFont("Arial", 11, XFontStyle.Regular);
            XFont Titlefont = new XFont("Arial", 14, XFontStyle.Bold);
            XFont Headerfont = new XFont("Arial", 18, XFontStyle.Bold);
            // Set page dimensions and margins
            double margin = 40;
            double pageWidth = page.Width.Point - margin * 2;
            double pageHeight = page.Height.Point - margin * 2;

            // Set the starting position for writing text
            double yPos = margin;

            // Write section header
            if (testSections.ContainsKey("Title"))
            {
                gfx.DrawString("MFGX Test Report ", Headerfont, XBrushes.Blue, margin, yPos);
                yPos += font.Height + 10;
                gfx.DrawString("SLA Dept. Liteon Group", Headerfont, XBrushes.Blue, margin, yPos);
                yPos += font.Height + 25;
                    
                foreach (string step in testSections["Title"])
                {
                    string[] Headerlines = step.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

                    foreach (string line in Headerlines)
                    {
                        // Determine the color based on the first line content
                        XBrush textBrush = XBrushes.Black; // Default color for other lines
                                                            // Calculate space needed for the line
                        XSize lineSize = gfx.MeasureString(line, font);

                        // Check if there's enough space for the line
                        if (yPos + lineSize.Height > pageHeight - margin)
                        {
                            // Add a new page
                            page = document.AddPage();
                            gfx = XGraphics.FromPdfPage(page);
                            yPos = margin; // Reset yPos for the new page
                        }

                        // Draw the line text with the determined color
                        gfx.DrawString(line, font, textBrush, margin, yPos);
                        yPos += lineSize.Height + 10; // Update yPos based on the height of the line
                    }
                }
            }
            
            if (testSections.ContainsKey("Test Summary"))
            {
                yPos += font.Height + 5; // Update yPos based on the height of the line
                DrawCenteredLineWithEquals(ref gfx, page, Titlefont, "TEST SUMMARY", XBrushes.DarkSeaGreen, margin, ref yPos);
                foreach (string step in testSections["Test Summary"])
                {
                    string[] lines = step.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
                    XBrush textBrush = XBrushes.Black; // Default color for other lines

                    foreach (string line in lines)
                    {
                        XSize lineSize = gfx.MeasureString(line, font);
                        XFont ResultFont = new XFont("Arial", 14, XFontStyle.Bold);

                        if (line.Contains("TestResult:"))
                            lineSize = gfx.MeasureString(line, ResultFont);
                        // Check if there's enough space for the line
                        if (yPos + lineSize.Height > pageHeight - margin)
                        {
                            // Add a new page
                            page = document.AddPage();
                            gfx = XGraphics.FromPdfPage(page);
                            yPos = margin; // Reset yPos for the new page
                        }

                        if(line.Contains("TestResult:"))
                        {
                            if(line.Contains("FAIL"))
                                gfx.DrawString(line, ResultFont, XBrushes.Red, margin, yPos);
                            else
                                gfx.DrawString(line, ResultFont, XBrushes.Green, margin, yPos);
                        }
                        else
                            gfx.DrawString(line, font, textBrush, margin, yPos);
 
                        yPos += lineSize.Height + 10; // Update yPos based on the height of the line
                    }
                }
            }


            if (testSections.ContainsKey("Table"))
            {
                yPos += font.Height + 10; // Update yPos based on the height of the line
                DrawCenteredLineWithEquals(ref gfx, page, Titlefont, "ITEM TABLE", XBrushes.DarkSeaGreen, margin, ref yPos);

                // Convert your data into a list of string arrays
                List<string[]> tableData = new List<string[]>();

                foreach (string step in testSections["Table"])
                {
                    // Split the step into columns based on multiple spaces
                    string[] columns = System.Text.RegularExpressions.Regex.Split(step, @"\s{2,}").Where(s => !string.IsNullOrEmpty(s)).ToArray();

                    // Add the columns array to the tableData list
                    tableData.Add(columns);
                }
                // Draw the table
                page = DrawTable(document,page,ref gfx, font, margin, ref yPos, tableData);

            }


            // Check if there's enough space for the Test Items section
            if (yPos + font.Height + 20 > pageHeight)
            {
                // Add a new page
                page = document.AddPage();
                gfx = XGraphics.FromPdfPage(page);
                yPos = margin; // Reset yPos for the new page
            }
            //if (testSections.ContainsKey("Test Items"))
            //{
            //    yPos += font.Height + 20; // Update yPos based on the height of the line

            //    DrawCenteredLineWithEquals(ref gfx, page, Titlefont, "ITEM DATA", XBrushes.DarkSeaGreen, margin, ref yPos);

            //    yPos += font.Height + 6; // Update yPos based on the height of the line
            //    foreach (string step in testSections["Test Items"])
            //    {
            //        // Split the step into lines
            //        string[] lines = step.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            //        bool isFirstLine = true;
            //        XBrush textBrush = XBrushes.Black; // Default color for other lines
            //        int isPassorFail = 0;
            //        foreach (string line in lines)
            //        {
            //            // Calculate space needed for the line
            //            XSize lineSize = gfx.MeasureString(line, font);
            //            CheckForPageOverflow(document,ref yPos, lineSize.Height, pageHeight, margin, ref page, ref gfx);

            //            if (isFirstLine)
            //            {
            //                if(line.Contains("PASS"))
            //                    isPassorFail = 1;
            //                else if (line.Contains("FAIL"))
            //                    isPassorFail = 0;
            //                else if(line.IndexOf("EXCEPTION", StringComparison.OrdinalIgnoreCase) >= 0)
            //                    isPassorFail = 3;
            //                else
            //                    isPassorFail = 2;

            //                if (isPassorFail == 2)
            //                {
            //                    DrawCenteredLineWithEquals(ref gfx, page, Titlefont, line.Replace("*", ""), XBrushes.Blue, margin, ref yPos);
            //                }
            //                else
            //                {
            //                    DrawSectionSeparator(ref gfx, font, ref yPos, margin);
            //                    DrawFirstLine(ref gfx, line, Titlefont, ref textBrush, margin, ref yPos);
            //                    isFirstLine = false; // Only check the first line
            //                }

            //            }
            //            else
            //            {
            //                gfx.DrawString(line, font, textBrush, margin, yPos);
            //                yPos += lineSize.Height + 6; // Update yPos based on the height of the line
            //            }
            //        }

            //        // Draw result and update yPos
            //        DrawResult(ref gfx,font, isPassorFail, ref yPos, margin);
            //    }
            //}
            if (testSections.ContainsKey("DetailItem"))
            {
                yPos += font.Height + 20;
                DrawCenteredLineWithEquals(ref gfx, page, Titlefont, "ITEM DATA", XBrushes.DarkSeaGreen, margin, ref yPos);
                yPos += font.Height + 6;

                foreach (var obj in testSections["DetailItem"])
                {
                    // 1) 如果是圖片步驟 → 畫圖後繼續
                    var imgStep = obj as PdfImageStep;
                    if (imgStep != null)
                    {
                        // 每張圖最多寬度 = 可用頁寬、最大高度可自行調整（這裡固定 240pt）
                        double pageAvailW = page.Width.Point - margin * 2;
                        DrawImageFit(document, ref page, ref gfx, imgStep.Path, margin, ref yPos,
                                     pageAvailW, imgStep.MaxHeightPt, smallfont);
                        continue;
                    }

                    // 2) 其他視為文字（保持你原本的處理）
                    string step = obj as string;
                    if (step == null) continue;

                    string[] lines = step.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
                    bool isFirstLine = true;
                    XBrush textBrush = XBrushes.Black;
                    int isPassorFail = 0;
                    int count = 0;

                    foreach (string line in lines)
                    {
                        count++;
                        XSize lineSize = string.IsNullOrWhiteSpace(line) ? new XSize(0, 0) : gfx.MeasureString(line, font);
                        CheckForPageOverflow(document, ref yPos, lineSize.Height, pageHeight, margin, ref page, ref gfx);

                        if (count == lines.Length)
                        {
                            if (line.Contains("PASS!!")) isPassorFail = 1;
                            else if (line.Contains("FAIL")) isPassorFail = 0;
                            else if (line.Contains("Exception Handling")) isPassorFail = 2;

                            XFont Resultfont = new XFont("Arial", 16, XFontStyle.Bold);

                            if (isPassorFail == 0)
                            {
                                gfx.DrawString(line, Resultfont, XBrushes.Red, margin, yPos);
                                yPos += font.Height + 30;
                            }
                            else if (isPassorFail == 2)
                            {
                                DrawCenteredLineWithEquals(ref gfx, page, Titlefont, line.Replace("*", ""), XBrushes.LightSkyBlue, margin, ref yPos);
                                yPos += font.Height + 5;
                            }
                            else
                            {
                                gfx.DrawString(line, Resultfont, XBrushes.Green, margin, yPos);
                                yPos += font.Height + 30;
                            }
                        }
                        else
                        {
                            if (isFirstLine)
                            {
                                textBrush = line.Contains("Exception") ? XBrushes.Red : XBrushes.Black;
                                gfx.DrawString(line, Titlefont, textBrush, margin, yPos);
                                yPos += lineSize.Height + 6;
                                isFirstLine = false;
                            }
                            else
                            {
                                double textWidth = page.Width - (2 * margin);
                                XRect rect = new XRect(margin, 0, textWidth, page.Height);
                                DrawStringWithAutoWrap(ref gfx, line, font, XBrushes.Black, rect, ref yPos, document, ref page);
                            }
                        }
                    }
                }
            }

            if (testSections.ContainsKey("DUT_LOG"))
            {
                yPos += font.Height + 10; // Update yPos based on the height of the line
                DrawCenteredLineWithEquals(ref gfx, page, Titlefont, "DUT_LOG", XBrushes.LightBlue, margin, ref yPos);


                foreach (string step in testSections["DUT_LOG"])
                {
                    string[] Headerlines = step.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

                    foreach (string line in Headerlines)
                    {

                        // Determine the color based on the first line content
                        // Calculate space needed for the line
                        XSize lineSize = gfx.MeasureString(line, font);

                        // Check if there's enough space for the line
                        if (yPos + lineSize.Height > pageHeight - margin)
                        {
                            // Add a new page
                            page = document.AddPage();
                            gfx = XGraphics.FromPdfPage(page);
                            yPos = margin; // Reset yPos for the new page
                        }

                        // Draw the line text with the determined color


                        double textWidth = page.Width - (2 * margin);
                        // 創建一個XRect來定義文本區域
                        XRect rect = new XRect(margin, 0, textWidth, page.Height);
                        // 調用之前提供的自動換行函式
                        if (line.Contains("{color:green}"))
                            DrawStringWithAutoWrap(ref gfx, line.Replace("{color:green}", ""), smallfont, XBrushes.Green, rect, ref yPos, document, ref page);
                        else if (line.Contains("{color:blue}"))
                            DrawStringWithAutoWrap(ref gfx, line.Replace("{color:blue}", ""), smallfont, XBrushes.Blue, rect, ref yPos, document, ref page);
                        else
                            DrawStringWithAutoWrap(ref gfx, line, smallfont, XBrushes.Black, rect, ref yPos, document, ref page);

                        yPos += lineSize.Height + 10; // Update yPos based on the height of the line
                    }
                }
            }
            if (testSections.ContainsKey("Variable"))
            {
                yPos += font.Height + 10; // Update yPos based on the height of the line
                DrawCenteredLineWithEquals(ref gfx, page, Titlefont, "Variable TABLE", XBrushes.LightBlue, margin, ref yPos);

                // Convert your data into a list of string arrays
                List<string[]> tableData = new List<string[]>();

                foreach (string step in testSections["Variable"])
                {
                    // Split the step into columns based on multiple spaces
                    string[] columns = System.Text.RegularExpressions.Regex.Split(step, @"\s{2,}").Where(s => !string.IsNullOrEmpty(s)).ToArray();

                    // Add the columns array to the tableData list
                    tableData.Add(columns);
                }
                // Draw the table
                page = DrawVarTable(document, page, ref gfx, font, margin, ref yPos, tableData);

            }

            if (testSections.ContainsKey("MES"))
            {
                yPos += font.Height + 10; // Update yPos based on the height of the line
                DrawCenteredLineWithEquals(ref gfx, page, Titlefont, "MES TABLE", XBrushes.LightBlue, margin, ref yPos);

                // Convert your data into a list of string arrays
                List<string[]> tableData = new List<string[]>();

                foreach (string step in testSections["MES"])
                {
                    // Split the step into columns based on multiple spaces
                    string[] columns = System.Text.RegularExpressions.Regex.Split(step, @"\s{2,}").Where(s => !string.IsNullOrEmpty(s)).ToArray();

                    // Add the columns array to the tableData list
                    tableData.Add(columns);
                }
                // Draw the table
                page = DrawMESTable(document, page, ref gfx, font, margin, ref yPos, tableData);

            }
            // Add some spacing between sections
            yPos += 22;
            
            // Save the document to the specified file path
            document.Save(filePath);
            document.Close();
            string absolutePath = Path.GetFullPath(filePath);
            currentPDFPath = absolutePath;
        }

        public void DrawStringWithAutoWrap(ref XGraphics gfx, string text, XFont font, XBrush brush, XRect rect, ref double yPos, PdfDocument document, ref PdfPage page)
        {
            // 分割文本為單詞
            string[] words = text.Select(c => c.ToString()).ToArray();
            string line = "";

            foreach (string word in words)
            {
                // 測量當前行加上新單詞的寬度
                XSize size = new XSize(0, 0);
                try
                {
                    if (!string.IsNullOrWhiteSpace(line) && !string.IsNullOrEmpty(word) && !string.IsNullOrWhiteSpace(word))
                    {
                        size = gfx.MeasureString(line + word, font);
                    }
                }catch(Exception)
                {

                }

                // 如果超過最大寬度，則換行
                if (size.Width > rect.Width)
                {
                    CheckForPageOverflow(document, ref yPos, size.Height, rect.Height, rect.X, ref page, ref gfx);
                    // 繪製當前行
                    gfx.DrawString(line.TrimEnd(), font, brush, new XPoint(40, yPos));
                    // 更新yPos到新行的位置
                    yPos += font.Height + 6; // 這裡加6作為行間距
                                             
                    line = "";              // 清空行，以便開始新行
                }

                // 添加單詞到當前行
                line += word;
                

            }

            // 繪製最後一行
            if (!string.IsNullOrWhiteSpace(line))
            {
                gfx.DrawString(line.TrimEnd(), font, brush, new XPoint(40, yPos));
                yPos += font.Height + 6;
            }
        }
        public void DrawCenteredLineWithEquals(ref XGraphics gfx, PdfPage page, XFont font, string text, XBrush brush, double margin, ref double yPos)
        {
            // 計算文字的寬度
            XSize textSize = gfx.MeasureString(text, font);

            // 計算等號的寬度
            XSize equalSignSize = gfx.MeasureString("=", font);

            // 計算每側可以放置多少個等號
            int equalSignsPerSide = (int)((page.Width - textSize.Width - (2 * margin)) / (2 * equalSignSize.Width));

            // 生成左側的等號字符串
            string leftEqualSigns = new string('=', equalSignsPerSide);

            // 生成右側的等號字符串
            string rightEqualSigns = new string('=', equalSignsPerSide);

            // 組合整個字符串
            string fullLine = leftEqualSigns + text + rightEqualSigns;

            // 計算居中的起始X坐標
            double centerStartX = (page.Width - gfx.MeasureString(fullLine, font).Width) / 2;

            // 繪製填滿整行的字符串
            gfx.DrawString(fullLine, font, brush, centerStartX, yPos);

            // 更新 yPos 為下一行的位置
            yPos += font.Height + 5;
        }
        void CheckForPageOverflow(PdfDocument document,ref double yPos, double lineHeight, double pageHeight, double margin, ref PdfPage page, ref XGraphics gfx)
        {
            if (yPos + lineHeight > pageHeight - margin)
            {
                page = document.AddPage();
                gfx = XGraphics.FromPdfPage(page);
                yPos = margin; // Reset yPos for the new page
            }
        }

        void DrawSectionSeparator(ref XGraphics gfx, XFont font, ref double yPos, double margin)
        {
            gfx.DrawString("==================================", font, XBrushes.Black, margin, yPos);
            yPos += font.Height + 6; // Update yPos based on the height of the line
        }

        void DrawFirstLine(ref XGraphics gfx, string line, XFont font, ref XBrush textBrush, double margin, ref double yPos)
        {
            bool isPass = false;
            if (line.Contains("PASS"))
                isPass = true;
            else
                isPass = false;
            line = line.Replace("(PASS)", "").Replace("=", "").Replace("(FAIL)","");

            if(isPass == false)
                gfx.DrawString(line, font, XBrushes.Red, margin, yPos);
            else
                gfx.DrawString(line, font, textBrush, margin, yPos);

            yPos += font.Height + 6; // Update yPos based on the height of the line
        }

        void DrawResult(ref XGraphics gfx, XFont font, int mode, ref double yPos, double margin)
        {
            XBrush textBrush;
            string resultText;
            XFont resultFont;

            if (mode == 1)
            {
                textBrush = XBrushes.GreenYellow;
                resultText = "PASS!!";
                resultFont = font; // 使用原始字體
                gfx.DrawString(resultText, resultFont, textBrush, margin, yPos);
                yPos += resultFont.Height + 50; // 根據結果字體的高度更新 yPos
            }
            else if (mode == 0)
            {
                textBrush = XBrushes.Red;
                resultText = "FAIL!!";
                resultFont = new XFont(font.Name, font.Size * 1.5, font.Style); // 創建一個新的字體實例，字體大小為原來的1.5倍
                gfx.DrawString(resultText, resultFont, textBrush, margin, yPos);
                yPos += resultFont.Height + 50; // 根據結果字體的高度更新 yPos
            }

        }
        private void DrawWrappedText(XGraphics gfx, string text, XFont font, double x, double y, double maxWidth, double maxHeight)
        {
            XRect rect = new XRect(x, y, maxWidth, maxHeight);
            XTextFormatter tf = new XTextFormatter(gfx);
            tf.DrawString(text, font, XBrushes.Black, rect, XStringFormats.TopLeft);
        }

        private double CalculateWrappedTextHeight(XGraphics gfx, string text, XFont font, double maxWidth)
        {
            XSize size = gfx.MeasureString(text, font, XStringFormats.TopLeft);
            return size.Height;
        }
        // Method to draw a table
        PdfPage DrawTable(PdfDocument document, PdfPage currentPage, ref XGraphics gfx, XFont font, double margin, ref double yPos, List<string[]> data)
        {
            double pageHeight = currentPage.Height.Point - margin * 2;

            // Extract the headers from the first record of data
            string[] headers = data.First();

            // Manually adjust the column widths based on the content width
            double[] columnWidths = new double[] { 30, 185, 50, 65, 58, 60 };
            double[] DataWidths = new double[] { 30, 190, 55, 60, 60, 60 };
            XFont headerFont = new XFont("Arial", 12, XFontStyle.Bold);

            // Calculate the space required for the table header
            double headerHeight = gfx.MeasureString("Sample Text", headerFont).Height + 8;

            // Check if there is enough space for the table header
            if (yPos + headerHeight > pageHeight)
            {
                // Add a new page
                currentPage = document.AddPage();
                gfx = XGraphics.FromPdfPage(currentPage);
                yPos = margin; // Reset yPos for the new page
            }

            
            // Draw header
            DrawRow(currentPage,ref gfx, headerFont, margin, ref yPos, columnWidths, headers, XBrushes.DarkGray);

            // Draw the rest of the table
            foreach (var row in data.Skip(1))
            {
                double rowHeight = gfx.MeasureString("Sample Text", font).Height + 8;

                // Check if the current row fits within the current page
                if (yPos + rowHeight > pageHeight)
                {
                    // Add a new page
                    currentPage = document.AddPage();
                    gfx = XGraphics.FromPdfPage(currentPage);
                    yPos = margin; // Reset yPos for the new page

                    // Redraw the table header on the new page
                    DrawRow(currentPage,ref gfx, headerFont, margin, ref yPos, columnWidths, headers, XBrushes.DarkGray);
                }

                // Draw the row
                DrawRow(currentPage,ref gfx, font, margin, ref yPos, DataWidths, row, XBrushes.Black);
            }

            return currentPage;
        }
        PdfPage DrawVarTable(PdfDocument document, PdfPage currentPage, ref XGraphics gfx, XFont font, double margin, ref double yPos, List<string[]> data)
        {
            double pageHeight = currentPage.Height.Point - margin * 2;

            // Extract the headers from the first record of data
            string[] headers = data.First();

            // Manually adjust the column widths based on the content width
            double[] columnWidths = new double[] { 180, 145};
            double[] DataWidths = new double[] { 180, 150};
            XFont headerFont = new XFont("Arial", 12, XFontStyle.Bold);

            // Calculate the space required for the table header
            double headerHeight = gfx.MeasureString("Sample Text", headerFont).Height + 8;

            // Check if there is enough space for the table header
            if (yPos + headerHeight > pageHeight)
            {
                // Add a new page
                currentPage = document.AddPage();
                gfx = XGraphics.FromPdfPage(currentPage);
                yPos = margin; // Reset yPos for the new page
            }


            // Draw header
            DrawRow(currentPage, ref gfx, headerFont, margin, ref yPos, columnWidths, headers, XBrushes.DarkGray);

            // Draw the rest of the table
            foreach (var row in data.Skip(1))
            {
                double rowHeight = gfx.MeasureString("Sample Text", font).Height + 8;

                // Check if the current row fits within the current page
                if (yPos + rowHeight > pageHeight)
                {
                    // Add a new page
                    currentPage = document.AddPage();
                    gfx = XGraphics.FromPdfPage(currentPage);
                    yPos = margin; // Reset yPos for the new page

                    // Redraw the table header on the new page
                    DrawRow(currentPage, ref gfx, headerFont, margin, ref yPos, columnWidths, headers, XBrushes.DarkGray);
                }

                // Draw the row
                DrawRow(currentPage, ref gfx, font, margin, ref yPos, DataWidths, row, XBrushes.Black);
            }

            return currentPage;
        }

        PdfPage DrawMESTable(PdfDocument document, PdfPage currentPage, ref XGraphics gfx, XFont font, double margin, ref double yPos, List<string[]> data)
        {
            double pageHeight = currentPage.Height.Point - margin * 2;

            // Extract the headers from the first record of data
            string[] headers = data.First();

            // Manually adjust the column widths based on the content width
            double[] columnWidths = new double[] { 60, 145 };
            double[] DataWidths = new double[] { 60, 150 };
            XFont headerFont = new XFont("Arial", 12, XFontStyle.Bold);

            // Calculate the space required for the table header
            double headerHeight = gfx.MeasureString("Sample Text", headerFont).Height + 8;

            // Check if there is enough space for the table header
            if (yPos + headerHeight > pageHeight)
            {
                // Add a new page
                currentPage = document.AddPage();
                gfx = XGraphics.FromPdfPage(currentPage);
                yPos = margin; // Reset yPos for the new page
            }


            // Draw header
            DrawRow(currentPage, ref gfx, headerFont, margin, ref yPos, columnWidths, headers, XBrushes.DarkGray);

            // Draw the rest of the table
            foreach (var row in data.Skip(1))
            {
                double rowHeight = gfx.MeasureString("Sample Text", font).Height + 8;

                // Check if the current row fits within the current page
                if (yPos + rowHeight > pageHeight)
                {
                    // Add a new page
                    currentPage = document.AddPage();
                    gfx = XGraphics.FromPdfPage(currentPage);
                    yPos = margin; // Reset yPos for the new page

                    // Redraw the table header on the new page
                    DrawRow(currentPage, ref gfx, headerFont, margin, ref yPos, columnWidths, headers, XBrushes.DarkGray);
                }

                // Draw the row
                DrawRow(currentPage, ref gfx, font, margin, ref yPos, DataWidths, row, XBrushes.Black);
            }

            return currentPage;
        }

        // 代表要插入 PDF 的圖片步驟（會被放進 testSections["DetailItem"]）
        private class PdfImageStep
        {
            public string Path { get; set; }
            public string Caption { get; set; }  // 可選
            public double MaxHeightPt { get; set; } = 240; // 單張最大高度（點），可調
        }

        // 等比縮放繪圖（置中對齊），必要時自動換頁
        private void DrawImageFit(PdfDocument document, ref PdfPage page, ref XGraphics gfx,
                                  string imagePath, double margin, ref double yPos,
                                  double maxWidthPt, double maxHeightPt, XFont captionFont = null)
        {
            if (string.IsNullOrWhiteSpace(imagePath) || !File.Exists(imagePath)) return;

            double pageWidth = page.Width.Point - margin * 2;
            double pageHeight = page.Height.Point - margin * 2;

            using (var img = XImage.FromFile(imagePath))
            {
                // 以 PDF point 計算原始尺寸
                double w = img.PixelWidth > 0 && img.HorizontalResolution > 0 ? img.PixelWidth * 72.0 / img.HorizontalResolution : img.PointWidth;
                double h = img.PixelHeight > 0 && img.VerticalResolution > 0 ? img.PixelHeight * 72.0 / img.VerticalResolution : img.PointHeight;

                double targetW = Math.Min(maxWidthPt, pageWidth);
                double targetH = Math.Min(maxHeightPt, pageHeight);

                double scale = Math.Min(targetW / Math.Max(1, w), targetH / Math.Max(1, h));
                scale = Math.Min(1.0, scale); // 不放大，僅縮小

                double drawW = w * scale;
                double drawH = h * scale;

                // 頁面空間不足則換頁
                if (yPos + drawH > pageHeight - margin)
                {
                    page = document.AddPage();
                    gfx = XGraphics.FromPdfPage(page);
                    yPos = margin;
                }

                double x = margin + (pageWidth - drawW) / 2.0;
                gfx.DrawImage(img, x, yPos, drawW, drawH);
                yPos += drawH + 6;

                if (captionFont != null)
                {
                    string cap = Path.GetFileName(imagePath);
                    var sz = gfx.MeasureString(cap, captionFont);
                    double cx = margin + (pageWidth - sz.Width) / 2.0;
                    gfx.DrawString(cap, captionFont, XBrushes.Blue, cx, yPos);
                    yPos += captionFont.Height + 8;
                }
            }
        }
        // Method to draw a single row
        void DrawRow(PdfPage page,ref XGraphics gfx, XFont font, double margin, ref double yPos, double[] columnWidths, string[] rowData, XBrush brush)
        {
 
            double xPosition = margin;
            for (int i = 0; i < rowData.Length; i++)
            {
                try
                {
                    gfx.DrawString(rowData[i], font, brush, xPosition, yPos);
                    if(i> columnWidths.Length-1)
                        xPosition += columnWidths[columnWidths.Length - 1];
                    else
                        xPosition += columnWidths[i];
                }
                catch (Exception ex)
                {
                    continue;
                }
            }

            yPos += font.Height + 5; // Update yPos for the next row

        }
    }

    //class Program
    //{
    //    static void Main(string[] args)
    //    {
    //        // 創建測試結果記錄器
    //        TestResultRecorder testRecorder = new TestResultRecorder();

    //        // 在測試過程中收集資料，根據段落分類添加步驟
    //        testRecorder.AddTestStep("Program Info", "Test Program V2.50.15");
    //        testRecorder.AddTestStep("Program Info", "Test config File version 0.0.1.0");
    //        testRecorder.AddTestStep("Test Summary", "Total Test Time: 7119 secs");
    //        testRecorder.AddTestStep("Test Summary", "SN: 2242I05MO");
    //        testRecorder.AddTestStep("Test Summary", "SSN: Scan");
    //        // ... 添加更多測試步驟

    //        // 測試結束後將記錄的資料輸出成文件
    //        string filePath = "TestResults.txt";
    //        testRecorder.ExportToFile(filePath);
    //    }
    //}

}
