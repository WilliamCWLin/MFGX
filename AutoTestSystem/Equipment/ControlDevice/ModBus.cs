using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AutoTestSystem.Base;
using AutoTestSystem.DAL;
using NModbus;
using NModbus.Serial;
using static AutoTestSystem.BLL.Bd;

namespace AutoTestSystem.Equipment.ControlDevice
{
    class ModBus : ControlDeviceBase
    {
        private string receivedValue = null;
        [Category("Comport Parameters"), Description("Select Comport"), TypeConverter(typeof(ComportList))]
        public string PortName { get; set; }

        [Category("Comport Parameters"), Description("baudRate")]
        public int BaudRate { get; set; } = 9600;

        [Category("Comport Parameters"), Description("DataBits")]
        public int DataBits { get; set; } = 8;

        /*[Category("NModBus Parameters"), Description("slaveID")]
        public byte slaveID { get; set; }*/

        Comport DeviceComport = null;
        SerialPort serialPort = null;
        IModbusSerialMaster modbusMaster = null;



        public ModBus()
        {
            BaudRate = 9600;
        }



        public override bool Init(string strParamInfo)
        {

                if (string.IsNullOrEmpty(PortName))
                {
                    LogMessage("NO COM Port Name", MessageLevel.Error);
                    //MessageBox.Show("NO COM Port Name", "Warning!!!");
                    return false;
                }

            try
            {
                // 設定串口參數
                Parity parity = Parity.None;
                StopBits stopBits = StopBits.One;
                serialPort = new SerialPort(PortName, BaudRate, parity, DataBits, stopBits);

                if (!serialPort.IsOpen)
                {
                   serialPort.Open();
                   LogMessage($"{serialPort.PortName} serialPort.OpenCOM(), Init Sucessed",MessageLevel.Info);

                    // 創建Modbus工廠
                    ModbusFactory factory = new ModbusFactory();
                    // 創建Modbus主機 (Master)
                    modbusMaster = factory.CreateRtuMaster(serialPort);
                    LogMessage($" Modbus Create Sucessed", MessageLevel.Info);
                }
             }
             catch (Exception ex)
             {
                LogMessage($"Modbus Error=>{ ex.Message} Init Fail", MessageLevel.Error);
                return false;
             }

            return true;
        }

        public override void OPEN()
        {
            serialPort.Open();
        }

        public override bool Status(ref string msg)
        {
            try
            {
                if (modbusMaster != null)
                {
                    if (serialPort.IsOpen)
                    {
                        msg = $"{serialPort.PortName}(OPEN)";
                        return true;
                    }
                    else
                    {
                        msg = $"{serialPort.PortName}(CLOSE)";
                        return false;
                    }
                }else
                {
                    msg = $"modbusMaster is  null, Modbus Create Fail";
                    return false;
                }
            }
            catch(Exception ex)
            {
                msg = $"{ex.Message}";
                return false;
            }

        }


        public override bool UnInit()
        {
            if (serialPort == null || modbusMaster == null)
                return false;

            modbusMaster.Dispose();
            serialPort.Close();
            return true;
        }

        public override bool ModbusSend(byte slaveID,ushort Address, ushort writeData)
        {
            try
            {
                if (modbusMaster != null)
                {
                    ushort[] ushortArray = new ushort[] { writeData };
                    modbusMaster.WriteMultipleRegisters(slaveID, Address, ushortArray);
                }
                else
                {
                    LogMessage($"ModbusSend Master is null", MessageLevel.Error);
                    return false;
                }

            }
            catch(Exception Send_ex)
            {
                LogMessage($"Modbus Send Error=>{ Send_ex.Message}", MessageLevel.Error);
                return false;
            }

            return true;
        }


        public override bool ModbusRead(byte slaveID,ushort Address,ushort numRegisters,ref ushort[] DataRecAll)
        {
            try
            {
                if (modbusMaster != null)
                {
                    modbusMaster.Transport.ReadTimeout = 1000; // 5秒
                    ushort[] registers = modbusMaster.ReadHoldingRegisters(slaveID, Address, numRegisters);
                    DataRecAll = registers;

                    /*foreach (var register in registers)
                    {
                        strRecAll += $"{register} ";
                    }*/
                }
                else
                {
                    LogMessage($"ModbusRead Master is null", MessageLevel.Error);
                    return false;
                }
            }
            catch (TimeoutException ex_Time)
            {
                LogMessage($"Modbus Read Timeout:{modbusMaster.Transport.ReadTimeout / 1000}s", MessageLevel.Warn);
                LogMessage($"TimeoutException:{ex_Time.Message}", MessageLevel.Error);
                return false;
            }
            catch (Exception rend_ex)
            {
                LogMessage($"Modbus Rend Error=>{ rend_ex.Message}", MessageLevel.Error);
                return false;
            }

            return true;
        }


