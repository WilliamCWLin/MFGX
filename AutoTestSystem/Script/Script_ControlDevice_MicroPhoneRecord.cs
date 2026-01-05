
using AutoTestSystem.Base;
using Manufacture;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static AutoTestSystem.BLL.Bd;

namespace AutoTestSystem.Script
{
    internal class Script_ControlDevice_MicroPhoneRecord : Script_ControlDevice_Base
    {
        string strActItem = string.Empty;
        string strOutputPath = string.Empty;
        string strOutData = string.Empty;
        int RecordTime = 2000;

        [Category("Common Parameters"), Description("自訂顯示名稱")]
        public string RecTime { get; set; }

        [Category("Common Parameters"), Description("自訂顯示名稱")]
        public string SavePath { get; set; }

        [Category("Common Parameters"), Description("自訂顯示名稱")]
        public int CheckSize { get; set; }

        public override void Dispose()
        {
            //throw new NotImplementedException();
        }

        public override bool Process()
        {
            //PushMoreData("SN", "123");

            string l_SavePath = ReplaceKeys(SavePath, "");

            bool ret = false;
            if (l_SavePath == string.Empty)
            {
                LogMessage("SavePath = string.Empty.", MessageLevel.Error);
                return false;
            }
            else
            {
                ret = CtrlDevice.Send("SetSaveFile", l_SavePath);
                if (!ret)
                {
                    return false;
                }
            }
            
            ret = CtrlDevice.Send("StartRecording", RecTime);

            if (File.Exists(l_SavePath))
            {
                FileInfo fileInfo = new FileInfo(l_SavePath);
                long fileSizeKB = fileInfo.Length / 1024; 
                int minimumSizeKB = CheckSize; 

                if (fileSizeKB > minimumSizeKB)
                {
                    var data = new Dictionary<string, object>
                    {
                        { "errorCode", 0 },{ "FileSizeKB", fileSizeKB }
                    };
                    strOutData = JsonConvert.SerializeObject(data);
                    LogMessage($"Data:{strOutData}", MessageLevel.Debug);
                }
                else
                {
                    var data = new Dictionary<string, object>
                    {
                        { "errorCode", -1 },{ "FileSizeKB", fileSizeKB }
                    };
                    strOutData = JsonConvert.SerializeObject(data);
                    LogMessage($"Data:{strOutData},FileSize<{minimumSizeKB}", MessageLevel.Error);
                }
            }
            else
            {
                var data = new Dictionary<string, object>
                    {
                        { "errorCode", -2 }
                    };
                strOutData = JsonConvert.SerializeObject(data);
                LogMessage($"File not Exists .Data:{strOutData}", MessageLevel.Error);
            }
            return ret;
        }


        public override bool PostProcess(string strCheckSpec, ref string strDataout)
        {



            string result = CheckRule(strOutData, strCheckSpec);
            strDataout = strOutData;

            if (result == "PASS" || strCheckSpec == "")
                return true;
            else
                return false;

        }

        public class Delay
        {

            public int DelayTime { get; set; }


        }

    }
}
