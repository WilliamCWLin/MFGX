using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading;
using Renci.SshNet;
using Renci.SshNet.Common;
using AutoTestSystem.Base;
using AutoTestSystem.Model;
using System.IO.Ports;

namespace AutoTestSystem.Equipment.ControlDevice
{
    public class SSHDevice : ControlDeviceBase
    {
        [Category("Information"), Description("Your Device IP")]
        public string host { get; set; }
        [Category("Information"), Description("Your Username")]
        public string username { get; set; }
        [Category("Information"), Description("Your Password")]
        public string password { get; set; }
        [Category("Information"), Description("Your Port")]
        public int port { get; set; } = 22;
        [Category("Information"), Description("Initial_Open")]
        public bool Initial_Open { get; set; } = true;
        [Category("Information"), Description("Your Timeout")]
        public int ConnectTimeout { get; set; } = 10000;

        private int WriteTimeout = 10000;
        private int ReadTimeout = 10000;

        private readonly object bufferLock = new object();
        private readonly Queue<string> dataQueue = new Queue<string>();

        private SshClient client;
        private ShellStream shellStream;
        private Thread readThread;
        private bool isReading = false;

        public override bool Init(string strParamInfo)
        {
            if (Simu)
            {
                if (GlobalNew.CurrentMode == "PROD")
                {
                    LogMessage("DUT 模擬狀態，勿用於生產中!!", MessageLevel.Fatal);
                    return false;
                }
                return true;
            }

            try
            {
                if (Initial_Open)
                    return Open(strParamInfo);
                else
                    LogMessage("SKIP SSH Connection.");
            }
            catch (Exception ex)
            {
                LogMessage($"Init error: {ex.Message}");
                return false;
            }

            return true;
        }

        public bool Open(string strParamInfo)
        {
            try
            {
                if (client == null || !client.IsConnected)
                {
                    client = new SshClient(host, port, username, password);
                    client.ConnectionInfo.Timeout = TimeSpan.FromMilliseconds(ConnectTimeout);
                    client.Connect();
                }

                if (client.IsConnected)
                {
                    var modes = new Dictionary<TerminalModes, uint>();
                    shellStream = client.CreateShellStream("xterm", 80, 24, 800, 600, 1024, modes);
                    StartReadingShell();
                    LogMessage("SSH ShellStream established.");
                    return true;
                }
            }
            catch (Exception ex)
            {
                LogMessage($"Open error: {ex.Message}");
            }

            return false;
        }

        private void StartReadingShell()
        {
            isReading = true;
            readThread = new Thread(() =>
            {
                try
                {
                    while (isReading && shellStream != null && shellStream.CanRead)
                    {
                        string line = shellStream.ReadLine(TimeSpan.FromMilliseconds(ReadTimeout));
                        if (!string.IsNullOrEmpty(line))
                        {
                            lock (bufferLock)
                            {
                                dataQueue.Enqueue(line);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogMessage($"Shell read error: {ex.Message}");
                }
            });
            readThread.IsBackground = true;
            readThread.Start();
        }

        public override bool SEND(string data)
        {
            if (Simu)
            {
                LogMessage($"[SIMU] SEND: {data}");
                return true;
            }

            try
            {
                if (shellStream != null && shellStream.CanWrite)
                {
                    shellStream.WriteLine(data);
                    return true;
                }
                else
                {
                    LogMessage("ShellStream is not writable.");
                }
            }
            catch (Exception ex)
            {
                LogMessage($"SEND error: {ex.Message}");
            }

            return false;
        }

        public override bool SEND(byte[] input)
        {
            if (Simu)
            {
                LogMessage("[SIMU] SEND (byte[]): " + BitConverter.ToString(input));
                return true;
            }

            try
            {
                if (shellStream != null && shellStream.CanWrite)
                {
                    shellStream.Write(input, 0, input.Length);
                    shellStream.Flush(); // 確保資料立即送出
                    return true;
                }
                else
                {
                    LogMessage("ShellStream is not writable.");
                }
            }
            catch (Exception ex)
            {
                LogMessage($"SEND(byte[]) error: {ex.Message}");
            }

            return false;
        }

        public override bool READ(ref string output)
        {
            lock (bufferLock)
            {
                if (dataQueue.Count > 0)
                {
                    output = dataQueue.Dequeue();
                    return true;
                }
            }
            return false;
        }

        public override bool UnInit()
        {
            try
            {
                isReading = false;
                readThread?.Join();
                shellStream?.Dispose();
                client?.Disconnect();
                LogMessage("SSH disconnected.");
            }
            catch (Exception ex)
            {
                LogMessage($"UnInit error: {ex.Message}");
                return false;
            }

            return true;
        }

        public override void SetTimeout(int timeout_comport, int timeout_total)
        {
            WriteTimeout = timeout_comport;
            ReadTimeout = timeout_comport;
        }

        public override void SetTimeout(int time)
        {
            ConnectTimeout = time;
            ReadTimeout = time;
        }

        public override void Dispose()
        {
            UnInit();
        }

        public override bool StartAction(string strItemName, string strParamIn, ref string strOutput)
        {
            if (strItemName == "open")
                return Open(strParamIn);
            else if (strItemName == "close")
                return UnInit();
            else
                return true;
        }

        public override bool Clear()
        {
            try
            {
                lock (bufferLock)
                {
                    dataQueue.Clear();
                }
                LogMessage("Queue cleared.");
                return true;
            }
            catch (Exception ex)
            {
                LogMessage($"Clear error: {ex.Message}");
                return false;
            }
        }
    }
}