        public override bool SEND(string command)
        {

            // 切換 RS485 串口的發送模式
            DeviceComport.SerialPort.RtsEnable = true;
            DeviceComport.SerialPort.DtrEnable = true;

            // 發送命令
            Write(command);

            return true;
        }

        public override bool READ(ref string output)
        {
            
            output =DeviceComport.ReadData();
            //MessageBox.Show("Arduino讀取完成:" + output);
            if(string.IsNullOrEmpty(output))
                return false;
            return true;
        }

        public override void Dispose()
        {
            throw new NotImplementedException();
        }

        private static byte[] HexStringToBytes(string hexString)
        {

            if (hexString == null || hexString.Length % 2 != 0)
            {
                throw new ArgumentException("Invalid Hex string");
            }

            byte[] result = new byte[hexString.Length / 2];
            for (int i = 0; i < hexString.Length; i += 2)
            {
                result[i / 2] = Convert.ToByte(hexString.Substring(i, 2), 16);
            }

            return result;
        }
        public bool Write(string data)
        {
            byte[] hexdata = HexStringToBytes(data);

            try
            {
                DeviceComport.SerialPort.Write(hexdata, 0, hexdata.Length);
            }
            catch (Exception ex)
            {
                LogMessage($"SerialPort Write Error:{ex.Message}",MessageLevel.Error);
                return false;
            }
            return true;
        }
              
        #region**SendModbusData 打包發送數據**
        /*public override bool SendHex(byte[] values, ref string strRecAll, int waitforbyte, int timeout)
        {
            //Ensure port is open:
            if (DeviceComport.SerialPort.IsOpen)
            {
                //Clear in/out buffers:
                DeviceComport.SerialPort.DiscardOutBuffer();//清空發送、接收緩衝區字節
                DeviceComport.SerialPort.DiscardInBuffer();

                byte[] response = new byte[values.Length + 2];
                Array.Copy(values, response, values.Length);

                //打包帶有 CRC 驗證的modbus 數據包:
                byte[] CRC = new byte[2];
                GetCRC(response, ref CRC);

                response[0] = Convert.ToByte(response[0]);//地址
                response[1] = Convert.ToByte(response[1]);//功能
                response[2] = Convert.ToByte(response[2]);
                response[3] = Convert.ToByte(response[3]);
                response[4] = Convert.ToByte(response[4]);
                response[5] = Convert.ToByte(response[5]);

                response[response.Length - 2] = CRC[0];
                response[response.Length - 1] = CRC[1];
                //values = response; //返回帶有 CRC 驗證的modbus 數據包

                //Send modbus message to Serial Port:
                try
                {
                    receivedValue = "";
                    //dataReceivedEvent.Reset();

                    DeviceComport.SerialPort.Write(response, 0, response.Length);
                    Thread.Sleep(1);
                    long lngStart = DateTime.Now.AddSeconds(timeout).Ticks;
                    while (DeviceComport.SerialPort.BytesToRead != waitforbyte)
                    {

                        var lngCurTime = DateTime.Now.Ticks;
                        if (lngCurTime > lngStart)
                        {
                            DeviceComport.SerialPort.DiscardInBuffer();
                            strRecAll = null;
                            return false;
                        }
                        else
                        {
                            Thread.Sleep(1);
                        }

                    }

                    DataReceived_485();

                    strRecAll = receivedValue;

                    return true;
                }
                catch (Exception)
                {
                    return false;
                }

            }
            else
            {
                return false;
            }

        }*/
        #endregion

        private void GetResponse(ref byte[] response)
        {
            //There is a bug in .Net 2.0 DataReceived Event that prevents people from using this
            //event as an interrupt to handle data (it doesn't fire all of the time).  Therefore
            //we have to use the ReadByte command for a fixed length as it's been shown to be reliable.
            for (int i = 0; i < response.Length; i++)
            {
                response[i] = (byte)(DeviceComport.SerialPort.ReadByte());//從輸入緩衝區同步讀取一個字節
            }
            return;
        }

