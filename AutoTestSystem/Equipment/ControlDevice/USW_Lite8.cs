using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AutoTestSystem.Base;
using AutoTestSystem.DAL;
using Manufacture;
using Newtonsoft.Json;
using Renci.SshNet;
using static AutoTestSystem.BLL.Bd;

namespace AutoTestSystem.Equipment.ControlDevice
{
    class USW_Lite8 : ControlDeviceBase
    {

        [Category("Common Parameters"), Description("Host")]
        public string Host { get; set; } = "";
        [Category("Common Parameters"), Description("UserName")]
        public string UserName { get; set; } = "";
        [Category("Common Parameters"), Description("Password")]
        public string Password { get; set; } = "";

        [JsonIgnore]
        [Browsable(false)]
        private string Command="";

        SshClient client = null;
        public USW_Lite8()
        {

        }

        public override bool Init(string strParamInfo)
        {             
            LogMessage($"Init Success.{strParamInfo}");

            
            try
            {
                if(client == null)
                {
                    client = new SshClient(Host, UserName, Password);
                    client.Connect();
                }


                LogMessage("Connected to the server.");
            }
            catch (Exception e)
            {
                LogMessage("An error occurred: " + e.Message);
                return false;
            }
            
            return true;
        }

        public override void OPEN()
        {
            LogMessage("OPEN");
        }

        public override bool UnInit()
        {
            LogMessage("UnInit");
            if(client !=null)
            {
                client.Disconnect();
                client = null;
            }

            return true;
        }

        public override bool SEND(string input)
        {
            Command = input;
            LogMessage(Command);
            return true;
        }

        public override bool READ(ref string output)
        {
            try
            {
                // 執行命令並抓取數值
                var command = client.CreateCommand(Command);
                string result = command.Execute();
                output = result;
                LogMessage($"Read Success.{output}");
            }
            catch(Exception ex)
            {
                LogMessage($"Exception.{ex.Message}");
                return false;
            }
            return true;
        }

        public override void Dispose()
        {
            throw new NotImplementedException();
        }
       
        public override void SetTimeout(int time)
        {
            LogMessage($"SetTimeout.{time}");
        }
    }
}
