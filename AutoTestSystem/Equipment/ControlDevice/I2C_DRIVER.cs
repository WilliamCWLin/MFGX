using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AutoTestSystem.DAL;
using static AutoTestSystem.BLL.Bd;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using AutoTestSystem.Base;
using Newtonsoft.Json.Linq;
using OpenCvSharp;
using System.ComponentModel;
using Manufacture;

namespace AutoTestSystem.Equipment.ControlDevice
{
    class I2C_DRIVER : ControlDeviceBase
    {
        DosCmd doscmd = new DosCmd();
        int waitTime = 0;
        string recvStr = "";
        string checkStr = "";
        string strOutData = string.Empty;
        string SendInit_Command = string.Empty;
        string ID_List = string.Empty;
        public bool Init_Succed = false;
        //string orginalID = string.Empty;

        [Category("I2CDrvCommon Parameters"), Description("選擇I2C_executable.exe路徑"), Editor(typeof(FileSelEditorRelPath), typeof(System.Drawing.Design.UITypeEditor))]
        public string Exe_Path { get; set; }

        [Category("I2CDrvCommon Parameters"), Description("I2CDriver_Comport(ex: COM3)")]
        public string I2CDrv_Comport { get; set; }

        [Category("I2CDrvCommon Parameters"), Description("Set up Pull up Resistance"), TypeConverter(typeof(Resistance))]
        public string PullupResistance { get; set; } = "1.1K";

        [Category("I2CDrvCommon Parameters"), Description("Set Speed"), TypeConverter(typeof(Speed))]
        public string I2C_Speed { get; set; } = "100K";

        [Category("I2CDrv Application"), Description("Choice Item Name")]
        public Item_Name Item_name { get; set; } = Item_Name.Default;

        /*[ReadOnly(true)]
        [Category("I2CDrvCommon Parameters"), Description("SlaveID")]
        public string orginalID { get; set; }*/

        public override bool SEND(string input)
        {
            try
            {
                string pattern = string.Empty;
                bool result = doscmd.SendCommand(input, ref recvStr, checkStr, waitTime);
                if (result == true)
                {
                    strOutData = recvStr;

                }
                else
                    return false;
            }catch(Exception i2c_ex)
            {
                LogMessage($"I2C Error:{i2c_ex.Message}", MessageLevel.Error);
                return false;
            }
            return true;
        }

        public override bool READ(ref string output)
        {
            output = strOutData;
            return true;
        }

        public override void Dispose()
        {
            throw new NotImplementedException();
        }

        public override bool Init(string strParamInfo)
        {
            bool com_ret = false;
            bool reset_ret = false;
            bool status_ret = false;
            string scan_address = string.Empty;
            string I2c_status_msg = string.Empty;
            Init_Succed = false;

           if (!string.IsNullOrEmpty(Exe_Path))
           {
           if (!File.Exists(Exe_Path))
           {
                   LogMessage("Exe_Path is not exist", MessageLevel.Error);
                   return false;
           }
           }else
           {
               LogMessage("Exe_Path can not be null.", MessageLevel.Error);
               return false;
           }

            if (string.IsNullOrEmpty(I2CDrv_Comport))
            {
                LogMessage("I2CDrv_Comport can not be null.", MessageLevel.Error);
                return false;
            }
            SetCheckstr($"Comport:{I2CDrv_Comport}");
            //Check I2C Driver Comport
            SendInit_Command = $"{Exe_Path} {I2CDrv_Comport}";
            com_ret = SEND(SendInit_Command);
            if (com_ret)
            {
                //Reset
                reset_ret = SEND(SendInit_Command + " x");
                if (reset_ret)
                {
                    status_ret = Status(ref I2c_status_msg);
                    LogMessage($"I2CDrv:{I2c_status_msg}", MessageLevel.Info);
                    if(status_ret == false)
                    {
                        return false;
                    }

                    switch (Item_name)
                    {
                        case Item_Name.Default:
                            break;
                        case Item_Name.LED:
                            bool LED_Init = false;
                            SetCheckstr($"Connected_Succeed:1");
                            LED_Init = SEND(SendInit_Command + " w 0x44 0x00,0x73 p");
                            LED_Init &= SEND(SendInit_Command + " w 0x44 0x01,0xff p");
                            LED_Init &= SEND(SendInit_Command + " w 0x44 0x61,0x00 p");
                            LED_Init &= SEND(SendInit_Command + " w 0x44 0x0E,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00 p");
                            LED_Init &= SEND(SendInit_Command + " w 0x44 0x02,0xff,0xff,0xff,0xff,0xff,0xff,0xff,0xff,0xff,0xff,0xff,0xff p");
                            if(LED_Init == false)
                            {
                                LogMessage("I2C Driver LED Init Fail.", MessageLevel.Error);
                                return false;
                            }else
                            {
                                LogMessage("I2C Driver LED Init Succeed.", MessageLevel.Debug);
                            }
                            break;
                    }
                    Init_Succed = true;
                    //Scan
                    //scan_ret = SEND(SendInit_Command + " d");
                    //READ(ref scan_address);
                    //ScanSlaveID(scan_address);
                }
                else
                    return false;
            }
            else
                return false;

            return true;
        }

