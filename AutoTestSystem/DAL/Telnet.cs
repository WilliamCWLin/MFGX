using AutoTestSystem.Model;
using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using static AutoTestSystem.BLL.Bd;

namespace AutoTestSystem.DAL
{
    public class Telnet : Communication
    {
        #region telnet的数据定义

        /// <summary>
        /// 标志符,代表是一个TELNET 指令
        /// </summary>
        private readonly Char IAC = Convert.ToChar(255);

        /// <summary>
        /// 表示一方要求另一方使用，或者确认你希望另一方使用指定的选项。
        /// </summary>
        private readonly Char DO = Convert.ToChar(253);

        /// <summary>
        /// 表示一方要求另一方停止使用，或者确认你不再希望另一方使用指定的选项。
        /// </summary>
        private readonly Char DONT = Convert.ToChar(254);

        /// <summary>
        /// 表示希望开始使用或者确认所使用的是指定的选项。
        /// </summary>
        private readonly Char WILL = Convert.ToChar(251);

        /// <summary>
        /// 表示拒绝使用或者继续使用指定的选项。
        /// </summary>
        private readonly Char WONT = Convert.ToChar(252);

        /// <summary>
        /// 表示后面所跟的是对需要的选项的子谈判
        /// </summary>
        private readonly Char SB = Convert.ToChar(250);

        /// <summary>
        /// 子谈判参数的结束
        /// </summary>
        private readonly Char SE = Convert.ToChar(240);

        private const Char IS = '0';
        private const Char SEND = '1';
        private const Char INFO = '2';
        private const Char VAR = '0';
        private const Char VALUE = '1';
        private const Char ESC = '2';
        private const Char USERVAR = '3';

        /// <summary>
        /// 流
        /// /// </summary>
        private byte[] m_byBuff = new byte[1024 * 16];

        /// <summary>
        /// 收到的控制信息
        /// </summary>
        private ArrayList m_ListOptions = new ArrayList();

        /// <summary>
        /// 存储准备发送的信息
        /// </summary>
        private string m_strResp;

        /// <summary>
        /// 一个Socket套接字
        /// </summary>
        public Socket socket = new Socket(SocketType.Stream, ProtocolType.Tcp);

        #endregion telnet的数据定义

        private string strWorkingData = "";  // 保存从服务器端接收到的数据
        public StringBuilder SessionLog = new StringBuilder();
        private IPAddress ipAdd;
        //private int retry = 0;
        public int retryTimes = 1;

        public Telnet(TelnetInfo telnetInfo)
        {
            HostIP = telnetInfo._Address;
            Port = telnetInfo._Port;
            //logPath = telnetInfo._logPath;
        }

        public Telnet(string Address, string _logPath, int Port = 23)
        {
            HostIP = Address;
            base.Port = Port;
            logPath = _logPath;
        }

        public Telnet(string Address, int Port = 23)
        {
            HostIP = Address;
            base.Port = Port;
        }

        public override bool Open()
        {
            Logger.Debug("start login telnet...");
            SessionLog.Clear();
            try
            {
                if (!IPAddress.TryParse(HostIP, out ipAdd)) //确定一个字符串是否是有效的IP地址。
                {
                    Logger.Error($"{HostIP} IP Address is invalid");
                    return false;
                }
                //PingIP(HostIP, 5);
                //PingIP(HostIP, 5);
                //PingIP(HostIP, 5);
                //Thread.Sleep(1000);
                PingIP(HostIP, 5);
                PingIP(HostIP, 5);
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
                Logger.Fatal($"Telnet connection Exception... {ex.ToString()}");
                return false;
            }
        }

