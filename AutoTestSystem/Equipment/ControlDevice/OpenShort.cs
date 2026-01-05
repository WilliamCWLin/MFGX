using System;
using System.IO.Ports;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using System.Diagnostics;
using AutoTestSystem.Base;
using System.ComponentModel;
using AutoTestSystem.DAL;
using System.Collections.Generic;
using System.Linq;
using AutoTestSystem.Model;
using Manufacture;

namespace AutoTestSystem.Equipment.ControlDevice
{
    public class OpenShort : ControlDeviceBase
    {
        // 串口設定屬性
        [Category("Comport"), Description("Select Comport"), TypeConverter(typeof(ComportList))]
        public string PortName { get; set; }
        [Category("Comport"), Description("")]
        public int BaudRate { get; set; } = 115200;
        [Category("Comport"), Description("")]
        public Parity Parity { get; set; } = Parity.None;
        [Category("Comport"), Description("")]
        public int DataBits { get; set; } = 8;
        [Category("Comport"), Description("")]
        public StopBits StopBits { get; set; } = StopBits.One;
        [Category("Comport"), Description("Select Comport")]
        public string Newline { get; set; } = "CRLF";

        // 超時設定
        private int WriteTimeout;
        private int ReadTimeout;

        // 內部變數
        private SerialPort serialPort;
        private readonly object bufferLock = new object();
        private readonly Queue<string> dataQueue = new Queue<string>();
        private List<byte> uartBuffer = new List<byte>();

        // 初始化串口
        public override bool Init(string strParamInfo)
        {
            if (Simu)
            {
                if (GlobalNew.CurrentMode == "PROD")
                {
                    LogMessage("DUT 模擬狀態，勿用於生產中!!\n DUT Simulation should not be used for production.", MessageLevel.Fatal);
                    return false;
                }
                return true;
            }
            if (serialPort != null && serialPort.IsOpen)
            {
                LogMessage("Serial port is already open.");
                return true;
            }

            try
            {
                serialPort = new SerialPort(PortName, BaudRate, Parity, DataBits, StopBits);
                serialPort.Open();

                // 設定換行字元
                switch (Newline)
                {
                    case "CR": serialPort.NewLine = "\r"; break;
                    case "LF": serialPort.NewLine = "\n"; break;
                    case "CRLF": serialPort.NewLine = "\r\n"; break;
                    case "TAB": serialPort.NewLine = "\t"; break;
                }

                //serialPort.DataReceived += SerialPort_DataReceived;
                LogMessage("Serial port opened successfully.");
            }
            catch (Exception ex)
            {
                LogMessage($"Failed to open serial port: {ex.Message}");
                return false;
            }

            return true;
        }

        // 關閉串口
        public override bool UnInit()
        {
            try
            {
                if (serialPort != null)
                {
                    if (serialPort.IsOpen)
                    {
                        Clear();
                        serialPort?.Close();
                        LogMessage("Serial port closed.");
                    }
                    serialPort?.Dispose();
                    serialPort = null;
                }
            }
            catch (Exception ex)
            {
                LogMessage($"Failed to UnInit serial port: {ex.Message}");
                return false;
            }
            return true;
        }

        // 發送字串資料
        public override bool SEND(string data)
        {
            if (Simu)
            {
                // 模擬模式略
                return true;
            }
            else
            {
                try
                {
                    if (serialPort?.IsOpen == true)
                    {
                        // 支援 "73 74 61 72 74 00 03 00 01 65 6e 64" 格式
                        // 1. 移除空白
                        string hexString = data.Replace(" ", "").Replace("\r", "").Replace("\n", "");

                        // 2. 轉成 byte[]
                        if (hexString.Length % 2 != 0)
                        {
                            LogMessage("SEND fail: hex string length is not even.");
                            return false;
                        }
                        byte[] bytes = new byte[hexString.Length / 2];
                        for (int i = 0; i < hexString.Length; i += 2)
                        {
                            bytes[i / 2] = Convert.ToByte(hexString.Substring(i, 2), 16);
                        }

                        // 3. 實際送出
                        serialPort.Write(bytes, 0, bytes.Length);
                        LogMessage($"Send HEX Data: {BitConverter.ToString(bytes)}");
                        return true;
                    }
                    else
                    {
                        LogMessage("Failed to send data: Serial Port is not open.");
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    LogMessage($"Unexpected error occurred while sending HEX data. Exception: {ex.Message}");
                    return false;
                }
            }
        }

        // 串口接收事件：累積資料並分包解析
        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                int bytesToRead = serialPort.BytesToRead;
                byte[] buffer = new byte[bytesToRead];
                serialPort.Read(buffer, 0, bytesToRead);

                lock (uartBuffer)
                {
                    uartBuffer.AddRange(buffer);

                    // 檢查是否有完整一包
                    while (TryExtractPacket(uartBuffer, out byte[] packet))
                    {
                        ParseAndLogPacket(packet);
                    }
                }
            }
            catch (Exception ex)
            {
                LogMessage($"SerialPort_DataReceived:{ex.Message}");
            }
        }

        // 分包判斷與提取
        private bool TryExtractPacket(List<byte> buffer, out byte[] packet)
        {
            packet = null;
            int endIdx = FindEndIndex(buffer);
            if (endIdx >= 2) // 至少要有 65 6E 64
            {
                packet = buffer.Take(endIdx + 1).ToArray();
                buffer.RemoveRange(0, endIdx + 1);
                return true;
            }
            return false;
        }

