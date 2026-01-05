using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using static AutoTestSystem.BLL.Bd;

namespace AutoTestSystem.DAL
{
    public class AsyncSocket : Communication
    {
        private byte[] m_byBuff = new byte[1024 * 16];
        public Socket socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
        private string strWorkingData = "";  // 保存从服务器端接收到的数据

        public StringBuilder SessionLog = new StringBuilder();
        private IPAddress ipAdd;

        //private int retry = 0;
        public int retryTimes = 1;

        public AsyncSocket(string Address, int Port = 23)
        {
            HostIP = Address;
            base.Port = Port;
        }

        public AsyncSocket(string Address, string _logPath, int Port = 23)
        {
            HostIP = Address;
            base.Port = Port;
            logPath = _logPath;
        }

        public override bool Open()
        {
            Logger.Debug("start connect socket...");
            SessionLog.Clear();
            try
            {
                if (!IPAddress.TryParse(HostIP, out ipAdd)) //确定一个字符串是否是有效的IP地址。
                {
                    Logger.Error($"{HostIP} IP Address is invalid");
                    return false;
                }
                PingIP(HostIP, 5);
                IPEndPoint iep = new IPEndPoint(ipAdd, Port);
                // Try a blocking connection to the server
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                {
                    NoDelay = true
                };
                socket.Connect(iep);
                //异步回调AsyncCallback recieveData = new AsyncCallback(OnRecievedData);
                socket.BeginReceive(m_byBuff, 0, m_byBuff.Length, 0, new AsyncCallback(OnRecievedData), socket);
                //bool loginResult = WaitFor("TelnetLogin", exceptStr, 30);
                SessionLog.Clear();
                return true;
            }
            catch (Exception ex)
            {
                Logger.Fatal($"socket connection Exception... {ex.StackTrace}");
                return false;
            }
        }

        public override void Close()
        {
            try
            {
                if (socket.Connected)
                {
                    socket.Close();
                    Logger.Debug("Close connect succeed.");
                    //socket.Dispose();
                }
            }
            catch (Exception ex)
            {
                Logger.Fatal("DisConnect Exception:" + ex);
                throw;
            }
        }

        /// <summary>
        /// 当接收完成后,执行的方法(供委托使用)
        /// </summary>
        /// <param name="ar"></param>
        private void OnRecievedData(IAsyncResult ar)
        {
            string mOutText = "";
            try
            {
                //从参数中获得给的socket 对象
                Socket sock = (Socket)ar.AsyncState;

                //EndReceive方法为结束挂起的异步读取
                int nBytesRec = sock.EndReceive(ar);
                //如果有接收到数据的话
                if (nBytesRec > 0)
                {
                    //声明一个字符串,用来存储解析过的字符串
                    string m_strLine = "";
                    //遍历Socket接收到的字符
                    /* 此循环用来调整linux 和 windows在换行上标记的区别
                     * 最后将调整好的字符赋予给 m_strLine */
                    for (int i = 0; i < nBytesRec; i++)
                    {
                        Char ch = Convert.ToChar(m_byBuff[i]);
                        switch (ch)
                        {
                            case '\r':
                                m_strLine += Convert.ToString("\r\n");
                                break;

                            case '\n':
                                break;

                            default:
                                m_strLine += Convert.ToString(ch);
                                break;
                        }
                    }

                    ////获得转义后的字符串的长度
                    //int strLinelen = m_strLine.Length;
                    ////如果长度为零
                    //if (strLinelen == 0)
                    //{
                    //    //则返回"\r\n" 即回车换行
                    //    m_strLine = Convert.ToString("\r\n");
                    //}
                    mOutText = m_strLine;
                    if (mOutText != "")
                    {
                        //Console.Write(mOutText);
                        mOutText = mOutText.Replace("\b", "");
                        Logger.Debug(mOutText);
                        SessionLog.Append(mOutText);
                        strWorkingData = mOutText;
                    }
                }
                else// 如果没有接收到任何数据的话
                {
                    Logger.Debug("OnRecievedData close,disconnect socket!!!");
                    sock.Shutdown(SocketShutdown.Both);
                    sock.Close();
                }
            }
            catch (SocketException ex) //接收时telnet断连retry处理
            {
                //Logger.Debug($"{ex.Message} DisConnect telnet。");
                Close();
                Logger.Fatal($"接收数据时出现异常:{ex.ToString()}");
                //Console.WriteLine($"{ex.ToString()} 重新连接Telnet,Retry.....");
                mOutText = "";
                SessionLog.Clear();
                //Thread.Sleep(3000);
                //Connect(Global.PROMPT);
            }
            catch (ObjectDisposedException) //对于ObjectDisposedException异常直接忽略
            {
            }
            catch (Exception ex)
            {
                Logger.Fatal(SessionLog.ToString());
                Logger.Fatal("接收数据时出现异常：" + ex.ToString());
                mOutText = "";
                SessionLog.Clear();
                //throw;
            }
        }

        #region magic Function