        /// <summary>
        /// 启动socket 进行telnet操作
        /// </summary>
        public override bool Open(string exceptStr)
        {
            Logger.Debug("start login telnet...");
            SessionLog.Clear();
            try
            {
                if (!IPAddress.TryParse(HostIP, out ipAdd)) //确定一个字符串是否是有效的IP地址。
                {
                    Logger.Debug($"{HostIP} IP Address is invalid");
                    return false;
                }
                //PingIP(HostIP, 5);
                //PingIP(HostIP, 5);
                //PingIP(HostIP, 5);
                //Thread.Sleep(1000);
                //PingIP(HostIP, 5);
                //PingIP(HostIP, 5);
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
                bool loginResult = WaitFor("TelnetLogin", exceptStr, 30);
                Logger.Debug(SessionLog.ToString());
                SessionLog.Clear();
                InputEvent.Set();
                if (loginResult)
                {
                    string recvStr = "";
                    SendCommand("", ref recvStr, exceptStr, 1);
                }
                return loginResult;
            }
            catch (Exception ex)
            {
                InputEvent.Set();
                Logger.Fatal($"Telnet connection fails... {ex.ToString()}");
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
                    Logger.Debug("Close telnet connect succeed.");
                    //socket.Dispose();
                }
            }
            catch (Exception ex)
            {
                Logger.Debug("DisConnect telnet Exception:" + ex.ToString());
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

                    //获得转义后的字符串的长度
                    int strLinelen = m_strLine.Length;
                    //如果长度为零
                    if (strLinelen == 0)
                    {
                        //则返回"\r\n" 即回车换行
                        m_strLine = Convert.ToString("\r\n");
                    }
                    ////建立一个流,把接收的信息(转换后的)存进 mToProcess 中
                    Byte[] mToProcess = new Byte[strLinelen];
                    for (int i = 0; i < strLinelen; i++)
                        mToProcess[i] = Convert.ToByte(m_strLine[i]);
                    //对接收的信息进行处理,包括对传输过来的信息的参数的存取和
                    mOutText = ProcessOptions(mToProcess);
                    //解析命令后返回 显示信息(即除掉了控制信息)
                    if (mOutText != "")
                    {
                        mOutText = mOutText.Replace("\b", "");
                        if (InputEvent.WaitOne())
                        {
                            //Logger.Debug(mOutText);
                            SessionLog.Append(mOutText);
                            strWorkingData = mOutText;
                        }
                    }
                    //接收完数据,处理完字符串数据等一系列事物之后,开始回发数据
                    RespondToOptions();
                }
                else// 如果没有接收到任何数据的话
                {
                    Logger.Debug("OnRecievedData关闭连接,关闭socket!!!");
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
                Logger.Debug(SessionLog.ToString());
                Logger.Fatal("接收数据时出现异常：" + ex.ToString());
                mOutText = "";
                SessionLog.Clear();
                //throw;
            }
        }

        /// <summary>
        ///  发送数据的函数
        /// </summary>
        private void RespondToOptions()
        {
            try
            {
                //声明一个字符串,来存储 接收到的参数
                string strOption;
                /*
                 * 此处的控制信息参数,是之前接受到信息之后保存的
                 * 例如 255   253   23   等等
                 */
                for (int i = 0; i < m_ListOptions.Count; i++)
                {
                    //获得一个控制信息参数
                    strOption = (string)m_ListOptions[i];
                    //根据这个参数,进行处理
                    ArrangeReply(strOption);
                }
                DispatchMessage(m_strResp);
                m_strResp = "";
                m_ListOptions.Clear();
            }
            catch (Exception ers)
            {
                Logger.Fatal("出错了,在回发数据的时候 " + ers.ToString());
                throw;
            }
        }

