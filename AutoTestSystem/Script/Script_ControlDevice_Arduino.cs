using AutoTestSystem.Base;
using AutoTestSystem.Equipment.ControlDevice;
using Manufacture;
using MvCamCtrl.NET;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static AutoTestSystem.BLL.Bd;

namespace AutoTestSystem.Script
{
    internal class Script_ControlDevice_Arduino : Script_ControlDevice_Base
    {

        string strActItem = string.Empty;
        string strParam = string.Empty;
        string strOutData = string.Empty;
        Arduino arduino = null;

        [Category("Common Parameters"), Description("自訂顯示名稱"), Editor(typeof(JsonEditor), typeof(UITypeEditor))]
        public string Params { get; set; }

        public override void Dispose()
        {
            //throw new NotImplementedException();
        }

        public override bool Process()
        {
            arduino = JsonConvert.DeserializeObject<Arduino>(Params);
            return Process(CtrlDevice);
        }

        public override bool PreProcess(string ActionItem, string Paraminput)
        {

            strActItem = ActionItem;
            strParam = Paraminput;
            arduino = JsonConvert.DeserializeObject<Arduino>(strParam);


            return true;
        }
        public override bool Process(ControlDeviceBase ControlDevice)
        {
            strOutData = string.Empty;
            bool pass_fail = true;
            string ack_data = "no_value";
            string end_data = "no_value";

            if (arduino.Mode == 0)
            {
                try
                {
                    ControlDevice.SetTimeout(arduino.Timeout);
                    ControlDevice.SEND(arduino.Send_Command);
                    LogMessage($"Script_ControlDevice_Arduino Send:  {arduino.Send_Command}");
                    ControlDevice.READ(ref ack_data);
                    LogMessage($"Script_ControlDevice_Arduino Read Ack:  {ack_data}");
                    if (!ack_data.Contains("Ack"))
                    {
                        MessageBox.Show("Please Check Arduino: NO Ack", "Wrning");
                        return false;
                    }

                    pass_fail = ControlDevice.READ(ref strOutData);
                    LogMessage($"Script_ControlDevice_Arduino Read Test Value:  {strOutData}");
                    ControlDevice.READ(ref end_data);
                    LogMessage($"Script_ControlDevice_Arduino Read END:  {end_data}");
                    if (!end_data.Contains("END"))
                        return false;
                    Sleep(arduino.DelayTime);
                }
                catch (Exception ex) {

                    Logger.Warn(ex.Message +"Please Check COM Port Correct or NOT");


                }
            }
            return pass_fail;
        }
        public override bool PostProcess(string strCheckSpec, ref string strDataout)
        {
            //strDataout= ref strOutData;
            //string result = ProcessData(strOutData, strCheckSpec);

            strDataout =  strOutData;
            return true;

        }

        public class Arduino
        {
            public int Mode { get; set; }
            public string Send_Command { get; set; }
            public int Timeout { get; set; }

            public int DelayTime { get; set; }
        }

    }
}
