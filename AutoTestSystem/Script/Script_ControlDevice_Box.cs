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
    internal class Script_ControlDevice_Box : Script_ControlDevice_Base
    {

        string strActItem = string.Empty;
        string strParam = string.Empty;
        string strOutData = string.Empty;
        BoxPropterty BoxProp = null;

        [Category("Common Parameters"), Description("自訂顯示名稱"), Editor(typeof(JsonEditor), typeof(UITypeEditor))]
        public string Params { get; set; }

        public override void Dispose()
        {
            //throw new NotImplementedException();
        }

        public override bool Process()
        {
            BoxProp = JsonConvert.DeserializeObject<BoxPropterty>(Params);
            return Process(CtrlDevice);
        }

        public override bool PreProcess(string ActionItem, string Paraminput)
        {
            strActItem = ActionItem;
            strParam = Paraminput;
            BoxProp = JsonConvert.DeserializeObject<BoxPropterty>(strParam);

            return true;
        }
        public override bool Process(ControlDeviceBase ControlDevice)
        {
            strOutData = string.Empty;
            bool pass_fail = false;

            if (BoxProp.Mode == 0)
            {
                try
                {
                    ControlDevice.SetTimeout(BoxProp.Timeout);
                    ControlDevice.SEND(BoxProp.Send_Command);

                    Sleep(BoxProp.DelayTime);
                    LogMessage($"{Description} Send:  {BoxProp.Send_Command}. WaitTime: {BoxProp.DelayTime}");
                    Logger.Debug($"{Description} Send:  {BoxProp.Send_Command}. WaitTime: {BoxProp.DelayTime}");
                    
                    pass_fail = ControlDevice.READ(ref strOutData);
                    LogMessage($"{Description} Read Test Value:  {strOutData}");
                    Logger.Debug($"{Description} Read Test Value:  {strOutData}");
                    //Sleep(BoxProp.DelayTime);
                }
                catch (Exception ex)
                {
                    Logger.Warn(ex.Message + "Please Check COM Port Correct or NOT");
                }
            }
            else if (BoxProp.Mode == 1)
            {
                try
                {
                    if (!BoxProp.Send_Command.EndsWith(Environment.NewLine))
                    {
                        // 在文字後面加上換行符
                        BoxProp.Send_Command += Environment.NewLine;
                    }
                        
                    ControlDevice.SetTimeout(BoxProp.Timeout);
                    ControlDevice.SEND(BoxProp.Send_Command);
                    LogMessage($"{Description} Send:  {BoxProp.Send_Command}");
                    Logger.Debug($"{Description} Send:  {BoxProp.Send_Command}");


                    pass_fail = ControlDevice.READ(ref strOutData);
                    LogMessage($"{Description} Read Test Value:  {strOutData}");
                    Logger.Debug($"{Description} Read Test Value:  {strOutData}");

                    Sleep(BoxProp.DelayTime);
                }
                catch (Exception ex)
                {
                    Logger.Warn(ex.Message + "Please Check COM Port Correct or NOT");
                }
            }
            return pass_fail;
        }
        public override bool PostProcess(string strCheckSpec, ref string strDataout)
        {
            strDataout = strOutData;
            return true;
        }

        public class BoxPropterty
        {
            public int Mode { get; set; }
            public string Send_Command { get; set; }
            public string CheckString { get; set; }
            public int Timeout { get; set; }
            public int DelayTime { get; set; }
        }

    }
}
