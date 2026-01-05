using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using System.ComponentModel;
using AutoTestSystem.Base;
using System.Xml.Linq;
using System.Text;
using System.IO.Ports;

namespace AutoTestSystem.Equipment.ControlDevice
{
    class ExeProcess : ControlDeviceBase
    {
        //[Category("Operation"), Description("")]
        //public bool Blocking { get; set; } = true;

        private int TotalTimeout;
        private Process p;  // 提升為類級變數
        private readonly object bufferLock = new object();
        private readonly Queue<string> dataQueue = new Queue<string>();
        private string ExexeName = string.Empty;
        private string tempBuffer = string.Empty;
        public override void Dispose()
        {
            if (p != null)
            {
                if (!string.IsNullOrEmpty(ExexeName))
                    KillProcessByExeName(ExexeName);
                try { if (!p.HasExited) p.Kill(); } catch { }
                try { p.Close(); } catch { }
                p.Dispose();
                p = null;
            }
            LogMessage("ExeProcess Dispose");
        }
        public ExeProcess()
        {
            // 創建新的 Process
            p = new Process();
        }
        public override bool Init(string strParamInfo)
        {
            return true;
        }

        public override bool UnInit()
        {
            Dispose();
            return true;
        }

        public override void SetTimeout(int time)
        {
            TotalTimeout = time;
        }
        public override bool CheckParam()
        {
            if(p != null)
            {
                if (p.HasExited && dataQueue.Count == 0)
                    return true;
                else
                    return false;
            }
            else
                return true;
        }
        public override bool Clear()
        {
            try
            {
                if(!string.IsNullOrEmpty(tempBuffer))
                    LogMessage("Response:\n" + tempBuffer);

                lock (bufferLock)
                {                   
                    dataQueue.Clear();
                }
                //if (p != null && !p.HasExited)
                //{
                //    p.Kill();
                //    p.Dispose();
                //}
                LogMessage("ExeProcess Clear Queue");
                return true;
            }
            catch (Exception ex)
            {
                LogMessage($"ClearBuffer Fail.{ex.Message}");
                return false;
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

        public override bool SEND(string data)
        {
            try
            {
                ExexeName = string.Empty;
                string[] parts = data.Split(new char[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 1)
                    return false;

                if (parts[0] == "cmd.exe")
                {
                    p.StartInfo.FileName = "cmd.exe";
                    p.StartInfo.Arguments = parts.Length == 2 ? "/c " + parts[1] : "";
                    ExexeName = ExtractExeName(parts[1]);
                }
                else
                {
                    string exePath = parts[0];  // 可以是相對路徑或絕對路徑

                    // 檢查是否為相對路徑，若是則拼接為絕對路徑
                    if (!Path.IsPathRooted(exePath))
                        exePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, exePath);

                    // 檢查檔案是否存在於指定路徑
                    if (!File.Exists(exePath))
                    {
                        // 如果檔案不存在，嘗試從系統路徑尋找
                        exePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), parts[0]);

                        // 如果檔案仍然不存在，拋出異常
                        if (!File.Exists(exePath))
                            throw new FileNotFoundException($"無法找到可執行檔：{parts[0]}");
                    }

                    // 設定啟動程式的路徑和參數
                    p.StartInfo.FileName = exePath;
                    p.StartInfo.WorkingDirectory = Path.GetDirectoryName(exePath);  // 設定工作目錄
                    p.StartInfo.Arguments = parts.Length == 2 ? parts[1] : "";  // 設定命令行引數
                }

                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardInput = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;
                p.StartInfo.CreateNoWindow = true;

                p.ErrorDataReceived += (sender, e) => {
                    lock (bufferLock)
                    {
                        if (!string.IsNullOrEmpty(e.Data))
                        {
                            dataQueue.Enqueue(e.Data + Environment.NewLine);

                            // 根據自定義換行符分割數據
                            tempBuffer += e.Data + Environment.NewLine; // 暫存數據到緩衝區
                        }
                    }
                };
                p.OutputDataReceived += (sender, e) =>
                {
                    lock (bufferLock)
                    {
                        if (!string.IsNullOrEmpty(e.Data))
                        {

                            dataQueue.Enqueue(e.Data + Environment.NewLine);

                            // 根據自定義換行符分割數據
                            tempBuffer += e.Data + Environment.NewLine; // 暫存數據到緩衝區
                        }
                    }
                };
                p.Exited += (sender, e) =>
                {
                    p.WaitForExit();  // 確保所有輸出讀取完成
                };
                p.Start();
                p.BeginOutputReadLine();
                p.BeginErrorReadLine();

                return true;
            }
            catch (Exception ex)
            {
                LogMessage($"{ex.Message}");
                return false;
            }
        }

        static string ExtractExeName(string output)
        {
            // 使用正則表達式來查找所有以 .exe 結尾的單詞
            var regex = new System.Text.RegularExpressions.Regex(@"\b(\S+\.exe)\b");
            var match = regex.Match(output);

            if (match.Success)
            {
                return match.Value; // 返回找到的程式名
            }
            return string.Empty; // 如果沒有找到 .exe 文件名
        }


        public void KillProcessByExeName(string exeName)
        {
            try
            {
                Process[] processes = Process.GetProcessesByName(exeName.Replace(".exe", ""));

                if (processes.Length == 0)
                {
                    LogMessage($"No process found with the name {exeName}");
                    return;
                }

                foreach (var process in processes)
                {
                    try
                    {
                        LogMessage($"Found process: {process.ProcessName} (PID: {process.Id})");

                        process.Kill();
                        LogMessage($"Process {process.ProcessName} (PID: {process.Id}) killed successfully.");
                    }
                    catch (Exception ex)
                    {
                        LogMessage($"Failed to kill process {process.ProcessName} (PID: {process.Id}). Error: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                LogMessage($"Error: {ex.Message}");
            }
        }
    }
}
