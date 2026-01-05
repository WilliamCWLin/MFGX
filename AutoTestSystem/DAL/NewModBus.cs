using NModbus;
using NModbus.Serial;
using System;
using System.ComponentModel;
using System.IO.Ports;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using static AutoTestSystem.BLL.Bd;

namespace AutoTestSystem.DAL
{
    public class NewModBus : Communication
    {
        SerialPort SerialPort;
        public TcpClient client = null;
        //public IModbusSerialMaster modbusMaster;
        public IModbusMaster modbusMaster = null;
        public string Tcp_IP = string.Empty;
        public int Tcp_Port;
        public Connect_Mode SelectMode = Connect_Mode.SerialPort;

        /*public ModBus(ModBusSerialConnetInfo serialConnetInfo, string _logPath = "")
        {
            // 創建Modbus工廠
            ModbusFactory factory = new ModbusFactory();
            SerialPort = new SerialPort
            {
                PortName = serialConnetInfo.PortName,
                BaudRate = serialConnetInfo.BaudRate,
                DataBits = serialConnetInfo.DataBits,
                Parity = serialConnetInfo.Parity,
                StopBits = serialConnetInfo.StopBits,
                ReadTimeout = serialConnetInfo.ReadTimeout,
                WriteTimeout = serialConnetInfo.WriteTimeout,
                WriteBufferSize = serialConnetInfo.WriteBufferSize,
                ReadBufferSize = serialConnetInfo.ReadBufferSize
            };
            logPath = _logPath;
            modbusMaster = factory.CreateRtuMaster(SerialPort);

        }*/

        public void OpenCOM()
        {
            throw new System.NotImplementedException();
        }
        public bool ModBus_CHK()
        {
            if (modbusMaster == null)
                return false;
            switch (SelectMode)
            {
                case Connect_Mode.SerialPort:
                    if (SerialPort.IsOpen == false)
                        return false;
                    break;
                case Connect_Mode.TCPIP:
                    if (!IsSocketConnected(client))
                        return false;
                    break;
            }

            return true;
        }