        private static string BytesToString(byte[] bytes, int startIndex = 0, int length = -1)
        {
            if (length == -1)
                length = bytes.Length - startIndex;
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < length; i++)
            {
                sb.Append(bytes[startIndex + i].ToString("X2"));
            }
            return sb.ToString();
        }

        private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            //SerialPort sp = (SerialPort)sender;
            //string indata = sp.ReadExisting();
            //sReceiveAll += indata;
            //InputEvent.Set();
            DataReceived_485();
        }

        private bool CheckResponse(byte[] response)
        {
            //Perform a basic CRC check:
            byte[] CRC = new byte[2];
            GetCRC(response, ref CRC);
            if (CRC[0] == response[response.Length - 2] && CRC[1] == response[response.Length - 1])
                return true;
            else
                return false;
        }
        public void GetCRC(byte[] message, ref byte[] CRC)
        {
            //Function expects a modbus message of any length as well as a 2 byte CRC array in which to 
            //return the CRC values:

            ushort CRCFull = 0xFFFF;
            byte CRCHigh = 0xFF, CRCLow = 0xFF;
            char CRCLSB;

            for (int i = 0; i < (message.Length) - 2; i++)
            {
                CRCFull = (ushort)(CRCFull ^ message[i]);

                for (int j = 0; j < 8; j++)
                {
                    CRCLSB = (char)(CRCFull & 0x0001);
                    CRCFull = (ushort)((CRCFull >> 1) & 0x7FFF);

                    if (CRCLSB == 1)
                        CRCFull = (ushort)(CRCFull ^ 0xA001);
                }
            }
            CRC[1] = CRCHigh = (byte)((CRCFull >> 8) & 0xFF);
            CRC[0] = CRCLow = (byte)(CRCFull & 0xFF);
        }
        public void DataReceived_485()
        {
            //Console.WriteLine("kkk");
            DateTime dt = DateTime.Now;
            if (DeviceComport.SerialPort.IsOpen)
            {
                try
                {
                    //string str = readBuffer.ToString();
                    int count = DeviceComport.SerialPort.BytesToRead;//讀取緩衝區數據字節（8位）數
                    if (count > 0)
                    {
                        Byte[] readBuffer = new Byte[DeviceComport.SerialPort.BytesToRead];//創建接收字節數組
                        DeviceComport.SerialPort.Read(readBuffer, 0, readBuffer.Length);//讀取接收的數據
                        String RecvDataText = null;
                        String RescDataCRC = null;

                        // CRC 驗證
                        if (CheckResponse(readBuffer))
                        {
                            //顯示輸入數據
                            for (int i = 0; i < readBuffer.Length; i++)
                            {
                                RecvDataText += (readBuffer[i].ToString("X2"));

                                //RecvDataText += (ReceivedData[i].ToString("X2") + " ");
                            }
                            byte[] CRC = new byte[2];
                            GetCRC(readBuffer, ref CRC);
                            //未進行CRC校驗
                            for (int j = 0; j < 2; j++)
                            {
                                RescDataCRC += (CRC[j].ToString("X2"));
                            }
                            //TextBox_Rec.Text
                            receivedValue += RecvDataText/* + "\r\n"*/;
                            DeviceComport.SerialPort.DiscardInBuffer();//清除串行驅動程序的接收緩衝區的數據；  
                            //dataReceivedEvent.Set();
                        }
                        else
                        {
                            //modbusStatus = "CRC校驗 error";
                            DeviceComport.SerialPort.DiscardInBuffer();
                        }
                    }
                    else
                    {
                        //**************************************************************************************************
                        //TextBox_Rec.Text += "讀取緩衝區數據失敗，請重啓系統" + "\r\n";
                    }
                }
                catch (Exception)
                {
                    DeviceComport.SerialPort.DiscardInBuffer();//讀取緩衝區數據字節（8位）數   失敗                           
                }


            }
            else
            {
                // modbusStatus = "Serial port not open";                    
            }
            //Console.WriteLine(weightdate);
        }
    
    public override void ClearBuffer()
        {
            DeviceComport.cleanBuffer();
        }

        public override void SetTimeout(int time)
        {
            DeviceComport.SetReadTimeout(time);
        }

        public class ComportList : TypeConverter  //下拉式選單
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
}