        private int FindEndIndex(List<byte> buffer)
        {
            for (int i = 0; i < buffer.Count - 2; i++)
            {
                if (buffer[i] == 0x65 && buffer[i + 1] == 0x6E && buffer[i + 2] == 0x64)
                    return i + 2;
            }
            return -1;
        }

        // 解析一包資料並顯示 160 pin bit 狀態
        private void ParseAndLogPacket(byte[] packet)
        { 
            int headerLen = 3; // 1 byte 開始 + 2 byte 數量
            int pinLineLen = 21; // 每個 PIN 狀況回傳 21 bytes
            int footerLen = 3; // 65 6E 64

            int pinLines = (packet.Length - headerLen - footerLen) / pinLineLen;
            if (pinLines <= 0)
            {
                LogMessage("Packet too short or no PIN lines.");
                return;
            }

            StringBuilder logMsg = new StringBuilder();
            logMsg.AppendLine("Start");

            // 先存所有 bit 行
            List<string> bitLines = new List<string>();
            // 再存所有 byte 行
            List<string> byteLines = new List<string>();

            for (int line = 0; line < pinLines; line++)
            {
                int offset = headerLen + line * pinLineLen;
                byte[] pinBytes = packet.Skip(offset).Take(pinLineLen).ToArray();

                int pinNo = pinBytes[0];

                // 1. 先組出所有 bit（bit 反轉，低位在前）
                StringBuilder bitLine = new StringBuilder();
                for (int i = 1; i < pinBytes.Length; i++)
                {
                    string bits = Convert.ToString(pinBytes[i], 2).PadLeft(8, '0');
                    bits = new string(bits.Reverse().ToArray());
                    bitLine.Append(bits);
                }
                string bits160 = bitLine.ToString().Substring(0, 160);
                bitLines.Add($"PIN_{pinNo:D3}: {bits160}");

                // 2. 組出 HEX bytes
                string hexBytes = BitConverter.ToString(pinBytes, 1, 20).Replace("-", " ");
                byteLines.Add($"PIN_{pinNo:D3}: [Bytes: {hexBytes}]");
            }

            // 先輸出所有 bit 行
            foreach (var line in bitLines)
                logMsg.AppendLine(line);

            // 再輸出所有 byte 行
            //foreach (var line in byteLines)
            //    logMsg.AppendLine(line);

            logMsg.AppendLine("END");

            lock (bufferLock)
            {
                dataQueue.Enqueue(logMsg.ToString());
            }
        }
        

        // 清除 buffer
        public override bool Clear()
        {
            try
            {
                dataQueue.Clear();
                uartBuffer.Clear();
                serialPort?.DiscardOutBuffer();
                serialPort?.DiscardInBuffer();
                LogMessage("Clear");
                return true;
            }
            catch (Exception ex)
            {
                LogMessage($"Clear Fail.{ex.Message}");
                return false;
            }
        }
        public override bool READ(ref byte[] packet, int timeout)
        {
            if (serialPort == null || !serialPort.IsOpen)
                return false;

            List<byte> buffer = new List<byte>();
            Stopwatch sw = new Stopwatch();
            sw.Start();

            try
            {
                while (sw.ElapsedMilliseconds < timeout)
                {
                    int bytesToRead = serialPort.BytesToRead;
                    if (bytesToRead > 0)
                    {
                        byte[] temp = new byte[bytesToRead];
                        serialPort.Read(temp, 0, bytesToRead);
                        buffer.AddRange(temp);

                        // 檢查是否有結束 byte
                        int endIdx = FindEndIndex(buffer);
                        if (endIdx >= 2)
                        {
                            packet = buffer.Take(endIdx + 1).ToArray();
                            // buffer.RemoveRange(0, endIdx + 1); // 若要保留剩餘資料可用
                            return true;
                        }
                    }
                    Thread.Sleep(5); // 避免CPU過高
                }
            }
            catch (Exception ex)
            {
                LogMessage($"[READ] Exception {ex.Message}", MessageLevel.Warn);
                return false;
            }

            // Timeout
            packet = null;
            return false;
        }


        // 其他原有方法（略，保留你的架構）
        public override bool READ(ref string output)
        {
            lock (bufferLock)
            {
                if (dataQueue.Count > 0)
                {
                    output = string.Join("", dataQueue);

                    dataQueue.Clear();

                    return true;
                }
            }
            return false;
        }
        public override bool READ(ref string output, int timeout)
        {
            try
            {
                serialPort.ReadTimeout = timeout;
                output = serialPort.ReadLine() + "\r\n";
            }
            catch (Exception ex)
            {
                LogMessage($"[READ] Exception {ex.Message}", MessageLevel.Warn);
            }
            return true;
        }
        public override void SetTimeout(int timeout_comport, int timeout_total)
        {
            WriteTimeout = timeout_comport;
            ReadTimeout = timeout_comport;
            if (serialPort != null)
            {
                serialPort.WriteTimeout = WriteTimeout;
                serialPort.ReadTimeout = ReadTimeout;
            }
        }
        public override void SetTimeout(int time)
        {
            WriteTimeout = time;
            ReadTimeout = time;
            if (serialPort != null)
            {
                serialPort.WriteTimeout = WriteTimeout;
                serialPort.ReadTimeout = ReadTimeout;
            }
        }
        public override void Dispose()
        {
            UnInit();
        }
        public override bool StartAction(string strItemName, string strParamIn, ref string strOutput)
        {
            return true;
        }
    }
}