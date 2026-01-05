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

namespace AutoTestSystem.Script
{
    internal class Script_DUT_UART_Raw_Pro_2 : ScriptDUTBase
    {
     
        string strOutData = string.Empty;

        [Category("Common Parameters"), Description("自訂顯示名稱")]
        public int Input_Data__Length { get; set; } = 5;

        [Category("Common Parameters"), Description("自訂顯示名稱")]
        public int ReadTimeOut { get; set; } = 3000;

        [Category("Common Parameters"), Description("自訂顯示名稱")]
        public int TotalTimeOut { get; set; } = 3000;

        [Category("Common Parameters"), Description("自訂顯示名稱"), TypeConverter(typeof(DataTypeList2))]
        public string Input_Data_Type { get; set; } = "HEX";

        [Category("Common Parameters"), Description("自訂顯示名稱")]
        public string Input_Data_01 { get; set; } = "0xDD //Sync1"; 
        
        [Category("Common Parameters"), Description("自訂顯示名稱")]
        public string Input_Data_02 { get; set; } = "0xBB //Sync2"; 
        
        [Category("Common Parameters"), Description("自訂顯示名稱")]
        public string Input_Data_03 { get; set; } = "0x01 //version"; 
        
        [Category("Common Parameters"), Description("自訂顯示名稱")]
        public string Input_Data_04 { get; set; } = "0x20 //event ID =Get version"; 
        
        [Category("Common Parameters"), Description("自訂顯示名稱")]
        public string Input_Data_05 { get; set; } = "0x00 //packet lenght"; 
        
        [Category("Common Parameters"), Description("自訂顯示名稱")]
        public string Input_Data_06 { get; set; } = "0xFF"; 
        
        [Category("Common Parameters"), Description("自訂顯示名稱")]
        public string Input_Data_07 { get; set; } = "0xFF"; 
        
        [Category("Common Parameters"), Description("自訂顯示名稱")]
        public string Input_Data_08 { get; set; } = "0xFF"; 
        
        [Category("Common Parameters"), Description("自訂顯示名稱")]
        public string Input_Data_09 { get; set; } = "0xFF"; 
        
        [Category("Common Parameters"), Description("自訂顯示名稱")]
        public string Input_Data_10 { get; set; } = "0xFF"; 

        [Category("Common Parameters"), Description("自訂顯示名稱"), TypeConverter(typeof(CRCTypeList))]
        public string Input_CRC_TYPE { get; set; } = "NO";

        [Category("Common Parameters"), Description("自訂顯示名稱"), TypeConverter(typeof(YES_NO_TypeList))]
        public string Input_Tail_Type { get; set; } = "NO";

        [Category("Common Parameters"), Description("自訂顯示名稱")]
        public string Input_Tail_Data { get; set; } = "0xFF";

        /////////////////////////////////////////////
        public override void Dispose()
        {
            //throw new NotImplementedException();
        }

        public static ushort CRC16ITU(List<byte> data)
        { 
            ushort POLY = 0x1021;
            ushort crc = 0xFFFF; 
            foreach (var byteData in data)
            {
                crc ^= (ushort)(byteData << 8); 
                for (int j = 0; j < 8; j++) 
                {
                    if ((crc & 0x8000) != 0) 
                        crc = (ushort)((crc << 1) ^ POLY);
                    else
                        crc = (ushort)(crc << 1);
                }
            }
            return (ushort)(crc & 0xFFFF); 
        }

        public static string Convert_to_Json(byte[] data)
        {

            var jsonObject = new JObject
            {
                ["errorCode"] = "0"
            };
            int json_data_count = data.Length;
            jsonObject[$"dataSize"] = json_data_count;

            // 
            for (int i = 0; i < json_data_count; i++)
            {
                jsonObject[$"data{i + 1}"] = $"0x{data[i]:X2}";
            }

            // JSON string
            return JsonConvert.SerializeObject(jsonObject, Formatting.Indented);

        }

        public void AddDataToList(string numericBase, string intput_data, List<byte> list)
        {
            try
            {
                switch (numericBase)
                {
                    case "DEC":
                        list.Add(Convert.ToByte((intput_data.Contains(' ') ? intput_data.Split(' ')[0] : intput_data), 10));
                        break;

                    case "HEX":
                        list.Add(Convert.ToByte((intput_data.Contains(' ') ? intput_data.Split(' ')[0] : intput_data), 16));
                        break;

                }
            }
            catch (Exception ex)
            {
                //index is 0 based but position is 1 based
                LogMessage($"{ex.Message} Token: '{intput_data}' Fail.", MessageLevel.Error);
            }

        }

