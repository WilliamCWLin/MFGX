
using AutoTestSystem.Model;
using Manufacture;
using NAudio.Wave;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Automation;
using System.Windows.Forms;

namespace AutoTestSystem.Script
{
    internal class Script_Extra_ControlUI : Script_Extra_Base
    {

        //[Category("Common Parameters"), Description("Am")]
        //public string EXE_PATH { get; set; } = "";
        [Category("Common Parameters"), Description("Value")]
        public bool Maximize { get; set; } = false;

        [Category("Common Parameters"), Description("Value")]
        public string CMD0 { get; set; } = "";
        [Category("Common Parameters"), Description("Value")]
        public string CMD1 { get; set; } = "";
        [Category("Common Parameters"), Description("Value")]
        public string CMD2 { get; set; } = "";
        [Category("Common Parameters"), Description("Value")]
        public string CMD3 { get; set; } = "";
        [Category("Common Parameters"), Description("Value")]
        public string CMD4 { get; set; } = "";
        [Category("Common Parameters"), Description("Value")]
        public string CMD5 { get; set; } = "";
        [Category("Common Parameters"), Description("Value")]
        public string CMD6 { get; set; } = "";
        [Category("Common Parameters"), Description("Value")]
        public string CMD7 { get; set; } = "";
        [Category("Common Parameters"), Description("Value")]
        public string FirmwareBin { get; set; } = "";
        [Category("Common Parameters"), Description("Value")]
        public string Textbox9 { get; set; } = "";


        [Category("Invoke button"), Description("select button")]
        public int Buttoncount { get; set; } = -1;

