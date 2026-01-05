using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AutoTestSystem.Base;
using static AutoTestSystem.BLL.Bd;

namespace AutoTestSystem.Equipment.ControlDevice
{
    public class TcpIpClient : ControlDeviceBase
    {
        [Category("Params"), Description("Set Server IP Address")]
        public string ServerIp { get; set; }
        [Category("Params"), Description("Set Server Port")]
        public int ServerPort { get; set; }
        [Category("Params"), Description("Set Retry Timeout")]
        public int TimeoutMilliSeconds { get; set; } = 10000;
        
        private TcpClient client = null;
        private NetworkStream stream = null;
        private string receivedMessage = "";
        private readonly object bufferLock = new object();
        private readonly Queue<string> dataQueue = new Queue<string>();
        int xNum;
        int yNum;
        int uNum;
        public TcpIpClient()
        {
            ServerIp = "127.0.0.1"; // Default IP
            ServerPort = 9001; // Default Port
        }
        public override bool Init(string strParamInfo) => true;
        public override void OPEN()
        {
            // Not applicable for TCP client
        }

        public override bool Status(ref string msg)
        {
            msg = client != null && client.Connected ? "Client is connected." : "Client is disconnected.";
            return client != null && client.Connected;
        }
        public void ConnectAndCommunicate(string server, int port, string input)
        {
            TcpClient client = null;
            NetworkStream stream = null;

            try
            {
                // 建立TCP連線
                client = new TcpClient(server, port);
                stream = client.GetStream();

                string hexString = "";
                // 处理换行符 "\r\n"

                //SEND
                byte[] data = Encoding.ASCII.GetBytes(input + "\r\n");
                hexString = BitConverter.ToString(data).Replace("-", " ");
                stream.Write(data, 0, data.Length);
                Logger.Info($"Send CMD:{input + "\r\n"},Hex:{hexString}");

                //READ
                byte[] buffer = new byte[8192];
                stream.ReadTimeout = 10000;
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                receivedMessage = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                dataQueue.Enqueue(receivedMessage);
            }
            catch (Exception ex)
            {
                Logger.Error("Error sending data: " + ex.Message);
            }
            finally
            {
                // 釋放資源
                Dispose();
            }
        }

