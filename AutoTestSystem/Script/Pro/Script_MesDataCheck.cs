
using AutoTestSystem.Base;
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
    internal class Script_MesDataCheck : Script_Extra_Base
    {
        public override void Dispose()
        {
            //throw new NotImplementedException();
        }
        public override bool PreProcess()
        {

            return true;

        }
        public override bool Process(ref string output)
        {
            Dictionary<string, string> mesData = new Dictionary<string, string>();
            Dictionary<string, string> Out_mesData = new Dictionary<string, string>();
            int mes_count = 1;

            mesData = PopMESLog();

            foreach (var mes in mesData)
            {
                Out_mesData.Add($"No.{mes_count}", mes.Value);
                mes_count++;
            }

            output = JsonConvert.SerializeObject(Out_mesData, Formatting.Indented);

            return true;
        }
        public override bool PostProcess()
        {
            
            return true;

        }   
    }
}
