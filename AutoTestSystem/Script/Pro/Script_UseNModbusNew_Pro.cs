using AutoTestSystem.Base;
using AutoTestSystem.Equipment.ControlDevice;
using AutoTestSystem.Script;
using Newtonsoft.Json;
using NModbus;
using NModbus.Serial;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static AutoTestSystem.DAL.Communication;
//using static AutoTestSystem.ModBusForm;

namespace AutoTestSystem.Script
{
    internal class Script_UseNModbusNew_Pro : Script_ControlDevice_Base
    {
        string Jsondata = string.Empty;
        private string receivedValue = null;

        /*[Category("NModBus Parameters"), Description("SlaveID")]
        public byte SlaveID { get; set; } = 1;*/
        /*[Category("NModBus Parameters"), Description("選擇NModBus_SlaveID"), TypeConverter(typeof(ModBus_SlaveID))]
        public byte SlaveID { get; set; }*/
        [Category("NModBus Parameters"), Description("Select SelectTransmitMode(功能碼)")]
        public TransmitMode FunctionMode { get; set; } = TransmitMode.Holding_registers_Single;


        [Category("NModBus Parameters"), Description("Address")]
        public string StartAddress { get; set; } = "0";

        [Category("NModBus Send Parameters"), Description("WriteData (If Not to use Please Dont Write Anything),輸入Hex前面要加0x,輸入多筆資料須以逗號區分,若FunctionMode 為SingleCoil_Status 請選True or False"), TypeConverter(typeof(DataCommandConverter))]
        public string WriteData { get; set; }
        [Category("NModBus Send Parameters"), Description("Send RetryTime")]
        public int SendRetryTime { get; set; } = 3;
        [Category("NModBus Send Parameters"), Description("Send Timeout")]
        public int SendTimeout { get; set; } = 3000;

        [Category("NModBus Read Parameters"), Description("Read RetryTime")]
        public int ReadRetryTime { get; set; } = 1;

        [Category("NModBus Read Parameters"), Description("Read NumRegisters (If Not to use Please Set 0)")]
        public ushort NumRegisters { get; set; } = 2;
        [Category("NModBus Read Parameters"), Description("Read Timeout")]
        public int ReadTimeout { get; set; } = 1000;
        [Category("NModBus Read Parameters"), Description("NModBus ReadFormat")]
        public ReadFormat Format { get; set; } = ReadFormat.HEX;

        [Category("NModBus Read Mode"), Description("NModBus TestMode")]
        public TestMode Test_Mode { get; set; } = TestMode.Default;




        public Script_UseNModbusNew_Pro()
        {

        }
        public override void Dispose()
        {

        }
        public override bool PreProcess()
        {
            if (FunctionMode == TransmitMode.ModbusIO16Bit)
            {
                LogMessage($"TransmitMode.ModbusIO16Bit Only use Specific IO device This Script is Not Use it", MessageLevel.Error);
                return false;
            }
            Jsondata = string.Empty;
            return true;
        }

