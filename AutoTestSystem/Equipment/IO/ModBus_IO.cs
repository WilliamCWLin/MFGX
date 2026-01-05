using Automation.BDaq;
using AutoTestSystem.Base;
using AutoTestSystem.DAL;
using AutoTestSystem.DevicesUI.IO;
using AutoTestSystem.Model;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static AutoTestSystem.BLL.Bd;
using static AutoTestSystem.DAL.Communication;

namespace AutoTestSystem.Equipment.IO
{
    class ModBus_IO : IOBase
    {

        [Category("Comport Parameters"), Description("SlaveID")]
        public string SlaveID { get; set; } = "1";

        [Category("Comport Parameters"), Description("Select Comport"), TypeConverter(typeof(ModBusComportList))]
        public string PortName { get; set; }

        [Category("Comport Parameters"), Description("BaudRate")]
        public int BaudRate { get; set; } = 115200;

        NewModBus modbusMaster = null;
        

        public override void Dispose()
        {
            throw new NotImplementedException();
        }



        public override bool Init(string strParamInfo)
        {
            try
            {
                if (string.IsNullOrEmpty(PortName))
                {
                    LogMessage("NO COM Port Name", MessageLevel.Error);
                    return false;
                }
                // 設定串口參數
                ModBusSerialConnetInfo ModBusDevieCOMinfo = new ModBusSerialConnetInfo { PortName = PortName, BaudRate = BaudRate };

                if (GlobalNew.comhandler.ContainsKey(PortName))
                {
                    modbusMaster = GlobalNew.comhandler[PortName];
                }
                else
                {
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
                        else
                        {
                            GlobalNew.comhandler.Add(PortName, modbusMaster);
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
                            else
                            {
                                GlobalNew.comhandler.Add(PortName, modbusMaster);
                            }
                        }
                    }
                }
                return true;
            }
            catch(Exception e)
            {
                LogMessage("Init Fail. " + e.Message, MessageLevel.Error);
                MessageBox.Show("Please check if the device is in use or if the driver is not installed and the device is disconnected.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
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
       

        public override bool InstantAI(string strParamIn,ref string Dataout)
        {
            throw new NotImplementedException();
        }

        public override bool GETALLIO(ref bool [] status)
        {
            try
            {
                if (modbusMaster != null)
                {
                    bool [] DataRecAll = null;
                    byte byteSlaveID = Convert.ToByte(SlaveID);
                    modbusMaster.SetTimeout(1000,1000);
                    bool pass_fail_flag = modbusMaster.Read(byteSlaveID, "0x00", (ushort)ChannelCount, ref DataRecAll, TransmitMode.Input_Status);

                    if (pass_fail_flag && DataRecAll != null)
                    {
                        status = DataRecAll;
                    }
                    else
                    {
                        LogMessage($"ModBus GetIO Fail.", MessageLevel.Error);
                        return false;
                    }
                }
                else
                {
                    LogMessage("ModBus modbusMaster is null.", MessageLevel.Error);
                    return false;
                }

            }
            catch (Exception ex)
            {
                LogMessage("ModBus GETIO Exception." + ex.Message, MessageLevel.Error);
                return false;
            }
            return true;
        }
        public override bool GETIO(int port, int bit, ref bool status)
        {
            try
            {
                if (modbusMaster != null)
                {
                    //ushort numRegisters = (ushort)pos;
                    ushort[] DataRecAll = null;
                    byte byteSlaveID = Convert.ToByte(SlaveID);
                    modbusMaster.SetTimeout(1000, 1000);
                    bool pass_fail_flag = modbusMaster.Read(byteSlaveID, "0x00", (ushort)ChannelCount, ref DataRecAll, TransmitMode.Input_Status);

                    if (pass_fail_flag && DataRecAll != null && ChannelCount >= bit)
                    {

                        if (DataRecAll[bit] == 1)
                        {
                            status = true;
                        }
                        else
                        {
                            status = false;
                        }
                    }
                    else
                    {
                        LogMessage($"ModBus GetIO Fail.", MessageLevel.Error);
                        return false;
                    }
                }
                else
                {
                    LogMessage("ModBus modbusMaster is null.", MessageLevel.Error);
                    return false;
                }

            }
            catch (Exception ex)
            {
                LogMessage("ModBus GETIO Exception." + ex.Message, MessageLevel.Error);
                return false;
            }
            return true;
        }
        public override bool GETIO(int portNum,ref bool status)
        {
            try
            {
                if (modbusMaster != null)
                {
                    //ushort numRegisters = (ushort)pos;
                    ushort[] DataRecAll = null;
                    byte byteSlaveID = Convert.ToByte(SlaveID);
                    modbusMaster.SetTimeout(1000, 1000);
                    bool pass_fail_flag = modbusMaster.Read(byteSlaveID,"0x00", (ushort)ChannelCount, ref DataRecAll,TransmitMode.Input_Status);

                    if (pass_fail_flag && DataRecAll != null && ChannelCount >=  portNum)
                    {

                        if (DataRecAll[portNum] == 1)
                        {
                            status = true;
                        }
                        else
                        {
                            status = false;
                        }
                    }
                    else
                    {
                        LogMessage($"ModBus GetIO Fail.", MessageLevel.Error);
                        return false;
                    }
                }
                else
                {
                    LogMessage("ModBus modbusMaster is null.", MessageLevel.Error);
                    return false;
                }

            }catch(Exception ex)
            {
                LogMessage("ModBus GETIO Exception." + ex.Message, MessageLevel.Error);
                return false;
            }
            return true;
        }
        public override bool SETIO(int bit, bool output)
        {
            try
            {
                if (modbusMaster != null)
                {
                    string ON_OFF = string.Empty;
                    if (output)
                        ON_OFF = "True";
                    else
                        ON_OFF = "False";
                    byte byteSlaveID = Convert.ToByte(SlaveID);
                    modbusMaster.SetTimeout(1000, 1000);
                    bool Ret = modbusMaster.Write(byteSlaveID, bit.ToString() , ON_OFF, TransmitMode.SingleCoil_Status);
                    if(!Ret)
                    {
                        LogMessage("ModBus SetIO Fail.", MessageLevel.Error);
                        return false;
                    }
                }
                else
                {
                    LogMessage("ModBus modbusMaster is null.", MessageLevel.Error);
                    return false;
                }               
            }
            catch (Exception ex)
            {
                LogMessage("ModBus SETIO Exception." + ex.Message, MessageLevel.Error);
                return false;
            }
            return true;
        }

        public override bool GETALLDO(ref bool [] status)
        {
            try
            {
                if (modbusMaster != null)
                {
                    string ON_OFF = string.Empty;
                    bool [] IO_Status = null;

                    byte byteSlaveID = Convert.ToByte(SlaveID);
                    modbusMaster.SetTimeout(1000, 1000);
                    bool Ret = modbusMaster.Read(byteSlaveID,"0x00", (ushort)ChannelCount, ref IO_Status, TransmitMode.SingleCoil_Status);
                    if (!Ret)
                    {
                        LogMessage("ModBus GETDO Fail.", MessageLevel.Error);
                        return false;
                    }

                    status = IO_Status;

                }
                else
                {
                    LogMessage("ModBus modbusMaster is null.", MessageLevel.Error);
                    return false;
                }
            }
            catch (Exception ex)
            {
                LogMessage("ModBus GETDO Exception." + ex.Message, MessageLevel.Error);
                return false;
            }
            return true;
        }

        public override bool GETDO(int portNum, int pos, ref bool status)
        {
            try
            {
                if (modbusMaster != null)
                {
                    string ON_OFF = string.Empty;
                    ushort[] IO_Status = null;

                    byte byteSlaveID = Convert.ToByte(SlaveID);
                    modbusMaster.SetTimeout(1000, 1000);
                    bool Ret = modbusMaster.Read(byteSlaveID, portNum.ToString(), 1, ref IO_Status, TransmitMode.SingleCoil_Status);
                    if (!Ret)
                    {
                        LogMessage("ModBus GETDO Fail.", MessageLevel.Error);
                        return false;
                    }

                    status = IO_Status.Any(x => x != 0);

                }
                else
                {
                    LogMessage("ModBus modbusMaster is null.", MessageLevel.Error);
                    return false;
                }
            }
            catch (Exception ex)
            {
                LogMessage("ModBus GETDO Exception." + ex.Message, MessageLevel.Error);
                return false;
            }
            return true;
        }

        public override bool UnInit()
        {
            try
            {
                if (modbusMaster != null)
                {
                    modbusMaster.Close();
                    GlobalNew.comhandler.Remove(PortName);
                    modbusMaster = null;
                }
            }
            catch (Exception ex)
            {
                LogMessage($"Modbus IO UnInit Error=>{ ex.Message}", MessageLevel.Error);
                return false;
            }
            return true;
        }

        public override bool Show()
        {
            //UnInit();

            //bool ret = Init("");

            //if (ret)
            //{
                /*using (var form = new IO_Controller())
                {
                    form.StartPosition = FormStartPosition.CenterScreen;
                    return form.ShowDialog() == DialogResult.OK;
                }*/
            //}
            return true;
        }
        public string ReadDataParse(ushort[] DataRecAll, ReadFormat Format)
        {
            int decimalNumber = 0;
            string hexNumber = string.Empty;
            string binaryNumber = string.Empty;
            string ParseResult = string.Empty;
            if (DataRecAll == null)
                return null;
            foreach (var register in DataRecAll)
            {
                decimalNumber = int.Parse($"{register}");
                hexNumber = decimalNumber.ToString("X2");
                binaryNumber = Convert.ToString(decimalNumber, 2).PadLeft(8, '0');

                switch (Format)
                {
                    case ReadFormat.HEX:
                        ParseResult += $"0x{hexNumber};";
                        break;
                    case ReadFormat.DEC:
                        ParseResult += $"{register};";
                        break;
                    case ReadFormat.BIN:
                        ParseResult += $"{binaryNumber};";
                        break;
                }
            }

            return ParseResult.TrimEnd(';');
        }
        public enum ReadFormat
        {
            HEX,
            DEC,
            BIN
        }
        


    }
}
