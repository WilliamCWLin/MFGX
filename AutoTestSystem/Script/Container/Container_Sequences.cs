using AutoTestSystem.Base;
using AutoTestSystem.DUT;
using AutoTestSystem.Equipment.IO;
using AutoTestSystem.Equipment.Motion;
using AutoTestSystem.Model;
using Manufacture;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static OpenCvSharp.ML.DTrees;

namespace AutoTestSystem.Script
{
    public class Container_Sequences : Script_Container_Base, IDisposable
    {
        [Category("Params"), Description("RetryTimes")]
        public int RetryTimes { get; set; }

        [Category("Params"), Description("Cycles")]
        public int Cycles { get; set; }

        [Category("MultiTest"), Description("Multi thread lock use. Empty if not used")]
        public string Lock_Key { get; set; } = "";
        [Category("MultiTest"), Description("Multi thread lock use. Empty if not used")]
        public bool OnlyAction { get; set; } = false;
        [Category("Base"), Description("Item Display")]
        public bool isTestItem { set; get; } = false;

        private static readonly Dictionary<string, object> _lockObjects = new Dictionary<string, object>();


        private static readonly Dictionary<string, bool> ActionDoneFlags = new Dictionary<string, bool>();
        private static readonly Dictionary<string, ManualResetEventSlim> WaitEvents = new Dictionary<string, ManualResetEventSlim>();


        private static object GetLockObject(string key)
        {
            if (!_lockObjects.ContainsKey(key))
            {
                _lockObjects[key] = new object();

                ActionDoneFlags[key] = false;
                WaitEvents[key] = new ManualResetEventSlim(false);

            }
            return _lockObjects[key];
        }
        public Container_Sequences()
        {
            RetryTimes = 0;
            Cycles = 1;
            FailJump = "Break";
            //Description = "Seq";
        }

        public override void Dispose()
        {

        }

        public override bool PreProcess(string ActionItem, string Paraminput)
        {
            return true;
        }

        public override int Process(TreeNode tn, object Component)
        {
            object[] context = Component as object[];
            DUT_BASE Dut = null;
            if (context.Length > 2)
            {
                Dut = (DUT_BASE)context[2];

            }

            if (!string.IsNullOrEmpty(Lock_Key))
            {
                lock (GetLockObject(Lock_Key))
                {
                    if(OnlyAction)
                    {
                        if (!ActionDoneFlags[Lock_Key])
                        {
                            ActionDoneFlags[Lock_Key] = true;
                            // 第一個進來的執行緒執行治具動作
                            int ret = Run((TreeNode)tn, Component);
                            
                            WaitEvents[Lock_Key].Set(); // 通知其他執行緒可以繼續
                            return ret;
                        }

                        // 其他執行緒等待治具動作完成
                        WaitEvents[Lock_Key].Wait();
                        if (GlobalNew.g_shouldStop == true)
                            return 0;
                        return 1;
                    }
                    else
                    {
                        int ret = 0;
                        ret = Run((TreeNode)tn, Component);
                        return ret;                      
                    }
                }
            }
            else
            {
                int ret = 0;

                ret = Run((TreeNode)tn, Component);

                return ret;
            }
        }
        public override int Process(TreeNode tn)
        {
            int ret = 0;

            ret = Run((TreeNode)tn);

            return ret;
        }
        public override bool Process()
        {
            return true;
        }
        public override bool PostProcess(string strCheckSpec, ref string strDataout)
        {
            return true;
        }


        public static void ClearSyncProp()
        {
            ActionDoneFlags.Clear();
            foreach (var evt in WaitEvents.Values)
            {
                evt.Dispose(); // 釋放資源
            }
            WaitEvents.Clear();

            // 清空 lock 物件
            _lockObjects.Clear();

        }

    }
}
