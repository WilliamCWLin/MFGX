using NationalInstruments.Visa;
using System;
using System.Threading;
using static AutoTestSystem.BLL.Bd;

namespace AutoTestSystem.DAL
{
    public class GPIB : Communication
    {
        public MessageBasedSession mbSession;
        public COMMTYPE ConnType;
        public string Board;
        public string GPIBAddress;
        private string IPAddress;
        private string PortNum;

        public GPIB(GPIBInfo _GPIBInfo)
        {
            Board = _GPIBInfo._Board;
            GPIBAddress = _GPIBInfo._GPIBAddress;
            mbSession.TimeoutMilliseconds = 10000;
            IPAddress = _GPIBInfo._IPAddress;
            PortNum = _GPIBInfo._PortNum;
            ConnType = _GPIBInfo._ConnType;
        }

        public override bool Open()
        {
            bool rReturn = false;
            using (var rmSession = new ResourceManager())
            {
                try
                {
                    string ResourceName;
                    if (ConnType == COMMTYPE.TCPIPINSTR)
                    {
                        ResourceName = $"TCPIP0::{IPAddress}::inst{Board}::INSTR";
                    }
                    else if (ConnType == COMMTYPE.TCPIPSOCKET)
                    {
                        ResourceName = $"TCPIP{Board}::{IPAddress}::{PortNum}::SOCKET";
                    }
                    else
                    {
                        ResourceName = $"GPIB{Board}::{GPIBAddress}::INSTR";
                    }

                    mbSession = (MessageBasedSession)rmSession.Open(ResourceName);
                    //mbSession.Timeout = Timeout;
                    //mbSession.Clear();
                    //mbSession.Write("*CLS\n"); //skip by Alice on 2018-9-21
                    //mbSession.Query("*IDN?\n");
                    //Logger.Debug(mbSession.Query("*IDN?\n"));
                    rReturn = true;
                }
                catch (InvalidCastException)
                {
                    Logger.Fatal("Resource selected must be a message-based session");
                }
                catch (Exception exp)
                {
                    Logger.Fatal(exp.Message);
                }
                return rReturn;
            }
        }

        public override void Write(string strCmd)
        {
            try
            {
                string textToWrite = ReplaceCommonEscapeSequences(strCmd);
                Logger.Debug("VISAsendCommand-->" + textToWrite);
                mbSession.RawIO.Write(textToWrite);
            }
            catch (Exception exp)
            {
                Logger.Debug(exp.Message);
            }
        }

        public override string Read()
        {
            string ResponseContext = String.Empty;
            try
            {
                ResponseContext = InsertCommonEscapeSequences(mbSession.RawIO.ReadString());
            }
            catch (Exception exp)
            {
                Logger.Debug(exp.Message);
            }
            finally
            {
                Logger.Debug("VISARead:" + ResponseContext);
            }
            return ResponseContext;
        }

        /// <summary>
        /// 发命令给仪器并读取响应
        /// </summary>
        /// <param name="strCmd"></param>
        /// <returns></returns>
        public override bool SendCommand(string command, ref string strRecAll, string DataToWaitFor, int timeout = 10)
        {
            try
            {
                long lngStart = DateTime.Now.AddSeconds(timeout).Ticks;
                long lngCurTime = 0;
                strRecAll = "";
                Logger.Debug($"VISASendComd-->{command}");
                Write(command);
                sReceiveAll = Read();
                while (sReceiveAll.ToLower().IndexOf(DataToWaitFor.ToLower()) == -1)
                {
                    lngCurTime = DateTime.Now.Ticks;
                    if (lngCurTime > lngStart)
                    {
                        strRecAll = sReceiveAll;
                        sReceiveAll = "";
                        Logger.Debug(strRecAll);
                        Logger.Debug($"Waiting for:{DataToWaitFor} TimeOut({timeout}),FAIL!!!");
                        return false;
                    }
                    else
                    {
                        Thread.Sleep(1);
                    }
                    Write(command);
                    sReceiveAll = Read();
                }
                strRecAll = sReceiveAll;
                sReceiveAll = "";
                Logger.Debug(strRecAll);
                Logger.Debug($"Waiting for:{DataToWaitFor} succeed!!");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Debug(ex.ToString());
                return false;
                throw;
            }
        }

        public override void Close()
        {
            mbSession.Clear();
            mbSession.Dispose();
        }

        public override void Dispose()
        {
            mbSession.Dispose();
        }

        private string ReplaceCommonEscapeSequences(string s)
        {
            return s.Replace("\\n", "\n").Replace("\\r", "\r");
        }

        private string InsertCommonEscapeSequences(string s)
        {
            return s.Replace("\n", "\\n").Replace("\r", "\\r");
        }
    }

    public class GPIBInfo
    {
        public string _Board { get; set; }
        public string _GPIBAddress { get; set; }
        public string _Timeout { get; set; } = "10";
        public string _IPAddress { get; set; } = null;
        public string _PortNum { get; set; } = null;
        public COMMTYPE _ConnType { get; set; } = COMMTYPE.BPIB;
    }

    public enum COMMTYPE
    {
        BPIB = 1,
        TCPIPINSTR = 2,
        TCPIPSOCKET = 3,
    }
}