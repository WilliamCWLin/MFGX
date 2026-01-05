using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Design;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AutoTestSystem.Base;
using AutoTestSystem.DAL;
using AutoTestSystem.Model;
using Newtonsoft.Json;
using NModbus;
using NModbus.Serial;
using static AutoTestSystem.BLL.Bd;
using static AutoTestSystem.DAL.Communication;
using static AutoTestSystem.Equipment.Motion.Motion_DIOparam;

namespace AutoTestSystem.Equipment.Motion
{
    public class LeadShine : MotionBase
    {
        public bool HomeFlag = false;
        public bool Status_TimeOut = false;

        [Category("Choose Motor Type"), Description("Choose Motor Type")]
        public MOTION_MOTOR Motor { get; set; } = MOTION_MOTOR.Stepper_Motor;

        [Category("Motion Parameters"), Description("SlaveID")]
        public string SlaveID { get; set; } = "1";
        [Category("Motion Parameters"), Description("移動每單位距離(mm/Plus)")]
        public double Resolution { get; set; } = 0.01;
        [Category("Motion Check Status"), Description("Check Motion Status")]
        public Check_Status CheckStatus { get; set; } = Check_Status.Disable;
        [Category("Motion Emergency Stop Check"), Description("Emergency Stop Setting(Channnel)")]
        public int EMG_Channel { get; set; } = 0;
        [Category("Motion Emergency Stop Check"), Description("Emergency Stop Setting(Status)")]
        public bool EMG_Status { get; set; } = false;
        [Category("MotionLimit Check"), Description("Limit Check Setting(Channnel)")]
        public int PositiveLimit_Channel { get; set; } = 1;
        [Category("MotionLimit Check"), Description("Limit Check Setting(Status)")]
        public bool PositiveLimit_Status { get; set; } = false;
        [Category("MotionLimit Check"), Description("Limit Check Setting(Channnel)")]
        public int NegativeLimit_Channel { get; set; } = 2;
        [Category("MotionLimit Check"), Description("Limit Check Setting(Status)")]
        public bool NegativeLimit_Status { get; set; } = false;

        [Browsable(false)]
        [Category("Move Position for Limit"), Description("軟體正負極限位功能(CheckStatus have to Set Motion)")]
        public bool LimitEnable { get; set; } = false;

        /*[Category("Motion Tuning Parameters Setting"), Description("Motion Control 參數設定"), Editor(typeof(MotionControler), typeof(System.Drawing.Design.UITypeEditor))]
        public string MotionControl_Param { get; set; }*/
        [Browsable(false)]
        [Category("Move Position for Limit"), Description("軟體正極限位(mm)")]
        public double PositiveLimit { get; set; }
        [Browsable(false)]
        [Category("Move Position for Limit"), Description("軟體負極限位(mm)")]
        public double NegativeLimit { get; set; }

        [Category("Reset Home Parameters Setting"), Description("回零模式")]
        public ResetMode Mode { get; set; } = ResetMode.Origin_Reset;

        [Category("Reset Home Parameters Setting"), Description("Z訊號啟動")]
        public Z_Signal ZSignal{ get; set; } = Z_Signal.OFF;
        /*[Category("Reset Home Parameters Setting"), Description("設定原點為0")]
        public bool SetZero { get; set; } = false;*/
        //[ReadOnly(true)]
        [Category("Reset Home Parameters Setting"), Description("回零方向")]
        public ResetDirection Dir { get; set; } = ResetDirection.Positive;
        /*[Category("Reset Home Parameters Setting"), Description("回零狀態檢查")]
        public bool Status_Check { get; set; } = true;*/
        //[ReadOnly(true)]
        [Category("Reset Home Parameters Setting for Position"), Description("定義原點訊號在座標軸上的位置(mm) ex:設定 60mm 會將原點訊號設定為60mm 並回持續走回零點位置")]
        public double HomePosition { get; set; } = 0;
        //[ReadOnly(true)]
        [Category("Reset Home Parameters Setting for Position"), Description("回零後機電移動到指定位置(mm)")]
        public double StopPosition { get; set; } = 0;
        //[ReadOnly(true)]
        [Category("Reset Home Parameters Setting for Speed"), Description("回零高速(回零第一階段)")]
        public int Stop_Vel_HS { get; set; } = 200;
        //[ReadOnly(true)]
        [Category("Reset Home Parameters Setting for Speed"), Description("回零低速(回零第二階段)")]
        public int Stop_Vel_LS { get; set; } = 100;
        //[ReadOnly(true)]
        [Category("Reset Home Parameters Setting for Acceleration and deceleration"), Description("加速度設定")]
        public int Tacc { get; set; } = 100;
        //[ReadOnly(true)]
        [Category("Reset Home Parameters Setting for Acceleration and deceleration"), Description("減速度設定")]
        public int Dac { get; set; } = 100;

        /* [Category("IOTrigger Parameters Setting"), Description("DIO 參數設定"), Editor(typeof(DIOEditor), typeof(System.Drawing.Design.UITypeEditor))]
         public string IOTrigger_Param { get; set; }*/
        [JsonIgnore]
        [Browsable(false)]
        [ReadOnly(true)]
        [Category("DITrigger_DI Parameters Setting"), Description("DI1 輸入口1")]
        public string IOTrigger_DI1 { get; set; } = "0x88";
        [JsonIgnore]
        [Browsable(false)]
        [ReadOnly(true)]
        [Category("DITrigger_DI Parameters Setting"), Description("DI2 輸入口2")]
        public string IOTrigger_DI2 { get; set; } = "0x22";
        [JsonIgnore]
        [Browsable(false)]
        [ReadOnly(true)]
        [Category("DITrigger_DI Parameters Setting"), Description("DI3 輸入口3")]
        public string IOTrigger_DI3 { get; set; } = "0x25";
        [JsonIgnore]
        [Browsable(false)]
        [ReadOnly(true)]
        [Category("DITrigger_DI Parameters Setting"), Description("DI4 輸入口4")]
        public string IOTrigger_DI4 { get; set; } = "0x26";
        [JsonIgnore]
        [Browsable(false)]
        [ReadOnly(true)]
        [Category("DITrigger_DI Parameters Setting"), Description("DI5 輸入口5")]
        public string IOTrigger_DI5 { get; set; } = "0x27";
        [JsonIgnore]
        [Browsable(false)]
        [ReadOnly(true)]
        [Category("DITrigger_DI Parameters Setting"), Description("DI6 輸入口6")]
        public string IOTrigger_DI6 { get; set; } = "0x00";
        [JsonIgnore]
        [Browsable(false)]
        [ReadOnly(true)]
        [Category("DITrigger_DI Parameters Setting"), Description("DI7 輸入口7")]
        public string IOTrigger_DI7 { get; set; } = "0x00";

