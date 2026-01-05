using AutoTestSystem.Base;
using AutoTestSystem.BLL;
using AutoTestSystem.DUT;
using AutoTestSystem.Model;
using AutoTestSystem.Script;

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Design;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

using System.Windows.Forms;
using System.Windows.Forms.Design;
using static AutoTestSystem.MainForm;
using static AutoTestSystem.Script.Script_Extra_MotionMovePath_Pro;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;

namespace AutoTestSystem.Equipment.Teach
{
    public class RotaryTestController : TeachBase, IDisposable
    {
        private readonly ConcurrentQueue<string> _uiLogQueue = new ConcurrentQueue<string>();
        private readonly object _ioLock = new object();
        private Dictionary<string, bool> _latestIo = new Dictionary<string, bool>();

        public void UiEnqueueLog(string msg) { _uiLogQueue.Enqueue(msg); }

        private Thread controllerThread;
        private bool controllerRunning = false;
        private readonly object rotationLock = new object();

        [Category("Common Parameters"), Description("Motor教導裝置選擇"), TypeConverter(typeof(Extra_TeachList))]
        public string DeviceSel { get; set; } = "";
        [Category("Common Parameters"), Description("IO教導裝置選擇"), TypeConverter(typeof(IOTeachList))]
        public string IODeviceSel { get; set; } = "";
        [JsonIgnore]
        [Browsable(false)]
        public List<DUT_BASE> UnitsOnDisk { get; set; } = new List<DUT_BASE>();

        [Category("Param"), Description("設定Active站")]
        public List<bool> ActiveList { get; set; } = new List<bool>();

