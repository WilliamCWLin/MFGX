/*
 * "AutoTestSystem --> MainForm UI"
 *
 * Corpright William & Zhibin
 *
 *.##.......####.########.########..#######..##....##
 *.##........##.....##....##.......##.....##.###...##
 *.##........##.....##....##.......##.....##.####..##
 *.##........##.....##....######...##.....##.##.##.##
 *.##........##.....##....##.......##.....##.##..####
 *.##........##.....##....##.......##.....##.##...###
 *.########.####....##....########..#######..##....##
 *
 *  1. <MainForm.cs> is a Main UI for operator & designer 
 *  2. You can run the MFG program in theme "8. Run task (keyin SN)"
 *  3. If you want to run UI, please keyin "Enter" in the SN text_label 
 *  4. EntryPoint: <Program.cs> entry point --> <MainForm.cs >
 * 
 */

/*****************************************************************************
*                    Includes Definitions
*****************************************************************************/
using AutoTestSystem.Base;

using AutoTestSystem.BLL;
using AutoTestSystem.DAL;
using AutoTestSystem.DUT;
//using AutoTestSystem.Equipment.VISA;
using AutoTestSystem.Equipment.DosBase;
using AutoTestSystem.Model;
using AutoTestSystem.Script;
using CsvHelper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using static AutoTestSystem.BLL.Bd;

/*****************************************************************************
*                    Function code
*****************************************************************************/
namespace AutoTestSystem
{
    public partial class MainForm : Form
    {
        // 參數設定
        #region ################################ 1.UI global parameter setting ############################

        ///***************************  1-1. Test result  ********************************\\   
        private delegate void SaveTestResult();                                                 // 定义生成结果委托
        private SaveTestResult saveTestResult;
        private List<TextBox> SNtextBoxes;
        private volatile bool startFlag                 = false;                                //! 启动信号
        private volatile bool startScanFlag             = true;                                 //! 开始扫描信号
        private bool pauseFlag                          = false;                                //! 暂停信号
        public static bool IsDebug                      = false;                                //! 调试信号
        private bool singleStepTest                     = false;                                //! 单步调试信号
        private bool isCycle                            = false;                                //! 循环测试信号
        public static bool IfCond                       = true;                                 //! IF条件语句结果
        private int sec                                 = 0;                                    //! 测试时间
        private int seqNo                               = -1;                                   //! 当前测试用例号
        private int itemsNo                             = -1;                                   //! 当前测试项目号
        private DateTime startTime                      = new DateTime();                       //! 开始测试时间
        private DateTime endTime                        = new DateTime();                       //! 结束测试时间
        private readonly ManualResetEvent pauseEvent    = new ManualResetEvent(true);           //! 暂停信号
        private readonly ManualResetEvent autoScanEvent = new ManualResetEvent(true);           //! 自动扫描信号
        public int PassNumOfCycleTest { get; set; }                                             //! 循环测试pass次数
        public int FailNumOfCycleTest { get; set; }                                             //! 循环测试fail次数
        private System.Threading.Timer timer;
        public static System.Timers.Timer Timer         = new System.Timers.Timer(1000);

        private INIHelper iniConfig;
        private List<Sequence> sequences                = null;                                 //! 测试用例队列
        public static MesPhases mesPhases               = null;
        public test_phases TestPhase                    = new test_phases();
        public Station Station                          = null;
        public Dictionary<string, object> Devices;
        //public Dictionary<string, IOBase> IODevices;
        //!GPIB连接
        public static Communication SampleComm;
        public static Communication STAComm;                                                     //! 陪测板
        public static DosCmd dosCmd                     = new DosCmd();

        public static string error_code                 = "";
        public static string error_details              = "";
        private string error_code_firstfail             = "";
        public string error_details_firstfail          = "";
        private string finalTestResult                  = "FAIL";                                //! 最终结果,默认值为FAIL：测试&&&&Json&&MES上传结果
        private bool stationStatus                      = true;                                  //! 总的测试结果，默认ture，有一个用例fail则为fail
        private string mesUrl;                                                                   //! 上传MESInfo的地址
        private string cellLogPath;
        public string HeaderFilePath;//! Header file Path路径     
        string CSVFile_path = Path.Combine(Directory.GetCurrentDirectory(), $"Output\\csv_file\\{System.DateTime.Now.ToString("yyyy")}\\{System.DateTime.Now.ToString("MM")}\\Result.csv");//csv path
        string CSVFile_path_PASS = $"Output\\csv_file\\{System.DateTime.Now.ToString("yyyy")}\\{System.DateTime.Now.ToString("MM")}\\PASS\\PASS_Result.csv";
        string CSVFile_path_FAIL = $"Output\\csv_file\\{System.DateTime.Now.ToString("yyyy")}\\{System.DateTime.Now.ToString("MM")}\\FAIL\\FAIL_Result.csv";
        public string DutMode;

        ///***************************  1-2. 定义DUT产品相关全局变量( Zhibin Li )  ********************************\\
        public static MainForm f1;                                                               //! 用例表头
        private readonly string[] colHeader             = new string[] {                         //! CSV 
            "NO.", "tResult","SN", "ItemName","Spec", "tValue", "StartTime", "EndTime"};                                                             //! 测试step表头
        public static DataTable tempDataView            = new DataTable();                       //! 使用DataTable 替代表格控件
        private Thread testThread;                                                               //! 运行测试主线程
        public static TestNew test                      = new TestNew();                         //! Test class

        public static string DUTIP                      = "";                                    //! DUT默认IP地址
        public static GPIBInfo GpibInfo;
        public static ConnectionInfo sshconInfo;                                                 //! SSH连接信息
        public static SerialConnetInfo FixCOMinfo;                                               //! 治具COM口连接信息
        public static SerialConnetInfo DUTCOMinfo;                                               //! DUT COM口连接信息
        public static SerialConnetInfo ControlDeviceCOMinfo;                                     //! ControlDevice COM 口连接信息
        public static TelnetInfo telnetInfo;                                                     //! Telnet连接
        public static Communication DUTCOMM;                                                     //! DUT 串口号
        public static Comport FixSerialPort;                                                     //! 治具串口号
        //public static GPIB GPIBCOMM;  
        public static DUT_BASE DUT_Device;
        public static VisaBase VISA_Device;
        public static IOBase IO_Device;
        public static ControlDeviceBase Comport_Device;
        public static MotionBase Motion_Device;
        public static ControlDeviceBase Control_Device;                                          //! Control Device Related ex: COM port Buadrate
        public static CCDBase CCD_Device;                                                        //! CCD Device Basler,HikVision....
        public static string PSNText;                                                            //! TextBox2 的内容  SN1
        public static string SNText2;                                                            //! TextBox1 的内容   SN2 ，主条码
        public static string output_match;
        public static string MAC;                                                                //! TextBox3 的内容 Mac
        public static string SN;                                                                 //! 测试SN
        public static string PSN;                                                                 //! 测试PSN
        public static string tResult;                                                            //! 测试反馈的结果
        public static string tValue;
        public static string tErrorcode;                                                         //! 测试反馈的值
        public static string MesMac;                                                             //! Mes获取的Mac
        public static string MesMBSN;                                                            //! Mes获取的Main board SN
        public static string MesSW;                                                              //! Mes获取的SW版本号
        public static string MesHW;                                                              //! Mes获取的HW版本号
        public static string MesSD0;                                                             //! Mes获取的SD0容量
        public static string MesSD1;                                                             //! Mes获取的SD1容量
        public static string lableMac;                                                           //! 扫描Lable获取的Mac
        public static List<string> stepTestResult;                                               //! Test class中StepTest方法返回的值，该值有2个成员，测试结果和测试值
        public static int RetryTimes;                                                            //! TestItem Retry 次数
        public static int timeOut;                                                               //! TestItem 超时时间
        public static bool ifFlag;                                                               //! testSetp流程中IF分支的Flag,true需要进入，false不需要进入
        public static bool forFlag;                                                              //! testSetp流程中For分支的Flag，true需要进入，false不需要进入
        public static int forLoopTimes;                                                          //! 记录testSetp流程中For分支剩余loop次数
        public static int forSeqName;                                                            //! 记录testSetp流程中For分支SeqName 项目位置
        public static int forItemName;                                                           //! 记录testSetp流程中For分支itemName 项目位置 
        public static bool forJumpFlag;                                                          //! 记录testSetp流程中For分支Jump flag
        public static string GUI_ErrorCode  = "";
        private bool isLoadRecipeSuccess = false;
        ///***************************  1-3. 定义DUT产品相关全局变量  ********************************\\
        public const string regexp_GATEWAY  = @"^((N)([1-9]|[A-Z])([1-9]|[A-C])([A-HJ-KM-NP-TV-Z]|[1-9]))([A-HJ-KM-NP-TV-Z]|[0-9]){4}$";
        public const string regexp_LEAF     = @"^((Q)([1-9]|[A-Z])([1-9]|[A-C])([A-HJ-KM-NP-TV-Z]|[1-9]))([A-HJ-KM-NP-TV-Z]|[0-9]){4}$";
        public const string regexp_Firefly  = "";                                               
                                           // @"^((G)([1-9]|[A-Z])([1-9]|[A-C])([A-HJ-KM-NP-TV-Z]|[1-9]))([A-HJ-KM-NP-TV-Z]|[0-9]){4}$";
        public string regexp = "";

        //private string[] colSFTResult = new string[] {
        //    "StartTime","SN","Result","EndTime","ErrorCode","csn","mac","throughput","eth0_send","eth0_receive",
        //    "eth1_send","eth1_receive","USBRW","USBRSPEED","USBWSPEED","ResetButton",
        //    "LEDTEST","Ledoff","W_x","W_y","W_L","B_x","B_y","B_L","G_x","G_y","G_Y","R_x","R_y","R_L" };     //! cvs收集测试数据
        private delegate void CollectCsvResult();                                                               //! 定义生成结果委托
        private CollectCsvResult collectCsvResult;
        public static List<string> ArrayListCsv;
        public static List<string> ArrayListCsvHeader;
        public static List<string> ArrayListCsv_yuqiang;
        public static List<string> ArrayListCsvHeader_yuqiang;
        public static List<string> ArrayListCsv_loss;
        public static List<string> ArrayListCsvHeader_loss;
        public static bool SetDefaultIP;
        public static string inPutValue         = "";                                                           //! in Put Value       
        public static string BtDevAddress       = "";                                                           //! Bt Dev Address (not use)                                                          
        public static string[] csvLines         = null;                                                         //! db csv Lines
        public static string CSN                = "";
        public static string DUTMesIP           = "";                                                           //! MES分配的IP地址

        public static bool SetIPflag            = false;                                                        //! 是否设置IP为默认
        private int ItemFailCount               = 0;                                                            
        public static string SSID_2G            = "";                                                           //! Global.STATIONNO + "_2G";
        public static string SSID_5G            = "";                                                           //! Global.STATIONNO + "_5G";
        public static string mes_qsdk_version   = "";
        public static int retry                 = 0;
        private string CSVFilePath              = "";
        private string WorkOrder                = "1";
        public static DosBase PC_OS;                                                                             //! for PC Command
        //public static DUT_BASE tempDUT = null;

        ///***************************  1-4. 系統時間  ********************************\\   
        //private DateTime startTime_thread     = new DateTime();                                                //! 开始测试时间
        //private DateTime endTime_thread       = new DateTime();
        TimeSpan elapsedTime;//! 系統執行時間
        DateTime startDateTime;
        DateTime endDateTime;
        Container_MainThread m_MainThread= null;
        DUT_BASE CurrentTempDUT;
        TreeNode JIG_InitailNode = null;
        ///***************************  1-5. TreeView Class  ********************************\\   
        public class TreeNodeData                                                                                //! 定义一个用于存储节点数据的类 
        {
            public string Text { get; set; }
            public int ImageIndex { get; set; }
            public bool Checked { get; set; }
            public List<TreeNodeData> ChildNodes { get; set; }
        }
	    #endregion 

        // 使用者權限
        #region ################################ 2.User level #############################################
        ///// Device test rule /////
        private string DeviceList_Ret =null;
        public enum TestStatus
        {
            PASS    = 1,                   // 测试pass
            FAIL    = 2,                   // 测试fail
            START   = 3,                   // 开始测试，正在测试中
            ABORT   = 4,
            Device_Not_Ready=5,
            IDLE    =6,
        }
        #endregion 

        public MainForm()
        {
            PSN = string.Empty;
            // UI配置
            #region ################################ 3.initial UI & resolution ############################

            //GlobalNew.UserLevel = 2;
            //Manufacture.Global_Memory.UserLevel = 2;

            Control.CheckForIllegalCrossThreadCalls = false;
            this.AutoScaleMode          = System.Windows.Forms.AutoScaleMode.Dpi; //设定按分辨率来缩放控件
            InitializeComponent();
            this.groupBox3.Location     = new Point(this.groupBox2.Location.X + this.groupBox2.Size.Width + 30, this.groupBox2.Location.Y);
            this.groupBox1.Location     = new Point(this.groupBox3.Location.X + this.groupBox3.Size.Width + 30, this.groupBox3.Location.Y);
            this.bt_debug.Location      = new Point(this.groupBox1.Location.X + this.groupBox1.Size.Width + 30, this.groupBox1.Location.Y * 9);
            this.lb_IPaddress.Location  = new Point(this.bt_debug.Location.X  + this.bt_debug.Size.Width  + 30, this.bt_debug.Location.Y);
            f1 = this;

            //! 读配置文件,初始化全局配置变量
            GlobalNew.InitStation();
            try
            {
                SetLable(Modelabel, $"{GlobalNew.CurrentMode}.", Color.CadetBlue, Color.Black);
                SetLable(Projectlabel, $"{GlobalNew.CurrentProject}", Color.CadetBlue, Color.Black);
                SetLable(Stationlabel, $"{GlobalNew.CurrentStation}", Color.CadetBlue, Color.Black);
                if (GlobalNew.ProtreeON == "1")
                    GlobalNew.DeviceListPath = $@"{GlobalNew.CurrentRecipePath}";


            }
            catch (Exception ex)
            {
                // 在這裡處理異常
                Logger.Error($"Load Recipe and Devices JSON Error: {ex.Message}");
            }
            SNtextBoxes = new List<TextBox>();
            GenerateTextBoxes();
            //! 用来防止初次开启界面卡顿现象        
            this.Visible = false;   
            this.Opacity = 0;

            // timer1.Start();
            f1.Text = f1.Text + " V" + GlobalNew.Version;
            #endregion
            
            // 系统使用的文件夹确认和初始化
            #region ################################ 4.initial file path ##################################





            //string driveLetter = "Z"; // 指定要映射的驱动器号
            //string serverPath = @"\\10.1.1.5\Data\EO5002"; // 网络共享目录路径
            //string username = "vvozaxartq"; // 访问共享目录所需的用户名
            //string password = "0915557200"; // 访问共享目录所需的密码

            ////// 使用 net use 命令建立网络驱动器映射
            //string command = $@"net use {driveLetter}: {serverPath} {password} /USER:{username}";
            //ExecuteCommand(command);



            // 4-4. 配置機台設備相關Log 
            CheckFolder(GlobalNew.EQLogPath);
            CheckFolder(GlobalNew.ConfigLogPath);


            Manufacture.Global_Memory.GlobalCreate("AutoTestSystem.Script", "AutoTestSystem.Base.ScriptBase");

            foreach (TextBox textBox in SNtextBoxes)
            {
                textBox.Enabled = false;
                textBox.KeyPress += TextBox_KeyPress;
            }
            string filePath = "Config\\Header.json";
            HeaderFilePath = Path.Combine(Directory.GetCurrentDirectory(), filePath);
            #endregion
        }

        public void NewDataFolder()
        {
            try
            {
                GlobalNew.LogPath = $@"{GlobalNew.LOGFOLDER}\Log\{GlobalNew.CurrentProject}\{GlobalNew.CurrentMode}\{GlobalNew.CurrentStation}\{GlobalNew.CurrentFixture}\{DateTime.Now.ToString("yyyyMMdd")}";
                CheckFolder(GlobalNew.LogPath + @"\PASS");
                CheckFolder(GlobalNew.LogPath + @"\FAIL");
                CheckFolder(GlobalNew.LogPath + @"\PASS\DUT");
                CheckFolder(GlobalNew.LogPath + @"\FAIL\DUT");
                CheckFolder(GlobalNew.LogPath + @"\PASS\EQ");
                CheckFolder(GlobalNew.LogPath + @"\FAIL\EQ");

                GlobalNew.csvLogPath = $@"{GlobalNew.LOGFOLDER}\CSV\{GlobalNew.CurrentProject}\{GlobalNew.CurrentMode}\{GlobalNew.CurrentStation}\{GlobalNew.CurrentFixture}\{DateTime.Now.ToString("yyyyMM")}";
                CheckFolder(GlobalNew.csvLogPath);
                CSVFile_path = $"{GlobalNew.csvLogPath}\\{GlobalNew.CurrentProject}_{GlobalNew.CurrentStation}[{GlobalNew.CurrentFixture}]_Result.csv";//csv path
                CSVFile_path_PASS = $"{GlobalNew.csvLogPath}\\PASS_{GlobalNew.CurrentProject}_{GlobalNew.CurrentStation}[{GlobalNew.CurrentFixture}]_Result.csv";
                CSVFile_path_FAIL = $"{GlobalNew.csvLogPath}\\FAIL_{GlobalNew.CurrentProject}_{GlobalNew.CurrentStation}[{GlobalNew.CurrentFixture}]_Result.csv";
            }
            catch(Exception ex)
            {
                Logger.Error($"NewDataFolder Error.Exception {ex.Message}");
            }
        }
        private void GenerateTextBoxes()
        {
            // 清除之前生成的 TextBox 控制項
            foreach (TextBox textBox in SNtextBoxes)
            {
                panel3.Controls.Remove(textBox);
                textBox.Dispose();
            }
            SNtextBoxes.Clear();

            // 動態生成對應數量的 TextBox 控制項
            int index = 1; // 起始索引

            foreach (var kvp in GlobalNew.ProductKeys)
            {
                string key = kvp.Key;     
                string value = kvp.Value;

                Label label = new Label();
                label.AutoSize = true;
                label.Font = new System.Drawing.Font("微软雅黑", 14F, FontStyle.Bold);
                label.Text = key+":";
                label.Location = new System.Drawing.Point(300, 6 + (index - 1) * 24); 
                panel3.Controls.Add(label); 

                TextBox textBox = new TextBox();
                textBox.Location = new System.Drawing.Point(455, 6+(index-1) * 24);
                textBox.Width = 170;
                textBox.Name = $"{key}";
                textBox.Visible = true;
                panel3.Controls.Add(textBox); 
                SNtextBoxes.Add(textBox);
                if(index == 1)
                {
                    textBox.Focus();
                }
                index++; 
            }
        }
        private void TextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            TextBox textBox = sender as TextBox;

            if (e.KeyChar == (char)Keys.Enter)
            {
                if (!CheckTextBoxPattern(textBox))
                {
                    MessageBox.Show("The input format is incorrect.。");
                    textBox.Clear();
                    textBox.Focus();
                    return;
                }

                textBox.Enabled = false;

                SwitchFocusToNextTextBox(textBox);

                if (AllTextBoxesFilledAndPatternsCorrect())
                {
                    int ChangeCount = 0;
                    string filePath = "ChangeCount.txt";
                    if (GlobalNew.CycleCheck > 0)
                    {
                        // Read the current count from the file if it exists
                        if (File.Exists(filePath))
                        {
                            string content = File.ReadAllText(filePath);
                            if (int.TryParse(content, out int currentCount))
                            {
                                ChangeCount = currentCount;
                            }
                        }

                        Logger.Error($"Count:{ChangeCount} > {GlobalNew.CycleCheck}");

                        if (ChangeCount > GlobalNew.CycleCheck)
                        {
                            AutoMessageForm customMessageForm = new AutoMessageForm("按C關閉程式後，更換線材\nPress C to close the program, then replace the cable.”", "C", "");
                            //customMessageForm.ShowDialog(); // 使用 ShowDialog 方法以模態方式顯示視窗
                            DialogResult result = customMessageForm.ShowDialog();
                            if (DialogResult.Yes == result)
                            {
                                this.Close();
                                return;
                            }
                        }
                    }

                    RunProTree();

                    if (GlobalNew.CycleCheck > 0)
                    {
                        ChangeCount++;
                        File.WriteAllText(filePath, ChangeCount.ToString());
                    }
                }
            }
        }
        private bool AllTextBoxesFilledAndPatternsCorrect()
        {
            foreach (TextBox textBox in SNtextBoxes)
            {
                if (string.IsNullOrWhiteSpace(textBox.Text) || !CheckTextBoxPattern(textBox))
                {
                    return false; 
                }
            }
            return true; 
        }
        private bool CheckTextBoxPattern(TextBox textBox)
        {
            if (GlobalNew.ProductKeys.ContainsKey(textBox.Name))
            {
                string pattern = GlobalNew.ProductKeys[textBox.Name];
                Regex regex = new Regex(pattern);
                Match match = regex.Match(textBox.Text);

                if (match.Success)
                {
                    textBox.Text  =  match.Value;
                }
                return regex.IsMatch(textBox.Text);
            }
            return true; 
        }

        private void SwitchFocusToNextTextBox(TextBox currentTextBox)
        {
            int currentIndex = SNtextBoxes.IndexOf(currentTextBox);

            if (currentIndex < SNtextBoxes.Count - 1)
            {
                SNtextBoxes[currentIndex + 1].Enabled = true;
                SNtextBoxes[currentIndex + 1].Focus();
            }
        }
        static int ExecuteCommand(string command)
        {

            Process process = Process.Start(new ProcessStartInfo
            {
                FileName = "cmd.exe",
                RedirectStandardInput = true, // 重定向标准输入流
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = false
            });

            if (process != null)
            {
                process.StandardInput.WriteLine(command); // 发送命令
                process.StandardInput.Flush();
                process.StandardInput.Close(); // 关闭标准输入流，告知 cmd.exe 命令已输入完毕

                process.WaitForExit();
                return process.ExitCode;
            }

            return -1; // 如果启动进程失败，则返回 -1
        }

