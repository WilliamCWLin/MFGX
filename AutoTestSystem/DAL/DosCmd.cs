using System.Diagnostics;
using System.Text;
using System.Threading;
using static AutoTestSystem.BLL.Bd;

namespace AutoTestSystem.DAL
{
    public class DosCmd : Communication
    {
        public string SendCommand3(string cmd)
        {
            Process process = new Process();
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.Arguments = "/c " + cmd;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;
            process.Start();
            process.BeginErrorReadLine();
            string output = process.StandardOutput.ReadToEnd();
            process.Close();

            return output;
        }
        //public bool SendCommand2(string command, ref string strRecAll, string DataToWaitFor, int timeout = 10)
        //{
        //    Logger.Debug($"DosSendComd-->{command}");
        //    bool rResult = false;
        //    using (Process process = new Process())
        //    {
        //        //process.StartInfo.WorkingDirectory = "";
        //        process.StartInfo.FileName = "cmd.exe";
        //        process.StartInfo.Arguments = "/c " + command;
        //        process.StartInfo.UseShellExecute = false;
        //        process.StartInfo.RedirectStandardInput = true;   //接受来自调用程序的输入信息
        //        process.StartInfo.RedirectStandardOutput = true;
        //        process.StartInfo.RedirectStandardError = true;
        //        process.StartInfo.CreateNoWindow = true;          //不显示程序窗口
        //        StringBuilder output = new StringBuilder();
        //        StringBuilder error = new StringBuilder();

        //        using (AutoResetEvent outputWaitHandle = new AutoResetEvent(false))
        //        using (AutoResetEvent errorWaitHandle = new AutoResetEvent(false))
        //        {
        //            process.OutputDataReceived += (sender, e) =>
        //            {
        //                if (e.Data == null)
        //                {
        //                    outputWaitHandle.Set();
        //                }
        //                else
        //                {
        //                    output.AppendLine(e.Data);
        //                }
        //            };
        //            process.ErrorDataReceived += (sender, e) =>
        //            {
        //                if (e.Data == null)
        //                {
        //                    errorWaitHandle.Set();
        //                }
        //                else
        //                {
        //                    error.AppendLine(e.Data);
        //                }
        //            };

        //            process.Start();

        //            process.BeginOutputReadLine();
        //            process.BeginErrorReadLine();

        //            if (process.WaitForExit(timeout * 1000) &&
        //                outputWaitHandle.WaitOne(timeout * 1000) &&
        //                errorWaitHandle.WaitOne(timeout * 1000))
        //            {
        //                // Process completed. Check process.ExitCode here.
        //                if (output.ToString().Contains(DataToWaitFor) && string.IsNullOrEmpty(error.ToString()))
        //                {
        //                    Logger.Info($"Waiting for:{DataToWaitFor} succeed!!");
        //                    rResult = true;
        //                }
        //            }
        //            else
        //            {
        //                // Timed out.
        //                Logger.Error($"Waiting for:{DataToWaitFor} TimeOut({timeout}),FAIL!, Error:{error.ToString()}");
        //            }
        //            strRecAll = output.ToString();
        //            Logger.Debug(output.ToString());
        //        }
        //    }
        //    return rResult;
        //}
        public bool SendNonBlockCommand(string command, int sleeptime)
        {
            bool rResult = false;
            Logger.Debug($"DosSendComd-->{command}");
            //说明：不管命令是否成功均执行exit命令，否则当调用ReadToEnd()方法时，会处于假死状态
            using (var p = new Process())
            {
                p.StartInfo.FileName = "cmd.exe";
                p.StartInfo.Arguments = "/c " + command;
                p.StartInfo.UseShellExecute = false; //是否使用操作系统shell启动
                p.StartInfo.RedirectStandardInput = true; //接受来自调用程序的输入信息
                p.StartInfo.RedirectStandardOutput = false; //由调用程序获取输出信息
                p.StartInfo.RedirectStandardError = false; //重定向标准错误输出
                p.StartInfo.CreateNoWindow = true; //不显示程序窗口
                //p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                //p.StartInfo.WorkingDirectory = directoryPath;
                var error = "";
                p.ErrorDataReceived += (sender, e) => { error += e.Data; };
                p.Start();
                Sleep(sleeptime);
                p.Close();
            }
            return rResult;
        }
        public override bool SendCommand(string command, ref string strRecAll, string DataToWaitFor, int timeout = 3)
        {
            bool rResult = false;
            Logger.Debug($"DosSendComd-->{command}");
            //说明：不管命令是否成功均执行exit命令，否则当调用ReadToEnd()方法时，会处于假死状态
            using (var p = new Process())
            {
                p.StartInfo.FileName = "cmd.exe";
                p.StartInfo.Arguments = "/c " + command;
                p.StartInfo.UseShellExecute = false; //是否使用操作系统shell启动
                p.StartInfo.RedirectStandardInput = true; //接受来自调用程序的输入信息
                p.StartInfo.RedirectStandardOutput = true; //由调用程序获取输出信息
                p.StartInfo.RedirectStandardError = true; //重定向标准错误输出
                p.StartInfo.CreateNoWindow = true; //不显示程序窗口
                //p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                //p.StartInfo.WorkingDirectory = directoryPath;
                var error = "";
                p.ErrorDataReceived += (sender, e) => { error += e.Data; };
                p.Start();
                //p.StandardInput.WriteLine(command);
                //p.StandardInput.AutoFlush = true;
                //p.StandardInput.Close();
                p.BeginErrorReadLine(); //获取cmd窗口的输出信息
                var output = p.StandardOutput.ReadToEnd();
                p.WaitForExit(timeout*1000);
                p.Close();
                Logger.Debug(output);
                if (output.Contains(DataToWaitFor))
                {
                    Logger.Info($"Waiting for:{DataToWaitFor} succeed!!");
                    rResult = true;
                }
                else
                {
                    // Timed out.
                    Logger.Error($"Waiting for:{DataToWaitFor} TimeOut({timeout}),FAIL!, Error:{error.ToString()}");
                }
                strRecAll = output;
            }
            return rResult;
        }

        public override void Close()
        {
            throw new System.NotImplementedException();
        }

        public override void Dispose()
        {
            throw new System.NotImplementedException();
        }

        public override bool Open()
        {
            throw new System.NotImplementedException();
        }

        public override string Read()
        {
            throw new System.NotImplementedException();
        }

        public override void Write(string data)
        {
            throw new System.NotImplementedException();
        }
    }
}
