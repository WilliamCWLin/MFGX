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
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace AutoTestSystem.DUT
{
    public class DUT_USB : DUT_BASE
    {
        int comportTimeOut = 0;
        int totalTimeOut = 0;
        Comport DutComport = null;
        DosCmd doscmd = new DosCmd();

        [Category("Params"), Description("Set Comport")]
        public string PortName { get; private set; }

        [Category("Params"), Description("Set BaudRate")]
        public int BaudRate { get; set; }

        [Category("Params"), Description("Set Caption"), ]
        public string Caption { get; set; }

        public DUT_USB()
        {
            BaudRate = 115200;
        }

        public override void Dispose()
        {
            throw new NotImplementedException();
        }

        public override bool Init(string strParamInfo)
        {
            return true;
        }

        public override bool StartAction(string strItemName, string strParamIn, ref string strOutput)
        {
            throw new NotImplementedException();
        }

        public override bool OPEN()
        {
            int retryTimes = 15;
            for (int i = 0; i < retryTimes; i++)
            {
                Thread.Sleep(4000);
                string output = doscmd.SendCommand3($"wmic path Win32_PnPEntity where \"Caption like '%{Caption}%'\" get Caption");
                if (output.Contains(Caption) == true)
                {
                    int header = output.LastIndexOf("(") + 1;
                    int tail = output.LastIndexOf(")");
                    PortName = output.Substring(header, tail - header);
                }

                if (PortName != null)
                {
                    SerialConnetInfo DUTCOMinfo = new SerialConnetInfo { PortName = PortName, BaudRate = BaudRate };
                    DutComport = new Comport(DUTCOMinfo);
                    string str;

                    try
                    {
                        str = JsonConvert.SerializeObject(DUTCOMinfo, Formatting.Indented);
                        LogMessage($"[INIT] {str}");
                    }
                    catch (Exception ex)
                    {
                        LogMessage($"[INIT] Error occurred while serializing the object: {ex.Message}", MessageLevel.Error);
                        return false;
                    }

                    try
                    {
                        if (!DutComport.OpenCOM_CHK())
                        {
                            LogMessage($"[INIT] Connecting to {PortName} ...");
                            continue;
                        }
                    }
                    catch (Exception ex)
                    {
                        LogMessage($"[INIT] DUTCom Open Error: {ex.Message}", MessageLevel.Error);
                        return false;
                    }

                    LogMessage($"[INIT] DUTCom Open success");
                    return true;
                }
                else
                {
                    LogMessage("[INIT] Searching DUTCom ...");
                    continue;
                }
            }

            if (PortName == null)
            {
                LogMessage($"[INIT] DUTCom PortName is null", MessageLevel.Error);
                return false;
            }
            else
            {
                LogMessage($"[INIT] DUTCom Open Fail", MessageLevel.Error);
                return false;
            }

        }

        public override bool UnInit()
        {
            if (DutComport == null)
                return false;
            DutComport.Close();

            return true;
        }

        public override bool SEND(string input)
        {
            DutComport.cleanBuffer();
            DutComport.WriteLine(input);
            LogMessage($"[SEND] {input}\n");
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

        public override bool READ(ref string output)
        {
            output = "";
            DateTime oldTime = DateTime.Now;

            while (true)
            {
                try
                {
                    DutComport.SetReadTimeout(comportTimeOut);
                    output += DutComport.ReadUARTData();
                }
                catch (TimeoutException tex)
                {
                    LogMessage($"[READ] {tex.Message}", MessageLevel.Warn);
                }
                catch (Exception ex)
                {
                    LogMessage($"[READ] {ex.Message}", MessageLevel.Warn);
                }

                if (output.Contains("ERROR") == true)
                {
                    DutComport.cleanBuffer();
                    LogMessage($"[READ] {output}\n", MessageLevel.Error);
                    return false;
                }
                if (output.Contains("OK") == true || output.Contains("I2CWR") == true)
                    break;
                    
                DateTime newTime = DateTime.Now;
                TimeSpan span = newTime - oldTime;
                double timeDiff = span.TotalSeconds;
                double timeoutSec = (double)(totalTimeOut / 1000);
                if (timeDiff > timeoutSec)
                    break;
            }

            if (string.IsNullOrEmpty(output))
            {
                DutComport.cleanBuffer();
                LogMessage($"[READ] Output is Null or Empty", MessageLevel.Error);
                return false;
            }

            DutComport.cleanBuffer();
            LogMessage($"[READ] {output}\n");
            return true;
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