        public override bool Process(ControlDeviceBase Modbus, ref string strOutData)
        {
            string modbus_msg = string.Empty;
            string DataHexValue = string.Empty;
            Dictionary<string, string> ModBus_output = new Dictionary<string, string>();

            if (Modbus is ModBusController)
            {

                ModBusController device = (ModBusController)Modbus;

                try
                {
                    bool ModBusRet = false;
                    string ModBusStr = string.Empty;
                    //string DataHexValue = string.Empty;
                    ushort[] readdata = null;
                    Modbus.SetModBusTimeout(SendTimeout, ReadTimeout);
                    if (!string.IsNullOrEmpty(WriteData))
                    {
                        if (FunctionMode != TransmitMode.SingleCoil_Status)
                        {
                            if (!WriteData.Contains(","))
                            {
                                //單筆資料
                                if (!WriteData.StartsWith("0x"))
                                {
                                    int IntWriteData = int.Parse(WriteData);
                                    DataHexValue = IntWriteData.ToString("X4");
                                }
                                else
                                    DataHexValue = WriteData;
                            }
                            else
                            {
                                //多筆資料
                                if (!WriteData.StartsWith("0x"))
                                {
                                    string[] decimalStrings = WriteData.Split(',');
                                    string[] hexValues = new string[decimalStrings.Length];
                                    for (int i = 0; i < decimalStrings.Length; i++)
                                    {
                                        int decimalNumber = int.Parse(decimalStrings[i].Trim());
                                        hexValues[i] = decimalNumber.ToString("X4");
                                    }
                                    DataHexValue = string.Join(",", hexValues);
                                }
                                else
                                    DataHexValue = WriteData;
                            }
                        }
                        else
                        {
                          if (string.IsNullOrEmpty(WriteData))
                          {
                               LogMessage($"TransmitMode.SingleCoil_Status WriteData can not be Empty", MessageLevel.Error);
                               return false;
                          }                          
                            DataHexValue = WriteData;
                        }

                        //Send                           
                        Modbus.RetryTimes(SendRetryTime);
                        ModBusRet = device.ModBusSEND(StartAddress, DataHexValue, FunctionMode);
                        if (ModBusRet)
                        {
                            LogMessage($"ModbusSend Successful", MessageLevel.Debug);
                            ModBus_output.Add("ModbusSend", $"Successful");
                        }
                        else
                        {
                            LogMessage($"ModbusSend Fail", MessageLevel.Debug);
                            ModBus_output.Add("ModbusSend", $"Fail");
                            Jsondata = JsonConvert.SerializeObject(ModBus_output, Formatting.Indented);
                            strOutData = Jsondata;
                            return false;
                        }
                    }
                    if (NumRegisters != 0)
                    {
                        //Read                           
                        Modbus.RetryTimes(ReadRetryTime);
                        ModBusRet = device.ModBusREAD(StartAddress, NumRegisters, ref readdata, FunctionMode);
                        if (ModBusRet)
                        {
                            switch (Test_Mode)
                            {
                                case TestMode.PowerCalculations:
                                    LogMessage($"PowerCalculations 的裝置\n", MessageLevel.Info);
                                    if (readdata.Length != 2)
                                    {
                                        LogMessage($"PowerCalculations: readdata Length is LessThan 2 Please Set NumRegisters Length EqealThan 2", MessageLevel.Info);
                                        LogMessage($"ModbusRead Fail", MessageLevel.Debug);
                                        ModBus_output.Add("ModbusRead", $"Fail");
                                        Jsondata = JsonConvert.SerializeObject(ModBus_output, Formatting.Indented);
                                        strOutData = Jsondata;
                                        return false;
                                    }
                                    
                                    double VoltagefloatData = Math.Round(readdata[0] / 100.0f, 1);
                                    double CurrentfloatData = readdata[1] / 1000.0f;
                                    double PowerfloatData = Math.Round(VoltagefloatData * CurrentfloatData * 0.9, 2);

                                    ModBus_output.Add($"Voltage(V)", $"{VoltagefloatData}");
                                    ModBus_output.Add($"Current(mA)", $"{CurrentfloatData}");
                                    ModBus_output.Add($"Power(W)", $"{PowerfloatData}");
                                    break;
                                case TestMode.Temperature_Humidity:
                                    LogMessage($"DL11_MC_S1 的裝置\n", MessageLevel.Info);
                                    if (readdata.Length != 2)
                                    {
                                        LogMessage($"DL11_MC_S1: readdata Length is LessThan 2 Please Set NumRegisters Length EqealThan 2", MessageLevel.Info);
                                        LogMessage($"ModbusRead Fail", MessageLevel.Debug);
                                        ModBus_output.Add("ModbusRead", $"Fail");
                                        Jsondata = JsonConvert.SerializeObject(ModBus_output, Formatting.Indented);
                                        strOutData = Jsondata;
                                        return false;
                                    }
                                    // 解析數據
                                    float temperature = readdata[0] / 10.0f; // 根據你的設備可能需要調整比例
                                    float humidity = readdata[1] / 10.0f;    // 根據你的設備可能需要調整比例

                                    LogMessage($"溫度: {temperature}°C 濕度: {humidity}%", MessageLevel.Info);

                                    ModBus_output.Add("Temperature", $"{temperature}");
                                    ModBus_output.Add("Humidity", $"{humidity}");
                                    break;
                                case TestMode.Air_Leak:
                                    LogMessage($"Air_Leak 的裝置\n", MessageLevel.Info);
                                    
                                    if (readdata.Length != 2)
                                    {
                                        LogMessage($"Air_Leak: readdata Length is LessThan 2 Please Set NumRegisters Length EqealThan 2", MessageLevel.Info);
                                        LogMessage($"ModbusRead Fail", MessageLevel.Debug);
                                        ModBus_output.Add("ModbusRead", $"Fail");
                                        Jsondata = JsonConvert.SerializeObject(ModBus_output, Formatting.Indented);
                                        strOutData = Jsondata;
                                        return false;
                                    }
                                    int AirData_Float = readdata[0] - 65335;//補數
                                    int AirData_Int = readdata[1];

                                    //高地位對調
                                    int temp = AirData_Float;
                                    AirData_Float = AirData_Int;
                                    AirData_Int = temp;
                                    //重新組合byte
                                    uint combined = ((uint)AirData_Float << 16 | (uint)(AirData_Int & 0xFFFF));
                                    byte[] bytes = BitConverter.GetBytes(combined);
                                    //轉成Floating
                                    float AirData_Result = BitConverter.ToSingle(bytes, 0);

                                    LogMessage($"Air_Leak: {AirData_Result}", MessageLevel.Info);
                                    ModBus_output.Add("Air_Leak", $"{AirData_Result}");                                                      
                                    break;
                                default:
                                    int data_count = 0;
                                    int decimalNumber = 0;
                                    string hexNumber = string.Empty;
                                    string binaryNumber = string.Empty;
                                    foreach (var register in readdata)
                                    {
                                        decimalNumber = int.Parse($"{register}");
                                        hexNumber = decimalNumber.ToString("X2");
                                        binaryNumber = Convert.ToString(decimalNumber, 2);

                                        switch (Format)
                                        {
                                            case ReadFormat.HEX:
                                                ModBus_output.Add($"ModBusHEX_Data[{data_count}]", $"0x{hexNumber}");
                                                break;
                                            case ReadFormat.DEC:
                                                ModBus_output.Add($"ModBusDEC_Data[{data_count}]", $"{register}");
                                                break;
                                            case ReadFormat.BIN:
                                                ModBus_output.Add($"ModBusBIN_Data[{data_count}]", binaryNumber);
                                                break;
                                        }
                                        data_count++;
                                    }
                                    break;
                            }
                            LogMessage($"ModbusRead Successful", MessageLevel.Info);
                            ModBus_output.Add("ModbusRead", $"Successful");
                        }
                        else
                        {
                            LogMessage($"ModbusRead Fail", MessageLevel.Debug);
                            ModBus_output.Add("ModbusRead", $"Fail");
                            Jsondata = JsonConvert.SerializeObject(ModBus_output, Formatting.Indented);
                            strOutData = Jsondata;
                            return false;
                        }
                    }




                }
                catch (Exception ex)
                {
                    Console.WriteLine($"發生錯誤: {ex.Message}");
                    LogMessage($"NModbus發生錯誤: {ex.Message}", MessageLevel.Error);
                    ModBus_output.Add("ModbusException", $"NModbus發生錯誤: {ex.Message}");
                    Jsondata = JsonConvert.SerializeObject(ModBus_output, Formatting.Indented);
                    strOutData = Jsondata;
                    return false;
                }

                Jsondata = JsonConvert.SerializeObject(ModBus_output, Formatting.Indented);
                LogMessage($"ModbusRead Data:{Jsondata}", MessageLevel.Info);
                strOutData = Jsondata;
            }
            else
            {
                LogMessage($"Modbus Device must is ModBusController", MessageLevel.Error);
                return false;
            }
            return true;
        }

