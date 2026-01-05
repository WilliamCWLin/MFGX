using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using AutoTestSystem.DAL;
using System.Text.RegularExpressions;
using static AutoTestSystem.BLL.Bd;
using AutoTestSystem.Base;
using System.ComponentModel;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net;

namespace AutoTestSystem.DUT
{
    public class DUT_HTTP_JSON : DUT_BASE
    {
        int comportTimeOut = 0;
        int totalTimeOut = 0;
        Comport DutComport = null;

        [Category("Params"), Description("IP_addr")]
        public string IP_addr { get; set; }

 

        public DUT_HTTP_JSON()
        {
            IP_addr = "10.0.0.2";
        }

        public override void Dispose()
        {
            throw new NotImplementedException();
        }

        public override bool Init(string strParamInfo)
        {
            return true;
        }

        public override bool Status(ref string msg)
        {
            return true;
        }
        public override bool StartAction(string strItemName, string strParamIn, ref string strOutput)
        {
            return true;
        }

        public override bool OPEN()
        {
            return true;
        }

        public override bool UnInit()
        {
            return true;
        }

        public override bool SEND(string input)
        {
            return true;
        }

        public override bool SEND(byte[] input)
        {
            return true;
        }

        public override bool READ(string ParamIn, ref string output)
        {
            return true;
        }

        public override bool READNOJSON(string ParamIn , ref string output)
        {
            return true;
        }

        public override void SetTimeout(int timeout_comport, int timeout_total)
        {
            totalTimeOut = timeout_total;
        }
        enum E_request_type : ushort
        {
            GET = 0,
            POST = 1
        }

        public override bool SendCGICommand(int request_type, string Checkstr,string CGICMD, string input,  ref string output)
        {
            string CGIURL = "http://" + IP_addr + CGICMD;
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(CGIURL);
            httpWebRequest.Timeout = totalTimeOut;
            
            httpWebRequest.ContentType = "application/json";
            if (request_type == (int)E_request_type.GET)
                httpWebRequest.Method = "Get";
            else
                httpWebRequest.Method = "POST";

            if (request_type != (int)E_request_type.GET)
            {
                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    streamWriter.Write(input);
                    LogMessage($"CGI writedata  {input} ",MessageLevel.Info);
                }
            }

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();
                LogMessage($"CGI result:  {result} ", MessageLevel.Info);
                output = result;
            }

            if (output.Contains(Checkstr))
                return true;
            else
                return false;           
            
        }

    }
}
