using AutoTestSystem.Model;
using Manufacture;
using NAudio.Wave;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using static AutoTestSystem.BLL.Bd;
using static AutoTestSystem.Script.Script_DUT_Related;
using static System.ComponentModel.TypeConverter;

namespace AutoTestSystem.Script
{
    internal class Script_Extra_CountDownEvent : Script_Extra_Base
    {
        string l_strOutData = string.Empty;

        // 使用字典來存儲不同key對應的CountdownEvent實例
        static Dictionary<string, CountdownEvent> countdownEvents = new Dictionary<string, CountdownEvent>();
        static Dictionary<string, bool> timeoutFlags = new Dictionary<string, bool>();
        static Dictionary<string, CancellationTokenSource> cancellationTokenSources = new Dictionary<string, CancellationTokenSource>();
        static readonly object lockObj = new object();

        [Category("Condition"), Description("CountdownEvent的Key")]
        public string key { get; set; }

        [Category("Condition"), Description("等待超時時間（毫秒）")]
        public int timeout { get; set; }

        [Category("Condition"), Description("是否重新開始")]
        public bool resetMode { get; set; }

        public override void Dispose()
        {
            //throw new NotImplementedException();
        }

        public static void ResetAll(int count)
        {
            lock (lockObj)
            {
                foreach (var key in countdownEvents.Keys.ToList())
                {
                    countdownEvents[key].Reset(count);
                    timeoutFlags[key] = false;
                    cancellationTokenSources[key]?.Cancel();
                    cancellationTokenSources[key] = new CancellationTokenSource();
                }
            }
        }

        public override bool PreProcess()
        {
            l_strOutData = "";
            // 重置 CountdownEvent 並設置新的計數器值

            // 使用 lock 確保 CountdownEvent 的初始化是原子性的
            lock (lockObj)
            {
                if (!countdownEvents.ContainsKey(key))
                {
                    countdownEvents[key] = new CountdownEvent(GlobalNew.EnableDeviceCount);
                    timeoutFlags[key] = false;
                    cancellationTokenSources[key] = new CancellationTokenSource();
                }
                else if (countdownEvents[key].CurrentCount == 0 || resetMode)
                {
                    countdownEvents[key].Reset(GlobalNew.EnableDeviceCount);
                    timeoutFlags[key] = false;
                    cancellationTokenSources[key]?.Cancel();
                    cancellationTokenSources[key] = new CancellationTokenSource();
                }
            }

            return true;
        }

        public override bool Process(ref string strOutData)
        {
            if (resetMode)
                return true;

            // 檢查是否已經發生超時
            //lock (lockObj)
            //{
            //    if (timeoutFlags.ContainsKey(key) && timeoutFlags[key])
            //    {
            //        strOutData = "等待超時";
            //        return false;
            //    }
            //}

            // 減少計數器
            countdownEvents[key].Signal();

            // 等待計數器達到零，並設置超時時間
            bool signaled;


            try
            {
                if (timeout <= 0)
                {
                    signaled = countdownEvents[key].Wait(Timeout.InfiniteTimeSpan, cancellationTokenSources[key].Token);
                }
                else
                {
                    signaled = countdownEvents[key].Wait(timeout, cancellationTokenSources[key].Token);
                }
            }
            catch (OperationCanceledException)
            {
                strOutData = "等待被取消";
                return false;
            }


            lock (lockObj)
            { 
                if (timeoutFlags[key] == false && signaled)
                {
                    return true;
                }
            }

            if (!signaled)
            {
                lock (lockObj)
                {
                    //timeoutFlags[key] = true;

                    countdownEvents[key].Reset(GlobalNew.EnableDeviceCount);
                    timeoutFlags[key] = false;
                    cancellationTokenSources[key]?.Cancel();
                    cancellationTokenSources[key] = new CancellationTokenSource();
                }
                strOutData = "等待超時";

                return false;
            }

            lock (lockObj)
            {
                countdownEvents[key].Reset(GlobalNew.EnableDeviceCount);
                timeoutFlags[key] = false;
            }

            return true;
        }

        public override bool PostProcess()
        {
            return true;
        }

        // 在另一個緒中調用此方法來中斷等待的緒
        public static void CancelWait(string key)
        {
            lock (lockObj)
            {
                if (cancellationTokenSources.ContainsKey(key))
                {
                    cancellationTokenSources[key].Cancel();
                }
            }
        }

        public static void ClearAll()
        {
            lock (lockObj)
            {
                // 清除 CountdownEvent
                foreach (var evt in countdownEvents.Values)
                {
                    evt.Dispose(); // 釋放資源
                }
                countdownEvents.Clear();

                // 清除 Timeout 標記
                timeoutFlags.Clear();

                // 清除 CancellationTokenSource
                foreach (var cts in cancellationTokenSources.Values)
                {
                    cts.Cancel();
                    cts.Dispose();
                }
                cancellationTokenSources.Clear();
            }
        }
    }
}
