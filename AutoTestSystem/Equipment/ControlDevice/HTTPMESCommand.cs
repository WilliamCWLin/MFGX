using System;
using System.ComponentModel;
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
using System.Threading;
using System.Net.Http;
using System.Net;
using AutoTestSystem.Model;


namespace AutoTestSystem.Equipment.ControlDevice
{
    class HTTPMESCommand: ControlDeviceBase
    {
       
       
   

        [Category("Params"), Description("Set URL")]
        public string URL { get; set; }

        [Category("Params"), Description("Set Terminal")]
        public string Terminal { get; set; }

        public override bool MESDataprocess(string MEStCMD, string MEStData, string checkStr, ref string strOutData)
        {
            string MESURL = string.Empty;
            string MESTTERMINAL = string.Empty;
            MESURL = URL;
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(MESURL);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            MESTTERMINAL = Terminal;
            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                var json = new Dictionary<string, object>
                        {
                            {"tcmd",MEStCMD},
                            {"tterminal",MESTTERMINAL},
                            {"tdata",MEStData}
                        };
                string writedata = CreateDataString(json);
                streamWriter.Write(writedata);
                LogMessage($" writedata  {writedata} ");                   
            }

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();
                LogMessage($" result:  {result} ");

                string decodedJsonData = Regex.Unescape($"Unescape result:  {result} ");
                LogMessage(decodedJsonData);
                
                strOutData = result;
            }

            if (strOutData.Contains(checkStr))
                return true;
            else
                return false;           
        }

        public override bool READ(ref string output)
        {
            //output = strOutData;
            return true;
        }

        public override void Dispose()
        {
            throw new NotImplementedException();
        }

        public override bool Init(string strParamInfo)
        {
            //Logger.Debug("MES Init!");

           /* 
            var httpWebRequest = (HttpWebRequest)WebRequest.Create("http://10.24.97.221:8000/api/mes@1/transfer");
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {

                var json = new Dictionary<string, object>
                        {
                            {"tcmd","C001"},
                            {"tterminal","10001293"},
                            {"tdata","ATE"}
                        };
                streamWriter.Write(CreateDataString(json));
            }

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();
                Logger.Info(result);
            }
            */
            return true;
        }

        public override bool UnInit()
        {
            return true;
        }

        public override void SetTimeout(int time)
        {

        }

        public override void SetCheckstr(string str)
        {
           //Checkstr= str;
        }

        public string CreateDataString(Dictionary<string, object> data)
        {
            try
            {
                string jsonStr = JsonConvert.SerializeObject(data);
                return jsonStr;
            }
            catch (Exception ex)
            {
                // 處理轉換錯誤
                return $"轉換為 JSON 字串時出現錯誤: {ex.Message}";
            }
        }

        public override void SetMEStcmd(string str)
        {
           // MESTCMD = str;
        }

        public override void SetMEStdata(string str)
        {
            //MESTDATA = str;
        }

        public override bool SEND(string input)
        {
            return true;
        }
    }
}

