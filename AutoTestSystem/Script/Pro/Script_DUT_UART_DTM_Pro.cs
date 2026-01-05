using AutoTestSystem.Base;
using AutoTestSystem.DUT;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Diagnostics;
using static AutoTestSystem.BLL.Bd;
using System.ComponentModel;
using AutoTestSystem.Model;
using System.Data;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Header;
using System.Web.UI.WebControls;

namespace AutoTestSystem.Script
{
    internal class Script_DUT_UART_DTM_Pro : ScriptDUTBase
    {
        string strOutData = string.Empty;

        [Category("Common Parameters"), Description("自訂顯示名稱")]
        public int ReadTimeOut { get; set; } = 3000;

        [Category("Common Parameters"), Description("自訂顯示名稱")]
        public int TotalTimeOut { get; set; } = 3000;

        [Category("Common Parameters"), Description("自訂顯示名稱"), TypeConverter(typeof(DataTypeList))]
        public string Data_Type { get; set; } = "HEX";

        [Category("Common Parameters"), Description("自訂顯示名稱")]
        public int Data_Length { get; set; } = 64;

        [Category("Common Parameters"), Description("自訂顯示名稱")]
        public int Data_Header { get; set; } = 0;

        [Category("Common Parameters"), Description("自訂顯示名稱")]
        public int Data_Tail { get; set; } = 0;

        [Category("DTM Parameters"), Description("自訂顯示名稱")]
        public string Mode { get; set; } = "Transmitter";

        [Category("DTM Parameters"), Description("自訂顯示名稱"), TypeConverter(typeof(FunctionList))]
        public string Funcion { get; set; } = "Reset";

        [Category("DTM Parameters"), Description("自訂顯示名稱"), TypeConverter(typeof(FrequencyList))]
        public string Frequency { get; set; } = "2402";

        [Category("DTM Parameters"), Description("Must be within the range -40 to 3")]
        public int Power { get; set; } = 0;

        [Category("DTM Parameters"), Description("Must be within the range 0 to 63")]
        public int Packet_Length { get; set; } = 0;

        [Category("DTM Parameters"), Description("自訂顯示名稱"), TypeConverter(typeof(PayloadList))]
        public string Packet_Type { get; set; } = "PRBS9";

        [Category("DTM Parameters"), Description("自訂顯示名稱"), TypeConverter(typeof(PHYList))]
        public string PHY { get; set; } = "1LE";


        public override void Dispose()
        {
            //throw new NotImplementedException();
        }
        
        public override bool PreProcess()
        {
            if (Data_Length == 0)
            {
                LogMessage("Read_Length can not be 0.", MessageLevel.Error);
                return false;
            }

            if (Data_Tail <= Data_Header)
            {
                if (Data_Tail == 0 && Data_Header == 0)
                {
                    Data_Tail = Data_Length;
                    return true;
                }
                LogMessage("Read_Tail can not be less than Read_Header.", MessageLevel.Error);
                return false;
            }

            if (Power < -40 || Power > 3)
            {
                LogMessage($"Power beyond the allowed range -40 to 3", MessageLevel.Error);
                return false;
            }

            if (Packet_Length < 0 || Packet_Length > 63)
            {
                LogMessage($"Packet_Length beyond the allowed range 0 to 63", MessageLevel.Error);
                return false;
            }

            return true;
        }
        
