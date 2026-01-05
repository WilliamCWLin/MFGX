
using AutoTestSystem.Model;
using Manufacture;
using NAudio.Wave;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Data;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static AutoTestSystem.BLL.Bd;
using AutoTestSystem.DAL;
using AutoTestSystem.Base;
using AutoTestSystem.Equipment.ControlDevice;
using static AutoTestSystem.I2CRW_Form;

namespace AutoTestSystem.Script
{
    internal class Script_I2CDriver_Pro : Script_ControlDevice_Base
    {
        string strOutData = string.Empty;
        string Jsondata = string.Empty;
        string I2c_cmd = string.Empty;
        string I2c_Writecmd = string.Empty;
        string I2c_Readcmd = string.Empty;
        string HexValue = string.Empty;
        string KeyName = string.Empty;
        //string orginalID = string.Empty;
        bool H_L_Swap = false;
        private string _strI2Cmode;
        public string ConvertIIC = string.Empty;
        public Dictionary<string, string> OutPutData = new Dictionary<string, string>();
        public Dictionary<string, string> Tmp_OutPutData = new Dictionary<string, string>();
        public List<Key_Value> key_ValueList = new List<Key_Value>();
        //public List<Key_Value> Address_key_ValueList = new List<Key_Value>();
        //I2CRW_Form HL_Bytes = new I2CRW_Form();

        [JsonIgnore]
        public Key_Value BytesDataItem;

        [Category("I2CDrvCommon Parameters"), Description("選擇I2CDrv_Mode"),TypeConverter(typeof(I2CMode))]
        public string I2CDrv_Mode {
            get
            {
                if (string.IsNullOrEmpty(_strI2Cmode))
                    return "Status";
                else
                    return _strI2Cmode;
            }
            set {

                ConvertIIC = I2CModeCovert(value);
                _strI2Cmode = value;
            }
        }
        [Category("I2CDrvCommon Parameters"), Description("自訂顯示名稱")]
        public string CheckStr { get; set; } = "Comport";

        //[Category("I2CDrvCommon Parameters"), Description("選擇I2CDrv_SlaveID"), TypeConverter(typeof(I2C_SlaveID))]
        [Category("I2CDrvCommon Parameters"), Description("I2CDrv_SlaveID(ex:0xaa)")]
        public string I2CDrv_SlaveID { get; set; }

        [Category("Read Write One Data Parameters"), Description("Bytes")]
        public string Bytes { get; set; }
        [Category("Read Write One Data Parameters"), Description("Address")]
        public string Address { get; set; }
        [Category("Read Write One Data Parameters"), Description("Reg Bytes(以逗號區分每個Bytes ex: 0x12,0x34,....)")]
        public string Reg { get; set; }

        [Category("Hight Low Bytes Select"), Description("Hight Low Bytes and KeyName Select(ALS、Temperature、Humidity)"), Editor(typeof(WREditor), typeof(System.Drawing.Design.UITypeEditor))]
        public string HightLowBytes_Select { get; set; }

        /*[Category("Write Multiple Address Data"), Description("Write Multiple Address Data"), Editor(typeof(WREditor), typeof(System.Drawing.Design.UITypeEditor))]
        public string MultipleAddressData { get; set; }*/

        [Category("Multiple Data Processing"), Description("Read Multiple Address Data")]
        public bool Read_Bytes { get; set; } = false;

        /*[Category("Multiple Data Processing"), Description("Write Multiple Data Length")]
        public int Data_Length { get; set; }*/
        [Category("Multiple Data Processing"), Description("Write Multiple Start Address")]
        public string Address_Start { get; set; } = "0x00";
        
        [Category("Multiple Data Processing"), Description("Write Multiple End Address")]
        public string Address_End { get; set; } = "0x01";

        [Category("Multiple Data Processing"), Description("Write Multiple Data Position")]
        public string PositionData { get; set; } = "Not_Used";
       
        [Category("Multiple Data Processing"), Description("Write Multiple Data Valve")]
        public string DataValve { get; set; } = "0x00";       

        [Category("Common Parameters"), Description("自訂顯示名稱")]
        public int WaitTime { get; set; } = 3000;


        public string strstringoutput = "";

