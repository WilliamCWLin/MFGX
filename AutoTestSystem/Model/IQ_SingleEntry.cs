
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Linq;

namespace AutoTestSystem.Model
{         
    internal class IQ_SingleEntry
    {
        [DllImport("SE_IVS.dll", EntryPoint = "StartAction", CallingConvention = CallingConvention.StdCall)]
        public static extern void API_Entry(string parm, StringBuilder result);


        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr LoadLibrary(string lpFileName);

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi)]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate void fpStartAction(string paramsStr, StringBuilder result);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern bool FreeLibrary(IntPtr hModule);


        public static bool SE_IVS(string PIN, ref string strOutData)
        {
            try
            {
                StringBuilder SE_Pout = new StringBuilder(4096);

                API_Entry(PIN, SE_Pout);

                strOutData = SE_Pout.ToString();

                return true;
            }
            catch (Exception ex)
            {
                // 記錄異常信息或進行其他處理
                strOutData = string.Empty;
                return false;
            }
        }
        public static bool SE_IVS(string PIN, Dictionary<string, string> strOutData)
        {
            try
            {
                StringBuilder SE_Pout = new StringBuilder(4096);

                API_Entry(PIN, SE_Pout);
                IQ_SingleEntry.ParseConfig(SE_Pout.ToString(), strOutData);

                return true;
            }
            catch (Exception ex)
            {
                // 記錄異常信息或進行其他處理

                return false;
            }
        }
        static string GetFullPath(string path)
        {
            if (Path.IsPathRooted(path))
            {
                // 如果是絕對路徑，直接返回
                return path;
            }
            else
            {
                // 如果是相對路徑，將其轉換為絕對路徑
                return Path.GetFullPath(path);
            }
        }
        public static bool SE_StartAction(string dllpath,string PIN, ref string strOutData, Dictionary<string, string> dictOutData = null)
        {
            try
            {
                string absdllpath = GetFullPath(dllpath);
                // 確保 DLL 路徑存在
                if (!File.Exists(absdllpath))
                {
                    strOutData = "DLL 路徑不存在";
                    return false;
                }

                string dllDirectory = Path.GetDirectoryName(absdllpath);
                string originalPath = Environment.GetEnvironmentVariable("PATH");

                // 檢查是否已經包含該路徑
                if (!originalPath.Split(';').Contains(dllDirectory))
                {
                    // 將新的路徑添加到 PATH 環境變數中
                    Environment.SetEnvironmentVariable("PATH", originalPath + ";" + dllDirectory, EnvironmentVariableTarget.Process);
                }

                // 載入 DLL
                IntPtr hModule = LoadLibrary(absdllpath);
                if (hModule == IntPtr.Zero)
                {
                    strOutData = "無法載入 DLL: " + Marshal.GetLastWin32Error();
                    return false;
                }

                IntPtr procAddress = GetProcAddress(hModule, "StartAction");
                if (procAddress == IntPtr.Zero)
                {
                    strOutData = "無法載入函式: GetProcAddress(StartAction) FAIL!";
                    return false;
                }

                fpStartAction fcStartAction = (fpStartAction)Marshal.GetDelegateForFunctionPointer(procAddress, typeof(fpStartAction));

                StringBuilder SE_Pout = new StringBuilder(80000);

                fcStartAction(PIN, SE_Pout);

                strOutData = SE_Pout.ToString();

                if(dictOutData != null)
                    IQ_SingleEntry.ParseConfig(SE_Pout.ToString(), dictOutData);

                return true;
            }
            catch (Exception ex)
            {
                strOutData = "錯誤: " + ex.Message;
                return false;
            }
        }
        public static string IntensityCheck(byte[] input, int iWidth, int iHeight, Dictionary<string, string> dictOutData = null)
        {
            if (input == null)
            {
                // 若 buffer 為 null，直接返回空字串
                return "";
            }

            GCHandle handle = GCHandle.Alloc(input, GCHandleType.Pinned); // 鎖定 input 緩衝區

            try
            {
                IntPtr start_ptr = handle.AddrOfPinnedObject(); // 獲取鎖定後的指標地址
                string str_Address = $"0x{start_ptr.ToString("X")}"; // 轉換為十六進制字符串

                // 替換參數 %buf_addr% 為實際的地址
                string SEPin = "[Action]\nItem=IntensityCheck_BMP\n[Image]\nPath=0\nAddress=%buf_addr%\nWidth=%width%\nHeight=%height%\nFormat=SE_BMP\n[Parameters]\nChannel=3\nImageMode=0\nRAW_BayerPattern=1\nRange_StartFromW=1\nRange_EndFromW=%line%\nDownsample=1\nBMPTarget_Channel_BGRY=4\nSearch_Left_Pixel=0\nSearch_Top_Pixel=0\nSearch_Right_Pixel=%width%\nSearch_Bottom_Pixel=%height%";
                string PIN_tmp = SEPin.Replace("%buf_addr%", str_Address);

                PIN_tmp = PIN_tmp.Replace("%width%", iWidth.ToString());
                PIN_tmp = PIN_tmp.Replace("%height%", iHeight.ToString());
                PIN_tmp = PIN_tmp.Replace("%line%", (iWidth * iHeight).ToString());

                //YData.Clear();
                string dataout = string.Empty;
                IQ_SingleEntry.SE_StartAction("IQ\\SE_IVS.dll", PIN_tmp, ref dataout, dictOutData); // 執行計算並獲取結果
                if (dictOutData != null)
                    IQ_SingleEntry.ParseConfig(dataout, dictOutData);
                //if(dataout != null)
                //double? red = YData.GetFormattedDoubleValue("Result_Check_luminance_Red");
                //double? green = YData.GetFormattedDoubleValue("Result_Check_luminance_Green");
                //double? blue = YData.GetFormattedDoubleValue("Result_Check_luminance_Blue");
                //double? y = YData.GetFormattedDoubleValue("Result_Check_luminance_Gray");

                //string rtnOut = $"R({red})\nG({green})\nB({blue})\nY({y})";

                return dataout; // 返回計算結果
            }
            catch (Exception ex)
            {
                return $"Exception:{ex.Message}"; // 發生例外時返回空字串
            }
            finally
            {
                // 確保釋放 GCHandle
                if (handle.IsAllocated)
                {
                    handle.Free();
                }
            }
        }
        public static string SFRCheck(string dllpath,string pin,byte[] input, int iWidth, int iHeight, Dictionary<string, string> dictOutData = null)
        {
            if (input == null)
            {
                // 若 buffer 為 null，直接返回空字串
                return "";
            }

            GCHandle handle = GCHandle.Alloc(input, GCHandleType.Pinned); // 鎖定 input 緩衝區

            try
            {
                IntPtr start_ptr = handle.AddrOfPinnedObject(); // 獲取鎖定後的指標地址
                string str_Address = $"0x{start_ptr.ToString("X")}"; // 轉換為十六進制字符串

                // 替換參數 %buf_addr% 為實際的地址
                string PIN_tmp = pin.Replace("%address%", str_Address);

                PIN_tmp = PIN_tmp.Replace("%width%", iWidth.ToString());
                PIN_tmp = PIN_tmp.Replace("%height%", iHeight.ToString());
                //PIN_tmp = PIN_tmp.Replace("\r", "");
                //YData.Clear();
                string dataout = string.Empty;
                IQ_SingleEntry.SE_StartAction(dllpath, PIN_tmp, ref dataout, dictOutData); // 執行計算並獲取結果

                //if(dataout != null)
                //double? red = YData.GetFormattedDoubleValue("Result_Check_luminance_Red");
                //double? green = YData.GetFormattedDoubleValue("Result_Check_luminance_Green");
                //double? blue = YData.GetFormattedDoubleValue("Result_Check_luminance_Blue");
                //double? y = YData.GetFormattedDoubleValue("Result_Check_luminance_Gray");

                //string rtnOut = $"R({red})\nG({green})\nB({blue})\nY({y})";

                return dataout; // 返回計算結果
            }
            catch (Exception ex)
            {
                return $"Exception:{ex.Message}"; // 發生例外時返回空字串
            }
            finally
            {
                // 確保釋放 GCHandle
                if (handle.IsAllocated)
                {
                    handle.Free();
                }
            }
        }
        public static bool SaveImage(byte[] data, int width, int height, string filePath)
        {
            using (Bitmap bmp = new Bitmap(width, height, PixelFormat.Format24bppRgb))
            {
                BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.WriteOnly, bmp.PixelFormat);
                Marshal.Copy(data, 0, bmpData.Scan0, data.Length);
                bmp.UnlockBits(bmpData);

                try
                {
                    // 確定文件擴展名
                    string extension = Path.GetExtension(filePath).ToLowerInvariant();

                    // 根據擴展名選擇圖像格式
                    System.Drawing.Imaging.ImageFormat imageFormat;
                    switch (extension)
                    {
                        case ".bmp":
                            imageFormat = System.Drawing.Imaging.ImageFormat.Bmp;
                            break;
                        case ".jpg":
                        case ".jpeg":
                            imageFormat = System.Drawing.Imaging.ImageFormat.Jpeg;
                            break;
                        case ".png":
                            imageFormat = System.Drawing.Imaging.ImageFormat.Png;
                            break;
                        default:
                            // 如果擴展名不被支持，則默認使用 JPEG 格式
                            imageFormat = System.Drawing.Imaging.ImageFormat.Jpeg;
                            break;
                    }

                    // 儲存 Bitmap
                    bmp.Save(filePath, imageFormat);
                    //MessageBox.Show($"圖片已儲存至 {filePath}", "儲存成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return true;
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                    //MessageBox.Show($"儲存文件時出錯：{ex.Message}", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);

                }

            }
        }

        public static void ParseConfig(string input, Dictionary<string, string> dictionary)
        {
            if (string.IsNullOrEmpty(input))
                return;
            string currentSection = string.Empty; // 當前的 section 名稱

            // 將輸入字符串按行分割
            string[] lines = input.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                string trimmedLine = line.Trim(); // 移除每行的首尾空格

                // 忽略注釋行（以 ; 開頭的行）
                if (trimmedLine.StartsWith(";"))
                    continue;

                // 檢查是否為 section 標題（以 [ 開頭並以 ] 結尾）
                if (trimmedLine.StartsWith("[") && trimmedLine.EndsWith("]"))
                {
                    currentSection = trimmedLine.Substring(1, trimmedLine.Length - 2); // 提取 section 名稱
                    continue;
                }

                // 解析 key=value 格式的行
                var match = Regex.Match(trimmedLine, @"^([^=]+)=(.*)$");
                if (match.Success)
                {
                    string key = match.Groups[1].Value.Trim(); // 提取 key
                    string value = match.Groups[2].Value.Trim(); // 提取 value

                    // 將 key 格式化為 SectionName_KeyName 的形式
                    string formattedKey = $"{currentSection}_{key}";

                    // 將解析結果加入字典中
                    dictionary[formattedKey] = value;
                }
            }
        }
        // 傳回值：true=成功；false=失敗（例如檔案暫時被寫入中）
        public static bool DrawElementsImage(string imagePath, PictureBox pictureBox, List<DrawElement> elements)
        {
            Bitmap work = null; // 最後要交給 PictureBox 的圖

            try
            {
                // ① 讀檔：等檔案穩定後，做一張完全解耦的 Bitmap 副本（不鎖檔）
                if (!TryLoadStableClone(imagePath,
                                                  maxTries: 20,      // 最多重試 20 次
                                                  stableMs: 60,      // 需連續 60ms 不變視為穩定
                                                  retryDelayMs: 20,  // 每次重試間隔 20ms
                                                  out work))
                {
                    return false; // 讀不到就離開（多半是相機正在寫）
                }

                // ② 在「記憶體圖」上繪製
                using (Graphics g = Graphics.FromImage(work))
                {
                    foreach (var e in elements)
                    {
                        using (var pen = new Pen(e.Color, e.BorderThickness))
                        using (var brush = new SolidBrush(e.Color))
                        using (var font = new Font("Arial", e.FontSize))
                        {
                            switch (e.Type)
                            {
                                case DrawElement.ElementType.Rectangle:
                                    g.DrawRectangle(pen, e.Rectangle); break;
                                case DrawElement.ElementType.Circle:
                                    g.DrawEllipse(pen, e.Rectangle); break;
                                case DrawElement.ElementType.Cross:
                                    g.DrawLine(pen,
                                        new Point(e.Rectangle.Left, e.Rectangle.Top + e.Rectangle.Height / 2),
                                        new Point(e.Rectangle.Right, e.Rectangle.Top + e.Rectangle.Height / 2));
                                    g.DrawLine(pen,
                                        new Point(e.Rectangle.Left + e.Rectangle.Width / 2, e.Rectangle.Top),
                                        new Point(e.Rectangle.Left + e.Rectangle.Width / 2, e.Rectangle.Bottom));
                                    break;
                                case DrawElement.ElementType.Diagonal:
                                    g.DrawLine(pen, new Point(e.Rectangle.Left, e.Rectangle.Top),
                                                   new Point(e.Rectangle.Right, e.Rectangle.Bottom));
                                    g.DrawLine(pen, new Point(e.Rectangle.Right, e.Rectangle.Top),
                                                   new Point(e.Rectangle.Left, e.Rectangle.Bottom));
                                    break;
                            }

                            if (!string.IsNullOrEmpty(e.Text))
                            {
                                var textPos = new PointF(e.Rectangle.Left, e.Rectangle.Bottom + 5);
                                g.DrawString(e.Text, font, brush, textPos);
                            }
                        }
                    }
                }

                // ③ 更新 PictureBox（在 UI thread）
                if (pictureBox.InvokeRequired)
                {
                    pictureBox.Invoke((Action)(() => SwapPictureBoxImage(pictureBox, work)));
                }
                else
                {
                    SwapPictureBoxImage(pictureBox, work);
                }

                // 已經把 work 的所有權交給 PictureBox，避免 finally 再 Dispose
                work = null;
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                if (work != null) work.Dispose();
            }
        }


        /// <summary>
        /// 嘗試在檔案「穩定」後，快速讀入並回傳一張與檔案完全解耦的 Bitmap 副本。
        /// 讀檔只維持極短時間，避免與外部寫入端(FileShare.None)撞鎖。
        /// </summary>
        public static bool TryLoadStableClone(string path, int maxTries, int stableMs, int retryDelayMs, out Bitmap clone)
        {
            clone = null;
            try
            {
                for (int i = 0; i < maxTries; i++)
                {
                    // 1) 先確認檔案存在
                    if (!File.Exists(path))
                    {
                        Thread.Sleep(retryDelayMs);
                        continue;
                    }

                    // 2) 檔案「穩定」判定：size/mtime 在 stableMs 期間不變
                    long size1; DateTime t1;
                    try
                    {
                        var fi1 = new FileInfo(path);
                        size1 = fi1.Length;
                        t1 = fi1.LastWriteTimeUtc;
                    }
                    catch
                    {
                        Thread.Sleep(retryDelayMs);
                        continue;
                    }

                    Thread.Sleep(stableMs);

                    long size2; DateTime t2;
                    try
                    {
                        var fi2 = new FileInfo(path);
                        size2 = fi2.Length;
                        t2 = fi2.LastWriteTimeUtc;
                    }
                    catch
                    {
                        Thread.Sleep(retryDelayMs);
                        continue;
                    }

                    if (size1 != size2 || t1 != t2)
                    {
                        // 還在寫入中，重試
                        Thread.Sleep(retryDelayMs);
                        continue;
                    }

                    // 3) 快速開檔 + 複製像素到新 Bitmap；立刻關閉檔案 Handle
                    try
                    {
                        using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
                        using (var img = Image.FromStream(fs, true, false)) // 先建 Image
                        {
                            // 用 new Bitmap(img) 做完全解耦的像素拷貝（之後與檔案/stream無關）
                            clone = new Bitmap(img);
                            return true;
                        }
                    }
                    catch (IOException)
                    {
                        // 極短時間內相機也在開檔（FileShare.None），讓一點時間再重試
                        Thread.Sleep(retryDelayMs);
                    }
                    catch
                    {
                        Thread.Sleep(retryDelayMs);
                    }
                }
            }
            catch { }
            return false;
        }

        /// <summary>
        /// 安全地將 Bitmap 指派給 PictureBox（先換新，再釋放舊圖，避免累積）。
        /// 注意：此方法需在 UI 執行緒呼叫；若在背景執行緒請用 pb.Invoke 包起來。
        /// </summary>
        public static void SwapPictureBoxImage(PictureBox pb, Bitmap newImage)
        {
            var old = pb.Image;
            pb.Image = newImage;   // 將所有權交給 PictureBox
            if (old != null) old.Dispose();
        }
        public static bool DrawElementsOnImage(string imagePath, PictureBox ImagePicturebox, List<DrawElement> elements)
        {
            try
            {
                // 加載圖片
                using (Bitmap bmp = new Bitmap(imagePath))
                {
                    // 使用 Graphics 繪製方框和文字
                    using (Graphics g = Graphics.FromImage(bmp))
                    {
                        foreach (var element in elements)
                        {
                            using (Font font = new Font("Arial", element.FontSize))
                            using (Brush textBrush = new SolidBrush(element.Color))
                            using (Pen pen = new Pen(element.Color, element.BorderThickness))
                            {
                                switch (element.Type)
                                {
                                    case DrawElement.ElementType.Rectangle:
                                        g.DrawRectangle(pen, element.Rectangle);
                                        break;
                                    case DrawElement.ElementType.Circle:
                                        g.DrawEllipse(pen, element.Rectangle);
                                        break;
                                    case DrawElement.ElementType.Cross:
                                        // 繪製十字線
                                        g.DrawLine(pen,
                                            new Point(element.Rectangle.Left, element.Rectangle.Top + element.Rectangle.Height / 2),
                                            new Point(element.Rectangle.Right, element.Rectangle.Top + element.Rectangle.Height / 2));
                                        g.DrawLine(pen,
                                            new Point(element.Rectangle.Left + element.Rectangle.Width / 2, element.Rectangle.Top),
                                            new Point(element.Rectangle.Left + element.Rectangle.Width / 2, element.Rectangle.Bottom));
                                        break;
                                    case DrawElement.ElementType.Diagonal:
                                        // 繪製對角線
                                        g.DrawLine(pen,
                                            new Point(element.Rectangle.Left, element.Rectangle.Top),
                                            new Point(element.Rectangle.Right, element.Rectangle.Bottom));
                                        g.DrawLine(pen,
                                            new Point(element.Rectangle.Right, element.Rectangle.Top),
                                            new Point(element.Rectangle.Left, element.Rectangle.Bottom));
                                        break;
                                }

                                // 繪製文字
                                PointF textPosition = new PointF(element.Rectangle.Left, element.Rectangle.Bottom + 5);
                                g.DrawString(element.Text, font, textBrush, textPosition);
                            }
                        }
                    }

                    // 將繪製好的圖像顯示在 PictureBox 中
                    ImagePicturebox.Invoke((Action)(() =>
                    {
                        ImagePicturebox.Image?.Dispose(); // 清理先前的圖片
                        ImagePicturebox.Image = (Bitmap)bmp.Clone(); // 使用 Clone 來避免對象被釋放
                    }));
                }

            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }

        public static bool DrawElementsOnImage(IntPtr imagePtr, int w, int h, PictureBox ImagePicturebox, List<DrawElement> elements)
        {
            try
            {
                // 加載圖片

                //CreateBitmapFromIntPtr(imagePtr, w, h, PixelFormat.Format24bppRgb);
                using (Bitmap bmp = CreateBitmapFromIntPtr(imagePtr, w, h, PixelFormat.Format24bppRgb))
                {
                    // 使用 Graphics 繪製方框和文字
                    using (Graphics g = Graphics.FromImage(bmp))
                    {
                        foreach (var element in elements)
                        {
                            using (Font font = new Font("Arial", element.FontSize))
                            using (Brush textBrush = new SolidBrush(element.Color))
                            using (Pen pen = new Pen(element.Color, element.BorderThickness))
                            {
                                switch (element.Type)
                                {
                                    case DrawElement.ElementType.Rectangle:
                                        g.DrawRectangle(pen, element.Rectangle);
                                        break;
                                    case DrawElement.ElementType.Circle:
                                        g.DrawEllipse(pen, element.Rectangle);
                                        break;
                                    case DrawElement.ElementType.Cross:
                                        // 繪製十字線
                                        g.DrawLine(pen,
                                            new Point(element.Rectangle.Left, element.Rectangle.Top + element.Rectangle.Height / 2),
                                            new Point(element.Rectangle.Right, element.Rectangle.Top + element.Rectangle.Height / 2));
                                        g.DrawLine(pen,
                                            new Point(element.Rectangle.Left + element.Rectangle.Width / 2, element.Rectangle.Top),
                                            new Point(element.Rectangle.Left + element.Rectangle.Width / 2, element.Rectangle.Bottom));
                                        break;
                                    case DrawElement.ElementType.Diagonal:
                                        // 繪製對角線
                                        g.DrawLine(pen,
                                            new Point(element.Rectangle.Left, element.Rectangle.Top),
                                            new Point(element.Rectangle.Right, element.Rectangle.Bottom));
                                        g.DrawLine(pen,
                                            new Point(element.Rectangle.Right, element.Rectangle.Top),
                                            new Point(element.Rectangle.Left, element.Rectangle.Bottom));
                                        break;
                                }

                                // 繪製文字
                                PointF textPosition = new PointF(element.Rectangle.Left, element.Rectangle.Bottom + 5);
                                g.DrawString(element.Text, font, textBrush, textPosition);
                            }
                        }
                    }

                    // 將繪製好的圖像顯示在 PictureBox 中
                    ImagePicturebox.Invoke((Action)(() =>
                    {
                        ImagePicturebox.Image?.Dispose(); // 清理先前的圖片
                        ImagePicturebox.Image = (Bitmap)bmp.Clone(); // 使用 Clone 來避免對象被釋放
                    }));
                }

            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }

        public static bool DrawElementsOnImage(IntPtr imagePtr, int w, int h, Dashboard dashboard, List<DrawElement> elements, string tabName)
        {
            try
            {
                // 加載圖片

                //CreateBitmapFromIntPtr(imagePtr, w, h, PixelFormat.Format24bppRgb);
                using (Bitmap bmp = CreateBitmapFromIntPtr(imagePtr, w, h, PixelFormat.Format24bppRgb))
                {
                    // 使用 Graphics 繪製方框和文字
                    using (Graphics g = Graphics.FromImage(bmp))
                    {
                        foreach (var element in elements)
                        {
                            using (Font font = new Font("Arial", element.FontSize))
                            using (Brush textBrush = new SolidBrush(element.Color))
                            using (Pen pen = new Pen(element.Color, element.BorderThickness))
                            {
                                switch (element.Type)
                                {
                                    case DrawElement.ElementType.Rectangle:
                                        g.DrawRectangle(pen, element.Rectangle);
                                        break;
                                    case DrawElement.ElementType.Circle:
                                        g.DrawEllipse(pen, element.Rectangle);
                                        break;
                                    case DrawElement.ElementType.Cross:
                                        // 繪製十字線
                                        g.DrawLine(pen,
                                            new Point(element.Rectangle.Left, element.Rectangle.Top + element.Rectangle.Height / 2),
                                            new Point(element.Rectangle.Right, element.Rectangle.Top + element.Rectangle.Height / 2));
                                        g.DrawLine(pen,
                                            new Point(element.Rectangle.Left + element.Rectangle.Width / 2, element.Rectangle.Top),
                                            new Point(element.Rectangle.Left + element.Rectangle.Width / 2, element.Rectangle.Bottom));
                                        break;
                                    case DrawElement.ElementType.Diagonal:
                                        // 繪製對角線
                                        g.DrawLine(pen,
                                            new Point(element.Rectangle.Left, element.Rectangle.Top),
                                            new Point(element.Rectangle.Right, element.Rectangle.Bottom));
                                        g.DrawLine(pen,
                                            new Point(element.Rectangle.Right, element.Rectangle.Top),
                                            new Point(element.Rectangle.Left, element.Rectangle.Bottom));
                                        break;
                                }

                                // 繪製文字
                                PointF textPosition = new PointF(element.Rectangle.Left, element.Rectangle.Bottom + 5);
                                g.DrawString(element.Text, font, textBrush, textPosition);
                            }
                        }
                    }

                    //// 將繪製好的圖像顯示在 TabUI 中
                    dashboard.ShowSingleImageInTab(tabName, (Bitmap)bmp.Clone());
                }

            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }

        public static void ClearPictureBox(PictureBox pictureBox)
        {
            // 獲取 PictureBox 的顯示區域大小
            Size size = pictureBox.ClientSize;

            // 創建一個與 PictureBox 顯示區域相同大小的黑色圖像
            using (Bitmap clearBmp = new Bitmap(size.Width, size.Height))
            {
                using (Graphics g = Graphics.FromImage(clearBmp))
                {
                    g.Clear(Color.Black); // 設置背景顏色為黑色
                }

                // 將黑色圖像顯示在 PictureBox 中
                pictureBox.Invoke((Action)(() =>
                {
                    pictureBox.Image?.Dispose(); // 清理先前的圖片
                    pictureBox.Image = (Bitmap)clearBmp.Clone(); // 使用 Clone 來避免對象被釋放
                }));
            }
        }
        public class DrawElement
        {
            public enum ElementType
            {
                Rectangle,
                Circle,
                Cross,   // 十字線
                Diagonal // 對角線
            }

            public Rectangle Rectangle { get; set; } // 矩形或範圍區域
            public string Text { get; set; }         // 要顯示的文字
            public Color Color { get; set; }         // 顏色
            public int FontSize { get; set; }        // 文字字體大小
            public float BorderThickness { get; set; } // 線條粗度
            public ElementType Type { get; set; }    // 元素類型（矩形、圓形、十字線、對角線）

            public DrawElement(Rectangle rectangle, string text, Color color, int fontSize, float borderThickness, ElementType type)
            {
                Rectangle = rectangle;
                Text = text;
                Color = color;
                FontSize = fontSize;
                BorderThickness = borderThickness;
                Type = type;
            }
        }

        public static Bitmap CreateBitmapFromIntPtr(IntPtr ptr, int width, int height, PixelFormat pixelFormat)
        {
            //// Calculate the stride (width * bytes per pixel)
            //int stride = width * Image.GetPixelFormatSize(pixelFormat) / 8;

            //// Create a Bitmap from the IntPtr
            //return new Bitmap(width, height, stride, pixelFormat, ptr);
            int stride = width * Image.GetPixelFormatSize(pixelFormat) / 8;
            Bitmap bmp = new Bitmap(width, height, pixelFormat);

            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, pixelFormat);
            try
            {
                // 複製 ptr 的資料到 bmp 的 buffer
                unsafe
                {
                    Buffer.MemoryCopy((void*)ptr, (void*)bmpData.Scan0, stride * height, stride * height);
                }
            }
            finally
            {
                bmp.UnlockBits(bmpData);
            }

            return bmp;


        }
    }
}
