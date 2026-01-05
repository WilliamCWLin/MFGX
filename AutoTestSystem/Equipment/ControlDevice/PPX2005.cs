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
    class PPX2005: ControlDeviceBase
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

        [Category("Work mode"), Description("Set Output mode (CVHS:0 CCHS:1 CVLS:2 CCLS:3)")]
        public string SetWorkmode { get; set; } = ":OUTP:MODE 0";

        [Category("Work mode"), Description("Query Output mode")]
        public string QueryWorkmode { get; set; } = ":OUTP:MODE?";

        [Category("Work mode"), Description("Check Output mode")]
        public string CheckWorkmode { get; set; } = "0";

        [Category("Output Enable"), Description("Set Output Voltage and Current")]
        public string SetOutput { get; set; } = ":OUTP 0";

        [Category("Output Enable"), Description("Query Output Voltage and Current")]
        public string QueryOutput { get; set; } = ":OUTP?";

        [Category("Output Enable"), Description("Check Output Voltage and Current")]
        public string CheckOutput { get; set; } = "0";


        [Category("Current Range"), Description("Set Current Range")]
        public string SetCurrentRange { get; set; } = "MEASure:SCALar:CURRent:RANGe 2";

        [Category("Current Range"), Description("Query Current Range")]
        public string QueryCurrentRange { get; set; } = "MEASure:SCALar:CURRent:RANGe?";

        [Category("Current Range"), Description("Check Current Range")]
        public string CheckCurrentRange { get; set; } = "2";

        [Category("Output Voltage and Current"), Description("Set Output Voltage and Current")]
        public string SetVoltageCurrent { get; set; } = ":APPLY 0.0,0.0";

        [Category("Output Voltage and Current"), Description("Query Output Voltage and Current")]
        public string QueryVoltageCurrent { get; set; } = ":APPLY?";

        [Category("Output Voltage and Current"), Description("Check Output Voltage and Current")]
        public string CheckVoltageCurrent { get; set; } = "+0.000, +0.0000";

        Comport DeviceComport = null;
       
        
        public PPX2005()
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

            if (!string.IsNullOrEmpty(QueryOutput))
            {
                if (!CheckCommand(QueryOutput, CheckOutput))
                {
                    if (SetOutput != "")
                    {
                        LogMessage($"Send {SetOutput}", MessageLevel.Info);
                        SEND(SetOutput);
                    }
                    if (!CheckCommand(QueryOutput, CheckOutput))
                    {
                        LogMessage($"Check {QueryOutput} Fail", MessageLevel.Info);
                        return false;
                    }
                }
            }

            
            if (!string.IsNullOrEmpty(QueryCurrentRange))
            {
                if (!CheckCommand(QueryCurrentRange, CheckCurrentRange))
                {
                    if (SetCurrentRange != "")
                    {
                        LogMessage($"Send {SetCurrentRange}", MessageLevel.Info);
                        SEND(SetCurrentRange);
                    }
                    if (!CheckCommand(QueryCurrentRange, CheckCurrentRange))
                    {
                        LogMessage($"Check {QueryCurrentRange} Fail", MessageLevel.Info);
                        return false;
                    }
                }
            }

            if (!string.IsNullOrEmpty(QueryVoltageCurrent))
            {
                if (!CheckCommand(QueryVoltageCurrent, CheckVoltageCurrent))
                {
                    if (SetVoltageCurrent != "")
                    {
                        LogMessage($"Send {SetVoltageCurrent}", MessageLevel.Info);
                        SEND(SetVoltageCurrent);
                    }
                    if (!CheckCommand(QueryVoltageCurrent, CheckVoltageCurrent))
                    {
                        LogMessage($"Check {QueryVoltageCurrent} Fail", MessageLevel.Info);
                        return false;
                    }
                }
            }


            return true;
            }
            public override bool CommandCheckForScript(string ReadCmd, string Checkstring)
            {
              bool CMDResult = false;
                CMDResult = CheckCommand(ReadCmd, Checkstring);
                return CMDResult;
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
                    LogMessage($"Output is null - > Fail", MessageLevel.Debug);
                    return false;
                }
                else
                {
                    if (!output.Contains(Checkstring))
                    {
                        LogMessage($"Check {output} contains {Checkstring} - > Fail", MessageLevel.Debug);
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

        /*public override bool Init(string strParamInfo)
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

            if (!CheckCommand("", QueryVersionCommand, VersionCheckString))
                return false;

            if (!CheckCommand(ClearErrInfo, "", ""))
                return false;

            if (!CheckCommand(ResetEquipment, "", ""))
                return false;

            if (!CheckCommand(SetWorkmode, QueryWorkmode, CheckWorkmode))
                return false;

            if (!CheckCommand(SetOutput, QueryOutput, CheckOutput))
                return false;

            if (!CheckCommand(SetCurrentRange, QueryCurrentRange, CheckCurrentRange))
                return false;

            if (!CheckCommand(SetVoltageCurrent, QueryVoltageCurrent, CheckVoltageCurrent))
                return false;

            return true;

        }

        public bool CheckCommand(string SetCmd,string ReadCmd,string Checkstring)
        {
            try
            {               
                if (SetCmd != "")
                {
                    LogMessage($"Send {SetCmd}", MessageLevel.Info);
                    SEND(SetCmd);
                }
                if (SetCmd == "*CLS" || SetCmd == "*RST")
                    return true;

                Sleep(200);

                if (ReadCmd != "")
                {
                    LogMessage($"Send {ReadCmd}", MessageLevel.Info);
                    SEND(ReadCmd);
                }

                //Sleep(500);

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
                        {
                            isTimeout = false;
                            break;
                        }

                        Sleep(10);
                    }
                    catch (Exception ex)
                    {
                        LogMessage(ex.Message, MessageLevel.Error);
                        break;
                    }
                }

                if(isTimeout == true)
                {
                    LogMessage($"Read is Timeout, Data is (\"{output}\")", MessageLevel.Error);
                    return false;
                }
                
                LogMessage($"Read {output}", MessageLevel.Info);

                if(string.IsNullOrEmpty(output))
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
            catch(Exception ex)
            {
                LogMessage($"Exception {ex.Message}", MessageLevel.Error);
                return false;
            }

            return true;
        }*/

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