        public override bool Process(DUT_BASE DUTDevice,ref string output)
        {
            var send_bytes = new List<byte>();
            DUTDevice.SetTimeout(ReadTimeOut, TotalTimeOut);

            switch (Funcion)
            {
                case "Stop":
                    send_bytes.Add(0xC0);
                    send_bytes.Add(0x00);
                    break;

                case "Start":              
                    uint freq = (Convert.ToUInt32(Frequency) - 2402) / 2;
                    switch (Mode)
                    {
                        case "Transmitter":
                            send_bytes.Add(Convert.ToByte(Convert.ToUInt32("80", 16) | freq));
                            break;

                        case "Receiver":
                            send_bytes.Add(Convert.ToByte(Convert.ToUInt32("40", 16) | freq));
                            break;
                    }

                    uint length = (uint)Packet_Length << 2;
                    switch (Packet_Type)
                    {
                        case "PRBS9":
                            send_bytes.Add(Convert.ToByte(length | 0b_00));
                            break;

                        case "11110000":
                            send_bytes.Add(Convert.ToByte(length | 0b_01));
                            break;

                        case "10101010":
                            send_bytes.Add(Convert.ToByte(length | 0b_10));
                            break;
                    }

                    break;

                case "Set_PHY":
                    send_bytes.Add(0x02);
                    switch (PHY)
                    {
                        case "1LE":
                            send_bytes.Add(0x04);
                            break;

                        case "2LE":
                            send_bytes.Add(0x08);
                            break;
                    }
                    break;

                case "Set_Tx_Power":
                    if (Mode == "Transmitter")
                    {
                        send_bytes.Add(0x09);
                        send_bytes.Add((byte)Convert.ToSByte(Power));
                    }
                    else
                    {
                        LogMessage($"[Mode] must be Transmitter !!", MessageLevel.Error);
                        return false;
                    }
                    break;

                case "Reset":
                    send_bytes.Add(0x00);
                    send_bytes.Add(0x00);
                    break;
            }

            DUTDevice.SEND(send_bytes.ToArray());
            LogMessage($"Send:  {BitConverter.ToString(send_bytes.ToArray())}");
            DUTDevice.READ(ref output, Data_Length, Data_Header, Data_Tail);
            LogMessage($"Read END:  {output}");

            strOutData = output;

            return true;
        }

        public override bool PostProcess()
        {
            string result = CheckRule(strOutData, Spec);

            if (result == "PASS" || Spec == "")
            {
                return true;
            }             
            else
            {
                LogMessage($"{result}",MessageLevel.Error);
            }
                
            return false;   
        }

        public byte[] StringToBytes(string text, string numericBase)
        {
            if (text == null)
            {
                return null;
            }
            else
            {
                var tokens = text.Split(new char[] { ' ', '-', ',' }, StringSplitOptions.RemoveEmptyEntries);
                var list = new List<byte>();
                for (int i = 0; i < tokens.Length; i++)
                {
                    try
                    {
                        switch (numericBase)
                        {
                            case "BIN":
                                list.Add(Convert.ToByte(tokens[i], 2));
                                break;

                            case "OCT":
                                list.Add(Convert.ToByte(tokens[i], 8));
                                break;

                            case "DEX":
                                list.Add(Convert.ToByte(tokens[i], 10));
                                break;

                            case "HEX":
                                list.Add(Convert.ToByte(tokens[i], 16));
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        //index is 0 based but position is 1 based
                        var position = i + 1;
                        LogMessage($"{ex.Message} Token: '{tokens[i]}' at position {position}.", MessageLevel.Error);
                    }
                }

                return list.ToArray();
            }
        }

        public class ModeList : TypeConverter  //下拉式選單
        {
            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                List<string> itemList = new List<string>() { "Transmitter", "Receiver" };
                return new StandardValuesCollection(itemList);
            }

            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                return true;
            }
        }

        public class FrequencyList : TypeConverter  //下拉式選單
        {
            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                List<string> itemList = new List<string>() { "2402", "2440", "2480" };
                return new StandardValuesCollection(itemList);
            }

            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                return true;
            }
        }

        public class FunctionList : TypeConverter  //下拉式選單
        {
            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                List<string> itemList = new List<string>() { "Stop", "Start", "Set_PHY", "Set_Tx_Power", "Reset" };
                return new StandardValuesCollection(itemList);
            }

            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                return true;
            }
        }

        public class PHYList : TypeConverter  //下拉式選單
        {
            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                List<string> itemList = new List<string>() { "1LE", "2LE" };
                return new StandardValuesCollection(itemList);
            }

            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                return true;
            }
        }

        public class PayloadList : TypeConverter  //下拉式選單
        {
            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                List<string> itemList = new List<string>() { "PRBS9", "11110000", "10101010" };
                return new StandardValuesCollection(itemList);
            }

            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                return true;
            }
        }
    }
}