        public override bool Write(byte slaveID, string Address, string writeData, TransmitMode mode)
        {

            try
            {
                switch (SelectMode)
                {
                    case Connect_Mode.SerialPort:
                        if (!SerialPort.IsOpen)
                        {
                            Logger.Error($"ModbusSend Master Comport is not connect");
                            return false;
                        }
                        break;
                    case Connect_Mode.TCPIP:
                        if (!IsSocketConnected(client))
                            return false;
                        break;
                }

                if (modbusMaster != null)
                {
                    ushort[] ushortArray = new ushort[] { 0 };
                    //Logger.Info($"Modbus--[SEND]");
                    Sleep(10);
                    if (mode == TransmitMode.Holding_registers_Single)
                    {
                        //ushortArray = new ushort[] { Convert.ToUInt16(writeData, 16)};
                        modbusMaster.WriteSingleRegister(slaveID, Convert.ToUInt16(Address, 16), Convert.ToUInt16(writeData, 16));
                    }
                    else if (mode == TransmitMode.Holding_registers_Multiple)
                    {

                        ushortArray = writeData.Replace(" ", ",").TrimEnd(',').Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries).Select(s => Convert.ToUInt16(s, 16)).ToArray();
                        modbusMaster.WriteMultipleRegisters(slaveID, Convert.ToUInt16(Address, 16), ushortArray);

                        // 將writeData轉換為ushort數組
                        /*ushortArray = writeData.Replace(" ", ",").TrimEnd(',').Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries).Select(s => Convert.ToUInt16(s, 16)).ToArray();

                        // 每次傳入2個寄存器
                        for (int i = 0; i < ushortArray.Length; i += 2)
                        {
                            ushort[] batch = ushortArray.Skip(i).Take(2).ToArray();
                            modbusMaster.WriteMultipleRegisters(slaveID, (ushort)(Convert.ToUInt16(Address, 16) +i), batch);
                        }*/


                    }
                    else if (mode == TransmitMode.SingleCoil_Status)
                    {

                        if (writeData == "True")
                        {

                            modbusMaster.WriteSingleCoil(slaveID, Convert.ToUInt16(Address,10), true);//0xFF00
                        }
                        else if (writeData == "False")
                        {
                            modbusMaster.WriteSingleCoil(slaveID, Convert.ToUInt16(Address, 10), false);//0x0000
                        }
                        else
                        {
                            Logger.Error($"ModbusSend {writeData} WriteSingleCoil Error,Must Send True or False");
                            return false;
                        }

                    }
                    else if (mode == TransmitMode.MultipleCoil_Status)
                    {
                        
                        string[] values = writeData.Contains(',') ? writeData.Split(',') : new string[] { writeData };
                        int[] decimalValues = Array.ConvertAll(values, val => Convert.ToInt32(val, 16));

                        //Check writeData contain only 0 or 1
                        foreach (int val in decimalValues)
                        {
                            if (val != 0 && val != 1 )
                            {
                                Logger.Error($"writeData must contain only '0' or '1'");
                                return false;
                            }
                        }

                        bool[] boolArray = Array.ConvertAll(decimalValues, val => val != 0);
                        modbusMaster.WriteMultipleCoils(slaveID, Convert.ToUInt16(Address, 10), boolArray);

                    }
                    else if(mode == TransmitMode.ModbusIO16Bit)
                    {
                        int decimalValue = Convert.ToInt32(writeData, 16);
                        string binaryString = Convert.ToString(Convert.ToInt32(writeData, 16), 2).PadLeft(16, '0');
                        bool[] boolArray = new bool[16];

                        for (int i = 0; i < 16; i++)
                        {
                            boolArray[15 - i] = (decimalValue & (1 << i)) != 0;
                            //boolArray[15-i] = binaryString[i] == '1';
                        }
                        modbusMaster.WriteMultipleCoils(slaveID, Convert.ToUInt16(Address, 10), boolArray);
                    }
                    else
                    {
                        Logger.Error($"ModbusSend Master is null");
                        return false;
                    }
                }
                else
                {
                    Logger.Error($"TransmitMode is Not Defind");
                    return false;
                }
            }
            catch (TimeoutException ex_Time)
            {
                if (slaveID == 0)
                {
                    return true;
                }
                //Logger.Warn($"Modbus Send Timeout:{Write_Timeout / 1000}s");
                Logger.Error($"TimeoutException:{ex_Time.Message}");
                return false;
            }
            catch (Exception Send_ex)
            {
                if (Send_ex.Message.Contains("Checksums failed to match"))
                {
                    Logger.Error($"Modbus Send Error=> invalid Code : Checksums failed to match");
                }
                else
                    Logger.Error($"Modbus Send Error=>{ Send_ex.Message}");
                return false;
            }

            return true;
        }

