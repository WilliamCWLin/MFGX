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
    internal class Script_SerialPortCMD_Pro : Script_ControlDevice_Base
    {
        private Relay_Channel _strChannel;
        public int ConvertCH;

        string Jsondata = string.Empty;
        [Category("Comment Parameters"), Description("Send Serial Comport comment")]
        public string Send_comment { get; set; }

        [Category("ReadMode Parameters"), Description("Enable Read comment")]
        public bool ReadMode { get; set; } = false;
        [Category("ReadMode Parameters"), Description("ReadMode CheckString")]
        public string StrContentCheck { get; set; }

        [Category("RelayMode Parameters"), Description("Enable RelayMode Function")]
        public Relay_Mode RelayMode { get; set; } = Relay_Mode.Disable;
        [Category("RelayMode Parameters"), Description("Select Relay Channel")]
        public int RelayChannel{ get; set; } = 1;
        /*public Relay_Channel RelayChannel {
            get
            {
                return _strChannel;
            }
            set
            {

                ConvertCH = CovertToINT(value);
                _strChannel = value;
            }
        }*/


        public Script_SerialPortCMD_Pro()
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

        public override bool Process(ControlDeviceBase Serial_port, ref string strOutData)
        {
            string buffer = string.Empty;
            bool Pass_Fail = false;
            string Relay_Comment = string.Empty;
            string CRC = string.Empty;
            Dictionary<string, string> SerialPort_output = new Dictionary<string, string>();
            try
            {
                switch (RelayMode)
                {
                    case Relay_Mode.Disable:
                        if (ReadMode)
                        {
                            LogMessage($"READ Mode On", MessageLevel.Info);
                            if (!string.IsNullOrEmpty(StrContentCheck))
                            {
                                LogMessage($"SEND:{Send_comment}", MessageLevel.Info);
                                SerialPort_output.Add("SEND", Send_comment);
                                Pass_Fail = Serial_port.SEND(Send_comment);

                                Pass_Fail &= Serial_port.READ(ref buffer);
                                SerialPort_output.Add("READ", buffer);
                                LogMessage($"READ:{buffer}", MessageLevel.Info);
                                SerialPort_output.Add("ContentCheck", StrContentCheck);
                                if (buffer.Trim() != StrContentCheck)
                                {
                                    LogMessage($"\r\nReadContentCheck=>\r\n{buffer}\r\nNot eqeal with=>\r\n{StrContentCheck}\r\n", MessageLevel.Warn);
                                    return false;
                                }
                            }
                            else
                            {
                                LogMessage($"StrContentCheck is Empty or null!!!", MessageLevel.Warn);
                                return false;
                            }
                        }
                        else
                        {
                            LogMessage($"SEND:{Send_comment}", MessageLevel.Info);
                            SerialPort_output.Add("SEND", Send_comment);
                            Pass_Fail = Serial_port.SEND(Send_comment);
                        }
                        break;
                    case Relay_Mode.Relay_ON:
                        Relay_Comment = $"A0 0{RelayChannel} 01";
                        CRC = RelayCRC("A0", $"0{RelayChannel}","01");
                        LogMessage($"SEND:{Relay_Comment} {CRC}", MessageLevel.Info);
                        SerialPort_output.Add("SEND", $"{Relay_Comment} {CRC}");
                        Pass_Fail = Serial_port.SEND($"{Relay_Comment} {CRC}");

                        Pass_Fail &= Serial_port.READ(ref buffer);
                        SerialPort_output.Add("READ", buffer);
                        LogMessage($"READ:{buffer}", MessageLevel.Info);
                        SerialPort_output.Add("ContentCheck", $"CH{RelayChannel}:ON");
                        if (buffer.Trim() != $"CH{RelayChannel}:ON")
                        {
                            LogMessage($"\r\nReadContentCheck=>\r\n{buffer}\r\nNot eqeal with=>\r\nCH{RelayChannel}:ON\r\n", MessageLevel.Warn);
                            return false;
                        }
                        break;
                    case Relay_Mode.Relay_OFF:
                        Relay_Comment = $"A0 0{RelayChannel} 00";
                        CRC = RelayCRC("A0", $"0{RelayChannel}","00");
                        LogMessage($"SEND:{Relay_Comment} {CRC}", MessageLevel.Info);
                        SerialPort_output.Add("SEND", $"{Relay_Comment} {CRC}");
                        Pass_Fail = Serial_port.SEND($"{Relay_Comment} {CRC}");

                        Pass_Fail &= Serial_port.READ(ref buffer);
                        SerialPort_output.Add("READ", buffer);
                        LogMessage($"READ:{buffer}", MessageLevel.Info);
                        SerialPort_output.Add("ContentCheck", $"CH{RelayChannel}:OFF");
                        if (buffer.Trim() != $"CH{RelayChannel}:OFF")
                        {
                            LogMessage($"\r\nReadContentCheck=>\r\n{buffer}\r\nNot eqeal with=>\r\nCH{RelayChannel}:OFF\r\n", MessageLevel.Warn);
                            return false;
                        }
                        break;
                }
            } catch (Exception ex)
            {
                LogMessage($"SerialPort Error:{ex.Message}", MessageLevel.Error);
                return false;
            }

            Jsondata = JsonConvert.SerializeObject(SerialPort_output, Formatting.Indented);
            LogMessage($"\nSerialPort Output:{Jsondata}", MessageLevel.Info);
            strOutData = Jsondata;
            return Pass_Fail;
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

        public int CovertToINT(Relay_Channel value)
        {
            if (value == Relay_Channel.CH1)
                return 1;
            else if (value == Relay_Channel.CH2)
                return 2;
            else if (value == Relay_Channel.CH3)
                return 3;
            else if (value == Relay_Channel.CH4)
                return 4;
            else
                return -1;
        }

        public string RelayCRC(string hex1, string hex2, string hex3)
        {
            // 將十六進制數字轉換為整數
            int num1 = Convert.ToInt32(hex1, 16);
            int num2 = Convert.ToInt32(hex2, 16);
            int num3 = Convert.ToInt32(hex3, 16);

            // 計算總和
            int sum = num1 + num2 + num3;

            // 將結果轉換回十六進制並輸出
            string hexSum = sum.ToString("X");
            return hexSum;
        }

        public enum Relay_Mode
        { 
            Disable,
            Relay_ON,
            Relay_OFF
        }

        public enum Relay_Channel
        {
            CH1,
            CH2,
            CH3,
            CH4
        }

    }
}
