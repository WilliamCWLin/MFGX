using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using AutoTestSystem.DAL;
using System.Text.RegularExpressions;
using static AutoTestSystem.BLL.Bd;
using AutoTestSystem.Base;
using System.ComponentModel;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace AutoTestSystem.DUT
{
    public class DUT_UART_HalfDuplex_NONJSON : DUT_BASE
    {
        int comportTimeOut = 0;
        int totalTimeOut = 0;
        Comport DutComport = null;

        [Category("Params"), Description("Select Comport"), TypeConverter(typeof(ComportList))]
        public string PortName { get; set; }

        [Category("Params"), Description("Set BaudRate")]
        public int BaudRate { get; set; }

        public DUT_UART_HalfDuplex_NONJSON()
        {
            BaudRate = 115200;
        }

        public override void Dispose()
        {
            throw new NotImplementedException();
        }

        public override bool Init(string strParamInfo)
        {
            if (PortName != null)
            {
                SerialConnetInfo DUTCOMinfo = new SerialConnetInfo { PortName = PortName, BaudRate = BaudRate };

                DutComport = new Comport(DUTCOMinfo);
                string str;

                try
                {
                    str = JsonConvert.SerializeObject(DUTCOMinfo, Formatting.Indented);
                }
                catch (Exception ex)
                {
                    LogMessage($"[INIT] Error occurred while serializing the object: {ex.Message}", MessageLevel.Error);
                    return false;
                }

                LogMessage($"[INIT] {str}");

                try
                {
                    if (!DutComport.OpenCOM_CHK())
                    {
                        LogMessage($"[INIT] DUTCom Open Fail", MessageLevel.Error);
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    LogMessage($"[INIT] DUTCom Open Error: {ex.Message}", MessageLevel.Error);
                    return false;
                }

                LogMessage($"[INIT] Open success");


                return true;
            }
            else
            {
                LogMessage($"[INIT] DUTCom PortName is null", MessageLevel.Error);
                return false;
            }

        }

        public override bool Status(ref string msg)
        {
            if (DutComport.SerialPort.IsOpen)
            {
                msg = $"{DutComport.SerialPort.PortName}(OPEN)";
                return true;
            }
            else
            {
                msg = $"{DutComport.SerialPort.PortName}(CLOSE)";
                return false;
            }
        }
        public override bool StartAction(string strItemName, string strParamIn, ref string strOutput)
        {
            throw new NotImplementedException();
        }

        public override bool OPEN()
        {
            try
            {
                if (!DutComport.OpenCOM_CHK())
                {
                    LogMessage($"[INIT] DUTCom Open Fail", MessageLevel.Error);
                    return false;
                }
            }
            catch (Exception ex)
            {
                LogMessage($"[INIT] DUTCom Open Error: {ex.Message}", MessageLevel.Error);
                return false;
            }

            LogMessage($"[INIT] Open success");

            return true;
        }

        public override bool UnInit()
        {
            try
            {
                if (DutComport != null)
                    DutComport.Close();
                else
                {
                    LogMessage($"[UnInit] Error: Comport is NULL", MessageLevel.Error);
                    return false;
                }                
            }
            catch (Exception ex)
            {
                LogMessage($"[UnInit] Error: {ex.Message}", MessageLevel.Error);
                return false;
            }


            return true;
        }

        public override bool SEND(string input)
        {
            DutComport.cleanBuffer();
            DutComport.WriteLine(input);
            LogMessage($"[SEND] {input}");
            //MessageBox.Show("DUT send UART Command:" + input);
            return true;
        }

        public override bool SEND(byte[] input)
        {
            DutComport.cleanBuffer();
            DutComport.Write(input);
            LogMessage($"[SEND] {input}");
            //MessageBox.Show("DUT send UART Command:" + input);
            return true;
        }

        public override bool READ(string ParamIn, ref string output)
        {
            output = "";
            DateTime oldTime = DateTime.Now;

            while (true)
            {
                try
                {
                    DutComport.SetReadTimeout(comportTimeOut);
                    output += DutComport.ReadUARTData();
                    LogMessage($"[READ] {output}");
                }
                catch (TimeoutException tex)
                {
                    //LogMessage.Info(tex.Message);
                    LogMessage($"[READ] TimeoutException {tex.Message}", MessageLevel.Warn);
                }
                catch (Exception ex)
                {
                    // LogMessage.Info(ex.Message);
                    LogMessage($"[READ] Exception {ex.Message}", MessageLevel.Warn);

                }

                
                if (output.Contains(ParamIn) == true)
                {
                    LogMessage($"[READ] {output}");
                    output = "";
                    var data = new Dictionary<string, object>
                        {
                            { "STATUS", "PASS" }
                        };
                    output = JsonConvert.SerializeObject(data);
                    break;
                }
                    


                DateTime newTime = DateTime.Now;
                TimeSpan span = newTime - oldTime;
                double timeDiff = span.TotalSeconds;
                double timeoutSec = (double)(totalTimeOut / 1000);
                if (timeDiff > timeoutSec)
                {
                    LogMessage($"[READ] Timeout", MessageLevel.Error);
                    var data = new Dictionary<string, object>
                        {
                            { "STATUS", "FAIL" }
                        };
                    output = JsonConvert.SerializeObject(data);
                    break;
                }
            }
            //MessageBox.Show("DUT read UART Command:" + output);
            if (string.IsNullOrEmpty(output))
            {
                DutComport.cleanBuffer();
                LogMessage($"[READ] Output is Null or Empty", MessageLevel.Error);
                return false;
            }

           
            

            return true;
        }

        public override bool READ(ref string output, int length, int header, int tail)
        {
           throw new NotImplementedException();
        }

        public override bool READNOJSON(string ParamIn , ref string output)
        {
            throw new NotImplementedException();
        }

        public override void SetTimeout(int timeout_comport, int timeout_total)
        {
            comportTimeOut = timeout_comport;
            totalTimeOut = timeout_total;
        }

        public override bool SendCGICommand(int request_type, string Checkstr, string CGICMD, string input, ref string output)
        {
            throw new NotImplementedException();
        }
    }
}
