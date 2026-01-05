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
using DocumentFormat.OpenXml.Drawing.Charts;

namespace AutoTestSystem.Equipment.Teach
{
    public class MultiTestController : TeachBase, IDisposable
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


        public int SegmentCount { get; private set; } = 2;

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
                dut.testUnit.InitializeStations(SegmentCount);
            }

            Start();

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
            nextLoadIndex = (nextLoadIndex + 1) % UnitsOnDisk.Count;

            controllerRunning = true;
            controllerThread = new Thread(ControllerLoop) { IsBackground = true };
            controllerThread.Start();

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
                            bool ret = ToNextStation();
                            if (!ret)
                            {
                                return;
                            }
                            //LogMessage($"EndDUT({nextLoadIndex})");
                            //if (!EndDUT())
                            //{
                            //    MessageBox.Show("EndDUT failed or timed out. Process interrupted. LoadNewDUT will not proceed.");
                            //    break;
                            //}
                            //LogMessage($"LoadNewDUT({nextLoadIndex})");
                            //LoadNewDUT();
                            
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

        private bool CanRotate()
        {
            if (!controllerRunning) return false;
            var active = UnitsOnDisk.Where(u => u.testUnit.IsActive);

            return active.All(u => u.testUnit.IsCurrentStationCompleted);
        }

        private bool ToNextStation()
        {
            //double currAngle = CurrentAngle;
            //int currIdx = MotionSegments.FindIndex(seg =>
            //{
            //    var motion = seg.Motions.Values.FirstOrDefault() as MotorMotion;
            //    return motion != null && Math.Abs(motion.Position - currAngle) < 1e-3;
            //});

            //int nextPhys = (currIdx + 1) % MotionSegments.Count;
            //statusForm.RotateTo(GetStationAngle(nextPhys)/*, _rotationDir*/);
            //bool MoveRet = ExecuteSegment(MotionSegments[nextLoadIndex]);
            //if(MoveRet == false)
            //{

            //    //GlobalNew.ShowMessage( "ExecuteSegment Fail.Please terminate the process and reinitialize.", "錯誤", MessageBoxIcon.Error);
            //    foreach (var u in UnitsOnDisk.Where(u => u.testUnit.IsActive))
            //    {
                    
            //        u.testUnit.ShowStatus = "Move Error";

            //    }
            //    AppendFormLog($"[RotateToNextStation]ExecuteSegment Fail.Please terminate the process and reinitialize.");
            //    return false;
            //}

            foreach (var u in UnitsOnDisk.Where(u => u.testUnit.IsActive))
            {
                u.LogMessage($"DUT is transferred from Station {u.testUnit.CurrentStationIndex} to Station {u.testUnit.CurrentStationIndex + 1}.");
                u.testUnit.CurrentStationIndex = (u.testUnit.CurrentStationIndex + 1) % SegmentCount;
                
            }

            foreach (var u in UnitsOnDisk.Where(u => u.testUnit.IsActive))
                u.testUnit.NotifyRotationDone();

            return true;
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

}
