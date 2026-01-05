using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AutoTestSystem.Base;
using AutoTestSystem.DAL;
using static AutoTestSystem.BLL.Bd;

namespace AutoTestSystem.Equipment.ControlDevice
{
    class GPP3060: ControlDeviceBase
    {

        [Category("Params"), Description("Select Comport"), TypeConverter(typeof(ComportList))]
        public string PortName { get; set; }

        [Category("Params"), Description("Set BaudRate")]
        public int baudrate { get; set; }


        [Category("Version Check"), Description("System Command")]
        public string QueryVersionCommand { get; set; } = "*IDN?";

        [Category("Version Check"), Description("Check System Command")]
        public string VersionCheckString { get; set; } = "";

        [Category("Reset"), Description("Error info clear")]
        public string ClearErrInfo { get; set; } = "*CLS";

        [Category("Reset"), Description("Reset Equipment")]
        public string ResetEquipment { get; set; } = "*RST";


        [Category("Terminal Check"), Description("Set Terminal Output")]
        public string SetTerminalOutput { get; set; } = "ROUTe:TERMinals REAR";

        [Category("Terminal Check"), Description("Query Terminal Output")]
        public string QueryTerminalOutput { get; set; } = "ROUTe:TERMinals?";

        [Category("Terminal Check"), Description("Check Terminal Output")]
        public string CheckTerminal { get; set; }

        [Category("Work mode"), Description("Queries CH1 work mode")]
        public string SetWorkmode { get; set; } = "LOAD1:CC ON";

        [Category("Work mode"), Description("Queries CH1 work mode")]
        public string QueryWorkmode { get; set; } = ":MODE1?";

        [Category("Work mode"), Description("Queries CH1 work mode")]
        public string CheckWorkmode { get; set; } = "CC";


        [Category("Set Channel"), Description("Queries CH1 work mode")]
        public string SetChannel { get; set; } = ":OUTPut1:STATe OFF";

        [Category("Set Channel"), Description("Queries CH1 Channel")]
        public string QueryChannel { get; set; } = ":OUTPut1:STATe?";

        [Category("Set Channel"), Description("Queries CH1 Channel")]
        public string CheckChannel { get; set; } = "OFF";

        Comport DeviceComport = null;
       
        
        public GPP3060()
        {
            baudrate = 115200;
        }



        public override bool Init(string strParamInfo)
        {

            if (string.IsNullOrEmpty(PortName))
            {
                LogMessage("NO COM Port Name", MessageLevel.Error);
                //MessageBox.Show("NO COM Port Name", "Warning!!!");
                return false;
            }
            SerialConnetInfo DevieCOMinfo = new SerialConnetInfo { PortName = PortName, BaudRate = baudrate };
            DeviceComport = new Comport(DevieCOMinfo);

            if (!DeviceComport.OpenCOM_CHK())
            {
                LogMessage("Init COM Port Fail", MessageLevel.Info);
                return false;
            }
            else
                LogMessage("Init COM Port Pass", MessageLevel.Info);

            SetTimeout(8000);

            if (!CheckCommand(QueryVersionCommand, VersionCheckString))
            {
                return false;
            }

            SEND(ClearErrInfo);
            LogMessage($"Send {ClearErrInfo}", MessageLevel.Info);
            SEND(ResetEquipment);
            LogMessage($"Send {ResetEquipment}", MessageLevel.Info);


            
            return true;

        }

        public override bool CheckParam()
        {
            if( !string.IsNullOrEmpty(QueryTerminalOutput))
            {
                if (!CheckCommand(QueryTerminalOutput, CheckTerminal))
                {
                    if (SetTerminalOutput != "")
                    {
                        LogMessage($"Send {SetTerminalOutput}", MessageLevel.Info);
                        SEND(SetTerminalOutput);
                    }
                    if (!CheckCommand(QueryTerminalOutput, CheckTerminal))
                    {
                        LogMessage($"Check {QueryTerminalOutput} Fail", MessageLevel.Info);
                        return false;
                    }
                }
            }

            if (!string.IsNullOrEmpty(QueryWorkmode))
            {
                if (!CheckCommand(QueryWorkmode, CheckWorkmode))
                {
                    if (SetWorkmode != "")
                    {
                        LogMessage($"Send {SetWorkmode}", MessageLevel.Info);
                        SEND(SetWorkmode);
                    }
                    if (!CheckCommand(QueryWorkmode, CheckWorkmode))
                    {
                        LogMessage($"Check {QueryWorkmode} Fail", MessageLevel.Info);
                        return false;
                    }
                }
            }
 
            if (!string.IsNullOrEmpty(QueryChannel))
            {
                if (!CheckCommand(QueryChannel, CheckChannel))
                {
                    if (SetChannel != "")
                    {
                        LogMessage($"Send {SetChannel}", MessageLevel.Info);
                        SEND(SetChannel);
                    }
                    if (!CheckCommand(QueryChannel, CheckChannel))
                    {
                        LogMessage($"Check {QueryChannel} Fail", MessageLevel.Info);
                        return false;
                    }
                }
            }


            return true;
        }
        public bool CheckCommand(string ReadCmd, string Checkstring)
        {
            try
            {
                
                SEND(ReadCmd);
                Sleep(200);
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                string output = string.Empty;
                bool isTimeout = true;
                while (stopwatch.ElapsedMilliseconds < 8000)
                {
                    try
                    {
                        string message = string.Empty;

                        READ(ref message);

                        output += $"{message}";
                        if (output.Contains("\n"))
                        //if (output.Contains(Checkstring))
                        {
                            isTimeout = false;
                            break;
                        }

                        Thread.Sleep(5);
                    }
                    catch (Exception ex)
                    {
                        LogMessage(ex.Message, MessageLevel.Error);
                        break;
                    }
                }

                if (isTimeout == true)
                {
                    LogMessage($"Read is Timeout, Data is (\"{output}\")", MessageLevel.Error);
                    return false;
                }

                LogMessage($"Read {output}", MessageLevel.Info);

                if (string.IsNullOrEmpty(output))
                {
                    LogMessage($"Output is null - > Fail", MessageLevel.Error);
                    return false;
                }
                else
                {
                    if (!output.Contains(Checkstring))
                    {
                        LogMessage($"Check {output} contains {Checkstring} - > Fail", MessageLevel.Error);
                        return false;
                    }
                }

            }
            catch (Exception ex)
            {
                LogMessage($"Exception {ex.Message}", MessageLevel.Error);
                return false;
            }

            return true;
        }

