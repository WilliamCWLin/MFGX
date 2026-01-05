using Automation.BDaq;
using AutoTestSystem.Base;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static AutoTestSystem.BLL.Bd;
namespace AutoTestSystem.Equipment.IO
{
    class ADV_USB4751 : IOBase
    {
        private InstantDoCtrl instantDoCtrl1 = new InstantDoCtrl();
        private InstantDiCtrl instantDiCtrl1 = new InstantDiCtrl();
        byte portData;

        private string _strParamInfoPath;

        [Category("Parameter"), Description("Devices"), TypeConverter(typeof(InstantDiCtrlDeviceList))]
        public string DI_DeviceName
        {
            set; get;
        }

        [Category("Parameter"), Description("Devices"), TypeConverter(typeof(InstantDoCtrlDeviceList))]
        public string DO_DeviceName
        {
            set; get;
        }

        [Category("Parameter"), Description("Profile Path"), Editor(typeof(Manufacture.FileSelEditorRelPath), typeof(System.Drawing.Design.UITypeEditor))]
        public string Profile_path
        {
            get { return _strParamInfoPath; }
            set { _strParamInfoPath = value; }
        }

        public override void Dispose()
        {
            throw new NotImplementedException();
        }



        public override bool Init(string strParamInfo)
        {
            try
            {
                if (DI_DeviceName != "" && DI_DeviceName != null)
                {
                    if (instantDiCtrl1 == null)
                        instantDiCtrl1 = new InstantDiCtrl();

                    if (instantDiCtrl1.SupportedDevices.Count == 0)
                    {
                        LogMessage("SupportedDevices is empty. Init Fail", MessageLevel.Error);
                        return false;
                    }

                    if (instantDiCtrl1.SupportedDevices.Count == 1)
                    {
                        if (instantDiCtrl1.SupportedDevices[0].Description.Contains("Demo"))
                        {
                            LogMessage("SupportedDevices is Demo. Init Fail", MessageLevel.Error);
                            return false;
                        }
                    }

                    instantDiCtrl1.SelectedDevice = new DeviceInformation(DI_DeviceName);
                    string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Profile_path);
                    ErrorCode err = instantDiCtrl1.LoadProfile(fullPath);
                    if ((err >= ErrorCode.ErrorHandleNotValid) && (err != ErrorCode.Success))
                    {
                        LogMessage("Sorry ! Some errors happened, the error code is: " + err.ToString());
                    }
                    else
                    {
                        LogMessage($"DiCtrl_SelectedDevice {instantDiCtrl1.SelectedDevice.ToString()} and Load Profile {err.ToString()}");
                    }
                }
                else
                {
                    LogMessage("DI_DeviceName is empty. " + strParamInfo, MessageLevel.Error);
                    return false;
                }

                if (DO_DeviceName != "")
                {
                    if (instantDoCtrl1 == null)
                        instantDoCtrl1 = new InstantDoCtrl();

                    if (instantDoCtrl1.SupportedDevices.Count == 0)
                    {
                        LogMessage("SupportedDevices is empty.", MessageLevel.Error);
                        return false;
                    }
                    if (instantDoCtrl1.SupportedDevices.Count == 1)
                    {
                        if (instantDoCtrl1.SupportedDevices[0].Description.Contains("Demo"))
                        {
                            LogMessage("SupportedDevices is Demo. Init Fail", MessageLevel.Error);
                            return false;
                        }
                    }

                    instantDoCtrl1.SelectedDevice = new DeviceInformation(DO_DeviceName);
                    string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Profile_path);
                    ErrorCode err = instantDoCtrl1.LoadProfile(fullPath);
                    if ((err >= ErrorCode.ErrorHandleNotValid) && (err != ErrorCode.Success))
                    {
                        LogMessage("Sorry ! Some errors happened, the error code is: " + err.ToString());
                    }
                    else
                    {
                        LogMessage($"DoCtrl_SelectedDevice {instantDoCtrl1.SelectedDevice.ToString()}  and Load Profile {err.ToString()}");
                    }
                }
                else
                {
                    LogMessage("DO_DeviceName is empty. " + strParamInfo, MessageLevel.Error);
                    return false;
                }