        public override bool UnInit()
        {
            SendInit_Command = string.Empty;
            Init_Succed = false;
            return true;
        }

        public override bool Status(ref string msg)
        {
            bool status_ret = false;
            bool speed_ret = false;
            string ConvertResistor = string.Empty;
            string I2C_read = string.Empty;

            //string pattern = @"SDA=(\d+) SCL=(\d+)";

            SetCheckstr($"SDA=1 SCL=1");

            if(I2C_Speed == "100K")
                speed_ret=SEND($"{SendInit_Command} 1");
            else if (I2C_Speed == "400K")
                speed_ret=SEND($"{SendInit_Command} 4");
            if(!speed_ret)
            {
                msg = $"I2C_Send_Speed_Fail";
                return false;
            }

            ConvertResistor = ResistorCovert(PullupResistance);
            status_ret = SEND($"{SendInit_Command} u {ConvertResistor}");
            status_ret &= SEND(SendInit_Command + " i");
            if (status_ret)
            {
                READ(ref I2C_read);
                msg = I2C_read;
            }
            else
            {
                msg = $"I2C_Send_Status_Fail";
                return false;
            }
         return true;
        }

        public override void SetTimeout(int time)
        {
            waitTime = time;
        }
        public override void SetCheckstr(string str)
        {
            checkStr = str;
        }
        public string ScanSlaveID(string id)
        { 
            ID_List = id;
            return SlaveIDSearch();
        }

        public string SlaveIDSearch()
        {
            List<string> I2C_SlaveIDKey = new List<string>();
            string SlaveID = string.Empty;
            I2C_SlaveIDKey = SlaveID_parse();

            if (I2C_SlaveIDKey.Count != 0)
            {
              string Device = string.Empty;
              foreach (var id in I2C_SlaveIDKey)
              {
                Device += id + ",";
              }
               SlaveID = Device.TrimEnd(',');

               /*DialogResult result;
               if (!string.IsNullOrEmpty(SlaveID))
               {
                  if (SlaveID != orginalID) 
                  {                        result = MessageBox.Show($"SlaveID:{orginalID} is not include in Device SlaveID {SlaveID} , Are you sure to Change this SlaveID", "Check Device SlaveID", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (result == DialogResult.Yes)
                    {
                        orginalID = SlaveID;
                    }
                  }
               }*/
            }
            return SlaveID;
        }

        public List<string> SlaveID_parse()
        {
            List<string> numbers = new List<string>();

            // 找到 "Comport" 的位置
            int startIndex = ID_List.IndexOf($"Start_to_Scan_id");
            if (startIndex != -1)
            {
                // 計算開始提取的位置，跳過 "Comport" 的長度
                startIndex += $"Start_to_Scan_id".Length;

                // 提取從 "Comport:COM3" 之後的字串
                string result = ID_List.Substring(startIndex).Trim();
                string pattern = @"\b[0-9a-fA-F]+\b";//@"\d+"

                // 使用正則表達式提取數字
                MatchCollection matches = Regex.Matches(result, pattern);
                foreach (Match match in matches)
                {
                    numbers.Add($"0x{match.Value}");
                }
            }

            return numbers;

        }       

        public string SendExeCommand()
        {
            return SendInit_Command;
        }

        public string ResistorCovert(string Resistor)
        {
            string _covertResistor = string.Empty;
            switch (Resistor)
            {
                case "Disable":
                    _covertResistor = HexToAscii("00");
                    break;
                case "2.2K":
                    _covertResistor = HexToAscii("09");
                    break;
                case "4.3K":
                    _covertResistor = HexToAscii("12");
                    break;
                case "4.7K":
                    _covertResistor = HexToAscii("24");
                    break;
                case "1.5K":
                    _covertResistor = HexToAscii("1b");
                    break;
                case "1.1K":
                    _covertResistor = HexToAscii("3f");
                    break;
                default:
                    break;
            }

            return _covertResistor;
        }

        static string HexToAscii(string hex)
        {
            // 去掉可能存在的空白字元
            hex = hex.Replace(" ", "");
            // 建立 byte 陣列
            byte[] bytes = new byte[hex.Length / 2];
            // 逐一解析每兩個 hex 字元，轉換為 byte
            for (int i = 0; i < hex.Length; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }
            // 將 byte 陣列轉換為 ASCII 字串
            return Encoding.ASCII.GetString(bytes);
        }

        public class Speed : TypeConverter  //下拉式選單
        {
            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {


                List<string> Speed_Key = new List<string>();

                Speed_Key.Add("100K");
                Speed_Key.Add("400K");

                return new StandardValuesCollection(Speed_Key);

            }

            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                return true;

            }

        }

        public class Resistance : TypeConverter  //下拉式選單
        {
            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {


                List<string> Resistance_Key = new List<string>();

                //Resistance_Key.Add("Disable");
                //Resistance_Key.Add("2.2K");
                Resistance_Key.Add("4.3K");
                Resistance_Key.Add("4.7K");
                Resistance_Key.Add("1.5K");
                Resistance_Key.Add("1.1K");

                return new StandardValuesCollection(Resistance_Key);

            }

            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                return true;

            }

        }

        public enum Item_Name
        {
            Default,
            LED
        }

        /*public override bool SEND(string input)
        {
            throw new NotImplementedException();
        }*/
    }
}

