using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static AutoTestSystem.BLL.Bd;

namespace AutoTestSystem.Script
{
    internal class Script_PassFail : Script_Extra_Base
    {
        [Category("Params"), Description("PASSFAIL (legacy, fixed output if needed)")]
        public bool PASSFAIL { get; set; } = false;

        [Category("Params"), Description("Pass rate percentage (0-100)")]
        public int PassRate { get; set; } = 50; // 預設 50% PASS

        // 每個執行緒各自的 Random，避免並行時回傳重複序列
        private static readonly ThreadLocal<Random> _rng =
            new ThreadLocal<Random>(() =>
                new Random(unchecked(Environment.TickCount * 31 + Thread.CurrentThread.ManagedThreadId)));

        public override void Dispose()
        {
        }

        public override bool PreProcess()
        {
            return true;
        }

        public override bool Process(ref string output)
        {
            // 限制 PassRate 範圍 0~100
            if (PassRate < 0) PassRate = 0;
            if (PassRate > 100) PassRate = 100;

            // 按設定機率決定是否 PASS
            bool pass = (_rng.Value.Next(100) < PassRate);
            output = pass ? "PASS" : "FAIL";
            return pass;
        }

        public override bool PostProcess()
        {
            return true;
        }
    }
}
