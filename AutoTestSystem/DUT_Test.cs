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
    public class DUT_Test : DUT_BASE
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
        private Simulator simulator;
        public DUT_Test()
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
            return true;
        }

        public override bool SEND(byte[] input)
        {
            return true;
        }

        public override bool READ(ref string output)
        {
            return true;
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

}
