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
    public class SecureShell : ControlDeviceBase
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
                // 先徹底清理：保證 shellStream 和 client 都是 null
                UnInit();

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
                    // 2. 假設已經在某處建立並連線好 sshClient 和 shellStream
                    //    並且訂閱事件：
                    shellStream.DataReceived += ShellStream_DataReceived;
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
        private void ShellStream_DataReceived(object sender, Renci.SshNet.Common.ShellDataEventArgs e)
        {
            try
            {
                // 把收到的 byte[] 轉成字串
                string data = Encoding.UTF8.GetString(e.Data);

                lock (bufferLock)
                {
                    // 將整筆資料放到佇列，後續可在別的緒拿出來處理
                    dataQueue.Enqueue(data);
                }
            }
            catch (Exception ex)
            {
                // 發生例外時也記錄
                LogMessage($"ShellStream_DataReceived Exception: {ex.Message}");
            }
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
                // 1. 停用事件 & 釋放 shellStream
                if (shellStream != null)
                {
                    shellStream.DataReceived -= ShellStream_DataReceived;
                    shellStream.Dispose();
                    shellStream = null;
                }

                // 2. 清空 queue
                lock (bufferLock)
                {
                    dataQueue.Clear();
                }

                // 3. Disconnect + Dispose client
                if (client != null)
                {
                    if (client.IsConnected)
                        client.Disconnect();
                    client.Dispose();
                    client = null;
                }

                LogMessage("SSH disconnected.");
                return true;
            }
            catch (Exception ex)
            {
                LogMessage($"UnInit error: {ex.Message}");
                return false;
            }
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
