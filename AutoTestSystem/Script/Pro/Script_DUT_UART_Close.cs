using AutoTestSystem.Base;
using AutoTestSystem.DUT;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Diagnostics;
using static AutoTestSystem.BLL.Bd;
using System.ComponentModel;
using System.Drawing.Design;
using Manufacture;

namespace AutoTestSystem.Script
{
    internal class Script_DUT_UART_Close : ScriptDUTBase
    {
        string strOutData = string.Empty;

        [Category("Common Parameters"), Description("自訂顯示名稱")]
        public int ReadTimeOut { get; set; } = 3000;

        [Category("Common Parameters"), Description("自訂顯示名稱")]
        public int TotalTimeOut { get; set; } = 3000; 

        public override void Dispose()
        {
            //throw new NotImplementedException();
        }
        
        public override bool PreProcess()
        {
            
            return true;
        }
        
        public override bool Process(DUT_BASE DUTDevice,ref string output)
        {
            try
            {
                bool Ret = false;
                DUTDevice.SetTimeout(ReadTimeOut, TotalTimeOut);
                Ret = DUTDevice.UnInit();
                if(Ret == false)
                {
                    return false;
                }
            }catch(Exception ex)
            {
                LogMessage($"{ex.Message}",MessageLevel.Error);
                return false;
            }

            return true;
        }

        public override bool PostProcess()
        {
            return true;
        }
    }
}
