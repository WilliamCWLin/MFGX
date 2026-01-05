using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using NModbus;
using NModbus.Serial;
using static AutoTestSystem.BLL.Bd;
using static AutoTestSystem.DAL.Communication;

namespace AutoTestSystem.Equipment.ControlDevice
{
    class SIMBA_Sensor : ControlDeviceBase
    {
        private int FilterStrength_Value = 20;
        public string DIVParam = string.Empty;
        private float CAP_Value;
        private float CAP_DeviceValue;
        private float SEn_Value;
        private float SEn_DeviceValue;
        private float Span_Value;
        private float Span_DeviceValue;

        [Category("SIMBA Sensor ID"), Description("SlaveID")]
        public byte SlaveID { get; set; } = 1;
        [Category("SIMBA Sensor Manual Mode"), Description("手動設定功能(自動:關閉 手動:開啟)")]
        public bool ManualMode { get; set; } = false;
        [Category("SIMBA Sensor Parameters"), Description("數字標定:分度值設定"), TypeConverter(typeof(DIV))]
        public string Div { get; set; } = "0.001";

        [Category("SIMBA Sensor Parameters"), Description("數字標定:最大秤量值設定")]
        public decimal CAP { get; set; }
        /*public float CAP {
            get
            {
                if (CAP_DeviceValue == 0)
                    return 10;
                else
                    return CAP_DeviceValue;
            }
            set
            {
                CAP_DeviceValue = CheckCAPValue(value);
            }
        }*/
        [Category("SIMBA Sensor Parameters"), Description("數字標定:靈敏度設定")]
        public decimal SEn { get; set; }
        /*public float SEn {
            get
            {
                if (SEn_DeviceValue == 0)
                    return 1;
                else
                    return SEn_DeviceValue;
            }
            set
            {
                SEn_DeviceValue = CheckSEnValue(value);
            }
        }*/
        [Category("SIMBA Sensor Parameters"), Description("數字標定:傳感器量程設定")]
        public decimal Span { get; set; }
        /*public float Span {
            get
            {
                if (Span_DeviceValue == 0)
                    return 10;
                else
                    return Span_DeviceValue;
            }
            set
            {
                Span_DeviceValue = CheckSpanValue(value);
            }
        }*/

        [Category("SIMBA Sensor ZeroRange Parameters"), Description("傳感器歸零範圍設定"), TypeConverter(typeof(Zero_Range))]
        public string ZeroRange { get; set; } = "Default";

        [Category("SIMBA Sensor Filter Parameters"), Description("傳感器濾波類型設定(0:不使用,1:複合濾波,2中位值濾波,3:一階濾波,4:滑動平均濾波,5:中位值平均濾波,\r\n6:滑動中位值平均濾波,7:平均值濾波+一階濾波,8:中位值濾波+一階濾波,9:滑動平均濾波+一階濾波,10:中位值平均濾波+一階濾波)"), TypeConverter(typeof(Filter))]
        public string FilterMode { get; set; } = "9";

        [Category("SIMBA Sensor Filter Parameters"), Description("傳感器一階濾波強度(0~50)")]
        public int FilterStrength
        {
            get
            {
                if (FilterStrength_Value < 0)
                    return 20;
                else
                    return FilterStrength_Value;
            }
            set
            {
                FilterStrength_Value = CheckFilterStrengthValue(value);
            }
        }



        /*[Category("NModBus Parameters"), Description("SlaveID"), Editor(typeof(ModBusEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public string SlaveID { get; set; }*/

        [Category("Comport Parameters"), Description("Select Comport"), TypeConverter(typeof(ModBusComportList))]
        public string PortName { get; set; }

        [Category("Comport Parameters"), Description("baudRate")]
        public int baudRate { get; set; } = 9600;

        NewModBus modbusMaster = null;

        public SIMBA_Sensor()
        {
            baudRate = 9600;
        }

