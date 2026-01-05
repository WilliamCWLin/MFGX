
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
    internal class Script_Extra_AmbaUSB_UIControl : Script_Extra_Base
    {

        [Category("App Params"), Description("Value")]
        public string APPFilePath { get; set; } = @"C:\Program Files\Ambarella\AmbaUSB\bin\ambausb.exe";

        [Category("App Params"), Description("Value")]
        public string ProcessName { get; set; } = "ambausb";

        [Category("Action Mode"), Description("Value")]
        public Mode ActionMode { get; set; } = Mode.Unlock;
        [Category("Unlock Mode"), Description("Value")]
        public string UST_FILE_PATH  { get; set; } = @"D:\verkada-delorean-5mp-secure-liteon-0927\cv75_lpddr5_540MHz-unlock.ust";
        [Category("CheckString"), Description("Check Key")]
        public string CheckOKString { get; set; } = "DRAM size is initialized to 4096 MB by UST, accessible size is 4096 MB";

        [Category("CheckString"), Description("Fail Keywords (comma separated)")]
        public string CheckFailKeywords { get; set; } = "fail,error,錯誤,失敗,Exception,abort";
        //[Category("CheckString"), Description("Check Key")]
        //public string CheckFailString { get; set; } = "";
        public enum Mode
        {
            Unlock,
            Flash
        }
        [Category("Burn Mode"), Description("Value")]
        public string CMD0 { get; set; } = "";
        [Category("Burn Mode"), Description("Value")]
        public string CMD1 { get; set; } = "";
        [Category("Burn Mode"), Description("Value")]
        public string CMD2 { get; set; } = "";
        [Category("Burn Mode"), Description("Value")]
        public string CMD3 { get; set; } = "";
        [Category("Burn Mode"), Description("Value")]
        public string CMD4 { get; set; } = "";
        [Category("Burn Mode"), Description("Value")]
        public string CMD5 { get; set; } = "";
        [Category("Burn Mode"), Description("Value")]
        public string CMD6 { get; set; } = "";
        [Category("Burn Mode"), Description("Value")]
        public string CMD7 { get; set; } = "";
        [Category("Burn Mode"), Description("Value")]
        public string FirmwareBin { get; set; } = "";
        [Category("Burn Mode"), Description("Value")]
        public string Textbox9 { get; set; } = "";


        [Category("CheckString"), Description("Wait Time")]
        public int Delay { get; set; } = 8000;

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool mouse_event(uint hWnd, uint hWindInsertAfter, uint X, uint Y, int cx);
        const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        const uint MOUSEEVENTF_LEFTUP = 0x0004;

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

            string processName = ProcessName; // 這裡填入你要檢查的應用程式名稱
            string applicationPath = APPFilePath; // 這裡填入應用程式的路徑

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
                    Thread.Sleep(2000);
                }
            }
            return true;
        }
        public bool ChecklogKey()
        {
            AutomationElement mainWindow = AutomationElement.RootElement.FindFirst(TreeScope.Descendants,
                new PropertyCondition(AutomationElement.NameProperty, "安霸EVK固件加載器"));

            if (mainWindow == null)
            {
                LogMessage("找不到主視窗");
                return false;
            }

            AutomationElement LogText = mainWindow.FindFirst(TreeScope.Descendants,
                new PropertyCondition(AutomationElement.AutomationIdProperty, "MainWindow.centralWidget.logGroupBox.logTextEdit"));

            if (LogText == null)
            {
                LogMessage("找不到 LogText 元件");
                return false;
            }

            int timeout = Delay;
            int interval = 2000;
            int elapsed = 0;

            string[] failKeywords = CheckFailKeywords.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            while (elapsed < timeout)
            {
                if (LogText.TryGetCurrentPattern(ValuePattern.Pattern, out object pattern))
                {
                    ValuePattern valuePattern = (ValuePattern)pattern;
                    string logContent = valuePattern.Current.Value;

                    if (!string.IsNullOrEmpty(logContent))
                    {
                        if (!string.IsNullOrEmpty(CheckOKString) && logContent.Contains(CheckOKString))
                        {
                            return true;
                        }

                        foreach (var keyword in failKeywords)
                        {
                            if (logContent.Contains(keyword.Trim()))
                            {
                                LogMessage($"偵測到失敗訊息: {keyword}");
                                return false;
                            }
                        }
                    }
                }

                Thread.Sleep(interval);
                elapsed += interval;
            }

            LogMessage("Timeout 未偵測到成功訊息");
            return false;
        }

        //public bool ChecklogKey()
        //{
        //    AutomationElement mainWindow = AutomationElement.RootElement.FindFirst(TreeScope.Descendants,
        //        new PropertyCondition(AutomationElement.NameProperty, "安霸EVK固件加載器"));

        //    bool keywordfound = false;

        //    Thread.Sleep(Delay);

        //    AutomationElement LogText = mainWindow.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.AutomationIdProperty, "MainWindow.centralWidget.logGroupBox.logTextEdit"));

        //    if (LogText.TryGetCurrentPattern(ValuePattern.Pattern, out object pattern))
        //    {
        //        ValuePattern invokepattern = (ValuePattern)pattern;
        //        string checkstring = invokepattern.Current.Value;

        //        if (!string.IsNullOrEmpty(CheckOKString))
        //        {
        //            if (!checkstring.Contains(CheckOKString))
        //            {
        //                LogMessage($"{checkstring}Can't match CheckOKString");
        //            }
        //            else
        //            {
        //                keywordfound = true;
        //            }
        //        }
        //        else
        //        {
        //            return true;
        //        }
        //        //if (!string.IsNullOrEmpty(CheckFailString))
        //        //{
        //        //    if (checkstring.Contains(CheckFailString))
        //        //    {
        //        //        LogMessage("Match CheckFailString");
        //        //        keywordfound = false;
        //        //    }
        //        //}
                    
        //    }     
        //    else
        //    {
        //        LogMessage("Can't find clear LogText");
        //        return false;
        //    }
                
        //    return keywordfound;

        //}
        public bool Unlock()
        {
            AutomationElement mainWindow = AutomationElement.RootElement.FindFirst(TreeScope.Descendants,
                            new PropertyCondition(AutomationElement.NameProperty, "安霸EVK固件加載器"));

            AutomationElementCollection FormElements = mainWindow.FindAll(TreeScope.Subtree, System.Windows.Automation.Condition.TrueCondition);
            foreach (AutomationElement Element in FormElements)
            {
                Console.WriteLine($"AutomationId: {Element.Current.AutomationId},Current.Name:{Element.Current.Name},ControlType:{Element.Current.ControlType},NativeWindowHandle:{Element.Current.NativeWindowHandle}");
            }
            AutomationElement ClearBtn = mainWindow.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.AutomationIdProperty, "MainWindow.centralWidget.logGroupBox.clearLogButton"));
            AutomationElement ListTree = mainWindow.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.AutomationIdProperty, "MainWindow.centralWidget.boardsListFrame.camTreeView"));
           
            if (ClearBtn != null)
            {
                InvokePattern invokePattern = ClearBtn.GetCurrentPattern(InvokePattern.Pattern) as InvokePattern;
                invokePattern.Invoke();

            }
            else
            {
                LogMessage("Can't find clear button");
                return false;
            }

            if (ListTree != null)
            {
                AutomationElementCollection ListTreeComponent = ListTree.FindAll(TreeScope.Subtree, System.Windows.Automation.Condition.TrueCondition);
                int CC = 0;
                foreach (AutomationElement element2 in ListTreeComponent)
                {
                    if (CC == 16)
                    {
                        int x = (int)(element2.Current.BoundingRectangle.Left + element2.Current.BoundingRectangle.Width / 2);
                        int y = (int)(element2.Current.BoundingRectangle.Top + element2.Current.BoundingRectangle.Height / 2);

                        Cursor.Position = new System.Drawing.Point(x, y);
                        mouse_event(MOUSEEVENTF_LEFTDOWN, (uint)x, (uint)y, 0, 0);
                        mouse_event(MOUSEEVENTF_LEFTUP, (uint)x, (uint)y, 0, 0);

                        Thread.Sleep(1000);

                        AutomationElement newForm = AutomationElement.RootElement.FindFirst(TreeScope.Children,
                           new PropertyCondition(AutomationElement.NameProperty, "Amboot工具向導"));

                        if (newForm != null)
                        {
                            AutomationElementCollection newFormElements = newForm.FindAll(TreeScope.Descendants, System.Windows.Automation.Condition.TrueCondition);

                            foreach (AutomationElement Element in newFormElements)
                            {
                                Console.WriteLine($"AutomationId: {Element.Current.AutomationId},Current.Name:{Element.Current.Name},ControlType:{Element.Current.ControlType},NativeWindowHandle:{Element.Current.NativeWindowHandle}");
                            }


                            AutomationElement LineEditBld = newForm.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.AutomationIdProperty, "AmbootWizard.frame_3.frameBld.lineEditBld"));
                            if(!LineEditBld.Current.IsEnabled)
                            {

                                
                                LogMessage("Edit is disable");
                                AutomationElement CancelButton = newForm.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.NameProperty, "Cancel"));
                                if (CancelButton != null)
                                {
                                    InvokePattern invokePattern = CancelButton.GetCurrentPattern(InvokePattern.Pattern) as InvokePattern;
                                    invokePattern.Invoke();

                                }
                                else
                                {
                                    MessageBox.Show("Please Close windows");
                                    return false;
                                }
                                return false;
                            }
                            if (LineEditBld.TryGetCurrentPattern(ValuePattern.Pattern, out object ust_path_pattern))
                            {
                                AutomationElement FindBtn = newForm.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.AutomationIdProperty, "AmbootWizard.frame_3.frameBld.btnFindBld"));
                                if (FindBtn != null)
                                {
                                    InvokePattern invokePattern = FindBtn.GetCurrentPattern(InvokePattern.Pattern) as InvokePattern;
                                    invokePattern.Invoke();
                                    Thread.Sleep(1500);
                                    AutomationElement ustFileFindForm = AutomationElement.RootElement.FindFirst(TreeScope.Descendants,
                                                    new PropertyCondition(AutomationElement.ClassNameProperty, "#32770"));
                                    //AutomationElementCollection ustFileFindForms = AutomationElement.RootElement.FindAll(TreeScope.Descendants,
                                    //                new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Window));
                                    //foreach (AutomationElement Element in ustFileFindForms)
                                    //{
                                    //    Console.WriteLine($"Form AutomationId: {Element.Current.AutomationId},Current.Name:{Element.Current.Name},ControlType:{Element.Current.ControlType},NativeWindowHandle:{Element.Current.NativeWindowHandle}");
                                    //}
                                    if(ustFileFindForm != null)
                                    {
                                        AutomationElementCollection ustFileFindFormElements = ustFileFindForm.FindAll(TreeScope.Descendants, System.Windows.Automation.Condition.TrueCondition);
                                        foreach (AutomationElement Element in ustFileFindFormElements)
                                        {
                                            if (Element.Current.AutomationId == "1148" && Element.Current.ClassName == "Edit")
                                            {
                                                if (Element.TryGetCurrentPattern(ValuePattern.Pattern, out object ust_path_edit))
                                                {
                                                    ValuePattern valueust_path_editPattern = ust_path_edit as ValuePattern;
                                                    valueust_path_editPattern.SetValue(UST_FILE_PATH);
                                                }

                                            }
                                            //Console.WriteLine($"File  AutomationId: {Element.Current.AutomationId},Current.Name:{Element.Current.Name},ControlType:{Element.Current.ControlType},NativeWindowHandle:{Element.Current.NativeWindowHandle}");
                                            //Console.WriteLine($"File  AutomationId: {Element.Current.AutomationId},Current.Name:{Element.Current.Name},ControlType:{Element.Current.ControlType},NativeWindowHandle:{Element.Current.NativeWindowHandle}");
                                        }

                                        AutomationElement OpenBtn = ustFileFindForm.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.AutomationIdProperty, "1"));
                                        if (OpenBtn != null)
                                        {
                                            InvokePattern invokeOpenBtnPattern = OpenBtn.GetCurrentPattern(InvokePattern.Pattern) as InvokePattern;
                                            invokeOpenBtnPattern.Invoke();
                                        }
                                    }
                                    else
                                    {
                                        AutomationElement CancelButton = newForm.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.NameProperty, "Cancel"));
                                        if (CancelButton != null)
                                        {
                                            InvokePattern invokeCancelButtonPattern = CancelButton.GetCurrentPattern(InvokePattern.Pattern) as InvokePattern;
                                            invokeCancelButtonPattern.Invoke();
                                        }

                                        return false;
                                    }
                                }
                                else
                                {
                                    MessageBox.Show("Please Close windows");
                                    return false;
                                }

                                ValuePattern valuePattern = ust_path_pattern as ValuePattern;
                                valuePattern.SetValue(UST_FILE_PATH);
                                Thread.Sleep(1000);
                            }
                            else
                            {
                                AutomationElement CancelButton = newForm.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.NameProperty, "Cancel"));
                                if (CancelButton != null)
                                {
                                    InvokePattern invokePattern = CancelButton.GetCurrentPattern(InvokePattern.Pattern) as InvokePattern;
                                    invokePattern.Invoke();
                                }

                                return false;
                            }

                            AutomationElement OKButton = newForm.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.NameProperty, "OK"));

                            if (OKButton != null)
                            {
                                InvokePattern invokePattern = OKButton.GetCurrentPattern(InvokePattern.Pattern) as InvokePattern;
                                invokePattern.Invoke();

                            }
                            else
                            {
                                return false;
                            }
                        }
                    }
                    //Console.WriteLine($"{element2.Current.Name} ID:{element2.Current.AutomationId}");
                    CC++;

                }
            }
            else
            {
                return false;
            }

            if(!ChecklogKey())
            {
                LogMessage("Can't find clear LogText");
                return false;
            }
            else
            {
                return true;
            }

        }

        public bool Burn()
        {
            AutomationElement mainWindow = AutomationElement.RootElement.FindFirst(TreeScope.Descendants,
                            new PropertyCondition(AutomationElement.NameProperty, "安霸EVK固件加載器"));
            AutomationElement LogText = mainWindow.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.AutomationIdProperty, "MainWindow.centralWidget.logGroupBox.logTextEdit"));
            AutomationElement Element_CMD0 = mainWindow.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.AutomationIdProperty, "MainWindow.centralWidget.preHookCmdFrame.lineEditCmd0"));
            AutomationElement Element_CMD1 = mainWindow.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.AutomationIdProperty, "MainWindow.centralWidget.preHookCmdFrame.lineEditCmd1"));
            AutomationElement Element_CMD2 = mainWindow.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.AutomationIdProperty, "MainWindow.centralWidget.preHookCmdFrame.lineEditCmd2"));
            AutomationElement Element_CMD3 = mainWindow.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.AutomationIdProperty, "MainWindow.centralWidget.preHookCmdFrame.lineEditCmd3"));
            AutomationElement Element_CMD4 = mainWindow.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.AutomationIdProperty, "MainWindow.centralWidget.preHookCmdFrame.lineEditCmd4"));
            AutomationElement Element_CMD5 = mainWindow.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.AutomationIdProperty, "MainWindow.centralWidget.preHookCmdFrame.lineEditCmd5"));
            AutomationElement Element_CMD6 = mainWindow.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.AutomationIdProperty, "MainWindow.centralWidget.preHookCmdFrame.lineEditCmd6"));
            AutomationElement Element_CMD7 = mainWindow.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.AutomationIdProperty, "MainWindow.centralWidget.preHookCmdFrame.lineEditCmd7"));
            AutomationElement FirmwareFile = mainWindow.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.AutomationIdProperty, "MainWindow.centralWidget.boardGroupBox.inputFrame.firmwareFrame.fileComboBox"));

            AutomationElement ClearBtn = mainWindow.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.AutomationIdProperty, "MainWindow.centralWidget.logGroupBox.clearLogButton"));
            AutomationElement DownloadBtn = mainWindow.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.AutomationIdProperty, "MainWindow.centralWidget.boardGroupBox.downloadButton"));
            AutomationElement ListTree = mainWindow.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.AutomationIdProperty, "MainWindow.centralWidget.boardsListFrame.camTreeView"));
            if (ClearBtn != null)
            {
                InvokePattern invokePattern = ClearBtn.GetCurrentPattern(InvokePattern.Pattern) as InvokePattern;
                invokePattern.Invoke();

            }
            else
            {
                LogMessage("Can't find clear button");
                return false;
            }

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

                }
            }
            else
            {
                LogMessage("Can't find textbox");
                return false;
            }



            if (DownloadBtn != null)
            {
                InvokePattern invokePattern = DownloadBtn.GetCurrentPattern(InvokePattern.Pattern) as InvokePattern;
                invokePattern.Invoke();

            }
            else
            {
                LogMessage("Can't find clear button");
                return false;
            }

            if (!ChecklogKey())
            {
                LogMessage("Can't find clear LogText");
                return false;
            }
            else
                return true;

        }
        public override bool Process(ref string strOutData)
        {
            try
            {
                AutomationElement mainWindow = AutomationElement.RootElement.FindFirst(TreeScope.Descendants,
                    new PropertyCondition(AutomationElement.NameProperty, "安霸EVK固件加載器"));
                AutomationElement MinimizeBtn = mainWindow.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.AutomationIdProperty, "Minimize"));

                if (ActionMode == Mode.Unlock)
                {
                    bool ret =  Unlock();

                    if (MinimizeBtn != null)
                    {
                        Thread.Sleep(2000);
                        InvokePattern invokePattern = MinimizeBtn.GetCurrentPattern(InvokePattern.Pattern) as InvokePattern;
                        invokePattern.Invoke();
                        
                        LogMessage("invoke MinimizeBtn");
                    }
                    else
                    {
                        LogMessage("Can't find MinimizeBtn button");
                        return false;
                    }

                    return ret;
                }
                else if(ActionMode == Mode.Flash)
                {
                    bool ret = Burn();
                    if (MinimizeBtn != null)
                    {
                        Thread.Sleep(1000);
                        InvokePattern invokePattern = MinimizeBtn.GetCurrentPattern(InvokePattern.Pattern) as InvokePattern;
                        invokePattern.Invoke();

                        LogMessage("invoke MinimizeBtn");
                    }
                    else
                    {
                        LogMessage("Can't find MinimizeBtn button");
                        return false;
                    }

                    return ret;
                }

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