        [JsonIgnore]
        [Browsable(false)]
        [ReadOnly(true)]
        [Category("DITrigger_DO Parameters Setting"), Description("DO1 輸出口1")]
        public string IOTrigger_DO1 { get; set; } = "0x24";
        [JsonIgnore]
        [Browsable(false)]
        [ReadOnly(true)]
        [Category("DITrigger_DO Parameters Setting"), Description("DO2 輸出口2")]
        public string IOTrigger_DO2 { get; set; } = "0x00";
        [JsonIgnore]
        [Browsable(false)]
        [ReadOnly(true)]
        [Category("DITrigger_DO Parameters Setting"), Description("DO3 輸出口3")]
        public string IOTrigger_DO3 { get; set; } = "0x00";


        [Category("Comport Parameters"), Description("Select Comport"), TypeConverter(typeof(ModBusComportList))]
        public string PortName { get; set; }

        [Category("Comport Parameters"), Description("baudRate")]
        public int baudRate { get; set; } = 115200;

        NewModBus modbusMaster = null;
        private static readonly Dictionary<string, object> _lockMotionObjects = new Dictionary<string, object>();

        private static object GetMotionLockObject(string key)
        {
            if (!_lockMotionObjects.ContainsKey(key))
            {
                _lockMotionObjects[key] = new object();
            }
            return _lockMotionObjects[key];
        }

        public LeadShine()
        {
            baudRate = 115200;
        }

        public enum Check_Status
        {
            Disable,
            Motion
        }
   
        public override bool Init(string strParamInfo)
        {

            if (string.IsNullOrEmpty(PortName))
            {
                LogMessage("NO COM Port Name", MessageLevel.Error);
                return false;
            }

            try
            {
                // 設定串口參數
                ModBusSerialConnetInfo ModBusDevieCOMinfo = new ModBusSerialConnetInfo { PortName = PortName, BaudRate = baudRate };

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
                            LogMessage("Init LeadShine Motion Fail", MessageLevel.Error);
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
                            modbusMaster = new NewModBus();
                            modbusMaster.ModbusConnectMode(Connect_Mode.SerialPort);
                            modbusMaster.ModbusSerialPortConnectInfo(ModBusDevieCOMinfo);
                            if (!modbusMaster.Open())
                            {
                                LogMessage("Init LeadShine Motion Fail", MessageLevel.Error);
                                return false;
                            }
                            else
                            {
                                GlobalNew.comhandler.Add(PortName, modbusMaster);
                            }
                        }
                    }

                }

                /*if (LimitEnable)
                {
                    bool Position_Limit = false;
                    Position_Limit = PostionLimitSet(PositiveLimit, NegativeLimit);
                    if (Position_Limit == false)
                    {
                        LogMessage($"Position_Limit Set  Fail", MessageLevel.Error);
                        return false;
                    }
                }*/

                if (CheckStatus == Check_Status.Motion)
                {
                    int status_init = 0;
                    bool Driver_init = false;

                    Driver_init = Err_Status();
                    switch (Motor)
                    {
                        case MOTION_MOTOR.Stepper_Motor:
                            Driver_init &= DriverStatus(1, ref status_init);
                            break;
                        case MOTION_MOTOR.Servo_Motor:
                            Driver_init &= DriverStatus(0, ref status_init);
                            break;
                    }
                    if (Driver_init == false)
                    {
                        LogMessage("DriverStatus_init Fail", MessageLevel.Warn);
                        return false;
                    }
                    if (status_init != 1)
                    {
                        LogMessage("Driver Status is not Ready", MessageLevel.Warn);
                        return false;
                    }

                }              