        public void CRCToList(string numericBase, List<byte> list)
        {

            try
            {
                switch (numericBase)
                {
                    case "NO":
                        break;

                    case "CRC_ITU":           
                        ushort crc = CRC16ITU(list);
                        list.Add((byte)(crc >> 8));     // high byte of CRC
                        list.Add((byte)(crc & 0xFF));   // low byte of CR
                        break;
                }
            }
            catch (Exception ex)
            {
                //index is 0 based but position is 1 based
                LogMessage($"{ex.Message} CRC Fail.", MessageLevel.Error);
            }

        }

        public void TailToList(string numericBase, string intput_data, List<byte> list)
        {
            try
            {
                switch (numericBase)
                {
                    case "NO":
                        break;

                    case "YES":
                        list.Add(Convert.ToByte((intput_data.Contains(' ') ? intput_data.Split(' ')[0] : intput_data), 16));
                        break;

                }
            }
            catch (Exception ex)
            {
                //index is 0 based but position is 1 based
                LogMessage($"{ex.Message} Tail Fail.", MessageLevel.Error);
            }

        }

        public byte[] StringToBytes(int size, string Input_Data_Mode, string CRC_Mode, string TAIL_Mode)
        {

            var list = new List<byte>();

            //////////////////////////////////////////////////
            // Add data
            //////////////////////////////////////////////////
            for (int i = 1; i <= size; i++)
            {
                
                switch (i)
                {
                    case 1:
                        AddDataToList(Input_Data_Mode, Input_Data_01, list);
                        break;
                    case 2:
                        AddDataToList(Input_Data_Mode, Input_Data_02, list);
                        break;
                    case 3:
                        AddDataToList(Input_Data_Mode, Input_Data_03, list);
                        break;
                    case 4:
                        AddDataToList(Input_Data_Mode, Input_Data_04, list);
                        break;
                    case 5:
                        AddDataToList(Input_Data_Mode, Input_Data_05, list);
                        break;
                    case 6:
                        AddDataToList(Input_Data_Mode, Input_Data_06, list);
                        break;                        
                    case 7:
                        AddDataToList(Input_Data_Mode, Input_Data_07, list);
                        break;
                    case 8:
                        AddDataToList(Input_Data_Mode, Input_Data_08, list);
                        break;
                    case 9:
                        AddDataToList(Input_Data_Mode, Input_Data_09, list);
                        break;
                    case 10:
                        AddDataToList(Input_Data_Mode, Input_Data_10, list);
                        break;                        
                }

            }
            
            //////////////////////////////////////////////////
            // Add CRC
            //////////////////////////////////////////////////
            CRCToList(CRC_Mode,list);

            //////////////////////////////////////////////////
            // Add Tail
            //////////////////////////////////////////////////
            TailToList(Input_Data_Mode, Input_Tail_Data, list);

            return list.ToArray();

        }









        
        /////////////////////////////////////////////
        public override bool PreProcess()
        {
            if (Input_Data_01 == null || Input_Data_01 == string.Empty)
            {
                LogMessage("Send_Data can not be null.", MessageLevel.Error);
                return false;
            }

            if (Input_Data__Length == 0)
            {
                LogMessage("Read_Length can not be 0.", MessageLevel.Error);
                return false;
            }
            if (Input_Data__Length > 10)
            {
                LogMessage("Read_Length can not over 10.", MessageLevel.Error);
                return false;
            }
            return true;
        }
        
        public override bool Process(DUT_BASE DUTDevice,ref string output)
        {

            DUTDevice.SetTimeout(ReadTimeOut, TotalTimeOut);

            // 1. Send //
            byte[] send_data = StringToBytes(Input_Data__Length, Input_Data_Type, Input_CRC_TYPE, Input_Tail_Type);
            DUTDevice.SEND(send_data);   

            LogMessage($"Data in Hex:",MessageLevel.Info);
            foreach (byte b in send_data)
                LogMessage($"0x{b:X2} ", MessageLevel.Info); 

            // 2. Read & Show on log //
            DUTDevice.READ("", ref output);
            LogMessage($"Read END:  {output}", MessageLevel.Info);

            // 3. Show read data //
            byte[] byteArray = Encoding.UTF8.GetBytes(output);
            LogMessage("Data in Hex:", MessageLevel.Info);
            foreach (byte b in byteArray)
                LogMessage($"0x{b:X2} ", MessageLevel.Info);

            // 4. save in json formatd
            strOutData = Convert_to_Json(byteArray);
            output = strOutData;

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

    }
}
