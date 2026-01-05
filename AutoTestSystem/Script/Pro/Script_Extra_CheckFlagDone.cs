
using AutoTestSystem.Model;
using Manufacture;
using NAudio.Wave;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Design;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static AutoTestSystem.BLL.Bd;

namespace AutoTestSystem.Script
{
    internal class Script_Extra_CheckFlagDone : Script_Extra_Base
    {
        [Category("Common Parameters"), Description("Key")]
        public string Key { get; set; } = "";

        [Category("Common Parameters"), Description("Key")]
        public int Timeout { get; set; } = 15000;
        public override void Dispose()
        {
            //throw new NotImplementedException();
        }
        public override bool PreProcess()
        {

            return true;
        }
        public override bool Process(ref string strOutData)
        {

            string ret = PopMoreData(Key);
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            while (ret != "1")
            {
                if (stopwatch.ElapsedMilliseconds >= Timeout) // Timeout after 10 seconds
                {
                    return false;
                }

                ret = PopMoreData(Key);
                Thread.Sleep(10); // Optional: sleep for a short period to avoid busy-waiting
            }

            return true;
        }
        public override bool PostProcess()
        {
            return true;

        }


    }
}
