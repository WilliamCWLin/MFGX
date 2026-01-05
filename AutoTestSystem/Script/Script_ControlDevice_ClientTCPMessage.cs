using AutoTestSystem.Base;
using AutoTestSystem.Equipment.ControlDevice;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using AutoTestSystem.Script;
using Newtonsoft.Json.Linq;
using Renci.SshNet.Security;
using AutoTestSystem.Model;
using static AutoTestSystem.Script.Script_FileProcessing_Pro;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Threading.Tasks;

namespace AutoTestSystem.Script
{
    internal class Script_Control_ClientTCPMessage : Script_ControlDevice_Base
    {
        public const int WM_CLOSE = 0x10;
        string receivedData = string.Empty;
        string messageBoxMessage = string.Empty;
        string hexstring = string.Empty;

        [Category("Common Parameters"), Description("Timeout")]
        public int Timeout { get; set; } = 10000;
        [Category("Common Parameters"), Description("String to send to server")]
        public string SendString { get; set; }

        [Category("Common Parameters"), Description("Expected string from server")]
        public string ReadString { get; set; }
        [Category("Common Parameters"), Description("請選擇只寫入、只讀取、讀/寫"), TypeConverter(typeof(Option))]
        public string Option_type { get; set; }
        
        private Dictionary<string, object> Devices = new Dictionary<string, object>();
        public static string option = "";
        
        public override void Dispose()
        {
            
        }

        public override bool PreProcess()
        {
            receivedData = string.Empty;
            return true;
        }

        public override bool Process(ControlDeviceBase ControlDevice, ref string output)
        {
            
            var data = new Dictionary<string, object>();
            if (Option_type == "Only_Write")
            {
                
                if (string.IsNullOrEmpty(SendString))
                {
                    LogMessage("Send string is empty.", MessageLevel.Error);
                    return false;
                }
                if (ControlDevice is TcpIpClient client)
                {
                    
                    string hexSendString =SendString;

                    if (!client.SEND(hexSendString))
                        
                    {
                        LogMessage("Failed to send string to server.", MessageLevel.Error);
                        return false;
                    }
                    else
                    {
                        output = hexSendString;
                        return true;
                    }
                }
                else
                {
                    LogMessage("ControlDevice is not an instance of TcpIpClient.", MessageLevel.Error);
                    return false;
                }
            }
            else if (Option_type == "Only_Read")
            {
                if (ControlDevice is TcpIpClient client)
                {
                    
                    ReadTCP readTCP = new ReadTCP(client);
                    if (ReadString == "START")
                    {
                        readTCP.Show();
                        readTCP.Update();
                        if (!client.READTimeout(ref receivedData, Timeout)) // 30 seconds total timeout for read and reconnect
                        {
                            LogMessage("Received Timeout.", MessageLevel.Error);
                            readTCP.Close();
                            return false;
                        }
                        readTCP.Close();
                    }
                    else
                    {
                        if (!client.READTimeout(ref receivedData, Timeout)) // 30 seconds total timeout for read and reconnect
                        {
                            LogMessage("Reveived Timeout.", MessageLevel.Error);
                            readTCP.Close();
                            return false;
                        }
                    }
                    string receivedString = receivedData.Trim();
                    string expectedStringWithCRLF = ReadString;
                    if (receivedString == expectedStringWithCRLF)
                    {
                        output = receivedData;
                        return true;
                    }
                    else
                    {
                        //output = receivedData;
                        LogMessage("Fail to received data", MessageLevel.Error);
                        return false;
                    }
                }
                else
                {
                    LogMessage("ControlDevice is not an instance of TcpIpClient.", MessageLevel.Error);
                    return false;
                }
            }
            else
            {
                string hexSendString = string.Empty;
                if (string.IsNullOrEmpty(SendString))
                {
                    LogMessage("Send string is empty.", MessageLevel.Error);
                    return false;
                }
                
                if (ControlDevice is TcpIpClient client)
                {
                    hexSendString = SendString;
                    if (!client.SEND(hexSendString))
                    {
                        LogMessage("Failed to send string to server.", MessageLevel.Error);
                        if (!client.READTimeout(ref receivedData, Timeout)) // 30 seconds timeout
                        { 
                            LogMessage("Received Timeout.", MessageLevel.Error);
                            return false;
                        }
                    }
                    if (!client.READTimeout(ref receivedData, Timeout)) // 30 seconds total timeout for read and reconnect
                    {
                        LogMessage("Recieved Timeout.", MessageLevel.Error);
                        return false;
                    }
                    string receivedString = receivedData.Trim();
                    string expectedStringWithCRLF = ReadString;

                    if (receivedString.Contains(expectedStringWithCRLF))
                    {
                        output = receivedData;
                        return true;
                    }
                    else
                    {
                        //output = receivedData;
                        LogMessage("Wrong recieved data.", MessageLevel.Error);
                        return false;
                    }
                }
                else
                {
                    LogMessage("ControlDevice is not an instance of TcpIpClient.", MessageLevel.Error);
                    return false;
                }
            }
        }
        public override bool PostProcess()
        {
            return true;
        }
        public class Option : TypeConverter  //下拉式選單
        {
            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                List<string> OptionKeys = new List<string>();
                OptionKeys.Add("Only_Write");
                OptionKeys.Add("Only_Read");
                OptionKeys.Add("Both");

                return new StandardValuesCollection(OptionKeys);
            }
            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                return true;
            }
        }        
    }
}

