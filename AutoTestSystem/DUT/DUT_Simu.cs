using System;
using System.Windows.Forms;
using System.Threading;
using AutoTestSystem.DAL;
using System.Text.RegularExpressions;
using static AutoTestSystem.BLL.Bd;
using AutoTestSystem.Base;
using System.ComponentModel;
using Manufacture;
using System.Drawing.Design;
using ClosedXML.Excel;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace AutoTestSystem.DUT
{
    public class DUT_Simu : DUT_BASE
    {
        //[Category("Params"), Description("自訂顯示名稱"), Editor(typeof(JsonEditor), typeof(UITypeEditor))]
        //public string Data { get; set; }
        int comportTimeOut = 0;
        int totalTimeOut = 0;
        [Category("Parameter"), Description("ms")]
        public int LineOutDelay { get; set; } = 50;

        [Category("Doc"), Description("Profile Path"), Editor(typeof(Manufacture.FileSelEditorRelPath), typeof(System.Drawing.Design.UITypeEditor))]
        public string DocPath { get; set; }

        [Category("Doc"), Description("index")]
        public int ColumnCommand { get; set; } = 4;
        [Category("Doc"), Description("index")]
        public int ColumnResult { get; set; } = 5;

        [Category("Other"), Description("")]
        public bool SimuFlag { get; set; } = false;
        private Simulator simulator;

        private readonly Queue<string> dataQueue = new Queue<string>();
        public DUT_Simu()
        {

        }

        public override void Dispose()
        {
            throw new NotImplementedException();
        }

        public override bool Init(string strParamInfo)
        {
            simulator = new Simulator(DocPath, LineOutDelay, ColumnCommand, ColumnResult);
            return true;
        }

        public override bool StartAction(string strItemName, string strParamIn, ref string strOutput)
        {
            throw new NotImplementedException();
        }

        public override bool OPEN()
        {
            return true;
        }

        public override bool UnInit()
        {
            return true;
        }

        public override bool SEND(string input)
        {
            simulator.Send(input); // 傳送指令到模擬器

            if (SimuFlag)
            {
                try
                {
                    string d = input.Replace("\r\n", "").Replace("\r", "").Replace("\n", "");

                    if (string.IsNullOrEmpty(CommandTable))
                    {
                        LogMessage($"Simulate command send fail.Data is null: {input}");
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
            return true;
        }

        public override bool SEND(byte[] input)
        {
            return true;
        }

        public override bool READ(ref string output)
        {
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
            
            return false;
        }

        //public override bool READ(string ParamIn, ref string output)
        //{
        //    output = "";
        //    DateTime oldTime = DateTime.Now;

        //    while (true)
        //    {
        //        try
        //        {
        //            string strOutput = simulator.ReadOutput(500);
        //            if (!string.IsNullOrEmpty(strOutput))
        //                LogMessage(strOutput);
        //            output += strOutput;
        //        }
        //        catch (TimeoutException tex)
        //        {
        //            //LogMessage.Info(tex.Message);
        //            LogMessage($"[READ] TimeoutException {tex.Message}", MessageLevel.Warn);
        //        }
        //        catch (Exception ex)
        //        {
        //            // LogMessage.Info(ex.Message);
        //            LogMessage($"[READ] Exception {ex.Message}", MessageLevel.Warn);

        //        }

        //        int headerCount = 0;
        //        int tailCount = 0;
        //        foreach (char c in output)
        //        {
        //            if (c == '{') headerCount++;
        //            if (c == '}') tailCount++;
        //        }

        //        if ((headerCount == tailCount) && (headerCount != 0))
        //        {
        //            if (output.Contains(ParamIn) == true)
        //                break;
        //            else
        //                output = "";
        //        }

        //        if (headerCount < tailCount)
        //        {
        //            LogMessage($"[READ] headerCount < tailCount", MessageLevel.Error);
        //            return false;
        //        }

        //        DateTime newTime = DateTime.Now;
        //        TimeSpan span = newTime - oldTime;
        //        double timeDiff = span.TotalSeconds;
        //        double timeoutSec = (double)(totalTimeOut / 1000);
        //        if (timeDiff > timeoutSec)
        //            break;
        //    }
        //    //MessageBox.Show("DUT read UART Command:" + output);
        //    if (string.IsNullOrEmpty(output))
        //    {
        //        LogMessage($"[READ] Output is Null or Empty", MessageLevel.Error);
        //        return false;
        //    }

        //    output = Regex.Replace(output, @"[\r\n]", "");
        //    output = Regex.Replace(output, @"[\r]", "");
        //    output = Regex.Replace(output, @"[\n]", "");
        //    output = Regex.Replace(output, @"[\t]", "");
        //    int header = output.IndexOf("{");
        //    int tail = output.LastIndexOf("}");
        //    if (header == -1 || tail == -1)
        //    {
        //        LogMessage($"[READ] header == -1 || tail == -1", MessageLevel.Error);
        //        return false;
        //    }

        //    int capture_length = tail - header + 1;
        //    output = output.Substring(header, capture_length);
        //    LogMessage($"[READ] {output}");

        //    return true;
        //}
        public override bool READ(string ParamIn, ref string output)
        {
            output = "";
            DateTime oldTime = DateTime.Now;

            while (true)
            {
                try
                {
                    string strOutput = simulator.ReadOutput(500);
                    if (!string.IsNullOrEmpty(strOutput))
                        LogMessage(strOutput);
                    output += strOutput;
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
                //DutComport.cleanBuffer();
                LogMessage($"[READ] Output is Null or Empty", MessageLevel.Error);
                return false;
            }




            return true;
        }

        public override bool READ(ref string output, int length, int header, int tail)
        {
            //output = Data;
            return true;
        }

        public override void SetTimeout(int timeout_comport, int timeout_total)
        {
            comportTimeOut = timeout_comport;
            totalTimeOut = timeout_total;
        }

        public override bool SendCGICommand(int request_type, string Checkstr, string CGICMD, string input, ref string output)
        {
            //output = Data;
            return true;
        }
    }

    public class Simulator
    {
        // 儲存指令與結果的字典
        private Dictionary<string, List<string>> commandResponses;

        // 指令處理的佇列和同步
        private Queue<string> commandQueue = new Queue<string>();
        private Queue<string> responseQueue = new Queue<string>(); // 用來儲存回應
        private AutoResetEvent commandEvent = new AutoResetEvent(false);
        private AutoResetEvent responseEvent = new AutoResetEvent(false); // 用來通知有新的回應

        private bool isRunning = true;
        private int delay = 50;
        private int CommandIndex = 4;
        private int ResultIndex = 5;
        // Constructor
        public Simulator(string csvFilePath, int D, int C, int R)
        {
            CommandIndex = C;
            ResultIndex = R;
            delay = D;
            // 初始化指令與結果
            commandResponses = LoadCommandsFromExcel(csvFilePath, C, R);

            // 啟動模擬器執行緒
            Thread simulatorThread = new Thread(RunSimulator)
            {
                IsBackground = true
            };
            simulatorThread.Start();
        }

        // 傳送指令到模擬器
        public void Send(string command)
        {
            responseQueue.Clear();
            lock (commandQueue)
            {
                commandQueue.Enqueue(command);
            }
            commandEvent.Set(); // 通知有新指令
        }

        // 從模擬器讀取回應
        public string ReadOutput(int timeoutMilliseconds = 1000)
        {
            // 等待直到有新的回應或超時
            if (responseEvent.WaitOne(timeoutMilliseconds))
            {
                lock (responseQueue)
                {
                    if (responseQueue.Count > 0)
                    {
                        return responseQueue.Dequeue();
                    }
                }
            }

            return null; // 如果超時或沒有數據，返回 null
        }

        private void RunSimulator()
        {
            while (isRunning)
            {
                commandEvent.WaitOne();

                string command;
                lock (commandQueue)
                {
                    if (commandQueue.Count == 0) continue;
                    command = commandQueue.Dequeue();
                }

                if (commandResponses.TryGetValue(command, out List<string> responses))
                {
                    foreach (var response in responses)
                    {
                        // 將回應存入回應隊列
                        lock (responseQueue)
                        {
                            responseQueue.Enqueue(response);
                        }

                        // 通知有新回應
                        responseEvent.Set();
                        Thread.Sleep(delay); // 模擬延遲
                    }
                }
                else
                {
                    // 如果指令未知，也將錯誤訊息放入回應隊列
                    lock (responseQueue)
                    {
                        responseQueue.Enqueue($"Error: Unknown command '{command}'");
                    }
                    responseEvent.Set();
                }
            }
        }

        public void Stop()
        {
            isRunning = false;
            commandEvent.Set();
        }

        public Dictionary<string, List<string>> LoadCommandsFromExcel(string filePath, int C, int R)
        {
            var result = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

            try
            {
                using (var workbook = new XLWorkbook(filePath))
                {
                    var worksheet = workbook.Worksheet(1);
                    foreach (var row in worksheet.RowsUsed().Skip(1)) // 假設第一行是標題
                    {
                        string command = row.Cell(C).GetValue<string>().Trim();
                        string response = row.Cell(R).GetValue<string>().Trim();

                        if (!string.IsNullOrEmpty(command))
                        {
                            // 拆分 response 的每行內容
                            //var responseLines = response.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
                            //var responseLines = response.Replace("\r", "").Replace("\n", "").Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                            var responseLines = response.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None)
                            .Select(line => line.Replace("\r", "").Replace("\n", " "))
                            .ToList();
                            if (!result.ContainsKey(command))
                            {
                                result[command] = new List<string>();
                            }
                            result[command].AddRange(responseLines);
                            //result[command].Add(response);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading Excel file: {ex.Message}");
            }

            return result;
        }

    }
}