        //public bool CheckCommand(string SetCmd,string ReadCmd,string Checkstring)
        //{
        //    try
        //    {
        //        if (SetCmd != "")
        //        {
        //            LogMessage($"Send {SetCmd}", MessageLevel.Info);
        //            SEND(SetCmd);
        //        }
        //        if (SetCmd == "*CLS" || SetCmd == "*RST")
        //            return true;

        //        Sleep(200);

        //        if (ReadCmd != "")
        //        {
        //            LogMessage($"Send {ReadCmd}", MessageLevel.Info);
        //            SEND(ReadCmd);
        //        }

        //        //Sleep(500);

        //        Stopwatch stopwatch = new Stopwatch();
        //        stopwatch.Start();
                
        //        string output = string.Empty;
        //        bool isTimeout = true;
        //        while (stopwatch.ElapsedMilliseconds < 8000)
        //        {
        //            try
        //            {
        //                string message = string.Empty;

        //                READ(ref message);

        //                output += $"{message}";
        //                //if (output.Contains("\n"))
        //                if (output.Contains(Checkstring))
        //                {
        //                    isTimeout = false;
        //                    break;
        //                }

        //                Sleep(1);
        //            }
        //            catch (Exception ex)
        //            {
        //                LogMessage(ex.Message, MessageLevel.Error);
        //                break;
        //            }
        //        }

        //        if(isTimeout == true)
        //        {
        //            LogMessage($"Read is Timeout, Data is (\"{output}\")", MessageLevel.Error);
        //            return false;
        //        }
                
        //        LogMessage($"Read {output}", MessageLevel.Info);

        //        if(string.IsNullOrEmpty(output))
        //        {
        //            LogMessage($"Output is null - > Fail", MessageLevel.Error);
        //            return false;                 
        //        }
        //        else
        //        {
        //            if (!output.Contains(Checkstring))
        //            {
        //                LogMessage($"Check {output} contains {Checkstring} - > Fail", MessageLevel.Error);
        //                return false;
        //            }
        //        }

        //    }
        //    catch(Exception ex)
        //    {
        //        LogMessage($"Exception {ex.Message}", MessageLevel.Error);
        //        return false;
        //    }

        //    return true;
        //}

        public override void OPEN()
        {
            DeviceComport.OpenCOM();
        }

        public override bool Status(ref string msg)
        {
            try
            {
                if (DeviceComport.SerialPort.IsOpen)
                {
                    msg = $"{DeviceComport.SerialPort.PortName}(OPEN)";
                    return true;
                }
                else
                {
                    msg = $"{DeviceComport.SerialPort.PortName}(CLOSE)";
                    return false;
                }
            }
            catch(Exception ex)
            {
                msg = $"{ex.Message}";
                return false;
            }

        }


public override bool UnInit()
        {
            if (DeviceComport == null)
                return false;
            DeviceComport.Close();

            return true;
        }

        public override bool SEND(string input)
        {

            DeviceComport.WriteLine(input);
            //MessageBox.Show("Arduino寫入完成:" + input);
            return true;
        }

        public override bool READ(ref string output)
        {
            
            output =DeviceComport.Read();
            //MessageBox.Show("Arduino讀取完成:" + output);
            if(string.IsNullOrEmpty(output))
                return false;
            return true;
        }

        public override void Dispose()
        {
            throw new NotImplementedException();
        }
        public override void ClearBuffer()
        {
            DeviceComport.cleanBuffer();
        }

        public override void SetTimeout(int time)
        {
            DeviceComport.SetReadTimeout(time);
        }

        public class ComportList : TypeConverter  //下拉式選單
        {
            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                string[] portNames = SerialPort.GetPortNames();
                if (portNames.Length > 0)
                {
                    return new StandardValuesCollection(portNames.ToArray());
                }
                else
                {
                    return new StandardValuesCollection(new int[] { });
                }
            }

            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                return true;

            }

        }


    }
}
