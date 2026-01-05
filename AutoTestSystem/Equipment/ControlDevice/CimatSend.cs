using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AutoTestSystem.Base;
using AutoTestSystem.DAL;
using static AutoTestSystem.BLL.Bd;

namespace AutoTestSystem.Equipment.ControlDevice
{
    class CimatSend : ControlDeviceBase
    {

        [Category("Params for Cimat Model"), Description("CheckSum Model")]
        public CheckSum_Mode CheckSumMode { get; set; } = CheckSum_Mode.CMT_DPS_12V5A_2C;


        [Category("Params"), Description("Select Comport"), TypeConverter(typeof(ComportList))]
        public string PortName { get; set; }

        [Category("Params"), Description("Set BaudRate")]
        public int baudrate { get; set; }

        Comport DeviceComport = null;
        byte[] commandByte = null;



        public CimatSend()
        {
            baudrate = 9600;
        }



        public override bool Init(string strParamInfo)
        {
            string buffer = string.Empty;
            if (string.IsNullOrEmpty(PortName))
            {
                LogMessage("NO COM Port Name", MessageLevel.Error);
                //MessageBox.Show("NO COM Port Name", "Warning!!!");
                return false;
            }
            SerialConnetInfo DevieCOMinfo = new SerialConnetInfo { PortName = PortName, BaudRate = baudrate };
            DeviceComport = new Comport(DevieCOMinfo);

            if (!DeviceComport.OpenCOM_CHK())
            {
                LogMessage("Init COM Port Fail", MessageLevel.Info);
                return false;
            }
            else
                LogMessage("Init COM Port Pass", MessageLevel.Info);

            return true;

        }

        public override void OPEN()
        {
            DeviceComport.OpenCOM();
        }

        public override bool Status(ref string msg)
        {
            try
            {
                if (DeviceComport.SerialPort.IsOpen)
                {
                    msg = $"{DeviceComport.SerialPort.PortName}(OPEN)";
                    return true;
                }
                else
                {
                    msg = $"{DeviceComport.SerialPort.PortName}(CLOSE)";
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
            if (DeviceComport == null)
                return false;
            DeviceComport.Close();

            return true;
        }

        public override bool SEND(string input)
        {
            try
            {
                DeviceComport.cleanBuffer();
                if (DeviceComport.SerialPort.IsOpen)
                {
                    // 構建要發送的字節數據
                    commandByte = BuildCommand(input);

                    byte[] ByteArray = ByteArrayForCheckSum(commandByte);

                    DeviceComport.Write(ByteArray);
                    LogMessage($"[SEND] {ByteArray}");
                    //MessageBox.Show("DUT send UART Command:" + input);
                }
                else
                {
                    LogMessage($"Cimat Device {DeviceComport.SerialPort.PortName}(CLOSE)", MessageLevel.Error);
                    return false;
                }
            }catch(Exception send_ex)
            {
                LogMessage($"[SEND_Exception]:{send_ex.Message}",MessageLevel.Error);
                return false;
            }
            return true;
        }

        static string FormatHexString(string hexString)
        {
            // 檢查字串長度是否為偶數
            if (hexString.Length % 2 != 0)
            {
                throw new ArgumentException("Hex string length must be even.");
            }

            // 初始化結果字串
            string result = "";

            // 以每兩個字節分隔並加上逗號
            for (int i = 0; i < hexString.Length; i += 2)
            {
                result += hexString.Substring(i, 2);
                if (i < hexString.Length - 2)
                {
                    result += ",";
                }
            }

            return result;
        }
        static byte[] BuildCommand(string Sendcomment)
        {
            string[] hexValues = null;
            // 移除字串中的"0x"和空格
            if (Sendcomment.Contains(" "))
                Sendcomment = Sendcomment.Replace(" ", "");
            if (Sendcomment.Contains("0x"))
                Sendcomment = Sendcomment.Replace("0x", "");

            if (Sendcomment.Contains(","))
            {
                hexValues = Sendcomment.Split(',');
            }
            else
            {
                // 將字串以每兩個字節分隔並加上逗號
                string formattedString = FormatHexString(Sendcomment);
                hexValues = formattedString.Split(',');
            }

            // 初始化byte數組
            byte[] byteArray = new byte[hexValues.Length];

            // 將每個16進制值轉換為byte
            for (int i = 0; i < hexValues.Length; i++)
            {
                byteArray[i] = Convert.ToByte(hexValues[i], 16);
            }

            return byteArray;
        }

        public byte[] ByteArrayForCheckSum(byte[] input)
       {
            // 建立一個新的陣列，其大小比原始陣列大 1
            byte[] NewBytes = new byte[input.Length + 1];

            switch (CheckSumMode)
            {
                case CheckSum_Mode.CMT_DPS_12V5A_2C:
                    LogMessage("CMT_DPS_12V5A_2C CheckSum", MessageLevel.Debug);
                    byte newByte = CalculateChecksum(input);

                    // 複製原始陣列到新的陣列
                    Array.Copy(input, NewBytes, input.Length);

                    // 將新的 byte 新增到最後一個位置
                    NewBytes[NewBytes.Length - 1] = newByte;

                    break;
                default:
                    NewBytes = input;
                    LogMessage("NO CheckSum",MessageLevel.Warn);
                    break;

            }

            return NewBytes;
       }

        public byte CalculateChecksum(byte[] command)
        {
            int sum = 0;
            for (int i = 0; i < command.Length; i++)
            {
                sum += command[i];
            }
            return (byte)(~(sum & 0xFF));
        }

        public override bool READ(ref string output)
        {
            
            output =DeviceComport.ReadBytes(commandByte);
            //MessageBox.Show("Arduino讀取完成:" + output);
            if(string.IsNullOrEmpty(output))
                return false;
            return true;
        }

        public override void Dispose()
        {
            throw new NotImplementedException();
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

        public enum CheckSum_Mode
        {
            CMT_DPS_12V3A_1C,
            CMT_DPS_12V5A_2C
        }

    }
}
