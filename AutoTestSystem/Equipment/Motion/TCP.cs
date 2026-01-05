using AutoTestSystem.Base;
using AutoTestSystem.DynamicProperty;
using AutoTestSystem.Equipment.Teach;
using AutoTestSystem.Interface.Capabilities;
using AutoTestSystem.Interface.Config;
using DocumentFormat.OpenXml;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static AutoTestSystem.BLL.Bd;

namespace AutoTestSystem.Equipment.Motion
{

    public class TCP : MotionBase, INotifyPropertyChanged, ITcpEquipmentOps
    {
        private string _tcpMode;    
        private string _tcpProtocol;
        private bool _toggle = false;
        private TCPTeach TCPioTeach;
        private DynamicPropertyManager _propertyManager;

        [Category("Params"), Description("Set Server IP Address"), TypeConverter(typeof(Option))]
        public string TCP_Mode
        {
            get => _tcpMode;
            set
            {
                if (_tcpMode != value)
                {
                    _tcpMode = value;
                    OnPropertyChanged(nameof(TCP_Mode));
                    this.RefreshDynamicProperties();
                }
            }
        }

        [Category("Params"), Description("Set TCP/IP Protocol"), TypeConverter(typeof(ProtocolOption))]
        public string TCP_Protocol
        {
            get => _tcpProtocol;
            set
            {
                if (_tcpProtocol != value)
                {
                    _tcpProtocol = value;
                    OnPropertyChanged(nameof(TCP_Protocol));
                }
            }
        }

        [Category("Params"), Description("Set Server IP Address")]
        public string Ip { get; set; }

        [Category("Params"), Description("Set Server Port")]
        public int Port { get; set; }

        [Category("Params"), Description("Set Retry Timeout")]
        public int TimeoutMilliSeconds { get; set; } = 10000;

        // 屬性顯示邏輯（你可以根據需求修改這些邏輯）
        [Browsable(false)]
        public bool ShouldShowProtocol {
            get
            {
                if (TCP_Mode == "Client")
                {
                    return true;
                } else
                {
                    _tcpProtocol = "";
                    return false;
                }
            }
        }
        private void ConfigurePropertyVisibility()
        {

            var rules = new PropertyVisibilityBuilder()
                .When(nameof(TCP_Protocol), () => ShouldShowProtocol)
                .Build();

            _propertyManager.AddVisibilityRules(rules);
            _propertyManager.Initialize();

        }

        // INotifyPropertyChanged 實作
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void ExecuteTcp(TcpConfig config)
        {
            _toggle = config.Toggle;
        }

        private TcpClient client = null;
        private TcpListener server = null;
        private NetworkStream stream = null;
        private string receivedMessage = "";
        private readonly object bufferLock = new object();
        private readonly Queue<string> dataQueue = new Queue<string>();
        public event EventHandler<DataReceivedEventArgs1> Client_DataReceived;
        public event EventHandler<DataReceivedEventArgs1> Server_DataReceived;
        [JsonIgnore]
        public Thread readThread;
        [JsonIgnore]
        public Thread ServerThread;
        int xNum;
        int yNum;
        int uNum;
        public TCP()
        {
            Ip = "127.0.0.1"; // Default IP
            Port = 9001; // Default Port
            _propertyManager = this.GetDynamicPropertyManager();
            ConfigurePropertyVisibility();
        }
        public override bool Init(string strParamInfo)
        {
            return TryConnect();
        }       
        public override bool Status(ref string msg)
        {
            switch (TCP_Mode)
            {
                case "Client":
                    msg = client != null && client.Connected ? "Client is connected." : "Client is disconnected.";
                    return client != null && client.Connected;
                case "Server":
                    msg = server != null ? "Server is running." : "Server is stopped.";
                    return server != null;
                default:
                    msg = "Invalid TCP mode.";
                    return false;
            }
        }
        public override bool Show()
        {
            using (var form = new TCPIOViewerForm(TCPioTeach))
            {
                form.ShowDialog();
            }

            return true;
        }
       
