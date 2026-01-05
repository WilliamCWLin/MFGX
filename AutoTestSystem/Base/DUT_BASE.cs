using AutoTestSystem.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.Text.RegularExpressions;
using AutoTestSystem.Script;
using AutoTestSystem.BLL;
using ZXing.Common;
using ZXing.Rendering;
using ZXing;
using AutoTestSystem.DUT;
using static AutoTestSystem.MainForm;
using System.Diagnostics;
using CsvHelper;
using System.IO;
using Manufacture;

namespace AutoTestSystem.Base
{
    public abstract class DUT_BASE : Manufacture.TerminalNode, IDisposable
    {
        //%ProjectName%_%StationName%_%ConfigVersion%
        [Category("Base"), Description("Config Common parameter"), Editor(typeof(DUTEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public string Config { get; set; } =
            @"[
          {""Key"":""METALOGPATH"",""Value"":""\\172.16.1.80\\Testlog\\camera\\%ProjectName%\\%StationName%\\%FixtureName%\\%NowTimeyyyy-MM-dd%\\%CheckFailitem%\\%ProductSN%""},
          {""Key"":""TESTLOGPATH"",""Value"":""\\172.16.1.80\\Testlog\\camera\\%ProjectName%\\%StationName%\\%FixtureName%\\%NowTimeyyyy-MM-dd%\\Daily.csv""}
        ]";
        [Category("Base"), Description("Multi-Devices Table多工測試共用腳本統一裝置名使用"), Editor(typeof(Muti_DeviceEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public string MultiDeviceTable { get; set; } = string.Empty;
        [Category("Base"), Description("Table Data"), Editor(typeof(DataTableEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public string TableData { get; set; }
        /*
         * [Path]
            LogPath=D:\YourProject\Log\%ProjectName%\%RunMode%\%StationName%\%FixtureName%\%NowTimeyyyyMMdd%\%TestResult%\%ProductSN%_%NowTimeHH_mm_ss%.txt
            ServerLogPath=D:\YourProject\Backup\Log\%ProjectName%\%RunMode%\%StationName%\%FixtureName%\%NowTimeyyyyMMdd%\%TestResult%\%ProductSN%_%NowTimeHH_mm_ss%.txt
         * */
        [Category("Base"), Description("Environment Variable"), Editor(typeof(ConfigEditor), typeof(UITypeEditor))]
        //public string EnvirVariable { get; set; } = "[SerialNumber]\r\nProductSN=[A-Z0-9]{2}\r\n[UI]\r\nTreeShow=0\r\nTitleBarcodeShow=0\r\nSignalLightShow=0\r\nTitleShow=1\r\n[Path]\r\nLogPath=D:\\YourProject\\Log\\%ProjectName%\\%RunMode%\\%StationName%\\%FixtureName%\\%NowTimeyyyyMMdd%\\%TestResult%\\%ProductSN%_%NowTimeHH_mm_ss%.txt\r\nServerLogPath=D:\\YourProject\\Backup\\Log\\%ProjectName%\\%RunMode%\\%StationName%\\%FixtureName%\\%NowTimeyyyyMMdd%\\%TestResult%\\%ProductSN%_%NowTimeHH_mm_ss%.txt";
        public string EnvirVariable { get; set; } = "[SerialNumber]\r\nProductSN=[A-Z0-9]{2}\r\n[UI]\r\nTreeShow=1\r\nTitleBarcodeShow=0\r\nSignalLightShow=0\r\nTitleShow=1\r\nRunMode=0\r\n[Path]\r\nBackupFolder=C:\\Camera\\%ProjectName%\\%StationName%\\%FixtureName%\\\r\n[Common]\r\nLogFileName=%ProductSN%_%ProjectName%_%StationName%_%FixtureName%_%FixturePart%_%NowTimeHHmmss%\r\nFixturePort=\r\nLogUseItemCode=0";


        [JsonIgnore]
        public MLog LOGGER;


        [JsonIgnore]
        public Stopwatch stopwatch = new Stopwatch();

        [JsonIgnore]
        [Browsable(false)]
        public Dictionary<string, string> EnvirConfig { get; set; } = new Dictionary<string, string>();

        [Category("Base"), Description("Config Common parameter"), Editor(typeof(TableEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public string CommandTable { get; set; }

        [JsonIgnore]
        [Browsable(false)]
        public TestUnit testUnit { get; set; }

        [JsonIgnore]
        [Browsable(false)]
        public bool TestResult { get; set; } = false;
        [JsonIgnore]
        private Dashboard _dutdashboard;
        [JsonIgnore]
        private DataCollection _dcData;

        [JsonIgnore]
        private TestInfoRecorder InfoRec;

        [JsonIgnore]
        private DataGridView _dataGridView;

        [JsonIgnore]
        [Browsable(false)]
        View _vlcviewer/* = new View()*/;

        [JsonIgnore]
        public bool isRunning;

        [JsonIgnore]
        [Browsable(false)]
        public bool isSimu;
        [JsonIgnore]
        [Browsable(false)]
        public View VlcForm
        {
            get { return _vlcviewer; }
            set { _vlcviewer = value; }
        }
        [JsonIgnore]
        [Browsable(false)]
        public DataCollection DataCollection
        {
            get { return _dcData; }
            set { _dcData = value; }
        }
        [JsonIgnore]
        [Browsable(false)]
        public TestInfoRecorder TestInfo
        {
            get { return InfoRec; }
            set { InfoRec = value; }
        }
        [JsonIgnore]
        [Browsable(false)]
        public DataGridView DataGridView
        {
            get { return _dataGridView; }
            set { _dataGridView = value; }
        }


        [Browsable(false)]
        public event EventHandler<MyEventArgs> ProgressChanged;
        [JsonIgnore]
        [Browsable(false)]
        Container_MainThread m_MainThread = null;


        [JsonIgnore]
        [Browsable(false)]
        public Container_MainThread MainThread
        {
            get { return m_MainThread; }
            set { m_MainThread = value; }
        }
        [JsonIgnore]
        [Browsable(false)]
        public Dashboard DutDashboard
        {
            get { return _dutdashboard; }
            set { _dutdashboard = value; }
        }

        [Browsable(false)] // 隱藏父類的屬性
        public new string ExID { get; set; }
        [Browsable(false)] // 隱藏父類的屬性
        public  new bool ShowItem { get; set; }
        [Browsable(false)] // 隱藏父類的屬性
        public new string FailStopGoto { get; set; }

        private TestFileManager testFileManager;
        [JsonIgnore]
        [Browsable(false)]
        public TestFileManager FileManager
        {
            get { return testFileManager; }
            set { testFileManager = value; }
        }
        public DUT_BASE()
        {
            if (string.IsNullOrEmpty(GlobalNew.FormMode) || GlobalNew.FormMode == "0")
            {
                _dcData = new DataCollection();
                _dataGridView = new DataGridView();
                InfoRec = new TestInfoRecorder();
                //_bindingSource = new BindingSource();
                isRunning = false;
                isSimu = false;
                //DUTLogger = LogManager.GetLogger("DUTLogger");
            }
            else
            {
                _dcData = new DataCollection();
                _dataGridView = new DataGridView();
                EnvirConfig = new Dictionary<string, string>();
                InfoRec = new TestInfoRecorder();
                isRunning = false;
                isSimu = false;
                testUnit = new TestUnit();
            }


        }
        protected virtual void OnProgressChanged(int pass_value, int fail_value)
        {
            ProgressChanged?.Invoke(this, new MyEventArgs(pass_value, fail_value));
        }
        private void HomeBtn_ClickAsync(object sender, EventArgs e)
        {
            Button clickedButton = sender as Button;

            if (clickedButton != null)
            {
                GO_HOME();
                DutDashboard.ClearTestItemView(this);
                DutDashboard.MemoryDataClear(this);
                DutDashboard.SetTestStatus(this, TestStatus.IDLE);
                DutDashboard.SetTrafficLight("");
            }
        }
        public bool GO_HOME()
        {
            bool isHomeSuccess = false;
            bool isHomeStart = false;
            foreach (TreeNode m in DutDashboard.MainProTreeView.GetTreeview().Nodes)
            {
                foreach (TreeNode n in m.Nodes)
                {
                    if (n.Tag is Container_JIG_INIT == true)
                    {
                        //JIG_InitailNode = n;
                        isHomeStart = true;
                        BlockingForm blockingForm = new BlockingForm();

                        int ResetRes = 0;
                        bool TestResult = false;
                        object[] context = new object[] { DutDashboard.MainProTreeView, false, this, TestResult, GlobalNew.Devices, null };

                        var task1 = Task.Factory.StartNew(() =>
                        {
                            ResetRes = ((Container_JIG_INIT)n.Tag).Process(n, context);
                        });
                        task1.ContinueWith(t =>
                        {
                            blockingForm.Close();
                            if (ResetRes == 1)
                            {
                                DutDashboard.ResetUISNtext();
                                isHomeSuccess = true;
                            }
                            else
                            {
                                LogMessage($"{Description}:Initial&GoHome Fail", MessageLevel.Error);
                            }
                            MemoryDataClear();
                        }, TaskScheduler.FromCurrentSynchronizationContext()); // 這樣會確保 ContinueWith 在 UI 主線程上執行}

                        blockingForm.ShowDialog();

                        //只能有一個HOME容器運行
                        break;
                    }
                }
            }

            if (isHomeStart == false)
            {
                DutDashboard.ResetUISNtext();
            }

            return false;
        }


        public void MemoryDataClear()
        {
            try
            {
                TestInfo.ClearTestSteps();
                DataCollection.Clear();
                DataCollection.SetMoreProp("Failitem", "");
                TraverseTreeViewClearDataItem(DutDashboard.MainProTreeView.GetTreeview().Nodes);
            }
            catch (Exception ex)
            {
                LogMessage($"MemoryDataClear Fail.Message:{ex.Message}", MessageLevel.Fatal);
            }
        }

        private void TraverseTreeViewClearDataItem(TreeNodeCollection nodes)
        {
            foreach (TreeNode node in nodes)
            {
                // 對節點的 Tag 做一些事情
                if (node.Tag is ScriptBase)
                {
                    // 清除測試資料
                    ScriptBase tagObject = (ScriptBase)node.Tag;
                    tagObject.RowDataItem.Clear();
                    tagObject.RowDataItem.RetryTimes = 0;
                    tagObject.RowDataItem.TestCount = 0;
                }
                if (node.Tag is CoreBase)
                {
                    // 清除測試資料
                    CoreBase tagObject = (CoreBase)node.Tag;
                    tagObject.DurationSec = -1;
                    tagObject.RetryCount = -1;
                    tagObject.StartTime = DateTime.Now;
                    tagObject.EndTime = DateTime.Now;
                    tagObject.Result = "N/A";

                }
                // 如果節點有子節點，遞迴地呼叫 TraverseTreeView 方法
                if (node.Nodes.Count > 0)
                {
                    TraverseTreeViewClearDataItem(node.Nodes);
                }
            }
        }
        public void UI_Create(bool isOneDutMode)
        {
            ParseConfig(EnvirVariable, EnvirConfig);
            _dutdashboard = new Dashboard(EnvirConfig);
            _dutdashboard.AttachEventHandler((sender, e) => HomeBtn_ClickAsync(sender, e));
            _dutdashboard.GenerateTextBoxes(ParseSectionData(EnvirVariable, "SerialNumber"));
            // 訂閱事件
            _dutdashboard.KeyPressHandled += RunKeyPressHandler;
            _dataGridView = _dutdashboard.DataGridView;
            _dutdashboard.Name = Description;
            _dutdashboard.DashBoardDescription.Text = Description;
            _dutdashboard.Dock = DockStyle.Fill;
            _dutdashboard.Margin = new Padding(0);
            
            LOGGER = new MLog();

            //============================================
            // 分割字串並提取路徑
            List<string> backupPaths = ParseSectionData(EnvirVariable, "Path")
                         .Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries)
                         .Select(line => line.Split('=')[1])
                         .ToList();

            // 如果 backupPaths 為空，添加預設路徑
            if (!backupPaths.Any())
            {
                string defaultBackupPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Backup");
                backupPaths.Add(defaultBackupPath);
            }

            testFileManager = new TestFileManager();
            //============================================

            var barcodeWriter = new BarcodeWriter
            {
                Format = BarcodeFormat.CODE_128,
                Options = new EncodingOptions
                {
                    Width = 300,
                    Height = 100,
                    Margin = 10
                },
                Renderer = new BitmapRenderer()
            };
            var barcodeBitmap = barcodeWriter.Write(Description);
            _dutdashboard.BarcodePicture.Image = barcodeBitmap;


            //    DUTLogger = LogManager.GetLogger("EquipmentLogger");
            //    Log4NetHelper.AddAppender(DUTLogger, _dutdashboard.DutLogRichTextBox);


            //_maintestitem = new Dut_TestItem(isOneDutMode);
            //DUTLogger = Log4NetHelper.GetLogger(this.GetType(), _maintestitem.DutLogRichTextBox);
            //_dashboard = new Dut_Dashboard(isOneDutMode);
            //_dashboard.AttachEventHandler((sender, e) => HomeBtn_ClickAsync(sender, e));


            // 設置 Dashboard 控制項的屬性
            //_dashboard.Dock = DockStyle.Fill;
            //_dashboard.Margin = new Padding(0);



            // 訂閱事件
            //_dashboard.KeyPressHandled += RunKeyPressHandler;

            //_dataGridView = _maintestitem.DataGridView;
            //_dashboard.Name = Description;
            //_dashboard.DashBoardDescription.Text = Description;

            //var barcodeWriter = new BarcodeWriter
            //{
            //    Format = BarcodeFormat.CODE_128,
            //    Options = new EncodingOptions
            //    {
            //        Width = 300,
            //        Height = 100,
            //        Margin = 10
            //    },
            //    Renderer = new BitmapRenderer()
            //};
            //var barcodeBitmap = barcodeWriter.Write(Description);
            //Dashboard.BarcodePicture.Image = barcodeBitmap;
            //barcodeBitmap.Save("barcode.png", ImageFormat.Png);

            //string treeWidthStr = DataCollection.GetMoreProp("TreeWidth");
            //if (float.TryParse(treeWidthStr, out float treeWidth))
            //{
            //    MainTestItem.TableTestitem_Panel.ColumnStyles[0].Width = treeWidth;
            //}
            //else
            //{
            //    MainTestItem.TableTestitem_Panel.ColumnStyles[0].Width = 0;
            //}
        }
        private double CheckMemoryUsage()
        {
            // 取得目前的進程
            Process currentProcess = Process.GetCurrentProcess();

            // 取得目前的記憶體使用量（以位元組為單位）
            long memoryUsage = currentProcess.PrivateMemorySize64;

            // 將記憶體使用量轉換為 MB
            double memoryUsageMB = memoryUsage / (1024.0 * 1024.0);
            LogMessage($"Start Memory.{memoryUsageMB:F2} MB");

            return memoryUsageMB;

        }
        public override void LogMessage(string message, MessageLevel mode = MessageLevel.Info)
        {
            switch (mode)
            {
                case MessageLevel.Debug:
                    LOGGER?.Debug($"[{Description}]  {message}");
                    break;
                case MessageLevel.Info:
                    LOGGER?.Info($"[{Description}] {message}");
                    break;
                case MessageLevel.Warn:
                    LOGGER?.Warn($"[{Description}]  {message}");
                    break;
                case MessageLevel.Error:
                    LOGGER?.Error($"[{Description}]  {message}");
                    break;
                case MessageLevel.Fatal:
                    LOGGER?.Fatal($"[{Description}]  {message}");
                    break;
                case MessageLevel.Raw:
                    LOGGER?.Raw($"{message}");
                    break;
            }

        }
        private double GetAvailableFreeSpace()
        {
            // 獲取當前可執行文件的目錄
            string exeDirectory = AppDomain.CurrentDomain.BaseDirectory;

            // 獲取硬碟槽的資訊
            DriveInfo driveInfo = new DriveInfo(Path.GetPathRoot(exeDirectory));

            // 獲取可用空間
            long availableFreeSpace = driveInfo.AvailableFreeSpace;

            double Free = availableFreeSpace / (1024.0 * 1024.0 * 1024.0);
            LogMessage($"FreeSpace.{Free:F2} GB");
            // 將容量轉換為 GB 並返回
            return Free;
        }

        static void RestartApplication()
        {
            try
            {
                // 取得當前程式的完整路徑
                string exePath = Application.ExecutablePath;

                // 啟動一個新的進程來執行當前程式
                Process.Start(exePath);

                // 終止當前進程
                Environment.Exit(0);
            }
            catch (Exception ex)
            {

            }
        }

        public void RunTree()
        {
            LOGGER.StetupWLog(Description, GlobalNew.LOGFOLDER, _dutdashboard.DutLogRichTextBox);

            if (CheckMemoryUsage() > 1500)
            {
                MessageBox.Show(
                    $"偵測到系統異常請重啟程式\nSystem error detected. Please restart the program.",
                    "警告", // 訊息框標題
                    MessageBoxButtons.OK, // 按鈕類型
                    MessageBoxIcon.Warning // 警告圖標
                );
                LogMessage($"偵測到系統異常請重啟程式\nSystem error detected. Please restart the program.");

                return;
            }

            double freedisk = GetAvailableFreeSpace();
            if (freedisk < 1.5)
            {

                MessageBox.Show(
                    $"偵測到系統異常請重啟程式FreeSpace({freedisk:F2}GB)\nSystem error detected. Please restart the program.",
                    "警告", // 訊息框標題
                    MessageBoxButtons.OK, // 按鈕類型
                    MessageBoxIcon.Warning // 警告圖標
                );
                LogMessage($"偵測到系統異常請重啟程式\nSystem error detected. Please restart the program.");

                return;
            }

            //***************鎖機***************
            //string flag = INIHelper.Readini("CountNum", "ABORT_FLAG", Global.IniConfigFile);
            //if (flag == "1")
            //{
            //    if (GlobalNew.CurrentUser != "pe")
            //    {
            //        DutDashboard.BlinkingTitle = true;
            //        MessageBox.Show("機台異常中，請切換PE帳號進行維修");
            //        return;
            //    }
            //}

            if (GlobalNew.CurrentMode != "PROD")
            {
                MessageBox.Show("注意工程或點檢模式勿用於生產中!!\n Engineering mode should not be used for production.", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                if (GlobalNew.CurrentUser == "rd")
                {
                    MessageBox.Show("勿使用RD帳號進行生產!!\n Do not use the RD account for production..", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    DutDashboard.ResetUISNtext(true);
                    return;
                }

                if (GlobalNew.CurrentUser == "pe")
                {
                    MessageBox.Show("勿使用PE帳號進行生產,僅供維修點檢測試用!!\n Do not use the PE account for production..", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            GlobalNew.g_isRunning = true;
            foreach (TreeNode n in DutDashboard.MainProTreeView.GetTreeview().Nodes)
            {
                if (n.Tag is Container_MainThread == true)
                {
                    Container_MainThread T = (Container_MainThread)n.Tag;
                    m_MainThread = T;
                    DutDashboard.SetCurrentThread(T);
                    if (T.isRunning == 0)
                    {
                        TestInfo.ClearTestSteps();
                        DataCollection.Clear();
                        DataCollection.SetMoreProp("Failitem", "");
                        SetConfig_Param();
                        bool TestResult = false;
                        object[] context = new object[] { DutDashboard.MainProTreeView, T.FailContinue, this, TestResult, GlobalNew.Devices, null, this };

                        n.EnsureVisible();

                        LogMessage("Start");
                        LogMessage($"{CheckMemoryUsage():F2}mb");
                        isRunning = true;
                        Task ta = T.ActPro(n, context);

                        ta.ContinueWith(t =>
                        {
                            m_MainThread = null;
                            DutDashboard.SetCurrentThread(null);
                            isRunning = false;
                            GlobalNew.g_isRunning = false;
                            OnProgressChanged(GlobalNew.Total_Pass_Num, GlobalNew.Total_Fail_Num);
                            DutDashboard.ResetUISNtext();
                        }, TaskScheduler.FromCurrentSynchronizationContext()); // 這樣會確保 ContinueWith 在 UI 主線程上執行}

                    }
                }
            }
        }

        public async Task RunLoopAsync()
        {
            DutDashboard.ResetDutInfo(this);

            while (!GlobalNew.g_shouldStop)
            {
                LOGGER.StetupWLog(Description, GlobalNew.LOGFOLDER, _dutdashboard.DutLogRichTextBox);

                foreach (TreeNode n in DutDashboard.MainProTreeView.GetTreeview().Nodes)
                {
                    if (n.Tag is Container_MainThread T)
                    {
                        m_MainThread = T;
                        DutDashboard.SetCurrentThread(T);

                        if (T.isRunning == 0)
                        {
                            TestInfo.ClearTestSteps();
                            DataCollection.Clear();
                            DataCollection.SetMoreProp("Failitem", "");
                            Container_Sequences.ClearSyncProp();
                            Script_Extra_GlobalPropManager.ClearSyncProp();
                            DutDashboard.MemoryDataClear(this);
                            SetConfig_Param();

                            bool TestResult = false;
                            object[] context = new object[] { DutDashboard.MainProTreeView, T.FailContinue, this, TestResult, GlobalNew.Devices, null, this };

                            n.EnsureVisible();

                            LogMessage("Start");
                            LogMessage($"{CheckMemoryUsage():F2}mb");

                            isRunning = true;

                            Task ta = T.ActPro(n, context);
                            await ta; // 等待緒完成

                            m_MainThread = null;
                            DutDashboard.SetCurrentThread(null);
                            isRunning = false;
                            OnProgressChanged(GlobalNew.Total_Pass_Num, GlobalNew.Total_Fail_Num);
                            DutDashboard.ResetUISNtext();
                        }
                    }
                }

                await Task.Delay(500); // 可選：避免 CPU 過度佔用，設定每輪間隔
            }

        }
        public async Task ForManageRunAsync()
        {
            LOGGER.StetupWLog(Description, GlobalNew.LOGFOLDER, _dutdashboard.DutLogRichTextBox);

            GlobalNew.g_isRunning = true;
            foreach (TreeNode n in DutDashboard.MainProTreeView.GetTreeview().Nodes)
            {
                if (n.Tag is Container_MainThread == true)
                {
                    Container_MainThread T = (Container_MainThread)n.Tag;
                    m_MainThread = T;
                    DutDashboard.SetCurrentThread(T);
                    if (T.isRunning == 0)
                    {
                        TestInfo.ClearTestSteps();
                        DataCollection.Clear();
                        DataCollection.SetMoreProp("Failitem", "");
                        SetConfig_Param();
                        bool TestResult = false;
                        object[] context = new object[] { DutDashboard.MainProTreeView, T.FailContinue, this, TestResult, GlobalNew.Devices, null, this };
                           
                        DutDashboard.TestTabControl.Invoke((Action)(() =>
                        {
                            n.EnsureVisible();
                        }));
                        LogMessage("Start");
                        LogMessage($"{CheckMemoryUsage():F2}mb");
                        isRunning = true;
                        Task ta = T.ActRotate(n, context);

                        await ta; // 等待緒完成

                        m_MainThread = null;
                        DutDashboard.SetCurrentThread(null);
                        isRunning = false;
                        OnProgressChanged(GlobalNew.Total_Pass_Num, GlobalNew.Total_Fail_Num);
                           
                    }
                }
            }           
        }
        public void RunKeyPressHandler(object sender, KeyPressEventArgs e)
        {
            RunTree();
        }



        private void CloseFileIfOpen(string filePath)
        {
            var fileName = Path.GetFileName(filePath);
            var processes = Process.GetProcesses().Where(p => p.MainWindowTitle.Contains(fileName)).ToList();

            foreach (var process in processes)
            {
                try
                {
                    process.Kill();
                    process.WaitForExit();
                }
                catch (Exception ex)
                {
                    LogMessage($"Failed to close process: {ex.Message}");
                }
            }
        }

        public virtual bool READ(ref string output, int timeout)
        {
            return true;
        }
        public virtual bool ClearBuffer()
        {
            return true;
        }

        public virtual bool PopAllLog()
        {
            return true;
        }
        public virtual bool Clear()
        {
            return true;
        }


        public virtual bool Status(ref string msg)
        {
            return true;
        }

        public abstract void Dispose();

        public abstract bool Init(string strParamInfo);
        public abstract bool UnInit();
        public abstract bool StartAction(string strItemName, string strParamIn, ref string strOutput);
        public abstract bool SEND(string input);
        public abstract bool SEND(byte[] input);

        public virtual bool READ(ref string output)
        {
            return true;
        }

        public virtual bool READ(ref string output, int length, int header, int tail)
        {
            return true;
        }

        public virtual bool READNOJSON(string ParamIn, ref string output)
        {
            return true;
        }

        public virtual bool READ(string ParamIn, ref string output)
        {
            return true;
        }

        public virtual bool Send_Read(string input, ref string output)
        {
            return true;
        }

        public virtual bool OPEN()
        {
            return true;
        }

        public virtual void SetTimeout(int time)
        {

        }

        //public override void LogMessage(string message, MessageLevel mode = MessageLevel.Info)
        //{
        //    if (DUTLogger == null)
        //        return;
        //    switch (mode)
        //    {
        //        case MessageLevel.Debug:
        //            DUTLogger.Debug($"[{/*GetType().Name*/Description}]  {message}");
        //            break;
        //        case MessageLevel.Info:
        //            DUTLogger.Info($"[{Description}]  {message}");
        //            break;
        //        case MessageLevel.Warn:
        //            DUTLogger.Warn($"[{Description}]  {message}");
        //            break;
        //        case MessageLevel.Error:
        //            DUTLogger.Error($"[{Description}]  {message}");
        //            break;
        //        case MessageLevel.Fatal:
        //            DUTLogger.Fatal($"[{Description}]  {message}");
        //            break;
        //    }

        //}
        public virtual void SetTimeout(int timeout_comport, int timeout_total) { }

        public virtual bool SendCGICommand(int request_type, string Checkstr, string CGICMD, string input, ref string output) { return true; }


        public virtual bool SaveImage(int Format_Mode, string strSavePath)
        {
            LogMessage("SaveImage Not Implemented");
            return true;
        }
        public virtual bool SaveImage(string strSavePath)
        {
            LogMessage("SaveImage Not Implemented");
            return true;
        }
        public virtual bool SaveImage(byte[] image, string strSavePath)
        {
            LogMessage("SaveImage Not Implemented");
            return true;
        }
        public virtual bool Play()
        {
            LogMessage("Play Not Implemented");
            return true;
        }
        public virtual bool Stop()
        {
            LogMessage("Stop Not Implemented");
            return true;
        }

        public virtual bool VideoInit(string rtspUrl)
        {
            LogMessage("VideoInit Not Implemented");
            return true;
        }

        public virtual bool VideoUnInit()
        {
            LogMessage("VideoUnInit Not Implemented");
            return true;
        }
        public virtual bool Preview(byte[] rgbBuffer)
        {
            LogMessage("Preview Not Implemented");
            return true;
        }
        public virtual byte[] CaptureImage()
        {
            LogMessage("Stop Not Implemented");
            return null;
        }
        public virtual bool CaptureImage(byte[] image)
        {
            LogMessage("Stop Not Implemented");
            return true;
        }
        public void SwitchTabControlIndex(int index)
        {

            if (DutDashboard == null)
                return;
            if (index >= DutDashboard.TestTabControl.TabCount)
                return;
            if (DutDashboard.TestTabControl.InvokeRequired)
            {
                DutDashboard.TestTabControl.Invoke((Action)(() =>
                {
                    DutDashboard.TestTabControl.SelectedIndex = index;
                }));
            }
            else
            {
                DutDashboard.TestTabControl.SelectedIndex = index;
            }
        }

        public void SetConfig_Param()
        {
            if (!string.IsNullOrEmpty(Config))
            {
                try
                {
                    List<Dictionary<string, string>> keyValuePairs = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(Config);

                    // 遍歷每個字典並調用 SetMoreProp 方法
                    foreach (var pair in keyValuePairs)
                    {
                        string key = string.Empty;
                        string value = string.Empty;
                        if (pair.Count > 1)
                        {
                            key = pair["Key"];
                            value = pair["Value"];
                            DataCollection.SetMoreProp(key, value);
                        }
                    }
                }
                catch (Exception param_ex)
                {
                    MessageBox.Show($"{param_ex}", "SetDutparam Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        public class DUTEditor : System.Drawing.Design.UITypeEditor
        {
            public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
            {
                using (var DUTDialog = new Form_SetVariable())
                {
                    if (value == null)
                        value = string.Empty;
                    DUTDialog.SetDUTForm_JSON(value.ToString());
                    if (DUTDialog.ShowDialog() == DialogResult.OK)
                    {
                        return DUTDialog.GetDUTForm_JSON();
                    }
                    else if (DUTDialog.ShowDialog() == DialogResult.Cancel)
                    {
                        MessageBox.Show($"The Dut param key or value exist \"Empty\",Please Check Dutparam From Setting", "SetDutparam Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }

                return value; // 如果用戶取消選擇，返回原始值
            }

            public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
            {
                return UITypeEditorEditStyle.Modal;
            }
        }
        public class Muti_DeviceEditor : System.Drawing.Design.UITypeEditor
        {
            public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
            {
                using (var MutiDevTable = new MutiDeviceSelect())
                {
                    // 如果有現有的值，將其加載到表單中
                    if (value != null)
                    {
                        MutiDevTable.LoadDataGridViewFromJson(value.ToString());
                    }

                    var result = MutiDevTable.ShowDialog();
                    string json = MutiDevTable.GetDataGridViewAsJson();
                    return MutiDevTable.GetDataGridViewAsJson();

                }

            }


            public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
            {
                return UITypeEditorEditStyle.Modal;
            }
        }
        public class DataTableEditor : System.Drawing.Design.UITypeEditor
        {
            public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
            {
                if (value == null)
                    value = string.Empty;
                using (var DUTDialog = new DataTableForm((string)value))
                {


                    if (DUTDialog.ShowDialog() == DialogResult.OK)
                    {
                        value = DUTDialog.JsonData;
                        // 處理 jsonData
                    }
                }

                return value; // 如果用戶取消選擇，返回原始值
            }

            public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
            {
                return UITypeEditorEditStyle.Modal;
            }
        }
        public class ConfigEditor : UITypeEditor
        {
            private string originalText;

            public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
            {
                return UITypeEditorEditStyle.Modal;
            }

            public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
            {
                IWindowsFormsEditorService editorService = provider.GetService(typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;

                if (editorService != null)
                {
                    TextBox textBox = new TextBox();
                    textBox.Multiline = true;
                    textBox.ScrollBars = ScrollBars.Both;
                    textBox.Text = value?.ToString();
                    textBox.Dock = DockStyle.Fill;

                    Button okButton = new Button();
                    okButton.Text = "OK";
                    okButton.DialogResult = DialogResult.OK;

                    Button cancelButton = new Button();
                    cancelButton.Text = "Cancel";
                    cancelButton.DialogResult = DialogResult.Cancel;

                    Form form = new Form();
                    form.Text = "Enter Command";
                    form.Size = new System.Drawing.Size(1200, 800);
                    form.StartPosition = FormStartPosition.CenterParent;
                    form.Controls.Add(textBox);
                    form.Controls.Add(okButton);
                    form.Controls.Add(cancelButton);

                    originalText = textBox.Text;

                    form.FormClosing += (s, e) =>
                    {
                        if (textBox.Text != originalText)
                        {
                            DialogResult res = MessageBox.Show("Do you want to save changes?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                            if (res == DialogResult.No)
                            {
                                textBox.Text = originalText;
                            }
                        }
                    };

                    DialogResult result = editorService.ShowDialog(form);

                    if (result == DialogResult.Cancel)
                    {
                        return textBox.Text;
                    }
                }

                return value;
            }

        }



        public class TableEditor : UITypeEditor
        {
            public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
            {
                string Sample = string.Empty;

                using (TableForm Tableform = new TableForm((string)value))
                {
                    var result = Tableform.ShowDialog();

                    if (result == DialogResult.OK)
                    {
                        return Tableform.ConvertDataGridViewToJson();
                    }
                    else
                    {
                        return value;
                    }
                }
            }

            public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
            {
                return UITypeEditorEditStyle.Modal;
            }
        }
        public class DictionaryEditor : UITypeEditor
        {
            public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
            {
                return UITypeEditorEditStyle.Modal;
            }

            public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
            {
                IWindowsFormsEditorService editorService = provider.GetService(typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;
                if (editorService != null && value is Dictionary<string, string> dictionary)
                {
                    using (Form form = new Form())
                    {
                        form.StartPosition = FormStartPosition.CenterScreen;
                        form.Size = new Size(600, 400); // 表单大小

                        DataGridView grid = new DataGridView
                        {
                            Dock = DockStyle.Fill,
                            AutoGenerateColumns = false,
                            RowTemplate = { Height = 25 }, // 调高行高
                            AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells,
                            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill // 自动调整列宽
                        };

                        grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Key", DataPropertyName = "Key" });
                        grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Value", DataPropertyName = "Value" });

                        BindingSource binding = new BindingSource();
                        binding.DataSource = new List<KeyValuePair<string, string>>(dictionary);
                        grid.DataSource = binding;

                        // 添加输入区域的控件
                        TextBox keyTextBox = new TextBox { Text = "Enter key", ForeColor = SystemColors.GrayText };
                        TextBox valueTextBox = new TextBox { Text = "Enter value", ForeColor = SystemColors.GrayText };
                        Button addButton = new Button { Text = "Add", Width = 100 };

                        // 模拟 Placeholder 功能
                        keyTextBox.GotFocus += (sender, e) =>
                        {
                            if (keyTextBox.Text == "Enter key")
                            {
                                keyTextBox.Text = "";
                                keyTextBox.ForeColor = SystemColors.WindowText;
                            }
                        };

                        keyTextBox.LostFocus += (sender, e) =>
                        {
                            if (string.IsNullOrWhiteSpace(keyTextBox.Text))
                            {
                                keyTextBox.Text = "Enter key";
                                keyTextBox.ForeColor = SystemColors.GrayText;
                            }
                        };

                        valueTextBox.GotFocus += (sender, e) =>
                        {
                            if (valueTextBox.Text == "Enter value")
                            {
                                valueTextBox.Text = "";
                                valueTextBox.ForeColor = SystemColors.WindowText;
                            }
                        };

                        valueTextBox.LostFocus += (sender, e) =>
                        {
                            if (string.IsNullOrWhiteSpace(valueTextBox.Text))
                            {
                                valueTextBox.Text = "Enter value";
                                valueTextBox.ForeColor = SystemColors.GrayText;
                            }
                        };

                        addButton.Click += (sender, e) =>
                        {
                            string key = keyTextBox.Text;
                            string valueText = valueTextBox.Text;  // 将 'value' 改为 'valueText'

                            if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(valueText) && key != "Enter key" && valueText != "Enter value")
                            {
                                if (dictionary.ContainsKey(key))
                                {
                                    MessageBox.Show("Key already exists. Please enter a unique key.");
                                }
                                else
                                {
                                    dictionary[key] = valueText;  // 使用 'valueText'
                                    binding.Add(new KeyValuePair<string, string>(key, valueText));  // 使用 'valueText'
                                    keyTextBox.Clear();
                                    valueTextBox.Clear();
                                    keyTextBox.Text = "Enter key";
                                    keyTextBox.ForeColor = SystemColors.GrayText;
                                    valueTextBox.Text = "Enter value";
                                    valueTextBox.ForeColor = SystemColors.GrayText;
                                }
                            }
                            else
                            {
                                MessageBox.Show("Key and Value cannot be empty or the placeholder text.");
                            }
                        };

                        FlowLayoutPanel inputPanel = new FlowLayoutPanel
                        {
                            Dock = DockStyle.Top,
                            Height = 30,
                            AutoSize = true
                        };

                        inputPanel.Controls.Add(keyTextBox);
                        inputPanel.Controls.Add(valueTextBox);
                        inputPanel.Controls.Add(addButton);

                        form.Controls.Add(grid);
                        form.Controls.Add(inputPanel);

                        if (form.ShowDialog() == DialogResult.OK)
                        {
                            dictionary.Clear();
                            foreach (KeyValuePair<string, string> kvp in binding.List)
                            {
                                dictionary[kvp.Key] = kvp.Value;
                            }
                        }
                    }
                }
                return value;
            }
        }


        // 解析整個配置文件，將結果填入字典中
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

        // 專門解析特定 section 的內容，並排除注釋行與空行
        public static string ParseSectionData(string input, string section)
        {
            if (string.IsNullOrEmpty(input))
                return "";
            var serialNumberSectionBuilder = new StringBuilder(); // 用於構建返回的字符串
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

                // 當前 section 是指定的 section，且該行不為空行時，將該行加入結果字符串
                if (currentSection == section && !string.IsNullOrEmpty(trimmedLine))
                {
                    serialNumberSectionBuilder.AppendLine(trimmedLine);
                }
            }

            // 返回構建的 section 內容，移除結尾的換行符
            return serialNumberSectionBuilder.ToString().TrimEnd();
        }
    }


    public class MyEventArgs : EventArgs
    {
        public int Pass { get; }
        public int Fail { get; }

        public MyEventArgs(int pass, int fail)
        {
            Pass = pass;
            Fail = fail;
        }
    }

}

