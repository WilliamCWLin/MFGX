using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AutoTestSystem.DAL;
using static AutoTestSystem.BLL.Bd;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using AutoTestSystem.Base;
using System.Threading;
using System.Net.Http;
using System.Net;

using System.Reflection;
using System.Security.Policy;
using AutoTestSystem.Model;

namespace AutoTestSystem.Equipment.ControlDevice
{
    class MES : ControlDeviceBase
    {
        private int TotalTimeout;

        private readonly object bufferLock = new object();
        private readonly Queue<string> dataQueue = new Queue<string>();
        string MESTCMD = string.Empty;
        string MESTDATA = string.Empty;
        string Checkstr = string.Empty;

        // 取得方法
        static object Sajet;
        static MethodInfo SajetTransStart;
        static MethodInfo SajetTransClose;
        static MethodInfo SajetTransData;
        [ReadOnly(true)]
        [Category("Params"), Description("請SajectConnect.dll及Chroma.xml跟sajet.ini放在主程式目錄")]
        public string DLL_PATH { get; set; } = "SajetConnect.dll";
        public override bool Init(string strParamInfo)
        {
            try
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
                if (Sajet == null)
                {
                    string dllPath = DLL_PATH;
                    // 載入 DLL
                    //Assembly assembly = Assembly.LoadFrom("Utility\\MES\\SajetConnect.dll");
                    string baseDirectory = Path.GetDirectoryName(Path.GetFullPath(dllPath)); // 獲取目錄 Utility\MES
                                                                                             // 檢查路徑檔案是否存在
                    if (!File.Exists(dllPath))
                    {
                        MessageBox.Show($"{dllPath}.檔案不存在。\nFile does not exist.");
                        return false;
                    }
                    if (!File.Exists(baseDirectory + "\\Chroma.xml") || !File.Exists(baseDirectory + "\\sajet.ini"))
                    {
                        MessageBox.Show($"Chroma.xml or sajet.ini.檔案不存在。\nFile does not exist.");
                        return false;
                    }

                    // 設定檔案載入事件
                    //AppDomain.CurrentDomain.ResourceResolve += (sender, args) =>
                    //{
                    //    string resourceName = args.Name;
                    //    string resourcePath = Path.Combine(baseDirectory, resourceName);

                    //    if (File.Exists(resourcePath))
                    //    {
                    //        LogMessage($"Loading missing resource: {resourcePath}");
                    //        return Assembly.LoadFrom(resourcePath);
                    //    }
                    //    return null; // 找不到時返回 null，系統會拋出錯誤
                    //};

                    // 載入 DLL
                    Assembly assembly = Assembly.LoadFrom(dllPath);
                    // 取得類型
                    Type type = assembly.GetType("SajetConnect.SajetConnect");

                    // 建立實例
                    Sajet = Activator.CreateInstance(type);

                    // 取得方法
                    SajetTransStart = type.GetMethod("SajetTransStart");
                    SajetTransClose = type.GetMethod("SajetTransClose");
                    SajetTransData = type.GetMethod("SajetTransData", new Type[] { typeof(string), typeof(string) });

                }

                if (Sajet != null)
                {
                    MoveFilesToMainDirectory();
                    object result = SajetTransStart.Invoke(Sajet, null);
                    string message = result as string;
                    LogMessage(message);
                    if (message.Contains("NG"))
                    {
                        return false;
                    }
                    
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return false;
            }
            return true;
        }
        public override bool SEND(string input)
        {
            if (Simu)
            {
                MessageBox.Show("模擬模式啟用中，請勿用於生產。\nSimulation mode is enabled, please do not use in production.");

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

            if (Sajet != null)
            {
                try
                {
                    string[] parts = input.Split(';');
                    string firstPart = parts[0];
                    string MEStCMD = string.Empty;
                    string MEStData = string.Empty;
                    if (parts.Length >= 2)
                    {
                        MEStCMD = parts[0];
                        MEStData = string.Join(";", parts.Skip(1));
                    }
                    else
                    {
                        LogMessage($"{input}.input format error.");
                        return false;
                    }

                    if (GlobalNew.CurrentMode == "ENG")
                    {
                        if(MEStCMD == "C002" || MEStCMD == "C003" || MEStCMD == "C004")
                        {
                            MessageBox.Show($"工程模式(ENG MODE)，BYPASS({MEStCMD})請勿用於生產。\nENG mode is enabled, please do not use in production.");

                            try
                            {
                                lock (bufferLock)
                                {
                                    dataQueue.Enqueue("OK;ENG Simu Data");
                                }
                            }
                            catch (Exception ex)
                            {
                                LogMessage("工程模式(ENG MODE)" + ex.Message);
                            }

                            return true;
                        }
                    }

                    object result = SajetTransData.Invoke(Sajet, new object[] { MEStCMD, MEStData });
                    string data = result as string;
                    //var jsonObject = new { data = data };
                    //string JSON = JsonConvert.SerializeObject(jsonObject);

                    lock (bufferLock)
                    {
                        dataQueue.Enqueue(data);
                    }

                    return true;

                }
                catch (Exception e)
                {
                    LogMessage(e.Message);
                    return false;
                }
            }

            return false;
        }
        public override bool MESDataprocess(string MEStCMD, string MEStData, string checkStr, ref string strOutData)
        {
            if (Sajet != null)
            {
                try
                {
                    if (GlobalNew.CurrentMode == "ENG")
                    {
                        if (MEStCMD == "C002" || MEStCMD == "C003" || MEStCMD == "C004")
                        {
                            MessageBox.Show($"工程模式(ENG MODE)，BYPASS({MEStCMD})請勿用於生產。\nENG mode is enabled, please do not use in production.");

                            try
                            {
                                var json = JsonConvert.SerializeObject(new { data = "OK;ENG Simu Data" });
                                strOutData = json;
                                lock (bufferLock)
                                {
                                    dataQueue.Enqueue(json);
                                }
                            }
                            catch (Exception ex)
                            {
                                LogMessage("工程模式(ENG MODE)" + ex.Message);
                            }

                            return true;
                        }
                    }
                    object result = SajetTransData.Invoke(Sajet, new object[] { MEStCMD, MEStData });
                    string data = result as string;
                    //注意工廠用URL模式會回JSON格式，所以為維持舊腳本PARSE需序列化JSON data
                    var jsonObject = new { data = data };
                    string JSON = JsonConvert.SerializeObject(jsonObject);

                    strOutData = JSON;
                    LogMessage(strOutData);

                    lock (bufferLock)
                    {
                        dataQueue.Enqueue(strOutData);
                    }
                    if (strOutData.Contains(checkStr))
                        return true;
                    else
                        return false;
                }
                catch (Exception e)
                {
                    LogMessage(e.Message);
                    return false;
                }
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
                    LogMessage(output);
                    return true;
                }
            }
            return false;
        }