        public override bool UnInit()
        { 
            switch (TCP_Mode)
            {
                case "Client":
                    if (server != null)
                    {
                        server.Stop();
                    }
                    if (client != null)
                    {
                        client.Close();
                        //server.Stop();
                        
                        client = null;
                        LogMessage("Client disconnected.");
                    }
                    if (stream != null)
                    {
                        stream.Close();
                        stream = null;
                        LogMessage("Stream Close.");
                    }
                    if (ServerThread != null)
                    {
                        ServerThread.Interrupt();
                        ServerThread = null;
                        LogMessage("Server thread interrupted.");
                    }

                    return true;
                case "Server":
                    if (client != null)
                    {
                        client.Close();
                    }
                    if (server != null)
                    {   
                        server.Stop();
                        server = null;
                        LogMessage("Server stopped.");
                    }
                    if (readThread != null)
                    {
                        readThread.Interrupt();
                        readThread = null;
                        LogMessage("Read thread interrupted.");
                    }

                    return true;
                default:
                    LogMessage("Invalid TCP mode.");
                    return false;
            }
        }
        public override bool StartAction(string strItemName, string strParamIn, ref string strOutput)
        {
            throw new NotImplementedException();
        }
        public override bool ServoON()
        {
            throw new NotImplementedException();
        }
        public override bool ServoOFF()
        {
            throw new NotImplementedException();
        }
        public override bool SetCommandPos(double in_pos)
        {
            throw new NotImplementedException();
        }
        public override bool GetCommandPos(ref double out_pos)
        {
            throw new NotImplementedException();
        }
        public override bool EmgStop()
        {
            throw new NotImplementedException();
        }
        public override bool SdStop()
        {
            throw new NotImplementedException();
        }
        public override bool Pause()
        {
            throw new NotImplementedException();
        }
        public override bool SyncHome(double in_start_vel, double in_max_vel, int Dir, int Timeout)
        {
            throw new NotImplementedException();
        }
        public override bool SyncMotionDone()
        {
            throw new NotImplementedException();
        }
        public override bool Check_IO_StartStatus(Dictionary<int, int> Devices_IO_Status)
        {
            throw new NotImplementedException();
        } 
        //-----------------------------------------------Won't be used in JM-----------------------------------------------//
        public override bool Relative_Move(double value, double in_start_vel, double in_max_vel, double tacc, double dac)
        {
            throw new NotImplementedException();
        }
        public override bool Absolute_Move(double value, double in_start_vel, double in_max_vel, double tacc, double dac)
        {
            //Won't be used in JM
            throw new NotImplementedException();
        }
        //-----------------------------------------------Won't be used in JM-----------------------------------------------//
        public override bool Relative_Move(string input)
        {
            try
            {
                if (SetCommand(input))
                    return true;
                else
                    return false;
            }
            catch (Exception e)
            {
                LogMessage($"Relative_Move Fail : {e}", MessageLevel.Error);
                return false;
            }
            
        }
        public override bool Absolute_Move(string input)
        {
            try
            {
                if (SetCommand(input))
                    return true;
                else
                    return false; 
            }   
            catch (Exception e)
            {
                LogMessage($"Absolute_Move Fail : {e}", MessageLevel.Error);
                return false;
            }
        }
        public override bool Recieve_MotionDone(ref string output,int timeout)
        {
            switch (TCP_Mode)
            {
                case "Client":
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
                case "Server":
                    if (server != null)
                    {
                        try
                        {
                            //using (client = server.AcceptTcpClient()) ;
                            //using (stream = client.GetStream())
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
                        }
                        catch (Exception ex)
                        {
                            Logger.Error("Error reading data on server: " + ex.Message);
                            return false;
                        }
                    }
                    Logger.Debug("No client pending or no data received.");
                    return false;
                default:
                    return true;
            }
        }
        public bool SEND(string input)
        {
            switch (TCP_Mode)
            {
                case "Client":
                    try
                    {
                        // 分離 ASCII 字串和數字部分
                        string textPart = "";
                        string numberPart = "";
                        string hexString = "";
                        // 处理换行符 "\r\n"
                        byte[] newlineBytes = Encoding.ASCII.GetBytes("\r\n");
                        bool containsInt = Regex.IsMatch(input, @"\d+");
                        byte[] data = Encoding.ASCII.GetBytes(input + "\r\n");
                        hexString = BitConverter.ToString(data).Replace("-", " ");
                        stream.Write(data, 0, data.Length);
                        Logger.Info($"Send CMD:{input + "\r\n"},Hex:{hexString}");
                        return true;
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("Error sending data: " + ex.Message);
                        return false;
                    }
                case "Server":
                    try
                    {
                        if (server != null)
                        {
                            // 分离 ASCII 字符串和数字部分
                            string textPart = "";
                            string numberPart = "";
                            string hexString = "";
                            // 处理换行符 "\r\n"
                            byte[] newlineBytes = Encoding.ASCII.GetBytes("\r\n");
                            bool containsInt = Regex.IsMatch(input, @"\d+");

                            //TcpClient client = server.AcceptTcpClient();
                            NetworkStream stream = client.GetStream();

                            byte[] data = Encoding.ASCII.GetBytes(input + "\r\n");
                            hexString = BitConverter.ToString(data).Replace("-", " ");
                            stream.Write(data, 0, data.Length);
                            Logger.Info($"Send CMD: {input + "\r\n"}, Hex: {hexString}");
                            return true;
                        }
                        else
                        {
                            Logger.Error("Server is not running.");
                            return false;
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("Error sending data: " + ex.Message);
                        return false;
                    }
                default:
                    Logger.Error("Invalid TCP mode.");
                    return false;
            }
        }
        public bool READ (ref string output)
        {
            switch (TCP_Mode)
            {
                case "Client":
                    if (client != null && client.Connected)
                    {
                        try
                        {
                            byte[] buffer = new byte[8192];
                            //stream.ReadTimeout = timeout;
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
                case "Server":
                    if (server != null)
                    {
                        if (client != null)
                        {
                            try
                            {
                                //using (client = server.AcceptTcpClient()) ;
                                //using (stream = client.GetStream())
                                {
                                    byte[] buffer = new byte[8192];
                                    //stream.ReadTimeout = timeout;
                                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                                    if (bytesRead > 0)
                                    {
                                        receivedMessage = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                                        output = receivedMessage;
                                        return true;
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Logger.Error("Error reading data on server: " + ex.Message);
                                return false;
                            }
                        }
                        else
                        {

                        }

                    }
                    Logger.Debug("No client pending or no data received.");
                    return false;
                default:
                    return true;
            }
        }
        public bool SetCommand(string input)
        {
            switch (TCP_Mode)
            {
                case "Client":
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
                            byte[] result = new byte[textBytes.Length + numberBytes.Length + newlineBytes.Length];
                            Buffer.BlockCopy(textBytes, 0, result, 0, textBytes.Length);
                            Buffer.BlockCopy(numberBytes, 0, result, textBytes.Length, numberBytes.Length);
                            Buffer.BlockCopy(newlineBytes, 0, result, textBytes.Length + numberBytes.Length, newlineBytes.Length);

                            hexString = BitConverter.ToString(result).Replace("-", " ");
                            stream.Write(result, 0, result.Length);
                        }
                        else
                        {
                            byte[] data = Encoding.ASCII.GetBytes(input + "\r\n");
                            hexString = BitConverter.ToString(data).Replace("-", " ");
                            stream.Write(data, 0, data.Length);
                        }
                        Logger.Info($"Send CMD:{input + "\r\n"},Hex:{hexString}");
                        return true;
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("Error sending data: " + ex.Message);
                        return false;
                    }
                case "Server":
                    try
                    {
                        if (server != null)
                        {
                            // 分离 ASCII 字符串和数字部分
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
                                // 将字符串部分转换为 ASCII 字节
                                byte[] textBytes = Encoding.ASCII.GetBytes(textPart);
                                // 将数字部分从10进制转换为16进制
                                int number = int.Parse(numberPart);
                                byte[] numberBytes = BitConverter.GetBytes((short)number); // 转换为两个字节
                                Array.Reverse(numberBytes); // 将高字节放前（网络字节序）

                                // 合并所有字节数据
                                byte[] result = new byte[textBytes.Length + numberBytes.Length + newlineBytes.Length];
                                Buffer.BlockCopy(textBytes, 0, result, 0, textBytes.Length);
                                Buffer.BlockCopy(numberBytes, 0, result, textBytes.Length, numberBytes.Length);
                                Buffer.BlockCopy(newlineBytes, 0, result, textBytes.Length + numberBytes.Length, newlineBytes.Length);

                                hexString = BitConverter.ToString(result).Replace("-", " ");
                                stream.Write(result, 0, result.Length);

                            }
                            else
                            {

                                //TcpClient client = server.AcceptTcpClient();
                                NetworkStream stream = client.GetStream();

                                byte[] data = Encoding.ASCII.GetBytes(input + "\r\n");
                                hexString = BitConverter.ToString(data).Replace("-", " ");
                                stream.Write(data, 0, data.Length);
                            }
                            Logger.Info($"Send CMD: {input + "\r\n"}, Hex: {hexString}");
                            return true;
                        }
                        else
                        {
                            Logger.Error("Server is not running.");
                            return false;
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("Error sending data: " + ex.Message);
                        return false;
                    }
                default:
                    Logger.Error("Invalid TCP mode.");
                    return false;
            }
        }
        public bool SEND_HEX(dynamic DUTDevice, string cmd)
        {
            return DUTDevice.SEND(ConvertHexStringToByteArray(cmd));
        }
        // 將十六進位字串轉換為byte[]
        private byte[] ConvertHexStringToByteArray(string hexString)
        {
            // 清除字串中的空格和0x
            hexString = hexString.Replace("0x", "").Replace(" ", "").ToUpper();

            // 檢查字串長度是否是偶數
            if (hexString.Length % 2 != 0)
            {
                LogMessage("Invalid hex string length.");
                return null;
            }

            try
            {
                // 將十六進位字串轉換為byte[]
                byte[] byteArray = Enumerable.Range(0, hexString.Length)
                                             .Where(x => x % 2 == 0)
                                             .Select(x => Convert.ToByte(hexString.Substring(x, 2), 16))
                                             .ToArray();

                return byteArray;
            }
            catch (FormatException)
            {
                LogMessage("Invalid hex string format.");
                return null;
            }
        }
        private void Connect_DataReceived()
        {
            try
            {
                while (true)
                {
                    if (stream != null && stream.DataAvailable)
                    {
                        byte[] data = new byte[256];
                        int bytes = stream.Read(data, 0, data.Length);
                        string responseData = Encoding.ASCII.GetString(data, 0, bytes);
                        Client_DataReceived?.Invoke(this, new DataReceivedEventArgs1(responseData));
                    }
                    Thread.Sleep(100); // 避免过度占用 CPU
                }
            }
            catch (Exception ex)
            {
                LogMessage($"NetworkStream_DataReceived: {ex.Message}");
            }
        }
        public override bool CheckConnect()
        {
            client = server.AcceptTcpClient();
            stream = client.GetStream();
            //while (true)
            //{
            //    if(client != null)
            //    {
            //        break;
            //    }
            //    else
            //    {
            //        continue;
            //    }
            //}
            return true;
        }
        public override bool ReleaseConnect()
        {
            if (client != null)
            {
                client.Close();
                client.Dispose();
                client = null;
                return true;
            }
            else
            {
                LogMessage("Client is null, don't need to release");
                return false;
            }

        }
        private void AcceptClient()
        {
            try
            {
                while (true)
                {
                    client = server.AcceptTcpClient();
                    stream = client.GetStream();
                    //ServerThread = new Thread(() => HandleClient(client));
                    //ServerThread.Start();
                }
            }
            catch (Exception ex)
            {
                LogMessage($"Accept client failed: {ex.Message}", MessageLevel.Info);
            }
        }
        private void HandleClient(TcpClient client)
        {
            try
            {
                NetworkStream stream = client.GetStream();
                while (true)
                {
                    if (stream.DataAvailable)
                    {
                        byte[] data = new byte[256];
                        int bytes = stream.Read(data, 0, data.Length);
                        string responseData = Encoding.ASCII.GetString(data, 0, bytes);
                        Server_DataReceived?.Invoke(this, new DataReceivedEventArgs1(responseData));

                        //lock (bufferLock)
                        //{
                        //    dataQueue.Enqueue(responseData); // 将数据存入队列
                        //}
                    }
                    Thread.Sleep(100); // 避免过度占用 CPU
                }
            }
            catch (Exception ex)
            {
                LogMessage($"Handle client failed: {ex.Message}", MessageLevel.Info);
            }
        }

        private void OnDataReceived(object sender, DataReceivedEventArgs1 e)
        {
            lock (bufferLock)
            {
                dataQueue.Enqueue(e.Data1); // 将数据存入队列
            }
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
            UnInit();
        }
        public void SetTimeout(int time)
        {
            if (client != null)
            {
                client.ReceiveTimeout = time;
                client.SendTimeout = time;
            }
        }
        public bool TryConnect()
        {
            switch(TCP_Mode)
            {
                case "Client":
                    try
                    {
                        if (client == null)
                        {
                            client = new TcpClient(Ip, Port);
                            stream = client.GetStream();
                            //dataQueue.Clear();
                            //Client_DataReceived += OnDataReceived;

                            //readThread = new Thread(Connect_DataReceived);
                            //readThread.Start();

                            LogMessage("Connected to server", MessageLevel.Info);
                            return true;
                        }
                        else
                        {
                            LogMessage("Client already exists", MessageLevel.Error);
                            return false;
                        }
                    }
                    catch (Exception ex)
                    {
                        LogMessage("Connection failed", MessageLevel.Info);
                        return false;
                    }
                case "Server":
                    try
                    {
                        if (server == null)
                        {
                            server = new TcpListener(IPAddress.Parse(Ip), Port);
                            server.Start();
                            //dataQueue.Clear();
                            //Server_DataReceived += OnDataReceived;

                            
                            //ServerThread = new Thread(AcceptClient);
                            //ServerThread.Start();

                            LogMessage("Server started", MessageLevel.Info);
                            return true;
                        }
                        else
                        {
                            LogMessage("Server already exists", MessageLevel.Error);
                            return false;
                        }
                    }
                    catch (Exception ex)
                    {
                        LogMessage($"Server start failed: {ex.Message}", MessageLevel.Info);
                        return false;
                    }
                default:
                    LogMessage("Invalid TCP mode", MessageLevel.Error);
                    return false;
            }
        }
        public bool Clear()
        {
            return true;
        }
        //public bool ReadAfterReconnect(ref string output)
        //{          
        //    DateTime start = DateTime.Now;

        //    try
        //    {
        //        while ((DateTime.Now - start).TotalMilliseconds < TimeoutMilliSeconds)
        //        {
        //            if ( Recieve_MotionDone(ref output,timeout))
        //            {
        //                return true;
        //            }
        //            DateTime reconnectStart = DateTime.Now;
        //            while ((DateTime.Now - reconnectStart).TotalMilliseconds < TimeoutMilliSeconds)
        //            {
        //                if (TryConnect())
        //                {
        //                    start = DateTime.Now; // 重新连接后重新记录start时间
        //                    break;
        //                }

        //                Thread.Sleep(1000); // 1 second delay before retrying
        //            }
        //            if (!client.Connected)
        //            {
        //                Logger.Debug("Failed to reconnect to server within timeout.");
        //                return false;
        //            }
        //        }
        //        Logger.Debug("Failed to receive data after reconnecting within the total timeout.client");
        //        return false;
        //    }catch(Exception ex)
        //    {
        //        Logger.Error($"Error to receive data after reconnecting within the total timeout.client{ex.Message}");
        //        return false;
        //    }
        //}

        #region TCPCommunication
        public enum FileFormat
        {
            JSON,
            CSV,
            Binary
        }
        private bool saveToFile(Dictionary<string, Dictionary<int, int>> data, string filePath, FileFormat format)
        {
            try
            {   // 取得檔案路徑的目錄部分
                string directoryPath = Path.GetDirectoryName(filePath);

                // 檢查目錄是否存在，如果不存在則建立
                if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                    Logger.Info($"Created directory: {directoryPath}");
                }
                switch (format)
                {
                    case FileFormat.JSON:
                        return saveAsJson(data, filePath);
                    case FileFormat.CSV:
                        return saveAsCsv(data, filePath);
                    case FileFormat.Binary:
                        return saveAsBinary(data, filePath);
                    default:
                        Logger.Error("Unsupported file format");
                        return false;
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Save file error: {ex.Message}");
                return false;
            }
        }

        private bool saveAsJson(Dictionary<string, Dictionary<int, int>> data, string filePath)
        {
            var jsonData = new
            {
                timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                registers = data.Select(kvp => new
                {
                    header = kvp.Key,
                    values = kvp.Value.Select(v => new { address = v.Key, value = v.Value })
                })
            };

            string json = Newtonsoft.Json.JsonConvert.SerializeObject(jsonData, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(filePath, json);
            return true;
        }

        private bool saveAsCsv(Dictionary<string, Dictionary<int, int>> data, string filePath)
        {
            using (var writer = new StreamWriter(filePath))
            {
                writer.WriteLine("Header,Address,Value,Timestamp");
                string timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

                foreach (var headerData in data)
                {
                    foreach (var register in headerData.Value)
                    {
                        writer.WriteLine($"{headerData.Key},{register.Key},{register.Value},{timestamp}");
                    }
                }
            }
            return true;
        }

        private bool saveAsBinary(Dictionary<string, Dictionary<int, int>> data, string filePath)
        {
            using (var writer = new BinaryWriter(File.Open(filePath, FileMode.Create)))
            {
                writer.Write(DateTime.UtcNow.ToBinary()); // 時間戳
                writer.Write(data.Count); // Header 數量

                foreach (var headerData in data)
                {
                    writer.Write(headerData.Key); // Header
                    writer.Write(headerData.Value.Count); // 該 Header 下的暫存器數量

                    foreach (var register in headerData.Value)
                    {
                        writer.Write(register.Key);   // Address
                        writer.Write(register.Value); // Value
                    }
                }
            }
            return true;
        }

        private Dictionary<int, int> readMultipleRegistersSlmp(string header, int startAddress, int count, uint dataLength)
        {
            byte head;
            if (header == "D") head = 0xA8;
            else
            {
                Logger.Error("No support header type");
                return null;
            }

            int actualRegisterCount = count * (int)dataLength;

            byte lowStartByte = (byte)(startAddress & 0xff);
            byte highStartByte = (byte)(startAddress >> 8 & 0xff);
            byte lowCountByte = (byte)(actualRegisterCount & 0xff);
            byte highCountByte = (byte)(actualRegisterCount >> 8 & 0xff);

            // 修改 payload 以支援批量讀取
            byte[] payload = new byte[] {
                0x50, 0x00, 0x00, 0xff, 0xff, 0x03, 0x00,
                0x0C, 0x00, 0x10, 0x00, 0x01, 0x04, 0x00, 0x00,
                lowStartByte, highStartByte, 0x00, head,
                lowCountByte, highCountByte
            };

            stream.Write(payload, 0, payload.Length);

            // 計算預期的回應長度
            int expectedResponseLength = 11 + (actualRegisterCount * 2); // header + data
            byte[] data = new byte[expectedResponseLength];

            stream.ReadTimeout = 1000;
            int bytes = stream.Read(data, 0, data.Length);

            if (data[9] == 0 && data[10] == 0)
            {
                var result = new Dictionary<int, int>();

                for (int i = 0; i < count; i++)
                {
                    if (dataLength == 1)
                    {
                        int dataIndex = 11 + (i * 2);
                        byte lowByte = data[dataIndex];
                        byte highByte = data[dataIndex + 1];
                        int value = (highByte << 8) + lowByte;
                        result[startAddress + i] = value;
                    }
                    else if (dataLength == 2)
                    {
                        int dataIndex = 11 + (i * 4);

                        byte byte1 = data[dataIndex];
                        byte byte2 = data[dataIndex + 1];
                        byte byte3 = data[dataIndex + 2];
                        byte byte4 = data[dataIndex + 3];

                        int value = (byte4 << 24) + (byte3 << 16) + (byte2 << 8) + byte1;
                        result[startAddress + (i * (int)dataLength)] = value;
                    }
                    else
                    {
                        Logger.Error($"Unsupported dataLength: {dataLength}");
                        return null;
                    }
                }

                Logger.Info($"Read {count} registers from {header}{startAddress} with dataLength {dataLength}, got {result.Count} values");
                return result;
            }
            else
            {
                Logger.Error("Error in SLMP response");
                return null;
            }
        }

        private Dictionary<string, Dictionary<int, int>> loadFromFile(string filePath, FileFormat format)
        {
            try
            {
                switch (format)
                {
                    case FileFormat.JSON:
                        return loadFromJson(filePath);
                    case FileFormat.CSV:
                        return loadFromCsv(filePath);
                    case FileFormat.Binary:
                        return loadFromBinary(filePath);
                    default:
                        Logger.Error("Unsupported file format");
                        return null;
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Load file error: {ex.Message}");
                return null;
            }
        }

        private Dictionary<string, Dictionary<int, int>> loadFromJson(string filePath)
        {
            string json = File.ReadAllText(filePath);
            dynamic jsonData = Newtonsoft.Json.JsonConvert.DeserializeObject(json);

            var result = new Dictionary<string, Dictionary<int, int>>();

            foreach (var register in jsonData.registers)
            {
                string header = register.header;
                if (!result.ContainsKey(header))
                {
                    result[header] = new Dictionary<int, int>();
                }

                foreach (var value in register.values)
                {
                    result[header][(int)value.address] = (int)value.value;
                }
            }

            return result;
        }

        private Dictionary<string, Dictionary<int, int>> loadFromCsv(string filePath)
        {
            var result = new Dictionary<string, Dictionary<int, int>>();

            using (var reader = new StreamReader(filePath))
            {
                string headerLine = reader.ReadLine(); // 跳過標題行

                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    var parts = line.Split(',');
                    if (parts.Length >= 3)
                    {
                        string header = parts[0];
                        int address = int.Parse(parts[1]);
                        int value = int.Parse(parts[2]);

                        if (!result.ContainsKey(header))
                        {
                            result[header] = new Dictionary<int, int>();
                        }

                        result[header][address] = value;
                    }
                }
            }

            return result;
        }

        private Dictionary<string, Dictionary<int, int>> loadFromBinary(string filePath)
        {
            var result = new Dictionary<string, Dictionary<int, int>>();

            using (var reader = new BinaryReader(File.Open(filePath, FileMode.Open)))
            {
                long timestamp = reader.ReadInt64(); // 讀取時間戳
                int headerCount = reader.ReadInt32(); // 讀取 Header 數量

                for (int i = 0; i < headerCount; i++)
                {
                    string header = reader.ReadString(); // 讀取 Header
                    int registerCount = reader.ReadInt32(); // 讀取該 Header 下的暫存器數量

                    if (!result.ContainsKey(header))
                    {
                        result[header] = new Dictionary<int, int>();
                    }

                    for (int j = 0; j < registerCount; j++)
                    {
                        int address = reader.ReadInt32(); // 讀取 Address
                        int value = reader.ReadInt32(); // 讀取 Value
                        result[header][address] = value;
                    }
                }
            }

            return result;
        }

        private bool writeMultipleRegistersSlmp(string header, Dictionary<int, int> registers, uint dataLength)
        {
            try
            {
                byte head;
                if (header == "D") head = 0xA8;
                else
                {
                    Logger.Error("No support header type");
                    return false;
                }

                // 將暫存器按地址排序
                var sortedRegisters = registers.OrderBy(kvp => kvp.Key).ToList();

                // 根據 dataLength 調整批次大小限制
                // 原本限制是960個暫存器，但當 dataLength = 2 時，每個邏輯暫存器佔用2個實際暫存器
                var maxLogicalRegisters = dataLength == 1 ? 960 : 480;
                var totalBatches = (int)Math.Ceiling((double)sortedRegisters.Count / maxLogicalRegisters);

                for (int batch = 0; batch < totalBatches; batch++)
                {
                    var batchRegisters = sortedRegisters.Skip(batch * maxLogicalRegisters).Take(maxLogicalRegisters).ToList();

                    if (batchRegisters.Count > 0)
                    {
                        int startAddress = batchRegisters.First().Key;
                        int logicalCount = batchRegisters.Count;

                        // 計算實際需要寫入的暫存器數量
                        int actualRegisterCount = logicalCount * (int)dataLength;

                        // 建構 SLMP 寫入命令
                        byte lowStartByte = (byte)(startAddress & 0xff);
                        byte highStartByte = (byte)(startAddress >> 8 & 0xff);
                        byte lowCountByte = (byte)(actualRegisterCount & 0xff);
                        byte highCountByte = (byte)(actualRegisterCount >> 8 & 0xff);

                        // 建立資料陣列
                        var dataBytes = new List<byte>();

                        if (dataLength == 1)
                        {
                            // dataLength = 1: 每個暫存器獨立寫入
                            foreach (var reg in batchRegisters)
                            {
                                dataBytes.Add((byte)(reg.Value & 0xff));      // 低位元組
                                dataBytes.Add((byte)(reg.Value >> 8 & 0xff)); // 高位元組
                            }
                        }
                        else if (dataLength == 2)
                        {
                            // dataLength = 2: 每個值拆分成兩個暫存器寫入
                            foreach (var reg in batchRegisters)
                            {
                                int value = reg.Value;

                                // 將32位值拆分成4個位元組 (Little Endian)
                                byte byte1 = (byte)(value & 0xff);         // 最低位元組
                                byte byte2 = (byte)((value >> 8) & 0xff);  // 次低位元組
                                byte byte3 = (byte)((value >> 16) & 0xff); // 次高位元組
                                byte byte4 = (byte)((value >> 24) & 0xff); // 最高位元組

                                // 第一個暫存器 (低16位)
                                dataBytes.Add(byte1); // 低位元組
                                dataBytes.Add(byte2); // 高位元組

                                // 第二個暫存器 (高16位)
                                dataBytes.Add(byte3); // 低位元組
                                dataBytes.Add(byte4); // 高位元組
                            }
                        }
                        else
                        {
                            Logger.Error($"Unsupported dataLength: {dataLength}");
                            return false;
                        }

                        int requestDataLength = 12 + dataBytes.Count;
                        byte lowrequestDataLengthByte = (byte)(requestDataLength & 0xff);
                        byte highrequestDataLengthByte = (byte)(requestDataLength >> 8 & 0xff);

                        // 建構完整的 payload
                        var payload = new List<byte>
                {
                    0x50, 0x00, 0x00, 0xff, 0xff, 0x03, 0x00,
                    lowrequestDataLengthByte, highrequestDataLengthByte, 0x10, 0x00, 0x01, 0x14, 0x00, 0x00,
                    lowStartByte, highStartByte, 0x00, head,
                    lowCountByte, highCountByte
                };

                        payload.AddRange(dataBytes);

                        // 發送命令
                        stream.Write(payload.ToArray(), 0, payload.Count);

                        // 讀取回應
                        byte[] response = new byte[11];
                        stream.ReadTimeout = 1000;
                        int bytes = stream.Read(response, 0, response.Length);

                        if (response[9] != 0 || response[10] != 0)
                        {
                            Logger.Error($"Error in SLMP write response for batch {batch + 1}");
                            return false;
                        }

                        Logger.Info($"Successfully wrote batch {batch + 1}/{totalBatches} ({logicalCount} logical registers, {actualRegisterCount} actual registers)");
                    }
                }

                Logger.Info($"Successfully wrote all {registers.Count} logical registers to {header} with dataLength {dataLength}");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error($"Write multiple registers SLMP error: {ex.Message}");
                return false;
            }
        }

        private bool writeRegistersToDevice(string header, Dictionary<int, int> registers, uint dataLength)
        {
            try
            {
                switch (TCP_Mode)
                {
                    case "Client":
                        if (client != null && client.Connected)
                        {
                            switch (TCP_Protocol)
                            {
                                case "SLMP":
                                    return writeMultipleRegistersSlmp(header, registers, dataLength);
                                case "Standard":
                                    Logger.Error("Standard protocol not implemented");
                                    return false;
                                default:
                                    Logger.Error($"TCP Protocol Not Support");
                                    return false;
                            }
                        }
                        Logger.Error("Client is not connected.");
                        return false;
                    case "Server":
                        Logger.Error("Server mode not implemented");
                        return false;
                    default:
                        Logger.Error("Wrong mode use");
                        return false;
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Write registers error: {ex.Message}");
                return false;
            }
        }

        public override bool readRegisterToFile(string header, int startAddress, int count, string filePath, int format, uint dataLength)
        {
            var allData = new Dictionary<string, Dictionary<int, int>>();
            try
            {
                switch (TCP_Mode)
                {
                    case "Client":
                        if (client != null && client.Connected)
                        {
                            switch (TCP_Protocol)
                            {
                                case "SLMP":
                                    var batchSize = dataLength == 1 ? 960 : 480;
                                    var totalBatches = (int)Math.Ceiling((double)count / batchSize);
                                    if (!allData.ContainsKey(header))
                                    {
                                        allData[header] = new Dictionary<int, int>();
                                    }
                                    for (int batch = 0; batch < totalBatches; batch++)
                                    {
                                        var currentStartAddress = startAddress + (batch * batchSize);
                                        var currentCount = Math.Min(batchSize, count - (batch * batchSize));

                                        var rangeData = readMultipleRegistersSlmp(header, currentStartAddress, currentCount, dataLength);

                                        if (rangeData != null)
                                        {
                                            foreach (var kvp in rangeData)
                                            {
                                                allData[header][kvp.Key] = kvp.Value;
                                            }
                                        }
                                        else
                                        {
                                            Logger.Error("No Data");
                                            return false;
                                        }
                                    }
                                    return saveToFile(allData, filePath, (FileFormat)format);
                                case "Standard":
                                    Logger.Error("Not implemented");
                                    return false;
                                default:
                                    Logger.Error($"TCP Protocol Not Support");
                                    return false;
                            }
                        }
                        Logger.Error("No data received or client is not connected.");
                        return false;
                    case "Server":
                        // Server 模式實現
                        Logger.Error("Server mode not implemented");
                        return false;
                    default:
                        Logger.Error("Wrong mode use");
                        return false;
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Read to file error: {ex.Message}");
                return false;
            }
        }
        public override bool readFromRegister(string header, int num, uint dataLength, bool other_format, ref double Output_Val)
        {
            switch (TCP_Mode)
            {
                case "Client":
                    if (client != null && client.Connected)
                    { 
                        try
                        {
                            switch (TCP_Protocol)
                            {
                                case "SLMP":
                                    
                                    byte head;
                                    byte readFormat = 0x00;
                                    byte readDatalength = 0x01;
                                    if (header == "D")
                                    {
                                        head = 0xA8;
                                    }
                                    else if (header == "M")
                                    {
                                        head = 0x90;
                                    }
                                    else if (header == "X")
                                    {
                                        head = 0x9C;
                                    }
                                    else if (header == "Y")
                                    {
                                        head = 0x9D;
                                    }
                                    else
                                    {
                                        Logger.Error("Not support header type");
                                        return false;
                                    }


                                    if (header == "D")
                                    {
                                        readFormat = 0x00;
                                        if (dataLength == 2)
                                        {
                                            readDatalength += 1;
                                        }
                                    }
                                    else
                                    {
                                        readFormat = 0x01;
                                    }

                                    if (header == "X" || header == "Y")
                                    {
                                        string octStr = Convert.ToString(num);
                                        num = Convert.ToInt32(octStr, 8);
                                    }

                                    byte lowRegNumByte = (byte)(num & 0xff);
                                    byte highRegNumByte = (byte)(num >> 8 & 0xff);

                                    byte[] payload = new byte[] { 0x50, 0x00, 0x00, 0xff, 0xff, 0x03, 0x00, 0x0C, 0x00, 0x10, 0x00, 0x01, 0x04, readFormat, 0x00, lowRegNumByte, highRegNumByte, 0x00, head, readDatalength, 0x00 };
                                    stream.Write(payload, 0, payload.Length);

                                    byte[] data = new Byte[20];

                                    stream.ReadTimeout = 1000;
                                    Int32 bytes = stream.Read(data, 0, data.Length);
                                    if (data[9] == 0 && data[10] == 0)
                                    {
                                        if (header == "D")
                                        {
                                            double afterConversion = 0;
                                            byte lowbyteResponse = data[11];
                                            byte hibyteResponse = data[12];
                                            if (dataLength == 2)
                                            {
                                                byte extralowbyteResponse = data[13];
                                                byte extrahighbyteResponse = data[14];

                                                if (other_format)
                                                { // 組合 4 個位元組成為浮點數
                                                    byte[] floatBytes = { lowbyteResponse, hibyteResponse, extralowbyteResponse, extrahighbyteResponse };
                                                    afterConversion = Math.Round(BitConverter.ToSingle(floatBytes, 0), 2);
                                                    Output_Val = afterConversion;
                                                }
                                                else
                                                {
                                                    afterConversion = (extrahighbyteResponse << 24) + (extralowbyteResponse << 16) + (hibyteResponse << 8) + lowbyteResponse;
                                                    Output_Val = afterConversion;
                                                }
                                                
                                            }
                                            else
                                            {
                                                afterConversion = (hibyteResponse << 8) + lowbyteResponse;
                                                Output_Val = afterConversion;
                                            }

                                            Logger.Info("Read Reg: " + header + " Num: " + num + " Value: " + afterConversion.ToString());
                                            return true;
                                        } else
                                        {
                                            byte lowbyteResponse = data[11];
                                            int afterConversion = lowbyteResponse >> 4;
                                            Output_Val = afterConversion;
                                            Logger.Info("Read Reg: " + header + " Num: " + num + " Value: " + afterConversion.ToString());
                                            return true;
                                        }
                                    }
                                    else
                                    {
                                        Logger.Error("Error in Answer");
                                        return false;
                                    }
                                case "Standard":
                                    Logger.Error($"Not Implement");
                                    return false;
                                default:
                                    Logger.Error($"TCP Protocol Not Support");
                                    return false;
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.Error($"TCP Protocol Error:" + ex.Message);
                        }
                    }
                    Logger.Debug("No data received or client is not connected.");
                    return false;
                case "Server":
                    try
                    {
                        throw new NotImplementedException();

                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"Not Implement");
                        return false;
                    }
                default:
                    return true;
            }
        }
        public override bool writeToRegister(string header, int num, string regData_str, uint dataLength, bool other_format )
        {

            

            switch (TCP_Mode)
            {
                case "Client":
                    if (client != null && client.Connected)
                    {
                        try
                        {
                            switch (TCP_Protocol)
                            {
                                case "SLMP":
                                    int regData = 0;
                                    double regData_double = 0;
                                    if (!other_format)
                                        regData = Convert.ToInt32(regData_str);
                                    else
                                        regData_double = Convert.ToDouble(regData_str);

                                        byte head;
                                    byte wrtieFormat = 0x00;
                                    byte requestDataLength = 0x0C;
                                    byte lowRegDataByte = 0x00;
                                    byte highRegDataByte = 0x00;
                                    byte extraHightRegDataByte = 0x00;
                                    byte extraLowtRegDataByte = 0x00;
                                    byte lowRegNumByte = 0x00;
                                    byte highRegNumByte = 0x00;
                                    byte writeDataLength = 0x01;
                                    switch (header) 
                                    {
                                        case "D":
                                            head = 0xA8;
                                            lowRegNumByte = (byte)(num & 0xff);
                                            highRegNumByte = (byte)(num >> 8 & 0xff);
                                            wrtieFormat = 0x00;
                                            requestDataLength += 2;
                                            if (dataLength == 2)
                                            {
                                                requestDataLength += 2;
                                                writeDataLength += 1;

                                                if (!other_format)
                                                {
                                                    lowRegDataByte = (byte)(regData & 0xff);
                                                    highRegDataByte = (byte)(regData >> 8 & 0xff);
                                                    extraLowtRegDataByte = (byte)(regData >> 16 & 0xff);
                                                    extraHightRegDataByte = (byte)(regData >> 24 & 0xff);
                                                }
                                                else 
                                                {
                                                    // 將 double 轉換為 8 個 byte
                                                    byte[] doubleBytes = BitConverter.GetBytes((float)regData_double);

                                                    // 前 4 個 byte 寫入當前暫存器
                                                    lowRegDataByte = doubleBytes[0];
                                                    highRegDataByte = doubleBytes[1];
                                                    extraLowtRegDataByte = doubleBytes[2];
                                                    extraHightRegDataByte = doubleBytes[3];

                                                }


                                            } else
                                            {
                                                lowRegDataByte = (byte)(regData & 0xff);
                                                highRegDataByte = (byte)(regData >> 8 & 0xff);
                                            }
                                            break;
                                        case "M":
                                            head = 0x90;
                                            lowRegNumByte = (byte)(num & 0xff);
                                            highRegNumByte = (byte)(num >> 8 & 0xff);
                                            wrtieFormat = 0x01;
                                            requestDataLength += 1;
                                            if (regData > 0)
                                            {
                                                lowRegDataByte = (byte)(regData << 4 & 0x10);
                                            }
                                            break;
                                        case "X":
                                            head = 0x9C;
                                            { 
                                                string octStr = Convert.ToString(num);
                                                num = Convert.ToInt32(octStr, 8);
                                            }
                                            lowRegNumByte = (byte)(num & 0xff);
                                            highRegNumByte = (byte)(num >> 8 & 0xff);
                                            wrtieFormat = 0x01;
                                            requestDataLength += 1;
                                            if (regData > 0)
                                            {
                                                lowRegDataByte = (byte)(regData << 4 & 0x10);
                                            }
                                            break;
                                        case "Y":
                                            head = 0x9D;
                                            {
                                                string octStr = Convert.ToString(num);
                                                num = Convert.ToInt32(octStr, 8);
                                            }
                                            lowRegNumByte = (byte)(num & 0xff);
                                            highRegNumByte = (byte)(num >> 8 & 0xff);
                                            wrtieFormat = 0x01;
                                            requestDataLength += 1;
                                            if (regData > 0)
                                            {
                                                lowRegDataByte = (byte)(regData << 4 & 0x10);
                                            }
                                            break;
                                        default:
                                            Logger.Error("Not support header type");
                                            return false;
                                    }
                                    

                                    byte[] payload = new byte[] { 0x50, 0x00, 0x00, 0xff, 0xff, 0x03, 0x00, requestDataLength, 0x00, 0x10, 0x00, 0x01, 0x14, wrtieFormat, 0x00, lowRegNumByte, highRegNumByte, 0x00, head, writeDataLength, 0x00};
                                    List<byte> payloadList = payload.ToList();
                                    if (header == "D")
                                    {
                                        payloadList.Add(lowRegDataByte);
                                        payloadList.Add(highRegDataByte);
                                        if (dataLength == 2)
                                        {
                                            payloadList.Add(extraLowtRegDataByte);
                                            payloadList.Add(extraHightRegDataByte);
                                        }
                                    } else
                                    {
                                        payloadList.Add(lowRegDataByte);
                                    }
                                    payload = payloadList.ToArray();
                                    stream.Write(payload, 0, payload.Length);

                                    byte[] data = new Byte[20];

                                    stream.ReadTimeout = 1000;
                                    Int32 bytes = stream.Read(data, 0, data.Length);
                                    if (data[9] == 0 && data[10] == 0)
                                    {
                                        if (_toggle && (header != "D"))
                                        {
                                            Thread.Sleep(100);
                                            lowRegDataByte = (byte)(~lowRegDataByte & 0x10);
                                            payload[payload.Length-1] = lowRegDataByte;
                                            stream.Write(payload, 0, payload.Length);
                                            bytes = stream.Read(data, 0, data.Length);
                                            if (data[9] == 0 && data[10] == 0)
                                            {
                                                return true;
                                            }
                                            else
                                            {
                                                Logger.Error("Error in toggle answer");
                                                return false;
                                            }
                                        }
                                        return true;
                                    }
                                    else
                                    {
                                        Logger.Error("Error in answer");
                                        return false;
                                    }
                                case "Standard":
                                    Logger.Error($"Not Implement");
                                    return false;
                                default:
                                    Logger.Error($"TCP Protocol Not Support");
                                    return false;
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.Error($"TCP Protocol Error:" + ex.Message);
                        }
                    }
                    Logger.Debug("No data received or client is not connected.");
                    return false;
                case "Server":
                    try
                    {
                        throw new NotImplementedException();

                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"Not Implement");
                        return false;
                    }
                default:
                    return true;
            }
        }
        public override bool writeRegisterFromFile(string filePath, uint dataLength)
        {
            try
            {
                // 從檔案名稱解析參數
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                string fileExtension = Path.GetExtension(filePath).TrimStart('.').ToUpper();

                // 解析檔案名稱格式: RegisterNumber_Count
                var parts = fileName.Split('_');
                if (parts.Length != 2)
                {
                    Logger.Error("Invalid file name format. Expected: RegisterNumber_Count");
                    return false;
                }

                // 解析 Register 和 Number
                string registerPart = parts[0];
                string header = "";
                int startAddress = 0;

                // 提取 header (D, M 等) 和 startAddress
                for (int i = 0; i < registerPart.Length; i++)
                {
                    if (char.IsDigit(registerPart[i]))
                    {
                        header = registerPart.Substring(0, i);
                        startAddress = int.Parse(registerPart.Substring(i));
                        break;
                    }
                }

                int count = int.Parse(parts[1]);

                // 根據副檔名確定格式
                FileFormat format;
                if (!Enum.TryParse(fileExtension, true, out format))
                {
                    Logger.Error($"Unsupported file format: {fileExtension}");
                    return false;
                }

                // 從檔案讀取資料
                var data = loadFromFile(filePath, format);
                if (data == null || !data.ContainsKey(header))
                {
                    Logger.Error("Failed to load data from file or header not found");
                    return false;
                }

                // 寫入暫存器
                return writeRegistersToDevice(header, data[header], dataLength);
            }
            catch (Exception ex)
            {
                Logger.Error($"Write from file error: {ex.Message}");
                return false;
            }
        }
        #endregion TCPCommunication
    }
    public class DataReceivedEventArgs1 : EventArgs
    {
        public string Data1 { get; }

        public DataReceivedEventArgs1(string data)
        {
            Data1 = data;
        }
    }

    public class Option : TypeConverter  //下拉式選單
    {
        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            List<string> OptionKeys = new List<string>();
            OptionKeys.Add("Client");
            OptionKeys.Add("Server");

            return new StandardValuesCollection(OptionKeys);
        }
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }
    }

    public class ProtocolOption : TypeConverter  //下拉式選單
    {
        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            List<string> OptionKeys = new List<string>();
            OptionKeys.Add("Standard");
            OptionKeys.Add("SLMP");

            return new StandardValuesCollection(OptionKeys);
        }
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }
    }
}
