
using LibUsbDotNet.Main;
using LibUsbDotNet;
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
using static AutoTestSystem.Script.Script_DUT_UART_DTM_Pro;

namespace AutoTestSystem.Script
{
    internal class Script_UsbDevice_Pro : Script_Extra_Base
    {
        string strOutData = string.Empty;

        [Category("Device Parameters"), Description("自訂顯示名稱")]
        public string VID { get; set; }

        [Category("Device Parameters"), Description("自訂顯示名稱")]
        public string PID { get; set; }

        [Category("Device Parameters"), Description("自訂顯示名稱")]
        public string REV { get; set; }

        public override void Dispose()
        {
            //throw new NotImplementedException();
        }

        public override bool PreProcess()
        {
            if (VID == null || VID == string.Empty)
            {
                LogMessage("VID can not be null.", MessageLevel.Error);
                return false;
            }

            if (PID == null || PID == string.Empty)
            {
                LogMessage("PID can not be null.", MessageLevel.Error);
                return false;
            }

            if (REV == null || REV == string.Empty)
            {
                LogMessage("REV can not be null.", MessageLevel.Error);
                return false;
            }

            return true;
        }

        public override bool Process(ref string output)
        {
            var UsbData = new Dictionary<string, object>
            {
                {"errorCode", "0"}
            };

            var usbDevices = UsbDevice.AllDevices;
            foreach (UsbRegistry usbDevice in usbDevices)
            {
                if (usbDevice.Vid == Convert.ToInt32(VID, 16) && 
                    usbDevice.Pid == Convert.ToInt32(PID, 16) && 
                    usbDevice.Rev == Convert.ToInt32(REV))
                {
                    UsbDeviceFinder MyUsbFinder = new UsbDeviceFinder(usbDevice.Vid, usbDevice.Pid);
                    UsbDevice MyUsbDevice = UsbDevice.OpenUsbDevice(MyUsbFinder);
                    string UsbVersion = MyUsbDevice.Info.Descriptor.BcdUsb.ToString("X");
                    UsbData.Add("version", UsbVersion);
                    MyUsbDevice.Close();
                    break;
                }
            }

            output = JsonConvert.SerializeObject(UsbData);
            LogMessage($"Read END:  {output}");
            strOutData = output;

            return true;
        }

        public override bool PostProcess()
        {
            string result = CheckRule(strOutData, Spec);

            if (result == "PASS")
                return true;
            else
                return false;
        }

    }
}
