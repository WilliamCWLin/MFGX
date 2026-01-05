using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using static AutoTestSystem.BLL.Bd;

namespace AutoTestSystem.DAL
{
    /// <summary>
    /// 同步Socket通信
    /// </summary>
    public class SyncSocket : Communication
    {
        private Socket socket;
        private IPAddress ipAdd;
        private byte[] m_byBuff = new byte[1024 * 16];

        public SyncSocket(string _IPaddress, int _port)
        {
            HostIP = _IPaddress;
            Port = _port;
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

        public override void Dispose()
        {
            ((IDisposable)socket).Dispose();
        }

        public override bool Open()
        {
            try
            {
                if (!IPAddress.TryParse(HostIP, out ipAdd)) //确定一个字符串是否是有效的IP地址。
                {
                    Logger.Debug($"{HostIP} IP Address is invalid");
                    return false;
                }
                PingIP(HostIP, 5);
                IPEndPoint iep = new IPEndPoint(ipAdd, Port);
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.NoDelay = true;
                socket.Connect(iep);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Fatal($"Telnet connection fails... {ex.ToString()}");
                throw;
            }
        }

        public override string Read()
        {
            string m_strLine = "";
            try
            {
                if (socket.Connected)
                {
                    int nBytesRec = socket.Receive(m_byBuff);
                    if (nBytesRec > 0)
                    {
                        //声明一个字符串,用来存储解析过的字符串
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
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            return m_strLine;
        }

        public override bool SendCommand(string command, ref string strRecAll, string DataToWaitFor, int timeout = 10)
        {
            throw new NotImplementedException();
        }

        public override void Write(string message)
        {
            try
            {
                if (socket.Connected)
                {
                    byte[] data = Encoding.Default.GetBytes(message);
                    socket.Send(data);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}