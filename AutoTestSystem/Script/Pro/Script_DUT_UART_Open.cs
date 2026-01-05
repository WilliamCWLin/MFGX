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
using System.IO.Ports;
using Automation.BDaq;
using DocumentFormat.OpenXml.Spreadsheet;
using OpenCvSharp.LineDescriptor;
using System.Web.UI.WebControls;
using DocumentFormat.OpenXml.Drawing.Diagrams;

namespace AutoTestSystem.Script
{
    internal class Script_DUT_UART_Open : ScriptDUTBase
    {
        public enum SetMode
        {
            Custom,
            Default,
        }

        string strOutData = string.Empty;

        [Category("Common Parameters"), Description("自訂顯示名稱")]
        public int ReadTimeOut { get; set; } = 3000;

        [Category("Common Parameters"), Description("自訂顯示名稱")]
        public int TotalTimeOut { get; set; } = 3000;

        [Category("Comport"), Description("")]
        public SetMode P0_Mode { get; set; } = SetMode.Default;     // 預設波特率

        [Category("Comport"), Description("")]
        public int P1_BaudRate { get; set; }     // 預設波特率
        [Category("Comport"), Description("")]
        public Parity P2_Parity { get; set; } // 預設奇偶檢查
        [Category("Comport"), Description("")]
        public int P3_DataBits { get; set; }           // 預設資料位
        [Category("Comport"), Description("")]
        public StopBits P4_StopBits { get; set; } // 預設停止位

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

                string jsonConfig = $@"
                {{
                    ""BaudRate"": {P1_BaudRate},
                    ""Parity"": ""{P2_Parity}"",
                    ""DataBits"": {P3_DataBits},
                    ""StopBits"": ""{P4_StopBits}"",
                }}";

                if(P0_Mode == SetMode.Default)
                    Ret = DUTDevice.Init("");
                else
                    Ret = DUTDevice.Init(jsonConfig);

                if (Ret == false)
                {
                    return false;
                }
            }
            catch(Exception ex)
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
