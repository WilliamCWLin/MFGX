using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Threading;

namespace AutoTestSystem.Script
{
    internal class Script_DelayTime_Pro : Script_Extra_Base
    {
        [Category("Params"), Description("DelayTime (e.g. 1000, 1s, 500ms)")]
        public string DelayTime { get; set; } = "500";

        public Script_DelayTime_Pro()
        {
            Description = "Delay";
            ShowItem = false;
        }

        public override void Dispose()
        {
        }

        public override bool PreProcess()
        {
            return true;
        }

        public override bool Process(ref string output)
        {

            int delay = ParseDelayTime(ReplaceProp(DelayTime));
            LogMessage($"Waiting {delay}ms..");
            Thread.Sleep(delay);

            var data = new Dictionary<string, object>
            {
                { "STATUS", "PASS" }
            };
            output = JsonConvert.SerializeObject(data);

            return true;
        }

        public override bool PostProcess()
        {
            return true;
        }

        /// <summary>
        /// 將 DelayTime 字串解析為毫秒數
        /// 支援格式：1000、1s、500ms
        /// </summary>
        private int ParseDelayTime(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return 0;

            input = input.Trim().ToLower();

            // 支援 "ms" 或 "s"
            if (input.EndsWith("ms"))
            {
                input = input.Replace("ms", "");
                if (int.TryParse(input, out int ms)) return ms;
            }
            else if (input.EndsWith("s"))
            {
                input = input.Replace("s", "");
                if (double.TryParse(input, out double sec)) return (int)(sec * 1000);
            }
            else
            {
                if (int.TryParse(input, out int ms)) return ms;
            }

            // 預設失敗回傳 0
            return 0;
        }
    }
}