        public override bool PostProcess()
        {
            string result = CheckRule(Jsondata, Spec);
            if (result == "PASS" || Spec == "")
            {
                return true;
            }
            else
            {
                LogMessage($"{result}", MessageLevel.Error);
                return false;
            }

        }
        public class DataCommandConverter : TypeConverter
        {
            private bool messageShown = false;
            public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
            {
                return destinationType == typeof(string);
            }

            public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
            {
                
                if (value is string)
                {                                     
                    return value;
                }              
                return base.ConvertTo(context, culture, value, destinationType);
            }

            public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
            {              
                return sourceType == typeof(string);
            }


            public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
            {
                dynamic currentObject = context.Instance;
                if (currentObject.FunctionMode == AutoTestSystem.DAL.Communication.TransmitMode.Holding_registers_Multiple)
                {
                   
                    if (!value.ToString().Contains(",") && !messageShown)
                    {
                        messageShown = true;
                        MessageBox.Show("MultipleCoil_Status or Holding_registers_Multiple Mode: The Data must 包含 逗號", "Note", MessageBoxButtons.OK, MessageBoxIcon.Warning);                       
                        return $"";
                    }
                    else if (value.ToString().EndsWith(",") && !messageShown)
                    {
                        messageShown = true;
                        MessageBox.Show("MultipleCoil_Status or Holding_registers_Multiple Mode: 數值格式錯誤 逗號不可再字尾", "Note", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return $"";
                    }
                    else if(value.ToString().StartsWith(",") && !messageShown)
                    {
                        messageShown = true;
                        MessageBox.Show("MultipleCoil_Status or Holding_registers_Multiple Mode: 數值格式錯誤 逗號不可再字首", "Note", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return $"";
                    }                

                }else if (currentObject.FunctionMode == AutoTestSystem.DAL.Communication.TransmitMode.MultipleCoil_Status)
                {
                    if (!value.ToString().Contains(",") && !messageShown)
                    {
                        messageShown = true;
                        MessageBox.Show("MultipleCoil_Status or Holding_registers_Multiple Mode: The Data must 包含 逗號", "Note", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return $"";
                    }                    
                    else
                    {
                        if (value.ToString().EndsWith(",") && !messageShown)
                        {
                            messageShown = true;
                            MessageBox.Show("MultipleCoil_Status or Holding_registers_Multiple Mode: 數值格式錯誤 逗號不可再字尾", "Note", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return $"";
                        }
                        if (value.ToString().StartsWith(",") && !messageShown)
                        {
                            messageShown = true;
                            MessageBox.Show("MultipleCoil_Status or Holding_registers_Multiple Mode: 數值格式錯誤 逗號不可再字首", "Note", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return $"";
                        }
                        else if(!messageShown)
                        {

                            string[] values = value.ToString().Split(',');
                            int[] decimalValues = Array.ConvertAll(values, val => Convert.ToInt32(val, 16));

                            //Check writeData contain only 0 or 1
                            foreach (int val in decimalValues)
                            {
                                if (val != 0 && val != 1)
                                {
                                    messageShown = true;
                                    MessageBox.Show("MultipleCoil_Status : 數值格式錯誤 只能是 0 或 1 數值", "Note", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    return $"";
                                }
                                if (messageShown)
                                    break;
                            }
                        }
                    }
                }
                else if(currentObject.FunctionMode == AutoTestSystem.DAL.Communication.TransmitMode.SingleCoil_Status)
                {
                    if (string.IsNullOrEmpty(value.ToString()) && !messageShown)
                    {
                        messageShown = true;
                        MessageBox.Show("TransmitMode.SingleCoil_Status WriteData can not be Empty", "Note", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return $"False";
                    }
                    else if (value.ToString() != "True" && value.ToString() != "False" && !messageShown)
                    {
                        messageShown = true;
                        MessageBox.Show("TransmitMode.SingleCoil_Status WriteData must be True or False", "Note", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return $"False";
                    }
                }
                else if(currentObject.FunctionMode == AutoTestSystem.DAL.Communication.TransmitMode.Holding_registers_Single)
                {
                    if (value.ToString().Contains(",") && !messageShown)
                    {
                        messageShown = true;
                        MessageBox.Show("Holding_registers_Single 資料不可包含逗號", "Note", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return $"";
                    }
                }
                else if(currentObject.FunctionMode == AutoTestSystem.DAL.Communication.TransmitMode.Input_Status)
                {
                    if(!string.IsNullOrEmpty(value.ToString()) && !messageShown)
                    {
                        messageShown = true;
                        MessageBox.Show("Input_Status 資料需要為空集合", "Note", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return $"";
                    }
                }
                messageShown = false;
                return value.ToString();
            }
            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                dynamic currentObject = context.Instance;
                List<string> CoilTable = new List<string>();
                if (currentObject.FunctionMode == AutoTestSystem.DAL.Communication.TransmitMode.SingleCoil_Status)
                {
                    CoilTable.Add("True");
                    CoilTable.Add("False");
                }

                return new StandardValuesCollection(CoilTable.ToArray());
            }

            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                return true;
            }
        }

        public enum TestMode
        {
            Default,
            PowerCalculations,
            Temperature_Humidity,
            Air_Leak
        }

        public enum ReadFormat
        {
            HEX,
            DEC,
            BIN
        }
    }
}
