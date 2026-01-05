using System;
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
using Newtonsoft.Json.Linq;
using OpenCvSharp;
using System.Diagnostics;
using System.Threading;
using AutoTestSystem.DUT;
using System.IO.Ports;
using System.ComponentModel;

namespace AutoTestSystem.Equipment.ControlDevice
{
    class CMD: ControlDeviceBase
    {
        [Category("Operation"), Description("")]
        public bool Blocking { get; set; } = true;
        
        // 超時設定

        private int TotalTimeout;

        private string tempBuffer = string.Empty;
        private readonly object bufferLock = new object();
        private readonly Queue<string> dataQueue = new Queue<string>();
        DosCmd doscmd = new DosCmd();

       
        public override bool READ(ref string output)
        {
            lock (bufferLock)
            {
                if (dataQueue.Count > 0)
                {
                    output = dataQueue.Dequeue();
                    LogMessage("Response:\n" + output);
                    return true;
                }
            }
            return false;
        }

        public override void Dispose()
        {
            throw new NotImplementedException();
        }

        public override bool Init(string strParamInfo)
        {
            return true;
        }

        public override bool UnInit()
        {
            return true;
        }

        public override void SetTimeout(int time)
        {
            TotalTimeout = time;
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
        public override bool SEND(string data)
        {
            try
            {
                if (Blocking)
                {
                    using (var p = new Process())
                    {
                        p.StartInfo.FileName = "cmd.exe";
                        string batFilePath = string.Empty;

                        p.StartInfo.Arguments = "/c " + data;

                        p.StartInfo.UseShellExecute = false;
                        p.StartInfo.RedirectStandardInput = true;
                        p.StartInfo.RedirectStandardOutput = true;// 確保子程序完全執行結束與ReadToEnd搭配同步阻塞效果
                        p.StartInfo.RedirectStandardError = true;
                        p.StartInfo.CreateNoWindow = true;
                        var error = "";
                        p.ErrorDataReceived += (sender, e) => { error += e.Data; };
                        LogMessage($"Send Data: {data}");
                        p.Start();
                        p.BeginErrorReadLine();
                        var output = p.StandardOutput.ReadToEnd();// 同步捕獲標準輸出
                        p.WaitForExit(TotalTimeout);// 確保子程序完全執行結束
                        p.Close();

                        //p.StartInfo.FileName = "C:\\Users\\X.DESKTOP-GCMMSI2\\Desktop\\MTE_MFGX-master\\MTE_MFGX-master\\AutoTestSystem\\bin\\Debug\\Utility\\EO5002_IQxel_BT\\ATSuite.exe";
                        //p.StartInfo.WorkingDirectory = @"C:\Users\X.DESKTOP-GCMMSI2\Desktop\MTE_MFGX-master\MTE_MFGX-master\AutoTestSystem\bin\Debug\Utility\EO5002_IQxel_BT\\";
                        ////p.StartInfo.Arguments = "/c " + data;

                        //p.StartInfo.UseShellExecute = false;
                        ////p.StartInfo.Verb = "runas";  // 要求以管理員身份執行
                        //p.StartInfo.RedirectStandardInput = true;
                        //p.StartInfo.RedirectStandardOutput = true;// 確保子程序完全執行結束與ReadToEnd搭配同步阻塞效果
                        //p.StartInfo.RedirectStandardError = true;
                        //p.StartInfo.CreateNoWindow = true;
                        //var error = "";

                        //p.ErrorDataReceived += (sender, e) => { error += e.Data; };

                        //// 逐行讀取輸出
                        //p.OutputDataReceived += (sender, e) =>
                        //{
                        //    if (!string.IsNullOrEmpty(e.Data))
                        //    {
                        //        lock (bufferLock)
                        //        {
                        //            dataQueue.Enqueue(e.Data);
                        //        }
                        //        LogMessage($"{e.Data}");
                        //    }

                        //};

                        //p.Start();
                        //// 開始非同步讀取標準輸出和錯誤輸出
                        //p.BeginOutputReadLine();

                        //p.BeginErrorReadLine();
                        ////var output = p.StandardOutput.ReadToEnd();// 同步捕獲標準輸出
                        //p.WaitForExit(TotalTimeout);// 確保子程序完全執行結束
                        ////p.Close();
                        //p.Kill();
                    }
                }
                else
                {
                    using (var p = new Process())
                    {
                        p.StartInfo.FileName = "cmd.exe";
                        p.StartInfo.Arguments = "/c " + data;
                        p.StartInfo.UseShellExecute = false;
                        p.StartInfo.RedirectStandardInput = true;
                        p.StartInfo.RedirectStandardOutput = false;
                        p.StartInfo.CreateNoWindow = true;
                        LogMessage($"Send Data: {data}");
                        p.Start();
                        Thread.Sleep(TotalTimeout);
                        p.Close();
                    }
                    lock (bufferLock)
                    {
                        dataQueue.Enqueue("{\"Status\":\"PASS\"}");
                    }
                }
            }
            catch(Exception ex)
            {
                LogMessage($"{ex.Message}");
                return false;
            }


            return true;
            
        }
    }
}