        /// <summary>
        /// 解析接收的数据,生成最终用户看到的有效文字,同时将附带的参数存储起来
        ///</summary>
        ///<param name="m_strLineToProcess">收到的处理后的数据</param>
        /// <returns></returns>
        private string ProcessOptions(byte[] m_strLineToProcess)
        {
            string m_DISPLAYTEXT = "";
            string m_strTemp = "";
            string m_strOption = "";
            string m_strNormalText = "";
            bool bScanDone = false;
            int ndx = 0;
            int ldx = 0;
            char ch;
            try
            {
                //把数据从byte[] 转化成string
                for (int i = 0; i < m_strLineToProcess.Length; i++)
                {
                    Char ss = Convert.ToChar(m_strLineToProcess[i]);
                    m_strTemp = m_strTemp + Convert.ToString(ss);
                }
                //此处意义为,当没描完数据前,执行扫描
                while (bScanDone != true)
                {
                    //获得长度
                    int lensmk = m_strTemp.Length;
                    //之后开始分析指令,因为每条指令为255 开头,故可以用此来区分出每条指令 每条NVT命令以字节IAC（0xFF）开始
                    ndx = m_strTemp.IndexOf(Convert.ToString(IAC));
                    //此处为出错判断,本无其他含义
                    if (ndx > lensmk)
                        ndx = m_strTemp.Length;

                    //此处为,如果搜寻到IAC标记的telnet 指令,则执行以下步骤
                    if (ndx != -1)
                    {
                        #region 如果存在IAC标志位

                        // 将 标志位IAC 的字符 赋值给最终显示文字
                        m_DISPLAYTEXT += m_strTemp.Substring(0, ndx);
                        // 此处获得命令码
                        ch = m_strTemp[ndx + 1];
                        //如果命令码是253(DO) 254(DONT)  521(WILL) 252(WONT) 的情况下
                        if (ch == DO || ch == DONT || ch == WILL || ch == WONT)  //选项协商
                        {
                            //将以IAC 开头3个字符组成的整个命令存储起来
                            m_strOption = m_strTemp.Substring(ndx, 3);
                            m_ListOptions.Add(m_strOption);
                            // 将 标志位IAC 的字符 赋值给最终显示文字
                            m_DISPLAYTEXT += m_strTemp.Substring(0, ndx);
                            //将处理过的字符串删去
                            string txt = m_strTemp.Substring(ndx + 3);
                            m_strTemp = txt;
                        }
                        //如果IAC后面又跟了个IAC (255)
                        else if (ch == IAC)
                        {
                            //则显示从输入的字符串头开始,到之前的IAC 结束
                            m_DISPLAYTEXT = m_strTemp.Substring(0, ndx);
                            //之后将处理过的字符串排除出去
                            m_strTemp = m_strTemp.Substring(ndx + 1);
                        }
                        //如果IAC后面跟的是SB(250)
                        else if (ch == SB)
                        {
                            m_DISPLAYTEXT = m_strTemp.Substring(0, ndx);
                            ldx = m_strTemp.IndexOf(Convert.ToString(SE));
                            m_strOption = m_strTemp.Substring(ndx, ldx);
                            m_ListOptions.Add(m_strOption);
                            m_strTemp = m_strTemp.Substring(ldx);
                        }

                        #endregion 如果存在IAC标志位
                    }
                    //若字符串里已经没有IAC标志位了
                    else
                    {
                        //显示信息累加上m_strTemp存储的字段
                        m_DISPLAYTEXT = m_DISPLAYTEXT + m_strTemp;
                        bScanDone = true;
                    }
                }
                //输出人看到的信息
                m_strNormalText = m_DISPLAYTEXT;
            }
            catch (Exception eP)
            {
                Logger.Fatal("Telnet解析传入的字符串错误:" + eP.ToString());
                throw;
                //throw new Exception("解析传入的字符串错误:" + eP.ToString());
            }
            return m_strNormalText;
        }

        #region magic Function