        void SetGlobalVariable(string key, ref string globalVariable, Dictionary<string, object> data)
        {
            if (data.ContainsKey(key))
            {
                if (data[key] == null)
                {
                    globalVariable = "";
                    Logger.Error($"JSONData {key} is null");
                }
                else
                {
                    globalVariable = data[key].ToString();
                }
            }
            else
            {
                Logger.Error($"JSONData {key} is missing");
            }
        }
        private void MainForm_Shown(object sender, EventArgs e)
        {
            // 4-1. 创建Txtdata 文件夹
            if (GlobalNew.ProtreeON != "1")
            {
                GlobalNew.txtLogPath = $@"{GlobalNew.LOGFOLDER}\TxtData\{GlobalNew.TESTMODE}\{DateTime.Now:yyyyMMdd}";
                GlobalNew.LogPath = $@"{GlobalNew.LOGFOLDER}\Log\{GlobalNew.CurrentMode}\{GlobalNew.CurrentStation}\{GlobalNew.CurrentFixture}\{DateTime.Now.ToString("yyyyMMdd")}";
                CheckFolder(GlobalNew.LogPath + @"\PASS");
                CheckFolder(GlobalNew.LogPath + @"\FAIL");
                CheckFolder(GlobalNew.LogPath + @"\PASS\DUT");
                CheckFolder(GlobalNew.LogPath + @"\FAIL\DUT");
                CheckFolder(GlobalNew.LogPath + @"\PASS\EQ");
                CheckFolder(GlobalNew.LogPath + @"\FAIL\EQ");
                CheckFolder(GlobalNew.txtLogPath + @"\PASS");
                CheckFolder(GlobalNew.txtLogPath + @"\FAIL");
                CheckFolder(GlobalNew.txtLogPath + @"\PASS\DUT");
                CheckFolder(GlobalNew.txtLogPath + @"\FAIL\DUT");
                CheckFolder(GlobalNew.txtLogPath + @"\PASS\EQ");
                CheckFolder(GlobalNew.txtLogPath + @"\FAIL\EQ");
                // 4-2. 创建Txtdata 文件夹
                GlobalNew.imageLogPath = $@"{GlobalNew.LOGFOLDER}\ImageData\{GlobalNew.TESTMODE}\{DateTime.Now:yyyyMMdd}";
                CheckFolder(GlobalNew.imageLogPath + @"\PASS");
                CheckFolder(GlobalNew.imageLogPath + @"\FAIL");
            }
            else
            {
                NewDataFolder();
            }

            // 設備初始化
            #region ################################ 5.Initial Devices ##################################

#if DEBUG
            //bt_debug.Visible = true;
            GlobalNew.TESTMODE = "debug";
#else

#endif


            //////////////////////////////////////////////////////////////////////////////////////
            // 1-1. Init data Grid View Columns
            //      初始化表格控件
            //////////////////////////////////////////////////////////////////////////////////////
            InitdataGridViewColumns();
            //////////////////////////////////////////////////////////////////////////////////////
            // 5-1. Use ProTreeView dll load device  ( Check Config\Devices\Devices.json )      //
            //      Add by MTE William                                                          //
            //////////////////////////////////////////////////////////////////////////////////////
            string ret = string.Empty;
            if (GlobalNew.ProtreeON == "1")
            {
                ret = ProTreeView.ProTreeView.Load_DeviceList(GlobalNew.CurrentRecipePath, GlobalNew.Devices);     //! ProTreeView load device
                Logger.Info($"Load device path. {GlobalNew.CurrentRecipePath}");
            }
            else
            {
                ret = ProTreeView.ProTreeView.Load_DeviceList(GlobalNew.DeviceListPath, GlobalNew.Devices);     //! ProTreeView load device
            }

            
            DeviceList_Ret = ret;
            //Logger.Debug("xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx");                                              //! ILog show in 
            if (ret == "Load_Devices success")                                                  
            {
                foreach (var value in GlobalNew.Devices.Values) 
                {
                    bool result;
                    switch (value)                                                                              //! Control Device Base
                    {
                        case ControlDeviceBase D:                                                              
                            result = D.Init("");
                            if (result)
                                Logger.Debug($"{D.Description} Init Success");          
                            else
                            {
                                Logger.Debug($"{D.Description} Init Fail");
                                GlobalNew.fail_device.Add(D.Description + " Init Fail");               
                            }
                            break;
                        case DUT_BASE D:
                            result = D.Init("");
                            if (result)
                                Logger.Debug($"{D.Description} Init Success");
                            else
                            {
                                Logger.Debug($"{D.Description} Init Fail");
                                GlobalNew.fail_device.Add("DUT_Fail");
                            }
                            break;
                        case IOBase I:
                            result = I.Init("");
                            if (result)
                                Logger.Debug($"{I.Description} Init Success");
                            else
                            {
                                Logger.Debug($"{I.Description} Init Fail");
                                GlobalNew.fail_device.Add(I.Description + " Init Fail");
                            }
                            break;
                        case CCDBase C:
                            result = C.Init("");

                            if (result)
                                Logger.Debug($"{C.Description} Init Success");
                            else
                            {
                                Logger.Debug($"{C.Description} Init Fail");
                                GlobalNew.fail_device.Add(C.Description + " Init Fail");
                            }
                            break;
                        case MotionBase M:
                            result = M.Init("");                            
                            if (result)
                                Logger.Debug($"{M.Description} Init Success");
                            else
                            {
                                Logger.Debug($"{M.Description} Init Fail");
                                GlobalNew.fail_device.Add(M.Description + " Init Fail");
                            }
                            break;
                        case VisaBase V:
                            result = V.Init("");
                            if (result)
                                Logger.Debug($"{V.Description} Init Success");
                            else
                            {
                                Logger.Debug($"{V.Description} Init Fail");
                                GlobalNew.fail_device.Add(V.Description + " Init Fail");
                            }
                            break;
                    }
                }
                if(GlobalNew.fail_device.Count == 0)
                    GlobalNew.g_Initial = true;

            }
            else
            {
                GlobalNew.g_Initial = false;
                GlobalNew.fail_device.Add(ret);
            }
            Logger.Debug(ret);
            //Logger.Debug("xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx");

            if(GlobalNew.ProtreeON == "1")
            {
                LogTabControl.Visible = true;
                treeViewSeq.Visible = false;
                MainProTreeView.ProcessNodeMouseClick += MainProTreeNodeMouseClick;
                MainProTreeView.SetMode(ProTreeView.ProTreeView.FlowMode.Process_Mode);
                MainProTreeView.TogglePropertyGridVisibility();
                MainProTreeView.ShowTip(true);
                MainProTreeView.Drop(false);
                MainProTreeView.KeyAction(false);
                
                Logger.Info($"Load recipe path. {GlobalNew.CurrentRecipePath}");
                string result = MainProTreeView.Read_Recipe(GlobalNew.CurrentRecipePath);
                if (result.Contains("success"))
                {
                    isLoadRecipeSuccess = true;
                    InitialGridView();
                }
                else
                {
                    isLoadRecipeSuccess = false;
                    Logger.Error($"Read_Recipe Fail. {GlobalNew.CurrentRecipePath}. ({result})");
                }

            }
            DUTIP = GlobalNew.DUTIP;

            //////////////////////////////////////////////////////////////////////////////////////
            // 5-2. initial sshconInfo 通信信息                                                 //
            //      Add by MTE William                                                          //
            //////////////////////////////////////////////////////////////////////////////////////
            if (!string.IsNullOrEmpty(GlobalNew.DUTIP) && !string.IsNullOrEmpty(GlobalNew.SSH_USERNAME) && !string.IsNullOrEmpty(GlobalNew.SSH_PASSWORD) && !string.IsNullOrEmpty(GlobalNew.SSH_PORT))
            {
                sshconInfo = new ConnectionInfo(GlobalNew.DUTIP, Int16.Parse(GlobalNew.SSH_PORT), GlobalNew.SSH_USERNAME,
                  new AuthenticationMethod[] { new PasswordAuthenticationMethod(GlobalNew.SSH_USERNAME, GlobalNew.SSH_PASSWORD) });
                Logger.Debug("initialize sshconInfo success!");
            }


            iniConfig = new INIHelper(GlobalNew.IniConfigFile);


            //////////////////////////////////////////////////////////////////////////////////////
            // 5-11. 更新状态栏                                                                  //
            //      GPIB address                                                                //
            //////////////////////////////////////////////////////////////////////////////////////    
            UpdateContLable();


            #endregion

            // Json file 
            #region ################################ 6.Json file parser #################################

            //////////////////////////////////////////////////////////////////////////////////////
            // 6-1. Excel to Jason
            //      Json Script是否存在，如不存在，重新生成test Json Script
            //////////////////////////////////////////////////////////////////////////////////////
            string jsonPath = string.Empty;
            if (GlobalNew.RECIPENAME == "Golden")
                jsonPath = $@"{System.Environment.CurrentDirectory}\Config\Golden.json";
            else if (GlobalNew.RECIPENAME == "Debug")
                jsonPath = $@"{System.Environment.CurrentDirectory}\Config\Debug.json";
            else
                jsonPath = $@"{System.Environment.CurrentDirectory}\Config\{GlobalNew.STATIONNAME}.json";
            if (File.Exists(jsonPath) == false)
            {
                //GlobalNew.ExcelToJson();
            }           

            //////////////////////////////////////////////////////////////////////////////////////
            // 6-2. Read Json to TreeView
            //      初始化TreeView 树状框
            //////////////////////////////////////////////////////////////////////////////////////
            JsonToTreeViewSeq();
            Logger.Debug($"upload test-case form {jsonPath}.");
            #endregion 

            // UI item 
            #region ################################ 7.Initial UI item ##################################


            //////////////////////////////////////////////////////////////////////////////////////
            // 7-2. Init panel1 (左下視窗)
            //      初始化扫码窗口，以条码长度确认扫码窗口是否显示，至少保证1个扫码窗口显示
            //////////////////////////////////////////////////////////////////////////////////////
            if (GlobalNew.SN2_Length == "0" || GlobalNew.SN2_Length == null)
            {
                label6.Visible = false;
                textBox2.Visible = false;
            }
            if (GlobalNew.MAC_Length == "0" || GlobalNew.MAC_Length == null)
            {
                label4.Visible = false;
                textBox3.Visible = false;
            }
            if (GlobalNew.PSN_LENGTH == "0" || GlobalNew.PSN_LENGTH == null)
            {
                label3.Visible = false;
                textBox1.Visible = false;
            }
            if (GlobalNew.EQ_Mode != "1")
            {
                Btn_ResetHome.Enabled  = false;
                Btn_ResetHome.Visible  = false;
                Btn_MotionStop.Enabled = false;
                Btn_MotionStop.Visible = false;
            }

            //GetFixName();  //!获取治具号和MES工站编号，更新主界面Lable。


            //////////////////////////////////////////////////////////////////////////////////////
            // 7-3. Init lable status 
            //      初始化测试窗口各种lable状态
            //////////////////////////////////////////////////////////////////////////////////////
            lb_mode.Text = GlobalNew.ProMode;
            lbl_StationNo.Text = GlobalNew.STATIONNO;
            lbl_testMode.Text = GlobalNew.TESTMODE;

            //////////////////////////////////////////////////////////////////////////////////////
            // 7-4. User mode
            //      Default = debug
            //////////////////////////////////////////////////////////////////////////////////////
            if (GlobalNew.TESTMODE.ToLower() == "debug")
            {
                lbl_testMode.BackColor = Color.Red;
            }

            //////////////////////////////////////////////////////////////////////////////////////
            // 7-5. Local IP
            //      显示本机IP地址，方便远程桌面
            //////////////////////////////////////////////////////////////////////////////////////
            string LocalIP = Bd.GetAllIpv4Address("192.168."); 
            lb_IPaddress.Text += LocalIP;

            //////////////////////////////////////////////////////////////////////////////////////
            // 7-3. Init lable status 
            //      初始化测试窗口各种lable状态
            //////////////////////////////////////////////////////////////////////////////////////
            if(GlobalNew.ProtreeON != "1")
            { 
                if (GlobalNew.PSN_LENGTH !="" && Convert.ToInt32(GlobalNew.PSN_LENGTH) != 0)
                {
                    SetTextBox(textBox1);           //!当SN1条码长度存在时，设置当前窗口的活动控件和焦点为textBox2
                }
                else
                {
                    SetTextBox(textBox2);           //!SN1条码长度不存在，SN2 条码为主条码，设置当前窗口的活动控件和焦点为textBox2
                }
            }
            else
            {
                if (SNtextBoxes.Count > 0)
                    SNtextBoxes[0].Focus();
            }

            #endregion 

        }