        public override async Task<bool> WriteAsync(byte slaveID, string Address, string writeData, TransmitMode mode)
        {
            try
            {
                if (SerialPort.IsOpen)
                {
                    if (modbusMaster != null)
                    {
                        //Logger.Info($"Modbus--[SEND]");
                        ushort[] ushortArray = new ushort[] { 0 };
                        if (mode == TransmitMode.Holding_registers_Single)
                        {
                            await modbusMaster.WriteSingleRegisterAsync(slaveID, Convert.ToUInt16(Address, 16), Convert.ToUInt16(writeData, 16));
                        }
                        else if (mode == TransmitMode.Holding_registers_Multiple)
                        {
                            ushortArray = writeData.Replace(" ", ",").TrimEnd(',').Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries).Select(s => Convert.ToUInt16(s, 16)).ToArray();
                            await modbusMaster.WriteMultipleRegistersAsync(slaveID, Convert.ToUInt16(Address, 16), ushortArray);
                        }
                        else if (mode == TransmitMode.Holding_registers_Single)
                        {
                            if (writeData == "True")
                            {
                                await modbusMaster.WriteSingleCoilAsync(slaveID, Convert.ToUInt16(Address, 10), true); // 0xFF00
                            }
                            else if (writeData == "False")
                            {
                                await modbusMaster.WriteSingleCoilAsync(slaveID, Convert.ToUInt16(Address, 10), false); // 0x0000
                            }
                        }
                        else if (mode == TransmitMode.Holding_registers_Multiple)
                        {
                            int decimalValue = Convert.ToInt32(writeData, 16);
                            bool[] boolArray = new bool[16];

                            for (int i = 0; i < 16; i++)
                            {
                                boolArray[15 - i] = (decimalValue & (1 << i)) != 0;
                            }
                            await modbusMaster.WriteMultipleCoilsAsync(slaveID, Convert.ToUInt16(Address, 16), boolArray);
                        }
                        else
                        {
                            Logger.Error($"ModbusSend Master is null");
                            return false;
                        }
                    }
                    else
                    {
                        Logger.Error($"TransmitMode is Not Defined");
                        return false;
                    }
                }
                else
                {
                    Logger.Error($"ModbusSend Master Comport is not connected");
                    return false;
                }
            }
            catch (Exception Send_ex)
            {
                if (Send_ex.Message.Contains("Checksums failed to match"))
                {
                    Logger.Error($"Modbus Send Error => invalid Code: Checksums failed to match");
                }
                else
                {
                    Logger.Error($"Modbus Send Error => {Send_ex.Message}");
                }
                return false;
            }
            return true;
        }

        //public override bool Read(byte slaveID, string Address, ushort numRegisters, ref ushort[] DataRecAll, TransmitMode mode)
        //{
        //    try
        //    {
        //        switch (SelectMode)
        //        {
        //            case Connect_Mode.SerialPort:
        //                if (!SerialPort.IsOpen)
        //                {
        //                    Logger.Error($"ModbusSend Master Comport is not connect");
        //                    return false;
        //                }
        //                break;
        //            case Connect_Mode.TCPIP:
        //                if (!IsSocketConnected(client))
        //                    return false;
        //                break;
        //        }

        //        if (modbusMaster != null)
        //        {
        //            //Logger.Info($"Modbus--[READ]");
        //            Sleep(5);
        //            if (mode == TransmitMode.Holding_registers_Single || mode == TransmitMode.Holding_registers_Multiple)
        //            {
        //                // 讀取保持寄存器 (功能碼 0x03)
        //                ushort[] registers = modbusMaster.ReadHoldingRegisters(slaveID, Convert.ToUInt16(Address, 16), numRegisters);
        //                DataRecAll = registers;
        //            }
        //            else if (mode == TransmitMode.SingleCoil_Status || mode == TransmitMode.MultipleCoil_Status || mode == TransmitMode.ModbusIO16Bit)
        //            {
        //                // 讀取線圈狀態 (功能碼 0x01)
        //                bool[] coilStatus = modbusMaster.ReadCoils(slaveID, Convert.ToUInt16(Address, 10), numRegisters);

        //                ushort[] registers = ConvertToUshortArray(coilStatus);
        //                DataRecAll = registers;
        //            }
        //            else if (mode == TransmitMode.Input_Status)
        //            {
        //                // 讀取離散量 (功能碼 0x02)
        //                bool[] discreteInputs = modbusMaster.ReadInputs(slaveID, Convert.ToUInt16(Address, 16), numRegisters);

        //                ushort[] registers = ConvertToUshortArray(discreteInputs);
        //                DataRecAll = registers;
        //            }
        //            else
        //            {
        //                Logger.Error($"TransmitMode is Not Defind");
        //                return false;
        //            }
        //        }
        //        else
        //        {
        //            Logger.Error($"ModbusRead Master is null");
        //            return false;
        //        }
        //    }
        //    catch (TimeoutException ex_Time)
        //    {
        //        /*if (Read_Timeout == 0)
        //        {
        //            return true;
        //        }*/
        //        Logger.Warn($"Modbus Read Timeout:{modbusMaster.Transport.ReadTimeout / 1000}s");
        //        Logger.Error($"TimeoutException:{ex_Time.Message}");
        //        return false;
        //    }
        //    catch (Exception rend_ex)
        //    {
        //        if (rend_ex.Message.Contains("Checksums failed to match"))
        //        {
        //            Logger.Error($"Modbus Read Error=> invalid Code : Checksums failed to match");
        //        }
        //        else
        //            Logger.Error($"Modbus Read Error=>{ rend_ex.Message}");

        //        return false;
        //    }

        //    return true;
        //}
        public override bool Read(byte slaveID, string Address, ushort numRegisters, ref ushort[] DataRecAll, TransmitMode mode)
        {
            int retryCount = 0;
            const int maxRetries = 2;

        Retry:
            try
            {
                switch (SelectMode)
                {
                    case Connect_Mode.SerialPort:
                        if (!SerialPort.IsOpen)
                        {
                            Logger.Error($"ModbusSend Master Comport is not connect");
                            return false;
                        }
                        break;
                    case Connect_Mode.TCPIP:
                        if (!IsSocketConnected(client))
                            return false;
                        break;
                }

                if (modbusMaster != null)
                {
                    Sleep(5);
                    if (mode == TransmitMode.Holding_registers_Single || mode == TransmitMode.Holding_registers_Multiple)
                    {
                        ushort[] registers = modbusMaster.ReadHoldingRegisters(slaveID, Convert.ToUInt16(Address, 16), numRegisters);
                        DataRecAll = registers;
                    }
                    else if (mode == TransmitMode.SingleCoil_Status || mode == TransmitMode.MultipleCoil_Status || mode == TransmitMode.ModbusIO16Bit)
                    {
                        bool[] coilStatus = modbusMaster.ReadCoils(slaveID, Convert.ToUInt16(Address, 10), numRegisters);
                        DataRecAll = ConvertToUshortArray(coilStatus);
                    }
                    else if (mode == TransmitMode.Input_Status)
                    {
                        bool[] discreteInputs = modbusMaster.ReadInputs(slaveID, Convert.ToUInt16(Address, 16), numRegisters);
                        DataRecAll = ConvertToUshortArray(discreteInputs);
                    }
                    else
                    {
                        Logger.Error($"TransmitMode is Not Defined");
                        return false;
                    }
                }
                else
                {
                    Logger.Error($"ModbusRead Master is null");
                    return false;
                }
            }
            catch (TimeoutException ex_Time)
            {
                Logger.Warn($"Modbus Read Timeout:{modbusMaster.Transport.ReadTimeout / 1000}s");
                Logger.Error($"TimeoutException:{ex_Time.Message}");

                if (retryCount < maxRetries)
                {
                    retryCount++;
                    Logger.Warn($"Retrying Modbus Read... Attempt {retryCount}");
                    goto Retry;
                }

                return false;
            }
            catch (Exception rend_ex)
            {
                if (rend_ex.Message.Contains("Checksums failed to match"))
                {
                    Logger.Error($"Modbus Read Error => invalid Code : Checksums failed to match");
                }
                else
                {
                    Logger.Error($"Modbus Read Error => {rend_ex.Message}");
                }

                if (retryCount < maxRetries)
                {
                    retryCount++;
                    Logger.Warn($"Retrying Modbus Read... Attempt {retryCount}");
                    goto Retry;
                }

                return false;
            }

            return true;
        }

        public override bool Read(byte slaveID, string Address, ushort numRegisters, ref bool [] DataRecAll, TransmitMode mode)
        {
            try
            {
                switch (SelectMode)
                {
                    case Connect_Mode.SerialPort:
                        if (!SerialPort.IsOpen)
                        {
                            Logger.Error($"ModbusSend Master Comport is not connect");
                            return false;
                        }
                        break;
                    case Connect_Mode.TCPIP:
                        if (!IsSocketConnected(client))
                            return false;
                        break;
                }

                if (modbusMaster != null)
                {
                    //Logger.Info($"Modbus--[READ]");
                    Sleep(5);
                    if (mode == TransmitMode.SingleCoil_Status || mode == TransmitMode.MultipleCoil_Status || mode == TransmitMode.ModbusIO16Bit)
                    {

                        ushort startAddress = ParseAddress(Address);
                        bool[] coilStatus = modbusMaster.ReadCoils(slaveID, startAddress, numRegisters);

                        // 讀取線圈狀態 (功能碼 0x01)
                        //bool[] coilStatus = modbusMaster.ReadCoils(slaveID, Convert.ToUInt16(Address, 10), numRegisters);

                        DataRecAll = coilStatus;
                    }
                    else if (mode == TransmitMode.Input_Status)
                    {
                        // 讀取離散量 (功能碼 0x02)
                        bool[] discreteInputs = modbusMaster.ReadInputs(slaveID, Convert.ToUInt16(Address, 16), numRegisters);
                        
                        DataRecAll = discreteInputs;
                    }
                    else
                    {
                        Logger.Error($"TransmitMode is Not Defind");
                        return false;
                    }
                }
                else
                {
                    Logger.Error($"ModbusRead Master is null");
                    return false;
                }
            }
            catch (TimeoutException ex_Time)
            {
                /*if (Read_Timeout == 0)
                {
                    return true;
                }*/
                Logger.Warn($"Modbus Read Timeout:{modbusMaster.Transport.ReadTimeout / 1000}s");
                Logger.Error($"TimeoutException:{ex_Time.Message}");
                return false;
            }
            catch (Exception rend_ex)
            {
                if (rend_ex.Message.Contains("Checksums failed to match"))
                {
                    Logger.Error($"Modbus Read Error=> invalid Code : Checksums failed to match");
                }
                else
                    Logger.Error($"Modbus Read Error=>{ rend_ex.Message}");

                return false;
            }

            return true;
        }

        public static ushort ParseAddress(string addressStr)
        {
            if (addressStr.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                return Convert.ToUInt16(addressStr.Substring(2), 16);
            }
            else
            {
                return Convert.ToUInt16(addressStr, 10);
            }
        }

        public override async Task<ushort[]> ReadAsync(byte slaveID, string Address, ushort numRegisters, TransmitMode mode)
        {
            ushort[] DataRecAll = null;
            //Task t = null;
            try
            {

                if (SerialPort.IsOpen)
                {
                    if (modbusMaster != null)
                    {
                        //await Task.Delay(50);
                        //Logger.Info($"Modbus--[READ]");
                        if (mode == TransmitMode.Holding_registers_Single || mode == TransmitMode.Holding_registers_Multiple)
                        {
                            // 讀取保持寄存器 (功能碼 0x03)
                            ushort[] registers = await modbusMaster.ReadHoldingRegistersAsync(slaveID, Convert.ToUInt16(Address, 16), numRegisters);
                            DataRecAll = registers;
                        }
                        else if (mode == TransmitMode.Holding_registers_Single || mode == TransmitMode.Holding_registers_Multiple)
                        {
                            // 讀取線圈狀態 (功能碼 0x01)
                            bool[] coilStatus = await modbusMaster.ReadCoilsAsync(slaveID, Convert.ToUInt16(Address, 16), numRegisters);
                            ushort[] registers = ConvertToUshortArray(coilStatus);
                            DataRecAll = registers;
                        }
                        else if (mode == TransmitMode.Input_Status)
                        {
                            // 讀取離散量 (功能碼 0x02)
                            bool[] discreteInputs = await modbusMaster.ReadInputsAsync(slaveID, Convert.ToUInt16(Address, 16), numRegisters);
                            ushort[] registers = ConvertToUshortArray(discreteInputs);
                            DataRecAll = registers;
                        }
                        else
                        {
                            Logger.Error($"TransmitMode is Not Defined");
                            return null;
                        }
                    }
                    else
                    {
                        Logger.Error($"ModbusRead Master is null");
                        return null;
                    }
                }
                else
                {
                    Logger.Error($"ModbusSend Master Comport is not connected");
                    return null;
                }
            }
            catch (SlaveException Slave_ex)
            {
                Logger.Error($"SlaveException: {Slave_ex.Message}");
                return null;
            }
            catch (TimeoutException ex_Time)
            {
                Logger.Warn($"Modbus Read Timeout: {modbusMaster.Transport.ReadTimeout / 1000}s");
                Logger.Error($"TimeoutException: {ex_Time.Message}");
                return null;
            }
            catch (Exception rend_ex)
            {
                if (rend_ex.Message.Contains("Checksums failed to match"))
                {
                    Logger.Error($"Modbus Read Error => invalid Code: Checksums failed to match");
                }
                else
                {
                    Logger.Error($"Modbus Read Error => {rend_ex.Message}");
                }
                return null;
            }

            return DataRecAll;
        }

        public override bool Open()
        {
            try
            {
                switch (SelectMode)
                {
                    case Connect_Mode.SerialPort:
                        // 創建Modbus工廠
                        ModbusFactory factory = new ModbusFactory();

                        modbusMaster = factory.CreateRtuMaster(SerialPort);

                        if (SerialPort.IsOpen == false)
                        {
                            SerialPort.Open();
                            //SerialPort.DataReceived += ComPort_DataReceived;
                            Logger.Info($"{SerialPort.PortName} serialPort.Open()!!");
                        }
                        break;
                    case Connect_Mode.TCPIP:
                        // 建立 TCP 客戶端
                        client = new TcpClient();
                        Task connectTask = client.ConnectAsync(Tcp_IP, Tcp_Port);
                        if (!connectTask.Wait(TimeSpan.FromMilliseconds(10000)))
                        {
                            Logger.Error($"ModBus TCP Connected TimeOut");
                            return false;
                        }
                        if (IsSocketConnected(client))
                        {
                            // 使用 NModbus 套件建立 Modbus TCP 主站
                            ModbusFactory Tcpfactory = new ModbusFactory();
                            modbusMaster = Tcpfactory.CreateMaster(client);
                        }
                        else
                        {
                            return false;
                        }
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"ModBus Open Exception: {ex.ToString()}");
                return false;
            }
            return true;
        }


        public bool IsSocketConnected(TcpClient client)
        {
            if (client == null || client.Client == null)
            {
                Logger.Error($"ModBus TCP device is null");
                return false;
            }

            // 檢查連線狀態
            if (!client.Client.Connected || client.Client.RemoteEndPoint == null)
            {
                Logger.Error($"ModBus TCP Connected Fail");
                return false;
            }
            return true;
        }

        /*public override bool Open(ModBusSerialConnetInfo serialConnetInfo)
        {
            try
            {
                SerialPort = new SerialPort
                {                 
                    PortName = serialConnetInfo.PortName,
                    BaudRate = serialConnetInfo.BaudRate,
                    DataBits = serialConnetInfo.DataBits,
                    Parity = serialConnetInfo.Parity,
                    StopBits = serialConnetInfo.StopBits,
                    ReadTimeout = serialConnetInfo.ReadTimeout,
                    WriteTimeout = serialConnetInfo.WriteTimeout,
                    WriteBufferSize = serialConnetInfo.WriteBufferSize,
                    ReadBufferSize = serialConnetInfo.ReadBufferSize
                };

                // 創建Modbus工廠
                ModbusFactory factory = new ModbusFactory();             
                modbusMaster = factory.CreateRtuMaster(SerialPort);

                if (SerialPort.IsOpen == false)
                {
                    SerialPort.Open();
                    //SerialPort.DataReceived += ComPort_DataReceived;
                    Logger.Info($"{SerialPort.PortName} serialPort.Open()!!");
                }
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error($"ModBus Open Exception: {ex.ToString()}");
                return false;
            }
        }*/

        static ushort[] ConvertToUshortArray(bool[] discreteInputs)
        {
            ushort[] registers = new ushort[discreteInputs.Length];

            for (int i = 0; i < discreteInputs.Length; i++)
            {
                registers[i] = discreteInputs[i] ? (ushort)1 : (ushort)0;
            }

            return registers;
        }

        public override void Close()
        {
            try
            {
                switch (SelectMode)
                {
                    case Connect_Mode.SerialPort:                                              
                        if (SerialPort != null)
                        {
                            Logger.Info($"{SerialPort.PortName} serialPort.Close!!");
                            // 關閉 SerialPort 連接                            
                            SerialPort?.DiscardOutBuffer();
                            SerialPort?.DiscardInBuffer();
                            SerialPort.Close();
                            SerialPort.Dispose();
                            SerialPort = null;
                        }                                              
                        if (modbusMaster != null)
                        {
                            modbusMaster.Dispose();
                            modbusMaster = null;
                        }
                        Logger.Info($"SerialPort Close!!");
                        break;
                    case Connect_Mode.TCPIP:
                        Logger.Info($"TCPIP.Close!!");
                        if (client != null)
                        {
                            // 關閉 TCP 連接
                            client.Close();
                            client = null;
                        }
                        if (modbusMaster != null)
                        {
                            modbusMaster.Dispose();
                            modbusMaster = null;
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.Fatal($"{ex.ToString()}");
                //throw;
            }
        }

        public string ModBus_ComportInfo()
        {
            switch (SelectMode)
            {
                case Connect_Mode.SerialPort:
                    return $"{SerialPort.PortName}";
                case Connect_Mode.TCPIP:
                    return $"TCP ";
            }
            return "UnDefind Devices";
        }
        /// <summary>
        /// 串口数据写入
        /// </summary>
        public override void Write(string data)
        {
            throw new System.NotImplementedException();
        }

        public override void Write(byte[] data)
        {
            throw new System.NotImplementedException();
        }

        public override void WriteLine(string sendstr)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// 串口读数据
        /// </summary>

        public override bool SendCommand(string command, ref string strRecAll, string DataToWaitFor, int timeout = 10)
        {
            Logger.Error($"Is not Use in ModBus Device ,FAIL!!!");
            return false;
        }

        public override void Dispose()
        {
            throw new System.NotImplementedException();
        }

        public override string Read()
        {
            Logger.Error($"Is not Use in ModBus Device ,FAIL!!!");
            return null;
        }

        public void SetTimeout(int Write_time, int Read_time)
        {
            modbusMaster.Transport.ReadTimeout = Read_time;
            modbusMaster.Transport.WriteTimeout = Write_time; // 1000ms
        }

        public void RetryTimes(int time)
        {
            modbusMaster.Transport.Retries = time;
            modbusMaster.Transport.WaitToRetryMilliseconds = 250;
        }

        public void ModbusTcpConnectInfo(string ipAddress, int port)
        {
            Tcp_IP = ipAddress;
            Tcp_Port = port;
        }

        public void ModbusSerialPortConnectInfo(ModBusSerialConnetInfo serialConnetInfo)
        {
            SerialPort = new SerialPort
            {
                PortName = serialConnetInfo.PortName,
                BaudRate = serialConnetInfo.BaudRate,
                DataBits = serialConnetInfo.DataBits,
                Parity = serialConnetInfo.Parity,
                StopBits = serialConnetInfo.StopBits,
                ReadTimeout = serialConnetInfo.ReadTimeout,
                WriteTimeout = serialConnetInfo.WriteTimeout,
                WriteBufferSize = serialConnetInfo.WriteBufferSize,
                ReadBufferSize = serialConnetInfo.ReadBufferSize
            };
        }

        public void ModbusConnectMode(Connect_Mode mode)
        {
            SelectMode = mode;
        }

    }

    public enum Connect_Mode
    {
        SerialPort,
        TCPIP
    }

    public class ModBusSerialConnetInfo
    {
        public string PortName { get; set; }
        public int BaudRate { get; set; }
        public int DataBits { get; private set; } = 8;
        public StopBits StopBits { get; set; } = StopBits.One;
        public Parity Parity { get; set; } = Parity.None;
        public int WriteTimeout { get; set; } = 0x1388;
        public int ReadTimeout { get; set; } = 0x1388;
        public int WriteBufferSize { get; set; } = 0x400;
        public int ReadBufferSize { get; set; } = 0x400;
    }



    public class ModBusComportList : TypeConverter  //下拉式選單
    {
        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            string[] portNames = SerialPort.GetPortNames();
            if (portNames.Length > 0)
            {
                return new StandardValuesCollection(portNames.ToArray());
            }
            else
            {
                return new StandardValuesCollection(new int[] { });
            }
        }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

    }
}