using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using AutoTestSystem.Base;
using AutoTestSystem.DAL;
using Manufacture;
using Newtonsoft.Json;
using static AutoTestSystem.BLL.Bd;

namespace AutoTestSystem.Equipment.ControlDevice
{
    class ControlDevSimu : ControlDeviceBase
    {

        [Category("Params"), Description("Set BaudRate")]
        public int baudrate { get; set; }

        [Category("Params"), Description("自訂顯示名稱"), Editor(typeof(JsonEditor), typeof(UITypeEditor))]
        public string Data { get; set; }

        Comport DeviceComport = null;
       
        
        public ControlDevSimu()
        {
            baudrate = 9600;
        }

        public override bool Init(string strParamInfo)
        {             
            LogMessage($"Init Success.{strParamInfo}");
               
            return true;
        }

        public override void OPEN()
        {
            LogMessage("OPEN");
        }

        public override bool UnInit()
        {
            LogMessage("UnInit");

            return true;
        }

        public override bool SEND(string input)
        {
            LogMessage($"Send:{input}");
            return true;
        }

        public override bool READ(ref string output)
        {
            output = Data;
            LogMessage($"Read Success.{output}");
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

        public override bool MESDataprocess(string MEStCMD, string MEStData, string checkStr, ref string strOutData)
        {
            string MESURL = string.Empty;
            string MESTTERMINAL = string.Empty;

            var json = new Dictionary<string, object>
                        {
                            {"tcmd",MEStCMD},
                            {"tterminal",MESTTERMINAL},
                            {"tdata",MEStData}
                        };

            string writedata = "";
            try
            {
                writedata = JsonConvert.SerializeObject(json);

            }
            catch (Exception ex)
            {

            }

            LogMessage($" writedata  {writedata} ");



            var result = Data;
            LogMessage($" result:  {result} ");

            string decodedJsonData = Regex.Unescape($"Unescape result:  {result} ");
            LogMessage(decodedJsonData);

            strOutData = result;


            if (strOutData.Contains(checkStr))
                return true;
            else
                return false;
        }

    }
}
