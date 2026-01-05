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
using OpenCvSharp.LineDescriptor;
using AutoTestSystem.Model;
using Manufacture;
using System.Windows.Forms;
using System.Timers;
using System.Collections.Concurrent;
using AutoTestSystem.BLL;

namespace AutoTestSystem.DUT
{
    public class DUT_SERIALPORT : DUT_BASE
    {
        // 串口設定屬性
        [Category("Comport"), Description("Select Comport"), TypeConverter(typeof(ComportList))]
        public string PortName { get; set; }
        [Category("Comport"), Description("")]
        public int BaudRate { get; set; } = 115200;     // 預設波特率
        [Category("Comport"), Description("")]
        public Parity Parity { get; set; } = Parity.None; // 預設奇偶檢查
        [Category("Comport"), Description("")]
        public int DataBits { get; set; } = 8;           // 預設資料位

        [Category("Comport"), Description("")]
        public StopBits StopBits { get; set; } = StopBits.One; // 預設停止位

        [Category("Comport"), Description("Select Comport")]
        public string Newline { get; set; } = "CRLF";
        [Category("Other"), Description("Select Comport")]
        public bool PreSendNewLine { get; set; } = false;
        [Category("Other"), Description("")]
        public bool SimuFlag { get; set; } = false;

        // 超時設定
        private int WriteTimeout;
        private int ReadTimeout;

        // 初始化 LargeDataHandler，預設 1MB 的容量
        LargeDataHandler dataHandler = new LargeDataHandler();

        // 內部變數
        private SerialPort serialPort;
        private string tempBuffer = string.Empty;
        private readonly object bufferLock = new object();

        private readonly object logQueueLock = new object();

        private readonly Queue<string> dataQueue = new Queue<string>();
        private readonly Queue<string> LogQueue = new Queue<string>();
        //private readonly ConcurrentQueue<string> logQueue = new ConcurrentQueue<string>();
        //private readonly System.Timers.Timer logtimer;

        //public DUT_SERIALPORT()
        //{
        //    // 初始化串口
        //    serialPort = null;
        //    // 初始化 log 處理計時器
        //    logtimer = new System.Timers.Timer(50); // 每秒處理一次
        //    logtimer.Elapsed += ProcessLogs;
        //    logtimer.AutoReset = true;

        //}
        //private void ProcessLogs(object sender, ElapsedEventArgs e)
        //{
        //    while (logQueue.TryDequeue(out string log))
        //    {
        //        LOGGER.Debug(log);
        //    }
        //}

