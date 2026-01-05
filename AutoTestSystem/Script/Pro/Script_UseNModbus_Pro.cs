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

namespace AutoTestSystem.Script
{
    internal class Script_UseNModbus_Pro : Script_ControlDevice_Base
    {
        string Jsondata = string.Empty;
        private string receivedValue = null;

        [Category("NModBus Parameters"), Description("SlaveID")]
        public byte SlaveID { get; set; } = 1;


        [Category("NModBus Parameters"), Description("Address")]
        public ushort Address { get; set; } = 0;

        [Category("NModBus Send Parameters"), Description("WriteData")]
        public ushort WriteData { get; set; } = 0;

        [Category("NModBus Read Parameters"), Description("Read NumRegisters")]
        public ushort NumRegisters { get; set; } = 2;

        [Category("NModBus Read Mode"), Description("NModBus TestMode")]
        public TestMode Test_Mode { get; set; } = TestMode.Default;




        public Script_UseNModbus_Pro()
        {

        }
        public override void Dispose()
        {

        }
        public override bool PreProcess()
        {
            Jsondata = string.Empty;
            return true;
        }
        
        public override bool Process(ControlDeviceBase Modbus , ref string strOutData)
        {
            string modbus_msg = string.Empty;
            bool modbus_status = false;
            Dictionary<string,string> ModBus_output = new Dictionary<string, string>();

                try
                {
                    modbus_status =Modbus.Status(ref modbus_msg);
                    if (modbus_status)
                    {
                        bool ModBusRet = false;
                        string ModBusStr = string.Empty;
                        ushort [] readdata = null;
                        if (WriteData != 0)
                        {
                            //Send
                            ModBusRet = Modbus.ModbusSend(SlaveID,Address, WriteData);
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
                            ModBusRet = Modbus.ModbusRead(SlaveID,Address, NumRegisters, ref readdata);
                            if (ModBusRet)
                            {
                                switch (Test_Mode)
                                {
                                    case TestMode.PowerCalculations:
                                        double VoltagefloatData = Math.Round(readdata[0] / 100.0f,1);
                                        double CurrentfloatData = readdata[1] / 1000.0f;
                                        double PowerfloatData = Math.Round(VoltagefloatData * CurrentfloatData * 0.9,2);

                                        ModBus_output.Add($"Voltage(V)", $"{VoltagefloatData}");
                                        ModBus_output.Add($"Current(mA)", $"{CurrentfloatData}");
                                        ModBus_output.Add($"Power(W)", $"{PowerfloatData}");
                                        break;
                                    case TestMode.Temperature_Humidity:
                                        LogMessage($"DL11_MC_S1 的裝置\n", MessageLevel.Info);
                                        // 解析數據
                                        float temperature = readdata[0] / 10.0f; // 根據你的設備可能需要調整比例
                                        float humidity = readdata[1] / 10.0f;    // 根據你的設備可能需要調整比例

                                        LogMessage($"溫度: {temperature}°C 濕度: {humidity}%", MessageLevel.Info);

                                        ModBus_output.Add("Temperature", $"{temperature}");
                                        ModBus_output.Add("Humidity", $"{humidity}");
                                        break;
                                     case TestMode.SIMBATOUCH:
                                        LogMessage($"SIMBATOUCH 的裝置\n", MessageLevel.Info);
                                        double Value = 0.00;
                                        // 將ushort數組轉換為int數組
                                        int[] intArray = readdata.Select(ushortValue => (int)ushortValue).ToArray();
                                        if(intArray[0] == 65535)//負數
                                        {
                                            Value = ~(intArray[0] - intArray[1]) / 100.0f;//補數
                                        }else
                                        {
                                            Value = intArray[1] / 100.0f;    // 根據你的設備可能需要調整比例
                                        }                                       
                                        LogMessage($"SIMBATOUCH數值: {Value}", MessageLevel.Info);
                                        ModBus_output.Add("SIMBATOUCH數值", $"{Value}");
                                    break;
                                    default:
                                        int data_count = 0;
                                        foreach (var register in readdata)
                                        {
                                            ModBus_output.Add($"ModBusData[{data_count}]", $"{register}");
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

                    }else
                    {
                        LogMessage($"Modbus Connect Fail", MessageLevel.Error);
                        ModBus_output.Add("ModbusStatus", $"Modbus Connect Fail");
                        Jsondata = JsonConvert.SerializeObject(ModBus_output, Formatting.Indented);
                        strOutData = Jsondata;
                        return false;
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
        public enum TestMode
        {
            Default,
            PowerCalculations,
            Temperature_Humidity,
            SIMBATOUCH
        }
    }
}