        [Category("SetMuti GetIO Parameters"), Description("自訂顯示名稱"), Editor(typeof(Muti_IOEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public string MutiGetDI { get; set; } = "";

        [Category("SetMuti GetIO Parameters"), Description("自訂顯示名稱"), Editor(typeof(Muti_IOEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public string PauseDIs { get; set; } = "";
        //[Category("Param"), Description("UI旋轉方向")]
        //public bool Clockwise { get; set; } = false;

        //private int _rotationDir = +1;

        [Category("Select Motion Path")]
        [Description("編輯路徑清單")]
        [Editor(typeof(PathListEditor), typeof(UITypeEditor))]
        public List<MotionSegment> MotionSegments { get; set; } = new List<MotionSegment> { };

        [JsonIgnore]
        [Browsable(false)]
        public double CurrentAngle { get; private set; } = 0;



        [JsonIgnore]
        [Browsable(false)]
        public Func<bool> RotationEnableSignal { get; set; } = () => true;

        private int nextLoadIndex = 0;
        private RotaryStatusForm statusForm;

        // RotaryTestController.cs
        private void AppendFormLog(string msg)
        {
            var form = statusForm;
            if (form != null && !form.IsDisposed && form.IsHandleCreated)
            {
                try
                {
                    form.BeginInvoke((Action)(() =>
                    {
                        form.AppendLog(msg);
                    }));
                }
                catch (ObjectDisposedException) { }
                catch (InvalidOperationException) { }
            }
        }
        public int UiDrainLogs(List<string> buffer, int maxCount)
        {
            int n = 0; string s;
            while (n < maxCount && _uiLogQueue.TryDequeue(out s)) { buffer.Add(s); n++; }
            return n;
        }

        private double _currentPhysicalAngle;

        // 可選的對外屬性（只讀）
        public double CurrentPhysicalAngle
        {
            get { return Interlocked.CompareExchange(ref _currentPhysicalAngle, 0.0, 0.0); }
            private set { Interlocked.Exchange(ref _currentPhysicalAngle, value); }
        }

        public void UiSetLatestAngle(double angle)
        {
            Interlocked.Exchange(ref _currentPhysicalAngle, angle);
        }

        public double UiGetLatestAngle()
        {
            // 讀取可用 CompareExchange 技巧（不改值，僅取得目前值）
            return Interlocked.CompareExchange(ref _currentPhysicalAngle, 0.0, 0.0);
        }

        public void UiSetLatestIo(Dictionary<string, bool> dict)
        {
            lock (_ioLock) { _latestIo = dict; }
        }
        public Dictionary<string, bool> UiGetLatestIo()
        {
            lock (_ioLock) { return new Dictionary<string, bool>(_latestIo); }
        }
        public double GetStationAngle(int index)
        {
            if (index < 0 || index >= MotionSegments.Count) return 0;
            var motion = MotionSegments[index].Motions.Values.FirstOrDefault() as MotorMotion;
            return motion?.Position ?? 0;
        }

        public override bool Init(string jsonParam)
        {
            UnitsOnDisk.Clear();
            nextLoadIndex = 0;

            // 取得 MotionTeach 實例
            if (!GlobalNew.Devices.ContainsKey(DeviceSel))
            {
                MessageBox.Show("找不到指定的 DeviceSel: " + DeviceSel);
                return false;
            }

            var teach = GlobalNew.Devices[DeviceSel] as MotionTeach;
            if (teach == null)
            {
                MessageBox.Show("DeviceSel 不是 MotionTeach 型別");
                return false;
            }

            if (teach.Path == null || teach.Path.Segments == null)
            {
                MessageBox.Show("MotionTeach Path 或 Segments 為 null");
                return false;
            }

            this.MotionSegments = teach.Path.Segments.ToList();
            if (MotionSegments.Count == 0)
            {
                MessageBox.Show("MotionSegments 為空");
                return false;
            }

            // 初始化 DUTs
            var duts = GlobalNew.Devices.Values.OfType<DUT_BASE>().ToList();
            if (duts.Count == 0)
            {
                MessageBox.Show("沒有任何 DUT_BASE 裝置");
                return false;
            }

            foreach (var dut in duts)
            {
                UnitsOnDisk.Add(dut);
                dut.testUnit.InitializeStations(MotionSegments.Count);
            }

            return true;
        }


        public override bool UnInit()
        {
            AppendFormLog("UnInit");
            controllerRunning = false;
            controllerThread?.Join();
            controllerThread = null;
            UnitsOnDisk.Clear();
            LogMessage("RotaryTestController 已停止，資源釋放完成。");
            return true;
        }

        public bool Start()
        {
            if (controllerRunning) return false;

            AppendFormLog("[ControllerLoop] Load First New DUT");
            LoadNewDUT();
            nextLoadIndex = (nextLoadIndex + 1) % UnitsOnDisk.Count;

            controllerRunning = true;
            controllerThread = new Thread(ControllerLoop) { IsBackground = true };
            controllerThread.Start();

            if (statusForm == null || statusForm.IsDisposed)
            {
                statusForm = new RotaryStatusForm(this);
                statusForm.RotateTo(GetStationAngle(nextLoadIndex)/*, _rotationDir*/);
                
                statusForm.Show();
                GetCheckDIs();
                //AppendFormLog($"Start.RotationDir({Clockwise}),StationCount({MotionSegments.Count})");
            }

            return true;
        }

        public void ShowStatusForm()
        {
            if (statusForm == null || statusForm.IsDisposed)
            {
                statusForm = new RotaryStatusForm(this);
                //statusForm.RotateTo(GetStationAngle(nextLoadIndex));
            }

            statusForm.Show();

            //if (!statusForm.Visible)
            //{
            //    statusForm.Show();
            //}
            //else
            //{
            //    statusForm.BringToFront();
            //    statusForm.Activate();
            //}
        }

        public bool InitialMove()
        {
            AppendFormLog("Start");
            if (controllerRunning) return false;
            nextLoadIndex = ActiveList.FindIndex(active => active);
            if (nextLoadIndex == -1)
            {
                //GlobalNew.ShowMessage("[InitialMove] ExecuteSegment Fail.", "錯誤", MessageBoxIcon.Error);

                LogMessage("啟動失敗，沒有任何 Active 工位！");
                return false;
            }

            lock (rotationLock)
            {
                if(nextLoadIndex >= MotionSegments.Count )
                {
                    LogMessage("InitialMove FAIL.The count of ActiveList does not match the count of MotionSegments.！");
                    return false;
                }
                if (GetCheckDIs())
                {
                    if (!ExecuteSegment(MotionSegments[nextLoadIndex]))
                    {
                       //GlobalNew.ShowMessage("[InitialMove] ExecuteSegment Fail.", "錯誤", MessageBoxIcon.Error);

                        LogMessage($"[InitialMove] ExecuteSegment Fail.");
                        return false;
                    }
                }
                else
                {
                    MessageBox.Show($"[InitialMove] CheckDIs Fail.");
                    LogMessage($"[InitialMove] CheckDIs Fail.");
                    return false;
                }
                    
                LogMessage($"InitialMove to station {nextLoadIndex} Success.");
            }

            return true;
        }

        public bool Stop()
        {
            AppendFormLog("Stop");
            if (!controllerRunning) return false;
            controllerRunning = false;

            GlobalNew.g_shouldStop = true;

            foreach (var device in GlobalNew.Devices.Values)
            {
                var motor = device as MotionBase;
                if (motor != null) motor.EmgStop();
            }

            foreach (var u in UnitsOnDisk)
            {
                u.testUnit.NotifyRotationDone();
                u.testUnit.IsActive = false;
            }

            LogMessage("[Stop()] Wait Thread Stop.");

            // ★ 讓 UI 可以處理 BeginInvoke / 訊息，避免 Join 死鎖
            if (controllerThread != null)
            {
                int waited = 0;
                while (controllerThread.IsAlive && waited < 10000)
                {
                    Application.DoEvents();                // 允許 UI 處理訊息
                    if (controllerThread.Join(100)) break; // 100ms 一次
                    waited += 100;
                }
            }
            controllerThread = null;

            LogMessage("[Stop()] Success.Thread ControllerLoop Stopped.");
            MFGX.Instance.StartButtonImage = Properties.Resources.icons8_play_30;
            return true;
        }


        private void ControllerLoop()
        {
            while (controllerRunning)
            {
                bool canRotate;

                lock (rotationLock)
                {

                    canRotate = CanRotate();

                    if (canRotate)
                    {
                        LogMessage($"Start({nextLoadIndex})");
                        // 開始轉之前做一次確認有問題則中斷流程
                        if (GetCheckDIs())
                        {
                            LogMessage($"RotateToNextStation({nextLoadIndex})");
                            bool ret = RotateToNextStation();
                            if (!ret)
                            {
                                return;
                            }
                            LogMessage($"EndDUT({nextLoadIndex})");
                            if (!EndDUT())
                            {
                                MessageBox.Show("EndDUT failed or timed out. Process interrupted. LoadNewDUT will not proceed.");
                                break;
                            }
                            LogMessage($"LoadNewDUT({nextLoadIndex})");
                            LoadNewDUT();
                            
                            nextLoadIndex = (nextLoadIndex + 1) % UnitsOnDisk.Count;
                            LogMessage($"nextLoadIndex++ -> {nextLoadIndex}");
                        }
                        else
                        {

                            var form = statusForm;
                            if (form != null && !form.IsDisposed && form.IsHandleCreated)
                            {
                                try
                                {
                                    form.BeginInvoke((Action)(() =>
                                    {
                                        MessageBox.Show(
                                            "ControllerLoop Error. CheckDIs Fail.\n" +
                                            "請檢查 IO 異常點位，排除後重新復歸再運行。\n" +
                                            "Please check the abnormal IO points, reset after resolving, and then resume operation.",
                                            "警告 / Warning",
                                            MessageBoxButtons.OK,
                                            MessageBoxIcon.Warning
                                        );
                                    }));
                                }
                                catch { }
                            }



                            return;
                        }
                    }
                }
                Thread.Sleep(canRotate ? 100 : 50);
            }
        }

        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);
        private bool canTest()
        {
            if (!GetPauseDIs())
            {
                foreach (var u in UnitsOnDisk)
                {
                    if (u.testUnit.IsActive)
                    {
                        u.MainThread?.T_PausePro();
                        u.DutDashboard.UpdateLabelText(u.DutDashboard.DashBoardlabel_result, "Pause", Color.Red, Color.White, 20f);
                    }
                }

                // 等待 DI 恢復
                var pauseSw = Stopwatch.StartNew();
                int pauseTimeoutMs = 60000; // 最多等待 60 秒


                var warningForm = new PauseWarningForm();

                warningForm.Show();
                warningForm.BringToFront();
                warningForm.Activate();
                SetForegroundWindow(warningForm.Handle);

                bool PauserRet = true;
                while (!GetPauseDIs())
                {
                    int remaining = (pauseTimeoutMs - (int)pauseSw.ElapsedMilliseconds) / 1000;
                    warningForm.UpdateCountdown(remaining);

                    if (pauseSw.ElapsedMilliseconds > pauseTimeoutMs)
                    {
                        warningForm.Close();
                        LogMessage($"[ExecuteSegment] Pause DI timeout. Abort.");
                        PauserRet = false;
                        GlobalNew.g_shouldStop = true;
                        foreach (var u in UnitsOnDisk)
                        {
                            if(u.testUnit.IsActive)
                            {
                                u.MainThread?.T_ContinuePro();
                                u.DutDashboard.UpdateLabelText(u.DutDashboard.DashBoardlabel_result, "Abort", Color.Yellow, Color.Black, 20f);
                            }

                        }

                        
                        return false;
            
                    }

                    Thread.Sleep(100);
                }

                warningForm.Close();
                foreach (var u in UnitsOnDisk)
                {
                    if (u.testUnit.IsActive)
                    {
                        u.MainThread?.T_ContinuePro();
                        u.DutDashboard.UpdateLabelText(u.DutDashboard.DashBoardlabel_result, "Testing", Color.Yellow, Color.Black, 20f);
                    }

                    u.testUnit.ShowStatus = string.Empty;
                    
                }

                LogMessage($"[ExecuteSegment] Pause DI recovered. Re-executing Absolute_Move...");

                return true;
            }

            return true;
        }
        private bool CanRotate()
        {
            if (!controllerRunning) return false;
            var active = UnitsOnDisk.Where(u => u.testUnit.IsActive);

            return active.All(u => u.testUnit.IsCurrentStationCompleted);
        }

        private bool RotateToNextStation()
        {
            double currAngle = CurrentAngle;
            int currIdx = MotionSegments.FindIndex(seg =>
            {
                var motion = seg.Motions.Values.FirstOrDefault() as MotorMotion;
                return motion != null && Math.Abs(motion.Position - currAngle) < 1e-3;
            });

            int nextPhys = (currIdx + 1) % MotionSegments.Count;
            statusForm.RotateTo(GetStationAngle(nextPhys)/*, _rotationDir*/);
            bool MoveRet = ExecuteSegment(MotionSegments[nextLoadIndex]);
            if(MoveRet == false)
            {

                //GlobalNew.ShowMessage( "ExecuteSegment Fail.Please terminate the process and reinitialize.", "錯誤", MessageBoxIcon.Error);
                foreach (var u in UnitsOnDisk.Where(u => u.testUnit.IsActive))
                {
                    
                    u.testUnit.ShowStatus = "Move Error";

                }
                AppendFormLog($"[RotateToNextStation]ExecuteSegment Fail.Please terminate the process and reinitialize.");
                return false;
            }

            foreach (var u in UnitsOnDisk.Where(u => u.testUnit.IsActive))
            {
                u.LogMessage($"DUT is transferred from Station {u.testUnit.CurrentStationIndex} to Station {u.testUnit.CurrentStationIndex + 1}.");
                u.testUnit.CurrentStationIndex = (u.testUnit.CurrentStationIndex + 1) % MotionSegments.Count;
                u.LogMessage($"DUT Locate at {GetStationAngle(u.testUnit.CurrentStationIndex)}.");
            }

            foreach (var u in UnitsOnDisk.Where(u => u.testUnit.IsActive))
                u.testUnit.NotifyRotationDone();

            return true;
        }

        public bool EndDUT()
        {
            var entry = UnitsOnDisk[nextLoadIndex];
            string sn = entry?.DataCollection?.GetMoreProp("ProductSN") ?? "(noSN)";

            if (!ActiveList[nextLoadIndex] || !entry.testUnit.IsActive) return true;

            var sw = Stopwatch.StartNew();
            int timeoutMs = 1000000;

            while (sw.ElapsedMilliseconds < timeoutMs)
            {
                if (entry.testUnit.IsTestEnd)
                {
                    AppendFormLog($"[EndDUT] {entry?.Description}({sn})");
                    entry.testUnit.IsActive = false;
                    entry.DutDashboard.BeginInvoke((Action)(() =>
                    {
                        entry.DutDashboard.DashBoardDescription.BackColor = Color.Black;
                        entry.DutDashboard.DashBoardDescription.ForeColor = Color.Lime;
                    }));
                    Thread.Sleep(300);
                    return true;
                }

                if (GlobalNew.g_shouldStop)
                {
                    return false;
                }

                Thread.Sleep(500);
            }

            entry.LogMessage("EndDUT Timeout!!");
            entry.testUnit.IsActive = false;
            return false;
        }

        public bool LoadNewDUT()
        {
            var entry = UnitsOnDisk[nextLoadIndex];
            AppendFormLog($"[LoadDUT]({entry?.Description})");
            entry.testUnit.Reset();

            if (!ActiveList[nextLoadIndex])
                entry.DutDashboard.SetTestStatus(entry, TestStatus.IDLE);

            entry.testUnit.IsActive = ActiveList[nextLoadIndex];

            if (ActiveList[nextLoadIndex])
            {
                var t = Task.Run(() => entry.ForManageRunAsync());
                t.ContinueWith(_ =>
                {
                    entry.LogMessage("TestProcess End");
                    entry.LogMessage($"Station: {entry.testUnit.CurrentStationIndex} Done。 (MarkComplete)");
                    entry.testUnit.MarkTestEnd();

                    if (GlobalNew.g_shouldStop)
                    {
                        AppendFormLog($"[LoadNewDUT]g_shouldStop = true");
                        entry.DutDashboard.SetTestStatus(entry, TestStatus.FAIL);
                        entry.testUnit.ShowStatus = "ABORT";
                    }
                    else
                    {
                        string SN = entry.DataCollection.GetMoreProp("ProductSN");
                        if (string.IsNullOrEmpty(SN))
                        {
                            entry.DutDashboard.SetTestStatus(entry, TestStatus.IDLE);
                            entry.testUnit.IsSkip = true;
                            entry.testUnit.ShowStatus = "SKIP";
                        }
                        else
                        {
                            string errorcode = entry.DataCollection?.GetMoreProp("Failitem") ?? "";
                            entry.DutDashboard.SetTestStatus(entry, entry.TestResult ? TestStatus.PASS : TestStatus.FAIL);
                            entry.testUnit.ShowStatus = entry.TestResult ? $"{SN}:PASS" : $"{SN}:{errorcode}";
                        }
                    }
                    entry.testUnit.IsTestEnd = true;
                    AppendFormLog($"[LoadNewDUT]ProcessEnd({entry?.Description}),{entry.testUnit.ShowStatus}");
                });
            }
            else
            {
                AppendFormLog($"Disable({entry?.Description})");
            }

            Thread.Sleep(100);
            entry.LogMessage($"[LoadNewDUT]TestThread started.");
            
            return true;
        }

        private bool ExecuteSegment(MotionSegment seg)
        {
            try
            {
                bool Ret = false;

                if (seg.MoveType == MoveType.Independent)
                {
                    var tasks = new List<Task>();
                    foreach (var kv in seg.Motions)
                    {
                        string axis = kv.Key;
                        Motion m = kv.Value;

                        if (GlobalNew.Devices.TryGetValue(axis, out var dev) &&
                            m is MotorMotion parameter &&
                            dev is MotionBase motionobj)
                        {
                            var t = Task.Run(() =>
                            {
                                if (!motionobj.Absolute_Move(parameter.Position, parameter.StartSpeed, parameter.MaxVel, parameter.Acceleration, parameter.Deceleration))
                                {
                                    motionobj.EmgStop();
                                    LogMessage($"絕對移動失敗，請重新復位原點", MessageLevel.Error);
                                    Ret = false;
                                    return;
                                }
                                
                                int status = 1;
                                double pos = 0;


                                var sw = System.Diagnostics.Stopwatch.StartNew(); // 開始計時
                                int timeoutMs = 30000; // 例如 30 秒

                                while (true)
                                {                                   
                                    motionobj.GetMotionStatus(ref status);
                                    AppendFormLog($"GetMotionStatus({status})°");
                                    motionobj.GetCurrentPos(ref pos);
                                    CurrentPhysicalAngle = pos;

                                    //if (statusForm != null && !statusForm.IsDisposed && statusForm.IsHandleCreated)
                                    {
                                        UiSetLatestAngle(pos);
                                    }

                                    AppendFormLog($"CurrentPhysicalAngle({pos})°");
                                    AppendFormLog($"GetCheckDIs Start");
                                    if (!GetCheckDIs())
                                    {
                                        motionobj.EmgStop();
                                        LogMessage($"[ExecuteSegment]Status:CheckDIs Fail.EmgStop!!!");
                                        AppendFormLog($"[ExecuteSegment]Status:CheckDIs Fail.EmgStop!!!");
                                        Ret = false;
                                        break;
                                    }
                                    if (!GetPauseDIs())
                                    {
                                        LogMessage($"[ExecuteSegment] Pause DI triggered. EmgStop and wait...");
                                        motionobj.EmgStop();

                                        // 等待 DI 恢復
                                        var pauseSw = Stopwatch.StartNew();
                                        int pauseTimeoutMs = 60000; // 最多等待 60 秒


                                        var warningForm = new PauseWarningForm();
   
                                        warningForm.Show();
                                        warningForm.BringToFront();
                                        warningForm.Activate();
                                        

                                        bool PauserRet = true;
                                        while (!GetPauseDIs())
                                        {
                                            int remaining = (pauseTimeoutMs - (int)pauseSw.ElapsedMilliseconds) / 1000;
                                            warningForm.UpdateCountdown(remaining);
                                            // ★ 新增：Stop 時立即跳出，避免 Join 等滿 60 秒
                                            if (GlobalNew.g_shouldStop)
                                            {
                                                LogMessage($"[ExecuteSegment] Stop requested during Pause wait.");
                                                PauserRet = false;
                                                break;
                                            }
                                            if (pauseSw.ElapsedMilliseconds > pauseTimeoutMs)
                                            {
                                                warningForm.Close();
                                                LogMessage($"[ExecuteSegment] Pause DI timeout. Abort.");
                                                PauserRet = false;
                                                break;
                                            }

                                            Thread.Sleep(100);
                                        }

                                        warningForm.Close();

                                        if (PauserRet == false)
                                            break;

                                        foreach (var u in UnitsOnDisk)
                                        {
                                            u.testUnit.ShowStatus = string.Empty;
                                        }


                                        LogMessage($"[ExecuteSegment] Pause DI recovered. Re-executing Absolute_Move...");

                                        // 重新執行移動命令
                                        if (!motionobj.Absolute_Move(parameter.Position, parameter.StartSpeed, parameter.MaxVel, parameter.Acceleration, parameter.Deceleration))
                                        {
                                            motionobj.EmgStop();
                                            LogMessage($"重新移動失敗，請重新復位原點", MessageLevel.Error);
                                            Ret = false;
                                            break;
                                        }

                                        // 重設計時器
                                        sw.Restart();
                                        continue; // 回到 while 迴圈繼續監控狀態
                                    }
 
                                    AppendFormLog($"GetCheckDIs End");
                                    if (status == 0)
                                    {
                                        double tolerance = 1.2;
                                        if (Math.Abs(pos - parameter.Position) <= tolerance)
                                        {
                                            LogMessage($"[ExecuteSegment] Move to {parameter.Position} Success.({pos}).");
                                            Ret = true;
                                            CurrentAngle = parameter.Position;
                                            AppendFormLog($"[ExecuteSegment] Rotate to {CurrentAngle}°");
                                            break;
                                        }
                                        else
                                        {
                                            LogMessage($"[ExecuteSegment] Position mismatch: pos={pos}, expected={parameter.Position}");
                                            motionobj.EmgStop();
                                            Ret = false;
                                            break;
                                        }

                                    }
                                    if (status == -99)
                                    {
                                        LogMessage($"[ExecuteSegment]Status:({status})");
                                        AppendFormLog($"[ExecuteSegment]Status:({status})");
                                        motionobj.EmgStop();
                                        Ret = false;
                                        break;
                                    }
                                    if (sw.ElapsedMilliseconds > timeoutMs)
                                    {
                                        double timeoutSec = sw.Elapsed.TotalSeconds;
                                        LogMessage($"[ExecuteSegment]Status: Timeout after {timeoutSec:F2} seconds");
                                        AppendFormLog($"[ExecuteSegment]Status: Timeout after {timeoutSec:F2} seconds");


                                        motionobj.EmgStop();
                                        Ret = false;
                                        break;
                                    }
                                    if (GlobalNew.g_shouldStop && !GlobalNew.g_recipesteprun)
                                    {
                                        motionobj.EmgStop();
                                        Ret = false;
                                        break;
                                    }
                                    Thread.Sleep(10);
                                }
                            });

                            tasks.Add(t);
                        }
                    }
                    Task.WaitAll(tasks.ToArray());
                }

                return Ret;
            }
            catch (Exception ex)
            {
                LogMessage($"執行段落「{seg.SegmentName}」時發生錯誤: {ex.Message}", MessageLevel.Error);
                return false;
            }
        }

        
        public bool GetCheckDIs()
        {
            if (!string.IsNullOrEmpty(MutiGetDI))
            {
                List<Script_IO_ControlTeach.IOData> dataList = JsonConvert.DeserializeObject<List<Script_IO_ControlTeach.IOData>>(MutiGetDI);
                bool Check_Done = false;

                if (IODeviceSel == "")
                {
                    return true;
                }

                if (GlobalNew.Devices.TryGetValue(IODeviceSel, out var device) && device is IOTeach iotech)
                {                  
                    var allStatus = iotech.GetAllInputStatusFromCards();


                    if (statusForm != null && !statusForm.IsDisposed && statusForm.IsHandleCreated)
                    {
                        UiSetLatestIo(allStatus);
                    }

                    Check_Done = true;

                    foreach (var item in dataList)
                    {
                        if (!allStatus.TryGetValue(item.IO_Name, out bool status))
                        {
                            LogMessage($"KeyName: {item.IO_Name} not found in IO map", MessageLevel.Error);
                            return false;
                        }
                     
                        if (status != bool.Parse(item.IO_Status))
                        {
                            LogMessage($"KeyName: {item.IO_Name}, Expected: {item.IO_Status}, Actual: {status}");
                            AppendFormLog($"[GetCheckDIs]CheckIO Fail.KeyName: {item.IO_Name}{iotech.DescribeSensorLocation(item.IO_Name)}, Expected: {item.IO_Status}, Actual: {status}");
                            Check_Done = false;
                            foreach (var u in UnitsOnDisk)
                            {
                                u.testUnit.ShowStatus = $"{item.IO_Name} Fail.Unexpected condition";
                            }
                            break;
                        }
                    }

                    if (!Check_Done)
                    {
                        GlobalNew.g_shouldStop = true;
                        LogMessage($"Check Abort DI Fail", MessageLevel.Error);
                        return false;
                    }

                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
                return true;
        }

        public bool GetPauseDIs()
        {
            if (!string.IsNullOrEmpty(PauseDIs))
            {
                List<Script_IO_ControlTeach.IOData> dataList = JsonConvert.DeserializeObject<List<Script_IO_ControlTeach.IOData>>(PauseDIs);
                bool Check_Done = false;

                if (IODeviceSel == "")
                {
                    return true;
                }

                if (GlobalNew.Devices.TryGetValue(IODeviceSel, out var device) && device is IOTeach iotech)
                {
                    var allStatus = iotech.GetAllInputStatusFromCards();
                    Check_Done = true;

                    foreach (var item in dataList)
                    {
                        if (!allStatus.TryGetValue(item.IO_Name, out bool status))
                        {
                            LogMessage($"KeyName: {item.IO_Name} not found in IO map", MessageLevel.Warn);
                            return false;
                        }

                        if (status != bool.Parse(item.IO_Status))
                        {
                            LogMessage($"KeyName: {item.IO_Name}, Expected: {item.IO_Status}, Actual: {status}");
                            AppendFormLog($"[GetCheckDIs]CheckIO Fail.KeyName: {item.IO_Name}{iotech.DescribeSensorLocation(item.IO_Name)}, Expected: {item.IO_Status}, Actual: {status}");
                            Check_Done = false;
                            foreach (var u in UnitsOnDisk)
                            {
                                u.testUnit.ShowStatus = $"{item.IO_Name} Fail.Unexpected condition";
                            }
                            break;
                        }
                    }

                    if (!Check_Done)
                    {
                        LogMessage($"Check Pause DI Fail", MessageLevel.Warn);
                        return false;
                    }

                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
                return true;
        }
        public override void Dispose() => UnInit();
        public override bool Show() => true;
        protected override string GetJsonParamString() => throw new NotImplementedException();
    }



public class PathListEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal; // 彈出對話框編輯
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            var editorService = provider.GetService(typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;
            if (editorService == null) return value;

            dynamic currentObject = context.Instance;
            MotionTeach Teachpath = null;
            List<string> Path_Names = new List<string>(); 

            Teachpath = (MotionTeach)GlobalNew.Devices[currentObject.DeviceSel];
            using (var editorForm = new MotionTeachForm(Teachpath))
            {
                if (editorForm.ShowDialog() == DialogResult.Cancel)
                {
                    return Teachpath.Path.Segments;
                }
            }

            return value;
        }


    }

    public class Muti_IOEditor : System.Drawing.Design.UITypeEditor
    {
        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            dynamic currentObject = context.Instance;
            // 檢查 IODeviceSel 是否為 null 或空字串
            if (string.IsNullOrEmpty(currentObject.IODeviceSel))
            {
                MessageBox.Show("請先選擇 IO 教導裝置 (IODeviceSel)！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return value;
            }
            using (var MutiIOTable = new MutiIOSelect(currentObject.IODeviceSel))
            {
                // 如果有現有的值，將其加載到表單中
                if (value != null)
                {
                    MutiIOTable.LoadDataGridViewFromJson(value.ToString());
                }

                var result = MutiIOTable.ShowDialog();
                string json = MutiIOTable.GetDataGridViewAsJson();
                return MutiIOTable.GetDataGridViewAsJson();
            }
        }


        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }
    }



}