        //解析传过来的参数,生成回发的数据到m_strResp
        private void ArrangeReply(string strOption)
        {
            try
            {
                Char Verb;
                Char Option;
                Char Modifier;
                Char ch;
                bool bDefined = false;
                //排错选项,无啥意义
                if (strOption.Length < 3) return;
                //获得命令码
                Verb = strOption[1];
                //获得选项码
                Option = strOption[2];
                //如果选项码为 回显(1) 或者是抑制继续进行(3)
                if (Option == 1 || Option == 3)
                {
                    bDefined = true;
                }
                // 设置回发消息,首先为标志位255
                m_strResp += IAC;
                //如果选项码为 回显(1) 或者是抑制继续进行(3) ==true
                if (bDefined == true)
                {
                    #region 继续判断

                    //如果命令码为253 (DO)
                    if (Verb == DO)
                    {
                        //我设置我应答的命令码为 251(WILL) 即为支持 回显或抑制继续进行
                        ch = WILL;
                        m_strResp += ch;
                        m_strResp += Option;
                    }
                    //如果命令码为 254(DONT)
                    if (Verb == DONT)
                    {
                        //我设置我应答的命令码为 252(WONT) 即为我也会"拒绝启动" 回显或抑制继续进行
                        ch = WONT;
                        m_strResp += ch;
                        m_strResp += Option;
                    }
                    //如果命令码为251(WILL)
                    if (Verb == WILL)
                    {
                        //我设置我应答的命令码为 253(DO) 即为我认可你使用回显或抑制继续进行
                        ch = DO;
                        m_strResp += ch;
                        m_strResp += Option;
                        //break;
                    }
                    //如果接受到的命令码为251(WONT)
                    if (Verb == WONT)
                    {
                        //应答  我也拒绝选项请求回显或抑制继续进行
                        ch = DONT;
                        m_strResp += ch;
                        m_strResp += Option;
                        //break;
                    }
                    //如果接受到250(sb,标志子选项开始)
                    if (Verb == SB)
                    {
                        /*
                         * 因为启动了子标志位,命令长度扩展到了4字节,
                         * 取最后一个标志字节为选项码
                         * 如果这个选项码字节为1(send)
                         * 则回发为 250(SB子选项开始) + 获取的第二个字节 + 0(is) + 255(标志位IAC) + 240(SE子选项结束)
                         */
                        Modifier = strOption[3];
                        if (Modifier == SEND)
                        {
                            ch = SB;
                            m_strResp += ch;
                            m_strResp += Option;
                            m_strResp += IS;
                            m_strResp += IAC;
                            m_strResp += SE;
                        }
                    }

                    #endregion 继续判断
                }
                else //如果选项码不是1 或者3
                {
                    #region 底下一系列代表,无论你发那种请求,我都不干

                    if (Verb == DO)
                    {
                        ch = WONT;
                        m_strResp += ch;
                        m_strResp += Option;
                    }
                    if (Verb == DONT)
                    {
                        ch = WONT;
                        m_strResp += ch;
                        m_strResp += Option;
                    }
                    if (Verb == WILL)
                    {
                        ch = DONT;
                        m_strResp += ch;
                        m_strResp += Option;
                    }
                    if (Verb == WONT)
                    {
                        ch = DONT;
                        m_strResp += ch;
                        m_strResp += Option;
                    }

                    #endregion 底下一系列代表,无论你发那种请求,我都不干
                }
            }
            catch (Exception eeeee)
            {
                Logger.Fatal("解析参数时出错:" + eeeee.ToString());
                throw;
            }
        }

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
                        Logger.Error($"Send command:{command},waiting for:{DataToWaitFor},TimeOut({timeout}), FAIL!!!");
                        strWorkingData = "";
                        return false;
                    }
                    Thread.Sleep(1); //垃圾回收，优化CPU占用率问题
                }
                InputEvent.Reset();
                //Logger.Info($"waiting for:\"{DataToWaitFor}\" success.");
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
                recvStr = "";
                cmd = cmd + "\n";
                Logger.Debug($"SendCommand-->{cmd}");
                SessionLog.Clear();
                strWorkingData = "";
                DispatchMessage(cmd);
                bool receiveResult = WaitFor(cmd, waitforStr, timeout);
                recvStr = SessionLog.ToString();
                InputEvent.Set();
                Logger.Debug(recvStr);
                //Logger.Debug(recvStr);
                //if (!receiveResult)
                //{
                //    PingIP(HostIP, 1);
                //}
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
                Logger.Fatal($"SocketException ex:{ex.ToString()},Reconnect Telnet,Retry.....");
                Close();
                if (Open(Global.PROMPT))
                {
                    return SendCommand(cmd, ref recvStr, waitforStr, timeout);
                }
                else
                {
                    Logger.Error($"Reconnect the Telnet failed！！！");
                    return false;
                }
            }
            catch (ObjectDisposedException)
            {
                if (Open(Global.PROMPT))
                {
                    return SendCommand(cmd, ref recvStr, waitforStr, timeout);
                }
                else
                {
                    Logger.Error($"Reconnect the Telnet failed！！！");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logger.Fatal($"{ex.ToString()}");
                recvStr = SessionLog.ToString();
                return false;
            }
            finally
            {
                InputEvent.Set();
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

    public class TelnetInfo
    {
        public string _Address { get; set; }
        public int _Port { get; set; } = 23;
        //public string _logPath;
    }
}