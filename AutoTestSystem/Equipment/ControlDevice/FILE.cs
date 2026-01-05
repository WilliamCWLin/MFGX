using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using AutoTestSystem.Base;

namespace AutoTestSystem.Equipment.ControlDevice
{
    class FILE : ControlDeviceBase
    {
        private int TotalTimeout;
        private readonly object bufferLock = new object();
        private readonly Queue<string> dataQueue = new Queue<string>();

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

        public override void Dispose() { }
        public override bool Init(string strParamInfo) => true;
        public override bool UnInit() => true;
        public override void SetTimeout(int time) => TotalTimeout = time;
        public override bool Clear()
        {
            try
            {
                dataQueue.Clear();
                LogMessage("ClearBuffer");
                return true;
            }
            catch (Exception ex)
            {
                LogMessage($"ClearBuffer Fail.{ex.Message}");
                return false;
            }
        }

        public override bool SEND(string data)
        {
            try
            {
                string[] parts = data.Split(new char[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
                string command;
                string argument;

                if (parts.Length == 1)
                {
                    command = "readalltext";
                    argument = parts[0];
                }
                else if (parts.Length == 2)
                {
                    command = parts[0];
                    argument = parts[1];
                }
                else
                {
                    LogMessage("Invalid command format");
                    return false;
                }

                string result;

                switch (command.ToLower())
                {
                    case "readalltext":
                        result = ReadAllText(argument);
                        break;
                    case "move":
                        result = Move(argument);
                        break;
                    case "copy":
                        result = Copy(argument);
                        break;
                    case "delete":
                        result = Delete(argument);
                        break;
                    case "exists":
                        result = Exists(argument);
                        break;
                    default:
                        result = "Unknown command";
                        break;
                }

                lock (bufferLock)
                {
                    dataQueue.Enqueue(result);
                }
                return true;
            }
            catch (Exception ex)
            {
                lock (bufferLock)
                {
                    dataQueue.Enqueue($"Error: {ex.Message}");
                }
                return false;
            }
        }

        //private string ReadAllText(string filePath)
        //{
        //    try
        //    {
        //        return File.Exists(filePath) ? File.ReadAllText(filePath) : "File not found";
        //    }
        //    catch (Exception ex)
        //    {
        //        return $"ReadAllText Error: {ex.Message}";
        //    }
        //}
        private string ReadAllText(string filePath)
        {
            try
            {
                // 記錄開始時間
                DateTime startTime = DateTime.Now;

                // 持續檢查檔案是否存在，直到超過指定的時間或檔案出現
                while (!File.Exists(filePath))
                {
                    // 如果已經超過 timeout 時間，則停止檢查
                    if ((DateTime.Now - startTime).TotalMilliseconds > TotalTimeout)
                        return $"ReadAllText Timeout: {filePath} not found";

                    Thread.Sleep(1000); // 每秒檢查一次
                }

                // 檔案存在後，讀取檔案內容
                return File.ReadAllText(filePath);
            }
            catch (Exception ex)
            {
                return $"ReadAllText Error: {ex.Message}";
            }
        }
        private string Move(string argument)
        {
            try
            {
                // 分割來源檔案和目標檔案路徑
                string[] paths = argument.Split(' ');
                if (paths.Length < 2) return "Move command requires source and destination";

                // 取得來源和目標檔案路徑
                string sourceFile = paths[0];
                string destinationFile = paths[1];

                // 檢查目標檔案是否已經存在
                if (File.Exists(destinationFile))
                {
                    // 如果檔案已存在，先刪除目標檔案
                    File.Delete(destinationFile);
                    LogMessage($"File {destinationFile} already exists. It has been deleted and will be replaced.");
                }

                // 移動檔案
                File.Move(sourceFile, destinationFile);
                return $"Move success. Moved {sourceFile} to {destinationFile}";
            }
            catch (Exception ex)
            {
                return $"Move Error: {ex.Message}";
            }
        }

        private string Copy(string argument)
        {
            try
            {
                string[] paths = argument.Split(' ');
                if (paths.Length < 2) return "Copy command requires source and destination";

                File.Copy(paths[0], paths[1], true);

                return $"Copy success. Copy {paths[0]} overwrite to {paths[1]}";
            }
            catch (Exception ex)
            {
                return $"Copy Error: {ex.Message}";
            }
        }

        private string Delete(string filePath)
        {
            try
            {
                if (!File.Exists(filePath)) return "File not found";
                File.Delete(filePath);
                return $"Delete success. Delete {filePath}";
            }
            catch (Exception ex)
            {
                return $"Delete Error: {ex.Message}";
            }
        }

        private string Exists(string filePath)
        {
            try
            {
                return File.Exists(filePath) ? $"File({filePath}) exists" : $"File({filePath}) not found";
            }
            catch (Exception ex)
            {
                return $"Exists Error: {ex.Message}";
            }
        }
    }
}