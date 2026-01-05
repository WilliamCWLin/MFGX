using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AutoTestSystem.Base;
using AutoTestSystem.DAL;
using NModbus;
using NModbus.Serial;
using static AutoTestSystem.BLL.Bd;
using static AutoTestSystem.DAL.Communication;

namespace AutoTestSystem.Equipment.ControlDevice
{
    class ModBusController : ControlDeviceBase
    {
        [Category("Common Parameters"), Description("SlaveID")]
        public byte SlaveID { get; set; } = 1;

        [Category("Connect Mode"), Description("Connect Method")]
        public Connect_Mode Mode { get; set; } = Connect_Mode.SerialPort;

        /*[Category("NModBus Parameters"), Description("SlaveID"), Editor(typeof(ModBusEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public string SlaveID { get; set; }*/

        [Category("Comport Parameters"), Description("Select Comport"), TypeConverter(typeof(ModBusComportList))]
        public string PortName { get; set; }

        [Category("Comport Parameters"), Description("baudRate")]
        public int baudRate { get; set; } = 9600;

        [Category("TCP Connect Parameters"), Description("Select IP")]
        public string IP { get; set; } = "";

        [Category("TCP Connect Parameters"), Description("Select Port")]
        public int Port { get; set; } = 502;


        NewModBus modbusMaster = null;
        private TcpClient client = null;

        public ModBusController()
        {
            baudRate = 9600;
        }

        public override bool Init(string strParamInfo)
        {
            try
            {
                switch (Mode)
                {
                    case Connect_Mode.SerialPort:

                        if (string.IsNullOrEmpty(PortName))
                        {
                            LogMessage("NO COM Port Name", MessageLevel.Error);
                            return false;
                        }
                        // 設定串口參數
                        ModBusSerialConnetInfo ModBusDevieCOMinfo = new ModBusSerialConnetInfo { PortName = PortName, BaudRate = baudRate };

                      
                        if (modbusMaster == null)
                        {
                            modbusMaster = new NewModBus();
                            modbusMaster.ModbusConnectMode(Connect_Mode.SerialPort);
                            modbusMaster.ModbusSerialPortConnectInfo(ModBusDevieCOMinfo);
                            if (!modbusMaster.Open())
                            {
                                LogMessage("Init ModBus Fail", MessageLevel.Error);
                                return false;
                            }                        

                        }
                        else
                        {
                            if (!modbusMaster.IsOpen)
                            {
                                //modbusMaster.Close();
                                //modbusMaster = null;
                                modbusMaster = new NewModBus();
                                modbusMaster.ModbusConnectMode(Connect_Mode.SerialPort);
                                modbusMaster.ModbusSerialPortConnectInfo(ModBusDevieCOMinfo);
                                if (!modbusMaster.Open())
                                {
                                    LogMessage("Init ModBus Fail", MessageLevel.Error);
                                    return false;
                                }                             
                            }
                        }

                        //}
                        break;
                    case Connect_Mode.TCPIP:

                        bool ret = TCPConnect();
                        if (ret == false)
                        {
                            LogMessage("Init ModBus Fail", MessageLevel.Error);
                            return false;
                        }

                        break;
                }

            }
            catch (Exception ex)
            {
                LogMessage($"Modbus Error=>{ ex.Message} Init Fail", MessageLevel.Error);
                return false;
            }

            LogMessage("Init ModBus Success", MessageLevel.Info);

            return true;
        }

        public override void OPEN()
        {
            throw new NotImplementedException();
        }

        public override bool Status(ref string msg)
        {

            try
            {
                if (modbusMaster.ModBus_CHK())
                {
                    msg = $"{modbusMaster.ModBus_ComportInfo()}(OPEN)";
                    return true;
                }
                else
                {
                    msg = $"{modbusMaster.ModBus_ComportInfo()}(CLOSE)";
                    return false;
                }
            }
            catch (Exception ex)
            {
                msg = $"{ex.Message}";
                return false;
            }

        }


        public override bool UnInit()
        {
            try
            {
                if (modbusMaster != null)
                {
                    modbusMaster.Close();
                    modbusMaster = null;
                }
            }
            catch (Exception ex)
            {
                LogMessage($"Modbus UnInit Error=>{ ex.Message}", MessageLevel.Error);
                return false;
            }
            return true;
        }

        public bool ModBusSEND(string Address, string writeData, TransmitMode Mode)
        {
            bool Pass_Fail = false;
            try
            {
                if (modbusMaster != null)
                {
                    Pass_Fail = modbusMaster.Write(SlaveID, Address, writeData, Mode);
                }

            }
            catch (Exception Send_ex)
            {
                LogMessage($"Modbus Send Error=>{ Send_ex.Message}", MessageLevel.Error);
                return false;
            }

            return Pass_Fail;
        }

        public bool ModBusREAD(string Address, ushort numRegisters, ref ushort[] DataRecAll, TransmitMode Mode)
        {
            bool Pass_Fail = false;
            try
            {
                if (modbusMaster != null)
                {
                    Pass_Fail = modbusMaster.Read(SlaveID, Address, numRegisters, ref DataRecAll, Mode);
                }
            }
            catch (TimeoutException ex_Time)
            {
                LogMessage($"TimeoutException:{ex_Time.Message}", MessageLevel.Error);
                return false;
            }
            catch (Exception rend_ex)
            {
                LogMessage($"Modbus Rend Error=>{ rend_ex.Message}", MessageLevel.Error);
                return false;
            }

            return Pass_Fail;
        }

        public override bool SEND(string command)
        {
            LogMessage($"This Function Modbus  is Not Use", MessageLevel.Error);
            return false;
        }

        public override bool READ(ref string output)
        {
            LogMessage($"This Function Modbus  is Not Use", MessageLevel.Error);
            return false;
        }

        public override void Dispose()
        {
            throw new NotImplementedException();
        }

        public override void ClearBuffer()
        {
            throw new NotImplementedException();
        }

        public override void SetTimeout(int time)
        {

        }

        public override void SetModBusTimeout(int writetime, int readtime)
        {
            if (modbusMaster != null)
                modbusMaster.SetTimeout(writetime, readtime);
        }

        public override void RetryTimes(int time)
        {
            if (modbusMaster != null)
                modbusMaster.RetryTimes(time);
        }

        public bool TCPConnect()
        {
            try
            {
                if (modbusMaster == null)
                {
                    modbusMaster = new NewModBus();
                    modbusMaster.ModbusConnectMode(Connect_Mode.TCPIP);
                    modbusMaster.ModbusTcpConnectInfo(IP, Port);
                    if (!modbusMaster.Open())
                    {
                        LogMessage("ModBus TCP Connect Fail", MessageLevel.Error);
                        return false;
                    }

                    LogMessage("ModBus TCP Connected Successed ", MessageLevel.Info);
                }
                return true;
            }
            catch (Exception ex)
            {
                LogMessage($"ModBus TCP Connect Error {ex.Message}", MessageLevel.Error);
                return false;
            }
        }

    }
}