        private float CheckCAPValue(float truncatedCAPValue)
        {
            string[] parts = truncatedCAPValue.ToString().Split('.');
            if (parts.Length > 1 && parts[1].Length > 3)
            {
                MessageBox.Show("輸入的浮點數不能超過小數點第三位。", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return CAP_Value;
            }
            /*else
            {
                truncatedCAPValue = (float)Math.Truncate(truncatedCAPValue * 1000) / 1000;
            }*/

            if (truncatedCAPValue < 0 || truncatedCAPValue > 1000)
            {
                MessageBox.Show($"Device_value Range is \"0\" to \"1000\"", "CAP Value Note", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return CAP_Value;
            }
            CAP_Value = truncatedCAPValue;
            return truncatedCAPValue;
        }

        private float CheckSEnValue(float truncatedSEnValue)
        {
            string[] parts = truncatedSEnValue.ToString().Split('.');
            if (parts.Length > 1 && parts[1].Length > 3)
            {
                MessageBox.Show("輸入的浮點數不能超過小數點第三位。", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return SEn_Value;
            }
            /*else
            {
                truncatedSEnValue = (float)Math.Truncate(truncatedSEnValue * 1000) / 1000;
            }*/

            if (truncatedSEnValue < 0.4 || truncatedSEnValue > 9)
            {
                MessageBox.Show($"Device_value Range is \"0.4\" to \"9\"", "CAP Value Note", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return SEn_Value;
            }
            SEn_Value = truncatedSEnValue;
            return truncatedSEnValue;
        }

        private float CheckSpanValue(float truncatedSpanValue)
        {
            string[] parts = truncatedSpanValue.ToString().Split('.');
            if (parts.Length > 1 && parts[1].Length > 3)
            {
                MessageBox.Show("輸入的浮點數不能超過小數點第三位。", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return Span_Value;
            }
            /*else
             {
                 truncatedSpanValue = (float)Math.Truncate(truncatedSpanValue * 1000) / 1000;
             }*/

            if (truncatedSpanValue < 0 || truncatedSpanValue > 100)
            {
                MessageBox.Show($"Device_value Range is \"0\" to \"100\"", "CAP Value Note", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return Span_Value;
            }
            Span_Value = truncatedSpanValue;
            return truncatedSpanValue;
        }


        public string MatchDIVParam(string value)
        {
            if (value == "0.0001")
                return "0x0000";
            else if (value == "0.001")
                return "0x0003";
            else if (value == "0.01")
                return "0x0006";
            else if (value == "0.1")
                return "0x0009";
            else if (value == "1")
                return "0x000C";
            else if (value == "5")
                return "0x000E";
            else
                return "Err";
        }
        public string ConverToHex(int value)
        {
            //int num = Convert.ToInt32(value, 16);
            string hexNumber = value.ToString("X2");
            return hexNumber;
        }

        private int CheckFilterStrengthValue(int FilterStrength)
        {
            if (FilterStrength < 0 || FilterStrength > 50)
            {
                MessageBox.Show($"FilterStrength Range is \"0\" to \"50\"", "FilterStrength Note", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return FilterStrength_Value;
            }
            return FilterStrength;
        }

        public static int ConvertDoubleToInt(decimal input)
        {
            string inputString = input.ToString("G17"); // 使用 "G17" 格式確保精度
            inputString = inputString.Replace(".", ""); // 去掉小數點

            if (inputString.Length > 5)
            {
                inputString = inputString.Substring(0, 5); // 截取前5個字元
            }

            if (double.TryParse(inputString, out double result))
            {
                string resultString = ((int)result).ToString();
                while (resultString.Length < 5)
                {
                    resultString += "0"; // 補足長度到5
                }
                return int.Parse(resultString);
            }
            else
            {
                throw new ArgumentException("輸入的數值無法轉換成整數。");
            }
        }


        public override bool Init(string strParamInfo)
        {
            string CAPParam = string.Empty;
            string SEnParam = string.Empty;
            string SpanParam = string.Empty;

            if (string.IsNullOrEmpty(PortName))
            {
                LogMessage("NO COM Port Name", MessageLevel.Error);
                return false;
            }

            try
            {
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
                    else
                    {
                        //modbusMaster.Close();
                        //modbusMaster = null;
                        if (!modbusMaster.IsOpen)
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
                    }
                }
                if (!ManualMode)
                {
                    int cap = ConvertDoubleToInt(CAP);
                    int sen = ConvertDoubleToInt(SEn);
                    int span = ConvertDoubleToInt(Span);
                    DIVParam = MatchDIVParam(Div);
                    LogMessage($"CAP:{cap},SEN:{sen},SPAN:{span} DIV:{DIVParam}", MessageLevel.Info);

                    CAPParam = ConverToHex(cap);
                    SEnParam = ConverToHex(sen);
                    SpanParam = ConverToHex(span);
                    bool Sensor_Reset = false;
                    bool Sensor_Set = false;
                    Sensor_Set = SEND("0x0058", DIVParam, TransmitMode.Holding_registers_Multiple);//分度值 0.01
                    if (Sensor_Set == false)
                    {
                        LogMessage("DIVParam Sensor_Set Fail", MessageLevel.Error);
                        return false;
                    }
                    Sensor_Set = SEND("0x0056", $"0x0000 0x{CAPParam}", TransmitMode.Holding_registers_Multiple);//最大秤量 101.97
                    if (Sensor_Set == false)
                    {
                        LogMessage("CAPParam Sensor_Set Fail", MessageLevel.Error);
                        return false;
                    }
                    Sensor_Set = SEND("0x002E", $"0x0000 0x{SEnParam}", TransmitMode.Holding_registers_Multiple);//敏度值 1.4350
                    if (Sensor_Set == false)
                    {
                        LogMessage("SEnParam Sensor_Set Fail", MessageLevel.Error);
                        return false;
                    }
                    Sensor_Set = SEND("0x0030", $"0x0000 0x{SpanParam}", TransmitMode.Holding_registers_Multiple);//實際量程 100.00
                    if (Sensor_Set == false)
                    {
                        LogMessage("SpanParam Sensor_Set Fail", MessageLevel.Error);
                        return false;
                    }

                    bool FilterTypeSet = FilterSelect();
                    if (FilterTypeSet == false)
                        return false;
                    bool FilterStrengthSet = Filter_Strength();
                    if (FilterStrengthSet == false)
                        return false;

                    if (ZeroRange != "Default")
                    {
                        int INTZero = int.Parse(ZeroRange);
                        string HexZero = INTZero.ToString("X4");
                        Sensor_Reset = SEND("0x005D", $"0x{HexZero}", TransmitMode.Holding_registers_Multiple);
                        if (Sensor_Reset == false)
                        {
                            LogMessage("Sensor_Reset Zero Range Fail", MessageLevel.Error);
                            return false;
                        }
                    }
                    Sensor_Reset = SEND("0x005E", "0x00FF", TransmitMode.Holding_registers_Multiple);
                    if (Sensor_Reset == false)
                    {
                        LogMessage("Sensor_Reset to Zero Fail", MessageLevel.Error);
                        return false;
                    }
                }
                else
                {
                    MessageBox.Show("The SIMBA Sensor is Manual Mode,Please Set Parameter from Electronic Keyboard", "ManualMode", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

            }
            catch (Exception ex)
            {
                LogMessage($"Modbus Error=>{ ex.Message} Init Fail", MessageLevel.Error);
                return false;
            }

            LogMessage("Sensor_Init Success", MessageLevel.Info);

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

        public bool FilterSelect()
        {
            try
            {
                bool Filter_ret = false;
                int INTFilter = int.Parse(FilterMode);
                string HexFilter = ConverToHex(INTFilter);
                LogMessage($"Sensor Filter Mode Set {FilterMode} => Hex : 0x00{HexFilter}", MessageLevel.Info);

                Filter_ret = SEND("0x0022", $"0x00{HexFilter}", TransmitMode.Holding_registers_Multiple);
                if (Filter_ret == false)
                {
                    LogMessage($"Sensor Filter Set Fail", MessageLevel.Error);
                    return false;
                }

            }
            catch (Exception filter_ex)
            {
                LogMessage($"Sensor Filter Error=>{ filter_ex.Message}", MessageLevel.Error);
                return false;
            }

            return true;
        }
        public bool Filter_Strength()
        {
            try
            {
                bool Filter_Strength_ret = false;
                string HexFilterStrength = ConverToHex(FilterStrength);
                LogMessage($"Sensor Filter Strength Set {FilterStrength} => Hex : 0x00{HexFilterStrength}", MessageLevel.Info);

                Filter_Strength_ret = SEND("0x0023", $"0x00{HexFilterStrength}", TransmitMode.Holding_registers_Multiple);
                if (Filter_Strength_ret == false)
                {
                    LogMessage($"Sensor Filter Strength Set Fail", MessageLevel.Error);
                    return false;
                }
            }
            catch (Exception filter_Strengthex)
            {
                LogMessage($"Sensor Filter Strength Error=>{ filter_Strengthex.Message}", MessageLevel.Error);
                return false;
            }

            return true;
        }
        public bool SEND(string Address, string writeData, TransmitMode Mode)
        {
            bool Pass_Fail = false;
            try
            {
                if (modbusMaster != null)
                {
                    modbusMaster.SetTimeout(1000, 1000);
                    modbusMaster.RetryTimes(1);
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


        public bool READ(string Address, ushort numRegisters, ref ushort[] DataRecAll, TransmitMode Mode)
        {
            bool Pass_Fail = false;
            try
            {
                if (modbusMaster != null)
                {
                    modbusMaster.SetTimeout(1000, 1000);
                    modbusMaster.RetryTimes(1);
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
            /*if(modbusMaster !=null)
                modbusMaster.SetTimeout(time);*/
        }

        public override void RetryTimes(int time)
        {
            if (modbusMaster != null)
                modbusMaster.RetryTimes(time);
        }
        public class DIV : TypeConverter  //下拉式選單
        {
            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {


                List<string> DIVKeys = new List<string>();

                DIVKeys.Add("0.0001");
                DIVKeys.Add("0.001");
                DIVKeys.Add("0.01");
                DIVKeys.Add("0.1");
                DIVKeys.Add("1");
                DIVKeys.Add("5");

                return new StandardValuesCollection(DIVKeys);

            }

            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                return true;

            }

        }

        public class Zero_Range : TypeConverter  //下拉式選單
        {
            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {


                List<string> ZeroKeys = new List<string>();

                ZeroKeys.Add("Default");
                ZeroKeys.Add("10");
                ZeroKeys.Add("20");
                ZeroKeys.Add("50");
                ZeroKeys.Add("100");

                return new StandardValuesCollection(ZeroKeys);

            }

            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                return true;

            }

        }

        public class Filter : TypeConverter  //下拉式選單
        {
            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {


                List<string> FilterKeys = new List<string>();

                FilterKeys.Add("0");
                FilterKeys.Add("1");
                FilterKeys.Add("2");
                FilterKeys.Add("3");
                FilterKeys.Add("4");
                FilterKeys.Add("5");
                FilterKeys.Add("6");
                FilterKeys.Add("7");
                FilterKeys.Add("8");
                FilterKeys.Add("9");
                FilterKeys.Add("10");

                return new StandardValuesCollection(FilterKeys);

            }

            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                return true;

            }
        }

    }
}
