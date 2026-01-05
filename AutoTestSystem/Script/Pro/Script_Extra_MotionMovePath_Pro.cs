
using AutoTestSystem.Base;
using AutoTestSystem.Equipment.Teach;
using AutoTestSystem.Model;
using Manufacture;
using NAudio.Wave;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Design;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static AutoTestSystem.BLL.Bd;

namespace AutoTestSystem.Script
{
    internal class Script_Extra_MotionMovePath_Pro : Script_Extra_Base
    {
        private readonly Dictionary<string, object> _deviceInstances;

        [Category("Common Parameters"), Description("教導裝置選擇"), TypeConverter(typeof(Extra_TeachList))]
        public string DeviceSel { get; set; } = "";

        [Category("Select Motion Path"), Description("路徑選擇"), TypeConverter(typeof(MotionPath_List))]
        public string Path { get; set; } = "";

        [Category("Select Segments of DelayTime"), Description("段落間延遲時間(Path選擇ALL才使用)")]
        public int SleepTime { get; set; } = 1000;

        [Category("Motion Parameters Setting for Timeout"), Description("Set Status TimeOut")]
        public int StatusTimeOut { get; set; } = 30000;

        public override void Dispose()
        {
            //throw new NotImplementedException();
        }
        public override bool PreProcess()
        {
            if (string.IsNullOrEmpty(DeviceSel))
            {
                LogMessage($"教導裝置DeviceSel不可為空白", MessageLevel.Error);
                return false;
            }
            if (string.IsNullOrEmpty(Path))
            {
                LogMessage($"Path路徑不可為空白", MessageLevel.Error);
                return false;
            }

            return true;
        }
        public override bool Process(ref string strOutData)
        {
            MotionTeach Device = null;

            Device = (MotionTeach)GlobalNew.Devices[DeviceSel];

            bool MoveRet = false;
            if (Path != "ALL")
            {
                var segment = Device.Path.Segments.FirstOrDefault(s => s.SegmentName == Path);
                MoveRet = ExecuteSegment(segment);
                if (!MoveRet)
                {
                    strOutData = "Move_Fail";
                    return false;
                }
            }
            else
            {
                // Perform other actions for "ALL"
                MoveRet = HandleAllSegments(Device.Path.Segments);
                if (!MoveRet)
                {
                    strOutData = "Move_Fail";
                    return false;
                }
            }
            
            strOutData = "Move_Done";
            return true;
        }
        private bool HandleAllSegments(IEnumerable<MotionSegment> segments)
        {
            bool Ret = false;
            Stopwatch stopwatch = Stopwatch.StartNew();
            foreach (var seg in segments)
            {
                LogMessage($"段落{seg.SegmentName}路徑移動--開始--時間{stopwatch.ElapsedMilliseconds / 1000.0}", MessageLevel.Info);
                Ret = ExecuteSegment(seg);
                if (Ret == false)
                    break;
                LogMessage($"段落{seg.SegmentName}路徑移動--完成--時間{stopwatch.ElapsedMilliseconds / 1000.0}", MessageLevel.Info);
                Sleep(SleepTime);
            }
            stopwatch.Stop();
            stopwatch.Reset();
            return Ret;
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

                        if (GlobalNew.Devices.TryGetValue(axis, out var dev) && m is MotorMotion parameter && dev is MotionBase motionobj)
                        {
                            var tcs = new TaskCompletionSource<bool>();
                            var t = Task.Run(() =>                           
                            {
                                //LogMessage($"#############Start Absolute_Move################## Elapsed Time:{stopwatch.ElapsedMilliseconds / 1000.0}", MessageLevel.Info);
                                if (!motionobj.Absolute_Move(parameter.Position, parameter.StartSpeed, parameter.MaxVel, parameter.Acceleration, parameter.Deceleration))
                                {
                                    // Handle move failure
                                    motionobj.EmgStop();
                                    LogMessage($"絕對移動失敗,請重新復位原點", MessageLevel.Error);
                                    tcs.SetResult(false); // Signal move failure
                                    return;
                                }
                                //LogMessage($"#############END Absolute_Move################## Elapsed Time:{stopwatch.ElapsedMilliseconds / 1000.0}", MessageLevel.Info);
                                int status = 1;
                                double pos = 0;
                                var Movestopwatch = Stopwatch.StartNew();
                                //LogMessage($"##################Start GetMotionStatus##################", MessageLevel.Info);
                                while (true)
                                {
                                    //LogMessage($"##################Start GetMotionStatus##################", MessageLevel.Info);
                                    motionobj.GetMotionStatus(ref status);
                                    motionobj.GetCurrentPos(ref pos);
                                    LogMessage($" Axis:{axis} Postion:{pos} Status:{status}");
                                    

                                    if (status == 0)//正常
                                    {
                                        //LogMessage($" {axis} Motion Done => Postion is {pos} Elapsed Time:{stopwatch.ElapsedMilliseconds / 1000.0} seconds");
                                        Ret = true;
                                        break;
                                    }
                                    if(status == -99)//運行異常
                                    {
                                        motionobj.EmgStop();
                                        //LogMessage($"{axis} Motion Aborted => Position is {pos} Elapsed Time:{stopwatch.ElapsedMilliseconds / 1000.0} seconds");
                                        break;
                                    }

                                    if (Movestopwatch.ElapsedMilliseconds > StatusTimeOut)
                                    {
                                        motionobj.EmgStop();
                                        LogMessage($"Status TimeOut", MessageLevel.Error);
                                        break;
                                    }
                                    Thread.Sleep(10);
                                }                             
                                //LogMessage($"##################END GetMotionStatus##################{status}", MessageLevel.Info);
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
                LogMessage($"執行段落「{seg.SegmentName}」時發生錯誤: {ex.Message}",MessageLevel.Error);
                return false;
                //MessageBox.Show($"執行段落「{seg.SegmentName}」時發生錯誤: {ex.Message}", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        public override bool PostProcess()
        {
            return true;

        }

        public class MotionPath_List : TypeConverter  //下拉式選單
        {
            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                try
                {
                    dynamic currentObject = context.Instance;
                    MotionTeach Teachpath = null;
                    List<string> Path_Names = new List<string>(); // 存储设备名称的变量

                    Teachpath = (MotionTeach)GlobalNew.Devices[currentObject.DeviceSel];
                    foreach (var name in Teachpath.Path.Segments)
                    {
                        Path_Names.Add(name.SegmentName);
                    }
                    Path_Names.Add("ALL");

                    return new StandardValuesCollection(Path_Names.ToArray());
                }catch
                {
                    return new StandardValuesCollection(new string[] { });
                }                        
            }

            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                return true;

            }
        }

    }
}
