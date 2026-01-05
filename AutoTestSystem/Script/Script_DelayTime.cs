
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
    internal class Script_DelayTime : Script_Extra_Base
    {

        string strActItem = string.Empty;
        string strParam = string.Empty;
        [Category("Params"), Description("DelayTime")]
        public int DelayTime { get; set; }

        Delay delay_param = null;
        public override void Dispose()
        {
            //throw new NotImplementedException();
        }
        public override bool Action(object o)
        {
            Thread.Sleep(DelayTime);
            LogMessage($"Delay {DelayTime}(ms)");
            return true;
        }

        public override bool PreProcess(string ActionItem, string Paraminput)
        {

            strActItem = ActionItem;
            strParam = Paraminput;

            delay_param = JsonConvert.DeserializeObject<Delay>(strParam);

            return true;
        }
        public override bool Process()
        {
            Sleep(delay_param.DelayTime);
            
            return true;
        }
        public override bool PostProcess()
        {
            
            return true;

        }




        public class Delay
        {
           
            public int DelayTime { get; set; }
            

        }

    }
}