        // Run task
        #region ################################ 8. Run task (keyin SN)    ##########################################
        //////////////////////////////////////////////////////////////////////////////////////
        // 8-1. SN check & Start
        //////////////////////////////////////////////////////////////////////////////////////
        private void TextBox2_KeyDown(object sender, KeyEventArgs e)
        {
            string filePath = "Config\\Header.json";
            HeaderFilePath = Path.Combine(Directory.GetCurrentDirectory(), filePath);
            //Logger.Info(" Header.json Path Combine absolutePath=>" + absolutePath + "\r\n");

            bool hdExists = File.Exists(HeaderFilePath);
            if (!hdExists)
            {
                MessageBox.Show("CSV Header file is not exist, Please save it in RecipePage!!!!", "Header file check", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            ////////////////////////////
            // 8-1-1. EQ mode         //
            //        確認是否是EQ mode//
            ////////////////////////////
            if (GlobalNew.EQ_Mode == "1" && GlobalNew.home_flag == false)
            {              
                MessageBox.Show("Please press the Home Reset Button", "EQ_Mode", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            //////////////////////////////////////
            // 8-1-2. Check same SN             //
            //        e=null时候，不执行&&后面的  //
            //        提前比对SN1和SN2的内容     //
            /////////////////////////////////////

            if(e != null && e.KeyCode == Keys.F2)
            {
                LogTabControl.Visible = true;
                treeViewSeq.Visible = false;
                MainProTreeView.Visible = true;
                MainProTreeView.SetMode(ProTreeView.ProTreeView.FlowMode.Process_Mode);
                MainProTreeView.TogglePropertyGridVisibility();
                string jsonRecipePath = $@"{System.Environment.CurrentDirectory}\Config\TestRecipePro.json";
                string result = MainProTreeView.Read_Recipe(GlobalNew.CurrentRecipePath);
                if (result.Contains("success"))
                {
                    InitialGridView();
                }
                else
                {
                    Logger.Error($"Read_Recipe Fail. {GlobalNew.CurrentRecipePath} ({result})");
                }
                GlobalNew.ProtreeON = "1";
                return;

            }
            else if (e != null && e.KeyCode == Keys.F3)
            {
                LogTabControl.Visible = false;
                treeViewSeq.Visible = true;
                dataGridViewDetail.Visible = true;
                MainProTreeView.Visible = false;
                GlobalNew.ProtreeON = "0";
                return;
            }
            else if (e != null && e.KeyCode != Keys.Enter)
                return;
            SNText2 = textBox2.Text;

            //if (GlobalNew.SN2_REPLACE_KEY != null || GlobalNew.SN2_REPLACE_KEY != "")
            //    SNText2 = SNText2.Replace(GlobalNew.SN2_REPLACE_KEY, "");

            SetTextBox(textBox2, false);

            if (GlobalNew.PSN_LENGTH != "" && SNText2 == PSNText)
            {
                bt_errorCode.Text = "Scan lable error";
                bt_errorCode.BackColor = Color.Red;
                SetTextBox(textBox2);
                return;
            }
            if (Convert.ToInt32(GlobalNew.PSN_LENGTH) != 0 && PSNText == null)
            {
                bt_errorCode.Text = "Scan lable error";
                bt_errorCode.BackColor = Color.Red;
                SetTextBox(textBox1);
                return;
            }
            // reset SN
            SN = "";
            PSN = "";
            /////////////////////////////////////////////////////
            // 8-1-3. Reset UI                                 //
            //        重置Json treeViewSeq.Node颜色            //        
            //        重置final Test Result (franklin add init)//
            /////////////////////////////////////////////////////
            for (int i = 0; i < treeViewSeq.Nodes.Count; i++)
            {
                for (int j = 0; j < treeViewSeq.Nodes[i].Nodes.Count; j++)
                { treeViewSeq.Nodes[i].Nodes[j].BackColor = Color.White; }
            }


            stationStatus = true; 
            //SetTestStatus(TestStatus.START);

            /////////////////////////////////
            // 8-1-4. Fail counter         //
            //      FAIL数量超过Limits提示  //
            /////////////////////////////////
            if (!CheckContinueFailNum() && GlobalNew.TESTMODE.ToLower() != "debug") { return; }

            ////////////////////////////////
            // 8-1-5. Check key in SN     //
            //      检查扫描的SN的长度及格式     //
            ///////////////////////////////
            if (SNText2.Length != Convert.ToInt32(GlobalNew.SN2_Length))
            {
                MessageBox.Show($"SN:{SNText2} length {SNText2.Length} is wrong,{GlobalNew.SN2_Length} is right.Please Scan again!", "SN Length Fail", 0, MessageBoxIcon.Error);
                SetTextBox(textBox2); return;
            }
            //if (GlobalNew.SN_PATTERN != "")
            //{
            //    /*if (GlobalNew.CheckSN_position == null)
            //        GlobalNew.CheckSN_position = "0";*/
            //    if (CheckSNFormat(SNText2, GlobalNew.CheckSN_position, GlobalNew.SN_PATTERN, GlobalNew.SN_Letter_case))
            //    {
            //        SN = SNText2;
            //        PSN = PSNText;
            //    }
            //    else
            //    {
            //        SetTextBox(textBox2); return;
            //    }
            //}
            SN = SNText2;
            PSN = PSNText;
            ////////////////////////////////
            // 8-1-6. Check device type   //
            //      根据扫描的SN判断机种    //
            ////////////////////////////////
            //JudgeProdMode(ScanSN);

            ////////////////////////////////
            // 8-1-7. Check SN            //
            //      检查SN规则是否正确     //
            ///////////////////////////////
            // 检查SN规则是否正确
            //if (GlobalNew.ProMode.ToUpper() == "VD5001" && GlobalNew.STATIONNAME.Contains("FT") == true)
            //{
            //    SN = SNText2.Substring(10).ToUpper(); //VD5001 FATP取SN条码
            //}
            //else
            //{
            //    SN = SNText2.ToUpper();
            //}

            ///////////////////////////////////
            // 8-1-8. Creat the task thread  //
            //      创建测试后台主线程        //
            //////////////////////////////////
            ///
            if (GlobalNew.ProtreeON == "1")
            {
                RunProTree();
            }
            else
            {
                testThread = new Thread(new ThreadStart(TestThread))
                {
                    IsBackground = true
                };      //!开始测试主线程
                testThread.Start();

            }



            ////////////////////////////////////////////////
            // 8-1-9. Finish & retun                      //
            //      测试线程结束或未启动，重新启动测试线程   //
            ///////////////////////////////////////////////
            //if (!testThread.IsAlive)   
            //{
            //    testThread = new Thread(new ThreadStart(TestThread));
            //    testThread.Start();
            //}
        }
        //////////////////////////////////////////////////////////////////////////////////////
        // 8-2. PSN keyIn
        //        確認SN是否正確
        //////////////////////////////////////////////////////////////////////////////////////
        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e != null && e.KeyCode != Keys.Enter) return;
            PSNText = textBox1.Text;
            //if (GlobalNew.PSN_REPLACE_KEY != null && GlobalNew.PSN_REPLACE_KEY != "")
            //    PSNText = PSNText.Replace(GlobalNew.PSN_REPLACE_KEY, "");
            //VD5001专用，SN 前缀“vkda.co/s/”
            if (GlobalNew.PSN_LENGTH != "" && PSNText.Length == Convert.ToInt32(GlobalNew.PSN_LENGTH))
            {
                SetTextBox(textBox1, false);
                SetTextBox(textBox2);
                //SetTestStatus(TestStatus.START);
            }
            else
            {
                bt_errorCode.Text = "Scan lable error";
                bt_errorCode.BackColor = Color.Red;
                SetTextBox(textBox2);
                return;
            }

            //代表沒有SN2 直接從PSN輸入後啟動測試
            if (GlobalNew.SN2_Length == "0" || GlobalNew.SN2_Length ==null)
            {

                string filePath = "Config\\Header.json";
                HeaderFilePath = Path.Combine(Directory.GetCurrentDirectory(), filePath);
                //Logger.Info(" Header.json Path Combine absolutePath=>" + absolutePath + "\r\n");

                bool hdExists = File.Exists(HeaderFilePath);
                if (!hdExists)
                {
                    MessageBox.Show("CSV Header file is not exist, Please save it in RecipePage!!!!", "Header file check", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                SN = "";
                PSN = "";
                /////////////////////////////////////////////////////
                // 8-1-3. Reset UI                                 //
                //        重置Json treeViewSeq.Node颜色            //        
                //        重置final Test Result (franklin add init)//
                /////////////////////////////////////////////////////
                for (int i = 0; i < treeViewSeq.Nodes.Count; i++)
                {
                    for (int j = 0; j < treeViewSeq.Nodes[i].Nodes.Count; j++)
                    { treeViewSeq.Nodes[i].Nodes[j].BackColor = Color.White; }
                }


                stationStatus = true;
                

                /////////////////////////////////
                // 8-1-4. Fail counter         //
                //      FAIL数量超过Limits提示  //
                /////////////////////////////////
                if (!CheckContinueFailNum() && GlobalNew.TESTMODE.ToLower() != "debug") { return; }

                ////////////////////////////////
                // 8-1-5. Check key in SN     //
                //      检查扫描的SN的长度及格式     //
                ///////////////////////////////
                /*if (PSNText.Length != Convert.ToInt32(GlobalNew.PSN_LENGTH))
                {
                    MessageBox.Show($"SN:{PSNText} length {PSNText.Length} is wrong,{GlobalNew.PSN_LENGTH} is right.Please Scan again!", "SN Length Fail", 0, MessageBoxIcon.Error);
                    SetTextBox(textBox2); return;
                }
                if (GlobalNew.CheckSN_position != "" && GlobalNew.SN_PATTERN != "")
                {
                    if (CheckSNFormat(PSNText, GlobalNew.CheckSN_position, GlobalNew.SN_PATTERN, GlobalNew.SN_Letter_case))
                    {
                        PSN = PSNText;
                    }
                    else
                    {
                        SetTextBox(textBox2); return;
                    }
                }*/
                PSN = PSNText;

                if (GlobalNew.ProtreeON == "1")
                {
                    RunProTree();
                }
                else
                {
                    testThread = new Thread(new ThreadStart(TestThread))
                    {
                        IsBackground = true
                    };      //!开始测试主线程
                    testThread.Start();

                }
            }
        }
        //////////////////////////////////////////////////////////////////////////////////////
        // 8-2. Test Status update                                                          //
        //       更新測試狀態                                                                //
        //////////////////////////////////////////////////////////////////////////////////////
        public void SetTestStatus(TestStatus testStatus)
        {
            try
            {
                switch (testStatus)
                {
                    case TestStatus.START:

                        if (GlobalNew.stopwatch.IsRunning)
                        {
                            GlobalNew.stopwatch.Reset();
                        }
                        GlobalNew.stopwatch.Start();
                        error_details_firstfail = "";
                        SaveRichTextPro(MainlogRichTextBox, true);
                        SaveRichTextPro(DUTRichTextBox, true);
                        SaveRichTextPro(EQRichTextBox, true);
                        SaveRichTextPro(richTextBox1, true);
                        SetButton(this.bt_Status, "Testing", Color.Yellow);
                        SetButton(this.bt_errorCode, " ", Color.Yellow);
                        SetButton(this.ConfigureBtn, "", ConfigureBtn.BackColor, false);
                        SetButton(this.BtnLogin, "", BtnLogin.BackColor, false);
                        SetButton(this.HomeBtn, "", HomeBtn.BackColor, false);

                        
                        startTime = DateTime.Now;
                        timer = new System.Threading.Timer(TimerCallBack, null, 0, 1000);
                        SetButtonPro(buttonBegin, Properties.Resources.pause);
                        SetButtonPro(buttonExit, Properties.Resources.stop);
                        singleStepTest = false;
                        startFlag = true;
                        pauseEvent.Set();
                        Logger.Debug($"Start test...SN:{SN},Station:{Global.FIXTURENAME},DUTMode:{DutMode},TestMode:{Global.TESTMODE},Isdebug:{IsDebug.ToString()},SoftVersion:{Global.Version}");
                        UpdateDetailViewClear();
                        break;

                    case TestStatus.FAIL:
                        Global.Total_Fail_Num++;
                        SetButton(this.bt_Status, "FAIL", Color.Red);
                        SetButton(this.bt_errorCode, error_details_firstfail, Color.Red);
                        UpdateContinueFail(false);
                        if (testStatus == TestStatus.FAIL && SetIPflag)
                        {//SRF 测试失败设回默认IP重测
                            string recvStr = "";
                            DUTCOMM.SendCommand($"luxsetip {DUTIP} 255.255.255.0", ref recvStr, Global.PROMPT, 10);
                        }
                        break;

                    case TestStatus.PASS:
                        Global.Total_Pass_Num++;
                        SetButton(this.bt_Status, "PASS", Color.Green);
                        if (GlobalNew.ShowTestTime == "1")
                            SetButton(this.bt_errorCode, sec.ToString(), Color.Green);
                        else
                            SetButton(this.bt_errorCode, "", Color.Green);
                        UpdateContinueFail(true);
                        break;

                    case TestStatus.ABORT:
                        Global.Total_Abort_Num++;
                        testThread.Abort();
                        testThread.Join(3000);
                        SetButton(this.bt_Status, "Abort", Color.Gray);
                        SetButton(this.bt_errorCode, error_details, Color.Gray);
                        saveTestResult();
                        break;

                    case TestStatus.Device_Not_Ready:
                       
                        SetButton(this.bt_Status, "NO TEST", Color.Red);
                        SetButton(this.bt_errorCode, error_details, Color.Red);
                        break;

                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.ToString());
            }
            finally
            {
                try
                {
                    switch (testStatus)
                    {
                        case TestStatus.START:
                            break;

                        case TestStatus.FAIL:
                        case TestStatus.PASS:
                        case TestStatus.ABORT:
                            GlobalNew.stopwatch.Stop();
                            GlobalNew.stopwatch.Reset();
                            
                            // 关闭DUT通信
                            if (DUTCOMM != null)
                            {
                                DUTCOMM.Close();
                            }
                            // 无论测试pass/fail，都弹出治具
                            if (Global.FIXTUREFLAG == "1")
                            {
                                //using (Comport fixCom = new Comport(FixCOMinfo, ""))
                                //{
                                FixSerialPort.OpenCOM();
                                var recvStr = "";
                                FixSerialPort.SendCommandToFix("AT+TESTEND%", ref recvStr, "OK", 10);
                                //fixCom.Close();
                                //}
                            }
#if DEBUG
#else
                            /*var files = Directory.GetFileSystemEntries($@"{Global.LOGFOLDER}\CsvData");
                            using (SFTPHelper sFTP = new SFTPHelper(Global.LOGSERVER, Global.LOGSERVERUser, Global.LOGSERVERPwd, "22"))
                            {
                                sFTP.Connect();
                                foreach (var file in files)
                                {
                                    string csvFileName = Path.GetFileName(file);
                                    //if (File.Exists(file) && csvFileName != $@"{DateTime.Now:yyyy-MM-dd--HH}-00-00_{Global.STATIONNO}.csv")
                                    if (File.Exists(file) && csvFileName != Path.GetFileName(CSVFilePath))
                                    {
                                        sFTP.Put(file, $@"/{csvFileName}");
                                        File.Move(file, $@"{Global.LOGFOLDER}\CsvData\Upload\{csvFileName}");
                                    }
                                }
                                sFTP.Disconnect();
                            }*/
#endif
                            break;

                        default:
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Fatal("SetTestStatus finally Exception:" + ex.ToString());
                    //throw;
                }
                finally
                {
                    try
                    {
                        if (testStatus != TestStatus.START)
                        {
                            //timer.Dispose();
                            endTime = DateTime.Now;
                            Logger.Debug($"Test end,ElapsedTime:{sec}s.");
                            startFlag = false;
                            SetTextBox(textBox2);
                            SetTextBox(textBox1);
                            SetButton(this.ConfigureBtn, "", ConfigureBtn.BackColor);
                            SetButton(this.BtnLogin, "", BtnLogin.BackColor);
                            SetButton(this.HomeBtn, "", HomeBtn.BackColor);
                            //UpdateContLable();
                            //WriteCountNumToFile();
                            //SaveRichText(false, RichTextBoxStreamType.UnicodePlainText);
                            if (finalTestResult == "FAIL")
                            {
                                SetButton(this.bt_errorCode, error_details_firstfail, Color.Red);
                            }
                            startScanFlag = true;
                            autoScanEvent.Set();    // 开始自动扫描
                        }
                    }
                    catch (Exception ex)
                    {
                        startScanFlag = true;
                        autoScanEvent.Set();
                        Logger.Fatal(ex.ToString());
                    }
                }
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////
        // 8-3. Run Task Thread                                                             //
        //       运行线程，开始测试                                                         //
        //////////////////////////////////////////////////////////////////////////////////////
        public void MemoryDataClear(DUT_BASE tempDUT)
        {
            GlobalNew.g_datacollection.Clear();
            GlobalNew.g_datacollection.SetMoreProp("Failitem", "");
            if (tempDUT != null)
            {
                tempDUT.TestInfo.ClearTestSteps();
                tempDUT.DataCollection.Clear();               
                tempDUT.DataCollection.SetMoreProp("Failitem", "");
            }
            
            TraverseTreeViewClearDataItem(MainProTreeView.GetTreeview().Nodes);
        }

        public void TestSummary(DUT_BASE tempDUT,bool result)
        {
            string EndTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            endDateTime = DateTime.Parse(EndTime);
            TimeSpan elapsedTime = endDateTime - startDateTime;

            string elapsedTimeString = string.Format("{0:hh\\:mm\\:ss\\.fff}", elapsedTime);
            double totalSeconds = elapsedTime.TotalSeconds;

            tempDUT.DataCollection.SetMoreProp("EndTotalTime", elapsedTimeString);
            tempDUT.DataCollection.SetMoreProp("EndTime", $"\"{EndTime}\"");           
            tempDUT.TestInfo.AddTestStep($"Test Summary", $"ProductSN: {tempDUT.DataCollection.GetMoreProp("ProductSN")}");
            tempDUT.TestInfo.AddTestStep($"Test Summary", $"Start Test Time: {tempDUT.DataCollection.GetMoreProp("StartTime")}");
                
            GlobalNew.LogPath = $@"{GlobalNew.LOGFOLDER}\Log\{GlobalNew.CurrentProject}\{GlobalNew.CurrentMode}\{GlobalNew.CurrentStation}\{GlobalNew.CurrentFixture}\{DateTime.Now.ToString("yyyyMMdd")}";
            //GlobalNew.LogPath = $@"{GlobalNew.LOGFOLDER}\Log\{GlobalNew.CurrentProject}\{GlobalNew.CurrentMode}\{GlobalNew.CurrentStation}\{GlobalNew.CurrentFixture}";

            GlobalNew.csvLogPath = $@"{GlobalNew.LOGFOLDER}\CSV\{GlobalNew.CurrentProject}\{GlobalNew.CurrentMode}\{GlobalNew.CurrentStation}\{GlobalNew.CurrentFixture}\{DateTime.Now.ToString("yyyyMM")}";
            CSVFile_path = $"{GlobalNew.csvLogPath}\\{GlobalNew.CurrentProject}_{GlobalNew.CurrentStation}[{GlobalNew.CurrentFixture}]_Result.csv";//csv path
            CSVFile_path_PASS = $"{GlobalNew.csvLogPath}\\PASS_{GlobalNew.CurrentProject}_{GlobalNew.CurrentStation}[{GlobalNew.CurrentFixture}]_Result.csv";
            CSVFile_path_FAIL = $"{GlobalNew.csvLogPath}\\FAIL_{GlobalNew.CurrentProject}_{GlobalNew.CurrentStation}[{GlobalNew.CurrentFixture}]_Result.csv";

            if (result)
            {
                finalTestResult = "PASS";
                SetTestStatus(TestStatus.PASS);

                tempDUT.DataCollection.SetMoreProp("Result", finalTestResult);
                tempDUT.TestInfo.AddTestStep($"Test Summary", $"TestResult: PASS");
                tempDUT.TestInfo.AddTestStep($"Test Summary", $"TotalTestTime: {totalSeconds.ToString("0.00")}s");
                tempDUT.TestInfo.AddTestStep($"Test Summary", $"End Test Time: {tempDUT.DataCollection.GetMoreProp("EndTime")}");




                WriteReport(tempDUT, $@"{GlobalNew.LOGFOLDER}\\Report\\{GlobalNew.CurrentProject}\\{GlobalNew.CurrentStation}\\{GlobalNew.CurrentFixture}\\{GlobalNew.CurrentMode}\\{DateTime.Now.ToString("yyyyMMdd")}\\PASS\\{tempDUT.DataCollection.GetMoreProp("ProductSN")}_{DateTime.Now.ToString("yyyMMdd_HHmmss")}.txt");
                WriteCSVResult(HeaderFilePath, CSVFile_path_PASS, tempDUT.DataCollection,false);                
                MainInfoRichText(MainInfoRichTextBox,$"Test Result : {finalTestResult}");
                GlobalNew.Total_Pass_Num++;
            }
            else
            {
                finalTestResult = "FAIL";
                error_details_firstfail = tempDUT.DataCollection.GetMoreProp("Failitem");
                SetTestStatus(TestStatus.FAIL);

                tempDUT.DataCollection.SetMoreProp("Result", finalTestResult);
                tempDUT.TestInfo.AddTestStep($"Test Summary", $"TestResult: FAIL ({error_details_firstfail})");
                tempDUT.TestInfo.AddTestStep($"Test Summary", $"TotalTestTime: {totalSeconds.ToString("0.00")}s");
                tempDUT.TestInfo.AddTestStep($"Test Summary", $"End Test Time: {tempDUT.DataCollection.GetMoreProp("EndTime")}");
                WriteReport(tempDUT, $@"{GlobalNew.LOGFOLDER}\\Report\\{GlobalNew.CurrentProject}\\{GlobalNew.CurrentStation}\\{GlobalNew.CurrentFixture}\\{GlobalNew.CurrentMode}\\{DateTime.Now.ToString("yyyyMMdd")}\\FAIL\\{tempDUT.DataCollection.GetMoreProp("ProductSN")}{error_details_firstfail}_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}.txt");

                WriteCSVResult(HeaderFilePath, CSVFile_path_FAIL, tempDUT.DataCollection,false);
                MainInfoRichText(MainInfoRichTextBox, $"Test Result : {finalTestResult}");
                MainInfoRichText(MainInfoRichTextBox, $"Error Code : {error_details_firstfail}");
                GlobalNew.Total_Fail_Num++;
            }

            MainInfoRichText(MainInfoRichTextBox, $"End Time: {EndTime}");
            MainInfoRichText(MainInfoRichTextBox, $"Total Time : {totalSeconds.ToString("0.00")}s");
            WriteCSVResult(HeaderFilePath, CSVFile_path, tempDUT.DataCollection,true);//Total log in csv

            sec = 0;
            timer.Dispose();

            UpdateContLable();
            WriteCountNumToFile();

        }
        public void StartInit(DUT_BASE tempDUT)
        {
            MainInfoRichText(MainInfoRichTextBox, $"",true);

            MainInfoRichText(MainInfoRichTextBox, $"=======Devices Info=======");
            foreach (var value in GlobalNew.Devices.Values)
            {
                string status = string.Empty;
                switch (value)
                {
                    case DUT_BASE D:

                            D.Status(ref status);
                            MainInfoRichText(MainInfoRichTextBox, $"{D.Description} : {status}");

                        break;
                    case Manufacture.Equipment D:

                            D.Status(ref status);
                            MainInfoRichText(MainInfoRichTextBox, $"{D.Description} : {status}");                   

                        break;
                }
            }
            MainInfoRichText(MainInfoRichTextBox, $"");
            MainInfoRichText(MainInfoRichTextBox, $"=======Test Info=======");
            if (GlobalNew.ProtreeON == "1")
            {
                if (tempDUT != null)
                {
                    //CurrentTempDUT = tempDUT;
                    if(Info!=null)
                    Info.UpdateDictData(CurrentTempDUT.DataCollection.GetData(), CurrentTempDUT.DataCollection.GetMESData(), CurrentTempDUT.DataCollection.GetSpecData());
                    foreach (TextBox textBox in SNtextBoxes)
                    {
                        tempDUT.DataCollection.SetMoreProp(textBox.Name, textBox.Text);
                        GlobalNew.g_datacollection.SetMoreProp(textBox.Name, textBox.Text);
                        MainInfoRichText(MainInfoRichTextBox,$"{textBox.Name} : {textBox.Text}");

                        Logger.Debug($"SetMoreProp:{textBox.Name} -> {textBox.Text}");
                    }

                    tempDUT.DataCollection.SetMoreProp("WorkID", GlobalNew.CurrentUser);
                    //MainInfoRichText(MainInfoRichTextBox,$"WorkID : {GlobalNew.CurrentUser}");
                    string StartTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                    startDateTime = DateTime.Parse(StartTime);

                    tempDUT.DataCollection.SetMoreProp("StartTime", $"\"{StartTime}\"");
                    tempDUT.DataCollection.SetMoreProp("OTPKey", GlobalNew.OTPKey);
                    tempDUT.DataCollection.SetMoreProp("StationName", GlobalNew.CurrentStation);
                    tempDUT.DataCollection.SetMoreProp("ProjectName", GlobalNew.CurrentProject);
                    tempDUT.DataCollection.SetMoreProp("RunMode", GlobalNew.CurrentMode);
                    tempDUT.DataCollection.SetMoreProp("FixtureName", GlobalNew.CurrentFixture);
                    MainInfoRichText(MainInfoRichTextBox, $"WorkID : {GlobalNew.CurrentUser}");
                    MainInfoRichText(MainInfoRichTextBox,$"Start Time : {StartTime}");
                    try
                    {
                        // 取得目前正在執行的程式的進程
                        Process currentProcess = Process.GetCurrentProcess();
                        // 取得進程的主模組，即執行檔本身
                        ProcessModule mainModule = currentProcess.MainModule;
                        // 取得執行檔的路徑
                        string exePath = mainModule.FileName;

                        // 建立 FileInfo 物件以取得檔案資訊
                        FileInfo fileInfo = new FileInfo(exePath);

                        // 取得執行檔的版本資訊
                        FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(exePath);
                        string exeName = System.Reflection.Assembly.GetExecutingAssembly().GetName().ToString();

                        tempDUT.TestInfo.AddTestStep($"Title", $"Application Release Time:{fileInfo.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss")}");
                        tempDUT.TestInfo.AddTestStep($"Title", $"Application Version:{System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString()}");
                        tempDUT.TestInfo.AddTestStep($"Title", $"Test Computer Name:{Environment.MachineName}");
                        tempDUT.TestInfo.AddTestStep($"Title", $"Test Computer User Name:{Environment.UserName}");
                        if (Environment.Is64BitOperatingSystem)
                            tempDUT.TestInfo.AddTestStep($"Title", $"Test Computer Operating System Bit: 64");
                        else
                            tempDUT.TestInfo.AddTestStep($"Title", $"Test Computer Operating System Bit: 32");

                        if (Environment.Is64BitProcess)
                            tempDUT.TestInfo.AddTestStep($"Title", $"Test Process System Bit: 64");
                        else
                            tempDUT.TestInfo.AddTestStep($"Title", $"Test Process System Bit: 32");

                        FileInfo ConfigfileInfo = new FileInfo(GlobalNew.CurrentRecipePath);

                        tempDUT.TestInfo.AddTestStep($"Title", $"Test Config File Name:{System.IO.Path.GetFileName(GlobalNew.CurrentRecipePath)}");
                        tempDUT.TestInfo.AddTestStep($"Title", $"Test Config File LastWriteTime :{ConfigfileInfo.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss")}");
                        tempDUT.TestInfo.AddTestStep($"Title", $"Login User :{GlobalNew.CurrentUser}");
                        

                        

                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"TestInfo Get Error.{ex.Message}");
                    }
                }
            }
            else
            {
                GlobalNew.g_datacollection.SetMoreProp("PSN", PSN);
                GlobalNew.g_datacollection.SetMoreProp("ProductSN", PSN);
                Logger.Debug($"SetMoreProp(\"ProductSN\", {SN});");
                GlobalNew.g_datacollection.SetMoreProp("SN", SN);
                GlobalNew.g_datacollection.SetMoreProp("WorkID", GlobalNew.CurrentUser);
                string StartTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                startDateTime = DateTime.Parse(StartTime);
                GlobalNew.g_datacollection.SetMoreProp("StartTime", startDateTime.ToString());
            }

        }
        public void End_TestTime(string TestResult, DUT_BASE tempDUT)
        {         
            string EndTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            endDateTime = DateTime.Parse(EndTime);
            GlobalNew.g_datacollection.SetMoreProp("EndTime", EndTime);
            GlobalNew.g_datacollection.SetMoreProp("Result", TestResult);
            if (tempDUT != null)
            {
                tempDUT.DataCollection.SetMoreProp("EndTime", $"\"EndTime.ToString()\"");
                tempDUT.DataCollection.SetMoreProp("Result", TestResult);
                MainInfoRichText(MainInfoRichTextBox,$"Test Result : {TestResult}");
            }
        }

        public void Total_TestTime(DUT_BASE tempDUT)
        {
            TimeSpan elapsedTime;
            elapsedTime = endDateTime - startDateTime;
            string elapsedTimeString = string.Format("{0:hh\\:mm\\:ss\\.fff}", elapsedTime);
            GlobalNew.g_datacollection.SetMoreProp("EndTotalTime", elapsedTimeString);
            if (tempDUT != null)
            {
                tempDUT.DataCollection.SetMoreProp("EndTotalTime", elapsedTimeString);
                
            }
                
        }
        private void TestThread()
        {
            try
            {
                //if (GlobalNew.fail_device.Count > 0)
                //{

                //    SetTestStatus(TestStatus.Device_Not_Ready);
                //    sec = 0;
                //    timer.Dispose();
                //    SetButton(bt_errorCode, "Device_Not_Ready");

                //    if (GlobalNew.fail_device.Count > 0)
                //    {
                //        for (int i = 0; i < GlobalNew.fail_device.Count; i++)
                //            Logger.Warn(GlobalNew.fail_device[i]);
                //    }

                //    MessageBox.Show("Please Check Device Setting & Restart EXE", "Warn!!!!");

                //    return;
                //}

                Dictionary<string, object> JsonDataLists = new Dictionary<string, object>();
                //设置cycles，debug mode生效
                if (GlobalNew.TESTMODE.ToLower() == "production") { GlobalNew.cycles = 1; }
                for (int i = 0; i < GlobalNew.cycles; i++)
                {
                    error_details_firstfail = "";
                    //=====================================================================
                    // 重置treeViewSeq.Node颜色
                    for (int h = 0; h < treeViewSeq.Nodes.Count; h++)
                    {
                        for (int j = 0; j < treeViewSeq.Nodes[h].Nodes.Count; j++)
                        { treeViewSeq.Nodes[h].Nodes[j].BackColor = Color.White; }
                    }
                    //重置finalTestResult
                    stationStatus = true; //franklin add init
                    MemoryDataClear(null);

                    //File.Delete("Output\\Result.csv");
                    SetTestStatus(TestStatus.START);
                    StartInit(null);
                    //=====================================================================
                    int noSum = 0; //表格控件中测试项目的序列号 

                    DataTable dt = new DataTable();

                    ItemsNew tempItems = new ItemsNew();

                    //取Json Script 中SeqName，TestItems 的数量
                    string jsonPath = string.Empty;
                    if (GlobalNew.RECIPENAME == "Golden")
                        jsonPath = $@"{System.Environment.CurrentDirectory}\Config\Golden.json";
                    else if (GlobalNew.RECIPENAME == "Debug")
                        jsonPath = $@"{System.Environment.CurrentDirectory}\Config\Debug.json";
                    else
                        jsonPath = $@"{System.Environment.CurrentDirectory}\Config\{GlobalNew.STATIONNAME}.json";
                    JArray Sequences = JArray.Parse(File.ReadAllText(jsonPath));
                    for (int j = 0; j < Sequences.Count; j++) 
                    {
                        //StepTest for 分支 SeqName 开始
                        forLoopStart:
                        ItemsNew tempSeq = new ItemsNew();
                        string SeqName = Sequences[j]["SeqName"].ToString();
                        bool SeqNodeEnalbe = Convert.ToBoolean(Sequences[j]["Enable"]);

                        Logger.Info("Start testSuite: " + SeqName);
                        int.TryParse(Sequences[j]["TotalNumber"].ToString(), out int TotalNumber);
                        JArray SeqItems = JArray.Parse(Sequences[j]["SeqItems"].ToString());

                        for (int k = 0; k < TotalNumber; k++)
                        {
                            tResult = "";
                            if (SeqNodeEnalbe == false) { treeViewSeq.Nodes[j].Nodes[k].ForeColor = Color.Gray; continue; }

                            //StepTest for 分支 使用goto 语句跳转回来时，首要先设置itemName的项目位置
                            if (forFlag == true && forJumpFlag == true) { k = forItemName; forJumpFlag = false; }

                            //取Json Script 中每个 TestItems 的成员值
                            tempItems.Enable            = SeqItems[k]["Enable"].ToString();                            
                            tempItems.ItemName          = SeqItems[k]["ItemName"].ToString();
                            tempItems.TestKeyword       = SeqItems[k]["TestKeyword"].ToString();
                            tempItems.Prefix            = SeqItems[k]["Prefix"].ToString();
                            tempItems.ErrorCode         = SeqItems[k]["ErrorCode"].ToString();
                            int.TryParse(SeqItems[k]["RetryTimes"].ToString(), out RetryTimes);
                            int.TryParse(SeqItems[k]["TimeOut"].ToString(), out timeOut);
                            tempItems.SubStr1           = SeqItems[k]["SubStr1"].ToString();
                            tempItems.SubStr2           = SeqItems[k]["SubStr2"].ToString();
                            tempItems.IfElse            = SeqItems[k]["IfElse"].ToString();
                            tempItems.For               = SeqItems[k]["For"].ToString();
                            tempItems.Mode              = SeqItems[k]["Mode"].ToString();
                            tempItems.ComdSend          = SeqItems[k]["ComdSend"].ToString();
                            tempItems.ExpectStr         = SeqItems[k]["ExpectStr"].ToString();
                            tempItems.CheckStr1         = SeqItems[k]["CheckStr1"].ToString();
                            tempItems.CheckStr2         = SeqItems[k]["CheckStr2"].ToString();
                            tempItems.Spec              = SeqItems[k]["Spec"].ToString();
                            tempItems.Limit_min         = SeqItems[k]["Limit_min"].ToString();
                            tempItems.Limit_max         = SeqItems[k]["Limit_max"].ToString();
                            tempItems.unit              = SeqItems[k]["unit"].ToString();
                            tempItems.Bypass            = SeqItems[k]["Bypass"].ToString();
                            tempItems.DllPlugin         = SeqItems[k]["DllPlugin"].ToString();
                            tempItems.StriptType        = SeqItems[k]["StriptType"].ToString();
                            tempItems.DeviceName        = SeqItems[k]["DeviceName"].ToString();
                            tempItems.SpecRule          = SeqItems[k]["SpecRule"].ToString();

                            //替换ComdSend中的变量,在使用以下变量时，需提前注意变量是否有赋值
                            tempItems.ComdSend = tempItems.ComdSend.Replace("<PSN>", PSN);
                            tempItems.ComdSend = tempItems.ComdSend.Replace("<SN>", SN);
                            ////// Peter add 增加時間項///////
                            tempItems.ComdSend = tempItems.ComdSend.Replace("<TIME>", DateTime.Now.ToString("yyyyMMdd_HHmmss"));

                            tempItems.ComdSend = tempItems.ComdSend.Replace("<MesMac>", MesMac);
                            tempItems.ComdSend = tempItems.ComdSend.Replace("<MesMBSN>", MesMBSN);
                            tempItems.ComdSend = tempItems.ComdSend.Replace("<MesSW>", MesSW);
                            tempItems.ComdSend = tempItems.ComdSend.Replace("<MesHW>", MesHW);
                            tempItems.ComdSend = tempItems.ComdSend.Replace("<File path>", PSN);  //当前程式运行的目录
                            tempItems.ComdSend = tempItems.ComdSend.Replace("<Share path>", GlobalNew.SharePath); //DUT mount 的PC本地路径

                            //替换Spec中的变量,在使用以下变量时，需提前注意变量是否有赋值
                            tempItems.Spec = tempItems.Spec.Replace("<PSN>", PSN);
                            tempItems.Spec = tempItems.Spec.Replace("<SN>", SN);                          
                            tempItems.Spec = tempItems.Spec.Replace("<MesMBSN>", MesMBSN);
                            tempItems.Spec = tempItems.Spec.Replace("<MesMac>", MesMac);
                            tempItems.Spec = tempItems.Spec.Replace("<MesSW>", MesSW);
                            tempItems.Spec = tempItems.Spec.Replace("<MesHW>", MesHW);
                            tempItems.Spec = tempItems.Spec.Replace("<MesSD0>", MesSD0);
                            tempItems.Spec = tempItems.Spec.Replace("<MesSD1>", MesSD1);

                            //替换CheckStr1中的变量,在使用以下变量时，需提前注意变量是否有赋值
                            tempItems.CheckStr1 = tempItems.CheckStr1.Replace("<lable_Mac>", lableMac);

                            if (Convert.ToBoolean(tempItems.Enable) == false) { treeViewSeq.Nodes[j].Nodes[k].ForeColor = Color.Gray; continue; }
                            noSum++;

                            //判断当前测试项目属于for分支开始
                            if (tempItems.For.Length > 0)
                            {
                                if (tempItems.For.ToLower().CheckStr("for") == true && forFlag == true)
                                {
                                    forFlag = true;
                                    forLoopTimes = Convert.ToInt32(tempItems.For.ToLower().Replace("for(", "").Replace(")", ""));
                                    forSeqName = j;
                                    forItemName = k;
                                }
                            }

                            //debug模式，树状框未勾选测试项目时，自动跳过
                            if (bt_debug.Checked == true && treeViewSeq.Nodes[j].Nodes[k].Checked == false) { continue; }


                            //ifFlag，正常测试跳过else项目
                            if (tempItems.IfElse.ToLower() == "else" || tempItems.IfElse.ToLower() == "elseif")
                            {
                                if (ifFlag == false)
                                {
                                    treeViewSeq.Nodes[j].Nodes[k].BackColor = Color.Gray;
                                    continue;
                                }
                            }
                            this.Invoke(new Action(() =>
                            {
                                treeViewSeq.SelectedNode = treeViewSeq.Nodes[j].Nodes[k]; //树状框实时显示当前测试项目

                            }));
                            this.Invoke(new Action(() =>
                            {
                                string specrule = GetViewSpec(tempItems.SpecRule);
                                tempDataView.Rows.Add(noSum.ToString(), "", PSN, tempItems.ItemName, specrule, "", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), ""); //表格添加测试项目和开始测试时间
                                dataGridViewDetail.CurrentCell = dataGridViewDetail.Rows[tempDataView.Rows.Count - 1].Cells[0]; //表格实时显示最新行
                            }));

                            //默认第一个测试项目为C002,检查SN是否属于该工站，debug模式默认跳过 
                            //if (j == 0 && k == 0 && GlobalNew.TESTMODE.ToLower() == "debug")
                            //{
                            //    Logger.Info("Start: " + tempItems.ItemName + " ,TestKeyword: " + tempItems.TestKeyword + " ,Retry: " + RetryTimes.ToString() + " ,TimeOut: " + timeOut.ToString());
                            //    Logger.Debug($"debug mode,don't checkMesRoute");
                            //    tResult = "PASS";
                            //    treeViewSeq.Nodes[j].Nodes[k].BackColor = Color.Green;   //设置当前树状框背景颜色
                            //    tempDataView.Rows[tempDataView.Rows.Count - 1]["EndTime"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"); //设置测试结束时间
                            //    tempDataView.Rows[tempDataView.Rows.Count - 1]["tResult"] = tResult;//设置测试结果
                            //    dataGridViewDetail.Rows[tempDataView.Rows.Count - 1].DefaultCellStyle.BackColor = Color.Green; //设置表格背景颜色-PASS绿色
                            //    continue;
                            //}
                           
                            // 运行测试步骤
                            for (int l = 0; l < RetryTimes; l++)    // Retry 
                            {
                                Logger.Info("Start: " + tempItems.ItemName + " ,TestKeyword: " + tempItems.TestKeyword + " ,Retry: " + RetryTimes.ToString() + " ,TimeOut: " + timeOut.ToString());
                               
                                if (tempItems.DllPlugin == "")
                                {
                                    //当DllPlugin为"",默认调用test class 中的StepTest 方法，需要提供参数TestKeyword、ComdSend、ExpectStr、timeOut
                                    stepTestResult = test.StepTest(tempItems, GlobalNew.Devices, timeOut);
                                }
                                else
                                {
                                    ////当DllPlugin不为"",动态调用外部dll，方法编写方式参考 test class中的StepTest

                                }

                                tResult = stepTestResult[0].ToUpper();
                                tValue = stepTestResult[1];

                                //删除反馈值的换行、Tab、开始、结尾的空白字符
                                tValue = tValue.Replace("\n", "").Replace("\t", "").Replace("\r", "").Replace("\r\n", "").Trim();

                                JsonDataLists[tempItems.ItemName] = tValue;
                                Logger.Debug($"tValue: {tValue}");
                                Logger.Debug($"SpecRule: {tempItems.SpecRule}");

                                if (tResult == "PASS") { Logger.Debug($"Waiting for:{tempItems.ExpectStr} succeed!!"); }

                                if (tResult == "FAIL") {
                                    GlobalNew.g_datacollection.SetMoreProp("RetryTimes", $"{RetryTimes}");
                                    Logger.Debug($"Waiting for:{tempItems.ExpectStr} TimeOut({timeOut.ToString()}),FAIL!!!"); 
                                }

                                //反馈值是否包含CheckStr1和CheckStr2
                                if (tResult == "PASS")
                                {
                                    if (Bd.CheckStr(tValue, tempItems.CheckStr1) == true && Bd.CheckStr(tValue, tempItems.CheckStr2) == true)
                                    {

                                        if (tempItems.CheckStr1.Length > 0)
                                        {
                                            if (Bd.CheckStr(tValue, tempItems.CheckStr1) == true)
                                            {
                                                tResult = "PASS";
                                                Logger.Debug($"ItemName: {tempItems.ItemName} ,CheckStr1: {tempItems.CheckStr1},PASS!!");
                                                if (tempItems.CheckStr2.Length > 0)
                                                {
                                                    if (Bd.CheckStr(tValue, tempItems.CheckStr2) == true)
                                                    {
                                                        tResult = "PASS";
                                                        Logger.Debug($"ItemName: {tempItems.ItemName} ,CheckStr2: {tempItems.CheckStr2},PASS!!");
                                                    }
                                                    else
                                                    {
                                                        tResult = "FAIL";
                                                        Logger.Debug($"ItemName: {tempItems.ItemName} ,CheckStr2: {tempItems.CheckStr2},FAIL!!");
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                tResult = "FAIL";
                                                Logger.Debug($"ItemName: {tempItems.ItemName} ,CheckStr1: {tempItems.CheckStr1},FAIL!!");
                                            }
                                        }
                                    }
                                    else { tResult = "FAIL"; Logger.Debug($"ItemName: {tempItems.ItemName} ,CheckStr1: {tempItems.CheckStr1} ,CheckStr2: {tempItems.CheckStr2} ,FAIL!!"); }
                                }

                                //当SubStr1不等于"",截取Sub1 和Sub2 之间的字符串，Sub2为"", 默认截取至tValue结尾字符
                                if (tResult == "PASS")
                                {
                                    if (tempItems.SubStr1 != "")
                                    {
                                        tValue = Bd.GetSubStringOfMid(tValue, tempItems.SubStr1, tempItems.SubStr2);
                                        if (tValue != null) { tResult = "PASS"; Logger.Debug($"SubStr tValue: {tValue} ,PASS!!"); }
                                        else { tResult = "FAIL"; Logger.Debug($"SubStr tValue: {tValue} ,FAIL!!"); }
                                    }

                                }


                                //Spec不等于""
                                if (tResult == "PASS" && tempItems.Spec != "")
                                {
                                    if (Bd.CheckSpec(tempItems.Spec, tValue) == true)
                                    {
                                        tResult = "PASS";
                                        Logger.Debug($"ItemName:{tempItems.ItemName} ,Value: {tValue} ,Spec: {tempItems.Spec},PASS!!");
                                    }
                                    else { tResult = "FAIL"; Logger.Debug($"ItemName:{tempItems.ItemName} ,Value: {tValue} ,Spec: {tempItems.Spec},FAIL!!"); }
                                }

                                //当Limits不等于""
                                if (tResult == "PASS")
                                {
                                    if (Bd.CompareLimit(tempItems.Limit_min, tempItems.Limit_max, tValue, false) == true)
                                    {
                                        tResult = "PASS";
                                        Logger.Debug($"ItemName:{tempItems.ItemName} ,Value: {tValue} ,Min: {tempItems.Limit_min} ,Max: {tempItems.Limit_max},PASS!!");
                                    }
                                    else { tResult = "FAIL"; Logger.Debug($"ItemName:{tempItems.ItemName} ,Value: {tValue} ,Min: {tempItems.Limit_min} ,Max: {tempItems.Limit_max},FAIL!!"); }
                                }

                                //Retry 循环尾 result 判断
                                if (tResult == "PASS") { break; }

                            }//retrytimes


                            // 让测试步骤bypass
                            if (tempItems.Bypass == "1")
                            {
                                Logger.Info($"Let this step:{tempItems.ItemName} bypass.");
                                tResult = "PASS";
                            }

                            string endTime = string.Empty;
                            //string elapsedTime = string.Empty;

                            //最终测试结果判断
                            if (tResult == "PASS")
                            {
                                treeViewSeq.Nodes[j].Nodes[k].BackColor = Color.Green;   //设置当前树状框背景颜色
                                //endTime_thread = DateTime.Now;                                
                                this.Invoke(new Action(() =>
                                {
                                    tempDataView.Rows[tempDataView.Rows.Count - 1]["tValue"] = tValue; //设置测试值
                                    tempDataView.Rows[tempDataView.Rows.Count - 1]["EndTime"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"); //设置测试结束时间
                                    tempDataView.Rows[tempDataView.Rows.Count - 1]["tResult"] = tResult;//设置测试结果

                                }));
                                tErrorcode = "PASS";                                //dataGridViewDetail.Rows[tempDataView.Rows.Count - 1].DefaultCellStyle.BackColor = Color.Green; //设置表格背景颜色-PASS绿色

                                //endTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                //GlobalNew.g_datacollection.SetMoreProp("EndTime", endTime);                                

                                //关闭if分支
                                if (tempItems.IfElse.ToLower() == "if" || tempItems.IfElse.ToLower() == "elseif") { ifFlag = false; }
                            }
                            else
                            {
                                string ifhaveFail = GlobalNew.g_datacollection.GetMoreProp("Failitem");
                                if(ifhaveFail == "")
                                {
                                    if (tempItems.ErrorCode != string.Empty)
                                    {
                                        Logger.Debug($"Set Error Code.{tempItems.ErrorCode}   Data:{tValue}");
                                        GlobalNew.g_datacollection.SetMoreProp("Failitem", tempItems.ErrorCode);
                                    }

                                    else
                                    {
                                        Logger.Debug($"Set Error Code.UnDefineErr");
                                        GlobalNew.g_datacollection.SetMoreProp("Failitem", "UnDefErr");//undefined errorcode
                                    }
                                }
                                else
                                {
                                    Logger.Debug($"Failitem({ifhaveFail}) already exists. It cannot be replaced with another Failitem({tempItems.ErrorCode}).");
                                }

                                treeViewSeq.Nodes[j].Nodes[k].BackColor = Color.Red;   //设置当前树状框背景颜色
                                //endTime_thread = DateTime.Now;                                
                                this.Invoke(new Action(() =>
                                {
                                    tempDataView.Rows[tempDataView.Rows.Count - 1]["tValue"] = tValue; //设置测试值
                                    tempDataView.Rows[tempDataView.Rows.Count - 1]["EndTime"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"); //设置测试结束时间
                                    tempDataView.Rows[tempDataView.Rows.Count - 1]["tResult"] = tResult;//设置测试结果

                                }));
                                tErrorcode = tempItems.ErrorCode;                                //dataGridViewDetail.Rows[tempDataView.Rows.Count - 1].DefaultCellStyle.BackColor = Color.Red; //设置表格背景颜色-FAIL红色

                                //endTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                //GlobalNew.g_datacollection.SetMoreProp("EndTime", endTime);                               
                                //启动if分支
                                if (tempItems.IfElse.ToLower() == "if" || tempItems.IfElse.ToLower() == "elseif") { ifFlag = true; }

                                ////判断fail是否继续测试
                                //if (Convert.ToInt32(GlobalNew.FAIL_CONTINUE) == 0) { break; }

                            }
                            if (GlobalNew.cycles > 1)
                            {                                
                                GlobalNew.g_datacollection.SetMoreProp("ProductSN", PSN + "_" + i.ToString());
                            }
                            else
                            {
                                GlobalNew.g_datacollection.SetMoreProp("ProductSN", PSN);
                            }
                            //End_TestTime(tResult,null);
                            //Total_TestTime(null);



                            if (tempItems.For.Length > 0)
                            {
                                //判断当前测试项目for分支结尾
                                if (tempItems.For.ToLower() == "endfor" && forFlag == true && forLoopTimes > 0)
                                {
                                    forLoopTimes--;
                                    j = forSeqName;
                                    forJumpFlag = true;
                                    goto forLoopStart;
                                }

                                //结束SetpTest for循环
                                if (tempItems.For.ToLower() == "endfor" && forFlag == true && forLoopTimes == 0)
                                {
                                    forFlag = false;
                                }
                                
                            }

                            stationStatus &= (tResult == "FAIL" ? false : true);
                            ////判断fail是否继续测试
                            if (tResult == "FAIL" && Convert.ToInt32(GlobalNew.FAIL_CONTINUE) == 0) 
                            {
                                /////error code assign to GUI 
                                GUI_ErrorCode = tempItems.ErrorCode;

                                if (tempItems.IfElse.Contains("JUMP"))
                                {
                                    int index=0;
                                    string Jump_string = tempItems.IfElse.Replace("JUMP(", "").Replace(")", "");

                                    // 遍历所有子对象
                                    foreach (JToken parentToken in Sequences.Children())
                                    {
                                        if (parentToken is JObject)
                                        {
                                            // 对于包含单个对象的数组，检查该对象是否包含特定字符串值
                                            JObject parentObject = (JObject)parentToken;
                                            JToken value;
                                            if (parentObject.TryGetValue("SeqName", out value) && value.Type == JTokenType.String && (string)value == Jump_string)
                                            {
                                                // 找到包含特定字符串值的对象，获取该对象的索引
                                                 index = Sequences.IndexOf(parentToken);
                                               
                                            }
                                        }
                                        else if (parentToken is JValue)
                                        {
                                            // 对于包含单个值的数组，将 JToken 对象转换为字符串并检查是否包含特定字符串值
                                            JValue parentValue = (JValue)parentToken;
                                            if (parentValue.Type == JTokenType.String && parentValue.Value<string>().Contains(Jump_string))
                                            {
                                                // 找到包含特定字符串值的值，获取该值的索引
                                                 index = Sequences.IndexOf(parentToken);
                                                Console.WriteLine("Element with targetValue found at index " + index);
                                            }
                                        }
                                    }
                                    j = index-1;
                                }
                                break;
                            }

                            //tempItems.tResult &= result;
                            //sequences[seqNo].TestResult &= tempItem.tResult;
                            //stationStatus &= sequences[seqNo].TestResult;
                        }
                        if (tResult == "FAIL" && Convert.ToInt32(GlobalNew.FAIL_CONTINUE) == 0)
                        {

                            if (tempItems.IfElse.Contains("JUMP"))
                            {
                                continue;
                            }

                            break;
                        }
                        //debug mod 和第一个大项 fail，不上传Mes，第一个大项默认Mes 测试项目
                        //if(GlobalNew.TESTMODE.ToLower()!="debug" && sequences.Count > 0) { UploadMes(); }

                        //治具出仓，保存log
                        //TestEnd();
                    }      //Sequences            
                    error_details_firstfail = GlobalNew.g_datacollection.GetMoreProp("Failitem");
                    Logger.Debug($"Failitem:{error_details_firstfail}");                    

                    if (stationStatus)
                    {
                        End_TestTime("PASS", null);
                        Total_TestTime(null);
                        finalTestResult = "PASS";
                        //JsonDataLists.Add("Info", jsonStr);
                        SetTestStatus(TestStatus.PASS);
                        WriteCSVResult(HeaderFilePath, CSVFile_path, GlobalNew.g_datacollection,false);//Total log in csv
                        WriteCSVResult(HeaderFilePath, CSVFile_path_PASS, GlobalNew.g_datacollection, false);
                        
                        sec = 0;
                        timer.Dispose();
                        GlobalNew.Total_Pass_Num++;
                        UpdateContLable();
                        WriteCountNumToFile();

                    }
                    else
                    {
                        End_TestTime("FAIL", null);
                        Total_TestTime(null);
                        finalTestResult = "FAIL";
                        string tmp2 = GlobalNew.g_datacollection.GetMoreProp("Failitem");
                        Logger.Debug($"Failitem:{tmp2}");
                        SetTestStatus(TestStatus.FAIL);
                        WriteCSVResult(HeaderFilePath, CSVFile_path, GlobalNew.g_datacollection, false);//Total log in csv
                        WriteCSVResult(HeaderFilePath, CSVFile_path_FAIL, GlobalNew.g_datacollection, false);
                        
                        sec = 0;
                        timer.Dispose();
                        GlobalNew.Total_Fail_Num++;
                        //SetButton(bt_errorCode, GUI_ErrorCode);

                        UpdateContLable();
                        WriteCountNumToFile();

                    }
                    //if (finalTestResult == "FAIL" && Convert.ToInt32(GlobalNew.CYCLEFAIL_CONTINUE) == 0)
                    //{
                    //    Logger.Error("finalTestResult == \"FAIL\" && Convert.ToInt32(GlobalNew.CYCLEFAIL_CONTINUE) == 0-->break");
                    //    break;
                    //}
                }//cycles

                while (false)
                {
                    // 如果开始标志为假则不运行程序
                    if (startFlag)
                    {
                        // 如果是第一个测试项，则记录sequence开始测试时间
                        if (itemsNo == 0)
                        {
                            //  sequences[seqNo].start_time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        }
                        // 当前测试用例项
                        Items tempItem = sequences[seqNo].SeqItems[itemsNo];
                        // 用于保存item测试结果，生成JSON格式文件
                        phase_items phase_item = new phase_items();
                        // 把测试结果置真,所有的测试步的结果,如果有一个假的测试项结果就会为假
                        //Sequences[seqNo].TestResult = true;
                        // tempItem.tResult = true;
                        //sequences[seqNo].IsTest = sequences[seqNo].IsTest | tempItem.isTest;
                        // 根据机型执行不同的测试用例
                        if (!String.IsNullOrEmpty(tempItem.Mode) && !tempItem.Mode.ToLower().Contains(DutMode.ToLower()))
                        {
                            //  tempItem.isTest = false;
                            SetTreeViewSeqColor(Color.Gray);
                        }

                        // 根据测试步骤的IF条件决定下一步骤不执行，
                        if (tempItem.IfElse.ToLower() == "else")
                        {
                            // if为true不执行else部分的,false执行。
                            //   tempItem.isTest = !IfCond;
                            //   if (!tempItem.isTest)
                            //   {
                            //       SetTreeViewSeqColor(Color.Gray);
                            //   }
                        }

                        if (singleStepTest)
                        {
                            SetTreeViewSeqColor(Color.Yellow);
                            if (pauseEvent.WaitOne())
                            {
                                // 每次测试前清除上次测试记录
                                tempItem.Clear();
                                //      Logger.Debug($"Start:{tempItem.ItemName},Keyword:{tempItem.TestKeyword},Retry {tempItem.RetryTimes},Timeout {tempItem.TimeOut}s,SubStr:{tempItem.SubStr1}-{tempItem.SubStr2},MesVer:{tempItem.MES_var},FTC:{tempItem.FTC}");
                                //   tempItem.startTime = DateTime.Now;
                                int retryTimes = 0;
                                if (!string.IsNullOrEmpty(tempItem.RetryTimes))
                                {
                                    retryTimes = int.Parse(tempItem.RetryTimes);
                                }
                                // 运行测试步骤
                                bool result = false;
                                for (retry = retryTimes; retry > -1; retry--)
                                {
                                    //if (test.StepTest(TestPhase, tempItem, retry, phase_item))
                                    //{
                                    //    result = true;
                                    //    break;
                                    //}
                                    //else
                                    //{
                                    //    if (retry == 0)
                                    //        result = false;
                                    //}
                                }

                                // 让测试步骤bypass
                                if (tempItem.Bypass == "1" && !result)
                                {
                                    Logger.Info($"Let this step:{tempItem.ItemName} bypass.");
                                    result = true;
                                }
                                else if (tempItem.Bypass == "0" && result)
                                {
                                    // 测试失败后的showlog步骤，show完log后测试结果设置为fail，并定义error_code和error_details
                                    error_code = tempItem.ErrorCode.Split(new string[] { "\n" }, 0)[0].Split(':')[0].Trim();
                                    error_details = tempItem.ErrorCode.Split(new string[] { "\n" }, 0)[0].Split(':')[1].Trim();
                                    Logger.Error($"Let this step:{tempItem.ItemName} byfail.Set error_code:{error_code},error_details:{error_details}");
                                    result = false;
                                }

                                // 根据测试结果显示测试步为绿色或者红色
                                SetTreeViewSeqColor(result ? Color.Green : Color.Red);

                                // if条件语句
                                if (tempItem.IfElse.ToLower() == "if")
                                {
                                    SetTreeViewSeqColor(result ? Color.Green : Color.Pink);
                                    // 设置if执行反馈结果，下面的测试步骤根据这个结果决定是否执行。
                                    IfCond = result;
                                    if (!result)
                                        Logger.Info($"if statement FAIL needs to continue, setting the test result to true");
                                    result = true;
                                }
                                else if (tempItem.IfElse.ToLower() == "else")
                                {
                                }
                                else
                                {
                                    IfCond = true;
                                }

                                // 记录测试结果
                                //  tempItem.tResult &= result;
                                // sequences[seqNo].TestResult &= tempItem.tResult;
                                //    stationStatus &= sequences[seqNo].TestResult;
                                // 如果是单步模式不往下运行,只运行当前测试项
                                if (singleStepTest)
                                {
                                    startFlag = false;
                                    SetTextBox(textBox2);
                                    continue;
                                }
                                else
                                {
                                    Station.status = stationStatus.ToString();
                                }


                                // 循环测试处理
                                if (!isCycle)
                                {
                                    // 测试fail停止测试,生成结果or fail继续测试。
                                    if (Global.FAIL_CONTINUE != "1")
                                    {
                                        if (sequences[seqNo].SeqName == "VerifyDUT" && TestPhase.phase_items.Count == 0)
                                        {
                                            //phase_item.mac = mescheckroute.GetCsnErroMessage(SN, out string sn, out string IPSN, out string MesMac, out string mesMsg) ? MesMac : "";
                                            phase_item.serial = SN;
                                            TestPhase.phase_items.Add(phase_item);
                                            Logger.Debug("Add phaseItem to testPhase");
                                        }
                                        // sequence结束时间.
                                        //    sequences[seqNo].finish_time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                        //   DateTime.TryParse(sequences[seqNo].start_time, out DateTime datetime);
                                        //    string elapsedTime = String.Format("{0,0:F0}", Convert.ToDouble((DateTime.Now - datetime).TotalSeconds));
                                        // 把seq测试结果保存到test_phase变量中.
                                        TestPhase.Copy(sequences[seqNo], this);
                                        // 加入station实例,记录测试结果 用于序列化Json文件.
                                        if (DutMode.ToLower() == "leaf" && tempItem.ItemName == "ReadBTZFwVersion") { }
                                        else { Station.test_phases.Add(TestPhase); }
                                        // 把testPhase初始化
                                        TestPhase = new test_phases();
                                        AddStationResult(false, error_code_firstfail, error_details_firstfail);

                                        UploadJsonToClient();
                                        PostAsyncJsonToMes();
                                        saveTestResult();
                                        collectCsvResult();
                                        SetTestStatus(TestStatus.FAIL);
                                    }
                                }
                                else
                                {

                                    lb_FailNum.InvokeOnToolStripItem(lb_FailNum => lb_FailNum.Text = Global.Total_Fail_Num.ToString());

                                }
                            }
                        }

                        // 测试项序号+1,继续下一个item.
                        itemsNo++;
                        // 测试结果处理.
                        if (Global.FAIL_CONTINUE == "1")
                        {
                            // 如果测试item是所在Seq中最后一个or测试失败.
                            if (itemsNo >= sequences[seqNo].SeqItems.Count)
                            {
                                // sequence结束时间.
                                //  sequences[seqNo].finish_time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                // DateTime.TryParse(sequences[seqNo].start_time, out DateTime datetime);
                                //    string elapsedTime = String.Format("{0,0:F0}", Convert.ToDouble((DateTime.Now - datetime).TotalSeconds));
                                // 把seq测试结果保存到test_phase变量中.
                                TestPhase.Copy(sequences[seqNo], this);
                                // 加入station实例,记录测试结果 用于序列化Json文件
                                if ((DutMode.ToLower() == "Leaf" && tempItem.ItemName == "ReadBTZFwVersion")) { }
                                else { Station.test_phases.Add(TestPhase); }
                                // 把testPhase初始化
                                TestPhase = new test_phases();

                                itemsNo = 0;
                                // 测试结束标志为真
                                // sequences[seqNo].IsTestFinished = true;
                                // 测试用例号+1
                                seqNo++;
                                // 如果是最后一个测试用例则上传测试结果,结束测试
                                if (seqNo >= sequences.Count)
                                {
                                    seqNo = 0;
                                    // 循环测试不生成结果
                                    if (!isCycle)
                                    {
                                        AddStationResult(stationStatus, error_code_firstfail, error_details_firstfail);
                                        //上传结果到MES失败,此处用&而不用&&。
                                        if (UploadJsonToClient() & PostAsyncJsonToMes() & stationStatus)
                                        {
                                            finalTestResult = stationStatus.ToString().ToUpper() == "TRUE" ? "PASS" : "FAIL";
                                            collectCsvResult();
                                            SetTestStatus(TestStatus.PASS);
                                        }
                                        else
                                        {
                                            collectCsvResult();
                                            SetTestStatus(TestStatus.FAIL);
                                        }
                                        startScanFlag = true;
                                        autoScanEvent.Set();    // 开始自动扫描
                                        saveTestResult();
                                    }
                                    Thread.Sleep(10);
                                }
                                else
                                {
                                    // 初始化要测试的测试用例参数,重复测试的时候,这些值需要清除
                                    sequences[seqNo].Clear();
                                    inPutValue = "";
                                }
                            }
                        }
                    }
                    else
                        Thread.Sleep(10);
                }

                SaveLogtoServer();
            }
            catch (Exception ex) when (ex.Message.Contains("正在中止线程"))
            {
                SaveLogtoServer();
                //abort线程忽略报错
                Logger.Warn(ex.Message);
                return;
            }

            catch (Exception ex)
            {
                SaveLogtoServer();
                Logger.Fatal("TestThread() Exception:" + ex.ToString());
            }
            finally
            {
                
                testThread.Abort();
            }
        }

        #endregion 

        // 测试前期处理函数 
        #region ################################ 9. Sub Function (for task) ########################################

        //////////////////////////////////////////////////////////////////////////////////////
        // 9-1. Pass / Fail number                                                          //
        //      更新状态栏计数                                                               //
        //////////////////////////////////////////////////////////////////////////////////////
        public void UpdateContLable()
        {

            lb_passNum.InvokeOnToolStripItem(lb_passNum => lb_passNum.Text = GlobalNew.Total_Pass_Num.ToString());
            lb_FailNum.InvokeOnToolStripItem(lb_FailNum => lb_FailNum.Text = GlobalNew.Total_Fail_Num.ToString());
            lb_YieldNum.InvokeOnToolStripItem(lb_YieldNum => lb_YieldNum.Text = $@"{(GlobalNew.Total_Pass_Num / (double)(GlobalNew.Total_Pass_Num + GlobalNew.Total_Fail_Num)),0:P2}");
        }

        //////////////////////////////////////////////////////////////////////////////////////
        // 9-2. Check fail counter                                                          //
        //      检查Fail次数是否超过限定值                                                    //
        //////////////////////////////////////////////////////////////////////////////////////
        private bool CheckContinueFailNum()
        {
            if (GlobalNew.Total_Fail_Num >= GlobalNew.CONTINUE_FAIL_LIMIT)
            {
                //  toolStripContinuFailNum.InvokeOnToolStripItem(toolStripContinuFailNum => toolStripContinuFailNum.ForeColor = Color.Red);
                ContinuousFailReset_Click(null, null);
                return false;
            }
            else
            {
                //  toolStripContinuFailNum.InvokeOnToolStripItem(toolStripContinuFailNum => toolStripContinuFailNum.ForeColor = Color.Black);
                return true;
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////
        // 9-3. Get Fix Name                                                                //
        //      获取治具编号/测试工站编号                                                     //
        //////////////////////////////////////////////////////////////////////////////////////
        public void GetFixName()
        {
            //return;
            if (Global.FIXTUREFLAG == "0") { return; }
            try
            {

                for (int i = 0; i < 3; i++)
                {
                    string recvStr = Environment.MachineName;           //以PC名称作为Fixture number 
#if DEBUG
                    recvStr = "L1A11-PCBA01-01";
#endif
                    if (recvStr.IndexOf('-') == -1)
                    {
                        // "PC Name error,PC Name:Line ID-Station Name-Fixture ID"
                        bt_errorCode.BackColor = Color.Red;
                        bt_errorCode.Text = "PC Name error,PC Name:Line ID-Station Name-Fixture ID";
                        return;
                    }
                    string[] pcName = recvStr.Split('-');
                    GlobalNew.STATIONNO = pcName[1] + "-" + pcName[2];
                    GlobalNew.STATIONNAME = pcName[1];
                    iniConfig.Writeini("Station", "STATIONNAME", GlobalNew.STATIONNAME);
                    iniConfig.Writeini("Station", "STATIONNO", GlobalNew.STATIONNO);
                    Logger.Debug($"Read fix number success,stationName:{GlobalNew.STATIONNAME}");
                    break;

                    if (i == 2)
                    {
                        MessageBox.Show($"Read FixNum error,Please check it!");
                        System.Environment.Exit(0);
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}");
                Application.Exit();
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////
        // 9-4. Update Continue status                                                      //
        //      根据测试结果更新连续Fail计数                                                  //
        //////////////////////////////////////////////////////////////////////////////////////
        public void UpdateContinueFail(bool testResult)
        {
            if (IsDebug)
            {
                return;
            }
            if (testResult)
                Global.ContinueFailNum = 0;
            else
                Global.ContinueFailNum++;
            iniConfig.Writeini("CountNum", "ContinueFailNum", Global.ContinueFailNum.ToString());
            //lb_ContinuousFailNum.InvokeOnToolStripItem(lb_ContinuousFailNum => lb_ContinuousFailNum.Text = Global.ContinueFailNum.ToString());
        }

        //////////////////////////////////////////////////////////////////////////////////////
        // 9-5. Write Count Num To File                                                     //
        //      把Fail PASS数量写入ini配置文件                                                //
        //////////////////////////////////////////////////////////////////////////////////////
        private void WriteCountNumToFile()
        {
            try
            {
                iniConfig.Writeini("CountNum", "Total_Pass_Num", GlobalNew.Total_Pass_Num.ToString());
                iniConfig.Writeini("CountNum", "Total_Fail_Num", GlobalNew.Total_Fail_Num.ToString());
                iniConfig.Writeini("CountNum", "Total_Abort_Num", Global.Total_Abort_Num.ToString());
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.ToString());
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////
        // 9-6. Add Station Result                                                          //
        //      顯示測試結果                                                                 //
        //////////////////////////////////////////////////////////////////////////////////////
        public void AddStationResult(bool _result, string _errorcode, string _errordetails)
        {
            Station.finish_time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            Station.status = _result ? "passed" : "failed";
            Station.error_code = _errorcode;
            Station.error_details = _errordetails;
            Station.CopyToMES(mesPhases);
        }

        //////////////////////////////////////////////////////////////////////////////////////
        // 9-7. Upload Json file                                                            //
        //      上傳Json API                                                                //
        //////////////////////////////////////////////////////////////////////////////////////
        private bool UploadJsonToClient()
        {
            DateTime startUpload = DateTime.Now;
            bool result = false;
            string JsonPath = $@"{Global.LogPath}\Json\{SN}_{DateTime.Now:HHmmss}.json";
            try
            {
                if (!JsonSerializer(Station, out string JsonStr, JsonPath))
                {
                    Logger.Error("Serialize station Json info error!!!...");
                    return false;
                }
                if (IsDebug)
                {
                    Logger.Debug($"IsDebug={IsDebug},don't upload Json to API");
                    return true;
                }
                if (!UploadJson(JsonPath))
                {
                    Thread.Sleep(1000);
                    result = UploadJson(JsonPath);
                }
                else
                    result = true;
            }
            catch (Exception ex)
            {
                result = false;
                Logger.Fatal(ex.ToString());
            }
            var elapsedTime = $"{Convert.ToDouble((DateTime.Now - startUpload).TotalSeconds),0:F0}";
            UpdateDetailView(SN, "UploadJson", null, null, null, null, elapsedTime, startUpload.ToString(), result.ToString());
            Logger.Debug($"UploadJsonToClient {(result ? "PASS" : "FAIL")}!! ElapsedTime:{elapsedTime}.");
            mesPhases.JSON_UPLOAD_Time = elapsedTime;
            return result;
        }

        public bool UploadJson(string JsonFilePath)
        {
            try
            {
                string cmd = $@"python {System.Environment.CurrentDirectory}\Config\{Global.PySCRIPT} -s {Global.STATIONNAME} -f {JsonFilePath}";
                string responds = RunDosCmd(cmd, out string errors, 30000);
                if (responds.Contains("Result:200"))
                {
                    mesPhases.JSON_UPLOAD = "TRUE";
                    return true;
                }
                else
                {
                    Logger.Error("Json-info upload to client fail:" + errors);
                    mesPhases.JSON_UPLOAD = "FALSE";
                    if (Station.status == "passed")
                    {
                        mesPhases.error_code = "JSON_UPLOAD";
                        mesPhases.status = "failed";
                        error_code = "JSON_UPLOAD";
                        error_details = "JSON_UPLOAD";
                        error_code_firstfail = "JSON_UPLOAD";
                        error_details_firstfail = "JSON_UPLOAD";
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logger.Fatal($"json_upload_Exception:{ex.ToString()}");
                return false;
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////
        // 9-8. Post Async Json To Mes                                                      //
        //      上传json文件to Mes                                                           //
        //////////////////////////////////////////////////////////////////////////////////////
        private bool PostAsyncJsonToMes()
        {
            DateTime startUpload = DateTime.Now;
            bool result = false;
            try
            {
                Logger.Debug("Start to Serialize station Json info...");
                MesPhases mesPhasesUpload = ForeachClassFields<MesPhases>(mesPhases, "TRUE");
                JsonSerializer(mesPhasesUpload, out string currentMes);
                mesUrl = $"http://{Global.MESIP}:{Global.MESPORT}/api/2/serial/{SN}/station/{Global.FIXTURENAME}/info";
                Logger.Debug($"mesUrl:{mesUrl}");
                if (Global.TESTMODE.ToLower() == "debug" || IsDebug)                                                   //debug模式下不上传测试结果到MES
                //if (IsDebug)                                                   //debug模式下不上传测试结果到MES
                {
                    Logger.Debug($"TESTMODE=debug or IsDebug={IsDebug},don't upload Json to MES");
                    return true;
                }
                Logger.Debug("Start to upload MES info...");
                var client = new HttpClient();
                StringContent content = new StringContent(currentMes, Encoding.UTF8, "application/json");
                HttpResponseMessage httpResponse = client.PostAsync(mesUrl, content).GetAwaiter().GetResult();
                if (httpResponse.IsSuccessStatusCode || httpResponse.StatusCode == HttpStatusCode.BadRequest)
                {
                    mesPhases.MES_UPLOAD = "TRUE";
                    result = true;
                }
                else
                {
                    if (Station.status == "passed")
                    {
                        error_code = "UploadJsonToMES";
                        error_details = "UploadJsonToMES";
                        error_code_firstfail = "UploadJsonToMES";
                        error_details_firstfail = "UploadJsonToMES";
                    }
                    mesPhases.MES_UPLOAD = "FALSE";
                    Logger.Error($"MES-info Upload Fail.Response code:{httpResponse.StatusCode}");
                }

                string responseBody = httpResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                Logger.Debug("MES responseBody:" + responseBody);
                if (responseBody.Contains("First test fail"))
                    SetLables(lbl_failCount, "1", Color.White);
                else if (responseBody.Contains("Second test fail"))
                    SetLables(lbl_failCount, "2", Color.White);
                else if (responseBody.Contains("NG"))
                    SetLables(lbl_failCount, "NG", Color.White);
                //SetDefaultIP = true;
                else if (responseBody.Contains("Next"))
                {
                    SetLables(lbl_failCount, responseBody.ToString(), Color.Red);
                    //error_details_firstfail = responseBody;
                }
                //2021 - 01 - 12 12:32:44 - MES responseBody: "First test fail"
                //2021 - 01 - 12 12:33:36 - MES responseBody: "Second test fail"
                //2021 - 01 - 12 12:47:58 - MES responseBody: "NG,Finally Test fail SN Need Reapri BUT PASS"
                //"Next:O-SFT /Current:O-RUNIN"
            }
            catch (Exception ex)
            {
                Logger.Fatal("UploadJsonToMESException:" + ex.ToString());
            }
            string elapsedTime = $"{Convert.ToDouble((DateTime.Now - startUpload).TotalSeconds),0:F0}";
            UpdateDetailView(SN, "UploadMes", null, null, null, null, elapsedTime, startUpload.ToString(), result.ToString());
            Logger.Debug($"PostAsyncJsonToMES {(result ? "PASS" : "FAIL")}!! ElapsedTime:{elapsedTime}.");
            return result;
        }


        //////////////////////////////////////////////////////////////////////////////////////
        // 9-9. Initial window form                                                         //
        //      初始化窗口表格控件                                                           //
        //////////////////////////////////////////////////////////////////////////////////////
        private void InitdataGridViewColumns()
        {

            tempDataView.Clear();



            //dataGridViewDetail.ColumnCount = colHeader.Length;


            for (int i = 0; i < colHeader.Length; i++)
            {
                //tempDataView.Columns.Add(colHeader[i]);
                //dataGridViewDetail.Columns[i].HeaderText = colHeader[i];
                //dataGridViewDetail.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
                //dataGridViewDetail.Columns[i].DefaultCellStyle.ForeColor = Color.White;
                //dataGridViewDetail.Columns[i].HeaderCell.Style.BackColor = Color.Blue;
                //dataGridViewDetail.Columns[i].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                //dataGridViewDetail.Columns[i].ReadOnly = true;
                //dataGridViewDetail.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                tempDataView.Columns.Add(colHeader[i]);
                
            }

            dataGridViewDetail.DataSource = tempDataView;
            //dataGridViewDetail.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            dataGridViewDetail.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridViewDetail.Columns["Spec"].DefaultCellStyle.WrapMode = DataGridViewTriState.True;

            dataGridViewDetail.Columns["NO."].Width = 50;
            dataGridViewDetail.Columns["SN"].Width = 80;
            dataGridViewDetail.Columns["tResult"].Width = 80;
            dataGridViewDetail.Columns["ItemName"].Width = 100;
            dataGridViewDetail.Columns["Spec"].Width = 150;
            dataGridViewDetail.Columns["tValue"].Width = 280;
            dataGridViewDetail.Columns["StartTime"].Width = 160;
            dataGridViewDetail.Columns["EndTime"].Width = 160;

        }

        #endregion 测试前期处理函数

        // 輸出Report
        #region ################################ 10. Write CSV Result  ############################################

        //////////////////////////////////////////////////////////////////////////////////////
        // 10-1. Write CSV Result                                                           //
        //       輸出Report                                                                 //
        //////////////////////////////////////////////////////////////////////////////////////
        static void WriteCSVResult(string HeaderFilePath, string csvFilePath, DataCollection g_datacollection,bool showolg)
        {      
            try
            {
                string directoryPath = Path.GetDirectoryName(csvFilePath);
                string csvlog = string.Empty;
                // 檢查目錄是否存在，如果不存在則創建
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                string temp = string.Empty;
                string jsonHeaderData = File.ReadAllText(HeaderFilePath, encoding: Encoding.UTF8);
                var headerData = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(jsonHeaderData);
                List<string> headers = headerData["headers"];

                if (headers.Count == 0)
                {
                    Logger.Debug("Header Count = 0,Can't Parse CSV Data");
                    return;
                }
                bool fileExists = File.Exists(csvFilePath);

                string directoryName = Path.GetDirectoryName(csvFilePath);
                if (!Directory.Exists(directoryName))
                {
                    Directory.CreateDirectory(directoryName);
                }
                bool writeHeaders = true;

                if (fileExists)
                {
                    var fileContent = File.ReadAllText(csvFilePath, encoding: Encoding.UTF8);
                    if (headers[0] != null)
                        writeHeaders = !fileContent.Contains(headers[0]);
                    else
                        MessageBox.Show("The first of Header is null rewrite Header to csv File", "Note", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                using (var writer = new StreamWriter(csvFilePath, append: true, encoding: Encoding.UTF8))
                using (var csv = new CsvWriter(writer, System.Globalization.CultureInfo.InvariantCulture))
                {
                    if (writeHeaders)
                    {
                        foreach (var header in headers)
                        {
                            csv.WriteField(header);
                        }
                        csv.NextRecord();
                    }
                    int count = 1;
                    // 將解析後的數據逐個添加到 CSV 檔案的一列中
                    foreach (var header in headers)
                    {
                        string fielddata = "";
                        //foreach (var data in dataList)
                        {
                            // 解析 JSON 字符串
                            //Dictionary<string, object> jsonData;
                            try
                            {
                                temp = g_datacollection.GetMoreProp(header);

                                if (temp != "")
                                {
                                    fielddata = temp;

                                    csvlog += $"{count++}.[{header}] = {temp}\n";
                                }
                                else
                                {
                                    csvlog += $"{count++}.[{header}] = NULL\n";
                                }
                                


                            }
                            catch (Exception ex)
                            {
                                continue;
                            }
                        }
                        csv.WriteField(fielddata);
                    }

                    csv.NextRecord();
                }

                if(showolg)
                    Logger.Info($"\n=============CSVData=============\n{csvlog}");

                Logger.Debug($"Save test CSV OK.{csvFilePath}");
                if(!string.IsNullOrEmpty(GlobalNew.ServerLOGFOLDER))
                {
                    string destinationPath = csvFilePath.Replace(GlobalNew.LOGFOLDER, GlobalNew.ServerLOGFOLDER);
                    CopyFile(csvFilePath, destinationPath);
                }
            }
            catch (IOException e1)
            {
                // 處理例外狀況             
                Logger.Debug($"Save test CSV Fail.{csvFilePath} Exception:{e1.Message}");
                MessageBox.Show($"CSV Warn: {e1.Message},Please Close {Path.GetFileName(csvFilePath)}", "CSV File", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                Logger.Debug($"Save test CSV Fail.{csvFilePath} Exception:{ex.Message}");
            }

            
        }

        //////////////////////////////////////////////////////////////////////////////////////
        // 10-2. Save Test Result ToCsv (not use)                                           //
        //       生成测试report,收集测试结果                                                  //
        //////////////////////////////////////////////////////////////////////////////////////
        private void SaveTestResultToCsv()
        {
            try
            {
                SetButtonPro(buttonBegin, Properties.Resources.start);
                SetButtonPro(buttonExit, Properties.Resources.close);
                string reportPath = Environment.CurrentDirectory + @"\Output\result.csv";
                CreatCSVFile(reportPath, colHeader);
                DataGridViewToCSV(dataGridViewDetail, true, reportPath);
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.ToString());
            }
        }


        //////////////////////////////////////////////////////////////////////////////////////
        // 10-3. Collect Result To Csv (not use)                                            //
        //       收集测试数据到CSV文件                                                        //
        //////////////////////////////////////////////////////////////////////////////////////
        private void CollectResultToCsv()
        {
            List<string[]> testDataList = new List<String[]>();
            string[] testData = new string[] { };
            string[] testData_yuqinag = null;
            CSVFilePath = $@"{Global.LOGFOLDER}\CsvData\{DateTime.Now.ToString("yyyy-MM-dd--HH")}-00-00_{Global.STATIONNO}.csv";
            string csvColumnPath = $@"{Environment.CurrentDirectory}\Config\{Global.STATIONNAME}_CSV_COLUMN.txt";
            //string reportPath =$@"{Environment.CurrentDirectory}\Output\SFTresult.csv";
            //string CSVFilePath = $@"{Environment.CurrentDirectory}\Output\{DateTime.Now.ToString("yyyy-MM-dd")}_{Global.STATIONNO}.csv";
            try
            {
                SetButtonPro(buttonBegin, Properties.Resources.start);
                SetButtonPro(buttonExit, Properties.Resources.close);

                if (Global.STATIONNAME == "MBLT")
                {
                    testData = new string[]                    {
                        DutMode,Global.STATIONNAME,"Luxshare",WorkOrder,Global.FIXTURENAME,"1",SN,mesPhases.FW_VERSION,mesPhases.HW_REVISION,mesPhases.test_software_version,startTime.ToString("yyyy/MM/dd HH:mm:ss"),sec.ToString(), finalTestResult,mesPhases.FIRST_FAIL,error_details_firstfail,"UTC",Global.TESTMODE=="debug"?"1":"0",mesPhases.JSON_UPLOAD,mesPhases.MES_UPLOAD,
                        mesPhases.CurrentShortTest,mesPhases.CURRENT_OPEN,"0.1","1",mesPhases.CURRENT_IDLE,"0.2","0.6",
                        mesPhases.USBC_VBUS,"4.75","5.25",mesPhases.DVDD3_3,"3.15","3.45",mesPhases.DVDD1_95,"1.9","2.05",mesPhases.DVDD2_2,"2.1","2.3",mesPhases.DVDD0_912,"0.87","0.95",mesPhases.DVDD1_29_MP_CORE,"1.25","1.35",mesPhases.DVDD1_35_MP_DDR,"1.283","1.45",
                        mesPhases.ThermalShutdownTest,mesPhases.ReadBoardRegisterValue,mesPhases.SaveIdentityInEnv,mesPhases.CheckBZTBootloaderVersion,mesPhases.CPUVersionTest,
                        mesPhases.SubsystemTest,mesPhases.EMMC_VENDOR,
                        mesPhases.MMCWrite117,"35","200",mesPhases.MMCWrite120,"35","200",mesPhases.MMCRead017,"15","40",mesPhases.MMCRead020,"15","40",
                        mesPhases.ResetButtonTest,mesPhases.LED_ALLOFF,mesPhases.LED_R_ON,mesPhases.LED_B_ON,mesPhases.LED_G_ON,mesPhases.LED_W_ON,
                        mesPhases.ETH0_THROUGHPUT_SEND,"900","1000",mesPhases.ETH0_THROUGHPUT_RECEIVE,"900","1000",mesPhases.ETH1_THROUGHPUT_SEND,"900","1000",mesPhases.ETH1_THROUGHPUT_RECEIVE,"900","1000"
                    };
                }
                if (Global.STATIONNAME == "SFT")
                {
                    testData = new string[]                    {
                        DutMode,Global.STATIONNAME,"Luxshare",WorkOrder,Global.FIXTURENAME,"1",SN,mesPhases.FW_VERSION,mesPhases.HW_REVISION,mesPhases.test_software_version,startTime.ToString("yyyy/MM/dd HH:mm:ss"),sec.ToString(), finalTestResult,mesPhases.FIRST_FAIL,error_details_firstfail,"UTC",Global.TESTMODE=="debug"?"1":"0",mesPhases.JSON_UPLOAD,mesPhases.MES_UPLOAD,
                        mesPhases.VerifySFIS,mesPhases.CurrentShortTest,mesPhases.CURRENT_OPEN,"0.1","1",mesPhases.VerifyDUT,
                        mesPhases.LED_MODEL,mesPhases.PHY_STATUS,mesPhases.EMMC_VENDOR,mesPhases.FUSB_STATUS,
                        mesPhases.ChildBoardSN, mesPhases.ResetButtonTest,
                        mesPhases.LED_OFF_LUM,"0","5",
                        mesPhases.W_x,"0.31","0.405",mesPhases.W_y,"0.33","0.395",mesPhases.W_L,"59582","87839",
                        mesPhases.G_x,"0.165","0.235",mesPhases.G_y,"0.675","0.735",mesPhases.G_L,"34895","60990",
                        mesPhases.R_x,"0.645","0.725",mesPhases.R_y,"0.255","0.335",mesPhases.R_L,"38499","64151",
                        mesPhases.B_x,"0.122","0.145",mesPhases.B_y,"0.04","0.075",mesPhases.B_L,"40154","68193",
                        mesPhases.ETH0_THROUGHPUT_SEND,"900","1000",mesPhases.ETH0_THROUGHPUT_RECEIVE,"900","1000",mesPhases.ETH1_THROUGHPUT_SEND,"900","1000",mesPhases.ETH1_THROUGHPUT_RECEIVE,"900","1000",
                        mesPhases.USB_WRITE_SPEED,"0","60",mesPhases.USB_READ_SPEED,"0","60"
                    };
                }
                if (Global.STATIONNAME == "RTT")
                {
                    testData = new string[]                    {
                        DutMode,Global.STATIONNAME,"Luxshare",WorkOrder,Global.FIXTURENAME,"1",SN,mesPhases.FW_VERSION,mesPhases.HW_REVISION,mesPhases.test_software_version,startTime.ToString("yyyy/MM/dd HH:mm:ss"),sec.ToString(), finalTestResult,mesPhases.FIRST_FAIL,error_details_firstfail,"UTC",Global.TESTMODE=="debug"?"1":"0",mesPhases.JSON_UPLOAD,mesPhases.MES_UPLOAD,
                        mesPhases.VerifySFIS,mesPhases.VerifyDUT,mesPhases.Temp_AfterBoot,"0","100",
                        mesPhases.LED_MODEL,mesPhases.PHY_STATUS,mesPhases.EMMC_VENDOR,mesPhases.FUSB_STATUS,
                        mesPhases.ChildBoardSN,mesPhases.LoadBZTFDrivers,mesPhases.LoadWiFiDrivers,
                        mesPhases.WIFI2G_THROUGHPUT_SERIAL,"900","1000",mesPhases.WIFI5G_THROUGHPUT_SERIAL,"900","1000",  mesPhases.WIFI2G_THROUGHPUT_PARALLEL,"900","1000",mesPhases.WIFI5G_THROUGHPUT_PARALLEL,"900","1000",
                        mesPhases.Temp_AfterWiFiSpeedTest,"0","100",mesPhases.BluetoothFunctionTest,mesPhases.SetBootcmdToDHCP
                    };
                }
                if (Global.STATIONNAME == "FRTT")
                {
                    testData = new string[]                    {
                        DutMode,Global.STATIONNAME,"Luxshare",WorkOrder,Global.FIXTURENAME,"1",SN,mesPhases.FW_VERSION,mesPhases.HW_REVISION,mesPhases.test_software_version,startTime.ToString("yyyy/MM/dd HH:mm:ss"),sec.ToString(), finalTestResult,mesPhases.FIRST_FAIL,error_details_firstfail,"UTC",Global.TESTMODE=="debug"?"1":"0",mesPhases.JSON_UPLOAD,mesPhases.MES_UPLOAD,
                        mesPhases.VerifySFIS,mesPhases.CurrentShortTest,mesPhases.CURRENT_OPEN,"0.1","1",mesPhases.VerifyDUT,
                        mesPhases.Temp_AfterBoot,"0","100",
                        mesPhases.LED_MODEL,mesPhases.PHY_STATUS,mesPhases.EMMC_VENDOR,mesPhases.FUSB_STATUS,
                        mesPhases.ChildBoardSN,mesPhases.LoadBZTFDrivers,mesPhases.LoadWiFiDrivers,mesPhases.ResetButtonTest,
                        mesPhases.LED_OFF_LUM,"0","5",
                        mesPhases.W_x,"0.31","0.405",mesPhases.W_y,"0.33","0.395",mesPhases.W_L,"59582","87839",
                        mesPhases.G_x,"0.165","0.235",mesPhases.G_y,"0.675","0.735",mesPhases.G_L,"34895","60990",
                        mesPhases.R_x,"0.645","0.725",mesPhases.R_y,"0.255","0.335",mesPhases.R_L,"38499","64151",
                        mesPhases.B_x,"0.122","0.145",mesPhases.B_y,"0.04","0.075",mesPhases.B_L,"40154","68193",
                        mesPhases.ETH0_THROUGHPUT_SEND,"900","1000",mesPhases.ETH0_THROUGHPUT_RECEIVE,"900","1000",mesPhases.ETH1_THROUGHPUT_SEND,"900","1000",mesPhases.ETH1_THROUGHPUT_RECEIVE,"900","1000",
                        mesPhases.USB_WRITE_SPEED,"0","60",mesPhases.USB_READ_SPEED,"0","60",
                        mesPhases.WIFI2G_THROUGHPUT_SERIAL,"900","1000",mesPhases.WIFI5G_THROUGHPUT_SERIAL,"900","1000",  mesPhases.WIFI2G_THROUGHPUT_PARALLEL,"900","1000",mesPhases.WIFI5G_THROUGHPUT_PARALLEL,"900","1000",
                        mesPhases.Temp_AfterWiFiSpeedTest,"0","100",mesPhases.BluetoothFunctionTest
                    };
                }

                if (Global.STATIONNAME == "MBFT")
                {
                    ArrayListCsv.InsertRange(0, new string[] {
                       DutMode,Global.STATIONNAME,"Luxshare",WorkOrder,Global.FIXTURENAME,"1",SN,mesPhases.FW_VERSION,mesPhases.HW_REVISION,mesPhases.test_software_version,startTime.ToString("yyyy/MM/dd HH:mm:ss"),sec.ToString(),finalTestResult,mesPhases.FIRST_FAIL,error_details_firstfail,"UTC",Global.TESTMODE=="debug" ? "1":"0",mesPhases.JSON_UPLOAD,mesPhases.MES_UPLOAD,
                         mesPhases.VerifySFIS,mesPhases.VerifyDUT,mesPhases.ThermalShutdownCheck,
                         mesPhases.LED_MODEL,mesPhases.PHY_STATUS,mesPhases.EMMC_VENDOR,mesPhases.FUSB_STATUS,
                         mesPhases.LoadWiFiDrivers,mesPhases.SpruceCanCommunicateTest
                    });
                    testData = ArrayListCsv.ToArray();
                }

                if (Global.STATIONNAME == "SRF")
                {
                    //#if DEBUG
                    ArrayListCsvHeader.InsertRange(0, new string[] {
                        "DEVICE_TYPE", "STATION_TYPE", "FACILITY_ID", "LINE_ID", "FIXTURE_ID", "DUT_POSITION", "SN", "FW_VERSION",  "HW_REVISION", "SW_VERSION",  "START_TIME", "TEST_DURATION","DUT_TEST_RESULT", "FIRST_FAIL", "ERROR_CODE", "TIME_ZONE", "TEST_DEBUG", "JSON_UPLOAD", "MES_UPLOAD",
                       "VERIFYSFIS", "DUT_OK", "TEMP_BOOT", "TEMP_BOOT_LIMIT_MIN", "TEMP_BOOT_LIMIT_MAX", "LED_MODEL", "PHY_STATUS", "MMC_MODEL", "FUSB_STATUS", "REPORT_CHILD", "LOAD_BZT", "LOAD_WIFI",
                    });
                    // 更新csv表头，写入txt
                    File.Delete(csvColumnPath);
                    using (StreamWriter sw = new StreamWriter(csvColumnPath, true, Encoding.Default))
                    {
                        foreach (var item in ArrayListCsvHeader)
                            sw.Write(item + "\t");
                    }


                    ArrayListCsvHeader_yuqiang.InsertRange(0, new string[] { "STATION_NO", "SN", "TIME" });
                    // 更新csv表头，写入txt
                    string csvColumnPath_yuqiang = $@"{Environment.CurrentDirectory}\Config\{Global.STATIONNAME}_CSV_COLUMN_yuqiang.txt";
                    File.Delete(csvColumnPath_yuqiang);
                    using (StreamWriter sw = new StreamWriter(csvColumnPath_yuqiang, true, Encoding.Default))
                    {
                        foreach (var item in ArrayListCsvHeader_yuqiang)
                            sw.Write(item + "\t");
                    }

                    ArrayListCsvHeader_loss.InsertRange(0, new string[] { "STATION_NO", "SN", "TIME" });
                    // 更新csv表头，写入txt
                    string csvColumnPath_loss = $@"{Environment.CurrentDirectory}\Config\{Global.STATIONNAME}_CSV_COLUMN_loss.txt";
                    File.Delete(csvColumnPath_loss);
                    using (StreamWriter sw = new StreamWriter(csvColumnPath_loss, true, Encoding.Default))
                    {
                        foreach (var item in ArrayListCsvHeader_loss)
                            sw.Write(item + "\t");
                    }
                    //#else
                    ArrayListCsv.InsertRange(0, new string[] {
                        DutMode, Global.STATIONNAME, "Luxshare", WorkOrder, Global.FIXTURENAME, "1", SN, mesPhases.FW_VERSION, mesPhases.HW_REVISION, mesPhases.test_software_version, startTime.ToString("yyyy/MM/dd HH:mm:ss"), sec.ToString(), finalTestResult, mesPhases.FIRST_FAIL, error_details_firstfail, "UTC", Global.TESTMODE == "debug" ? "1" : "0", mesPhases.JSON_UPLOAD, mesPhases.MES_UPLOAD,
                         mesPhases.VerifySFIS,mesPhases.VerifyDUT,mesPhases.Temp_AfterBoot,"0","100",
                         mesPhases.LED_MODEL,mesPhases.PHY_STATUS,mesPhases.EMMC_VENDOR,mesPhases.FUSB_STATUS,
                         mesPhases.ChildBoardSN,mesPhases.LoadBZTFDrivers,mesPhases.LoadWiFiDrivers
                    });
                    testData = ArrayListCsv.ToArray();

                    ArrayListCsv_yuqiang.InsertRange(0, new string[] { Global.FIXTURENAME, SN, startTime.ToString("yyyy/MM/dd HH:mm:ss") });
                    testData_yuqinag = ArrayListCsv_yuqiang.ToArray();
                    //#endif
                    List<string[]> testDataList_yuqinag = new List<string[]>();
                    testDataList_yuqinag.Add(testData_yuqinag);
                    string reportPathSRF_yuqiang = $@"{Global.LogPath}\{DateTime.Now.ToString("yyyy-MM-dd")}_{Global.STATIONNO}_yuqiang.csv";
                    CreatCSVFile(reportPathSRF_yuqiang, csvColumnPath_yuqiang);
                    WriteCSV(reportPathSRF_yuqiang, true, testDataList_yuqinag);
                    testDataList_yuqinag.Clear();


                    ArrayListCsv_loss.InsertRange(0, new string[] { Global.FIXTURENAME, SN, startTime.ToString("yyyy/MM/dd HH:mm:ss") });
                    string[] testData_loss = ArrayListCsv_loss.ToArray();
                    //#endif
                    List<string[]> testDataList_loss = new List<string[]>();
                    testDataList_loss.Add(testData_loss);
                    string reportPathSRF_loss = $@"{Global.LogPath}\{DateTime.Now.ToString("yyyy-MM-dd")}_{Global.STATIONNO}_pathloss.csv";
                    CreatCSVFile(reportPathSRF_loss, csvColumnPath_loss);
                    WriteCSV(reportPathSRF_loss, true, testDataList_loss);
                    testDataList_loss.Clear();

                }

                //testData = new string[] { startTime.ToString(),SN,stationStatus.ToString(),endTime.ToString(),error_code_firstfail,CSN,"", "canceled","","","","",
                //"canceled","","","canceled",mesPhases.LEDIrradianceTest,mesPhases.Ledoff,mesPhases.W_x,mesPhases.W_y,mesPhases.W_L,
                //mesPhases.B_x,mesPhases.B_y,mesPhases.B_L,mesPhases.G_x,mesPhases.G_y,mesPhases.G_L,mesPhases.R_x,mesPhases.R_y,mesPhases.R_L};

                CreatCSVFile(CSVFilePath, csvColumnPath);
                testDataList.Add(testData);
                WriteCSV(CSVFilePath, true, testDataList);

                string CSVFilePath2 = $@"{Global.LogPath}\{DateTime.Now.ToString("yyyy-MM-dd")}_{Global.STATIONNO}.csv";
                CreatCSVFile(CSVFilePath2, csvColumnPath);
                WriteCSV(CSVFilePath2, true, testDataList);

                testDataList.Clear();
                Logger.Debug($"Export test results to {CSVFilePath} succeed");
            }
            catch (Exception ex)
            {
                Logger.Fatal($"Export test results to CSVFilePath error!:{ex.Message} ");
                //if (ReadCSV(CSVFilePath).Count >= 65535)
                //{
                //    string renamePath = CSVFilePath.Insert(CSVFilePath.LastIndexOf("."), DateTime.Now.ToString("yyyy-MM-dd-HHmm"));
                //    // 重命名
                //    FileInfo fi = new FileInfo(CSVFilePath);     //result.csv
                //    fi.MoveTo(renamePath);                      //result2020121115.csv
                //}
            }
        }

        #endregion 

        // 控件更新处理函数
        #region ################################ 11. Thread Delegate function  ############################################# 

        //////////////////////////////////////////////////////////////////////////////////////
        // 11-1. tree View Seq Delegate                                                     //
        //       更新测试项颜色                                                              //
        //////////////////////////////////////////////////////////////////////////////////////
        private delegate void treeViewSeqDelegate(Color colors);                        //! 定义更新log委托
        public void SetTreeViewSeqColor(Color colors)
        {
            if (this.treeViewSeq.InvokeRequired)
            {
                treeViewSeqDelegate d = new treeViewSeqDelegate(SetTreeViewSeqColor);   //! 实体化委托
                this.Invoke(d, new object[] { colors });
            }
            else
            {
                if (this.treeViewSeq.IsDisposed) return;
                treeViewSeq.Nodes[seqNo].Expand();                                      //! 更新测试项颜色
                treeViewSeq.Nodes[seqNo].Nodes[itemsNo].BackColor = colors;
                treeViewSeq.Nodes[seqNo].Nodes[itemsNo].EnsureVisible();
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////
        // 11-2. Show tree View Seq Delegate                                                //
        //       显示测试项                                                                  //
        //////////////////////////////////////////////////////////////////////////////////////
        private delegate void ShowTreeViewDelegate(bool checkAll = true);               //! 定义更新log委托
        public void ShowTreeView(bool checkAll = true)
        {
            if (checkAll == true)
            {
                bt_debug.Checked = true;
                for (int i = 0; i < treeViewSeq.Nodes.Count; i++)
                {
                    treeViewSeq.Nodes[i].Checked = true;
                    for (int j = 0; j < treeViewSeq.Nodes[i].Nodes.Count; j++)
                    {
                        treeViewSeq.Nodes[i].Nodes[j].Checked = true;
                    }
                }

                _ = treeViewSeq.Nodes.Count;
            }
            if (checkAll == false)
            {
                bt_debug.Checked = false;
                bt_debug.Checked = true;
            }
        }


        //////////////////////////////////////////////////////////////////////////////////////
        // 11-3. Update Detail View Clear                                                   //
        //       更新data Grid View                                                         //
        //////////////////////////////////////////////////////////////////////////////////////
        private delegate void UpdateDataViewDeClear();                                  //! 定义更新log委托
        public void UpdateDetailViewClear()
        {
            if (this.dataGridViewDetail.InvokeRequired)
            {
                UpdateDataViewDeClear d = new UpdateDataViewDeClear(UpdateDetailViewClear); //实体化委托
                this.Invoke(d, new object[] { });
            }
            else
            {
                if (!this.dataGridViewDetail.IsDisposed)
                {
                    //this.dataGridViewDetail.Rows.Clear();                             //! 删除 dataGridViewDetail 控件中的每一行
                    while (dataGridViewDetail.Rows.Count > 0)
                    {
                        dataGridViewDetail.Rows.RemoveAt(0);
                    }
                }
            }
        }


        //////////////////////////////////////////////////////////////////////////////////////
        // 11-4. Update Detail View                                                         //
        //       定义更新dataGridView委托 更新测试结果                                        //
        //////////////////////////////////////////////////////////////////////////////////////
        private delegate void UpdateDataViewDe(string UUT_SN, string ItemName, string Spec, string Limit_min, string CurrentValue, string Limit_max, string ElapsedTime, string StartTime, string TestResult);
        public void UpdateDetailView(string UUT_SN, string ItemName, string Spec, string Limit_min, string CurrentValue, string Limit_max, string ElapsedTime, string StartTime, string TestResult)
        {
            Spec = Spec ?? "--";
            Spec = Spec == "" ? "--" : Spec;
            //Limit_min = Limit_min ?? "--";
            //Limit_min = Limit_min == "" ? "--" : Limit_min;
            CurrentValue = CurrentValue ?? "--";
            //Limit_max = Limit_max ?? "--";
            //Limit_max = Limit_max == "" ? "--" : Limit_max;

            if (this.dataGridViewDetail.InvokeRequired)
            {
                UpdateDataViewDe d = new UpdateDataViewDe(UpdateDetailView);            //! 实体化委托
                this.Invoke(d, new object[] { UUT_SN, ItemName, Spec,  CurrentValue, ElapsedTime, StartTime, TestResult });
            }
            else
            {
                if (!this.dataGridViewDetail.IsDisposed)
                {
                    string[] paramArray = new string[] { UUT_SN, ItemName, Spec, CurrentValue, ElapsedTime, StartTime, TestResult };
                    int i = dataGridViewDetail.Rows.Add();
                    for (int j = 0; j < colHeader.Length; j++)
                    {
                        dataGridViewDetail.Rows[i].Cells[colHeader[j]].Value = paramArray[j];
                        dataGridViewDetail.Rows[i].DefaultCellStyle.ForeColor = Color.Black;
                    }
                    if (TestResult.ToLower() == "false" || TestResult.ToLower() == "fail")
                    {
                        //dataGridViewDetail.Rows[i].DefaultCellStyle.BackColor = (TestResult == "False") ? Red : Color.Blue;
                        dataGridViewDetail.Rows[i].DefaultCellStyle.ForeColor = Color.Red;
                    }
                }
            }
        }
        //////////////////////////////////////////////////////////////////////////////////////
        // 11-5. Save Rich Text (右下角)                                                     //
        //       更新大視窗                                                                  //
        //////////////////////////////////////////////////////////////////////////////////////
        private delegate void SaveRichTextDelegate(bool isClear, RichTextBoxStreamType a);//定义更新log委托
        public void SaveRichText(bool isClear = false, RichTextBoxStreamType a = 0)
        {
            try
            {
                if (this.richTextBox1.InvokeRequired)
                {
                    SaveRichTextDelegate d = new SaveRichTextDelegate(SaveRichText);    //实体化委托
                    this.Invoke(d, new object[] { isClear, a });
                }
                else
                {
                    if (!this.richTextBox1.IsDisposed)
                    {
                        if (isClear)
                        {
                            richTextBox1.Clear();
                            return;
                        }
                        cellLogPath = $@"{Global.LogPath}\{finalTestResult}_{SN}_{error_details_firstfail}_{DateTime.Now.ToString("hh-mm-ss")}.txt";
                        //cellLogPath = cellLogPath.ReplaceStr(new char[] { '\\', '/', ':', '*', '?', '<', '>', '|', '"' });
                        Logger.Debug($"Save test log OK.{cellLogPath}");
                        richTextBox1.SaveFile(cellLogPath, a);                  //!保存测试log
                        //CopyLogToServer("Z", cellLogPath);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.ToString());
                // throw;
            }
        }

        private delegate void MainInfoRichTextDelegatePro(RichTextBox textbox, string info, bool isClear, RichTextBoxStreamType a = RichTextBoxStreamType.PlainText);//定义更新log委托
        public void MainInfoRichText(RichTextBox textbox, string info = "", bool isClear = false, RichTextBoxStreamType a = RichTextBoxStreamType.PlainText)
        {
            try
            {
                if (textbox.InvokeRequired)
                {
                    MainInfoRichTextDelegatePro d = new MainInfoRichTextDelegatePro(MainInfoRichText);    //实体化委托
                    this.Invoke(d, new object[] { textbox, info, a });
                }
                else
                {
                    if (!textbox.IsDisposed)
                    {
                        if (isClear)
                        {
                            textbox.Clear();
                            return;
                        }

                        textbox.AppendText(info+"\n");


                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.ToString());
                // throw;
            }
        }
        //////////////////////////////////////////////////////////////////////////////////////
        // 11-5. Save Rich Text (右下角)                                                     //
        //       更新大視窗                                                                  //
        //////////////////////////////////////////////////////////////////////////////////////
        private delegate void SaveRichTextDelegatePro(RichTextBox textbox, bool isClear, string path, RichTextBoxStreamType a);//定义更新log委托
        public void SaveRichTextPro(RichTextBox textbox,bool isClear = false, string path = "", RichTextBoxStreamType a = RichTextBoxStreamType.PlainText)
        {
            try
            {
                if (textbox.InvokeRequired)
                {
                    SaveRichTextDelegatePro d = new SaveRichTextDelegatePro(SaveRichTextPro);    //实体化委托
                    this.Invoke(d, new object[] { textbox, isClear, path, a });
                }
                else
                {
                    if (!textbox.IsDisposed)
                    {
                        if (isClear)
                        {
                            textbox.Clear();
                            return;
                        }
                        
                        //cellLogPath = cellLogPath.ReplaceStr(new char[] { '\\', '/', ':', '*', '?', '<', '>', '|', '"' });
                        
                        if(GlobalNew.ProtreeON != "1")
                        {
                  //!保存测试log
                        }
                        if (GlobalNew.ProtreeON == "1")
                        {
                            string directoryPath = Path.GetDirectoryName($"{GlobalNew.LogPath}\\{path}");

                            // 檢查目錄是否存在，如果不存在則創建
                            if (!Directory.Exists(directoryPath))
                            {
                                Directory.CreateDirectory(directoryPath);
                            }
                            textbox.SaveFile($"{GlobalNew.LogPath}\\{path}", a);
                            if (!string.IsNullOrEmpty(GlobalNew.ServerLOGFOLDER))
                            {
                                string org = $"{GlobalNew.LogPath}\\{path}";
                                string des = org.Replace(GlobalNew.LOGFOLDER, GlobalNew.ServerLOGFOLDER);
                                CopyFile(org, des);
                            }
                            Logger.Debug($"Save test log OK.{GlobalNew.LogPath}\\{path}");
                        }
                            
                        else
                        {
                            Logger.Debug($"Save test log OK.{GlobalNew.txtLogPath}\\{path}");
                            textbox.SaveFile($"{GlobalNew.txtLogPath}\\{path}", a);
                        }
                        CopyLogToServer("Z", path);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.ToString());
                // throw;
            }
        }


        // 使用本地映射盘的方式操作共享文件夹
        //     mapDrive = y映射的本地盘符名Z:
        //     logfile  = 要上传的文件路径
        private void CopyLogToServer(string mapDrive, string logfile)
        {
            try
            {

                //string comline = $@"net use {mapDrive}: /del /y&net use {mapDrive}: {GlobalNew.LOGSERVER} {GlobalNew.LOGSERVERPwd} /USER:{GlobalNew.LOGSERVERUser}";
                string FileName = Path.GetFileName($@"{GlobalNew.txtLogPath}\\{logfile}");
                string destPath = $@"{GlobalNew.LOGSERVER}\{GlobalNew.CurrentProject}\{GlobalNew.CurrentStation}\{GlobalNew.CurrentFixture}\{DateTime.Now.ToString("yyyyMMdd")}\{logfile}";
                string directoryPath = Path.GetDirectoryName(destPath);
                //RunDosCmd(comline, out string output);

                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                    //CreateDirectoryRecursively(directoryPath);
                }
                if(GlobalNew.ProtreeON == "1")
                {
                    //Logger.Debug($"Log Copy {GlobalNew.LogPath}\\{logfile} to {destPath}");
                    File.Copy($@"{GlobalNew.LogPath}\{logfile}", destPath, true);
                }
                    
                else
                    File.Copy($@"{GlobalNew.txtLogPath}\{logfile}", destPath, true);
                Logger.Debug("Upload test log to logServer success.");
                //  }
            }
            catch (Exception ex)
            {
                Logger.Fatal("Upload test log to logServer Exception:" + ex.Message);
            }
            finally
            {
                //RunDosCmd($"net use {mapDrive}: /del /y", out string output);
            }
        }
        static bool IsDriveMapped(string driveLetter)
        {
            DriveInfo[] drives = DriveInfo.GetDrives();
            foreach (DriveInfo drive in drives)
            {
                if (drive.IsReady && drive.DriveType == DriveType.Network && drive.Name.StartsWith(driveLetter + ":"))
                {
                    return true;
                }
            }
            return false;
        }
        static void CreateDirectoryRecursively(string path)
        {
            // 分割路径成为各个目录
            string[] folders = path.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            // 逐级创建目录
            for (int i =3; i <= folders.Length; i++)
            {
                string subPath = string.Join(Path.DirectorySeparatorChar.ToString(), folders, 0, i);
                if (!Directory.Exists(subPath))
                {
                    Directory.CreateDirectory(subPath);
                    Console.WriteLine($"已创建目录：{subPath}");
                }
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////
        // 11-6. Set UI item                                                                //
        //       啟動按鈕                                                                    //
        //////////////////////////////////////////////////////////////////////////////////////
        private delegate void SetButtonDelegate(Button bts, string text, Color color, bool isEnable);
        public void SetButton(Button button, string strInfo, Color color, bool isEnable = true)
        {
            if (button.InvokeRequired)
            {
                SetButtonDelegate d = new SetButtonDelegate(SetButton); //实体化委托
                this.Invoke(d, new object[] { button, strInfo, color, isEnable });
            }
            else
            {
                if (!button.IsDisposed)
                {
                    button.BackColor = color;
                    button.Text = strInfo;
                    button.Enabled = isEnable;
                }
            }
        }

        private delegate void SetButtonDelegate1(Button bts, string text);
        public void SetButton(Button button, string strInfo)
        {
            if (button.InvokeRequired)
            {
                SetButtonDelegate1 d = new SetButtonDelegate1(SetButton); //实体化委托
                this.Invoke(d, new object[] { button, strInfo });
            }
            else
            {
                if (!button.IsDisposed)
                {
                    button.Text = strInfo;
                }
            }
        }

        private delegate void SetButtonProDelegate(Button bts, Image text);
        public void SetButtonPro(Button button, Image strInfo)
        {
            if (button.InvokeRequired)
            {
                SetButtonProDelegate d = new SetButtonProDelegate(SetButtonPro); //实体化委托
                this.Invoke(d, new object[] { button, strInfo });
            }
            else
            {
                if (!button.IsDisposed)
                {
                    button.Image = strInfo;
                }
            }
        }

        private delegate void SetLableDelegate(Label bts, string text, Color color, bool visible);
        public void SetLables(Label label, string strInfo, Color color, bool visible = true)
        {
            if (label.InvokeRequired)
            {
                SetLableDelegate d = new SetLableDelegate(SetLables); //实体化委托
                this.Invoke(d, new object[] { label, strInfo, color, visible });
            }
            else
            {
                if (!label.IsDisposed)
                {
                    label.BackColor = color;
                    label.Text = strInfo;
                    label.Visible = visible;
                }
            }
        }

        private delegate void LableDelegate(Label bts, string text, Color color, Color fcolor, bool visible);
        public void SetLable(Label label, string strInfo, Color color, Color fcolor, bool visible = true)
        {
            if (label.InvokeRequired)
            {
                LableDelegate d = new LableDelegate(SetLable); //实体化委托
                this.Invoke(d, new object[] { label, strInfo, color, fcolor, visible });
            }
            else
            {
                if (!label.IsDisposed)

                    label.ForeColor = fcolor;
                label.BackColor = color;
                label.Text = strInfo;
                label.Visible = visible;
            }
        }
    

    private delegate void SetTextBoxDelegate(TextBox textBox, bool isEnable, string text);
        public void SetTextBox(TextBox textBox, bool isEnable = true, string text = "")
        {
            if (textBox.InvokeRequired)
            {
                SetTextBoxDelegate d = new SetTextBoxDelegate(SetTextBox); //实体化委托
                this.Invoke(d, new object[] { textBox, isEnable, text });
            }
            else
            {
                if (!textBox.IsDisposed)
                {
                    textBox.Enabled = isEnable;
                    textBox.Text = text;
                    this.ActiveControl = textBox; //设置当前窗口的活动控件为textBox1
                    textBox.Focus();
                }
            }
        }

        public bool CheckSNFormat(string product_sn,int positionToCheck, string pattern,bool Letter_case)
        {
            string Final_pattern = string.Empty;
            string Final_morepattern = string.Empty;
            int position = 0;
            //Dictionary<int, string> Choseposition = new Dictionary<int, string>();
            //Dictionary<int, string> morepattern = new Dictionary<int, string>();
            //pattern = $"^{Regex.Escape(pattern)}";
                try
                {
                    position = positionToCheck;
                    if (position > (int)product_sn.Count())
                    {
                        MessageBox.Show("The String position can not longer than ProductSN length!!!");
                        return false;
                    }
                    string str_pattern = @"\w+";//[a-zA-Z0-9_] 包含數字0~9、字母大小寫a~z、底線
                    bool str_matches = Regex.IsMatch(product_sn, str_pattern);
                    if(!str_matches)
                    {
                       MessageBox.Show($"Please Check SN Format, must included \" a-zA-Z0-9_ \" !!!:","Format Check", MessageBoxButtons.OK, MessageBoxIcon.Error);
                       return false;
                     }
                   
                   if (Letter_case)
                     Final_pattern = $".{{0,{positionToCheck}}}{Regex.Escape(pattern)}";
                   else
                      Final_pattern = $"(?i)^.{{0,{positionToCheck}}}{Regex.Escape(pattern)}";

                    MatchCollection matches = Regex.Matches(product_sn, Final_pattern);

                    if (matches.Count > 0)
                    {
                        return true;
                    }
                    else
                    {
                    MessageBox.Show($"SN:{product_sn} Format is wrong,Start with position \"{position} \" have to included \"{pattern}\".Please Scan again!", "SN Format Fail", 0, MessageBoxIcon.Error);
                    return false;
                    }
                }catch(Exception e1)
                {
                    MessageBox.Show($"Format Error, Please Check SN Format!!!:" +e1.Message, "Format Check",MessageBoxButtons.OK,MessageBoxIcon.Error);
                    return false;
                }
            //}
        }

        #endregion 控件更新处理函数

        // Treeview (UI) & Json
        #region ################################ 12. Json Script  ############################################# 

        //////////////////////////////////////////////////////////////////////////////////////
        // 12-1. Read Json_Script to TreeView                                               //
        //       读取Json_Script，初始化TreeView 树状框                                       //
        //////////////////////////////////////////////////////////////////////////////////////
        private void JsonToTreeViewSeq()
        {
            try {
                string jsonPath = string.Empty;
                if (GlobalNew.RECIPENAME == "Golden")
                    jsonPath = $@"{System.Environment.CurrentDirectory}\Config\Golden.json";
                else if (GlobalNew.RECIPENAME == "Debug")
                    jsonPath = $@"{System.Environment.CurrentDirectory}\Config\Debug.json";
                else
                    jsonPath = $@"{System.Environment.CurrentDirectory}\Config\{GlobalNew.STATIONNAME}.json";
                JArray Sequences = JArray.Parse(File.ReadAllText(jsonPath));
                for (int i = 0; i < Sequences.Count; i++)
                {

                    TreeNode tempSeq = new TreeNode();
                    tempSeq.Text = Sequences[i]["SeqName"].ToString();
                    tempSeq.ImageIndex = 1;
                    tempSeq.Checked = true;
                    treeViewSeq.Nodes.Add(tempSeq);

                    int.TryParse(Sequences[i]["TotalNumber"].ToString(), out int TotalNumber);
                    JArray SeqItems = JArray.Parse(Sequences[i]["SeqItems"].ToString());

                    for (int j = 0; j < TotalNumber; j++)
                    {
                        TreeNode tempItems = new TreeNode();
                        tempItems.Text = SeqItems[j]["ItemName"].ToString();
                        tempItems.ImageIndex = 0;
                        tempItems.Checked = true;
                        tempSeq.Nodes.Add(tempItems);
                    }
                }

                treeViewSeq.ExpandAll();
                treeViewSeq.SelectedNode = treeViewSeq.Nodes[0];
            }
            catch(Exception e)
            {
                MessageBox.Show($"{e.Message}");
                return;
            }

        }


        //////////////////////////////////////////////////////////////////////////////////////
        // 12-2. Save TreeView to Json_Script                                               //
        //       将TreeView 列表序列化为JSON字符串                                            //
        //////////////////////////////////////////////////////////////////////////////////////
        private void SaveTreeViewToJson(TreeView treeView, string filePath)
        {
            // 1. 创建一个用于存储TreeView数据的对象列表
            List<TreeNodeData> treeNodeDataList = new List<TreeNodeData>();

            // 2. 遍历TreeView的根节点
            foreach (TreeNode rootNode in treeView.Nodes)
            {
                TreeNodeData rootNodeData = GetTreeNodeData(rootNode);
                treeNodeDataList.Add(rootNodeData);
            }

            // 3. 将对象列表序列化为JSON字符串
            string json = JsonConvert.SerializeObject(treeNodeDataList, Formatting.Indented);

            // 4. 将JSON字符串写入文件
            File.WriteAllText(filePath, json);
        }

        //////////////////////////////////////////////////////////////////////////////////////
        // 12-3. Get TreeView to Json_Script                                                //
        //       遍历TreeView的根节点                                                        //
        //////////////////////////////////////////////////////////////////////////////////////
        private TreeNodeData GetTreeNodeData(TreeNode node)
        {
            // 
            TreeNodeData nodeData = new TreeNodeData
            {
                Text = node.Text,
                ImageIndex = node.ImageIndex,
                Checked = node.Checked,
                ChildNodes = new List<TreeNodeData>()
            };

            foreach (TreeNode childNode in node.Nodes)
            {
                TreeNodeData childNodeData = GetTreeNodeData(childNode);
                nodeData.ChildNodes.Add(childNodeData);
            }

            return nodeData;
        }

        #endregion 窗体响应处理函数

        // 窗体响应处理函数
        #region ################################ 13. UI function  ################################################# 

        //////////////////////////////////////////////////////////////////////////////////////
        // 13-1. Close UI Form
        //       關閉視窗時
        //////////////////////////////////////////////////////////////////////////////////////
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            /////MTE Peter CCD Uninit///
            //if(CCD_Device != null)
            //    CCD_Device.UnInit();
            /////MTE Peter CCD Uninit///
            /////MTE Peter Control Device Uninit///
            //if(Control_Device != null)
            //    Control_Device.UnInit();
            /////MTE Peter Control Device Uninit///
            ///

            //NEW Uninitial//
            foreach (var value in GlobalNew.Devices.Values)
            {
                bool result;
                switch (value)
                {
                    case DUT_BASE D:
                        result = D.UnInit();
                        if (result)
                            Logger.Debug($"{D.Description} Uninit Success");
                        else
                            Logger.Debug($"{D.Description} Uninit Fail");
                        break;
                    case IOBase I:
                        result = I.UnInit();
                        if (result)
                            Logger.Debug($"{I.Description} Uninit Success");
                        else
                            Logger.Debug($"{I.Description} Uninit Fail");
                        break;
                    case ControlDeviceBase D:
                        result = D.UnInit();
                        if (result)
                            Logger.Debug($"{D.Description} Uninit Success");
                        else
                            Logger.Debug($"{D.Description} Uninit Fail");
                        break;
              
                    case CCDBase C:
                        result = C.UnInit();

                        if (result)
                            Logger.Debug($"{C.Description} Uninit Success");
                        else
                            Logger.Debug($"{C.Description} Uninit Fail");
                        break;
                    case MotionBase M:
                        result = M.UnInit();
                        //Motion_Device = M;
                        if (result)
                            Logger.Debug($"{M.Description} UnInit Success");
                        else
                        {
                            Logger.Debug($"{M.Description} UnInit Fail");                          
                        }
                        break;
                    case VisaBase V:
                        result = V.UnInit();
                        //Motion_Device = M;
                        if (result)
                            Logger.Debug($"{V.Description} UnInit Success");
                        else
                        {
                            Logger.Debug($"{V.Description} UnInit Fail");
                        }
                        break;
                }
            }
            //NEW Uninitial//

            KillProcess("Scan_Barcode_C03");

        }

        //////////////////////////////////////////////////////////////////////////////////////
        // 13-2. UI Update timer
        //       这样处理界面显示时不卡
        //////////////////////////////////////////////////////////////////////////////////////
        private void Timer1_Tick(object sender, EventArgs e)
        {
            timer1.Stop();
            this.Visible = true;
            this.Opacity = 1;
        }

        //////////////////////////////////////////////////////////////////////////////////////
        // 13-3. Log button (右上角)
        //       打開log儲存的資料夾
        //////////////////////////////////////////////////////////////////////////////////////
        private void LogButton_MouseEnter(object sender, EventArgs e)
        {
            toolTip1.SetToolTip((Button)sender, "open test log");
        }

        private void LogButton_Click(object sender, EventArgs e)
        {
            string path = GlobalNew.LogPath;
            System.Diagnostics.Process.Start(path);
        }

        //////////////////////////////////////////////////////////////////////////////////////
        // 13-4. Time stamp
        //       時間搓
        //////////////////////////////////////////////////////////////////////////////////////
        private void TimerCallBack(object stateInfo)
        {
            if(GlobalNew.ShowTestTime == "1")
            { 
                try
                {
                    if (startFlag)
                    {
                        sec++;

                        SetButton(bt_errorCode, sec.ToString());
                        toolStripTestTime.InvokeOnToolStripItem(toolStripTestTime => toolStripTestTime.Text = sec.ToString() + "s");
                        if (isCycle)
                        {
                        }

                    }
                }
                catch (Exception ex)
                {
                    Logger.Fatal(ex.ToString());
                    //throw;
                }
            }
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            //try
            //{
            //    if (startFlag)
            //    {
            //        sec++;
            //        SetButton(bt_errorCode, sec.ToString());
            //        toolStripTestTime.InvokeOnToolStripItem(toolStripTestTime => toolStripTestTime.Text = sec.ToString() + "s");
            //        if (isCycle)
            //        {
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Logger.Fatal(ex.ToString());
            //    //throw;
            //}
        }

        //////////////////////////////////////////////////////////////////////////////////////
        // 13-5. Tree View Seq (左上角)
        //       樹狀結構 (Srcipt)
        //////////////////////////////////////////////////////////////////////////////////////
        private void TreeViewSeq_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                int len = richTextBox1.Text.IndexOf(e.Node.Text.Substring(2).Trim());
                if (len > 0)
                {
                    //! 光标跳到指定行
                    richTextBox1.Select(len, 0);   
                    richTextBox1.ScrollToCaret();
                }
            }

            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                treeViewSeq.SelectedNode = e.Node;
                if (e.Node.Nodes.Count <= 0)
                {
                    //! 显示右键菜单
                    contextMenuStripRightKey.Show(MousePosition.X, MousePosition.Y);
                }
            }
        }

        private void Not_Select_ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowTreeView(false);
        }

        private void Select_ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowTreeView(true);
        }

        //////////////////////////////////////////////////////////////////////////////////////
        // 13-6. bt_debug (右上角)
        //       Debug mode，可以勾選樹狀結構 (Srcipt)
        //////////////////////////////////////////////////////////////////////////////////////
        private void CheckBox2_CheckedChanged(object sender, EventArgs e)
        {

            if (bt_debug.CheckState == CheckState.Checked)
            {

                this.treeViewSeq.CheckBoxes = true;
                this.buttonBegin.Enabled    = true;
                this.buttonBegin.Visible    = true;
                this.buttonExit.Enabled     = true;
                this.buttonExit.Visible     = true;
            }
            else
            {
                this.treeViewSeq.CheckBoxes = false;
                this.buttonBegin.Enabled    = false;
                this.buttonBegin.Visible    = false;
                this.buttonExit.Enabled     = false;
                this.buttonExit.Visible     = false;
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////
        // 13-7. bt_debug other (右上角)
        //       Debug 打開之後的內部按鈕
        //////////////////////////////////////////////////////////////////////////////////////
        private void ButtonBegin_Click(object sender, EventArgs e)
        {
            if (startFlag)
            {
                //!当startflag信号为真是进入,说明正在测试
                if (!pauseFlag)
                {
                    //!按下暂停键,pause信号为真
                    pauseFlag = true;
                    SetButtonPro(buttonBegin, Properties.Resources.start);
                    pauseEvent.Reset();
                }
                else
                {
                    //!暂停状态下,按下开始键,!发送信号,pause结束
                    pauseEvent.Set();
                    SetButtonPro(buttonBegin, Properties.Resources.pause);
                    //!pause信号值假
                    pauseFlag = false;
                }
            }
            else
            {
                //!startflag信号为假的时候进入,说明未开始测试
                TextBox2_KeyDown(null, null);
            }
        }

        private void ButtonExit_Click(object sender, EventArgs e)
        {
            if (startFlag)
            {
                //!当startflag为真时,结束测试,显示测试结果
                pauseEvent.Reset();
                saveTestResult();
                // sequences[seqNo].IsTestFinished = true;
                startFlag = false;
                isCycle = false;
                testThread.Abort();
                testThread.Join(3000);
                timer.Dispose(); 
                testThread = new Thread(TestThread);
                testThread.Start();
                buttonBegin.Enabled = true;
                SetTextBox(textBox2);
            }
            //else  //!当startflag信号为假时,退出测试
            //this.Close();
        }


        //////////////////////////////////////////////////////////////////////////////////////
        // 13-8. Reset button (右上角)
        //       Reset UI form
        //////////////////////////////////////////////////////////////////////////////////////
        private void ContinuousFailReset_Click(object sender, EventArgs e)
        {
            var resetForm = new Reset();
            resetForm.Show();
        }


        //////////////////////////////////////////////////////////////////////////////////////
        // 13-9. Save Tree View As Json Button (Not use)
        //       在需要保存TreeView为JSON文件的地方调用此方法
        //////////////////////////////////////////////////////////////////////////////////////
        private void SaveTreeViewAsJsonButton_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "JSON Files (*.json)|*.json";
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = saveFileDialog.FileName;
                SaveTreeViewToJson(treeViewSeq, filePath);
                MessageBox.Show("TreeView已保存为JSON文件。");
            }
        }
        //////////////////////////////////////////////////////////////////////////////////////
        // 13-10. Mac keyIn
        //        確認mac是否正確
        //////////////////////////////////////////////////////////////////////////////////////
        private void textBox3_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Back && textBox3.Text.Length == 0)
            {
                SetTextBox(textBox1);
            }
            if (e != null && e.KeyCode != Keys.Enter) return;
            MAC = textBox3.Text;

            //VD5001专用，MAC 前缀“E0:A7”
            if (MAC.Length == Convert.ToInt32(GlobalNew.MAC_Length) && MAC.Substring(0, 5) == "E0:A7")
            {
                SetTextBox(textBox2);
            }


        }



        //////////////////////////////////////////////////////////////////////////////////////
        // 13-12. Message box show
        //////////////////////////////////////////////////////////////////////////////////////
        private void button1_Click(object sender, EventArgs e)
        {
            //// 创建测试后台主线程
            //testThread = new Thread(new ThreadStart(TestThread))
            //{
            //    IsBackground = true
            //};      //!开始测试主线程
            //testThread.Start();


            //Motion_Device.SyncHome(10, 500);

        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void dataGridViewDetail_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (this.dataGridViewDetail.Columns[e.ColumnIndex].Name == "tResult")
            {
                string cellValue = e.Value.ToString();
                if (cellValue == "PASS")
                {
                    e.CellStyle.BackColor = Color.LightGreen;
                    e.CellStyle.ForeColor = Color.Black;
                }else if(cellValue == "FAIL")
                {
                    e.CellStyle.BackColor = Color.LightCoral;
                    e.CellStyle.ForeColor = Color.Black;
                }
            }
        }


        //////////////////////////////////////////////////////////////////////////////////////
        // 13-13. Configure Button(右上角白色)   
        //        Edit script 編輯腳本 (add by william)
        //////////////////////////////////////////////////////////////////////////////////////        
        private void ConfigureBtn_Click(object sender, EventArgs e)
        {
            //LoginForm LoginF = null;
            //if (GlobalNew.Weblogin != "1")
            //{
            //    LoginF = new LoginForm(false);
            //}
            //else
            //{
            //    LoginF = new LoginForm();
            //}
            //DialogResult res = LoginF.ShowDialog();
            //if (res == DialogResult.OK)
            {
                if (GlobalNew.UserLevel > 0)
                {
                    if (GlobalNew.ProtreeON == "1")
                    {
                        foreach (TextBox textBox in SNtextBoxes)
                        {
                            this.Invoke(new Action(() =>
                            {
                                textBox.Enabled = false;
                                textBox.Text = "";

                            }));
                        }

                        RecipeMenu menu = new RecipeMenu();
                        DialogResult result = menu.ShowDialog();
                        //if (result == DialogResult.Yes)
                        {
                            string ret = MainProTreeView.Read_Recipe(GlobalNew.CurrentRecipePath);
                            
                            if (ret.Contains("success"))
                            {
                                //InitialGridView();
                                isLoadRecipeSuccess = true;
                                Logger.Info($"Load Recipe Success. {GlobalNew.CurrentRecipePath}");
                            }
                            else
                            {
                                isLoadRecipeSuccess = false;
                                Logger.Error($"Load Recipe Fail. ({ret})");
                            }

                        }

                        iniConfig.Writeini("Recipe", "CurrentRecipePath", GlobalNew.CurrentRecipePath);
                        iniConfig.Writeini("Recipe", "CurrentProject", GlobalNew.CurrentProject);
                        iniConfig.Writeini("Recipe", "CurrentMode", GlobalNew.CurrentMode);
                        iniConfig.Writeini("Recipe", "CurrentStation", GlobalNew.CurrentStation);
                        iniConfig.Writeini("Recipe", "CurrentFixture", GlobalNew.CurrentFixture);

                        SetLable(Modelabel, $"{GlobalNew.CurrentMode}.", Color.AliceBlue, Color.Black);
                        SetLable(Projectlabel, $"{GlobalNew.CurrentProject}", Color.Black, Color.White);
                        SetLable(Stationlabel, $"{GlobalNew.CurrentStation}", Color.YellowGreen, Color.Black);
                        //InitialGridView();

                        NewDataFolder();
                    }
                    else
                    {
                        RecipeManagement CG_Form2 = new RecipeManagement();
                        DialogResult result = CG_Form2.ShowDialog(); // 通过 ShowDialog() 方法显示 RecipeManagement 窗体
                        if (result == DialogResult.Yes)
                        {
                            treeViewSeq.Nodes.Clear();
                            JsonToTreeViewSeq();
                        }
                    }
                    //RecipeManagement CG_Form2 = new RecipeManagement();
                    //DialogResult result = CG_Form2.ShowDialog(); // 通过 ShowDialog() 方法显示 RecipeManagement 窗体
                    //if (result == DialogResult.Yes)
                    //{
                    //    treeViewSeq.Nodes.Clear();
                    //    JsonToTreeViewSeq();

                    //    string jsonRecipePath = $@"{System.Environment.CurrentDirectory}\Config\TestRecipePro.json";
                    //    MainProTreeView.Read_Recipe(jsonRecipePath);

                    //    if(GlobalNew.ProtreeON == "1")
                    //        InitialGridView();
                    //}
                }
                else
                {
                    MessageBox.Show("權限不足");
                }
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////
        // 13-14. Login Button(右上角藍色)   
        //        Login User ID (add by william)
        //////////////////////////////////////////////////////////////////////////////////////
        private void BtnLogin_Click(object sender, EventArgs e)
        {
            // another UI
            LoginForm CG_Form = null;
            //LoginForm CG_Form = new LoginForm(false);
            if (GlobalNew.Weblogin != "1")
            {
                CG_Form = new LoginForm(false);
            }
            else
            {
                CG_Form = new LoginForm();
            }
            DialogResult res = CG_Form.ShowDialog();
            if (res == DialogResult.OK)
            {
                MessageBox.Show("Login successful");
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////
        // 13-15. Reset Home Button(右上角綠色)   
        //        Reset UI form (add by william)
        //////////////////////////////////////////////////////////////////////////////////////
        private void Btn_ResetHome_Click_1(object sender, EventArgs e)
        {
            
            try
            {
                if (GlobalNew.Devices != null)
                {
                   if (GlobalNew.ResetHome_status.ContainsKey(3) && GlobalNew.ResetHome_status.ContainsKey(4) && GlobalNew.ResetHome_status.ContainsKey(0) && GlobalNew.ResetHome_check.ContainsKey(0))
                    {
                        GlobalNew.ResetHome_status[3] = 0;
                        GlobalNew.ResetHome_status[4] = 0;
                        GlobalNew.ResetHome_status[0] = 0;
                        GlobalNew.ResetHome_check[0] = 0;
                    }
                    else
                    {
                        GlobalNew.ResetHome_status.Add(3, 0);
                        GlobalNew.ResetHome_status.Add(4, 0);
                        GlobalNew.ResetHome_status.Add(0, 0);
                        GlobalNew.ResetHome_check.Add(0, 0);
                    }

                    MotionBase M = (MotionBase)GlobalNew.Devices["AMAX_1220"];
                    //MotionBase M = Motion_Device;

                    GlobalNew.home_flag = M.Check_IO_StartStatus(GlobalNew.ResetHome_check);
                    if (GlobalNew.home_flag)
                    {
                        M.Relative_Move(100, 10, 300, 0.3, 0.3);
                        //M.SyncHome(10, 80, 0);
                    }

                    GlobalNew.home_flag &= M.Check_IO_StartStatus(GlobalNew.ResetHome_status);
                    if (GlobalNew.home_flag)
                    {
                        textBox2.Enabled = true;
                        MessageBox.Show("Reset Home Ready!!:" + GlobalNew.home_flag.ToString() + " Please scan the SN barcode and put it into Jigs", "Reset Home", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        M.SetCommandPos(0);
                    }
                    else
                    {
                        textBox2.Enabled = false;
                        MessageBox.Show("Reset Home Fail!!:" + GlobalNew.home_flag.ToString() + " Please \"pull up\" handle then \"cover\" lift and right Fix Block", "Reset Home", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Devices is empty,Please check Devices status!!!", "Devices status", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Please check Driver is install or not" + ex.Message);
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////
        // 13-16. Reset Motion Stop(右上角紅色)   
        //       Reset UI form (add by william)
        //////////////////////////////////////////////////////////////////////////////////////
        private void Btn_MotionStop_Click_1(object sender, EventArgs e)
        {
            try
            {
                if (GlobalNew.Devices != null)
                {
                    MotionBase M = (MotionBase)GlobalNew.Devices["AMAX_1220"];
                    //MotionBase M = Motion_Device;
                    GlobalNew.Emg_flag = M.EmgStop();
                }else
                {
                    MessageBox.Show("Devices is empty,Please check Devices status!!!", "Devices status", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Please check Driver is install or not" + ex.Message);
            }
        }

        #endregion 控件更新处理函数

        // 進階樹狀流程初始化及運行
        #region ################################ 13. 進階樹狀流程初始化及運行  ################################################# 
        public void ClearGridView()
        {
            int count = GlobalNew.Devices.Count;

            foreach (var value in GlobalNew.Devices.Values)
            {
                if (value is DUT_BASE)
                {
                    DUT_BASE temp = (DUT_BASE)value;
                    temp.DataGridView.Columns.Clear();
                    temp.DataGridView.Rows.Clear();
                    DataGridView dataGridView = temp.DataGridView;
                    dataGridView.Columns.Clear();


                    //break;
                }



            }
        }
        public void InitialGridView2()
        {
            int count = GlobalNew.Devices.Count;

            foreach (var value in GlobalNew.Devices.Values)
            {
                if (value is DUT_BASE)
                {
                    DUT_BASE temp = (DUT_BASE)value;


                    splitContainerMain.Panel2.Controls.Add(temp.DataGridView);
                    //this.splitContainerMain.Panel2.Controls.Add(this.dataGridViewDetail);

                    break;
                }



            }
        }
        public void InitialGridView()
        {
            int count = GlobalNew.Devices.Count;
            GlobalNew.DataGridViewsList.Clear();
            foreach (Control control in splitContainerMain.Panel2.Controls.OfType<DataGridView>().ToList())
            {
                splitContainerMain.Panel2.Controls.Remove(control);
                control.Dispose();
            }
            foreach (var value in GlobalNew.Devices.Values)
            {
                if (value is DUT_BASE)
                {
                    DUT_BASE temp = (DUT_BASE)value;

                    if (temp.DataGridView != null && temp.DataGridView.Parent != null)
                    {
                        temp.DataGridView.Parent.Controls.Remove(temp.DataGridView);
                    }
                    temp.DataGridView.Columns.Clear();
                    temp.DataGridView.Rows.Clear();
                   DataGridView dataGridView = temp.DataGridView;

                    // 註冊事件
                    //dataGridView.RowsAdded += new DataGridViewRowsAddedEventHandler(dataGridView_RowsAdded);
                    //dataGridView.CellFormatting += new DataGridViewCellFormattingEventHandler(dataGridView_CellFormatting);
                    //dataGridView.KeyDown += new KeyEventHandler(dataGridView_KeyDown);
                    // 設定外觀
                    dataGridView.Dock = DockStyle.Fill;

                    //dataGridView.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders;

                    // 設定 RowHeaders 的寬度
                    dataGridView.RowHeadersWidth = 20;
                    dataGridView.AllowUserToAddRows = false;

                    dataGridView.EnableHeadersVisualStyles = false;
                    dataGridView.ColumnHeadersDefaultCellStyle.Font = new Font("Calibri", 11, FontStyle.Bold);
                    dataGridView.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
                    dataGridView.ColumnHeadersDefaultCellStyle.BackColor = Color.LightBlue;
                    dataGridView.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    dataGridView.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
                    dataGridView.RowHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
                    dataGridView.DefaultCellStyle.Font = new Font("Helvetica", 9, FontStyle.Regular);

                    dataGridView.DefaultCellStyle.SelectionBackColor = dataGridView.DefaultCellStyle.BackColor;
                    dataGridView.DefaultCellStyle.SelectionForeColor = dataGridView.DefaultCellStyle.ForeColor;
                    //dataGridView.CellBorderStyle = DataGridViewCellBorderStyle.Single;
                    dataGridView.GridColor = Color.FromArgb(226, 226, 226);

                    dataGridView.ReadOnly = true;

                    dataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

                    // 設定列
                    dataGridView.Columns.Add("No", "No");
                    dataGridView.Columns.Add("ID", "ID");
                    dataGridView.Columns["ID"].Visible = false;
                    dataGridView.Columns.Add("Item", "Item");
                    dataGridView.Columns.Add("Spec", "Spec");
                    dataGridView.Columns.Add("Value", "Value");

                    dataGridView.Columns.Add("Result", "Result");
                    dataGridView.Columns["Result"].HeaderCell.Style.ForeColor = Color.Blue;



                    dataGridView.Columns.Add("TestTime", "TestTime(s)");
                    if (GlobalNew.ShowTestTime == "1")
                        dataGridView.Columns["TestTime"].Visible = true;
                    else
                        dataGridView.Columns["TestTime"].Visible = false;
                    dataGridView.Columns.Add("Eslapse", "Eslapse(s)");
                    dataGridView.Columns.Add("Retry", "Retry");
                    dataGridView.Columns["Retry"].Visible = false;
                    dataGridView.Columns["Eslapse"].Visible = false;
                    //dataGridView.Columns["No"].Width = 30;
                    //dataGridView.Columns["Item"].Width = 120;
                    //dataGridView.Columns["Spec"].Width = 150;
                    //dataGridView.Columns["Value"].Width = 150;
                    //dataGridView.Columns["Result"].Width = 100;
                    //dataGridView.Columns["TestTime"].Width = 100;


                    //dataGridView.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
                    dataGridView.Columns["No"].FillWeight = 5;
                    dataGridView.Columns["Item"].FillWeight = 18;
                    dataGridView.Columns["Spec"].FillWeight = 28;
                    dataGridView.Columns["Value"].FillWeight = 32;
                    dataGridView.Columns["Result"].FillWeight = 10;
                    dataGridView.Columns["TestTime"].FillWeight = 12;
                    dataGridView.Columns["Eslapse"].Width = 12;
                    dataGridView.Columns["Retry"].Width = 10;
                    // 設定文字對齊和欄高
                    dataGridView.Columns["Value"].DefaultCellStyle.WrapMode = DataGridViewTriState.True;
                    dataGridView.Columns["Spec"].DefaultCellStyle.WrapMode = DataGridViewTriState.True;
                    dataGridView.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;

                    dataGridView.RowTemplate.Height = 120;  // 設定每一行的高度為 120

                    // 將 DataGridView 加入至 GlobalNew.DataGridViewsList
                    if (GlobalNew.DataGridViewsList.ContainsKey(temp.Description))
                        GlobalNew.DataGridViewsList[temp.Description] = dataGridView;
                    else
                        GlobalNew.DataGridViewsList.Add(temp.Description, dataGridView);

                    Number = 0;
                    TraverseTreeViewNodes(MainProTreeView.GetTreeview().Nodes, dataGridView);
                    dataGridViewDetail.Visible = false;

                    splitContainerMain.Panel2.Controls.Add(dataGridView);
                    //this.splitContainerMain.Panel2.Controls.Add(this.dataGridViewDetail);

                    //break;
                }



            }
        }
        public static int Number = 0;
        private void TraverseTreeViewNodes(TreeNodeCollection nodes, DataGridView dv)
        {
            foreach (TreeNode node in nodes)
            {
                if (node.Tag is Container_JIG_INIT)
                {
                    continue;
                }
                    
                if (node.Tag != null && node.Checked && node.Tag is ScriptBase)
                {
                    ScriptBase tagObject = (ScriptBase)node.Tag;
                    //Logger.Debug($"{tagObject.Description}");
                    if (tagObject.ShowItem == true)
                    {
                        string ShowSpec = string.Empty;

                        try
                        {
                            if (tagObject.Spec != string.Empty)
                            {
                                SpecParamsContainer specParams2 = JsonConvert.DeserializeObject<SpecParamsContainer>(tagObject.Spec);
                                if(specParams2.specParams != null)
                                {
                                    foreach (var param in specParams2.specParams)
                                    {

                                        switch (param.SpecType)
                                        {
                                            case SpecType.Range:
                                                ShowSpec += $"{param.MinLimit} < {param.Name} < {param.MaxLimit}\n";
                                                break;

                                            case SpecType.Equal:
                                                ShowSpec += $"{param.Name} = {param.SpecValue}\n";
                                                break;
                                            case SpecType.GreaterThan:
                                                ShowSpec += $"{param.Name} > {param.SpecValue}\n";

                                                break;
                                            case SpecType.LessThan:
                                                ShowSpec += $"{param.Name} < {param.SpecValue}\n";
                                                break;
                                            default:
                                                ShowSpec += "";
                                                break;
                                        }
                                    }
                                }

                            }

                        }
                        catch (Newtonsoft.Json.JsonReaderException)
                        {
                            ShowSpec += "無法解析輸入數據為 JSON 格式";
                        }
                        catch (Exception ex)
                        {
                            ShowSpec += $"處理數據時出現錯誤: {ex.Message}";
                        }

                        ShowSpec = ShowSpec.TrimEnd('\n');

                        ScriptBase.DataItem newItem = new ScriptBase.DataItem();
                        newItem.No = Number++;
                        newItem.Item = tagObject.Description;
                        if (ShowSpec == string.Empty)
                            newItem.Spec = "N/A";
                        else
                            newItem.Spec = ShowSpec;
                        //newItem.DutList = new List<string> { "Dut1", "Dut2" };
                        newItem.TestResult = "PASS";

                        object[] rowValues = { newItem.No, tagObject.ID, newItem.Item, newItem.Spec/*, newItem.Value, "PASS", newItem.TestResult, newItem.TestTime */};
                        dv.Rows.Add(rowValues);
                        dv.Rows[dv.Rows.Count - 1].Cells[2].Style.BackColor = Color.Aquamarine;
                        dv.Rows[dv.Rows.Count - 1].Cells[3].Style.BackColor = Color.Aquamarine;
                        dv.Rows[dv.Rows.Count - 1].Cells["Result"].Style.Font = new Font("Helvetica", 9, FontStyle.Bold);
                    }
                }

                if (node.Nodes.Count > 0)
                {
                    TraverseTreeViewNodes(node.Nodes, dv);
                }
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

                // 如果節點有子節點，遞迴地呼叫 TraverseTreeView 方法
                if (node.Nodes.Count > 0)
                {
                    TraverseTreeViewClearDataItem(node.Nodes);
                }
            }
        }

        public void ClearDataGridTestData(DUT_BASE DUT)
        {

            foreach (string key in GlobalNew.DataGridViewsList.Keys)
            {
                foreach (DataGridViewRow row in GlobalNew.DataGridViewsList[key].Rows)
                {
                    row.Cells["Result"].Value = "";
                    row.Cells["Result"].Style.ForeColor = Color.Black;

                    if (key == DUT.Description)
                    {
                        row.Cells["Value"].Value = "";
                        row.Cells["TestTime"].Value = "";
                        row.Cells["Eslapse"].Value = "";
                        row.Cells["Retry"].Value = "";

                    }


                }

            }
        }
        public void ResetDutInfo()
        {
            this.Invoke(new Action(() =>
            {
                MainProTreeView.GetTreeview().BeginUpdate();
                MainProTreeView.ClearNodeColor();
                MainProTreeView.GetTreeview().EndUpdate();

                if (CurrentTempDUT == null)
                {
                    finalTestResult = "FAIL";
                    error_details_firstfail = "Not Found DUT";
                    Logger.Error("Not Found DUT");
                    SetButton(bt_errorCode, error_details_firstfail);
                    SetTestStatus(TestStatus.FAIL);
                    return;
                }

                ClearDataGridTestData(CurrentTempDUT);
                MemoryDataClear(CurrentTempDUT);
                StartInit(CurrentTempDUT);


                SetTestStatus(TestStatus.START);

            }));

        }

        public void RecordInfo(bool PassorFail)
        {
            //sec = 0;

            timer.Dispose();

            this.Invoke(new Action(() =>
            {
                bool result = PassorFail;

                TestSummary(CurrentTempDUT, result);

                SaveLogtoFile(CurrentTempDUT.DataCollection.GetMoreProp("ProductSN"));
            }));

        }
        public void RunProTree()
        {
            //MainProTreeView.GetTreeview().CollapseAll();
            foreach (TreeNode n in MainProTreeView.GetTreeview().Nodes)
            {
                if (n.Tag is Container_MainThread == true)
                {
                    Container_MainThread T = (Container_MainThread)n.Tag;
                    m_MainThread = T;
                    if (T.isRunning == 0)
                    {
                        foreach (var value in GlobalNew.Devices.Values)
                        {
                            if (value is DUT_BASE)
                            {
                                CurrentTempDUT = (DUT_BASE)value;

                                break;
                            }
                        }

                        if(CurrentTempDUT != null)
                        {
                            //if (CurrentTempDUT.Enable == false)
                            //{
                            //    MessageBox.Show($"Main Dut({CurrentTempDUT.Description}) must be Enable", "Warn!!!!");
                            //    return;
                            //}
                            CurrentTempDUT.TestInfo.ClearTestSteps();
                            CurrentTempDUT.DataCollection.Clear();
                            CurrentTempDUT.DataCollection.SetMoreProp("Failitem", "");
                        }
                        else
                        {
                            MessageBox.Show("Can't Found DUT Device");
                            return;
                        }

                        if (GlobalNew.CurrentMode != "PROD")
                        {
                            MessageBox.Show(this, "注意工程或點檢模式勿用於生產中!!\n Engineering mode should not be used for production.", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                        else
                        {
                            if (GlobalNew.CurrentUser == "rd")
                            {
                                MessageBox.Show(this, "勿使用RD帳號進行生產!!\n Do not use the RD account for production..", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return;
                            }
                            
                        }


                        bool TestResult = false;
                        object[] context = new object[] { MainProTreeView, T.FailContinue, CurrentTempDUT, TestResult, GlobalNew.Devices, JIG_InitailNode ,this};

                        n.EnsureVisible();

                        Task ta = T.Act(n, context);

                        ta.ContinueWith(t =>
                        {
                            ResetUISNtext();
                        }, TaskScheduler.FromCurrentSynchronizationContext()); // 這樣會確保 ContinueWith 在 UI 主線程上執行}

                    }
                }
            }
        }
        //public void RunProTree()
        //{
        //    MainProTreeView.GetTreeview().CollapseAll();
        //    foreach (TreeNode n in MainProTreeView.GetTreeview().Nodes)
        //    {               
        //        if (n.Tag is Container_MainThread == true)
        //        {
        //            Container_MainThread T = (Container_MainThread)n.Tag;
        //            m_MainThread = T;
        //            if (T.isRunning == 0)
        //            {
        //                MainProTreeView.ClearNodeColor();
        //                DUT_BASE tempDUT = null;
        //                foreach (var value in GlobalNew.Devices.Values)
        //                {
        //                    if (value is DUT_BASE)
        //                    {
        //                        tempDUT = (DUT_BASE)value;
        //                        CurrentTempDUT = tempDUT;
        //                        break;
        //                    }
        //                }
        //                if (tempDUT == null)
        //                {
        //                    finalTestResult = "FAIL";
        //                    error_details_firstfail = "Not Found DUT";
        //                    Logger.Error("Not Found DUT");
        //                    SetButton(bt_errorCode, error_details_firstfail);
        //                    SetTestStatus(TestStatus.FAIL);
        //                    return;
        //                }

        //                ClearDataGridTestData(tempDUT);                       
        //                MemoryDataClear(tempDUT);
        //                StartInit(tempDUT);
        //                bool TestResult = false;
        //                object[] context = new object[] { MainProTreeView, T.FailContinue, tempDUT , TestResult,GlobalNew.Devices , JIG_InitailNode };

        //                //qBtn_SingleRun.Text = "Pause";
        //                n.EnsureVisible();

        //                SetTestStatus(TestStatus.START);

        //                Task ta = T.Act(n, context);                       

        //                ta.ContinueWith(t =>
        //                {
        //                    //復歸治具
        //                    //bool isJigRest = JIG_INITIAL();

        //                    bool result = (bool)context[3];

        //                    TestSummary(tempDUT, result);

        //                    SaveLogtoFile(tempDUT.DataCollection.GetMoreProp("ProductSN"));

        //                    ResetUISNtext();

        //                }, TaskScheduler.FromCurrentSynchronizationContext()); // 這樣會確保 ContinueWith 在 UI 主線程上執行}

        //            }
        //            //else if (T.isRunning == 1)
        //            //{
        //            //    qBtn_SingleRun.Text = "Continue";
        //            //    T.T_Pause();
        //            //}
        //            //else if (T.isRunning == 2)
        //            //{
        //            //    qBtn_SingleRun.Text = "Pause";
        //            //    T.T_Continue();
        //            //}
        //        }
        //    }
        //}
        public void ResetUISNtext()
        {
            foreach (TextBox textBox in SNtextBoxes)
            {
                this.Invoke(new Action(() =>
                {
                    //textBox.Enabled = true;
                    textBox.Text = "";

                }));
            }
            this.Invoke(new Action(() =>
            {
                if(SNtextBoxes.Count > 0)
                {
                    SNtextBoxes[0].Enabled = true;
                    SNtextBoxes[0].Focus();
                }

            }));
        }

        public void LockUISNtext()
        {
            foreach (TextBox textBox in SNtextBoxes)
            {
                this.Invoke(new Action(() =>
                {
                    textBox.Enabled = false;
                    textBox.Text = "";

                }));
            }
        }
        public void WriteReport(DUT_BASE tmp,string filePath)
        {
            string directoryPath = Path.GetDirectoryName(filePath);

            // 檢查目錄是否存在，如果不存在則創建
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            using (StreamWriter file = new StreamWriter(filePath))
            {
                string[] headers = { "No", "Item", "Result", "TestTime", "Eslapse", "Retry" };
                string formattedHeader = String.Format("{0,-4} {1,-35} {2,-8} {3,-10} {4,-8} {5,-10}", headers);
                tmp.TestInfo.AddTestStep("Table", formattedHeader);

                foreach (DataGridViewRow row in tmp.DataGridView.Rows)
                {
                    if (!row.IsNewRow)
                    {
                        StringBuilder rowData = new StringBuilder();

                        foreach (DataGridViewCell cell in row.Cells)
                        {
                            if (headers.Contains(cell.OwningColumn.Name))
                            {
                                rowData.Append(cell.Value.ToString()).Append("\t");
                            }
                        }
                        string formattedStep = String.Format("{0,-4} {1,-35} {2,-8} {3,-10} {4,-8} {5,-10}", rowData.ToString().Split('\t'));
                        tmp.TestInfo.AddTestStep("Table", formattedStep);
                    }
                }
            }

            //Variable Table
            string[] Varheaders = { "Key", "Value"};
            string VarformattedHeader = String.Format("{0,-30} {1,-55}", Varheaders);
            tmp.TestInfo.AddTestStep("Variable", VarformattedHeader);
            foreach (var item in tmp.DataCollection.GetData())
            {
                StringBuilder rowData = new StringBuilder();

                rowData.Append(item.Key + "\t" + item.Value);

                string formattedStep = String.Format("{0,-30} {1,-55}", rowData.ToString().Split('\t'));
                tmp.TestInfo.AddTestStep("Variable", formattedStep);
            }

            //MES Table
            string[] MESheaders = { "No.", "Value" };
            string MESformattedHeader = String.Format("{0,-10} {1,-35}", MESheaders);
            tmp.TestInfo.AddTestStep("MES", MESformattedHeader);
            int no_count = 0;
            foreach (var item in tmp.DataCollection.GetMESData())
            {
                StringBuilder rowData = new StringBuilder();

                rowData.Append(no_count++ + "\t" + item.Value);

                string formattedStep = String.Format("{0,-10} {1,-35}", rowData.ToString().Split('\t'));
                tmp.TestInfo.AddTestStep("MES", formattedStep);
            }

            tmp.TestInfo.ExportToPdf(filePath.Replace(".txt",".pdf")) ;
            
            tmp.TestInfo.ExportToFile(filePath);
            if (!string.IsNullOrEmpty(GlobalNew.ServerLOGFOLDER))
            {
                string des = filePath.Replace(GlobalNew.LOGFOLDER, GlobalNew.ServerLOGFOLDER);
                CopyFile(filePath, des);
            }
        }

        public static void CopyFile(string sourcePath, string destinationPath)
        {
            try
            {
                // 確保目標目錄存在
                var directory = Path.GetDirectoryName(destinationPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                
                // 複製文件
                File.Copy(sourcePath, destinationPath, true);
                //Logger.Debug($"Copy File [{sourcePath}] to [{destinationPath}]");

            }
            catch (FileNotFoundException fnfEx)
            {
                //Console.WriteLine("錯誤：找不到源文件。" + fnfEx.Message);
                Logger.Debug($"{fnfEx.Message} Copy File to {destinationPath}");
            }
            catch (DirectoryNotFoundException dnfEx)
            {
                //Console.WriteLine("錯誤：找不到一個或多個目錄。" + dnfEx.Message);
                Logger.Debug($"{dnfEx.Message} Copy File to {destinationPath}");
            }
            catch (IOException ioEx)
            {
                //Console.WriteLine("錯誤：IO異常。" + ioEx.Message);
                Logger.Debug($"{ioEx.Message} Copy File to {destinationPath}");
            }
            catch (Exception ex)
            {
                //Console.WriteLine("錯誤：未預期的異常。" + ex.Message);
                Logger.Debug($"{ex.Message} Copy File to {destinationPath}");
            }
        }
        private void SaveLogtoServer()
        {
            if (error_details_firstfail == "")
                cellLogPath = $@"{finalTestResult}\{PSN}_{DateTime.Now.ToString("hh-mm-ss")}.txt";
            else
                cellLogPath = $@"{finalTestResult}\{PSN}_{error_details_firstfail}_{DateTime.Now.ToString("hh-mm-ss")}.txt";

            SaveRichTextPro(richTextBox1, false, cellLogPath);
        }
        private void SaveLogtoFile(string SN)
        {
            if(error_details_firstfail == "")
                cellLogPath = $@"{finalTestResult}\{SN}_{DateTime.Now.ToString("HH-mm-ss")}.txt";
            else
                cellLogPath = $@"{finalTestResult}\{SN}_{error_details_firstfail}_{DateTime.Now.ToString("HH-mm-ss")}.txt";

            SaveRichTextPro(MainlogRichTextBox, false, cellLogPath);

            if (error_details_firstfail == "")
                cellLogPath = $@"{finalTestResult}\DUT\{SN}_{DateTime.Now.ToString("HH-mm-ss")}.txt";
            else
                cellLogPath = $@"{finalTestResult}\DUT\{SN}_{error_details_firstfail}_{DateTime.Now.ToString("HH-mm-ss")}.txt";

            SaveRichTextPro(DUTRichTextBox, false, cellLogPath);

            if (error_details_firstfail == "")
                cellLogPath = $@"{finalTestResult}\EQ\{SN}_{DateTime.Now.ToString("HH-mm-ss")}.txt";
            else
                cellLogPath = $@"{finalTestResult}\EQ\{SN}_{error_details_firstfail}_{DateTime.Now.ToString("HH-mm-ss")}.txt";

            SaveRichTextPro(EQRichTextBox, false, cellLogPath);

        }
        private void MainProTreeNodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Manufacture.CoreBase obj = (Manufacture.CoreBase)e.Node.Tag;
                int len = MainlogRichTextBox.Text.IndexOf(obj.Description.Substring(2).Trim());
                if (len > 0)
                {
                    //! 光标跳到指定行
                    MainlogRichTextBox.Select(len, 0);
                    MainlogRichTextBox.ScrollToCaret();
                }
            }
        }

        #endregion 進階樹狀流程初始化及運行

        //新增規格卡控顯示於UI中
        public string GetViewSpec(string SpecParam)
        {
            string ShowSpec = string.Empty;

            try
            {
                if (SpecParam != string.Empty)
                {
                    SpecParamsContainer specParams2 = JsonConvert.DeserializeObject<SpecParamsContainer>(SpecParam);

                    foreach (var param in specParams2.specParams)
                    {

                        switch (param.SpecType)
                        {
                            case SpecType.Range:
                                ShowSpec += $"{param.MinLimit} < {param.Name} < {param.MaxLimit}\n";
                                break;

                            case SpecType.Equal:
                                ShowSpec += $"{param.Name} = {param.SpecValue}\n";
                                break;
                            case SpecType.GreaterThan:
                                ShowSpec += $"{param.Name} > {param.SpecValue}\n";

                                break;
                            case SpecType.LessThan:
                                ShowSpec += $"{param.Name} < {param.SpecValue}\n";
                                break;
                            default:
                                ShowSpec += "";
                                break;
                        }
                    }
                }

            }
            catch (Newtonsoft.Json.JsonReaderException)
            {
                ShowSpec += "無法解析輸入數據為 JSON 格式";
            }
            catch (Exception ex)
            {
                ShowSpec += $"處理數據時出現錯誤: {ex.Message}";
            }

            ShowSpec = ShowSpec.TrimEnd('\n');

            return ShowSpec;
            //if (ShowSpec == string.Empty)
            //    newItem.Spec = "N/A";
            //else
            //    newItem.Spec = ShowSpec;
        }

        private void bt_Status_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && e.Clicks == 2)
            {
                if (m_MainThread == null)
                    return;

                if (m_MainThread.isRunning == 1)
                {
                    bt_Status.Text = "Pause";
                    m_MainThread.T_Pause();
                }
                else if (m_MainThread.isRunning == 2)
                {
                    bt_Status.Text = "Testing";
                    m_MainThread.T_Continue();
                }
            }

        }

        private void bt_errorCode_Click(object sender, EventArgs e)
        {
            if(CurrentTempDUT != null)
            {
                CurrentTempDUT.TestInfo.ShowCurrentPDF();
            }
        }
        //private bool JIG_INITIAL()
        //{
        //    if (JIG_InitailNode == null)
        //        return true;

        //    //BlockingForm blockingForm = new BlockingForm();

        //    DUT_Simu SimuDUT = new DUT_Simu();
        //    bool TestResult = false;
        //    object[] context = new object[] { MainProTreeView, false, SimuDUT, TestResult, GlobalNew.Devices };

        //    Container_JIG_INIT xx = new Container_JIG_INIT();
        //    xx.Process(JIG_InitailNode, context);


        //    //Container_MainThread T = new Container_MainThread();
        //    //Task ta = T.Act(JIG_InitailNode, context);

        //    //ta.ContinueWith(t =>
        //    //{
        //    //    //blockingForm.Close();
        //    //    bool result = (bool)context[3];
        //    //    if (result)
        //    //    {
        //    //        ResetUISNtext();
        //    //        Logger.Info("Go Home Successfull!!");
        //    //        return true;
        //    //    }
        //    //    else
        //    //    {
        //    //        Logger.Info("Go Home FAIL!!");
        //    //        return false;
        //    //    }
        //    //}, TaskScheduler.FromCurrentSynchronizationContext()); // 這樣會確保 ContinueWith 在 UI 主線程上執行}

        //    //blockingForm.ShowDialog();

        //    return false;
        //}

        private void HomeBtn_ClickAsync(object sender, EventArgs e)
        {
            if (!isLoadRecipeSuccess)
            {
                MessageBox.Show("載入處方失敗，無法完成初始化動作.");
                return;
            }
                
            bool isHomeStart = false;
            bool isHomeInitSuccess = false;
            LockUISNtext();

            if (!RecipeManagement.StaticUnInitDevices(GlobalNew.Devices))
            {
                GlobalNew.g_Initial = false;
                DialogResult = DialogResult.No;

            }
            if (!RecipeManagement.InitDevices(GlobalNew.CurrentRecipePath, GlobalNew.Devices))
            {
                GlobalNew.g_Initial = false;
            }
            else
                GlobalNew.g_Initial = true;


            InitialGridView();

            if (GlobalNew.g_Initial == true)
            {
                foreach (TreeNode m in MainProTreeView.GetTreeview().Nodes)
                {
                    foreach (TreeNode n in m.Nodes)
                    {
                        if (n.Tag is Container_JIG_INIT == true)
                        {
                            JIG_InitailNode = n;
                            isHomeStart = true;
                            BlockingForm blockingForm = new BlockingForm();

                            DUT_Simu SimuDUT = new DUT_Simu();
                            SimuDUT.isSimu = true;
                            bool TestResult = false;
                            int ResetRes = 0;
                            object[] context = new object[] { MainProTreeView, false, SimuDUT, TestResult, GlobalNew.Devices, JIG_InitailNode };
                            //Container_MainThread T = new Container_MainThread();
                            //Task ta = T.Act(n, context);
                            var task1 = Task.Factory.StartNew(() =>
                            {
                                ResetRes = ((Container_JIG_INIT)n.Tag).Process(n, context);
                            });
                            task1.ContinueWith(t =>
                            {
                                blockingForm.Close();
                                //bool result = (bool)context[3];
                                if (ResetRes == 1)
                                {
                                    isHomeInitSuccess = true;
                                    Logger.Info("Go Home Successfull!!");
                                }
                                else
                                {
                                    Logger.Info("Go Home FAIL!!");
                                }
                                MemoryDataClear(SimuDUT);
                            }, TaskScheduler.FromCurrentSynchronizationContext()); // 這樣會確保 ContinueWith 在 UI 主線程上執行}

                            blockingForm.ShowDialog();

                            //只能有一個HOME容器運行
                            break;
                        }
                    }
                }
                //當裝置初始化完成且沒有拉HOME流程式可開啟輸號輸入
                if (isHomeStart == false)
                {
                    JIG_InitailNode = null;
                    ResetUISNtext();
                }
            }
            else
            {
                string message = "Some devices have not been initialized successfully.\n"
                 + "有裝置尚未初始化成功，請檢查並重試。";
                MessageBox.Show(message);
            }

            if (isHomeInitSuccess == true)
            {
                this.Invoke((MethodInvoker)delegate
                {
                    ResetUISNtext();
                });
            }
        }
        DataInfo Info = null;
        private void MainlogRichTextBox_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.X >= MainlogRichTextBox.Width - 50 && e.X <= MainlogRichTextBox.Width && e.Y >= 0 && e.Y <= 50)
            {
                if (CurrentTempDUT != null)
                {
                    if (Info == null)
                    {
                        Info = new DataInfo(CurrentTempDUT.DataCollection.GetData(), CurrentTempDUT.DataCollection.GetMESData(), CurrentTempDUT.DataCollection.GetSpecData());
                        Info.StartTimer(700);
                    }

                    Info.Left = 0;
                    Info.Top = 0;
                    Info.UpdateDictData(CurrentTempDUT.DataCollection.GetData(), CurrentTempDUT.DataCollection.GetMESData(), CurrentTempDUT.DataCollection.GetSpecData());
                    Info.Show();
                }
            }

        }


        private void MainInfoRichTextBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (Info == null)
                return;

                Point screenPosition = this.PointToScreen(e.Location);

            // 如果鼠标在屏幕左侧 formWidth 像素范围内
            if (screenPosition.X <= 20)
            {
                // 如果窗体当前已经显示，则不做任何操作
                //if (Info.Left == visibleLeft)
                //    return;

                // 将窗体从左侧滑出来
                Info.Left = 0;
                Info.UpdateDictData(CurrentTempDUT.DataCollection.GetData(), CurrentTempDUT.DataCollection.GetMESData(), CurrentTempDUT.DataCollection.GetSpecData());
                Info.Show();
            }
            else
            {
                // 如果窗体当前已经隐藏，则不做任何操作
                //if (Info.Left == hiddenLeft)
                //    return;

                // 将窗体隐藏在最左侧
                Info.Hide();
            }
        }

    }
}