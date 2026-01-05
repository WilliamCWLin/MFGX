
using AutoTestSystem.Base;
using Manufacture;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static AutoTestSystem.BLL.Bd;

namespace AutoTestSystem.Script
{
    internal class Script_ControlDevice_AudioRecord : Script_ControlDevice_Base
    {
        string strActItem = string.Empty;
        string strOutputPath = string.Empty;
        string strOutData = string.Empty;
        int RecordTime = 2000;

        [Category("Common Parameters"), Description("自訂顯示名稱")]
        public int RecTime { get; set; }

        [Category("Common Parameters"), Description("自訂顯示名稱")]
        public string SavePath { get; set; }

        public override void Dispose()
        {
            //throw new NotImplementedException();
        }

        public override bool Process()
        {
            if (SavePath == string.Empty)
            {
                LogMessage("SavePath = string.Empty.", MessageLevel.Error);
                return false;
            }
            else
            {
                RecordTime = RecTime;
                strOutputPath = SavePath;
                //Logger.Info($"SetBit:{strSetBit},Output:{bOutput}");
            }
            return Process(CtrlDevice);
        }
        public override bool PreProcess(string ActionItem, string Paraminput)
        {
            try
            {
                string[] InputParamArr = null;
                InputParamArr = Paraminput.Split(',');

                if (SavePath == string.Empty)
                {
                    LogMessage("Parameter format error.",MessageLevel.Error);
                    return false;
                }
                else
                {
                    RecordTime = RecTime;
                    strOutputPath = SavePath;
                    //Logger.Info($"SetBit:{strSetBit},Output:{bOutput}");
                }

                return true;
            }
            catch (Exception ex)
            {
                LogMessage($"Script_IO_Related_SetIO PreProcess Exception: {ex.Message}", MessageLevel.Error);
                return false;
            }

        }
        public override bool Process(ControlDeviceBase WaveDevice)
        {
            
            WaveDevice.PerformAction("Save", strOutputPath, true);//儲存WAV
            WaveDevice.PerformAction("Stop", RecordTime.ToString(), true); //設定計時器，在20秒後停止錄音

            // 開始錄音            
            bool ret = WaveDevice.PerformAction("Start", String.Empty, true);//開始錄音
            Thread.Sleep(RecordTime);


            var data = new Dictionary<string, object>
                        {
                            { "errorCode", 0 }
                        };
            strOutData = JsonConvert.SerializeObject(data);


            return true;
        }
        public override bool PostProcess(string strCheckSpec, ref string strDataout)
        {
            //string result = ProcessData(strOutData, strCheckSpec);
            //strDataout = strOutData;

            //if (result == "PASS" || strCheckSpec == "")
            //    return true;
            //else
            //    return false;
            return true;

        }

        public class Delay
        {

            public int DelayTime { get; set; }


        }

    }
}