        [Category("Delay"), Description("Wait Time")]
        public int Delay { get; set; } = 8000;

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWindInsertAfter,int X,int Y, int cx,int cy,uint uFlags);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        private const uint SWP_NOZORDER = 0x004;
        public override void Dispose()
        {
            //throw new NotImplementedException();
        }
        public override bool PreProcess()
        {

            string processName = "ambausb"; // 這裡填入你要檢查的應用程式名稱
            string applicationPath = @"C:\Program Files\Ambarella\AmbaUSB\bin\ambausb.exe"; // 這裡填入應用程式的路徑

            // 檢查應用程式是否已經在運行
            Process[] processes = System.Diagnostics.Process.GetProcessesByName(processName);
            if (processes.Length > 0)
            {
                // 如果應用程式已經在運行，恢復它而不是最大化
                foreach (var process in processes)
                {
                    IntPtr hWnd = process.MainWindowHandle;
                    if (hWnd == IntPtr.Zero)
                    {
                        // 等待主窗口創建
                        process.WaitForInputIdle();
                        hWnd = process.MainWindowHandle;
                    }

                    if (hWnd != IntPtr.Zero)
                    {
                        int screenWidth = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width;
                        int screenHeight = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height;
                        int windowWidth = 1900;
                        int windowHeight = 800;

                        int posX = (screenWidth - windowWidth) / 2;
                        int posY = (screenHeight - windowHeight) / 2;

                        //SetWindowPos(hWnd, IntPtr.Zero, posX, posY, windowWidth, windowHeight, SWP_NOZORDER);
                        //SetForegroundWindow(hWnd);
                        ShowWindow(hWnd, 3);
                        //Console.WriteLine($"{processName} 已經在運行，恢復窗口");
                    }
                    else
                    {
                        Console.WriteLine($"{processName} 沒有主窗口");
                 
                    }
                }
                if (!string.IsNullOrEmpty(FirmwareBin))
                {
                    // 檢查檔案路徑格式是否正確
                    bool isValidPath = IsValidPath(FirmwareBin);                  

                    // 檢查檔案是否存在
                    if(isValidPath)
                    {
                        bool fileExists = File.Exists(FirmwareBin);

                        if(fileExists)
                            return true;
                        else
                        {
                            LogMessage($"File is not Exists: {FirmwareBin}");
                            return false;
                        }
                    }
                    else
                    {
                        LogMessage($"File invalid: {FirmwareBin}");
                        return false;
                    }
                }


                return true;
            }
            //else
            //{
            //    LogMessage("Not Yet Start ambausb.exe");
            //    return false;
            //}

            else
            {
                // 如果應用程式沒有在運行，啟動它
                Process process = System.Diagnostics.Process.Start(applicationPath);
                if (process != null)
                {
                    // 等待應用程式啟動完成
                    process.WaitForInputIdle();
                    Console.WriteLine($"{processName} 已啟動並準備好");

                    // 確保窗口不會最大化
                    IntPtr hWnd = process.MainWindowHandle;
                    if (hWnd != IntPtr.Zero)
                    {
                        ShowWindow(hWnd, 9);
                    }
                }
            }
            return true;
        }
        public override bool Process(ref string strOutData)
        {
            try
            {
                AutomationElement mainWindow = AutomationElement.RootElement.FindFirst(TreeScope.Descendants,
                new PropertyCondition(AutomationElement.NameProperty, "安霸EVK固件加載器"));

                if (mainWindow != null)
                {
                    //TraverseAndDisplayElements(mainWindow);
                    //LogMessage("找到主視窗。");

                    // 找到所有按鈕
                    AutomationElementCollection buttons = mainWindow.FindAll(TreeScope.Descendants,
                        new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Button));

                    //if(Maximize)
                    //{
                    //    if (buttons.Count > 0)
                    //    {
                    //        int count = 0;
                    //        foreach (AutomationElement button in buttons)
                    //        {
                    //            // 獲取按鈕的名稱
                    //            string buttonName = button.Current.Name;
                    //            if (buttonName == "最大化")
                    //            {
                    //                InvokePattern invokePattern = button.GetCurrentPattern(InvokePattern.Pattern) as InvokePattern;
                    //                invokePattern.Invoke();
                    //                LogMessage("Invoke Maximize button");
                    //            }
                    //            if (Buttoncount != -1)
                    //            {
                    //                if (count == Buttoncount)
                    //                {
                    //                    InvokePattern invokePattern = button.GetCurrentPattern(InvokePattern.Pattern) as InvokePattern;
                    //                    invokePattern.Invoke();
                    //                }
                    //            }
                    //            count++;
                    //        }
                    //    }
                    //    else
                    //    {
                    //        LogMessage("Can't find Maximize button");
                    //    }
                    //}


                    AutomationElementCollection textboxes = mainWindow.FindAll(TreeScope.Descendants,
                    new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Edit));

                    if (textboxes.Count > 0)
                    {
                        int count = 0;
                        foreach (AutomationElement textbox in textboxes)
                        {
                            // 獲取文本框的名稱
                            string textboxName = textbox.Current.Name;
                            //Console.WriteLine("文本框的名稱是: " + textboxName);

                            // 設定文本框的值
                            ValuePattern valuePattern = textbox.GetCurrentPattern(ValuePattern.Pattern) as ValuePattern;
                            if (valuePattern != null && !valuePattern.Current.IsReadOnly)
                            {
                                if (count == 0)
                                    valuePattern.SetValue(CMD0);
                                if (count == 1)
                                    valuePattern.SetValue(CMD1);
                                if (count == 2)
                                    valuePattern.SetValue(CMD2);
                                if (count == 3)
                                    valuePattern.SetValue(CMD3);
                                if (count == 4)
                                    valuePattern.SetValue(CMD4);
                                if (count == 5)
                                    valuePattern.SetValue(CMD5);
                                if (count == 6)
                                    valuePattern.SetValue(CMD6);
                                if (count == 7)
                                    valuePattern.SetValue(CMD7);
                                if (count == 8)
                                {
                                    valuePattern.SetValue(FirmwareBin);
                                }
                                if (count == 9)
                                {
                                    valuePattern.SetValue(Textbox9);
                                }

                                //LogMessage("已設定文本框的值。");
                                count++;
                            }
                            else
                            {
                                LogMessage("無法設定文本框的值。");
                            }
                        }
                    }
                    else
                    {
                        LogMessage("Can't find textbox");
                        return false;
                    }


                    if (buttons.Count > 0)
                    {
                        int count = 0;
                        foreach (AutomationElement button in buttons)
                        {
                            string buttonName = button.Current.Name;
                            Console.WriteLine($"按鈕名稱: {buttonName}");

                            if (Buttoncount != -1)
                            {
                                if (button.Current.IsEnabled)
                                {
                                    if (count == Buttoncount)
                                    {
                                        InvokePattern invokePattern = button.GetCurrentPattern(InvokePattern.Pattern) as InvokePattern;
                                        invokePattern.Invoke();
                                    }
                                }
                                else
                                {
                                    return false;
                                }
                            }
                            count++;
                        }
                    }
                    else
                    {
                        LogMessage("Can't find button");
                        return false;
                    }

                    Thread.Sleep(Delay);
                    //AutomationElementCollection logElements = mainWindow.FindAll(TreeScope.Descendants,
                    //                new OrCondition(
                    //                    new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Text),
                    //                    new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Document)));

                    //if (logElements.Count > 0)
                    //{
                    //    foreach (AutomationElement logElement in logElements)
                    //    {
                    //        // 獲取日誌元件的文本內容
                    //        object patternObj;
                    //        if (logElement.TryGetCurrentPattern(TextPattern.Pattern, out patternObj))
                    //        {
                    //            TextPattern textPattern = (TextPattern)patternObj;
                    //            string logText = textPattern.DocumentRange.GetText(-1).Trim();
                    //            Console.WriteLine($"日誌內容: {logText}");
                    //        }
                    //        else
                    //        {
                    //            Console.WriteLine("無法獲取日誌元件的文本內容");
                    //        }
                    //    }
                    //}
                    //else
                    //{
                    //    Console.WriteLine("找不到任何日誌元件");
                    //}

                    if (buttons.Count > 0)
                    {
                        foreach (AutomationElement button in buttons)
                        {
                            // 獲取按鈕的名稱
                            string buttonName = button.Current.Name;
                            if (buttonName == "最小化")
                            {
                                InvokePattern invokePattern = button.GetCurrentPattern(InvokePattern.Pattern) as InvokePattern;
                                invokePattern.Invoke();
                                LogMessage("Invoke Minimize button");
                            }
                        }
                    }
                    else
                    {
                        LogMessage("Can't find button");
                    }

                }
                else
                {

                    LogMessage("找不到目標應用程式的視窗。");
                    return false;
                }

