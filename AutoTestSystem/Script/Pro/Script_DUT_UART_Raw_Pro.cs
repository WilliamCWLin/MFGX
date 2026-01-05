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
    internal class Script_DUT_UART_Raw_Pro : ScriptDUTBase
    {
        string strOutData = string.Empty;

        [Category("Common Parameters"), Description("自訂顯示名稱")]
        public string Send_Data { get; set; }

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

        public override void Dispose()
        {
            //throw new NotImplementedException();
        }
        
        public override bool PreProcess()
        {
            if (Send_Data == null || Send_Data == string.Empty)
            {
                LogMessage("Send_Data can not be null.", MessageLevel.Error);
                return false;
            }

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

            return true;
        }
        
        public override bool Process(DUT_BASE DUTDevice,ref string output)
        {
            DUTDevice.SetTimeout(ReadTimeOut, TotalTimeOut);

            DUTDevice.SEND(StringToBytes(Send_Data, Data_Type));
            LogMessage($"Send:  {Send_Data}");

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
    }
}