        public override bool SEND(string input)
        {
            try
            {
                ConnectAndCommunicate(ServerIp, ServerPort, input);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error("Error sending data: " + ex.Message);
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

        public bool READTimeout(ref string output,int timeout)
        {

            if (client != null && client.Connected)
            {
                try
                {   
                    byte[] buffer = new byte[8192];
                    stream.ReadTimeout = timeout;
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead > 0)
                    {
                        receivedMessage = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                        output = receivedMessage;
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error("Error reading data: " + ex.Message);
                    return false;
                }
            }
            Logger.Debug("No data received or client is not connected.");
            return false;
        }
        public bool READTP(ref int xNum, ref int yNum, ref int uNum, ref string output,int timeout)
        {

            if (client != null && client.Connected)
            {
                try
                {
                    byte[] buffer = new byte[8192];
                    stream.ReadTimeout = timeout;
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead > 0)
                    {

                        xNum = Convert.ToInt16(buffer[2] * 256 + buffer[3]);
                        yNum = Convert.ToInt16(buffer[6] * 256 + buffer[7]);
                        uNum = Convert.ToInt16(buffer[10] * 256 + buffer[11]);
                        receivedMessage = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                        string xValue = receivedMessage.Substring(0, 2); //  X+
                        string yValue = receivedMessage.Substring(4, 2); // "Y+"
                        string uValue = receivedMessage.Substring(8, 2); // "U+"
                        string new_X = xValue + xNum;
                        string new_Y = yValue + yNum;
                        string new_U = uValue + uNum;
                        if (new_X.Contains("X-"))
                        {
                            xNum = -xNum;
                        }
                        else if (new_Y.Contains("Y-"))
                        {
                            yNum = -yNum;
                        }
                        else if (new_U.Contains("U-"))
                        {
                            uNum = -uNum;
                        }
                        
                        output = receivedMessage;
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error("Error reading data: " + ex.Message);
                }
            }
            Logger.Debug("No data received or client is not connected.");
            return false;
        }
        public bool SEND_ASCII(string inputX, string inputY, string inputU,ref string output)
        {
            int timeout = 10000;
            byte[] outputX = Encoding.ASCII.GetBytes("");
            byte[] outputY = Encoding.ASCII.GetBytes("");
            byte[] outputU = Encoding.ASCII.GetBytes("");
            byte[] newlineBytes = Encoding.ASCII.GetBytes("\r\n");
            IQ_Transfer(inputX, ref outputX);
            IQ_Transfer(inputY, ref outputY);
            IQ_Transfer(inputU, ref outputU);
            byte[] stradd = new byte[outputX.Length + outputY.Length + outputY.Length + newlineBytes.Length];
            outputX.CopyTo(stradd, 0);
            outputY.CopyTo(stradd, outputX.Length);
            outputU.CopyTo(stradd, outputX.Length + outputY.Length);

            //SEND("TP");
            //if (READTP(ref xNum,ref yNum,ref uNum,ref output,timeout))
            //{
            //    if (xNum +  > )
            //}

            stream.Write(stradd, 0, stradd.Length);
            if (!READTimeout(ref output,timeout))
            {
                return false;
            }
            else
            {
                if (output.Contains("OK"))
                {
                    Thread.Sleep(600);
                    return true;
                }
                else if (output.Contains("Error"))
                {
                    return false;
                }
                else
                {
                    LogMessage("Moving Fail", MessageLevel.Error);
                    return false;
                }
            }
        }

        public bool IQ_Transfer(string input, ref byte[] result)
        {
            try
            {
                // 分離 ASCII 字串和數字部分
                string textPart = "";
                string numberPart = "";
                string hexString = "";
                // 处理换行符 "\r\n"
                byte[] newlineBytes = Encoding.ASCII.GetBytes("\r\n");
                bool containsInt = Regex.IsMatch(input, @"\d+");

                if (containsInt)
                {
                    foreach (char c in input)
                    {
                        if (char.IsDigit(c))
                        {
                            numberPart += c;
                        }
                        else
                        {
                            textPart += c;
                        }
                    }
                    // 將字串部分轉換為 ASCII 位元組
                    byte[] textBytes = Encoding.ASCII.GetBytes(textPart);
                    // 將數字部分從10進制轉換為16進制
                    int number = int.Parse(numberPart);
                    //int hexString = int.Parse(number.ToString("X"));
                    byte[] numberBytes = BitConverter.GetBytes((short)number); // 转换为两个字节
                    Array.Reverse(numberBytes); // 将高字节放前（网络字节序)

                    // 合并所有字节数据
                    result = new byte[textBytes.Length + numberBytes.Length];
                    Buffer.BlockCopy(textBytes, 0, result, 0, textBytes.Length);
                    Buffer.BlockCopy(numberBytes, 0, result, textBytes.Length, numberBytes.Length);
                    
                    hexString = BitConverter.ToString(result).Replace("-", " ");
                    return true;
                }
                else
                {
                    byte[] data = Encoding.ASCII.GetBytes(input + "\r\n");
                    hexString = BitConverter.ToString(data).Replace("-", " ");
                }
                Logger.Info($"Send CMD:{input + "\r\n"},Hex:{hexString}");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error("Error sending data: " + ex.Message);
                return false;
            }

        }

        public override void Dispose()
        {
            if (client != null)
            {
                client.Close();
                client = null;
                LogMessage("Client disconnected.");
            }
        }

        public override void SetTimeout(int time)
        {
            if (client != null)
            {
                client.ReceiveTimeout = time;
                client.SendTimeout = time;
            }
        }
        public bool TryConnect()
        {
            try
            {
                if (client == null)
                {
                    client = new TcpClient(ServerIp, ServerPort);
                    stream = client.GetStream();
                    LogMessage("Connected to server ", MessageLevel.Info);
                    return true;
                }
                else
                {
                    LogMessage("Dont have client", MessageLevel.Error);
                    return false;
                }
                
            }
            catch (Exception ex)
            {
                LogMessage("Connect fail", MessageLevel.Info);
                return false;
            }
        }
 
        public bool ReadAfterReconnect(ref string output)
        {          
            DateTime start = DateTime.Now;

            try
            {

                while ((DateTime.Now - start).TotalMilliseconds < TimeoutMilliSeconds)
                {
                    if (READ(ref output))
                    {
                        return true;
                    }
                    DateTime reconnectStart = DateTime.Now;
                    while ((DateTime.Now - reconnectStart).TotalMilliseconds < TimeoutMilliSeconds)
                    {
                        if (TryConnect())
                        {
                            start = DateTime.Now; // 重新连接后重新记录start时间
                            break;
                        }

                        Thread.Sleep(1000); // 1 second delay before retrying
                    }
                    if (!client.Connected)
                    {
                        Logger.Debug("Failed to reconnect to server within timeout.");
                        return false;
                    }
                }
                Logger.Debug("Failed to receive data after reconnecting within the total timeout.client");
                return false;
            }catch(Exception ex)
            {
                Logger.Error($"Error to receive data after reconnecting within the total timeout.client{ex.Message}");
                return false;
            }
        }
        //public bool SendAfterReconnect(ref string output)
        //{
        //    DateTime start = DateTime.Now;



        //    while ((DateTime.Now - start).TotalMilliseconds < TimeoutMilliSeconds)
        //    {
        //        if (SEND(output))
        //        {
        //            return true;
        //        }
        //        DateTime reconnectStart = DateTime.Now;
        //        while ((DateTime.Now - reconnectStart).TotalMilliseconds < TimeoutMilliSeconds)
        //        {
        //            if (TryConnect())
        //            {
        //                start = DateTime.Now; // 重新连接后重新记录start时间
        //                break;
        //            }

        //            Thread.Sleep(1000); // 1 second delay before retrying
        //        }
        //        if (!client.Connected)
        //        {
        //            Logger.Debug("Failed to reconnect to server within timeout.");
        //            return false;
        //        }
        //    }
        //    Logger.Debug("Failed to receive data after reconnecting within the total timeout.client");
        //    return false;
        //}
    }
}