                LogMessage(strOutData);

            }
            catch (Exception ex)
            {
                LogMessage(ex.Message);
                return false;
            }


            return true;
        }
        public override bool PostProcess()
        {
            return true;

        }
        static bool IsValidPath(string path)
        {
            try
            {
                var result = Path.GetFullPath(path);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        static void TraverseAndDisplayElements(AutomationElement element)
        {
            // 顯示當前元件的類型
            Console.WriteLine($"元件名稱: {element.Current.Name}, 類型: {element.Current.ControlType.ProgrammaticName}");

            // 遍歷所有子元件
            AutomationElementCollection children = element.FindAll(TreeScope.Descendants, new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Custom));
            ValuePattern valuePattern = element.GetCurrentPattern(ValuePattern.Pattern) as ValuePattern;

            if (element.Current.AutomationId == "MainWindow.centralWidget.boardInfoFrame")
            {
                //AutomationElementCollection children = element.FindAll(TreeScope.Children, System.Windows.Automation.Condition.TrueCondition);
                ValuePattern valuePattern2 = element.GetCurrentPattern(ValuePattern.Pattern) as ValuePattern;

                object patternObj;
                if (element.TryGetCurrentPattern(TextPattern.Pattern, out patternObj))
                {
                    TextPattern textPattern = (TextPattern)patternObj;
                    string logText = textPattern.DocumentRange.GetText(-1).Trim();
                    Console.WriteLine($"日誌內容: {logText}");
                }
            }
            foreach (AutomationElement child in children)
            {
                TraverseAndDisplayElements(child);
            }
        }
    }
}
