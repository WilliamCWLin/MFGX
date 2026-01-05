using Renci.SshNet;
using System;
using System.Text;
using static AutoTestSystem.BLL.Bd;

namespace AutoTestSystem.DAL
{
    public class SSH : Communication
    {
        public SshClient sshClient;
        private ShellStream stream;
        private ConnectionInfo sshconInfo;

        public SSH(ConnectionInfo _sshconInfo)
        {
            sshconInfo = _sshconInfo;
        }

        //hostIP = Global.DUTIP, username = Global.SSH_USERNAME, password = Global.SSH_PASSWORD, sshconInfo
        public SSH(string _hostIP, string _username, string _password, ConnectionInfo _sshconInfo)
        {
            HostIP = _hostIP;
            Username = _username;
            Password = _password;
            sshconInfo = _sshconInfo;
        }

        /// <summary>
        /// 登录SSH
        /// </summary>
        /// <returns></returns>
        public override bool Open()
        {
            bool conResult = true;
            try
            {
                sshClient = new SshClient(sshconInfo);
                sshClient.Connect();
            }
            catch (Exception ex)
            {
                Logger.Debug(ex.ToString());
                conResult = false;
            }
            return conResult;
        }

        ///// <summary>
        ///// 登录SSH
        ///// </summary>
        ///// <returns></returns>
        //public override bool Open()
        //{
        //    bool conResult = true;
        //    try
        //    {
        //        sshClient = new SshClient(hostIP, username, password);
        //        sshClient.Connect();
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.Debug(ex.ToString());
        //        conResult = false;
        //    }
        //    return conResult;
        //}

        /// <summary>
        /// 登录SSH
        /// </summary>
        public bool Open(ConnectionInfo conInfo)
        {
            bool conResult = true;
            try
            {
                sshClient = new SshClient(conInfo);
                sshClient.Connect();
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.ToString());
                conResult = false;
            }
            return conResult;
        }

        public override void Close()
        {
            try
            {
                sshClient.Disconnect();
                sshClient.Dispose();
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.ToString());
                //throw;
            }
        }

        /// <summary>
        /// 用于连续发送命令
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="comd"></param>
        /// <param name="line">返回的full log</param>
        /// <param name="waitFor">需要等待的字符串</param>
        /// <param name="timeout">连续几秒没有收到流则超时</param>
        /// <returns></returns>
        public bool SendCommand(ShellStream stream, string comd, ref string line, string waitFor = "XXXXXXXX", int timeout = 3)
        {
            line = "";
            StringBuilder sb = new StringBuilder();
            try
            {
                Logger.Debug($"SshSendCommand-->{comd}");
                // Send the command
                stream.WriteLine(string.Format("{0} \n", comd));
                // Read with a suitable timeout to avoid hanging
                while ((line = stream.ReadLine(TimeSpan.FromSeconds(timeout))) != null)
                {
                    //字符串去除制表符回车符换行符
                    line = line.Replace("\n", "").Replace("\t", "").Replace("\r", "");
                    sb.Append(line.ToString() + "\n");
                    if (line.Contains(waitFor))
                    {
                        Logger.Debug($"Waiting for:{waitFor} succeed!!");
                        break;
                    }
                    // Thread.Sleep(1);
                }
                line = sb.ToString();
                Logger.Debug(line);
                return true;
            }
            catch (Exception ex)
            {
                line = sb.ToString();
                Logger.Debug(ex.ToString());
                Logger.Debug("发送命令失败！！！" + line);
                return false;
            }
        }

        /// <summary>
        /// 用于连续发送命令
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="comd"></param>
        /// <param name="line">返回的full log</param>
        /// <param name="waitFor">需要等待的字符串</param>
        /// <param name="timeout">连续几秒没有收到流则超时</param>
        /// <returns></returns>
        public override bool SendCommand(string comd, ref string line, string waitFor = "XXXXXXXX", int timeout = 3)
        {
            line = "";
            stream = sshClient.CreateShellStream("", 0, 0, 0, 0, 0);
            StringBuilder sb = new StringBuilder();
            try
            {
                Logger.Debug($"SshSendCommand-->{comd}");
                // Send the command
                stream.WriteLine(string.Format("{0} \n", comd));
                // Read with a suitable timeout to avoid hanging
                while ((line = stream.ReadLine(TimeSpan.FromSeconds(timeout))) != null)
                {
                    //字符串去除制表符回车符换行符
                    line = line.Replace("\n", "").Replace("\t", "").Replace("\r", "");
                    sb.Append(line.ToString() + "\n");
                    if (line.Contains(waitFor))
                    {
                        Logger.Info($"Waiting for:{waitFor} succeed!!");
                        break;
                    }
                    // Thread.Sleep(1);
                }
                line = sb.ToString();
                Logger.Debug(line);
                return true;
            }
            catch (Exception ex)
            {
                line = sb.ToString();
                Logger.Debug("发送命令失败！！！" + line);
                Logger.Debug(ex.ToString());
                return false;
            }
        }

        public override void Write(string data)
        {
            stream = sshClient.CreateShellStream("", 0, 0, 0, 0, 0);
            stream.Write(data);
        }

        public override void WriteLine(string data)
        {
            stream = sshClient.CreateShellStream("", 0, 0, 0, 0, 0);
            stream.WriteLine(data);
        }

        public override void Dispose()
        {
            ((IDisposable)sshClient).Dispose();
        }

        public override string Read()
        {
            stream = sshClient.CreateShellStream("", 0, 0, 0, 0, 0);
            return stream.ReadLine();
        }
    }
}