                return true;
            }
            catch (Exception e)
            {
                LogMessage("Init Fail. " + e.Message, MessageLevel.Error);
                MessageBox.Show("Please check if the device is in use or if the driver is not installed and the device is disconnected.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
        public override bool Status(ref string msg)
        {
            try
            {
                if (instantDoCtrl1.Initialized)
                {
                    if (instantDoCtrl1.SupportedDevices.Any(node => node.Description == DO_DeviceName))
                    {
                        msg = $"{DO_DeviceName}(ON)";
                        return true;

                    }

                    msg = $"{DO_DeviceName}(Not Exist)";

                    return false;
                }
                else
                {
                    msg = $"{DO_DeviceName}(OFF)";
                    return false;
                }
            }
            catch (Exception ex)
            {
                msg = $"{DO_DeviceName}{ex.Message}";
                return false;
            }

        }

        public override bool InstantAI(string strParamIn, ref string Dataout)
        {
            throw new NotImplementedException();
        }
        public override bool GETIO(int portNum,int pos, ref bool status)
        {
            try
            {
                if (!instantDoCtrl1.Initialized)
                {
                    //Logger.Error("ADV_USB4751 No device be selected or device open failed!. ");
                    LogMessage("ADV_USB4751 No device be selected or device open failed!. ", MessageLevel.Error);
                    return false;
                }

                byte Data;
                ErrorCode err = ErrorCode.Success;
                err = instantDiCtrl1.Read(portNum, out Data);
                if (err != ErrorCode.Success)
                {
                    LogMessage("ADV_USB4751 GETIO Exception." + err.ToString(), MessageLevel.Error);
                    return false;
                }

                status = ((Data >> pos) & 0x1) == 1;
                return true;
            }
            catch (Exception ex)
            {
                LogMessage("ADV_USB4751 GETIO Exception." + ex.Message, MessageLevel.Error);
                return false;
            }

        }
        public override bool SETIO(int portNum, int bit, bool output)
        {
            if (!instantDoCtrl1.Initialized)
            {
                Logger.Error("ADV_USB4751 No device be selected or device open failed!. ");
                return false;
            }

            ErrorCode err = ErrorCode.Success;
            BitArray bits = new BitArray(new byte[] { portData });
            bits[bit] = output;
            byte[] bytes = new byte[1];
            bits.CopyTo(bytes, 0);
            portData = bytes[0];

            err = instantDoCtrl1.Write(portNum, portData);
            Logger.Info($"ADV_USB4751 SetIO bits = {bytes} portdata = {portData}  output = {output} bit = { bit}");
            if (err != ErrorCode.Success)
            {
                Logger.Error("ADV_USB4751 SetIO Fail. " + err.ToString());
                return false;
            }

            return true;
        }

        public override bool UnInit()
        {
            try
            {
                if (instantDoCtrl1 != null)
                {
                    if (instantDoCtrl1.SupportedDevices.Count == 0)
                    {
                        LogMessage("SupportedDevices is empty.", MessageLevel.Error);
                        return false;
                    }
                    if (instantDoCtrl1.SupportedDevices.Count == 1)
                    {
                        if (instantDoCtrl1.SupportedDevices[0].Description.Contains("Demo"))
                        {
                            LogMessage("SupportedDevices is Demo. UnInit Fail", MessageLevel.Error);
                            return false;
                        }
                    }

                    if (!instantDoCtrl1.SupportedDevices.Any(node => node.Description == DO_DeviceName))
                    {
                        LogMessage($"{DO_DeviceName} not exist", MessageLevel.Error);
                        return false;

                    }

                    if (!instantDoCtrl1.Initialized)
                    {
                        LogMessage("ADV_USB4751 instantDoCtrl1 is not Initialized!. ");
                        return false;
                    }

                    instantDoCtrl1.Cleanup();
                    instantDoCtrl1 = null;
                }
                else
                    return false;

                if (instantDiCtrl1 != null)
                {
                    if (instantDiCtrl1.SupportedDevices.Count == 0)
                    {
                        LogMessage("SupportedDevices is empty.", MessageLevel.Error);
                        return false;
                    }

                    if (instantDiCtrl1.SupportedDevices.Count == 1)
                    {
                        if (instantDiCtrl1.SupportedDevices[0].Description.Contains("Demo"))
                        {
                            LogMessage("SupportedDevices is Demo. UnInit Fail", MessageLevel.Error);
                            return false;
                        }
                    }

                    if (!instantDiCtrl1.SupportedDevices.Any(node => node.Description == DI_DeviceName))
                    {
                        LogMessage($"{DI_DeviceName} not exist", MessageLevel.Error);
                        return false;

                    }

                    if (!instantDiCtrl1.Initialized)
                    {
                        LogMessage("ADV_USB4751 instantDiCtrl1 is not Initialized!. ");
                        return false;
                    }

                    instantDiCtrl1.Cleanup();
                    instantDiCtrl1 = null;
                }
                else
                    return false;

                return true;
            }
            catch (Exception ex)
            {
                LogMessage("ADV_USB4751 Dispose Exception." + ex.Message);
                return false;
            }
        }

        public class InstantDoCtrlDeviceList : TypeConverter  //下拉式選單
        {
            public static readonly InstantDoCtrl DoCtrl = new InstantDoCtrl();
            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                if (DoCtrl.SupportedDevices.Count > 0)
                {
                    List<string> hwList = new List<string>();
                    foreach (DeviceTreeNode node in DoCtrl.SupportedDevices)
                    {
                        if (node.Description.Contains("Demo") == false)
                            hwList.Add(node.Description);
                    }

                    return new StandardValuesCollection(hwList.ToArray());
                }
                else
                {
                    return new StandardValuesCollection(new int[] { 0 });
                }
            }

            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                return true;
            }
        }

        public class InstantDiCtrlDeviceList : TypeConverter  //下拉式選單
        {
            public static readonly InstantDiCtrl DiCtrl = new InstantDiCtrl();
            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {

                if (DiCtrl.SupportedDevices.Count > 0)
                {
                    List<string> hwList = new List<string>();
                    foreach (DeviceTreeNode node in DiCtrl.SupportedDevices)
                    {
                        if (node.Description.Contains("Demo") == false && node.Description.Contains("4751"))
                            hwList.Add(node.Description);
                    }

                    return new StandardValuesCollection(hwList.ToArray());
                }
                else
                {
                    return new StandardValuesCollection(new int[] { 0 });
                }
            }

            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                return true;
            }
        }
    }
}
