
using AutoTestSystem.Base;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static AutoTestSystem.BLL.Bd;

namespace AutoTestSystem.Script
{
    internal class Script_ControlDevice_ArduinoRecord : Script_ControlDevice_Base
    {
        string strActItem = string.Empty;
        string strOutputPath = string.Empty;
        string strOutData = string.Empty;
        int RecordTime = 2000;

        Delay delay_param = null;
        public override void Dispose()
        {
            //throw new NotImplementedException();
        }
        public override bool PreProcess(string ActionItem, string Paraminput)
        {
            try
            {
                string[] InputParamArr = null;
                InputParamArr = Paraminput.Split(',');

                if (InputParamArr.Length < 2)
                {
                    Logger.Error("Parameter format error.");
                    return false;
                }
                else
                {
                    RecordTime = Int32.Parse(InputParamArr[0]);
                    strOutputPath = InputParamArr[1];
                    //Logger.Info($"SetBit:{strSetBit},Output:{bOutput}");
                }

                return true;
            }
            catch (Exception ex)
            {
                Logger.Info($"Script_IO_Related_SetIO PreProcess Exception: {ex.Message}");
                return false;
            }

        }
        public override bool Process(ControlDeviceBase WaveDevice)
        {
            
            WaveDevice.PerformAction("Save", strOutputPath);//儲存WAV
            WaveDevice.PerformAction("Stop", RecordTime.ToString()); //設定計時器，在20秒後停止錄音

            // 開始錄音            
            WaveDevice.PerformAction("Start", String.Empty);//開始錄音
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
