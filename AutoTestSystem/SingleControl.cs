/*
 * "Not use"
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
 *  1. <SingleControl.cs> is out of service   
 * 
 */


using AutoTestSystem.BLL;
using AutoTestSystem.DAL;
using AutoTestSystem.Model;
using Checkroute;
using Newtonsoft.Json;
using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutoTestSystem
{
    public partial class SingleControl : UserControl, ISavelog
    {
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000;                                               //Turn on WS_EX_COMPOSITED
                return cp;
            }
        }//!解决刷新控件时窗体闪烁

        public int localNo;                                                             //从1开始
        public int letoutIndex;                                                         //从1开始 即columnIndex
        public int rowIndex;                                                            //从1开始
        public string WebPsIp = "192.168.10.";                                          //WebPowerSwitch IP

        Color Red = Color.FromArgb(255, 53, 53);
        public LoadSeq loadSeq = null;
        public DataTable testCaseTable;
        public string[] itemHeader;                                                     //用例表头
        public readonly object wLock = new object();                                    //!互斥锁  
        public object cycleClock;
        private Thread testThread;                                                      //!运行主线程
        int sec = 0;                                                                    //!测试时间
        public bool startFlag = false;                                                         //!启动信号
        bool ifCond = true;                                                             //!IF条件语句结果
        int seqNo = -1;                                                                 //!当前测试用例号
        int itemsNo = -1;                                                               //!当前测试项目号     
        DateTime startTime = new DateTime();                                            //!开始测试时间
        DateTime endTime = new DateTime();                                              //!结束测试时间
        System.Threading.Timer timer;

        List<Sequence> Sequences = null;                                                //!测试用例队列  
        MesPhases mesPhases = null;
        test_phases testPhase = new test_phases();
        Station station = null;
        static Mescheckroute mescheckroute = new Mescheckroute(Global.MESIP);

        SSH sshcon;
        ConnectionInfo sshconInfo;                                                      //!SSH连接信息
        SerialConnetInfo FixCOMinfo;                                                    //!治具COM口连接信息
        SerialConnetInfo DUTCOMinfo;                                                    //!DUT COM口连接信息
        WebSwitchConInfo webSwitchCon;
        public DLIOutletClient c;
        Telnet telnet;
        ///GPIB mPS;                                                                    //!GPIB连接

        public string error_code = null;
        public string error_details = null;
        string finalTestResult = "FAIL";                                                //最终结果默认值为FAIL：测试&&MES上传结果
        string mesUrl;
        private string SN;
        public string cellLogPath;
        private string DUTIP;
        private string MesMac;
        private string cellMode;                                                        //机种
        private string reNameCellLogPath;
        public bool SetIPflag = false;                                                  //是否设置IP为默认

        public enum TestStatus
        {
            PASS = 1,                                                                   //测试pass
            FAIL = 2,                                                                   //测试fail
            START = 3,                                                                  //开始测试，正在测试中
        }

        public SingleControl()
        {
            //设定按字体来缩放控件
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            InitializeComponent();
        }

        public SingleControl(int _localNo)
        {
            localNo = _localNo;
            letoutIndex = _localNo % 8;                                                 //余数
            rowIndex = (_localNo) / 8 + 1;
            if (letoutIndex == 0)
            {
                letoutIndex = 8;
                rowIndex--;
            }
            WebPsIp += rowIndex.ToString();
            InitializeComponent();
            lb_cellNum.Text = _localNo.ToString();
            Sequences = ObjectCopier.Clone<List<Sequence>>(Global.Sequences);           //!克隆测试用例序列对象

            testThread = new Thread(new ThreadStart(TestThread));                       //!创建测试主线程
            testThread.IsBackground = true;                                             //!设置后台进程，解决关闭主窗口进程不退出问题。
        }


        /// <summary>
        /// 根据扫描的SN判断机种
        /// </summary>
        /// <param name="sn"></param>
        /// <param name="isJudge">是否判断</param>
        public void JudgeProdMode(string sn, bool isJudge = true)
        {
            if (isJudge && !string.IsNullOrEmpty(sn))
            {
                if (sn[0] == 'N')
                {
                    cellMode = Global.ProMode1;
                }
                if (sn[0] == 'Q')
                {
                    cellMode = Global.ProMode2;
                }
            }
            else
            {
                cellMode = Global.ProMode;               // 默认机种
            }
            this.lb_mode.Text = cellMode;
        }

        public void StartTest()
        {
            SN = tb_sn.Text.Trim();
            GetDUTIpFormMES();
            JudgeProdMode(SN);
            if (!testThread.IsAlive)
            {
                testThread.Start();
            }
            StartTestInit();
        }

        private void GetDUTIpFormMES()
        {
            if (mescheckroute.GETIP(SN, out DUTIP, out string mesMsg) && mesMsg.Contains("OK"))
            {
                SaveLog("Get UUT IP Form MES:IP=" + DUTIP);
            }
            else
            {
                SaveLog("Get UUT IP Form MES FAIL!!!!");
            }
        }

        /// <summary>
        /// 重命名测试Log文件
        /// </summary>
        /// <param name="oldNamePath"></param>
        private void ReNameFile(string oldNamePath)
        {
            try
            {
                string logfilename = Path.GetFileName(oldNamePath);
                reNameCellLogPath = Path.GetDirectoryName(oldNamePath) + $@"\{finalTestResult}_{error_details}_{logfilename}";
                FileInfo fi = new FileInfo(oldNamePath);
                fi.MoveTo(reNameCellLogPath);
            }
            catch (Exception ex)
            {
                SaveLog(ex.ToString());
                throw;
            }
        }

        private void StartTestInit()
        {
            telnet = new Telnet(DUTIP, cellLogPath);
            mesPhases = new MesPhases { IP = DUTIP, NO = localNo.ToString() };      //每次测试前初始化MES上传信息
            station = new Station(SN, Global.FIXTURENAME, DateTime.Now.ToString(), Global.TESTMODE, Global.Version.ToString());
            webSwitchCon = new WebSwitchConInfo { IPAddress = WebPsIp, Username = "admin", Password = "1234" };
            c = new DLIOutletClient(webSwitchCon, cellLogPath);
            cycleClock = Form1.CycleLock[(localNo - 1) / 8];

            seqNo = 0;
            itemsNo = 0;
            SetTestStatus(TestStatus.START);
        }

        public void SetTestStatus(TestStatus testStatus)
        {
            try
            {
                switch (testStatus)
                {
                    case TestStatus.START:
                        this.BackColor = Color.Yellow;
                        sec = 0;                                                        //测试计时归零
                        startTime = DateTime.Now;                                       //!记录开始时间 
                        startFlag = true;                                               //!开始信号
                        SaveLog($"TestStart....CellNO={localNo},SN={SN},Station={Global.FIXTURENAME},DUTMode={cellMode},TestMode={Global.TESTMODE},WebPS={WebPsIp}");
                        timer = new System.Threading.Timer(TimerCallBack, null, 0, 1000);
                        break;
                    case TestStatus.FAIL:
                        FailDealWith();
                        this.BackColor = Color.Red;
                        endTime = DateTime.Now;                                         //!记录测试结束时间 
                        startFlag = false;                                              //!停止信号
                        ReNameFile(cellLogPath);
                        Form1.checkSNlist.Remove(SN);
                        SN = "";
                        timer.Dispose();
                        break;
                    case TestStatus.PASS:
                        this.BackColor = Color.Green;
                        endTime = DateTime.Now;                                         //!记录测试结束时间 
                        startFlag = false;                                              //!停止信号
                        ReNameFile(cellLogPath);
                        Form1.checkSNlist.Remove(SN);
                        SN = "";
                        timer.Dispose();
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                SaveLog(ex.ToString());
                throw;
            }
        }

        private void FailDealWith()
        {
            SaveLog("show information and dealwith after test fail:");
            string recvStr = "";
            if (!telnet.socket.Connected)
            {
                if (telnet.Connect(Global.PROMPT))
                {
                    telnet.TelnetSend("ps", ref recvStr, Global.PROMPT, 20);
                    telnet.TelnetSend("luxshare_tool --get-cpuinfo temperature", ref recvStr, Global.PROMPT, 20);
                    if (SetIPflag)
                    {
                        telnet.TelnetSend("luxshare_tool --set-ipaddr-env " + DUTIP, ref recvStr, Global.PROMPT, 20);
                    }
                }
            }
            else
            {
                telnet.TelnetSend("ps", ref recvStr, Global.PROMPT, 20);
                telnet.TelnetSend("luxshare_tool --get-cpuinfo temperature", ref recvStr, Global.PROMPT, 20);
                if (SetIPflag)
                {
                    telnet.TelnetSend("luxshare_tool --set-ipaddr-env " + DUTIP, ref recvStr, Global.PROMPT, 20);
                }
            }
        }

        /// <summary>
        /// 运行线程
        /// </summary>
        private void TestThread()
        {
            try
            {
                while (true)
                {
                    if (startFlag)                                                      ///!如果开始标志为假则不运行程序
                    {
                        if (itemsNo == 0)                                               ///如果是第一个测试项，则记录sequence开始测试时间
                        {
                            Sequences[seqNo].start_time = DateTime.Now.ToString();
                        }

                        Items tempItem = Sequences[seqNo].SeqItems[itemsNo];            //!当前测试用项   

                        phase_items phase_item = new phase_items();                     //保存Seq测试结果，生成JSON格式文件

                        tempItem.tResult = true;                                        //!把测试结果置真,所有的测试步的结果,如果有一个假的测试项结果就会为假
                        Sequences[seqNo].IsTest = Sequences[seqNo].IsTest | tempItem.isTest;

                        if (ifCond)                                                     ///!根据测试步骤的IF条件决定是否执行
                        {
                            if (tempItem.IfElse == "if_false")
                            {
                                tempItem.isTest = false;
                            }
                        }
                        else
                        {
                            if (tempItem.IfElse == "if_true")
                            {
                                tempItem.isTest = false;
                            }
                        }

                        if (tempItem.isTest)
                        {
                            tempItem.Clear();  //每次测试前清除上次测试记录
                            SaveLog($"Start: {tempItem.ItemName},{(tempItem.RetryTimes == "0" ? "..." : $"Retry {tempItem.RetryTimes} times")}");
                            SetLables(this.lb_testName, tempItem.ItemName);
                            tempItem.startTime = DateTime.Now;                          //测试项开始时间 
                            int RetryTimes = 0;
                            if (!string.IsNullOrEmpty(tempItem.RetryTimes))
                            {
                                RetryTimes = int.Parse(tempItem.RetryTimes);
                            }
                            ///!运行测试步骤
                            bool result = true;
                            for (int i = RetryTimes; i > -1; i--)
                            {
                                if (StepTest(tempItem, i, phase_item))
                                {
                                    result = true;
                                    break;
                                }
                                else
                                {
                                    if (i == 0)
                                    {
                                        result = false;
                                    }
                                }
                            }

                            ///!记录测试结果
                            tempItem.tResult &= result;
                            Sequences[seqNo].TestResult &= tempItem.tResult;

                            station.status = Sequences[seqNo].TestResult.ToString();

                            if (!tempItem.tResult)                                      //测试fail停止测试,生成结果。 
                            {
                                Sequences[seqNo].finish_time = DateTime.Now.ToString(); //sequence结束时间。    
                                testPhase.Copy(Sequences[seqNo], this);                 //把seq测试结果保存到test_phase变量中
                                station.test_phases.Add(testPhase);                     //加入station实例,记录测试结果 用于序列化Json文件                            
                                testPhase = new test_phases();                          //!把testPhase初始化
                                AddStationResult(false, error_code, error_details);
                                SetMESInfo(mesPhases, Sequences[seqNo].SeqName, Sequences[seqNo].TestResult ? "Pass" : "Fail");

                                UploadJsonToClient();
                                PostJsonToMES();                                        //上传结果到MES
                                SetTestStatus(TestStatus.FAIL);                         //!设置测试结果为FAIL
                            }
                        }

                        itemsNo++;                                                      //!测试项序号+1,继续下一个item
                        if (tempItem.tResult)                                           ///!测试结果处理
                        {
                            if (itemsNo >= Sequences[seqNo].SeqItems.Count)             //!如果测试item是所在Seq中最后一个
                            {
                                Sequences[seqNo].finish_time = DateTime.Now.ToString(); //sequence结束时间。    
                                testPhase.Copy(Sequences[seqNo], this);                 //把seq测试结果保存到test_phase变量中
                                station.test_phases.Add(testPhase);                     //加入station实例,记录测试结果 用于序列化Json文件                            
                                testPhase = new test_phases();                          //!把testPhase初始化
                                SetMESInfo(mesPhases, Sequences[seqNo].SeqName, Sequences[seqNo].TestResult ? "Pass" : "Fail");

                                itemsNo = 0;
                                Sequences[seqNo].IsTestFinished = true;                 //!测试结束标志为真
                                seqNo++;                                                //!测试用例号+1
                                if (seqNo >= Sequences.Count)                           //!如果是最后一个测试用例则上传测试结果,结束测试
                                {
                                    seqNo = 0;
                                    AddStationResult(true, error_code, error_details);
                                    UploadJsonToClient();
                                    if (!PostJsonToMES())                               //上传结果到MES失败
                                    {
                                        SetTestStatus(TestStatus.FAIL);                 //!设置测试结果为FAIL
                                    }
                                    else
                                    {
                                        SetTestStatus(TestStatus.PASS);                 //!设置测试结果为PASS
                                        finalTestResult = "PASS";
                                    }
                                    Thread.Sleep(200);
                                }
                                else
                                {
                                    Sequences[seqNo].Clear();                          //!初始化要测试的测试用例参数,重复测试的时候,这些值需要清除
                                }
                            }
                        }
                    }
                    else
                    {
                        Thread.Sleep(10);
                    }
                }
            }
            catch (Exception ex)
            {
                SaveLog("TestThread():" + ex.ToString());
                throw;
            }
        }

        private bool StepTest(Items item, int retryTimes, phase_items phaseItem)
        {
            //Thread.Sleep(1);
            //return true;
            bool rReturn = false;

            if (Global.TESTMODE == "debug"
                && (item.ItemName.Contains("Set ipaddr_env") || item.ItemName.Contains("Get ipaddr_env")))
            {
                return rReturn = true;
            }

            try
            {
                switch (item.ItemName)
                {
                    case "VeritfyDUTMAC":
                        {
                            string revStr = "";
                            if (telnet.TelnetSend(item.ComdSend, ref revStr, item.ExpectStr, Int16.Parse(item.TimeOut))
                                && revStr.Contains(item.CheckStr1)
                                && revStr.Contains(item.CheckStr2)
                                && mescheckroute.GetCsnErroMessage(SN, out string sn, out string IPSN, out string MesMac, out string mesMsg)
                                && mesMsg.Contains("OK"))
                            {
                                if (!string.IsNullOrEmpty(item.SubStr1) || !string.IsNullOrEmpty(item.SubStr2))     //需要提取TestValue
                                {
                                    item.TestValue = GetSubStringOfMid(revStr, item.SubStr1, item.SubStr2);
                                    if (string.IsNullOrEmpty(item.TestValue)) { SaveLog("Error! Get TestValue IsNullOrEmpty."); }
                                    else
                                    {
                                        SaveLog("GetTestValue:" + item.TestValue);
                                    }
                                }
                                SaveLog("GetMesMac:" + MesMac);
                                if (item.TestValue == MesMac)
                                {
                                    rReturn = true;
                                }
                                else
                                {
                                    error_code = item.ErrorCode.Split(':')[0].Trim();
                                    error_details = item.ErrorCode.Split(':')[1].Trim();
                                }
                            }
                            else
                            {
                                error_code = item.ErrorCode.Split(':')[0].Trim();
                                error_details = item.ErrorCode.Split(':')[1].Trim();
                            }
                        }
                        break;
                    case "VeritfyDUTSN":
                        {
                            string revStr = "";
                            if (telnet.TelnetSend(item.ComdSend, ref revStr, item.ExpectStr, Int16.Parse(item.TimeOut))
                                && revStr.Contains(item.CheckStr1)
                                && revStr.Contains(item.CheckStr2))
                            {
                                if (!string.IsNullOrEmpty(item.SubStr1) || !string.IsNullOrEmpty(item.SubStr2))     //需要提取TestValue
                                {
                                    item.TestValue = GetSubStringOfMid(revStr, item.SubStr1, item.SubStr2);
                                    if (string.IsNullOrEmpty(item.TestValue)) { SaveLog("Error! Get TestValue IsNullOrEmpty."); }
                                    else
                                    {
                                        SaveLog("GetTestValue:" + item.TestValue);
                                    }
                                }

                                if (item.TestValue == SN)
                                {
                                    rReturn = true;
                                }
                                else
                                {
                                    error_code = item.ErrorCode.Split(':')[0].Trim();
                                    error_details = item.ErrorCode.Split(':')[1].Trim();
                                }
                            }
                            else
                            {
                                error_code = item.ErrorCode.Split(':')[0].Trim();
                                error_details = item.ErrorCode.Split(':')[1].Trim();
                            }
                        }
                        break;
                    case "Veritfy_QSDK_Version":
                        {
                            string revStr = "";
                            if (telnet.TelnetSend(item.ComdSend, ref revStr, item.ExpectStr, Int16.Parse(item.TimeOut))
                                && revStr.Contains(item.CheckStr1)
                                && revStr.Contains(item.CheckStr2))
                            {
                                if (!string.IsNullOrEmpty(item.SubStr1) || !string.IsNullOrEmpty(item.SubStr2))     //需要提取TestValue
                                {
                                    item.TestValue = GetSubStringOfMid(revStr, item.SubStr1, item.SubStr2);
                                    if (string.IsNullOrEmpty(item.TestValue)) { SaveLog("Error! Get TestValue IsNullOrEmpty."); }
                                    else
                                    {
                                        SaveLog("GetTestValue:" + item.TestValue);
                                    }
                                }
                                string mes_qsdk_version = mescheckroute.getFirmwareFW(SN);
                                SaveLog("Get mes_qsdk_version:" + mes_qsdk_version);
                                if (item.TestValue == mes_qsdk_version)
                                {
                                    station.luxshare_qsdk_version = mes_qsdk_version;
                                    rReturn = true;
                                }
                                else
                                {
                                    //station.luxshare_qsdk_version = item.TestValue;
                                    error_code = item.ErrorCode.Split(':')[0].Trim();
                                    error_details = item.ErrorCode.Split(':')[1].Trim();
                                }
                            }
                            else
                            {
                                error_code = item.ErrorCode.Split(':')[0].Trim();
                                error_details = item.ErrorCode.Split(':')[1].Trim();
                            }
                        }
                        break;

                    case "CheckEeroTest":
                        {
                            if (Global.TESTMODE == "debug")
                            {
                                return rReturn = true;
                            }
                            if (mescheckroute.CheckEeroTest(SN, Global.TESTMODE, out string mesMsg) && mesMsg.Contains("OK"))
                            {
                                rReturn = true;
                            }
                            else
                            {
                                SaveLog("mesMsg:" + mesMsg);
                                error_code = item.ErrorCode.Split(':')[0].Trim();
                                error_details = item.ErrorCode.Split(':')[1].Trim();
                            }
                        }
                        break;
                    case "checkroute":
                        {
                            if (Global.TESTMODE == "debug")
                            {
                                return rReturn = true;
                            }
                            if (mescheckroute.checkroute(SN, Global.FIXTURENAME, out string mesMsg) && mesMsg.Contains("OK"))
                            {
                                rReturn = true;
                            }
                            else
                            {
                                SaveLog("mesMsg:" + mesMsg);
                                error_code = item.ErrorCode.Split(':')[0].Trim();
                                error_details = item.ErrorCode.Split(':')[1].Trim();
                            }
                        }
                        break;
                    case "checkEeroABA":
                        {
                            if (Global.TESTMODE == "debug")
                            {
                                return rReturn = true;
                            }
                            if (mescheckroute.checkEeroABA(SN, Global.FIXTURENAME, Global.STATIONNAME, out string mesMsg) && mesMsg.Contains("OK"))
                            {
                                rReturn = true;
                            }
                            else
                            {
                                SaveLog("mesMsg:" + mesMsg);
                                error_code = item.ErrorCode.Split(':')[0].Trim();
                                error_details = item.ErrorCode.Split(':')[1].Trim();
                            }
                        }
                        break;
                    case "WaitForTelnet":
                    case "PingUUT&WaitForTelnet":
                        {
                            telnet.DisConnect();
                            if (Power_OnOff(letoutIndex, false)
                                && Power_OnOff(letoutIndex, true)
                                && PingIP(DUTIP, int.Parse(item.TimeOut))
                                && telnet.Connect(Global.PROMPT))
                            {
                                rReturn = true;
                            }
                            else
                            {
                                error_code = item.ErrorCode.Split(':')[0].Trim();
                                error_details = item.ErrorCode.Split(':')[1].Trim();
                            }
                        }
                        break;
                    case "ReportChildBoard":
                        {
                            if (mescheckroute.GetCsnErroMessage(SN, out string serialNum, out string ItemPartSN, out MesMac, out string mesMsg)
                                && mesMsg.Contains("OK"))
                            {
                                phaseItem.serial = ItemPartSN.Trim();
                                if (cellMode == "GateWay")
                                {
                                    phaseItem.model = "alma";
                                }
                                if (cellMode == "Leaf")
                                {
                                    phaseItem.model = "adventure";
                                }
                                rReturn = true;
                            }
                            else
                            {
                                error_code = item.ErrorCode.Split(':')[0].Trim();
                                error_details = item.ErrorCode.Split(':')[1].Trim();
                            }
                        }
                        break;
                    case "PingUUT":
                        rReturn = PingIP(DUTIP, int.Parse(item.TimeOut));
                        if (!rReturn)
                        {
                            error_code = item.ErrorCode.Split(':')[0].Trim();
                            error_details = item.ErrorCode.Split(':')[1].Trim();
                        }
                        break;
                    case "PowerCycleTest":
                        {
                            lock (cycleClock)
                            {
                                telnet.DisConnect();
                                bool powercycle_ALL = true;
                                for (int i = 0; i < int.Parse(item.TimeOut); i++)
                                {
                                    SaveLog($"--Times {i} ");
                                    bool powercycle = PowerCycleOutlet(letoutIndex);
                                    Thread.Sleep(1000);
                                    powercycle_ALL &= powercycle;
                                }
                                rReturn = powercycle_ALL;
                                if (!rReturn)
                                {
                                    error_code = item.ErrorCode.Split(':')[0].Trim();
                                    error_details = item.ErrorCode.Split(':')[1].Trim();
                                }
                            }
                        }
                        break;

                    default:
                        {
                            if (cellMode == "Leaf" && (item.ItemName == "check PHY info" || item.ItemName.Contains("ReadBTZFwVersion")))
                            {
                                return rReturn = true;
                            }

                            SaveLog("Warning!!!,this is default testmethod.", 2);
                            string[] ErrorList = item.ErrorCode.Split(new string[] { "\n" }, 0);                    //如果有多行errorcode

                            string revStr = "";
                            if (telnet.TelnetSend(item.ComdSend, ref revStr, item.ExpectStr, Int16.Parse(item.TimeOut))
                                && revStr.Contains(item.CheckStr1)
                                && revStr.Contains(item.CheckStr2))
                            {
                                rReturn = true;
                                if (!string.IsNullOrEmpty(item.SubStr1) || !string.IsNullOrEmpty(item.SubStr2))     //需要提取TestValue
                                {
                                    item.TestValue = GetSubStringOfMid(revStr, item.SubStr1, item.SubStr2);
                                    if (string.IsNullOrEmpty(item.TestValue)) { SaveLog("Error! Get TestValue IsNullOrEmpty."); }
                                    else
                                    {
                                        SaveLog("GetTestValue:" + item.TestValue);
                                    }
                                }

                                if (!string.IsNullOrEmpty(item.Limit_min) && !string.IsNullOrEmpty(item.Limit_max)) //需要比较最小值和最大值
                                {
                                    rReturn = double.Parse(item.TestValue) >= double.Parse(item.Limit_min) && double.Parse(item.TestValue) <= double.Parse(item.Limit_max);
                                    if (!rReturn)
                                    {
                                        if (double.Parse(item.TestValue) < double.Parse(item.Limit_min))
                                        {
                                            error_code = ErrorList[0].Split(':')[0].Trim();                 //TooLow
                                            error_details = ErrorList[0].Split(':')[1].Trim();
                                        }
                                        else
                                        {
                                            if (ErrorList.Length > 1)                                       //TooHight
                                            {
                                                error_code = ErrorList[1].Split(':')[0].Trim();
                                                error_details = ErrorList[1].Split(':')[1].Trim();
                                            }
                                        }
                                    }
                                }

                                if (string.IsNullOrEmpty(item.Limit_min) && !string.IsNullOrEmpty(item.Limit_max))  //只需比较最大值
                                {
                                    rReturn = double.Parse(item.TestValue) <= double.Parse(item.Limit_max);
                                    if (!rReturn)
                                    {
                                        error_code = ErrorList[0].Split(':')[0].Trim();
                                        error_details = ErrorList[0].Split(':')[1].Trim();
                                    }
                                }

                                if (!string.IsNullOrEmpty(item.Limit_min) && string.IsNullOrEmpty(item.Limit_max))  //只需比较最小值
                                {
                                    rReturn = double.Parse(item.TestValue) >= double.Parse(item.Limit_min);
                                    if (!rReturn)
                                    {
                                        error_code = ErrorList[0].Split(':')[0].Trim();
                                        error_details = ErrorList[0].Split(':')[1].Trim();
                                    }
                                }
                            }
                            else
                            {
                                error_code = ErrorList[0].Split(':')[0].Trim();
                                error_details = ErrorList[0].Split(':')[1].Trim();
                            }
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                SaveLog(ex.ToString(), 2);
                rReturn = false;
                error_code = item.ErrorCode.Split(':')[0].Trim();
                error_details = item.ErrorCode.Split(':')[1].Trim();
            }
            finally
            {
                if (retryTimes == 0 || rReturn)
                {
                    item.ElapsedTime = String.Format("{0,0:F3}", Convert.ToDouble((DateTime.Now - item.startTime).TotalSeconds));
                    SaveLog($"{item.ItemName} {(rReturn ? "PASS" : "FAIL")}!! ElapsedTime:{item.ElapsedTime}, ErrorCode:{error_code}{error_details}, Limit_min:{item.Limit_min}, TestValue:{item.TestValue}, Limit_max:{item.Limit_max}");

                    if (!item.ItemName.StartsWith("Check") && !item.ItemName.StartsWith("check")
                        && item.ItemName != "PingUUT&WaitForTelnet" && !item.ItemName.Contains("VeritfyDUT")
                        && item.ItemName != "ReadBTZFwVersion" && item.ItemName != "PowerCycleTest"
                        && item.ItemName != "WaitForTelnet" && item.ItemName != "HardwareReset" && item.ItemName != "RAMStressTest"
                        && item.ItemName != "CPUStressTest" && item.ItemName != "MMCStressTest")
                    {
                        //SaveLog("Copy test info to Json:" + item.ItemName);
                        phaseItem.Copy(item);
                        //SaveLog("Add phaseItem to testPhase");
                        testPhase.phase_items.Add(phaseItem);
                    }
                    SetMESInfo(mesPhases, item.MES_var, item.TestValue);
                }

                if (item.ItemName == "Get ipaddr_env" && rReturn)
                {
                    SetIPflag = true;
                }
            }
            return rReturn;
        }

        #region TestsSep func

        /// <summary>
        /// 截取sub1和sub2中间字符串
        public static string GetSubStringOfMid(string souce, string sub1, string sub2)
        {
            int p1 = 0, p2, start = 0, length = 0;

            if (!string.IsNullOrEmpty(sub1))
            {
                p1 = souce.IndexOf(sub1);
                start = p1 + sub1.Length;
            }
            else
            {
                start = 0;                      //从字符串的0位置开始截取
            }

            if (!string.IsNullOrEmpty(sub2))
            {
                p2 = souce.IndexOf(sub2);
            }
            else
            {
                return souce.Substring(start); //一直截取到字符串末尾
            }
            length = p2 - start;
            return souce.Substring(start, length).Replace("\n", "").Replace("\t", "").Replace("\r", "").Trim();
        }

        /// <summary>
        /// 截取sub1和sub2第n次出现的中间字符串
        /// </summary>
        /// <param name="souce"></param>
        /// <param name="sub1"></param>
        /// <param name="sub2"></param>
        /// <param name="no">第几次出现</param>
        /// <returns></returns>
        public string GetSubStringOfMid(string souce, string sub1, string sub2, int no)
        {

            int p1 = GetPosition(souce, sub1, no);
            int p2 = GetPosition(souce, sub2, no);
            if (p1 == 0 || p2 == 0)
            {
                return "";
            }
            int start = p1 + sub1.Length;
            if ((p2 - start) < 0)
            {
                return "";
            }
            return souce.Substring(start, p2 - start).ToString().Trim();
        }

        /// <summary>
        /// 获取当前字符在字符串中第no次出现的位置
        /// </summary>
        /// <param name="s">字符串</param>
        /// <param name="key">字符</param>
        /// <param name="no">第几次出现</param>
        /// <returns>返回位置</returns>
        public static int GetPosition(string souces, string key, int no)
        {
            int pos = 0;  //!出现的次数,每出现一次+1
            if (no == 0)    //!如果是查找第0次出现则直接返回
                return 0;

            for (int i = 0; i < souces.Length; i++)  //!遍历
            {
                if (souces.IndexOf(key, i) > -1)   //!查找到关键字
                {
                    i = souces.IndexOf(key, i);
                    pos++;  //!出现次数+1
                    if (pos >= no)  //!如果是需要的出现次数,则返回当前位置
                        return i;
                }
            }
            return 0;
        }

        #endregion
        public void AddStationResult(bool _result, string _errorcode, string _errordetails)
        {
            station.finish_time = DateTime.Now.ToString();
            station.status = _result ? "PASS" : "FAIL";
            station.error_code = _errorcode;
            station.error_details = _errordetails;
            station.CopyToMES(mesPhases); //结果复制到MES
        }


        delegate void SetLableDelegate(Label bts, string text, bool visible);
        public void SetLables(Label label, string strInfo, bool visible = true)
        {
            if (label.InvokeRequired)
            {
                SetLableDelegate d = new SetLableDelegate(SetLables);
                this.Invoke(d, new object[] { label, strInfo, visible });
            }
            else
            {
                if (!label.IsDisposed)
                {
                    //label.BackColor = color;
                    label.Text = strInfo;
                    //label.Visible = visible;
                }
            }
        }

        private bool UploadJsonToClient()
        {
            SaveLog("Start to Serialize station Json info...");
            bool result = true;
            try
            {
                ///!序列化Json 配置，忽略值为null字段
                JsonSerializerSettings setting = new JsonSerializerSettings();
                JsonConvert.DefaultSettings = new Func<JsonSerializerSettings>(() =>
                {
                    setting.DateFormatHandling = DateFormatHandling.MicrosoftDateFormat;
                    setting.DateFormatString = "yyyy-MM-dd HH:mm:ss";                                                //!日期类型默认格式化处理
                    setting.NullValueHandling = NullValueHandling.Ignore;                                            //!空值处理,忽略空值
                    return setting;
                });
                string JsonClintContent = JsonConvert.SerializeObject(station, Formatting.Indented, setting);        //!Station Json数据序列化
                SaveLog(JsonClintContent);
                string JsonPath = Global.LogPath + @"\Json\" + SN + "_" + DateTime.Now.ToString("hhmmss") + ".json"; //!将内容写进json文件中
                File.WriteAllText(JsonPath, JsonClintContent);

                if (!UploadJson(JsonPath))
                {
                    Thread.Sleep(3000);
                    UploadJson(JsonPath);
                }
            }
            catch (Exception ex)
            {
                result = false;
                SaveLog(ex.ToString());
            }
            return result;
        }

        public bool UploadJson(string JsonFilePath)
        {
            if (Global.TESTMODE.ToLower() == "debug")
            {
                return true;
            }
            try
            {
                string[] strArr = new string[1];//参数列表
                string cmd = $@"python {Application.StartupPath}\{Global.PySCRIPT} {JsonFilePath}";
                RunDOSCmd(cmd, out string errors);
                SaveLog("pythonUplaod error message:" + errors);
                if (string.IsNullOrEmpty(errors))
                {

                    SaveLog("Jsoninfo upload to client succss.");
                    mesPhases.JSON_UPLOAD = "Pass";
                    return true;
                }
                else
                {
                    SaveLog("Jsoninfo upload to client fail.");
                    mesPhases.JSON_UPLOAD = "Fail";
                    return false;
                }
            }
            catch (Exception ex)
            {
                SaveLog($"json_upload_Exception:{ex.ToString()}");
                return false;
            }
        }


        public string RunDOSCmd(string cmd, out string errors)//, string directoryPath = @"C:\Windows\System32")
        {
            SaveLog($"DOSCommand-->:{cmd}");
            //说明：不管命令是否成功均执行exit命令，否则当调用ReadToEnd()方法时，会处于假死状态
            using (Process p = new Process())
            {
                p.StartInfo.FileName = "cmd.exe";
                p.StartInfo.Arguments = "/c " + cmd;
                p.StartInfo.UseShellExecute = false;        //是否使用操作系统shell启动
                p.StartInfo.RedirectStandardInput = true;   //接受来自调用程序的输入信息
                p.StartInfo.RedirectStandardOutput = true;  //由调用程序获取输出信息
                p.StartInfo.RedirectStandardError = true;   //重定向标准错误输出
                p.StartInfo.CreateNoWindow = true;          //不显示程序窗口
                p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                // p.StartInfo.WorkingDirectory = directoryPath;
                string error = "";
                p.ErrorDataReceived += new DataReceivedEventHandler((sender, e) =>
                { error += e.Data; });
                p.Start();
                p.StandardInput.WriteLine(cmd);
                p.StandardInput.AutoFlush = true;
                p.StandardInput.Close();
                p.BeginErrorReadLine(); //获取cmd窗口的输出信息
                string output = p.StandardOutput.ReadToEnd();
                p.WaitForExit();
                p.Close();
                SaveLog(output);
                errors = error;
                return output;
            }
        }

        /// <summary>
        /// 运行DOS命令
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="errors">命令错误信息</param>
        /// <param name="timeout">超时时间</param>
        /// <returns></returns>
        public string RunDOSCmd(string cmd, out string errors, int timeout = 10)//, string directoryPath = @"C:\Windows\System32")
        {
            using (Process process = new Process())
            {
                process.StartInfo.FileName = "cmd.exe";
                process.StartInfo.Arguments = "/c " + cmd;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardInput = true;   //接受来自调用程序的输入信息
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.CreateNoWindow = true;          //不显示程序窗口
                StringBuilder output = new StringBuilder();
                StringBuilder error = new StringBuilder();

                using (AutoResetEvent outputWaitHandle = new AutoResetEvent(false))
                using (AutoResetEvent errorWaitHandle = new AutoResetEvent(false))
                {
                    process.OutputDataReceived += (sender, e) =>
                    {
                        if (e.Data == null)
                        {
                            outputWaitHandle.Set();
                        }
                        else
                        {
                            output.AppendLine(e.Data);
                        }
                    };
                    process.ErrorDataReceived += (sender, e) =>
                    {
                        if (e.Data == null)
                        {
                            errorWaitHandle.Set();
                        }
                        else
                        {
                            error.AppendLine(e.Data);
                        }
                    };

                    process.Start();

                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    if (process.WaitForExit(timeout) &&
                        outputWaitHandle.WaitOne(timeout) &&
                        errorWaitHandle.WaitOne(timeout))
                    {
                        // Process completed. Check process.ExitCode here.
                    }
                    else
                    {
                        // Timed out.
                        SaveLog("RunDOSCmd output--> Timeout...");
                    }
                    SaveLog("RunDOSCmd output-->" + output.ToString());
                    errors = error.ToString();
                    return output.ToString();
                }
            }
        }

        private bool PostJsonToMES()
        {
            SaveLog("Start to upload MES info...");

            bool result = false;
            try
            {
                ///!序列化Json 配置，忽略值为null字段
                JsonSerializerSettings setting = new JsonSerializerSettings();
                JsonConvert.DefaultSettings = new Func<JsonSerializerSettings>(() =>
                {
                    setting.DateFormatHandling = DateFormatHandling.MicrosoftDateFormat;
                    setting.DateFormatString = "yyyy-MM-dd HH:mm:ss";                                       //!日期类型默认格式化处理
                    setting.NullValueHandling = NullValueHandling.Ignore;                                   //!空值处理,忽略空值
                    return setting;
                });
                string CurrentMES = JsonConvert.SerializeObject(mesPhases, Formatting.Indented, setting);   //!MES Json数据
                SaveLog($"MES Info:\n{CurrentMES}");

                if (Global.TESTMODE.ToLower() == "debug") //debug模式下不上传测试结果
                {
                    return true;
                }
                mesUrl = $"http://{Global.MESIP}:{Global.MESPORT}/api/2/serial/{SN}/station/{Global.MESSTATIONNAME}/info";
                var client = new HttpClient();
                StringContent content = new StringContent(CurrentMES, Encoding.UTF8, "application/json");
                HttpResponseMessage httpResponse = client.PostAsync(mesUrl, content).GetAwaiter().GetResult();
                if (httpResponse.IsSuccessStatusCode)
                {
                    SaveLog($"MESinfo Upload Pass.");
                    result = true;
                }
                else
                {
                    error_code = "JsonUpload";
                    error_details = "Upload MES";
                    SaveLog($"MESinfp Upload Fail.Response code:{httpResponse.StatusCode}");
                }
                string responseBody = httpResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                SaveLog("MES responseBody:" + responseBody);
            }
            catch (Exception ex)
            {
                SaveLog("UploadJsonToMESException:" + ex.ToString());
            }
            return result;
        }

        /// <summary>
        /// 用反射的方式修改实例的变量值。给变量赋值。
        /// </summary>       
        public void SetMESInfo(MesPhases mesPhases, string varName, string varNewValue)
        {
            try
            {
                if (!string.IsNullOrEmpty(varName))
                {
                    Type myType = typeof(MesPhases);
                    FieldInfo myFieldInfo = myType.GetField(varName, BindingFlags.Public | BindingFlags.Instance);
                    if (myFieldInfo != null)
                    {
                        myFieldInfo.SetValue(mesPhases, varNewValue);
                    }
                    SaveLog($"Set MES variable:{varName}={varNewValue}");
                }
            }
            catch (Exception ex)
            {
                SaveLog(ex.ToString(), 2);
                //throw;
            }
        }

        #region WebSwitch Outlet ON/Off PowerCycle
        public async Task<bool> GetStatus(int index)
        {
            var switchInfo = await c.GetSwitchInfo();
            if (switchInfo == null || switchInfo.Outlets.Length == 0)
            {
                SaveLog($"Get Outlet {index} Status FAIL! ");
                return false;
            }
            SaveLog($"Get Outlet {index} Status: " + switchInfo.Outlets[index - 1].ToDetailsString());
            return switchInfo.Outlets[index - 1].IsOn;
        }

        public bool PowerCycleOutlet(int index)
        {
            bool rReturn = false;
            if (GetStatus(index).GetAwaiter().GetResult())
            {
                rReturn = c.CycleOutlet(index).GetAwaiter().GetResult();
            }
            else
            {
                bool rReturnoff = c.SetOutlet(index, false).GetAwaiter().GetResult();
                Thread.Sleep(1000);
                bool rReturnon = c.SetOutlet(index, true).GetAwaiter().GetResult();
                rReturn = rReturnoff && rReturnon;
            }
            return rReturn;
        }

        private bool Power_OnOff(int index, bool desiredState)
        {
            return c.SetOutlet(index, desiredState).GetAwaiter().GetResult();
        }
        #endregion

        [DllImport("kernel32.dll")]
        public static extern uint WinExec(string path, uint uCmdShow);
        /// <summary>
        /// 每秒Ping IP地址一次，ping通立即返回true，超过times后返回失败
        /// </summary>
        private bool PingIP(string address, int times)
        {
            try
            {
                if (IPAddress.TryParse(address, out IPAddress ipAdd))                             //确定一个字符串是否是有效的IP地址。
                {
                    WinExec("arp -d", 0);
                    Thread.Sleep(1000);
                    for (int i = times; i > 0; i--)
                    {
                        var pingReply = Ping(address);
                        if (pingReply.Status == 0)
                        {
                            SaveLog($"Ping {address},来自 {pingReply.Address} 的回复：字节={pingReply.Buffer.Length} 时间={pingReply.RoundtripTime} TTL={pingReply.Options.Ttl} {pingReply.Status}");
                            break;
                        }
                        else
                        {
                            SaveLog($"ping {address} ：{pingReply.Status}");
                            if (i == 1)
                            {
                                SaveLog($"ping {address} ：失败！！！！！");
                                return false;
                            }
                        }
                    }
                }
                else
                {
                    SaveLog($"{address} IP 地址无效");
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                SaveLog(ex.ToString());
                return false;
                throw;
            }

        }
        public PingReply Ping(string address)
        {
            Ping ping = null;
            try
            {
                ping = new Ping();
                return ping.Send(address);
            }
            finally
            {
                if (ping != null)
                {
                    // 2.0 下ping 的一个bug，需要显示转型后释放
                    IDisposable disposable = ping;
                    disposable.Dispose();
                    ping.Dispose();
                }
            }
        }

        private void TimerCallBack(object stateInfo)
        {
            //AutoResetEvent autoEvent = (AutoResetEvent)stateInfo;
            try
            {
                if (startFlag)
                {
                    sec++;
                    TimeSpan ts = new TimeSpan(0, 0, sec);
                    string ss = ts.Hours.ToString().PadLeft(2, '0') + ":" + ts.Minutes.ToString().PadLeft(2, '0') + ":" + ts.Seconds.ToString().PadLeft(2, '0');
                    SetLables(lb_testTime, ss, true);
                    // timer.Change(TimeSpan.FromSeconds(1), Timeout.InfiniteTimeSpan);

                }
            }
            catch (Exception ex)
            {
                SaveLog(ex.ToString());
                throw;
            }
        }

        public void SaveLog(string log, int type = 1)
        {
            try
            {
                lock (wLock)
                {
                    using (StreamWriter sw = new StreamWriter(this.cellLogPath, true, Encoding.Default))
                    {
                        sw.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} - {log}");
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void lb_testName_DoubleClick(object sender, EventArgs e)
        {
            if (!startFlag || Sequences[seqNo].IsTestFinished)
            {
                try
                {
                    System.Diagnostics.Process.Start(reNameCellLogPath);
                }
                catch (Exception)
                {
                    return;
                }
            }
            else
            {
                try
                {
                    System.Diagnostics.Process.Start(cellLogPath);
                }
                catch (Exception)
                {
                    return;
                }
            }
        }
    }
}