        // 初始化串口
        public override bool Init(string strParamInfo)
        {
            if (SimuFlag)
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
                if(string.IsNullOrEmpty(strParamInfo))
                    serialPort = new SerialPort(PortName, BaudRate, Parity, DataBits, StopBits);
                else
                {
                    try
                    {
                        SerialPortConfig config = JsonConvert.DeserializeObject<SerialPortConfig>(strParamInfo);
                        serialPort = new SerialPort(PortName, config.BaudRate, config.Parity, config.DataBits, config.StopBits);
                        
                    }
                    catch (Exception ex)
                    {
                        LogMessage($"Failed to open serial port: {ex.Message}");
                        return false;
                    }
                }
                serialPort.Open();
                tempBuffer = string.Empty;
                dataQueue.Clear();
                // 設定換行字元
                switch (Newline)
                {
                    case "CR":
                        serialPort.NewLine = "\r";
                        break;
                    case "LF":
                        serialPort.NewLine = "\n";
                        break;
                    case "CRLF":
                        serialPort.NewLine = "\r\n";
                        break;
                    case "TAB":
                        serialPort.NewLine = "\t";
                        break;
                }

                serialPort.DataReceived += SerialPort_DataReceived;
                //logtimer.Start();
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
                //logtimer.Stop();
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
            if (SimuFlag)
            {
                try
                {
                    string d = data.Replace("\r\n", "").Replace("\r", "").Replace("\n", "");

                    if (string.IsNullOrEmpty(CommandTable))
                    {
                        LogMessage($"Simulate command send fail.Data is null: {data}");
                        return false;
                    }

                    // 使用 Newtonsoft.Json 將 JSON 反序列化為字典
                    var dictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(CommandTable);

                    if (dictionary.ContainsKey(d))
                    {
                        string inputData = dictionary[d];
                        // 使用 Split 方法按行分割字串
                        int currentIndex = 0;
                        int segmentSize = 10;
                        // 處理字串，並按照每段大小切割
                        while (currentIndex < inputData.Length)
                        {
                            // 計算當前小段的長度
                            int length = Math.Min(segmentSize, inputData.Length - currentIndex);

                            // 提取小段
                            string segment = inputData.Substring(currentIndex, length);

                            // 如果是最後一段，附加換行符 "\r\n"
                            //if (currentIndex + length >= inputData.Length)
                            //{
                            //    segment += "\r\n";
                            //}

                            // 將小段放入 Queue
                            dataQueue.Enqueue(segment);

                            // 移動索引到下一個小段
                            currentIndex += length;
                        }


                        LogMessage("\n" + inputData);


                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("模擬模式送出指令異常Simulate command and return empty data table.: " + ex.Message);
                }
                return true;
            }
            else
            {
                try
                {
                    if (serialPort?.IsOpen == true)
                    {
                        LogMessage($"Send Data: {data}");
                        Thread.Sleep(50);
                        if (PreSendNewLine)
                        {
                            serialPort.Write(serialPort.NewLine);
                            Thread.Sleep(10);
                        }
                        lock(logQueueLock)
                        {
                            LogQueue.Enqueue($"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [{Description}]Send Command:{data}\n");
                        }
                        

                        foreach (char c in data)
                        {
                            serialPort.Write(c.ToString());
                            System.Threading.Thread.Sleep(5); // 模擬鍵入延遲
                        }

                        //serialPort.Write(data);
                        
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
                    LogMessage($"Unexpected error occurred while sending data. Exception: {ex.Message}");
                    return false;
                }
            }

        }

        
        // 接收資料事件處理
        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            SetMLoggerThread(LOGGER);
            try
            {
                string data = serialPort.ReadExisting(); // 讀取接收到的數據
                string hexData = string.Join(" ", data.Select(c => $"0x{((int)c):X2}"));

                lock (bufferLock)
                {
                    dataQueue.Enqueue(data); // 整筆數據放入隊列
                   
                }
                lock (logQueueLock)
                {
                    LogQueue.Enqueue(data);
                }
            }
            catch (Exception ex)
            {
                LogMessage($"SerialPort_DataReceived:{ex.Message}");
            }
        }

        public override bool READ(ref string output)
        {
            lock (bufferLock)
            {
                //string data = serialPort.ReadExisting(); // 讀取接收到的數據

                //if (!string.IsNullOrEmpty(data))  // 確保不處理空數據
                //{
                //    dataQueue.Enqueue(data);
                //}
                if (dataQueue.Count > 0)
                {
                    // 將 queue 內容一次性取出並合併
                    output = string.Join("", dataQueue);
                    //if (DutDashboard != null)
                    //    DutDashboard.MainInfoRichText(DutDashboard.DutLogRichTextBox, output, isNewline: true);
                    //else
                    // 清空 queue
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

        // 從佇列中取出資料
        //public string GetNextData()
        //{
        //    lock (bufferLock)
        //    {
        //        if (dataQueue.Count > 0)
        //        {
        //            return dataQueue.Dequeue();
        //        }
        //    }
        //    return null;
        //}

        // 從串口讀取資料

        // 設定超時時間
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

        // 釋放資源
        public override void Dispose()
        {
            UnInit();
        }

        // Log 訊息

        public override bool StartAction(string strItemName, string strParamIn, ref string strOutput)
        {
            if (strItemName == "Print All")
            {
                TestInfo.AddTestStep($"DUT_LOG", dataHandler.GetData());
                //LogMessage(dataHandler.GetData(), MessageLevel.Info);
            }
            else if (strItemName == "WriteLog")
            {
                dataHandler?.AppendData(strParamIn);
            }
            else if (strItemName == "Clear")
            {
                dataHandler?.ClearData();
                dataQueue.Clear();
            }
            else if (strItemName == "PopDutLog")
            {
                if (LogQueue.Count > 0)
                {
                    LogMessage("================================DUT_LOG================================\r\n", MessageLevel.Raw);
                    LogMessage(string.Join("", LogQueue), MessageLevel.Raw);
                    LogMessage("\r\n", MessageLevel.Raw);
                    // 清空 queue
                    LogQueue.Clear();
                    return true;
                }

            }
            else if (strItemName == "ClearDutLog")
            {
                lock (logQueueLock)
                {
                    LogQueue.Clear();
                }
            }
            return true;
        }

        public override bool ClearBuffer()
        {
            lock (logQueueLock)
            {
                if (LogQueue != null)
                    LogQueue.Clear();
            }
            if (dataQueue != null)
                dataQueue.Clear();
            return true;
        }
        public override bool PopAllLog()
        {
            lock (logQueueLock)
            {
                if (LogQueue.Count > 0)
                {
                    LogMessage("==============DUT Communication Log==============\r\n", MessageLevel.Raw);
                    LogMessage(string.Join("", LogQueue), MessageLevel.Raw);
                    LogMessage("\r\n", MessageLevel.Raw);
                    // 清空 queue
                    LogQueue.Clear();
                    return true;
                }
            }

            return true;
        }
        
        public override bool SEND(byte[] input)
        {
            if (input != null && input.Length > 0)
            {
                // 使用SerialPort寫入byte數據
                try
                {
                    serialPort.Write(input, 0, input.Length);
                    return true;
                }
                catch (Exception ex)
                {
                    LogMessage($"Error writing to serial port: {ex.Message}");
                    return false;
                }
            }

            return false;
        }
        public override bool Clear()
        {
            try
            {
                dataHandler.ClearData();
                dataQueue.Clear();
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
    }

    public class LargeDataHandler
    {
        private readonly StringBuilder stringBuilder;

        public LargeDataHandler(int initialCapacity = 1024 * 1024) // 預設初始化 1MB
        {
            stringBuilder = new StringBuilder(initialCapacity);
        }

        // 添加資料到緩存中
        public void AppendData(string data)
        {
            if (!string.IsNullOrEmpty(data))
            {
                stringBuilder.Append(data);

            }
        }

        // 獲取目前所有累積的資料
        public string GetData()
        {
            return stringBuilder.ToString();
        }

        // 清空所有累積的資料
        public void ClearData()
        {
            stringBuilder.Clear();
        }
    }
    public class SerialPortConfig
    {
        public string PortName { get; set; }
        public int BaudRate { get; set; }
        public Parity Parity { get; set; }
        public int DataBits { get; set; }
        public StopBits StopBits { get; set; }
    }
}
