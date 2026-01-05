using AutoTestSystem.Base;
using AutoTestSystem.Equipment.Teach;
using AutoTestSystem.Model;
using DocumentFormat.OpenXml.Vml.Office;
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
using static AutoTestSystem.MainForm;

namespace AutoTestSystem.Script
{
    internal class Script_Extra_RotateManage : Script_Extra_Base
    {
        private readonly Dictionary<string, object> _deviceInstances;

        [Category("Common Parameters"), Description("教導裝置選擇"), TypeConverter(typeof(RotaryControllerList))]
        public string DeviceSel { get; set; } = "";

        [Category("Control Action"), Description("對 DUT testUnit 執行的動作")]
        public RotationAction ActionItem { get; set; } = RotationAction.WaitForRotation;

        [Category("Select Segments of DelayTime"), Description("等待旋轉完成後，額外延遲毫秒 (只有選擇 WaitForRotation 時使用)")]
        public int SleepTime { get; set; } = 1000;

        public enum RotationAction
        {
            //[Description("標記當前站位完成 (Mark Station Complete)")]
            //MarkComplete,
            [Description("啟動圓盤及移動至初始工位")]
            Start,
            [Description("等待旋轉完成 (Wait For Rotation)")]
            WaitForRotation,
            [Description("跳過測工位 (Abort For Rotation)")]
            Abort
        }

        public override void Dispose()
        {
            // 無需實作
        }

        public override bool PreProcess()
        {
            if (string.IsNullOrEmpty(DeviceSel))
            {
                LogMessage("教導裝置 DeviceSel 不可為空白", MessageLevel.Error);
                return false;
            }
            return true;
        }

        public override bool Process(ref string strOutData)
        {
            // 1. 取得 RotaryTestController 實例
            if (!(GlobalNew.Devices[DeviceSel] is RotaryTestController controller))
            {
                LogMessage($"找不到名稱為 {DeviceSel} 的 RotaryTestController", MessageLevel.Error);
                return false;
            }
            
            // 2. 取得目前執行此腳本的 DUT
            var dut = HandleDevice;          // DUT_BASE
            var tu = dut.testUnit;          // TestUnit

            // 3. 根據 Action 執行對應操作
            switch (ActionItem)
            {
                //case RotationAction.MarkComplete:
                //    // 標記當前站位完成
                //    tu.MarkCurrentStationComplete();
                //    LogMessage($"Station: {tu.CurrentStationIndex} Mark Complete");
                //    break;
                case RotationAction.Start:
                    // 標記當前站位完成
                    bool ret = controller.InitialMove();

                    if (!ret)
                    {
                        LogMessage($"InitialMove fail");
                        return false;
                    }
                       
                    LogMessage($"Start Thread.Move to Intial Station");
                    break;
                case RotationAction.WaitForRotation:
                    tu.MarkCurrentStationComplete();
                    LogMessage($"Station: {tu.CurrentStationIndex} Mark Complete");

                    LogMessage($"Waiting for rotation to complete...");
                    // 同步等待
                    tu.WaitForRotationAsync().GetAwaiter().GetResult();
                    LogMessage($"Rotation complete, proceeding to next step.");
                    // 額外延遲
                    if (SleepTime > 0)
                    {
                        Thread.Sleep(SleepTime);
                    }
                    break;
                case RotationAction.Abort:
                    //controller.Stop();cz
                    //GlobalNew.g_HomeProcessSuccess = false;// 強制圓盤模式下按停止下次啟動需HOME
                    LogMessage($"*******Process Aborted.*******");
                    //GlobalNew.ShowMessage("Process Aborted.!Please Check the issue and Click the stop button", "錯誤", MessageBoxIcon.Error);
                    GlobalNew.g_shouldStop = true;

                    return false;
                    //GlobalNew.g_isRunning = false;
                    //break;
            }

            return true;
        }

        public override bool PostProcess()
        {
            return true;
        }

        // 供 PropertyGrid 顯示 RotaryController 下拉
        public class RotaryControllerList : TypeConverter
        {
            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                try
                {
                    var names = GlobalNew.Devices
                        .Where(kv => kv.Value is RotaryTestController)
                        .Select(kv => kv.Key)
                        .ToArray();
                    return new StandardValuesCollection(names);
                }
                catch
                {
                    return new StandardValuesCollection(new string[0]);
                }
            }

            public override bool GetStandardValuesSupported(ITypeDescriptorContext context) => true;
        }
    }
}