        /// <summary>
        /// 将信息转化成charp[] 流的形式,使用socket 进行发出
        /// 发出结束之后,使用一个匿名委托,进行接收,
        /// 之后这个委托里,又有个委托,意思是接受完了之后执行OnRecieveData 方法
        /// </summary>
        /// <param name="strText"></param>
        private void DispatchMessage(string strText)
        {
            try
            {
                //申请一个与字符串相当长度的char流
                Byte[] smk = new Byte[strText.Length];
                for (int i = 0; i < strText.Length; i++)
                {
                    //解析字符串,将其存储到char流中去
                    Byte ss = Convert.ToByte(strText[i]);
                    smk[i] = ss;
                }
                //发送char流,之后发送完毕后执行委托中的方法(此处为匿名委托)
                IAsyncResult ar2 = socket.BeginSend(smk, 0, smk.Length, SocketFlags.None, delegate (IAsyncResult ar)
                {
                    //当执行完"发送数据" 这个动作后
                    // 获取Socket对象,对象从beginsend 中的最后个参数上获得
                    Socket sock1 = (Socket)ar.AsyncState;
                    if (sock1.Connected)//如果连接还是有效
                    {
                        sock1.BeginReceive(m_byBuff, 0, m_byBuff.Length, 0, new AsyncCallback(OnRecievedData), sock1);
                    }
                }, socket);

                socket.EndSend(ar2);
            }
            catch (Exception ers)
            {
                Logger.Fatal("Error!,When DispatchMessage:" + ers.ToString());
                throw;
            }
        }

        public bool WaitFor(string command, string DataToWaitFor, int timeout)
        {
            try
            {
                long lngStart = DateTime.Now.AddSeconds(timeout).Ticks;
                long lngCurTime = 0;

                while (strWorkingData.ToLower().IndexOf(DataToWaitFor.ToLower()) == -1)
                {
                    lngCurTime = DateTime.Now.Ticks;
                    if (lngCurTime > lngStart)
                    {
                        Logger.Error($"Send command:{command},waiting for:{DataToWaitFor},TimeOut({timeout}), FAIL!!! ");
                        strWorkingData = "";
                        return false;
                    }
                    else
                    {
                        Thread.Sleep(1); //垃圾回收，优化CPU占用率问题
                    }
                }
                strWorkingData = "";
                return true;
            }
            catch (Exception)
            {
                strWorkingData = "";
                return false;
                throw;
            }
        }

        /// <summary>
        /// 发送命令，在timeout时间内接收到期待的字符串，并返回全部接收的字符串。
        /// </summary>
        /// <param name="cmd">命令</param>
        /// <param name="recvStr">发送后的全部接收</param>
        /// <param name="waitforStr">期待的字符串</param>
        /// <param name="timeout">超时时间</param>
        /// <returns></returns>
        public override bool SendCommand(string cmd, ref string recvStr, string waitforStr, int timeout = 10)
        {
            try
            {
                //Thread.Sleep(10); //连续发命令的间隔
                // message = message + "\r\n";
                Logger.Debug($"SendCommand-->{cmd}");
                recvStr = "";
                SessionLog.Clear();
                DispatchMessage(cmd);
                DispatchMessage("\n");
                bool receiveResult = WaitFor(cmd, waitforStr, timeout);
                recvStr = SessionLog.ToString();
                //Logger.Debug(recvStr);
                if (!receiveResult)
                {
                    PingIP(HostIP, 5);
                }
                //if (!receiveResult && retry < retryTimes)
                //{
                //    retry++;
                //    Logger.Debug(recvStr);
                //    Logger.Debug($"发送命令没得到期待提示符，retry发送第 {retry} 次：");
                //    receiveResult = TelnetSend(message, ref recvStr, waitforStr, timeout);
                //}
                //retry = 0;
                return receiveResult;
            }
            catch (SocketException ex) //发送命令时telnet断连retry处理--
            {
                //socket.Shutdown(SocketShutdown.Both);
                //socket.Close();
                Logger.Fatal($"SocketException ex:{ex.ToString()},Reconnect ,Retry.....");
                Close();
                if (Open())
                {
                    return SendCommand(cmd, ref recvStr, waitforStr, timeout);
                }
                else
                {
                    Logger.Error($"Reconnect failed！！！");
                    return false;
                }
            }
            catch (ObjectDisposedException)
            {
                if (Open())
                {
                    return SendCommand(cmd, ref recvStr, waitforStr, timeout);
                }
                else
                {
                    Logger.Error($"Reconnect failed！！！");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logger.Fatal($"{ex.ToString()}");
                recvStr = SessionLog.ToString();
                return false;
            }
        }

        #endregion magic Function

        public override void Write(string data)
        {
            DispatchMessage(data);
        }

        public override void WriteLine(string data)
        {
            DispatchMessage(data);
            DispatchMessage("\n");
        }

        public override void Dispose()
        {
            ((IDisposable)socket).Dispose();
        }

        public override string Read()
        {
            throw new NotImplementedException();
        }
    }
}