                /* else
                 {
                     if(!comhandler.ContainsKey(PortName))
                         comhandler.Add(PortName, modbusMaster);
                 }*/



            }
            catch (Exception ex)
            {
                LogMessage($"LeadShine Error=>{ ex.Message} Init Fail", MessageLevel.Error);
                return false;
            }

            LogMessage("Init LeadShine Motion Success", MessageLevel.Info);

            return true;
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
                    GlobalNew.comhandler.Remove(PortName);
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

        public override bool Broadcast(string writeData)
        {
            bool Pass_Fail = false;
            try
            {
                if (modbusMaster != null)
                {
                    modbusMaster.SetTimeout(3000, 0);
                    modbusMaster.RetryTimes(0);
                    Pass_Fail = modbusMaster.Write(0, "0x6002", writeData, TransmitMode.Holding_registers_Single);
                }
                else
                    LogMessage($"Modbus Master is null", MessageLevel.Error);

            }
            catch (Exception Send_ex)
            {
                LogMessage($"Modbus Send Error=>{ Send_ex.Message}", MessageLevel.Error);
                return false;
            }

            return Pass_Fail;
        }

        public void ChangeSlaveID(string id)
        {
            SlaveID = id;
        }

        public bool SEND(string Address, string writeData, TransmitMode mode)
        {
            bool Pass_Fail = false;

            try
            {
                if (modbusMaster != null)
                {
                    if (!string.IsNullOrEmpty(PortName))
                    {
                        lock (GetMotionLockObject(PortName))
                        {
                            modbusMaster.SetTimeout(3000,3000);
                            modbusMaster.RetryTimes(1);
                            byte byteSlaveID = Convert.ToByte(SlaveID);
                            Pass_Fail = modbusMaster.Write(byteSlaveID, Address, writeData, mode);
                        }
                    }
                }
                else
                    LogMessage($"Modbus Master is null", MessageLevel.Error);

            }
            catch (Exception Send_ex)
            {
                LogMessage($"Modbus Send Error=>{ Send_ex.Message}", MessageLevel.Error);
                return false;
            }

            return Pass_Fail;
        }


        public bool READ(string Address, ushort numRegisters, ref ushort[] DataRecAll, TransmitMode mode)
        {
            bool Pass_Fail = false;
            try
            {
                if (modbusMaster != null)
                {
                    if (!string.IsNullOrEmpty(PortName))
                    {
                        lock (GetMotionLockObject(PortName))
                        {                           
                            modbusMaster.SetTimeout(3000,3000);
                            modbusMaster.RetryTimes(1);
                            byte byteSlaveID = Convert.ToByte(SlaveID);
                            Pass_Fail = modbusMaster.Read(byteSlaveID, Address, numRegisters, ref DataRecAll, mode);
                        }
                    }
                }
                else
                    LogMessage($"Modbus Master is null", MessageLevel.Error);
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


        public bool AsyncSEND(string Address, string writeData, TransmitMode mode)
        {
            bool Pass_Fail = false;
            var tasks = new List<Task>();
            try
            {
                if (modbusMaster != null)
                {

                    if (!string.IsNullOrEmpty(PortName))
                    {
                        lock (GetMotionLockObject(PortName))
                        {
                            modbusMaster.SetTimeout(3000, 3000);
                            modbusMaster.RetryTimes(1);
                            byte byteSlaveID = Convert.ToByte(SlaveID);
                            var t = Task.Run(async () =>
                            {
                                await modbusMaster.WriteAsync(byteSlaveID, Address, writeData, mode);
                            });
                            tasks.Add(t);
                            Task.WaitAll(tasks.ToArray());
                        }
                    }
                }
                else
                    LogMessage($"Modbus Master is null", MessageLevel.Error);

            }
            catch (Exception Send_ex)
            {
                LogMessage($"Modbus Send Error=>{ Send_ex.Message}", MessageLevel.Error);
                return false;
            }

            return Pass_Fail;
        }


        public bool AsyncREAD(string Address, ushort numRegisters, ref ushort[] DataRecAll, TransmitMode mode)
        {
            bool Pass_Fail = false;
            var tasks = new List<Task>();
            ushort[] AsyncdataRecAll = null;
            try
            {
                if (modbusMaster != null)
                {
                    if (!string.IsNullOrEmpty(PortName))
                    {
                        lock (GetMotionLockObject(PortName))
                        {
                            modbusMaster.SetTimeout(3000, 3000);
                            modbusMaster.RetryTimes(1);
                            byte byteSlaveID = Convert.ToByte(SlaveID);


                            var t = Task.Run(async () =>
                            {
                                var result = await modbusMaster.ReadAsync(byteSlaveID, Address, numRegisters, mode);
                                return result;
                            });

                            tasks.Add(t);

                            Task.WaitAll(tasks.ToArray());
                            if (t.IsCompleted)
                            {
                                AsyncdataRecAll = t.Result;
                                Pass_Fail = true;
                            }

                            if (Pass_Fail && AsyncdataRecAll != null)
                            {
                                DataRecAll = AsyncdataRecAll;
                            }
                            else
                            {
                                LogMessage($"Pass_Fail is Fail or AsyncdataRecAll is null", MessageLevel.Error);
                                return false;
                            }
                        }
                    }
                }
                else
                    LogMessage($"Modbus Master is null", MessageLevel.Error);
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

        public override bool Get_IO_Status(ref int status, ushort port_num)
        {
            //short nRet = 0;
            bool pass_fail_flag = false;
            ushort[] IOData = null;

            try
            {
                if (modbusMaster != null)
                {
                    modbusMaster.SetTimeout(50, 50);
                    modbusMaster.RetryTimes(1);
                    pass_fail_flag = READ("0x00", 16, ref IOData, TransmitMode.Input_Status);

                    if (pass_fail_flag && IOData != null && port_num < IOData.Length)
                    {

                        if (IOData[port_num] == 1)
                        {
                            status = 1;
                        }
                        else
                        {
                            status = 0;
                        }
                    }
                    else
                    {
                        LogMessage($"Modbus Get_IO_Status Fail", MessageLevel.Error);
                        return false;
                    }
                }
                else
                    LogMessage($"Modbus Master is null", MessageLevel.Error);
            }
            catch (Exception ex)
            {
                LogMessage($"Modbus Get_IO_Status Error {ex.Message}", MessageLevel.Error);
                return false;

            }

            return pass_fail_flag;
        }

        public override bool Get_IO_Status(ref ushort[] IORecAll)
        {
            bool pass_fail_flag = false;
            ushort[] IOData = null;
            try
            {
                if (modbusMaster != null)
                {
                    modbusMaster.SetTimeout(50, 50);
                    modbusMaster.RetryTimes(1);
                    pass_fail_flag = READ("0x00", 16, ref IOData, TransmitMode.Input_Status);

                    IORecAll = IOData;
                }
            }
            catch (Exception ex)
            {
                LogMessage($"Modbus Get_IO_Status Error {ex.Message}", MessageLevel.Error);
                return false;

            }
            return pass_fail_flag;
        }

        public override bool SetGet_IO(string port_num, string status, ref ushort[] IORecAll)
        {
            bool pass_fail_flag = false;
            ushort[] IOData = null;
            try
            {
                if (modbusMaster != null)
                {
                    pass_fail_flag = SEND(port_num, status, TransmitMode.ModbusIO16Bit);

                    pass_fail_flag &= READ("0x00", 16, ref IOData, TransmitMode.ModbusIO16Bit);

                    if (pass_fail_flag && IOData != null)
                    {
                        IORecAll = IOData;
                    }
                    else
                    {
                        LogMessage($"Modbus Get_IO_Status Fail", MessageLevel.Error);
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                LogMessage($"Modbus Set_IO_Status Error {ex.Message}", MessageLevel.Error);
                return false;

            }

            return pass_fail_flag;

        }

        public override bool SetGet_IO(ushort port_num, int status, ref int output_status)
        {
            bool pass_fail_flag = false;
            ushort[] IOData = null;
            try
            {
                if (modbusMaster != null)
                {
                    modbusMaster.SetTimeout(50, 50);
                    modbusMaster.RetryTimes(1);
                    bool BOOL_status = ConvertToBoolean(status);
                    if (BOOL_status == true)
                        pass_fail_flag = SEND(port_num.ToString("X2"), "True", TransmitMode.SingleCoil_Status);
                    else
                        pass_fail_flag = SEND(port_num.ToString("X2"), "False", TransmitMode.SingleCoil_Status);

                    pass_fail_flag = READ("0x00", 16, ref IOData, TransmitMode.ModbusIO16Bit);

                    if (pass_fail_flag && IOData != null && port_num < IOData.Length)
                    {

                        if (IOData[port_num] == 1)
                        {
                            output_status = 1;
                        }
                        else
                        {
                            output_status = 0;
                        }
                    }
                    else
                    {
                        LogMessage($"Modbus Get_IO_Status Fail", MessageLevel.Error);
                        return false;
                    }

                }
                else
                    LogMessage($"Modbus Master is null", MessageLevel.Error);
            }
            catch (Exception ex)
            {
                LogMessage($"Modbus Set_IO_Status Error {ex.Message}", MessageLevel.Error);
                return false;

            }

            return pass_fail_flag;
        }

        static bool ConvertToBoolean(int number)
        {
            return number == 1;
        }

        public override bool Pause()
        {
            throw new NotImplementedException();
        }

        public bool Err_Status()
        {
            if (CheckStatus == Check_Status.Motion)
            {
                //LogMessage("=======Err_Status =======Start", MessageLevel.Debug);
                bool Err_info = false;
                Err_info = CheckDriverIO();//確認是否急停狀態
                Err_info &= CheckErrInfo(); //確認是否警報訊息
                if (!Err_info)
                    return false;
                //LogMessage("=======Err_Status =======END", MessageLevel.Debug);
            }            
            return true;
        }
        public override void GetMotionStatus(ref int ConvertStatus)
        {
            int Path_Status = 0;
            int status = 0;
            string RecDataAll = string.Empty;

            if(MotionDone(ref Path_Status)) 
            { 
                    if (Path_Status == 0)
                    {
                        ConvertStatus = 0;
                    }
                    else
                        ConvertStatus = 1;                   
            }
            else
            {
                EmgStop();
                ConvertStatus = -99;
                LogMessage($"Motion Error:{status}", MessageLevel.Error);
            }
        }

        public override bool MotionDone(ref int status)
        {
            string statusdata = string.Empty;
            ushort[] Data = null;
            bool Status_Read = false;
            try
            {
                if (!Err_Status())
                {
                    return false;
                }

                Status_Read = READ("0x6002", 1, ref Data, TransmitMode.Holding_registers_Single);//運行狀態確認
                if (Status_Read == false || Data == null)
                {
                    LogMessage($"0x6002 Motion Status Read Fail", MessageLevel.Error);
                    return false;
                }

                statusdata = ReadDataParse(Data, ReadFormat.HEX);
                if (statusdata == null)
                {
                    LogMessage($"statusdata is null", MessageLevel.Error);
                    return false;
                }

                if (statusdata == "0x00")
                {
                    if (!Err_Status())//CHECK ERROR MSG AGAIN
                    {
                        return false;
                    }else
                        status = 0;
                }
                else
                    status = 1;
                                                                       
            }
            catch (Exception ex)
            {
                LogMessage($"MotionSatus Error: {ex.Message}", MessageLevel.Error);
                return false;
            }

            return true;
        }

        public override bool HomeDone(ref int status)
        {
            string statusdata = string.Empty;
            ushort[] Data = null;
            bool Status_Read = false;
            try
            {
                if (!CheckHome_Err())
                {
                    return false;
                }

                Status_Read = READ("0x6002", 1, ref Data, TransmitMode.Holding_registers_Single);//運行狀態確認
                if (Status_Read == false || Data == null)
                {
                    LogMessage($"0x6002 Motion Status Read Fail", MessageLevel.Error);
                    return false;
                }

                statusdata = ReadDataParse(Data, ReadFormat.HEX);
                if (statusdata == null)
                {
                    LogMessage($"statusdata is null", MessageLevel.Error);
                    return false;
                }

                if (statusdata == "0x00")
                {
                    if (!CheckHome_Err())
                    {
                        return false;
                    }else
                        status = 0;
                }
                else
                    status = 1;

            }
            catch (Exception ex)
            {
                LogMessage($"MotionSatus Error: {ex.Message}", MessageLevel.Error);
                return false;
            }

            return true;
        }

        public bool DriverStatus(int Bitposition, ref int status)
        {
            string statusdata = string.Empty;
            ushort[] Data = null;
            bool Status_Read = false;
            try
            {
                switch (Motor)
                {
                    case MOTION_MOTOR.Stepper_Motor:
                        Status_Read = READ("0x1003", 1, ref Data, TransmitMode.Holding_registers_Single);//運行狀態確認
                        break;
                    case MOTION_MOTOR.Servo_Motor:
                        Status_Read = READ("0x0B05", 1, ref Data, TransmitMode.Holding_registers_Single);//運行狀態確認
                        break;
                    default:
                        LogMessage($"Undefind Motor Type", MessageLevel.Error);
                        return false;
                }
               
                if (Status_Read == false || Data == null)
                {
                    LogMessage($"Motion Status Read Fail", MessageLevel.Error);
                    return false;
                }
                statusdata = ReadDataParse(Data, ReadFormat.BIN);
                if (statusdata == null)
                {
                    LogMessage($"statusdata is null", MessageLevel.Error);
                    return false;
                }

                int datalength = statusdata.Length - 1;
                if (datalength >= Bitposition)
                {
                    char bitChar = statusdata[datalength - Bitposition];
                    int bit = bitChar - '0';
                    status = bit;
                    //LogMessage($"第 {Bitposition} 位的位元是: {status}");
                }
                else
                {
                    LogMessage($"二進位數字長度不足", MessageLevel.Error);
                    return false;
                }
            }
            catch (Exception ex)
            {
                LogMessage($"MotionSatus Error: {ex.Message}", MessageLevel.Error);
                return false;
            }

            return true;
        }

        public bool CheckHome_Err()
        {
            bool DriverIO = false;
           if(Mode == ResetMode.Limit_Reset)
           {
                bool Err_info = CheckErrInfo(); //確認是否警報訊息
                if(Err_info)
                {
                    DriverIO = EMGIO();
                }
            }
            else
            {
                DriverIO = Err_Status();
            }
           return DriverIO;
        }
        public bool EMGIO()
        {
            string IOstatusdata = string.Empty;
            ushort[] IO_Data = null;
            bool IO_Status = false;
            try
            {
                switch (Motor)
                {
                    case MOTION_MOTOR.Stepper_Motor:
                        IO_Status = READ("0x0179", 1, ref IO_Data, TransmitMode.Holding_registers_Single);
                        if (IO_Status == false)
                        {
                            LogMessage($"Read IO_Status (0x0179) Fail", MessageLevel.Error);
                            return false;
                        }
                        break;
                    case MOTION_MOTOR.Servo_Motor:
                        IO_Status = READ("0x602E", 1, ref IO_Data, TransmitMode.Holding_registers_Single);
                        if (IO_Status == false)
                        {
                            LogMessage($"Read IO_Status (0x602E) Fail", MessageLevel.Error);
                            return false;
                        }
                        break;
                    default:
                        LogMessage($"Undefind Motor Type", MessageLevel.Error);
                        return false;
                }
                IOstatusdata = ReadDataParse(IO_Data, ReadFormat.BIN);

                if (!string.IsNullOrEmpty(IOstatusdata))
                {
                    int IO_Statusdata = Convert.ToInt32(IOstatusdata, 2);
                    int EMGstatus_result = EMG_Status ? 1 : 0;                   
                    bool EMG = ((IO_Statusdata >> EMG_Channel) & 1) == EMGstatus_result;            
                    //LogMessage($"EMG IOstatusdata:{IOstatusdata} , Check EMG_data :{EMG}", MessageLevel.Info);
                    if (EMG)
                    {
                        //EmgStop();
                        LogMessage($"The Emergency stop button is pressed", MessageLevel.Error);
                        MessageBox.Show("The Emergency stop button is pressed,Please release the Button", "EMG Note", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return false;
                    }                 
                }
                else
                {
                    LogMessage($"EMGIO status data is null", MessageLevel.Error);
                    return false;
                }
            }
            catch (Exception exio)
            {
                LogMessage($"CheckEMGIO Error: {exio.Message}", MessageLevel.Error);
                return false;
            }
            return true;
        }
        public bool CheckDriverIO()
        {
            string IOstatusdata = string.Empty;
            ushort[] IO_Data = null;
            bool IO_Status = false;
            try
            {              
                switch (Motor)
                {                 
                    case  MOTION_MOTOR.Stepper_Motor:
                        IO_Status = READ("0x0179", 1, ref IO_Data, TransmitMode.Holding_registers_Single);
                        if (IO_Status == false)
                        {
                            LogMessage($"Read IO_Status (0x0179) Fail", MessageLevel.Error);
                            return false;
                        }
                        break;
                    case  MOTION_MOTOR.Servo_Motor:
                        IO_Status = READ("0x602E", 1, ref IO_Data, TransmitMode.Holding_registers_Single);
                        if (IO_Status == false)
                        {
                            LogMessage($"Read IO_Status (0x602E) Fail", MessageLevel.Error);
                            return false;
                        }
                        break;
                    default:
                        LogMessage($"Undefind Motor Type", MessageLevel.Error);
                        return false;
                }               
                IOstatusdata = ReadDataParse(IO_Data, ReadFormat.BIN);

                if (!string.IsNullOrEmpty(IOstatusdata))
                {
                    int IO_Statusdata = Convert.ToInt32(IOstatusdata, 2);
                    int EMGstatus_result = EMG_Status ? 1 : 0;
                    int PositiveLimitstatus_result = PositiveLimit_Status ? 1 : 0;
                    int NegativeLimitstatus_result = NegativeLimit_Status ? 1 : 0;
                    bool EMG = ((IO_Statusdata >> EMG_Channel) & 1) == EMGstatus_result;
                    bool PL = ((IO_Statusdata >> PositiveLimit_Channel) & 1) == PositiveLimitstatus_result;
                    bool NL = ((IO_Statusdata >> NegativeLimit_Channel) & 1) == NegativeLimitstatus_result;
                    //LogMessage($"EMG IOstatusdata:{IOstatusdata} , Check EMG_data :{EMG}", MessageLevel.Info);
                    //LogMessage($"PL IOstatusdata:{IOstatusdata} , Check PL_data :{PL}", MessageLevel.Info);
                    LogMessage($"NL IOstatusdata:{IOstatusdata} , Check NL_data :{NL}", MessageLevel.Info);
                    if (EMG)
                    {
                        //EmgStop();
                        LogMessage($"The Emergency stop button is pressed", MessageLevel.Error);
                        MessageBox.Show("The Emergency stop button is pressed,Please release the Button", "EMG Note", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return false;
                    }
                    if (PL)
                    {                   
                        LogMessage($"The PositiveLimit Sensor is ON!!!", MessageLevel.Error);
                        MessageBox.Show("The PositiveLimit Sensor is ON!!!,Please Reset Motion To Home", "Reset Motion Note", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return false;
                    }
                    if (NL)
                    {                       
                        LogMessage($"The NegativeLimit Sensor is ON!!!", MessageLevel.Error);
                        MessageBox.Show("The NegativeLimit Sensor is ON!!!,Please Reset Motion To Home", "Reset Motion Note", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return false;
                    }
                }
                else
                {
                    LogMessage($"IO status data is null", MessageLevel.Error);
                    return false;
                }
            }
            catch (Exception exio)
            {
                LogMessage($"CheckDriverIO Error: {exio.Message}", MessageLevel.Error);
                return false;
            }
            return true;
        }

        public bool CheckErrInfo()
        {
            string Errstatusdata = string.Empty;
            ushort[] ERRData = null;
            bool Err_ret = false;
            try
            {
                switch (Motor)
                {
                    case MOTION_MOTOR.Stepper_Motor:
                        Err_ret = READ("0x2203", 6, ref ERRData, TransmitMode.Holding_registers_Single);
                        if (Err_ret == false)
                        {
                            LogMessage($"CheckErrInfo:Read Err_ret (0x2203) Fail", MessageLevel.Error);
                            return false;
                        }
                        break;
                    case MOTION_MOTOR.Servo_Motor:
                        Err_ret = READ("0x0B03", 6, ref ERRData, TransmitMode.Holding_registers_Single);
                        if (Err_ret == false)
                        {
                            LogMessage($"CheckErrInfo:Read Err_ret (0x0B03) Fail", MessageLevel.Error);
                            return false;
                        }
                        break;
                    default:
                        LogMessage($"Undefind Motor Type", MessageLevel.Error);
                        return false;
                }             
                Errstatusdata = ReadDataParse(ERRData, ReadFormat.HEX);
                //LogMessage($"Errstatusdata:{Errstatusdata}", MessageLevel.Info);
                if (!Errstatusdata.Contains("0x00"))
                {
                    LogMessage($"CheckErrInfo Error Information: {Errstatusdata}", MessageLevel.Error);
                    return false;
                }

            }
            catch (Exception ex_err)
            {
                LogMessage($"CheckErrInf Error: {ex_err.Message}", MessageLevel.Error);
                return false;
            }

            return true;
        }

        public override bool EmgStop()
        {
            bool Move_Stop = SEND("0x6002", "0x0040", TransmitMode.Holding_registers_Single);
            if (Move_Stop == false)
            {
                MessageBox.Show("Move Stop Fail, Please Press Emergency stop button for the machine", "Motion Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            
            return true;

            /*bool Pass_Fail = false;
            try
            {
                if (modbusMaster != null)
                {
                    modbusMaster.SetTimeout(3000, 0);
                    modbusMaster.RetryTimes(0);
                    Pass_Fail = modbusMaster.Write(0, "0x6002", "0x0040", TransmitMode.Holding_registers_Single);//廣播停止
                }
                else
                    LogMessage($"Modbus Master is null", MessageLevel.Error);

            }
            catch (Exception Send_ex)
            {
                LogMessage($"Modbus Send Error=>{ Send_ex.Message}", MessageLevel.Error);
                return false;
            }

            return Pass_Fail;*/
        }

        public override bool Relative_Move(double Distance, double in_start_vel, double in_max_vel, double tacc, double dac)
        {
            bool Move_Flag = false;
            bool EMG_Stop = false;
            Dictionary<string, string> ModBus_output = new Dictionary<string, string>();

            try
            {
                int INT_Distance = Convert.ToInt32(Distance / Resolution);
                string Distance_hexValue = INT_Distance.ToString("X"); // 轉換為十六進制表示 //移動距離
                int Velocity_DEC = int.Parse($"{in_max_vel}");
                int Tacc_DEC = int.Parse($"{tacc}");
                int Dac_DEC = int.Parse($"{dac}");

                string Velocity_hexValue = Velocity_DEC.ToString("X"); // 轉換為十六進制表示 //移動速度
                string Tacc_hexValue = Tacc_DEC.ToString("X"); // 轉換為十六進制表示 //加速度
                string Dac_hexValue = Dac_DEC.ToString("X"); // 轉換為十六進制表示   //減速度

                // 判斷十六進制表示的長度
                int length = Distance_hexValue.Length;

                // 根據長度來區分高位元和低位元
                string highByte, lowByte;
                if (length <= 2)
                {
                    highByte = "0000";
                    lowByte = Distance_hexValue.PadLeft(4, '0');
                }
                else if (length < 4)
                {
                    highByte = "0000";
                    lowByte = Distance_hexValue.Substring(length - 3, 3).PadLeft(4, '0');
                }
                else
                {
                    highByte = Distance_hexValue.Substring(0, length - 4).PadLeft(4, '0');
                    lowByte = Distance_hexValue.Substring(length - 4, 4);
                }

                Move_Flag = SEND("0x6200", $"0x0041 0x{highByte} 0x{lowByte} 0x{Velocity_hexValue} 0x{Tacc_hexValue} 0x{Dac_hexValue}", TransmitMode.Holding_registers_Multiple);//相對路徑
                Move_Flag &= SEND("0x6002", $"0x0010", TransmitMode.Holding_registers_Single);//PR0 路徑:0
                if (Move_Flag == false)
                {
                    EmgStop();
                    LogMessage($"Relative_Move Fail", MessageLevel.Error);
                    return false;
                }
                
            }
            catch (Exception ex)
            {
                EMG_Stop = EmgStop();
                LogMessage($"Relative_Move Error=>{ ex.Message}", MessageLevel.Error);
                return false;
            }

            return true;
        }

        public override bool Absolute_Move(double Distance, double in_start_vel, double in_max_vel, double tacc, double dac)
        {
            bool Move_Flag = false;
            bool EMG_Stop = false;

            Dictionary<string, string> ModBus_output = new Dictionary<string, string>();

            try
            {

                int INT_Distance = Convert.ToInt32(Distance / Resolution);
                string Distance_hexValue = INT_Distance.ToString("X"); // 轉換為十六進制表示 //移動距離
                int Velocity_DEC = int.Parse($"{in_max_vel}");
                int Tacc_DEC = int.Parse($"{tacc}");
                int Dac_DEC = int.Parse($"{dac}");

                string Velocity_hexValue = Velocity_DEC.ToString("X"); // 轉換為十六進制表示 //移動速度
                string Tacc_hexValue = Tacc_DEC.ToString("X"); // 轉換為十六進制表示 //加速度
                string Dac_hexValue = Dac_DEC.ToString("X"); // 轉換為十六進制表示   //減速度
                //LogMessage($"Hex_Distance:{Distance_hexValue}\r\nVelocity_hexValue:{Velocity_hexValue}\r\nTacc_hexValue:{Tacc_hexValue}\r\nDac_hexValue:{Dac_hexValue}\r\n", MessageLevel.Info);

                // 判斷十六進制表示的長度
                int length = Distance_hexValue.Length;

                // 根據長度來區分高位元和低位元
                string highByte, lowByte;
                if (length <= 2)
                {
                    highByte = "0000";
                    lowByte = Distance_hexValue.PadLeft(4, '0');
                }
                else if (length < 4)
                {
                    highByte = "0000";
                    lowByte = Distance_hexValue.Substring(length - 3, 3).PadLeft(4, '0');
                }
                else
                {
                    highByte = Distance_hexValue.Substring(0, length - 4).PadLeft(4, '0');
                    lowByte = Distance_hexValue.Substring(length - 4, 4);
                }
                //LogMessage($"Hex_Distance_H:0x{highByte}\r\nHex_Distance_L:0x{lowByte}\r\n", MessageLevel.Info);
                //LogMessage($"##########SEND Moving Parameter--##########Start", MessageLevel.Info);
                Move_Flag = SEND("0x6200", $"0x0001 0x{highByte} 0x{lowByte} 0x{Velocity_hexValue} 0x{Tacc_hexValue} 0x{Dac_hexValue}", TransmitMode.Holding_registers_Multiple);//絕對路徑
                //LogMessage($"##########SEND Moving Parameter--##########End", MessageLevel.Info);
                //LogMessage($"##########SEND Trigger--##########Start", MessageLevel.Info);
                Move_Flag &= SEND("0x6002", $"0x0010", TransmitMode.Holding_registers_Single); //PR0 路徑:0
                //LogMessage($"##########SEND Trigger--##########End", MessageLevel.Info);
                if (Move_Flag == false)
                {
                    EmgStop();
                    LogMessage($"Absolute_Move Fail", MessageLevel.Error);
                    return false;
                }
              
            }
            catch (Exception ex)
            {
                EMG_Stop = EmgStop();
                LogMessage($"Absolute_Move Error=>{ ex.Message}", MessageLevel.Error);
                return false;
            }

            return true;
        }
        public bool PositionValue(ref double Value)
        {
            try
            {
                bool Motion_Position = false;
                ushort[] PositionData = null;
                string Position_Data = string.Empty;
                //int Value = 0;
                Motion_Position = READ("0x602C", 2, ref PositionData, TransmitMode.Holding_registers_Single);
                if (Motion_Position == false)
                {
                    LogMessage($"Motion_Position Get Fail", MessageLevel.Error);
                    return false;
                }
                int[] intArray = PositionData.Select(ushortValue => (int)ushortValue).ToArray();
                if (intArray[0] == 65535)//負數
                {
                    Value = ~(intArray[0] - intArray[1]);//補數
                }
                else
                {
                    Value = intArray[1];    // 根據你的設備可能需要調整比例
                }
                Value = Value * Resolution;
                //LogMessage($"Current PositionData:{Value}", MessageLevel.Info);
            }
            catch (Exception ex)
            {
                LogMessage($"PositionData Error:{ex.Message}", MessageLevel.Error);
                return false;
            }
            return true;
        }

        public override bool PostionLimitSet(double PositiveLimit, double NegativeLimit)
        {
            bool Postionret = false;
            bool Negtionret = false;
            try
            {
                int INT_PositiveLimit = Convert.ToInt32(PositiveLimit / Resolution);
                // 取高16位
                int Poshigh16 = (INT_PositiveLimit >> 16) & 0xFFFF;
                string Poshexhigh16 = Poshigh16.ToString("X");
                // 取低16位
                int Poslow16 = INT_PositiveLimit & 0xFFFF;
                string Poshexlow16 = Poslow16.ToString("X");

                Postionret = SEND("0x6006", $"0x{Poshexhigh16}", TransmitMode.Holding_registers_Single);
                Postionret &= SEND("0x6007", $"0x{Poshexlow16}", TransmitMode.Holding_registers_Single);
                if (Postionret == false)
                {
                    LogMessage($"PositiveLimit Set Fail", MessageLevel.Error);
                    return false;
                }

                int INT_NegativeLimit = Convert.ToInt32(NegativeLimit / Resolution);
                // 取高16位
                int Neghigh16 = (INT_NegativeLimit >> 16) & 0xFFFF;
                string Neghexhigh16 = Neghigh16.ToString("X");
                // 取低16位
                int Neglow16 = INT_NegativeLimit & 0xFFFF;
                string Neghexlow16 = Neglow16.ToString("X");

                Negtionret = SEND("0x6008", $"0x{Neghexhigh16}", TransmitMode.Holding_registers_Single);
                Negtionret &= SEND("0x6009", $"0x{Neghexlow16}", TransmitMode.Holding_registers_Single);
                if (Negtionret == false)
                {
                    LogMessage($"NegativeLimit Set Fail", MessageLevel.Error);
                    return false;
                }
            }
            catch (Exception ex)
            {
                LogMessage($"Limit Postion Set Error:{ex.Message}", MessageLevel.Error);
                return false;
            }
            return true;
        }
       
        public string HomeMode()
        {
            int dir = 0;
            int specify_location = 1;
            string Signal = string.Empty;
            string FinalByte = string.Empty;
            string hexResult = string.Empty;
            string mode = string.Empty;
            try
            {
                switch (Mode)
                {
                    case ResetMode.Limit_Reset:
                        mode = "000000";
                        break;
                    case ResetMode.Origin_Reset:
                        mode = "000001"; 
                        break;
                    case ResetMode.Z_Reset:
                        mode = "000010";
                        
                        break;
                    case ResetMode.Torque_Reset:
                        mode = "000011";
                        break;
                    case ResetMode.Immediately_Reset:
                        mode = "001000";
                        break;
                }
                switch (Dir)
                {
                    case ResetDirection.Negative:
                        dir = 0;
                        break;
                    case ResetDirection.Positive:
                        dir = 1;
                        break;
                }

                switch (ZSignal)
                {
                    case Z_Signal.OFF:
                            Signal = "0000";
                        break;
                    case Z_Signal.ON:
                            Signal = "0001";
                        break;
                }

                // 輸入新的前3個bit值
                string newBits = $"{Signal}{mode}{specify_location}{dir}";

                // 將新的前3個bit值轉換為byte
                int newBitsByte = Convert.ToInt32(newBits, 2);

                // 將結果轉換為Hex
                hexResult = newBitsByte.ToString("X4");
                hexResult = $"0x{hexResult}";             
            }
            catch (Exception ex)
            {
                LogMessage($"Home Set Mode Error:{ex.Message}", MessageLevel.Error);
                return "";
            }

            return hexResult;
        }
        public bool HomeParam(double Home_Position, double Stop_Position, int Stop_vel_HS, int Stop_vel_LS, int tacc, int dac)
        {
            bool Home_Flag = false;

            try
            {
                int INTHome_Position = Convert.ToInt32(Home_Position / Resolution);
                //string hexPosValue = Home_Position.ToString("X");
                // 取高16位
                int high16 = (INTHome_Position >> 16) & 0xFFFF;
                string hexhigh16 = high16.ToString("X");
                // 取低16位
                int low16 = INTHome_Position & 0xFFFF;
                string hexlow16 = low16.ToString("X");

                int INTStop_Position = Convert.ToInt32(Stop_Position / Resolution);
                //string hexStopPosValue = Stop_Position.ToString("X");
                // 取高16位
                int stop_high16 = (INTStop_Position >> 16) & 0xFFFF;
                string stophexhigh16 = stop_high16.ToString("X");
                // 取低16位
                int stoplow16 = INTStop_Position & 0xFFFF;
                string stophexlow16 = stoplow16.ToString("X");

                string hex_vel_HS = Stop_vel_HS.ToString("X");
                string hex_vel_LS = Stop_vel_LS.ToString("X");
                string hex_tacc = tacc.ToString("X");
                string hex_dac = dac.ToString("X");

                //Home_Position Setting
                string Mode_Str = HomeMode();
                Home_Flag = SEND("0x600A",$"{Mode_Str} 0x{hexhigh16} 0x{hexlow16} 0x{stophexhigh16} 0x{stophexlow16} 0x{hex_vel_HS} 0x{hex_vel_LS} 0x{hex_tacc} 0x{hex_dac}", TransmitMode.Holding_registers_Multiple);
                if (Home_Flag == false)
                {
                    LogMessage($"Home_Position Set Fail", MessageLevel.Error);
                    return false;
                }
            }
            catch (Exception Ex)
            {
                LogMessage($"HomeParam Set Error:{Ex.Message}", MessageLevel.Error);
                return false;
            }

            return true;
        }

        /*public override bool Trigger(TriggerMode CTRG, string HexAddress, string HexData, ref string Rec_Data)
        {
            bool CTRG_ret = false;
            string Output = string.Empty;
            switch (CTRG)
            {
                case TriggerMode.RS485_Trigger:
                    CTRG_ret = RS485Trigger(HexAddress, HexData);
                    break;
                case TriggerMode.Register_Trigger:
                    CTRG_ret = RegisterTrigger(HexData, ref Output);
                    break;
                case TriggerMode.IOTrigger:
                    CTRG_ret = IOTrigger();
                    break;
            }
            Rec_Data = Output;
            if (CTRG_ret == false)
            {
                LogMessage($"Trigger Fail", MessageLevel.Error);
                return false;
            }

            return true;
        }*/
        /*public bool RS485Trigger(string HexAddress, string HexData)
        {
            bool PR_Ret = false;
            try
            {
                PR_Ret = SEND(HexAddress, HexData, TransmitMode.Holding_registers_Single);
                if (PR_Ret)
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                LogMessage($"RS485Trigger Error:{ex.Message}", MessageLevel.Error);
                return false;
            }
        }*/
        /*public bool RegisterTrigger(string HexData, ref string output)
        {
            bool Home_Flag = false;
            ushort[] Data = null;
            try
            {
                // Set Point to Zero
                Home_Flag = SEND("0x6002", HexData, TransmitMode.Holding_registers_Single);
                Home_Flag = READ("0x6002", 1, ref Data, TransmitMode.Holding_registers_Single);
                if (Home_Flag == false || Data == null)
                {
                    LogMessage($"Home Point to Zero Fail", MessageLevel.Error);
                    return false;
                }
                output = ReadDataParse(Data, ReadFormat.HEX);
                if (string.IsNullOrEmpty(output))
                {
                    LogMessage($"ReadDataParse output Data is null", MessageLevel.Error);
                    return false;
                }
            }
            catch (Exception ex)
            {
                LogMessage($"Home Point to Zero Error:{ex.Message}", MessageLevel.Error);
                return false;
            }
            return true;
        }*/
        /*public bool IOTrigger()
        {
            bool DI_ctrg = false;
            bool DO_ctrg = false;
            //DI
            DI_ctrg = SEND("0x0145", IOTrigger_DI1, TransmitMode.Holding_registers_Single);
            DI_ctrg &= SEND("0x0147", IOTrigger_DI2, TransmitMode.Holding_registers_Single);
            DI_ctrg &= SEND("0x0149", IOTrigger_DI3, TransmitMode.Holding_registers_Single);
            DI_ctrg &= SEND("0x014B", IOTrigger_DI4, TransmitMode.Holding_registers_Single);
            DI_ctrg &= SEND("0x014D", IOTrigger_DI5, TransmitMode.Holding_registers_Single);
            DI_ctrg &= SEND("0x014F", IOTrigger_DI6, TransmitMode.Holding_registers_Single);
            DI_ctrg &= SEND("0x0151", IOTrigger_DI7, TransmitMode.Holding_registers_Single);
            if (DI_ctrg == false)
            {
                LogMessage($"Set DI IOTrigger Fail", MessageLevel.Error);
                return false;
            }
            //DO
            DO_ctrg = SEND("0x0157", IOTrigger_DO1, TransmitMode.Holding_registers_Single);
            DO_ctrg &= SEND("0x0159", IOTrigger_DO2, TransmitMode.Holding_registers_Single);
            DO_ctrg &= SEND("0x015B", IOTrigger_DO3, TransmitMode.Holding_registers_Single);
            if (DO_ctrg == false)
            {
                LogMessage($"Set DO IOTrigger Fail", MessageLevel.Error);
                return false;
            }


            return true;
        }*/
        //Home Set and Trigger --END
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
        public override bool SdStop()
        {
            return true;
        }

        public override bool ServoOFF()
        {
            return true;
        }

        public override bool ServoON()
        {

            return true;
        }
        public override bool GetCommandPos(ref double out_pos)
        {
            double Pos = 0;
            bool Pos_Ret = false;
            Pos_Ret = PositionValue(ref Pos);
            if (Pos_Ret == false)
            {
                LogMessage($"Get Position Fail", MessageLevel.Error);
                return false;
            }
            LogMessage($"Get Current Position: {Pos}", MessageLevel.Info);
            out_pos = Pos;
            return true;
        }
        public override bool GetCurrentPos(ref double out_pos)
        {
            return GetCommandPos(ref out_pos);
        }
        public override bool SetCommandPos(double in_pos)
        {

            return true;
        }

        public override bool StartAction(string strItemName, string strParamIn, ref string strOutput)
        {
            throw new NotImplementedException();
        }
        public override bool SyncHome(double in_start_vel, double in_max_vel, int Dir, int Timeout)
        {
            throw new NotImplementedException();
        }
        public override bool SyncHome()
        {
            bool TrigerRun = false;
            bool ret = false;
            //double out_homepos = 0;
            string RecDataAll = string.Empty;

            //Home Param Set
            ret = HomeParam(HomePosition, StopPosition, Stop_Vel_HS, Stop_Vel_LS, Tacc, Dac);
            if (ret == false)
            {
                return false;
            }
            //TrigerRun = Trigger(TriggerMode.Register_Trigger, "", "0x020", ref RecDataAll);
            TrigerRun = SEND("0x6002", "0x020", TransmitMode.Holding_registers_Single);
            if (TrigerRun == false)
            {
                EmgStop();
                LogMessage($"Reset Home Fail Stop Motor", MessageLevel.Error);
                return false;
            }

            /*if (Status_Check)
            {
                bool Homme_Done = SyncResetHomeDone();
                if (Homme_Done)
                {
                    bool homePos = GetCurrentPos(ref out_homepos);
                    if(!homePos)
                    {
                        LogMessage($"Reset HomePos Fail, ResetHomeDone Fail", MessageLevel.Error);
                        return false;
                    }

                    if (SetZero)
                    {
                        bool home_end = SEND("0x6002", $"0x0021", TransmitMode.Holding_registers_Single);//Set Zero Point
                        if (home_end == false)
                        {
                            LogMessage($"Reset Home Fail, Set Zero Point Fail", MessageLevel.Error);
                            return false;
                        }
                        LogMessage($"Set HomeZero Point Succeed", MessageLevel.Info);
                    }
                }
                else
                {
                    bool homePos = GetCurrentPos(ref out_homepos);
                    if (!homePos)
                    {
                        LogMessage($"Reset HomePos Fail, ResetHomeDone Fail", MessageLevel.Error);
                        return false;
                    }
                    LogMessage($"Reset Home Fail:Position=>{out_homepos}, ResetHomeDone Fail", MessageLevel.Error);
                    return false;
                }
            }*/
        
            //LogMessage($"Reset Home Done, ResetHomeDone:Position=>{out_homepos}", MessageLevel.Info);
            return true;
        }

        public override bool Check_IO_StartStatus(Dictionary<int, int> Devices_IO_Status)
        {
            throw new NotImplementedException();
        }     

        /*public override bool SyncResetHomeDone()
        {
            try
            {
                int status = 0;
                HomeDone(ref  status);
                //LogMessage($"=====Home Status Done:{status}=====");
            }
            catch(Exception ex)
            {
                LogMessage($"Motion Home Error:{ex.Message}", MessageLevel.Error);
                return false;
            }

            return true;
        }*/

        public override void Dispose()
        {
            throw new NotImplementedException();
        }

        public enum ReadFormat
        {
            HEX,
            DEC,
            BIN
        }

    }
}
