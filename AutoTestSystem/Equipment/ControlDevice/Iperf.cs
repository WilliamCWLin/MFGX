using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AutoTestSystem.DAL;
using static AutoTestSystem.BLL.Bd;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using AutoTestSystem.Base;
using Newtonsoft.Json.Linq;
using OpenCvSharp;
using System.ComponentModel;

namespace AutoTestSystem.Equipment.ControlDevice
{
    class Iperf: ControlDeviceBase
    {
        DosCmd doscmd = new DosCmd();
        int waitTime = 0;
        string recvStr = "";
        string checkStr = "";
        string strOutData = string.Empty;
        string CMD = string.Empty;

        [Category("Common Parameters"), Description("自訂顯示名稱")]
        public string port { get; set; }

        public override bool SendNonblock(string input ,ref string output)
        {
            return true;
        }
        public override bool Send(string input,string strActItem)
        {
            return true;

        }

        public override bool READ(ref string output)
        {

            return true;
        }

        public override void Dispose()
        {
            throw new NotImplementedException();
        }

        public override bool Init(string strParamInfo)
        {
            if (port == null || port == string.Empty)
            {
                CMD = "iperf3.exe -s";
            }
            else
            {
                CMD = "iperf3.exe -s -p " + port;
               
            }
            bool result = doscmd.SendNonBlockCommand(CMD, 1000);
            LogMessage("[iperf] Init OK");
            return true;
        }

        public override bool UnInit()
        {
            CMD = "taskkill /F /IM iperf3.exe";
            bool result = doscmd.SendNonBlockCommand(CMD, 1000);
            return true;
        }

        public override void SetTimeout(int time)
        {
            waitTime = time;
        }
        public override void SetCheckstr(string str)
        {
            checkStr = str;
        }

        public string CreateDataString(Dictionary<string, object> data)
        {
            try
            {
                string jsonStr = JsonConvert.SerializeObject(data, Formatting.Indented);
                return jsonStr;
            }
            catch (Exception ex)
            {
                // 處理轉換錯誤
                return $"轉換為 JSON 字串時出現錯誤: {ex.Message}";
            }
        }

        public override bool SEND(string input)
        {
            throw new NotImplementedException();
        }
    }
}