        public override void Dispose()
        {
            //throw new NotImplementedException();
        }
        public override bool PreProcess()
        {
            string RegData = string.Empty;
            I2c_cmd = string.Empty;
            I2c_Writecmd = string.Empty;
            I2c_Readcmd = string.Empty;
            Jsondata = string.Empty;
            HexValue = string.Empty;
            KeyName = string.Empty;
            OutPutData.Clear();
            Tmp_OutPutData.Clear();
            key_ValueList.Clear();
            //Address_key_ValueList.Clear();

            if (!string.IsNullOrEmpty(HightLowBytes_Select))
            {
                key_ValueList = JsonConvert.DeserializeObject<List<Key_Value>>(HightLowBytes_Select);
            }

            /*if (!string.IsNullOrEmpty(MultipleAddressData))
            {
                Address_key_ValueList = JsonConvert.DeserializeObject<List<Key_Value>>(MultipleAddressData);
            }*/

            if (CheckStr == null || CheckStr == string.Empty)
            {
                LogMessage("CheckStr can not be null.", MessageLevel.Error);
                return false;
            }

            if (I2CDrv_Mode == "Read" || I2CDrv_Mode == "Write" || I2CDrv_Mode == "W_R" || I2CDrv_Mode == "MultipleData")
            {
                if (string.IsNullOrEmpty(I2CDrv_SlaveID))
                {
                    LogMessage("I2CDrv_SlaveID can not be null.", MessageLevel.Error);
                    return false;
                }//else
                /*{
                    I2CDrv_SlaveID = CheckSlaveID();
                }*/

                if (I2CDrv_Mode == "Read")
                {
                    if(string.IsNullOrEmpty(Address) || string.IsNullOrEmpty(Bytes))
                    {
                        LogMessage("Address or  Bytes can not be null.", MessageLevel.Error);
                        return false;
                    }
                    I2c_cmd = $"w {I2CDrv_SlaveID} {Address} r {I2CDrv_SlaveID} {Bytes}";
                    LogMessage($"I2C Set Command: {I2c_cmd}", MessageLevel.Info);
                }
                else if (I2CDrv_Mode == "Write")
                {

                    if (string.IsNullOrEmpty(Reg) || string.IsNullOrEmpty(Address))
                    {
                        LogMessage("I2CDrv Reg or Address can not be null.", MessageLevel.Error);
                        return false;
                    }
                    //RegData = RegParse(Reg);
                    I2c_cmd = $"w {I2CDrv_SlaveID} {Address},{Reg}";
                    LogMessage($"I2C Set Command: {I2c_cmd}", MessageLevel.Info);
                }
                else if(I2CDrv_Mode == "W_R")
                {
                    // 初始化資料陣列
                    if (string.IsNullOrEmpty(Reg) || string.IsNullOrEmpty(Address))
                    {
                        LogMessage("I2CDrv_Reg or Address can not be null.", MessageLevel.Error);
                        return false;
                    }
                    //RegData = RegParse(Reg);
                    I2c_Writecmd = $"w {I2CDrv_SlaveID} {Address},{Reg}";
                    I2c_Readcmd = $"w {I2CDrv_SlaveID} {Address} r {I2CDrv_SlaveID} {Bytes}";
                    LogMessage($"I2C Set Write Command: {I2c_Writecmd}", MessageLevel.Info);
                    LogMessage($"I2C Set Read Command: {I2c_Readcmd}", MessageLevel.Info);
                }
                else if(I2CDrv_Mode == "MultipleData")
                {
                    try
                    {
                        int Data_Length = 0;
                        byte start = Convert.ToByte(Address_Start, 16);
                        byte end = Convert.ToByte(Address_End, 16);
                        Data_Length = end - start + 1;

                        // 初始化資料陣列
                        byte[] data = new byte[Data_Length];
                        int[] positions = new int[Data_Length];
                        string MutiData = string.Empty;

                        LogMessage($"PositionData:{PositionData}", MessageLevel.Debug);
                        if (PositionData == "Not_Used")
                        {
                            for (int position = 0; position < Data_Length; position++)
                            {
                                MutiData = $"{SetValueAtPosition(data, position, DataValve)}";
                            }
                        }
                        else
                        {
                            int count = 0;
                            for (byte i = start; i <= end; i++)
                            {
                                string HexString = $"0x{i:X2}";
                                if (PositionData.Contains(HexString))
                                {
                                    MutiData = $"{SetValueAtPosition(data, count, DataValve)}";
                                    LogMessage($"0x{i:X2}", MessageLevel.Info);
                                }
                                count++;
                                //LogMessage($"0x{i:X2}", MessageLevel.Info);
                            }

                        }                  

                    if (Read_Bytes == false)
                    {
                        I2c_cmd = $"w {I2CDrv_SlaveID} {Address_Start},{MutiData}";
                        LogMessage($"I2C Set Command: {I2c_cmd}", MessageLevel.Info);
                    }
                    else
                    {
                        I2c_Writecmd = $"w {I2CDrv_SlaveID} {Address_Start},{MutiData}";
                        I2c_Readcmd = $"w {I2CDrv_SlaveID} {Address_Start} r {I2CDrv_SlaveID} {Data_Length}";
                        LogMessage($"I2C Set Write Command: {I2c_Writecmd}", MessageLevel.Info);
                        LogMessage($"I2C Set Read Command: {I2c_Readcmd}", MessageLevel.Info);
                    }

                    }
                    catch (Exception ex)
                    {
                        LogMessage($"I2C MultipleData Exception : {ex.Message}", MessageLevel.Error);
                        return false;
                    }
                }
            }
            else
            {
                I2c_cmd = $"{ConvertIIC}";
                LogMessage($"I2C Set Command: {I2c_cmd}", MessageLevel.Info);
            }

            return true;
        }
        public override bool Process(ControlDeviceBase PCCmd, ref string strOutData)
        {
            string output = string.Empty;
            string Send_Command = string.Empty;
            string common_cmd = string.Empty;
            string I2c_status_msg = string.Empty;
            bool init_ret = false;
            bool ret = false;
            //string Dec_result = string.Empty;           

            if (PCCmd is I2C_DRIVER Command)
            {
                common_cmd = Command.SendExeCommand();
            }

            if (I2CDrv_Mode == "Init")
            {
                init_ret = PCCmd.Init("");
                if (init_ret == false)
                {
                   OutPutData.Add($"I2CDrv_Status", "I2c_Fail:未能找到 SDA 和 SCL 的數值");
                   string I2CDrv_Fail = JsonConvert.SerializeObject(OutPutData, Formatting.Indented);
                   strOutData = I2CDrv_Fail;
                   return false;
                }
                OutPutData.Add($"I2CDrv_Status", "I2c Init Successed!!");
                return true;
            }

            PCCmd.SetTimeout(WaitTime);
            PCCmd.SetCheckstr(ReplaceProp(CheckStr));

            if (I2CDrv_Mode == "W_R" || Read_Bytes == true)
            {
                //Write
                Send_Command = $"{common_cmd} {I2c_Writecmd}"; //寫入設定暫存的Address並給予Data 
                ret = PCCmd.SEND(ReplaceProp(Send_Command));
                if (ret == false)
                {
                    LogMessage($"Send  Fail", MessageLevel.Warn);
                    OutPutData.Add($"Status", $"Send Fail");
                    string Status_Err = JsonConvert.SerializeObject(OutPutData, Formatting.Indented);
                    strOutData = Status_Err;
                    return false;
                }
                //Read
                Send_Command = $"{common_cmd} {I2c_Readcmd}";
                ret = PCCmd.SEND(ReplaceProp(Send_Command)); //先設定寫入暫存的Address並讀取該位址Data
                ret &= PCCmd.READ(ref output);
                if (ret == false)
                {
                    LogMessage($"Read  Fail", MessageLevel.Warn);
                    OutPutData.Add($"Status", $"Read Fail");
                    string Status_Err = JsonConvert.SerializeObject(OutPutData, Formatting.Indented);
                    strOutData = Status_Err;
                    return false;
                }

            }
            else
            {
                Send_Command = $"{common_cmd} {I2c_cmd}";

                LogMessage($"Send:  {ReplaceProp(Send_Command)}\n");
                OutPutData.Add($"Checkstring", $"{ ReplaceProp(CheckStr)}");
                OutPutData.Add($"Send", $"{ ReplaceProp(Send_Command)}");

                ret = PCCmd.SEND(ReplaceProp(Send_Command));
                if (ret == false)
                {
                    LogMessage($"Send  Fail", MessageLevel.Warn);
                    OutPutData.Add($"Status", $"Send Fail");
                    string Status_Err = JsonConvert.SerializeObject(OutPutData, Formatting.Indented);
                    strOutData = Status_Err;
                    return false;
                }
                ret = PCCmd.READ(ref output);
                if (ret == false)
                {
                    LogMessage($"Read Fail", MessageLevel.Warn);
                    OutPutData.Add($"Status", $"Read Fail");
                    string Status_Err = JsonConvert.SerializeObject(OutPutData, Formatting.Indented);
                    strOutData = Status_Err;
                    return false;
                }

            }
           //Parser I2C Data -Start    
           if(I2CDrv_Mode == "Status")
           {
                // 使用正則表達式來匹配 SDA 和 SCL 的數值
                string pattern = @"SDA=(\d+) SCL=(\d+)";
                Match match = Regex.Match(output, pattern);

                if (match.Success)
                {
                    string sdaValue = match.Groups[1].Value;
                    string sclValue = match.Groups[2].Value;
                    if(sdaValue == "1" && sclValue == "1")//當 SDA 和 SCL 的數值為1
                    {
                        OutPutData.Add($"I2CDrv_Status", "Pass");
                    }else
                    {
                        OutPutData.Add($"I2CDrv_Status", "Fail");
                    }
                }
                else
                {
                    LogMessage($"I2CDrv_Status Fail:未能找到 SDA 和 SCL 的數值", MessageLevel.Warn);
                    OutPutData.Add($"I2CDrv_Status", "I2c_Fail:未能找到 SDA 和 SCL 的數值");
                    string I2CDrv_Fail = JsonConvert.SerializeObject(OutPutData, Formatting.Indented);
                    strOutData = I2CDrv_Fail;
                    return false;
                }
            }
           else if(I2CDrv_Mode == "Disconnect")
            {
                if (output.Contains("Disconnect I2C:-1 0"))
                {
                    OutPutData.Add($"I2CDrv_Disconnect", "Succeed");
                }else
                {
                    LogMessage($"I2CDrv_Disconnect : Fail", MessageLevel.Warn);
                    OutPutData.Add($"I2CDrv_Disconnect", "Fail");
                    string I2CDrv_Fail = JsonConvert.SerializeObject(OutPutData, Formatting.Indented);
                    strOutData = I2CDrv_Fail;
                    return false;
                }
                    
            }
           else if (I2CDrv_Mode == "Reset")
           {
                if (output.Contains("Bus reset. SDA = 1, SCL = 1"))
                {
                    OutPutData.Add($"I2CDrv_Reset", "Pass");
                }else
                {
                    LogMessage($"I2CDrv_Reset : Fail", MessageLevel.Warn);
                    OutPutData.Add($"I2CDrv_Reset", "Fail");
                    string I2CDrv_Fail = JsonConvert.SerializeObject(OutPutData, Formatting.Indented);
                    strOutData = I2CDrv_Fail;
                    return false;
                }                
            }
           else if(I2CDrv_Mode == "Scan")
           {
                string I2C_SlaveID = ScanSlaveID(output);
                if (!string.IsNullOrEmpty(I2C_SlaveID))
                {
                    OutPutData.Add($"I2CDrv_SlaveID", I2C_SlaveID);
                }
                else
                {
                    LogMessage($"I2CDrv_Scan : Fail",MessageLevel.Warn);
                    OutPutData.Add($"I2CDrv_Scan", "Fail");
                    string I2CDrv_Fail = JsonConvert.SerializeObject(OutPutData, Formatting.Indented);
                    strOutData = I2CDrv_Fail;
                    return false;
                }
            }
           else if (I2CDrv_Mode == "Read" || I2CDrv_Mode == "W_R" || Read_Bytes == true)
            {
                LogMessage($"HEX_Read:{output}\n", MessageLevel.Info);
                string[] cmd = common_cmd.Split(' ');
                int startIndex = output.IndexOf($"Comport:{cmd[1]}");
                if (startIndex != -1)
                {
                    // 計算開始提取的位置，跳過 "Comport" 的長度
                    startIndex += $"Comport:{cmd[1]}".Length;
                    // 提取從 "Comport:COM3" 之後的字串
                    string result = output.Substring(startIndex).Trim();
                    string[] data_list = result.Split(',');
                    int count = 0;
                    foreach (var list in data_list)
                    {
                        OutPutData.Add($"HexBytes[{count}]",list);
                        if (!string.IsNullOrEmpty(HightLowBytes_Select))
                        {
                            ParseHLByte(count,list);
                        }
                        count++;
                    }

                    HexToDec();
                }
                else
                {
                    LogMessage($"startIndex Error!!",MessageLevel.Warn);
                    OutPutData.Add($"Status", $"startIndex Error!!");
                    string Status_Err = JsonConvert.SerializeObject(OutPutData, Formatting.Indented);
                    strOutData = Status_Err;
                    return false;
                }
            }
            //Parser I2C Data -End   

            Jsondata = JsonConvert.SerializeObject(OutPutData, Formatting.Indented);
            strOutData = Jsondata;
            LogMessage($"Read:{Jsondata}\n");

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

        static string SetValueAtPosition(byte[] data, int position, string value)
        {
            string hexString = string.Empty;
            byte byteValue = Convert.ToByte(value, 16);
            if (position >= 0 && position < data.Length)
            {
                data[position] = byteValue;
            }

            hexString = string.Join(",", data.Select(b => $"0x{b:X2}"));

            return hexString;
        }

        public void MultipleAddressDataParse()
        {

        }

        public void ParseHLByte(int Bytescount, string ReadValue)
        {
            string HightLow_B = string.Empty;
            string Hight_B = string.Empty;
            string Low_B = string.Empty;
            H_L_Swap = false;
            //key_ValueList = JsonConvert.DeserializeObject<List<Key_Value>>(HightLowBytes_Select);
            // 遍歷並打印 Key 和 Value
            foreach (var item in key_ValueList)
            {
                KeyName = item.KeyName;
                if (item.Hight_Bytes > item.Low_Bytes)
                {
                    H_L_Swap = true;
                }

                if (Bytescount == item.Hight_Bytes)
                {
                    Hight_B = ReadValue.Replace("0x", "");
                }
                else if (Bytescount == item.Low_Bytes)
                {
                    Low_B = ReadValue.Replace("0x", "");
                }
            }

            HexValue += Hight_B + Low_B;
        }
        static string SwapBytes(string input)
        {
            if (input.Length % 2 != 0)
            {
                throw new ArgumentException("Input string length must be even.");
            }

            char[] charArray = input.ToCharArray();
            for (int i = 0; i < charArray.Length; i += 4)
            {
                // Swap the first two bytes with the next two bytes
                char temp1 = charArray[i];
                char temp2 = charArray[i + 1];
                charArray[i] = charArray[i + 2];
                charArray[i + 1] = charArray[i + 3];
                charArray[i + 2] = temp1;
                charArray[i + 3] = temp2;
            }

            return new string(charArray);
        }
        public void HexToDec()
        {

            if (H_L_Swap)
            {
                HexValue = SwapBytes(HexValue);
                LogMessage($"Read Swap HexValue:{HexValue}\n", MessageLevel.Info);
            }
            else
                LogMessage($"Read HexValue:{HexValue}\n", MessageLevel.Info);

            if (!string.IsNullOrEmpty(HexValue))
            {
                int decimalValue=0;
                int Data_num = 0;
                string decimalData = string.Empty;
                string input = HexValue; // 請將這裡替換為你的實際字符串
                string fristTwoChars = string.Empty;
                string remainingChars = string.Empty;
                string pattern = ".{1,4}";
                //Dictionary<string, string> DECData = new Dictionary<string, string>();

                // 使用正則表達式將字符串每4個字符拆分成一組
                var matches = Regex.Matches(input, pattern);

                // 將16進制字串轉換成10進制數字
                foreach (Match match in matches)
                {
                    decimalValue = Convert.ToInt32(match.Value, 16);
                    //fristTwoChars = decimalValue.ToString().Substring(0,2);
                    //remainingChars = decimalValue.ToString().Substring(2);
                    OutPutData.Add($"DECData[{Data_num}]", decimalValue.ToString());
                    Data_num++;                   
                }

                CovertToSensorData();
            }
        }
        public void CovertToSensorData()
        {
            bool keyName_exist = true;
            int count = 0;
            foreach (var name in key_ValueList)
            {
                string oldKey = $"DECData[{count}]";
                string newKey = name.KeyName;
                if (string.IsNullOrEmpty(newKey))
                {
                    keyName_exist = false;
                    MessageBox.Show("The KeyName is exist null or Empty", "KeyName Check", MessageBoxButtons.OK,MessageBoxIcon.Error);
                    break;
                }
                if (OutPutData.ContainsKey(oldKey))
                {
                    Tmp_OutPutData[newKey] = OutPutData[oldKey];
                }
                count++;
            }
            if (keyName_exist != false)
            {
                double value = 0;
                foreach (var result in Tmp_OutPutData)
                {
                    string data_value = result.Value;
                    //VD5005 VD5006 IRB Setting
                    if (result.Key == "Temperature")
                    {
                        value = -45 + 175 * int.Parse(result.Value) / Math.Pow(2, 16);
                        data_value = value.ToString();
                    }
                    else if (result.Key == "Humidity")
                    {
                        value = 100 * int.Parse(result.Value) / Math.Pow(2, 16);
                        data_value = value.ToString();
                    }
                    OutPutData.Add(result.Key, data_value);
                }
            }
        }
            public string RegParse(string Reg_input)
        {
            string Reg_output = string.Empty;
            string [] Reg_Data = null;
            if (!string.IsNullOrEmpty(Reg_input))
            {
              if (Reg_input.Contains("0x"))
                  Reg_input = Reg_input.Replace("0x", "");

                string pattern = ".{1,2}";
                // 使用正則表達式將字符串每4個字符拆分成一組
                var matches = Regex.Matches(Reg_input, pattern);

                // 將16進制字串轉換成10進制數字
                foreach (Match match in matches)
                {
                   Console.WriteLine(match.Value);
                   Reg_output += "0x" + match.Value + ",";
                }
                Reg_output = Reg_output.TrimEnd(',');
            }

            return Reg_output;
        }

            public string I2CModeCovert(string iicmode)
        {
            string _covertiic = string.Empty;
            switch (iicmode)
            {
                case "Status":
                    _covertiic = "i";
                    break;
                case "Disconnect":
                    _covertiic = "D";
                    break;
                case "Reset":
                    _covertiic = "x";
                    break;
                case "Scan":
                    _covertiic = "d";
                    break;
                /*case "Read":
                    _covertiic = "r";
                    break;
                case "Write":
                    _covertiic = "w";
                    break;*/
                case "Monitor":
                    _covertiic = "m";
                    break;
                case "Capture":
                    _covertiic = "c";
                    break;
                default:
                    break;
            }

            return _covertiic;
        }

        public string ScanSlaveID(string id)
        {
            string I2C_SlaveIDKey = string.Empty;

            foreach (var value in GlobalNew.Devices.Values)
            {
                if (value is I2C_DRIVER ID)
                {
                    I2C_SlaveIDKey = ID.ScanSlaveID(id);
                    break;
                }
            }

            /*if (!I2C_SlaveIDKey.Contains(I2CDrv_SlaveID))
            {
                DialogResult result = MessageBox.Show($"SlaveID:{I2CDrv_SlaveID} is not include in Device SlaveID {I2C_SlaveIDKey} , Are you sure to Change this SlaveID", "Check Device SlaveID", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    return I2CDrv_SlaveID;
                }
            }*/
            return I2C_SlaveIDKey;
        }

        public class I2CMode : TypeConverter  //下拉式選單
        {
            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {


                List<string> I2C_ModeKey = new List<string>();

                I2C_ModeKey.Add("Init");
                I2C_ModeKey.Add("Disconnect");
                I2C_ModeKey.Add("Status");
                I2C_ModeKey.Add("Reset");
                I2C_ModeKey.Add("Scan");
                I2C_ModeKey.Add("Read");
                I2C_ModeKey.Add("Write");
                I2C_ModeKey.Add("W_R");
                I2C_ModeKey.Add("MultipleData");
                //I2C_ModeKey.Add("Monitor");
                //I2C_ModeKey.Add("Capture");

                return new StandardValuesCollection(I2C_ModeKey);

            }

            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                return true;

            }

        }


        /*public class I2C_SlaveID : TypeConverter  //下拉式選單
        {
            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                string SlaveIDKey = string.Empty;
                List<string> I2C_SlaveIDKey = new List<string>();

                foreach (var value in GlobalNew.Devices.Values)
                {
                    if (value is I2C_DRIVER ID)
                    {
                        I2C_SlaveIDKey = ID.SlaveID_parse();
                        break;
                    }
                }
        
                return new StandardValuesCollection(I2C_SlaveIDKey);
            }

            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                return true;

            }
           
        }*/

        public class WREditor : System.Drawing.Design.UITypeEditor
        {
            public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
            {
                using (var WRDialog = new I2CRW_Form())
                {
                    if (value == null)
                        value = string.Empty;

                    WRDialog.SetParam(value.ToString());
                    if (WRDialog.ShowDialog() == DialogResult.OK)
                    {
                        return WRDialog.GetParam();
                    }
                    else
                    {
                        MessageBox.Show($"The Hight Low Bytes or KeyName is exist \"Empty\" or \"Null\",Please Check I2CRW From Setting", "SetI2CRWparam Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }

                return value; // 如果用戶取消選擇，返回原始值
            }

            public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
            {
                return UITypeEditorEditStyle.Modal;
            }
        }
    }
}