        public override void Dispose()
        {
            throw new NotImplementedException();
        }


        public string GetAbsoluteDirectoryPath()
        {
            string fullPath = Path.GetFullPath(DLL_PATH);
            string directoryPath = Path.GetDirectoryName(fullPath);
            return directoryPath;
        }

        public void MoveFilesToMainDirectory()
        {
            //string sourceDirectory = GetAbsoluteDirectoryPath();
            //string mainDirectory = AppDomain.CurrentDomain.BaseDirectory;

            string sourceDirectory = Path.GetFullPath(GetAbsoluteDirectoryPath().TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
            string mainDirectory = Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));

            if (sourceDirectory.Equals(mainDirectory, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            string[] filesToMove = { "sajet.ini", "Chroma.xml" };

            try
            {
                foreach (string fileName in filesToMove)
                {
                    string sourceFile = Path.Combine(sourceDirectory, fileName);
                    string destinationFile = Path.Combine(mainDirectory, fileName);

                    if (File.Exists(sourceFile))
                    {
                        File.Copy(sourceFile, destinationFile, true); // 移動文件
                    }
                    else
                    {
                        LogMessage($"{fileName} 不存在於源目錄中。");
                    }
                }
            }
            catch (Exception ex)
            {
                LogMessage(ex.Message);
            }
        }
        public override bool UnInit()
        {
            if (Simu)
            {
                return true;
            }
            if (Sajet != null)
            {
                object result = SajetTransClose.Invoke(Sajet, null);
                string message = result as string;
                LogMessage(message);
            }

            return true;
        }

        public override void SetTimeout(int time)
        {

        }
        public override void SetCheckstr(string str)
        {
            Checkstr = str;
        }

        public string CreateDataString(Dictionary<string, object> data)
        {
            try
            {
                string jsonStr = JsonConvert.SerializeObject(data);
                return jsonStr;
            }
            catch (Exception ex)
            {
                // 處理轉換錯誤
                return $"轉換為 JSON 字串時出現錯誤: {ex.Message}";
            }
        }

        public override void SetMEStcmd(string str)
        {
            MESTCMD = str;
        }

        public override void SetMEStdata(string str)
        {
            MESTDATA = str;
        }
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
    }
}

